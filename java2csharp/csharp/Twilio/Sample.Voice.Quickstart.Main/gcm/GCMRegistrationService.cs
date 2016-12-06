using System;

namespace com.twilio.voice.quickstart.gcm
{

	using IntentService = android.app.IntentService;
	using Intent = android.content.Intent;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;
	using Log = android.util.Log;

	using GoogleCloudMessaging = com.google.android.gms.gcm.GoogleCloudMessaging;
	using InstanceID = com.google.android.gms.iid.InstanceID;

	public class GCMRegistrationService : IntentService
	{

		private const string TAG = "GCMRegistration";

		public GCMRegistrationService() : base("GCMRegistrationService")
		{
		}

		public override void onCreate()
		{
			base.onCreate();
		}

		protected internal override void onHandleIntent(Intent intent)
		{
			try
			{
				InstanceID instanceID = InstanceID.getInstance(this);
				string token = instanceID.getToken(getString(R.@string.gcm_defaultSenderId), GoogleCloudMessaging.INSTANCE_ID_SCOPE, null);
				sendGCMTokenToActivity(token);
			}
			catch (Exception e)
			{
				/*
				 * If we are unable to retrieve the GCM token we notify the Activity
				 * letting the user know this step failed.
				 */
				Log.e(TAG, "Failed to retrieve GCM token", e);
				sendGCMTokenToActivity(null);
			}
		}

		/// <summary>
		/// Send the GCM Token to the Voice Activity.
		/// </summary>
		/// <param name="gcmToken"> The new token. </param>
		private void sendGCMTokenToActivity(string gcmToken)
		{
			Intent intent = new Intent(VoiceActivity.ACTION_SET_GCM_TOKEN);
			intent.putExtra(VoiceActivity.KEY_GCM_TOKEN, gcmToken);
			LocalBroadcastManager.getInstance(this).sendBroadcast(intent);
		}
	}

}