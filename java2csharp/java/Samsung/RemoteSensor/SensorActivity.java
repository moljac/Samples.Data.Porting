package com.example.remotesensorsampleapp;

import java.util.List;
import java.util.Locale;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.ActivityNotFoundException;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.net.Uri;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.remotesensor.Srs;
import com.samsung.android.sdk.remotesensor.SrsRemoteSensor;
import com.samsung.android.sdk.remotesensor.SrsRemoteSensorEvent;
import com.samsung.android.sdk.remotesensor.SrsRemoteSensorManager;
import com.samsung.android.sdk.remotesensor.SrsRemoteSensorManager.EventListener;

public class SensorActivity extends Activity implements EventListener,
		OnClickListener, OnCheckedChangeListener {

	private static final String GEAR_PACKAGE_NAME = "com.samsung.accessory";
	private static final String GEAR_FIT_PACKAGE_NAME = "com.samsung.android.wms";
	private static final String REMOTESENSOR_PACKAGE_NAME = "com.samsung.android.sdk.remotesensor";
	private boolean mBroadcastState = false;

	public static final int USER_ACTIVITY = 0;
	public static final int PEDOMETER = 1;
	public static final int WEARING_STATE = 2;

	private final static String[] userActivityValues = { "Unknown State",
			"Walk", "Run" };

	private final static String[] wearingStateValues = { "Not Wearing",
			"Wearing" };

	public static class SensorData {

		public String mSensorDetails;

		public String mValue;
	}

	static SrsRemoteSensorManager mServiceManager = null;
	Srs remoteSensor = null;

	List<SrsRemoteSensor> activitySensorList = null;
	List<SrsRemoteSensor> pedoSensorList = null;
	List<SrsRemoteSensor> wearingSensorList = null;

	SrsRemoteSensor userActivitySensor = null;
	SrsRemoteSensor pedometerSensor = null;
	SrsRemoteSensor wearingSensor = null;

	Button mBtnUserActivityInfo = null;
	ToggleButton mBtnUserActivityValue = null;
	Button mBtnPedometerInfo = null;
	ToggleButton mBtnPedometerValue = null;
	Button mBtnWearingStateInfo = null;
	Button mBtnWearingStateValue = null;

	TextView mTextUserActivityInfo = null;
	TextView mTextUserActivityValue = null;
	TextView mTextPedometerInfo = null;
	TextView mTextPedometerValue = null;
	TextView mTextWearingStateInfo = null;
	TextView mTextWearingStateValue = null;

	SensorData mSensorDataUserActivity = null;
	SensorData mSensorDataPedometer = null;
	SensorData mSensorDataWearingState = null;

	Thread sensorValueChangedThread;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_sensor);

		remoteSensor = new Srs();

		if (checkPermission()) {
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

		mBtnUserActivityInfo.setOnClickListener(this);
		mBtnUserActivityValue.setOnCheckedChangeListener(this);
		mBtnPedometerInfo.setOnClickListener(this);
		mBtnPedometerValue.setOnCheckedChangeListener(this);
		mBtnWearingStateInfo.setOnClickListener(this);
		mBtnWearingStateValue.setOnClickListener(this);

		mSensorDataUserActivity = new SensorData();
		mSensorDataPedometer = new SensorData();
		mSensorDataWearingState = new SensorData();

	}

	private void getPedometerSensorInfo() {
		mSensorDataPedometer.mSensorDetails = "";
		mSensorDataPedometer.mValue = "";
		stopPedometerEvent();

		pedoSensorList = mServiceManager
				.getSensorList(SrsRemoteSensor.TYPE_PEDOMETER);
		if ((pedoSensorList != null) && (pedoSensorList.size() > 0)) {
			SrsRemoteSensor sensor;
			sensor = pedoSensorList.get(0);
			mSensorDataPedometer.mSensorDetails = sensor.toString();

			mTextPedometerInfo.setText(mSensorDataPedometer.mSensorDetails);

		} else {
			mSensorDataPedometer.mSensorDetails = "Peodometer Sensor is not available";
			mTextPedometerInfo.setText(mSensorDataPedometer.mSensorDetails);

			mSensorDataPedometer.mValue = " ";

			mTextPedometerValue.setText(mSensorDataPedometer.mValue);
		}

	}

	private void getPedometerEvent() {
		if ((mServiceManager != null) && (pedoSensorList != null)
				&& (pedoSensorList.size() > 0)) {

			boolean bRet = false;

			pedometerSensor = pedoSensorList.get(0);

			/**
			 * The rate argument set the interval between the successive events.
			 * Now there are four values : SENSOR_DELAY_SLOW,
			 * SENSOR_DELAY_NORMAL, SENSOR_DELAY_FAST, SENSOR_DELAY_FASTEST. The
			 * maxBatchReportLatency argument specify the maximum batching time.
			 * When the maxBatchReportLatency is set, the events are batched and
			 * delivered later. The unit is microsecond.
			 * 
			 * Note : The maxBatchReportLatency is not supported now. Please set
			 * it to zero. For TYPE_PEDOMETER sensor, the rate argument has no
			 * effects now. The interval is fixed as 5 minutes and it is less
			 * than 5 minutes when the pedometer data is already available as in
			 * case that another application has already registered the
			 * TYPE_PEDOMETER sensor. The power of the wearable device was
			 * considered for interval decision. For TYPE_USER_ACTIVITY sensor,
			 * the event delay is about 4 to 5 seconds. This value corresponds
			 * to 4 to 8 steps and is to prevent a noise.
			 */
			bRet = mServiceManager.registerListener(this, pedometerSensor,
					SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);

			if (bRet == false) {
				Toast.makeText(this,
						"registerListener for pedometer sensor is falied",
						Toast.LENGTH_SHORT).show();
			} else {
				Toast.makeText(this, "getPedometerEvent", Toast.LENGTH_SHORT)
						.show();
			}

		} else {
			mSensorDataPedometer.mValue = "Peodometer Sensor is not available";
			mTextPedometerValue.setText(mSensorDataPedometer.mValue);
		}
	}

	private void stopPedometerEvent() {
		SrsRemoteSensor sensor;
		if ((mServiceManager != null) && (pedoSensorList != null)
				&& (pedoSensorList.size() > 0)) {
			sensor = pedoSensorList.get(0);

			mServiceManager.unregisterListener(this, sensor);
			pedometerSensor = null;

		} else {
			mSensorDataPedometer.mValue = "Pedometer Sensor is not available";

		}

	}

	private void getUserActivitySensorInfo() {

		stopUserActivityEvent();

		mSensorDataUserActivity.mSensorDetails = " ";
		mSensorDataUserActivity.mValue = " ";

		/**
		 * getSensorList() gets the remote sensor list. The supported sensor
		 * types are : TYPE_ALL, TYPE_PEDOMETER, TYPE_USER_ACTIVITY
		 */
		activitySensorList = mServiceManager
				.getSensorList(SrsRemoteSensor.TYPE_USER_ACTIVITY);
		if ((activitySensorList != null) && (activitySensorList.size() > 0)) {
			SrsRemoteSensor sensor;

			sensor = activitySensorList.get(0);
			mSensorDataUserActivity.mSensorDetails = sensor.toString();
			mTextUserActivityInfo
					.setText(mSensorDataUserActivity.mSensorDetails);

		} else {
			mSensorDataUserActivity.mSensorDetails = "User Activity Sensor is not available";
			mSensorDataUserActivity.mValue = " ";

			mTextUserActivityInfo
					.setText(mSensorDataUserActivity.mSensorDetails);
			mTextUserActivityValue.setText(mSensorDataUserActivity.mValue);
		}
	}

	private void getUserActivityEvent() {

		if ((mServiceManager != null) && (activitySensorList != null)
				&& (activitySensorList.size() > 0)) {

			boolean bRet = false;

			userActivitySensor = activitySensorList.get(0);

			/**
			 * The rate argument set the interval between the successive events.
			 * Now there are four values : SENSOR_DELAY_SLOW,
			 * SENSOR_DELAY_NORMAL, SENSOR_DELAY_FAST, SENSOR_DELAY_FASTEST. The
			 * maxBatchReportLatency argument specify the maximum batching time.
			 * When the maxBatchReportLatency is set, the events are batched and
			 * delivered later. The unit is microsecond.
			 * 
			 * Note : The maxBatchReportLatency is not supported now. Please set
			 * it to zero. For TYPE_PEDOMETER sensor, the rate argument has no
			 * effects now. The interval is fixed as 5 minutes and it is less
			 * than 5 minutes when the pedometer data is already available as in
			 * case that another application has already registered the
			 * TYPE_PEDOMETER sensor. The power of the wearable device was
			 * considered for interval decision. For TYPE_USER_ACTIVITY sensor,
			 * the event delay is about 4 to 5 seconds. This value corresponds
			 * to 4 to 8 steps and is to prevent a noise.
			 */
			bRet = mServiceManager.registerListener(this, userActivitySensor,
					SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);

			if (bRet == false) {
				Toast.makeText(this,
						"registerListener for user activity sensor is falied",
						Toast.LENGTH_SHORT).show();
			} else {
				Toast.makeText(this, "getUserActivityEvent", Toast.LENGTH_SHORT)
						.show();
			}

		} else {
			mSensorDataUserActivity.mValue = "User Activity Sensor is not available";
			mTextUserActivityValue.setText(mSensorDataUserActivity.mValue);
		}
	}

	private void stopUserActivityEvent() {
		SrsRemoteSensor sensor;

		if ((mServiceManager != null) && (activitySensorList != null)
				&& (activitySensorList.size() > 0)) {

			sensor = activitySensorList.get(0);

			mServiceManager.unregisterListener(this, sensor);
			userActivitySensor = null;

		} else {
			mSensorDataUserActivity.mValue = "User Activity Sensor is not available";

		}
	}

	private void getWearingStateSensorInfo() {
		mSensorDataWearingState.mSensorDetails = " ";
		mSensorDataWearingState.mValue = " ";

		/**
		 * getSensorList() gets the remote sensor list. The supported sensor
		 * types are : TYPE_ALL, TYPE_PEDOMETER, TYPE_USER_ACTIVITY
		 */
		wearingSensorList = mServiceManager
				.getSensorList(SrsRemoteSensor.TYPE_WEARING_STATE);
		if ((wearingSensorList != null) && (wearingSensorList.size() > 0)) {
			SrsRemoteSensor sensor;

			sensor = wearingSensorList.get(0);
			mSensorDataWearingState.mSensorDetails = sensor.toString();
			mTextWearingStateInfo
					.setText(mSensorDataWearingState.mSensorDetails);

		} else {
			mSensorDataWearingState.mSensorDetails = "Wearing State Sensor is not available";
			mSensorDataWearingState.mValue = " ";

			mTextWearingStateInfo
					.setText(mSensorDataWearingState.mSensorDetails);
			mTextWearingStateValue.setText(mSensorDataWearingState.mValue);
		}
	}

	private void getWearingStateEvent() {
		if ((mServiceManager != null) && (wearingSensorList != null)
				&& (wearingSensorList.size() > 0)) {
			boolean bRet = false;

			wearingSensor = wearingSensorList.get(0);

			bRet = mServiceManager.requestTriggerSensor(this, wearingSensor);

			if (bRet == false) {
				Toast.makeText(
						this,
						"requestTriggerSensor for wearing state sensor is falied",
						Toast.LENGTH_SHORT).show();
			}

		} else {

			mSensorDataWearingState.mValue = "Wearing State Sensor is not available";
			mTextWearingStateValue.setText(mSensorDataWearingState.mValue);
		}
	}

	@Override
	protected void onDestroy() {

		super.onDestroy();

		unregisterBroadcastReceiver();

		if (userActivitySensor != null) {
			mServiceManager.unregisterListener(this, userActivitySensor);
		}

		if (pedometerSensor != null) {
			mServiceManager.unregisterListener(this, pedometerSensor);
		}
	}

	private boolean checkPermission() {
		PackageManager packageManager = this.getPackageManager();
		boolean mIsSapInstalled = false;
		boolean mIsWingtipInstalled = false;
		boolean mIsSapPermissionGranted = false;
		boolean mIsWingtipPermissionGranted = false;

		if (packageManager == null) {
			return false;
		}

		/* If the Remote Sensor service is not installed, return */
		if ((checkPackage(packageManager, REMOTESENSOR_PACKAGE_NAME) == false)
				|| ((checkPackage(packageManager, GEAR_PACKAGE_NAME) == false) && (checkPackage(
						packageManager, GEAR_FIT_PACKAGE_NAME) == false))) {
			return true;
		}

		/*
		 * If the Remote Sensor service is not having permission to access Gear
		 * Manger, launch the Samsung App Store to download Remote Sensor
		 * Service
		 */
		if (checkPackage(packageManager, GEAR_PACKAGE_NAME) == true) {
			mIsSapInstalled = true;

			mIsSapPermissionGranted = true; // add
		}

		/*
		 * If the Remote Sensor service is not having permission to access Gear
		 * Fit Manger, launch the Samsung App Store to download Remote Sensor
		 * Service
		 */
		if (checkPackage(packageManager, GEAR_FIT_PACKAGE_NAME) == true) {
			mIsWingtipInstalled = true;

			if (packageManager
					.checkPermission(
							"com.samsung.android.sdk.permission.SESSION_MANAGER_SERVICE",
							"com.samsung.android.sdk.remotesensor") == PackageManager.PERMISSION_GRANTED) {

				mIsWingtipPermissionGranted = true;
			}
		}

		if (((mIsWingtipInstalled == true) && (mIsWingtipPermissionGranted == false))
				|| ((mIsSapInstalled == true) && (mIsSapPermissionGranted == false))) {

			invokeInstallOption(R.string.rss_permission_msg_str, null);

			return false;
		}

		/*
		 * If the Remote Sensor application is not having permission to access
		 * Remote Sensor Service, launch the Samsung App Store to download
		 * Remote Sensor Application
		 */
		if (packageManager.checkPermission(
				"com.samsung.android.sdk.permission.REMOTE_SENSOR_SERVICE",
				"com.example.remotesensorsampleapp") != PackageManager.PERMISSION_GRANTED) {

			invokeInstallOption(R.string.permission_msg_str, null);

			return false;
		}

		return true;
	}

	@Override
	protected void onResume() {

		super.onResume();

		if (userActivitySensor != null) {
			mServiceManager.registerListener(this, userActivitySensor,
					SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);
		}

		if (pedometerSensor != null) {
			mServiceManager.registerListener(this, pedometerSensor,
					SrsRemoteSensorManager.SENSOR_DELAY_NORMAL, 0);
		}

	}

	@Override
	protected void onPause() {

		super.onPause();

		if (userActivitySensor != null) {
			mServiceManager.unregisterListener(this, userActivitySensor);
		}

		if (pedometerSensor != null) {
			mServiceManager.unregisterListener(this, pedometerSensor);
		}

	}

	private void registerBroadcastReceiver() {
		mBroadcastState = true;

		IntentFilter filter = new IntentFilter();

		filter.addAction(Intent.ACTION_PACKAGE_ADDED);
		filter.addDataScheme("package");

		this.registerReceiver(btReceiver, filter);
	}

	private void unregisterBroadcastReceiver() {

		if ((btReceiver != null) && (mBroadcastState)) {
			this.unregisterReceiver(btReceiver);
		}

		mBroadcastState = false;
	}

	BroadcastReceiver btReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {

			if (intent == null) {
				return;
			}

			String action = intent.getAction();

			if (action == null) {
				return;
			}

			if (action.equals(Intent.ACTION_PACKAGE_ADDED)) {
				String appName = intent.getDataString();

				if (appName != null) {
					if (appName.toLowerCase(Locale.ENGLISH).contains(
							GEAR_FIT_PACKAGE_NAME.toLowerCase(Locale.ENGLISH))) {
						invokeInstallOption(R.string.rss_msg_str, null);
					} else if (appName.toLowerCase(Locale.ENGLISH).contains(
							REMOTESENSOR_PACKAGE_NAME
									.toLowerCase(Locale.ENGLISH))) {
						if (checkPermission()) {
							initializeeSRS();
						}
					}
				}
			}
		}
	};

	private void invokeInstallOption(final int msgID, String msg) {

		DialogInterface.OnClickListener msgClick = new DialogInterface.OnClickListener() {
			@Override
			public void onClick(DialogInterface dialog, int selButton) {
				switch (selButton) {
				case DialogInterface.BUTTON_POSITIVE:

					Intent intent = null;

					if ((msgID == R.string.rss_msg_str)
							|| (msgID == R.string.rss_permission_msg_str)) {
						intent = new Intent(
								Intent.ACTION_VIEW,
								Uri.parse("samsungapps://ProductDetail/"
										+ "com.samsung.android.sdk.remotesensor"));
					} else if (msgID == R.string.manager_msg_str) {
						intent = new Intent(Intent.ACTION_VIEW,
								Uri.parse("samsungapps://ProductDetail/"
										+ "com.samsung.android.wms"));
					} else if (msgID == R.string.permission_msg_str) {
						Uri packageURI = Uri.parse("package:"
								+ getPackageName());

						intent = new Intent(Intent.ACTION_DELETE, packageURI);
						intent.setFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
					} else if (msgID == -1) {
						finish();
					}

					if (intent != null) {
						try {
							startActivity(intent);
						} catch (ActivityNotFoundException eRun) {
						}
					}

					break;

				default:
					break;
				}
			}
		};

		AlertDialog.Builder message = new AlertDialog.Builder(this,
				AlertDialog.THEME_HOLO_DARK);

		if (msgID != -1) {
			message.setMessage(msgID);
		}

		if (msg != null) {
			message.setMessage(msg);
		}

		message.setPositiveButton(R.string.ok_str, msgClick);
		message.setCancelable(false);

		message.show();

	}

	private boolean initializeeSRS() {
		boolean srsInitState = false;

		try {
			/**
			 * initialize() initialize Remote Sensor package. This needs to be
			 * called first. If the device does not support Remote Sensor,
			 * SsdkUnsupportedException is thrown.
			 */
			remoteSensor.initialize(getApplicationContext());

			srsInitState = true;

		} catch (SsdkUnsupportedException e) {
			srsInitState = false;

			switch (e.getType()) {
			case SsdkUnsupportedException.LIBRARY_NOT_INSTALLED:
				registerBroadcastReceiver();

				try {
					if ((remoteSensor.isFeatureEnabled(Srs.TYPE_GEAR_MANAGER) == false)
							&& (remoteSensor
									.isFeatureEnabled(Srs.TYPE_GEAR_FIT_MANAGER) == false)) {
						Toast.makeText(
								this,
								"Install Gear Manager or Gear Fit Manager package",
								Toast.LENGTH_SHORT).show();
						invokeInstallOption(R.string.manager_msg_str, null);
						break;
					}

					if (remoteSensor
							.isFeatureEnabled(Srs.TYPE_REMOTE_SENSOR_SERVICE) == false) {
						Toast.makeText(this,
								"Install Remote Sensor Service package",
								Toast.LENGTH_SHORT).show();
						invokeInstallOption(R.string.rss_msg_str, null);
					}

				} catch (RuntimeException eRun) {
					Toast.makeText(this,
							"RuntimeException = " + eRun.getMessage(),
							Toast.LENGTH_SHORT).show();
				}

				break;

			case SsdkUnsupportedException.LIBRARY_UPDATE_IS_REQUIRED:
				Toast.makeText(this, "Package update is required",
						Toast.LENGTH_SHORT).show();
				break;

			default:
				Toast.makeText(this,
						"SsdkUnsupportedException = " + e.getType(),
						Toast.LENGTH_SHORT).show();
				break;
			}
		} catch (IllegalArgumentException e) {
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		} catch (SecurityException e) {
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
			invokeInstallOption(-1, e.getMessage());
		}

		return srsInitState;
	}

	private boolean checkPackage(PackageManager packageManager,
			String szPackageName) {
		PackageInfo packageInfo = null;
		boolean bReturn = false;

		try {
			packageInfo = packageManager.getPackageInfo(szPackageName, 0);

			if (packageInfo != null) {
				return true;
			}

			bReturn = false;

		} catch (PackageManager.NameNotFoundException e1) {
			bReturn = false;
		}

		return bReturn;
	}

	@Override
	public void onAccuracyChanged(SrsRemoteSensor sensor, int accuracy) {

	}

	@Override
	public void onSensorValueChanged(final SrsRemoteSensorEvent event) {
		sensorValueChangedThread = new Thread(new Runnable() {

			@Override
			public void run() {
				if (event.sensor.getType() == SrsRemoteSensor.TYPE_PEDOMETER) {
					mSensorDataPedometer.mValue = "Step count : ("
							+ Float.toString(event.values[0]) + ")";

					setUISensorValue(event.sensor.getType(),
							mSensorDataPedometer.mValue);

				} else if (event.sensor.getType() == SrsRemoteSensor.TYPE_USER_ACTIVITY) {
					String data = null;
					int index = (int) event.values[0];

					if ((index >= 0) && (index <= 2)) {
						data = userActivityValues[index];
					} else {
						data = "Invalid Data : " + index;
					}

					mSensorDataUserActivity.mValue = "Activity : (" + data
							+ ")";

					setUISensorValue(event.sensor.getType(),
							mSensorDataUserActivity.mValue);

				} else if (event.sensor.getType() == SrsRemoteSensor.TYPE_WEARING_STATE) {
					String data = null;
					int index = (int) event.values[0];

					if ((index == 0) || (index == 1)) {
						data = wearingStateValues[index];
					} else {
						data = "Invalid Data : " + index;
					}

					mSensorDataWearingState.mValue = "Wearing State : (" + data
							+ ")";

					setUISensorValue(event.sensor.getType(),
							mSensorDataWearingState.mValue);

				}

			}
		});

		sensorValueChangedThread.start();

	}

	private void setUISensorValue(final int sensorType, final String value) {
		runOnUiThread(new Runnable() {

			@Override
			public void run() {
				try {
					if (sensorType == SrsRemoteSensor.TYPE_USER_ACTIVITY) {
						mTextUserActivityValue.setText(value.toString());
					} else if (sensorType == SrsRemoteSensor.TYPE_PEDOMETER) {
						mTextPedometerValue.setText(value.toString());
					} else if (sensorType == SrsRemoteSensor.TYPE_WEARING_STATE) {
						mTextWearingStateValue.setText(value.toString());
					}

				} catch (IllegalStateException e) {
					Log.d("RemoteSensorSampleApp", e.getMessage());

				}

			}
		});
	}

	@Override
	public void onSensorDisabled(SrsRemoteSensor sensor) {

	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.btn_pedometer:
			getPedometerSensorInfo();
			break;

		case R.id.btn_user_activity:
			getUserActivitySensorInfo();
			break;

		case R.id.btn_wearing_state:
			getWearingStateSensorInfo();
			break;
		case R.id.btn_wearing_state_value:
			getWearingStateEvent();

		default:
			break;

		}

	}

	@Override
	public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
		switch (buttonView.getId()) {
		case R.id.btn_pedometer_value:
			if (isChecked) {
				getPedometerEvent();
			} else {
				stopPedometerEvent();
			}
			break;

		case R.id.btn_user_activity_value:
			if (isChecked) {
				getUserActivityEvent();
			} else {
				stopUserActivityEvent();
			}
			break;

		default:
			break;

		}

	}

}
