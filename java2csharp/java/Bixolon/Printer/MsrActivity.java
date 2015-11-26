package com.bixolon.printersample;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.os.Handler;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;

import com.bixolon.printer.BixolonPrinter;

public class MsrActivity extends Activity implements OnClickListener {
	private EditText mTrack1EditText;
	private EditText mTrack2EditText;
	private EditText mTrack3EditText;
	
	private byte[] mTrack1Data;
	private byte[] mTrack2Data;
	private byte[] mTrack3Data;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_msr);
		
		Intent intent = getIntent();
		int extra = intent.getIntExtra(MainActivity.EXTRA_NAME_MSR_MODE, 0);
		EditText editText = (EditText) findViewById(R.id.editText1);
		
		switch (extra) {
		case BixolonPrinter.MSR_MODE_TRACK123_COMMAND:
			editText.setText("Track 1/2/3 read mode command");
			break;
		case BixolonPrinter.MSR_MODE_TRACK1_AUTO:
			editText.setText("Track 1 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_TRACK2_AUTO:
			editText.setText("Track 2 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_TRACK3_AUTO:
			editText.setText("Track 3 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_TRACK12_AUTO:
			editText.setText("Track 1/2 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_TRACK23_AUTO:
			editText.setText("Track 2/3 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_TRACK123_AUTO:
			editText.setText("Track 1/2/3 read mode auto trigger");
			break;
		case BixolonPrinter.MSR_MODE_NOT_USED:
		default:
			editText.setText("MSR not used");
			break;
		}
		
		Button button1 = (Button) findViewById(R.id.button1);
		Button button2 = (Button) findViewById(R.id.button2);
		
		if (extra == BixolonPrinter.MSR_MODE_TRACK123_COMMAND) {
			button1.setOnClickListener(this);
			button2.setOnClickListener(this);
		} else {
			button1.setEnabled(false);
			button2.setEnabled(false);
		}
		
		mTrack1EditText = (EditText) findViewById(R.id.editText2);
		mTrack2EditText = (EditText) findViewById(R.id.editText3);
		mTrack3EditText = (EditText) findViewById(R.id.editText4);
		
		IntentFilter filter = new IntentFilter();
		filter.addAction(MainActivity.ACTION_GET_MSR_TRACK_DATA);
		registerReceiver(mReceiver, filter);
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		unregisterReceiver(mReceiver);
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			MainActivity.mBixolonPrinter.setMsrReaderMode();
			break;
			
		case R.id.button2:
			MainActivity.mBixolonPrinter.cancelMsrReaderMode();
			break;
		}
	}
	
	BroadcastReceiver mReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			if (intent.getAction().equals(MainActivity.ACTION_GET_MSR_TRACK_DATA)) {
				Bundle bundle = intent.getBundleExtra(MainActivity.EXTRA_NAME_MSR_TRACK_DATA);
				mTrack1EditText.setText("");
				mTrack2EditText.setText("");
				mTrack3EditText.setText("");
				
				mTrack1Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK1);
				if (mTrack1Data != null) {
					new Handler().postDelayed(new Runnable() {
						
						@Override
						public void run() {
							mTrack1EditText.setText(new String(mTrack1Data));
						}
					}, 100);
				}
				
				mTrack2Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK2);
				if (mTrack2Data != null) {
					new Handler().postDelayed(new Runnable() {
						
						@Override
						public void run() {
							mTrack2EditText.setText(new String(mTrack2Data));
						}
					}, 100);
				}
				
				mTrack3Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK3);
				if (mTrack3Data != null) {
					new Handler().postDelayed(new Runnable() {
						
						@Override
						public void run() {
							mTrack3EditText.setText(new String(mTrack3Data));
						}
					}, 100);
				}
			}
		}
	};
}
