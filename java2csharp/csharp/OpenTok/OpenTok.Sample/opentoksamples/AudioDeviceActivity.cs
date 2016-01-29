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
	using ProgressBar = android.widget.ProgressBar;
	using RelativeLayout = android.widget.RelativeLayout;

	using CustomAudioDevice = com.opentok.android.demo.audio.CustomAudioDevice;
	using OpenTokConfig = com.opentok.android.demo.config.OpenTokConfig;
	using ClearNotificationService = com.opentok.android.demo.services.ClearNotificationService;

	/// <summary>
	/// This application demonstrates the basic workflow for getting started with the
	/// OpenTok 2.0 Android SDK. For more information, see the README.md file in the
	/// samples directory.
	/// </summary>
	public class AudioDeviceActivity : Activity, Session.SessionListener, Publisher.PublisherListener, Subscriber.VideoListener
	{


		private const string LOGTAG = "demo-hello-world";
		private Session mSession;
		private Publisher mPublisher;
		private Subscriber mSubscriber;
		private List<Stream> mStreams;
		private Handler mHandler = new Handler();

		private RelativeLayout mPublisherViewContainer;
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
			base.onCreate(savedInstanceState);

			ContentView = R.layout.main_layout;

			ActionBar actionBar = ActionBar;
			actionBar.HomeButtonEnabled = true;
			actionBar.DisplayHomeAsUpEnabled = true;

			mPublisherViewContainer = (RelativeLayout) findViewById(R.id.publisherview);
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

			Intent notificationIntent = new Intent(this, typeof(AudioDeviceActivity));
			notificationIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP;
			PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);

			mNotifyBuilder.ContentIntent = intent;
			if (mConnection == null)
			{
				mConnection = new ServiceConnectionAnonymousInnerClassHelper(this);
			}

			if (!mIsBound)
			{
				bindService(new Intent(AudioDeviceActivity.this, typeof(ClearNotificationService)), mConnection, Context.BIND_AUTO_CREATE);
				mIsBound = true;
				startService(notificationIntent);
			}

		}

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			private readonly AudioDeviceActivity outerInstance;

			public ServiceConnectionAnonymousInnerClassHelper(AudioDeviceActivity outerInstance)
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
				// Add a custom audio device before session initialization
				CustomAudioDevice customAudioDevice = new CustomAudioDevice(AudioDeviceActivity.this);
				AudioDeviceManager.AudioDevice = customAudioDevice;

				mSession = new Session(AudioDeviceActivity.this, OpenTokConfig.API_KEY, OpenTokConfig.SESSION_ID);
				mSession.SessionListener = this;
				mSession.connect(OpenTokConfig.TOKEN);
			}
		}

		public override void onConnected(Session session)
		{
			Log.i(LOGTAG, "Connected to the session.");
			if (mPublisher == null)
			{
				mPublisher = new Publisher(AudioDeviceActivity.this, "publisher");
				mPublisher.PublisherListener = this;
				attachPublisherView(mPublisher);
				mSession.publish(mPublisher);
			}
		}

		public override void onDisconnected(Session session)
		{
			Log.i(LOGTAG, "Disconnected from the session.");
			if (mPublisher != null)
			{
				mPublisherViewContainer.removeView(mPublisher.View);
			}

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
			mSubscriber = new Subscriber(AudioDeviceActivity.this, stream);
			mSubscriber.VideoListener = this;
			mSession.subscribe(mSubscriber);
			// start loading spinning
			mLoadingSub.Visibility = View.VISIBLE;
		}

		private void unsubscribeFromStream(Stream stream)
		{
			mStreams.Remove(stream);
			if (mSubscriber.Stream.Equals(stream))
			{
				mSubscriberViewContainer.removeView(mSubscriber.View);
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
			mSubscriberViewContainer.addView(subscriber.View, layoutParams);
			subscriber.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE, BaseVideoRenderer.STYLE_VIDEO_FILL);
		}

		private void attachPublisherView(Publisher publisher)
		{
			mPublisher.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE, BaseVideoRenderer.STYLE_VIDEO_FILL);
			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(320, 240);
			layoutParams.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM, RelativeLayout.TRUE);
			layoutParams.addRule(RelativeLayout.ALIGN_PARENT_RIGHT, RelativeLayout.TRUE);
			layoutParams.bottomMargin = dpToPx(8);
			layoutParams.rightMargin = dpToPx(8);
			mPublisherViewContainer.addView(publisher.View, layoutParams);
		}

		public override void onError(Session session, OpentokError exception)
		{
			Log.i(LOGTAG, "Session exception: " + exception.Message);
		}

		public override void onStreamReceived(Session session, Stream stream)
		{

			if (!OpenTokConfig.SUBSCRIBE_TO_SELF)
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
			if (!OpenTokConfig.SUBSCRIBE_TO_SELF)
			{
				if (mSubscriber != null)
				{
					unsubscribeFromStream(stream);
				}
			}
		}

		public override void onStreamCreated(PublisherKit publisher, Stream stream)
		{
			if (OpenTokConfig.SUBSCRIBE_TO_SELF)
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
			if ((OpenTokConfig.SUBSCRIBE_TO_SELF && mSubscriber != null))
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

	}
}