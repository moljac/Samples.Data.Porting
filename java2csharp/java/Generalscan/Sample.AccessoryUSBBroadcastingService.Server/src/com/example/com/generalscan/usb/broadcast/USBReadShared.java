package com.example.com.generalscan.usb.broadcast;

import android.content.Context;
import android.content.SharedPreferences;

public class USBReadShared {

	public static boolean AutoConnect(Context mContext) {
		SharedPreferences sharedata = mContext.getSharedPreferences("data", 0);
		boolean isFirst = sharedata.getBoolean(UsbConstant.AutoConnect, true);

		return isFirst;
	}
	public static String getPacketName(Context mContext) {
		SharedPreferences sharedata = mContext.getSharedPreferences("data", 0);
		String PacketName = sharedata.getString(UsbConstant.PacketName, "");
		return PacketName;
	}
	public static String getAppName(Context mContext) {
		SharedPreferences sharedata = mContext.getSharedPreferences("data", 0);
		String AppName = sharedata.getString(UsbConstant.AppName, "");
		return AppName;
	}
}
