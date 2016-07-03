package com.generalscan.activity.bluetoothConnect;

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
import android.widget.Toast;

import com.generalscan.NotifyStyle;
import com.generalscan.OnConnectedListener;
import com.generalscan.OnDataReceive;
import com.generalscan.OnDisconnectListener;
import com.generalscan.OnReadDataReceive;
import com.generalscan.SendConstant;
import com.generalscan.activity.usb.UsbServiceActivity.ReadBroadcast;
import com.generalscan.bluetooth.BluetoothConnect;
import com.generalscan.bluetooth.BluetoothSettings;
import com.generalscan.sdk.R;

/**
 * 测试界面
 * 
 * @author Administrator
 * 
 */
public class BluetoothConnectActivity extends Activity {

	// 开启蓝牙配对
	private Button myBtnBlue;
	// 选择要连接的设备
	private Button myBtnBlueDevice;
	// 连接蓝牙
	private Button myBtnBlueConnect;
	// 关闭连接
	private Button myBtnBlueStop;
	// 发送指定内容
	private Button myBtnSendContent;
	// 发送数据列表
	private Button myBtnSendList;
	// 清空
	private Button myBtnClear;
	private Activity myActivity;

	private ReadBroadcast mReadBroadcast;//读取数据的广播

	
	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.bluetooth_main);
		myActivity = this;
		// 设置提示类型，为无提示
		BluetoothConnect.CurrentNotifyStyle = NotifyStyle.NotificationStyle1;
		// 绑定蓝牙服务(开始必须要进行绑定服务)
		// Bind Bluetooth Service(Must bind service before start)
		BluetoothConnect.BindService(myActivity);

		// 查找控件
		// Find View
		findViewByid();
		// 设置button事件
		// Set button event
		setListener();
		// 获取传递过来的数据
		// Get data
		GetData();
		new Thread() {

			@Override
			public void run() {
				try {
					Thread.sleep(500);
				} catch (InterruptedException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}

				BluetoothConnect.Connect();
			}

		}.start();

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

	
	private void findViewByid() {
		myBtnBlue = (Button) findViewById(R.id.btnBlue);
		myBtnBlueDevice = (Button) findViewById(R.id.btnSet);
		myBtnBlueConnect = (Button) findViewById(R.id.btnConnect);
		myBtnSendContent = (Button) findViewById(R.id.btnSendContent);
		myBtnBlueStop = (Button) findViewById(R.id.btnStop);
		myBtnSendList = (Button) findViewById(R.id.btnStartScan);
		myBtnClear = (Button) findViewById(R.id.btnClear);

	}

	private void setListener() {
		// 显示系统蓝牙设置，开启蓝牙
		// Display System Bluetooth Configuration and Open Bluetooth
		myBtnBlue.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				BluetoothSettings.ACTION_BLUETOOTH_SETTINGS(myActivity);
			}

		});
		// 选择已经配对的蓝牙设备
		// Select the paired bluetooth scanner
		myBtnBlueDevice.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				BluetoothSettings.SetScaner(myActivity);
			}

		});
		// 连接蓝牙
		// Connect bluetooth
		myBtnBlueConnect.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				BluetoothConnect.Connect();
			}

		});
		// 停止连接
		// Stop bluetooth
		myBtnBlueStop.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				BluetoothConnect.Stop(myActivity);
			}

		});

		// 发送命令
		// Send command
		myBtnSendContent.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				// 获取输入的命令
				String text = ((EditText) findViewById(R.id.editText2))
						.getText().toString();
				BluetoothConnect.BluetoothSend(text);
			}

		});

		// 发送列表
		// Send list
		myBtnSendList.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				// 显示所有发送内容，详见SendList类
				// Please see Class SendList
				BluetoothSendList SendList = new BluetoothSendList(myActivity);
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
	
	/**
	 * 设置数据接受 Settings Data
	 */
	private void GetData() {
		
		/**
		 * 连接成功
		 */
		BluetoothConnect.SetOnConnectedListener(new OnConnectedListener() {

			@Override
			public void Connected() {
				Toast.makeText(myActivity, "连接成功", Toast.LENGTH_SHORT).show();
			}

		});
		/**
		 * 断开连接
		 */
		BluetoothConnect.SetOnDisconnectListener(new OnDisconnectListener() {

			@Override
			public void Disconnected() {
				Toast.makeText(myActivity, "断开连接", Toast.LENGTH_SHORT).show();
			}

		});
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
	
	@Override
	protected void onDestroy() {
		BluetoothConnect.UnBindService(myActivity);
		super.onDestroy();
	}

}