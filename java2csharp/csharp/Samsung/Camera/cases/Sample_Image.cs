using System;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using DocumentsContract = android.provider.DocumentsContract;
	using MediaStore = android.provider.MediaStore;
	using DisplayMetrics = android.util.DisplayMetrics;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ImageView = android.widget.ImageView;
	using Toast = android.widget.Toast;

	using SCameraImage = com.samsung.android.sdk.camera.image.SCameraImage;
	using SCameraImageCore = com.samsung.android.sdk.camera.image.SCameraImageCore;
	using SCameraImageMatrix = com.samsung.android.sdk.camera.image.SCameraImageMatrix;


	public class Sample_Image : Activity
	{
		/// <summary>
		/// Tag for the <seealso cref="Log"/>.
		/// </summary>
		private const string TAG = "Sample_IPX";

		private SCamera mScamera;

		/// <summary>
		/// Image buffer for processing image.
		/// </summary>
		private sbyte[] buffer = null;
		private static ImageView mInputView;
		private static ImageView mOutputView;
		private static Bitmap mInputBitmap;
		private static Bitmap mOutputBitmap;
		private static SCameraImage mInputImage = null;
		private static SCameraImage mOutputImage = null;

		/// <summary>
		/// Current Screen Width width app will use.
		/// </summary>
		private int mScreenWidth = 0;

		 /// 
		 /// <summary>
		 /// Current Screen Height app will use.
		 /// 
		 /// </summary>
		private int mScreenHeight = 0;

		private const int SELECT_PICTURE = 1;

		/// <summary>
		/// Prepares an UI, like button, dialog, etc. and
		/// </summary>
		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_ipx;
			Log.v(TAG, "onCreate");

			mInputView = (ImageView)findViewById(R.id.image_in);
			mOutputView = (ImageView)findViewById(R.id.image_out);

			// Get screen width.
			DisplayMetrics dm = new DisplayMetrics();
			WindowManager.DefaultDisplay.getMetrics(dm);
			mScreenWidth = dm.widthPixels;
			mScreenHeight = dm.heightPixels;

			// initialize SCamera
			mScamera = new SCamera();
			try
			{
				mScamera.initialize(this);
			}
			catch (SsdkUnsupportedException)
			{
				showAlertDialog("Fail to initialize SCamera.", true);
				return;
			}
			if (!mScamera.isFeatureEnabled(SCamera.SCAMERA_IMAGE))
			{
				showAlertDialog("This device does not support SCamera Image Processing Accelerator feature.", true);
				return;
			}
			Log.v(TAG, "SDK feature check passed");

			// initialize Gellery button
			ImageView openGalleryButton = (ImageView) findViewById(R.id.button1);
			openGalleryButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			makeBufferWithDefaultPath();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Image outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Image outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				Intent intent = new Intent();
				intent.Type = "image/*";
				intent.Action = Intent.ACTION_GET_CONTENT;
				startActivityForResult(Intent.createChooser(intent, "Select Picture"), SELECT_PICTURE);

			}
		}

		/// <summary>
		/// Get selected image from gallery.
		/// </summary>
		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			if (resultCode == RESULT_OK)
			{
				if (requestCode == SELECT_PICTURE)
				{
					Uri selectedImageUri = data.Data;

					if (!ContentResolver.getType(selectedImageUri).Equals("image/jpeg"))
					{
						Toast.makeText(this, "Only support JPEG file.", Toast.LENGTH_LONG).show();
						return;
					}

					string imagePath = getPath(selectedImageUri);
					if (imagePath == null)
					{
						Log.d(TAG, "imagepath from getPath null");
						// get the id of the image selected by the user
						string wholeID = DocumentsContract.getDocumentId(data.Data);
						string id = wholeID.Split(":", true)[1];

						string[] projection = new string[] {MediaStore.Images.Media.DATA};
						string whereClause = MediaStore.Images.Media._ID + "=?";
						Cursor cursor = ContentResolver.query(Uri, projection, whereClause, new string[]{id}, null);
						if (cursor != null)
						{
							int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
							if (cursor.moveToFirst())
							{
								imagePath = cursor.getString(column_index);
							}

							cursor.close();
						}
						else
						{
							imagePath = selectedImageUri.Path;
							Toast.makeText(this, imagePath, Toast.LENGTH_LONG).show();
						}
					}

					Log.d(TAG, "selected file path " + imagePath);

					if (mInputImage != null)
					{
						mInputImage.release();
					}

					//Make SImage from selected Image and show to Screen.
					mInputImage = new SCameraImage(imagePath,SCameraImage.FORMAT_DEFAULT);
					mInputBitmap = getOutputBitmap(mInputImage);
					if (mInputBitmap == null)
					{
						makeBufferWithDefaultPath();
					}
					else
					{
						mInputView.ImageBitmap = mInputBitmap;
					}
					mOutputView.ImageBitmap = null;
				}
			}
		}

		private Uri Uri
		{
			get
			{
				string state = Environment.ExternalStorageState;
				if (!state.Equals(Environment.MEDIA_MOUNTED, StringComparison.CurrentCultureIgnoreCase))
				{
					return MediaStore.Images.Media.INTERNAL_CONTENT_URI;
				}
    
				return MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
			}
		}

		private string getPath(Uri uri)
		{
			if (uri == null)
			{
				return null;
			}
			string[] projection = new string[] {MediaStore.Images.Media.DATA};
			Cursor cursor = ContentResolver.query(uri, projection, null, null, null);
			if (cursor != null)
			{
				int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
				cursor.moveToFirst();

				string path = cursor.getString(column_index);
				cursor.close();

				return path;
			}
			return uri.Path;
		}
		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.menu_ipx, menu);
			return true;
		}

		/// <summary>
		/// Each Image process working from selected item.
		/// </summary>
		public override bool onOptionsItemSelected(MenuItem item)
		{
			// Handle action bar item clicks here. The action bar will
			// automatically handle clicks on the Home/Up button, so long
			// as you specify a parent activity in AndroidManifest.xml.

			int id = item.ItemId;

			if (mOutputBitmap != null && id != R.id.Save_as_Jpeg && id != R.id.Save_as_Raw)
			{
				mOutputBitmap.recycle();
				mOutputBitmap = null;
			}

			if (mOutputImage != null && id != R.id.Save_as_Jpeg && id != R.id.Save_as_Raw)
			{
				mOutputImage.release();
				mOutputImage = null;
			}

			switch (id)
			{
				case R.id.Sobel:
				{
					Log.v(TAG, "Sobel");
					//Process sobel.
					try
					{
						mOutputImage = SCameraImageCore.processSobel(mInputImage,100);

						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);

						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
						Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
						return false;
					}
					return true;
				}

				case R.id.Median:
				{
					Log.v(TAG, "Median");
					//Process median.
					try
					{
						mOutputImage = SCameraImageCore.processMedian(mInputImage,5);
						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);

						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
						Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
						return false;
					}
					return true;
				}

				case R.id.Enhance_Contrast:
				{
					Log.v(TAG, "Enhance_Contrast");
					//Process Enhance_Contrast.
					try
					{
						mOutputImage = SCameraImageCore.enhanceContrast(mInputImage,100, 0.5f);
						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);

						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
						Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
						return false;
					}
					return true;
				}

				case R.id.Equalize_Histogram:
				{
					Log.v(TAG, "Equalize_Histogram");
					//Process Equalize_Histogram.
					try
					{
						mOutputImage = SCameraImageCore.equalizeHistogram(mInputImage);
						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);

						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
					   Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
					   return false;
					}
					return true;
				}
				case R.id.Warp_Affine:
				{
					Log.v(TAG, "Warp_Affine");
					//Make matrix for Warp_Affine
					try
					{
						SCameraImageMatrix mat = SCameraImageCore.DefaultAffineMatrix;
						SCameraImageCore.calculateAffineScale(mat,0.5f,0.5f);
						SCameraImageCore.calculateAffineSkew(mat,0.9f,0.8f);
						SCameraImageCore.calculateAffineRotation(mat,30,400,600);
						//Process Warp_Affine.
						mOutputImage = SCameraImageCore.warpAffine(mInputImage,mat);
						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);
						mat.release();
						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
						Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
						return false;
					}
					return true;
				}
				case R.id.Spatial_Filter:
				{
					Log.v(TAG, "Spatial_Filter");
					//Make matrix for Spatial_Filter
					int N = 7;
					try
					{
						SCameraImageMatrix matrix = new SCameraImageMatrix(N, N);
						for (int i = 0;i < N;i++)
						{
							for (int j = 0;j < N;j++)
							{
								matrix.setAt(i,j,1);
							}
						}
						//Process Spatial_Filter.
						mOutputImage = SCameraImageCore.filterSpatial(mInputImage,matrix);
						//Getting bitmap from output.
						mOutputBitmap = getOutputBitmap(mOutputImage);

						matrix.release();
						if (mOutputBitmap == null)
						{
							Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
							mOutputImage.release();
							return false;
						}
						else
						{
							//Show output image
							mOutputView.ImageBitmap = mOutputBitmap;
						}
					}
					catch (System.ArgumentException)
					{
						Toast.makeText(Application.BaseContext, "Failed to apply effect", Toast.LENGTH_SHORT).show();
						return false;
					}
					return true;
				}

				case R.id.Save_as_Jpeg:
				{
					Log.v(TAG, "Save_as_Jpeg");
					if (mOutputImage == null)
					{
						Toast.makeText(Application.BaseContext, "No image to save", Toast.LENGTH_SHORT).show();
						return true;
					}
					//Make filepath.
					File fileInjpg = new File(FileName + ".jpg");
					//Save to file.
					try
					{
						mOutputImage.saveAsJpeg(fileInjpg.AbsolutePath, 95);
					}
					catch (System.ArgumentException e)
					{
						Toast.makeTextuniquetempvar.show();
						return true;
					}
					Toast.makeTextuniquetempvar.show();

					MediaScannerConnection.scanFile(this, new string[]{fileInjpg.AbsolutePath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper(this));
					return true;
				}

				case R.id.Save_as_Raw:
				{
					Log.v(TAG, "Save_as_Raw");
					if (mOutputImage == null)
					{
						Toast.makeText(Application.BaseContext, "No image to save", Toast.LENGTH_SHORT).show();
						return true;
					}
					//Make filepath.
					File fileInyuv = new File(FileName + ".yuv");
					//Save to file.
					try
					{
						mOutputImage.saveAsRaw(fileInyuv.AbsolutePath);
					}
					catch (System.ArgumentException e)
					{
						Toast.makeTextuniquetempvar.show();
						return true;
					}
					Toast.makeTextuniquetempvar.show();
					return true;
				}

			}
			return base.onOptionsItemSelected(item);
		}

		private class OnScanCompletedListenerAnonymousInnerClassHelper : MediaScannerConnection.OnScanCompletedListener
		{
			private readonly Sample_Image outerInstance;

			public OnScanCompletedListenerAnonymousInnerClassHelper(Sample_Image outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onScanCompleted(string path, Uri uri)
			{
				Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
			}
		}

		/// <summary>
		/// Make file name for save image.
		/// </summary>
		private string FileName
		{
			get
			{
				GregorianCalendar calendar = new GregorianCalendar();
				calendar.TimeZone = TimeZone.Default;
				long dateTaken = calendar.TimeInMillis;
    
				File dir = new File(Environment.ExternalStorageDirectory.Path + "/DCIM/Camera/");
				if (!dir.exists())
				{
					dir.mkdirs();
				}
				return Environment.ExternalStorageDirectory.Path + "/DCIM/Camera/" + dateTaken.ToString();
			}
		}

		/// <summary>
		/// Load default image.
		/// </summary>
		private void makeBufferWithDefaultPath()
		{
			Log.v(TAG, "makeBufferWithDefaultPath");
			buffer = null;
			System.IO.Stream inputStream = Resources.openRawResource(R.raw.flower_resize);
			ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();

			int i;
			try
			{
				i = inputStream.Read();
				while (i != -1)
				{
					byteArrayOutputStream.write(i);
					i = inputStream.Read();
				}

				buffer = byteArrayOutputStream.toByteArray();
				inputStream.Close();
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			mInputImage = new SCameraImage(SCameraImage.FORMAT_DEFAULT, buffer);
			mInputBitmap = getOutputBitmap(mInputImage);
			mInputView.ImageBitmap = mInputBitmap;
		}

		/// <summary>
		/// Getting bitmap for show UI. Proportion screen width, bitmap size calcurate.
		/// </summary>
		private Bitmap getOutputBitmap(SCameraImage mImage)
		{
			float scaleWidth = (float)mScreenWidth / (float)mImage.Width;
			float scaleHeight = (float)mScreenHeight / (float)mImage.Height;
			float scale = Math.Min(scaleWidth, scaleHeight);
			try
			{
				if (scale < 1)
				{
					return mImage.getBitmap((int)(scale * mImage.Width), (int)(scale * mImage.Height));
				}
				else
				{
					return mImage.Bitmap;
				}
			}
			catch (System.OutOfMemoryException)
			{
				Log.e(TAG,"Not enough memory to getbitmap");
				return null;
			}
			catch (System.ArgumentException e)
			{
				Toast.makeTextuniquetempvar.show();
				return null;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showAlertDialog(String message, final boolean finishActivity)
		private void showAlertDialog(string message, bool finishActivity)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.app.AlertDialog.Builder dialog = new android.app.AlertDialog.Builder(this);
			AlertDialog.Builder dialog = new AlertDialog.Builder(this);
			dialog.setMessage(message).setIcon(android.R.drawable.ic_dialog_alert).setTitle("Alert").setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper(this, finishActivity, dialog))
			   .Cancelable = false;

			runOnUiThread(() =>
			{
				dialog.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly Sample_Image outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Image outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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

		protected internal override void onDestroy()
		{
			Log.v(TAG, "onDestroy");
			if (mInputBitmap != null)
			{
				mInputBitmap.recycle();
			}
			if (mOutputBitmap != null)
			{
				mOutputBitmap.recycle();
			}
			if (mInputImage != null)
			{
				mInputImage.release();
			}
			buffer = null;

			mScamera = null;
			base.onPause();
		}
	}

}