package com.example.com.generalscan.usb.broadcast;

import android.content.Context;
import android.content.SharedPreferences.Editor;

public class USBSaveShared {

	public static void SaveAutoConnect(Context mContext, boolean value) {
		Editor sharedata = mContext.getSharedPreferences("data", 0).edit();
		sharedata.putBoolean(UsbConstant.AutoConnect, value);
		sharedata.commit();
	}

	public static void SavePacketName(Context mContext, String packetName) {
		Editor sharedata = mContext.getSharedPreferences("data", 0).edit();
		sharedata.putString(UsbConstant.PacketName, packetName);
		sharedata.commit();
	}
	public static void SaveAppName(Context mContext, String appName) {
		Editor sharedata = mContext.getSharedPreferences("data", 0).edit();
		sharedata.putString(UsbConstant.AppName, appName);
		sharedata.commit();
	}
}
