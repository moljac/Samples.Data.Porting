using System.Collections.Generic;

namespace com.sinch.android.rtc.sample.push
{

	using Call = com.sinch.android.rtc.calling.Call;
	using CallEndCause = com.sinch.android.rtc.calling.CallEndCause;
	using CallListener = com.sinch.android.rtc.calling.CallListener;

	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class CallScreenActivity : BaseActivity
	{

		internal static readonly string TAG = typeof(CallScreenActivity).Name;

		private AudioPlayer mAudioPlayer;
		private Timer mTimer;
		private UpdateCallDurationTask mDurationTask;

		private string mCallId;
		private long mCallStart = 0;

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
			mCallStart = DateTimeHelperClass.CurrentUnixTimeMillis();
			mCallId = Intent.getStringExtra(SinchService.CALL_ID);
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
				call.addCallListener(new SinchCallListener(this));
				mCallerName.Text = call.RemoteUserId;
				mCallState.Text = call.State.ToString();
			}
			else
			{
				Log.e(TAG, "Started with invalid callId, aborting.");
				finish();
			}
		}

		public override void onPause()
		{
			base.onPause();
			mDurationTask.cancel();
			mTimer.cancel();
		}

		public override void onResume()
		{
			base.onResume();
			mTimer = new Timer();
			mDurationTask = new UpdateCallDurationTask(this);
			mTimer.schedule(mDurationTask, 0, 500);
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

		private class SinchCallListener : CallListener
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
				outerInstance.mCallStart = DateTimeHelperClass.CurrentUnixTimeMillis();
			}

			public override void onCallProgressing(Call call)
			{
				Log.d(TAG, "Call progressing");
				outerInstance.mAudioPlayer.playProgressTone();
			}

			public override void onShouldSendPushNotification(Call call, IList<PushPair> pushPairs)
			{
				// no need to implement if you use managed push
			}

		}
	}

}