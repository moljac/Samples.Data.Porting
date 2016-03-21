namespace com.sinch.android.rtc.sample.push.gcm
{

	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	using WakefulBroadcastReceiver = android.support.v4.content.WakefulBroadcastReceiver;

	public class GcmBroadcastReceiver : WakefulBroadcastReceiver
	{

		public override void onReceive(Context context, Intent intent)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			ComponentName comp = new ComponentName(context.PackageName, typeof(GcmIntentService).FullName);
			startWakefulService(context, (intent.setComponent(comp)));
			ResultCode = Activity.RESULT_OK;
		}
	}
}