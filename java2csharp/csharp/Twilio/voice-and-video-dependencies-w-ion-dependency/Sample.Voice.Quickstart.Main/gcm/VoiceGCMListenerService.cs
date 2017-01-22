namespace com.twilio.voice.quickstart.gcm
{

	using TargetApi = android.annotation.TargetApi;
	using Notification = android.app.Notification;
	using NotificationManager = android.app.NotificationManager;
	using PendingIntent = android.app.PendingIntent;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using StatusBarNotification = android.service.notification.StatusBarNotification;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using Log = android.util.Log;

	using GcmListenerService = com.google.android.gms.gcm.GcmListenerService;

	public class VoiceGCMListenerService : GcmListenerService
	{

		private const string TAG = "VoiceGCMListenerService";

		/*
		 * Notification related keys
		 */
		private const string NOTIFICATION_ID_KEY = "NOTIFICATION_ID";
		private const string CALL_SID_KEY = "CALL_SID";

		private NotificationManager notificationManager;

		public override void onCreate()
		{
			base.onCreate();
			notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
		}

		public override void onMessageReceived(string from, Bundle bundle)
		{
			Log.d(TAG, "onMessageReceived " + from);

			if (IncomingCallMessage.isValidMessage(bundle))
			{
				/*
				 * Generate a unique notification id using the system time
				 */
				int notificationId = (int)DateTimeHelperClass.CurrentUnixTimeMillis();

				/*
				 * Create an IncomingCallMessage from the bundle
				 */
				IncomingCallMessage incomingCallMessage = new IncomingCallMessage(bundle);

				showNotification(incomingCallMessage, notificationId);
				sendIncomingCallMessageToActivity(incomingCallMessage, notificationId);
			}

		}

		/*
		 * Show the notification in the Android notification drawer
		 */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TargetApi(android.os.Build.VERSION_CODES.KITKAT_WATCH) private void showNotification(com.twilio.voice.IncomingCallMessage incomingCallMessage, int notificationId)
		private void showNotification(IncomingCallMessage incomingCallMessage, int notificationId)
		{
			string callSid = incomingCallMessage.CallSid;

			if (!incomingCallMessage.Cancelled)
			{
				/*
				 * Create a PendingIntent to specify the action when the notification is
				 * selected in the notification drawer
				 */
				Intent intent = new Intent(this, typeof(VoiceActivity));
				intent.Action = VoiceActivity.ACTION_INCOMING_CALL;
				intent.putExtra(VoiceActivity.INCOMING_CALL_MESSAGE, incomingCallMessage);
				intent.putExtra(VoiceActivity.INCOMING_CALL_NOTIFICATION_ID, notificationId);
				intent.addFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP);
				PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, intent, PendingIntent.FLAG_ONE_SHOT);

				/*
				 * Pass the notification id and call sid to use as an identifier to cancel the
				 * notification later
				 */
				Bundle extras = new Bundle();
				extras.putInt(NOTIFICATION_ID_KEY, notificationId);
				extras.putString(CALL_SID_KEY, callSid);

				/*
				 * Create the notification shown in the notification drawer
				 */
				NotificationCompat.Builder notificationBuilder = (new NotificationCompat.Builder(this)).setSmallIcon(R.drawable.ic_call_white_24px).setContentTitle(getString(R.@string.app_name)).setContentTextuniquetempvar.setAutoCancel(true).setExtras(extras).setContentIntent(pendingIntent).setColor(Color.rgb(214, 10, 37));

				notificationManager.notify(notificationId, notificationBuilder.build());
			}
			else
			{
				if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M)
				{
					/*
					 * If the incoming call was cancelled then remove the notification by matching
					 * it with the call sid from the list of notifications in the notification drawer.
					 */
					StatusBarNotification[] activeNotifications = notificationManager.ActiveNotifications;
					foreach (StatusBarNotification statusBarNotification in activeNotifications)
					{
						Notification notification = statusBarNotification.Notification;
						Bundle extras = notification.extras;
						string notificationCallSid = extras.getString(CALL_SID_KEY);
						if (callSid.Equals(notificationCallSid))
						{
							notificationManager.cancel(extras.getInt(NOTIFICATION_ID_KEY));
						}
					}
				}
				else
				{
					/*
					 * Prior to Android M the notification manager did not provide a list of
					 * active notifications so we lazily clear all the notifications when
					 * receiving a cancelled call.
					 *
					 * In order to properly cancel a notification using
					 * NotificationManager.cancel(notificationId) we should store the call sid &
					 * notification id of any incoming calls using shared preferences or some other form
					 * of persistent storage.
					 */
					notificationManager.cancelAll();
				}
			}
		}

		/*
		 * Send the IncomingCallMessage to the VoiceActivity
		 */
		private void sendIncomingCallMessageToActivity(IncomingCallMessage incomingCallMessage, int notificationId)
		{
			Intent intent = new Intent(VoiceActivity.ACTION_INCOMING_CALL);
			intent.putExtra(VoiceActivity.INCOMING_CALL_MESSAGE, incomingCallMessage);
			intent.putExtra(VoiceActivity.INCOMING_CALL_NOTIFICATION_ID, notificationId);
			LocalBroadcastManager.getInstance(this).sendBroadcast(intent);
		}

	}

}