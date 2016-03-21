using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Configuration = android.content.res.Configuration;
	using Matrix = android.graphics.Matrix;
	using RectF = android.graphics.RectF;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
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
	using OrientationEventListener = android.view.OrientationEventListener;
	using Surface = android.view.Surface;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using Spinner = android.widget.Spinner;
	using Toast = android.widget.Toast;

	using AutoFitTextureView = com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;


	/// <summary>
	/// High Speed Video via Constrained Capture Session
	/// </summary>
	public class Sample_Constrained_HighSpeedVideo : Activity, AdapterView.OnItemSelectedListener
	{
		/// <summary>
		/// Tag for the <seealso cref="Log"/>.
		/// </summary>
		private const string TAG = "Sample_ConstrainedHSV";

		private SCamera mSCamera;
		private SCameraManager mSCameraManager;

		/// <summary>
		/// A reference to the opened <seealso cref="com.samsung.android.sdk.camera.SCameraDevice"/>.
		/// </summary>
		private SCameraDevice mSCameraDevice;
		private SCameraConstrainedHighSpeedCaptureSession mSCameraSession;
		private SCameraCharacteristics mCharacteristics;

		/// <summary>
		/// <seealso cref="com.samsung.android.sdk.camera.SCaptureRequest.Builder"/> for the camera preview and recording
		/// </summary>
		private SCaptureRequest.Builder mPreviewBuilder;

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
		/// Current app state.
		/// </summary>
		private CAMERA_STATE mState = CAMERA_STATE.IDLE;

		/// <summary>
		/// The <seealso cref="MediaRecorder"/> for recording audio and video.
		/// </summary>
		private MediaRecorder mMediaRecorder;

		private VideoParameter mVideoParameter;

		/// <summary>
		/// Button to record video
		/// </summary>
		private Button mRecordButton;

		private IList<VideoParameter> mVideoParameterList = new List<VideoParameter>();
		private Spinner mVideoSpinner;

		private long mRecordingStartTime;

		private enum CAMERA_STATE
		{
			IDLE,
			PREVIEW,
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


		private class VideoParameter
		{
			internal readonly Size mVideoSize;
			internal readonly Range<int?> mFpsRange;

			internal VideoParameter(Size videoSize, Range<int?> fpsRange)
			{
				mVideoSize = new Size(videoSize.Width, videoSize.Height);
				mFpsRange = new Range<>(fpsRange.Lower, fpsRange.Upper);
			}

			public override string ToString()
			{
				return mVideoSize.ToString() + " @ " + mFpsRange.Upper + "FPS";
			}

			public override bool Equals(object o)
			{
				return o is VideoParameter && mVideoSize.Equals(((VideoParameter) o).mVideoSize) && mFpsRange.Equals(((VideoParameter) o).mFpsRange);
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
					case CLOSING:
						// do nothing
						break;
				}
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_highspeedvideo;
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

				if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
				{
					showAlertDialog("Device running Android prior to M is not compatible with the Constrained high speed session APIs.", true);
					Log.e(TAG, "Device running Android prior to M is not compatible with the Constrained high speed session APIs.");

					return false;
				}

				// acquires camera characteristics
				mCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(mCameraId);

				if (!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE))
				{
					showAlertDialog("Required AF mode is not supported.", true);
					return false;
				}

				if (!contains(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES), SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_CONSTRAINED_HIGH_SPEED_VIDEO) || mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).HighSpeedVideoSizes.length == 0 || mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).HighSpeedVideoFpsRanges.length == 0)
				{
					showAlertDialog("High speed video recording capability is not supported.", true);
					return false;
				}


				mVideoParameterList.Clear();

				foreach (Size videoSize in mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).HighSpeedVideoSizes)
				{
					foreach (Range<int?> fpsRange in mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).getHighSpeedVideoFpsRangesFor(videoSize))
					{

						//we will record constant fps video
						if (fpsRange.Lower.Equals(fpsRange.Upper))
						{
							mVideoParameterList.Add(new VideoParameter(videoSize, fpsRange));
						}
					}
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
		/// Prepares an UI, like button, etc.
		/// </summary>
		private void createUI()
		{

			ArrayAdapter<VideoParameter> videoParameterArrayAdapter = new ArrayAdapter<VideoParameter>(this, android.R.layout.simple_spinner_item, mVideoParameterList);

			mVideoSpinner = (Spinner) findViewById(R.id.videolist);
			mVideoSpinner.Adapter = videoParameterArrayAdapter;
			mVideoSpinner.OnItemSelectedListener = this;

			mVideoParameter = mVideoParameterList[0];
			mVideoSpinner.Selection = 0;

			mRecordButton = (Button) findViewById(R.id.record);
			mRecordButton.Enabled = true;
			mRecordButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

			// Set SurfaceTextureListener that handle life cycle of TextureView
			mTextureView.SurfaceTextureListener = new SurfaceTextureListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance)
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
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public SurfaceTextureListenerAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance)
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
				mMediaRecorder = new MediaRecorder();

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
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance)
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

			if (null == mSCamera || null == mSCameraDevice || null == mSCameraManager || !mTextureView.Available)
			{
				return;
			}

			try
			{
				mPreviewSize = mVideoParameter.mVideoSize;

				// Android Camera HAL3.2
				// Note that for the use case of multiple output streams, application must select one
				// unique size from this metadata to use. Otherwise a request error might occur.
				// The camera device will only support up to 2 output high speed streams
				// (processed non-stalling format defined in android.request.maxNumOutputStreams) in this mode.
				// This control will be effective only if all of below conditions are true:
				//
				// The application created no more than maxNumHighSpeedStreams processed non-stalling format output streams,
				// where maxNumHighSpeedStreams is calculated as min(2, android.request.maxNumOutputStreams[Processed (but not-stalling)]).
				// The stream sizes are selected from the sizes reported by android.control.availableHighSpeedVideoConfigurations.
				// No processed non-stalling or raw streams are configured.

				Log.e(TAG, "Preview size: " + mPreviewSize + " Video size: " + mVideoParameter.mVideoSize);

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

				prepareMediaRecorder();

				SurfaceTexture texture = mTextureView.SurfaceTexture;

				// Set default buffer size to camera preview size.
				texture.setDefaultBufferSize(mPreviewSize.Width, mPreviewSize.Height);

				Surface previewSurface = new Surface(texture);
				Surface recorderSurface = mMediaRecorder.Surface;

				// Creates SCaptureRequest.Builder for preview and recording with output target.
				mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_RECORD);

				// {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} supports only 24fps.
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, mVideoParameter.mFpsRange);
				mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
				mPreviewBuilder.addTarget(previewSurface);
				mPreviewBuilder.addTarget(recorderSurface);

				// Creates a CameraCaptureSession here.
				IList<Surface> outputSurface = Arrays.asList(previewSurface, recorderSurface);
				mSCameraDevice.createConstrainedHighSpeedCaptureSession(outputSurface, new StateCallbackAnonymousInnerClassHelper(this), mBackgroundHandler);

			}
			catch (CameraAccessException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private class StateCallbackAnonymousInnerClassHelper : SCameraCaptureSession.StateCallback
		{
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public StateCallbackAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConfigured(SCameraCaptureSession sCameraCaptureSession)
			{
				if (outerInstance.State == CAMERA_STATE.CLOSING)
				{
					return;
				}
				outerInstance.mSCameraSession = (SCameraConstrainedHighSpeedCaptureSession) sCameraCaptureSession;
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
				mSCameraSession.setRepeatingBurst(mSCameraSession.createHighSpeedRequestList(mPreviewBuilder.build()), mSessionCaptureCallback, mBackgroundHandler);
				State = CAMERA_STATE.PREVIEW;
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
			}
			catch (CameraAccessException)
			{
				showAlertDialog("Fail to stop preview.", true);
			}
		}

		/// <summary>
		/// Prepares the media recorder to begin recording.
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
			if (mVideoParameter.mVideoSize.Width * mVideoParameter.mVideoSize.Height >= 1920 * 1080)
			{
				bitrate = 14000000;
			}
			else if (mVideoParameter.mVideoSize.Width * mVideoParameter.mVideoSize.Height >= 1280 * 720)
			{
				bitrate = 9730000;
			}
			else if (mVideoParameter.mVideoSize.Width * mVideoParameter.mVideoSize.Height >= 640 * 480)
			{
				bitrate = 2500000;
			}
			else if (mVideoParameter.mVideoSize.Width * mVideoParameter.mVideoSize.Height >= 320 * 240)
			{
				bitrate = 622000;
			}
			mMediaRecorder.VideoEncodingBitRate = bitrate;

			mMediaRecorder.VideoFrameRate = mVideoParameter.mFpsRange.Upper;
			mMediaRecorder.setVideoSize(mVideoParameter.mVideoSize.Width, mVideoParameter.mVideoSize.Height);
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
				mVideoSpinner.Enabled = false;
        
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
				mVideoSpinner.Enabled = true;
        
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
//ORIGINAL LINE: final java.io.File file = new java.io.File(dir, createFileName() + "_hsv.mp4");
				File file = new File(dir, createFileName() + "_hsv.mp4");
				(new File(getExternalFilesDir(null), "temp.mp4")).renameTo(file);
        
				MediaScannerConnection.scanFile(Sample_Constrained_HighSpeedVideo.this, new string[]{file.AbsolutePath}, null, new OnScanCompletedListenerAnonymousInnerClassHelper(this));
        
				Toast.makeTextuniquetempvar.show();
        
				if (!isPausing)
				{
					createPreviewSession();
				}
			}
		}

		private class OnScanCompletedListenerAnonymousInnerClassHelper : MediaScannerConnection.OnScanCompletedListener
		{
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public OnScanCompletedListenerAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onScanCompleted(string path, Uri uri)
			{
				Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
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
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			public OrientationEventListenerAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance) : base(outerInstance)
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

		public override void onNothingSelected<T1>(AdapterView<T1> parent)
		{
		}
		public override void onItemSelected<T1>(AdapterView<T1> adapterView, View view, int position, long id)
		{
			VideoParameter videoParameter = mVideoParameterList[position];

			if (!videoParameter.Equals(mVideoParameter))
			{
				mVideoParameter = videoParameter;

				if (State == CAMERA_STATE.PREVIEW)
				{
					mMediaRecorder.reset();
					createPreviewSession();
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
			Log.e(TAG, message);
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
			private readonly Sample_Constrained_HighSpeedVideo outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Constrained_HighSpeedVideo outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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