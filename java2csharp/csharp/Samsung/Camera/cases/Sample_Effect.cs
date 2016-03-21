using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases
{

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
	using MediaRecorder = android.media.MediaRecorder;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using HandlerThread = android.os.HandlerThread;
	using DateFormat = android.text.format.DateFormat;
	using Log = android.util.Log;
	using Range = android.util.Range;
	using Size = android.util.Size;
	using TypedValue = android.util.TypedValue;
	using Gravity = android.view.Gravity;
	using OrientationEventListener = android.view.OrientationEventListener;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using LinearLayout = android.widget.LinearLayout;
	using SeekBar = android.widget.SeekBar;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using SCameraFilter = com.samsung.android.sdk.camera.filter.SCameraFilter;
	using SCameraFilterInfo = com.samsung.android.sdk.camera.filter.SCameraFilterInfo;
	using SCameraFilterManager = com.samsung.android.sdk.camera.filter.SCameraFilterManager;
	using SCameraEffectProcessor = com.samsung.android.sdk.camera.processor.SCameraEffectProcessor;
	using SCameraProcessorManager = com.samsung.android.sdk.camera.processor.SCameraProcessorManager;
	using SCameraProcessorParameter = com.samsung.android.sdk.camera.processor.SCameraProcessorParameter;
	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;


	public class Sample_Effect : Activity, AdapterView.OnItemSelectedListener
	{
		private bool InstanceFieldsInitialized = false;

		public Sample_Effect()
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
		private const string TAG = "Sample_Effect";

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;

		/// <summary>
		/// A reference to the opened <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private SCameraDevice mSCameraDevice;
		private SCameraCaptureSession mSCameraSession;
		private SCameraCharacteristics mCharacteristics;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest.Builder"/> for the camera preview and recording
		/// </summary>
		private SCaptureRequest.Builder mPreviewBuilder;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest.Builder"/> for the still capture
		/// </summary>
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
		/// An <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView"/> for camera preview.
		/// </summary>
		private AutoFitTextureView mTextureView = null;

		private ImageReader mImageReader;
		private ImageSaver mImageSaver;

		/// <summary>
		/// A camera related listener/callback will be posted in this handler.
		/// </summary>
		private HandlerThread mBackgroundHandlerThread;
		private Handler mBackgroundHandler;

		/// <summary>
		/// An orientation listener for jpeg orientation
		/// </summary>
		private OrientationEventListener mOrientationListener;
		private int mLastOrientation = 0;

		/// <summary>
		/// A <seealso cref="Semaphore"/> to prevent the app from exiting before closing the camera.
		/// </summary>
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
		private const int MAX_PREVIEW_WIDTH = 1280;
		/// <summary>
		/// Maximum preview height app will use.
		/// </summary>
		private const int MAX_PREVIEW_HEIGHT = 720;

		/// <summary>
		/// Maximum preview FPS app will use.
		/// </summary>
		private const int MAX_PREVIEW_FPS = 24;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.processor.SCameraEffectProcessor"/> for applies the filter effect on image data
		/// </summary>
		private SCameraEffectProcessor mProcessor;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.filter.SCameraFilterManager"/> for creating and retrieving available filters
		/// </summary>
		private static SCameraFilterManager mSCameraFilterManager;

		/// <summary>
		/// The <seealso cref="android.media.MediaRecorder"/> for recording audio and video.
		/// </summary>
		private MediaRecorder mMediaRecorder;

		/// <summary>
		/// The <seealso cref="android.util.Size"/> of video recording.
		/// </summary>
		private Size mVideoSize;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.filter.SCameraFilter"/> for contains information of filter.
		/// </summary>
		private SCameraFilter mFilter;

		/// <summary>
		/// list of retrieving available filters.
		/// </summary>
		private IList<SCameraFilterInfo> mFilterInfoList;

		/// <summary>
		/// list of the information available with the filter of the parameter.
		/// </summary>
		private IDictionary<string, IList<FilterParameterInfo>> mFilterParameterInfoList = new Dictionary<string, IList<FilterParameterInfo>>();


		/// <summary>
		/// Button to capture
		/// </summary>
		private Button mPictureButton;

		/// <summary>
		/// Button to record video
		/// </summary>
		private Button mRecordButton;

		private ArrayAdapter<string> mEffectAdapter = null;

		private IList<View> mParameterViewList = new List<View>();

		private long mRecordingStartTime;

		private enum CAMERA_STATE
		{
			IDLE,
			PREVIEW,
			WAIT_AF,
			WAIT_AE,
			TAKE_PICTURE,
			RECORD_VIDEO,
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


		private class FilterParameterInfo
		{
			internal readonly string mParameterName;
			internal readonly Range<int?> mParameterRange;

			internal FilterParameterInfo(string name, Range<int?> range)
			{
				mParameterName = name;
				mParameterRange = range;
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
				// Depends on the current state and capture result, app will take next action.
				switch (outerInstance.State)
				{

					case IDLE:
					case PREVIEW:
					case TAKE_PICTURE:
					case CLOSING:
						// do nothing
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

								// If device is legacy device then skip AE pre-capture.
								if (outerInstance.mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY)
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
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				Image image = reader.acquireNextImage();
				// process effect using image data, must be called after {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#startStreamProcessing()}, but before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#stopStreamProcessing()}.
				outerInstance.mProcessor.requestProcess(image);
				// after using the image object, should be called the close().
				image.close();
			}
		}

		private SCameraEffectProcessor.EventCallback mProcessorCallback = new EventCallbackAnonymousInnerClassHelper();

		private class EventCallbackAnonymousInnerClassHelper : SCameraEffectProcessor.EventCallback
		{
			public EventCallbackAnonymousInnerClassHelper()
			{
			}


			public override void onProcessCompleted(Image image)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.mImageSaver.save(image, outerInstance.createFileName() + "_effect.jpg");
				outerInstance.unlockAF();
			}

			public override void onError(int i)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("Fail to create result: " + i, false);
				outerInstance.unlockAF();
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_effect;
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

			OrientationListener = true;

			if (!checkRequiredFeatures())
			{
				return;
			}
			createProcessor();
			createUI();
			openCamera();
		}

		public override void onPause()
		{

			if (State == CAMERA_STATE.RECORD_VIDEO)
			{
				stopRecordVideo(true);
			}

			State = CAMERA_STATE.CLOSING;

			OrientationListener = false;

			stopBackgroundThread();

			closeCamera();
			deinitProcessor();

			Spinner spinner = (Spinner) findViewById(R.id.effectlist);
			spinner.OnItemSelectedListener = null;

			base.onPause();
		}

		private bool checkRequiredFeatures()
		{
			try
			{
				mCameraId = null;

				// Find camera device that facing to given facing parameter.
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

				// acquires camera characteristics
				mCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(mCameraId);

				if (!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE))
				{
					showAlertDialog("Required AF mode is not supported.", true);
					return false;
				}

				if (!mSCamera.isFeatureEnabled(SCamera.SCAMERA_FILTER))
				{
					showAlertDialog("This device does not support SCamera Filter feature.", true);
					return false;
				}

				if (!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR))
				{
					showAlertDialog("This device does not support SCamera Processor feature.", true);
					return false;
				}

				SCameraProcessorManager processorManager = mSCamera.SCameraProcessorManager;
				if (!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_EFFECT))
				{
					showAlertDialog("This device does not support Effect Processor.", true);
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

		private void createProcessor()
		{
			SCameraProcessorManager processorManager = mSCamera.SCameraProcessorManager;

			// create an {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor}
			mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_EFFECT);

			// retrieving an {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager}
			if (mSCameraFilterManager == null)
			{
				mSCameraFilterManager = mSCamera.SCameraFilterManager;
			}

			// retrieving available filters
			mFilterInfoList = mSCameraFilterManager.AvailableFilters;

			IList<string> filterName = new List<string>();
			foreach (SCameraFilterInfo filterInfo in mFilterInfoList)
			{
				filterName.Add(filterInfo.Name);
			}

			mEffectAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_spinner_item, filterName);

			// Add filter parameter info, refer {@link com.samsung.android.sdk.camera.filter.SCameraFilter#setParameter(String, Number)}
			mFilterParameterInfoList["Beauty"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(0, 4)));
			mFilterParameterInfoList["Brightness"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(-100, 100)));
			mFilterParameterInfoList["Contrast"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(-100, 100)));
			mFilterParameterInfoList["Saturate"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(-100, 100)));
			mFilterParameterInfoList["Temperature"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(-100, 100)));
			mFilterParameterInfoList["Tint Control"] = Arrays.asList(new FilterParameterInfo("intensity", new Range<int?>(-100, 100)));
			mFilterParameterInfoList["Highlights and Shadows"] = Arrays.asList(new FilterParameterInfo("highlight", new Range<int?>(-100, 100)), new FilterParameterInfo("shadow", new Range<int?>(-100, 100)));
		}


		private void initProcessor()
		{

			SCameraProcessorParameter parameter = mProcessor.Parameters;

			parameter.set(SCameraEffectProcessor.STILL_INPUT_FORMAT, ImageFormat.JPEG);
			parameter.set(SCameraEffectProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
			parameter.set(SCameraEffectProcessor.STILL_SIZE, new Size(mImageReader.Width, mImageReader.Height));
			parameter.set(SCameraEffectProcessor.STREAM_SIZE, mPreviewSize);

			// changes the settings for this processor. must be called before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#initialize()}.
			mProcessor.Parameters = parameter;
			mProcessor.initialize();
			mProcessor.setEventCallback(mProcessorCallback, mBackgroundHandler);
		}

		private void deinitProcessor()
		{
			if (mProcessor != null)
			{
				mProcessor.deinitialize();
				mProcessor.close();
				mProcessor = null;
			}
		}

		/// <summary>
		/// Prepares an UI, like button, etc.
		/// </summary>
		private void createUI()
		{

			Spinner spinner = (Spinner) findViewById(R.id.effectlist);
			spinner.Adapter = mEffectAdapter;
			spinner.OnItemSelectedListener = this;

			mPictureButton = (Button) findViewById(R.id.picture);
			mPictureButton.Enabled = true;
			mPictureButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mRecordButton = (Button) findViewById(R.id.record);
			mRecordButton.Enabled = true;
			mRecordButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

			// Set SurfaceTextureListener that handle life cycle of TextureView
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Effect outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				// take picture is only works under preview state.
				if (outerInstance.State == CAMERA_STATE.PREVIEW)
				{
					outerInstance.lockAF();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly Sample_Effect outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (outerInstance.State == CAMERA_STATE.PREVIEW)
				{
					outerInstance.recordVideo();
				}
				else if (outerInstance.State == CAMERA_STATE.RECORD_VIDEO)
				{
					outerInstance.stopRecordVideo(false);
				}
			}
		}

		private class SurfaceTextureListenerAnonymousInnerClassHelper : TextureView.SurfaceTextureListener
		{
			private readonly Sample_Effect outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_Effect outerInstance)
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

		private void closeCamera()
		{
			try
			{
				mCameraOpenCloseLock.acquire();

				stopPreview();

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

				if (mImageReader != null)
				{
					mImageReader.close();
					mImageReader = null;
				}

				if (null != mMediaRecorder)
				{
					mMediaRecorder.release();
					mMediaRecorder = null;
				}

				mSCameraManager = null;
				mSCamera = null;
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

		/// <summary>
		/// Opens a <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private void openCamera()
		{
			try
			{
				if (!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS))
				{
					showAlertDialog("Time out waiting to lock camera opening.", true);
				}

				mSCameraManager = mSCamera.SCameraManager;

				SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);
				StreamConfigurationMap streamConfigurationMap = characteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

				// Acquires supported preview size list that supports YUV420_888.
				// Input Surface from EffectProcessor will have a format of ImageFormat.YUV420_888
				mPreviewSize = streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)[0];
				foreach (Size option in streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888))
				{
					// preview size must be supported by effect processor
					if (!contains(mProcessor.Parameters.get(SCameraEffectProcessor.STREAM_SIZE_LIST), option))
					{
						continue;
					}

					// Find maximum preview size that is not larger than MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT and closest to MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT.
					if (option.Width > MAX_PREVIEW_WIDTH || option.Height > MAX_PREVIEW_HEIGHT)
					{
						continue;
					}

					int areaCurrent = Math.Abs((mPreviewSize.Width * mPreviewSize.Height) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
					int areaNext = Math.Abs((option.Width * option.Height) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));

					if (areaCurrent > areaNext)
					{
						mPreviewSize = option;
					}
				}

				mVideoSize = streamConfigurationMap.getOutputSizes(typeof(MediaRecorder))[streamConfigurationMap.getOutputSizes(typeof(MediaRecorder)).length - 1];
				foreach (Size option in streamConfigurationMap.getOutputSizes(typeof(MediaRecorder)))
				{
					if (option.Width == option.Height * mPreviewSize.Width / mPreviewSize.Height && option.Width <= mPreviewSize.Width)
					{
						mVideoSize = option;
						break;
					}
				}

				mMediaRecorder = new MediaRecorder();

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
				mImageReader = ImageReader.newInstance(jpegSize.Width, jpegSize.Height, ImageFormat.JPEG, 1);
				mImageReader.setOnImageAvailableListener(mImageCallback, mBackgroundHandler);

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

				initProcessor();
				prepareMediaRecorder();

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
			catch (IOException e)
			{
				throw new Exception("Fail to prepare media recorder", e);
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraDevice.StateCallback
		{
			private readonly Sample_Effect outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onOpened(SCameraDevice sCameraDevice)
			{
				Log.v(TAG, "onOpened");
				outerInstance.mCameraOpenCloseLock.release();
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
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

				// set a surface of UI preview, must be called before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#initialize()}.
				mProcessor.OutputSurface = surface;

				// retrieving a surface of camera preview, this must be set to preview request.
				Surface cameraSurface = mProcessor.InputSurface;

				// Creates SCaptureRequest.Builder for preview and recording with output target.
				mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_RECORD);

				// {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} supports only 24fps.
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, Range.create(MAX_PREVIEW_FPS, MAX_PREVIEW_FPS));
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mPreviewBuilder.addTarget(cameraSurface);

				// Creates SCaptureRequest.Builder for still capture with output target.
				mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
				mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mCaptureBuilder.addTarget(mImageReader.Surface);

				// Creates a SCameraCaptureSession here.
				IList<Surface> outputSurface = Arrays.asList(cameraSurface, mImageReader.Surface);
				mSCameraDevice.createCaptureSession(outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);

			}
			catch (CameraAccessException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_Effect outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Effect outerInstance)
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
			}
		}

		/// <summary>
		/// Starts a preview.
		/// </summary>
		private void startPreview()
		{
			try
			{
				// Starts displaying the preview.
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
				State = CAMERA_STATE.PREVIEW;
				// must be called after setRepeatingRequest(include surface of camera preview).
				mProcessor.startStreamProcessing();
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to start preview.", true);
			}
		}

		/// <summary>
		/// Stop a preview.
		/// </summary>
		private void stopPreview()
		{
			try
			{
				if (mSCameraSession != null)
				{
					mSCameraSession.stopRepeating();
				}

				if (mProcessor != null && State == CAMERA_STATE.PREVIEW)
				{
					mProcessor.stopStreamProcessing();
				}
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to stop preview.", true);
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
				mRecordButton.Enabled = false;
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
			private readonly Sample_Effect outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper2(Sample_Effect outerInstance)
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
			try
			{
				// Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
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
			private readonly Sample_Effect outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper3(Sample_Effect outerInstance)
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
				runOnUiThread(() =>
				{
					outerInstance.mRecordButton.Enabled = true;
				});
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
			private readonly Sample_Effect outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper4(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				outerInstance.isAETriggered = true;
			}
		}

		/// <summary>
		/// Prepares the mediarecorder to begin recording.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void prepareMediaRecorder() throws java.io.IOException
		private void prepareMediaRecorder()
		{
			mMediaRecorder.AudioSource = MediaRecorder.AudioSource.MIC;
			mMediaRecorder.VideoSource = MediaRecorder.VideoSource.SURFACE;
			mMediaRecorder.OutputFormat = MediaRecorder.OutputFormat.MPEG_4;
			mMediaRecorder.OutputFile = (new File(getExternalFilesDir(null), "temp.mp4")).AbsolutePath;

			int bitrate = 384000;
			if (mVideoSize.Width * mVideoSize.Height >= 1920 * 1080)
			{
				bitrate = 14000000;
			}
			else if (mVideoSize.Width * mVideoSize.Height >= 1280 * 720)
			{
				bitrate = 9730000;
			}
			else if (mVideoSize.Width * mVideoSize.Height >= 640 * 480)
			{
				bitrate = 2500000;
			}
			else if (mVideoSize.Width * mVideoSize.Height >= 320 * 240)
			{
				bitrate = 622000;
			}
			mMediaRecorder.VideoEncodingBitRate = bitrate;

			mMediaRecorder.VideoFrameRate = MAX_PREVIEW_FPS;
			mMediaRecorder.setVideoSize(mVideoSize.Width, mVideoSize.Height);
			mMediaRecorder.VideoEncoder = MediaRecorder.VideoEncoder.H264;
			mMediaRecorder.AudioEncoder = MediaRecorder.AudioEncoder.AAC;
			mMediaRecorder.OrientationHint = JpegOrientation;
			mMediaRecorder.prepare();
		}

		private void recordVideo()
		{
			lock (this)
			{
				State = CAMERA_STATE.RECORD_VIDEO;
        
				// UI
				mRecordButton.Text = R.@string.button_title_stop;
				mPictureButton.Enabled = false;
        
				// set the surface for recording, {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} performs an operation for recording.
				mProcessor.RecordingSurface = mMediaRecorder.Surface;
				// Start recording
				mMediaRecorder.start();
        
				mRecordingStartTime = DateTimeHelperClass.CurrentUnixTimeMillis();
			}
		}

		private void stopRecordVideo(bool isPausing)
		{
			lock (this)
			{
        
				// prevents terminated during that the operation to start.
				if (!isPausing && (DateTimeHelperClass.CurrentUnixTimeMillis() - mRecordingStartTime) < 1000)
				{
					return;
				}
        
				// UI
				mRecordButton.Text = R.@string.button_title_record;
				mPictureButton.Enabled = true;
        
				// {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} stop an operation for recording.
				mProcessor.RecordingSurface = null;
				// Stop recording
				mMediaRecorder.stop();
				mMediaRecorder.reset();
        
				// Save recording file
				File dir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).AbsolutePath + "/Camera/");
				if (!dir.exists())
				{
					dir.mkdirs();
				}
        
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File file = new java.io.File(dir, createFileName() + "_effect.mp4");
				File file = new File(dir, createFileName() + "_effect.mp4");
				(new File(getExternalFilesDir(null), "temp.mp4")).renameTo(file);
        
				MediaScannerConnection.scanFile(Sample_Effect.this, new string[]{file.AbsolutePath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper(this));
        
				Toast.makeTextuniquetempvar.show();
        
				if (!isPausing)
				{
					try
					{
						prepareMediaRecorder();
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
        
				State = CAMERA_STATE.PREVIEW;
			}
		}

		private class OnScanCompletedListenerAnonymousInnerClassHelper : MediaScannerConnection.OnScanCompletedListener
		{
			private readonly Sample_Effect outerInstance;

			public OnScanCompletedListenerAnonymousInnerClassHelper(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onScanCompleted(string path, Uri uri)
			{
				Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
			}
		}

		/// <summary>
		/// Take picture.
		/// </summary>
		private void takePicture()
		{
			try
			{
				// Sets orientation
				mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, JpegOrientation);

				mSCameraSession.capture(mCaptureBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper5(this), mBackgroundHandler);
				State = CAMERA_STATE.TAKE_PICTURE;
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to take picture.", true);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper5 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_Effect outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper5(Sample_Effect outerInstance)
			{
				this.outerInstance = outerInstance;
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
			private readonly Sample_Effect outerInstance;

			public OrientationEventListenerAnonymousInnerClassHelper(Sample_Effect outerInstance) : base(outerInstance)
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
		/// Starts background thread that callback from camera will posted.
		/// </summary>
		private void startBackgroundThread()
		{
			mBackgroundHandlerThread = new HandlerThread("Background Thread");
			mBackgroundHandlerThread.start();
			mBackgroundHandler = new Handler(mBackgroundHandlerThread.Looper);
		}

		/// <summary>
		/// Stops background thread.
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
		}

		public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
		{

			SCameraFilterInfo filterInfo = mFilterInfoList[position];

			Log.v(TAG, "Filter selected: " + filterInfo.Name);

			// create a SCameraFilter using SCameraFilterManager.
			mFilter = mSCameraFilterManager.createFilter(filterInfo);

			LinearLayout layout = (LinearLayout) findViewById(R.id.parameterbox);

			foreach (View v in mParameterViewList)
			{
				layout.removeView(v);
				if (v is SeekBar)
				{
					((SeekBar) v).OnSeekBarChangeListener = null;
				}
			}
			mParameterViewList.Clear();

			if (mFilterParameterInfoList[filterInfo.Name] != null)
			{
				IList<FilterParameterInfo> filterParameterInfoList = mFilterParameterInfoList[filterInfo.Name];

				foreach (FilterParameterInfo info in filterParameterInfoList)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String paramName = info.mParameterName;
					string paramName = info.mParameterName;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.util.Range<Integer> paramRange = info.mParameterRange;
					Range<int?> paramRange = info.mParameterRange;

					TextView parameterLabel = new TextView(this);
					parameterLabel.Text = info.mParameterName;

					LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT);
					layoutParams.gravity = Gravity.CENTER;
					parameterLabel.LayoutParams = layoutParams;

					SeekBar seekBar = new SeekBar(this);
					seekBar.Max = info.mParameterRange.Upper - info.mParameterRange.Lower;
					seekBar.Progress = (info.mParameterRange.Upper - info.mParameterRange.Lower) / 2;

					// set a key and value of filter parameters.
					mFilter.setParameter(info.mParameterName, seekBar.Progress + info.mParameterRange.Lower);

					layoutParams = new LinearLayout.LayoutParams((int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, 60, Resources.DisplayMetrics), LinearLayout.LayoutParams.WRAP_CONTENT);
					layoutParams.gravity = Gravity.CENTER;
					seekBar.LayoutParams = layoutParams;

					seekBar.OnSeekBarChangeListener = new OnSeekBarChangeListenerAnonymousInnerClassHelper(this, paramName, paramRange, seekBar);

					layout.addView(parameterLabel);
					layout.addView(seekBar);
					mParameterViewList.Add(parameterLabel);
					mParameterViewList.Add(seekBar);
				}

			}

			// set a filter parameters.
			SCameraProcessorParameter parameter = mProcessor.Parameters;
			parameter.set(SCameraEffectProcessor.FILTER_EFFECT, mFilter);
			mProcessor.Parameters = parameter;
		}

		private class OnSeekBarChangeListenerAnonymousInnerClassHelper : SeekBar.OnSeekBarChangeListener
		{
			private readonly Sample_Effect outerInstance;

			private string paramName;
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private android.util.Range<int?> paramRange;
			private Range<int?> paramRange;
			private SeekBar seekBar;

			public OnSeekBarChangeListenerAnonymousInnerClassHelper<T1>(Sample_Effect outerInstance, string paramName, Range<T1> paramRange, SeekBar seekBar)
			{
				this.outerInstance = outerInstance;
				this.paramName = paramName;
				this.paramRange = paramRange;
				this.seekBar = seekBar;
			}

			public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
			{
				Log.e(TAG, string.Format("Parameter({0}) --> {1:D}", paramName, progress + paramRange.Lower));

				// set a key and value of filter parameters.
				outerInstance.mFilter.setParameter(paramName, progress + paramRange.Lower);

				// set a filter parameters.
				SCameraProcessorParameter param = outerInstance.mProcessor.Parameters;
				param.set(SCameraEffectProcessor.FILTER_EFFECT, outerInstance.mFilter);
				outerInstance.mProcessor.Parameters = param;
			}

			public override void onStartTrackingTouch(SeekBar seekBar)
			{
			}

			public override void onStopTrackingTouch(SeekBar seekBar)
			{
			}
		}

		public override void onNothingSelected<T1>(AdapterView<T1> parent)
		{
		}


		/// <summary>
		/// Saves <seealso cref="android.media.Image"/> to file.
		/// </summary>
		private class ImageSaver
		{
			private readonly Sample_Effect outerInstance;

			public ImageSaver(Sample_Effect outerInstance)
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
						// after using the image object, should be called the close().
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

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private <T> boolean contains(final T[] array, final T key)
		private bool contains<T>(T[] array, T key)
		{
			foreach (T i in array)
			{
				if (i.Equals(key))
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
			private readonly Sample_Effect outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Effect outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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