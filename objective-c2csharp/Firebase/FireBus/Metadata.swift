//
//  Metadata.swift
//  Firebus
//
//  Created by Katherine Fang on 6/23/14.
//  Copyright (c) 2014 Firebase. All rights reserved.
//

import Foundation
import MapKit

class Metadata {
    var metadata : Dictionary<String, Any>
    var pin : MKPointAnnotation
    
    init(metadata: Dictionary<String, Any>, pin: MKPointAnnotation) {
        self.metadata = metadata
        self.pin = pin
    }
}