using System.Collections.Generic;

namespace com.sinch.android.rtc.sample.push
{

	using Call = com.sinch.android.rtc.calling.Call;
	using CallEndCause = com.sinch.android.rtc.calling.CallEndCause;
	using CallListener = com.sinch.android.rtc.calling.CallListener;

	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;

	public class IncomingCallScreenActivity : BaseActivity
	{

		internal static readonly string TAG = typeof(IncomingCallScreenActivity).Name;
		private string mCallId;
		private AudioPlayer mAudioPlayer;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.incoming;

			Button answer = (Button) findViewById(R.id.answerButton);
			answer.OnClickListener = mClickListener;
			Button decline = (Button) findViewById(R.id.declineButton);
			decline.OnClickListener = mClickListener;

			mAudioPlayer = new AudioPlayer(this);
			mAudioPlayer.playRingtone();
			mCallId = Intent.getStringExtra(SinchService.CALL_ID);
		}

		protected internal override void onResume()
		{
			base.onResume();
			if (Intent != null)
			{
				if (Intent.getStringExtra(SinchService.CALL_ID) != null)
				{
					mCallId = Intent.getStringExtra(SinchService.CALL_ID);
				}
			}
		}


		protected internal override void onServiceConnected()
		{
			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				call.addCallListener(new SinchCallListener(this));
				TextView remoteUser = (TextView) findViewById(R.id.remoteUser);
				remoteUser.Text = call.RemoteUserId;
			}
			else
			{
				Log.e(TAG, "Started with invalid callId, aborting");
				finish();
			}
		}

		private void answerClicked()
		{
			mAudioPlayer.stopRingtone();
			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				Log.d(TAG, "Answering call");
				call.answer();
				Intent intent = new Intent(this, typeof(CallScreenActivity));
				intent.putExtra(SinchService.CALL_ID, mCallId);
				startActivity(intent);
			}
			else
			{
				finish();
			}
		}

		private void declineClicked()
		{
			mAudioPlayer.stopRingtone();
			Call call = SinchServiceInterface.getCall(mCallId);
			if (call != null)
			{
				call.hangup();
			}
			finish();
		}

		private class SinchCallListener : CallListener
		{
			private readonly IncomingCallScreenActivity outerInstance;

			public SinchCallListener(IncomingCallScreenActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onCallEnded(Call call)
			{
				CallEndCause cause = call.Details.EndCause;
				Log.d(TAG, "Call ended, cause: " + cause.ToString());
				outerInstance.mAudioPlayer.stopRingtone();
				finish();
			}

			public override void onCallEstablished(Call call)
			{
				Log.d(TAG, "Call established");
			}

			public override void onCallProgressing(Call call)
			{
				Log.d(TAG, "Call progressing");
			}

			public override void onShouldSendPushNotification(Call call, IList<PushPair> pushPairs)
			{
				// no need to implement for managed push
			}

		}

		private View.OnClickListener mClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				switch (v.Id)
				{
					case R.id.answerButton:
						outerInstance.answerClicked();
						break;
					case R.id.declineButton:
						outerInstance.declineClicked();
						break;
				}
			}
		}
	}

}