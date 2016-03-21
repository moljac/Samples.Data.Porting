namespace com.sinch.android.rtc.sample.pstn
{

	using Call = com.sinch.android.rtc.calling.Call;
	using CallClient = com.sinch.android.rtc.calling.CallClient;
	using CallClientListener = com.sinch.android.rtc.calling.CallClientListener;

	using Service = android.app.Service;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;

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
			mSinchServiceInterface = new SinchServiceInterface(this);
		}


		private const string APP_KEY = "enter-application-key";
		private const string APP_SECRET = "enter-application-secret";
		private const string ENVIRONMENT = "sandbox.sinch.com";

		public const string CALL_ID = "CALL_ID";
		internal static readonly string TAG = typeof(SinchService).Name;

		private SinchServiceInterface mSinchServiceInterface;
		private SinchClient mSinchClient;
		private string mUserId;

		private StartFailedListener mListener;

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

		private void start(string userName)
		{
			if (mSinchClient == null)
			{
				mUserId = userName;
				mSinchClient = Sinch.SinchClientBuilder.context(ApplicationContext).userId(userName).applicationKey(APP_KEY).applicationSecret(APP_SECRET).environmentHost(ENVIRONMENT).build();

				mSinchClient.SupportCalling = true;

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

		private bool Started
		{
			get
			{
				return (mSinchClient != null && mSinchClient.Started);
			}
		}

		public override IBinder onBind(Intent intent)
		{
			return mSinchServiceInterface;
		}

		public class SinchServiceInterface : Binder
		{
			private readonly SinchService outerInstance;

			public SinchServiceInterface(SinchService outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual Call callPhoneNumber(string phoneNumber)
			{
				return outerInstance.mSinchClient.CallClient.callPhoneNumber(phoneNumber);
			}

			public virtual Call callUser(string userId)
			{
				return outerInstance.mSinchClient.CallClient.callUser(userId);
			}

			public virtual string UserName
			{
				get
				{
					return outerInstance.mUserId;
				}
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

			public virtual Call getCall(string callId)
			{
				return outerInstance.mSinchClient.CallClient.getCall(callId);
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