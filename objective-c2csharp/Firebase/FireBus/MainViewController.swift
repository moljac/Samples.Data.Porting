//
//  MainViewController.swift
//  Firebus
//
//  Created by Katherine Fang on 6/23/14.
//  Copyright (c) 2014 Firebase. All rights reserved.
//

import UIKit
import MapKit
import CoreLocation
import QuartzCore

class MainViewController : UIViewController, FlipsideViewControllerDelegate, MKMapViewDelegate {
    var flipsidePopoverController : UIPopoverController?
    var busLocations : Dictionary<Int, Metadata> = [:]
    @IBOutlet var map: MKMapView
    
    override func viewDidLoad() {
        super.viewDidLoad()
        
        busLocations = [:]
        map.delegate = self
        moveMapToSF()
        setupOpacityTimer()
        setupFirebaseCallbacks()
    }
    
    /*
    This is where all of the Firebase magic happens. We do some basic setup:
    
    1) Watch for when we're connected to display the progress HUD
    2) Observe for new and changing buses
    3) Observe for removed buses
    
    That's it! No more pull to refresh!
    
    */
    func setupFirebaseCallbacks() {
        // Register for changes on connection state
        var firebusRoot = Firebase(url: "https://publicdata-transit.firebaseio.com/")
        firebusRoot.childByAppendingPath("/.info/connected").observeEventType(FEventTypeValue, withBlock: { snapshot in
            if !snapshot.value.boolValue {
                MBProgressHUD.showHUDAddedTo(self.view, animated: true)
            }
        })
        
        // Observe when buses are added
        var firebusSF = firebusRoot.childByAppendingPath("sf-muni/vehicles")
        firebusSF.observeEventType(FEventTypeChildAdded, withBlock: {
            snapshot in
            let bus = self.snapshotToDictionary(snapshot)
            MBProgressHUD.hideHUDForView(self.view, animated: true)
            self.addBusToMap(bus, key:snapshot.name.toInt()!);
        })
        // ... and changed
        firebusSF.observeEventType(FEventTypeChildChanged, withBlock: {
            snapshot in
            let bus = self.snapshotToDictionary(snapshot)
            if let busMetadata = self.busLocations[snapshot.name.toInt()!] {
                self.animateBus(busMetadata, toNewPosition: bus)
            }
        })
        
        // Observe when buses are removed
        firebusSF.observeEventType(FEventTypeChildRemoved, withBlock: {
            snapshot in
            if let busMetadata = self.busLocations[snapshot.name.toInt()!] {
                self.busLocations.removeValueForKey(snapshot.name.toInt()!)
                self.map.removeAnnotation(busMetadata.pin)
            }
        })
    }
    
    /* Helper function to turn a snapshot into a dictionary for easier access */
    func snapshotToDictionary(snapshot: FDataSnapshot) -> Dictionary<String, Any> {
        var d = Dictionary<String, Any>()
        
        d["routeTag"] = snapshot.value.objectForKey("routeTag") as String
        d["heading"] = snapshot.value.objectForKey("heading") as Int
        d["id"] = snapshot.value.objectForKey("id") as Int
        d["lat"] = snapshot.value.objectForKey("lat") as Double
        d["lon"] = snapshot.value.objectForKey("lon") as Double
        d["predictable"] = snapshot.value.objectForKey("predictable") as Bool
        d["secsSinceReport"] = snapshot.value.objectForKey("secsSinceReport") as Int
        d["speedKmHr"] = snapshot.value.objectForKey("speedKmHr") as Int
        d["timestamp"] = snapshot.value.objectForKey("timestamp") as Double
        d["vtype"] = snapshot.value.objectForKey("vtype") as String
        
        if snapshot.value.objectForKey("dirTag") {
            d["dirTag"] = snapshot.value.objectForKey("dirTag") as String // Sometimes missing
        } else {
            d["dirTag"] = "OB"
        }
        
        return d
    }
    
    func moveMapToSF() {
        let sf = CLLocationCoordinate2DMake(37.778905, -122.391635)
        let span = MKCoordinateSpanMake(0.025, 0.025)
        let region = MKCoordinateRegionMake(sf, span)
        map.region = region
    }
    
    func setupOpacityTimer() {
        NSTimer.scheduledTimerWithTimeInterval(1.0, target: self, selector: "adjustAgeOpacity", userInfo: nil, repeats: true)
    }
    
    func adjustAgeOpacity() {
        for (key, busMetadata) in self.busLocations {
            let busPin = busMetadata.pin as PointAnnotation
            let metadata = busMetadata.metadata
            
            // Bus tag opacity fades as the data grows stale (takes 2 minutes to disappear)
            let ts = metadata["timestamp"] as Double
            let now = NSDate().timeIntervalSince1970 as Double
            let age = now - ts / 1000
            let alpha = (age > 120) ? 0.01 : (1.0 - (age/120.0))
            
            // Change alpha as animation
            if let busView = map.viewForAnnotation(busPin) {
                let alphaDelta = fabs(Double(busView.alpha) - alpha)
                if (alphaDelta > 0.02) {
                    var animation = CABasicAnimation(keyPath: "opacity")
                    animation.duration = 0.95
                    animation.fromValue = busView.alpha
                    animation.toValue = alpha
                    animation.delegate = busView
                    animation.removedOnCompletion = true
                    busView.layer.addAnimation(animation, forKey: "alphaAnimation")
                }
                busView.alpha = CGFloat(alpha)
            }
        }
    }
    
    func addBusToMap(bus: Dictionary<String, Any>, key: Int) {
        dispatch_async(dispatch_get_main_queue(), {
            if bus != nil && self.busLocations[key] == nil {
                // Set up the bus annotation
                let busPin = PointAnnotation()
                busPin.setCoordinate(CLLocationCoordinate2DMake(bus["lat"] as Double, bus["lon"] as Double))
                busPin.title = bus["routeTag"] as String
                busPin.key = String(key)
                busPin.route = bus["routeTag"] as String
                busPin.vtype = bus["vtype"] as String
                
                // Checking containsString is hard in Swift, so bridge to ObjC
                let dirTag = (bus["dirTag"] as String).bridgeToObjectiveC()
                busPin.outbound = dirTag.rangeOfString("OB").location != Foundation.NSNotFound
                
                // Create metadata and save to map and locations
                let busMetadata = Metadata(metadata:bus, pin:busPin)
                self.map.addAnnotation(busPin)
                self.busLocations[key] = busMetadata
            }
        })
    }
    
    /* Animates a bus. This is technically broken (try panning the map). 
     * StackOverflow has some questions (but no answers) to this problem.
     * http://stackoverflow.com/questions/24408099/move-animation-of-mkannotationview-in-mkmapview-is-not-working-propertly-ios7
    * http://stackoverflow.com/questions/19268080/car-annotation-animation-like-uber-app-not-working
    */
    func animateBus(busMetadata:Metadata, toNewPosition newMetadata:Dictionary<String, Any>) {
        dispatch_async(dispatch_get_main_queue(), {
            let busPin = busMetadata.pin
            
            if let busView = self.map.viewForAnnotation(busPin) {
                
                let newCoord = CLLocationCoordinate2DMake(newMetadata["lat"] as Double, newMetadata["lon"] as Double)
                let mapPoint = MKMapPointForCoordinate(newCoord)
                let toPos = self.map.convertCoordinate(newCoord, toPointToView: self.map)
                
                if MKMapRectContainsPoint(self.map.visibleMapRect, mapPoint) {
                    var animation = CABasicAnimation(keyPath: "position")
                    animation.fromValue = NSValue(CGPoint:busView.center)
                    animation.toValue = NSValue(CGPoint:toPos)
                    animation.duration = 5.5
                    animation.delegate = busView
                    animation.fillMode = kCAFillModeForwards
                    busView.layer.addAnimation(animation, forKey: "positionAnimation")
                }
                
                busView.center = toPos
                busMetadata.metadata = newMetadata
                busPin.setCoordinate(newCoord)
            }
            })
    }
    
    func flipsideViewControllerDidFinish(controller: FlipsideViewController) {
        if UIDevice.currentDevice().userInterfaceIdiom == .Phone {
            self.dismissViewControllerAnimated(true, completion: nil)
        } else {
            self.flipsidePopoverController!.dismissPopoverAnimated(true)
        }
    }
    
    @IBAction func showInfo(sender:UIBarButtonItem) {
        if UIDevice.currentDevice().userInterfaceIdiom == .Phone {
            let controller = FlipsideViewController(nibName: "FBusFlipsideViewController", bundle: nil)
            controller.delegate = self
            controller.modalTransitionStyle = .FlipHorizontal
            self.presentViewController(controller, animated: true, completion: nil)
        } else {
            if !flipsidePopoverController {
                let controller = FlipsideViewController(nibName: "FBusFlipsideViewController", bundle: nil)
                controller.delegate = self
                flipsidePopoverController = UIPopoverController(contentViewController: controller)
            }
            if flipsidePopoverController!.popoverVisible {
               flipsidePopoverController!.dismissPopoverAnimated(true)
            } else {
                flipsidePopoverController!.presentPopoverFromBarButtonItem(sender, permittedArrowDirections: .Any, animated: true)
            }
        }
    }
    
    func mapView(mapView:MKMapView!, viewForAnnotation annotation: MKAnnotation!) -> MKAnnotationView! {
        var pinView : MKAnnotationView?
        
        // if we are dealing with transit
        if let busAnnotation = annotation as? PointAnnotation {
            pinView = mapView.dequeueReusableAnnotationViewWithIdentifier(busAnnotation.key)
            
            if !pinView {
                pinView = MKAnnotationView(annotation: annotation, reuseIdentifier: busAnnotation.key)
            }
            
            pinView!.canShowCallout = true
            
            let image = ImageUtils.imageFromText(busAnnotation.route, outbound: busAnnotation.outbound, vtype: busAnnotation.vtype)
            let imageView = UIImageView(image: image)
            imageView.layer.cornerRadius = 5.0
            imageView.layer.masksToBounds = true
            imageView.layer.borderColor = UIColor.lightGrayColor().CGColor
            imageView.layer.borderWidth = 1.0
            pinView!.addSubview(imageView)
        }
        return pinView
    }
}