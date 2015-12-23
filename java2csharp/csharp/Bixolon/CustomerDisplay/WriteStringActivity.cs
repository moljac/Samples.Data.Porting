namespace com.bixolon.customerdisplaysample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;

	using BixolonCustomerDisplay = com.bixolon.customerdisplay.BixolonCustomerDisplay;

	public class WriteStringActivity : Activity, View.OnClickListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_write_string;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_write_string, menu);
			return true;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio0:
					MainActivity.mBcd.ScrollMode = BixolonCustomerDisplay.MODE_OVERWRITE;
					break;
				case R.id.radio1:
					MainActivity.mBcd.ScrollMode = BixolonCustomerDisplay.MODE_VERTICAL;
					break;
				case R.id.radio2:
					MainActivity.mBcd.ScrollMode = BixolonCustomerDisplay.MODE_HORIZONTAL;
					break;
				}

				EditText editText = (EditText) findViewById(R.id.editText3);
				string @string = editText.Text.ToString();
				if (@string.Length > 0)
				{
					int interval = int.Parse(@string);
					MainActivity.mBcd.BlinkInterval = interval;
				}

				radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio3:
					MainActivity.mBcd.DimmingControl = BixolonCustomerDisplay.BRIGHTNESS_20;
					break;
				case R.id.radio4:
					MainActivity.mBcd.DimmingControl = BixolonCustomerDisplay.BRIGHTNESS_40;
					break;
				case R.id.radio5:
					MainActivity.mBcd.DimmingControl = BixolonCustomerDisplay.BRIGHTNESS_60;
					break;
				case R.id.radio6:
					MainActivity.mBcd.DimmingControl = BixolonCustomerDisplay.BRIGHTNESS_100;
					break;
				}

				radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio7:
					MainActivity.mBcd.clearLineBlinking(BixolonCustomerDisplay.UPPER_LINE);
					MainActivity.mBcd.clearLineBlinking(BixolonCustomerDisplay.LOWER_LINE);
					break;
				case R.id.radio8:
					MainActivity.mBcd.LineBlinking = BixolonCustomerDisplay.UPPER_LINE;
					break;
				case R.id.radio9:
					MainActivity.mBcd.LineBlinking = BixolonCustomerDisplay.LOWER_LINE;
					break;
				}

				editText = (EditText) findViewById(R.id.editText1);
				string text = editText.Text.ToString();
				MainActivity.mBcd.writeString(text, BixolonCustomerDisplay.UPPER_LINE);

				editText = (EditText) findViewById(R.id.editText2);
				text = editText.Text.ToString();
				MainActivity.mBcd.writeString(text, BixolonCustomerDisplay.LOWER_LINE);
				break;

			case R.id.button2:
				MainActivity.mBcd.clearScreen();
				break;
			}
		}
	}

}