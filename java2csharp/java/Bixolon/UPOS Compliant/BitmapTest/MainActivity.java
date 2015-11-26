package com.bxl.bitmaptest;

import java.io.IOException;
import java.io.InputStream;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Set;

import jpos.JposException;
import jpos.POSPrinter;
import jpos.POSPrinterConst;
import jpos.config.JposEntry;
import android.bluetooth.BluetoothAdapter;
import android.bluetooth.BluetoothDevice;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.provider.Settings;
import android.support.v7.app.ActionBarActivity;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;
import android.widget.SeekBar;
import android.widget.TextView;
import android.widget.Toast;

import com.bxl.config.editor.BXLConfigLoader;

public class MainActivity extends ActionBarActivity implements
		View.OnClickListener, AdapterView.OnItemClickListener,
		SeekBar.OnSeekBarChangeListener, OnCheckedChangeListener {

	private static final int REQUEST_CODE_BLUETOOTH = 1;
	private static final int REQUEST_CODE_ACTION_PICK = 2;

	private static final String DEVICE_ADDRESS_START = " (";
	private static final String DEVICE_ADDRESS_END = ")";

	private final ArrayList<CharSequence> bondedDevices = new ArrayList<>();
	private ArrayAdapter<CharSequence> arrayAdapter;

	private TextView pathTextView;
	private TextView progressTextView;
	private RadioGroup openRadioGroup;
	private Button openFromDeviceStorageButton;
	
	private BXLConfigLoader bxlConfigLoader;
	private POSPrinter posPrinter;
	private String logicalName;
	private int brightness = 50;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		pathTextView = (TextView) findViewById(R.id.textViewPath);
		progressTextView = (TextView) findViewById(R.id.textViewProgress);
		
		openRadioGroup = (RadioGroup) findViewById(R.id.radioGroupOpen);
		openRadioGroup.setOnCheckedChangeListener(this);

		openFromDeviceStorageButton = (Button) findViewById(R.id.buttonOpenFromDeviceStorage);
		openFromDeviceStorageButton.setOnClickListener(this);
		
		findViewById(R.id.buttonOpenPrinter).setOnClickListener(this);
		findViewById(R.id.buttonPrint).setOnClickListener(this);
		findViewById(R.id.buttonClosePrinter).setOnClickListener(this);

		SeekBar seekBar = (SeekBar) findViewById(R.id.seekBarBrightness);
		seekBar.setOnSeekBarChangeListener(this);

		setBondedDevices();

		arrayAdapter = new ArrayAdapter<>(this,
				android.R.layout.simple_list_item_single_choice, bondedDevices);
		ListView listView = (ListView) findViewById(R.id.listViewPairedDevices);
		listView.setAdapter(arrayAdapter);

		listView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
		listView.setOnItemClickListener(this);

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
		getMenuInflater().inflate(R.menu.menu_main, menu);
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
		switch (requestCode) {
		case REQUEST_CODE_BLUETOOTH:
			setBondedDevices();
			break;

		case REQUEST_CODE_ACTION_PICK:
			if (data != null) {
				Uri uri = data.getData();
				ContentResolver cr = getContentResolver();
				Cursor c = cr.query(uri,
						new String[] { MediaStore.Images.Media.DATA }, null,
						null, null);
				if (c == null || c.getCount() == 0) {
					return;
				}

				c.moveToFirst();
				int columnIndex = c
						.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
				String text = c.getString(columnIndex);
				c.close();

				pathTextView.setText(text);
			}
			break;
		}
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
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

	@Override
	public void onItemClick(AdapterView<?> parent, View view, int position,
			long id) {
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
					BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER, logicalName,
					BXLConfigLoader.DEVICE_BUS_BLUETOOTH, address);
			
			bxlConfigLoader.saveFile();
		} catch (Exception e) {
			e.printStackTrace();
		}
	}

	@Override
	public void onProgressChanged(SeekBar seekBar, int progress,
			boolean fromUser) {
		progressTextView.setText(Integer.toString(progress));
		brightness = progress;
	}

	@Override
	public void onStartTrackingTouch(SeekBar seekBar) {

	}

	@Override
	public void onStopTrackingTouch(SeekBar seekBar) {

	}
	
	@Override
	public void onCheckedChanged(RadioGroup group, int checkedId) {
		switch (checkedId) {
		case R.id.radioDeviceStorage:
			openFromDeviceStorageButton.setEnabled(true);
			break;
			
		case R.id.radioProjectResources:
			openFromDeviceStorageButton.setEnabled(false);
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
	
	private void openFromDeviceStorage() {
		String externalStorageState = Environment.getExternalStorageState();
		
		if (externalStorageState.equals(Environment.MEDIA_MOUNTED)) {
			Intent intent = new Intent(Intent.ACTION_PICK);
			intent.setType(android.provider.MediaStore.Images.Media.CONTENT_TYPE);
			startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
		}
	}
	
	private void openPrinter() {
		try {
			posPrinter.open(logicalName);
			posPrinter.claim(0);
			posPrinter.setDeviceEnabled(true);
		} catch (JposException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
			
			try {
				posPrinter.close();
			} catch (JposException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
		}
	}
	
	private void closePrinter() {
		try {
			posPrinter.close();
		} catch (JposException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
	}
	
	private void print() {
		InputStream is = null;
		try {
			ByteBuffer buffer = ByteBuffer.allocate(4);
			buffer.put((byte) POSPrinterConst.PTR_S_RECEIPT);
			buffer.put((byte) brightness);
			buffer.put((byte) 0x00);
			buffer.put((byte) 0x00);

			switch (openRadioGroup.getCheckedRadioButtonId()) {
			case R.id.radioDeviceStorage:
				posPrinter.printBitmap(buffer.getInt(0), pathTextView.getText().toString(),
						posPrinter.getRecLineWidth(), POSPrinterConst.PTR_BM_LEFT);
				break;
				
			case R.id.radioProjectResources:
				is = getResources().openRawResource(R.raw.project_resource1);
				Bitmap bitmap = BitmapFactory.decodeStream(is);
				posPrinter.printBitmap(buffer.getInt(0), bitmap,
						posPrinter.getRecLineWidth(), POSPrinterConst.PTR_BM_LEFT);
				break;
			}
		} catch (JposException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		} finally {
			if (is != null) {
				try {
					is.close();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}
	}
}
