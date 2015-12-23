namespace com.firebase.samples.logindemo
{

	using Application = android.app.Application;

	using FacebookSdk = com.facebook.FacebookSdk;
	using Firebase = com.firebase.client.Firebase;

	/// <summary>
	/// Initialize Firebase with the application context. This must happen before the client is used.
	/// 
	/// @author mimming
	/// @since 12/17/14
	/// </summary>
	public class LoginDemoApplication : Application
	{
		public override void onCreate()
		{
			base.onCreate();
			Firebase.AndroidContext = this;
			FacebookSdk.sdkInitialize(this);
		}
	}

}