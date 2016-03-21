using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Log = android.util.Log;
	using View = android.view.View;
	using Button = android.widget.Button;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using SCameraFilter = com.samsung.android.sdk.camera.filter.SCameraFilter;
	using SCameraFilterInfo = com.samsung.android.sdk.camera.filter.SCameraFilterInfo;
	using SCameraFilterManager = com.samsung.android.sdk.camera.filter.SCameraFilterManager;


	public class Sample_Filter : Activity, View.OnClickListener
	{
		/// <summary>
		/// Tag for the <seealso cref="Log"/>.
		/// </summary>
		private const string TAG = "Sample_Filter";

		// SCameraFilter only supports a 8192x8192 or less resolution.
		private const int MAX_IMAGE_SIZE = 8192;

		private readonly int PROCESS_BITMAP = 1;
		private readonly int PROCESS_FILE = 2;

		private readonly string mCameraDirectory = Environment.ExternalStorageDirectory.Path + "/DCIM/Camera";

		private SCamera mSCamera = null;

		/// <summary>
		/// list of retrieving available filters.
		/// </summary>
		private IList<SCameraFilterInfo> mFilterInfoList;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.filter.SCameraFilterManager"/> for creating and retrieving available filters
		/// </summary>
		private SCameraFilterManager mSCameraFilterManager;


		/// <summary>
		/// Button to processing for bitmap format
		/// </summary>
		private Button mBitmapButton = null;

		/// <summary>
		/// Button to processing for image file
		/// </summary>
		private Button mFileButton = null;

		/// <summary>
		/// ImageView to display a selected image.
		/// </summary>
		private ImageView mInputImageView = null;

		/// <summary>
		/// ImageView to display a processed image.
		/// </summary>
		private ImageView mOutputImageView = null;

		/// <summary>
		/// TextView to display a selected filter.
		/// </summary>
		private TextView mSelectedFilterTextView = null;

		/// <summary>
		/// ID of selected filter.
		/// </summary>
		private int mFilterID = -1;

		/// <summary>
		/// list of filter name
		/// </summary>
		private string[] mFilterNames = null;

		/// <summary>
		/// list of file in [/DCIM/Camera]
		/// </summary>
		private string[] mFileNames = null;

		/// <summary>
		/// file path of a processed image.
		/// </summary>
		private string mInputImage = null;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_filter;

			mSelectedFilterTextView = (TextView) findViewById(R.id.selected_textview);

			findViewById(R.id.btn_select_image).OnClickListener = this;
			findViewById(R.id.btn_select_filter).OnClickListener = this;

			mBitmapButton = (Button) findViewById(R.id.btn_bitmap);
			mFileButton = (Button) findViewById(R.id.btn_file);

			mBitmapButton.OnClickListener = this;
			mFileButton.OnClickListener = this;

			mBitmapButton.Enabled = false;
			mFileButton.Enabled = false;

			mInputImageView = (ImageView) findViewById(R.id.input_image_view);
			mOutputImageView = (ImageView) findViewById(R.id.output_image_view);

			mSCamera = new SCamera();
			try
			{
				mSCamera.initialize(this);
			}
			catch (SsdkUnsupportedException)
			{
				showAlertDialog("Fail to initialize SCamera.", true);
				return;
			}
			if (!mSCamera.isFeatureEnabled(SCamera.SCAMERA_FILTER))
			{
				showAlertDialog("This device does not support SCamera Filter feature.", true);
				return;
			}

			// retrieving an {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager}
			mSCameraFilterManager = mSCamera.SCameraFilterManager;

			// retrieving available filters
			mFilterInfoList = mSCameraFilterManager.AvailableFilters;

			if (mFilterInfoList.Count > 0)
			{
				mSelectedFilterTextView.Text = mFilterInfoList[0].Name;
			}

		}

		protected internal override void onDestroy()
		{
			mSCamera = null;
			base.onDestroy();
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
				case R.id.btn_select_image:
					selectImage();
					break;
				case R.id.btn_select_filter:
					selectFilter();
					break;
				case R.id.btn_bitmap:
					process(PROCESS_BITMAP);
					break;
				case R.id.btn_file:
					process(PROCESS_FILE);
					break;
			}
		}

		/// <summary>
		/// select an image file for processing filter.
		/// </summary>
		private void selectImage()
		{

			mFileNames = null;

			File cameraDirectory = new File(mCameraDirectory);
			if (!cameraDirectory.exists())
			{
				cameraDirectory.mkdirs();
				return;
			}

			File[] cameraImages = cameraDirectory.listFiles(new FileFilterAnonymousInnerClassHelper(this));
			if (cameraImages == null || cameraImages.Length == 0)
			{
				return;
			}

			mFileNames = new string[cameraImages.Length];

			for (int i = 0; i < cameraImages.Length; i++)
			{
				mFileNames[i] = mCameraDirectory + "/" + cameraImages[i].Name;
			}

			(new AlertDialog.Builder(Sample_Filter.this)).setTitle("Select Image").setItems(mFileNames, new OnClickListenerAnonymousInnerClassHelper(this))
				   .show();

		}

		private class FileFilterAnonymousInnerClassHelper : FileFilter
		{
			private readonly Sample_Filter outerInstance;

			public FileFilterAnonymousInnerClassHelper(Sample_Filter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual bool accept(File file)
			{
				return file.File && file.Name.toLowerCase().EndsWith(".jpg", StringComparison.Ordinal);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly Sample_Filter outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Filter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				string inputImage = outerInstance.mFileNames[which];

				if (!outerInstance.setImageView(outerInstance.mInputImageView, inputImage))
				{
					outerInstance.showAlertDialog("This image resolution is greater than the " + MAX_IMAGE_SIZE + "x" + MAX_IMAGE_SIZE, false);
					return;
				}

				outerInstance.mInputImage = inputImage;
				outerInstance.mBitmapButton.Enabled = true;
				outerInstance.mFileButton.Enabled = true;
			}
		}

		protected internal override void onResume()
		{
			base.onResume();

			if (mInputImage != null)
			{
				File inputFile = new File(mInputImage);
				if (!inputFile.exists())
				{
					mInputImage = null;
					mBitmapButton.Enabled = false;
					mFileButton.Enabled = false;
					mInputImageView.ImageDrawable = getDrawable(R.drawable.bg_image_frame);
					mOutputImageView.ImageDrawable = getDrawable(R.drawable.bg_image_frame);
				}
			}
		}

		/// <summary>
		/// select an filter file for processing.
		/// </summary>
		private void selectFilter()
		{

			int filterNum = mFilterInfoList.Count;
			if (filterNum < 1)
			{
				return;
			}

			mFilterNames = new string[filterNum];

			for (int i = 0; i < filterNum; i++)
			{
				mFilterNames[i] = mFilterInfoList[i].Name;
			}

			(new AlertDialog.Builder(Sample_Filter.this)).setTitle("Select Filter").setItems(mFilterNames, new OnClickListenerAnonymousInnerClassHelper2(this))
				   .show();
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly Sample_Filter outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(Sample_Filter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				outerInstance.mFilterID = which;
				outerInstance.mSelectedFilterTextView.Text = outerInstance.mFilterNames[which];
			}
		}

		private void process(int type)
		{
			if (mInputImage == null)
			{
				Toast tempToast = Toast.makeText(Sample_Filter.this, "please select image.", Toast.LENGTH_SHORT);
				tempToast.show();
				return;
			}

			string outputPath = mCameraDirectory + "/FILTER_IMAGE.jpg";
			processFilter(mInputImage, outputPath, type);
			setImageView(mOutputImageView, outputPath);

			MediaScannerConnection.scanFile(this, new string[]{outputPath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper(this));
		}

		private class OnScanCompletedListenerAnonymousInnerClassHelper : MediaScannerConnection.OnScanCompletedListener
		{
			private readonly Sample_Filter outerInstance;

			public OnScanCompletedListenerAnonymousInnerClassHelper(Sample_Filter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onScanCompleted(string path, Uri uri)
			{
				Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
			}
		}

		/// <summary>
		/// display an image file.
		/// </summary>
		private bool setImageView(ImageView imageview, string filepath)
		{

			bool bSCameraFilterSupported = true;

			BitmapFactory.Options options = new BitmapFactory.Options();
			options.inJustDecodeBounds = true;
			BitmapFactory.decodeFile(filepath, options);

			int photoWidth = options.outWidth;
			int photoHeight = options.outHeight;

			int targetWidth = photoWidth;
			int targetHeight = photoHeight;

			int photoSize = (photoWidth > photoHeight) ? photoWidth : photoHeight;

			if (photoSize > MAX_IMAGE_SIZE)
			{
				bSCameraFilterSupported = false;
				return bSCameraFilterSupported;
			}

			if (photoSize > 1920)
			{
				int scale = (photoSize / 1920) + 1;
				targetWidth = photoWidth / scale;
				targetHeight = photoHeight / scale;
			}

			int scaleFactor = Math.Min(photoWidth / targetWidth, photoHeight / targetHeight);

			options.inJustDecodeBounds = false;
			options.inDither = false;
			options.inSampleSize = scaleFactor;
			Bitmap orgImage = BitmapFactory.decodeFile(filepath, options);
			imageview.ImageBitmap = orgImage;

			return bSCameraFilterSupported;
		}

		/// <summary>
		/// save an bitmap object.
		/// </summary>
		private void saveImage(Bitmap bitmap, string filename)
		{

			if (bitmap != null)
			{

				File file = new File(filename);
				System.IO.Stream @out = null;
				try
				{
					file.createNewFile();
					@out = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
					bitmap.compress(Bitmap.CompressFormat.JPEG, 95, @out);
					bitmap.recycle();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				finally
				{
					if (@out != null)
					{
						try
						{
						@out.Close();
						}
						catch (IOException e)
						{
							Console.WriteLine(e.ToString());
							Console.Write(e.StackTrace);
						}
					}
				}

			}
		}

		// ----------------------------------------------------------------------------

		/// <summary>
		/// process a filter
		/// </summary>
		private void processFilter(string inputFile, string outputFile, int type)
		{
			Log.e(TAG, "InputFile:  " + inputFile);
			Log.e(TAG, "OutputFile:  " + outputFile);
			Log.e(TAG, "Type:" + type);
			Log.e(TAG, "Filter:  " + mFilterID);

			SCameraFilter filter;

			// create a SCameraFilter using SCameraFilterManager.
			if (mFilterID == -1)
			{
				filter = mSCameraFilterManager.createFilter(mFilterInfoList[0]);
			}
			else
			{
				filter = mSCameraFilterManager.createFilter(mFilterInfoList[mFilterID]);
			}

			switch (type)
			{
				case PROCESS_BITMAP:
					Bitmap inputBitmap = BitmapFactory.decodeFile(inputFile);

					// process filter using bitmap object
					Bitmap outputBitmap = filter.processImage(inputBitmap);
					if (outputBitmap == null)
					{
						return;
					}

					// save a bitmap object
					saveImage(outputBitmap, outputFile);

					inputBitmap.recycle();
					outputBitmap.recycle();
					break;
				case PROCESS_FILE:
					// process filter using image file
					filter.processImage(inputFile, outputFile);
					break;
			}
		}

		/// <summary>
		/// Shows alert dialog.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showAlertDialog(String message, final boolean finishActivity)
		private void showAlertDialog(string message, bool finishActivity)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.app.AlertDialog.Builder dialog = new android.app.AlertDialog.Builder(this);
			AlertDialog.Builder dialog = new AlertDialog.Builder(this);
			dialog.setMessage(message).setIcon(android.R.drawable.ic_dialog_alert).setTitle("Alert").setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper3(this, finishActivity, dialog))
			   .Cancelable = false;

			runOnUiThread(() =>
			{
				dialog.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : DialogInterface.OnClickListener
		{
			private readonly Sample_Filter outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper3(Sample_Filter outerInstance, bool finishActivity, AlertDialog.Builder dialog)
			{
				this.outerInstance = outerInstance;
				this.finishActivity = finishActivity;
				this.dialog = dialog;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				dialog.dismiss();
				if (finishActivity)
				{
					finish();
				}
			}
		}
	}

}