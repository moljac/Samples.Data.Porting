using System.Collections.Generic;

namespace com.opentok.android.demo.opentoksamples
{

	using Activity = android.app.Activity;
	using FragmentTransaction = android.app.FragmentTransaction;
	using NotificationManager = android.app.NotificationManager;
	using PendingIntent = android.app.PendingIntent;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Configuration = android.content.res.Configuration;
	using BitmapFactory = android.graphics.BitmapFactory;
	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Window = android.view.Window;
	using AlphaAnimation = android.view.animation.AlphaAnimation;
	using ProgressBar = android.widget.ProgressBar;
	using RelativeLayout = android.widget.RelativeLayout;
	using LayoutParams = android.widget.RelativeLayout.LayoutParams;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using StreamVideoType = com.opentok.android.Stream.StreamVideoType;
	using OpenTokConfig = com.opentok.android.demo.config.OpenTokConfig;
	using ClearNotificationService = com.opentok.android.demo.services.ClearNotificationService;
	using ClearBinder = com.opentok.android.demo.services.ClearNotificationService.ClearBinder;
	using AudioLevelView = com.opentok.android.demo.ui.AudioLevelView;
	using PublisherControlFragment = com.opentok.android.demo.ui.fragments.PublisherControlFragment;
	using PublisherStatusFragment = com.opentok.android.demo.ui.fragments.PublisherStatusFragment;
	using SubscriberControlFragment = com.opentok.android.demo.ui.fragments.SubscriberControlFragment;
	using SubscriberQualityFragment = com.opentok.android.demo.ui.fragments.SubscriberQualityFragment;


	public class UIActivity : Activity, Session.SessionListener, Session.ArchiveListener, Session.StreamPropertiesListener, Publisher.PublisherListener, Subscriber.VideoListener, Subscriber.SubscriberListener, SubscriberControlFragment.SubscriberCallbacks, PublisherControlFragment.PublisherCallbacks
	{

		private const string LOGTAG = "demo-UI";
		private const int ANIMATION_DURATION = 3000;

		private Session mSession;
		private Publisher mPublisher;
		private Subscriber mSubscriber;
		private List<Stream> mStreams = new List<Stream>();
		private Handler mHandler = new Handler();

		private bool mSubscriberAudioOnly = false;
		private bool archiving = false;
		private bool resumeHasRun = false;

		// View related variables
		private RelativeLayout mPublisherViewContainer;
		private RelativeLayout mSubscriberViewContainer;
		private RelativeLayout mSubscriberAudioOnlyView;

		// Fragments
		private SubscriberControlFragment mSubscriberFragment;
		private PublisherControlFragment mPublisherFragment;
		private PublisherStatusFragment mPublisherStatusFragment;
		private SubscriberQualityFragment mSubscriberQualityFragment;
		private FragmentTransaction mFragmentTransaction;

		// Spinning wheel for loading subscriber view
		private ProgressBar mLoadingSub;

		private AudioLevelView mAudioLevelView;

		private SubscriberQualityFragment.CongestionLevel congestion = SubscriberQualityFragment.CongestionLevel.Low;

		private bool mIsBound = false;
		private NotificationCompat.Builder mNotifyBuilder;
		private NotificationManager mNotificationManager;
		private ServiceConnection mConnection;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			// Remove title bar
			requestWindowFeature(Window.FEATURE_NO_TITLE);

			loadInterface();

			if (savedInstanceState == null)
			{
				mFragmentTransaction = FragmentManager.beginTransaction();
				initSubscriberFragment();
				initPublisherFragment();
				initPublisherStatusFragment();
				initSubscriberQualityFragment();
				mFragmentTransaction.commitAllowingStateLoss();
			}

			mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

			sessionConnect();
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_main, menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			// Handle item selection
			switch (item.ItemId)
			{
				case R.id.menu_settings:
					if (mSubscriber != null)
					{
						onViewClick.onClick(null);
					}
					return true;

				default:
					return base.onOptionsItemSelected(item);
			}
		}

		public override void onConfigurationChanged(Configuration newConfig)
		{
			base.onConfigurationChanged(newConfig);

			// Remove publisher & subscriber views because we want to reuse them
			if (mSubscriber != null)
			{
				mSubscriberViewContainer.removeView(mSubscriber.View);

				if (mSubscriberFragment != null)
				{
					FragmentManager.beginTransaction().remove(mSubscriberFragment).commit();

					initSubscriberFragment();
					if (mSubscriberQualityFragment != null)
					{
						FragmentManager.beginTransaction().remove(mSubscriberQualityFragment).commit();
						initSubscriberQualityFragment();
					}
				}
			}
			if (mPublisher != null)
			{
				mPublisherViewContainer.removeView(mPublisher.View);

				if (mPublisherFragment != null)
				{
					FragmentManager.beginTransaction().remove(mPublisherFragment).commit();

					initPublisherFragment();
				}

				if (mPublisherStatusFragment != null)
				{
					FragmentManager.beginTransaction().remove(mPublisherStatusFragment).commit();

					initPublisherStatusFragment();
				}
			}

			loadInterface();
		}

		public virtual void loadInterface()
		{
			ContentView = R.layout.layout_ui_activity;

			mLoadingSub = (ProgressBar) findViewById(R.id.loadingSpinner);

			mPublisherViewContainer = (RelativeLayout) findViewById(R.id.publisherView);
			mSubscriberViewContainer = (RelativeLayout) findViewById(R.id.subscriberView);
			mSubscriberAudioOnlyView = (RelativeLayout) findViewById(R.id.audioOnlyView);

			//Initialize 
			mAudioLevelView = (AudioLevelView) findViewById(R.id.subscribermeter);
			mAudioLevelView.Icons = BitmapFactory.decodeResource(Resources, R.drawable.headset);
			// Attach running video views
			if (mPublisher != null)
			{
				attachPublisherView(mPublisher);
			}

			// show subscriber status
			mHandler.postDelayed(() =>
			{
				if (mSubscriber != null)
				{
					attachSubscriberView(mSubscriber);

					if (mSubscriberAudioOnly)
					{
						mSubscriber.View.Visibility = View.GONE;
						AudioOnlyView = true;
						congestion = SubscriberQualityFragment.CongestionLevel.High;
					}
				}
			}, 0);

			loadFragments();
		}

		public virtual void loadFragments()
		{
			// show subscriber status
			mHandler.postDelayed(() =>
			{
				if (mSubscriber != null)
				{
					mSubscriberFragment.showSubscriberWidget(true);
					mSubscriberFragment.initSubscriberUI();

					if (congestion != SubscriberQualityFragment.CongestionLevel.Low)
					{
						mSubscriberQualityFragment.Congestion = congestion;
						mSubscriberQualityFragment.showSubscriberWidget(true);
					}
				}
			}, 0);

			// show publisher status
			mHandler.postDelayed(() =>
			{
				if (mPublisher != null)
				{
					mPublisherFragment.showPublisherWidget(true);
					mPublisherFragment.initPublisherUI();

					if (archiving)
					{
						mPublisherStatusFragment.updateArchivingUI(true);
						setPubViewMargins();
					}
				}
			}, 0);

		}

		public virtual void initSubscriberFragment()
		{
			mSubscriberFragment = new SubscriberControlFragment();
			FragmentManager.beginTransaction().add(R.id.fragment_sub_container, mSubscriberFragment).commit();
		}

		public virtual void initPublisherFragment()
		{
			mPublisherFragment = new PublisherControlFragment();
			FragmentManager.beginTransaction().add(R.id.fragment_pub_container, mPublisherFragment).commit();
		}

		public virtual void initPublisherStatusFragment()
		{
			mPublisherStatusFragment = new PublisherStatusFragment();
			FragmentManager.beginTransaction().add(R.id.fragment_pub_status_container, mPublisherStatusFragment).commit();
		}

		public virtual void initSubscriberQualityFragment()
		{
			mSubscriberQualityFragment = new SubscriberQualityFragment();
			FragmentManager.beginTransaction().add(R.id.fragment_sub_quality_container, mSubscriberQualityFragment).commit();
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

			Intent notificationIntent = new Intent(this, typeof(UIActivity));
			notificationIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP;
			PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);

			mNotifyBuilder.ContentIntent = intent;
			if (mConnection == null)
			{
				mConnection = new ServiceConnectionAnonymousInnerClassHelper(this);
			}

			if (!mIsBound)
			{
				Log.d(LOGTAG, "mISBOUND GOT CALLED");
				bindService(new Intent(UIActivity.this, typeof(ClearNotificationService)), mConnection, Context.BIND_AUTO_CREATE);
				mIsBound = true;
				startService(notificationIntent);
			}

		}

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			private readonly UIActivity outerInstance;

			public ServiceConnectionAnonymousInnerClassHelper(UIActivity outerInstance)
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
					if (mSubscriberAudioOnly)
					{
						mSubscriber.View.Visibility = View.GONE;
						AudioOnlyView = true;
						congestion = SubscriberQualityFragment.CongestionLevel.High;
					}
				}
			}, 500);

			loadFragments();
		}

		private void sessionConnect()
		{
			if (mSession == null)
			{
				mSession = new Session(this, OpenTokConfig.API_KEY, OpenTokConfig.SESSION_ID);
				mSession.SessionListener = this;
				mSession.ArchiveListener = this;
				mSession.StreamPropertiesListener = this;
				mSession.connect(OpenTokConfig.TOKEN);
			}
		}

		private void attachPublisherView(Publisher publisher)
		{
			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
			mPublisherViewContainer.addView(publisher.View, layoutParams);
			mPublisherViewContainer.DrawingCacheEnabled = true;
			publisher.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE, BaseVideoRenderer.STYLE_VIDEO_FILL);
			publisher.View.OnClickListener = onViewClick;
		}

		public virtual void onMuteSubscriber()
		{
			if (mSubscriber != null)
			{
				mSubscriber.SubscribeToAudio = !mSubscriber.SubscribeToAudio;
			}
		}

		public virtual void onMutePublisher()
		{
			if (mPublisher != null)
			{
				mPublisher.PublishAudio = !mPublisher.PublishAudio;
			}
		}

		public virtual void onSwapCamera()
		{
			if (mPublisher != null)
			{
				mPublisher.swapCamera();
			}
		}

		public virtual void onEndCall()
		{
			if (mSession != null)
			{
				mSession.disconnect();
			}

			finish();
		}

		private void attachSubscriberView(Subscriber subscriber)
		{
			RelativeLayout.LayoutParams layoutParams = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
			mSubscriberViewContainer.removeView(mSubscriber.View);
			mSubscriberViewContainer.addView(subscriber.View, layoutParams);
			subscriber.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE, BaseVideoRenderer.STYLE_VIDEO_FILL);
			subscriber.View.OnClickListener = onViewClick;
		}

		private void subscribeToStream(Stream stream)
		{
			mSubscriber = new Subscriber(this, stream);
			mSubscriber.SubscriberListener = this;
			mSubscriber.VideoListener = this;
			mSession.subscribe(mSubscriber);

			if (mSubscriber.SubscribeToVideo)
			{
				// start loading spinning
				mLoadingSub.Visibility = View.VISIBLE;
			}
		}

		private void unsubscriberFromStream(Stream stream)
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

		private bool AudioOnlyView
		{
			set
			{
				mSubscriberAudioOnly = value;
    
				if (value)
				{
					mSubscriber.View.Visibility = View.GONE;
					mSubscriberAudioOnlyView.Visibility = View.VISIBLE;
					mSubscriberAudioOnlyView.OnClickListener = onViewClick;
    
					// Audio only text for subscriber
					TextView subStatusText = (TextView) findViewById(R.id.subscriberName);
					subStatusText.Text = R.@string.audioOnly;
					AlphaAnimation aa = new AlphaAnimation(1.0f, 0.0f);
					aa.Duration = ANIMATION_DURATION;
					subStatusText.startAnimation(aa);
    
    
					mSubscriber.AudioLevelListener = new AudioLevelListenerAnonymousInnerClassHelper(this);
				}
				else
				{
					if (!mSubscriberAudioOnly)
					{
						mSubscriber.View.Visibility = View.VISIBLE;
						mSubscriberAudioOnlyView.Visibility = View.GONE;
    
						mSubscriber.AudioLevelListener = null;
					}
				}
			}
		}

		private class AudioLevelListenerAnonymousInnerClassHelper : SubscriberKit.AudioLevelListener
		{
			private readonly UIActivity outerInstance;

			public AudioLevelListenerAnonymousInnerClassHelper(UIActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onAudioLevelUpdated(SubscriberKit subscriber, float audioLevel)
			{
				outerInstance.mAudioLevelView.MeterValue = audioLevel;
			}
		}

		private View.OnClickListener onViewClick = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				bool visible = false;

				if (outerInstance.mPublisher != null)
				{
					// check visibility of bars
					if (!outerInstance.mPublisherFragment.PubControlWidgetVisible)
					{
						visible = true;
					}
					outerInstance.mPublisherFragment.publisherClick();
					if (outerInstance.archiving)
					{
						outerInstance.mPublisherStatusFragment.publisherClick();
					}
					outerInstance.setPubViewMargins();
					if (outerInstance.mSubscriber != null)
					{
						outerInstance.mSubscriberFragment.showSubscriberWidget(visible);
						outerInstance.mSubscriberFragment.initSubscriberUI();
					}
				}
			}
		}

		public virtual Publisher Publisher
		{
			get
			{
				return mPublisher;
			}
		}

		public virtual Subscriber Subscriber
		{
			get
			{
				return mSubscriber;
			}
		}

		public virtual Handler Handler
		{
			get
			{
				return mHandler;
			}
		}

		public override void onConnected(Session session)
		{
			Log.i(LOGTAG, "Connected to the session.");
			if (mPublisher == null)
			{
				mPublisher = new Publisher(this, "Publisher");
				mPublisher.PublisherListener = this;
				attachPublisherView(mPublisher);
				mSession.publish(mPublisher);
			}
		}

		public override void onDisconnected(Session session)
		{
			Log.i(LOGTAG, "Disconnected to the session.");

			if (mPublisher != null)
			{
				mPublisherViewContainer.removeView(mPublisher.Renderer.View);
			}

			if (mSubscriber != null)
			{
				mSubscriberViewContainer.removeView(mSubscriber.Renderer.View);
			}

			mPublisher = null;
			mSubscriber = null;
			mStreams.Clear();
			mSession = null;

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
			mStreams.Remove(stream);
			if (!OpenTokConfig.SUBSCRIBE_TO_SELF)
			{
				if (mSubscriber != null && mSubscriber.Stream.StreamId.Equals(stream.StreamId))
				{
					mSubscriberViewContainer.removeView(mSubscriber.View);
					mSubscriber = null;
					findViewById(R.id.avatar).Visibility = View.GONE;
					mSubscriberAudioOnly = false;
					if (mStreams.Count > 0)
					{
						subscribeToStream(mStreams[0]);
					}
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
			mPublisherFragment.showPublisherWidget(true);
			mPublisherFragment.initPublisherUI();
			mPublisherStatusFragment.showPubStatusWidget(true);
			mPublisherStatusFragment.initPubStatusUI();
		}

		public override void onStreamDestroyed(PublisherKit publisher, Stream stream)
		{

			if (OpenTokConfig.SUBSCRIBE_TO_SELF && mSubscriber != null)
			{
				unsubscriberFromStream(stream);
			}
		}

		public override void onError(Session session, OpentokError exception)
		{
			Toast.makeText(this, exception.Message, Toast.LENGTH_LONG).show();
		}

		public virtual void setPubViewMargins()
		{
			RelativeLayout.LayoutParams pubLayoutParams = (RelativeLayout.LayoutParams) mPublisherViewContainer.LayoutParams;
			int bottomMargin = 0;
			bool controlBarVisible = mPublisherFragment.PubControlWidgetVisible;
			bool statusBarVisible = mPublisherStatusFragment.PubStatusWidgetVisible;
			RelativeLayout pubControlContainer = mPublisherFragment.PublisherContainer;
			RelativeLayout pubStatusContainer = mPublisherStatusFragment.PubStatusContainer;

			if (pubControlContainer != null && pubStatusContainer != null)
			{

				RelativeLayout.LayoutParams pubControlLayoutParams = (RelativeLayout.LayoutParams) pubControlContainer.LayoutParams;
				RelativeLayout.LayoutParams pubStatusLayoutParams = (RelativeLayout.LayoutParams) pubStatusContainer.LayoutParams;

				// setting margins for publisher view on portrait orientation
				if (Resources.Configuration.orientation == Configuration.ORIENTATION_PORTRAIT)
				{
					if (statusBarVisible && archiving)
					{
						// height of publisher control bar + height of publisher status
						// bar + 20 px
						bottomMargin = pubControlLayoutParams.height + pubStatusLayoutParams.height + dpToPx(20);
					}
					else
					{
						if (controlBarVisible)
						{
							// height of publisher control bar + 20 px
							bottomMargin = pubControlLayoutParams.height + dpToPx(20);
						}
						else
						{
							bottomMargin = dpToPx(20);
						}
					}
				}

				// setting margins for publisher view on landscape orientation
				if (Resources.Configuration.orientation == Configuration.ORIENTATION_LANDSCAPE)
				{
					if (statusBarVisible && archiving)
					{
						bottomMargin = pubStatusLayoutParams.height + dpToPx(20);
					}
					else
					{
						bottomMargin = dpToPx(20);
					}
				}

				pubLayoutParams.bottomMargin = bottomMargin;
				pubLayoutParams.leftMargin = dpToPx(20);

				mPublisherViewContainer.LayoutParams = pubLayoutParams;
			}
			if (mSubscriber != null)
			{
				if (mSubscriberAudioOnly)
				{
					RelativeLayout.LayoutParams subLayoutParams = (RelativeLayout.LayoutParams) mSubscriberAudioOnlyView.LayoutParams;
					int subBottomMargin = 0;
					subBottomMargin = pubLayoutParams.bottomMargin;
					subLayoutParams.bottomMargin = subBottomMargin;
					mSubscriberAudioOnlyView.LayoutParams = subLayoutParams;
				}

				setSubQualityMargins();
			}
		}

		public virtual void setSubQualityMargins()
		{
			RelativeLayout subQualityContainer = mSubscriberQualityFragment.SubQualityContainer;
			RelativeLayout pubControlContainer = mPublisherFragment.PublisherContainer;
			RelativeLayout pubStatusContainer = mPublisherStatusFragment.PubStatusContainer;

			if (subQualityContainer != null && pubControlContainer != null && pubStatusContainer != null)
			{
				RelativeLayout.LayoutParams subQualityLayoutParams = (RelativeLayout.LayoutParams) subQualityContainer.LayoutParams;
				bool pubControlBarVisible = mPublisherFragment.PubControlWidgetVisible;
				bool pubStatusBarVisible = mPublisherStatusFragment.PubStatusWidgetVisible;
				RelativeLayout.LayoutParams pubControlLayoutParams = (RelativeLayout.LayoutParams) pubControlContainer.LayoutParams;
				RelativeLayout.LayoutParams pubStatusLayoutParams = (RelativeLayout.LayoutParams) pubStatusContainer.LayoutParams;
				RelativeLayout.LayoutParams audioMeterLayoutParams = (RelativeLayout.LayoutParams) mAudioLevelView.LayoutParams;

				int bottomMargin = 0;

				// control pub fragment
				if (Resources.Configuration.orientation == Configuration.ORIENTATION_PORTRAIT)
				{
					if (pubControlBarVisible)
					{
						bottomMargin = pubControlLayoutParams.height + dpToPx(10);
					}
					if (pubStatusBarVisible && archiving)
					{
						bottomMargin = pubStatusLayoutParams.height + dpToPx(10);
					}
					if (bottomMargin == 0)
					{
						bottomMargin = dpToPx(10);
					}
					subQualityLayoutParams.rightMargin = dpToPx(10);
				}

				if (Resources.Configuration.orientation == Configuration.ORIENTATION_LANDSCAPE)
				{
					if (!pubControlBarVisible)
					{
						subQualityLayoutParams.rightMargin = dpToPx(10);
						bottomMargin = dpToPx(10);
						audioMeterLayoutParams.rightMargin = 0;
						mAudioLevelView.LayoutParams = audioMeterLayoutParams;

					}
					else
					{
						subQualityLayoutParams.rightMargin = pubControlLayoutParams.width;
						bottomMargin = dpToPx(10);
						audioMeterLayoutParams.rightMargin = pubControlLayoutParams.width;
					}
					if (pubStatusBarVisible && archiving)
					{
						bottomMargin = pubStatusLayoutParams.height + dpToPx(10);
					}
					mAudioLevelView.LayoutParams = audioMeterLayoutParams;
				}

				subQualityLayoutParams.bottomMargin = bottomMargin;

				mSubscriberQualityFragment.SubQualityContainer.LayoutParams = subQualityLayoutParams;
			}

		}

		public override void onError(PublisherKit publisher, OpentokError exception)
		{
			Log.i(LOGTAG, "Publisher exception: " + exception.Message);
		}

		public override void onConnected(SubscriberKit subscriber)
		{
			mLoadingSub.Visibility = View.GONE;
			mSubscriberFragment.showSubscriberWidget(true);
			mSubscriberFragment.initSubscriberUI();
		}

		public override void onDisconnected(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Subscriber disconnected.");
		}

		public override void onVideoDataReceived(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "First frame received");

			// stop loading spinning
			mLoadingSub.Visibility = View.GONE;
			attachSubscriberView(mSubscriber);
		}

		public override void onError(SubscriberKit subscriber, OpentokError exception)
		{
			Log.i(LOGTAG, "Subscriber exception: " + exception.Message);
		}

		public override void onVideoDisabled(SubscriberKit subscriber, string reason)
		{
			Log.i(LOGTAG, "Video disabled:" + reason);
			if (mSubscriber == subscriber)
			{
				AudioOnlyView = true;
			}

			if (reason.Equals("quality"))
			{
				mSubscriberQualityFragment.Congestion = SubscriberQualityFragment.CongestionLevel.High;
				congestion = SubscriberQualityFragment.CongestionLevel.High;
				setSubQualityMargins();
				mSubscriberQualityFragment.showSubscriberWidget(true);
			}
		}

		public override void onVideoEnabled(SubscriberKit subscriber, string reason)
		{
			Log.i(LOGTAG, "Video enabled:" + reason);
			if (mSubscriber == subscriber)
			{
				AudioOnlyView = false;
			}
			if (reason.Equals("quality"))
			{
				mSubscriberQualityFragment.Congestion = SubscriberQualityFragment.CongestionLevel.Low;
				congestion = SubscriberQualityFragment.CongestionLevel.Low;
				mSubscriberQualityFragment.showSubscriberWidget(false);
			}
		}

		public override void onStreamHasAudioChanged(Session session, Stream stream, bool audioEnabled)
		{
			Log.i(LOGTAG, "Stream audio changed");
		}

		public override void onStreamHasVideoChanged(Session session, Stream stream, bool videoEnabled)
		{
			Log.i(LOGTAG, "Stream video changed");
		}

		public override void onStreamVideoDimensionsChanged(Session session, Stream stream, int width, int height)
		{
			Log.i(LOGTAG, "Stream video dimensions changed");
		}

		public override void onArchiveStarted(Session session, string id, string name)
		{
			Log.i(LOGTAG, "Archiving starts");
			mPublisherFragment.showPublisherWidget(false);

			archiving = true;
			mPublisherStatusFragment.updateArchivingUI(true);
			mPublisherFragment.showPublisherWidget(true);
			mPublisherFragment.initPublisherUI();
			setPubViewMargins();

			if (mSubscriber != null)
			{
				mSubscriberFragment.showSubscriberWidget(true);
			}
		}

		public override void onArchiveStopped(Session session, string id)
		{
			Log.i(LOGTAG, "Archiving stops");
			archiving = false;

			mPublisherStatusFragment.updateArchivingUI(false);
			setPubViewMargins();

			if (mSubscriber != null)
			{
				setSubQualityMargins();
			}
		}

		/// <summary>
		/// Converts dp to real pixels, according to the screen density.
		/// </summary>
		/// <param name="dp"> A number of density-independent pixels. </param>
		/// <returns> The equivalent number of real pixels. </returns>
		public virtual int dpToPx(int dp)
		{
			double screenDensity = Resources.DisplayMetrics.density;
			return (int)(screenDensity * (double) dp);
		}

		public override void onVideoDisableWarning(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Video may be disabled soon due to network quality degradation. Add UI handling here.");
			mSubscriberQualityFragment.Congestion = SubscriberQualityFragment.CongestionLevel.Mid;
			congestion = SubscriberQualityFragment.CongestionLevel.Mid;
			setSubQualityMargins();
			mSubscriberQualityFragment.showSubscriberWidget(true);
		}

		public override void onVideoDisableWarningLifted(SubscriberKit subscriber)
		{
			Log.i(LOGTAG, "Video may no longer be disabled as stream quality improved. Add UI handling here.");
			mSubscriberQualityFragment.Congestion = SubscriberQualityFragment.CongestionLevel.Low;
			congestion = SubscriberQualityFragment.CongestionLevel.Low;
			mSubscriberQualityFragment.showSubscriberWidget(false);
		}

		public override void onStreamVideoTypeChanged(Session session, Stream stream, Stream.StreamVideoType videoType)
		{
			Log.i(LOGTAG, "Stream video type changed");
		}

	}

}