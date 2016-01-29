using System.Collections.Generic;

namespace com.opentok.android.demo.opentoksamples
{

	using ActionBar = android.app.ActionBar;
	using Activity = android.app.Activity;
	using NotificationManager = android.app.NotificationManager;
	using PendingIntent = android.app.PendingIntent;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using Log = android.util.Log;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using WebSettings = android.webkit.WebSettings;
	using WebView = android.webkit.WebView;
	using WebViewClient = android.webkit.WebViewClient;
	using ProgressBar = android.widget.ProgressBar;
	using RelativeLayout = android.widget.RelativeLayout;

	using PublisherKitVideoType = com.opentok.android.PublisherKit.PublisherKitVideoType;
	using ScreensharingCapturer = com.opentok.android.demo.screensharing.ScreensharingCapturer;
	using ClearNotificationService = com.opentok.android.demo.services.ClearNotificationService;
	using ClearBinder = com.opentok.android.demo.services.ClearNotificationService.ClearBinder;


	public class ScreenSharingActivity : Activity, Session.SessionListener, Publisher.PublisherListener, Subscriber.VideoListener, Subscriber.SubscriberListener
	{

		private const string LOGTAG = "demo-hello-world";
		private Session mSession;
		private Publisher mPublisher;
		private Subscriber mSubscriber;
		private List<Stream> mStreams;
		private Handler mHandler = new Handler();

		private WebView mPubScreenWebView;
		private RelativeLayout mSubscriberViewContainer;

		// Spinning wheel for loading subscriber view
		private ProgressBar mLoadingSub;

		private bool resumeHasRun = false;

		private bool mIsBound = false;
		private NotificationCompat.Builder mNotifyBuilder;
		private NotificationManager mNotificationManager;
		private ServiceConnection mConnection;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.i(LOGTAG, "ONCREATE");
			base.onCreate(savedInstanceState);

			ContentView = R.layout.screensharing_layout;

			ActionBar actionBar = ActionBar;
			actionBar.HomeButtonEnabled = true;
			actionBar.DisplayHomeAsUpEnabled = true;

			//We are using a webView to show the screensharing action
			//If we want to share our screen we could use: mView = ((Activity)this.context).getWindow().getDecorView().findViewById(android.R.id.content);
			mPubScreenWebView = (WebView) findViewById(R.id.webview_screen);

			mSubscriberViewContainer = (RelativeLayout) findViewById(R.id.subscriberview);
			mLoadingSub = (ProgressBar) findViewById(R.id.loadingSpinner);

			mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

			mStreams = new List<Stream>();

			sessionConnect();
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			switch (item.ItemId)
			{
				case android.R.id.home:
					onBackPressed();
					return true;
				default:
					return base.onOptionsItemSelected(item);
			}
		}

		public override void onPause()
		{
			base.onPause();

			if (mSession != null)
			{
				mSession.onPause();

				if (mSubscriber != null)
				{
					mSubscriberViewContainer.removeView(mSubscriber.View);
				}
			}

			mNotifyBuilder = (new NotificationCompat.Builder(this)).setContentTitle(this.Title).setContentText(Resources.getString(R.@string.notification)).setSmallIcon(R.drawable.ic_launcher).setOngoing(true);

			Intent notificationIntent = new Intent(this, typeof(ScreenSharingActivity));
			notificationIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP;
			PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);

			mNotifyBuilder.ContentIntent = intent;
			if (mConnection == null)
			{
				mConnection = new ServiceConnectionAnonymousInnerClassHelper(this);
			}

			if (!mIsBound)
			{
				bindService(new Intent(ScreenSharingActivity.this, typeof(ClearNotificationService)), mConnection, Context.BIND_AUTO_CREATE);
				mIsBound = true;
				startService(notificationIntent);
			}

		}

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			private readonly ScreenSharingActivity outerInstance;

			public ServiceConnectionAnonymousInnerClassHelper(ScreenSharingActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onServiceConnected(ComponentName className, IBinder binder)
			{
				((ClearNotificationService.ClearBinder) binder).service.startService(new Intent(outerInstance, typeof(ClearNotificationService)));
				NotificationManager mNotificationManager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
				mNotificationManager.notify(ClearNotificationService.NOTIFICATION_ID, outerInstance.mNotifyBuilder.build());
			}

			public override void onServiceDisconnected(ComponentName className)
			{
				outerInstance.mConnection = null;
			}

		}

		public override void onResume()
		{
			base.onResume();

			if (mIsBound)
			{
				unbindService(mConnection);
				mIsBound = false;
			}

			if (!resumeHasRun)
			{
				resumeHasRun = true;
				return;
			}
			else
			{
				if (mSession != null)
				{
					mSession.onResume();
				}
			}
			mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);

			reloadInterface();
		}

		public override void onStop()
		{
			base.onStop();

			if (mIsBound)
			{
				unbindService(mConnection);
				mIsBound = false;
			}

			if (mIsBound)
			{
				unbindService(mConnection);
				mIsBound = false;
			}
			if (Finishing)
			{
				mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);
				if (mSession != null)
				{
					mSession.disconnect();
				}
			}
		}

		public override void onDestroy()
		{
			mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);
			if (mIsBound)
			{
				unbindService(mConnection);
				mIsBound = false;
			}

			if (mSession != null)
			{
				mSession.disconnect();
			}

			base.onDestroy();
			finish();
		}

		public override void onBackPressed()
		{
			if (mSession != null)
			{
				mSession.disconnect();
			}

			base.onBackPressed();
		}

		public virtual void reloadInterface()
		{
			mHandler.postDelayed(() =>
			{
				if (mSubscriber != null)
				{
					attachSubscriberView(mSubscriber);
				}
			}, 500);
		}

		private void sessionConnect()
		{
			if (mSession == null)
			{
				mSession = new Session(ScreenSharingActivity.this, com.opentok.android.demo.config.OpenTokConfig.API_KEY, com.opentok.android.demo.config.OpenTokConfig.SESSION_ID);
				mSession.SessionListener = this;
				mSession.connect(com.opentok.android.demo.config.OpenTokConfig.TOKEN);
			}
		}

		public override void onConnected(Session session)
		{
			Log.i(LOGTAG, "Connected to the session.");

			//Start screensharing
			if (mPublisher == null)
			{
				mPublisher = new Publisher(ScreenSharingActivity.this, "publisher");
				mPublisher.PublisherListener = this;
				mPublisher.PublisherVideoType = PublisherKit.PublisherKitVideoType.PublisherKitVideoTypeScreen;
				mPublisher.AudioFallbackEnabled = false;
				ScreensharingCapturer screenCapturer = new ScreensharingCapturer(this, mPubScreenWebView);
				mPublisher.Capturer = screenCapturer;
				loadScreenWebView();

				mSession.publish(mPublisher);
			}

		}

		public override void onDisconnected(Session session)
		{
			Log.i(LOGTAG, "Disconnected from the session.");
			if (mSubscriber != null)
			{
				mSubscriberViewContainer.removeView(mSubscriber.View);
			}

			mPublisher = null;
			mSubscriber = null;
			mStreams.Clear();
			mSession = null;
		}

		private void subscribeToStream(Stream stream)
		{
			mSubscriber = new Subscriber(ScreenSharingActivity.this, stream);
			mSubscriber.VideoListener = this;
			mSubscriber.SubscriberListener = this;
			mSession.subscribe(mSubscriber);
			mSubscriberViewContainer.Visibility = View.VISIBLE;
			if (mSubscriber.SubscribeToVideo)
			{
				// start loading spinning
				mLoadingSub.Visibility = View.VISIBLE;
			}
		}

		private void unsubscribeFromStream(Stream stream)
		{
			mStreams.Remove(stream);
			if (mSubscriber.Stream.Equals(stream))
			{
				mSubscriberViewContainer.removeView(mSubscriber.View);
				mSubscriberViewContainer.Visibility = View.GONE;
				mSubscriber = null;
				if (mStreams.Count > 0)
				{
					subscribeToStream(mStreams[0]);
				}
			}
		}

		private void attachSubscriberView(Subscriber subscriber)
		{
			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(Resources.DisplayMetrics.widthPixels, Resources.DisplayMetrics.heightPixels);
			mSubscriberViewContainer.removeView(mSubscriber.View);
			mSubscriberViewContainer.addView(mSubscriber.View, layoutParams);
		}


		private void loadScreenWebView()
		{
			mPubScreenWebView.WebViewClient = new WebViewClient();
			WebSettings webSettings = mPubScreenWebView.Settings;
			webSettings.JavaScriptEnabled = true;
			mPubScreenWebView.setLayerType(View.LAYER_TYPE_SOFTWARE, null); // to turn off hardware-accelerated canvas
			mPubScreenWebView.loadUrl("http://www.tokbox.com");
		}


		public override void onError(Session session, OpentokError exception)
		{
			Log.i(LOGTAG, "Session exception: " + exception.Message);
		}

		public override void onStreamReceived(Session session, Stream stream)
		{
			if (!com.opentok.android.demo.config.OpenTokConfig.SUBSCRIBE_TO_SELF)
			{
				mStreams.Add(stream);
				if (mSubscriber == null)
				{
					subscribeToStream(stream);
				}
			}
		}

		public override void onStreamDropped(Session session, Stream stream)
		{
			if (!com.opentok.android.demo.config.OpenTokConfig.SUBSCRIBE_TO_SELF)
			{
				if (mSubscriber != null)
				{
					unsubscribeFromStream(stream);
				}
			}
		}

		public override void onStreamCreated(PublisherKit publisher, Stream stream)
		{
			if (com.opentok.android.demo.config.OpenTokConfig.SUBSCRIBE_TO_SELF)
			{
				mStreams.Add(stream);
				if (mSubscriber == null)
				{
					subscribeToStream(stream);
				}
			}
		}

		public override void onStreamDestroyed(PublisherKit publisher, Stream stream)
		{
			if ((com.opentok.android.demo.config.OpenTokConfig.SUBSCRIBE_TO_SELF && mSubscriber != null))
			{
				unsubscribeFromStream(stream);
			}
		}

		public override void onError(PublisherKit publisher, OpentokError exception)
		{
			Log.i(LOGTAG, "Publisher exception: " + exception.Message);
		}

		public override void onVideoDataReceived(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "First frame received");

			// stop loading spinning
			mLoadingSub.Visibility = View.GONE;
			attachSubscriberView(mSubscriber);
		}

		/// <summary>
		/// Converts dp to real pixels, according to the screen density.
		/// </summary>
		/// <param name="dp"> A number of density-independent pixels. </param>
		/// <returns> The equivalent number of real pixels. </returns>
		private int dpToPx(int dp)
		{
			double screenDensity = this.Resources.DisplayMetrics.density;
			return (int)(screenDensity * (double) dp);
		}

		public override void onVideoDisabled(SubscriberKit subscriber, string reason)
		{
			Log.i(LOGTAG, "Video disabled:" + reason);
		}

		public override void onVideoEnabled(SubscriberKit subscriber, string reason)
		{
			Log.i(LOGTAG, "Video enabled:" + reason);
		}

		public override void onVideoDisableWarning(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Video may be disabled soon due to network quality degradation. Add UI handling here.");
		}

		public override void onVideoDisableWarningLifted(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Video may no longer be disabled as stream quality improved. Add UI handling here.");
		}


		public override void onConnected(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Subscriber is connected: ");

		}

		public override void onDisconnected(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Subscriber is disconnected: ");

		}

		public override void onError(SubscriberKit subscriber, OpentokError exception)
		{
			Log.i(LOGTAG, "Subscriber exception: " + exception.Message);
		}
	}
}