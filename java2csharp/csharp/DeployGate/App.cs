namespace com.deploygate.sample
{

	using Application = android.app.Application;

	using DeployGate = com.deploygate.sdk.DeployGate;

	public class App : Application
	{
		public override void onCreate()
		{
			base.onCreate();

			// Install DeployGate to this application instance.
			DeployGate.install(this);

			// [[ Since DeployGate SDK r2 ]]
			//
			// If you want to prevent the app distributed by someone else,
			// specify your username explicitly here, like:
			//
			// DeployGate.install(this, "YOURUSERNAME");
		}
	}

}