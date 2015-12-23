using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace com.bixolon.printersample
{


	using ActionBar = android.app.ActionBar;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogFragment = android.app.DialogFragment;
	using ListActivity = android.app.ListActivity;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using UsbDevice = android.hardware.usb.UsbDevice;
	using UsbManager = android.hardware.usb.UsbManager;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using Message = android.os.Message;
	using StrictMode = android.os.StrictMode;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ListView = android.widget.ListView;
	using ProgressBar = android.widget.ProgressBar;
	using SimpleAdapter = android.widget.SimpleAdapter;
	using Toast = android.widget.Toast;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class MainActivity : ListActivity
	{

		public const string TAG = "BixolonPrinterSample";

		internal const string ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES = "com.bixolon.anction.GET_DEFINED_NV_IMAGE_KEY_CODES";
		internal const string ACTION_COMPLETE_PROCESS_BITMAP = "com.bixolon.anction.COMPLETE_PROCESS_BITMAP";
		internal const string ACTION_GET_MSR_TRACK_DATA = "com.bixolon.anction.GET_MSR_TRACK_DATA";
		internal const string EXTRA_NAME_NV_KEY_CODES = "NvKeyCodes";
		internal const string EXTRA_NAME_MSR_MODE = "MsrMode";
		internal const string EXTRA_NAME_MSR_TRACK_DATA = "MsrTrackData";
		internal const string EXTRA_NAME_BITMAP_WIDTH = "BitmapWidth";
		internal const string EXTRA_NAME_BITMAP_HEIGHT = "BitmapHeight";
		internal const string EXTRA_NAME_BITMAP_PIXELS = "BitmapPixels";

		internal static readonly int REQUEST_CODE_SELECT_FIRMWARE = int.MaxValue;
		internal static readonly int RESULT_CODE_SELECT_FIRMWARE = int.MaxValue - 1;
		internal static readonly int MESSAGE_START_WORK = int.MaxValue - 2;
		internal static readonly int MESSAGE_END_WORK = int.MaxValue - 3;

		internal const string FIRMWARE_FILE_NAME = "FirmwareFileName";

		// Name of the connected device
		private string mConnectedDeviceName = null;

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

		internal static BixolonPrinter mBixolonPrinter;

		private static readonly string[][] FUNCTIONS = new string[][]
		{
			new string[] {"getStatus", "Supported all model"},
			new string[] {"enable automatic status back", "Supported all model"},
			new string[] {"disable automatic status back", "Supported all model"},
			new string[] {"printSelfTest", "Supported all model"},
			new string[] {"getPrinterId", "Supported all model"},
			new string[] {"setCodePage", "Supported all model (2 bytes font is POS printers only)"},
			new string[] {"getPrintSpeed", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"setPrintSpeed", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"getPrintDensity", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"setPrintDensity", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"getPowerSavingMode", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"setPowerSavingMode", "Supported SRP-350II, SRP-350IIK, SRP-350plusII, SRP-352plusII"},
			new string[] {"set page mode", "Supported SPP-100II, SPP-R200II, SPP-R300, SPP-R400, SRP-350IIOBE"},
			new string[] {"initialize", "Supported all model"},
			new string[] {"getBsCodePage", "Supported SRP-275II only"},
			new string[] {"setBsCodePage", "Supported SRP-275II only"},
			new string[] {"setPrintColor", "Supported SRP-275II only"},
			new string[] {"printText", "Supported all model"},
			new string[] {"print1dBarcode", "Supported all model except for SRP-275II"},
			new string[] {"printPdf417", "Supported all model except for SRP-275II"},
			new string[] {"printQrCode", "Supported all model except for SRP-275II"},
			new string[] {"printMaxiCode", "Supported SPP-R200II, SPP-R300, SPP-R400"},
			new string[] {"printDataMatrix", "Supported SPP-R200II, SPP-R300, SPP-R400"},
			new string[] {"printBitmap", "Supported all model"},
			new string[] {"printPdfFiles", "Supported all model"},
			new string[] {"kickOutDrawer", "Supported POS printer only"},
			new string[] {"executeDirectIo", "Supported all model"},
			new string[] {"NV image manager", "Supported all model except for SRP-275II"},
			new string[] {"updateFirmware", "Supported all model"},
			new string[] {"MSR manager", "Supported SPP-R200II, SPP-R300, SPP-R400"}
		};

		private bool mIsConnected;

		public override void onCreate(Bundle savedInstanceState)
		{
			if (Build.VERSION.SDK_INT == Build.VERSION_CODES.HONEYCOMB || Build.VERSION.SDK_INT == Build.VERSION_CODES.HONEYCOMB_MR1)
			{
				StrictMode.ThreadPolicy = (new StrictMode.ThreadPolicy.Builder()).detectDiskReads().detectDiskWrites().detectNetwork().penaltyLog().build();
				StrictMode.VmPolicy = (new StrictMode.VmPolicy.Builder()).detectLeakedSqlLiteObjects().detectLeakedClosableObjects().penaltyLog().penaltyDeath().build();
			}
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
			for (int i = 0; i < FUNCTIONS.Length; i++)
			{
				Dictionary<string, string> hashMap = new Dictionary<string, string>();
				hashMap["item1"] = FUNCTIONS[i][0];
				hashMap["item2"] = FUNCTIONS[i][1];
				data.Add(hashMap);
			}

			SimpleAdapter adapter = new SimpleAdapter(this, data, android.R.layout.simple_list_item_2, new string[] {"item1", "item2"}, new int[] {android.R.id.text1, android.R.id.text2});
			mListView = (ListView) findViewById(android.R.id.list);
			mListView.Adapter = adapter;
			mListView.Enabled = false;

			mProgressBar = (ProgressBar) findViewById(R.id.progressBar1);

			mBixolonPrinter = new BixolonPrinter(this, mHandler, null);
		}

		public override void onDestroy()
		{
			base.onDestroy();

			try
			{
				unregisterReceiver(mUsbReceiver);
			}
			catch (System.ArgumentException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			mBixolonPrinter.disconnect();
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
				mBixolonPrinter.findBluetoothPrinters();
				break;

			case R.id.item2:
				mBixolonPrinter.findNetworkPrinters(3000);
				return true;

			case R.id.item3:
				if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.HONEYCOMB_MR1)
				{
					mBixolonPrinter.findUsbPrinters();
				}
				return true;

			case R.id.item4:
				mBixolonPrinter.disconnect();
				return true;
			}
			return false;
		}

		protected internal override void onListItemClick(ListView l, View v, int position, long id)
		{
			switch (position)
			{
			case 0: // getStatus
				mBixolonPrinter.Status;
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
				mBixolonPrinter.PrintSpeed;
				break;

			case 7: // setPrintSpeed
				DialogManager.showPrintSpeedDialog(mPrintSpeedDialog, MainActivity.this);
				break;

			case 8: // getPrintDensity
				mBixolonPrinter.PrintDensity;
				break;

			case 9: // setPrintDensity
				DialogManager.showPrintDensityDialog(mPrintDensityDialog, MainActivity.this);
				break;

			case 10: // getPowerSavingMode
				mBixolonPrinter.PowerSavingMode;
				break;

			case 11: // setPowerSavingMode
				DialogManager.showPowerSavingModeDialog(mPowerSavingModeDialog, MainActivity.this);
				break;

			case 12: // set page mode
				Intent intent = new Intent(MainActivity.this, typeof(PageModeActivity));
				startActivity(intent);
				break;

			case 13: // initialize
				mBixolonPrinter.initialize();
				break;

			case 14: // getBsCodePage
				mBixolonPrinter.BsCodePage;
				break;

			case 15: // setBsCodePage
				DialogManager.showBsCodePageDialog(mBsCodePageDialog, MainActivity.this);
				break;

			case 16: // setPrintColor
				DialogManager.showPrintColorDialog(mPrintColorDialog, MainActivity.this);
				break;

			case 17: // printText
				intent = new Intent(MainActivity.this, typeof(PrintTextActivity));
				startActivity(intent);
				break;

			case 18: // print1dBarcode
				intent = new Intent(MainActivity.this, typeof(Print1dBarcodeActivity));
				startActivity(intent);
				break;

			case 19: // printPdf417
				DialogManager.showPdf417Dialog(mPdf417Dialog, MainActivity.this);
				break;

			case 20: // printQrCode
				DialogManager.showQrCodeDialog(mQrCodeDialog, MainActivity.this);
				break;

			case 21: // printMaxiCode
				DialogManager.showMaxiCodeDialog(mMaxiCodeDialog, MainActivity.this);
				break;

			case 22: // printDataMatrix
				DialogManager.showDataMatrixDialog(mDataMatrixDialog, MainActivity.this);
				break;

			case 23: // printBitmap
				intent = new Intent(MainActivity.this, typeof(PrintBitmapAcitivity));
				startActivity(intent);
				break;

			case 24: // printPdfFiles
				File file = new File(Environment.ExternalStorageDirectory.AbsolutePath);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File[] files = file.listFiles(new java.io.FilenameFilter()
				File[] files = file.listFiles(new FilenameFilterAnonymousInnerClassHelper(this));

				if (files.Length > 0)
				{
					new DialogFragmentAnonymousInnerClassHelper(this, files)
					.show(FragmentManager, "PrintPdfFiles");
				}
				else
				{
					Toast.makeText(ApplicationContext, "No PDF file", Toast.LENGTH_SHORT).show();
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
				intent = new Intent(MainActivity.this, typeof(NvImageActivity));
				startActivity(intent);
				break;

			case 28: // updateFirmware
				intent = new Intent(MainActivity.this, typeof(FileExplorerActivity));
				startActivityForResult(intent, REQUEST_CODE_SELECT_FIRMWARE);
				break;

			case 29: // MSR manager
				mBixolonPrinter.MsrMode;
				break;
			}
		}

		private class FilenameFilterAnonymousInnerClassHelper : FilenameFilter
		{
			private readonly MainActivity outerInstance;

			public FilenameFilterAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool accept(File dir, string filename)
			{
				return filename.ToUpper(Locale.Default).EndsWith(".PDF", StringComparison.Ordinal);
			}
		}

		private class DialogFragmentAnonymousInnerClassHelper : DialogFragment
		{
			private readonly MainActivity outerInstance;

			private File[] files;

			public DialogFragmentAnonymousInnerClassHelper(MainActivity outerInstance, File[] files)
			{
				this.outerInstance = outerInstance;
				this.files = files;
			}


			public virtual Dialog onCreateDialog(Bundle savedInstanceState)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = new String[files.length];
				string[] items = new string[files.Length];
				for (int i = 0; i < items.Length; i++)
				{
					items[i] = files[i].AbsolutePath;
				}

				AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
				builder.setTitle("PrintPdfFiles").setItems(items, new OnClickListenerAnonymousInnerClassHelper(this, items));
				return builder.create();
			};

			private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
			{
				private readonly DialogFragmentAnonymousInnerClassHelper outerInstance;

				private string[] items;

				public OnClickListenerAnonymousInnerClassHelper(DialogFragmentAnonymousInnerClassHelper outerInstance, string[] items)
				{
					this.outerInstance = outerInstance;
					this.items = items;
				}


				public override void onClick(DialogInterface dialog, int which)
				{
					mBixolonPrinter.printPdf(items[which], 1, 50, true, true, true);
				}
			}

		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			if (requestCode == REQUEST_CODE_SELECT_FIRMWARE && resultCode == RESULT_CODE_SELECT_FIRMWARE)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String binaryFilePath = data.getStringExtra(FIRMWARE_FILE_NAME);
				string binaryFilePath = data.getStringExtra(FIRMWARE_FILE_NAME);
				mHandler.obtainMessage(MESSAGE_START_WORK).sendToTarget();
				new Thread(() =>
				{

					mBixolonPrinter.updateFirmware(binaryFilePath);
					try
					{
						Thread.Sleep(5000);
					}
					catch (InterruptedException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
					mHandler.obtainMessage(MESSAGE_END_WORK).sendToTarget();
				}).start();
			}
			else
			{
				base.onActivityResult(requestCode, resultCode, data);
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

		private void dispatchMessage(Message msg)
		{
			switch (msg.arg1)
			{
			case BixolonPrinter.PROCESS_GET_STATUS:
				if (msg.arg2 == BixolonPrinter.STATUS_NORMAL)
				{
					Toast.makeText(ApplicationContext, "No error", Toast.LENGTH_SHORT).show();
				}
				else
				{
					StringBuilder buffer = new StringBuilder();
					if ((msg.arg2 & BixolonPrinter.STATUS_COVER_OPEN) == BixolonPrinter.STATUS_COVER_OPEN)
					{
						buffer.Append("Cover is open.\n");
					}
					if ((msg.arg2 & BixolonPrinter.STATUS_PAPER_NOT_PRESENT) == BixolonPrinter.STATUS_PAPER_NOT_PRESENT)
					{
						buffer.Append("Paper end sensor: paper not present.\n");
					}

					Toast.makeText(ApplicationContext, buffer.ToString(), Toast.LENGTH_SHORT).show();
				}
				break;

			case BixolonPrinter.PROCESS_GET_PRINTER_ID:
				Bundle data = msg.Data;
				Toast.makeText(ApplicationContext, data.getString(BixolonPrinter.KEY_STRING_PRINTER_ID), Toast.LENGTH_SHORT).show();
				break;

			case BixolonPrinter.PROCESS_GET_BS_CODE_PAGE:
				data = msg.Data;
				Toast.makeText(ApplicationContext, data.getString(BixolonPrinter.KEY_STRING_CODE_PAGE), Toast.LENGTH_SHORT).show();
				break;

			case BixolonPrinter.PROCESS_GET_PRINT_SPEED:
				switch (msg.arg2)
				{
				case BixolonPrinter.PRINT_SPEED_LOW:
					Toast.makeText(ApplicationContext, "Print speed: low", Toast.LENGTH_SHORT).show();
					break;
				case BixolonPrinter.PRINT_SPEED_MEDIUM:
					Toast.makeText(ApplicationContext, "Print speed: medium", Toast.LENGTH_SHORT).show();
					break;
				case BixolonPrinter.PRINT_SPEED_HIGH:
					Toast.makeText(ApplicationContext, "Print speed: high", Toast.LENGTH_SHORT).show();
					break;
				}
				break;

			case BixolonPrinter.PROCESS_GET_PRINT_DENSITY:
				switch (msg.arg2)
				{
				case BixolonPrinter.PRINT_DENSITY_LIGHT:
					Toast.makeText(ApplicationContext, "Print density: light", Toast.LENGTH_SHORT).show();
					break;
				case BixolonPrinter.PRINT_DENSITY_DEFAULT:
					Toast.makeText(ApplicationContext, "Print density: default", Toast.LENGTH_SHORT).show();
					break;
				case BixolonPrinter.PRINT_DENSITY_DARK:
					Toast.makeText(ApplicationContext, "Print density: dark", Toast.LENGTH_SHORT).show();
					break;
				}
				break;

			case BixolonPrinter.PROCESS_GET_POWER_SAVING_MODE:
				string text = "Power saving mode: ";
				if (msg.arg2 == 0)
				{
					text += false;
				}
				else
				{
					text += true + "\n(Power saving time: " + msg.arg2 + ")";
				}
				Toast.makeText(ApplicationContext, text, Toast.LENGTH_SHORT).show();
				break;

			case BixolonPrinter.PROCESS_AUTO_STATUS_BACK:
				StringBuilder buffer = new StringBuilder(0);
				if ((msg.arg2 & BixolonPrinter.AUTO_STATUS_COVER_OPEN) == BixolonPrinter.AUTO_STATUS_COVER_OPEN)
				{
					buffer.Append("Cover is open.\n");
				}
				if ((msg.arg2 & BixolonPrinter.AUTO_STATUS_NO_PAPER) == BixolonPrinter.AUTO_STATUS_NO_PAPER)
				{
					buffer.Append("Paper end sensor: no paper present.\n");
				}

				if (buffer.Capacity > 0)
				{
					Toast.makeText(ApplicationContext, buffer.ToString(), Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(ApplicationContext, "No error.", Toast.LENGTH_SHORT).show();
				}
				break;

			case BixolonPrinter.PROCESS_GET_NV_IMAGE_KEY_CODES:
				data = msg.Data;
				int[] value = data.getIntArray(BixolonPrinter.NV_IMAGE_KEY_CODES);

				Intent intent = new Intent();
				intent.Action = ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES;
				intent.putExtra(EXTRA_NAME_NV_KEY_CODES, value);
				sendBroadcast(intent);
				break;

			case BixolonPrinter.PROCESS_EXECUTE_DIRECT_IO:
				buffer = new StringBuilder();
				data = msg.Data;
				sbyte[] response = data.getByteArray(BixolonPrinter.KEY_STRING_DIRECT_IO);
				for (int i = 0; i < response.Length && response[i] != 0; i++)
				{
					buffer.Append(response[i].ToString("x") + " ");
				}

				Toast.makeText(ApplicationContext, buffer.ToString(), Toast.LENGTH_SHORT).show();
				break;

			case BixolonPrinter.PROCESS_MSR_TRACK:
				intent = new Intent();
				intent.Action = ACTION_GET_MSR_TRACK_DATA;
				intent.putExtra(EXTRA_NAME_MSR_TRACK_DATA, msg.Data);
				sendBroadcast(intent);
				break;

			case BixolonPrinter.PROCESS_GET_MSR_MODE:
				intent = new Intent(MainActivity.this, typeof(MsrActivity));
				intent.putExtra(EXTRA_NAME_MSR_MODE, msg.arg2);
				startActivity(intent);
				break;
			}
		}

		private readonly Handler mHandler = new Handler(new CallbackAnonymousInnerClassHelper());

		private class CallbackAnonymousInnerClassHelper : Handler.Callback
		{
			public CallbackAnonymousInnerClassHelper()
			{
			}


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public boolean handleMessage(android.os.Message msg)
			public override bool handleMessage(Message msg)
			{
				Log.d(TAG, "mHandler.handleMessage(" + msg + ")");

				switch (msg.what)
				{
				case BixolonPrinter.MESSAGE_STATE_CHANGE:
					switch (msg.arg1)
					{
					case BixolonPrinter.STATE_CONNECTED:
						outerInstance.Status = getString(R.@string.title_connected_to, outerInstance.mConnectedDeviceName);
						outerInstance.mListView.Enabled = true;
						outerInstance.mIsConnected = true;
						invalidateOptionsMenu();
						break;

					case BixolonPrinter.STATE_CONNECTING:
						outerInstance.Status = R.@string.title_connecting;
						break;

					case BixolonPrinter.STATE_NONE:
						outerInstance.Status = R.@string.title_not_connected;
						outerInstance.mListView.Enabled = false;
						outerInstance.mIsConnected = false;
						invalidateOptionsMenu();
						outerInstance.mProgressBar.Visibility = View.INVISIBLE;
						break;
					}
					return true;

				case BixolonPrinter.MESSAGE_WRITE:
					switch (msg.arg1)
					{
					case BixolonPrinter.PROCESS_SET_DOUBLE_BYTE_FONT:
						mHandler.obtainMessage(MESSAGE_END_WORK).sendToTarget();

						Toast.makeText(ApplicationContext, "Complete to set double byte font.", Toast.LENGTH_SHORT).show();
						break;

					case BixolonPrinter.PROCESS_DEFINE_NV_IMAGE:
						mBixolonPrinter.DefinedNvImageKeyCodes;
						Toast.makeText(ApplicationContext, "Complete to define NV image", Toast.LENGTH_LONG).show();
						break;

					case BixolonPrinter.PROCESS_REMOVE_NV_IMAGE:
						mBixolonPrinter.DefinedNvImageKeyCodes;
						Toast.makeText(ApplicationContext, "Complete to remove NV image", Toast.LENGTH_LONG).show();
						break;

					case BixolonPrinter.PROCESS_UPDATE_FIRMWARE:
						mBixolonPrinter.disconnect();
						Toast.makeText(ApplicationContext, "Complete to download firmware.\nPlease reboot the printer.", Toast.LENGTH_SHORT).show();
						break;
					}
					return true;

				case BixolonPrinter.MESSAGE_READ:
					outerInstance.dispatchMessage(msg);
					return true;

				case BixolonPrinter.MESSAGE_DEVICE_NAME:
					outerInstance.mConnectedDeviceName = msg.Data.getString(BixolonPrinter.KEY_STRING_DEVICE_NAME);
					Toast.makeText(ApplicationContext, outerInstance.mConnectedDeviceName, Toast.LENGTH_LONG).show();
					return true;

				case BixolonPrinter.MESSAGE_TOAST:
					outerInstance.mListView.Enabled = false;
					Toast.makeText(ApplicationContext, msg.Data.getString(BixolonPrinter.KEY_STRING_TOAST), Toast.LENGTH_SHORT).show();
					return true;

				case BixolonPrinter.MESSAGE_BLUETOOTH_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No paired device", Toast.LENGTH_SHORT).show();
					}
					else
					{
						DialogManager.showBluetoothDialog(outerInstance, (ISet<BluetoothDevice>) msg.obj);
					}
					return true;

				case BixolonPrinter.MESSAGE_PRINT_COMPLETE:
					Toast.makeText(ApplicationContext, "Complete to print", Toast.LENGTH_SHORT).show();
					return true;

				case BixolonPrinter.MESSAGE_ERROR_INVALID_ARGUMENT:
					Toast.makeText(ApplicationContext, "Invalid argument", Toast.LENGTH_SHORT).show();
					return true;

				case BixolonPrinter.MESSAGE_ERROR_NV_MEMORY_CAPACITY:
					Toast.makeText(ApplicationContext, "NV memory capacity error", Toast.LENGTH_SHORT).show();
					return true;

				case BixolonPrinter.MESSAGE_ERROR_OUT_OF_MEMORY:
					Toast.makeText(ApplicationContext, "Out of memory", Toast.LENGTH_SHORT).show();
					return true;

				case BixolonPrinter.MESSAGE_COMPLETE_PROCESS_BITMAP:
					string text = "Complete to process bitmap.";
					Bundle data = msg.Data;
					sbyte[] value = data.getByteArray(BixolonPrinter.KEY_STRING_MONO_PIXELS);
					if (value != null)
					{
						Intent intent = new Intent();
						intent.Action = ACTION_COMPLETE_PROCESS_BITMAP;
						intent.putExtra(EXTRA_NAME_BITMAP_WIDTH, msg.arg1);
						intent.putExtra(EXTRA_NAME_BITMAP_HEIGHT, msg.arg2);
						intent.putExtra(EXTRA_NAME_BITMAP_PIXELS, value);
						sendBroadcast(intent);
					}

					Toast.makeText(ApplicationContext, text, Toast.LENGTH_SHORT).show();
					return true;

				case MESSAGE_START_WORK:
					outerInstance.mListView.Enabled = false;
					outerInstance.mProgressBar.Visibility = View.VISIBLE;
					return true;

				case MESSAGE_END_WORK:
					outerInstance.mListView.Enabled = true;
					outerInstance.mProgressBar.Visibility = View.INVISIBLE;
					return true;

				case BixolonPrinter.MESSAGE_USB_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No connected device", Toast.LENGTH_SHORT).show();
					}
					else
					{
						DialogManager.showUsbDialog(outerInstance, (ISet<UsbDevice>) msg.obj, mUsbReceiver);
					}
					return true;

				case BixolonPrinter.MESSAGE_USB_SERIAL_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No connected device", Toast.LENGTH_SHORT).show();
					}
					else
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.HashMap<String, android.hardware.usb.UsbDevice> usbDeviceMap = (java.util.HashMap<String, android.hardware.usb.UsbDevice>) msg.obj;
						Dictionary<string, UsbDevice> usbDeviceMap = (Dictionary<string, UsbDevice>) msg.obj;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = usbDeviceMap.keySet().toArray(new String[usbDeviceMap.size()]);
						string[] items = usbDeviceMap.Keys.toArray(new string[usbDeviceMap.Count]);
						(new AlertDialog.Builder(outerInstance)).setItems(items, new OnClickListenerAnonymousInnerClassHelper2(this, usbDeviceMap, items))
					   .show();
					}
					return true;

				case BixolonPrinter.MESSAGE_NETWORK_DEVICE_SET:
					if (msg.obj == null)
					{
						Toast.makeText(ApplicationContext, "No connectable device", Toast.LENGTH_SHORT).show();
					}
					DialogManager.showNetworkDialog(outerInstance, (ISet<string>) msg.obj);
					return true;
				}
				return false;
			}

			private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
			{
				private readonly CallbackAnonymousInnerClassHelper outerInstance;

				private Dictionary<string, UsbDevice> usbDeviceMap;
				private string[] items;

				public OnClickListenerAnonymousInnerClassHelper2(CallbackAnonymousInnerClassHelper outerInstance, Dictionary<string, UsbDevice> usbDeviceMap, string[] items) : base(outerInstance.outerInstance)
				{
					this.outerInstance = outerInstance;
					this.usbDeviceMap = usbDeviceMap;
					this.items = items;
				}


				public override void onClick(DialogInterface dialog, int which)
				{
					mBixolonPrinter.connect(usbDeviceMap[items[which]]);
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
				Log.d(TAG, "mUsbReceiver.onReceive(" + context + ", " + intent + ")");
				string action = intent.Action;

				if (UsbManager.ACTION_USB_DEVICE_ATTACHED.Equals(action))
				{
					mBixolonPrinter.connect();
					Toast.makeText(ApplicationContext, "Found USB device", Toast.LENGTH_SHORT).show();
				}
				else if (UsbManager.ACTION_USB_DEVICE_DETACHED.Equals(action))
				{
					mBixolonPrinter.disconnect();
					Toast.makeText(ApplicationContext, "USB device removed", Toast.LENGTH_SHORT).show();
				}

			}
		}
	}

}