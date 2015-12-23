using System;
using System.Collections.Generic;

namespace com.bxl.bitmaptest
{


	using JposException = jpos.JposException;
	using POSPrinter = jpos.POSPrinter;
	using POSPrinterConst = jpos.POSPrinterConst;
	using JposEntry = jpos.config.JposEntry;
	using BluetoothAdapter = android.bluetooth.BluetoothAdapter;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using ContentResolver = android.content.ContentResolver;
	using Intent = android.content.Intent;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using MediaStore = android.provider.MediaStore;
	using Settings = android.provider.Settings;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using RadioGroup = android.widget.RadioGroup;
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;
	using SeekBar = android.widget.SeekBar;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using BXLConfigLoader = com.bxl.config.editor.BXLConfigLoader;

	public class MainActivity : ActionBarActivity, View.OnClickListener, AdapterView.OnItemClickListener, SeekBar.OnSeekBarChangeListener, RadioGroup.OnCheckedChangeListener
	{

		private const int REQUEST_CODE_BLUETOOTH = 1;
		private const int REQUEST_CODE_ACTION_PICK = 2;

		private const string DEVICE_ADDRESS_START = " (";
		private const string DEVICE_ADDRESS_END = ")";

		private readonly List<CharSequence> bondedDevices = new List<CharSequence>();
		private ArrayAdapter<CharSequence> arrayAdapter;

		private TextView pathTextView;
		private TextView progressTextView;
		private RadioGroup openRadioGroup;
		private Button openFromDeviceStorageButton;

		private BXLConfigLoader bxlConfigLoader;
		private POSPrinter posPrinter;
		private string logicalName;
		private int brightness = 50;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			pathTextView = (TextView) findViewById(R.id.textViewPath);
			progressTextView = (TextView) findViewById(R.id.textViewProgress);

			openRadioGroup = (RadioGroup) findViewById(R.id.radioGroupOpen);
			openRadioGroup.OnCheckedChangeListener = this;

			openFromDeviceStorageButton = (Button) findViewById(R.id.buttonOpenFromDeviceStorage);
			openFromDeviceStorageButton.OnClickListener = this;

			findViewById(R.id.buttonOpenPrinter).OnClickListener = this;
			findViewById(R.id.buttonPrint).OnClickListener = this;
			findViewById(R.id.buttonClosePrinter).OnClickListener = this;

			SeekBar seekBar = (SeekBar) findViewById(R.id.seekBarBrightness);
			seekBar.OnSeekBarChangeListener = this;

			setBondedDevices();

			arrayAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_single_choice, bondedDevices);
			ListView listView = (ListView) findViewById(R.id.listViewPairedDevices);
			listView.Adapter = arrayAdapter;

			listView.ChoiceMode = ListView.CHOICE_MODE_SINGLE;
			listView.OnItemClickListener = this;

			bxlConfigLoader = new BXLConfigLoader(this);
			try
			{
				bxlConfigLoader.openFile();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				bxlConfigLoader.newFile();
			}
			posPrinter = new POSPrinter(this);
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();

			try
			{
				posPrinter.close();
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.menu_main, menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			int id = item.ItemId;

			if (id == R.id.action_settings)
			{
				Intent intent = new Intent(Settings.ACTION_BLUETOOTH_SETTINGS);
				startActivityForResult(intent, REQUEST_CODE_BLUETOOTH);
				return true;
			}

			return base.onOptionsItemSelected(item);
		}

		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			switch (requestCode)
			{
			case REQUEST_CODE_BLUETOOTH:
				setBondedDevices();
				break;

			case REQUEST_CODE_ACTION_PICK:
				if (data != null)
				{
					Uri uri = data.Data;
					ContentResolver cr = ContentResolver;
					Cursor c = cr.query(uri, new string[] {MediaStore.Images.Media.DATA}, null, null, null);
					if (c == null || c.Count == 0)
					{
						return;
					}

					c.moveToFirst();
					int columnIndex = c.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
					string text = c.getString(columnIndex);
					c.close();

					pathTextView.Text = text;
				}
				break;
			}
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonOpenFromDeviceStorage:
				openFromDeviceStorage();
				break;

			case R.id.buttonOpenPrinter:
				openPrinter();
				break;

			case R.id.buttonPrint:
				print();
				break;

			case R.id.buttonClosePrinter:
				closePrinter();
				break;
			}
		}

		public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			string device = ((TextView) view).Text.ToString();

			logicalName = device.Substring(0, device.IndexOf(DEVICE_ADDRESS_START, StringComparison.Ordinal));

			string address = StringHelperClass.SubstringSpecial(device, device.IndexOf(DEVICE_ADDRESS_START, StringComparison.Ordinal) + DEVICE_ADDRESS_START.Length, device.IndexOf(DEVICE_ADDRESS_END, StringComparison.Ordinal));

			try
			{
				foreach (object entry in bxlConfigLoader.Entries)
				{
					JposEntry jposEntry = (JposEntry) entry;
					bxlConfigLoader.removeEntry(jposEntry.LogicalName);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			try
			{
				bxlConfigLoader.addEntry(logicalName, BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER, logicalName, BXLConfigLoader.DEVICE_BUS_BLUETOOTH, address);

				bxlConfigLoader.saveFile();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			progressTextView.Text = Convert.ToString(progress);
			brightness = progress;
		}

		public override void onStartTrackingTouch(SeekBar seekBar)
		{

		}

		public override void onStopTrackingTouch(SeekBar seekBar)
		{

		}

		public override void onCheckedChanged(RadioGroup group, int checkedId)
		{
			switch (checkedId)
			{
			case R.id.radioDeviceStorage:
				openFromDeviceStorageButton.Enabled = true;
				break;

			case R.id.radioProjectResources:
				openFromDeviceStorageButton.Enabled = false;
				break;
			}
		}

		private void setBondedDevices()
		{
			logicalName = null;
			bondedDevices.Clear();

			BluetoothAdapter bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
			ISet<BluetoothDevice> bondedDeviceSet = bluetoothAdapter.BondedDevices;

			foreach (BluetoothDevice device in bondedDeviceSet)
			{
				bondedDevices.Add(device.Name + DEVICE_ADDRESS_START + device.Address + DEVICE_ADDRESS_END);
			}

			if (arrayAdapter != null)
			{
				arrayAdapter.notifyDataSetChanged();
			}
		}

		private void openFromDeviceStorage()
		{
			string externalStorageState = Environment.ExternalStorageState;

			if (externalStorageState.Equals(Environment.MEDIA_MOUNTED))
			{
				Intent intent = new Intent(Intent.ACTION_PICK);
				intent.Type = MediaStore.Images.Media.CONTENT_TYPE;
				startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
			}
		}

		private void openPrinter()
		{
			try
			{
				posPrinter.open(logicalName);
				posPrinter.claim(0);
				posPrinter.DeviceEnabled = true;
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();

				try
				{
					posPrinter.close();
				}
				catch (JposException e1)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e1.ToString());
					Console.Write(e1.StackTrace);
				}
			}
		}

		private void closePrinter()
		{
			try
			{
				posPrinter.close();
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
		}

		private void print()
		{
			System.IO.Stream @is = null;
			try
			{
				ByteBuffer buffer = ByteBuffer.allocate(4);
				buffer.put((sbyte) POSPrinterConst.PTR_S_RECEIPT);
				buffer.put((sbyte) brightness);
				buffer.put((sbyte) 0x00);
				buffer.put((sbyte) 0x00);

				switch (openRadioGroup.CheckedRadioButtonId)
				{
				case R.id.radioDeviceStorage:
					posPrinter.printBitmap(buffer.getInt(0), pathTextView.Text.ToString(), posPrinter.RecLineWidth, POSPrinterConst.PTR_BM_LEFT);
					break;

				case R.id.radioProjectResources:
					@is = Resources.openRawResource(R.raw.project_resource1);
					Bitmap bitmap = BitmapFactory.decodeStream(@is);
					posPrinter.printBitmap(buffer.getInt(0), bitmap, posPrinter.RecLineWidth, POSPrinterConst.PTR_BM_LEFT);
					break;
				}
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
			finally
			{
				if (@is != null)
				{
					try
					{
						@is.Close();
					}
					catch (IOException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}
		}
	}

}