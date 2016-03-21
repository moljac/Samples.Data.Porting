using System.Collections.Generic;

/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file DeviceSelectActivity.java
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.mediabrowser.devicepicker
{

	using ListActivity = android.app.ListActivity;
	using Intent = android.content.Intent;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using LruCache = android.util.LruCache;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using android.widget;
	using com.samsung.android.sdk.mediacontrol;


	/// <summary>
	/// Activity enabling user to select a remote AllShare device for playback.
	/// </summary>
	public class DeviceSelectActivity : ListActivity, View.OnClickListener, SmcDeviceFinder.StatusListener, SmcDeviceFinder.DeviceListener
	{


		/// <summary>
		/// Current list of AllShare devices
		/// </summary>
		private IList<SmcDevice> mDevices = new List<SmcDevice>();

		/// <summary>
		/// Type of AllShare devices that should be presented to user
		/// </summary>
		private int mType;

		private SmcDeviceFinder mDeviceFinder;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			mType = Intent.getIntExtra("deviceType", SmcDevice.TYPE_IMAGEVIEWER);

			ContentView = R.layout.device_select;

			TextView noDevicesTextView = (TextView) findViewById(R.id.no_devices);
			noDevicesTextView.Visibility = View.GONE;

			if (FrameworkInstalled)
			{
				noDevicesTextView.Text = R.@string.device_picker_no_devices;
			}
			else
			{
				noDevicesTextView.Text = R.@string.device_picker_framework_not_installed;
				findViewById(R.id.refresh_button).Visibility = View.GONE;
			}

			ListView.EmptyView = noDevicesTextView;

			ListAdapter = new DevicesAdapter(this);

			findViewById(R.id.cancel_button).OnClickListener = this;
			findViewById(R.id.refresh_button).OnClickListener = this;

			SmcDeviceFinder df = new SmcDeviceFinder(this);
			df.StatusListener = this;
			df.start();
		}


		protected internal override void onPause()
		{
			base.onPause();
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			// Disconnect from AllShare Service

			if (mDeviceFinder != null)
			{
				mDeviceFinder.stop();
			}
		}


		public override void onClick(View v)
		{
			switch (v.Id)
			{
				case R.id.cancel_button:
					// Finish activity without returning result.
					finish();
					break;
				case R.id.refresh_button:
					// Rescan network for AllShare devices.
						if (mDeviceFinder != null)
						{
							mDeviceFinder.rescan();
						}
					break;
			}
		}

		protected internal override void onListItemClick(ListView l, View v, int position, long id)
		{
			SmcDevice device = mDevices[position];
			Intent ret = new Intent();
			ret.putExtra("deviceId", device.Id);
			ret.putExtra("deviceName", device.Name);
			ret.putExtra("deviceType", device.DeviceType);
			setResult(RESULT_OK, ret);
			finish();
		}


		public override void onStarted(SmcDeviceFinder deviceFinder, int error)
		{
			mDeviceFinder = deviceFinder;
			if (mType != 0)
			{
				mDeviceFinder.setDeviceListener(mType, this);
			}
			else
			{
				mDeviceFinder.setDeviceListener(SmcDevice.TYPE_IMAGEVIEWER, this);
				mDeviceFinder.setDeviceListener(SmcDevice.TYPE_AVPLAYER, this);
			}
			mDeviceFinder.rescan();
			refreshDevicesList();
		}

		public override void onStopped(SmcDeviceFinder deviceFinder)
		{
			// Remove listeners so that no references remain
			// and GC can collect the service and this activity.
			if (mDeviceFinder == deviceFinder)
			{
				if (mType != 0)
				{
					mDeviceFinder.setDeviceListener(mType, null);
				}
				else
				{
					mDeviceFinder.setDeviceListener(SmcDevice.TYPE_IMAGEVIEWER, null);
					mDeviceFinder.setDeviceListener(SmcDevice.TYPE_AVPLAYER, null);
				}
				mDeviceFinder.StatusListener = null;
				mDeviceFinder = null;
			}
		}


		///////////////////////////////////////////////////////////////////////////
		// This methods handle devices appearing and disappearing in network
		///////////////////////////////////////////////////////////////////////////

		public override void onDeviceAdded(SmcDeviceFinder deviceFinder, SmcDevice smcDevice)
		{
			refreshDevicesList();
		}

		public override void onDeviceRemoved(SmcDeviceFinder deviceFinder, SmcDevice SmcDevice, int error)
		{
			refreshDevicesList();
		}


		/// <summary>
		/// Refresh the list of displayed devices.
		/// </summary>
		private void refreshDevicesList()
		{
			mDevices = Devices;
			((BaseAdapter) ListAdapter).notifyDataSetChanged();
		}


		private IList<SmcDevice> Devices
		{
			get
			{
				if (mType == 0)
				{
					IList<SmcDevice> list = mDeviceFinder.getDeviceList(SmcDevice.TYPE_AVPLAYER);
					((List<SmcDevice>)list).AddRange(mDeviceFinder.getDeviceList(SmcDevice.TYPE_IMAGEVIEWER));
					return list;
				}
				else
				{
					return mDeviceFinder.getDeviceList(mType);
				}
			}
		}

		/// <summary>
		/// Adapter for displaying devices in a list.
		/// </summary>
		private class DevicesAdapter : BaseAdapter
		{
			private readonly DeviceSelectActivity outerInstance;

			/// <summary>
			/// Cache for device icons
			/// </summary>
			internal LruCache<Uri, Bitmap> mIconsCache;

			public DevicesAdapter(DeviceSelectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
				// We create a 1MB cache for icons, with item size being the bitmap size in bytes.
				mIconsCache = new LruCacheAnonymousInnerClassHelper(this);
			}

			private class LruCacheAnonymousInnerClassHelper : LruCache<Uri, Bitmap>
			{
				private readonly DevicesAdapter outerInstance;

				public LruCacheAnonymousInnerClassHelper(DevicesAdapter outerInstance) : base(1024 * 1024)
				{
					this.outerInstance = outerInstance;
				}

				protected internal override int sizeOf(Uri key, Bitmap value)
				{
					return value.ByteCount;
				}
			}

			public override int Count
			{
				get
				{
					return outerInstance.mDevices.Count;
				}
			}

			public override SmcDevice getItem(int position)
			{
				return outerInstance.mDevices[position];
			}

			public override long getItemId(int position)
			{
				return position;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					convertView = LayoutInflater.inflate(R.layout.device_item, null);
				}

				TextView label = (TextView) convertView.findViewById(R.id.deviceName);
				ImageView icon = (ImageView) convertView.findViewById(R.id.deviceIcon);

				SmcDevice device = outerInstance.mDevices[position];
				if (device is SmcImageViewer)
				{
					label.Text = "[ImageViewer] " + device.Name;
				}
				else if (device is SmcAvPlayer)
				{
					label.Text = "[AVPlayer] " + device.Name;
				}
				else if (device is SmcProvider)
				{
					label.Text = "[Provider] " + device.Name;
				}
				else
				{
					label.Text = device.Name;
				}
				Uri iconPath = device.IconUri;
				icon.Tag = iconPath;
				if (iconPath != null)
				{
					Bitmap b = mIconsCache.get(iconPath);
					if (b == null)
					{
						// Clear the image so we don't display stale icon.
						icon.ImageResource = R.drawable.ic_launcher;
						(new IconLoader(this, icon)).execute(iconPath);
					}
					else
					{
						icon.ImageBitmap = b;
					}
				}
				else
				{
					icon.ImageResource = R.drawable.ic_launcher;
				}

				return convertView;
			}

			private class IconLoader : AsyncTask<Uri, Void, Bitmap>
			{
				private readonly DeviceSelectActivity.DevicesAdapter outerInstance;

				internal readonly ImageView mImageView;

				internal IconLoader(DeviceSelectActivity.DevicesAdapter outerInstance, ImageView imageView)
				{
					this.outerInstance = outerInstance;
					mImageView = imageView;
				}

				protected internal override Bitmap doInBackground(params Uri[] @params)
				{
					System.IO.Stream @in = null;
					try
					{
						URL url = new URL(@params[0].ToString());
						URLConnection conn = url.openConnection();
						@in = conn.InputStream;
						Bitmap bitmap = BitmapFactory.decodeStream(@in);

						// Add the bitmap to cache.
						if (outerInstance.mIconsCache != null)
						{
							outerInstance.mIconsCache.put(@params[0], bitmap);
						}
						//return the bitmap only if target image view is still valid
						if (@params[0].Equals(mImageView.Tag))
						{
							return bitmap;
						}
						else
						{
							return null;
						}
					}
					catch (IOException)
					{
						// Failed to retrieve icon, ignore it
						return null;
					}
					finally
					{
						if (@in != null)
						{
							try
							{
							@in.Close();
							}
						catch (IOException)
						{
						}
						}
					}
				}

				protected internal override void onPostExecute(Bitmap result)
				{
					if (result != null && outerInstance.mIconsCache != null)
					{
						mImageView.ImageBitmap = result;
					}
				}
			}
		}

		/// <summary>
		/// Returns true if AllShare Framework is installed on device.
		/// </summary>
		private bool FrameworkInstalled
		{
			get
			{
				try
				{
					Smc smc = new Smc();
					smc.initialize(this);
					//if no error
					return true;
				}
				catch (SsdkUnsupportedException)
				{
					return false;
				}
			}
		}


	}

}