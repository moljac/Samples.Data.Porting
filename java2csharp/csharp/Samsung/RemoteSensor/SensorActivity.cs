using System;
using System.Collections.Generic;
using System.Threading;

namespace com.example.remotesensorsampleapp
{


	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using ActivityNotFoundException = android.content.ActivityNotFoundException;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using PackageInfo = android.content.pm.PackageInfo;
	using PackageManager = android.content.pm.PackageManager;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using CompoundButton = android.widget.CompoundButton;
	using OnCheckedChangeListener = android.widget.CompoundButton.OnCheckedChangeListener;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;
	using ToggleButton = android.widget.ToggleButton;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Srs = com.samsung.android.sdk.remotesensor.Srs;
	using SrsRemoteSensor = com.samsung.android.sdk.remotesensor.SrsRemoteSensor;
	using SrsRemoteSensorEvent = com.samsung.android.sdk.remotesensor.SrsRemoteSensorEvent;
	using SrsRemoteSensorManager = com.samsung.android.sdk.remotesensor.SrsRemoteSensorManager;
	using EventListener = com.samsung.android.sdk.remotesensor.SrsRemoteSensorManager.EventListener;

	public class SensorActivity : Activity, SrsRemoteSensorManager.EventListener, View.OnClickListener, CompoundButton.OnCheckedChangeListener
	{

		private const string GEAR_PACKAGE_NAME = "com.samsung.accessory";
		private const string GEAR_FIT_PACKAGE_NAME = "com.samsung.android.wms";
		private const string REMOTESENSOR_PACKAGE_NAME = "com.samsung.android.sdk.remotesensor";
		private bool mBroadcastState = false;

		public const int USER_ACTIVITY = 0;
		public const int PEDOMETER = 1;
		public const int WEARING_STATE = 2;

		private static readonly string[] userActivityValues = new string[] {"Unknown State", "Walk", "Run"};

		private static readonly string[] wearingStateValues = new string[] {"Not Wearing", "Wearing"};

		public class SensorData
		{

			public string mSensorDetails;

			public string mValue;
		}

		internal static SrsRemoteSensorManager mServiceManager = null;
		internal Srs remoteSensor = null;

		internal IList<SrsRemoteSensor> activitySensorList = null;
		internal IList<SrsRemoteSensor> pedoSensorList = null;
		internal IList<SrsRemoteSensor> wearingSensorList = null;

		internal SrsRemoteSensor userActivitySensor = null;
		internal SrsRemoteSensor pedometerSensor = null;
		internal SrsRemoteSensor wearingSensor = null;

		internal Button mBtnUserActivityInfo = null;
		internal ToggleButton mBtnUserActivityValue = null;
		internal Button mBtnPedometerInfo = null;
		internal ToggleButton mBtnPedometerValue = null;
		internal Button mBtnWearingStateInfo = null;
		internal Button mBtnWearingStateValue = null;

		internal TextView mTextUserActivityInfo = null;
		internal TextView mTextUserActivityValue = null;
		internal TextView mTextPedometerInfo = null;
		internal TextView mTextPedometerValue = null;
		internal TextView mTextWearingStateInfo = null;
		internal TextView mTextWearingStateValue = null;

		internal SensorData mSensorDataUserActivity = null;
		internal SensorData mSensorDataPedometer = null;
		internal SensorData mSensorDataWearingState = null;

		internal Thread sensorValueChangedThread;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_sensor;

			remoteSensor = new Srs();

			if (checkPermission())
			{
				initializeeSRS();
			}
			mServiceManager = new SrsRemoteSensorManager(remoteSensor);

			mBtnUserActivityInfo = (Button) findViewById(R.id.btn_user_activity);
			mBtnUserActivityValue = (ToggleButton) findViewById(R.id.btn_user_activity_value);
			mBtnPedometerInfo = (Button) findViewById(R.id.btn_pedometer);
			mBtnPedometerValue = (ToggleButton) findViewById(R.id.btn_pedometer_value);
			mBtnWearingStateInfo = (Button) findViewById(R.id.btn_wearing_state);
			mBtnWearingStateValue = (Button) findViewById(R.id.btn_wearing_state_value);

			mTextUserActivityInfo = (TextView) findViewById(R.id.text_user_activity_info);
			mTextUserActivityValue = (TextView) findViewById(R.id.text_user_activity_value);
			mTextPedometerInfo = (TextView) findViewById(R.id.text_pedometer_info);
			mTextPedometerValue = (TextView) findViewById(R.id.text_pedometer_value);
			mTextWearingStateInfo = (TextView) findViewById(R.id.text_wearing_state_info);
			mTextWearingStateValue = (TextView) findViewById(R.id.text_wearing_state_value);

			mBtnUserActivityInfo.OnClickListener = this;
			mBtnUserActivityValue.OnCheckedChangeListener = this;
			mBtnPedometerInfo.OnClickListener = this;
			mBtnPedometerValue.OnCheckedChangeListener = this;
			mBtnWearingStateInfo.OnClickListener = this;
			mBtnWearingStateValue.OnClickListener = this;

			mSensorDataUserActivity = new SensorData();
			mSensorDataPedometer = new SensorData();
			mSensorDataWearingState = new SensorData();

		}

		private void getPedometerSensorInfo()
		{
			mSensorDataPedometer.mSensorDetails = "";
			mSensorDataPedometer.mValue = "";
			stopPedometerEvent();

			pedoSensorList = mServiceManager.getSensorList(SrsRemoteSensor.TYPE_PEDOMETER);
			if ((pedoSensorList != null) && (pedoSensorList.Count > 0))
			{
				SrsRemoteSensor sensor;
				sensor = pedoSensorList[0];
				mSensorDataPedometer.mSensorDetails = sensor.ToString();

				mTextPedometerInfo.Text = mSensorDataPedometer.mSensorDetails;

			}
			else
			{
				mSensorDataPedometer.mSensorDetails = "Peodometer Sensor is not available";
				mTextPedometerInfo.Text = mSensorDataPedometer.mSensorDetails;

				mSensorDataPedometer.mValue = " ";

				mTextPedometerValue.Text = mSensorDataPedometer.mValue;
			}

		}

		private void getPedometerEvent()
		{
			if ((mServiceManager != null) && (pedoSensorList != null) && (pedoSensorList.Count > 0))
			{

				bool bRet = false;

				pedometerSensor = pedoSensorList[0];

				/// <summary>
				/// The rate argument set the interval between the successive events.
				/// Now there are four values : SENSOR_DELAY_SLOW,
				/// SENSOR_DELAY_NORMAL, SENSOR_DELAY_FAST, SENSOR_DELAY_FASTEST. The
				/// maxBatchReportLatency argument specify the maximum batching time.
				/// When the maxBatchReportLatency is set, the events are batched and
				/// delivered later. The unit is microsecond.
				/// 
				/// Note : The maxBatchReportLatency is not supported now. Please set
				/// it to zero. For TYPE_PEDOMETER sensor, the rate argument has no
				/// effects now. The interval is fixed as 5 minutes and it is less
				/// than 5 minutes when the pedometer data is already available as in
				/// case that another application has already registered the
				/// TYPE_PEDOMETER sensor. The power of the wearable device was
				/// considered for interval decision. For TYPE_USER_ACTIVITY sensor,
				/// the event delay is about 4 to 5 seconds. This value corresponds
				/// to 4 to 8 steps and is to prevent a noise.
				/// </summary>
				bRet = mServiceManager.registerListener(this, pedometerSensor, SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);

				if (bRet == false)
				{
					Toast.makeText(this, "registerListener for pedometer sensor is falied", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(this, "getPedometerEvent", Toast.LENGTH_SHORT).show();
				}

			}
			else
			{
				mSensorDataPedometer.mValue = "Peodometer Sensor is not available";
				mTextPedometerValue.Text = mSensorDataPedometer.mValue;
			}
		}

		private void stopPedometerEvent()
		{
			SrsRemoteSensor sensor;
			if ((mServiceManager != null) && (pedoSensorList != null) && (pedoSensorList.Count > 0))
			{
				sensor = pedoSensorList[0];

				mServiceManager.unregisterListener(this, sensor);
				pedometerSensor = null;

			}
			else
			{
				mSensorDataPedometer.mValue = "Pedometer Sensor is not available";

			}

		}

		private void getUserActivitySensorInfo()
		{

			stopUserActivityEvent();

			mSensorDataUserActivity.mSensorDetails = " ";
			mSensorDataUserActivity.mValue = " ";

			/// <summary>
			/// getSensorList() gets the remote sensor list. The supported sensor
			/// types are : TYPE_ALL, TYPE_PEDOMETER, TYPE_USER_ACTIVITY
			/// </summary>
			activitySensorList = mServiceManager.getSensorList(SrsRemoteSensor.TYPE_USER_ACTIVITY);
			if ((activitySensorList != null) && (activitySensorList.Count > 0))
			{
				SrsRemoteSensor sensor;

				sensor = activitySensorList[0];
				mSensorDataUserActivity.mSensorDetails = sensor.ToString();
				mTextUserActivityInfo.Text = mSensorDataUserActivity.mSensorDetails;

			}
			else
			{
				mSensorDataUserActivity.mSensorDetails = "User Activity Sensor is not available";
				mSensorDataUserActivity.mValue = " ";

				mTextUserActivityInfo.Text = mSensorDataUserActivity.mSensorDetails;
				mTextUserActivityValue.Text = mSensorDataUserActivity.mValue;
			}
		}

		private void getUserActivityEvent()
		{

			if ((mServiceManager != null) && (activitySensorList != null) && (activitySensorList.Count > 0))
			{

				bool bRet = false;

				userActivitySensor = activitySensorList[0];

				/// <summary>
				/// The rate argument set the interval between the successive events.
				/// Now there are four values : SENSOR_DELAY_SLOW,
				/// SENSOR_DELAY_NORMAL, SENSOR_DELAY_FAST, SENSOR_DELAY_FASTEST. The
				/// maxBatchReportLatency argument specify the maximum batching time.
				/// When the maxBatchReportLatency is set, the events are batched and
				/// delivered later. The unit is microsecond.
				/// 
				/// Note : The maxBatchReportLatency is not supported now. Please set
				/// it to zero. For TYPE_PEDOMETER sensor, the rate argument has no
				/// effects now. The interval is fixed as 5 minutes and it is less
				/// than 5 minutes when the pedometer data is already available as in
				/// case that another application has already registered the
				/// TYPE_PEDOMETER sensor. The power of the wearable device was
				/// considered for interval decision. For TYPE_USER_ACTIVITY sensor,
				/// the event delay is about 4 to 5 seconds. This value corresponds
				/// to 4 to 8 steps and is to prevent a noise.
				/// </summary>
				bRet = mServiceManager.registerListener(this, userActivitySensor, SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);

				if (bRet == false)
				{
					Toast.makeText(this, "registerListener for user activity sensor is falied", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(this, "getUserActivityEvent", Toast.LENGTH_SHORT).show();
				}

			}
			else
			{
				mSensorDataUserActivity.mValue = "User Activity Sensor is not available";
				mTextUserActivityValue.Text = mSensorDataUserActivity.mValue;
			}
		}

		private void stopUserActivityEvent()
		{
			SrsRemoteSensor sensor;

			if ((mServiceManager != null) && (activitySensorList != null) && (activitySensorList.Count > 0))
			{

				sensor = activitySensorList[0];

				mServiceManager.unregisterListener(this, sensor);
				userActivitySensor = null;

			}
			else
			{
				mSensorDataUserActivity.mValue = "User Activity Sensor is not available";

			}
		}

		private void getWearingStateSensorInfo()
		{
			mSensorDataWearingState.mSensorDetails = " ";
			mSensorDataWearingState.mValue = " ";

			/// <summary>
			/// getSensorList() gets the remote sensor list. The supported sensor
			/// types are : TYPE_ALL, TYPE_PEDOMETER, TYPE_USER_ACTIVITY
			/// </summary>
			wearingSensorList = mServiceManager.getSensorList(SrsRemoteSensor.TYPE_WEARING_STATE);
			if ((wearingSensorList != null) && (wearingSensorList.Count > 0))
			{
				SrsRemoteSensor sensor;

				sensor = wearingSensorList[0];
				mSensorDataWearingState.mSensorDetails = sensor.ToString();
				mTextWearingStateInfo.Text = mSensorDataWearingState.mSensorDetails;

			}
			else
			{
				mSensorDataWearingState.mSensorDetails = "Wearing State Sensor is not available";
				mSensorDataWearingState.mValue = " ";

				mTextWearingStateInfo.Text = mSensorDataWearingState.mSensorDetails;
				mTextWearingStateValue.Text = mSensorDataWearingState.mValue;
			}
		}

		private void getWearingStateEvent()
		{
			if ((mServiceManager != null) && (wearingSensorList != null) && (wearingSensorList.Count > 0))
			{
				bool bRet = false;

				wearingSensor = wearingSensorList[0];

				bRet = mServiceManager.requestTriggerSensor(this, wearingSensor);

				if (bRet == false)
				{
					Toast.makeText(this, "requestTriggerSensor for wearing state sensor is falied", Toast.LENGTH_SHORT).show();
				}

			}
			else
			{

				mSensorDataWearingState.mValue = "Wearing State Sensor is not available";
				mTextWearingStateValue.Text = mSensorDataWearingState.mValue;
			}
		}

		protected internal override void onDestroy()
		{

			base.onDestroy();

			unregisterBroadcastReceiver();

			if (userActivitySensor != null)
			{
				mServiceManager.unregisterListener(this, userActivitySensor);
			}

			if (pedometerSensor != null)
			{
				mServiceManager.unregisterListener(this, pedometerSensor);
			}
		}

		private bool checkPermission()
		{
			PackageManager packageManager = this.PackageManager;
			bool mIsSapInstalled = false;
			bool mIsWingtipInstalled = false;
			bool mIsSapPermissionGranted = false;
			bool mIsWingtipPermissionGranted = false;

			if (packageManager == null)
			{
				return false;
			}

			/* If the Remote Sensor service is not installed, return */
			if ((checkPackage(packageManager, REMOTESENSOR_PACKAGE_NAME) == false) || ((checkPackage(packageManager, GEAR_PACKAGE_NAME) == false) && (checkPackage(packageManager, GEAR_FIT_PACKAGE_NAME) == false)))
			{
				return true;
			}

			/*
			 * If the Remote Sensor service is not having permission to access Gear
			 * Manger, launch the Samsung App Store to download Remote Sensor
			 * Service
			 */
			if (checkPackage(packageManager, GEAR_PACKAGE_NAME) == true)
			{
				mIsSapInstalled = true;

				mIsSapPermissionGranted = true; // add
			}

			/*
			 * If the Remote Sensor service is not having permission to access Gear
			 * Fit Manger, launch the Samsung App Store to download Remote Sensor
			 * Service
			 */
			if (checkPackage(packageManager, GEAR_FIT_PACKAGE_NAME) == true)
			{
				mIsWingtipInstalled = true;

				if (packageManager.checkPermission("com.samsung.android.sdk.permission.SESSION_MANAGER_SERVICE", "com.samsung.android.sdk.remotesensor") == PackageManager.PERMISSION_GRANTED)
				{

					mIsWingtipPermissionGranted = true;
				}
			}

			if (((mIsWingtipInstalled == true) && (mIsWingtipPermissionGranted == false)) || ((mIsSapInstalled == true) && (mIsSapPermissionGranted == false)))
			{

				invokeInstallOption(R.@string.rss_permission_msg_str, null);

				return false;
			}

			/*
			 * If the Remote Sensor application is not having permission to access
			 * Remote Sensor Service, launch the Samsung App Store to download
			 * Remote Sensor Application
			 */
			if (packageManager.checkPermission("com.samsung.android.sdk.permission.REMOTE_SENSOR_SERVICE", "com.example.remotesensorsampleapp") != PackageManager.PERMISSION_GRANTED)
			{

				invokeInstallOption(R.@string.permission_msg_str, null);

				return false;
			}

			return true;
		}

		protected internal override void onResume()
		{

			base.onResume();

			if (userActivitySensor != null)
			{
				mServiceManager.registerListener(this, userActivitySensor, SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);
			}

			if (pedometerSensor != null)
			{
				mServiceManager.registerListener(this, pedometerSensor, SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);
			}

		}

		protected internal override void onPause()
		{

			base.onPause();

			if (userActivitySensor != null)
			{
				mServiceManager.unregisterListener(this, userActivitySensor);
			}

			if (pedometerSensor != null)
			{
				mServiceManager.unregisterListener(this, pedometerSensor);
			}

		}

		private void registerBroadcastReceiver()
		{
			mBroadcastState = true;

			IntentFilter filter = new IntentFilter();

			filter.addAction(Intent.ACTION_PACKAGE_ADDED);
			filter.addDataScheme("package");

			this.registerReceiver(btReceiver, filter);
		}

		private void unregisterBroadcastReceiver()
		{

			if ((btReceiver != null) && (mBroadcastState))
			{
				this.unregisterReceiver(btReceiver);
			}

			mBroadcastState = false;
		}

		internal BroadcastReceiver btReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}


			public override void onReceive(Context context, Intent intent)
			{

				if (intent == null)
				{
					return;
				}

				string action = intent.Action;

				if (action == null)
				{
					return;
				}

				if (action.Equals(Intent.ACTION_PACKAGE_ADDED))
				{
					string appName = intent.DataString;

					if (appName != null)
					{
						if (appName.ToLower(Locale.ENGLISH).Contains(GEAR_FIT_PACKAGE_NAME.ToLower(Locale.ENGLISH)))
						{
							outerInstance.invokeInstallOption(R.@string.rss_msg_str, null);
						}
						else if (appName.ToLower(Locale.ENGLISH).Contains(REMOTESENSOR_PACKAGE_NAME.ToLower(Locale.ENGLISH)))
						{
							if (outerInstance.checkPermission())
							{
								outerInstance.initializeeSRS();
							}
						}
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void invokeInstallOption(final int msgID, String msg)
		private void invokeInstallOption(int msgID, string msg)
		{

			DialogInterface.OnClickListener msgClick = new OnClickListenerAnonymousInnerClassHelper(this, msgID);

			AlertDialog.Builder message = new AlertDialog.Builder(this, AlertDialog.THEME_HOLO_DARK);

			if (msgID != -1)
			{
				message.Message = msgID;
			}

			if (msg != null)
			{
				message.Message = msg;
			}

			message.setPositiveButton(R.@string.ok_str, msgClick);
			message.Cancelable = false;

			message.show();

		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly SensorActivity outerInstance;

			private int msgID;

			public OnClickListenerAnonymousInnerClassHelper(SensorActivity outerInstance, int msgID)
			{
				this.outerInstance = outerInstance;
				this.msgID = msgID;
			}

			public override void onClick(DialogInterface dialog, int selButton)
			{
				switch (selButton)
				{
				case DialogInterface.BUTTON_POSITIVE:

					Intent intent = null;

					if ((msgID == R.@string.rss_msg_str) || (msgID == R.@string.rss_permission_msg_str))
					{
						intent = new Intent(Intent.ACTION_VIEW, Uri.parse("samsungapps://ProductDetail/" + "com.samsung.android.sdk.remotesensor"));
					}
					else if (msgID == R.@string.manager_msg_str)
					{
						intent = new Intent(Intent.ACTION_VIEW, Uri.parse("samsungapps://ProductDetail/" + "com.samsung.android.wms"));
					}
					else if (msgID == R.@string.permission_msg_str)
					{
						Uri packageURI = Uri.parse("package:" + PackageName);

						intent = new Intent(Intent.ACTION_DELETE, packageURI);
						intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK;
					}
					else if (msgID == -1)
					{
						finish();
					}

					if (intent != null)
					{
						try
						{
							startActivity(intent);
						}
						catch (ActivityNotFoundException)
						{
						}
					}

					break;

				default:
					break;
				}
			}
		}

		private bool initializeeSRS()
		{
			bool srsInitState = false;

			try
			{
				/// <summary>
				/// initialize() initialize Remote Sensor package. This needs to be
				/// called first. If the device does not support Remote Sensor,
				/// SsdkUnsupportedException is thrown.
				/// </summary>
				remoteSensor.initialize(ApplicationContext);

				srsInitState = true;

			}
			catch (SsdkUnsupportedException e)
			{
				srsInitState = false;

				switch (e.Type)
				{
				case SsdkUnsupportedException.LIBRARY_NOT_INSTALLED:
					registerBroadcastReceiver();

					try
					{
						if ((remoteSensor.isFeatureEnabled(Srs.TYPE_GEAR_MANAGER) == false) && (remoteSensor.isFeatureEnabled(Srs.TYPE_GEAR_FIT_MANAGER) == false))
						{
							Toast.makeText(this, "Install Gear Manager or Gear Fit Manager package", Toast.LENGTH_SHORT).show();
							invokeInstallOption(R.@string.manager_msg_str, null);
							break;
						}

						if (remoteSensor.isFeatureEnabled(Srs.TYPE_REMOTE_SENSOR_SERVICE) == false)
						{
							Toast.makeText(this, "Install Remote Sensor Service package", Toast.LENGTH_SHORT).show();
							invokeInstallOption(R.@string.rss_msg_str, null);
						}

					}
					catch (Exception eRun)
					{
						Toast.makeTextuniquetempvar.show();
					}

					break;

				case SsdkUnsupportedException.LIBRARY_UPDATE_IS_REQUIRED:
					Toast.makeText(this, "Package update is required", Toast.LENGTH_SHORT).show();
					break;

				default:
					Toast.makeTextuniquetempvar.show();
					break;
				}
			}
			catch (System.ArgumentException e)
			{
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
			catch (SecurityException e)
			{
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
				invokeInstallOption(-1, e.Message);
			}

			return srsInitState;
		}

		private bool checkPackage(PackageManager packageManager, string szPackageName)
		{
			PackageInfo packageInfo = null;
			bool bReturn = false;

			try
			{
			packageInfo = packageManager.getPackageInfo(szPackageName, 0);

				if (packageInfo != null)
				{
					return true;
				}

				bReturn = false;

			}
			catch (PackageManager.NameNotFoundException)
			{
				bReturn = false;
			}

			return bReturn;
		}

		public override void onAccuracyChanged(SrsRemoteSensor sensor, int accuracy)
		{

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onSensorValueChanged(final com.samsung.android.sdk.remotesensor.SrsRemoteSensorEvent event)
		public override void onSensorValueChanged(SrsRemoteSensorEvent @event)
		{
			sensorValueChangedThread = new Thread(() =>
			{

				if (@event.sensor.Type == SrsRemoteSensor.TYPE_PEDOMETER)
				{
					mSensorDataPedometer.mValue = "Step count : (" + Convert.ToString(@event.values[0]) + ")";

					setUISensorValue(@event.sensor.Type, mSensorDataPedometer.mValue);

				}
				else if (@event.sensor.Type == SrsRemoteSensor.TYPE_USER_ACTIVITY)
				{
					string data = null;
					int index = (int) @event.values[0];

					if ((index >= 0) && (index <= 2))
					{
						data = userActivityValues[index];
					}
					else
					{
						data = "Invalid Data : " + index;
					}

					mSensorDataUserActivity.mValue = "Activity : (" + data + ")";

					setUISensorValue(@event.sensor.Type, mSensorDataUserActivity.mValue);

				}
				else if (@event.sensor.Type == SrsRemoteSensor.TYPE_WEARING_STATE)
				{
					string data = null;
					int index = (int) @event.values[0];

					if ((index == 0) || (index == 1))
					{
						data = wearingStateValues[index];
					}
					else
					{
						data = "Invalid Data : " + index;
					}

					mSensorDataWearingState.mValue = "Wearing State : (" + data + ")";

					setUISensorValue(@event.sensor.Type, mSensorDataWearingState.mValue);

				}

			});

			sensorValueChangedThread.Start();

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void setUISensorValue(final int sensorType, final String value)
		private void setUISensorValue(int sensorType, string value)
		{
			runOnUiThread(() =>
			{

				try
				{
					if (sensorType == SrsRemoteSensor.TYPE_USER_ACTIVITY)
					{
						mTextUserActivityValue.Text = value.ToString();
					}
					else if (sensorType == SrsRemoteSensor.TYPE_PEDOMETER)
					{
						mTextPedometerValue.Text = value.ToString();
					}
					else if (sensorType == SrsRemoteSensor.TYPE_WEARING_STATE)
					{
						mTextWearingStateValue.Text = value.ToString();
					}

				}
				catch (System.InvalidOperationException e)
				{
					Log.d("RemoteSensorSampleApp", e.Message);

				}

			});
		}

		public override void onSensorDisabled(SrsRemoteSensor sensor)
		{

		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.btn_pedometer:
				PedometerSensorInfo;
				break;

			case R.id.btn_user_activity:
				UserActivitySensorInfo;
				break;

			case R.id.btn_wearing_state:
				WearingStateSensorInfo;
				break;
			case R.id.btn_wearing_state_value:
				WearingStateEvent;

				goto default;
			default:
				break;

			}

		}

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			switch (buttonView.Id)
			{
			case R.id.btn_pedometer_value:
				if (isChecked)
				{
					PedometerEvent;
				}
				else
				{
					stopPedometerEvent();
				}
				break;

			case R.id.btn_user_activity_value:
				if (isChecked)
				{
					UserActivityEvent;
				}
				else
				{
					stopUserActivityEvent();
				}
				break;

			default:
				break;

			}

		}

	}

}