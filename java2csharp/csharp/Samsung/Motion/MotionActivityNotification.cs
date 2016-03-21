using System.Text;

namespace com.samsung.sdk.motion.test
{

	using Smotion = com.samsung.android.sdk.motion.Smotion;
	using SmotionActivityNotification = com.samsung.android.sdk.motion.SmotionActivityNotification;
	using Info = com.samsung.android.sdk.motion.SmotionActivityNotification.Info;
	using InfoFilter = com.samsung.android.sdk.motion.SmotionActivityNotification.InfoFilter;

	using Looper = android.os.Looper;
	using Time = android.text.format.Time;

	public class MotionActivityNotification
	{

		private SmotionActivityNotification mActivityNotification;

		private SmotionActivityNotification.InfoFilter mFilter;

		internal MotionActivityNotification(Looper looper, Smotion motion)
		{
			// TODO Auto-generated constructor stub
			mActivityNotification = new SmotionActivityNotification(looper, motion);
			mFilter = null;
			initialize();
		}

		internal virtual void start()
		{
			initialize();
			if (mFilter != null)
			{
				mActivityNotification.start(mFilter, changeListener);
			}
		}

		internal virtual void stop()
		{
			if (mFilter != null)
			{
				mActivityNotification.stop();
			}
			mFilter = null;
			initialize();
		}

		internal virtual void addActivity(int activity_type)
		{
			if (mFilter == null)
			{
				mFilter = new SmotionActivityNotification.InfoFilter();
			}
			mFilter.addActivity(activity_type);
		}

		internal virtual void initialize()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Ready");
			MotionTest.displayData(0, sb.ToString());
		}

		private string getStatus(int status)
		{
			string str = null;
			switch (status)
			{
				case SmotionActivityNotification.Info.STATUS_UNKNOWN:
					str = "Unknown";
					break;
				case SmotionActivityNotification.Info.STATUS_STATIONARY:
					str = "Stationary";
					break;
				case SmotionActivityNotification.Info.STATUS_WALK:
					str = "Walk";
					break;
				case SmotionActivityNotification.Info.STATUS_RUN:
					str = "Run";
					break;
				case SmotionActivityNotification.Info.STATUS_VEHICLE:
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
				case SmotionActivityNotification.Info.ACCURACY_LOW:
					str = "Low";
					break;
				case SmotionActivityNotification.Info.ACCURACY_MID:
					str = "Mid";
					break;
				case SmotionActivityNotification.Info.ACCURACY_HIGH:
					str = "High";
					break;
				default:
					break;
			}
			return str;
		}

		private void displayData(SmotionActivityNotification.Info info)
		{
			StringBuilder sb = new StringBuilder();
			string str = null;
			long timestamp = info.TimeStamp;
			Time time = new Time();
			time.set(timestamp);
			Formatter form = new Formatter();
			form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
			sb.Append("[" + form.ToString() + "] ");
			form.close();
			str = getStatus(info.Status);

			sb.Append("(" + getAccuracy(info.Accuracy) + ")");
			if (str != null)
			{
				MotionTest.displayData(0, str, sb.ToString());
			}
		}

		internal virtual StringBuilder checkActivityStatus()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Status valid check\n");
			sb.Append("Stationary : " + mActivityNotification.isActivitySupported(SmotionActivityNotification.Info.STATUS_STATIONARY) + "\n");
			sb.Append("Walk : " + mActivityNotification.isActivitySupported(SmotionActivityNotification.Info.STATUS_WALK) + "\n");
			sb.Append("Run : " + mActivityNotification.isActivitySupported(SmotionActivityNotification.Info.STATUS_STATIONARY) + "\n");
			sb.Append("Vehicle : " + mActivityNotification.isActivitySupported(SmotionActivityNotification.Info.STATUS_RUN) + "\n");
			return sb;
		}

		private SmotionActivityNotification.ChangeListener changeListener = new ChangeListenerAnonymousInnerClassHelper();

		private class ChangeListenerAnonymousInnerClassHelper : SmotionActivityNotification.ChangeListener
		{
			public ChangeListenerAnonymousInnerClassHelper()
			{
			}


			public override void onChanged(SmotionActivityNotification.Info info)
			{
				// TODO Auto-generated method stub
				MotionTest.playSound();
				outerInstance.displayData(info);
			}
		}
	}

}