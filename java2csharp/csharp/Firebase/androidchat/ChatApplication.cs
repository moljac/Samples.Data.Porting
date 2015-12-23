namespace com.firebase.androidchat
{

	using Firebase = com.firebase.client.Firebase;

	/// <summary>
	/// @author Jenny Tong (mimming)
	/// @since 12/5/14
	/// 
	/// Initialize Firebase with the application context. This must happen before the client is used.
	/// </summary>
	public class ChatApplication : android.app.Application
	{
		public override void onCreate()
		{
			base.onCreate();
			Firebase.AndroidContext = this;
		}
	}

}