package com.generalscan.activity;

import android.app.ListActivity;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.view.View;
import android.view.Window;
import android.widget.ArrayAdapter;
import android.widget.ListAdapter;
import android.widget.ListView;

import com.generalscan.activity.bluetoothConnect.BluetoothConnectActivity;
import com.generalscan.activity.speech.SpeechSDKActivity;
import com.generalscan.activity.usb.UsbConnectActivity;
import com.generalscan.activity.usb.UsbServiceActivity;
import com.generalscan.activity.usbhost.UsbControllerActivity;
import com.generalscan.sdk.R;

public class MainActivity extends ListActivity {
	private String[] note_array;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);

		PackageManager pm = getPackageManager();
		PackageInfo pi = null;
		try {
			pi = pm.getPackageInfo("com.generalscan.sdk", 0);
		} catch (NameNotFoundException e) {
			e.printStackTrace();
		}

		note_array = new String[6];
		note_array[0] = this.getResources().getString(R.string.bluetooth);
		note_array[1] = this.getResources().getString(R.string.usb);
		note_array[2] = this.getResources().getString(R.string.tts);
		note_array[3] = this.getResources().getString(R.string.usbService);
		note_array[4] = this.getResources().getString(R.string.usbHost);
		note_array[5] = this.getResources().getString(R.string.edit) + pi.versionName;

		setAdapter();
	}

	private void setAdapter() {

		ListAdapter adapter = new ArrayAdapter<String>(this,
				android.R.layout.simple_list_item_1, note_array);

		setListAdapter(adapter);

	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		Intent intent = null;
		switch (position) {
		case 0:
			intent = new Intent(this, BluetoothConnectActivity.class);
			break;
		case 1:
			intent = new Intent(this, UsbConnectActivity.class);
			break;
		case 2:
			intent = new Intent(this, SpeechSDKActivity.class);
			break;
		case 3:
			intent = new Intent(this, UsbServiceActivity.class);
			break;
		case 4:
			intent = new Intent(this, UsbControllerActivity.class);
			break;
		}
		if (intent != null) {
			startActivity(intent);
		}
	}

}
