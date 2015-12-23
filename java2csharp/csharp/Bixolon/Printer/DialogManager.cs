using System.Collections.Generic;

namespace com.bixolon.printersample
{


	using AlertDialog = android.app.AlertDialog;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using IntentFilter = android.content.IntentFilter;
	using UsbDevice = android.hardware.usb.UsbDevice;
	using UsbManager = android.hardware.usb.UsbManager;
	using Handler = android.os.Handler;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using CheckBox = android.widget.CheckBox;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using Toast = android.widget.Toast;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class DialogManager
	{

		private static readonly string[] CODE_PAGE_ITEMS = new string[] {"Page 0 437 (USA, Standard Europe)", "Page 1 Katakana", "Page 2 850 (Multilingual)", "Page 3 860 (Portuguese)", "Page 4 863 (Canadian-French)", "Page 5 865 (Nordic)", "Page 16 1252 (Latin I)", "Page 17 866 (Cyrillic #2)", "Page 18 852 (Latin 2)", "Page 19 858 (Euro)", "Page 21 862 (Hebrew DOS code)", "Page 22 864 (Arabic)", "Page 23 Thai42", "Page 24 1253 (Greek)", "Page 25 1254 (Turkish)", "Page 26 1257 (Baltic)", "Page 27 Farsi", "Page 28 1251 (Cyrillic)", "Page 29 737 (Greek)", "Page 30 775 (Baltic)", "Page 31 Thai14", "Page 33 1255 (Hebrew New code)", "Page 34 Thai 11", "Page 35 Thai 18", "Page 36 855 (Cyrillic)", "Page 37 857 (Turkish)", "Page 38 928 (Greek)", "Page 39 Thai 16", "Page 40 1256 (Arabic)", "Page 41 1258 (Vietnam)", "Page 42 KHMER(Cambodia)", "Page 47 1250 (Czech)", "KS5601 (double byte font)", "BIG5 (double byte font)", "GB2312 (double byte font)", "SHIFT-JIS (double byte font)"};

		private static readonly string[] PRINTER_ID_ITEMS = new string[] {"Firmware version", "Manufacturer", "Printer model", "Code page"};

		private static readonly string[] PRINT_SPEED_ITEMS = new string[] {"High speed", "Medium speed", "Low Speed"};

		private static readonly string[] PRINT_DENSITY_ITEMS = new string[] {"Light density", "Default density", "Dark density"};

		private static readonly string[] PRINT_COLOR_ITEMS = new string[] {"Black", "Red"};

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showBluetoothDialog(android.content.Context context, final java.util.Set<android.bluetooth.BluetoothDevice> pairedDevices)
		internal static void showBluetoothDialog(Context context, ISet<BluetoothDevice> pairedDevices)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = new String[pairedDevices.size()];
			string[] items = new string[pairedDevices.Count];
			int index = 0;
			foreach (BluetoothDevice device in pairedDevices)
			{
				items[index++] = device.Address;
			}

			(new AlertDialog.Builder(context)).setTitle("Paired Bluetooth printers").setItems(items, new OnClickListenerAnonymousInnerClassHelper(items))
				   .show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private string[] items;

			public OnClickListenerAnonymousInnerClassHelper(string[] items)
			{
				this.items = items;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonPrinter.connect(items[which]);

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showUsbDialog(final android.content.Context context, final java.util.Set<android.hardware.usb.UsbDevice> usbDevices, final android.content.BroadcastReceiver usbReceiver)
		internal static void showUsbDialog(Context context, ISet<UsbDevice> usbDevices, BroadcastReceiver usbReceiver)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = new String[usbDevices.size()];
			string[] items = new string[usbDevices.Count];
			int index = 0;
			foreach (UsbDevice device in usbDevices)
			{
				items[index++] = "Device name: " + device.DeviceName + ", Product ID: " + device.ProductId + ", Device ID: " + device.DeviceId;
			}

			(new AlertDialog.Builder(context)).setTitle("Connected USB printers").setItems(items, new OnClickListenerAnonymousInnerClassHelper2(context, usbDevices, usbReceiver))
				   .show();
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private Context context;
			private ISet<UsbDevice> usbDevices;
			private BroadcastReceiver usbReceiver;

			public OnClickListenerAnonymousInnerClassHelper2(Context context, ISet<UsbDevice> usbDevices, BroadcastReceiver usbReceiver)
			{
				this.context = context;
				this.usbDevices = usbDevices;
				this.usbReceiver = usbReceiver;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonPrinter.connect((UsbDevice) usbDevices.ToArray()[which]);

				// listen for new devices
				IntentFilter filter = new IntentFilter();
				filter.addAction(UsbManager.ACTION_USB_DEVICE_ATTACHED);
				filter.addAction(UsbManager.ACTION_USB_DEVICE_DETACHED);
				context.registerReceiver(usbReceiver, filter);
			}
		}

		internal static void showNetworkDialog(Context context, ISet<string> ipAddressSet)
		{
			if (ipAddressSet != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = ipAddressSet.toArray(new String[ipAddressSet.size()]);
				 string[] items = ipAddressSet.toArray(new string[ipAddressSet.Count]);

				(new AlertDialog.Builder(context)).setTitle("Connectable network printers").setItems(items, new OnClickListenerAnonymousInnerClassHelper3(items))
			   .show();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : DialogInterface.OnClickListener
		{
			private string[] items;

			public OnClickListenerAnonymousInnerClassHelper3(string[] items)
			{
				this.items = items;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonPrinter.connect(items[which], 9100, 5000);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showPdf417Dialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showPdf417Dialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View layout = inflater.inflate(R.layout.dialog_print_pdf417, null);
				View layout = inflater.inflate(R.layout.dialog_print_pdf417, null);

				dialog = (new AlertDialog.Builder(context)).setView(layout).setTitle("PDF417").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper4(dialog, context, layout))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper5(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private Context context;
			private View layout;

			public OnClickListenerAnonymousInnerClassHelper4(AlertDialog dialog, Context context, View layout)
			{
				this.dialog = dialog;
				this.context = context;
				this.layout = layout;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) layout.findViewById(R.id.editText1);
				string data = editText.Text.ToString();

				editText = (EditText) layout.findViewById(R.id.editText2);
				string @string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(context, "Please enter the width again.", Toast.LENGTH_SHORT).show();
					return;
				}
				int width = int.Parse(@string);

				editText = (EditText) layout.findViewById(R.id.editText3);
				@string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(context, "Please enter the height again.", Toast.LENGTH_SHORT).show();
					return;
				}
				int height = int.Parse(@string);

				int alignment = BixolonPrinter.ALIGNMENT_LEFT;
				RadioGroup radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup1);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio1:
					alignment = BixolonPrinter.ALIGNMENT_CENTER;
					break;

				case R.id.radio2:
					alignment = BixolonPrinter.ALIGNMENT_RIGHT;
					break;
				}

				CheckBox checkBox = (CheckBox) layout.findViewById(R.id.checkBox1);
				if (checkBox.Checked)
				{
					MainActivity.mBixolonPrinter.printPdf417(data, alignment, width, height, false);
					MainActivity.mBixolonPrinter.formFeed(true);
				}
				else
				{
					MainActivity.mBixolonPrinter.printPdf417(data, alignment, width, height, true);
				}

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper5(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showQrCodeDialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showQrCodeDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View layout = inflater.inflate(R.layout.dialog_print_qrcode, null);
				View layout = inflater.inflate(R.layout.dialog_print_qrcode, null);

				dialog = (new AlertDialog.Builder(context)).setView(layout).setTitle("QR Code").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper6(dialog, context, layout))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper7(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper6 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private Context context;
			private View layout;

			public OnClickListenerAnonymousInnerClassHelper6(AlertDialog dialog, Context context, View layout)
			{
				this.dialog = dialog;
				this.context = context;
				this.layout = layout;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) layout.findViewById(R.id.editText1);
				string data = editText.Text.ToString();

				int model = BixolonPrinter.QR_CODE_MODEL2;
				RadioGroup radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup1);
				if (radioGroup.CheckedRadioButtonId == R.id.radio0)
				{
					model = BixolonPrinter.QR_CODE_MODEL1;
				}

				editText = (EditText) layout.findViewById(R.id.editText2);
				string @string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(context, "Please enter the size again.", Toast.LENGTH_SHORT).show();
					return;
				}
				int size = int.Parse(@string);

				int alignment = BixolonPrinter.ALIGNMENT_LEFT;
				radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup2);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio3:
					alignment = BixolonPrinter.ALIGNMENT_CENTER;
					break;

				case R.id.radio4:
					alignment = BixolonPrinter.ALIGNMENT_RIGHT;
					break;
				}

				CheckBox checkBox = (CheckBox) layout.findViewById(R.id.checkBox1);
				if (checkBox.Checked)
				{
					MainActivity.mBixolonPrinter.printQrCode(data, alignment, model, size, false);
					MainActivity.mBixolonPrinter.formFeed(true);
				}
				else
				{
					MainActivity.mBixolonPrinter.printQrCode(data, alignment, model, size, true);
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper7 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper7(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showMaxiCodeDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View layout = inflater.inflate(R.layout.dialog_print_maxicode, null);
				View layout = inflater.inflate(R.layout.dialog_print_maxicode, null);

				dialog = (new AlertDialog.Builder(context)).setView(layout).setTitle("Maxi Code").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper8(dialog, layout))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper9(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper8 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View layout;

			public OnClickListenerAnonymousInnerClassHelper8(AlertDialog dialog, View layout)
			{
				this.dialog = dialog;
				this.layout = layout;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) layout.findViewById(R.id.editText1);
				string data = editText.Text.ToString();

				int mode = BixolonPrinter.MAXI_CODE_MODE2;
				RadioGroup radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup1);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio0:
					mode = BixolonPrinter.MAXI_CODE_MODE2;
					sbyte[] header = new sbyte[] {(sbyte)'[', (sbyte)')', (sbyte)'>', 0x1e, (sbyte)'0', (sbyte)'1', 0x1d, (sbyte)'9', (sbyte)'6', (sbyte)'1', (sbyte)'2', (sbyte)'3', (sbyte)'4', (sbyte)'5', (sbyte)'6', (sbyte)'7', (sbyte)'8', (sbyte)'9', 0x1d, (sbyte)'0', (sbyte)'0', (sbyte)'7', 0x1d, (sbyte)'2', (sbyte)'5', (sbyte)'0', 0x1d};
					data = StringHelperClass.NewString(header) + data;
					break;

				case R.id.radio1:
					mode = BixolonPrinter.MAXI_CODE_MODE3;
					header = new sbyte[] {(sbyte)'[', (sbyte)')', (sbyte)'>', 0x1e, (sbyte)'0', (sbyte)'1', 0x1d, (sbyte)'9', (sbyte)'6', (sbyte)'F', (sbyte)'A', (sbyte)'B', (sbyte)'C', (sbyte)'D', (sbyte)'E', 0x1d, (sbyte)'0', (sbyte)'0', (sbyte)'7', 0x1d, (sbyte)'2', (sbyte)'5', (sbyte)'0', 0x1d};
					data = StringHelperClass.NewString(header) + data;
					break;

				case R.id.radio2:
					mode = BixolonPrinter.MAXI_CODE_MODE4;
					header = new sbyte[] {(sbyte)'E', 0x1d, (sbyte)'7', 0x1d, (sbyte)'5', 0x1d};
					data = StringHelperClass.NewString(header) + data;
					break;
				}

				int alignment = BixolonPrinter.ALIGNMENT_LEFT;
				radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup2);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio4:
					alignment = BixolonPrinter.ALIGNMENT_CENTER;
					break;

				case R.id.radio5:
					alignment = BixolonPrinter.ALIGNMENT_RIGHT;
					break;
				}

				MainActivity.mBixolonPrinter.printMaxiCode(data, alignment, mode, true);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper9 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper9(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showDataMatrixDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View layout = inflater.inflate(R.layout.dialog_print_datamatrix, null);
				View layout = inflater.inflate(R.layout.dialog_print_datamatrix, null);

				dialog = (new AlertDialog.Builder(context)).setView(layout).setTitle("DATAMATRIX").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper10(dialog, layout))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper11(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper10 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View layout;

			public OnClickListenerAnonymousInnerClassHelper10(AlertDialog dialog, View layout)
			{
				this.dialog = dialog;
				this.layout = layout;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) layout.findViewById(R.id.editText1);
				string data = editText.Text.ToString();

				editText = (EditText) layout.findViewById(R.id.editText2);
				int size = int.Parse(editText.Text.ToString());

				int alignment = BixolonPrinter.ALIGNMENT_LEFT;
				RadioGroup radioGroup = (RadioGroup) layout.findViewById(R.id.radioGroup1);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio1:
					alignment = BixolonPrinter.ALIGNMENT_CENTER;
					break;

				case R.id.radio2:
					alignment = BixolonPrinter.ALIGNMENT_RIGHT;
					break;
				}

				MainActivity.mBixolonPrinter.printDataMatrix(data, alignment, size, true);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper11 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper11(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showCodePageDialog(android.app.AlertDialog dialog, final android.content.Context context, final android.os.Handler handler)
		internal static void showCodePageDialog(AlertDialog dialog, Context context, Handler handler)
		{
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Code page").setItems(CODE_PAGE_ITEMS, new OnClickListenerAnonymousInnerClassHelper12(dialog, context, handler))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper12 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private Context context;
			private Handler handler;

			public OnClickListenerAnonymousInnerClassHelper12(AlertDialog dialog, Context context, Handler handler)
			{
				this.dialog = dialog;
				this.context = context;
				this.handler = handler;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_437_USA;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page437), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 1:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_KATAKANA;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_katakana), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 2:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_850_MULTILINGUAL;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page850), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 3:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_860_PORTUGUESE;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page860), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 4:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_863_CANADIAN_FRENCH;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page863), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 5:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_865_NORDIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page865), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 6:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1252_LATIN1;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1252), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 7:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_866_CYRILLIC2;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page866), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 8:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_852_LATIN2;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page852), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 9:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_858_EURO;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page858), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 10:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_862_HEBREW_DOS_CODE;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page862), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 11:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_864_ARABIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page864), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 12:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_THAI42;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_thai42), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 13:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1253_GREEK;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1253), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 14:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1254_TURKISH;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1254), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 15:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1257_BALTIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1257), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 16:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_FARSI;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_farsi), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 17:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1251_CYRILLIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1251), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 18:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_737_GREEK;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page737), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 19:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_775_BALTIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page775), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 20:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_THAI14;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_thai14), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 21:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1255_HEBREW_NEW_CODE;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1255), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 22:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_THAI11;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_thai11), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 23:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_THAI18;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_thai18), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 24:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_855_CYRILLIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page855), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 25:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_857_TURKISH;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page857), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 26:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_928_GREEK;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page928), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 27:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_THAI16;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_thai16), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 28:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1256_ARABIC;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1256), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 29:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1258_VIETNAM;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1258), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 30:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_KHMER_CAMBODIA;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_khmer_cambodia), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 31:
					MainActivity.mBixolonPrinter.SingleByteFont = BixolonPrinter.CODE_PAGE_1250_CZECH;
					MainActivity.mBixolonPrinter.printText(CODE_PAGE_ITEMS[which] + "\n", BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A | BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					MainActivity.mBixolonPrinter.printText(context.getString(R.@string.code_page_windows1250), BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
					break;

				case 32:
					handler.obtainMessage(MainActivity.MESSAGE_START_WORK).sendToTarget();
					(new Handler()).postDelayed(() =>
					{

						MainActivity.mBixolonPrinter.DoubleByteFont = BixolonPrinter.DOUBLE_BYTE_FONT_KS5601;
					}, 1000);
					break;

				case 33:
					handler.obtainMessage(MainActivity.MESSAGE_START_WORK).sendToTarget();
					(new Handler()).postDelayed(() =>
					{

						MainActivity.mBixolonPrinter.DoubleByteFont = BixolonPrinter.DOUBLE_BYTE_FONT_BIG5;
					}, 1000);
					break;

				case 34:
					handler.obtainMessage(MainActivity.MESSAGE_START_WORK).sendToTarget();
					(new Handler()).postDelayed(() =>
					{

						MainActivity.mBixolonPrinter.DoubleByteFont = BixolonPrinter.DOUBLE_BYTE_FONT_GB2312;
					}, 1000);
					break;

				case 35:
					handler.obtainMessage(MainActivity.MESSAGE_START_WORK).sendToTarget();
					(new Handler()).postDelayed(() =>
					{

						MainActivity.mBixolonPrinter.DoubleByteFont = BixolonPrinter.DOUBLE_BYTE_FONT_SHIFT_JIS;
					}, 1000);
					break;
				}

			}
		}

		internal static void showPrinterIdDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Get printer ID").setItems(PRINTER_ID_ITEMS, new OnClickListenerAnonymousInnerClassHelper13(dialog))
					   .create();

			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper13 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper13(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					MainActivity.mBixolonPrinter.getPrinterId(BixolonPrinter.PRINTER_ID_FIRMWARE_VERSION);
					break;
				case 1:
					MainActivity.mBixolonPrinter.getPrinterId(BixolonPrinter.PRINTER_ID_MANUFACTURER);
					break;
				case 2:
					MainActivity.mBixolonPrinter.getPrinterId(BixolonPrinter.PRINTER_ID_PRINTER_MODEL);
					break;
				case 3:
					MainActivity.mBixolonPrinter.getPrinterId(BixolonPrinter.PRINTER_ID_CODE_PAGE);
					break;
				}

			}
		}

		private static int mSpeed = BixolonPrinter.PRINT_SPEED_HIGH;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showPrintSpeedDialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showPrintSpeedDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Print speed").setSingleChoiceItems(PRINT_SPEED_ITEMS, 0, new OnClickListenerAnonymousInnerClassHelper14(dialog))
				   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper15(dialog))
				   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper16(dialog))
				   .create();
			}
			mSpeed = BixolonPrinter.PRINT_SPEED_HIGH;
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper14 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper14(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					mSpeed = BixolonPrinter.PRINT_SPEED_HIGH;
					break;
				case 1:
					mSpeed = BixolonPrinter.PRINT_SPEED_MEDIUM;
					break;
				case 2:
					mSpeed = BixolonPrinter.PRINT_SPEED_LOW;
					break;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper15 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper15(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonPrinter.PrintSpeed = mSpeed;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper16 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper16(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		private static int mDensity = BixolonPrinter.PRINT_DENSITY_DEFAULT;
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showPrintDensityDialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showPrintDensityDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Print density").setSingleChoiceItems(PRINT_DENSITY_ITEMS, 1, new OnClickListenerAnonymousInnerClassHelper17(dialog))
				   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper18(dialog))
				   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper19(dialog))
				   .create();
			}
			mDensity = BixolonPrinter.PRINT_DENSITY_DEFAULT;
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper17 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper17(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					mDensity = BixolonPrinter.PRINT_DENSITY_LIGHT;
					break;
				case 1:
					mDensity = BixolonPrinter.PRINT_DENSITY_DEFAULT;
					break;
				case 2:
					mDensity = BixolonPrinter.PRINT_DENSITY_DARK;
					break;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper18 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper18(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonPrinter.PrintDensity = mDensity;

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper19 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper19(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showDirectIoDialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showDirectIoDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_direct_io, null);
				View view = inflater.inflate(R.layout.dialog_direct_io, null);

				dialog = (new AlertDialog.Builder(context)).setView(view).setTitle("Direct IO").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper20(dialog, context, view))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper21(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper20 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private Context context;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper20(AlertDialog dialog, Context context, View view)
			{
				this.dialog = dialog;
				this.context = context;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				string @string = editText.Text.ToString();
				StringTokenizer stringTokenize = new StringTokenizer(@string);

				List<sbyte?> arrayList = new List<sbyte?>();
				while (stringTokenize.hasMoreTokens())
				{
					try
					{
						sbyte b = sbyte.Parse(stringTokenize.nextToken(), 16);
						arrayList.Add(b);
					}
					catch (System.FormatException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(context, "Invalid command!", Toast.LENGTH_SHORT).show();
						return;
					}
				}

				CheckBox checkBox = (CheckBox) view.findViewById(R.id.checkBox1);
				bool hasResponse = checkBox.Checked;

				if (arrayList.Count > 0)
				{
					ByteBuffer buffer = ByteBuffer.allocate(arrayList.Count);
					for (int i = 0; i < arrayList.Count; i++)
					{
						buffer.put(arrayList[i]);
					}

					MainActivity.mBixolonPrinter.executeDirectIo(buffer.array(), hasResponse);
				}

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper21 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper21(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showPowerSavingModeDialog(android.app.AlertDialog dialog, final android.content.Context context)
		internal static void showPowerSavingModeDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_power_saving_mode, null);
				View view = inflater.inflate(R.layout.dialog_power_saving_mode, null);

				dialog = (new AlertDialog.Builder(context)).setView(view).setTitle("Set page mode").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper22(dialog, context, view))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper23(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper22 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private Context context;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper22(AlertDialog dialog, Context context, View view)
			{
				this.dialog = dialog;
				this.context = context;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				CheckBox checkBox = (CheckBox) view.findViewById(R.id.checkBox1);
				bool enabled = checkBox.Checked;
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				string @string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(context, "Please enter the time again.", Toast.LENGTH_SHORT).show();
					return;
				}
				int time = int.Parse(@string);
				MainActivity.mBixolonPrinter.setPowerSavingMode(enabled, time);
			}

		}

		private class OnClickListenerAnonymousInnerClassHelper23 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper23(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showBsCodePageDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				string[] items = new string[CODE_PAGE_ITEMS.Length - 4];
				for (int i = 0; i < items.Length; i++)
				{
					items[i] = CODE_PAGE_ITEMS[i];
				}
				dialog = (new AlertDialog.Builder(context)).setItems(items, new OnClickListenerAnonymousInnerClassHelper24(dialog))
			   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper24 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper24(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_437_USA;
					break;
				case 1:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_KATAKANA;
					break;
				case 2:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_850_MULTILINGUAL;
					break;
				case 3:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_860_PORTUGUESE;
					break;
				case 4:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_863_CANADIAN_FRENCH;
					break;
				case 5:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_865_NORDIC;
					break;
				case 6:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1252_LATIN1;
					break;
				case 7:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_866_CYRILLIC2;
					break;
				case 8:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_852_LATIN2;
					break;
				case 9:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_858_EURO;
					break;
				case 10:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_862_HEBREW_DOS_CODE;
					break;
				case 11:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_864_ARABIC;
					break;
				case 12:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_THAI42;
					break;
				case 13:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1253_GREEK;
					break;
				case 14:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1254_TURKISH;
					break;
				case 15:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1257_BALTIC;
					break;
				case 16:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_FARSI;
					break;
				case 17:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1251_CYRILLIC;
					break;
				case 18:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_737_GREEK;
					break;
				case 19:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_775_BALTIC;
					break;
				case 20:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_THAI14;
					break;
				case 21:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1255_HEBREW_NEW_CODE;
					break;
				case 22:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_THAI11;
					break;
				case 23:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_THAI18;
					break;
				case 24:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_855_CYRILLIC;
					break;
				case 25:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_857_TURKISH;
					break;
				case 26:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_928_GREEK;
					break;
				case 27:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_THAI16;
					break;
				case 28:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1256_ARABIC;
					break;
				case 29:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1258_VIETNAM;
					break;
				case 30:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_KHMER_CAMBODIA;
					break;
				case 31:
					MainActivity.mBixolonPrinter.BsCodePage = BixolonPrinter.CODE_PAGE_1250_CZECH;
					break;
				}
			}
		}

		internal static void showPrintColorDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Print color").setItems(PRINT_COLOR_ITEMS, new OnClickListenerAnonymousInnerClassHelper25(dialog))
			   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper25 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper25(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					MainActivity.mBixolonPrinter.PrintColor = BixolonPrinter.COLOR_BLACK;
					break;

				case 1:
					MainActivity.mBixolonPrinter.PrintColor = BixolonPrinter.COLOR_RED;
					break;
				}
			}
		}
	}

}