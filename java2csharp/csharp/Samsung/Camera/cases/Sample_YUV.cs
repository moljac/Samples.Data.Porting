using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using TargetApi = android.annotation.TargetApi;
	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Configuration = android.content.res.Configuration;
	using ImageFormat = android.graphics.ImageFormat;
	using Matrix = android.graphics.Matrix;
	using RectF = android.graphics.RectF;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
	using StreamConfigurationMap = android.hardware.camera2.@params.StreamConfigurationMap;
	using Image = android.media.Image;
	using ImageReader = android.media.ImageReader;
	using ImageWriter = android.media.ImageWriter;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using HandlerThread = android.os.HandlerThread;
	using DateFormat = android.text.format.DateFormat;
	using Log = android.util.Log;
	using Size = android.util.Size;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using Toast = android.widget.Toast;

	using InputConfiguration = com.samsung.android.sdk.camera.@params.InputConfiguration;
	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;


//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TargetApi(23) public class Sample_YUV extends android.app.Activity
	public class Sample_YUV : Activity
	{
		private bool InstanceFieldsInitialized = false;

		public Sample_YUV()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mImageSaver = new ImageSaver(this, this);
		}

		private static readonly string TAG = typeof(Sample_YUV).Name;

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;
		private SCameraDevice mSCameraDevice;
		private SCameraCaptureSession mSCameraSession;
		private SCameraCharacteristics mCharacteristics;
		private SCaptureRequest.Builder mRequestBuilder;

		/// <summary>
		/// ID of the current <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private string mCameraId;

		private HandlerThread mBackgroundHandlerThread;
		private Handler mBackgroundHandler;
		private HandlerThread mReaderHandlerThread;
		private Handler mReaderHandler;

		private Semaphore mCameraOpenCloseLock = new Semaphore(1);

		private Size mPreviewSize;

		private const int MAX_PREVIEW_WIDTH = 1920;
		private const int MAX_PREVIEW_HEIGHT = 1080;
		private AutoFitTextureView mTextureView;

		private ImageReader mYUVReader;
		private ImageReader mJpegReader;
		private ImageWriter mReprocessWriter;

		private ImageSaver mImageSaver;

		private BlockingQueue<STotalCaptureResult> mCaptureResultQueue = new LinkedBlockingQueue<STotalCaptureResult>();

		private ImageReader.OnImageAvailableListener mYUVImageListener = new OnImageAvailableListenerAnonymousInnerClassHelper();

		private class OnImageAvailableListenerAnonymousInnerClassHelper : ImageReader.OnImageAvailableListener
		{
			public OnImageAvailableListenerAnonymousInnerClassHelper()
			{
			}

			public override void onImageAvailable(ImageReader reader)
			{

				STotalCaptureResult result = null;
				Image image = reader.acquireNextImage();

				try
				{
					result = outerInstance.mCaptureResultQueue.take();
				}
				catch (InterruptedException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}


				{
				// Simple YUV processing that makes brightness value be quantized by 10.
					ByteBuffer y_buffer = image.Planes[0].Buffer;
					sbyte[] y_byte_array = new sbyte[y_buffer.capacity()];
					y_buffer.get(y_byte_array);

					int size = image.Width * image.Height;
					for (int i = 0; i < size; i++)
					{
						y_byte_array[i] = (sbyte)(y_byte_array[i] / 10 * 10);
					}

					y_buffer.rewind();
					y_buffer.put(y_byte_array);
				}

				try
				{
					SCaptureRequest.Builder builder = outerInstance.mSCameraDevice.createReprocessCaptureRequest(result);
					builder.addTarget(outerInstance.mJpegReader.Surface);

					// Option #1. Put Image obtained from ImageReader directly to ImageWriter
					outerInstance.mReprocessWriter.queueInputImage(image);

					/* Option #2. Obtain input Image from ImageWriter and copy to it. Then push back to ImageWriter. potentially with zero copy
					Image inputImage = mReprocessWriter.dequeueInputImage();
					//copy image to inputImage here
					mReprocessWriter.queueInputImage(inputImage);
					*/


					outerInstance.mSCameraSession.capture(builder.build(), null, outerInstance.mBackgroundHandler);
				}
				catch (CameraAccessException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private ImageReader.OnImageAvailableListener mJpegImageListener = new OnImageAvailableListenerAnonymousInnerClassHelper2();

		private class OnImageAvailableListenerAnonymousInnerClassHelper2 : ImageReader.OnImageAvailableListener
		{
			public OnImageAvailableListenerAnonymousInnerClassHelper2()
			{
			}

			public override void onImageAvailable(ImageReader reader)
			{
				Image image = reader.acquireNextImage();
				outerInstance.mImageSaver.save(image, outerInstance.createFileName() + ".jpg");
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_yuv;
		}

		protected internal override void onResume()
		{
			base.onResume();

			// initialize SCamera
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

			if (!checkRequiredFeatures())
			{
				return;
			}

			mSCameraManager = mSCamera.SCameraManager;
			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

			// Set SurfaceTextureListener that handle life cycle of TextureView
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);

			findViewById(R.id.picture).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			startBackgroundThread();

			openCamera();
		}

		private class SurfaceTextureListenerAnonymousInnerClassHelper : TextureView.SurfaceTextureListener
		{
			private readonly Sample_YUV outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
			{
				// "onSurfaceTextureAvailable" is called, which means that CameraCaptureSession is not created.
				// We need to configure transform for TextureView and crate CameraCaptureSession.
				outerInstance.configureTransform(width, height);
				outerInstance.createPreviewSession();
			}

			public override void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
			{
				// SurfaceTexture size changed, we need to configure transform for TextureView, again.
				outerInstance.configureTransform(width, height);
			}

			public override bool onSurfaceTextureDestroyed(SurfaceTexture surface)
			{
				return true;
			}

			public override void onSurfaceTextureUpdated(SurfaceTexture surface)
			{
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_YUV outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.takePicture();
			}
		}

		private void takePicture()
		{
			try
			{
				SCaptureRequest.Builder builder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
				builder.addTarget(mYUVReader.Surface);

				mSCameraSession.capture(builder.build(), new CaptureCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_YUV outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				try
				{
					outerInstance.mCaptureResultQueue.put(result);
				}
				catch (InterruptedException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		protected internal override void onPause()
		{
			stopBackgroundThread();
			closeCamera();

			base.onPause();
		}

		private void createPreviewSession()
		{
			lock (this)
			{
        
				if (null == mSCameraDevice || null == mSCameraManager || null == mPreviewSize || !mTextureView.Available)
				{
					return;
				}
        
				try
				{
					SurfaceTexture texture = mTextureView.SurfaceTexture;
        
					// Set default buffer size to camera preview size.
					texture.setDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);
        
					Surface surface = new Surface(texture);
        
					// Creates CaptureRequest.Builder for preview with output target.
					mRequestBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
					mRequestBuilder.addTarget(surface);
        
					// Creates a CameraCaptureSession here.
					IList<Surface> outputSurface = new List<Surface>();
					outputSurface.Add(mYUVReader.Surface);
					outputSurface.Add(surface);
					outputSurface.Add(mJpegReader.Surface);
        
					mSCameraDevice.createReprocessableCaptureSession(new InputConfiguration(mYUVReader.Width, mYUVReader.Height, mYUVReader.ImageFormat), outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
				}
				catch (CameraAccessException)
				{
					showAlertDialog("Fail to create camera capture session.", true);
				}
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_YUV outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConfigured(SCameraCaptureSession sCameraCaptureSession)
			{
				outerInstance.mSCameraSession = sCameraCaptureSession;

				// Configures an ImageWriter
				outerInstance.mReprocessWriter = ImageWriter.newInstance(outerInstance.mSCameraSession.InputSurface, 2);
				outerInstance.mReprocessWriter.setOnImageReleasedListener(new OnImageReleasedListenerAnonymousInnerClassHelper(this), outerInstance.mBackgroundHandler);
				outerInstance.startPreview();
			}

			private class OnImageReleasedListenerAnonymousInnerClassHelper : ImageWriter.OnImageReleasedListener
			{
				private readonly StateCallbackAnonymousInnerClassHelper outerInstance;

				public OnImageReleasedListenerAnonymousInnerClassHelper(StateCallbackAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void onImageReleased(ImageWriter writer)
				{
					Log.d(TAG, "onImageReleased");
				}
			}

			public override void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession)
			{
				outerInstance.showAlertDialog("Fail to create camera capture session.", true);
			}
		}

		/// <summary>
		/// Starts a preview.
		/// </summary>
		private void startPreview()
		{
			lock (this)
			{
				if (mSCameraSession == null)
				{
					return;
				}
        
				try
				{
					// Starts displaying the preview.
					mSCameraSession.setRepeatingRequest(mRequestBuilder.build(), null, mBackgroundHandler);
				}
				catch (CameraAccessException)
				{
					showAlertDialog("Fail to start preview.", true);
				}
			}
		}

		private void openCamera()
		{
			lock (this)
			{
				try
				{
					if (!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS))
					{
						showAlertDialog("Time out waiting to lock camera opening.", true);
					}
        
        
        
					// acquires camera characteristics
					mCharacteristics = mSCameraManager.getCameraCharacteristics(mSCameraManager.CameraIdList[0]);
        
					StreamConfigurationMap streamConfigurationMap = mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
        
					// Acquires supported preview size list that supports SurfaceTexture
					mPreviewSize = streamConfigurationMap.getOutputSizes(typeof(SurfaceTexture))[0];
					foreach (Size option in streamConfigurationMap.getOutputSizes(typeof(SurfaceTexture)))
					{
						// Find maximum preview size that is not larger than MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT
						int areaCurrent = Math.Abs((mPreviewSize.Width * mPreviewSize.Height) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
						int areaNext = Math.Abs((option.Width * option.Height) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
        
						if (areaCurrent > areaNext)
						{
							mPreviewSize = option;
						}
					}
        
					// Acquires supported input size for YUV_420_888 format.
					Size yuvSize = streamConfigurationMap.getInputSizes(ImageFormat.YUV_420_888)[0];
        
					// Configures an ImageReader
					mYUVReader = ImageReader.newInstance(yuvSize.Width, yuvSize.Height, ImageFormat.YUV_420_888, 2);
					mJpegReader = ImageReader.newInstance(mYUVReader.Width, mYUVReader.Height, ImageFormat.JPEG, 2);
        
					mYUVReader.setOnImageAvailableListener(mYUVImageListener, mReaderHandler);
					mJpegReader.setOnImageAvailableListener(mJpegImageListener, mReaderHandler);
        
					// Set the aspect ratio to TextureView
					int orientation = Resources.Configuration.orientation;
					if (orientation == Configuration.ORIENTATION_LANDSCAPE)
					{
						mTextureView.setAspectRatio(mPreviewSize.Width, mPreviewSize.Height);
					}
					else
					{
						mTextureView.setAspectRatio(mPreviewSize.Height, mPreviewSize.Width);
					}
        
					// Opening the camera device here
					mSCameraManager.openCamera(mCameraId, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
				}
				catch (CameraAccessException e)
				{
					showAlertDialog("Cannot open the camera.", true);
					Log.e(TAG, "Cannot open the camera.", e);
				}
				catch (InterruptedException e)
				{
					throw new Exception("Interrupted while trying to lock camera opening.", e);
				}
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraDevice.StateCallback
		{
			private readonly Sample_YUV outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onOpened(SCameraDevice sCameraDevice)
			{
				outerInstance.mCameraOpenCloseLock.release();
				outerInstance.mSCameraDevice = sCameraDevice;
				outerInstance.createPreviewSession();
			}

			public override void onDisconnected(SCameraDevice sCameraDevice)
			{
				outerInstance.mCameraOpenCloseLock.release();
				Log.e(TAG, "Camera Disconnected.");
				outerInstance.showAlertDialog("Camera disconnected.", true);
			}

			public override void onError(SCameraDevice sCameraDevice, int i)
			{
				outerInstance.mCameraOpenCloseLock.release();
				outerInstance.showAlertDialog("Error while camera open.", true);
			}
		}

		/// <summary>
		/// Closes a camera and release resources.
		/// </summary>
		private void closeCamera()
		{
			lock (this)
			{
				try
				{
					mCameraOpenCloseLock.acquire();
        
					if (mSCameraSession != null)
					{
						mSCameraSession.close();
						mSCameraSession = null;
					}
        
					if (mSCameraDevice != null)
					{
						mSCameraDevice.close();
						mSCameraDevice = null;
					}
        
					mSCameraManager = null;
				}
				catch (InterruptedException e)
				{
					Log.e(TAG, "Interrupted while trying to lock camera closing.", e);
				}
				finally
				{
					mCameraOpenCloseLock.release();
				}
			}
		}

		/// <summary>
		/// Configures requires transform <seealso cref="android.graphics.Matrix"/> to TextureView.
		/// </summary>
		private void configureTransform(int viewWidth, int viewHeight)
		{
			if (null == mTextureView || null == mPreviewSize)
			{
				return;
			}

			int rotation = WindowManager.DefaultDisplay.Rotation;
			Matrix matrix = new Matrix();
			RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
			RectF bufferRect = new RectF(0, 0, mPreviewSize.Height, mPreviewSize.Width);
			float centerX = viewRect.centerX();
			float centerY = viewRect.centerY();
			if (Surface.ROTATION_90 == rotation || Surface.ROTATION_270 == rotation)
			{
				bufferRect.offset(centerX - bufferRect.centerX(), centerY - bufferRect.centerY());
				matrix.setRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.FILL);
				float scale = Math.Max((float) viewHeight / mPreviewSize.Height, (float) viewWidth / mPreviewSize.Width);
				matrix.postScale(scale, scale, centerX, centerY);
				matrix.postRotate(90 * (rotation - 2), centerX, centerY);
			}
			else
			{
				matrix.postRotate(90 * rotation, centerX, centerY);
			}

			mTextureView.Transform = matrix;
			mTextureView.SurfaceTexture.setDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);
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
			dialog.setMessage(message).setIcon(android.R.drawable.ic_dialog_alert).setTitle("Alert").setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper(this, finishActivity, dialog))
				   .Cancelable = false;

			runOnUiThread(() =>
			{
				dialog.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly Sample_YUV outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_YUV outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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

		/// <summary>
		/// Starts back ground thread that callback from camera will posted.
		/// </summary>
		private void startBackgroundThread()
		{
			mBackgroundHandlerThread = new HandlerThread("Background Thread");
			mBackgroundHandlerThread.start();
			mBackgroundHandler = new Handler(mBackgroundHandlerThread.Looper);

			mReaderHandlerThread = new HandlerThread("Reader Thread");
			mReaderHandlerThread.start();
			mReaderHandler = new Handler(mReaderHandlerThread.Looper);
		}

		/// <summary>
		/// Stops back ground thread.
		/// </summary>
		private void stopBackgroundThread()
		{
			if (mBackgroundHandlerThread != null)
			{
				mBackgroundHandlerThread.quitSafely();
				try
				{
					mBackgroundHandlerThread.join();
					mBackgroundHandlerThread = null;
					mBackgroundHandler = null;
				}
				catch (InterruptedException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			if (mReaderHandlerThread != null)
			{
				mReaderHandlerThread.quitSafely();
				try
				{
					mReaderHandlerThread.join();
					mReaderHandlerThread = null;
					mReaderHandler = null;
				}
				catch (InterruptedException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		/// <summary>
		/// Saves <seealso cref="android.media.Image"/> to file.
		/// </summary>
		private class ImageSaver
		{
			private readonly Sample_YUV outerInstance;

			public ImageSaver(Sample_YUV outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: void save(final android.media.Image image, String filename)
			internal virtual void save(Image image, string filename)
			{

				File dir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).AbsolutePath + "/Camera/");
				if (!dir.exists())
				{
					dir.mkdirs();
				}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(dir, filename);
				File file = new File(dir, filename);

				outerInstance.mBackgroundHandler.post(() =>
				{
					ByteBuffer buffer = image.Planes[0].Buffer;
					sbyte[] bytes = new sbyte[buffer.remaining()];
					buffer.get(bytes);
					System.IO.FileStream output = null;
					try
					{
						output = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
						output.Write(bytes, 0, bytes.Length);
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
					finally
					{
						image.close();
						if (null != output)
						{
							try
							{
								output.Close();
							}
							catch (IOException e)
							{
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}
						}
					}

					MediaScannerConnection.scanFile(outerInstance, new string[]{file.AbsolutePath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper(this));

					runOnUiThread(() =>
					{
						Toast.makeTextuniquetempvar.show();
					});
				});
			}

			private class OnScanCompletedListenerAnonymousInnerClassHelper : MediaScannerConnection.OnScanCompletedListener
			{
				private readonly ImageSaver outerInstance;

				public OnScanCompletedListenerAnonymousInnerClassHelper(ImageSaver outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void onScanCompleted(string path, Uri uri)
				{
					Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
				}
			}
		}

		/// <summary>
		/// Creates file name based on current time.
		/// </summary>
		private string createFileName()
		{
			GregorianCalendar calendar = new GregorianCalendar();
			calendar.TimeZone = TimeZone.Default;
			long dateTaken = calendar.TimeInMillis;

			return DateFormat.format("yyyyMMdd_kkmmss", dateTaken).ToString();
		}

		/// <summary>
		/// Convert int array to Integer list.
		/// </summary>
		private IList<int?> asList(int[] array)
		{
			IList<int?> list = new List<int?>();

			foreach (int value in array)
			{
				list.Add(value);
			}
			return list;
		}

		private bool checkRequiredFeatures()
		{
			if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
			{
				showAlertDialog("Device running Android prior to M is not compatible with the reprocessing APIs.", true);
				Log.e(TAG, "Device running Android prior to M is not compatible with the reprocessing APIs.");

				return false;
			}

			try
			{
				mCameraId = null;
				foreach (string id in mSCamera.SCameraManager.CameraIdList)
				{
					SCameraCharacteristics cameraCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(id);
					if (cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_BACK)
					{
						mCameraId = id;
						break;
					}
				}

				if (mCameraId == null)
				{
					showAlertDialog("No back-facing camera exist.", true);
					return false;
				}

				mCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(mCameraId);

				if (!asList(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES)).Contains(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_YUV_REPROCESSING))
				{
					showAlertDialog("YUV reprocessing capability is not supported.", true);
					Log.e(TAG, "YUV reprocessing capability is not supported.");

					return false;
				}

			}
			catch (CameraAccessException e)
			{
				showAlertDialog("Cannot access the camera.", true);
				Log.e(TAG, "Cannot access the camera.", e);
				return false;
			}

			return true;
		}
	}

}