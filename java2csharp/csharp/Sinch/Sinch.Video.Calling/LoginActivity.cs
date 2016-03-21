namespace com.sinch.android.rtc.sample.calling
{

	using ProgressDialog = android.app.ProgressDialog;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using Toast = android.widget.Toast;

	public class LoginActivity : BaseActivity, SinchService.StartFailedListener
	{

		private Button mLoginButton;
		private EditText mLoginName;
		private ProgressDialog mSpinner;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.login;

			mLoginName = (EditText) findViewById(R.id.loginName);

			mLoginButton = (Button) findViewById(R.id.loginButton);
			mLoginButton.Enabled = false;
			mLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly LoginActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(LoginActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.loginClicked();
			}
		}

		protected internal override void onServiceConnected()
		{
			mLoginButton.Enabled = true;
			SinchServiceInterface.StartListener = this;
		}

		protected internal override void onPause()
		{
			if (mSpinner != null)
			{
				mSpinner.dismiss();
			}
			base.onPause();
		}

		public virtual void onStartFailed(SinchError error)
		{
			Toast.makeText(this, error.ToString(), Toast.LENGTH_LONG).show();
			if (mSpinner != null)
			{
				mSpinner.dismiss();
			}
		}

		public virtual void onStarted()
		{
			openPlaceCallActivity();
		}

		private void loginClicked()
		{
			string userName = mLoginName.Text.ToString();

			if (userName.Length == 0)
			{
				Toast.makeText(this, "Please enter a name", Toast.LENGTH_LONG).show();
				return;
			}

			if (!SinchServiceInterface.Started)
			{
				SinchServiceInterface.startClient(userName);
				showSpinner();
			}
			else
			{
				openPlaceCallActivity();
			}
		}

		private void openPlaceCallActivity()
		{
			Intent mainActivity = new Intent(this, typeof(PlaceCallActivity));
			startActivity(mainActivity);
		}

		private void showSpinner()
		{
			mSpinner = new ProgressDialog(this);
			mSpinner.Title = "Logging in";
			mSpinner.Message = "Please wait...";
			mSpinner.show();
		}
	}

}