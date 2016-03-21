namespace com.sinch.android.rtc.sample.messaging
{

	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Bundle = android.os.Bundle;
	using IBinder = android.os.IBinder;

	public abstract class BaseActivity : Activity, ServiceConnection
	{

		private SinchService.SinchServiceInterface mSinchServiceInterface;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ApplicationContext.bindService(new Intent(this, typeof(SinchService)), this, BIND_AUTO_CREATE);
		}

		public override void onServiceConnected(ComponentName componentName, IBinder iBinder)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			if (typeof(SinchService).FullName.Equals(componentName.ClassName))
			{
				mSinchServiceInterface = (SinchService.SinchServiceInterface) iBinder;
				onServiceConnected();
			}
		}

		public override void onServiceDisconnected(ComponentName componentName)
		{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
			if (typeof(SinchService).FullName.Equals(componentName.ClassName))
			{
				mSinchServiceInterface = null;
				onServiceDisconnected();
			}
		}

		protected internal virtual void onServiceConnected()
		{
			// for subclasses
		}

		protected internal virtual void onServiceDisconnected()
		{
			// for subclasses
		}

		protected internal virtual SinchService.SinchServiceInterface SinchServiceInterface
		{
			get
			{
				return mSinchServiceInterface;
			}
		}

	}

}