using System;
using System.Collections.Generic;

namespace com.bxl.postest.settings
{

	using JposEntry = jpos.config.JposEntry;
	using Bundle = android.os.Bundle;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using ContextMenu = android.view.ContextMenu;
	using ContextMenuInfo = android.view.ContextMenu.ContextMenuInfo;
	using Menu = android.view.Menu;
	using MenuInflater = android.view.MenuInflater;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using AdapterContextMenuInfo = android.widget.AdapterView.AdapterContextMenuInfo;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using BXLConfigLoader = com.bxl.config.editor.BXLConfigLoader;

	public class SettingsActivity : ActionBarActivity, EntryDialogFragment.OnClickListener
	{

		private BXLConfigLoader bxlConfigLoader;

		private ArrayAdapter<string> arrayAdapter;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_settings;

			bxlConfigLoader = new BXLConfigLoader(this);

			arrayAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1);

			ListView listView = (ListView) findViewById(R.id.listView1);
			listView.Adapter = arrayAdapter;
			registerForContextMenu(listView);

			try
			{
				bxlConfigLoader.openFile();
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.List<?> entries = bxlConfigLoader.getEntries();
				IList<?> entries = bxlConfigLoader.Entries;
				foreach (object entry in entries)
				{
					arrayAdapter.add(((JposEntry) entry).LogicalName);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				bxlConfigLoader.newFile();
			}
		}

		protected internal override void onStop()
		{
			base.onStop();

			try
			{
				bxlConfigLoader.saveFile();
				Toast.makeText(this, "File saved", Toast.LENGTH_SHORT).show();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.settings, menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			switch (item.ItemId)
			{
			case R.id.action_add_entry:
				EntryDialogFragment.showDialog(SupportFragmentManager, "Add entry", null);
				return true;

			default:
				return base.onOptionsItemSelected(item);
			}
		}

		public override void onCreateContextMenu(ContextMenu menu, View v, ContextMenu.ContextMenuInfo menuInfo)
		{
			base.onCreateContextMenu(menu, v, menuInfo);
			MenuInflater inflater = MenuInflater;
			inflater.inflate(R.menu.context_menu, menu);
		}

		public override bool onContextItemSelected(MenuItem item)
		{
			AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.MenuInfo;
			string logicalName = ((TextView) info.targetView).Text.ToString();

			switch (item.ItemId)
			{
			case R.id.context_modify_entry:
				int deviceCategory = bxlConfigLoader.getDeviceCategory(logicalName);
				string dc = null;
				switch (deviceCategory)
				{
				case BXLConfigLoader.DEVICE_CATEGORY_CASH_DRAWER:
					dc = Resources.getStringArray(R.array.device_categories)[0];
					break;

				case BXLConfigLoader.DEVICE_CATEGORY_MSR:
					dc = Resources.getStringArray(R.array.device_categories)[1];
					break;

				case BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER:
					dc = Resources.getStringArray(R.array.device_categories)[2];
					break;

				case BXLConfigLoader.DEVICE_CATEGORY_SMART_CARD_RW:
					dc = Resources.getStringArray(R.array.device_categories)[3];
					break;
				}

				int deviceBus = bxlConfigLoader.getDeviceBus(logicalName);
				string db = null;
				switch (deviceBus)
				{
				case BXLConfigLoader.DEVICE_BUS_BLUETOOTH:
					db = Resources.getStringArray(R.array.device_bus)[0];
					break;

				case BXLConfigLoader.DEVICE_BUS_ETHERNET:
					db = Resources.getStringArray(R.array.device_bus)[1];
					break;

				case BXLConfigLoader.DEVICE_BUS_USB:
					db = Resources.getStringArray(R.array.device_bus)[2];
					break;

				case BXLConfigLoader.DEVICE_BUS_WIFI:
					db = Resources.getStringArray(R.array.device_bus)[3];
					break;

				case BXLConfigLoader.DEVICE_BUS_WIFI_DIRECT:
					db = Resources.getStringArray(R.array.device_bus)[4];
					break;
				}

				EntryInfo entryInfo = new EntryInfo(logicalName, dc, bxlConfigLoader.getProductName(logicalName), db, bxlConfigLoader.getAddress(logicalName));
				EntryDialogFragment.showDialog(SupportFragmentManager, "Modify entry", entryInfo);
				return true;

			case R.id.context_remove_entry:
				if (bxlConfigLoader.removeEntry(logicalName))
				{
					arrayAdapter.remove(logicalName);
				}
				else
				{
					Toast.makeText(this, "Remove failed", Toast.LENGTH_SHORT).show();
				}
				return true;

			default:
				return base.onContextItemSelected(item);
			}
		}

		public virtual void onClick(bool isModified, EntryInfo entryInfo)
		{
			string[] deviceCategories = Resources.getStringArray(R.array.device_categories);
			int deviceCategory = 0;
			if (entryInfo.DeviceCategory.Equals(deviceCategories[0]))
			{
				deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_CASH_DRAWER;
			}
			else if (entryInfo.DeviceCategory.Equals(deviceCategories[1]))
			{
				deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_MSR;
			}
			else if (entryInfo.DeviceCategory.Equals(deviceCategories[2]))
			{
				deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_POS_PRINTER;
			}
			else if (entryInfo.DeviceCategory.Equals(deviceCategories[3]))
			{
				deviceCategory = BXLConfigLoader.DEVICE_CATEGORY_SMART_CARD_RW;
			}

			string[] deviceBuses = Resources.getStringArray(R.array.device_bus);
			int deviceBus = 0;
			if (entryInfo.DeviceBus.Equals(deviceBuses[0]))
			{
				deviceBus = BXLConfigLoader.DEVICE_BUS_BLUETOOTH;
			}
			else if (entryInfo.DeviceBus.Equals(deviceBuses[1]))
			{
				deviceBus = BXLConfigLoader.DEVICE_BUS_ETHERNET;
			}
			else if (entryInfo.DeviceBus.Equals(deviceBuses[2]))
			{
				deviceBus = BXLConfigLoader.DEVICE_BUS_USB;
			}
			else if (entryInfo.DeviceBus.Equals(deviceBuses[3]))
			{
				deviceBus = BXLConfigLoader.DEVICE_BUS_WIFI;
			}
			else if (entryInfo.DeviceBus.Equals(deviceBuses[4]))
			{
				deviceBus = BXLConfigLoader.DEVICE_BUS_WIFI_DIRECT;
			}

			if (isModified)
			{
				bxlConfigLoader.modifyEntry(entryInfo.LogicalName, deviceBus, entryInfo.Address);
			}
			else
			{
				try
				{
					bxlConfigLoader.addEntry(entryInfo.LogicalName, deviceCategory, entryInfo.ProductName, deviceBus, entryInfo.Address);
					arrayAdapter.add(entryInfo.LogicalName);
				}
				catch (System.ArgumentException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
				}
			}
		}
	}

}