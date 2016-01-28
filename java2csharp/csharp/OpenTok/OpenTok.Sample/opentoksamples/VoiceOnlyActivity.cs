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
	using BitmapFactory = android.graphics.BitmapFactory;
	using AudioManager = android.media.AudioManager;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using ImageView = android.widget.ImageView;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using ArchiveListener = com.opentok.android.Session.ArchiveListener;
	using SessionListener = com.opentok.android.Session.SessionListener;
	using OpenTokConfig = com.opentok.android.demo.config.OpenTokConfig;
	using ClearNotificationService = com.opentok.android.demo.services.ClearNotificationService;
	using ClearBinder = com.opentok.android.demo.services.ClearNotificationService.ClearBinder;
	using MeterView = com.opentok.android.demo.ui.MeterView;


	public class VoiceOnlyActivity : Activity, Session.SessionListener, Session.ArchiveListener
	{
		private bool InstanceFieldsInitialized = false;

		public VoiceOnlyActivity()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mSubscriberAdapter = new MyAdapter(this, this, R.layout.voice_row);
		}


		private const string LOGTAG = "demo-voice-only";
		private Session mSession;
		private Publisher mPublisher;
		private List<Subscriber> mSubscribers = new List<Subscriber>();
		private Dictionary<Stream, Subscriber> mSubscriberStream = new Dictionary<Stream, Subscriber>();
		private MyAdapter mSubscriberAdapter;
		private Handler mHandler = new Handler();

		private bool mIsBound = false;
		private NotificationCompat.Builder mNotifyBuilder;
		private NotificationManager mNotificationManager;
		private ServiceConnection mConnection;

		public class MyAdapter : BaseAdapter
		{
			private readonly VoiceOnlyActivity outerInstance;

			internal readonly Context mContext;
			internal int mResource;

			internal MyAdapter(VoiceOnlyActivity outerInstance, Context context, int resource) : base()
			{
				this.outerInstance = outerInstance;
				this.mContext = context;
				this.mResource = resource;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				LayoutInflater inflater = (LayoutInflater) mContext.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
				View rowView = inflater.inflate(mResource, parent, false);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.opentok.android.Subscriber subscriber = mSubscribers.get(position);
				Subscriber subscriber = outerInstance.mSubscribers[position];

				// Set name
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.TextView name = (android.widget.TextView) rowView.findViewById(R.id.subscribername);
				TextView name = (TextView) rowView.findViewById(R.id.subscribername);
				name.Text = subscriber.Stream.Name;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ImageView picture = (android.widget.ImageView) rowView.findViewById(R.id.subscriberpicture);
				ImageView picture = (ImageView) rowView.findViewById(R.id.subscriberpicture);

				// Initialize meter view
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.opentok.android.demo.ui.MeterView meterView = (com.opentok.android.demo.ui.MeterView) rowView.findViewById(R.id.volume);
				MeterView meterView = (MeterView) rowView.findViewById(R.id.volume);
				meterView.setIcons(BitmapFactory.decodeResource(Resources, R.drawable.unmute_sub), BitmapFactory.decodeResource(Resources, R.drawable.mute_sub));
				subscriber.AudioLevelListener = new AudioLevelListenerAnonymousInnerClassHelper(this, subscriber, meterView);

				meterView.setOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this, subscriber, name, picture));

				return rowView;
			}

			private class AudioLevelListenerAnonymousInnerClassHelper : SubscriberKit.AudioLevelListener
			{
				private readonly MyAdapter outerInstance;

				private Subscriber subscriber;
				private MeterView meterView;

				public AudioLevelListenerAnonymousInnerClassHelper(MyAdapter outerInstance, Subscriber subscriber, MeterView meterView)
				{
					this.outerInstance = outerInstance;
					this.subscriber = subscriber;
					this.meterView = meterView;
				}

				public override void onAudioLevelUpdated(SubscriberKit subscriber, float audioLevel)
				{
					meterView.MeterValue = audioLevel;
				}
			}

			private class OnClickListenerAnonymousInnerClassHelper : MeterView.OnClickListener
			{
				private readonly MyAdapter outerInstance;

				private Subscriber subscriber;
				private TextView name;
				private ImageView picture;

				public OnClickListenerAnonymousInnerClassHelper(MyAdapter outerInstance, Subscriber subscriber, TextView name, ImageView picture)
				{
					this.outerInstance = outerInstance;
					this.subscriber = subscriber;
					this.name = name;
					this.picture = picture;
				}

				public virtual void onClick(MeterView view)
				{
					subscriber.SubscribeToAudio = !view.Muted;
					float alpha = view.Muted ? 0.70f : 1.0f;
					name.Alpha = alpha;
					picture.Alpha = alpha;
				}
			}

			public override int Count
			{
				get
				{
					// TODO Auto-generated method stub
					return outerInstance.mSubscribers.Count;
				}
			}

			public override object getItem(int position)
			{
				// TODO Auto-generated method stub
				return outerInstance.mSubscribers[position];
			}

			public override long getItemId(int position)
			{
				// TODO Auto-generated method stub
				return 0;
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.i(LOGTAG, "ONCREATE");
			base.onCreate(savedInstanceState);

			ContentView = R.layout.voice_only_layout;

			ListView listView = (ListView) findViewById(R.id.listview);
			listView.Adapter = mSubscriberAdapter;

			// Set meter view icons for publisher
			MeterView mv = (MeterView) findViewById(R.id.publishermeter);
			mv.setIcons(BitmapFactory.decodeResource(Resources, R.drawable.unmute_pub), BitmapFactory.decodeResource(Resources, R.drawable.mute_pub));

			ActionBar actionBar = ActionBar;
			actionBar.HomeButtonEnabled = true;
			actionBar.DisplayHomeAsUpEnabled = true;

			mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

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
			}

			mNotifyBuilder = (new NotificationCompat.Builder(this)).setContentTitle(this.Title).setContentText(Resources.getString(R.@string.notification)).setSmallIcon(R.drawable.ic_launcher).setOngoing(true);

			Intent notificationIntent = new Intent(this, typeof(VoiceOnlyActivity));
			notificationIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP;
			PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);

			mNotifyBuilder.ContentIntent = intent;
			if (mConnection == null)
			{
				mConnection = new ServiceConnectionAnonymousInnerClassHelper(this);
			}

			if (!mIsBound)
			{
				bindService(new Intent(VoiceOnlyActivity.this, typeof(ClearNotificationService)), mConnection, Context.BIND_AUTO_CREATE);
				mIsBound = true;
				startService(notificationIntent);
			}
		}

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			private readonly VoiceOnlyActivity outerInstance;

			public ServiceConnectionAnonymousInnerClassHelper(VoiceOnlyActivity outerInstance)
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
			if (mSession != null)
			{
				mSession.onResume();
			}

			mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);
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
			restartAudioMode();

			base.onDestroy();
			finish();
		}

		public override void onBackPressed()
		{
			if (mSession != null)
			{
				mSession.disconnect();
			}
			restartAudioMode();

			base.onBackPressed();
		}

		public virtual void restartAudioMode()
		{
			AudioManager Audio = (AudioManager) getSystemService(Context.AUDIO_SERVICE);
			Audio.Mode = AudioManager.MODE_NORMAL;
			this.VolumeControlStream = AudioManager.USE_DEFAULT_STREAM_TYPE;
		}

		private void sessionConnect()
		{
			if (mSession == null)
			{
				mSession = new Session(this, OpenTokConfig.API_KEY, OpenTokConfig.SESSION_ID);
				mSession.SessionListener = this;
				mSession.ArchiveListener = this;
				mSession.connect(OpenTokConfig.TOKEN);
			}
		}

		public virtual void onEndCall(View v)
		{
			finish();
		}

		public override void onConnected(Session session)
		{
			mPublisher = new Publisher(this, "Publisher");
			// Publish audio only
			mPublisher.PublishVideo = false;
			mSession.publish(mPublisher);

			// Initialize publisher meter view
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.opentok.android.demo.ui.MeterView meterView = (com.opentok.android.demo.ui.MeterView) findViewById(R.id.publishermeter);
			MeterView meterView = (MeterView) findViewById(R.id.publishermeter);
			mPublisher.AudioLevelListener = new AudioLevelListenerAnonymousInnerClassHelper(this, meterView);
			meterView.setOnClickListener(new OnClickListenerAnonymousInnerClassHelper(this));
		}

		private class AudioLevelListenerAnonymousInnerClassHelper : PublisherKit.AudioLevelListener
		{
			private readonly VoiceOnlyActivity outerInstance;

			private MeterView meterView;

			public AudioLevelListenerAnonymousInnerClassHelper(VoiceOnlyActivity outerInstance, MeterView meterView)
			{
				this.outerInstance = outerInstance;
				this.meterView = meterView;
			}

			public override void onAudioLevelUpdated(PublisherKit publisher, float audioLevel)
			{
				meterView.MeterValue = audioLevel;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : MeterView.OnClickListener
		{
			private readonly VoiceOnlyActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(VoiceOnlyActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(MeterView view)
			{
				outerInstance.mPublisher.PublishAudio = !view.Muted;
			}
		}

		public override void onDisconnected(Session session)
		{
		}

		public override void onStreamReceived(Session session, Stream stream)
		{
			Subscriber subscriber = new Subscriber(this, stream);

			// Subscribe audio only
			subscriber.SubscribeToVideo = false;

			mSession.subscribe(subscriber);
			mSubscribers.Add(subscriber);
			mSubscriberStream[stream] = subscriber;
			mSubscriberAdapter.notifyDataSetChanged();
		}

		public override void onStreamDropped(Session session, Stream stream)
		{
			Subscriber subscriber = mSubscriberStream[stream];
			if (subscriber != null)
			{
				mSession.unsubscribe(subscriber);
				mSubscribers.Remove(subscriber);
				mSubscriberStream.Remove(stream);
				mSubscriberAdapter.notifyDataSetChanged();
			}
		}

		public override void onError(Session session, OpentokError error)
		{
			Toast.makeText(this, error.Message, Toast.LENGTH_LONG).show();
		}

		private Runnable mHideStatus = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{
				findViewById(R.id.archivingbar).Visibility = View.GONE;
			}
		}

		private void setArchiving(int text, int img)
		{
			findViewById(R.id.archivingbar).Visibility = View.VISIBLE;
			TextView statusText = (TextView) findViewById(R.id.archivingstatus);
			ImageView archiving = (ImageView) findViewById(R.id.archivingimg);
			statusText.Text = text;
			archiving.ImageResource = img;
			mHandler.removeCallbacks(mHideStatus);
			mHandler.postDelayed(mHideStatus, 5000);
		}

		public override void onArchiveStarted(Session session, string id, string name)
		{
			setArchiving(R.@string.archivingOn, R.drawable.archiving_on);
		}

		public override void onArchiveStopped(Session session, string id)
		{
			setArchiving(R.@string.archivingOff, R.drawable.archiving_off);
		}

	}

}