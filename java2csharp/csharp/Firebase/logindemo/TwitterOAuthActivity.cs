using System;

namespace com.firebase.samples.logindemo
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using Uri = android.net.Uri;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using WebView = android.webkit.WebView;
	using WebViewClient = android.webkit.WebViewClient;

	using Twitter = twitter4j.Twitter;
	using TwitterException = twitter4j.TwitterException;
	using TwitterFactory = twitter4j.TwitterFactory;
	using AccessToken = twitter4j.auth.AccessToken;
	using RequestToken = twitter4j.auth.RequestToken;
	using ConfigurationBuilder = twitter4j.conf.ConfigurationBuilder;

	/// <summary>
	/// The TwitterOAuthActivity provides a simple web view for users authenticating with Twitter. To do this authentication,
	/// we do the following steps:
	/// <p/>
	/// 1. Using twitter4j, get the request token, request token secret, and oauth verifier
	/// 2. Open a web view for the user to give the application access
	/// 3. Using twitter4j, get the authentication token, secret, and user id with a accepted request token
	/// 4. Return to the <seealso cref="com.firebase.samples.logindemo.MainActivity"/> with the new access token
	/// </summary>
	public class TwitterOAuthActivity : Activity
	{

		private static readonly string TAG = typeof(TwitterOAuthActivity).Name;

		private WebView mTwitterView;

		private Twitter mTwitter;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			// setup twitter client
			mTwitter = new TwitterFactory(new ConfigurationBuilder()
					.setOAuthConsumerKey(Resources.getString(R.@string.twitter_consumer_key)).setOAuthConsumerSecret(Resources.getString(R.@string.twitter_consumer_secret)).build()).Instance;

			// setup twitter webview
			mTwitterView = new WebView(this);
			mTwitterView.Settings.JavaScriptEnabled = true;

			// initialize view
			ContentView = mTwitterView;

			// start the web view
			loginToTwitter();
		}

		private void loginToTwitter()
		{
			// fetch the oauth request token then prompt the user to authorize the application
			new AsyncTaskAnonymousInnerClassHelper(this)
			.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, RequestToken>
		{
			private readonly TwitterOAuthActivity outerInstance;

			public AsyncTaskAnonymousInnerClassHelper(TwitterOAuthActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override RequestToken doInBackground(params Void[] @params)
			{
				RequestToken token = null;
				try
				{
					token = outerInstance.mTwitter.getOAuthRequestToken("oauth://cb");
				}
				catch (TwitterException te)
				{
					Log.e(TAG, te.ToString());
				}
				return token;
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override protected void onPostExecute(final twitter4j.auth.RequestToken token)
			protected internal override void onPostExecute(RequestToken token)
			{
				outerInstance.mTwitterView.WebViewClient = new WebViewClientAnonymousInnerClassHelper(this, token);
				outerInstance.mTwitterView.loadUrl(token.AuthorizationURL);
			}

			private class WebViewClientAnonymousInnerClassHelper : WebViewClient
			{
				private readonly AsyncTaskAnonymousInnerClassHelper outerInstance;

				private RequestToken token;

				public WebViewClientAnonymousInnerClassHelper(AsyncTaskAnonymousInnerClassHelper outerInstance, RequestToken token)
				{
					this.outerInstance = outerInstance;
					this.token = token;
				}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onPageFinished(final android.webkit.WebView view, final String url)
				public override void onPageFinished(WebView view, string url)
				{
					if (url.StartsWith("oauth://cb", StringComparison.Ordinal))
					{
						outerInstance.outerInstance.getTwitterOAuthTokenAndLogin(token, Uri.parse(url).getQueryParameter("oauth_verifier"));
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void getTwitterOAuthTokenAndLogin(final twitter4j.auth.RequestToken requestToken, final String oauthVerifier)
		private void getTwitterOAuthTokenAndLogin(RequestToken requestToken, string oauthVerifier)
		{
			// once a user authorizes the application, get the auth token and return to the MainActivity
			new AsyncTaskAnonymousInnerClassHelper2(this, requestToken, oauthVerifier)
			.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper2 : AsyncTask<Void, Void, AccessToken>
		{
			private readonly TwitterOAuthActivity outerInstance;

			private RequestToken requestToken;
			private string oauthVerifier;

			public AsyncTaskAnonymousInnerClassHelper2(TwitterOAuthActivity outerInstance, RequestToken requestToken, string oauthVerifier)
			{
				this.outerInstance = outerInstance;
				this.requestToken = requestToken;
				this.oauthVerifier = oauthVerifier;
			}

			protected internal override AccessToken doInBackground(params Void[] @params)
			{
				AccessToken accessToken = null;
				try
				{
					accessToken = outerInstance.mTwitter.getOAuthAccessToken(requestToken, oauthVerifier);
				}
				catch (TwitterException te)
				{
					Log.e(TAG, te.ToString());
				}
				return accessToken;
			}

			protected internal override void onPostExecute(AccessToken token)
			{
				Intent resultIntent = new Intent();
				resultIntent.putExtra("oauth_token", token.Token);
				resultIntent.putExtra("oauth_token_secret", token.TokenSecret);
				resultIntent.putExtra("user_id", token.UserId + "");
				setResult(MainActivity.RC_TWITTER_LOGIN, resultIntent);
				finish();
			}
		}
	}

}