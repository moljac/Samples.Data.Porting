package com.example.com.generalscan.usb.broadcast;

import android.graphics.drawable.Drawable;

public class AppInfo {

	private String packageName;
	private Drawable drawable;
	private String name;
	
	public void setPackage(String packageName) {
		this.packageName = packageName;
	}

	public void setDrawable(Drawable drawable) {
		this.drawable= drawable;
	}

	public void setName(String str_name) {
		this.name = str_name;
	}

	public String getPackageName() {
		return packageName;
	}

	public Drawable getDrawable() {
		return drawable;
	}

	public String getName() {
		return name;
	}

}
