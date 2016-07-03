package com.example.com.generalscan.usb.broadcast;

import android.app.Service;
import android.content.Context;
import android.content.Intent;
import android.os.IBinder;

import com.generalscan.NotifyStyle;
import com.generalscan.OnConnectedListener;
import com.generalscan.OnDisconnectListener;
import com.generalscan.OnNotifyIntentCaller;
import com.generalscan.SendConstant;
import com.generalscan.usb.UsbConnect;
import com.generalscan.usb.UsbSend;

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
		// USB服务
		UsbConnect.BindService(mContext);
		// 设置连接提示类型
		UsbConnect.CurrentNotifyStyle = NotifyStyle.NotificationStyle1;
		setListener();
	}

	private void setListener() {
		UsbConnect.SetOnConnectedListener(new OnConnectedListener() {

			@Override
			public void Connected() {
				Intent intent = new Intent();

				intent.setAction(UsbConstant.ConnectedAction);

				mContext.sendBroadcast(intent);
			}

		});
		UsbConnect.SetOnDisconnectListener(new OnDisconnectListener() {

			@Override
			public void Disconnected() {
				Intent intent = new Intent();

				intent.setAction(UsbConstant.DisconnectAction);

				mContext.sendBroadcast(intent);
			}

		});
		UsbConnect.SetOnNotifyIntentCaller(new OnNotifyIntentCaller() {

			@Override
			public Intent initIntent(Intent intent) {
				intent = new Intent();
				intent.setClass(mContext, UsbServiceActivity.class);
				intent.putExtra(UsbConstant.NotifyIntent, UsbConstant.NotifyIntentData);
				return intent;
			}

		});
	}

	@Override
	public void onDestroy() {

		UsbConnect.UnBindService(mContext);
		super.onDestroy();
	}

}
