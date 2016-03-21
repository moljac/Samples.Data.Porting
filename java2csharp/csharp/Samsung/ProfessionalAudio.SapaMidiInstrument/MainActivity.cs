namespace com.samsung.audiosuite.sapamidisample
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
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;

	using FloatingController = com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

	public class MainActivity : Activity, View.OnTouchListener
	{

		internal const string FLOATING_ALIGNMENT_STATE_TAG = "floating_alignment_state_tag";
		private const string TAG = "audiosuite:sapamidisample:j:MainActivity";

		//Reference to local service.
		internal WeakReference<MainService> mService;

		//Floating controller.
		internal FloatingController mFloatingController;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			Log.d(TAG, "onCreate");
			ContentView = R.layout.activity_main;
			base.onCreate(savedInstanceState);
			mService = null;
			//binding to local service.
			bindService(new Intent(this, typeof(MainService)), mConnection, 0);

			//Views are being set from the layout.

			mFloatingController = (FloatingController)findViewById(R.id.jam_control);
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
			ViewGroup vg = (ViewGroup) findViewById(R.id.layout_keys);

			int size = vg.ChildCount;
			for (int i = 0; i < size; ++i)
			{
				View v = vg.getChildAt(i);
				if (v is KeyButton)
				{
					v.OnTouchListener = this;
				}
			}

		}

		protected internal override void onResume()
		{
			Log.d(TAG, "onResume");
			base.onResume();
		}

		protected internal override void onDestroy()
		{
			Log.d(TAG, "onDestroy");
			unbindService(mConnection);
			SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
			preferences.edit().putInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.BarAlignment).apply();
			base.onDestroy();
		}

		public override bool onTouch(View v, MotionEvent ev)
		{

			KeyButton k = (KeyButton) v;

			try
			{
			switch (ev.Action)
			{
				case MotionEvent.ACTION_DOWN:
					if (mService != null && mService.get() != null)
					{
					MainActivity.this.mService.get().sendNoteOn(k.Note, k.Velocity);
					}
					break;
				case MotionEvent.ACTION_UP:
					if (mService != null && mService.get() != null)
					{
					MainActivity.this.mService.get().sendNoteOff(k.Note, k.Velocity);
					}
					break;
			}
			}
			catch (System.NullReferenceException)
			{
				;
			}

			return false;
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
				//Connection bridge is set to the FloatingController.
				//You can have only one AudioAppConnectionBridge in the application
				//so you need to pass it from service to FloatingController.

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

				if (outerInstance.mService != null && outerInstance.mService.get() != null)
				{
					try
					{
						Log.d(TAG, "Connection bridge set " + outerInstance.mService.get().SapaAppService);
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
	}

}