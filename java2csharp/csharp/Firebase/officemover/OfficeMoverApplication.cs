namespace com.firebase.officemover
{

	using Firebase = com.firebase.client.Firebase;

	/// <summary>
	/// @author Jenny Tong (mimming)
	/// @since 12/8/14
	/// 
	/// Initialize Firebase with the application context. This must happen before the client is used.
	/// </summary>
	public class OfficeMoverApplication : android.app.Application
	{
		public override void onCreate()
		{
			base.onCreate();
			Firebase.AndroidContext = this;
		}
	}

}