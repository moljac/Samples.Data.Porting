using System;
using System.Collections.Generic;

namespace com.twilio.voice.quickstart
{

	using Manifest = android.Manifest;
	using NotificationManager = android.app.NotificationManager;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using PackageManager = android.content.pm.PackageManager;
	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using SystemClock = android.os.SystemClock;
	using NonNull = android.support.annotation.NonNull;
	using CoordinatorLayout = android.support.design.widget.CoordinatorLayout;
	using FloatingActionButton = android.support.design.widget.FloatingActionButton;
	using Snackbar = android.support.design.widget.Snackbar;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;
	using AlertDialog = android.support.v7.app.AlertDialog;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Log = android.util.Log;
	using View = android.view.View;
	using Chronometer = android.widget.Chronometer;

	using ConnectionResult = com.google.android.gms.common.ConnectionResult;
	using GoogleApiAvailability = com.google.android.gms.common.GoogleApiAvailability;
	using FutureCallback = com.koushikdutta.@async.future.FutureCallback;
	using Ion = com.koushikdutta.ion.Ion;
	using GCMRegistrationService = com.twilio.voice.quickstart.gcm.GCMRegistrationService;

	public class VoiceActivity : AppCompatActivity
	{
		private bool InstanceFieldsInitialized = false;

		public VoiceActivity()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			registrationListener_Renamed = registrationListener();
			outgoingCallListener_Renamed = outgoingCallListener();
			incomingCallListener_Renamed = incomingCallListener();
			incomingCallMessageListener_Renamed = incomingCallMessageListener();
		}


		private const string TAG = "VoiceActivity";

		private const string ACCESS_TOKEN_SERVICE_URL = "PROVIDE_YOUR_ACCESS_TOKEN_SERVER";

		private const int MIC_PERMISSION_REQUEST_CODE = 1;
		private const int PLAY_SERVICES_RESOLUTION_REQUEST = 9000;

		private bool speakerPhone;
		private AudioManager audioManager;
		private int savedAudioMode = AudioManager.MODE_INVALID;

		private bool isReceiverRegistered;
		private VoiceBroadcastReceiver voiceBroadcastReceiver;

		// Empty HashMap, never populated for the Quickstart
		internal Dictionary<string, string> twiMLParams = new Dictionary<string, string>();

		private CoordinatorLayout coordinatorLayout;
		private FloatingActionButton callActionFab;
		private FloatingActionButton hangupActionFab;
		private FloatingActionButton speakerActionFab;
		private Chronometer chronometer;

		private OutgoingCall activeOutgoingCall;
		private IncomingCall activeIncomingCall;

		public const string ACTION_SET_GCM_TOKEN = "SET_GCM_TOKEN";
		public const string INCOMING_CALL_MESSAGE = "INCOMING_CALL_MESSAGE";
		public const string INCOMING_CALL_NOTIFICATION_ID = "INCOMING_CALL_NOTIFICATION_ID";
		public const string ACTION_INCOMING_CALL = "INCOMING_CALL";

		public const string KEY_GCM_TOKEN = "GCM_TOKEN";

		private NotificationManager notificationManager;
		private string gcmToken;
		private string accessToken;
		private AlertDialog alertDialog;

		internal RegistrationListener registrationListener_Renamed;
		internal OutgoingCall.Listener outgoingCallListener_Renamed;
		internal IncomingCall.Listener incomingCallListener_Renamed;
		internal IncomingCallMessageListener incomingCallMessageListener_Renamed;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_voice;
			coordinatorLayout = (CoordinatorLayout) findViewById(R.id.coordinator_layout);
			callActionFab = (FloatingActionButton) findViewById(R.id.call_action_fab);
			hangupActionFab = (FloatingActionButton) findViewById(R.id.hangup_action_fab);
			speakerActionFab = (FloatingActionButton) findViewById(R.id.speakerphone_action_fab);
			chronometer = (Chronometer) findViewById(R.id.chronometer);

			callActionFab.OnClickListener = callActionFabClickListener();
			hangupActionFab.OnClickListener = hangupActionFabClickListener();
			speakerActionFab.OnClickListener = speakerphoneActionFabClickListener();

			notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

			/*
			 * Setup the broadcast receiver to be notified of GCM Token updates
			 * or incoming call messages in this Activity.
			 */
			voiceBroadcastReceiver = new VoiceBroadcastReceiver(this);
			registerReceiver();

			/*
			 * Needed for setting/abandoning audio focus during a call
			 */
			audioManager = (AudioManager) getSystemService(Context.AUDIO_SERVICE);

			/*
			 * Enable changing the volume using the up/down keys during a conversation
			 */
			VolumeControlStream = AudioManager.STREAM_VOICE_CALL;

			/*
			 * Displays a call dialog if the intent contains an incoming call message
			 */
			handleIncomingCallIntent(Intent);


			/*
			 * Ensure the microphone permission is enabled
			 */
			if (!checkPermissionForMicrophone())
			{
				requestPermissionForMicrophone();
			}
			else
			{
				startGCMRegistration();
			}
		}

		protected internal override void onNewIntent(Intent intent)
		{
			base.onNewIntent(intent);
			handleIncomingCallIntent(intent);
		}

		private void startGCMRegistration()
		{
			if (checkPlayServices())
			{
				Intent intent = new Intent(this, typeof(GCMRegistrationService));
				startService(intent);
			}
		}

		private IncomingCallMessageListener incomingCallMessageListener()
		{
			return new IncomingCallMessageListenerAnonymousInnerClassHelper(this);
		}

		private class IncomingCallMessageListenerAnonymousInnerClassHelper : IncomingCallMessageListener
		{
			private readonly VoiceActivity outerInstance;

			public IncomingCallMessageListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onIncomingCall(IncomingCall incomingCall)
			{
				Log.d(TAG, "Incoming call from " + incomingCall.From);
				outerInstance.activeIncomingCall = incomingCall;
				outerInstance.alertDialog = createIncomingCallDialog(outerInstance, incomingCall, outerInstance.answerCallClickListener(), outerInstance.cancelCallClickListener());
				outerInstance.alertDialog.show();
			}

			public override void onIncomingCallCancelled(IncomingCall incomingCall)
			{
				Log.d(TAG, "Incoming call from " + incomingCall.From + " was cancelled");
				if (outerInstance.activeIncomingCall != null && incomingCall.CallSid == outerInstance.activeIncomingCall.CallSid && incomingCall.State == CallState.PENDING)
				{
					outerInstance.activeIncomingCall = null;
					if (outerInstance.alertDialog != null)
					{
						outerInstance.alertDialog.dismiss();
					}
				}
			}

		}

		private RegistrationListener registrationListener()
		{
			return new RegistrationListenerAnonymousInnerClassHelper(this);
		}

		private class RegistrationListenerAnonymousInnerClassHelper : RegistrationListener
		{
			private readonly VoiceActivity outerInstance;

			public RegistrationListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onRegistered(string accessToken, string gcmToken)
			{
				Log.d(TAG, "Successfully registered");
			}

			public override void onError(RegistrationException error, string accessToken, string gcmToken)
			{
				Log.e(TAG, string.Format("Error: {0:D}, {1}", error.ErrorCode, error.Message));
			}
		}

		private OutgoingCall.Listener outgoingCallListener()
		{
			return new ListenerAnonymousInnerClassHelper(this);
		}

		private class ListenerAnonymousInnerClassHelper : OutgoingCall.Listener
		{
			private readonly VoiceActivity outerInstance;

			public ListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConnected(OutgoingCall outgoingCall)
			{
				Log.d(TAG, "Connected");
			}

			public override void onDisconnected(OutgoingCall outgoingCall)
			{
				outerInstance.resetUI();
				Log.d(TAG, "Disconnect");
			}

			public override void onDisconnected(OutgoingCall outgoingCall, CallException error)
			{
				outerInstance.resetUI();
				Log.e(TAG, string.Format("Error: {0:D}, {1}", error.ErrorCode, error.Message));
			}
		}

		private IncomingCall.Listener incomingCallListener()
		{
			return new ListenerAnonymousInnerClassHelper(this);
		}

		private class ListenerAnonymousInnerClassHelper : IncomingCall.Listener
		{
			private readonly VoiceActivity outerInstance;

			public ListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onConnected(IncomingCall incomingCall)
			{
				Log.d(TAG, "Connected");
			}

			public override void onDisconnected(IncomingCall incomingCall)
			{
				outerInstance.resetUI();
				Log.d(TAG, "Disconnected");
			}

			public override void onDisconnected(IncomingCall incomingCall, CallException error)
			{
				outerInstance.resetUI();
				Log.e(TAG, string.Format("Error: {0:D}, {1}", error.ErrorCode, error.Message));
			}
		}

		/*
		 * The UI state when there is an active call
		 */
		private void setCallUI()
		{
			callActionFab.hide();
			hangupActionFab.show();
			speakerActionFab.show();
			chronometer.Visibility = View.VISIBLE;
			chronometer.Base = SystemClock.elapsedRealtime();
			chronometer.start();
		}

		/*
		 * Reset UI elements
		 */
		private void resetUI()
		{
			speakerPhone = false;
			audioManager.SpeakerphoneOn = speakerPhone;
			AudioFocus = speakerPhone;
			speakerActionFab.ImageDrawable = ContextCompat.getDrawable(VoiceActivity.this, R.drawable.ic_volume_down_white_24px);
			speakerActionFab.hide();
			callActionFab.show();
			hangupActionFab.hide();
			chronometer.Visibility = View.INVISIBLE;
			chronometer.stop();
		}

		protected internal override void onResume()
		{
			base.onResume();
			registerReceiver();
		}

		protected internal override void onPause()
		{
			base.onPause();
			LocalBroadcastManager.getInstance(this).unregisterReceiver(voiceBroadcastReceiver);
			isReceiverRegistered = false;
		}

		private void handleIncomingCallIntent(Intent intent)
		{
			if (intent != null && intent.Action != null && intent.Action == VoiceActivity.ACTION_INCOMING_CALL)
			{
				IncomingCallMessage incomingCallMessage = intent.getParcelableExtra(INCOMING_CALL_MESSAGE);
				VoiceClient.handleIncomingCallMessage(ApplicationContext, incomingCallMessage, incomingCallMessageListener_Renamed);
			}
		}

		private void registerReceiver()
		{
			if (!isReceiverRegistered)
			{
				IntentFilter intentFilter = new IntentFilter();
				intentFilter.addAction(ACTION_SET_GCM_TOKEN);
				intentFilter.addAction(ACTION_INCOMING_CALL);
				LocalBroadcastManager.getInstance(this).registerReceiver(voiceBroadcastReceiver, intentFilter);
				isReceiverRegistered = true;
			}
		}

		private class VoiceBroadcastReceiver : BroadcastReceiver
		{
			private readonly VoiceActivity outerInstance;

			public VoiceBroadcastReceiver(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onReceive(Context context, Intent intent)
			{
				string action = intent.Action;
				if (action.Equals(ACTION_SET_GCM_TOKEN))
				{
					string gcmToken = intent.getStringExtra(KEY_GCM_TOKEN);
					Log.i(TAG, "GCM Token : " + gcmToken);
					outerInstance.gcmToken = gcmToken;
					if (gcmToken == null)
					{
						Snackbar.make(outerInstance.coordinatorLayout, "Failed to get GCM Token. Unable to receive calls", Snackbar.LENGTH_LONG).show();
					}
					outerInstance.retrieveAccessToken();
				}
				else if (action.Equals(ACTION_INCOMING_CALL))
				{
					/*
					 * Remove the notification from the notification drawer
					 */
					outerInstance.notificationManager.cancel(intent.getIntExtra(VoiceActivity.INCOMING_CALL_NOTIFICATION_ID, 0));
					/*
					 * Handle the incoming call message
					 */
					VoiceClient.handleIncomingCallMessage(ApplicationContext, (IncomingCallMessage)intent.getParcelableExtra(INCOMING_CALL_MESSAGE), outerInstance.incomingCallMessageListener_Renamed);
				}
			}
		}

		private DialogInterface.OnClickListener answerCallClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly VoiceActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				outerInstance.answer();
				outerInstance.setCallUI();
				outerInstance.alertDialog.dismiss();
			}
		}

		private DialogInterface.OnClickListener cancelCallClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly VoiceActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialogInterface, int i)
			{
				outerInstance.activeIncomingCall.reject();
				outerInstance.alertDialog.dismiss();
			}
		}

		public static AlertDialog createIncomingCallDialog(Context context, IncomingCall incomingCall, DialogInterface.OnClickListener answerCallClickListener, DialogInterface.OnClickListener cancelClickListener)
		{
			AlertDialog.Builder alertDialogBuilder = new AlertDialog.Builder(context);
			alertDialogBuilder.Icon = R.drawable.ic_call_black_24dp;
			alertDialogBuilder.Title = "Incoming Call";
			alertDialogBuilder.setPositiveButton("Accept", answerCallClickListener);
			alertDialogBuilder.setNegativeButton("Reject", cancelClickListener);
			alertDialogBuilder.Message = incomingCall.From + " is calling.";
			return alertDialogBuilder.create();
		}

		/*
		 * Register your GCM token with Twilio to enable receiving incoming calls via GCM
		 */
		private void register()
		{
			VoiceClient.register(ApplicationContext, accessToken, gcmToken, registrationListener_Renamed);
		}

		private View.OnClickListener callActionFabClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly VoiceActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.activeOutgoingCall = VoiceClient.call(ApplicationContext, outerInstance.accessToken, outerInstance.twiMLParams, outerInstance.outgoingCallListener_Renamed);
				outerInstance.setCallUI();
			}
		}

		private View.OnClickListener hangupActionFabClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly VoiceActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.resetUI();
				outerInstance.disconnect();
			}
		}

		private View.OnClickListener speakerphoneActionFabClickListener()
		{
			return new OnClickListenerAnonymousInnerClassHelper3(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly VoiceActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.toggleSpeakerPhone();
			}
		}

		/*
		 * Accept an incoming Call
		 */
		private void answer()
		{
			activeIncomingCall.accept(incomingCallListener_Renamed);
		}

		/*
		 * Disconnect an active Call
		 */
		private void disconnect()
		{
			if (activeOutgoingCall != null)
			{
				activeOutgoingCall.disconnect();
				activeOutgoingCall = null;
			}
			else if (activeIncomingCall != null)
			{
				activeIncomingCall.reject();
				activeIncomingCall = null;
			}
		}

		/*
		 * Get an access token from your Twilio access token server
		 */
		private void retrieveAccessToken()
		{
			Ion.with(ApplicationContext).load(ACCESS_TOKEN_SERVICE_URL).asString().Callback = new FutureCallbackAnonymousInnerClassHelper(this);
		}

		private class FutureCallbackAnonymousInnerClassHelper : FutureCallback<string>
		{
			private readonly VoiceActivity outerInstance;

			public FutureCallbackAnonymousInnerClassHelper(VoiceActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCompleted(Exception e, string accessToken)
			{
				if (e == null)
				{
					Log.d(TAG, "Access token: " + accessToken);
					outerInstance.accessToken = accessToken;
					outerInstance.callActionFab.show();
					if (outerInstance.gcmToken != null)
					{
						outerInstance.register();
					}
				}
				else
				{
					Snackbar.make(outerInstance.coordinatorLayout, "Error retrieving access token. Unable to make calls", Snackbar.LENGTH_LONG).show();
				}
			}
		}

		private void toggleSpeakerPhone()
		{
			speakerPhone = !speakerPhone;

			AudioFocus = speakerPhone;
			audioManager.SpeakerphoneOn = speakerPhone;

			if (speakerPhone)
			{
				speakerActionFab.ImageDrawable = ContextCompat.getDrawable(VoiceActivity.this, R.drawable.ic_volume_mute_white_24px);
			}
			else
			{
				speakerActionFab.ImageDrawable = ContextCompat.getDrawable(VoiceActivity.this, R.drawable.ic_volume_down_white_24px);
			}
		}

		private bool AudioFocus
		{
			set
			{
				if (audioManager != null)
				{
					if (value)
					{
						savedAudioMode = audioManager.Mode;
						// Request audio focus before making any device switch.
						audioManager.requestAudioFocus(null, AudioManager.STREAM_VOICE_CALL, AudioManager.AUDIOFOCUS_GAIN_TRANSIENT);
    
						/*
						 * Start by setting MODE_IN_COMMUNICATION as default audio mode. It is
						 * required to be in this mode when playout and/or recording starts for
						 * best possible VoIP performance. Some devices have difficulties with speaker mode
						 * if this is not set.
						 */
						audioManager.Mode = AudioManager.MODE_IN_COMMUNICATION;
					}
					else
					{
						audioManager.Mode = savedAudioMode;
						audioManager.abandonAudioFocus(null);
					}
				}
			}
		}

		private bool checkPermissionForMicrophone()
		{
			int resultMic = ContextCompat.checkSelfPermission(this, Manifest.permission.RECORD_AUDIO);
			if (resultMic == PackageManager.PERMISSION_GRANTED)
			{
				return true;
			}
			return false;
		}

		private void requestPermissionForMicrophone()
		{
			if (ActivityCompat.shouldShowRequestPermissionRationale(this, Manifest.permission.RECORD_AUDIO))
			{
				Snackbar.make(coordinatorLayout, "Microphone permissions needed. Please allow in your application settings.", Snackbar.LENGTH_LONG).show();
			}
			else
			{
				ActivityCompat.requestPermissions(this, new string[]{Manifest.permission.RECORD_AUDIO}, MIC_PERMISSION_REQUEST_CODE);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions, @NonNull int[] grantResults)
		public override void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			/*
			 * Check if microphone permissions is granted
			 */
			if (requestCode == MIC_PERMISSION_REQUEST_CODE && permissions.Length > 0)
			{
				bool granted = true;
				if (granted)
				{
					startGCMRegistration();
				}
				else
				{
					Snackbar.make(coordinatorLayout, "Microphone permissions needed. Please allow in your application settings.", Snackbar.LENGTH_LONG).show();
				}
			}
		}

		/// <summary>
		/// Check the device to make sure it has the Google Play Services APK. If
		/// it doesn't, display a dialog that allows users to download the APK from
		/// the Google Play Store or enable it in the device's system settings.
		/// </summary>
		private bool checkPlayServices()
		{
			GoogleApiAvailability apiAvailability = GoogleApiAvailability.Instance;
			int resultCode = apiAvailability.isGooglePlayServicesAvailable(this);
			if (resultCode != ConnectionResult.SUCCESS)
			{
				if (apiAvailability.isUserResolvableError(resultCode))
				{
					apiAvailability.getErrorDialog(this, resultCode, PLAY_SERVICES_RESOLUTION_REQUEST).show();
				}
				else
				{
					Log.e(TAG, "This device is not supported.");
					finish();
				}
				return false;
			}
			return true;
		}
	}

}