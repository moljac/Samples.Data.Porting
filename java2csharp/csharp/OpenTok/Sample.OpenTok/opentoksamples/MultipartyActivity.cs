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
	using IBinder = android.os.IBinder;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using ViewPager = android.support.v4.view.ViewPager;
	using Log = android.util.Log;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using EditText = android.widget.EditText;
	using ScrollView = android.widget.ScrollView;
	using TextView = android.widget.TextView;

	using MySession = com.opentok.android.demo.multiparty.MySession;
	using ClearNotificationService = com.opentok.android.demo.services.ClearNotificationService;
	using ClearBinder = com.opentok.android.demo.services.ClearNotificationService.ClearBinder;

	public class MultipartyActivity : Activity
	{

		private const string LOGTAG = "demo-subclassing";

		private MySession mSession;
		private EditText mMessageEditText;
		private bool resumeHasRun = false;
		private bool mIsBound = false;
		private NotificationCompat.Builder mNotifyBuilder;
		private NotificationManager mNotificationManager;
		private ServiceConnection mConnection;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.room;

			ActionBar actionBar = ActionBar;
			actionBar.HomeButtonEnabled = true;
			actionBar.DisplayHomeAsUpEnabled = true;

			mSession = new MySession(this);

			mMessageEditText = (EditText) findViewById(R.id.message);

			ViewGroup preview = (ViewGroup) findViewById(R.id.preview);
			mSession.PreviewView = preview;

			mNotificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);

			ViewPager playersView = (ViewPager) findViewById(R.id.pager);
			mSession.PlayersViewContainer = playersView;
			mSession.setMessageView((TextView) findViewById(R.id.messageView), (ScrollView) findViewById(R.id.scroller));

			mSession.connect();
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

			Intent notificationIntent = new Intent(this, typeof(MultipartyActivity));
			notificationIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP;
			PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);
			mNotifyBuilder.ContentIntent = intent;

			if (mConnection == null)
			{
				mConnection = new ServiceConnectionAnonymousInnerClassHelper(this);
			}

			if (!mIsBound)
			{
				bindService(new Intent(MultipartyActivity.this, typeof(ClearNotificationService)), mConnection, Context.BIND_AUTO_CREATE);
				mIsBound = true;
				startService(notificationIntent);
			}

		}

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			private readonly MultipartyActivity outerInstance;

			public ServiceConnectionAnonymousInnerClassHelper(MultipartyActivity outerInstance)
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

		public virtual void onClickSend(View v)
		{
			if (mMessageEditText.Text.ToString().CompareTo("") == 0)
			{
				Log.i(LOGTAG, "Cannot Send - Empty String Message");
			}
			else
			{
				Log.i(LOGTAG, "Sending a chat message");
				mSession.sendChatMessage(mMessageEditText.Text.ToString());
				mMessageEditText.Text = "";
			}
		}

	}

}