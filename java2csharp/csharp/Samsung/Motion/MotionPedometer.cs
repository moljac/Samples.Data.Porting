using System.Text;

namespace com.samsung.sdk.motion.test
{

	using Smotion = com.samsung.android.sdk.motion.Smotion;
	using SmotionPedometer = com.samsung.android.sdk.motion.SmotionPedometer;
	using Info = com.samsung.android.sdk.motion.SmotionPedometer.Info;

	using SuppressLint = android.annotation.SuppressLint;
	using Handler = android.os.Handler;
	using Looper = android.os.Looper;


	internal class MotionPedometer
	{

		private SmotionPedometer mPedometer = null;

		private static readonly string[] sResults = new string[] {"Calorie", "Distance", "Speed", "Total Count", "Run Flat Count", "Walk Flat Count", "Run Up Count", "Run Down Count", "Walk Up Count", "Walk Down Count"};

		private int mMode = MotionTest.MODE_PEDOMETER;

		private Timer mTimer;

		private SmotionPedometer.Info mInfo;

		private long mInterval = 10000;

		private bool mIsUpDownAvailable;

		internal MotionPedometer(Looper looper, Smotion motion, bool isUpDownAvailable)
		{
			mPedometer = new SmotionPedometer(looper, motion);
			mIsUpDownAvailable = isUpDownAvailable;
			initialize();
		}

		internal virtual void start(int mode)
		{
			mMode = mode;

			initialize();
			mPedometer.start(changeListener);
			if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC)
			{
				startTimer();
			}
		}

		internal virtual void stop()
		{
			mPedometer.stop();
			if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC)
			{
				stopTimer();
			}
			initialize();
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

		internal virtual void initialize()
		{
			string status = "Ready";
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < sResults.Length; i++)
			{
				if (i >= 6)
				{
					if (mIsUpDownAvailable)
					{
						sb.Append(sResults[i] + " : \n");
					}
				}
				else
				{
					sb.Append(sResults[i] + " : \n");
				}
			}
			if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC || MotionTest.mTestMode == MotionTest.MODE_PEDOMETER_PERIODIC)
			{
				sb.Append("Interval : ");
			}
			MotionTest.displayData(0, status, sb.ToString());
		}

		private string getStatus(int status)
		{
			string str = null;
			switch (status)
			{
				case SmotionPedometer.Info.STATUS_WALK_UP:
					str = "Walk Up";
					break;
				case SmotionPedometer.Info.STATUS_WALK_DOWN:
					str = "Walk Down";
					break;
				case SmotionPedometer.Info.STATUS_WALK_FLAT:
					str = "Walk";
					break;
				case SmotionPedometer.Info.STATUS_RUN_DOWN:
					str = "Run Down";
					break;
				case SmotionPedometer.Info.STATUS_RUN_UP:
					str = "Run Up";
					break;
				case SmotionPedometer.Info.STATUS_RUN_FLAT:
					str = "Run";
					break;
				case SmotionPedometer.Info.STATUS_STOP:
					str = "Stop";
					break;
				case SmotionPedometer.Info.STATUS_UNKNOWN:
					str = "Unknown";
					break;
				default:
					break;
			}
			return str;
		}

		private SmotionPedometer.ChangeListener changeListener = new ChangeListenerAnonymousInnerClassHelper();

		private class ChangeListenerAnonymousInnerClassHelper : SmotionPedometer.ChangeListener
		{
			public ChangeListenerAnonymousInnerClassHelper()
			{
			}


			public override void onChanged(SmotionPedometer.Info info)
			{
				// TODO Auto-generated method stub
				if (outerInstance.mMode == MotionTest.MODE_PEDOMETER)
				{
					outerInstance.displayData(info);
				}
			}
		}

		private void displayData(SmotionPedometer.Info info)
		{
			// TODO Auto-generated method stub
			long timestamp = DateTimeHelperClass.CurrentUnixTimeMillis();
			StringBuilder sb = new StringBuilder();
			double calorie = info.Calorie;
			double distance = info.Distance;
			double speed = info.Speed;
			long totalCount = info.getCount(SmotionPedometer.Info.COUNT_TOTAL);
			long runCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_FLAT);
			long walkCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_FLAT);
			long runUpCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_UP);
			long runDownCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_DOWN);
			long walkUpCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_UP);
			long walkDownCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_DOWN);

			sb.Append(sResults[0] + " : " + calorie + "\n");
			sb.Append(sResults[1] + " : " + distance + "\n");
			sb.Append(sResults[2] + " : " + speed + "\n");
			sb.Append(sResults[3] + " : " + totalCount + "\n");
			sb.Append(sResults[4] + " : " + runCount + "\n");
			sb.Append(sResults[5] + " : " + walkCount + "\n");
			if (mIsUpDownAvailable)
			{
				sb.Append(sResults[6] + " : " + runUpCount + "\n");
				sb.Append(sResults[7] + " : " + runDownCount + "\n");
				sb.Append(sResults[8] + " : " + walkUpCount + "\n");
				sb.Append(sResults[9] + " : " + walkDownCount + "\n");
			}
			if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC || MotionTest.mTestMode == MotionTest.MODE_PEDOMETER_PERIODIC)
			{
				sb.Append("Interval : " + mInterval / 1000 + " sec");
			}
			string str = getStatus(info.Status);

			if (str != null)
			{
				MotionTest.displayData(timestamp, str, sb.ToString());
			}
		}

		internal class MyTimer : TimerTask
		{
			private readonly MotionPedometer outerInstance;

			public MyTimer(MotionPedometer outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void run()
			{
				// TODO Auto-generated method stub
				outerInstance.mInfo = outerInstance.mPedometer.Info;
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
				if (outerInstance.mInfo != null)
				{
					MotionTest.playSound();
					outerInstance.displayData(outerInstance.mInfo);
				}
			}
		}
	}

}