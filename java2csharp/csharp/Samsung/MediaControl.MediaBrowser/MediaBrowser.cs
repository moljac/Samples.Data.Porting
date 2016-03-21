using System.Collections.Generic;

namespace com.samsung.android.sdk.sample.mediabrowser
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ListView = android.widget.ListView;
	using SearchView = android.widget.SearchView;
	using Toast = android.widget.Toast;
	using com.samsung.android.sdk.mediacontrol;
	using DevicePicker = com.samsung.android.sdk.sample.mediabrowser.devicepicker.DevicePicker;


	public class MediaBrowser : Activity, SmcProvider.ResponseListener, AdapterView.OnItemClickListener, AdapterView.OnItemLongClickListener
	{

		public const int REQUEST_SIZE = 100;

		private DevicePicker mSourceDevicePicker;
		private DevicePicker mPlayerDevicePicker;

		private SmcProvider mProvider;
		private SmcDevice mPlayer;

		private ListView mListView;
		private ItemAdapter mItemAdapter;

		private Stack<SmcItem> mItemStack;
		private SmcItem mCurrentFolder;
		private SmcItem mItemToPlay;

		private Smc mSmcLib;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.main;
			mSourceDevicePicker = (DevicePicker) FragmentManager.findFragmentById(R.id.sourcePicker);
			mSourceDevicePicker.DeviceType = SmcDevice.TYPE_PROVIDER;
			mSourceDevicePicker.DeviceSelectedListener = mSourceDevicePickerListener;

			mPlayerDevicePicker = (DevicePicker) FragmentManager.findFragmentById(R.id.playerPicker);
			//mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_IMAGEVIEWER);
			mPlayerDevicePicker.DeviceSelectedListener = mPlayerDevicePickerListener;

			mListView = (ListView) findViewById(R.id.listView);
			mListView.OnItemClickListener = this;
			mItemAdapter = new ItemAdapter(this);
			mListView.Adapter = mItemAdapter;
			mListView.OnItemLongClickListener = this;
			mItemStack = new Stack<SmcItem>();

			mSmcLib = new Smc();
			try
			{
				mSmcLib.initialize(BaseContext);
			}
			catch (SsdkUnsupportedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace); //TODO Handle exceptions.
			}

		}


		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.menu, menu);
			MenuItem mi = menu.findItem(R.id.search);
			SearchView searchView = (SearchView) mi.ActionView;
			searchView.IconifiedByDefault = true;
			searchView.OnQueryTextListener = new OnQueryTextListenerAnonymousInnerClassHelper(this);
			mi.OnActionExpandListener = new OnActionExpandListenerAnonymousInnerClassHelper(this);
			return true;
		}

		private class OnQueryTextListenerAnonymousInnerClassHelper : SearchView.OnQueryTextListener
		{
			private readonly MediaBrowser outerInstance;

			public OnQueryTextListenerAnonymousInnerClassHelper(MediaBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool onQueryTextSubmit(string query)
			{
				outerInstance.search(query);
				return true;
			}

			public override bool onQueryTextChange(string newText)
			{
				return false;
			}
		}

		private class OnActionExpandListenerAnonymousInnerClassHelper : MenuItem.OnActionExpandListener
		{
			private readonly MediaBrowser outerInstance;

			public OnActionExpandListenerAnonymousInnerClassHelper(MediaBrowser outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool onMenuItemActionExpand(MenuItem item)
			{
				Toast.makeText(outerInstance, "Type * in search box to list all items", Toast.LENGTH_LONG).show();
				return true;
			}

			public override bool onMenuItemActionCollapse(MenuItem item)
			{
				outerInstance.browse(outerInstance.mCurrentFolder);
				return true;
			}
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			Intent intent;
			switch (item.ItemId)
			{
				case R.id.upload_audio:
					intent = (new Intent(Intent.ACTION_GET_CONTENT)).setType("audio/*");
					startActivityForResult(intent, 0);
					return true;
				case R.id.upload_image:
					intent = (new Intent(Intent.ACTION_PICK)).setType("image/*");
					startActivityForResult(intent, 0);
					return true;
				case R.id.upload_video:
					intent = (new Intent(Intent.ACTION_PICK)).setType("video/*");
					startActivityForResult(intent, 0);
					return true;
			}
			return base.onOptionsItemSelected(item);
		}

		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			if (resultCode == RESULT_OK && data != null)
			{
				data.setClass(this, typeof(Uploader));
				startActivity(data);
			}
			else if (resultCode == RESULT_CANCELED)
			{
				Toast.makeText(this, "Upload canceled", Toast.LENGTH_SHORT).show();
			}
		}

		/// <summary>
		/// This listener gets notified when source device is selected or deselected
		/// </summary>
		private DevicePicker.DevicePickerResult mSourceDevicePickerListener = new DevicePickerResultAnonymousInnerClassHelper();

		private class DevicePickerResultAnonymousInnerClassHelper : DevicePicker.DevicePickerResult
		{
			public DevicePickerResultAnonymousInnerClassHelper()
			{
			}

			public virtual void onDeviceSelected(SmcDevice device)
			{
				outerInstance.mProvider = (SmcProvider) device;
				//set listeners and browse the root folder
				outerInstance.mProvider.ResponseListener = outerInstance;
				outerInstance.mProvider.ResponseListener = outerInstance;
				outerInstance.browse(outerInstance.mProvider.RootFolder);
			}

			public virtual void onAllShareDisabled()
			{
				outerInstance.mProvider = null;
				//clear file list
				outerInstance.mItemAdapter.clear();
				findViewById(R.id.no_files).Visibility = View.VISIBLE;
			}
		}

		/// <summary>
		/// This listener gets notified when target device is selected or deselected
		/// </summary>
		private DevicePicker.DevicePickerResult mPlayerDevicePickerListener = new DevicePickerResultAnonymousInnerClassHelper2();

		private class DevicePickerResultAnonymousInnerClassHelper2 : DevicePicker.DevicePickerResult
		{
			public DevicePickerResultAnonymousInnerClassHelper2()
			{
			}

			public virtual void onDeviceSelected(SmcDevice device)
			{
				outerInstance.mPlayer = device;
				//if there was an item requested to play before, then play it
				if (outerInstance.mItemToPlay != null)
				{
					outerInstance.playItem(outerInstance.mItemToPlay);
					outerInstance.mItemToPlay = null;
				}
			}

			public virtual void onAllShareDisabled()
			{
				if (outerInstance.mPlayer is SmcImageViewer)
				{
					((SmcImageViewer) outerInstance.mPlayer).hide();
				}
				else if (outerInstance.mPlayer is SmcAvPlayer)
				{
					((SmcAvPlayer) outerInstance.mPlayer).stop();
				}
				outerInstance.mPlayer = null;
			}
		}

		public override void onBackPressed()
		{
			if (mItemStack.Count > 0)
			{
				browse(mItemStack.Pop());
				return;
			}
			if (mPlayer is SmcAvPlayer)
			{
				((SmcAvPlayer) mPlayer).stop();
			}
			else if (mPlayer is SmcImageViewer)
			{
				((SmcImageViewer) mPlayer).hide();
			}
			base.onBackPressed();
		}



		public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			SmcItem item = mItemAdapter.getItem(position);
			if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_FOLDER)
			{
				mItemStack.Push(mCurrentFolder);
				browse(item);
			}
			else
			{
				playItem(item);
			}
		}

		/// <summary>
		/// This method plays an <seealso cref="SmcItem"/>. If current player is not compatible with the type of item, then it will show
		/// device picker dialog;
		/// </summary>
		/// <param name="item"> an item to play </param>
		private void playItem(SmcItem item)
		{
			if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_IMAGE)
			{
				if (mPlayer is SmcImageViewer)
				{
					((SmcImageViewer) mPlayer).show(item);
				}
				else
				{
					Toast.makeText(this, "Please select ImageViewer", Toast.LENGTH_SHORT).show();
					//mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_IMAGEVIEWER);
					mPlayerDevicePicker.showPickerDialog();
					mItemToPlay = item;
				}
			}
			else if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_AUDIO || item.MediaType == SmcItem.MEDIA_TYPE_ITEM_VIDEO)
			{
				if (mPlayer is SmcAvPlayer)
				{
					((SmcAvPlayer) mPlayer).play(item, null);
				}
				else
				{
					Toast.makeText(this, "Please select AVPlayer", Toast.LENGTH_SHORT).show();
					//mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_AVPLAYER);
					mPlayerDevicePicker.showPickerDialog();
					mItemToPlay = item;
				}
			}
		}

		/// <summary>
		/// Lists contents of a directory.
		/// </summary>
		/// <param name="item"> directory to list </param>
		private void browse(SmcItem item)
		{
			if (mProvider != null)
			{
				mCurrentFolder = item;
				mItemAdapter.clear();
				mProvider.browse(item, 0, REQUEST_SIZE);
				findViewById(R.id.progress).Visibility = View.VISIBLE;
				findViewById(R.id.no_files).Visibility = View.GONE;
			}
		}

		/// <summary>
		/// Performs search in current provider.
		/// </summary>
		/// <param name="query"> </param>
		private void search(string query)
		{
			if (mProvider != null)
			{
				if (query.Equals("*"))
				{
					query = "";
				}
				mItemAdapter.clear();
				SmcProvider.SearchCriteria sc = new SmcProvider.SearchCriteria(query);
				mProvider.search(sc, 0, REQUEST_SIZE);
				findViewById(R.id.progress).Visibility = View.VISIBLE;
				findViewById(R.id.no_files).Visibility = View.GONE;
			}
		}

		/// <summary>
		/// Called when file listing is completed
		/// </summary>
		private void onBrowseComplete()
		{
			findViewById(R.id.progress).Visibility = View.GONE;
			findViewById(R.id.no_files).Visibility = mItemAdapter.Count == 0 ? View.VISIBLE : View.GONE;
		}


		public override bool onItemLongClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			SmcItem item = mItemAdapter.getItem(position);
			LinkedHashMap<string, string> props = new LinkedHashMap<string, string>();
			props.put(getString(R.@string.title), item.Title);
			if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_AUDIO)
			{
				props.put(getString(R.@string.artist), item.Artist);
				props.put(getString(R.@string.album), item.AlbumTitle);
				props.put(getString(R.@string.genre), item.Genre);
				props.put(getString(R.@string.bitrate), item.Bitrate + " B/s");
				props.put(getString(R.@string.duration), item.Duration + "s");
			}
			else if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_IMAGE)
			{
				props.put(getString(R.@string.resolution), item.Resolution);
				if (item.Location != null)
				{
					props.put(getString(R.@string.location), item.Location.ToString());
				}
			}
			else if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_VIDEO)
			{
				props.put(getString(R.@string.resolution), item.Resolution);
				props.put(getString(R.@string.bitrate), item.Bitrate + " B/s");
				props.put(getString(R.@string.duration), item.Duration + "s");
			// } else if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_FOLDER) { // Prevent
			}
			props.put(getString(R.@string.mime_type), item.MimeType);
			PropertiesDialog dialog = new PropertiesDialog();
			Bundle b = new Bundle();
			b.putSerializable("props", props);
			dialog.Arguments = b;
			dialog.show(FragmentManager, "props");
			return true;
		}

		public override void onBrowse(SmcProvider smcProvider, IList<SmcItem> items, int requestedStartIndex, int requestedCount, SmcItem requestedFolderItem, bool endOfItems, int error)
		{
			mItemAdapter.add(items);
			if (endOfItems)
			{
				onBrowseComplete();
			}
			else
			{
				//if the callback didn't return full list of files, make request for the rest of them
				mProvider.browse(requestedFolderItem, requestedStartIndex + items.Count, REQUEST_SIZE);
			}
		}

		public override void onSearch(SmcProvider smcProvider, IList<SmcItem> items, int requestedStartIndex, int requestedCount, SmcProvider.SearchCriteria searchCriteria, bool endOfItems, int error)
		{
			mItemAdapter.add(items);
			if (endOfItems)
			{
				onBrowseComplete();
			}
			else
			{
				//if the callback didn't return full list of files, make request for the rest of them
				mProvider.search(searchCriteria, requestedStartIndex + items.Count, REQUEST_SIZE);
			}
		}

		public override void onUpload(SmcProvider smcProvider, SmcItem Item, int error)
		{
			//To change body of implemented methods use File | Settings | File Templates.
		}

		public override void onUploadCancel(SmcProvider smcProvider, SmcItem Item, int error)
		{
			//To change body of implemented methods use File | Settings | File Templates.
		}
	}

}