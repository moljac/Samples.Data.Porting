namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Slook = com.samsung.android.sdk.look.Slook;

	public class InformationActivity : Activity
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_information;

			Slook slook = new Slook();
			LinearLayout l = (LinearLayout) findViewById(R.id.information);

			try
			{
				slook.initialize(this);
			}
			catch (SsdkUnsupportedException e)
			{
				l.addView(createTextView(e.ToString()));
				return;
			}

			l.addView(createTextView("*VersionCode:" + slook.VersionCode));
			l.addView(createTextView("*Feature"));
			l.addView(createTextView("   - AirButton:" + slook.isFeatureEnabled(Slook.AIRBUTTON)));
			l.addView(createTextView("   - PointerIcon:" + slook.isFeatureEnabled(Slook.SPEN_HOVER_ICON)));
			l.addView(createTextView("   - SmartClip:" + slook.isFeatureEnabled(Slook.SMARTCLIP)));
			l.addView(createTextView("   - WritingBuddy:" + slook.isFeatureEnabled(Slook.WRITINGBUDDY)));
		}

		private TextView createTextView(string str)
		{
			TextView tv = new TextView(this);
			tv.Text = str;
			return tv;
		}
	}

}