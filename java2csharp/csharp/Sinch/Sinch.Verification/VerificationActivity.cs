using System;

namespace com.sinch.verification.sample
{


	using Manifest = android.Manifest;
	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using PackageManager = android.content.pm.PackageManager;
	using Bundle = android.os.Bundle;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using Log = android.util.Log;
	using View = android.view.View;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;
	using ProgressBar = android.widget.ProgressBar;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	public class VerificationActivity : Activity, ActivityCompat.OnRequestPermissionsResultCallback
	{

		private static readonly string TAG = typeof(Verification).Name;
		private readonly string APPLICATION_KEY = "enter-app-key";

		private Verification mVerification;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_verification;
			showProgress();
			initiateVerification();
		}

		internal virtual void createVerification(string phoneNumber, string method, bool skipPermissionCheck)
		{
			Config config = SinchVerification.config().applicationKey(APPLICATION_KEY).context(ApplicationContext).build();
			VerificationListener listener = new MyVerificationListener(this);

			if (method.Equals(MainActivity.SMS, StringComparison.CurrentCultureIgnoreCase))
			{

				if (!skipPermissionCheck && ContextCompat.checkSelfPermission(this, Manifest.permission.READ_SMS) == PackageManager.PERMISSION_DENIED)
				{
					ActivityCompat.requestPermissions(this, new string[]{Manifest.permission.READ_SMS}, 0);
					hideProgressBar();
				}
				else
				{
					mVerification = SinchVerification.createSmsVerification(config, phoneNumber, listener);
					mVerification.initiate();
				}

			}
			else
			{
				TextView messageText = (TextView) findViewById(R.id.textView);
				messageText.Text = R.@string.flashcalling;
				mVerification = SinchVerification.createFlashCallVerification(config, phoneNumber, listener);
				mVerification.initiate();
			}
		}

		public virtual void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			if (grantResults.Length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED)
			{

			}
			else
			{
				if (ActivityCompat.shouldShowRequestPermissionRationale(this, permissions[0]))
				{
					Toast.makeTextuniquetempvar.show();
				}
				enableInputField(true);
			}
			initiateVerificationAndSuppressPermissionCheck();
		}

		internal virtual void initiateVerification()
		{
			initiateVerification(false);
		}

		internal virtual void initiateVerificationAndSuppressPermissionCheck()
		{
			initiateVerification(true);
		}

		internal virtual void initiateVerification(bool skipPermissionCheck)
		{
			Intent intent = Intent;
			if (intent != null)
			{
				string phoneNumber = intent.getStringExtra(MainActivity.INTENT_PHONENUMBER);
				string method = intent.getStringExtra(MainActivity.INTENT_METHOD);
				TextView phoneText = (TextView) findViewById(R.id.numberText);
				phoneText.Text = phoneNumber;
				createVerification(phoneNumber, method, skipPermissionCheck);
			}
		}

		public virtual void onSubmitClicked(View view)
		{
			string code = ((EditText) findViewById(R.id.inputCode)).Text.ToString();
			if (code.Length > 0)
			{
				if (mVerification != null)
				{
					mVerification.verify(code);
					showProgress();
					TextView messageText = (TextView) findViewById(R.id.textView);
					messageText.Text = "Verification in progress";
					enableInputField(false);
				}
			}
		}

		internal virtual void enableInputField(bool enable)
		{
			View container = findViewById(R.id.inputContainer);
			if (enable)
			{
				container.Visibility = View.VISIBLE;
				EditText input = (EditText) findViewById(R.id.inputCode);
				input.requestFocus();
			}
			else
			{
				container.Visibility = View.GONE;
			}
		}

		internal virtual void hideProgressBarAndShowMessage(int message)
		{
			hideProgressBar();
			TextView messageText = (TextView) findViewById(R.id.textView);
			messageText.Text = message;
		}

		internal virtual void hideProgressBar()
		{
			ProgressBar progressBar = (ProgressBar) findViewById(R.id.progressIndicator);
			progressBar.Visibility = View.INVISIBLE;
			TextView progressText = (TextView) findViewById(R.id.progressText);
			progressText.Visibility = View.INVISIBLE;
		}

		internal virtual void showProgress()
		{
			ProgressBar progressBar = (ProgressBar) findViewById(R.id.progressIndicator);
			progressBar.Visibility = View.VISIBLE;
		}

		internal virtual void showCompleted()
		{
			ImageView checkMark = (ImageView) findViewById(R.id.checkmarkImage);
			checkMark.Visibility = View.VISIBLE;
		}

		internal class MyVerificationListener : VerificationListener
		{
			private readonly VerificationActivity outerInstance;

			public MyVerificationListener(VerificationActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onInitiated()
			{
				Log.d(TAG, "Initialized!");
				outerInstance.showProgress();
			}

			public override void onInitiationFailed(Exception exception)
			{
				Log.e(TAG, "Verification initialization failed: " + exception.Message);
				outerInstance.hideProgressBarAndShowMessage(R.@string.failed);
			}

			public override void onVerified()
			{
				Log.d(TAG, "Verified!");
				outerInstance.hideProgressBarAndShowMessage(R.@string.verified);
				outerInstance.showCompleted();
			}

			public override void onVerificationFailed(Exception exception)
			{
				Log.e(TAG, "Verification failed: " + exception.Message);
				if (exception is CodeInterceptionException)
				{
					outerInstance.hideProgressBar();
				}
				else
				{
					outerInstance.hideProgressBarAndShowMessage(R.@string.failed);
				}
				outerInstance.enableInputField(true);
			}
		}

	}

}