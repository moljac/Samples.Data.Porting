using System;
using System.Collections.Generic;
using System.Text;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Configuration = android.content.res.Configuration;
	using ImageFormat = android.graphics.ImageFormat;
	using Matrix = android.graphics.Matrix;
	using PixelFormat = android.graphics.PixelFormat;
	using Point = android.graphics.Point;
	using Rect = android.graphics.Rect;
	using RectF = android.graphics.RectF;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
	using BlackLevelPattern = android.hardware.camera2.@params.BlackLevelPattern;
	using Face = android.hardware.camera2.@params.Face;
	using MeteringRectangle = android.hardware.camera2.@params.MeteringRectangle;
	using StreamConfigurationMap = android.hardware.camera2.@params.StreamConfigurationMap;
	using ExifInterface = android.media.ExifInterface;
	using Image = android.media.Image;
	using ImageReader = android.media.ImageReader;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using HandlerThread = android.os.HandlerThread;
	using Looper = android.os.Looper;
	using DateFormat = android.text.format.DateFormat;
	using Log = android.util.Log;
	using Pair = android.util.Pair;
	using Range = android.util.Range;
	using Rational = android.util.Rational;
	using Size = android.util.Size;
	using SparseArray = android.util.SparseArray;
	using SparseIntArray = android.util.SparseIntArray;
	using OrientationEventListener = android.view.OrientationEventListener;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using WebView = android.webkit.WebView;
	using Toast = android.widget.Toast;

	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;
	using FaceRectView = com.samsung.android.sdk.camera.sample.cases.util.FaceRectView;
	using SettingDialog = com.samsung.android.sdk.camera.sample.cases.util.SettingDialog;
	using SettingItem = com.samsung.android.sdk.camera.sample.cases.util.SettingItem;


	public class Sample_Single : Activity, SettingDialog.OnCameraSettingUpdatedListener
	{
		private bool InstanceFieldsInitialized = false;

		public Sample_Single()
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

		/// <summary>
		/// Tag for the <seealso cref="Log"/>.
		/// </summary>
		private const string TAG = "Sample_Single";

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;
		private SCameraDevice mSCameraDevice;
		private SCameraCaptureSession mSCameraSession;
		private SCameraCharacteristics mCharacteristics;
		private SCaptureRequest.Builder mPreviewBuilder;
		private SCaptureRequest.Builder mCaptureBuilder;

		/// <summary>
		/// Current Preview Size.
		/// </summary>
		private Size mPreviewSize;
		/// <summary>
		/// ID of the current <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private string mCameraId;

		/// <summary>
		/// Lens facing. Camera with this facing will be opened
		/// </summary>
		private int mLensFacing;
		private IList<int?> mLensFacingList;

		/// <summary>
		/// Image saving format.
		/// </summary>
		private int mImageFormat;
		private IList<int?> mImageFormatList;

		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.SettingDialog"/> for camera setting UI.
		/// </summary>
		private SettingDialog mSettingDialog;

		/// <summary>
		/// An <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView"/> for camera preview.
		/// </summary>
		private AutoFitTextureView mTextureView;

		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.FaceRectView"/> for face detection UI.
		/// </summary>
		private FaceRectView mFaceRectView;

		private ImageReader mJpegReader;
		private ImageReader mRawReader;
		private ImageSaver mImageSaver;

		/// <summary>
		/// A camera related listener/callback will be posted in this handler.
		/// </summary>
		private Handler mBackgroundHandler;
		private HandlerThread mBackgroundHandlerThread;
		/// <summary>
		/// A image saving worker Runnable will be posted to this handler.
		/// </summary>
		private Handler mImageSavingHandler;
		private HandlerThread mImageSavingHandlerThread;

		private BlockingQueue<SCaptureResult> mCaptureResultQueue;

		/// <summary>
		/// An orientation listener for jpeg orientation
		/// </summary>
		private OrientationEventListener mOrientationListener;
		private int mLastOrientation = 0;

		private Semaphore mCameraOpenCloseLock = new Semaphore(1);

		/// <summary>
		/// True if <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER"/> is triggered.
		/// </summary>
		private bool isAFTriggered;
		/// <summary>
		/// True if <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_PRECAPTURE_TRIGGER"/> is triggered.
		/// </summary>
		private bool isAETriggered;

		/// <summary>
		/// Current app state.
		/// </summary>
		private CAMERA_STATE mState = CAMERA_STATE.IDLE;

		/// <summary>
		/// Maximum preview width app will use.
		/// </summary>
		private const int MAX_PREVIEW_WIDTH = 1920;
		/// <summary>
		/// Maximum preview height app will use.
		/// </summary>
		private const int MAX_PREVIEW_HEIGHT = 1080;

		/// <summary>
		/// Conversion from device rotation to DNG orientation
		/// </summary>
		private static readonly SparseIntArray DNG_ORIENTATION = new SparseIntArray();
		static Sample_Single()
		{
			DNG_ORIENTATION.append(0, ExifInterface.ORIENTATION_NORMAL);
			DNG_ORIENTATION.append(90, ExifInterface.ORIENTATION_ROTATE_90);
			DNG_ORIENTATION.append(180, ExifInterface.ORIENTATION_ROTATE_180);
			DNG_ORIENTATION.append(270, ExifInterface.ORIENTATION_ROTATE_270);
		}

		public virtual void onCameraSettingUpdated(int key, int value)
		{
			switch (key)
			{
				case SettingItem.SETTING_TYPE_REQUEST_KEY:
					if (State == CAMERA_STATE.PREVIEW)
					{
						startPreview();
					}
					break;
				case SettingItem.SETTING_TYPE_CAMERA_FACING:
					if (mLensFacing != value)
					{
						mLensFacing = value;
						closeCamera();
						State = CAMERA_STATE.IDLE;
						openCamera(mLensFacing);
					}
					break;
				case SettingItem.SETTING_TYPE_IMAGE_FORMAT:
					if (mImageFormat != value)
					{
						mImageFormat = value;
					}
					break;
			}
		}

		private enum CAMERA_STATE
		{
			IDLE,
			PREVIEW,
			WAIT_AF,
			WAIT_AE,
			TAKE_PICTURE,
			CLOSING
		}

		private CAMERA_STATE State
		{
			set
			{
				lock (this)
				{
					mState = value;
				}
			}
			get
			{
				return mState;
			}
		}


		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback"/> for <seealso cref="com.samsung.android.sdk.camera.SCameraCaptureSession#setRepeatingRequest(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)"/>
		/// </summary>
		private SCameraCaptureSession.CaptureCallback mSessionCaptureCallback = new CaptureCallbackAnonymousInnerClassHelper();

		private class CaptureCallbackAnonymousInnerClassHelper : SCameraCaptureSession.CaptureCallback
		{
			public CaptureCallbackAnonymousInnerClassHelper()
			{
			}


			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				// Remove comment, if you want to check request/result from console log.
				// dumpCaptureResultToLog(result);
				// dumpCaptureRequestToLog(request);

				// Depends on the current state and capture result, app will take next action.
				switch (outerInstance.State)
				{

					case IDLE:
					case TAKE_PICTURE:
					case CLOSING:
						// do nothing
						break;
					case PREVIEW:
						if (result.get(SCaptureResult.STATISTICS_FACES) != null)
						{
							outerInstance.processFace(result.get(SCaptureResult.STATISTICS_FACES), result.get(SCaptureResult.SCALER_CROP_REGION));
						}
						break;

					// If AF is triggered and AF_STATE indicates AF process is finished, app will trigger AE pre-capture.
					case WAIT_AF:
					{
						if (outerInstance.isAFTriggered)
						{
							int afState = result.get(SCaptureResult.CONTROL_AF_STATE);
							// Check if AF is finished.
							if (SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState || SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState)
							{

								// If AE mode is off or device is legacy device then skip AE pre-capture.
								if (result.get(SCaptureResult.CONTROL_AE_MODE) != SCaptureResult.CONTROL_AE_MODE_OFF && outerInstance.mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY)
								{
									outerInstance.triggerAE();
								}
								else
								{
									outerInstance.takePicture();
								}
								outerInstance.isAFTriggered = false;
							}
						}
						break;
					}

					// If AE is triggered and AE_STATE indicates AE pre-capture process is finished, app will take a picture.
					case WAIT_AE:
					{
						if (outerInstance.isAETriggered)
						{
							int? aeState = result.get(SCaptureResult.CONTROL_AE_STATE);
							if (null == aeState || SCaptureResult.CONTROL_AE_STATE_CONVERGED == aeState || SCaptureResult.CONTROL_AE_STATE_FLASH_REQUIRED == aeState || SCaptureResult.CONTROL_AE_STATE_LOCKED == aeState)
							{ // Legacy device might have null AE_STATE. However, this should not be happened as we skip triggerAE() for legacy device
								outerInstance.takePicture();
								outerInstance.isAETriggered = false;
							}
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// A <seealso cref="android.media.ImageReader.OnImageAvailableListener"/> for still capture.
		/// </summary>
		private ImageReader.OnImageAvailableListener mImageCallback = new OnImageAvailableListenerAnonymousInnerClassHelper();

		private class OnImageAvailableListenerAnonymousInnerClassHelper : ImageReader.OnImageAvailableListener
		{
			public OnImageAvailableListenerAnonymousInnerClassHelper()
			{
			}


			public override void onImageAvailable(ImageReader reader)
			{
				if (outerInstance.mImageFormat == ImageFormat.JPEG)
				{
					outerInstance.mImageSaver.save(reader.acquireNextImage(), outerInstance.createFileName() + ".jpg");
				}
				else
				{
					outerInstance.mImageSaver.save(reader.acquireNextImage(), outerInstance.createFileName() + ".dng");
				}
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_single;
		}

		public override void onResume()
		{
			base.onResume();

			State = CAMERA_STATE.IDLE;

			startBackgroundThread();

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

			mCaptureResultQueue = new LinkedBlockingQueue<SCaptureResult>();

			OrientationListener = true;
			createUI();
			checkRequiredFeatures();
			openCamera(mLensFacing);
		}

		public override void onPause()
		{
			State = CAMERA_STATE.CLOSING;

			if (mSettingDialog != null)
			{
				mSettingDialog.dismiss();
				mSettingDialog = null;
			}

			OrientationListener = false;

			stopBackgroundThread();
			closeCamera();

			mSCamera = null;
			base.onPause();
		}

		/// <summary>
		/// Prepares an UI, like button, dialog, etc.
		/// </summary>
		private void createUI()
		{
			mSettingDialog = new SettingDialog(this);
			mSettingDialog.OnCaptureRequestUpdatedListener = this;

			findViewById(R.id.picture).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			findViewById(R.id.setting).setOnClickListener(new OnClickListenerAnonymousInnerClassHelper2(this));

			findViewById(R.id.info).OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);

			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);
			mFaceRectView = (FaceRectView) findViewById(R.id.face);

			// Set SurfaceTextureListener that handle life cycle of TextureView
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Single outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				// take picture is only works under preview state.
				if (outerInstance.State == CAMERA_STATE.PREVIEW)
				{

					// No AF lock is required for AF modes OFF/EDOF.
					if (outerInstance.mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) != SCaptureRequest.CONTROL_AF_MODE_OFF && outerInstance.mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) != SCaptureRequest.CONTROL_AF_MODE_EDOF)
					{
						outerInstance.lockAF();

					// No AE pre-capture is required for AE mode OFF or device is LEGACY.
					}
					else if (outerInstance.mPreviewBuilder.get(SCaptureRequest.CONTROL_AE_MODE) != SCaptureRequest.CONTROL_AE_MODE_OFF && outerInstance.mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY)
					{
						outerInstance.triggerAE();

					// If AE/AF is skipped, run still capture directly.
					}
					else
					{
						outerInstance.takePicture();
					}
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly Sample_Single outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (outerInstance.State == CAMERA_STATE.PREVIEW)
				{
					outerInstance.mSettingDialog.show();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly Sample_Single outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.showInformationDialog();
			}
		}

		private class SurfaceTextureListenerAnonymousInnerClassHelper : TextureView.SurfaceTextureListener
		{
			private readonly Sample_Single outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
			{
				// "onSurfaceTextureAvailable" is called, which means that SCameraCaptureSession is not created.
				// We need to configure transform for TextureView and crate SCameraCaptureSession.
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
        
					if (mJpegReader != null)
					{
						mJpegReader.close();
						mJpegReader = null;
					}
        
					if (mRawReader != null)
					{
						mRawReader.close();
						mRawReader = null;
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

		private void checkRequiredFeatures()
		{
			try
			{
				// Find available lens facing value for this device
				ISet<int?> lensFacings = new HashSet<int?>();
				foreach (string id in mSCamera.SCameraManager.CameraIdList)
				{
					SCameraCharacteristics cameraCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(id);
					lensFacings.Add(cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING));
				}
				mLensFacingList = new List<int?>(lensFacings);

				mLensFacing = mLensFacingList[mLensFacingList.Count - 1].Value;

			}
			catch (CameraAccessException e)
			{
				showAlertDialog("Cannot access the camera.", true);
				Log.e(TAG, "Cannot access the camera.", e);
			}
		}

		/// <summary>
		/// Opens a <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private void openCamera(int facing)
		{
			lock (this)
			{
				try
				{
					if (!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS))
					{
						showAlertDialog("Time out waiting to lock camera opening.", true);
					}
        
					mSCameraManager = mSCamera.SCameraManager;
        
					mCameraId = null;
        
					// Find camera device that facing to given facing parameter.
					foreach (string id in mSCamera.SCameraManager.CameraIdList)
					{
						SCameraCharacteristics cameraCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(id);
						if (cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING) == facing)
						{
							mCameraId = id;
							break;
						}
					}
        
					if (mCameraId == null)
					{
						showAlertDialog("No camera exist with given facing: " + facing, true);
						return;
					}
        
					// acquires camera characteristics
					mCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(mCameraId);
        
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
        
					// Acquires supported size for JPEG format
					Size[] jpegSizeList = null;
					jpegSizeList = streamConfigurationMap.getOutputSizes(ImageFormat.JPEG);
					if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && 0 == jpegSizeList.Length)
					{
						// If device has 'SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BURST_CAPTURE' getOutputSizes can return zero size list
						// for a format value in getOutputFormats.
						jpegSizeList = streamConfigurationMap.getHighResolutionOutputSizes(ImageFormat.JPEG);
					}
					Size jpegSize = jpegSizeList[0];
        
					// Configures an ImageReader
					mJpegReader = ImageReader.newInstance(jpegSize.Width, jpegSize.Height, ImageFormat.JPEG, 1);
					mJpegReader.setOnImageAvailableListener(mImageCallback, mImageSavingHandler);
        
					if (contains(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES), SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_RAW))
					{
						Size[] rawSizeList = streamConfigurationMap.getOutputSizes(ImageFormat.RAW_SENSOR);
						if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && 0 == rawSizeList.Length)
						{
							rawSizeList = streamConfigurationMap.getHighResolutionOutputSizes(ImageFormat.RAW_SENSOR);
						}
						Size rawSize = rawSizeList[0];
        
						mRawReader = ImageReader.newInstance(rawSize.Width, rawSize.Height, ImageFormat.RAW_SENSOR, 1);
						mRawReader.setOnImageAvailableListener(mImageCallback, mImageSavingHandler);
        
						mImageFormatList = Arrays.asList(ImageFormat.JPEG, ImageFormat.RAW_SENSOR);
					}
					else
					{
						if (mRawReader != null)
						{
							mRawReader.close();
							mRawReader = null;
						}
						mImageFormatList = Arrays.asList(ImageFormat.JPEG);
					}
					mImageFormat = ImageFormat.JPEG;
        
					// Set the aspect ratio to TextureView
					int orientation = Resources.Configuration.orientation;
					if (orientation == Configuration.ORIENTATION_LANDSCAPE)
					{
						mTextureView.setAspectRatio(mPreviewSize.Width, mPreviewSize.Height);
						mFaceRectView.setAspectRatio(mPreviewSize.Width, mPreviewSize.Height);
					}
					else
					{
						mTextureView.setAspectRatio(mPreviewSize.Height, mPreviewSize.Width);
						mFaceRectView.setAspectRatio(mPreviewSize.Height, mPreviewSize.Width);
					}
        
					// calculate transform matrix for face rect view
					configureFaceRectTransform();
        
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
			private readonly Sample_Single outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onOpened(SCameraDevice sCameraDevice)
			{
				outerInstance.mCameraOpenCloseLock.release();
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.mSettingDialog.dismiss();
				outerInstance.mSCameraDevice = sCameraDevice;
				outerInstance.createPreviewSession();
			}

			public override void onDisconnected(SCameraDevice sCameraDevice)
			{
				outerInstance.mCameraOpenCloseLock.release();
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("Camera disconnected.", true);
			}

			public override void onError(SCameraDevice sCameraDevice, int i)
			{
				outerInstance.mCameraOpenCloseLock.release();
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("Error while camera open.", true);
			}
		}

		/// <summary>
		/// Create a <seealso cref="com.samsung.android.sdk.camera.SCameraCaptureSession"/> for preview.
		/// </summary>
		private void createPreviewSession()
		{
			lock (this)
			{
        
				if (null == mSCamera || null == mSCameraDevice || null == mSCameraManager || null == mPreviewSize || !mTextureView.Available)
				{
					return;
				}
        
				try
				{
					SurfaceTexture texture = mTextureView.SurfaceTexture;
        
					// Set default buffer size to camera preview size.
					texture.setDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);
        
					Surface surface = new Surface(texture);
        
					// Creates SCaptureRequest.Builder for preview with output target.
					mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
					mPreviewBuilder.addTarget(surface);
        
					// Creates SCaptureRequest.Builder for still capture with output target.
					mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
        
					// Setup SettingDialog. SettingDialog will add setting item depends on camera characteristics.
					// and updates builders as setting value changes.
					runOnUiThread(() =>
					{
						mSettingDialog.configure(mCharacteristics, mLensFacing, mLensFacingList, mImageFormat, mImageFormatList, mPreviewBuilder, mCaptureBuilder);
					});
        
					// Creates a SCameraCaptureSession here.
					IList<Surface> outputSurface = new List<Surface>();
					outputSurface.Add(surface);
					outputSurface.Add(mJpegReader.Surface);
					if (mRawReader != null)
					{
						outputSurface.Add(mRawReader.Surface);
					}
        
					mSCameraDevice.createCaptureSession(outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
				}
				catch (CameraAccessException)
				{
					showAlertDialog("Fail to create camera capture session.", true);
				}
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_Single outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConfigured(SCameraCaptureSession sCameraCaptureSession)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.mSCameraSession = sCameraCaptureSession;
				outerInstance.startPreview();
			}

			public override void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("Fail to create camera capture session.", true);
				outerInstance.State = CAMERA_STATE.IDLE;
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
					mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
					State = CAMERA_STATE.PREVIEW;
				}
				catch (CameraAccessException)
				{
					showAlertDialog("Fail to start preview.", true);
				}
			}
		}

		/// <summary>
		/// Starts AF process by triggering <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER_START"/>.
		/// </summary>
		private void lockAF()
		{
			try
			{
				State = CAMERA_STATE.WAIT_AF;
				isAFTriggered = false;

				// Set AF trigger to SCaptureRequest.Builder
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_START);

				// App should send AF triggered request for only a single capture.
				mSCameraSession.capture(mPreviewBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper2(this), mBackgroundHandler);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to trigger AF", true);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper2 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_Single outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper2(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				outerInstance.isAFTriggered = true;
			}
		}

		/// <summary>
		/// Unlock AF.
		/// </summary>
		private void unlockAF()
		{
			// If we send TRIGGER_CANCEL. Lens move to its default position. This results in bad user experience.
			if (mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) == SCaptureRequest.CONTROL_AF_MODE_AUTO || mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) == SCaptureRequest.CONTROL_AF_MODE_MACRO)
			{
				State = CAMERA_STATE.PREVIEW;
				return;
			}

			// Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
			try
			{
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
				mSCameraSession.capture(mPreviewBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper3(this), mBackgroundHandler);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to cancel AF", false);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper3 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_Single outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper3(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.State = CAMERA_STATE.PREVIEW;
			}
		}

		/// <summary>
		/// Starts AE pre-capture
		/// </summary>
		private void triggerAE()
		{
			try
			{
				State = CAMERA_STATE.WAIT_AE;
				isAETriggered = false;

				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER, SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER_START);

				// App should send AE triggered request for only a single capture.
				mSCameraSession.capture(mPreviewBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper4(this), mBackgroundHandler);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER, SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER_IDLE);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to trigger AE", true);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper4 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_Single outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper4(Sample_Single outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				outerInstance.isAETriggered = true;
			}
		}

		/// <summary>
		/// Take picture.
		/// </summary>
		private void takePicture()
		{
			if (State == CAMERA_STATE.CLOSING)
			{
				return;
			}

			try
			{
				// Sets orientation
				mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, JpegOrientation);

				if (mImageFormat == ImageFormat.JPEG)
				{
					mCaptureBuilder.addTarget(mJpegReader.Surface);
				}
				else
				{
					mCaptureBuilder.addTarget(mRawReader.Surface);
				}

				mSCameraSession.capture(mCaptureBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper5(this), mBackgroundHandler);

				if (mImageFormat == ImageFormat.JPEG)
				{
					mCaptureBuilder.removeTarget(mJpegReader.Surface);
				}
				else
				{
					mCaptureBuilder.removeTarget(mRawReader.Surface);
				}

				State = CAMERA_STATE.TAKE_PICTURE;
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to start preview.", true);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper5 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_Single outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper5(Sample_Single outerInstance)
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

				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.unlockAF();
			}

			public override void onCaptureFailed(SCameraCaptureSession session, SCaptureRequest request, SCaptureFailure failure)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("JPEG Capture failed.", false);
				outerInstance.unlockAF();
			}
		}

		/// <summary>
		/// Process face information to draw face UI
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void processFace(final android.hardware.camera2.params.Face[] faces, final android.graphics.Rect zoomRect)
		private void processFace(Face[] faces, Rect zoomRect)
		{
			runOnUiThread(() =>
			{
				mFaceRectView.setFaceRect(faces, zoomRect);
				mFaceRectView.invalidate();
			});
		}

		/// <summary>
		/// Returns required orientation that the jpeg picture needs to be rotated to be displayed upright.
		/// </summary>
		private int JpegOrientation
		{
			get
			{
				int degrees = mLastOrientation;
    
				if (mCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_FRONT)
				{
					degrees = -degrees;
				}
    
				return (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) + degrees + 360) % 360;
			}
		}

		/// <summary>
		/// Enable/Disable an orientation listener.
		/// </summary>
		private bool OrientationListener
		{
			set
			{
				if (mOrientationListener == null)
				{
    
					mOrientationListener = new OrientationEventListenerAnonymousInnerClassHelper(this);
				}
    
				if (value)
				{
					mOrientationListener.enable();
				}
				else
				{
					mOrientationListener.disable();
				}
			}
		}

		private class OrientationEventListenerAnonymousInnerClassHelper : OrientationEventListener
		{
			private readonly Sample_Single outerInstance;

			public OrientationEventListenerAnonymousInnerClassHelper(Sample_Single outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onOrientationChanged(int orientation)
			{
				if (orientation == ORIENTATION_UNKNOWN)
				{
					return;
				}
				outerInstance.mLastOrientation = (orientation + 45) / 90 * 90;
			}
		}

		/// <summary>
		/// Configure required transform for <seealso cref="android.hardware.camera2.params.Face"/> to be displayed correctly in the screen.
		/// </summary>
		private void configureFaceRectTransform()
		{
			int orientation = Resources.Configuration.orientation;
			int degrees = WindowManager.DefaultDisplay.Rotation * 90;

			int result;
			if (mCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_FRONT)
			{
				result = (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) + degrees) % 360;
				result = (360 - result) % 360; // compensate the mirror
			}
			else
			{
				result = (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) - degrees + 360) % 360;
			}
			mFaceRectView.setTransform(mPreviewSize, mCharacteristics.get(SCameraCharacteristics.LENS_FACING), result, orientation);
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
		/// Starts back ground thread that callback from camera will posted.
		/// </summary>
		private void startBackgroundThread()
		{
			mBackgroundHandlerThread = new HandlerThread("Background Thread");
			mBackgroundHandlerThread.start();
			mBackgroundHandler = new Handler(mBackgroundHandlerThread.Looper);

			mImageSavingHandlerThread = new HandlerThread("Saving Thread");
			mImageSavingHandlerThread.start();
			mImageSavingHandler = new Handler(mImageSavingHandlerThread.Looper);
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

			if (mImageSavingHandlerThread != null)
			{
				mImageSavingHandlerThread.quitSafely();
				try
				{
					mImageSavingHandlerThread.join();
					mImageSavingHandlerThread = null;
					mImageSavingHandler = null;
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
			private readonly Sample_Single outerInstance;

			public ImageSaver(Sample_Single outerInstance)
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

				if (image.Format == ImageFormat.RAW_SENSOR)
				{
					SCaptureResult result = null;
					try
					{
						result = outerInstance.mCaptureResultQueue.take();
					}
					catch (InterruptedException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}

					try
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.samsung.android.sdk.camera.SDngCreator dngCreator = new com.samsung.android.sdk.camera.SDngCreator(mCharacteristics, result);
						SDngCreator dngCreator = new SDngCreator(outerInstance.mCharacteristics, result);
						dngCreator.Orientation = DNG_ORIENTATION.get(outerInstance.JpegOrientation);

						(new Handler(Looper.myLooper())).post(() =>
						{
							ByteBuffer buffer = image.Planes[0].Buffer;
							sbyte[] bytes = new sbyte[buffer.remaining()];
							buffer.get(bytes);
							System.IO.FileStream output = null;
							try
							{
								output = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
								dngCreator.writeImage(output, image);
							}
							catch (IOException e)
							{
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}
							finally
							{
								image.close();
								dngCreator.close();
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
					catch (System.ArgumentException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						outerInstance.showAlertDialog("Fail to save DNG file.", false);
						image.close();
						return;
					}
				}
				else
				{
					(new Handler(Looper.myLooper())).post(() =>
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

						MediaScannerConnection.scanFile(outerInstance, new string[]{file.AbsolutePath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper2(this));

						runOnUiThread(() =>
						{
							Toast.makeTextuniquetempvar.show();
						});
					});
				}
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

			private class OnScanCompletedListenerAnonymousInnerClassHelper2 : MediaScannerConnection.OnScanCompletedListener
			{
				private readonly ImageSaver outerInstance;

				public OnScanCompletedListenerAnonymousInnerClassHelper2(ImageSaver outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void onScanCompleted(string path, Uri uri)
				{
					Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private boolean contains(final int[] array, final int key)
		private bool contains(int[] array, int key)
		{
			foreach (int i in array)
			{
				if (i == key)
				{
					return true;
				}
			}
			return false;
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
			private readonly Sample_Single outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Single outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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
		/// Shows the camera device information into dialog.
		/// </summary>
		private void showInformationDialog()
		{

			StringBuilder builder = new StringBuilder();

			builder.Append("<html><body><pre>");
			if (mCharacteristics != null)
			{
				builder.Append(string.Format("Camera Id: {0}\n", mCameraId));

				// Check supported hardware level.
				SparseArray<string> level = new SparseArray<string>();
				level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_FULL, "Full");
				level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LIMITED, "Limited");
				level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY, "Legacy");

				builder.Append(string.Format("Supported H/W Level: {0}\n", level.get(mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL))));

				// Available characteristics tag.
				builder.Append("\nCharacteristics [\n");
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCameraCharacteristics.Key<?> key : mCharacteristics.getKeys())
				foreach (SCameraCharacteristics.Key<?> key in mCharacteristics.Keys)
				{
					if (mCharacteristics.get(key) is int[])
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.ToString((int[])mCharacteristics.get(key))));
					}
					else if (mCharacteristics.get(key) is Range[])
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.deepToString((Range[])mCharacteristics.get(key))));
					}
					else if (mCharacteristics.get(key) is Size[])
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.deepToString((Size[]) mCharacteristics.get(key))));
					}
					else if (mCharacteristics.get(key) is float[])
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.ToString((float[])mCharacteristics.get(key))));
					}
					else if (mCharacteristics.get(key) is StreamConfigurationMap)
					{
						builder.Append(string.Format("\t{0} --> [\n", key.Name));
						{
							StreamConfigurationMap streamConfigurationMap = mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
							SparseArray<string> formatMap = new SparseArray<string>();
							formatMap.put(ImageFormat.JPEG, "JPEG");
							if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
							{
								formatMap.put(ImageFormat.PRIVATE, "PRIVATE");
								formatMap.put(ImageFormat.DEPTH16, "DEPTH16");
								formatMap.put(ImageFormat.DEPTH_POINT_CLOUD, "DEPTH_POINT_CLOUD");
							}
							formatMap.put(ImageFormat.NV16, "NV16");
							formatMap.put(ImageFormat.NV21, "NV21");
							formatMap.put(ImageFormat.RAW10, "RAW10");
							formatMap.put(ImageFormat.RAW_SENSOR, "RAW_SENSOR");
							formatMap.put(ImageFormat.RGB_565, "RGB_565");
							formatMap.put(ImageFormat.UNKNOWN, "UNKNOWN");
							formatMap.put(ImageFormat.YUV_420_888, "420_888");
							formatMap.put(ImageFormat.YUY2, "YUY2");
							formatMap.put(ImageFormat.YV12, "YV12");
							formatMap.put(PixelFormat.RGBA_8888, "RGBA_8888");

							foreach (int format in streamConfigurationMap.OutputFormats)
							{
								builder.Append(string.Format("\t\t{0}(0x{1:x}) --> {2}\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getOutputSizes(format))));
								if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
								{
									builder.Append(string.Format("\t\tHigh Resolution {0}(0x{1:x}) --> {2}\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getHighResolutionOutputSizes(format))));
								}
							}

							builder.Append(string.Format("\n\t\tHigh speed video fps --> {0}\n", Arrays.deepToString(streamConfigurationMap.HighSpeedVideoFpsRanges)));
							builder.Append(string.Format("\t\tHigh speed video size --> {0}\n", Arrays.deepToString(streamConfigurationMap.HighSpeedVideoSizes)));

							if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
							{
								builder.Append(string.Format("\n\t\tInput formats [\n"));
								foreach (int format in streamConfigurationMap.InputFormats)
								{
									builder.Append(string.Format("\t\t\t{0}(0x{1:x}) --> {2}\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getInputSizes(format))));
								}
							}

							builder.Append(string.Format("\t\t]\n"));

						}
						builder.Append("\t]\n");
					}
					else if (mCharacteristics.get(key) is BlackLevelPattern)
					{
						BlackLevelPattern pattern = mCharacteristics.get(SCameraCharacteristics.SENSOR_BLACK_LEVEL_PATTERN);
						int[] patternArray = new int[BlackLevelPattern.COUNT];
						pattern.copyTo(patternArray, 0);

						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.ToString(patternArray)));
					}
					else if (mCharacteristics.get(key) is bool[])
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, Arrays.ToString((bool[])mCharacteristics.get(key))));
					}
					else
					{
						builder.Append(string.Format("\t{0} --> {1}\n", key.Name, mCharacteristics.get(key).ToString()));
					}
				}
				builder.Append("]\n");

				// Available characteristics tag.
				builder.Append("\nAvailable characteristics keys [\n");
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCameraCharacteristics.Key<?> key : mCharacteristics.getKeys())
				foreach (SCameraCharacteristics.Key<?> key in mCharacteristics.Keys)
				{
					builder.Append(string.Format("\t{0}\n", key.Name));
				}
				builder.Append("]\n");

				// Available request tag.
				builder.Append("\nAvailable request keys [\n");
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCaptureRequest.Key<?> key : mCharacteristics.getAvailableCaptureRequestKeys())
				foreach (SCaptureRequest.Key<?> key in mCharacteristics.AvailableCaptureRequestKeys)
				{
					builder.Append(string.Format("\t{0}\n", key.Name));
				}
				builder.Append("]\n");

				// Available result tag.
				builder.Append("\nAvailable result keys [\n");
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCaptureResult.Key<?> key : mCharacteristics.getAvailableCaptureResultKeys())
				foreach (SCaptureResult.Key<?> key in mCharacteristics.AvailableCaptureResultKeys)
				{
					builder.Append(string.Format("\t{0}\n", key.Name));
				}
				builder.Append("]\n");

				// Available capability.
				builder.Append("\nAvailable capabilities [\n");
				SparseArray<string> capabilityName = new SparseArray<string>();
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BACKWARD_COMPATIBLE, "BACKWARD_COMPATIBLE");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_MANUAL_SENSOR, "MANUAL_SENSOR");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_MANUAL_POST_PROCESSING, "MANUAL_POST_PROCESSING");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_RAW, "RAW");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_READ_SENSOR_SETTINGS, "READ_SENSOR_SETTINGS");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BURST_CAPTURE, "BURST_CAPTURE");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_DEPTH_OUTPUT, "DEPTH_OUTPUT");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_PRIVATE_REPROCESSING, "PRIVATE_REPROCESSING");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_YUV_REPROCESSING, "YUV_REPROCESSING");
				capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_CONSTRAINED_HIGH_SPEED_VIDEO, "HIGH_SPEED_VIDEO");

				foreach (int value in mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES))
				{
					builder.Append(string.Format("\t{0}\n", capabilityName.get(value)));
				}
				builder.Append("]\n");

				{
					builder.Append("\nSamsung extend tags\n");

					// RT-HDR.
					if (mCharacteristics.Keys.contains(SCameraCharacteristics.LIVE_HDR_INFO_LEVEL_RANGE))
					{
						builder.Append(string.Format("\tRT-HDR: {0}\n", mCharacteristics.get(SCameraCharacteristics.LIVE_HDR_INFO_LEVEL_RANGE).ToString()));
					}
					else
					{
						builder.Append("\tRT-HDR: not available\n");
					}

					// Metering mode.
					builder.Append("\tAvailable Metering mode: [\n");
					if (mCharacteristics.Keys.contains(SCameraCharacteristics.METERING_AVAILABLE_MODES))
					{
						SparseArray<string> stringMap = new SparseArray<string>();
						stringMap.put(SCameraCharacteristics.METERING_MODE_CENTER, "Center");
						stringMap.put(SCameraCharacteristics.METERING_MODE_MATRIX, "Matrix");
						stringMap.put(SCameraCharacteristics.METERING_MODE_SPOT, "Spot");
						stringMap.put(SCameraCharacteristics.METERING_MODE_MANUAL, "Manual");

						foreach (int mode in mCharacteristics.get(SCameraCharacteristics.METERING_AVAILABLE_MODES))
						{
							builder.Append(string.Format("\t\t{0}\n", stringMap.get(mode)));
						}
					}
					else
					{
						builder.Append("\t\tnot available\n");
					}
					builder.Append("\t]\n");

					// PAF.
					builder.Append("\tPhase AF: ");
					if (mCharacteristics.Keys.contains(SCameraCharacteristics.PHASE_AF_INFO_AVAILABLE))
					{
						builder.Append(mCharacteristics.get(SCameraCharacteristics.PHASE_AF_INFO_AVAILABLE)).Append("\n");
					}
					else
					{
						builder.Append("not available\n");
					}

					// Stabilization operation mode.
					builder.Append("\tStabilization modes: ");
					if (mCharacteristics.Keys.contains(SCameraCharacteristics.LENS_INFO_AVAILABLE_OPTICAL_STABILIZATION_OPERATION_MODE))
					{
						builder.Append(Arrays.ToString(mCharacteristics.get(SCameraCharacteristics.LENS_INFO_AVAILABLE_OPTICAL_STABILIZATION_OPERATION_MODE))).Append("\n");
					}
					else
					{
						builder.Append("not available\n");
					}
				}
			}

			builder.Append("</pre></body></html>");

			View dialogView = LayoutInflater.inflate(R.layout.information_dialog_single, null);
			((WebView)dialogView.findViewById(R.id.information)).loadDataWithBaseURL(null, builder.ToString(), "text/html", "utf-8", null);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.app.AlertDialog.Builder dialog = new android.app.AlertDialog.Builder(this);
			AlertDialog.Builder dialog = new AlertDialog.Builder(this);
			dialog.setTitle("Information").setView(dialogView).setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper2(this, dialog));

			runOnUiThread(() =>
			{
				dialog.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly Sample_Single outerInstance;

			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper2(Sample_Single outerInstance, AlertDialog.Builder dialog)
			{
				this.outerInstance = outerInstance;
				this.dialog = dialog;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				dialog.dismiss();
			}
		}

		/// <summary>
		/// Dump <seealso cref="com.samsung.android.sdk.camera.SCaptureResult"/> to console log.
		/// </summary>
		private void dumpCaptureResultToLog(SCaptureResult result)
		{

			Log.v(TAG, "Dump of SCaptureResult Frame#" + result.FrameNumber + " Seq.#" + result.SequenceId);
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCaptureResult.Key<?> key : result.getKeys())
			foreach (SCaptureResult.Key<?> key in result.Keys)
			{
				if (result.get(key) is int[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((int[]) result.get(key)));
				}
				else if (result.get(key) is float[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((float[])result.get(key)));
				}
				else if (result.get(key) is long[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((long[])result.get(key)));
				}
				else if (result.get(key) is MeteringRectangle[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((MeteringRectangle[]) result.get(key)));
				}
				else if (result.get(key) is Rational[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Rational[]) result.get(key)));
				}
				else if (result.get(key) is Face[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Face[]) result.get(key)));
				}
				else if (result.get(key) is Point[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Point[]) result.get(key)));
				}
				else if (result.get(key) is Pair)
				{
					Pair value = (Pair)result.get(key);
					Log.v(TAG, key.Name + ": (" + value.first + ", " + value.second + ")");
				}
				else
				{
					Log.v(TAG, key.Name + ": " + result.get(key));
				}
			}
		}

		/// <summary>
		/// Dump <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest"/> to console log.
		/// </summary>
		private void dumpCaptureRequestToLog(SCaptureRequest request)
		{

			Log.v(TAG, "Dump of SCaptureRequest");
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: for(com.samsung.android.sdk.camera.SCaptureRequest.Key<?> key : request.getKeys())
			foreach (SCaptureRequest.Key<?> key in request.Keys)
			{
				if (request.get(key) is int[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((int[]) request.get(key)));
				}
				else if (request.get(key) is float[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((float[]) request.get(key)));
				}
				else if (request.get(key) is long[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.ToString((long[]) request.get(key)));
				}
				else if (request.get(key) is MeteringRectangle[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((MeteringRectangle[]) request.get(key)));
				}
				else if (request.get(key) is Rational[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Rational[]) request.get(key)));
				}
				else if (request.get(key) is Face[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Face[]) request.get(key)));
				}
				else if (request.get(key) is Point[])
				{
					Log.v(TAG, key.Name + ": " + Arrays.deepToString((Point[]) request.get(key)));
				}
				else
				{
					Log.v(TAG, key.Name + ": " + request.get(key));
				}
			}
		}
	}
}