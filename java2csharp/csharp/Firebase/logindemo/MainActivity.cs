using System.Collections.Generic;

namespace com.firebase.samples.logindemo
{

	using AlertDialog = android.app.AlertDialog;
	using ProgressDialog = android.app.ProgressDialog;
	using Intent = android.content.Intent;
	using IntentSender = android.content.IntentSender;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;

	using AccessToken = com.facebook.AccessToken;
	using AccessTokenTracker = com.facebook.AccessTokenTracker;
	using CallbackManager = com.facebook.CallbackManager;
	using LoginManager = com.facebook.login.LoginManager;
	using LoginButton = com.facebook.login.widget.LoginButton;
	using AuthData = com.firebase.client.AuthData;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using GoogleAuthException = com.google.android.gms.auth.GoogleAuthException;
	using GoogleAuthUtil = com.google.android.gms.auth.GoogleAuthUtil;
	using UserRecoverableAuthException = com.google.android.gms.auth.UserRecoverableAuthException;
	using ConnectionResult = com.google.android.gms.common.ConnectionResult;
	using Scopes = com.google.android.gms.common.Scopes;
	using SignInButton = com.google.android.gms.common.SignInButton;
	using GoogleApiClient = com.google.android.gms.common.api.GoogleApiClient;
	using Plus = com.google.android.gms.plus.Plus;


	/// <summary>
	/// This application demos the use of the Firebase Login feature. It currently supports logging in
	/// with Google, Facebook, Twitter, Email/Password, and Anonymous providers.
	/// <p/>
	/// The methods in this class have been divided into sections based on providers (with a few
	/// general methods).
	/// <p/>
	/// Facebook provides its own API via the <seealso cref="com.facebook.login.widget.LoginButton"/>.
	/// Google provides its own API via the <seealso cref="com.google.android.gms.common.api.GoogleApiClient"/>.
	/// Twitter requires us to use a Web View to authenticate, see
	/// <seealso cref="com.firebase.samples.logindemo.TwitterOAuthActivity"/>
	/// Email/Password is provided using <seealso cref="com.firebase.client.Firebase"/>
	/// Anonymous is provided using <seealso cref="com.firebase.client.Firebase"/>
	/// </summary>
	public class MainActivity : ActionBarActivity, GoogleApiClient.ConnectionCallbacks, GoogleApiClient.OnConnectionFailedListener
	{

		private static readonly string TAG = typeof(MainActivity).Name;

		/* *************************************
		 *              GENERAL                *
		 ***************************************/
		/* TextView that is used to display information about the logged in user */
		private TextView mLoggedInStatusTextView;

		/* A dialog that is presented until the Firebase authentication finished. */
		private ProgressDialog mAuthProgressDialog;

		/* A reference to the Firebase */
		private Firebase mFirebaseRef;

		/* Data from the authenticated user */
		private AuthData mAuthData;

		/* Listener for Firebase session changes */
		private Firebase.AuthStateListener mAuthStateListener;

		/* *************************************
		 *              FACEBOOK               *
		 ***************************************/
		/* The login button for Facebook */
		private LoginButton mFacebookLoginButton;
		/* The callback manager for Facebook */
		private CallbackManager mFacebookCallbackManager;
		/* Used to track user logging in/out off Facebook */
		private AccessTokenTracker mFacebookAccessTokenTracker;


		/* *************************************
		 *              GOOGLE                 *
		 ***************************************/
		/* Request code used to invoke sign in user interactions for Google+ */
		public const int RC_GOOGLE_LOGIN = 1;

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

		/* The login button for Google */
		private SignInButton mGoogleLoginButton;

		/* *************************************
		 *              TWITTER                *
		 ***************************************/
		public const int RC_TWITTER_LOGIN = 2;

		private Button mTwitterLoginButton;

		/* *************************************
		 *              PASSWORD               *
		 ***************************************/
		private Button mPasswordLoginButton;

		/* *************************************
		 *            ANONYMOUSLY              *
		 ***************************************/
		private Button mAnonymousLoginButton;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			/* Load the view and display it */
			ContentView = R.layout.activity_main;

			/* *************************************
			 *              FACEBOOK               *
			 ***************************************/
			/* Load the Facebook login button and set up the tracker to monitor access token changes */
			mFacebookCallbackManager = CallbackManager.Factory.create();
			mFacebookLoginButton = (LoginButton) findViewById(R.id.login_with_facebook);
			mFacebookAccessTokenTracker = new AccessTokenTrackerAnonymousInnerClassHelper(this);

			/* *************************************
			 *               GOOGLE                *
			 ***************************************/
			/* Load the Google login button */
			mGoogleLoginButton = (SignInButton) findViewById(R.id.login_with_google);
			mGoogleLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			/* Setup the Google API object to allow Google+ logins */
			mGoogleApiClient = (new GoogleApiClient.Builder(this)).addConnectionCallbacks(this).addOnConnectionFailedListener(this).addApi(Plus.API).addScope(Plus.SCOPE_PLUS_LOGIN).build();

			/* *************************************
			 *                TWITTER              *
			 ***************************************/
			mTwitterLoginButton = (Button) findViewById(R.id.login_with_twitter);
			mTwitterLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			/* *************************************
			 *               PASSWORD              *
			 ***************************************/
			mPasswordLoginButton = (Button) findViewById(R.id.login_with_password);
			mPasswordLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);

			/* *************************************
			 *              ANONYMOUSLY            *
			 ***************************************/
			/* Load and setup the anonymous login button */
			mAnonymousLoginButton = (Button) findViewById(R.id.login_anonymously);
			mAnonymousLoginButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper4(this);

			/* *************************************
			 *               GENERAL               *
			 ***************************************/
			mLoggedInStatusTextView = (TextView) findViewById(R.id.login_status);

			/* Create the Firebase ref that is used for all authentication with Firebase */
			mFirebaseRef = new Firebase(Resources.getString(R.@string.firebase_url));

			/* Setup the progress dialog that is displayed later when authenticating with Firebase */
			mAuthProgressDialog = new ProgressDialog(this);
			mAuthProgressDialog.Title = "Loading";
			mAuthProgressDialog.Message = "Authenticating with Firebase...";
			mAuthProgressDialog.Cancelable = false;
			mAuthProgressDialog.show();

			mAuthStateListener = new AuthStateListenerAnonymousInnerClassHelper(this);
			/* Check if the user is authenticated with Firebase already. If this is the case we can set the authenticated
			 * user and hide hide any login buttons */
			mFirebaseRef.addAuthStateListener(mAuthStateListener);
		}

		private class AccessTokenTrackerAnonymousInnerClassHelper : AccessTokenTracker
		{
			private readonly MainActivity outerInstance;

			public AccessTokenTrackerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override void onCurrentAccessTokenChanged(AccessToken oldAccessToken, AccessToken currentAccessToken)
			{
				Log.i(TAG, "Facebook.AccessTokenTracker.OnCurrentAccessTokenChanged");
				outerInstance.onFacebookAccessTokenChange(currentAccessToken);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MainActivity outerInstance)
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

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.loginWithTwitter();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.loginWithPassword();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.loginAnonymously();
			}
		}

		private class AuthStateListenerAnonymousInnerClassHelper : Firebase.AuthStateListener
		{
			private readonly MainActivity outerInstance;

			public AuthStateListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onAuthStateChanged(AuthData authData)
			{
				outerInstance.mAuthProgressDialog.hide();
				outerInstance.AuthenticatedUser = authData;
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			// if user logged in with Facebook, stop tracking their token
			if (mFacebookAccessTokenTracker != null)
			{
				mFacebookAccessTokenTracker.stopTracking();
			}

			// if changing configurations, stop tracking firebase session.
			mFirebaseRef.removeAuthStateListener(mAuthStateListener);
		}

		/// <summary>
		/// This method fires when any startActivityForResult finishes. The requestCode maps to
		/// the value passed into startActivityForResult.
		/// </summary>
		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			IDictionary<string, string> options = new Dictionary<string, string>();
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
			else if (requestCode == RC_TWITTER_LOGIN)
			{
				options["oauth_token"] = data.getStringExtra("oauth_token");
				options["oauth_token_secret"] = data.getStringExtra("oauth_token_secret");
				options["user_id"] = data.getStringExtra("user_id");
				authWithFirebase("twitter", options);
			}
			else
			{
				/* Otherwise, it's probably the request by the Facebook login button, keep track of the session */
				mFacebookCallbackManager.onActivityResult(requestCode, resultCode, data);
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			/* If a user is currently authenticated, display a logout menu */
			if (this.mAuthData != null)
			{
				MenuInflater.inflate(R.menu.main, menu);
				return true;
			}
			else
			{
				return false;
			}
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			int id = item.ItemId;
			if (id == R.id.action_logout)
			{
				logout();
				return true;
			}
			return base.onOptionsItemSelected(item);
		}

		/// <summary>
		/// Unauthenticate from Firebase and from providers where necessary.
		/// </summary>
		private void logout()
		{
			if (this.mAuthData != null)
			{
				/* logout of Firebase */
				mFirebaseRef.unauth();
				/* Logout of any of the Frameworks. This step is optional, but ensures the user is not logged into
				 * Facebook/Google+ after logging out of Firebase. */
				if (this.mAuthData.Provider.Equals("facebook"))
				{
					/* Logout from Facebook */
					LoginManager.Instance.logOut();
				}
				else if (this.mAuthData.Provider.Equals("google"))
				{
					/* Logout from Google+ */
					if (mGoogleApiClient.Connected)
					{
						Plus.AccountApi.clearDefaultAccount(mGoogleApiClient);
						mGoogleApiClient.disconnect();
					}
				}
				/* Update authenticated user and show login buttons */
				AuthenticatedUser = null;
			}
		}

		/// <summary>
		/// This method will attempt to authenticate a user to firebase given an oauth_token (and other
		/// necessary parameters depending on the provider)
		/// </summary>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void authWithFirebase(final String provider, java.util.Map<String, String> options)
		private void authWithFirebase(string provider, IDictionary<string, string> options)
		{
			if (options.ContainsKey("error"))
			{
				showErrorDialog(options["error"]);
			}
			else
			{
				mAuthProgressDialog.show();
				if (provider.Equals("twitter"))
				{
					// if the provider is twitter, we pust pass in additional options, so use the options endpoint
					mFirebaseRef.authWithOAuthToken(provider, options, new AuthResultHandler(this, provider));
				}
				else
				{
					// if the provider is not twitter, we just need to pass in the oauth_token
					mFirebaseRef.authWithOAuthToken(provider, options["oauth_token"], new AuthResultHandler(this, provider));
				}
			}
		}

		/// <summary>
		/// Once a user is logged in, take the mAuthData provided from Firebase and "use" it.
		/// </summary>
		private AuthData AuthenticatedUser
		{
			set
			{
				if (value != null)
				{
					/* Hide all the login buttons */
					mFacebookLoginButton.Visibility = View.GONE;
					mGoogleLoginButton.Visibility = View.GONE;
					mTwitterLoginButton.Visibility = View.GONE;
					mPasswordLoginButton.Visibility = View.GONE;
					mAnonymousLoginButton.Visibility = View.GONE;
					mLoggedInStatusTextView.Visibility = View.VISIBLE;
					/* show a provider specific status text */
					string name = null;
					if (value.Provider.Equals("facebook") || value.Provider.Equals("google") || value.Provider.Equals("twitter"))
					{
						name = (string) value.ProviderData.get("displayName");
					}
					else if (value.Provider.Equals("anonymous") || value.Provider.Equals("password"))
					{
						name = value.Uid;
					}
					else
					{
						Log.e(TAG, "Invalid provider: " + value.Provider);
					}
					if (name != null)
					{
						mLoggedInStatusTextView.Text = "Logged in as " + name + " (" + value.Provider + ")";
					}
				}
				else
				{
					/* No authenticated user show all the login buttons */
					mFacebookLoginButton.Visibility = View.VISIBLE;
					mGoogleLoginButton.Visibility = View.VISIBLE;
					mTwitterLoginButton.Visibility = View.VISIBLE;
					mPasswordLoginButton.Visibility = View.VISIBLE;
					mAnonymousLoginButton.Visibility = View.VISIBLE;
					mLoggedInStatusTextView.Visibility = View.GONE;
				}
				this.mAuthData = value;
				/* invalidate options menu to hide/show the logout button */
				supportInvalidateOptionsMenu();
			}
		}

		/// <summary>
		/// Show errors to users
		/// </summary>
		private void showErrorDialog(string message)
		{
			(new AlertDialog.Builder(this)).setTitle("Error").setMessage(message).setPositiveButton(android.R.@string.ok, null).setIcon(android.R.drawable.ic_dialog_alert).show();
		}

		/// <summary>
		/// Utility class for authentication results
		/// </summary>
		private class AuthResultHandler : Firebase.AuthResultHandler
		{
			private readonly MainActivity outerInstance;


			internal readonly string provider;

			public AuthResultHandler(MainActivity outerInstance, string provider)
			{
				this.outerInstance = outerInstance;
				this.provider = provider;
			}

			public override void onAuthenticated(AuthData authData)
			{
				outerInstance.mAuthProgressDialog.hide();
				Log.i(TAG, provider + " auth successful");
				outerInstance.AuthenticatedUser = authData;
			}

			public override void onAuthenticationError(FirebaseError firebaseError)
			{
				outerInstance.mAuthProgressDialog.hide();
				outerInstance.showErrorDialog(firebaseError.ToString());
			}
		}

		/* ************************************
		 *             FACEBOOK               *
		 **************************************
		 */
		private void onFacebookAccessTokenChange(AccessToken token)
		{
			if (token != null)
			{
				mAuthProgressDialog.show();
				mFirebaseRef.authWithOAuthToken("facebook", token.Token, new AuthResultHandler(this, "facebook"));
			}
			else
			{
				// Logged out of Facebook and currently authenticated with Firebase using Facebook, so do a logout
				if (this.mAuthData != null && this.mAuthData.Provider.Equals("facebook"))
				{
					mFirebaseRef.unauth();
					AuthenticatedUser = null;
				}
			}
		}

		/* ************************************
		 *              GOOGLE                *
		 **************************************
		 */
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
			mAuthProgressDialog.show();
			/* Get OAuth token in Background */
			AsyncTask<Void, Void, string> task = new AsyncTaskAnonymousInnerClassHelper(this);
			task.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, string>
		{
			private readonly MainActivity outerInstance;

			public AsyncTaskAnonymousInnerClassHelper(MainActivity outerInstance)
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
				if (token != null)
				{
					/* Successfully got OAuth token, now login with Google */
					outerInstance.mFirebaseRef.authWithOAuthToken("google", token, new AuthResultHandler(outerInstance, "google"));
				}
				else if (errorMessage != null)
				{
					outerInstance.mAuthProgressDialog.hide();
					outerInstance.showErrorDialog(errorMessage);
				}
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

		/* ************************************
		 *               TWITTER              *
		 **************************************
		 */
		private void loginWithTwitter()
		{
			startActivityForResult(new Intent(this, typeof(TwitterOAuthActivity)), RC_TWITTER_LOGIN);
		}

		/* ************************************
		 *              PASSWORD              *
		 **************************************
		 */
		public virtual void loginWithPassword()
		{
			mAuthProgressDialog.show();
			mFirebaseRef.authWithPassword("test@firebaseuser.com", "test1234", new AuthResultHandler(this, "password"));
		}

		/* ************************************
		 *             ANONYMOUSLY            *
		 **************************************
		 */
		private void loginAnonymously()
		{
			mAuthProgressDialog.show();
			mFirebaseRef.authAnonymously(new AuthResultHandler(this, "anonymous"));
		}
	}

}