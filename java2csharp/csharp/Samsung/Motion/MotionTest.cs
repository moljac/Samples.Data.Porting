using System.Text;

namespace com.samsung.sdk.motion.test
{

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Smotion = com.samsung.android.sdk.motion.Smotion;
	using SmotionActivity = com.samsung.android.sdk.motion.SmotionActivity;
	using SmotionActivityNotification = com.samsung.android.sdk.motion.SmotionActivityNotification;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using AudioManager = android.media.AudioManager;
	using SoundPool = android.media.SoundPool;
	using Bundle = android.os.Bundle;
	using Looper = android.os.Looper;
	using Time = android.text.format.Time;
	using Log = android.util.Log;
	using Gravity = android.view.Gravity;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using AdapterView = android.widget.AdapterView;
	using OnItemSelectedListener = android.widget.AdapterView.OnItemSelectedListener;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using RadioButton = android.widget.RadioButton;
	using RadioGroup = android.widget.RadioGroup;
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;
	using RelativeLayout = android.widget.RelativeLayout;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;
	using ToggleButton = android.widget.ToggleButton;

	public class MotionTest : Activity
	{

		internal const int MODE_CALL = 0;

		internal const int MODE_PEDOMETER = 1;

		internal const int MODE_PEDOMETER_PERIODIC = 2;

		internal const int MODE_ACTIVITY = 3;

		internal const int MODE_ACTIVITY_NOTIFICATION = 4;

		internal static int mTestMode = 0;

		private Smotion mMotion;

		private static SoundPool mSoundPool;

		private MotionCall mCall;

		private MotionPedometer mPedometer;

		private MotionActivity mActivity;

		private MotionActivityNotification mActivityNotification;

		private RadioGroup mRadioGroup_activity_mode = null;

		private RadioButton mRadio_acitivity_realtime = null;

		private RadioButton mRadio_acitivity_batch = null;

		private RadioButton mRadio_acitivity_all = null;

		private LinearLayout mCheckBox_Group = null;

		private LinearLayout mCheckBox_Group2 = null;

		private CheckBox mCheckBox_Stationary = null;

		private CheckBox mCheckBox_Walk = null;

		private CheckBox mCheckBox_Run = null;

		private CheckBox mCheckBox_Vehicle = null;

		private CheckBox mCheckBox_Activity_Periodic = null;

		private EditText mEditText_Activity_Periodic_input = null;

		private LinearLayout mLinearLayout_Activity_Periodic_interval = null;

		private Spinner mSpin = null;

		private ToggleButton mBtn_start = null;

		private static bool mIsTesting = false;

		private static int mSoundId;

		private static TextView mTv_timestamp = null;

		private static TextView mTv_result1 = null;

		private static TextView mTv_result2 = null;

		private static Button mBtn_updateInfo = null;

		private int mActivityMode = SmotionActivity.Info.MODE_REALTIME;

		private RelativeLayout mRelativeLayout;

		internal static bool mIsUpdateInfo = false;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;
			if (initialize())
			{
				mBtn_start.Enabled = true;
				mSpin.Clickable = true;
				mTv_timestamp.Visibility = View.VISIBLE;
			}
			else
			{
				MotionTest.displayData(0, "Not supported", "");
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			stopMotionTest();
		}

		public virtual bool initialize()
		{
			mTv_timestamp = (TextView) findViewById(R.id.res_timestamp);
			mTv_result1 = (TextView) findViewById(R.id.res_result1);
			mTv_result2 = (TextView) findViewById(R.id.res_result2);
			mRelativeLayout = (RelativeLayout) findViewById(R.id.res_layout);

			// Smotion iniialize
			mMotion = new Smotion();

			try
			{
				mMotion.initialize(this);
			}
			catch (System.ArgumentException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}
			catch (SsdkUnsupportedException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}

			// SoundPool
			this.VolumeControlStream = AudioManager.STREAM_MUSIC;
			mSoundPool = new SoundPool(10, AudioManager.STREAM_MUSIC, 0);
			mSoundId = mSoundPool.load(ApplicationContext, R.raw.dingdong, 0);

			// RadioGroup
			mRadioGroup_activity_mode = (RadioGroup) findViewById(R.id.radioGroup_activity_mode);
			mRadioGroup_activity_mode.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			// RadioButton
			mRadio_acitivity_realtime = (RadioButton) findViewById(R.id.radio_realtime);
			mRadio_acitivity_batch = (RadioButton) findViewById(R.id.radio_batch);
			mRadio_acitivity_all = (RadioButton) findViewById(R.id.radio_all);

			// ToggleButton Start/Stop
			mBtn_start = (ToggleButton) findViewById(R.id.toggle_start);
			mBtn_start.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			// CheckBox Group
			mCheckBox_Group = (LinearLayout) findViewById(R.id.checkbox_group);
			mCheckBox_Group2 = (LinearLayout) findViewById(R.id.checkbox_group2);
			mCheckBox_Stationary = (CheckBox) findViewById(R.id.cb_stationary);
			mCheckBox_Walk = (CheckBox) findViewById(R.id.cb_walk);
			mCheckBox_Run = (CheckBox) findViewById(R.id.cb_run);
			mCheckBox_Vehicle = (CheckBox) findViewById(R.id.cb_vehicle);
			mCheckBox_Activity_Periodic = (CheckBox) findViewById(R.id.cb_activity_realtime_periodic);
			mEditText_Activity_Periodic_input = (EditText) findViewById(R.id.et_activity_realtime_periodic_input);
			mLinearLayout_Activity_Periodic_interval = (LinearLayout) findViewById(R.id.interval_group);

			// Spinner
			mSpin = (Spinner) findViewById(R.id.spinner_mode);
			mSpin.OnItemSelectedListener = new OnItemSelectedListenerAnonymousInnerClassHelper(this);

			mCheckBox_Activity_Periodic.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			mBtn_updateInfo = (Button) findViewById(R.id.btn_activity_batch_updateinfo);
			mBtn_updateInfo.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			return true;
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly MotionTest outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onCheckedChanged(RadioGroup group, int checkedId)
			{
				// TODO Auto-generated method stub
				switch (checkedId)
				{
					case R.id.radio_realtime:
						outerInstance.mActivityMode = SmotionActivity.Info.MODE_REALTIME;
						outerInstance.mCheckBox_Activity_Periodic.Visibility = View.VISIBLE;
						mBtn_updateInfo.Visibility = View.VISIBLE;
						if (outerInstance.mCheckBox_Activity_Periodic.Checked)
						{
							outerInstance.mCheckBox_Activity_Periodic.Checked = false;
						}
						break;
					case R.id.radio_batch:
						outerInstance.mActivityMode = SmotionActivity.Info.MODE_BATCH;
						outerInstance.mCheckBox_Activity_Periodic.Visibility = View.GONE;
						mBtn_updateInfo.Visibility = View.VISIBLE;
						mBtn_updateInfo.Enabled = false;
						outerInstance.mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
						if (outerInstance.mCheckBox_Activity_Periodic.Checked)
						{
							outerInstance.mCheckBox_Activity_Periodic.Checked = false;
						}
						break;
					case R.id.radio_all:
						outerInstance.mActivityMode = SmotionActivity.Info.MODE_ALL;
						outerInstance.mCheckBox_Activity_Periodic.Visibility = View.VISIBLE;
						mBtn_updateInfo.Visibility = View.VISIBLE;
						mBtn_updateInfo.Enabled = false;
						if (outerInstance.mCheckBox_Activity_Periodic.Checked)
						{
							outerInstance.mCheckBox_Activity_Periodic.Checked = false;
						}
						break;
					default:
						break;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : ToggleButton.OnClickListener
		{
			private readonly MotionTest outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				// Perform action on clicks
				if (outerInstance.mBtn_start.Checked)
				{
					outerInstance.startMotionTest();
				}
				else
				{
					outerInstance.stopMotionTest();
				}
			}
		}

		private class OnItemSelectedListenerAnonymousInnerClassHelper : AdapterView.OnItemSelectedListener
		{
			private readonly MotionTest outerInstance;

			public OnItemSelectedListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemSelected<T1>(AdapterView<T1> adapterView, View view, int position, long flag)
			{
				// TODO Auto-generated method stub
				mTestMode = position;
				outerInstance.mBtn_start.Enabled = true;
				switch (mTestMode)
				{
					case MODE_CALL:
						mBtn_updateInfo.Visibility = View.GONE;
						outerInstance.setTextView(mTv_result1, true);
						if (!outerInstance.mMotion.isFeatureEnabled(Smotion.TYPE_CALL))
						{
							outerInstance.mBtn_start.Enabled = false;
							MotionTest.displayData(0, "Not supported", "");
						}
						else
						{
							if (outerInstance.mCall == null)
							{
								outerInstance.mCall = new MotionCall(Looper.MainLooper, outerInstance.mMotion);
							}
							outerInstance.mCall.initialize();
						}
						break;
					case MODE_PEDOMETER:
					case MODE_PEDOMETER_PERIODIC:
						mBtn_updateInfo.Visibility = View.GONE;
						outerInstance.setTextView(mTv_result1, true);
						if (!outerInstance.mMotion.isFeatureEnabled(Smotion.TYPE_PEDOMETER))
						{
							outerInstance.mBtn_start.Enabled = false;
							MotionTest.displayData(0, "Not supported", "");
						}
						else
						{
							if (outerInstance.mPedometer == null)
							{
								bool isPedometerUpDownAvailable = outerInstance.mMotion.isFeatureEnabled(Smotion.TYPE_PEDOMETER_WITH_UPDOWN_STEP);
								outerInstance.mPedometer = new MotionPedometer(Looper.MainLooper, outerInstance.mMotion, isPedometerUpDownAvailable);
							}
							outerInstance.mPedometer.initialize();
						}
						break;
					case MODE_ACTIVITY:

						if (!outerInstance.mMotion.isFeatureEnabled(Smotion.TYPE_ACTIVITY))
						{
							outerInstance.mBtn_start.Enabled = false;
							outerInstance.mRadio_acitivity_realtime.Enabled = false;
							outerInstance.mRadio_acitivity_batch.Enabled = false;
							outerInstance.mRadio_acitivity_all.Enabled = false;
							outerInstance.mCheckBox_Activity_Periodic.Enabled = false;
							MotionTest.displayData(0, "Not supported", "");
						}
						else
						{
							outerInstance.mRadioGroup_activity_mode.check(R.id.radio_realtime);
							outerInstance.setTextView(mTv_result1, false);
							mBtn_updateInfo.Visibility = View.VISIBLE;
							mBtn_updateInfo.Enabled = false;
							if (outerInstance.mActivity == null)
							{
								outerInstance.mActivity = new MotionActivity(Looper.MainLooper, outerInstance.mMotion);
							}
							outerInstance.mActivity.initialize();
							outerInstance.displayActivityStatus(outerInstance.mActivity.checkActivityStatus());

						}
						break;
					case MODE_ACTIVITY_NOTIFICATION:
						mBtn_updateInfo.Visibility = View.GONE;
						outerInstance.setTextView(mTv_result1, false);
						if (!outerInstance.mMotion.isFeatureEnabled(Smotion.TYPE_ACTIVITY_NOTIFICATION))
						{
							outerInstance.mBtn_start.Enabled = false;
							outerInstance.mCheckBox_Stationary.Enabled = false;
							outerInstance.mCheckBox_Walk.Enabled = false;
							outerInstance.mCheckBox_Run.Enabled = false;
							outerInstance.mCheckBox_Vehicle.Enabled = false;
							MotionTest.displayData(0, "Not supported", "");
						}
						else
						{
							if (outerInstance.mActivityNotification == null)
							{
								outerInstance.mActivityNotification = new MotionActivityNotification(Looper.MainLooper, outerInstance.mMotion);
							}
							outerInstance.mActivityNotification.initialize();
							outerInstance.displayActivityStatus(outerInstance.mActivityNotification.checkActivityStatus());
						}
						break;
					default:
						break;
				}
				outerInstance.initializeView();
			}

			public override void onNothingSelected<T1>(AdapterView<T1> adapterView)
			{
				// TODO Auto-generated method stub
			}
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
		{
			private readonly MotionTest outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
			{
				// TODO Auto-generated method stub
				if (isChecked)
				{
					outerInstance.mLinearLayout_Activity_Periodic_interval.Visibility = View.VISIBLE;
				}
				else
				{
					outerInstance.mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
					string str = outerInstance.mEditText_Activity_Periodic_input.Text.ToString();
					if (str.Equals("") || long.Parse(str) <= 0)
					{
						str = "10";
						outerInstance.mEditText_Activity_Periodic_input.Text = str;
					}
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MotionTest outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View arg0)
			{
				// TODO Auto-generated method stub
				if (outerInstance.mActivity != null)
				{
					if (outerInstance.mActivity.UpdateInfoBatchModeSupport || outerInstance.mActivityMode == SmotionActivity.Info.MODE_REALTIME)
					{
						mIsUpdateInfo = true;
						outerInstance.mActivity.updateInfo();
					}
					else
					{
						if (outerInstance.mActivityMode == SmotionActivity.Info.MODE_ALL)
						{
							Log.d("activity", "mode all");
							mIsUpdateInfo = true;
							outerInstance.mActivity.updateInfo();
						}
						Toast.makeText(ApplicationContext, "This device doesn't support updateInfo() in batch mode!!", Toast.LENGTH_SHORT).show();
					}

				}
			}
		}

		private void startMotionTest()
		{
			mIsTesting = true;
			mSpin.Enabled = false;
			switch (mTestMode)
			{
				case MODE_CALL:
					mCall.start();
					break;
				case MODE_PEDOMETER:
				case MODE_PEDOMETER_PERIODIC:
					mPedometer.start(mTestMode);
					break;
				case MODE_ACTIVITY:
					bool isPeriodicMode = mCheckBox_Activity_Periodic.Checked;
					string str = mEditText_Activity_Periodic_input.Text.ToString();
					if (str.Equals("") && isPeriodicMode)
					{
						showAlertMessage("Input periodic time!");
					}
					else
					{
						long interval = long.Parse(str) * 1000;
						if (interval <= 0)
						{
							showAlertMessage("Periodic time must be an excess of 0!");
						}
						else
						{
							mActivity.start(mActivityMode, isPeriodicMode, interval);
							mRadio_acitivity_realtime.Enabled = false;
							mRadio_acitivity_batch.Enabled = false;
							mRadio_acitivity_all.Enabled = false;
							mBtn_updateInfo.Enabled = true;
							mCheckBox_Activity_Periodic.Visibility = View.GONE;
							mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
						}
					}
					break;
				case MODE_ACTIVITY_NOTIFICATION:
					bool[] notificationFilter = new bool[] {false, true, false, false, false};
					notificationFilter[0] = mCheckBox_Stationary.Checked;
					notificationFilter[1] = mCheckBox_Walk.Checked;
					notificationFilter[2] = mCheckBox_Run.Checked;
					notificationFilter[3] = mCheckBox_Vehicle.Checked;

					int count = 0;
					for (int i = 0; i < notificationFilter.Length; i++)
					{
						if (notificationFilter[i])
						{
							addActivity(i);
							count++;
						}
					}

					if (count == 0)
					{
						showAlertMessage("Filter is not selected");
					}
					else
					{
						mActivityNotification.start();

						mCheckBox_Stationary.Enabled = false;
						mCheckBox_Walk.Enabled = false;
						mCheckBox_Run.Enabled = false;
						mCheckBox_Vehicle.Enabled = false;
					}
					break;
				default:
					break;
			}
		}

		private void stopMotionTest()
		{
			if (mIsTesting)
			{
				mIsTesting = false;
				mSpin.Enabled = true;
				switch (mTestMode)
				{
					case MODE_CALL:
						mCall.stop();
						break;
					case MODE_PEDOMETER:
					case MODE_PEDOMETER_PERIODIC:
						mPedometer.stop();
						break;
					case MODE_ACTIVITY:
						mActivity.stop();
						mRadio_acitivity_realtime.Enabled = true;
						mRadio_acitivity_batch.Enabled = true;
						mRadio_acitivity_all.Enabled = true;
						mCheckBox_Activity_Periodic.Enabled = true;
						mEditText_Activity_Periodic_input.Enabled = true;
						mBtn_updateInfo.Visibility = View.VISIBLE;
						mBtn_updateInfo.Enabled = false;
						if (mActivityMode == SmotionActivity.Info.MODE_REALTIME || mActivityMode == SmotionActivity.Info.MODE_ALL)
						{
							mCheckBox_Activity_Periodic.Visibility = View.VISIBLE;
						}
						mBtn_updateInfo.Enabled = false;
						if (mCheckBox_Activity_Periodic.Checked)
						{
							mCheckBox_Activity_Periodic.Visibility = View.VISIBLE;
							mLinearLayout_Activity_Periodic_interval.Visibility = View.VISIBLE;
						}
						break;
					case MODE_ACTIVITY_NOTIFICATION:
						mCheckBox_Stationary.Enabled = true;
						mCheckBox_Walk.Enabled = true;
						mCheckBox_Run.Enabled = true;
						mCheckBox_Vehicle.Enabled = true;
						mActivityNotification.stop();
						break;
					default:
						break;
				}
			}
		}

		private void addActivity(int activity_type)
		{
			switch (activity_type)
			{
				case 0:
					mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_STATIONARY);
					break;
				case 1:
					mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_WALK);
					break;
				case 2:
					mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_RUN);
					break;
				case 3:
					mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_VEHICLE);
					break;
				default:
					break;
			}
		}

		private void initializeView()
		{
			switch (mTestMode)
			{
				case MODE_CALL:
					mTv_timestamp.Visibility = View.VISIBLE;
					mTv_result1.Visibility = View.VISIBLE;
					mTv_result2.Visibility = View.GONE;
					mCheckBox_Group.Visibility = View.GONE;
					mCheckBox_Group2.Visibility = View.GONE;
					mRadioGroup_activity_mode.Visibility = View.GONE;
					mCheckBox_Activity_Periodic.Visibility = View.GONE;
					mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
					break;
				case MODE_PEDOMETER:
				case MODE_PEDOMETER_PERIODIC:
					mTv_timestamp.Visibility = View.VISIBLE;
					mTv_result1.Visibility = View.VISIBLE;
					mTv_result2.Visibility = View.VISIBLE;
					mCheckBox_Group.Visibility = View.GONE;
					mCheckBox_Group2.Visibility = View.GONE;
					mRadioGroup_activity_mode.Visibility = View.GONE;
					mCheckBox_Activity_Periodic.Visibility = View.GONE;
					mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
					break;
				case MODE_ACTIVITY:
					mTv_timestamp.Visibility = View.GONE;
					mTv_result1.Visibility = View.VISIBLE;
					mTv_result2.Visibility = View.VISIBLE;
					mCheckBox_Group.Visibility = View.GONE;
					mCheckBox_Group2.Visibility = View.GONE;
					mRadioGroup_activity_mode.Visibility = View.VISIBLE;
					if (mRadio_acitivity_batch.Checked)
					{
						mRadio_acitivity_batch.Checked = false;
						mRadio_acitivity_realtime.Checked = true;
					}
					mCheckBox_Activity_Periodic.Visibility = View.VISIBLE;
					if (mCheckBox_Activity_Periodic.Checked)
					{
						mCheckBox_Activity_Periodic.Checked = false;
					}
					mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
					break;
				case MODE_ACTIVITY_NOTIFICATION:
					mTv_timestamp.Visibility = View.GONE;
					mTv_result1.Visibility = View.VISIBLE;
					mTv_result2.Visibility = View.VISIBLE;
					mCheckBox_Group.Visibility = View.VISIBLE;
					mCheckBox_Group2.Visibility = View.VISIBLE;
					mRadioGroup_activity_mode.Visibility = View.GONE;
					mCheckBox_Activity_Periodic.Visibility = View.GONE;
					mLinearLayout_Activity_Periodic_interval.Visibility = View.GONE;
					break;
				default:
					break;
			}
		}

		private void showAlertMessage(string msg)
		{
			if (msg == null)
			{
				throw new System.NullReferenceException("AlertMessage is Null!!");
			}

			AlertDialog.Builder builder = new AlertDialog.Builder(this);
			builder.Title = "Error";
			builder.Message = msg;
			builder.setNegativeButton("close", new OnClickListenerAnonymousInnerClassHelper(this));
			builder.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly MotionTest outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MotionTest outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub
				outerInstance.mBtn_start.Checked = false;
				outerInstance.mSpin.Enabled = true;
			}
		}

		internal static void playSound()
		{
			mSoundPool.play(mSoundId, 100, 100, 1, 0, 1.0f);
		}

		internal static void displayData(long timestamp, string str1)
		{
			// TODO Auto-generated method stub
			if (mTv_timestamp.Visibility == View.VISIBLE)
			{
				if (timestamp == 0)
				{
					mTv_timestamp.Text = "[00:00:00]";
				}
				else
				{
					Time time = new Time();
					time.set(timestamp);
					Formatter form = new Formatter();
					form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
					mTv_timestamp.Text = (new StringBuilder("[")).Append(form.ToString()).Append("]").ToString();
					form.close();
				}
			}
			if (mTv_result1.Visibility == View.VISIBLE)
			{
				mTv_result1.Text = (new StringBuilder(str1)).ToString();
			}
			if (mTv_result2.Visibility == View.VISIBLE)
			{
				mTv_result2.Text = "";
			}
		}

		internal static void displayData(long timestamp, string str1, string str2)
		{
			// TODO Auto-generated method stub
			displayData(timestamp, str1);
			mTv_result2.Text = (new StringBuilder(str2)).ToString();
		}

		internal virtual void displayActivityStatus(StringBuilder sb)
		{
			if (sb == null)
			{
				return;
			}
			Toast.makeText(ApplicationContext, sb.ToString(), Toast.LENGTH_SHORT).show();
		}

		internal virtual void setTextView(View v, bool isActivity)
		{

			mRelativeLayout.removeView(v);

			TextView tx = (TextView) v;
			RelativeLayout.LayoutParams @params = new RelativeLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
			tx.Text = "Ready";
			tx.Gravity = Gravity.CENTER_HORIZONTAL;
			@params.addRule(RelativeLayout.BELOW, R.id.spinner_mode);

			if (isActivity)
			{
				@params.topMargin = 200;
			}
			else
			{
				@params.topMargin = 400;
			}

			mRelativeLayout.addView(tx, @params);
		}

	}

}