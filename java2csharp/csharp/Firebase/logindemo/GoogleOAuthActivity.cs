namespace com.firebase.samples.logindemo
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using IntentSender = android.content.IntentSender;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;

	using GoogleAuthException = com.google.android.gms.auth.GoogleAuthException;
	using GoogleAuthUtil = com.google.android.gms.auth.GoogleAuthUtil;
	using UserRecoverableAuthException = com.google.android.gms.auth.UserRecoverableAuthException;
	using ConnectionResult = com.google.android.gms.common.ConnectionResult;
	using Scopes = com.google.android.gms.common.Scopes;
	using GoogleApiClient = com.google.android.gms.common.api.GoogleApiClient;
	using Plus = com.google.android.gms.plus.Plus;

	/// <summary>
	/// This is a sample GoogleOAuthActivity that has been extracted from <seealso cref="com.firebase.samples.logindemo.MainActivity"/> to
	/// allow for better visibility.
	/// </summary>
	public class GoogleOAuthActivity : Activity, GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener
	{

		private static readonly string TAG = typeof(GoogleOAuthActivity).Name;

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

			mGoogleApiClient = (new GoogleApiClient.Builder(this)).addConnectionCallbacks(this).addOnConnectionFailedListener(this).addApi(Plus.API).addScope(Plus.SCOPE_PLUS_LOGIN).build();

			mGoogleLoginClicked = true;
			if (!mGoogleApiClient.Connecting)
			{
				if (mGoogleConnectionResult != null)
				{
					resolveSignInError();
				}
				else if (mGoogleApiClient.Connected)
				{
					GoogleOAuthTokenAndLogin;
				}
				else
				{
						/* connect API now */
					Log.d(TAG, "Trying to connect to Google API");
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
					mGoogleConnectionResult.startResolutionForResult(this, MainActivity.RC_GOOGLE_LOGIN);
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
			private readonly GoogleOAuthActivity outerInstance;

			public AsyncTaskAnonymousInnerClassHelper(GoogleOAuthActivity outerInstance)
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
						startActivityForResult(recover, MainActivity.RC_GOOGLE_LOGIN);
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
				Intent resultIntent = new Intent();
				if (token != null)
				{
					resultIntent.putExtra("oauth_token", token);
				}
				else if (errorMessage != null)
				{
					resultIntent.putExtra("error", errorMessage);
				}
				setResult(MainActivity.RC_GOOGLE_LOGIN, resultIntent);
				finish();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onConnected(final android.os.Bundle bundle)
		public override void onConnected(Bundle bundle)
		{
			/* Connected with Google API, use this to authenticate with Firebase */
			GoogleOAuthTokenAndLogin;
		}

		public override void onConnectionFailed(ConnectionResult result)
		{
			if (!mGoogleIntentInProgress)
			{
				/* Store the ConnectionResult so that we can use it later when the user clicks on the Google+ login button */
				mGoogleConnectionResult = result;

				if (mGoogleLoginClicked)
				{
					/* The user has already clicked login so we attempt to resolve all errors until the user is signed in,
					 * or they cancel. */
					resolveSignInError();
				}
				else
				{
					Log.e(TAG, result.ToString());
				}
			}
		}

		public override void onConnectionSuspended(int i)
		{
			// ignore
		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
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

}