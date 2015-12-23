namespace com.firebase.drawing
{

	using Application = android.app.Application;

	using Firebase = com.firebase.client.Firebase;

	/// <summary>
	/// Initialize Firebase with the application context and set disk persistence (to ensure our data survives
	/// app restarts).
	/// These must happen before the Firebase client is used.
	/// </summary>
	public class DrawingApplication : Application
	{
		public override void onCreate()
		{
			base.onCreate();
			Firebase.AndroidContext = this;
			Firebase.DefaultConfig.PersistenceEnabled = true;
			//Firebase.getDefaultConfig().setLogLevel(Logger.Level.DEBUG);
			SyncedBoardManager.Context = this;
		}
	}

}