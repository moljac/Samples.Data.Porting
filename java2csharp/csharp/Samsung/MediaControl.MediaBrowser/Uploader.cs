using System.Collections.Generic;

namespace com.samsung.android.sdk.sample.mediabrowser
{

	using Activity = android.app.Activity;
	using Cursor = android.database.Cursor;
	using Bundle = android.os.Bundle;
	using MediaStore = android.provider.MediaStore;
	using View = android.view.View;
	using MimeTypeMap = android.webkit.MimeTypeMap;
	using Button = android.widget.Button;
	using ProgressBar = android.widget.ProgressBar;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;
	using Smc = com.samsung.android.sdk.mediacontrol.Smc;
	using SmcDevice = com.samsung.android.sdk.mediacontrol.SmcDevice;
	using SmcItem = com.samsung.android.sdk.mediacontrol.SmcItem;
	using SmcProvider = com.samsung.android.sdk.mediacontrol.SmcProvider;
	using DevicePicker = com.samsung.android.sdk.sample.mediabrowser.devicepicker.DevicePicker;

	public class Uploader : Activity, DevicePicker.DevicePickerResult, SmcProvider.EventListener, SmcProvider.ResponseListener
	{
		internal DevicePicker mTargetDevicePicker;
		internal Button mUploadButton;
		private SmcProvider provider;
		private bool uploadInProgress;
		private SmcItem itemToUpload;
		private ProgressBar progressBar;
		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.uploader;

			mTargetDevicePicker = (DevicePicker) FragmentManager.findFragmentById(R.id.targetPicker);
			mTargetDevicePicker.DeviceType = SmcDevice.TYPE_PROVIDER;
			mTargetDevicePicker.DeviceSelectedListener = this;
			mUploadButton = (Button) findViewById(R.id.button);
			mUploadButton.OnClickListener = uploadClickListener;
			progressBar = (ProgressBar) findViewById(R.id.progress);


			SmcItem.LocalContent content = LocalContent;

			if (content == null)
			{
				Toast.makeText(this, "Content not supported", Toast.LENGTH_SHORT).show();
				this.finish();
			}
			else
			{
				itemToUpload = new SmcItem(content);
				((TextView)findViewById(R.id.header)).Text = "File: " + itemToUpload.Uri.ToString();
			}
		}

		private SmcItem.LocalContent LocalContent
		{
			get
			{
				SmcItem.LocalContent content = null;
				if (Intent.Data != null)
				{
					if (Intent.Data.Scheme.Equals("content"))
					{
						Cursor c = ContentResolver.query(Intent.Data, new string[]{MediaStore.MediaColumns.DATA, MediaStore.MediaColumns.TITLE, MediaStore.MediaColumns.MIME_TYPE}, null, null, null);
						if (c != null)
						{
							if (c.moveToFirst() && c.getString(2) != null)
							{
								content = (new SmcItem.LocalContent(c.getString(0), c.getString(2))).setTitle(c.getString(1));
							}
							c.close();
						}
					}
					else
					{
						string path = Intent.DataString;
						if (path != null)
						{
							string name = path.Substring(path.LastIndexOf('/') + 1);
							string extension = name.Substring(name.LastIndexOf('.') + 1);
							content = (new SmcItem.LocalContent(path, MimeTypeMap.Singleton.getMimeTypeFromExtension(extension))).setTitle(name);
						}
					}
				}
				return content;
			}
		}




		public virtual void onDeviceSelected(SmcDevice device)
		{
			mUploadButton.Enabled = true;
			provider = (SmcProvider) device;
			provider.EventListener = this;
			provider.ResponseListener = this;
			mUploadButton.Enabled = provider.Uploadable;
			if (!provider.Uploadable)
			{
				Toast.makeText(this, "This provider doesn't support file upload", Toast.LENGTH_SHORT).show();
			}
		}

		public virtual void onAllShareDisabled()
		{
			mUploadButton.Enabled = false;
			cancelUpload();
			provider.ResponseListener = null;
			provider.EventListener = null;
			provider = null;
		}


		private View.OnClickListener uploadClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				if (outerInstance.itemToUpload == null)
				{
					Toast.makeText(outerInstance, "Content not supported", Toast.LENGTH_SHORT).show();
				}
				if (outerInstance.uploadInProgress)
				{
					outerInstance.cancelUpload();
				}
				else
				{
					outerInstance.startUpload();
				}
			}
		}

		private void startUpload()
		{
			if (provider != null)
			{
				provider.upload(itemToUpload);
				uploadInProgress = true;
				mUploadButton.Text = "Cancel";
			}
			else
			{
				Toast.makeText(this, "Please select target device", Toast.LENGTH_SHORT).show();
			}
		}

		private void cancelUpload()
		{
			if (provider != null && uploadInProgress)
			{
				provider.uploadCancel(itemToUpload);
				uploadInProgress = false;
				progressBar.Progress = 0;
				mUploadButton.Text = R.@string.upload;
			}
			else
			{
				Toast.makeText(this, "Please select target device", Toast.LENGTH_SHORT).show();
			}
		}

		public override void onContentUpdated(SmcProvider smcProvider, int error)
		{

		}

		public override void onUploadProgressUpdated(SmcProvider smcProvider, long receivedSize, long totalSize, SmcItem smcItem, int error)
		{
			progressBar.Max = (int) totalSize;
			progressBar.Progress = (int) receivedSize;
		}

		public override void onUploadCompleted(SmcProvider smcProvider, SmcItem smcItem)
		{
			mUploadButton.Text = R.@string.upload;
			progressBar.Progress = 0;
			uploadInProgress = false;
			Toast.makeText(this, "File uploaded successfully", Toast.LENGTH_SHORT).show();
		}

		public override void onBrowse(SmcProvider smcProvider, IList<SmcItem> smcItems, int requestedStartIndex, int requestedCount, SmcItem requestedFolder, bool endOfItems, int error)
		{

		}

		public override void onSearch(SmcProvider smcProvider, IList<SmcItem> smcItems, int requestedStartIndex, int requestedCount, SmcProvider.SearchCriteria searchCriteria, bool endOfItems, int error)
		{

		}

		public override void onUpload(SmcProvider smcProvider, SmcItem smcItem, int error)
		{
			if (error != Smc.SUCCESS)
			{
				Toast.makeTextuniquetempvar.show();
			}
		}

		public override void onUploadCancel(SmcProvider smcProvider, SmcItem smcItem, int error)
		{
			Toast.makeText(this, "Upload canceled", Toast.LENGTH_SHORT).show();
		}
	}

}