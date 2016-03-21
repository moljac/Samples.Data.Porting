using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using CameraAccessException = android.hardware.camera2.CameraAccessException;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using TextureView = android.view.TextureView;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using TextView = android.widget.TextView;
	using ToggleButton = android.widget.ToggleButton;



	public class Sample_Torch : Activity
	{
		private static readonly string TAG = typeof(Sample_Torch).Name;
		private SCamera mSCamera;
		private SCameraManager mSCameraManager;

		private IDictionary<string, ToggleButton> mCameraIdViewMap;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_torch;
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

			createUI();
		}

		private void createUI()
		{

			ViewGroup torchButtons = (ViewGroup) findViewById(R.id.torch_buttons_layout);
			torchButtons.removeAllViews();

			mCameraIdViewMap = new Dictionary<>();
			mSCameraManager = mSCamera.SCameraManager;

			int numberOfCameraWithFlash = 0;

			try
			{
				foreach (string id in mSCameraManager.CameraIdList)
				{
					ViewGroup torchItem = (ViewGroup)LayoutInflater.inflate(R.layout.torch_item, null);
					torchButtons.addView(torchItem);

					((TextView)torchItem.findViewById(R.id.cameraId_textView)).Text = id;

					ToggleButton toggleButton = (ToggleButton)torchItem.findViewById(R.id.toggleButton);
					toggleButton.Tag = id;

					if (mSCameraManager.getCameraCharacteristics(id).get(SCameraCharacteristics.FLASH_INFO_AVAILABLE))
					{
						numberOfCameraWithFlash++;

						toggleButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
					}
					else
					{
						toggleButton.Checked = false;
						toggleButton.Enabled = false;
					}
					mCameraIdViewMap[id] = toggleButton;
				}

				mSCameraManager.registerTorchCallback(new TorchCallbackAnonymousInnerClassHelper(this), null);
			}
			catch (CameraAccessException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			((TextView)findViewById(R.id.totalNumber_textView)).Text = "Number of camera device with flash unit = " + numberOfCameraWithFlash;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Sample_Torch outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Torch outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				string cameraId = (string) v.Tag;
				try
				{
					outerInstance.mSCameraManager.setTorchMode(cameraId, ((ToggleButton) v).Checked);
				}
				catch (Exception)
				{
					outerInstance.showAlertDialog("Fail to change torch mode for camera with iduniquetempvar.", true);
				}
			}
		}

		private class TorchCallbackAnonymousInnerClassHelper : SCameraManager.TorchCallback
		{
			private readonly Sample_Torch outerInstance;

			public TorchCallbackAnonymousInnerClassHelper(Sample_Torch outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onTorchModeUnavailable(string cameraId)
			{
				ToggleButton toggleButton = outerInstance.mCameraIdViewMap[cameraId];

				if (null != toggleButton)
				{
					toggleButton.Enabled = false;
				}
			}

			public override void onTorchModeChanged(string cameraId, bool enabled)
			{
				ToggleButton toggleButton = outerInstance.mCameraIdViewMap[cameraId];

				if (null != toggleButton)
				{
					//Enable toggle button as mode changed is invoked.
					toggleButton.Enabled = true;

					if (toggleButton.Checked != enabled)
					{
						toggleButton.Checked = enabled;
					}
				}
			}
		}

		private bool checkRequiredFeatures()
		{
			if (Build.VERSION.SDK_INT < Build.VERSION_CODES.M)
			{
				showAlertDialog("Device running Android prior to M is not compatible with torch mode control APIs.", true);
				Log.e(TAG, "Device running Android prior to M is not compatible with torch mode control APIs.");
				return false;
			}

			try
			{
				bool flashFound = false;

				foreach (string id in mSCamera.SCameraManager.CameraIdList)
				{
					SCameraCharacteristics cameraCharacteristics = mSCamera.SCameraManager.getCameraCharacteristics(id);
					if (cameraCharacteristics.get(SCameraCharacteristics.FLASH_INFO_AVAILABLE))
					{
						flashFound = true;
						break;
					}
				}

				// no camera device with flash unit.
				if (!flashFound)
				{
					showAlertDialog("Device does not have a camera device with flash unit.", true);
					Log.e(TAG, "Device does not have a camera device with flash unit.");

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
			private readonly Sample_Torch outerInstance;

			private bool finishActivity;
			private AlertDialog.Builder dialog;

			public OnClickListenerAnonymousInnerClassHelper(Sample_Torch outerInstance, bool finishActivity, AlertDialog.Builder dialog)
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