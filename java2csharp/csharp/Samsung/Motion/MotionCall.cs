using System.Text;

namespace com.samsung.sdk.motion.test
{

	using Smotion = com.samsung.android.sdk.motion.Smotion;
	using SmotionCall = com.samsung.android.sdk.motion.SmotionCall;
	using Info = com.samsung.android.sdk.motion.SmotionCall.Info;

	using Looper = android.os.Looper;

	internal class MotionCall
	{
		private SmotionCall mCall = null;

		internal MotionCall(Looper looper, Smotion motion)
		{
			mCall = new SmotionCall(looper, motion);
		}

		internal virtual void start()
		{
			initialize();
			mCall.start(changeListener);
		}

		internal virtual void stop()
		{
			initialize();
			mCall.stop();
		}

		internal virtual void initialize()
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Ready");
			MotionTest.displayData(0, sb.ToString());
		}

		private void displayData(SmotionCall.Info info)
		{
			StringBuilder sb = new StringBuilder();
			long timestamp = DateTimeHelperClass.CurrentUnixTimeMillis();
			switch (info.CallPosition)
			{
				case SmotionCall.POSITION_LEFT:
					sb.Append("Left");
					break;
				case SmotionCall.POSITION_RIGHT:
					sb.Append("Right");
					break;
				default:
					break;
			}
			MotionTest.displayData(timestamp, sb.ToString());
		}

		private SmotionCall.ChangeListener changeListener = new ChangeListenerAnonymousInnerClassHelper();

		private class ChangeListenerAnonymousInnerClassHelper : SmotionCall.ChangeListener
		{
			public ChangeListenerAnonymousInnerClassHelper()
			{
			}


			public override void onChanged(SmotionCall.Info info)
			{
				// TODO Auto-generated method stub
				MotionTest.playSound();
				outerInstance.displayData(info);
			}
		}
	}

}