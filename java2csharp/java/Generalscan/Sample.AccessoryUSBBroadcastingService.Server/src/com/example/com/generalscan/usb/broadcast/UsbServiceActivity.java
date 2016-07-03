package com.example.com.generalscan.usb.broadcast;

import java.util.ArrayList;

import android.app.Activity;
import android.app.ActivityManager;
import android.app.ActivityManager.RunningServiceInfo;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.generalscan.SendConstant;

public class UsbServiceActivity extends Activity {

	private Button mBtnStart;//开启服务
	private Button mBtnStop;//停止服务
	private Button mBtnNew;//新界面
	private Button myBtnClear;//清空数据
	private CheckBox mCkbAuto;//是否自动启动
	private Activity mActivity;
	private ReadBroadcast readBroadcast;

	private Button mBtnApp;//选择APP
	private TextView mTvApp;//显示当前APP 
	private Button mBtnStartApp;//开始APP
	private boolean isFinish = false;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		mActivity = this;
		this.setContentView(R.layout.usb_service);
		findViewbyID();
		setListener();
		initValue();
		getIntentData();

	}

	private void getIntentData() {

		// 判断是否是从NOTIFY中启动
		Intent intent = this.getIntent();
		if (intent.hasExtra(UsbConstant.NotifyIntent)) {
			String str = intent.getStringExtra(UsbConstant.NotifyIntent);
			if (str.equals(UsbConstant.NotifyIntentData)) {
				isFinish = false;
				//显示默认启动程序
				String appName = USBReadShared.getAppName(mActivity);
				if (appName != null && !appName.equals("")) {
					mTvApp.setText(mTvApp.getText()+appName);
				} 
			}
		} else {
			isFinish = true;
		}
		if (isFinish) {
			// 判断是否有默认启动应用程序
			String packetName = USBReadShared.getPacketName(mActivity);
			if (packetName != null && !packetName.equals("")) {
				if(AppFilter.startAPP(mActivity, packetName)){
					isFinish = true;
				}else{
					//没有启动选项，不关闭
					isFinish = false;
				}
				
			} else {
				isFinish = false;
			}
		}
		if (isFinish) {
			this.finish();
		}
	}

	private void setBroadcast() {
		// 设置数据广播
		readBroadcast = new ReadBroadcast();
		IntentFilter filter = new IntentFilter();
		filter.addAction(SendConstant.GetDataAction);
		filter.addAction(SendConstant.GetReadDataAction);
		filter.addAction(UsbConstant.ConnectedAction);
		filter.addAction(UsbConstant.DisconnectAction);
		registerReceiver(readBroadcast, filter);

	}

	private void findViewbyID() {
		mBtnStart = (Button) this.findViewById(R.id.button1);
		mBtnStop = (Button) this.findViewById(R.id.button2);
		mBtnNew = (Button) this.findViewById(R.id.button3);
		myBtnClear = (Button) findViewById(R.id.btnClear);
		mCkbAuto = (CheckBox) findViewById(R.id.ckbAuto);
		mBtnApp = (Button) findViewById(R.id.btnApp);
	
		mTvApp= (TextView) findViewById(R.id.tvAppName);
		mBtnStartApp= (Button) findViewById(R.id.btnStart);
		}

	private void setListener() {
		mBtnStart.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mActivity, UsbService.class);
				mActivity.startService(intent);
			}

		});
		mBtnStop.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mActivity, UsbService.class);
				mActivity.stopService(intent);
			}

		});
		mBtnNew.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mActivity, UsbDataActivity.class);
				mActivity.startActivity(intent);
			}

		});
		myBtnClear.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				
				((TextView) findViewById(R.id.tvGetData)).setText("");
			}

		});
		mCkbAuto.setOnCheckedChangeListener(new OnCheckedChangeListener() {

			@Override
			public void onCheckedChanged(CompoundButton arg0, boolean arg1) {
				// 保存状态
				USBSaveShared.SaveAutoConnect(mActivity, arg1);

			}

		});
		mBtnApp.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mActivity, AppListActivity.class);
				mActivity.startActivityForResult(intent, 0);
			}

		});
		mBtnStartApp.setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// 判断是否有默认启动应用程序
				String packetName = USBReadShared.getPacketName(mActivity);
				if (packetName != null && !packetName.equals("")) {
					AppFilter.startAPP(mActivity, packetName);
					mActivity.finish();
				}else{
					//Toast.makeText(mActivity, "请选择默认开启程序", Toast.LENGTH_SHORT).show();
				}
			}
		});
	}

	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		if (requestCode == 0) {
			this.finish();
		}
	}

	private void initValue() {
		if (USBReadShared.AutoConnect(mActivity)) {
			mCkbAuto.setChecked(true);
			// 自动开启服务
			Intent intent = new Intent();
			intent.setClass(mActivity, UsbService.class);
			mActivity.startService(intent);
		} else {
			mCkbAuto.setChecked(false);
		}
		if (isWorked()) {
			mBtnStart.setEnabled(false);
			mBtnStop.setEnabled(true);
		} else {
			mBtnStart.setEnabled(true);
			mBtnStop.setEnabled(false);
		}

	}

	public boolean isWorked() {
		ActivityManager myManager = (ActivityManager) getSystemService(Context.ACTIVITY_SERVICE);
		ArrayList<RunningServiceInfo> runningService = (ArrayList<RunningServiceInfo>) myManager
				.getRunningServices(30);
		for (int i = 0; i < runningService.size(); i++) {
			if (runningService.get(i).service
					.getClassName()
					.toString()
					.equals("com.example.com.generalscan.usb.broadcast.UsbService")) {
				return true;
			}
		}
		return false;
	}

	public class ReadBroadcast extends BroadcastReceiver {

		public ReadBroadcast() {

		}

		@Override
		public void onReceive(Context context, Intent intent) {
			// 连接成功
			if (intent.getAction().equals(UsbConstant.ConnectedAction)) {
				mBtnStart.setEnabled(false);
				mBtnStop.setEnabled(true);

			} else if (intent.getAction().equals(UsbConstant.DisconnectAction)) {
				mBtnStart.setEnabled(true);
				mBtnStop.setEnabled(false);
			}

			// 接收数据的广播
			else if (intent.getAction().equals(SendConstant.GetDataAction)) {

				String data = intent.getStringExtra(SendConstant.GetData);

				((TextView) findViewById(R.id.tvGetData)).append(data);
			}
		}
	}

	@Override
	protected void onStart() {
		// 设置读取数据
		setBroadcast();
		super.onStart();
	}

	@Override
	protected void onStop() {
		if (readBroadcast != null) {
			// 取消广播
			this.unregisterReceiver(readBroadcast);
		}
		super.onStop();
	}
}
