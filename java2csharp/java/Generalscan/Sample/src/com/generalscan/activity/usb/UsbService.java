package com.generalscan.activity.usb;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.IBinder;

import com.generalscan.NotifyStyle;
import com.generalscan.usb.UsbConnect;

public class UsbService extends Service {
	private Context mContext;

	@Override
	public IBinder onBind(Intent arg0) {
		// TODO Auto-generated method stub
		return null;
	}

	@Override
	public void onCreate() {
		super.onCreate();
		mContext = this;
		// 开启USB服务
		UsbConnect.BindService(mContext);
		// 设置连接提示类型
		UsbConnect.CurrentNotifyStyle = NotifyStyle.NotificationStyle1;

	}

	@Override
	public void onDestroy() {

		UsbConnect.UnBindService(mContext);
		super.onDestroy();
	}

}
