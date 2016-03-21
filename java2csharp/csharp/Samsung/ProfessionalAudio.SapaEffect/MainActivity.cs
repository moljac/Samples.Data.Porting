namespace com.samsung.audiosuite.sapaeffectsample
{

	using ComponentName = android.content.ComponentName;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Bundle = android.os.Bundle;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using View = android.view.View;

	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using FloatingController = com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

	public class MainActivity : EffectSampleActivity
	{

		internal const string VOLUME_DOWN = "sapaeffectsample.volumedown";
		internal const string VOLUME_UP = "sapaeffectsample.volumeup";

		private const string TAG = "audiosuite:sapaeffectsample:j:MainActivity";

		// Reference to local service.
		internal WeakReference<MainService> mService;

		private SapaAppInfo mVisibleAppInfo;

		// Floating controller.
		internal FloatingController mFloatingController;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.d(TAG, "onCreate");
			base.onCreate(savedInstanceState);

			this.mFloatingController = (FloatingController) findViewById(R.id.jam_control);

			this.mService = null;
			// binding to local service.
			this.bindService(new Intent(this, typeof(MainService)), this.mConnection, 0);

			Intent intent = Intent;
			if (intent != null)
			{
				this.readIntent(intent);
			}

			this.mVolDownButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			this.mVolUpButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				if (outerInstance.mService != null && outerInstance.mService.get() != null && outerInstance.mVisibleAppInfo != null)
				{
					try
					{
						outerInstance.mService.get().decreaseVolume(outerInstance.mVisibleAppInfo.App);
						if (outerInstance.mService.get().isMinVolume(outerInstance.mVisibleAppInfo.App))
						{
							outerInstance.setMinVolumeStateOnButtons();
						}
						else
						{
							outerInstance.setBetweenVolumeStateOnButtons();
						}
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				if (outerInstance.mService != null && outerInstance.mService.get() != null && outerInstance.mVisibleAppInfo != null)
				{
					try
					{
						outerInstance.mService.get().increaseVolume(outerInstance.mVisibleAppInfo.App);
						if (outerInstance.mService.get().isMaxVolume(outerInstance.mVisibleAppInfo.App))
						{
							outerInstance.setMaxVolumeStateOnButtons();
						}
						else
						{
							outerInstance.setBetweenVolumeStateOnButtons();
						}
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			}
		}

		protected internal override void onResume()
		{
			Log.d(TAG, "onResume");
			base.onResume();
			if (mService != null && mService.get() != null)
			{
				try
				{
					updateVolumeTextView(mService.get().getVolumeText(mVisibleAppInfo));
					if (mVisibleAppInfo != null)
					{
						MainActivity.this.CurrentState = MainActivity.this.mVisibleAppInfo.App;
					}
				}
				catch (System.NullReferenceException)
				{
					;
				}
			}
			else
			{
				updateVolumeTextView("Service, not ready");
			}
		}

		public override void onNewIntent(Intent intent)
		{
			Log.d(TAG, "onNewIntent");
			this.readIntent(intent);
		}

		private void readIntent(Intent intent)
		{
			SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
			if (info != null)
			{
				this.mVisibleAppInfo = info;

			}
		}

		protected internal override void onDestroy()
		{
			Log.d(TAG, "onDestroy");
			unbindService(mConnection);
			base.onDestroy();
		}

		private ServiceConnection mConnection = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}


			public override void onServiceConnected(ComponentName className, IBinder binder)
			{
				Log.d(TAG, "onServiceConnected");
				outerInstance.mService = new WeakReference<MainService>(((MainService.LocalBinder) binder).getMainService(outerInstance));
				// Connection bridge is set to the FloatingController.
				// You can have only one AudioAppConnectionBridge in the application
				// so you need to pass it from service to FloatingController.
				if (outerInstance.mService != null && outerInstance.mService.get() != null && outerInstance.mFloatingController != null)
				{
					try
					{
						outerInstance.mFloatingController.SapaAppService = outerInstance.mService.get().SapaAppService;
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
				// Set buttons in state from the service.
				if (outerInstance.mVisibleAppInfo != null)
				{
					outerInstance.CurrentState = outerInstance.mVisibleAppInfo.App;
				}
				if (outerInstance.mService != null && outerInstance.mService.get() != null)
				{
					try
					{
						outerInstance.updateVolumeTextView(outerInstance.mService.get().getVolumeText(outerInstance.mVisibleAppInfo));
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			}

			public override void onServiceDisconnected(ComponentName name)
			{
				Log.d(TAG, "onServiceDisconnected");
				outerInstance.mService = null;
			}

		}

		private SapaApp CurrentState
		{
			set
			{
				if (this.mService != null && mService.get() != null)
				{
					try
					{
						if (this.mService.get().isMaxVolume(value))
						{
							this.setMaxVolumeStateOnButtons();
						}
						else if (this.mService.get().isMinVolume(value))
						{
							this.setMinVolumeStateOnButtons();
						}
						else
						{
							this.setBetweenVolumeStateOnButtons();
						}
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			}
		}


		internal virtual void onMaxVolume()
		{
			this.runOnUiThread(() =>
			{

				setMaxVolumeStateOnButtons();
			});
		}

		internal virtual void onMinVolume()
		{
			this.runOnUiThread(() =>
			{

				setMinVolumeStateOnButtons();
			});
		}

		internal virtual void onBetweenVolume()
		{
			this.runOnUiThread(() =>
			{

				setBetweenVolumeStateOnButtons();
			});
		}

		internal virtual SapaAppInfo VisibleApp
		{
			get
			{
				return mVisibleAppInfo;
			}
		}
	}

}