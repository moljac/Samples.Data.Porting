package com.example.generalscanusbbroadcasttest;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;

public class MainActivity extends Activity {

	private Button myBtnClear;
	private Context mContext;
	private ReadBroadcast readBroadcast;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		mContext = this;
		this.setContentView(R.layout.usb_service);

		findViewbyID();
		setListener();

	}

	private void setBroadcast() {
		// 设置数据广播  Setting for Broadcast 
		readBroadcast = new ReadBroadcast();
		IntentFilter filter = new IntentFilter();
		filter.addAction(SendConstant.GetDataAction);
		filter.addAction(SendConstant.GetReadDataAction);

		registerReceiver(readBroadcast, filter);

	}

	private void findViewbyID() {

		myBtnClear = (Button) findViewById(R.id.btnClear);
	}

	private void setListener() {

		myBtnClear.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				((TextView) findViewById(R.id.tvGetData)).setText("");
			}

		});
	}

	public class ReadBroadcast extends BroadcastReceiver {

		public ReadBroadcast() {

		}

		@Override
		public void onReceive(Context context, Intent intent) {

			// 接收数据  Receive Data
			if (intent.getAction().equals(SendConstant.GetDataAction)) {

				String data = intent.getStringExtra(SendConstant.GetData);

				((TextView) findViewById(R.id.tvGetData)).append(data);
			}

		}

	}

	@Override
	protected void onStart() {
		// 设置读取数据的广播   Setting for broadcast
		setBroadcast();
		super.onStart();
	}

	@Override
	protected void onStop() {
		if (readBroadcast != null) {
			// 取消广播   Cancel for broadcast
			this.unregisterReceiver(readBroadcast);
		}
		super.onStop();
	}
}
