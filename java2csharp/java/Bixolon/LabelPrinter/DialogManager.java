package com.bixolon.labelprintersample;

import java.util.Set;

import android.app.AlertDialog;
import android.bluetooth.BluetoothDevice;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.DialogInterface.OnClickListener;
import android.content.IntentFilter;
import android.hardware.usb.UsbDevice;
import android.hardware.usb.UsbManager;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;

import com.bixolon.labelprinter.BixolonLabelPrinter;

public class DialogManager {
	
	static void showBluetoothDialog(Context context, final Set<BluetoothDevice> pairedDevices) {
		final String[] items = new String[pairedDevices.size()];
		int index = 0;
		for (BluetoothDevice device : pairedDevices) {
			items[index++] = device.getAddress();
		}

		new AlertDialog.Builder(context).setTitle("Paired Bluetooth printers")
				.setItems(items, new DialogInterface.OnClickListener() {
					
					public void onClick(DialogInterface dialog, int which) {
						MainActivity.mBixolonLabelPrinter.connect(items[which]);
						
					}
				}).show();
	}
	
	static void showUsbDialog(final Context context, final Set<UsbDevice> usbDevices, final BroadcastReceiver usbReceiver) {
		final String[] items = new String[usbDevices.size()];
		int index = 0;
		for (UsbDevice device : usbDevices) {
			items[index++] = "Device name: " + device.getDeviceName() + ", Product ID: " + device.getProductId() + ", Device ID: " + device.getDeviceId();
		}

		new AlertDialog.Builder(context).setTitle("Connected USB printers")
				.setItems(items, new DialogInterface.OnClickListener() {
					
					public void onClick(DialogInterface dialog, int which) {
						MainActivity.mBixolonLabelPrinter.connect((UsbDevice) usbDevices.toArray()[which]);
						
						// listen for new devices
						IntentFilter filter = new IntentFilter();
						filter.addAction(UsbManager.ACTION_USB_DEVICE_ATTACHED);
						filter.addAction(UsbManager.ACTION_USB_DEVICE_DETACHED);
						context.registerReceiver(usbReceiver, filter);
					}
				}).show();
	}

	static void showNetworkDialog(Context context, Set<String> ipAddressSet) {
		if (ipAddressSet != null) {
			 final String[] items = ipAddressSet.toArray(new String[ipAddressSet.size()]);
			
			new AlertDialog.Builder(context).setTitle("Connectable network printers")
			.setItems(items, new DialogInterface.OnClickListener() {
				
				public void onClick(DialogInterface dialog, int which) {
					MainActivity.mBixolonLabelPrinter.connect(items[which], 9100, 5000);
				}
			}).show();
		}
	}
	
	static void showWifiDialog(AlertDialog dialog, Context context, final BixolonLabelPrinter printer) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View layout = inflater.inflate(R.layout.dialog_wifi, null);

			dialog = new AlertDialog.Builder(context).setView(layout).setTitle("Wi-Fi Connect")
					.setPositiveButton("OK", new OnClickListener() {

						public void onClick(DialogInterface dialog, int which) {
							EditText editText = (EditText) layout.findViewById(R.id.editText1);
							String ip = editText.getText().toString();

							editText = (EditText) layout.findViewById(R.id.editText2);
							int port = Integer.parseInt(editText.getText().toString());

							printer.connect(ip, port, 5000);
						}
					}).setNegativeButton("Cancel", new DialogInterface.OnClickListener() {

						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub

						}
					}).create();
		}
		dialog.show();
	}

	static void showPrinterInformationDialog(AlertDialog dialog, Context context, final BixolonLabelPrinter printer) {
		if (dialog == null) {
			final CharSequence[] ITEMS = {
					"Model name",
					"Firmware version"
			};
			dialog = new AlertDialog.Builder(context).setTitle("Get printer ID")
					.setItems(ITEMS, new OnClickListener() {

						public void onClick(DialogInterface dialog, int which) {
							
							if (BixolonLabelPrinter.D) {
								Log.i("DialogManager", "++ showPrinterInformationDialog ++");
							}
							
							if (which == 0) {
								if (BixolonLabelPrinter.D) {
									Log.i("DialogManager", "++ showPrinterInformationDialog Model Name++");
								}
								printer.getPrinterInformation(BixolonLabelPrinter.PRINTER_INFORMATION_MODEL_NAME);
							} else if (which == 1) {
								printer.getPrinterInformation(BixolonLabelPrinter.PRINTER_INFORMATION_FIRMWARE_VERSION);
							}

						}
					}).create();

		}
		dialog.show();
	}
	
	private static int mmCheckedItem = 0;
	static void showSetPrintingTypeDialog(AlertDialog dialog, Context context) {
		mmCheckedItem = 0;
		
		if (dialog == null) {
			final CharSequence[] ITEMS = {
					"Direct thermal",
					"Thermal transter"
			};
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_printing_type)
					.setSingleChoiceItems(ITEMS, 0, new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							mmCheckedItem = which;
						}
					}).setPositiveButton("OK", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							int type = BixolonLabelPrinter.PRINTING_TYPE_DIRECT_THERMAL;
							if (mmCheckedItem == 1) {
								type = BixolonLabelPrinter.PRINTING_TYPE_THERMAL_TRANSFER;
							}
							
							MainActivity.mBixolonLabelPrinter.setPrintingType(type);
						}
					}).setNegativeButton("Cancel", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub
							
						}
					}).create();
		}
		dialog.show();
	}
	
	static void showSetMarginDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_margin, null);
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_margin).setView(view)
					.setPositiveButton("OK", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							EditText editText = (EditText) view.findViewById(R.id.editText1);
							if(editText.getText().toString().equals("")){
								editText.setText("0");
							}
							int horizontalMargin = Integer.parseInt(editText.getText().toString());
							editText = (EditText) view.findViewById(R.id.editText2);
							if(editText.getText().toString().equals("")){
								editText.setText("0");
							}
							int verticalMargin = Integer.parseInt(editText.getText().toString());
							MainActivity.mBixolonLabelPrinter.setMargin(horizontalMargin, verticalMargin);
						}
					}).setNegativeButton("Cancel", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub
							
						}
					}).create();
		}
		dialog.show();
	}
	
	
	static void showSeBackFeedDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_back_feed, null);
			
			final EditText editText = (EditText) view.findViewById(R.id.editText1);
			
			final RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
			radioGroup.setOnCheckedChangeListener(new OnCheckedChangeListener() {
				
				public void onCheckedChanged(RadioGroup group, int checkedId) {
					editText.setEnabled(checkedId == R.id.radio0);
				}
			});
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_back_feed_option).setView(view)
						.setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								
								if(editText.getText().toString().equals("")){
									editText.setText("1");
								}
								int backfeedStep = Integer.parseInt(editText.getText().toString());
								
								MainActivity.mBixolonLabelPrinter.setBackFeedOption(radioGroup.getCheckedRadioButtonId() == R.id.radio0, backfeedStep);
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showSetWidthDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_width, null);
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_width).setView(view)
					.setPositiveButton("OK", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							EditText editText = (EditText) view.findViewById(R.id.editText1);
							String string = editText.getText().toString();
							
							if (string != null && string.length() > 0) {
								int labelWidth = Integer.parseInt(string);
								MainActivity.mBixolonLabelPrinter.setWidth(labelWidth);
							}
						}
					}).setNegativeButton("Cancel", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub
							
						}
					}).create();
		}
		dialog.show();
	}
	
	static void showSetLengthDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_length, null);
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_length).setView(view)
					.setPositiveButton("OK", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							EditText editText = (EditText) view.findViewById(R.id.editText1);
							String string1 = editText.getText().toString();
							
							editText = (EditText) view.findViewById(R.id.editText2);
							String string2 = editText.getText().toString();
							
							if (string1 != null && string1.length() > 0 && string2 != null && string2.length() > 0) {
								int labelLength = Integer.parseInt(editText.getText().toString());
								int gapLength = Integer.parseInt(editText.getText().toString());
								
								int mediaType = BixolonLabelPrinter.MEDIA_TYPE_GAP;
								RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
								switch (radioGroup.getCheckedRadioButtonId()) {
								case R.id.radio1:
									mediaType = BixolonLabelPrinter.MEDIA_TYPE_CONTINUOUS;
									break;
								case R.id.radio2:
									mediaType = BixolonLabelPrinter.MEDIA_TYPE_BLACK_MARK;
									break;
								}
								
								int offsetLength = 0;
								editText = (EditText) view.findViewById(R.id.editText3);
								String string = editText.getText().toString();
								if (string != null && string.length() > 0) {
									offsetLength = Integer.parseInt(string);
								}
								
								MainActivity.mBixolonLabelPrinter.setLength(labelLength, gapLength, mediaType, offsetLength);
							}
						}
					}).setNegativeButton("Cancel", new OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							// TODO Auto-generated method stub
							
						}
					}).create();
		}
		dialog.show();
	}
	
	static void showSetBufferModeDialog(AlertDialog dialog, Context context) {
		mmCheckedItem = 0;
		if (dialog == null) {
			dialog = new AlertDialog.Builder(context).setTitle("Set buffer mode")
						.setSingleChoiceItems(new String[] {"false", "true"}, 0, new OnClickListener() {
							
							public void onClick(DialogInterface arg0, int arg1) {
								mmCheckedItem = arg1;
							}
						}).setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								
								MainActivity.mBixolonLabelPrinter.setBufferMode(mmCheckedItem == 1);								
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();


		

	}
	static void showSetSpeedDialog(AlertDialog dialog, Context context) {
		mmCheckedItem = 0;
		if (dialog == null) {
			String[] items = {
					"2.5 ips",
					"3.0 ips",
					"4.0 ips",
					"5.0 ips",
					"6.0 ips",
					"7.0 ips",
					"8.0 ips"
			};
			dialog = new AlertDialog.Builder(context).setTitle("Set speed")
						.setSingleChoiceItems(items, 0, new OnClickListener() {
							
							public void onClick(DialogInterface arg0, int arg1) {
								mmCheckedItem = arg1;
								
							}
						}).setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								switch (mmCheckedItem) {
								case 0:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_25IPS);
									break;
								case 1:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_30IPS);
									break;
								case 2:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_40IPS);
									break;
								case 3:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_50IPS);
									break;
								case 4:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_60IPS);
									break;
								case 5:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_70IPS);
									break;
								case 6:
									MainActivity.mBixolonLabelPrinter.setSpeed(BixolonLabelPrinter.SPEED_80IPS);
									break;
								}
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showSetDensityDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_density, null);
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_density).setView(view)
						.setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								EditText editText = (EditText) view.findViewById(R.id.editText1);
								int density = Integer.parseInt(editText.getText().toString());
								MainActivity.mBixolonLabelPrinter.setDensity(density);
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showSetOrientationDialog(AlertDialog dialog, Context context) {
		mmCheckedItem = 0;
		if (dialog == null) {
			dialog = new AlertDialog.Builder(context).setTitle("Set orientation")
						.setSingleChoiceItems(new String[] {"Print from top to bottom (default)", "Print from bottom to top"}, 0, new OnClickListener() {
							
							public void onClick(DialogInterface arg0, int arg1) {
								mmCheckedItem = arg1;
								
							}
						}).setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								
								if (mmCheckedItem == 0) {
									MainActivity.mBixolonLabelPrinter.setOrientation(BixolonLabelPrinter.ORIENTATION_TOP_TO_BOTTOM);
								} else {
									MainActivity.mBixolonLabelPrinter.setOrientation(BixolonLabelPrinter.ORIENTATION_BOTTOM_TO_TOP);
								}
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showSetOffsetDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_offset, null);
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.set_offset).setView(view)
						.setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								EditText editText = (EditText) view.findViewById(R.id.editText1);
								int offset = Integer.parseInt(editText.getText().toString());
								MainActivity.mBixolonLabelPrinter.setOffset(offset);
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showCutterPositionSettingDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_set_offset, null);
			
			dialog = new AlertDialog.Builder(context).setTitle("Cutter position setting").setView(view)
						.setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								EditText editText = (EditText) view.findViewById(R.id.editText1);
								int position = Integer.parseInt(editText.getText().toString());
								MainActivity.mBixolonLabelPrinter.setCutterPosition(position);
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
	
	static void showAutoCutterDialog(AlertDialog dialog, Context context) {
		if (dialog == null) {
			LayoutInflater inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			View view = inflater.inflate(R.layout.dialog_set_auto_cutter, null);
			
			final EditText editText = (EditText) view.findViewById(R.id.editText1);
			
			final RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
			radioGroup.setOnCheckedChangeListener(new OnCheckedChangeListener() {
				
				public void onCheckedChanged(RadioGroup group, int checkedId) {
					editText.setEnabled(checkedId == R.id.radio0);
				}
			});
			
			dialog = new AlertDialog.Builder(context).setTitle(R.string.cutting_action).setView(view)
						.setPositiveButton("OK", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								
								int cuttingPeriod = Integer.parseInt(editText.getText().toString());
								
								MainActivity.mBixolonLabelPrinter.setAutoCutter(radioGroup.getCheckedRadioButtonId() == R.id.radio0, cuttingPeriod);
							}
						}).setNegativeButton("Cancel", new OnClickListener() {
							
							public void onClick(DialogInterface dialog, int which) {
								// TODO Auto-generated method stub
								
							}
						}).create();
		}
		dialog.show();
	}
}
