package com.bixolon.labelprintersample;

import java.util.ArrayList;
import java.util.Set;

import android.annotation.SuppressLint;
import android.app.ActionBar;
import android.app.AlertDialog;
import android.app.ListActivity;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Handler;
import android.os.Message;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.Toast;

import com.bixolon.labelprinter.BixolonLabelPrinter;

@SuppressLint("HandlerLeak")
public class MainActivity extends ListActivity {

	private static final String[] FUNCTIONS = {
		"drawText",
		"drawVectorFontText",
		"draw1dBarcode",
		"drawMaxicode",
		"drawPdf417",
		"drawQrCode",
		"drawDataMatrix",
		"drawBlock",
		"drawCircle",
		"setCharacterSet",
		"setPrintingType",
		"setMargin",
		"setBackFeedOption",
		"setLength",
		"setWidth",
		"setBufferMode",
		"clearBuffer",
		"setSpeed",
		"setDensity",
		"setOrientation",
		"setOffset",
		"setCutterPosition",
		"drawBitmap",
		"initializePrinter",
		"printInformation",
		"setAutoCutter",
		"getStatus",
		"getPrinterInformation",
		"executeDirectIo"
	};

	// Name of the connected device
	private String mConnectedDeviceName = null;

	private ListView mListView;
	private AlertDialog mWifiDialog;
	private AlertDialog mPrinterInformationDialog;
	private AlertDialog mSetPrintingTypeDialog;
	private AlertDialog mSetMarginDialog;
	private AlertDialog mSetWidthDialog;
	private AlertDialog mSetLengthDialog;
	private AlertDialog mSetBufferModeDialog;
	private AlertDialog mSetSpeedDialog;
	private AlertDialog mSetDensityDialog;
	private AlertDialog mSetOrientationDialog;
	private AlertDialog mSetOffsetDialog;
	private AlertDialog mCutterPositionSettingDialog;
	private AlertDialog mAutoCutterDialog;
	private AlertDialog mSetBackfeedDialog;
	private AlertDialog mGetCharacterSetDialog;

	private boolean mIsConnected;

	static BixolonLabelPrinter mBixolonLabelPrinter;
	
	private boolean checkedManufacture = false;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		ArrayList<String> list = new ArrayList<String>();
		for (int i = 0; i < FUNCTIONS.length; i++) {
			list.add(FUNCTIONS[i]);
		}

		ArrayAdapter<String> adapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, list);
		mListView = (ListView) findViewById(android.R.id.list);
		mListView.setAdapter(adapter);
		mListView.setEnabled(false);

		mBixolonLabelPrinter = new BixolonLabelPrinter(this, mHandler, null);
	}

	@Override
	public void onDestroy() {
		try {
			unregisterReceiver(mUsbReceiver);
		} catch (IllegalArgumentException e) {
			e.printStackTrace();
		}
		mBixolonLabelPrinter.disconnect();
		super.onDestroy();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;
	}

	@Override
	public boolean onPrepareOptionsMenu(Menu menu) {
		if (mIsConnected) {
			menu.getItem(0).setEnabled(false);
			menu.getItem(1).setEnabled(false);
			menu.getItem(2).setEnabled(false);
			menu.getItem(3).setEnabled(true);
		} else {
			menu.getItem(0).setEnabled(true);
			menu.getItem(1).setEnabled(true);
			menu.getItem(2).setEnabled(true);
			menu.getItem(3).setEnabled(false);
		}
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.item1:
			mBixolonLabelPrinter.findBluetoothPrinters();
			break;

		case R.id.item2:
			mBixolonLabelPrinter.findNetworkPrinters(3000);
			return true;

		case R.id.item3:
			if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR1) {
				mBixolonLabelPrinter.findUsbPrinters();
			}
			return true;
		

		case R.id.item4:
			mBixolonLabelPrinter.disconnect();
			return true;

		}
		return false;
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		switch (position) {
		case 0:	// drawText
			Intent intent = new Intent(MainActivity.this, DrawTextActivity.class);
			startActivity(intent);
			break;
			
		case 1:	// drawVectorFontText
			intent = new Intent(MainActivity.this, DrawVectorTextActivity.class);
			startActivity(intent);
			break;
			
		case 2:	// draw1dBarcode
			intent = new Intent(MainActivity.this, Draw1dBarcodeActivity.class);
			startActivity(intent);
			break;
			
		case 3:	// drawMaxicode
			intent = new Intent(MainActivity.this, DrawMaxicodeActivity.class);
			startActivity(intent);
			break;
			
		case 4:	// drawPdf417
			intent = new Intent(MainActivity.this, DrawPdf417Activity.class);
			startActivity(intent);
			break;
			
		case 5:	// drawQrCode
			intent = new Intent(MainActivity.this, DrawQrCodeActivity.class);
			startActivity(intent);
			break;
			
		case 6:	// drawDataMatrix
			intent = new Intent(MainActivity.this, DrawDataMatrixActivity.class);
			startActivity(intent);
			break;
			
		case 7:	// drawBlock
			intent = new Intent(MainActivity.this, DrawBlockActivity.class);
			startActivity(intent);
			break;
			
		case 8:	// drawCircle
			intent = new Intent(MainActivity.this, DrawCircleActivity.class);
			startActivity(intent);
			break;
			
		case 9:	// setCharacterSet
			intent = new Intent(MainActivity.this, CharacterSetSelectionActivity.class);
			startActivity(intent);
			break;
			
		case 10:	// setPrintingType
			DialogManager.showSetPrintingTypeDialog(mSetPrintingTypeDialog, MainActivity.this);
			break;
			
		case 11:	// setMargin
			DialogManager.showSetMarginDialog(mSetMarginDialog, MainActivity.this);
			break;
			
		case 12:	// setBackFeedOption
			DialogManager.showSeBackFeedDialog(mSetBackfeedDialog, MainActivity.this);
			break;
			
		case 13:	// setLength
			DialogManager.showSetLengthDialog(mSetLengthDialog, MainActivity.this);
			break;
			
		case 14:	// setWidth
			DialogManager.showSetWidthDialog(mSetWidthDialog, MainActivity.this);
			break;
			
		case 15:	// setBufferMode
			DialogManager.showSetBufferModeDialog(mSetBufferModeDialog, MainActivity.this);
			break;
			
		case 16:	// clearBuffer
			mBixolonLabelPrinter.clearBuffer();
			break;
			
		case 17:	// setSpeed
			DialogManager.showSetSpeedDialog(mSetSpeedDialog, MainActivity.this);
			break;
			
		case 18:	// setDensity
			DialogManager.showSetDensityDialog(mSetDensityDialog, MainActivity.this);
			break;
			
		case 19:	// setOrientation
			DialogManager.showSetOrientationDialog(mSetOrientationDialog, MainActivity.this);
			break;
			
		case 20:	// setOffset
			DialogManager.showSetOffsetDialog(mSetOffsetDialog, MainActivity.this);
			break;
			
		case 21:	// setCutterPosition
			DialogManager.showCutterPositionSettingDialog(mCutterPositionSettingDialog, MainActivity.this);
			break;
			
		case 22:	// drawBitmap
			intent = new Intent(MainActivity.this, DrawBitmapActivity.class);
			startActivity(intent);
			break;
			
		case 23:	// initializePrinter
			mBixolonLabelPrinter.initializePrinter();
			break;
			
		case 24:	// printInformation
			mBixolonLabelPrinter.printInformation();
			break;
			
		case 25:	// setAutoCutter
			DialogManager.showAutoCutterDialog(mAutoCutterDialog, MainActivity.this);
			break;
			
		case 26:	// getStatus
			mBixolonLabelPrinter.getStatus(true);
			break;
			
		case 27:	// getPrinterInformation
			DialogManager.showPrinterInformationDialog(mPrinterInformationDialog, MainActivity.this, mBixolonLabelPrinter);
			break;
			
		case 28:	// executeDirectIo
			intent = new Intent(MainActivity.this, DirectIoActivity.class);
			startActivity(intent);
			break;
		}
	}

	private final void setStatus(int resId) {
		final ActionBar actionBar = getActionBar();
		actionBar.setSubtitle(resId);
	}

	private final void setStatus(CharSequence subtitle) {
		final ActionBar actionBar = getActionBar();
		actionBar.setSubtitle(subtitle);
	}

	@SuppressLint("HandlerLeak")
	private void dispatchMessage(Message msg) {
		switch (msg.arg1) {
		case BixolonLabelPrinter.PROCESS_GET_STATUS:
			byte[] report = (byte[]) msg.obj;
			StringBuffer buffer = new StringBuffer();
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_PAPER_EMPTY) == BixolonLabelPrinter.STATUS_1ST_BYTE_PAPER_EMPTY) {
				buffer.append("Paper Empty.\n");
			}
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_COVER_OPEN) == BixolonLabelPrinter.STATUS_1ST_BYTE_COVER_OPEN) {
				buffer.append("Cover open.\n");
			}
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_CUTTER_JAMMED) == BixolonLabelPrinter.STATUS_1ST_BYTE_CUTTER_JAMMED) {
				buffer.append("Cutter jammed.\n");
			}
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_TPH_OVERHEAT) == BixolonLabelPrinter.STATUS_1ST_BYTE_TPH_OVERHEAT) {
				buffer.append("TPH(thermal head) overheat.\n");
			}
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_AUTO_SENSING_FAILURE) == BixolonLabelPrinter.STATUS_1ST_BYTE_AUTO_SENSING_FAILURE) {
				buffer.append("Gap detection error. (Auto-sensing failure)\n");
			}
			if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_RIBBON_END_ERROR) == BixolonLabelPrinter.STATUS_1ST_BYTE_RIBBON_END_ERROR) {
				buffer.append("Ribbon end error.\n");
			}

			if (report.length == 2) {
				if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_BUILDING_IN_IMAGE_BUFFER) == BixolonLabelPrinter.STATUS_2ND_BYTE_BUILDING_IN_IMAGE_BUFFER) {
					buffer.append("On building label to be printed in image buffer.\n");
				}
				if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_PRINTING_IN_IMAGE_BUFFER) == BixolonLabelPrinter.STATUS_2ND_BYTE_PRINTING_IN_IMAGE_BUFFER) {
					buffer.append("On printing label in image buffer.\n");
				}
				if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_PAUSED_IN_PEELER_UNIT) == BixolonLabelPrinter.STATUS_2ND_BYTE_PAUSED_IN_PEELER_UNIT) {
					buffer.append("Issued label is paused in peeler unit.\n");
				}
			}
			if (buffer.length() == 0) {
				buffer.append("No error");
			}
			Toast.makeText(getApplicationContext(), buffer.toString(), Toast.LENGTH_SHORT).show();
			break;
			
		case BixolonLabelPrinter.PROCESS_GET_INFORMATION_MODEL_NAME:
		case BixolonLabelPrinter.PROCESS_GET_INFORMATION_FIRMWARE_VERSION:
		case BixolonLabelPrinter.PROCESS_EXECUTE_DIRECT_IO:
			Toast.makeText(getApplicationContext(), (String) msg.obj, Toast.LENGTH_SHORT).show();
			break;
		}
	}

	private final Handler mHandler = new Handler() {
		@Override
		public void handleMessage(Message msg) {
			switch (msg.what) {
			case BixolonLabelPrinter.MESSAGE_STATE_CHANGE:
				switch (msg.arg1) {
				case BixolonLabelPrinter.STATE_CONNECTED:
					setStatus(getString(R.string.title_connected_to, mConnectedDeviceName));
					mListView.setEnabled(true);
					mIsConnected = true;
					invalidateOptionsMenu();
					break;

				case BixolonLabelPrinter.STATE_CONNECTING:
					setStatus(R.string.title_connecting);
					break;

				case BixolonLabelPrinter.STATE_NONE:
					setStatus(R.string.title_not_connected);
					mListView.setEnabled(false);
					mIsConnected = false;
					invalidateOptionsMenu();
					break;
				}
				break;

			case BixolonLabelPrinter.MESSAGE_READ:
				MainActivity.this.dispatchMessage(msg);
				break;

			case BixolonLabelPrinter.MESSAGE_DEVICE_NAME:
				mConnectedDeviceName = msg.getData().getString(BixolonLabelPrinter.DEVICE_NAME);
				Toast.makeText(getApplicationContext(), mConnectedDeviceName, Toast.LENGTH_LONG).show();
				break;

			case BixolonLabelPrinter.MESSAGE_TOAST:
				mListView.setEnabled(false);
				Toast.makeText(getApplicationContext(), msg.getData().getString(BixolonLabelPrinter.TOAST), Toast.LENGTH_SHORT).show();
				break;
				
			case BixolonLabelPrinter.MESSAGE_BLUETOOTH_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No paired device", Toast.LENGTH_SHORT).show();
				} else {
					DialogManager.showBluetoothDialog(MainActivity.this, (Set<BluetoothDevice>) msg.obj);
				}
				break;
				
				case BixolonLabelPrinter.MESSAGE_USB_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No connected device", Toast.LENGTH_SHORT).show();
				} else {
					DialogManager.showUsbDialog(MainActivity.this, (Set<UsbDevice>) msg.obj, mUsbReceiver);
				}
				break;
				
				case BixolonLabelPrinter.MESSAGE_NETWORK_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No connectable device", Toast.LENGTH_SHORT).show();
				}
				DialogManager.showNetworkDialog(MainActivity.this, (Set<String>) msg.obj);
				break;
				
			}
		}
	};

	private BroadcastReceiver mUsbReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			String action = intent.getAction();

			if (UsbManager.ACTION_USB_DEVICE_ATTACHED.equals(action)) {
				mBixolonLabelPrinter.connect();
				Toast.makeText(getApplicationContext(), "Found USB device", Toast.LENGTH_SHORT).show();
			} else if (UsbManager.ACTION_USB_DEVICE_DETACHED.equals(action)) {
				mBixolonLabelPrinter.disconnect();
				Toast.makeText(getApplicationContext(), "USB device removed", Toast.LENGTH_SHORT).show();
			}

		}
	};
}
