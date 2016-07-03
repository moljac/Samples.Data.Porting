
package com.generalscan.activity.usbhost;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.ActivityInfo;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.text.method.ScrollingMovementMethod;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.widget.Button;
import android.widget.ScrollView;
import android.widget.TextView;

import com.generalscan.SendConstant;
import com.generalscan.sdk.R;
import com.generalscan.usb.suspension.FloatWindowService;
import com.generalscan.usbcontroller.UsbConnectThread;


public class UsbControllerActivity extends Activity {

	private static int sendCount = 0;
	private int readCount = 0;

	private ScrollView mScrollView;
	private TextView mTvData;

	private TextView mTvSendCount;
	private TextView mTvReadCount;
	private Button mBtnConnect;

	private Button mBtnClean;

	private Button mBtnSend;


	private static String mSaveData = "";

	private Activity mActivity;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.main);
		mActivity = this;
		// 日志监听
		// LogcatHelper.getInstance(mActivity).start();

		findViewById();

		setListener();

		// 设置数据
		mTvData.setText(mSaveData);

	}

	private void findViewById() {
		mBtnConnect = ((Button) findViewById(R.id.btnConnect));
		mTvData = ((TextView) findViewById(R.id.tvData));
		mBtnSend = ((Button) findViewById(R.id.btnSend));
		mBtnClean = ((Button) findViewById(R.id.btnClear));
		mTvSendCount = (TextView) findViewById(R.id.tvSendCount);
		mTvReadCount = (TextView) findViewById(R.id.tvReadCount);
		mScrollView = (ScrollView) findViewById(R.id.scrollView1);
	}

	private void setListener() {
		mTvData.setMovementMethod(ScrollingMovementMethod.getInstance());

		UsbConnectThread.start(mActivity);

		mBtnConnect.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				// 连接Usb
				if (FloatWindowService.getInstance(mActivity) != null) {
					FloatWindowService.getInstance(mActivity).usbConnect();
				}
			}
		});

		mBtnSend.setOnClickListener(new View.OnClickListener() {
			@Override
			public void onClick(View v) {

				sendA();
			}
		});
		mBtnClean.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {
				sendCount = 0;
				readCount = 0;
				mTvSendCount.setText(mActivity.getResources().getString(
						R.string.SendCount) + 0);
				mTvReadCount.setText(mActivity.getResources().getString(
						R.string.ReadCount) + 0);
				mTvData.setText("");
				mSaveData = mTvData.getText().toString();
			}
		});
		setReadBroadcast();
		setScreen();

	}

	/**
	 * 设置屏幕转屏
	 */
	private void setScreen() {
		setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_REVERSE_PORTRAIT);

	}

	public void sendA() {
		if (FloatWindowService.getInstance(mActivity) != null) {
			FloatWindowService.getInstance(mActivity).sendA();
		}
		sendCount++;
		mHandler.obtainMessage().sendToTarget();
	}

	private Handler mHandler = new Handler() {

		@Override
		public void handleMessage(Message msg) {

			super.handleMessage(msg);
			mTvSendCount.setText(String.valueOf(sendCount));
		}

	};

	private ReadBroadcast readBroadcast;

	/**
	 * 读取数据的广播
	 */
	private void setReadBroadcast() {
		// 设置数据广播
		readBroadcast = new ReadBroadcast();
		IntentFilter filter = new IntentFilter();
		filter.addAction(SendConstant.GetDataAction);
		filter.addAction(SendConstant.GetReadDataAction);
		registerReceiver(readBroadcast, filter);

	}

	public class ReadBroadcast extends BroadcastReceiver {

		@Override
		public void onReceive(Context context, Intent intent) {

			// 接收数据的广播
			if (intent.getAction().equals(SendConstant.GetDataAction)) {

				String data = intent.getStringExtra(SendConstant.GetData);
				mTvData.append(data);

				// 滑动到最后一行
				// 内层高度超过外层
				int offset = mTvData.getMeasuredHeight()
						- mScrollView.getMeasuredHeight();
				if (offset < 0) {
					offset = 0;
				}
				mScrollView.scrollTo(0, offset);
			}

		}
	}

	@Override
	protected void onDestroy() {

		if (readBroadcast != null) {
			// 取消广播
			this.unregisterReceiver(readBroadcast);
		}
		// 停止日志监听
		// LogcatHelper.getInstance(mActivity).stop();
		super.onDestroy();
	}
}