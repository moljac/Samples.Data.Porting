namespace com.twilio.voice.quickstart.gcm
{

	using Intent = android.content.Intent;
	using Log = android.util.Log;

	using InstanceIDListenerService = com.google.android.gms.iid.InstanceIDListenerService;

	public class VoiceInstanceIDListenerService : InstanceIDListenerService
	{

		private const string TAG = "VoiceGCMListenerService";

		public override void onTokenRefresh()
		{
			base.onTokenRefresh();

			Log.d(TAG, "onTokenRefresh");

			Intent intent = new Intent(this, typeof(GCMRegistrationService));
			startService(intent);
		}
	}

}