package com.generalscan.activity.usb;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;

import com.generalscan.NotifyStyle;
import com.generalscan.OnDataReceive;
import com.generalscan.OnDataReceiveBattery;
import com.generalscan.OnReadDataReceive;
import com.generalscan.sdk.R;
import com.generalscan.usb.UsbConnect;

/**
 * 测试界面
 * 
 * @author Administrator
 * 
 */
public class UsbConnectActivity extends Activity {

	// 发送指定内容
	private Button myBtnSendContent;
	// 发送数据列表
	private Button myBtnSendList;

	private Button myBtnClear;

	private Button mSetChange;

	private Activity myActivity;

	/** Called when the activity is first created. */

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);

		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.usb_main);
		myActivity = this;
		// 开启USB服务
		UsbConnect.BindService(myActivity);
		// 设置连接提示类型
		UsbConnect.CurrentNotifyStyle = NotifyStyle.NotificationStyle1;
		// 查找控件
		// Find View
		findViewByid();
		// 设置button事件
		// Set button event
		setListener();
		// 获取传递过来的数据
		// Get data
		GetData();

		
		
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

	/**
	 * 设置数据接受 Settings Data
	 */
	private void GetData() {
		
		UsbConnect.SetOnDataReceiveBattery(new OnDataReceiveBattery() {
			
			@Override
			public void Battery(String arg0, String arg1) {
				((EditText) findViewById(R.id.editText1)).append(arg1);
			}
		});
		// 监听接受到的数据
		// Receive data from bluetooth scanner
		UsbConnect.SetOnDataReceive(new OnDataReceive() {

			@Override
			public void DataReceive(String data) {

				// ((EditText) findViewById(R.id.editText1)).append(data);
				((TextView) findViewById(R.id.tvGetData)).append(data);
			}

		});

		
		// 发送读取内容，获取的数据信息
		UsbConnect.SetOnReadDataReceive(new OnReadDataReceive() {

			@Override
			public void ReadDataReceive(String Name, String data) {
				// 如果接受到的是充电类型
				if (Name.equals(myActivity.getString(R.string.gs_read_charge))) {
					// 获取0，1标示
					data = data.substring(7, 8);
					if (data.equals("0")) {
						data = myActivity
								.getString(R.string.gs_usb_charge_fast);

					} else {
						data = myActivity
								.getString(R.string.gs_usb_charge_normal);

					}
					((EditText) findViewById(R.id.editText1)).append(Name + ":"
							+ data);
				} else {
					((EditText) findViewById(R.id.editText1)).append(Name + ":"
							+ data);
				}

			}

		});
		
		
	}

	@Override
	protected void onDestroy() {
		UsbConnect.UnBindService(myActivity);
		super.onDestroy();
	}

}