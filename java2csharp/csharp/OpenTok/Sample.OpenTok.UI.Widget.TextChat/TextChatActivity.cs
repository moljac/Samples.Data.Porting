namespace com.opentok.android.ui.textchat.sample
{

	using FragmentTransaction = android.app.FragmentTransaction;
	using NotificationManager = android.app.NotificationManager;
	using ServiceConnection = android.content.ServiceConnection;
	using Bundle = android.os.Bundle;
	using FragmentActivity = android.support.v4.app.FragmentActivity;
	using NotificationCompat = android.support.v4.app.NotificationCompat;
	using Log = android.util.Log;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ProgressBar = android.widget.ProgressBar;
	using RelativeLayout = android.widget.RelativeLayout;

	using ChatMessage = com.opentok.android.ui.textchat.widget.ChatMessage;
	using TextChatFragment = com.opentok.android.ui.textchat.widget.TextChatFragment;

	public class TextChatActivity : FragmentActivity, Session.SignalListener, Session.SessionListener, TextChatFragment.TextChatListener
	{

		private const string LOGTAG = "demo-text-chat";
		private const string SIGNAL_TYPE = "TextChat";

		// Replace with a generated Session ID
		public const string SESSION_ID = "";
		// Replace with a generated token (from the dashboard or using an OpenTok server SDK)
		public const string TOKEN = "";
		// Replace with your OpenTok API key
		public const string API_KEY = "";

		private Session mSession;

		private ProgressBar mLoadingBar;

		private bool resumeHasRun = false;

		private bool mIsBound = false;
		private NotificationCompat.Builder mNotifyBuilder;
		private NotificationManager mNotificationManager;
		private ServiceConnection mConnection;

		private RelativeLayout mTextChatViewContainer;
		private TextChatFragment mTextChatFragment;

		private FragmentTransaction mFragmentTransaction;

		private bool msgError = false;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.layout_textchat_activity;

			mLoadingBar = (ProgressBar) findViewById(R.id.load_spinner);
			mLoadingBar.Visibility = View.VISIBLE;

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
		}

		public override void onResume()
		{
			base.onResume();
			if (mSession != null)
			{
				mSession.onResume();
			}
		}

		public override void onStop()
		{
			base.onStop();
			if (Finishing && mSession != null)
			{
				mSession.disconnect();
			}
		}

		public override void onDestroy()
		{
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

		private void sessionConnect()
		{
			if (mSession == null)
			{
				mSession = new Session(TextChatActivity.this, API_KEY, SESSION_ID);
				mSession.SignalListener = this;
				mSession.SessionListener = this;
				mSession.connect(TOKEN);
			}
		}

		// Initialize a TextChatFragment instance and add it to the UI
		private void loadTextChatFragment()
		{
			int containerId = R.id.fragment_textchat_container;
			mFragmentTransaction = FragmentManager.beginTransaction();
			mTextChatFragment = (TextChatFragment)this.FragmentManager.findFragmentByTag("TextChatFragment");

			if (mTextChatFragment == null)
			{
				mTextChatFragment = new TextChatFragment();
				mTextChatFragment.MaxTextLength = 1050;
				mTextChatFragment.setTextChatListener(this);
				mTextChatFragment.setSenderInfo(mSession.Connection.ConnectionId, mSession.Connection.Data);

				mFragmentTransaction.add(containerId, mTextChatFragment, "TextChatFragment").commit();
			}
		}

		public virtual bool onMessageReadyToSend(ChatMessage msg)
		{
			Log.d(LOGTAG, "TextChat listener: onMessageReadyToSend: " + msg.Text);

			if (mSession != null)
			{
				mSession.sendSignal(SIGNAL_TYPE, msg.Text);
			}
			return msgError;
		}

		public override void onSignalReceived(Session session, string type, string data, Connection connection)
		{
			Log.d(LOGTAG, "onSignalReceived. Type: " + type + " data: " + data);
			ChatMessage msg = null;
			if (!connection.ConnectionId.Equals(mSession.Connection.ConnectionId))
			{
				// The signal was sent from another participant. The sender ID is set to the sender's
				// connection ID. The sender alias is the value added as connection data when you
				// created the user's token.
				msg = new ChatMessage(connection.ConnectionId, connection.Data, data);
				// Add the new ChatMessage to the text-chat component
				mTextChatFragment.addMessage(msg);
			}
		}


		public override void onConnected(Session session)
		{
			Log.d(LOGTAG, "The session is connected.");

			mLoadingBar.Visibility = View.GONE;
			//loading text-chat ui component
			loadTextChatFragment();
		}

		public override void onDisconnected(Session session)
		{
			Log.d(LOGTAG, "The session disconnected.");
		}

		public override void onError(Session session, OpentokError opentokError)
		{
			Log.d(LOGTAG, "Session error. OpenTokError: " + opentokError.ErrorCode + " - " + opentokError.Message);
			OpentokError.ErrorCode errorCode = opentokError.ErrorCode;
			msgError = true;
		}

		public override void onStreamReceived(Session session, Stream stream)
		{
		}

		public override void onStreamDropped(Session session, Stream stream)
		{
		}

	}

}