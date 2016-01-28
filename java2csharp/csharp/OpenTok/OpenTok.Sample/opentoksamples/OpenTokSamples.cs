namespace com.opentok.android.demo.opentoksamples
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using WindowManager = android.view.WindowManager;
	using AdapterView = android.widget.AdapterView;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ListView = android.widget.ListView;

	/// <summary>
	/// Main demo app for getting started with the OpenTok Android SDK. It contains:
	/// - a basic hello-world activity - a basic hello-world activity with control
	/// bar with action buttons to switch camera, audio mute and end call. - a basic
	/// hello-world activity with a customer video capturer out of SDK.
	/// </summary>
	public class OpenTokSamples : Activity
	{

		private const string LOGTAG = "demo-opentok-sdk";

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.main_activity;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ListView listActivities = (android.widget.ListView) findViewById(R.id.listview);
			ListView listActivities = (ListView) findViewById(R.id.listview);
			string[] activityNames = new string[] {getString(R.@string.helloworld), getString(R.@string.helloworldui), getString(R.@string.helloworldcapturer), getString(R.@string.helloworldrenderer), getString(R.@string.helloworldsubclassing), getString(R.@string.voinceonly), getString(R.@string.audiodevice), getString(R.@string.helloworldemulator), getString(R.@string.screensharing), getString(R.@string.defaultcameracapturer), getString(R.@string.screenshot)};

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ArrayAdapter<String> adapter = new android.widget.ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, activityNames);
			ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, activityNames);
			listActivities.Adapter = adapter;

			listActivities.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly OpenTokSamples outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(OpenTokSamples outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> a, View v, int position, long id)
			{
				// these positions are hard-coded to some example activities,
				// they match
				// the array contents of activityNames above.
				if (0 == position)
				{
					outerInstance.startHelloWorld();
				}
				else if (1 == position)
				{
					outerInstance.startHelloWorldUI();
				}
				else if (2 == position)
				{
					outerInstance.startHelloWorldVideoCapturer();
				}
				else if (3 == position)
				{
					outerInstance.startHelloWorldVideoRenderer();
				}
				else if (4 == position)
				{
					outerInstance.startHelloWorldSubclassing();
				}
				else if (5 == position)
				{
					outerInstance.startVoiceOnly();
				}
				else if (6 == position)
				{
					outerInstance.startAudioDevice();
				}
				else if (7 == position)
				{
					outerInstance.startHelloWorldEmulator();
				}
				else if (8 == position)
				{
					outerInstance.startScreensharing();
				}
				else if (9 == position)
				{
					outerInstance.startDefaultCameraCapturer();
				}
				else if (10 == position)
				{
					outerInstance.startScreenshot();
				}
				else
				{
					Log.wtf(LOGTAG, "unknown item clicked?");
				}
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			return true;
		}


		public override void onPause()
		{
			base.onPause();
		}

		public override void onResume()
		{
			base.onResume();
		}

		/// <summary>
		/// Starts the Hello-World demo app. See OpenTokHelloWorld.java
		/// </summary>
		public virtual void startHelloWorld()
		{

			Log.i(LOGTAG, "starting hello-world app");

			Intent intent = new Intent(OpenTokSamples.this, typeof(HelloWorldActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);
		}

		/// <summary>
		/// Starts the Hello-World app with UI. See OpenTokUI.java
		/// </summary>
		public virtual void startHelloWorldUI()
		{

			Log.i(LOGTAG, "starting hello-world app with UI");

			Intent intent = new Intent(OpenTokSamples.this, typeof(UIActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);
		}

		/// <summary>
		/// Starts the Hello-World app using a custom video capturer. See
		/// VideoCapturerActivity.java
		/// </summary>
		public virtual void startHelloWorldVideoCapturer()
		{

			Log.i(LOGTAG, "starting hello-world app using a customer video capturer");

			Intent intent = new Intent(OpenTokSamples.this, typeof(VideoCapturerActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app using a custom video renderer. See
		/// VideoRendererActivity.java
		/// </summary>
		public virtual void startHelloWorldVideoRenderer()
		{

			Log.i(LOGTAG, "starting hello-world app using a customer video capturer");

			Intent intent = new Intent(OpenTokSamples.this, typeof(VideoRendererActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app using subclassing. See
		/// MultipartyActivity.java
		/// </summary>
		public virtual void startHelloWorldSubclassing()
		{

			Log.i(LOGTAG, "starting hello-world app using subclassing");

			Intent intent = new Intent(OpenTokSamples.this, typeof(MultipartyActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the voice only Hello-World app. See
		/// VoiceOnlyActivity.java
		/// </summary>
		public virtual void startVoiceOnly()
		{

			Log.i(LOGTAG, "starting hello-world app using voice only");

			Intent intent = new Intent(OpenTokSamples.this, typeof(VoiceOnlyActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app using a custom audio device. See
		/// AudioDeviceActivity.java
		/// </summary>
		public virtual void startAudioDevice()
		{

			Log.i(LOGTAG, "starting hello-world app using a custom audio device");

			Intent intent = new Intent(OpenTokSamples.this, typeof(AudioDeviceActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app in the emulator. See
		/// EmulatorActivity.java
		/// </summary>
		public virtual void startHelloWorldEmulator()
		{

			Log.i(LOGTAG, "starting hello-world app for Android emulator");

			Intent intent = new Intent(OpenTokSamples.this, typeof(EmulatorActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app to share a website. See
		/// ScreenSharingActivity.java
		/// </summary>
		public virtual void startScreensharing()
		{

			Log.i(LOGTAG, "starting hello-world app for screensharing");

			Intent intent = new Intent(OpenTokSamples.this, typeof(ScreenSharingActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app using the default video capturer feature. See
		/// DefaultCameraCapturerActivity.java
		/// </summary>
		public virtual void startDefaultCameraCapturer()
		{

			Log.i(LOGTAG, "starting hello-world app for default video capturer setting a preferred resolution and frame rate");

			Intent intent = new Intent(OpenTokSamples.this, typeof(DefaultCameraCapturerActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}

		/// <summary>
		/// Starts the Hello-World app using a custom video renderer with the screenshot option. See
		/// ScreenshotActivity.java
		/// </summary>
		public virtual void startScreenshot()
		{

			Log.i(LOGTAG, "starting hello-world app with screenshot option");

			Intent intent = new Intent(OpenTokSamples.this, typeof(ScreenshotActivity));
			intent.Flags = Intent.FLAG_ACTIVITY_NEW_TASK | Intent.FLAG_ACTIVITY_CLEAR_TOP;
			startActivity(intent);

		}
	}
}