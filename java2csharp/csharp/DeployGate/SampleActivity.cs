using System;

namespace com.deploygate.sample
{

	using Activity = android.app.Activity;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;

	using DeployGate = com.deploygate.sdk.DeployGate;
	using DeployGateCallback = com.deploygate.sdk.DeployGateCallback;

	public class SampleActivity : Activity, DeployGateCallback
	{

		private const string TAG = "SampleActivity";

		private TextView mAvailableText;
		private TextView mManagedText;
		private TextView mAuthorizedText;
		private TextView mTitleText;
		private EditText mLogMessage;
		private Button mCrashButton;

		private static readonly int[] sLogButtonIds = new int[] {R.id.logError, R.id.logWarn, R.id.logDebug, R.id.logInfo, R.id.logVerbose};

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_crash_me;

			mAvailableText = (TextView) findViewById(R.id.available);
			mManagedText = (TextView) findViewById(R.id.managed);
			mAuthorizedText = (TextView) findViewById(R.id.authorized);
			mTitleText = (TextView) findViewById(R.id.title);
			mCrashButton = (Button) findViewById(R.id.button);
			mLogMessage = (EditText) findViewById(R.id.message);
		}

		protected internal override void onResume()
		{
			base.onResume();

			// register for callback, also request refreshing (second argument)
			DeployGate.registerCallback(this, true);
		}

		protected internal override void onPause()
		{
			base.onPause();

			// unregister to stop callback
			DeployGate.unregisterCallback(this);
		}


		/// <summary>
		/// Called when the log buttons clicked. Each button has ID that can be used
		/// to change log level.
		/// </summary>
		/// <param name="v"> View instance of the button </param>
		public virtual void onLogClick(View v)
		{
			string text = mLogMessage.Text.ToString();
			switch (v.Id)
			{
				case R.id.logError:
					DeployGate.logError(text);
					break;
				case R.id.logWarn:
					DeployGate.logWarn(text);
					break;
				case R.id.logDebug:
					DeployGate.logDebug(text);
					break;
				case R.id.logInfo:
					DeployGate.logInfo(text);
					break;
				case R.id.logVerbose:
					DeployGate.logVerbose(text);
					break;
				default:
					return;
			}
		}

		/// <summary>
		/// Called when the crash button clicked
		/// </summary>
		/// <param name="v"> View instance of the button </param>
		public virtual void onCrashMeClick(View v)
		{
			// let's throw!
			throw new Exception("CRASH TEST BUTTON CLICKED YAY!");
		}

		public override void onInitialized(bool isServiceAvailable)
		{
			// will be called to notify DeployGate SDK has initialized
			Log.d(TAG, "DeployGate SDK initialized, is DeployGate available? : " + isServiceAvailable);
			mAvailableText.Text = isServiceAvailable ? R.@string.available_yes : R.@string.available_no;
		}

		public override void onStatusChanged(bool isManaged, bool isAuthorized, string loginUsername, bool isStopped)
		{
			// will be called when DeployGate status has changed, including this
			// activity starting and resuming.
			mManagedText.Text = isManaged ? R.@string.managed_yes : R.@string.managed_no;
			mAuthorizedText.Text = getString(isAuthorized ? R.@string.authorized_yes : R.@string.authorized_no, loginUsername);

			mCrashButton.Enabled = isAuthorized;
			mLogMessage.Enabled = isAuthorized;
			foreach (int id in sLogButtonIds)
			{
				findViewById(id).Enabled = isAuthorized;
			}
		}

		public override void onUpdateAvailable(int serial, string versionName, int versionCode)
		{
			// will be called on app update is available.
			mTitleText.TextColor = Color.GREEN;
			mTitleText.Text = string.Format("Update is Available: #{0:D}, {1}({2:D})", serial, versionName, versionCode);
		}
	}

}