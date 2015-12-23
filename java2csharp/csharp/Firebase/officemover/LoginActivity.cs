namespace com.firebase.officemover
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using IntentSender = android.content.IntentSender;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;

	using GoogleAuthException = com.google.android.gms.auth.GoogleAuthException;
	using GoogleAuthUtil = com.google.android.gms.auth.GoogleAuthUtil;
	using UserRecoverableAuthException = com.google.android.gms.auth.UserRecoverableAuthException;
	using ConnectionResult = com.google.android.gms.common.ConnectionResult;
	using Scopes = com.google.android.gms.common.Scopes;
	using SignInButton = com.google.android.gms.common.SignInButton;
	using GoogleApiClient = com.google.android.gms.common.api.GoogleApiClient;
	using Plus = com.google.android.gms.plus.Plus;

	/// <summary>
	/// This class implements Google+ Sign-in. There's not much Firebase specific stuff here.
	/// 
	/// If you'd like to learn more about Google+ Sign-in, you can find the official documentation here:
	/// https://developers.google.com/+/mobile/android/sign-in
	/// </summary>
	public class LoginActivity : Activity, GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener
	{

		private static readonly string TAG = typeof(LoginActivity).Name;

		/* Request code used to invoke sign in user interactions for Google+ */
		public const int RC_GOOGLE_LOGIN = 1;
		public const string AUTH_TOKEN_EXTRA = "authToken";

		/* Client used to interact with Google APIs. */
		private GoogleApiClient mGoogleApiClient;

		/* A flag indicating that a PendingIntent is in progress and prevents us from starting further intents. */
		private bool mGoogleIntentInProgress;

		/* Track whether the sign-in button has been clicked so that we know to resolve all issues preventing sign-in
		 * without waiting. */
		private bool mGoogleLoginClicked;

		/* Store the connection result from onConnectionFailed callbacks so that we can resolve them when the user clicks
		 * sign-in. */
		private ConnectionResult mGoogleConnectionResult;


		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_login;

			/* Load the Google login button */
			SignInButton googleLoginButton = (SignInButton) findViewById(R.id.login_with_google);
			googleLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			/* Setup the Google API object to allow Google+ logins */
			mGoogleApiClient = (new GoogleApiClient.Builder(this)).addConnectionCallbacks(this).addOnConnectionFailedListener(this).addApi(Plus.API).addScope(Plus.SCOPE_PLUS_LOGIN).build();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly LoginActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(LoginActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.mGoogleLoginClicked = true;
				if (!outerInstance.mGoogleApiClient.Connecting)
				{
					if (outerInstance.mGoogleConnectionResult != null)
					{
						outerInstance.resolveSignInError();
					}
					else if (outerInstance.mGoogleApiClient.Connected)
					{
						outerInstance.GoogleOAuthTokenAndLogin;
					}
					else
					{
					/* connect API now */
						Log.d(TAG, "Trying to connect to Google API");
						outerInstance.mGoogleApiClient.connect();
					}
				}
			}
		}

		protected internal override void onStart()
		{
			base.onStart();
			Intent intent = Intent;
			bool signout = intent.getBooleanExtra("SIGNOUT", false);
			if (signout)
			{
				signOut();
			}
			else if (!mGoogleIntentInProgress)
			{
				// auto sign in
				mGoogleApiClient.connect();
			}

		}

		public override void onConnected(Bundle bundle)
		{
			/* Connected with Google API, use this to authenticate with Firebase */
			Log.i(TAG, "Login Connected");
			GoogleOAuthTokenAndLogin;
		}

		public override void onConnectionSuspended(int i)
		{
			Log.i(TAG, "Login Suspended");
			// ignore
		}

		public override void onConnectionFailed(ConnectionResult connectionResult)
		{
			if (!mGoogleIntentInProgress)
			{
				/* Store the ConnectionResult so that we can use it later when the user clicks on the Google+ login button */
				mGoogleConnectionResult = connectionResult;

				if (mGoogleLoginClicked)
				{
					/* The user has already clicked login so we attempt to resolve all errors until the user is signed in,
					 * or they cancel. */
					resolveSignInError();
				}
				else
				{
					Log.e(TAG, connectionResult.ToString());
				}
			}
		}


		/// <summary>
		/// This method fires when any startActivityForResult finishes. The requestCode maps to
		/// the value passed into startActivityForResult.
		/// </summary>
		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			if (requestCode == RC_GOOGLE_LOGIN)
			{
				/* This was a request by the Google API */
				if (resultCode != RESULT_OK)
				{
					mGoogleLoginClicked = false;
				}
				mGoogleIntentInProgress = false;
				if (!mGoogleApiClient.Connecting)
				{
					mGoogleApiClient.connect();
				}
			}
		}

		/* A helper method to resolve the current ConnectionResult error. */
		private void resolveSignInError()
		{
			if (mGoogleConnectionResult.hasResolution())
			{
				try
				{
					mGoogleIntentInProgress = true;
					mGoogleConnectionResult.startResolutionForResult(this, RC_GOOGLE_LOGIN);
				}
				catch (IntentSender.SendIntentException)
				{
					// The intent was canceled before it was sent.  Return to the default
					// state and attempt to connect to get an updated ConnectionResult.
					mGoogleIntentInProgress = false;
					mGoogleApiClient.connect();
				}
			}
		}

		private void getGoogleOAuthTokenAndLogin()
		{
			/* Get OAuth token in Background */
			AsyncTask<Void, Void, string> task = new AsyncTaskAnonymousInnerClassHelper(this);
			task.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, string>
		{
			private readonly LoginActivity outerInstance;

			public AsyncTaskAnonymousInnerClassHelper(LoginActivity outerInstance)
			{
				this.outerInstance = outerInstance;
				errorMessage = null;
			}

			internal string errorMessage;
			protected internal override string doInBackground(params Void[] @params)
			{
				string token = null;

				try
				{
					string scope = string.Format("oauth2:{0}", Scopes.PLUS_LOGIN);
					token = GoogleAuthUtil.getToken(outerInstance, Plus.AccountApi.getAccountName(outerInstance.mGoogleApiClient), scope);
				}
				catch (IOException transientEx)
				{
					/* Network or server error */
					Log.e(TAG, "Error authenticating with Google: " + transientEx);
					errorMessage = "Network error: " + transientEx.Message;
				}
				catch (UserRecoverableAuthException e)
				{
					Log.w(TAG, "Recoverable Google OAuth error: " + e.ToString());
					/* We probably need to ask for permissions, so start the intent if there is none pending */
					if (!outerInstance.mGoogleIntentInProgress)
					{
						outerInstance.mGoogleIntentInProgress = true;
						Intent recover = e.Intent;
						startActivityForResult(recover, RC_GOOGLE_LOGIN);
					}
				}
				catch (GoogleAuthException authEx)
				{
					/* The call is not ever expected to succeed assuming you have already verified that
					 * Google Play services is installed. */
					Log.e(TAG, "Error authenticating with Google: " + authEx.Message, authEx);
					errorMessage = "Error authenticating with Google: " + authEx.Message;
				}
				return token;
			}

			protected internal override void onPostExecute(string token)
			{
				outerInstance.mGoogleLoginClicked = false;
				Intent intentWithToken = new Intent(outerInstance, typeof(OfficeMoverActivity));
				intentWithToken.putExtra(AUTH_TOKEN_EXTRA, token);
				startActivity(intentWithToken);
			}
		}


		public virtual void signOut()
		{
			if (mGoogleApiClient.Connected)
			{
				Plus.AccountApi.clearDefaultAccount(mGoogleApiClient);
				mGoogleApiClient.disconnect();
				mGoogleApiClient.connect();
			}
		}
	}

}