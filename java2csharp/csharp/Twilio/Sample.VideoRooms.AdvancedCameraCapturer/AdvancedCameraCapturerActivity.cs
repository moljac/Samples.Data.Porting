namespace com.twilio.video.examples.advancedcameracapturer
{

	using Manifest = android.Manifest;
	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using PackageManager = android.content.pm.PackageManager;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Camera = android.hardware.Camera;
	using Bundle = android.os.Bundle;
	using NonNull = android.support.annotation.NonNull;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using View = android.view.View;
	using Button = android.widget.Button;
	using ImageView = android.widget.ImageView;
	using Toast = android.widget.Toast;


	/// <summary>
	/// This example demonstrates advanced use cases of <seealso cref="com.twilio.video.CameraCapturer"/>. Current
	/// use cases shown are as follows:
	/// 
	/// <ol>
	///     <li>Setting Custom <seealso cref="android.hardware.Camera.Parameters"/></li>
	///     <li>Taking a picture while capturing</li>
	/// </ol>
	/// </summary>
	public class AdvancedCameraCapturerActivity : Activity
	{
		private const int CAMERA_PERMISSION_REQUEST_CODE = 100;

		private LocalMedia localMedia;
		private VideoView videoView;
		private Button toggleFlashButton;
		private Button takePictureButton;
		private ImageView pictureImageView;
		private AlertDialog pictureDialog;
		private CameraCapturer cameraCapturer;
		private LocalVideoTrack localVideoTrack;
		private bool flashOn = false;
		private readonly View.OnClickListener toggleFlashButtonClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				outerInstance.toggleFlash();
			}
		}
		private readonly View.OnClickListener takePictureButtonClickListener = new OnClickListenerAnonymousInnerClassHelper2();

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper2()
			{
			}

			public override void onClick(View v)
			{
				outerInstance.takePicture();
			}
		}

		/// <summary>
		/// An example of a <seealso cref="CameraParameterUpdater"/> that shows how to toggle the flash of a
		/// camera if supported by the device.
		/// </summary>
		private readonly CameraParameterUpdater flashToggler = new CameraParameterUpdaterAnonymousInnerClassHelper();

		private class CameraParameterUpdaterAnonymousInnerClassHelper : CameraParameterUpdater
		{
			public CameraParameterUpdaterAnonymousInnerClassHelper()
			{
			}

			public override void apply(Camera.Parameters parameters)
			{
				if (parameters.FlashMode != null)
				{
					string flashMode = outerInstance.flashOn ? Camera.Parameters.FLASH_MODE_OFF : Camera.Parameters.FLASH_MODE_TORCH;
					parameters.FlashMode = flashMode;
					outerInstance.flashOn = !outerInstance.flashOn;
				}
				else
				{
					Toast.makeText(outerInstance, R.@string.flash_not_supported, Toast.LENGTH_LONG).show();
				}
			}
		}

		/// <summary>
		/// An example of a <seealso cref="com.twilio.video.CameraCapturer.PictureListener"/> that decodes the
		/// image to a <seealso cref="Bitmap"/> and shows the result in an alert dialog.
		/// </summary>
		private readonly CameraCapturer.PictureListener photographer = new PictureListenerAnonymousInnerClassHelper();

		private class PictureListenerAnonymousInnerClassHelper : CameraCapturer.PictureListener
		{
			public PictureListenerAnonymousInnerClassHelper()
			{
			}

			public override void onShutter()
			{

			}

			public override void onPictureTaken(sbyte[] bytes)
			{
				Bitmap bitmap = BitmapFactory.decodeByteArray(bytes, 0, bytes.Length);

				if (bitmap != null)
				{
					outerInstance.showPicture(bitmap);
				}
				else
				{
					Toast.makeText(outerInstance, R.@string.take_picture_failed, Toast.LENGTH_LONG).show();
				}
			}
		}

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_advanced_camera_capturer;

			localMedia = LocalMedia.create(this);
			videoView = (VideoView) findViewById(R.id.video_view);
			toggleFlashButton = (Button) findViewById(R.id.toggle_flash_button);
			takePictureButton = (Button) findViewById(R.id.take_picture_button);
			pictureImageView = (ImageView) LayoutInflater.inflate(R.layout.picture_image_view, null);
			pictureDialog = (new AlertDialog.Builder(this)).setView(pictureImageView).setTitle(null).setPositiveButton(R.@string.close, new OnClickListenerAnonymousInnerClassHelper(this))
				   .create();

			if (!checkPermissionForCamera())
			{
				requestPermissionForCamera();
			}
			else
			{
				addCameraVideo();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly AdvancedCameraCapturerActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(AdvancedCameraCapturerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onClick(final android.content.DialogInterface dialog, int which)
			public override void onClick(DialogInterface dialog, int which)
			{
				dialog.dismiss();
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults)
		public override void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			if (requestCode == CAMERA_PERMISSION_REQUEST_CODE)
			{
				bool cameraPermissionGranted = true;

				foreach (int grantResult in grantResults)
				{
					cameraPermissionGranted &= grantResult == PackageManager.PERMISSION_GRANTED;
				}

				if (cameraPermissionGranted)
				{
					addCameraVideo();
				}
				else
				{
					Toast.makeText(this, R.@string.permissions_needed, Toast.LENGTH_LONG).show();
					finish();
				}
			}
		}

		protected internal override void onDestroy()
		{
			localVideoTrack.removeRenderer(videoView);
			localMedia.removeVideoTrack(localVideoTrack);
			localMedia.release();
			base.onDestroy();
		}

		private bool checkPermissionForCamera()
		{
			int resultCamera = ContextCompat.checkSelfPermission(this, Manifest.permission.CAMERA);

			return resultCamera == PackageManager.PERMISSION_GRANTED;
		}

		private void requestPermissionForCamera()
		{
			ActivityCompat.requestPermissions(this, new string[]{Manifest.permission.CAMERA}, CAMERA_PERMISSION_REQUEST_CODE);
		}

		private void addCameraVideo()
		{
			cameraCapturer = new CameraCapturer(this, CameraCapturer.CameraSource.BACK_CAMERA);
			localVideoTrack = localMedia.addVideoTrack(true, cameraCapturer);
			localVideoTrack.addRenderer(videoView);
			toggleFlashButton.OnClickListener = toggleFlashButtonClickListener;
			takePictureButton.OnClickListener = takePictureButtonClickListener;
		}

		private void toggleFlash()
		{
			// Request an update to camera parameters with flash toggler
			cameraCapturer.updateCameraParameters(flashToggler);
		}

		private void takePicture()
		{
			cameraCapturer.takePicture(photographer);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showPicture(final android.graphics.Bitmap bitmap)
		private void showPicture(Bitmap bitmap)
		{
			// TODO: Remove when SDK invokes callback on calling thread
			runOnUiThread(() =>
			{
				pictureImageView.ImageBitmap = bitmap;
				pictureDialog.show();
			});
		}
	}

}