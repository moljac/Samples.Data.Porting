package com.bxl.postest.settings;

import java.util.List;

import jpos.config.JposEntry;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.view.ContextMenu;
import android.view.ContextMenu.ContextMenuInfo;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView.AdapterContextMenuInfo;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.bxl.config.editor.BXLConfigLoader;
import com.bxl.postest.R;

public class SettingsActivity extends ActionBarActivity implements EntryDialogFragment.OnClickListener {
	
	private BXLConfigLoader bxlConfigLoader;
	
	private ArrayAdapter<String> arrayAdapter;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_settings);
		
		bxlConfigLoader = new BXLConfigLoader(this);
		
		arrayAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1);
		
		ListView listView = (ListView) findViewById(R.id.listView1);
		listView.setAdapter(arrayAdapter);
		registerForContextMenu(listView);
		
		try {
			bxlConfigLoader.openFile();
			List<?> entries = bxlConfigLoader.getEntries();
			for (Object entry : entries) {
				arrayAdapter.add(((JposEntry) entry).getLogicalName());
			}
		} catch (Exception e) {
			e.printStackTrace();
			bxlConfigLoader.newFile();
		}
	}
	
	@Override
	protected void onStop() {
		super.onStop();
		
		try {
			bxlConfigLoader.saveFile();
			Toast.makeText(this, "File saved", Toast.LENGTH_SHORT).show();
		} catch (Exception e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		}
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.settings, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.action_add_entry:
			EntryDialogFragment.showDialog(getSupportFragmentManager(), "Add entry", null);
			return true;
			
		default:
			return super.onOptionsItemSelected(item);
		}
	}
	
	@Override
	public void onCreateContextMenu(ContextMenu menu, View v,
			ContextMenuInfo menuInfo) {
		super.onCreateContextMenu(menu, v, menuInfo);
		MenuInflater inflater = getMenuInflater();
		inflater.inflate(R.menu.context_menu, menu);
	}
	
	@Override
	public boolean onContextItemSelected(MenuItem item) {
		AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.getMenuInfo();
		String logicalName = ((TextView) info.targetView).getText().toString();
		
		switch (item.getItemId()) {
		case R.id.context_modify_entry:
			int deviceCategory = bxlConfigLoader.getDeviceCategory(logicalName);
			String dc = null;
			switch (deviceCategory) {
			case BXLConfigLoader.DEVICE_CATEGORY_CASH_DRAWER:
				dc = getResources().getStringArray(R.array.device_categories)[0];
				break;
				
			case BXLConfigLoader.DEVICE_CATEGORY_MSR:
				dc = getResources().getStringArray(R.array.device_categories)[1];
				break;
				
			case BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER:
				dc = getResources().getStringArray(R.array.device_categories)[2];
				break;
				
			case BXLConfigLoader.DEVICE_CATEGORY_SMART_CARD_RW:
				dc = getResources().getStringArray(R.array.device_categories)[3];
				break;
			}
			
			int deviceBus = bxlConfigLoader.getDeviceBus(logicalName);
			String db = null;
			switch (deviceBus) {
			case BXLConfigLoader.DEVICE_BUS_BLUETOOTH:
				db = getResources().getStringArray(R.array.device_bus)[0];
				break;
				
			case BXLConfigLoader.DEVICE_BUS_ETHERNET:
				db = getResources().getStringArray(R.array.device_bus)[1];
				break;
				
			case BXLConfigLoader.DEVICE_BUS_USB:
				db = getResources().getStringArray(R.array.device_bus)[2];
				break;
				
			case BXLConfigLoader.DEVICE_BUS_WIFI:
				db = getResources().getStringArray(R.array.device_bus)[3];
				break;
				
			case BXLConfigLoader.DEVICE_BUS_WIFI_DIRECT:
				db = getResources().getStringArray(R.array.device_bus)[4];
				break;
			}
			
			EntryInfo entryInfo = new EntryInfo(logicalName,
					dc,
					bxlConfigLoader.getProductName(logicalName),
					db,
					bxlConfigLoader.getAddress(logicalName));
			EntryDialogFragment.showDialog(getSupportFragmentManager(), "Modify entry", entryInfo);
			return true;
			
		case R.id.context_remove_entry:
			if (bxlConfigLoader.removeEntry(logicalName)) {
				arrayAdapter.remove(logicalName);
			} else {
				Toast.makeText(this, "Remove failed", Toast.LENGTH_SHORT).show();
			}
			return true;
			
		default:
			return super.onContextItemSelected(item);
		}
	}

	@Override
	public void onClick(boolean isModified, EntryInfo entryInfo) {
		String[] deviceCategories = getResources().getStringArray(R.array.device_categories);
		int deviceCategory = 0;
		if (entryInfo.getDeviceCategory().equals(deviceCategories[0])) {
			deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_CASH_DRAWER;
		} else if (entryInfo.getDeviceCategory().equals(deviceCategories[1])) {
			deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_MSR;
		} else if (entryInfo.getDeviceCategory().equals(deviceCategories[2])) {
			deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER;
		} else if (entryInfo.getDeviceCategory().equals(deviceCategories[3])) {
			deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_SMART_CARD_RW;
		}
		
		String[] deviceBuses = getResources().getStringArray(R.array.device_bus);
		int deviceBus = 0;
		if (entryInfo.getDeviceBus().equals(deviceBuses[0])) {
			deviceBus = BXLConfigLoader.DEVICE_BUS_BLUETOOTH;
		} else if (entryInfo.getDeviceBus().equals(deviceBuses[1])) {
			deviceBus = BXLConfigLoader.DEVICE_BUS_ETHERNET;
		} else if (entryInfo.getDeviceBus().equals(deviceBuses[2])) {
			deviceBus = BXLConfigLoader.DEVICE_BUS_USB;
		} else if (entryInfo.getDeviceBus().equals(deviceBuses[3])) {
			deviceBus = BXLConfigLoader.DEVICE_BUS_WIFI;
		} else if (entryInfo.getDeviceBus().equals(deviceBuses[4])) {
			deviceBus = BXLConfigLoader.DEVICE_BUS_WIFI_DIRECT;
		}
		
		if (isModified) {
			bxlConfigLoader.modifyEntry(entryInfo.getLogicalName(), deviceBus, entryInfo.getAddress());
		} else {
			try {
				bxlConfigLoader.addEntry(entryInfo.getLogicalName(), deviceCategory,
						entryInfo.getProductName(), deviceBus, entryInfo.getAddress());
				arrayAdapter.add(entryInfo.getLogicalName());
			} catch (IllegalArgumentException e) {
				e.printStackTrace();
				Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
			}
		}
	}
}
