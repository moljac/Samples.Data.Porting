using System.Collections.Generic;

/*
 * Copyright (C) 2009 The Android Open Source Project
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace com.zj.printdemo
{

	using BluetoothService = com.zj.btsdk.BluetoothService;

	using Activity = android.app.Activity;
	using BluetoothAdapter = android.bluetooth.BluetoothAdapter;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using Window = android.view.Window;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;

	/// <summary>
	/// This Activity appears as a dialog. It lists any paired devices and
	/// devices detected in the area after discovery. When a device is chosen
	/// by the user, the MAC address of the device is sent back to the parent
	/// Activity in the result Intent.
	/// </summary>
	public class DeviceListActivity : Activity
	{
		// Return Intent extra
		public static string EXTRA_DEVICE_ADDRESS = "device_address";

		// Member fields
		internal BluetoothService mService = null;
		private ArrayAdapter<string> mPairedDevicesArrayAdapter;
		private ArrayAdapter<string> mNewDevicesArrayAdapter;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			// Setup the window
			requestWindowFeature(Window.FEATURE_INDETERMINATE_PROGRESS);
			ContentView = R.layout.device_list; //��ʾ�б����

			// Set result CANCELED incase the user backs out
			Result = Activity.RESULT_CANCELED;

			// Initialize the button to perform device discovery
			Button scanButton = (Button) findViewById(R.id.button_scan);
			scanButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			// Initialize array adapters. One for already paired devices and
			// one for newly discovered devices
			mPairedDevicesArrayAdapter = new ArrayAdapter<string>(this, R.layout.device_name);
			mNewDevicesArrayAdapter = new ArrayAdapter<string>(this, R.layout.device_name);

			// Find and set up the ListView for paired devices
			ListView pairedListView = (ListView) findViewById(R.id.paired_devices);
			pairedListView.Adapter = mPairedDevicesArrayAdapter;
			pairedListView.OnItemClickListener = mDeviceClickListener;

			// Find and set up the ListView for newly discovered devices
			ListView newDevicesListView = (ListView) findViewById(R.id.new_devices);
			newDevicesListView.Adapter = mNewDevicesArrayAdapter;
			newDevicesListView.OnItemClickListener = mDeviceClickListener;

			// Register for broadcasts when a device is discovered
			IntentFilter filter = new IntentFilter(BluetoothDevice.ACTION_FOUND);
			this.registerReceiver(mReceiver, filter);

			// Register for broadcasts when discovery has finished
			filter = new IntentFilter(BluetoothAdapter.ACTION_DISCOVERY_FINISHED);
			this.registerReceiver(mReceiver, filter);

			mService = new BluetoothService(this, null);

			// Get a set of currently paired devices
			ISet<BluetoothDevice> pairedDevices = mService.PairedDev;

			// If there are paired devices, add each one to the ArrayAdapter
			if (pairedDevices.Count > 0)
			{
				findViewById(R.id.title_paired_devices).Visibility = View.VISIBLE;
				foreach (BluetoothDevice device in pairedDevices)
				{
					mPairedDevicesArrayAdapter.add(device.Name + "\n" + device.Address);
				}
			}
			else
			{
				string noDevices = Resources.getText(R.@string.none_paired).ToString();
				mPairedDevicesArrayAdapter.add(noDevices);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly DeviceListActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DeviceListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.doDiscovery();
				v.Visibility = View.GONE;
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			if (mService != null)
			{
				mService.cancelDiscovery();
			}
			mService = null;
			this.unregisterReceiver(mReceiver);
		}

		/// <summary>
		/// Start device discover with the BluetoothAdapter
		/// </summary>
		private void doDiscovery()
		{

			// Indicate scanning in the title
			ProgressBarIndeterminateVisibility = true;
			Title = R.@string.scanning;

			// Turn on sub-title for new devices
			findViewById(R.id.title_new_devices).Visibility = View.VISIBLE;

			// If we're already discovering, stop it
			if (mService.Discovering)
			{
				mService.cancelDiscovery();
			}

			// Request discover from BluetoothAdapter
			mService.startDiscovery();
		}

		// The on-click listener for all devices in the ListViews
		private AdapterView.OnItemClickListener mDeviceClickListener = new OnItemClickListenerAnonymousInnerClassHelper();

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			public OnItemClickListenerAnonymousInnerClassHelper()
			{
			}

			public virtual void onItemClick<T1>(AdapterView<T1> av, View v, int arg2, long arg3)
			{
				// Cancel discovery because it's costly and we're about to connect
				outerInstance.mService.cancelDiscovery();

				// Get the device MAC address, which is the last 17 chars in the View
				string info = ((TextView) v).Text.ToString();
				string address = info.Substring(info.Length - 17);

				// Create the result Intent and include the MAC address
				Intent intent = new Intent();
				intent.putExtra(EXTRA_DEVICE_ADDRESS, address);
				Log.d("���ӵ�ַ", address);

				// Set result and finish this Activity
				setResult(Activity.RESULT_OK, intent);
				finish();
			}
		}

		// The BroadcastReceiver that listens for discovered devices and
		// changes the title when discovery is finished
		private readonly BroadcastReceiver mReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}

			public override void onReceive(Context context, Intent intent)
			{
				string action = intent.Action;

				// When discovery finds a device
				if (BluetoothDevice.ACTION_FOUND.Equals(action))
				{
					// Get the BluetoothDevice object from the Intent
					BluetoothDevice device = intent.getParcelableExtra(BluetoothDevice.EXTRA_DEVICE);
					// If it's already paired, skip it, because it's been listed already
					if (device.BondState != BluetoothDevice.BOND_BONDED)
					{
						outerInstance.mNewDevicesArrayAdapter.add(device.Name + "\n" + device.Address);
					}
				// When discovery is finished, change the Activity title
				}
				else if (BluetoothAdapter.ACTION_DISCOVERY_FINISHED.Equals(action))
				{
					ProgressBarIndeterminateVisibility = false;
					Title = R.@string.select_device;
					if (outerInstance.mNewDevicesArrayAdapter.Count == 0)
					{
						string noDevices = Resources.getText(R.@string.none_found).ToString();
						outerInstance.mNewDevicesArrayAdapter.add(noDevices);
					}
				}
			}
		}

	}

}