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
	using ImageFormat = android.graphics.ImageFormat;
	using Matrix = android.graphics.Matrix;
	using Point = android.graphics.Point;
	using RectF = android.graphics.RectF;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
	using StreamConfigurationMap = android.hardware.camera2.@params.StreamConfigurationMap;
	using Image = android.media.Image;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using HandlerThread = android.os.HandlerThread;
	using DateFormat = android.text.format.DateFormat;
	using Log = android.util.Log;
	using Size = android.util.Size;
	using Gravity = android.view.Gravity;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using Button = android.widget.Button;
	using FrameLayout = android.widget.FrameLayout;
	using ImageView = android.widget.ImageView;
	using Toast = android.widget.Toast;

	using SCameraPanoramaProcessor = com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor;
	using SCameraProcessorManager = com.samsung.android.sdk.camera.processor.SCameraProcessorManager;
	using SCameraProcessorParameter = com.samsung.android.sdk.camera.processor.SCameraProcessorParameter;
	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;
	using RectView = com.samsung.android.sdk.camera.sample.cases.util.RectView;


	public class Sample_Panorama : Activity
	{
		private bool InstanceFieldsInitialized = false;

		public Sample_Panorama()
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
		private const string TAG = "Sample_Panorama";

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;
		private SCameraDevice mSCameraDevice;
		private SCameraCaptureSession mSCameraSession;
		private SCaptureRequest.Builder mPreviewBuilder;
		private SCameraPanoramaProcessor mProcessor;

		/// <summary>
		/// Current Preview Size.
		/// </summary>
		private Size mPreviewSize;

		/// <summary>
		/// ID of the current <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private string mCameraId;

		/// <summary>
		/// An <seealso cref="AutoFitTextureView"/> for camera preview.
		/// </summary>
		private AutoFitTextureView mTextureView;

		/// <summary>
		/// A <seealso cref="Button"/> to start and stop taking picture.
		/// </summary>
		private Button mPictureButton;

		/// <summary>
		/// An <seealso cref="ImageView"/> for panorama stitching preview.
		/// </summary>
		private ImageView mPanoramaPreview;

		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.sample.cases.util.RectView"/> for panorama tracing rect.
		/// </summary>
		private RectView mPanoramaRectView;

		/// <summary>
		/// Temp data to be used panorama processing in callback.
		/// </summary>
		private readonly PanoramaData mPanoramaData = new PanoramaData();

		/// <summary>
		/// Scale factor for panorama preview.
		/// </summary>
		private const float PANORAMA_PREVIEW_SCALE = 0.8f;

		private HandlerThread mBackgroundHandlerThread;
		private Handler mBackgroundHandler;

		private ImageSaver mImageSaver;

		private Semaphore mCameraOpenCloseLock = new Semaphore(1);

		/// <summary>
		/// True if <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER"/> is triggered.
		/// </summary>
		private bool isAFTriggered;

		/// <summary>
		/// True if <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_LOCK"/> and <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AWB_LOCK"/> are requested.
		/// </summary>
		private bool isAEAWBLocked;

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

		private enum CAMERA_STATE
		{
			IDLE,
			PREVIEW,
			WAIT_AF,
			WAIT_AEAWB_LOCK,
			TAKE_PICTURE,
			PROCESSING
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
				switch (outerInstance.mState)
				{
					case com.samsung.android.sdk.camera.sample.cases.Sample_Panorama.CAMERA_STATE.IDLE:
					case com.samsung.android.sdk.camera.sample.cases.Sample_Panorama.CAMERA_STATE.PREVIEW:
					case com.samsung.android.sdk.camera.sample.cases.Sample_Panorama.CAMERA_STATE.TAKE_PICTURE:
						// do nothing
						break;

					// If AF is triggered and AF_STATE indicates AF process is finished, app will lock AE/AWB
					case com.samsung.android.sdk.camera.sample.cases.Sample_Panorama.CAMERA_STATE.WAIT_AF:
					{
						if (outerInstance.isAFTriggered)
						{
							int? afState = result.get(SCaptureResult.CONTROL_AF_STATE);
							if (null == afState || SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState || SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState)
							{ // in this way, app will compatible with legacy device
								outerInstance.lockAEAWB();
								outerInstance.isAFTriggered = false;
							}
						}
						break;
					}

					// If AE/AWB is locked and AE_STATE/AWB_STATE indicates lock is completed, app will take a picture.
					case com.samsung.android.sdk.camera.sample.cases.Sample_Panorama.CAMERA_STATE.WAIT_AEAWB_LOCK:
					{
						if (outerInstance.isAEAWBLocked)
						{
							int? aeState = result.get(SCaptureResult.CONTROL_AE_STATE);
							int? awbState = result.get(SCaptureResult.CONTROL_AWB_STATE);
							if (null == aeState || null == awbState || (SCaptureResult.CONTROL_AE_STATE_LOCKED == aeState && (SCaptureResult.CONTROL_AWB_STATE_LOCKED == awbState || SCaptureResult.CONTROL_AWB_STATE_CONVERGED == awbState)))
							{ // in this way, app will compatible with legacy device
								outerInstance.startTakePicture();
								outerInstance.isAEAWBLocked = false;
							}
						}
						break;
					}
				}
			}
		}

		/// <summary>
		/// Temp data to be used in <seealso cref="com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor.EventCallback"/>.
		/// </summary>
		private class PanoramaData
		{
			public int direction;
			public Point baseRectOffset, traceRectOffset;
			public Bitmap livePreview;

			public PanoramaData()
			{
				clearData();
			}

			public virtual void clearData()
			{
				direction = 0;
				baseRectOffset = new Point();
				traceRectOffset = new Point();
				livePreview = null;
			}
		}

		/// <summary>
		/// A <seealso cref="com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor.EventCallback"/> that handles events related to panorama processing.
		/// </summary>
		private SCameraPanoramaProcessor.EventCallback mProcessorCallback = new EventCallbackAnonymousInnerClassHelper();

		private class EventCallbackAnonymousInnerClassHelper : SCameraPanoramaProcessor.EventCallback
		{
			public EventCallbackAnonymousInnerClassHelper()
			{
			}


			public override void onError(int code)
			{
				StringBuilder builder = new StringBuilder();
				builder.Append("Fail to create result: ");

				switch (code)
				{
					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_DECODING_FAIL:
					{
						builder.Append("decoding fail");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_ENCODING_FAIL:
					{
						builder.Append("encoding fail");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PROCESSING_FAIL:
					{
						builder.Append("processing fail");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_UNKNOWN_ERROR:
					{
						builder.Append("unknown error");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_MAX_FRAME_COUNT:
					{
						builder.Append("reached to max frame count");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_NO_DIRECTION:
					{
						builder.Append("direction was not found");
						break;
					}

					case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_TRACING:
					{
						builder.Append("tracing fail");
						break;
					}
				}

				outerInstance.showAlertDialog(builder.ToString(), false);
				outerInstance.endTakePicture();
			}

			public override void onRectChanged(int x, int y)
			{
				lock (outerInstance.mPanoramaData)
				{
					outerInstance.mPanoramaData.traceRectOffset.set(x, y);
				}

				// Draw tracing rect.
				runOnUiThread(() =>
				{
					lock (outerInstance.mPanoramaData)
					{
						if (null == outerInstance.mPanoramaRectView || View.INVISIBLE == outerInstance.mPanoramaRectView.Visibility)
						{
							return;
						}

						// Merge coordinates.
						Size rectSize = outerInstance.mPanoramaRectView.RectSize;
						int offsetX = outerInstance.mPanoramaData.baseRectOffset.x + (int)(rectSize.Width * (outerInstance.mPanoramaData.traceRectOffset.x / 1000.0));
						int offsetY = outerInstance.mPanoramaData.baseRectOffset.y - (int)(rectSize.Height * (outerInstance.mPanoramaData.traceRectOffset.y / 1000.0));

						outerInstance.mPanoramaRectView.setRectOffset(offsetX, offsetY);
						outerInstance.mPanoramaRectView.invalidate();
					}
				});
			}

			public override void onDirectionChanged(int direction)
			{
				lock (outerInstance.mPanoramaData)
				{
					outerInstance.mPanoramaData.direction = direction;
				}
			}

			public override void onStitchingProgressed(int progress)
			{
				// Stitching progress.
			}

			public override void onLivePreviewDataStitched(Bitmap data)
			{
				lock (outerInstance.mPanoramaData)
				{
					outerInstance.mPanoramaData.livePreview = data;
				}

				// Draw panorama preview.
				runOnUiThread(() =>
				{
					lock (outerInstance.mPanoramaData)
					{
						if (null == outerInstance.mPanoramaPreview || null == outerInstance.mPanoramaRectView)
						{
							return;
						}

						// Scale panorama preview data.
						int scaledWidth = (int)(outerInstance.mPanoramaData.livePreview.Width * PANORAMA_PREVIEW_SCALE);
						int scaledHeight = (int)(outerInstance.mPanoramaData.livePreview.Height * PANORAMA_PREVIEW_SCALE);
						Bitmap scaledLivePreview = Bitmap.createScaledBitmap(outerInstance.mPanoramaData.livePreview, scaledWidth, scaledHeight, true);
						outerInstance.mPanoramaPreview.ImageBitmap = scaledLivePreview;

						// Setup tracing rect size.
						Size rectSize = outerInstance.mPanoramaRectView.RectSize;
						if (rectSize.Width < 1 || rectSize.Height < 1)
						{
							outerInstance.mPanoramaRectView.setRectSize(scaledLivePreview.Width, scaledLivePreview.Height);
						}

						// Setup preview and tracing rect gravity.
						if (outerInstance.mPanoramaData.direction > 0)
						{
							if (View.INVISIBLE == outerInstance.mPanoramaPreview.Visibility)
							{
								FrameLayout.LayoutParams @params = (FrameLayout.LayoutParams)outerInstance.mPanoramaPreview.LayoutParams;

								switch (outerInstance.mPanoramaData.direction)
								{
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
										@params.gravity = Gravity.CENTER_VERTICAL | Gravity.END;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
										@params.gravity = Gravity.CENTER_HORIZONTAL | Gravity.BOTTOM;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
										@params.gravity = Gravity.CENTER_VERTICAL | Gravity.START;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
										@params.gravity = Gravity.CENTER_HORIZONTAL | Gravity.TOP;
										break;
								}

								outerInstance.mPanoramaPreview.LayoutParams = @params;
								outerInstance.mPanoramaPreview.Visibility = View.VISIBLE;
							}

							if (View.INVISIBLE == outerInstance.mPanoramaRectView.Visibility)
							{
								switch (outerInstance.mPanoramaData.direction)
								{
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
										outerInstance.mPanoramaRectView.RectGravity = RectView.GRAVITY_END | RectView.GRAVITY_CENTER_VERTICAL;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
										outerInstance.mPanoramaRectView.RectGravity = RectView.GRAVITY_BOTTOM | RectView.GRAVITY_CENTER_HORIZONTAL;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
										outerInstance.mPanoramaRectView.RectGravity = RectView.GRAVITY_START | RectView.GRAVITY_CENTER_VERTICAL;
										break;
									case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
										outerInstance.mPanoramaRectView.RectGravity = RectView.GRAVITY_TOP | RectView.GRAVITY_CENTER_HORIZONTAL;
										break;
								}

								outerInstance.mPanoramaRectView.Visibility = View.VISIBLE;
							}

							// Change base rect coordinates.
							switch (outerInstance.mPanoramaData.direction)
							{
								case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
									outerInstance.mPanoramaData.baseRectOffset.x = -(scaledLivePreview.Width - rectSize.Width);
									break;
								case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
									outerInstance.mPanoramaData.baseRectOffset.y = -(scaledLivePreview.Height - rectSize.Height);
									break;
								case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
									outerInstance.mPanoramaData.baseRectOffset.x = scaledLivePreview.Width - rectSize.Width;
									break;
								case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
									outerInstance.mPanoramaData.baseRectOffset.y = scaledLivePreview.Height - rectSize.Height;
									break;
							}
						}
					}
				});
			}

			public override void onMaxFramesCaptured()
			{
				// Will be stopped.
			}

			public override void onMovingTooFast()
			{
				// Move slowly.
			}

			public override void onProcessCompleted(Image result)
			{
				outerInstance.mImageSaver.save(result, outerInstance.createFileName() + "_panorama.jpg");
				outerInstance.endTakePicture();
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_panorama;
		}

		public override void onResume()
		{
			base.onResume();

			startBackgroundThread();

			//Initialize SCamera.
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
			createProcessor();
			createUI();
			openCamera();
		}

		public override void onPause()
		{

			stopBackgroundThread();

			if (CAMERA_STATE.TAKE_PICTURE == mState)
			{
				endTakePicture();
			}

			deinitProcessor();
			closeCamera();

			base.onPause();
		}

		/// <returns> true, If device supports required feature. </returns>
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

				if (null == mCameraId)
				{
					showAlertDialog("No back-facing camera exist.", true);
					return false;
				}

				SCameraCharacteristics cameraCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(mCameraId);

				if (!contains(cameraCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE))
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
				if (!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_PANORAMA))
				{
					showAlertDialog("This device does not support Panorama Processor.", true);
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

		/// <summary>
		/// Create <seealso cref="com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor"/>.
		/// </summary>
		private void createProcessor()
		{
			SCameraProcessorManager processorManager = mSCamera.SCameraProcessorManager;

			mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_PANORAMA);
		}

		/// <summary>
		/// Initialize <seealso cref="com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor"/>.
		/// </summary>
		private void initProcessor()
		{
			SCameraProcessorParameter parameter = mProcessor.Parameters;

			parameter.set(SCameraPanoramaProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
			parameter.set(SCameraPanoramaProcessor.STREAM_SIZE, mPreviewSize);
			parameter.set(SCameraPanoramaProcessor.CAMERA_ID, int.Parse(mCameraId));

			mProcessor.Parameters = parameter;
			mProcessor.initialize();
			mProcessor.setEventCallback(mProcessorCallback, mBackgroundHandler);
		}

		/// <summary>
		/// Deinitialize <seealso cref="com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor"/>.
		/// </summary>
		private void deinitProcessor()
		{
			if (null != mProcessor)
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
			mPictureButton = (Button) findViewById(R.id.picture);
			mPictureButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);

			mPanoramaPreview = (ImageView) findViewById(R.id.panorama_preview);
			mPanoramaRectView = (RectView) findViewById(R.id.panorama_rect);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Panorama outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Panorama outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (CAMERA_STATE.PREVIEW == outerInstance.mState)
				{
					outerInstance.lockAF();
				}
				else if (CAMERA_STATE.TAKE_PICTURE == outerInstance.mState)
				{
					outerInstance.stopTakePicture();
				}
			}
		}

		private class SurfaceTextureListenerAnonymousInnerClassHelper : TextureView.SurfaceTextureListener
		{
			private readonly Sample_Panorama outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_Panorama outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height)
			{
				outerInstance.configureTransform(width, height);
				outerInstance.createPreviewSession();
			}

			public override void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height)
			{
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

				if (null != mSCameraSession)
				{
					mSCameraSession.close();
					mSCameraSession = null;
				}

				if (null != mSCameraDevice)
				{
					mSCameraDevice.close();
					mSCameraDevice = null;
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
				// Input Surface from panorama processor will have a format of ImageFormat.YUV420_888
				mPreviewSize = streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)[0];

				foreach (Size option in streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888))
				{
					// preview size must be supported by panorama processor
					if (!contains(mProcessor.Parameters.get(SCameraPanoramaProcessor.STREAM_SIZE_LIST), option))
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

				int orientation = Resources.Configuration.orientation;
				if (Configuration.ORIENTATION_LANDSCAPE == orientation)
				{
					mTextureView.setAspectRatio(mPreviewSize.Width, mPreviewSize.Height);
				}
				else
				{
					mTextureView.setAspectRatio(mPreviewSize.Height, mPreviewSize.Width);
				}

				initProcessor();

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
			private readonly Sample_Panorama outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Panorama outerInstance)
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
				outerInstance.showAlertDialog("Camera disconnected.", true);
			}

			public override void onError(SCameraDevice sCameraDevice, int i)
			{
				outerInstance.mCameraOpenCloseLock.release();
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
				texture.setDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);

				Surface surface = new Surface(texture);

				mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mPreviewBuilder.addTarget(surface);
				mPreviewBuilder.addTarget(mProcessor.InputSurface);

				//HAL Workaround
				mPreviewBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);

				IList<Surface> outputSurface = Arrays.asList(surface, mProcessor.InputSurface);
				mSCameraDevice.createCaptureSession(outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);
			}
			catch (CameraAccessException e)
			{
				showAlertDialog("Fail to session. " + e.Message, true);
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_Panorama outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Panorama outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConfigured(SCameraCaptureSession sCameraCaptureSession)
			{
				outerInstance.mSCameraSession = sCameraCaptureSession;
				outerInstance.startPreview();
			}

			public override void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession)
			{
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
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
				mState = CAMERA_STATE.PREVIEW;
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
				mState = CAMERA_STATE.WAIT_AF;
				isAFTriggered = false;

				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_START);
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
			private readonly Sample_Panorama outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper(Sample_Panorama outerInstance)
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
			private readonly Sample_Panorama outerInstance;

			public CaptureCallbackAnonymousInnerClassHelper2(Sample_Panorama outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result)
			{
				outerInstance.mState = CAMERA_STATE.PREVIEW;
			}
		}

		/// <summary>
		/// Starts AE/AWB lock by request <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_LOCK"/> and <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AWB_LOCK"/>.
		/// </summary>
		private void lockAEAWB()
		{
			try
			{
				mState = CAMERA_STATE.WAIT_AEAWB_LOCK;

				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AWB_LOCK, true);
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);

				isAEAWBLocked = true;
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to lock AE/AWB", true);
			}
		}

		/// <summary>
		/// Unlock AE/AWB.
		/// </summary>
		private void unlockAEAWB()
		{
			try
			{
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, false);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AWB_LOCK, false);
				mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to unlock AE/AWB", true);
			}
		}

		/// <summary>
		/// Starts taking picture.
		/// </summary>
		private void startTakePicture()
		{
			runOnUiThread(() =>
			{
				if (null != mPictureButton)
				{
					mPictureButton.Text = R.@string.button_title_stop;
				}
				initPanoramaUI();
			});

			mPanoramaData.clearData();
			mProcessor.start();

			mState = CAMERA_STATE.TAKE_PICTURE;
		}

		/// <summary>
		/// Stops taking picture.
		/// </summary>
		private void stopTakePicture()
		{
			mState = CAMERA_STATE.PROCESSING;
			mProcessor.stop();
		}

		/// <summary>
		/// End taking picture.
		/// </summary>
		private void endTakePicture()
		{
			runOnUiThread(() =>
			{
				if (null != mPictureButton)
				{
					mPictureButton.Text = R.@string.button_title_picture;
				}
				hidePanoramaUI();
			});

			unlockAEAWB();
			unlockAF();
		}

		/// <summary>
		/// Initialize panorama ui.
		/// </summary>
		private void initPanoramaUI()
		{
			if (null != mPanoramaPreview)
			{
				mPanoramaPreview.ImageBitmap = null;
			}
			if (null != mPanoramaRectView)
			{
				mPanoramaRectView.clearRect();
			}
		}

		/// <summary>
		/// Hide panorama ui.
		/// </summary>
		private void hidePanoramaUI()
		{
			if (null != mPanoramaPreview)
			{
				mPanoramaPreview.Visibility = View.INVISIBLE;
			}
			if (null != mPanoramaRectView)
			{
				mPanoramaRectView.Visibility = View.INVISIBLE;
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
			if (null != mBackgroundHandlerThread)
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
			private readonly Sample_Panorama outerInstance;

			public ImageSaver(Sample_Panorama outerInstance)
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
			private readonly Sample_Panorama outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Panorama outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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