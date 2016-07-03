package com.generalscan.activity.usb;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import com.generalscan.SendConstant;
import com.generalscan.sdk.R;
import com.generalscan.usb.UsbConnect;

public class UsbDataActivity extends Activity {

	// 发送指定内容
	private Button myBtnSendContent;
	// 发送数据列表
	private Button myBtnSendList;

	private Button myBtnClear;

	private Button mSetChange;

	private Activity myActivity;

	private ReadBroadcast readBroadcast;

	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.usb_main);
		myActivity = this;
		// 查找控件
		// Find View
		findViewByid();
		// 设置button事件
		// Set button event
		setListener();

	}

	private void setBroadcast() {
		// 设置数据广播
		readBroadcast = new ReadBroadcast();
		IntentFilter filter = new IntentFilter();
		filter.addAction(SendConstant.GetDataAction);
		filter.addAction(SendConstant.GetReadDataAction);

		registerReceiver(readBroadcast, filter);

	}

	private void findViewByid() {

		myBtnSendContent = (Button) findViewById(R.id.btnSendContent);

		myBtnSendList = (Button) findViewById(R.id.btnStartScan);

		myBtnClear = (Button) findViewById(R.id.btnClear);
		mSetChange = (Button) findViewById(R.id.btnSetChange);
	}

	private void setListener() {

		// 发送命令
		// Send command
		myBtnSendContent.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				// 获取输入的命令
				String text = ((EditText) findViewById(R.id.editText2))
						.getText().toString();
				UsbConnect.UsbSend(text);
			}

		});

		// 发送列表
		// Send list
		myBtnSendList.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				// 显示所有发送内容，详见SendList类
				// Please see Class SendList
				UsbSendList SendList = new UsbSendList(myActivity);
				SendList.ShowoDialog();
			}

		});

		myBtnClear.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				((EditText) findViewById(R.id.editText1)).setText("");
				((TextView) findViewById(R.id.tvGetData)).setText("");
			}

		});
		// 设置充电模式
		mSetChange.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				EditText edt1 = ((EditText) findViewById(R.id.edt1));
				String max = edt1.getText().toString();
				EditText edt2 = ((EditText) findViewById(R.id.edt2));
				String min = edt2.getText().toString();
				EditText edt3 = ((EditText) findViewById(R.id.edt3));
				String change = edt3.getText().toString();
				try {
					UsbConnect.setNormalCharge(Integer.valueOf(max));
					UsbConnect.setFastCharge(Integer.valueOf(min));
					UsbConnect.setChargeWarn(Integer.valueOf(change));

				} catch (Exception ex) {

				}
			}

		});
	}

	public class ReadBroadcast extends BroadcastReceiver {

		public ReadBroadcast() {

		}

		@Override
		public void onReceive(Context context, Intent intent) {
			
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
				if (name.equals(myActivity.getString(R.string.gs_read_charge))) {
					// 获取0，1标示
					data = data.substring(7, 8);
					if (data.equals("0")) {
						data = myActivity
								.getString(R.string.gs_usb_charge_fast);

					} else {
						data = myActivity
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
		if (readBroadcast != null) {
			// 取消广播
			this.unregisterReceiver(readBroadcast);
		}
		super.onStop();
	}

}
