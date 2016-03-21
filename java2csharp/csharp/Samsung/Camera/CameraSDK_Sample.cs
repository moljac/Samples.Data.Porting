using System.Collections.Generic;
using System.Text;

namespace com.samsung.android.sdk.camera.sample
{

	using Manifest = android.Manifest;
	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogFragment = android.app.DialogFragment;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using PackageInfo = android.content.pm.PackageInfo;
	using PackageManager = android.content.pm.PackageManager;
	using Bundle = android.os.Bundle;
	using DrawableRes = android.support.annotation.DrawableRes;
	using NonNull = android.support.annotation.NonNull;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using Log = android.util.Log;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ListView = android.widget.ListView;

	using Sample_Constrained_HighSpeedVideo = com.samsung.android.sdk.camera.sample.cases.Sample_Constrained_HighSpeedVideo;
	using Sample_DOF = com.samsung.android.sdk.camera.sample.cases.Sample_DOF;
	using Sample_Effect = com.samsung.android.sdk.camera.sample.cases.Sample_Effect;
	using Sample_Filter = com.samsung.android.sdk.camera.sample.cases.Sample_Filter;
	using Sample_HDR = com.samsung.android.sdk.camera.sample.cases.Sample_HDR;
	using Sample_HighSpeedVideo = com.samsung.android.sdk.camera.sample.cases.Sample_HighSpeedVideo;
	using Sample_Image = com.samsung.android.sdk.camera.sample.cases.Sample_Image;
	using Sample_LowLight = com.samsung.android.sdk.camera.sample.cases.Sample_LowLight;
	using Sample_Panorama = com.samsung.android.sdk.camera.sample.cases.Sample_Panorama;
	using Sample_Single = com.samsung.android.sdk.camera.sample.cases.Sample_Single;
	using Sample_Torch = com.samsung.android.sdk.camera.sample.cases.Sample_Torch;
	using Sample_YUV = com.samsung.android.sdk.camera.sample.cases.Sample_YUV;
	using Sample_ZSL = com.samsung.android.sdk.camera.sample.cases.Sample_ZSL;

	public class CameraSDK_Sample : Activity
	{
		private bool InstanceFieldsInitialized = false;

		public CameraSDK_Sample()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			SAMPLE_NAMES_LIST = new string[] {SAMPLE_SINGLE, SAMPLE_EFFECT, SAMPLE_HDR, SAMPLE_LLS, SAMPLE_PANORAMA, SAMPLE_DOF, SAMPLE_FILTER, SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO, SAMPLE_HIGHSPEEDVIDEO, SAMPLE_IPX, SAMPLE_TORCH, SAMPLE_ZSL, SAMPLE_YUV, SAMPLE_VERSION};
		}


		private readonly string SAMPLE_SINGLE = "Single";
		private readonly string SAMPLE_EFFECT = "Effect";
		private readonly string SAMPLE_HDR = "High Dynamic Range";
		private readonly string SAMPLE_LLS = "Low Light";
		private readonly string SAMPLE_PANORAMA = "Panorama";
		private readonly string SAMPLE_DOF = "Depth of Field";
		private readonly string SAMPLE_FILTER = "Filter";
		private readonly string SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO = "Constrained High Speed Video Recording";
		private readonly string SAMPLE_HIGHSPEEDVIDEO = "High Speed Video Recording via Scene Mode";
		private readonly string SAMPLE_IPX = "Image Processing Accelerator";
		private readonly string SAMPLE_TORCH = "Torch";
		private readonly string SAMPLE_ZSL = "Opaque reprocessing (Simple ZSL)";
		private readonly string SAMPLE_YUV = "YUV reprocessing";
		private readonly string SAMPLE_VERSION = "Version";

		private string[] SAMPLE_NAMES_LIST;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_camera_sdk_sample;

			createUI();

			if (savedInstanceState == null)
			{
				//Check permissions
				ensurePermissions(Manifest.permission.CAMERA, Manifest.permission.RECORD_AUDIO, Manifest.permission.READ_EXTERNAL_STORAGE, Manifest.permission.WRITE_EXTERNAL_STORAGE);
			}
		}

		protected internal override void onResume()
		{
			Log.e("SEC", "onResume");
			base.onResume();


		}

		private void ensurePermissions(params string[] permissions)
		{
			List<string> deniedPermissionList = new List<string>();

			foreach (string permission in permissions)
			{
				if (PackageManager.PERMISSION_GRANTED != ContextCompat.checkSelfPermission(this, permission))
				{
					deniedPermissionList.Add(permission);
				}
			}

			if (deniedPermissionList.Count > 0)
			{
				ActivityCompat.requestPermissions(this, deniedPermissionList.ToArray(), 0);
			}
		}

		public override void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			foreach (int result in grantResults)
			{
				if (result != PackageManager.PERMISSION_GRANTED)
				{
					showAlertDialog("Requested permission is not granted.", true);
				}
			}
		}

		protected internal override void onSaveInstanceState(Bundle outState)
		{
			Log.e("SEC", "onSaveInstanceState");
			base.onSaveInstanceState(outState);
		}

		protected internal override void onPause()
		{
			Log.e("SEC", "onPause");
			base.onPause();
		}

		/// <summary>
		/// Shows alert dialog.
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showAlertDialog(final String message, final boolean finishActivity)
		private void showAlertDialog(string message, bool finishActivity)
		{
			AlertDialogFragment.newInstance(android.R.drawable.ic_dialog_alert, "Alert", message, finishActivity).show(FragmentManager, "alert_dialog");
		}

		private void createUI()
		{

			ArrayAdapter arrayAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, SAMPLE_NAMES_LIST);

			ListView listView = (ListView) findViewById(R.id.sample_list);
			listView.Adapter = arrayAdapter;
			listView.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly CameraSDK_Sample outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(CameraSDK_Sample outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
			{

				if (position >= outerInstance.SAMPLE_NAMES_LIST.Length)
				{
					return;
				}

				switch (outerInstance.SAMPLE_NAMES_LIST[position])
				{
					case outerInstance.SAMPLE_SINGLE:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Single));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_EFFECT:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Effect));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_HDR:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_HDR));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_LLS:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_LowLight));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_PANORAMA:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Panorama));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_DOF:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_DOF));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_FILTER:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Filter));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Constrained_HighSpeedVideo));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_HIGHSPEEDVIDEO:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_HighSpeedVideo));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_IPX:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Image));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_TORCH:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_Torch));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_ZSL:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_ZSL));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_YUV:
					{
						Intent intent = new Intent(outerInstance, typeof(Sample_YUV));
						startActivity(intent);
						break;
					}

					case outerInstance.SAMPLE_VERSION:
					{
						try
						{
							StringBuilder builder = new StringBuilder();
							PackageInfo packageInfo = PackageManager.getPackageInfo(PackageName, 0);

							builder.Append(string.Format("Version code: {0:D}\n", packageInfo.versionCode)).Append(string.Format("Version name: {0}\n", packageInfo.versionName));

							AlertDialogFragment.newInstance(android.R.drawable.ic_dialog_info, "CameraSDK Sample Application Version", builder.ToString(), false).show(FragmentManager, "info_dialog");
							break;

						}
						catch (PackageManager.NameNotFoundException)
						{
							// This should not happen.
						}
					}
				break;
				}
			}
		}

		/// <summary>
		/// Alert Dialog Fragment </summary>
		public class AlertDialogFragment : DialogFragment
		{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static AlertDialogFragment newInstance(@DrawableRes int iconId, CharSequence title, CharSequence message, boolean finishActivity)
			public static AlertDialogFragment newInstance(int iconId, CharSequence title, CharSequence message, bool finishActivity)
			{
				AlertDialogFragment fragment = new AlertDialogFragment();

				Bundle args = new Bundle();
				args.putInt("icon", iconId);
				args.putCharSequence("title", title);
				args.putCharSequence("message", message);
				args.putBoolean("finish_activity", finishActivity);

				fragment.Arguments = args;
				return fragment;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NonNull @Override public android.app.Dialog onCreateDialog(android.os.Bundle savedInstanceState)
			public override Dialog onCreateDialog(Bundle savedInstanceState)
			{
				bool finishActivity = Arguments.getBoolean("finish_activity");

				return (new AlertDialog.Builder(Activity)).setTitle(Arguments.getCharSequence("title")).setIcon(Arguments.getInt("icon")).setMessage(Arguments.getCharSequence("message")).setPositiveButton(android.R.@string.ok, finishActivity ? new OnClickListenerAnonymousInnerClassHelper(this): null)
								.setCancelable(false).create();
			}

			private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
			{
				private readonly AlertDialogFragment outerInstance;

				public OnClickListenerAnonymousInnerClassHelper(AlertDialogFragment outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void onClick(DialogInterface dialog, int which)
				{
					dialog.dismiss();
					Activity.finish();
				}
			}
		}
	}

}