using System.Text;

namespace com.samsung.sdk.motion.test
{

	using Smotion = com.samsung.android.sdk.motion.Smotion;
	using SmotionActivity = com.samsung.android.sdk.motion.SmotionActivity;
	using Info = com.samsung.android.sdk.motion.SmotionActivity.Info;

	using SuppressLint = android.annotation.SuppressLint;
	using Handler = android.os.Handler;
	using Looper = android.os.Looper;
	using Time = android.text.format.Time;


	internal class MotionActivity
	{

		private SmotionActivity mActivity = null;

		private int mMode = SmotionActivity.Info.MODE_REALTIME;

		private string[] mQueue = new string[2];

		private Timer mTimer;

		private SmotionActivity.Info[] mInfo;

		private long mInterval;

		private bool mIsPeriodicMode;

		private bool mIsStarting = false;

		internal MotionActivity(Looper looper, Smotion motion)
		{
			mActivity = new SmotionActivity(looper, motion);
			initialize();
		}

		internal virtual void start(int mode, bool isPeriodicMode, long interval)
		{
			initialize();
			mMode = mode;
			mIsPeriodicMode = isPeriodicMode;
			mInterval = interval;
			mActivity.start(mMode, changeListener);
			if (mIsPeriodicMode)
			{
				startTimer();
			}
			mIsStarting = true;
			MotionTest.displayData(0, getMode(mode), "");
		}

		internal virtual void stop()
		{
			if (mIsStarting)
			{
				mActivity.stop();
			}
			if (mIsPeriodicMode)
			{
				stopTimer();
			}
			mIsStarting = false;
			initialize();
		}

		internal virtual void initialize()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Ready");
			MotionTest.displayData(0, sb.ToString());

			mInfo = new SmotionActivity.Info[1];
			mQueue = new string[2];
		}

		internal virtual void updateInfo()
		{
			if (mActivity == null)
			{
				return;
			}
			mActivity.updateInfo();
		}

		internal virtual bool UpdateInfoBatchModeSupport
		{
			get
			{
				return mActivity.UpdateInfoBatchModeSupport;
			}
		}

		private void displayData(int mode, SmotionActivity.Info[] info)
		{
			// TODO Auto-generated method stub
			string str = getMode(mMode);
			StringBuilder sb = new StringBuilder();
			if (mIsPeriodicMode && (mode == SmotionActivity.Info.MODE_REALTIME))
			{
				sb.Append("Periodic Mode : " + mInterval / 1000 + " sec" + "\n");
			}

			sb.Append("<" + getMode(mode) + ">");
			if (MotionTest.mIsUpdateInfo)
			{
				if (mMode == SmotionActivity.Info.MODE_ALL)
				{
					if (UpdateInfoBatchModeSupport)
					{
						sb.Append(" - Update" + " All" + " Data");
					}
					else
					{
						sb.Append(" - Update " + getMode(mode) + " Data");
					}
				}
				else
				{
					sb.Append(" - Update " + getMode(mode) + " Data");
				}
				MotionTest.mIsUpdateInfo = false;
			}
			sb.Append("\n");
			for (int i = 0; i < info.Length; i++)
			{
				long timestamp = 0;
				if (mIsPeriodicMode)
				{
					if (mode == SmotionActivity.Info.MODE_REALTIME)
					{
						timestamp = DateTimeHelperClass.CurrentUnixTimeMillis();
					}
					else if (mode == SmotionActivity.Info.MODE_BATCH)
					{
						timestamp = info[i].TimeStamp;
					}
				}
				else
				{
					timestamp = info[i].TimeStamp;
				}

				Time time = new Time();
				time.set(timestamp);
				Formatter form = new Formatter();
				form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
				sb.Append("[" + form.ToString() + "] ");
				form.close();
				sb.Append(getStatus(info[i].Status));
				sb.Append("(" + getAccuracy(info[i].Accuracy) + ")");
				sb.Append("\n");
			}

			switch (mode)
			{
				case SmotionActivity.Info.MODE_REALTIME:
					mQueue[0] = sb.ToString();
					break;

				case SmotionActivity.Info.MODE_BATCH:
					mQueue[1] = sb.ToString();
					break;
				default:
					break;
			}

			sb = new StringBuilder();
			for (int i = 0; i < mQueue.Length; i++)
			{
				if (mQueue[i] != null)
				{
					sb.Append(mQueue[i] + "\n");
				}
			}
			if (str != null)
			{
				MotionTest.displayData(0, str, sb.ToString());
			}
		}

		private string getStatus(int status)
		{
			string str = null;
			switch (status)
			{
				case SmotionActivity.Info.STATUS_UNKNOWN:
					str = "Unknown";
					break;
				case SmotionActivity.Info.STATUS_STATIONARY:
					str = "Stationary";
					break;
				case SmotionActivity.Info.STATUS_WALK:
					str = "Walk";
					break;
				case SmotionActivity.Info.STATUS_RUN:
					str = "Run";
					break;
				case SmotionActivity.Info.STATUS_VEHICLE:
					str = "Vehicle";
					break;
				default:
					break;
			}
			return str;
		}

		private string getAccuracy(int accuracy)
		{
			string str = null;
			switch (accuracy)
			{
				case SmotionActivity.Info.ACCURACY_LOW:
					str = "Low";
					break;
				case SmotionActivity.Info.ACCURACY_MID:
					str = "Mid";
					break;
				case SmotionActivity.Info.ACCURACY_HIGH:
					str = "High";
					break;
				default:
					break;
			}
			return str;
		}

		private string getMode(int mode)
		{
			string str = null;
			switch (mode)
			{
				case SmotionActivity.Info.MODE_REALTIME:
					str = "Real time";
					break;
				case SmotionActivity.Info.MODE_BATCH:
					str = "Batch";
					break;
				case SmotionActivity.Info.MODE_ALL:
					str = "ALL";
					break;
				default:
					break;
			}
			return str;
		}

		internal virtual StringBuilder checkActivityStatus()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Status valid check\n");
			sb.Append("Stationary : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_STATIONARY) + "\n");
			sb.Append("Walk : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_WALK) + "\n");
			sb.Append("Run : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_STATIONARY) + "\n");
			sb.Append("Vehicle : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_RUN) + "\n");
			return sb;
		}

		private void startTimer()
		{
			if (mTimer == null)
			{
				mTimer = new Timer();
				mTimer.schedule(new MyTimer(this), 0, mInterval);
			}
		}

		private void stopTimer()
		{
			if (mTimer != null)
			{
				mTimer.cancel();
				mTimer = null;
			}
		}

		private SmotionActivity.ChangeListener changeListener = new ChangeListenerAnonymousInnerClassHelper();

		private class ChangeListenerAnonymousInnerClassHelper : SmotionActivity.ChangeListener
		{
			public ChangeListenerAnonymousInnerClassHelper()
			{
			}


			public override void onChanged(int mode, SmotionActivity.Info[] infoArray)
			{
				if (outerInstance.mIsPeriodicMode && (mode == SmotionActivity.Info.MODE_REALTIME))
				{
					return;
				}
				MotionTest.playSound();
				outerInstance.displayData(mode, infoArray);

			}
		}

		internal class MyTimer : TimerTask
		{
			private readonly MotionActivity outerInstance;

			public MyTimer(MotionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void run()
			{
				// TODO Auto-generated method stub
				outerInstance.mInfo[0] = outerInstance.mActivity.Info;
				handler.sendEmptyMessage(0);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("HandlerLeak") private final android.os.Handler handler = new android.os.Handler()
		private readonly Handler handler = new HandlerAnonymousInnerClassHelper();

		private class HandlerAnonymousInnerClassHelper : Handler
		{
			public HandlerAnonymousInnerClassHelper()
			{
			}

			public override void handleMessage(android.os.Message msg)
			{
				// TODO Auto-generated method stub
				if (outerInstance.mInfo[0] != null)
				{
					MotionTest.playSound();
					outerInstance.displayData(SmotionActivity.Info.MODE_REALTIME, outerInstance.mInfo);
				}
			}
		}
	}

}