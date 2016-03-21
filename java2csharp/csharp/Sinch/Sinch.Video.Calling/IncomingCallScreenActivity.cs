using System.Collections.Generic;

namespace com.sinch.android.rtc.sample.calling
{

	using Call = com.sinch.android.rtc.calling.Call;
	using CallEndCause = com.sinch.android.rtc.calling.CallEndCause;
	using CallListener = com.sinch.android.rtc.calling.CallListener;

	using Intent = android.content.Intent;
	using PackageManager = android.content.pm.PackageManager;
	using Bundle = android.os.Bundle;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

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
				try
				{
					call.answer();
					Intent intent = new Intent(this, typeof(CallScreenActivity));
					intent.putExtra(SinchService.CALL_ID, mCallId);
					startActivity(intent);
				}
				catch (MissingPermissionException e)
				{
					ActivityCompat.requestPermissions(this, new string[]{e.RequiredPermission}, 0);
				}
			}
			else
			{
				finish();
			}
		}

		public virtual void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			if (grantResults[0] == PackageManager.PERMISSION_GRANTED)
			{
				Toast.makeText(this, "You may now answer the call", Toast.LENGTH_LONG).show();
			}
			else
			{
				Toast.makeText(this, "This application needs permission to use your microphone to function properly.", Toast.LENGTH_LONG).show();
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
				// Send a push through your push provider here, e.g. GCM
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