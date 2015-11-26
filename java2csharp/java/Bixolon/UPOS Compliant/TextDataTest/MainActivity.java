package com.bxl.textdatatest;

import java.util.ArrayList;
import java.util.Set;

import jpos.JposException;
import jpos.POSPrinter;
import jpos.POSPrinterConst;
import jpos.config.JposEntry;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.Intent;
import android.os.Bundle;
import android.provider.Settings;
import android.support.v7.app.ActionBarActivity;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.bxl.config.editor.BXLConfigLoader;

public class MainActivity extends ActionBarActivity
		implements OnItemClickListener, OnClickListener {
	
	private static final int REQUEST_CODE_BLUETOOTH = 1;
	
	private static final String DEVICE_ADDRESS_START = " (";
	private static final String DEVICE_ADDRESS_END = ")";
	
	private EditText dataEditText;
	private Spinner escapeSequencesSpinner;

	private final ArrayList<CharSequence> bondedDevices = new ArrayList<>();
	private ArrayAdapter<CharSequence> arrayAdapter;
	
	private BXLConfigLoader bxlConfigLoader;
	private POSPrinter posPrinter;
	private String logicalName;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		setBondedDevices();
		
		arrayAdapter = new ArrayAdapter<>(this,
				android.R.layout.simple_list_item_single_choice, bondedDevices);
		ListView listView = (ListView) findViewById(R.id.listViewPairedDevices);
		listView.setAdapter(arrayAdapter);

		listView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
		listView.setOnItemClickListener(this);
		
		dataEditText = (EditText) findViewById(R.id.editTextData);
		dataEditText.setSelection(dataEditText.getText().length());
		
		escapeSequencesSpinner = (Spinner) findViewById(R.id.spinnerEscapeSequences);
		
		findViewById(R.id.buttonAdd).setOnClickListener(this);
		findViewById(R.id.buttonPrint).setOnClickListener(this);

		bxlConfigLoader = new BXLConfigLoader(this);
		try {
			bxlConfigLoader.openFile();
		} catch (Exception e) {
			e.printStackTrace();
			bxlConfigLoader.newFile();
		}
		posPrinter = new POSPrinter(this);
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();

		try {
			posPrinter.close();
		} catch (JposException e) {
			e.printStackTrace();
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			Intent intent = new Intent(Settings.ACTION_BLUETOOTH_SETTINGS);
			startActivityForResult(intent, REQUEST_CODE_BLUETOOTH);
			return true;
		}
		return super.onOptionsItemSelected(item);
	}
	
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_CODE_BLUETOOTH) {
			setBondedDevices();
		}
	}
	
	@Override
	public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
		String device = ((TextView) view).getText().toString();

		logicalName = device.substring(0, device.indexOf(DEVICE_ADDRESS_START));

		String address = device.substring(device.indexOf(DEVICE_ADDRESS_START)
				+ DEVICE_ADDRESS_START.length(),
				device.indexOf(DEVICE_ADDRESS_END));

		try {
			for (Object entry : bxlConfigLoader.getEntries()) {
				JposEntry jposEntry = (JposEntry) entry;
				bxlConfigLoader.removeEntry(jposEntry.getLogicalName());
			}
		} catch (Exception e) {
			e.printStackTrace();
		}

		try {
			bxlConfigLoader.addEntry(logicalName,
					BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER,
					logicalName,
					BXLConfigLoader.DEVICE_BUS_BLUETOOTH, address);
			
			bxlConfigLoader.saveFile();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}
	
	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonAdd:
			String text = EscapeSequence.getString(escapeSequencesSpinner.getSelectedItemPosition());
			dataEditText.getText().insert(dataEditText.getSelectionStart(), text);
			break;
			
		case R.id.buttonPrint:
			print();
			break;
		}
	}
	
	private void setBondedDevices() {
		logicalName = null;
		bondedDevices.clear();

		BluetoothAdapter bluetoothAdapter = BluetoothAdapter
				.getDefaultAdapter();
		Set<BluetoothDevice> bondedDeviceSet = bluetoothAdapter
				.getBondedDevices();

		for (BluetoothDevice device : bondedDeviceSet) {
			bondedDevices.add(device.getName() + DEVICE_ADDRESS_START
					+ device.getAddress() + DEVICE_ADDRESS_END);
		}

		if (arrayAdapter != null) {
			arrayAdapter.notifyDataSetChanged();
		}
	}
	
	private void print() {
		String data = dataEditText.getText().toString();
		
		try {
			posPrinter.open(logicalName);
			posPrinter.claim(0);
			posPrinter.setDeviceEnabled(true);
			posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, data);
		} catch (JposException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		} finally {
			try {
				posPrinter.close();
			} catch (JposException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
}
