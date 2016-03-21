namespace com.samsung.audiosuite.sapainstrumentsample
{

	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using SharedPreferences = android.content.SharedPreferences;
	using Bundle = android.os.Bundle;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using View = android.view.View;
	using ImageButton = android.widget.ImageButton;

	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using FloatingController = com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

	/// <summary>
	/// This activity is used when application is started from other audio apps using SapaAppService.
	/// </summary>
	public class MainActivity : Activity
	{

		internal const string FLOATING_ALIGNMENT_STATE_TAG = "floating_alignment_state_tag";
		// Id of action for start playing.
		internal const string ACTION_PLAY = "sapainstrumentsample.play";
		// Id of action for stop playing.
		internal const string ACTION_STOP = "sapainstrumentsample.stop";

		private const string TAG = "audiosuite:sapainstrumentsample:j:MainActivity";

		// Reference to local service.
		internal WeakReference<MainService> mService;

		private ImageButton mPlayButton;
		private ImageButton mStopButton;

		// Floating controller.
		internal FloatingController mFloatingController;

		// Reference to currently visible SapaAppInfo.
		private SapaAppInfo mVisibleAppInfo;

		public override void onNewIntent(Intent intent)
		{
			Log.d(TAG, "onNewIntent");
			this.readIntent(intent);
			changeTitle();
		}

		/// <summary>
		/// This method is responsible for updating visible SapaAppInfo and sets state of view
		/// accordingly.
		/// </summary>
		/// <param name="intent">
		///            Intent received when activity is shown. </param>
		private void readIntent(Intent intent)
		{
			SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
			if (info != null)
			{
				this.mVisibleAppInfo = info;
				if (this.mVisibleAppInfo.getActionInfo(ACTION_PLAY) != null)
				{
					if (this.mVisibleAppInfo.getActionInfo(ACTION_PLAY).Visible)
					{
						this.changeButtonsOnStop();
					}
					else
					{
						this.changeButtonsOnPlay();
					}
				}
			}
		}

		private void changeTitle()
		{
			if (mVisibleAppInfo != null && mVisibleAppInfo.App != null)
			{
				this.Title = getString(R.@string.app_name) + " for " + mVisibleAppInfo.App.InstanceId;
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.d(TAG, "onCreate");
			ContentView = R.layout.activity_main;
			base.onCreate(savedInstanceState);
			this.mService = null;



			// binding to local service.
			this.bindService(new Intent(this, typeof(MainService)), this.mConnection, 0);

			// Views are being set from the layout.
			mFloatingController = (FloatingController) findViewById(R.id.jam_control);
			//Load position form shared preference
			SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
			int barAlignment = preferences.getInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.BarAlignment);
			/// <summary>
			///This method is used to set floating controller position
			/// To set position you must used one of below value:
			/// HORIZONTAL POSITION
			/// 5 - BOTTOM RIGHT
			/// 6 - BOTTOM LEFT
			/// 4 - TOP RIGHT
			/// 7 - TOP LEFT
			/// 
			/// VERTICAL POSITION
			/// 15 - BOTTOM RIGHT
			/// 16 - BOTTOM LEFT
			/// 14 - TOP RIGHT
			/// 17 - TOP LEFT
			/// </summary>
			mFloatingController.loadBarState(barAlignment);
			this.mPlayButton = (ImageButton) findViewById(R.id.playButton);
			this.mStopButton = (ImageButton) findViewById(R.id.stopButton);

			// Received intent is being read.
			Intent intent = Intent;
			if (intent != null)
			{
				this.readIntent(intent);
				changeTitle();
			}

			// Controls actions are being set.
			// Only one button is visible at a time, so visibility needs to be
			// changed.
			this.mPlayButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			this.mStopButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
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
				outerInstance.mPlayButton.Visibility = View.GONE;
				outerInstance.mStopButton.Visibility = View.VISIBLE;
				if (outerInstance.mService != null && outerInstance.mService.get() != null && outerInstance.mVisibleAppInfo != null)
				{
					try
					{
						outerInstance.mService.get().play(outerInstance.mVisibleAppInfo.App);
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
				outerInstance.mPlayButton.Visibility = View.VISIBLE;
				outerInstance.mStopButton.Visibility = View.GONE;
				if (outerInstance.mService != null && outerInstance.mService.get() != null && outerInstance.mVisibleAppInfo != null)
				{
					try
					{
						outerInstance.mService.get().stop(outerInstance.mVisibleAppInfo.App);
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
			}
		}

		protected internal override void onDestroy()
		{
			Log.d(TAG, "onDestroy");
			// Local MainService is being unbound.
			unbindService(mConnection);
			SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
			preferences.edit().putInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.BarAlignment).apply();
			base.onDestroy();
		}

		// This instance of ServiceConnection is responsible for connection to local
		// MainService.
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

				outerInstance.setButtonsState();
			}

			public override void onServiceDisconnected(ComponentName name)
			{
				Log.d(TAG, "onServiceDisconnected");
				outerInstance.mService = null;
			}

		}

		// When play is called from FloatingController we need to change UI
		// accordingly.
		internal virtual void changeButtonsOnPlay()
		{
			this.runOnUiThread(() =>
			{

				mPlayButton.Visibility = View.GONE;
				mStopButton.Visibility = View.VISIBLE;
			});
		}

		// When stop is called from FloatingController we need to change UI
		// accordingly.
		internal virtual void changeButtonsOnStop()
		{
			this.runOnUiThread(() =>
			{

				mPlayButton.Visibility = View.VISIBLE;
				mStopButton.Visibility = View.GONE;
			});
		}

		/// <summary>
		/// This method is responsible for setting buttons state according to current state.
		/// </summary>
		private void setButtonsState()
		{
			if (this.mService != null && mService.get() != null)
			{
				try
				{
					if (this.mService.get().Playing)
					{
						mPlayButton.Visibility = View.GONE;
						mStopButton.Visibility = View.VISIBLE;
					}
					else
					{
						mPlayButton.Visibility = View.VISIBLE;
						mStopButton.Visibility = View.GONE;
					}
				}
				catch (System.NullReferenceException)
				{
					;
				}
			}
		}

		/// <summary>
		/// This method returns information whether given in parameter sapaApp instance is currently
		/// visible for user.
		/// </summary>
		/// <param name="sapaApp">
		///            True when given in parameter sapaApp instance is currently visible for user, false
		///            otherwise.
		/// @return </param>
		internal virtual bool isVisibleInstance(SapaApp sapaApp)
		{
			if (this.mVisibleAppInfo != null && this.mVisibleAppInfo.App != null)
			{
				return this.mVisibleAppInfo.App.InstanceId.Equals(sapaApp.InstanceId);
			}
			return false;
		}
	}

}