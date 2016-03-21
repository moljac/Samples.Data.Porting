using System;
using System.Collections.Generic;
using System.Text;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Configuration = android.content.res.Configuration;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using ImageFormat = android.graphics.ImageFormat;
	using Matrix = android.graphics.Matrix;
	using RectF = android.graphics.RectF;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
	using StreamConfigurationMap = android.hardware.camera2.@params.StreamConfigurationMap;
	using Image = android.media.Image;
	using ImageReader = android.media.ImageReader;
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
	using OrientationEventListener = android.view.OrientationEventListener;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using ImageView = android.widget.ImageView;
	using Toast = android.widget.Toast;

	using SCameraHdrProcessor = com.samsung.android.sdk.camera.processor.SCameraHdrProcessor;
	using SCameraProcessorManager = com.samsung.android.sdk.camera.processor.SCameraProcessorManager;
	using SCameraProcessorParameter = com.samsung.android.sdk.camera.processor.SCameraProcessorParameter;
	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;


	public class Sample_HDR : Activity
	{
		private bool InstanceFieldsInitialized = false;

		public Sample_HDR()
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
			mSessionCaptureCallback = new HdrSessionCaptureCallback(this, this);
			mHdrCaptureCallback = new HdrCaptureCallback(this, this);
		}

		/// <summary>
		/// Tag for the <seealso cref="Log"/>.
		/// </summary>
		private const string TAG = "Sample_HDR";

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;
		private SCameraDevice mSCameraDevice;
		private SCameraCaptureSession mSCameraSession;
		private SCaptureRequest.Builder mPreviewBuilder;
		private SCaptureRequest.Builder mCaptureBuilder;

		/// <summary>
		/// ID of the current <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private string mCameraId;
		private SCameraCharacteristics mCharacteristics;

		/// <summary>
		/// HDR(High Dynamic Range) processor
		/// </summary>
		private SCameraHdrProcessor mProcessor;

		/// <summary>
		/// An <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView"/> for camera preview.
		/// </summary>
		private AutoFitTextureView mTextureView;
		private ImageReader mImageReader;
		private ImageSaver mImageSaver;

		/// <summary>
		/// Input image list to produce high dynamic range image
		/// </summary>
		private IList<Image> mInputImageList = new List<Image>();

		private Size mPreviewSize;

		/// <summary>
		/// A camera related listener/callback will be posted in this handler.
		/// </summary>
		private Handler mBackgroundHandler;
		private HandlerThread mBackgroundHandlerThread;

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
		/// Number of input images to produce high dynamic range image from device
		/// </summary>
		private int mDeviceHdrInputCnt = 0;

		/// <summary>
		/// AE compensation step from device. Smallest step by which the exposure compensation can be changed.
		/// </summary>
		private float mDeviceAeStep = 0;


		private int mReceivedImgCnt = 0;

		/// <summary>
		/// AE compensation values of input images. {short, medium, long}
		/// </summary>
		private readonly int[] AE_COMPENSATIONS = new int[] {-2, 0, 2};
		private readonly sbyte IDX_SHORT_AE = 0;
		private readonly sbyte IDX_MEDIUM_AE = 1;
		private readonly sbyte IDX_LONG_AE = 2;

		private enum CAMERA_STATE
		{
			IDLE,
			PREVIEW,
			WAIT_AF,
			WAIT_AE_LOCK,
			TAKE_HDR_PICTURE,
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
		private class HdrSessionCaptureCallback : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_HDR outerInstance;

			public HdrSessionCaptureCallback(Sample_HDR outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal CaptureHelper mResultListener = null;

			public virtual CaptureHelper ResultListener
			{
				set
				{
					mResultListener = value;
				}
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				// Depends on the current state and capture result, app will take next action.
				switch (outerInstance.State)
				{
					case IDLE:
					case PREVIEW:
					case CLOSING:
						// do nothing
						break;
					case TAKE_HDR_PICTURE:
						if (mResultListener != null)
						{
							mResultListener.checkPresetResult(result);
						}
						break;

					case WAIT_AF:
					{
						if (outerInstance.isAFTriggered)
						{
							int? afState = result.get(SCaptureResult.CONTROL_AF_STATE);
							// Check if AF is finished. If finished, take hdr picture.						
							if (null == afState || SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState || SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState)
							{
								outerInstance.isAFTriggered = false;
								outerInstance.lockAE();
							}
						}
						break;
					}

					case WAIT_AE_LOCK:
					{
						if (result.get(SCaptureResult.CONTROL_AE_STATE) != null && result.get(SCaptureResult.CONTROL_AE_STATE) == SCaptureResult.CONTROL_AE_STATE_LOCKED)
						{
							outerInstance.takeHdrPicture();
						}
						break;
					}
				}
			}
		}

		private HdrSessionCaptureCallback mSessionCaptureCallback;

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

				outerInstance.mReceivedImgCnt++;

				Image image = reader.acquireNextImage();
				outerInstance.mInputImageList.Add(image);

				// if we received enough images to produce hdr, request process to hdr processor.
				if (outerInstance.mReceivedImgCnt == outerInstance.mDeviceHdrInputCnt)
				{
					outerInstance.mReceivedImgCnt = 0;
					outerInstance.mProcessor.requestMultiProcess(outerInstance.mInputImageList);
				}
			}
		}

		private Bitmap decodeToBitmap(ByteBuffer jpegData, int sampleSize)
		{
			sbyte[] jpegDataArray = new sbyte[jpegData.remaining()];
			jpegData.get(jpegDataArray);
			jpegData.rewind();

			BitmapFactory.Options option = new BitmapFactory.Options();
			option.inSampleSize = sampleSize;

			return BitmapFactory.decodeByteArray(jpegDataArray, 0, jpegDataArray.Length, option);
		}

		/// <summary>
		/// Callback to receive result from hdr processor.
		/// </summary>
		private SCameraHdrProcessor.EventCallback mProcessorCallback = new EventCallbackAnonymousInnerClassHelper();

		private class EventCallbackAnonymousInnerClassHelper : SCameraHdrProcessor.EventCallback
		{
			public EventCallbackAnonymousInnerClassHelper()
			{
			}


				/// <summary>
				/// Called when <seealso cref="SCameraHdrProcessor#requestMultiProcess"/> is finished.
				/// </summary>
			public override void onProcessCompleted(Image result)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}

				// decode result image to bitmap
				int sampleSize = 4;
				ByteBuffer jpegData = result.Planes[0].Buffer;
				Bitmap resultImg = outerInstance.decodeToBitmap(jpegData, sampleSize);

				// save result image to file
				outerInstance.mImageSaver.save(result, outerInstance.createFileName() + "_hdr.jpg");

				// decode input images
				Bitmap shortExpImg = outerInstance.decodeToBitmap(outerInstance.mInputImageList[outerInstance.IDX_SHORT_AE].Planes[0].Buffer, sampleSize);
				Bitmap mediumExpImg = outerInstance.decodeToBitmap(outerInstance.mInputImageList[outerInstance.IDX_MEDIUM_AE].Planes[0].Buffer, sampleSize);
				Bitmap longExpImg = outerInstance.decodeToBitmap(outerInstance.mInputImageList[outerInstance.IDX_LONG_AE].Planes[0].Buffer, sampleSize);

				// show result dialog
				showResultDialog(shortExpImg, mediumExpImg, longExpImg, resultImg);
				outerInstance.clearInputImages();
				outerInstance.unlockAF();

				// done. re-start preview with default ae
				outerInstance.startPreview();
				outerInstance.State = CAMERA_STATE.PREVIEW;
			}

			/// <summary>
			/// Called when error occurred.
			/// </summary>
			public override void onError(int code)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}

				StringBuilder builder = new StringBuilder();
				builder.Append("Fail to create result: ");

				switch (code)
				{
					case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_DECODING_FAIL:
					{
						builder.Append("decoding fail");
						break;
					}

					case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_ENCODING_FAIL:
					{
						builder.Append("encoding fail");
						break;
					}

					case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_PROCESSING_FAIL:
					{
						builder.Append("processing fail");
						break;
					}

					case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_UNKNOWN_ERROR:
					{
						builder.Append("unknown error");
						break;
					}
				}
				outerInstance.showAlertDialog(builder.ToString(), false);
				outerInstance.clearInputImages();
				outerInstance.unlockAF();
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_hdr;
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
			State = CAMERA_STATE.CLOSING;

			OrientationListener = false;

			stopBackgroundThread();

			deinitProcessor();
			closeCamera();

			base.onPause();
		}

		private bool checkRequiredFeatures()
		{
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

				if (!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE))
				{
					showAlertDialog("Required AF mode is not supported.", true);
					return false;
				}

				if (!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR))
				{
					showAlertDialog("This device does not support SCamera Processor feature.", true);
					return false;
				}

				SCameraProcessorManager processorManager = mSCamera.SCameraProcessorManager;
				if (!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_HDR))
				{
					showAlertDialog("This device does not support HDR Processor.", true);
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

			mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_HDR);
		}

		private void initProcessor()
		{
			SCameraProcessorParameter parameter = mProcessor.Parameters;

			mDeviceHdrInputCnt = parameter.get(SCameraHdrProcessor.MULTI_INPUT_COUNT_RANGE).Upper;

			parameter.set(SCameraHdrProcessor.STILL_INPUT_FORMAT, ImageFormat.JPEG);
			parameter.set(SCameraHdrProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
			parameter.set(SCameraHdrProcessor.STILL_SIZE, new Size(mImageReader.Width, mImageReader.Height));

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
		/// Prepares an UI, like button, dialog, etc.
		/// </summary>
		private void createUI()
		{
			findViewById(R.id.picture).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

			// Set SurfaceTextureListener that handle life cycle of TextureView
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_HDR outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_HDR outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				// take picture is only works under preview state. lock af first to take hdr picture.		
				if (outerInstance.State == CAMERA_STATE.PREVIEW)
				{
					outerInstance.lockAF();
				}
			}
		}

		private class SurfaceTextureListenerAnonymousInnerClassHelper : TextureView.SurfaceTextureListener
		{
			private readonly Sample_HDR outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_HDR outerInstance)
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

				if (mImageReader != null)
				{
					mImageReader.close();
					mImageReader = null;
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

				// Acquires camera characteristics
				SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);

				// Acquires ae step from device. Smallest step by which the exposure compensation can be changed.
				mDeviceAeStep = characteristics.get(SCameraCharacteristics.CONTROL_AE_COMPENSATION_STEP).floatValue();

				StreamConfigurationMap streamConfigurationMap = characteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

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
				mImageReader = ImageReader.newInstance(jpegSize.Width, jpegSize.Height, ImageFormat.JPEG, mProcessor.Parameters.get(SCameraHdrProcessor.MULTI_INPUT_COUNT_RANGE).Upper);
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

				// Initialize hdr processor
				initProcessor();

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

		private class StateCallbackAnonymousInnerClassHelper : SCameraDevice.StateCallback
		{
			private readonly Sample_HDR outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_HDR outerInstance)
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

				// Creates SCaptureRequest.Builder for preview with output target.
				mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mPreviewBuilder.addTarget(surface);

				mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
				mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mCaptureBuilder.addTarget(mImageReader.Surface);

				// HAL Workaround
				mPreviewBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);
				mCaptureBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);

				// Creates a SCameraCaptureSession here.
				IList<Surface> outputSurface = Arrays.asList(surface, mImageReader.Surface);
				mSCameraDevice.createCaptureSession(outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);

			}
			catch (CameraAccessException e)
			{
				showAlertDialog("Fail to create session. " + e.Message, true);
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_HDR outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_HDR outerInstance)
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
				outerInstance.State = CAMERA_STATE.PREVIEW;
			}

			public override void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.showAlertDialog("Fail to create camera session.", true);
			}
		}

		/// <summary>
		/// Starts a preview.
		/// </summary>
		private void startPreview()
		{
			try
			{
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, false);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, 0);
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to start preview.", true);
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
				mSCameraSession.capture(mPreviewBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to trigger AF", true);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_HDR outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper(Sample_HDR outerInstance)
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
			// Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
			try
			{
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
				mSCameraSession.capture(mPreviewBuilder.build(), new CaptureCallbackAnonymousInnerClassHelper2(this), mBackgroundHandler);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to cancel AF", false);
			}
		}

		private class CaptureCallbackAnonymousInnerClassHelper2 : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_HDR outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper2(Sample_HDR outerInstance)
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

		private void lockAE()
		{
			try
			{
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
				State = CAMERA_STATE.WAIT_AE_LOCK;
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to lock AE", true);
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
			private readonly Sample_HDR outerInstance;

			public OrientationEventListenerAnonymousInnerClassHelper(Sample_HDR outerInstance) : base(outerInstance)
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

		private void clearInputImages()
		{
			foreach (Image i in mInputImageList)
			{
				i.close();
			}
			mInputImageList.Clear();
		}

		/// <summary>
		/// Starts back ground thread that callback from camera will posted.
		/// </summary>
		private void startBackgroundThread()
		{
			mBackgroundHandlerThread = new HandlerThread("Background Thread");
			mBackgroundHandlerThread.start();
			mBackgroundHandler = new Handler(mBackgroundHandlerThread.Looper);
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
		}

		/// <summary>
		/// Saves <seealso cref="android.media.Image"/> to file.
		/// </summary>
		private class ImageSaver
		{
			private readonly Sample_HDR outerInstance;

			public ImageSaver(Sample_HDR outerInstance)
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
			private readonly Sample_HDR outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_HDR outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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
		/// Shows hdr result into dialog.
		/// </summary>
		private void showResultDialog(params object[] args)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.app.AlertDialog.Builder dialog = new android.app.AlertDialog.Builder(this);
			AlertDialog.Builder dialog = new AlertDialog.Builder(this);

			View dialogView = LayoutInflater.inflate(R.layout.result_dialog_hdr, null);

			ImageView shortExpImage = (ImageView)dialogView.findViewById(R.id.shortExpImage);
			shortExpImage.ImageBitmap = (Bitmap)args[0];

			ImageView mediumExpImage = (ImageView)dialogView.findViewById(R.id.mediumExpImage);
			mediumExpImage.ImageBitmap = (Bitmap)args[1];

			ImageView longExpImage = (ImageView)dialogView.findViewById(R.id.longExpImage);
			longExpImage.ImageBitmap = (Bitmap)args[2];

			ImageView resultImage = (ImageView) dialogView.findViewById(R.id.resultHDRImage);
			resultImage.ImageBitmap = (Bitmap) args[3];

			dialog.setView(dialogView).setTitle("Capture result").setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper2(this, dialog));

			runOnUiThread(() =>
			{
				dialog.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly Sample_HDR outerInstance;

			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper2(Sample_HDR outerInstance, AlertDialog.Builder dialog)
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
		/// take multi pictures with different AE to produce high dynamic range image
		/// </summary>
		private void takeHdrPicture()
		{
			State = CAMERA_STATE.TAKE_HDR_PICTURE;
			clearInputImages();

			// 1st.short exposure. make helper to capture short exposure image
			ExposureCaptureHelper shortCapture = new ExposureCaptureHelper(this, this);
			shortCapture.AeBias = (int)(AE_COMPENSATIONS[IDX_SHORT_AE] / mDeviceAeStep);

			// 2nd.medium exposure. make helper to capture medium exposure image
			ExposureCaptureHelper mediumCapture = new ExposureCaptureHelper(this, this);
			mediumCapture.AeBias = (int)(AE_COMPENSATIONS[IDX_MEDIUM_AE] / mDeviceAeStep);

			// 3rd.long exposure. make helper to capture long exposure image
			ExposureCaptureHelper longCapture = new ExposureCaptureHelper(this, this);
			longCapture.AeBias = (int)(AE_COMPENSATIONS[IDX_LONG_AE] / mDeviceAeStep);

			// set order
			shortCapture.Next = mediumCapture;
			mediumCapture.Next = longCapture;
			longCapture.Next = null;

			// start capture
			shortCapture.start();
		}

		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback"/> for <seealso cref="com.samsung.android.sdk.camera.SCameraCaptureSession#capture(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)"/>
		/// </summary>
		private class HdrCaptureCallback : SCameraCaptureSession.CaptureCallback
		{
			private readonly Sample_HDR outerInstance;

			public HdrCaptureCallback(Sample_HDR outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal CaptureHelper mResultListener = null;

			internal virtual CaptureHelper ResultListener
			{
				set
				{
					mResultListener = value;
				}
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				if (mResultListener != null)
				{
					mResultListener.checkCaptureResult(result);
				}
			}
		}

		private HdrCaptureCallback mHdrCaptureCallback;

		/// <summary>
		/// CaptureHelper interface
		/// </summary>
		private interface CaptureHelper
		{
			void start();
			void preset();
			void checkPresetResult(STotalCaptureResult result);
			void capture();
			void checkCaptureResult(STotalCaptureResult result);
			CaptureHelper Next {set;}
		}

		/// <summary>
		/// ExposureCaptureHelper to support capture image with specific exposure compensation
		/// </summary>
		private class ExposureCaptureHelper : CaptureHelper
		{
			private readonly Sample_HDR outerInstance;

			public ExposureCaptureHelper(Sample_HDR outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal CaptureHelper mNext = null;
			internal int mAeBias = 0;

			public virtual void start()
			{
				preset();
			}

			public virtual void preset()
			{
				try
				{
					outerInstance.mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
					outerInstance.mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, mAeBias);

					outerInstance.mSessionCaptureCallback.ResultListener = ExposureCaptureHelper.this;
					outerInstance.mSCameraSession.setRepeatingRequest(outerInstance.mPreviewBuilder.build(), outerInstance.mSessionCaptureCallback, outerInstance.mBackgroundHandler);
				}
				catch (CameraAccessException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			public virtual void checkPresetResult(STotalCaptureResult result)
			{
				if (result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) != null && result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) == mAeBias)
				{
					if (result.get(SCaptureResult.CONTROL_AE_STATE) != null && result.get(SCaptureResult.CONTROL_AE_STATE) == SCaptureResult.CONTROL_AE_STATE_LOCKED)
					{
						outerInstance.mSessionCaptureCallback.ResultListener = null;
						capture();
					}
				}
			}

			public virtual void capture()
			{
				// Orientation
				outerInstance.mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, outerInstance.JpegOrientation);

				outerInstance.mCaptureBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
				outerInstance.mCaptureBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, mAeBias);

				outerInstance.mHdrCaptureCallback.ResultListener = ExposureCaptureHelper.this;
				try
				{
					outerInstance.mSCameraSession.capture(outerInstance.mCaptureBuilder.build(), outerInstance.mHdrCaptureCallback, outerInstance.mBackgroundHandler);
				}
				catch (CameraAccessException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				if (mNext != null)
				{
					mNext.start();
				}
			}

			public virtual void checkCaptureResult(STotalCaptureResult result)
			{
				if (result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) != null)
				{
					int aeBias = result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION);
					Log.d(TAG, "aeBias of captured image : " + aeBias);
				}
			}

			public virtual CaptureHelper Next
			{
				set
				{
					mNext = value;
				}
			}

			public virtual int AeBias
			{
				set
				{
					mAeBias = value;
				}
			}
		}
	}

}