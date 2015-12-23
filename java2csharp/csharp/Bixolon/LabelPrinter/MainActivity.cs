using System.Collections.Generic;
using System.Text;

namespace com.bixolon.labelprintersample
{


	using SuppressLint = android.annotation.SuppressLint;
	using ActionBar = android.app.ActionBar;
	using AlertDialog = android.app.AlertDialog;
	using ListActivity = android.app.ListActivity;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using UsbDevice = android.hardware.usb.UsbDevice;
	using UsbManager = android.hardware.usb.UsbManager;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using Message = android.os.Message;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ListView = android.widget.ListView;
	using Toast = android.widget.Toast;

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("HandlerLeak") public class MainActivity extends android.app.ListActivity
	public class MainActivity : ListActivity
	{

		private static readonly string[] FUNCTIONS = new string[] {"drawText", "drawVectorFontText", "draw1dBarcode", "drawMaxicode", "drawPdf417", "drawQrCode", "drawDataMatrix", "drawBlock", "drawCircle", "setCharacterSet", "setPrintingType", "setMargin", "setBackFeedOption", "setLength", "setWidth", "setBufferMode", "clearBuffer", "setSpeed", "setDensity", "setOrientation", "setOffset", "setCutterPosition", "drawBitmap", "initializePrinter", "printInformation", "setAutoCutter", "getStatus", "getPrinterInformation", "executeDirectIo"};

		// Name of the connected device
		private string mConnectedDeviceName = null;

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

		private bool mIsConnected;

		internal static BixolonLabelPrinter mBixolonLabelPrinter;

		private bool checkedManufacture = false;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			List<string> list = new List<string>();
			for (int i = 0; i < FUNCTIONS.Length; i++)
			{
				list.Add(FUNCTIONS[i]);
			}

			ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, list);
			mListView = (ListView) findViewById(android.R.id.list);
			mListView.Adapter = adapter;
			mListView.Enabled = false;

			mBixolonLabelPrinter = new BixolonLabelPrinter(this, mHandler, null);
		}

		public override void onDestroy()
		{
			try
			{
				unregisterReceiver(mUsbReceiver);
			}
			catch (System.ArgumentException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			mBixolonLabelPrinter.disconnect();
			base.onDestroy();
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_main, menu);
			return true;
		}

		public override bool onPrepareOptionsMenu(Menu menu)
		{
			if (mIsConnected)
			{
				menu.getItem(0).Enabled = false;
				menu.getItem(1).Enabled = false;
				menu.getItem(2).Enabled = false;
				menu.getItem(3).Enabled = true;
			}
			else
			{
				menu.getItem(0).Enabled = true;
				menu.getItem(1).Enabled = true;
				menu.getItem(2).Enabled = true;
				menu.getItem(3).Enabled = false;
			}
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			switch (item.ItemId)
			{
			case R.id.item1:
				mBixolonLabelPrinter.findBluetoothPrinters();
				break;

			case R.id.item2:
				mBixolonLabelPrinter.findNetworkPrinters(3000);
				return true;

			case R.id.item3:
				if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR1)
				{
					mBixolonLabelPrinter.findUsbPrinters();
				}
				return true;


			case R.id.item4:
				mBixolonLabelPrinter.disconnect();
				return true;

			}
			return false;
		}

		protected internal override void onListItemClick(ListView l, View v, int position, long id)
		{
			switch (position)
			{
			case 0: // drawText
				Intent intent = new Intent(MainActivity.this, typeof(DrawTextActivity));
				startActivity(intent);
				break;

			case 1: // drawVectorFontText
				intent = new Intent(MainActivity.this, typeof(DrawVectorTextActivity));
				startActivity(intent);
				break;

			case 2: // draw1dBarcode
				intent = new Intent(MainActivity.this, typeof(Draw1dBarcodeActivity));
				startActivity(intent);
				break;

			case 3: // drawMaxicode
				intent = new Intent(MainActivity.this, typeof(DrawMaxicodeActivity));
				startActivity(intent);
				break;

			case 4: // drawPdf417
				intent = new Intent(MainActivity.this, typeof(DrawPdf417Activity));
				startActivity(intent);
				break;

			case 5: // drawQrCode
				intent = new Intent(MainActivity.this, typeof(DrawQrCodeActivity));
				startActivity(intent);
				break;

			case 6: // drawDataMatrix
				intent = new Intent(MainActivity.this, typeof(DrawDataMatrixActivity));
				startActivity(intent);
				break;

			case 7: // drawBlock
				intent = new Intent(MainActivity.this, typeof(DrawBlockActivity));
				startActivity(intent);
				break;

			case 8: // drawCircle
				intent = new Intent(MainActivity.this, typeof(DrawCircleActivity));
				startActivity(intent);
				break;

			case 9: // setCharacterSet
				intent = new Intent(MainActivity.this, typeof(CharacterSetSelectionActivity));
				startActivity(intent);
				break;

			case 10: // setPrintingType
				DialogManager.showSetPrintingTypeDialog(mSetPrintingTypeDialog, MainActivity.this);
				break;

			case 11: // setMargin
				DialogManager.showSetMarginDialog(mSetMarginDialog, MainActivity.this);
				break;

			case 12: // setBackFeedOption
				DialogManager.showSeBackFeedDialog(mSetBackfeedDialog, MainActivity.this);
				break;

			case 13: // setLength
				DialogManager.showSetLengthDialog(mSetLengthDialog, MainActivity.this);
				break;

			case 14: // setWidth
				DialogManager.showSetWidthDialog(mSetWidthDialog, MainActivity.this);
				break;

			case 15: // setBufferMode
				DialogManager.showSetBufferModeDialog(mSetBufferModeDialog, MainActivity.this);
				break;

			case 16: // clearBuffer
				mBixolonLabelPrinter.clearBuffer();
				break;

			case 17: // setSpeed
				DialogManager.showSetSpeedDialog(mSetSpeedDialog, MainActivity.this);
				break;

			case 18: // setDensity
				DialogManager.showSetDensityDialog(mSetDensityDialog, MainActivity.this);
				break;

			case 19: // setOrientation
				DialogManager.showSetOrientationDialog(mSetOrientationDialog, MainActivity.this);
				break;

			case 20: // setOffset
				DialogManager.showSetOffsetDialog(mSetOffsetDialog, MainActivity.this);
				break;

			case 21: // setCutterPosition
				DialogManager.showCutterPositionSettingDialog(mCutterPositionSettingDialog, MainActivity.this);
				break;

			case 22: // drawBitmap
				intent = new Intent(MainActivity.this, typeof(DrawBitmapActivity));
				startActivity(intent);
				break;

			case 23: // initializePrinter
				mBixolonLabelPrinter.initializePrinter();
				break;

			case 24: // printInformation
				mBixolonLabelPrinter.printInformation();
				break;

			case 25: // setAutoCutter
				DialogManager.showAutoCutterDialog(mAutoCutterDialog, MainActivity.this);
				break;

			case 26: // getStatus
				mBixolonLabelPrinter.getStatus(true);
				break;

			case 27: // getPrinterInformation
				DialogManager.showPrinterInformationDialog(mPrinterInformationDialog, MainActivity.this, mBixolonLabelPrinter);
				break;

			case 28: // executeDirectIo
				intent = new Intent(MainActivity.this, typeof(DirectIoActivity));
				startActivity(intent);
				break;
			}
		}

		private int Status
		{
			set
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final android.app.ActionBar actionBar = getActionBar();
				ActionBar actionBar = ActionBar;
				actionBar.Subtitle = value;
			}
		}

		private CharSequence Status
		{
			set
			{
	//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
	//ORIGINAL LINE: final android.app.ActionBar actionBar = getActionBar();
				ActionBar actionBar = ActionBar;
				actionBar.Subtitle = value;
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("HandlerLeak") private void dispatchMessage(android.os.Message msg)
		private void dispatchMessage(Message msg)
		{
			switch (msg.arg1)
			{
			case BixolonLabelPrinter.PROCESS_GET_STATUS:
				sbyte[] report = (sbyte[]) msg.obj;
				StringBuilder buffer = new StringBuilder();
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_PAPER_EMPTY) == BixolonLabelPrinter.STATUS_1ST_BYTE_PAPER_EMPTY)
				{
					buffer.Append("Paper Empty.\n");
				}
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_COVER_OPEN) == BixolonLabelPrinter.STATUS_1ST_BYTE_COVER_OPEN)
				{
					buffer.Append("Cover open.\n");
				}
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_CUTTER_JAMMED) == BixolonLabelPrinter.STATUS_1ST_BYTE_CUTTER_JAMMED)
				{
					buffer.Append("Cutter jammed.\n");
				}
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_TPH_OVERHEAT) == BixolonLabelPrinter.STATUS_1ST_BYTE_TPH_OVERHEAT)
				{
					buffer.Append("TPH(thermal head) overheat.\n");
				}
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_AUTO_SENSING_FAILURE) == BixolonLabelPrinter.STATUS_1ST_BYTE_AUTO_SENSING_FAILURE)
				{
					buffer.Append("Gap detection error. (Auto-sensing failure)\n");
				}
				if ((report[0] & BixolonLabelPrinter.STATUS_1ST_BYTE_RIBBON_END_ERROR) == BixolonLabelPrinter.STATUS_1ST_BYTE_RIBBON_END_ERROR)
				{
					buffer.Append("Ribbon end error.\n");
				}

				if (report.Length == 2)
				{
					if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_BUILDING_IN_IMAGE_BUFFER) == BixolonLabelPrinter.STATUS_2ND_BYTE_BUILDING_IN_IMAGE_BUFFER)
					{
						buffer.Append("On building label to be printed in image buffer.\n");
					}
					if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_PRINTING_IN_IMAGE_BUFFER) == BixolonLabelPrinter.STATUS_2ND_BYTE_PRINTING_IN_IMAGE_BUFFER)
					{
						buffer.Append("On printing label in image buffer.\n");
					}
					if ((report[1] & BixolonLabelPrinter.STATUS_2ND_BYTE_PAUSED_IN_PEELER_UNIT) == BixolonLabelPrinter.STATUS_2ND_BYTE_PAUSED_IN_PEELER_UNIT)
					{
						buffer.Append("Issued label is paused in peeler unit.\n");
					}
				}
				if (buffer.Length == 0)
				{
					buffer.Append("No error");
				}
				Toast.makeText(ApplicationContext, buffer.ToString(), Toast.LENGTH_SHORT).show();
				break;

			case BixolonLabelPrinter.PROCESS_GET_INFORMATION_MODEL_NAME:
			case BixolonLabelPrinter.PROCESS_GET_INFORMATION_FIRMWARE_VERSION:
			case BixolonLabelPrinter.PROCESS_EXECUTE_DIRECT_IO:
				Toast.makeText(ApplicationContext, (string) msg.obj, Toast.LENGTH_SHORT).show();
				break;
			}
		}

		private readonly Handler mHandler = new HandlerAnonymousInnerClassHelper();

		private class HandlerAnonymousInnerClassHelper : Handler
		{
			public HandlerAnonymousInnerClassHelper()
			{
			}

			public override void handleMessage(Message msg)
			{
				switch (msg.what)
				{
				case BixolonLabelPrinter.MESSAGE_STATE_CHANGE:
					switch (msg.arg1)
					{
					case BixolonLabelPrinter.STATE_CONNECTED:
						outerInstance.Status = getString(R.@string.title_connected_to, outerInstance.mConnectedDeviceName);
						outerInstance.mListView.Enabled = true;
						outerInstance.mIsConnected = true;
						invalidateOptionsMenu();
						break;

					case BixolonLabelPrinter.STATE_CONNECTING:
						outerInstance.Status = R.@string.title_connecting;
						break;

					case BixolonLabelPrinter.STATE_NONE:
						outerInstance.Status = R.@string.title_not_connected;
						outerInstance.mListView.Enabled = false;
						outerInstance.mIsConnected = false;
						invalidateOptionsMenu();
						break;
					}
					break;

				case BixolonLabelPrinter.MESSAGE_READ:
					outerInstance.dispatchMessage(msg);
					break;

				case BixolonLabelPrinter.MESSAGE_DEVICE_NAME:
					outerInstance.mConnectedDeviceName = msg.Data.getString(BixolonLabelPrinter.DEVICE_NAME);
					Toast.makeText(ApplicationContext, outerInstance.mConnectedDeviceName, Toast.LENGTH_LONG).show();
					break;

				case BixolonLabelPrinter.MESSAGE_TOAST:
					outerInstance.mListView.Enabled = false;
					Toast.makeText(ApplicationContext, msg.Data.getString(BixolonLabelPrinter.TOAST), Toast.LENGTH_SHORT).show();
					break;

				case BixolonLabelPrinter.MESSAGE_BLUETOOTH_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No paired device", Toast.LENGTH_SHORT).show();
					}
					else
					{
						DialogManager.showBluetoothDialog(outerInstance, (ISet<BluetoothDevice>) msg.obj);
					}
					break;

					case BixolonLabelPrinter.MESSAGE_USB_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No connected device", Toast.LENGTH_SHORT).show();
					}
					else
					{
						DialogManager.showUsbDialog(outerInstance, (ISet<UsbDevice>) msg.obj, mUsbReceiver);
					}
					break;

					case BixolonLabelPrinter.MESSAGE_NETWORK_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No connectable device", Toast.LENGTH_SHORT).show();
					}
					DialogManager.showNetworkDialog(outerInstance, (ISet<string>) msg.obj);
					break;

				}
			}
		}

		private BroadcastReceiver mUsbReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}


			public override void onReceive(Context context, Intent intent)
			{
				string action = intent.Action;

				if (UsbManager.ACTION_USB_DEVICE_ATTACHED.Equals(action))
				{
					mBixolonLabelPrinter.connect();
					Toast.makeText(ApplicationContext, "Found USB device", Toast.LENGTH_SHORT).show();
				}
				else if (UsbManager.ACTION_USB_DEVICE_DETACHED.Equals(action))
				{
					mBixolonLabelPrinter.disconnect();
					Toast.makeText(ApplicationContext, "USB device removed", Toast.LENGTH_SHORT).show();
				}

			}
		}
	}

}