package com.bixolon.printersample;

import java.io.File;
import java.io.FilenameFilter;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.Locale;
import java.util.Set;

import android.app.ActionBar;
import android.app.AlertDialog;
import android.app.Dialog;
import android.app.DialogFragment;
import android.app.ListActivity;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbManager;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.os.StrictMode;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.SimpleAdapter;
import android.widget.Toast;

import com.bixolon.printer.BixolonPrinter;

public class MainActivity extends ListActivity {
	
	public static final String TAG = "BixolonPrinterSample";
	
	static final String ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES = "com.bixolon.anction.GET_DEFINED_NV_IMAGE_KEY_CODES";
	static final String ACTION_COMPLETE_PROCESS_BITMAP = "com.bixolon.anction.COMPLETE_PROCESS_BITMAP";
	static final String ACTION_GET_MSR_TRACK_DATA = "com.bixolon.anction.GET_MSR_TRACK_DATA";
	static final String EXTRA_NAME_NV_KEY_CODES = "NvKeyCodes";
	static final String EXTRA_NAME_MSR_MODE = "MsrMode";
	static final String EXTRA_NAME_MSR_TRACK_DATA = "MsrTrackData";
	static final String EXTRA_NAME_BITMAP_WIDTH = "BitmapWidth";
	static final String EXTRA_NAME_BITMAP_HEIGHT = "BitmapHeight";
	static final String EXTRA_NAME_BITMAP_PIXELS = "BitmapPixels";
	
	static final int REQUEST_CODE_SELECT_FIRMWARE = Integer.MAX_VALUE;
	static final int RESULT_CODE_SELECT_FIRMWARE = Integer.MAX_VALUE - 1;
	static final int MESSAGE_START_WORK = Integer.MAX_VALUE - 2;
	static final int MESSAGE_END_WORK = Integer.MAX_VALUE - 3;
	
	static final String FIRMWARE_FILE_NAME = "FirmwareFileName";

	// Name of the connected device
	private String mConnectedDeviceName = null;
	
	private ListView mListView;
	private ProgressBar mProgressBar;
	
	private AlertDialog mPdf417Dialog;
	private AlertDialog mQrCodeDialog;
	private AlertDialog mMaxiCodeDialog;
	private AlertDialog mDataMatrixDialog;
	private AlertDialog mCodePageDialog;
	private AlertDialog mPrinterIdDialog;
	private AlertDialog mPrintSpeedDialog;
	private AlertDialog mPrintDensityDialog;
	private AlertDialog mDirectIoDialog;
	private AlertDialog mPowerSavingModeDialog;
	private AlertDialog mBsCodePageDialog;
	private AlertDialog mPrintColorDialog;
	
	static BixolonPrinter mBixolonPrinter;
	
	private static final String[][] FUNCTIONS = {
		// Status check
		{"getStatus", "Supported all model"},							// 0
		{"enable automatic status back", "Supported all model"},		// 1
		{"disable automatic status back", "Supported all model"},		// 2
		{"printSelfTest", "Supported all model"},						// 3
		
		// Configuration
		{"getPrinterId", "Supported all model"},												// 4
		{"setCodePage", "Supported all model (2 bytes font is POS printers only)"},				// 5
		{"getPrintSpeed", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},		// 6
		{"setPrintSpeed", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},		// 7
		{"getPrintDensity", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},	// 8
		{"setPrintDensity", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},	// 9
		{"getPowerSavingMode", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},// 10
		{"setPowerSavingMode", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},// 11
		{"set page mode", "Supported SPP-100II, SPP-R200II, SPP-R300, SPP-R400, SRP-350IIOBE"},	// 12
		{"initialize", "Supported all model"},													// 13
		{"getBsCodePage", "Supported SRP-275II only"},											// 14
		{"setBsCodePage", "Supported SRP-275II only"},											// 15
		{"setPrintColor", "Supported SRP-275II only"},											// 16
		
		// Print
		{"printText", "Supported all model"},							// 17
		{"print1dBarcode", "Supported all model except for SRP-275II"},	// 18
		{"printPdf417", "Supported all model except for SRP-275II"},	// 19
		{"printQrCode", "Supported all model except for SRP-275II"},	// 20
		{"printMaxiCode", "Supported SPP-R200II, SPP-R300, SPP-R400"},	// 21
		{"printDataMatrix", "Supported SPP-R200II, SPP-R300, SPP-R400"},// 22
		{"printBitmap", "Supported all model"},							// 23
		{"printPdfFiles", "Supported all model"},						// 24
		
		// Additional functions
		{"kickOutDrawer", "Supported POS printer only"},					// 25
		{"executeDirectIo", "Supported all model"},							// 26
		{"NV image manager", "Supported all model except for SRP-275II"},	// 27
		{"updateFirmware", "Supported all model"},							// 28
		{"MSR manager", "Supported SPP-R200II, SPP-R300, SPP-R400"}			// 29
	};
	
	private boolean mIsConnected;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		if (Build.VERSION.SDK_INT == Build.VERSION_CODES.HONEYCOMB || Build.VERSION.SDK_INT == Build.VERSION_CODES.HONEYCOMB_MR1) {
			StrictMode.setThreadPolicy(new StrictMode.ThreadPolicy.Builder().detectDiskReads().detectDiskWrites().detectNetwork().penaltyLog().build());
			StrictMode.setVmPolicy(new StrictMode.VmPolicy.Builder().detectLeakedSqlLiteObjects().detectLeakedClosableObjects().penaltyLog().penaltyDeath().build());
		}
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		ArrayList<HashMap<String, String>> data = new ArrayList<HashMap<String, String>>();
		for (int i = 0; i < FUNCTIONS.length; i++) {
			HashMap<String, String> hashMap = new HashMap<String, String>();
			hashMap.put("item1", FUNCTIONS[i][0]);
			hashMap.put("item2", FUNCTIONS[i][1]);
			data.add(hashMap);
		}

		SimpleAdapter adapter = new SimpleAdapter(this, data, android.R.layout.simple_list_item_2,
				new String[] {"item1", "item2"}, new int[] {android.R.id.text1, android.R.id.text2});
		mListView = (ListView) findViewById(android.R.id.list);
		mListView.setAdapter(adapter);
		mListView.setEnabled(false);
		
		mProgressBar = (ProgressBar) findViewById(R.id.progressBar1);
		
		mBixolonPrinter = new BixolonPrinter(this, mHandler, null);
	}

	@Override
	public void onDestroy() {
		super.onDestroy();
		
		try {
			unregisterReceiver(mUsbReceiver);
		} catch (IllegalArgumentException e) {
			e.printStackTrace();
		}
		
		mBixolonPrinter.disconnect();
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
			mBixolonPrinter.findBluetoothPrinters();
			break;

		case R.id.item2:
			mBixolonPrinter.findNetworkPrinters(3000);
			return true;

		case R.id.item3:
			if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR1) {
				mBixolonPrinter.findUsbPrinters();
			}
			return true;
			
		case R.id.item4:
			mBixolonPrinter.disconnect();
			return true;
		}
		return false;
	}

	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		switch (position) {
		case 0:	// getStatus
			mBixolonPrinter.getStatus();
			break;
			
		case 1: // enable automatic status back
			mBixolonPrinter.automateStatusBack(true);
			break;
			
		case 2: // disable automatic status back
			mBixolonPrinter.automateStatusBack(false);
			break;
			
		case 3: // printSelfTest
			mBixolonPrinter.printSelfTest(true);
			break;
			
		case 4: // getPrinterId
			DialogManager.showPrinterIdDialog(mPrinterIdDialog, MainActivity.this);
			break;
			
		case 5: // setCodePage
			DialogManager.showCodePageDialog(mCodePageDialog, MainActivity.this, mHandler);
			break;		
			
		case 6: // getPrintSpeed
			mBixolonPrinter.getPrintSpeed();
			break;
			
		case 7: // setPrintSpeed
			DialogManager.showPrintSpeedDialog(mPrintSpeedDialog, MainActivity.this);
			break;
			
		case 8: // getPrintDensity
			mBixolonPrinter.getPrintDensity();
			break;
			
		case 9: // setPrintDensity
			DialogManager.showPrintDensityDialog(mPrintDensityDialog, MainActivity.this);
			break;
			
		case 10: // getPowerSavingMode
			mBixolonPrinter.getPowerSavingMode();
			break;
			
		case 11: // setPowerSavingMode
			DialogManager.showPowerSavingModeDialog(mPowerSavingModeDialog, MainActivity.this);
			break;
			
		case 12: // set page mode
			Intent intent = new Intent(MainActivity.this, PageModeActivity.class);
			startActivity(intent);
			break;
			
		case 13:	// initialize
			mBixolonPrinter.initialize();
			break;
			
		case 14:	// getBsCodePage
			mBixolonPrinter.getBsCodePage();
			break;
			
		case 15:	// setBsCodePage
			DialogManager.showBsCodePageDialog(mBsCodePageDialog, MainActivity.this);
			break;
			
		case 16:	// setPrintColor
			DialogManager.showPrintColorDialog(mPrintColorDialog, MainActivity.this);
			break;

		case 17:	// printText
			intent = new Intent(MainActivity.this, PrintTextActivity.class);
			startActivity(intent);
			break;

		case 18:	// print1dBarcode
			intent = new Intent(MainActivity.this, Print1dBarcodeActivity.class);
			startActivity(intent);
			break;

		case 19:	// printPdf417
			DialogManager.showPdf417Dialog(mPdf417Dialog, MainActivity.this);
			break;

		case 20:	// printQrCode
			DialogManager.showQrCodeDialog(mQrCodeDialog, MainActivity.this);
			break;

		case 21:	// printMaxiCode
			DialogManager.showMaxiCodeDialog(mMaxiCodeDialog, MainActivity.this);
			break;

		case 22: // printDataMatrix
			DialogManager.showDataMatrixDialog(mDataMatrixDialog, MainActivity.this);
			break;

		case 23: // printBitmap
			intent = new Intent(MainActivity.this, PrintBitmapAcitivity.class);
			startActivity(intent);
			break;
			
		case 24: // printPdfFiles
			File file = new File(Environment.getExternalStorageDirectory().getAbsolutePath());
			final File[] files = file.listFiles(new FilenameFilter() {
				
				@Override
				public boolean accept(File dir, String filename) {
					return filename.toUpperCase(Locale.getDefault()).endsWith(".PDF");
				}
			});
			
			if (files.length > 0) {
				new DialogFragment() {
					
					public Dialog onCreateDialog(Bundle savedInstanceState) {
						final String[] items = new String[files.length];
						for (int i = 0; i < items.length; i++) {
							items[i] = files[i].getAbsolutePath();
						}
						
						AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
						builder.setTitle("PrintPdfFiles").setItems(items, new DialogInterface.OnClickListener() {
							
							@Override
							public void onClick(DialogInterface dialog, int which) {
								mBixolonPrinter.printPdf(items[which], 1, 50, true, true, true);
							}
						});
						return builder.create();
					};
					
				}.show(getFragmentManager(), "PrintPdfFiles");
			} else {
				Toast.makeText(getApplicationContext(), "No PDF file", Toast.LENGTH_SHORT).show();
			}
			break;
			
		case 25: // kickOutDrawer
			mBixolonPrinter.kickOutDrawer(BixolonPrinter.DRAWER_CONNECTOR_PIN2);
			mBixolonPrinter.kickOutDrawer(BixolonPrinter.DRAWER_CONNECTOR_PIN5);
			break;
			
		case 26: // directIo
			DialogManager.showDirectIoDialog(mDirectIoDialog, MainActivity.this);
			break;
			
		case 27: // NV image manager
			intent = new Intent(MainActivity.this, NvImageActivity.class);
			startActivity(intent);
			break;
			
		case 28: // updateFirmware
			intent = new Intent(MainActivity.this, FileExplorerActivity.class);
			startActivityForResult(intent, REQUEST_CODE_SELECT_FIRMWARE);
			break;
			
		case 29: // MSR manager
			mBixolonPrinter.getMsrMode();
			break;
		}
	}
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_CODE_SELECT_FIRMWARE && resultCode == RESULT_CODE_SELECT_FIRMWARE) {
			final String binaryFilePath = data.getStringExtra(FIRMWARE_FILE_NAME);
			mHandler.obtainMessage(MESSAGE_START_WORK).sendToTarget();
			new Thread(new Runnable() {
				
				public void run() {
					mBixolonPrinter.updateFirmware(binaryFilePath);
					try {
						Thread.sleep(5000);
					} catch (InterruptedException e) {
						// TODO Auto-generated catch block
						e.printStackTrace();
					}
					mHandler.obtainMessage(MESSAGE_END_WORK).sendToTarget();
				}
			}).start();
		} else {
			super.onActivityResult(requestCode, resultCode, data);
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

	private void dispatchMessage(Message msg) {
		switch (msg.arg1) {
		case BixolonPrinter.PROCESS_GET_STATUS:
			if (msg.arg2 == BixolonPrinter.STATUS_NORMAL) {
				Toast.makeText(getApplicationContext(), "No error", Toast.LENGTH_SHORT).show();
			} else {
				StringBuffer buffer = new StringBuffer();
				if ((msg.arg2 & BixolonPrinter.STATUS_COVER_OPEN) == BixolonPrinter.STATUS_COVER_OPEN) {
					buffer.append("Cover is open.\n");
				}
				if ((msg.arg2 & BixolonPrinter.STATUS_PAPER_NOT_PRESENT) == BixolonPrinter.STATUS_PAPER_NOT_PRESENT) {
					buffer.append("Paper end sensor: paper not present.\n");
				}

				Toast.makeText(getApplicationContext(), buffer.toString(), Toast.LENGTH_SHORT).show();
			}
			break;
			
		case BixolonPrinter.PROCESS_GET_PRINTER_ID:
			Bundle data = msg.getData();
			Toast.makeText(getApplicationContext(), data.getString(BixolonPrinter.KEY_STRING_PRINTER_ID), Toast.LENGTH_SHORT).show();
			break;
			
		case BixolonPrinter.PROCESS_GET_BS_CODE_PAGE:
			data = msg.getData();
			Toast.makeText(getApplicationContext(), data.getString(BixolonPrinter.KEY_STRING_CODE_PAGE), Toast.LENGTH_SHORT).show();
			break;
			
		case BixolonPrinter.PROCESS_GET_PRINT_SPEED:
			switch (msg.arg2) {
			case BixolonPrinter.PRINT_SPEED_LOW:
				Toast.makeText(getApplicationContext(), "Print speed: low", Toast.LENGTH_SHORT).show();
				break;
			case BixolonPrinter.PRINT_SPEED_MEDIUM:
				Toast.makeText(getApplicationContext(), "Print speed: medium", Toast.LENGTH_SHORT).show();
				break;
			case BixolonPrinter.PRINT_SPEED_HIGH:
				Toast.makeText(getApplicationContext(), "Print speed: high", Toast.LENGTH_SHORT).show();
				break;
			}
			break;
			
		case BixolonPrinter.PROCESS_GET_PRINT_DENSITY:
			switch (msg.arg2) {
			case BixolonPrinter.PRINT_DENSITY_LIGHT:
				Toast.makeText(getApplicationContext(), "Print density: light", Toast.LENGTH_SHORT).show();
				break;
			case BixolonPrinter.PRINT_DENSITY_DEFAULT:
				Toast.makeText(getApplicationContext(), "Print density: default", Toast.LENGTH_SHORT).show();
				break;
			case BixolonPrinter.PRINT_DENSITY_DARK:
				Toast.makeText(getApplicationContext(), "Print density: dark", Toast.LENGTH_SHORT).show();
				break;
			}
			break;
			
		case BixolonPrinter.PROCESS_GET_POWER_SAVING_MODE:
			String text = "Power saving mode: ";
			if (msg.arg2 == 0) {
				text += false;
			} else {
				text += true + "\n(Power saving time: " + msg.arg2 + ")";
			}
			Toast.makeText(getApplicationContext(), text, Toast.LENGTH_SHORT).show();
			break;

		case BixolonPrinter.PROCESS_AUTO_STATUS_BACK:
			StringBuffer buffer = new StringBuffer(0);
			if ((msg.arg2 & BixolonPrinter.AUTO_STATUS_COVER_OPEN) == BixolonPrinter.AUTO_STATUS_COVER_OPEN) {
				buffer.append("Cover is open.\n");
			}
			if ((msg.arg2 & BixolonPrinter.AUTO_STATUS_NO_PAPER) == BixolonPrinter.AUTO_STATUS_NO_PAPER) {
				buffer.append("Paper end sensor: no paper present.\n");
			}
			
			if (buffer.capacity() > 0) {
				Toast.makeText(getApplicationContext(), buffer.toString(), Toast.LENGTH_SHORT).show();
			} else {
				Toast.makeText(getApplicationContext(), "No error.", Toast.LENGTH_SHORT).show();
			}
			break;
			
		case BixolonPrinter.PROCESS_GET_NV_IMAGE_KEY_CODES:
			data = msg.getData();
			int[] value = data.getIntArray(BixolonPrinter.NV_IMAGE_KEY_CODES);
			
			Intent intent = new Intent();
			intent.setAction(ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES);
			intent.putExtra(EXTRA_NAME_NV_KEY_CODES, value);
			sendBroadcast(intent);
			break;
			
		case BixolonPrinter.PROCESS_EXECUTE_DIRECT_IO:
			buffer = new StringBuffer();
			data = msg.getData();
			byte[] response = data.getByteArray(BixolonPrinter.KEY_STRING_DIRECT_IO);
			for (int i = 0; i < response.length && response[i] != 0; i++) {
				buffer.append(Integer.toHexString(response[i]) + " ");
			}
			
			Toast.makeText(getApplicationContext(), buffer.toString(), Toast.LENGTH_SHORT).show();
			break;
			
		case BixolonPrinter.PROCESS_MSR_TRACK:
			intent = new Intent();
			intent.setAction(ACTION_GET_MSR_TRACK_DATA);
			intent.putExtra(EXTRA_NAME_MSR_TRACK_DATA, msg.getData());
			sendBroadcast(intent);
			break;
			
		case BixolonPrinter.PROCESS_GET_MSR_MODE:
			intent = new Intent(MainActivity.this, MsrActivity.class);
			intent.putExtra(EXTRA_NAME_MSR_MODE, msg.arg2);
			startActivity(intent);
			break;
		}
	}

	private final Handler mHandler = new Handler(new Handler.Callback() {
		
		@SuppressWarnings("unchecked")
		@Override
		public boolean handleMessage(Message msg) {
			Log.d(TAG, "mHandler.handleMessage(" + msg + ")");
			
			switch (msg.what) {
			case BixolonPrinter.MESSAGE_STATE_CHANGE:
				switch (msg.arg1) {
				case BixolonPrinter.STATE_CONNECTED:
					setStatus(getString(R.string.title_connected_to, mConnectedDeviceName));
					mListView.setEnabled(true);
					mIsConnected = true;
					invalidateOptionsMenu();
					break;

				case BixolonPrinter.STATE_CONNECTING:
					setStatus(R.string.title_connecting);
					break;

				case BixolonPrinter.STATE_NONE:
					setStatus(R.string.title_not_connected);
					mListView.setEnabled(false);
					mIsConnected = false;
					invalidateOptionsMenu();
					mProgressBar.setVisibility(View.INVISIBLE);
					break;
				}
				return true;
				
			case BixolonPrinter.MESSAGE_WRITE:
				switch (msg.arg1) {
				case BixolonPrinter.PROCESS_SET_DOUBLE_BYTE_FONT:
					mHandler.obtainMessage(MESSAGE_END_WORK).sendToTarget();
					
					Toast.makeText(getApplicationContext(), "Complete to set double byte font.", Toast.LENGTH_SHORT).show();
					break;
					
				case BixolonPrinter.PROCESS_DEFINE_NV_IMAGE:
					mBixolonPrinter.getDefinedNvImageKeyCodes();
					Toast.makeText(getApplicationContext(), "Complete to define NV image", Toast.LENGTH_LONG).show();
					break;
					
				case BixolonPrinter.PROCESS_REMOVE_NV_IMAGE:
					mBixolonPrinter.getDefinedNvImageKeyCodes();
					Toast.makeText(getApplicationContext(), "Complete to remove NV image", Toast.LENGTH_LONG).show();
					break;
					
				case BixolonPrinter.PROCESS_UPDATE_FIRMWARE:
					mBixolonPrinter.disconnect();
					Toast.makeText(getApplicationContext(), "Complete to download firmware.\nPlease reboot the printer.", Toast.LENGTH_SHORT).show();
					break;
				}
				return true;

			case BixolonPrinter.MESSAGE_READ:
				MainActivity.this.dispatchMessage(msg);
				return true;

			case BixolonPrinter.MESSAGE_DEVICE_NAME:
				mConnectedDeviceName = msg.getData().getString(BixolonPrinter.KEY_STRING_DEVICE_NAME);
				Toast.makeText(getApplicationContext(), mConnectedDeviceName, Toast.LENGTH_LONG).show();
				return true;

			case BixolonPrinter.MESSAGE_TOAST:
				mListView.setEnabled(false);
				Toast.makeText(getApplicationContext(), msg.getData().getString(BixolonPrinter.KEY_STRING_TOAST), Toast.LENGTH_SHORT).show();
				return true;
				
			case BixolonPrinter.MESSAGE_BLUETOOTH_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No paired device", Toast.LENGTH_SHORT).show();
				} else {
					DialogManager.showBluetoothDialog(MainActivity.this, (Set<BluetoothDevice>) msg.obj);
				}
				return true;
				
			case BixolonPrinter.MESSAGE_PRINT_COMPLETE:
				Toast.makeText(getApplicationContext(), "Complete to print", Toast.LENGTH_SHORT).show();
				return true;
				
			case BixolonPrinter.MESSAGE_ERROR_INVALID_ARGUMENT:
				Toast.makeText(getApplicationContext(), "Invalid argument", Toast.LENGTH_SHORT).show();
				return true;
				
			case BixolonPrinter.MESSAGE_ERROR_NV_MEMORY_CAPACITY:
				Toast.makeText(getApplicationContext(), "NV memory capacity error", Toast.LENGTH_SHORT).show();
				return true;
				
			case BixolonPrinter.MESSAGE_ERROR_OUT_OF_MEMORY:
				Toast.makeText(getApplicationContext(), "Out of memory", Toast.LENGTH_SHORT).show();
				return true;
				
			case BixolonPrinter.MESSAGE_COMPLETE_PROCESS_BITMAP:
				String text = "Complete to process bitmap.";
				Bundle data = msg.getData();
				byte[] value = data.getByteArray(BixolonPrinter.KEY_STRING_MONO_PIXELS);
				if (value != null) {
					Intent intent = new Intent();
					intent.setAction(ACTION_COMPLETE_PROCESS_BITMAP);
					intent.putExtra(EXTRA_NAME_BITMAP_WIDTH, msg.arg1);
					intent.putExtra(EXTRA_NAME_BITMAP_HEIGHT, msg.arg2);
					intent.putExtra(EXTRA_NAME_BITMAP_PIXELS, value);
					sendBroadcast(intent);
				}
				
				Toast.makeText(getApplicationContext(), text, Toast.LENGTH_SHORT).show();
				return true;
				
			case MESSAGE_START_WORK:
				mListView.setEnabled(false);
				mProgressBar.setVisibility(View.VISIBLE);
				return true;
				
			case MESSAGE_END_WORK:
				mListView.setEnabled(true);
				mProgressBar.setVisibility(View.INVISIBLE);
				return true;
				
			case BixolonPrinter.MESSAGE_USB_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No connected device", Toast.LENGTH_SHORT).show();
				} else {
					DialogManager.showUsbDialog(MainActivity.this, (Set<UsbDevice>) msg.obj, mUsbReceiver);
				}
				return true;
				
			case BixolonPrinter.MESSAGE_USB_SERIAL_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No connected device", Toast.LENGTH_SHORT).show();
				} else {
					final HashMap<String, UsbDevice> usbDeviceMap = (HashMap<String, UsbDevice>) msg.obj;
					final String[] items = usbDeviceMap.keySet().toArray(new String[usbDeviceMap.size()]);
					new AlertDialog.Builder(MainActivity.this).setItems(items, new DialogInterface.OnClickListener() {
						
						@Override
						public void onClick(DialogInterface dialog, int which) {
							mBixolonPrinter.connect(usbDeviceMap.get(items[which]));
						}
					}).show();
				}
				return true;
				
			case BixolonPrinter.MESSAGE_NETWORK_DEVICE_SET:
				if (msg.obj == null) {
					Toast.makeText(getApplicationContext(), "No connectable device", Toast.LENGTH_SHORT).show();
				}
				DialogManager.showNetworkDialog(MainActivity.this, (Set<String>) msg.obj);
				return true;
			}
			return false;
		}
	});

	private BroadcastReceiver mUsbReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			Log.d(TAG, "mUsbReceiver.onReceive(" + context + ", " + intent + ")");
			String action = intent.getAction();

			if (UsbManager.ACTION_USB_DEVICE_ATTACHED.equals(action)) {
				mBixolonPrinter.connect();
				Toast.makeText(getApplicationContext(), "Found USB device", Toast.LENGTH_SHORT).show();
			} else if (UsbManager.ACTION_USB_DEVICE_DETACHED.equals(action)) {
				mBixolonPrinter.disconnect();
				Toast.makeText(getApplicationContext(), "USB device removed", Toast.LENGTH_SHORT).show();
			}

		}
	};
}
