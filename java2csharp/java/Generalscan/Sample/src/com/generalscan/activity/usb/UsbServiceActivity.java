package com.generalscan.activity.usb;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import com.generalscan.SendConstant;
import com.generalscan.sdk.R;

public class UsbServiceActivity extends Activity {

	private Button mBtnStart;//开始服务
	private Button mBtnStop;//停止服务
	private Button mBtnNew;//新界面
	private Button myBtnClear;//清空数据
	private Context mContext;
	private ReadBroadcast mReadBroadcast;//读取数据的广播

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		mContext = this;
		this.setContentView(R.layout.usb_service);

		findViewbyID();
		setListener();
		
	}
	/**
	 * 设置广播
	 */
	private void setBroadcast() {
		// 设置数据广播
		mReadBroadcast = new ReadBroadcast();
		IntentFilter filter = new IntentFilter();
		filter.addAction(SendConstant.GetDataAction);
		filter.addAction(SendConstant.GetReadDataAction);
		filter.addAction(SendConstant.GetBatteryDataAction);
		registerReceiver(mReadBroadcast, filter);

	}

	private void findViewbyID() {
		mBtnStart = (Button) this.findViewById(R.id.button1);
		mBtnStop = (Button) this.findViewById(R.id.button2);
		mBtnNew = (Button) this.findViewById(R.id.button3);
		myBtnClear = (Button) findViewById(R.id.btnClear);
	}

	private void setListener() {
		//开启服务
		mBtnStart.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mContext, UsbService.class);
				mContext.startService(intent);
			}

		});
		//停止服务
		mBtnStop.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mContext, UsbService.class);
				mContext.stopService(intent);
			}

		});
		//新的界面
		mBtnNew.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				Intent intent = new Intent();
				intent.setClass(mContext, UsbDataActivity.class);
				mContext.startActivity(intent);
			}

		});
		//清空数据
		myBtnClear.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				((EditText) findViewById(R.id.editText1)).setText("");
				((TextView) findViewById(R.id.tvGetData)).setText("");
			}

		});
	}
	/**
	 * 广播接收器
	 * @author Administrator
	 *
	 */
	public class ReadBroadcast extends BroadcastReceiver {

		public ReadBroadcast() {

		}

		@Override
		public void onReceive(Context context, Intent intent) {
			//接受电量数据广播
			if (intent.getAction().equals(SendConstant.GetBatteryDataAction)) {
				
				String data = intent.getStringExtra(SendConstant.GetBatteryData);

				((EditText) findViewById(R.id.editText1)).append(data);
			}
			// 接收数据的广播
			if (intent.getAction().equals(SendConstant.GetDataAction)) {

				String data = intent.getStringExtra(SendConstant.GetData);

				((TextView) findViewById(R.id.tvGetData)).append(data);
			}
			// 接收发送数据的广播
			if (intent.getAction().equals(SendConstant.GetReadDataAction)) {
				String name = intent.getStringExtra(SendConstant.GetReadName);
				String data = intent.getStringExtra(SendConstant.GetReadData);

				// 如果接受到的是充电类型
				if (name.equals(mContext.getString(R.string.gs_read_charge))) {
					// 获取0，1标示
					data = data.substring(7, 8);
					if (data.equals("0")) {
						data = mContext
								.getString(R.string.gs_usb_charge_fast);

					} else {
						data = mContext
								.getString(R.string.gs_usb_charge_normal);

					}
					((EditText) findViewById(R.id.editText1)).append(name + ":"
							+ data);
				} else {
					((EditText) findViewById(R.id.editText1)).append(name + ":"
							+ data);
				}
			}
		}

	}

	@Override
	protected void onStart() {
		// 设置读取数据的广播
		setBroadcast();
		super.onStart();
	}

	@Override
	protected void onStop() {
		if (mReadBroadcast != null) {
			// 取消广播
			this.unregisterReceiver(mReadBroadcast);
		}
		super.onStop();
	}
}
