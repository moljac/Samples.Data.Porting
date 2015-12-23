namespace com.bixolon.printersample
{

	using Activity = android.app.Activity;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class MsrActivity : Activity, View.OnClickListener
	{
		private EditText mTrack1EditText;
		private EditText mTrack2EditText;
		private EditText mTrack3EditText;

		private sbyte[] mTrack1Data;
		private sbyte[] mTrack2Data;
		private sbyte[] mTrack3Data;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_msr;

			Intent intent = Intent;
			int extra = intent.getIntExtra(MainActivity.EXTRA_NAME_MSR_MODE, 0);
			EditText editText = (EditText) findViewById(R.id.editText1);

			switch (extra)
			{
			case BixolonPrinter.MSR_MODE_TRACK123_COMMAND:
				editText.Text = "Track 1/2/3 read mode command";
				break;
			case BixolonPrinter.MSR_MODE_TRACK1_AUTO:
				editText.Text = "Track 1 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_TRACK2_AUTO:
				editText.Text = "Track 2 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_TRACK3_AUTO:
				editText.Text = "Track 3 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_TRACK12_AUTO:
				editText.Text = "Track 1/2 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_TRACK23_AUTO:
				editText.Text = "Track 2/3 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_TRACK123_AUTO:
				editText.Text = "Track 1/2/3 read mode auto trigger";
				break;
			case BixolonPrinter.MSR_MODE_NOT_USED:
			default:
				editText.Text = "MSR not used";
				break;
			}

			Button button1 = (Button) findViewById(R.id.button1);
			Button button2 = (Button) findViewById(R.id.button2);

			if (extra == BixolonPrinter.MSR_MODE_TRACK123_COMMAND)
			{
				button1.OnClickListener = this;
				button2.OnClickListener = this;
			}
			else
			{
				button1.Enabled = false;
				button2.Enabled = false;
			}

			mTrack1EditText = (EditText) findViewById(R.id.editText2);
			mTrack2EditText = (EditText) findViewById(R.id.editText3);
			mTrack3EditText = (EditText) findViewById(R.id.editText4);

			IntentFilter filter = new IntentFilter();
			filter.addAction(MainActivity.ACTION_GET_MSR_TRACK_DATA);
			registerReceiver(mReceiver, filter);
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			unregisterReceiver(mReceiver);
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				MainActivity.mBixolonPrinter.setMsrReaderMode();
				break;

			case R.id.button2:
				MainActivity.mBixolonPrinter.cancelMsrReaderMode();
				break;
			}
		}

		internal BroadcastReceiver mReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}


			public override void onReceive(Context context, Intent intent)
			{
				if (intent.Action.Equals(MainActivity.ACTION_GET_MSR_TRACK_DATA))
				{
					Bundle bundle = intent.getBundleExtra(MainActivity.EXTRA_NAME_MSR_TRACK_DATA);
					outerInstance.mTrack1EditText.Text = "";
					outerInstance.mTrack2EditText.Text = "";
					outerInstance.mTrack3EditText.Text = "";

					outerInstance.mTrack1Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK1);
					if (outerInstance.mTrack1Data != null)
					{
						(new Handler()).postDelayed(() =>
						{

							outerInstance.mTrack1EditText.Text = new string(outerInstance.mTrack1Data);
						}, 100);
					}

					outerInstance.mTrack2Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK2);
					if (outerInstance.mTrack2Data != null)
					{
						(new Handler()).postDelayed(() =>
						{

							outerInstance.mTrack2EditText.Text = new string(outerInstance.mTrack2Data);
						}, 100);
					}

					outerInstance.mTrack3Data = bundle.getByteArray(BixolonPrinter.KEY_STRING_MSR_TRACK3);
					if (outerInstance.mTrack3Data != null)
					{
						(new Handler()).postDelayed(() =>
						{

							outerInstance.mTrack3EditText.Text = new string(outerInstance.mTrack3Data);
						}, 100);
					}
				}
			}
		}
	}

}