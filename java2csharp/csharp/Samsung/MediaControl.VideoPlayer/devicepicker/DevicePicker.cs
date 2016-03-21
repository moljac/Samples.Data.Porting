/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file DevicePicker.java
/// </summary>

namespace com.samsung.android.sdk.sample.videoplayer.devicepicker
{

	using Activity = android.app.Activity;
	using Fragment = android.app.Fragment;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using ImageView = android.widget.ImageView;
	using Smc = com.samsung.android.sdk.mediacontrol.Smc;
	using SmcDevice = com.samsung.android.sdk.mediacontrol.SmcDevice;
	using SmcDeviceFinder = com.samsung.android.sdk.mediacontrol.SmcDeviceFinder;

	/// <summary>
	/// AllShare icon fragment.
	/// <p/>
	/// Fragment that displays AllShare icon with number of available devices.
	/// <p/>
	/// If AllShare is connected (active), the icon is blue and clicking on it
	/// disconnects the AllShare device.
	/// <p/>
	/// If AllShare is not connected and there are any available devices,
	/// then clicking on the icon displays the device selection dialog and conveys
	/// the selected device to host activity.
	/// <p/>
	/// If there are no detected devices, clicking on icon rescans the network.
	/// 
	/// @version 4
	/// </summary>
	public class DevicePicker : Fragment, View.OnClickListener, SmcDeviceFinder.StatusListener, SmcDeviceFinder.DeviceListener
	{


		/// <summary>
		/// Callback interface for device selection events.
		/// </summary>
		public interface DevicePickerResult
		{

			/// <summary>
			/// User has selected an AllShare device
			/// </summary>
			/// <param name="device"> the selected device </param>
			void onDeviceSelected(SmcDevice device);

			/// <summary>
			/// User clicked to disable AllShare
			/// </summary>
			void onAllShareDisabled();
		}

		/// <summary>
		/// The type of device we are interested in
		/// </summary>
		private int mType = SmcDevice.TYPE_AVPLAYER;

		/// <summary>
		/// Listener to be notified of events
		/// </summary>
		private DevicePickerResult mPickerListener;

		/// <summary>
		/// Device finder instance
		/// </summary>
		private SmcDeviceFinder mDeviceFinder;

		/// <summary>
		/// The ImageView displaying AllShare icon
		/// </summary>
		private ImageView mIcon;

		/// <summary>
		/// Flag indicating if AllShare is currently active
		/// </summary>
		private bool mActive;

		private string mDeviceId;

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			// Set view, remember ImageView for icon and setup onclick listener.
			View v = inflater.inflate(R.layout.device_picker, container, false);
			mIcon = (ImageView) v.findViewById(R.id.devicePickerIcon);
			mIcon.OnClickListener = this;
			return v;
		}

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			/*
			if(savedInstanceState!=null){
			    mDeviceId = savedInstanceState.getString("deviceId");
			}
			*/
		}

		public override void onSaveInstanceState(Bundle outState)
		{
			base.onSaveInstanceState(outState);

			/*
			//if(mDeviceId!=null)
			outState.putString("deviceId", mDeviceId);
			*/
		}

		/// 
		/// <summary>
		/// Set the type of device.
		/// <p/>
		/// This has two effects:
		/// <ul>
		/// <li>Only devices of this type are counted when displaying number
		/// of devices on AllShare icon.
		/// <li>Only devices of this type are shown in displayed device select dialog.
		/// </ul>
		/// </summary>
		/// <param name="type"> The type of device to use </param>
		public virtual int DeviceType
		{
			set
			{
				mType = value;
			}
		}


		/// <summary>
		/// Sets the listener for event notifications.
		/// </summary>
		/// <param name="listener"> the new listener </param>
		public virtual DevicePickerResult DeviceSelectedListener
		{
			set
			{
				mPickerListener = value;
				restoreDevice();
			}
		}

		public override void onActivityCreated(Bundle savedInstanceState)
		{
			base.onActivityCreated(savedInstanceState);

			// The service provider needs to be created after device type is set
			// It could also be created in onStart or onResume, but we the sooner
			// we create it, the sooner we get devices.
			SmcDeviceFinder df = new SmcDeviceFinder(Activity);
			df.StatusListener = this;
			df.start();

		}


		private void restoreDevice()
		{
			if (mDeviceFinder != null && mDeviceId != null && mPickerListener != null)
			{
				SmcDevice d = mDeviceFinder.getDevice(mType, mDeviceId);
				if (d != null)
				{
					mPickerListener.onDeviceSelected(d);
					Active = true;
				}
			}
		}

		/// <summary>
		/// Changes the active state of picker.
		/// <p/>
		/// Picker should be active if AllShare is actively running,
		/// i.e. device is connected and used.
		/// </summary>
		/// <param name="newState"> new active state </param>
		public virtual bool Active
		{
			set
			{
				if (value == mActive)
				{
					// No change in state, do nothing
					return;
				}
				mActive = value;
				mIcon.ImageResource = value ? R.drawable.allshare_icons_active : R.drawable.allshare_icons_inactive;
				updateButtonCounter();
			}
		}

		public override void onClick(View v)
		{
			if (v != mIcon)
			{
				return;
			}

			if (mDeviceFinder != null)
			{

				int numDevices = mDeviceFinder.getDeviceList(mType).size();

				// If no devices found, try refreshing the list.
				if (numDevices == 0)
				{
					mDeviceFinder.rescan();
				}

				// If we are already active, disable allshare
				if (mActive)
				{
					Active = false;
					if (mPickerListener != null)
					{
						mPickerListener.onAllShareDisabled();
					}
					return;
				}
			}

			// Devices are available, and we are not connected
			// Ask user to select device
			showPickerDialog();
		}

		public override void onDetach()
		{
			if (mDeviceFinder != null)
			{
				mDeviceFinder.stop();
				mDeviceFinder = null;
			}
			base.onDetach();
		}

		///////////////////////////////////////////////////////////////////////////
		// These methods handle device finder start hide event.
		///////////////////////////////////////////////////////////////////////////

		public override void onStarted(SmcDeviceFinder deviceFinder, int error)
		{
			if (error == Smc.SUCCESS)
			{
				mDeviceFinder = deviceFinder;
				mDeviceFinder.setDeviceListener(mType, this);
				mDeviceFinder.rescan();
				updateButtonCounter();
				restoreDevice();
			}
		}

		public override void onStopped(SmcDeviceFinder deviceFinder)
		{
			if (mDeviceFinder == deviceFinder)
			{
				mDeviceFinder.setDeviceListener(mType, null);
				mDeviceFinder.StatusListener = null;
				mDeviceFinder = null;
			}
		}

		///////////////////////////////////////////////////////////////////////////
		// These methods handle devices appearing and disappearing in network.
		///////////////////////////////////////////////////////////////////////////

		public override void onDeviceAdded(SmcDeviceFinder deviceFinder, SmcDevice smcDevice)
		{
			// We aren't interested in individual devices, only in their number
			updateButtonCounter();
		}

		public override void onDeviceRemoved(SmcDeviceFinder deviceFinder, SmcDevice smcDevice, int error)
		{
			// We aren't interested in individual devices, only in their number
			updateButtonCounter();
			//if current device has been removed
			if (smcDevice.Id.Equals(mDeviceId))
			{
				Active = false;
				if (mPickerListener != null)
				{
					mPickerListener.onAllShareDisabled();
				}
			}
		}



		/// <summary>
		/// Methods that selects which icon to display, based on number of
		/// available devices in network.
		/// </summary>
		private void updateButtonCounter()
		{
			if (mDeviceFinder != null)
			{
				int numDevices = mDeviceFinder.getDeviceList(mType).size();

				mIcon.Drawable.Level = numDevices;
				if (numDevices == 0)
				{
					Active = false;
				}
			}
		}

		public virtual void showPickerDialog()
		{
			Intent intent = new Intent(Activity, typeof(DeviceSelectActivity));
			intent.putExtra("deviceType", mType);
			startActivityForResult(intent, 0);
		}

		/// <summary>
		/// Callback when user has selected device in device select activity.
		/// </summary>
		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			if (resultCode == Activity.RESULT_OK)
			{
				mDeviceId = data.getStringExtra("deviceId");
				int type = data.getIntExtra("deviceType", -1);

				if (mDeviceFinder != null && mPickerListener != null)
				{
					SmcDevice d = mDeviceFinder.getDevice(type, mDeviceId);
					if (d != null)
					{
						mPickerListener.onDeviceSelected(d);
						Active = true;
					}
				}
			}
		}
	}

}