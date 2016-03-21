namespace com.sinch.android.rtc.sample.push.gcm
{


	using IntentService = android.app.IntentService;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using IBinder = android.os.IBinder;

	public class GcmIntentService : IntentService, ServiceConnection
	{

		private Intent mIntent;

		public GcmIntentService() : base("GcmIntentService")
		{
		}

		protected internal override void onHandleIntent(Intent intent)
		{
			if (SinchHelpers.isSinchPushIntent(intent))
			{
				mIntent = intent;
				connectToService();
			}
			else
			{
				GcmBroadcastReceiver.completeWakefulIntent(intent);
			}
		}

		private void connectToService()
		{
			ApplicationContext.bindService(new Intent(this, typeof(SinchService)), this, Context.BIND_AUTO_CREATE);
		}

		public override void onServiceConnected(ComponentName componentName, IBinder iBinder)
		{
			if (mIntent == null)
			{
				return;
			}

			if (SinchHelpers.isSinchPushIntent(mIntent))
			{
				SinchService.SinchServiceInterface sinchService = (SinchService.SinchServiceInterface) iBinder;
				if (sinchService != null)
				{
					NotificationResult result = sinchService.relayRemotePushNotificationPayload(mIntent);
					// handle result, e.g. show a notification or similar
				}
			}

			GcmBroadcastReceiver.completeWakefulIntent(mIntent);
			mIntent = null;
		}

		public override void onServiceDisconnected(ComponentName componentName)
		{
		}

	}
}