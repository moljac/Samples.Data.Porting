using System.Collections.Generic;

namespace com.bixolon.labelprintersample
{

	using AlertDialog = android.app.AlertDialog;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using OnClickListener = android.content.DialogInterface.OnClickListener;
	using IntentFilter = android.content.IntentFilter;
	using UsbDevice = android.hardware.usb.UsbDevice;
	using UsbManager = android.hardware.usb.UsbManager;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	public class DialogManager
	{

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
				MainActivity.mBixolonLabelPrinter.connect(items[which]);

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
				MainActivity.mBixolonLabelPrinter.connect((UsbDevice) usbDevices.ToArray()[which]);

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


			public virtual void onClick(DialogInterface dialog, int which)
			{
				MainActivity.mBixolonLabelPrinter.connect(items[which], 9100, 5000);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: static void showWifiDialog(android.app.AlertDialog dialog, android.content.Context context, final com.bixolon.labelprinter.BixolonLabelPrinter printer)
		internal static void showWifiDialog(AlertDialog dialog, Context context, BixolonLabelPrinter printer)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View layout = inflater.inflate(R.layout.dialog_wifi, null);
				View layout = inflater.inflate(R.layout.dialog_wifi, null);

				dialog = (new AlertDialog.Builder(context)).setView(layout).setTitle("Wi-Fi Connect").setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper4(dialog, printer, layout))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper5(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private BixolonLabelPrinter printer;
			private View layout;

			public OnClickListenerAnonymousInnerClassHelper4(AlertDialog dialog, BixolonLabelPrinter printer, View layout)
			{
				this.dialog = dialog;
				this.printer = printer;
				this.layout = layout;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) layout.findViewById(R.id.editText1);
				string ip = editText.Text.ToString();

				editText = (EditText) layout.findViewById(R.id.editText2);
				int port = int.Parse(editText.Text.ToString());

				printer.connect(ip, port, 5000);
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
//ORIGINAL LINE: static void showPrinterInformationDialog(android.app.AlertDialog dialog, android.content.Context context, final com.bixolon.labelprinter.BixolonLabelPrinter printer)
		internal static void showPrinterInformationDialog(AlertDialog dialog, Context context, BixolonLabelPrinter printer)
		{
			if (dialog == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CharSequence[] ITEMS = { "Model name", "Firmware version" };
				CharSequence[] ITEMS = new CharSequence[] {"Model name", "Firmware version"};
				dialog = (new AlertDialog.Builder(context)).setTitle("Get printer ID").setItems(ITEMS, new OnClickListenerAnonymousInnerClassHelper6(dialog, printer))
					   .create();

			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper6 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private BixolonLabelPrinter printer;

			public OnClickListenerAnonymousInnerClassHelper6(AlertDialog dialog, BixolonLabelPrinter printer)
			{
				this.dialog = dialog;
				this.printer = printer;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{

				if (BixolonLabelPrinter.D)
				{
					Log.i("DialogManager", "++ showPrinterInformationDialog ++");
				}

				if (which == 0)
				{
					if (BixolonLabelPrinter.D)
					{
						Log.i("DialogManager", "++ showPrinterInformationDialog Model Name++");
					}
					printer.getPrinterInformation(BixolonLabelPrinter.PRINTER_INFORMATION_MODEL_NAME);
				}
				else if (which == 1)
				{
					printer.getPrinterInformation(BixolonLabelPrinter.PRINTER_INFORMATION_FIRMWARE_VERSION);
				}

			}
		}

		private static int mmCheckedItem = 0;
		internal static void showSetPrintingTypeDialog(AlertDialog dialog, Context context)
		{
			mmCheckedItem = 0;

			if (dialog == null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CharSequence[] ITEMS = { "Direct thermal", "Thermal transter" };
				CharSequence[] ITEMS = new CharSequence[] {"Direct thermal", "Thermal transter"};
				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_printing_type).setSingleChoiceItems(ITEMS, 0, new OnClickListenerAnonymousInnerClassHelper7(dialog))
					   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper8(dialog))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper9(dialog))
					   .create();
			}
			dialog.show();
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
				mmCheckedItem = which;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper8 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper8(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				int type = BixolonLabelPrinter.PRINTING_TYPE_DIRECT_THERMAL;
				if (mmCheckedItem == 1)
				{
					type = BixolonLabelPrinter.PRINTING_TYPE_THERMAL_TRANSFER;
				}

				MainActivity.mBixolonLabelPrinter.PrintingType = type;
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

		internal static void showSetMarginDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_margin, null);
				View view = inflater.inflate(R.layout.dialog_set_margin, null);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_margin).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper10(dialog, view))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper11(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper10 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper10(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "0";
				}
				int horizontalMargin = int.Parse(editText.Text.ToString());
				editText = (EditText) view.findViewById(R.id.editText2);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "0";
				}
				int verticalMargin = int.Parse(editText.Text.ToString());
				MainActivity.mBixolonLabelPrinter.setMargin(horizontalMargin, verticalMargin);
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


		internal static void showSeBackFeedDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_back_feed, null);
				View view = inflater.inflate(R.layout.dialog_set_back_feed, null);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.EditText editText = (android.widget.EditText) view.findViewById(R.id.editText1);
				EditText editText = (EditText) view.findViewById(R.id.editText1);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.RadioGroup radioGroup = (android.widget.RadioGroup) view.findViewById(R.id.radioGroup1);
				RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
				radioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(editText);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_back_feed_option).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper12(dialog, editText, radioGroup))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper13(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private EditText editText;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(EditText editText)
			{
				this.editText = editText;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				editText.Enabled = checkedId == R.id.radio0;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper12 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private EditText editText;
			private RadioGroup radioGroup;

			public OnClickListenerAnonymousInnerClassHelper12(AlertDialog dialog, EditText editText, RadioGroup radioGroup)
			{
				this.dialog = dialog;
				this.editText = editText;
				this.radioGroup = radioGroup;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{

				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "1";
				}
				int backfeedStep = int.Parse(editText.Text.ToString());

				MainActivity.mBixolonLabelPrinter.setBackFeedOption(radioGroup.CheckedRadioButtonId == R.id.radio0, backfeedStep);
			}
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
				// TODO Auto-generated method stub

			}
		}

		internal static void showSetWidthDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_width, null);
				View view = inflater.inflate(R.layout.dialog_set_width, null);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_width).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper14(dialog, view))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper15(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper14 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper14(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				string @string = editText.Text.ToString();

				if (@string != null && @string.Length > 0)
				{
					int labelWidth = int.Parse(@string);
					MainActivity.mBixolonLabelPrinter.Width = labelWidth;
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
				// TODO Auto-generated method stub

			}
		}

		internal static void showSetLengthDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_length, null);
				View view = inflater.inflate(R.layout.dialog_set_length, null);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_length).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper16(dialog, view))
					   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper17(dialog))
					   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper16 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper16(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				string string1 = editText.Text.ToString();

				editText = (EditText) view.findViewById(R.id.editText2);
				string string2 = editText.Text.ToString();

				if (string1 != null && string1.Length > 0 && string2 != null && string2.Length > 0)
				{
					int labelLength = int.Parse(editText.Text.ToString());
					int gapLength = int.Parse(editText.Text.ToString());

					int mediaType = BixolonLabelPrinter.MEDIA_TYPE_GAP;
					RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
					switch (radioGroup.CheckedRadioButtonId)
					{
					case R.id.radio1:
						mediaType = BixolonLabelPrinter.MEDIA_TYPE_CONTINUOUS;
						break;
					case R.id.radio2:
						mediaType = BixolonLabelPrinter.MEDIA_TYPE_BLACK_MARK;
						break;
					}

					int offsetLength = 0;
					editText = (EditText) view.findViewById(R.id.editText3);
					string @string = editText.Text.ToString();
					if (@string != null && @string.Length > 0)
					{
						offsetLength = int.Parse(@string);
					}

					MainActivity.mBixolonLabelPrinter.setLength(labelLength, gapLength, mediaType, offsetLength);
				}
			}
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
				// TODO Auto-generated method stub

			}
		}

		internal static void showSetBufferModeDialog(AlertDialog dialog, Context context)
		{
			mmCheckedItem = 0;
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Set buffer mode").setSingleChoiceItems(new string[] {"false", "true"}, 0, new OnClickListenerAnonymousInnerClassHelper18())
						   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper19(dialog))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper20(dialog))
						   .create();
			}
			dialog.show();




		}

		private class OnClickListenerAnonymousInnerClassHelper18 : DialogInterface.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper18()
			{
			}


			public virtual void onClick(DialogInterface arg0, int arg1)
			{
				mmCheckedItem = arg1;
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

				MainActivity.mBixolonLabelPrinter.BufferMode = mmCheckedItem == 1;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper20 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper20(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}
		internal static void showSetSpeedDialog(AlertDialog dialog, Context context)
		{
			mmCheckedItem = 0;
			if (dialog == null)
			{
				string[] items = new string[] {"2.5 ips", "3.0 ips", "4.0 ips", "5.0 ips", "6.0 ips", "7.0 ips", "8.0 ips"};
				dialog = (new AlertDialog.Builder(context)).setTitle("Set speed").setSingleChoiceItems(items, 0, new OnClickListenerAnonymousInnerClassHelper21())
						   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper22(dialog))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper23(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper21 : DialogInterface.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper21()
			{
			}


			public virtual void onClick(DialogInterface arg0, int arg1)
			{
				mmCheckedItem = arg1;

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper22 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper22(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (mmCheckedItem)
				{
				case 0:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_25IPS;
					break;
				case 1:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_30IPS;
					break;
				case 2:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_40IPS;
					break;
				case 3:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_50IPS;
					break;
				case 4:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_60IPS;
					break;
				case 5:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_70IPS;
					break;
				case 6:
					MainActivity.mBixolonLabelPrinter.Speed = BixolonLabelPrinter.SPEED_80IPS;
					break;
				}
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

		internal static void showSetDensityDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_density, null);
				View view = inflater.inflate(R.layout.dialog_set_density, null);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_density).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper24(dialog, view))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper25(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper24 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper24(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				int density = int.Parse(editText.Text.ToString());
				MainActivity.mBixolonLabelPrinter.Density = density;
			}
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
				// TODO Auto-generated method stub

			}
		}

		internal static void showSetOrientationDialog(AlertDialog dialog, Context context)
		{
			mmCheckedItem = 0;
			if (dialog == null)
			{
				dialog = (new AlertDialog.Builder(context)).setTitle("Set orientation").setSingleChoiceItems(new string[] {"Print from top to bottom (default)", "Print from bottom to top"}, 0, new OnClickListenerAnonymousInnerClassHelper26())
						   .setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper27(dialog))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper28(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper26 : DialogInterface.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper26()
			{
			}


			public virtual void onClick(DialogInterface arg0, int arg1)
			{
				mmCheckedItem = arg1;

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper27 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper27(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{

				if (mmCheckedItem == 0)
				{
					MainActivity.mBixolonLabelPrinter.Orientation = BixolonLabelPrinter.ORIENTATION_TOP_TO_BOTTOM;
				}
				else
				{
					MainActivity.mBixolonLabelPrinter.Orientation = BixolonLabelPrinter.ORIENTATION_BOTTOM_TO_TOP;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper28 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper28(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showSetOffsetDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_offset, null);
				View view = inflater.inflate(R.layout.dialog_set_offset, null);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.set_offset).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper29(dialog, view))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper30(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper29 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper29(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				int offset = int.Parse(editText.Text.ToString());
				MainActivity.mBixolonLabelPrinter.Offset = offset;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper30 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper30(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showCutterPositionSettingDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_set_offset, null);
				View view = inflater.inflate(R.layout.dialog_set_offset, null);

				dialog = (new AlertDialog.Builder(context)).setTitle("Cutter position setting").setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper31(dialog, view))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper32(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper31 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private View view;

			public OnClickListenerAnonymousInnerClassHelper31(AlertDialog dialog, View view)
			{
				this.dialog = dialog;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) view.findViewById(R.id.editText1);
				int position = int.Parse(editText.Text.ToString());
				MainActivity.mBixolonLabelPrinter.CutterPosition = position;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper32 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper32(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		internal static void showAutoCutterDialog(AlertDialog dialog, Context context)
		{
			if (dialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
				View view = inflater.inflate(R.layout.dialog_set_auto_cutter, null);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.EditText editText = (android.widget.EditText) view.findViewById(R.id.editText1);
				EditText editText = (EditText) view.findViewById(R.id.editText1);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.RadioGroup radioGroup = (android.widget.RadioGroup) view.findViewById(R.id.radioGroup1);
				RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
				radioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper2(editText);

				dialog = (new AlertDialog.Builder(context)).setTitle(R.@string.cutting_action).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper33(dialog, editText, radioGroup))
						   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper34(dialog))
						   .create();
			}
			dialog.show();
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper2 : RadioGroup.OnCheckedChangeListener
		{
			private EditText editText;

			public OnCheckedChangeListenerAnonymousInnerClassHelper2(EditText editText)
			{
				this.editText = editText;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				editText.Enabled = checkedId == R.id.radio0;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper33 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;
			private EditText editText;
			private RadioGroup radioGroup;

			public OnClickListenerAnonymousInnerClassHelper33(AlertDialog dialog, EditText editText, RadioGroup radioGroup)
			{
				this.dialog = dialog;
				this.editText = editText;
				this.radioGroup = radioGroup;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{

				int cuttingPeriod = int.Parse(editText.Text.ToString());

				MainActivity.mBixolonLabelPrinter.setAutoCutter(radioGroup.CheckedRadioButtonId == R.id.radio0, cuttingPeriod);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper34 : DialogInterface.OnClickListener
		{
			private AlertDialog dialog;

			public OnClickListenerAnonymousInnerClassHelper34(AlertDialog dialog)
			{
				this.dialog = dialog;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}
	}

}