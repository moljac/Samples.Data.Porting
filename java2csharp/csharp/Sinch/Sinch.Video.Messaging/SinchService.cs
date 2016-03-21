namespace com.sinch.android.rtc.sample.messaging
{

	using Service = android.app.Service;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;

	using com.sinch.android.rtc;
	using MessageClientListener = com.sinch.android.rtc.messaging.MessageClientListener;
	using WritableMessage = com.sinch.android.rtc.messaging.WritableMessage;

	public class SinchService : Service
	{
		private bool InstanceFieldsInitialized = false;

		public SinchService()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mServiceInterface = new SinchServiceInterface(this);
		}


		private const string APP_KEY = "enter-application-key";
		private const string APP_SECRET = "enter-application-secret";
		private const string ENVIRONMENT = "sandbox.sinch.com";

		private static readonly string TAG = typeof(SinchService).Name;

		private SinchServiceInterface mServiceInterface;

		private SinchClient mSinchClient = null;
		private StartFailedListener mListener;

		public class SinchServiceInterface : Binder
		{
			private readonly SinchService outerInstance;

			public SinchServiceInterface(SinchService outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool Started
			{
				get
				{
					return outerInstance.Started;
				}
			}

			public virtual void startClient(string userName)
			{
				outerInstance.start(userName);
			}

			public virtual void stopClient()
			{
				outerInstance.stop();
			}

			public virtual StartFailedListener StartListener
			{
				set
				{
					outerInstance.mListener = value;
				}
			}

			public virtual void sendMessage(string recipientUserId, string textBody)
			{
				outerInstance.sendMessage(recipientUserId, textBody);
			}

			public virtual void addMessageClientListener(MessageClientListener listener)
			{
				outerInstance.addMessageClientListener(listener);
			}

			public virtual void removeMessageClientListener(MessageClientListener listener)
			{
				outerInstance.removeMessageClientListener(listener);
			}
		}

		public override void onCreate()
		{
			base.onCreate();
		}

		public override void onDestroy()
		{
			if (mSinchClient != null && mSinchClient.Started)
			{
				mSinchClient.terminate();
			}
			base.onDestroy();
		}

		public override IBinder onBind(Intent intent)
		{
			return mServiceInterface;
		}

		private bool Started
		{
			get
			{
				return (mSinchClient != null && mSinchClient.Started);
			}
		}

		public virtual void sendMessage(string recipientUserId, string textBody)
		{
			if (Started)
			{
				WritableMessage message = new WritableMessage(recipientUserId, textBody);
				mSinchClient.MessageClient.send(message);
			}
		}

		public virtual void addMessageClientListener(MessageClientListener listener)
		{
			if (mSinchClient != null)
			{
				mSinchClient.MessageClient.addMessageClientListener(listener);
			}
		}

		public virtual void removeMessageClientListener(MessageClientListener listener)
		{
			if (mSinchClient != null)
			{
				mSinchClient.MessageClient.removeMessageClientListener(listener);
			}
		}

		private void start(string userName)
		{
			if (mSinchClient == null)
			{
				mSinchClient = Sinch.SinchClientBuilder.context(ApplicationContext).userId(userName).applicationKey(APP_KEY).applicationSecret(APP_SECRET).environmentHost(ENVIRONMENT).build();

				mSinchClient.SupportMessaging = true;
				mSinchClient.startListeningOnActiveConnection();

				mSinchClient.addSinchClientListener(new MySinchClientListener(this));
				mSinchClient.start();
			}
		}

		private void stop()
		{
			if (mSinchClient != null)
			{
				mSinchClient.terminate();
				mSinchClient = null;
			}
		}

		public interface StartFailedListener
		{

			void onStartFailed(SinchError error);

			void onStarted();
		}

		private class MySinchClientListener : SinchClientListener
		{
			private readonly SinchService outerInstance;

			public MySinchClientListener(SinchService outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClientFailed(SinchClient client, SinchError error)
			{
				if (outerInstance.mListener != null)
				{
					outerInstance.mListener.onStartFailed(error);
				}
				outerInstance.mSinchClient.terminate();
				outerInstance.mSinchClient = null;
			}

			public override void onClientStarted(SinchClient client)
			{
				Log.d(TAG, "SinchClient started");
				if (outerInstance.mListener != null)
				{
					outerInstance.mListener.onStarted();
				}
			}

			public override void onClientStopped(SinchClient client)
			{
				Log.d(TAG, "SinchClient stopped");
			}

			public override void onLogMessage(int level, string area, string message)
			{
				switch (level)
				{
					case Log.DEBUG:
						Log.d(area, message);
						break;
					case Log.ERROR:
						Log.e(area, message);
						break;
					case Log.INFO:
						Log.i(area, message);
						break;
					case Log.VERBOSE:
						Log.v(area, message);
						break;
					case Log.WARN:
						Log.w(area, message);
						break;
				}
			}

			public override void onRegistrationCredentialsRequired(SinchClient client, ClientRegistration clientRegistration)
			{
			}
		}
	}

}