namespace com.sinch.android.rtc.sample.pstn
{

	using Call = com.sinch.android.rtc.calling.Call;

	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	public class PlaceCallActivity : BaseActivity
	{

		private Button mCallButton;
		private EditText mCallName;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.main;

			mCallName = (EditText) findViewById(R.id.callName);
			mCallButton = (Button) findViewById(R.id.callButton);
			mCallButton.Enabled = false;
			mCallButton.OnClickListener = buttonClickListener;

			Button stopButton = (Button) findViewById(R.id.stopButton);
			stopButton.OnClickListener = buttonClickListener;
		}

		protected internal override void onServiceConnected()
		{
			TextView userName = (TextView) findViewById(R.id.loggedInName);
			userName.Text = SinchServiceInterface.UserName;
			mCallButton.Enabled = true;
		}

		public override void onDestroy()
		{
			if (SinchServiceInterface != null)
			{
				SinchServiceInterface.stopClient();
			}
			base.onDestroy();
		}

		private void stopButtonClicked()
		{
			if (SinchServiceInterface != null)
			{
				SinchServiceInterface.stopClient();
			}
			finish();
		}

		private void callButtonClicked()
		{
			string phoneNumber = mCallName.Text.ToString();
			if (phoneNumber.Length == 0)
			{
				Toast.makeText(this, "Please enter a number to call", Toast.LENGTH_LONG).show();
				return;
			}

			Call call = SinchServiceInterface.callPhoneNumber(phoneNumber);
			string callId = call.CallId;

			Intent callScreen = new Intent(this, typeof(CallScreenActivity));
			callScreen.putExtra(SinchService.CALL_ID, callId);
			startActivity(callScreen);
		}

		private View.OnClickListener buttonClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				switch (v.Id)
				{
					case R.id.callButton:
						outerInstance.callButtonClicked();
						break;

					case R.id.stopButton:
						outerInstance.stopButtonClicked();
						break;

				}
			}
		}
	}

}