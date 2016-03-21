using System.Collections.Generic;

namespace com.sinch.android.rtc.sample.video
{

	using Call = com.sinch.android.rtc.calling.Call;
	using CallEndCause = com.sinch.android.rtc.calling.CallEndCause;
	using CallState = com.sinch.android.rtc.calling.CallState;
	using VideoCallListener = com.sinch.android.rtc.video.VideoCallListener;
	using VideoController = com.sinch.android.rtc.video.VideoController;

	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using LinearLayout = android.widget.LinearLayout;
	using RelativeLayout = android.widget.RelativeLayout;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class CallScreenActivity : BaseActivity
	{

		internal static readonly string TAG = typeof(CallScreenActivity).Name;
		internal const string CALL_START_TIME = "callStartTime";
		internal const string ADDED_LISTENER = "addedListener";

		private AudioPlayer mAudioPlayer;
		private Timer mTimer;
		private UpdateCallDurationTask mDurationTask;

		private string mCallId;
		private long mCallStart = 0;
		private bool mAddedListener = false;
		private bool mVideoViewsAdded = false;

		private TextView mCallDuration;
		private TextView mCallState;
		private TextView mCallerName;

		private class UpdateCallDurationTask : TimerTask
		{
			private readonly CallScreenActivity outerInstance;

			public UpdateCallDurationTask(CallScreenActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void run()
			{
				outerInstance.runOnUiThread(() =>
				{
					outerInstance.updateCallDuration();
				});
			}
		}

		protected internal override void onSaveInstanceState(Bundle savedInstanceState)
		{
			savedInstanceState.putLong(CALL_START_TIME, mCallStart);
			savedInstanceState.putBoolean(ADDED_LISTENER, mAddedListener);
		}

		protected internal override void onRestoreInstanceState(Bundle savedInstanceState)
		{
			mCallStart = savedInstanceState.getLong(CALL_START_TIME);
			mAddedListener = savedInstanceState.getBoolean(ADDED_LISTENER);
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.callscreen;

			mAudioPlayer = new AudioPlayer(this);
			mCallDuration = (TextView) findViewById(R.id.callDuration);
			mCallerName = (TextView) findViewById(R.id.remoteUser);
			mCallState = (TextView) findViewById(R.id.callState);
			Button endCallButton = (Button) findViewById(R.id.hangupButton);

			endCallButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mCallId = Intent.getStringExtra(SinchService.CALL_ID);
			if (savedInstanceState == null)
			{
				mCallStart = DateTimeHelperClass.CurrentUnixTimeMillis();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly CallScreenActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(CallScreenActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.endCall();
			}
		}

		public override void onServiceConnected()
		{
			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				if (!mAddedListener)
				{
					call.addCallListener(new SinchCallListener(this));
					mAddedListener = true;
				}
			}
			else
			{
				Log.e(TAG, "Started with invalid callId, aborting.");
				finish();
			}

			updateUI();
		}

		private void updateUI()
		{
			if (SinchServiceInterface == null)
			{
				return; // early
			}

			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				mCallerName.Text = call.RemoteUserId;
				mCallState.Text = call.State.ToString();
				if (call.State == CallState.ESTABLISHED)
				{
					addVideoViews();
				}
			}
		}

		public override void onStop()
		{
			base.onStop();
			mDurationTask.cancel();
			mTimer.cancel();
			removeVideoViews();
		}

		public override void onStart()
		{
			base.onStart();
			mTimer = new Timer();
			mDurationTask = new UpdateCallDurationTask(this);
			mTimer.schedule(mDurationTask, 0, 500);
			updateUI();
		}

		public override void onBackPressed()
		{
			// User should exit activity by ending call, not by going back.
		}

		private void endCall()
		{
			mAudioPlayer.stopProgressTone();
			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				call.hangup();
			}
			finish();
		}

		private string formatTimespan(long timespan)
		{
			long totalSeconds = timespan / 1000;
			long minutes = totalSeconds / 60;
			long seconds = totalSeconds % 60;
			return string.format(Locale.US, "%02d:%02d", minutes, seconds);
		}

		private void updateCallDuration()
		{
			if (mCallStart > 0)
			{
				mCallDuration.Text = formatTimespan(DateTimeHelperClass.CurrentUnixTimeMillis() - mCallStart);
			}
		}

		private void addVideoViews()
		{
			if (mVideoViewsAdded || SinchServiceInterface == null)
			{
				return; //early
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.sinch.android.rtc.video.VideoController vc = getSinchServiceInterface().getVideoController();
			VideoController vc = SinchServiceInterface.VideoController;
			if (vc != null)
			{
				RelativeLayout localView = (RelativeLayout) findViewById(R.id.localVideo);
				localView.addView(vc.LocalView);
				localView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this, vc);

				LinearLayout view = (LinearLayout) findViewById(R.id.remoteVideo);
				view.addView(vc.RemoteView);
				mVideoViewsAdded = true;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly CallScreenActivity outerInstance;

			private VideoController vc;

			public OnClickListenerAnonymousInnerClassHelper2(CallScreenActivity outerInstance, VideoController vc)
			{
				this.outerInstance = outerInstance;
				this.vc = vc;
			}

			public override void onClick(View v)
			{
				vc.toggleCaptureDevicePosition();
			}
		}

		private void removeVideoViews()
		{
			if (SinchServiceInterface == null)
			{
				return; // early
			}

			VideoController vc = SinchServiceInterface.VideoController;
			if (vc != null)
			{
				LinearLayout view = (LinearLayout) findViewById(R.id.remoteVideo);
				view.removeView(vc.RemoteView);

				RelativeLayout localView = (RelativeLayout) findViewById(R.id.localVideo);
				localView.removeView(vc.LocalView);
				mVideoViewsAdded = false;
			}
		}

		private class SinchCallListener : VideoCallListener
		{
			private readonly CallScreenActivity outerInstance;

			public SinchCallListener(CallScreenActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onCallEnded(Call call)
			{
				CallEndCause cause = call.Details.EndCause;
				Log.d(TAG, "Call ended. Reason: " + cause.ToString());
				outerInstance.mAudioPlayer.stopProgressTone();
				VolumeControlStream = AudioManager.USE_DEFAULT_STREAM_TYPE;
				string endMsg = "Call ended: " + call.Details.ToString();
				Toast.makeText(outerInstance, endMsg, Toast.LENGTH_LONG).show();

				outerInstance.endCall();
			}

			public override void onCallEstablished(Call call)
			{
				Log.d(TAG, "Call established");
				outerInstance.mAudioPlayer.stopProgressTone();
				outerInstance.mCallState.Text = call.State.ToString();
				VolumeControlStream = AudioManager.STREAM_VOICE_CALL;
				AudioController audioController = outerInstance.SinchServiceInterface.AudioController;
				audioController.enableSpeaker();
				outerInstance.mCallStart = DateTimeHelperClass.CurrentUnixTimeMillis();
				Log.d(TAG, "Call offered video: " + call.Details.VideoOffered);
			}

			public override void onCallProgressing(Call call)
			{
				Log.d(TAG, "Call progressing");
				outerInstance.mAudioPlayer.playProgressTone();
			}

			public override void onShouldSendPushNotification(Call call, IList<PushPair> pushPairs)
			{
				// Send a push through your push provider here, e.g. GCM
			}

			public override void onVideoTrackAdded(Call call)
			{
				Log.d(TAG, "Video track added");
				outerInstance.addVideoViews();
			}
		}
	}

}