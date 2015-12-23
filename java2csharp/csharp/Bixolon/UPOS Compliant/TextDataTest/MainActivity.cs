using System;
using System.Collections.Generic;

namespace com.bxl.textdatatest
{


	using JposException = jpos.JposException;
	using POSPrinter = jpos.POSPrinter;
	using POSPrinterConst = jpos.POSPrinterConst;
	using JposEntry = jpos.config.JposEntry;
	using BluetoothAdapter = android.bluetooth.BluetoothAdapter;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Settings = android.provider.Settings;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using EditText = android.widget.EditText;
	using ListView = android.widget.ListView;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using BXLConfigLoader = com.bxl.config.editor.BXLConfigLoader;

	public class MainActivity : ActionBarActivity, AdapterView.OnItemClickListener, View.OnClickListener
	{

		private const int REQUEST_CODE_BLUETOOTH = 1;

		private const string DEVICE_ADDRESS_START = " (";
		private const string DEVICE_ADDRESS_END = ")";

		private EditText dataEditText;
		private Spinner escapeSequencesSpinner;

		private readonly List<CharSequence> bondedDevices = new List<CharSequence>();
		private ArrayAdapter<CharSequence> arrayAdapter;

		private BXLConfigLoader bxlConfigLoader;
		private POSPrinter posPrinter;
		private string logicalName;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			setBondedDevices();

			arrayAdapter = new ArrayAdapter<>(this, android.R.layout.simple_list_item_single_choice, bondedDevices);
			ListView listView = (ListView) findViewById(R.id.listViewPairedDevices);
			listView.Adapter = arrayAdapter;

			listView.ChoiceMode = ListView.CHOICE_MODE_SINGLE;
			listView.OnItemClickListener = this;

			dataEditText = (EditText) findViewById(R.id.editTextData);
			dataEditText.Selection = dataEditText.Text.length();

			escapeSequencesSpinner = (Spinner) findViewById(R.id.spinnerEscapeSequences);

			findViewById(R.id.buttonAdd).OnClickListener = this;
			findViewById(R.id.buttonPrint).OnClickListener = this;

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
			MenuInflater.inflate(R.menu.main, menu);
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
			if (requestCode == REQUEST_CODE_BLUETOOTH)
			{
				setBondedDevices();
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

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonAdd:
				string text = EscapeSequence.getString(escapeSequencesSpinner.SelectedItemPosition);
				dataEditText.Text.insert(dataEditText.SelectionStart, text);
				break;

			case R.id.buttonPrint:
				print();
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

		private void print()
		{
			string data = dataEditText.Text.ToString();

			try
			{
				posPrinter.open(logicalName);
				posPrinter.claim(0);
				posPrinter.DeviceEnabled = true;
				posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, data);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
			finally
			{
				try
				{
					posPrinter.close();
				}
				catch (JposException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}
	}

}