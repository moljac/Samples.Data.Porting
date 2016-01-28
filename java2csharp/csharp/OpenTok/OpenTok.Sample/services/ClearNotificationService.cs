namespace com.opentok.android.demo.services
{

	using NotificationManager = android.app.NotificationManager;
	using Service = android.app.Service;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;

	public class ClearNotificationService : Service
	{
		private bool InstanceFieldsInitialized = false;

		public ClearNotificationService()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mBinder = new ClearBinder(this, this);
		}

		public const string MY_SERVICE = "ClearNotificationService";

		public class ClearBinder : Binder
		{
			private readonly ClearNotificationService outerInstance;

			public readonly Service service;

			public ClearBinder(ClearNotificationService outerInstance, Service service)
			{
				this.outerInstance = outerInstance;
				this.service = service;
			}
		}

		public static int NOTIFICATION_ID = 1;
		private NotificationManager mNotificationManager;
		private IBinder mBinder;

		public override IBinder onBind(Intent intent)
		{
			return mBinder;
		}

		public override int onStartCommand(Intent intent, int flags, int startId)
		{
			return Service.START_STICKY;
		}

		public override void onCreate()
		{
			mNotificationManager = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
			mNotificationManager.cancel(NOTIFICATION_ID);
		}

		public override void onDestroy()
		{
			// Cancel the persistent notification.
			mNotificationManager.cancel(NOTIFICATION_ID);
		}

	}
}