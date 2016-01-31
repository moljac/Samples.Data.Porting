using System;
using System.Collections.Generic;

namespace com.opentok.android.demo.video
{

	using Context = android.content.Context;
	using ImageFormat = android.graphics.ImageFormat;
	using PixelFormat = android.graphics.PixelFormat;
	using SurfaceTexture = android.graphics.SurfaceTexture;
	using Camera = android.hardware.Camera;
	using PreviewCallback = android.hardware.Camera.PreviewCallback;
	using Size = android.hardware.Camera.Size;
	using Build = android.os.Build;
	using Log = android.util.Log;
	using Display = android.view.Display;
	using Surface = android.view.Surface;
	using WindowManager = android.view.WindowManager;


	public class CustomEmulatorVideoCapturer : BaseVideoCapturer, Camera.PreviewCallback
	{

		private const string LOGTAG = "customer-video-capturer";

		private int mCameraIndex = 0;
		private Camera mCamera;
		private Camera.CameraInfo mCurrentDeviceInfo = null;
		private ReentrantLock mPreviewBufferLock = new ReentrantLock();
		private static readonly int PIXEL_FORMAT = ImageFormat.NV21;
		private const int PREFERRED_CAPTURE_WIDTH = 640;
		private const int PREFERRED_CAPTURE_HEIGHT = 480;

		private bool isCaptureStarted = false;
		private bool isCaptureRunning = false;

		private readonly int mNumCaptureBuffers = 3;
		private int mExpectedFrameSize = 0;

		private int mCaptureWidth = -1;
		private int mCaptureHeight = -1;
		private int mCaptureFPS = -1;

		private Display mCurrentDisplay;
		private SurfaceTexture mSurfaceTexture;

		public CustomEmulatorVideoCapturer(Context context)
		{

			// Initialize front camera by default
			this.mCameraIndex = FrontCameraIndex;

			// Get current display to query UI orientation
			WindowManager windowManager = (WindowManager) context.getSystemService(Context.WINDOW_SERVICE);
			mCurrentDisplay = windowManager.DefaultDisplay;

		}

		public override int startCapture()
		{
			if (isCaptureStarted)
			{
				return -1;
			}

			// Set the preferred capturing size
			configureCaptureSize(PREFERRED_CAPTURE_WIDTH, PREFERRED_CAPTURE_HEIGHT);

			// Set the capture parameters
			Camera.Parameters parameters = mCamera.Parameters;
			parameters.setPreviewSize(mCaptureWidth, mCaptureHeight);
			parameters.PreviewFormat = PIXEL_FORMAT;
			parameters.PreviewFrameRate = mCaptureFPS;
			try
			{
				mCamera.Parameters = parameters;
			}
			catch (Exception e)
			{
				Log.e(LOGTAG, "setParameters failed", e);
				return -1;
			}

			// Create capture buffers
			PixelFormat pixelFormat = new PixelFormat();
			PixelFormat.getPixelFormatInfo(PIXEL_FORMAT, pixelFormat);
			int bufSize = mCaptureWidth * mCaptureHeight * pixelFormat.bitsPerPixel / 8;
			sbyte[] buffer = null;
			for (int i = 0; i < mNumCaptureBuffers; i++)
			{
				buffer = new sbyte[bufSize];
				mCamera.addCallbackBuffer(buffer);
			}

			try
			{
				mSurfaceTexture = new SurfaceTexture(42);
				mCamera.PreviewTexture = mSurfaceTexture;

			}
			catch (Exception e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			// Start preview
			mCamera.PreviewCallbackWithBuffer = this;
			mCamera.startPreview();

			mPreviewBufferLock.@lock();
			mExpectedFrameSize = bufSize;
			isCaptureRunning = true;
			mPreviewBufferLock.unlock();

			isCaptureStarted = true;

			return 0;
		}

		public override int stopCapture()
		{
			mPreviewBufferLock.@lock();
			try
			{
				if (isCaptureRunning)
				{
					isCaptureRunning = false;
					mCamera.stopPreview();
					mCamera.PreviewCallbackWithBuffer = null;
				}
			}
			catch (Exception e)
			{
				Log.e(LOGTAG, "Failed to stop camera", e);
				return -1;
			}
			mPreviewBufferLock.unlock();

			isCaptureStarted = false;
			return 0;
		}

		public override void destroy()
		{
			if (mCamera == null)
			{
				return;
			}
			stopCapture();
			mCamera.release();
			mCamera = null;
		}

		public override bool CaptureStarted
		{
			get
			{
				return isCaptureStarted;
			}
		}

		public override CaptureSettings CaptureSettings
		{
			get
			{
    
				// Set the preferred capturing size
				configureCaptureSize(PREFERRED_CAPTURE_WIDTH, PREFERRED_CAPTURE_HEIGHT);
    
				CaptureSettings settings = new CaptureSettings();
				settings.fps = mCaptureFPS;
				settings.width = mCaptureWidth;
				settings.height = mCaptureHeight;
				settings.format = NV21;
				settings.expectedDelay = 0;
				return settings;
			}
		}

		public override void onPause()
		{
		}

		public override void onResume()
		{
		}

		/*
		 * Get the natural camera orientation
		 */
		private int NaturalCameraOrientation
		{
			get
			{
				if (mCurrentDeviceInfo != null)
				{
					return mCurrentDeviceInfo.orientation;
				}
				else
				{
					return 0;
				}
			}
		}

		/*
		 * Check if the current camera is a front camera
		 */
		public virtual bool FrontCamera
		{
			get
			{
				return (mCurrentDeviceInfo != null && mCurrentDeviceInfo.facing == Camera.CameraInfo.CAMERA_FACING_FRONT);
			}
		}

		/*
		 * Returns the currently active camera ID.
		 */
		public virtual int CameraIndex
		{
			get
			{
				return mCameraIndex;
			}
		}

		/*
		 * Switching between cameras if there are multiple cameras on the device.
		 */
		public virtual void swapCamera(int index)
		{
			bool wasStarted = this.isCaptureStarted;

			if (mCamera != null)
			{
				stopCapture();
				mCamera.release();
				mCamera = null;
			}

			this.mCameraIndex = index;
			this.mCamera = Camera.open(index);
			this.mCurrentDeviceInfo = new Camera.CameraInfo();
			Camera.getCameraInfo(index, mCurrentDeviceInfo);

			if (wasStarted)
			{
				startCapture();
			}
		}

		/*
		 * Set current camera orientation
		 */
		private int compensateCameraRotation(int uiRotation)
		{

			int cameraRotation = 0;
			switch (uiRotation)
			{
				case (Surface.ROTATION_0):
					cameraRotation = 0;
					break;
				case (Surface.ROTATION_90):
					cameraRotation = 270;
					break;
				case (Surface.ROTATION_180):
					cameraRotation = 180;
					break;
				case (Surface.ROTATION_270):
					cameraRotation = 90;
					break;
				default:
					break;
			}

			int cameraOrientation = this.NaturalCameraOrientation;

			int totalCameraRotation = 0;
			bool usingFrontCamera = this.FrontCamera;
			if (usingFrontCamera)
			{
				// The front camera rotates in the opposite direction of the
				// device.
				int inverseCameraRotation = (360 - cameraRotation) % 360;
				totalCameraRotation = (inverseCameraRotation + cameraOrientation) % 360;
			}
			else
			{
				totalCameraRotation = (cameraRotation + cameraOrientation) % 360;
			}

			return totalCameraRotation;
		}

		/*
		 * Set camera index
		 */
		private static int FrontCameraIndex
		{
			get
			{
				for (int i = 0; i < Camera.NumberOfCameras; ++i)
				{
					Camera.CameraInfo info = new Camera.CameraInfo();
					Camera.getCameraInfo(i, info);
					if (info.facing == Camera.CameraInfo.CAMERA_FACING_FRONT)
					{
						return i;
					}
				}
				return 0;
			}
		}

		private void configureCaptureSize(int preferredWidth, int preferredHeight)
		{
			Camera.Parameters parameters = mCamera.Parameters;

			IList<Camera.Size> sizes = parameters.SupportedPreviewSizes;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("deprecation") java.util.List<Integer> frameRates = parameters.getSupportedPreviewFrameRates();
			IList<int?> frameRates = parameters.SupportedPreviewFrameRates;
			int maxFPS = 0;
			if (frameRates != null)
			{
				foreach (int? frameRate in frameRates)
				{
					if (frameRate > maxFPS)
					{
						maxFPS = frameRate.Value;
					}
				}
			}
			mCaptureFPS = maxFPS;

			int maxw = 0;
			int maxh = 0;
			for (int i = 0; i < sizes.Count; ++i)
			{
				Camera.Size s = sizes[i];
				if (s.width >= maxw && s.height >= maxh)
				{
					if (s.width <= preferredWidth && s.height <= preferredHeight)
					{
						maxw = s.width;
						maxh = s.height;
					}
				}
			}
			if (maxw == 0 || maxh == 0)
			{
				Camera.Size s = sizes[0];
				maxw = s.width;
				maxh = s.height;
			}

			mCaptureWidth = maxw;
			mCaptureHeight = maxh;
		}

		public override void init()
		{
			mCamera = Camera.open(mCameraIndex);
			mCurrentDeviceInfo = new Camera.CameraInfo();
			Camera.getCameraInfo(mCameraIndex, mCurrentDeviceInfo);

		}

		public override void onPreviewFrame(sbyte[] data, Camera camera)
		{
			mPreviewBufferLock.@lock();
			if (isCaptureRunning)
			{
				if (data.Length == mExpectedFrameSize)
				{
					int currentRotation = 0;

					// Get the rotation of the camera depends on the app is running on the virtual device or real device
					if (Build.BRAND.contains("generic"))
					{
						currentRotation = 0; //it is running on emulator
					}
					else
					{
						currentRotation = compensateCameraRotation(mCurrentDisplay.Rotation);
					}
					// Send frame to OpenTok
					provideByteArrayFrame(data, NV21, mCaptureWidth, mCaptureHeight, currentRotation, FrontCamera);

					// Reuse the video buffer
					camera.addCallbackBuffer(data);
				}
			}
			mPreviewBufferLock.unlock();
		}
	}

}