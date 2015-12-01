//
//  FlipsideViewController.swift
//  Firebus
//
//  Created by Katherine Fang on 6/23/14.
//  Copyright (c) 2014 Firebase. All rights reserved.
//

import Foundation
import UIKit

@class_protocol protocol FlipsideViewControllerDelegate {
    func flipsideViewControllerDidFinish(controller:FlipsideViewController);
}

class FlipsideViewController : UIViewController {
    weak var delegate : FlipsideViewControllerDelegate?
    
    init(nibName: String?, bundle:NSBundle?) {
        super.init(nibName: nibName, bundle: bundle)
        // !!! took out check for self != nil
        self.contentSizeForViewInPopover = CGSizeMake(320.0, 480.0)
    }
    
    @IBAction func done(AnyObject) {
        delegate?.flipsideViewControllerDidFinish(self)
    }
    
    @IBAction func github(AnyObject) {
        UIApplication.sharedApplication().openURL(NSURL.URLWithString("https://github.com/firebase/firebus"))
    }
}