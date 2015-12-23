namespace com.bixolon.labelprintersample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	public class DrawMaxicodeActivity : Activity, View.OnClickListener
	{

		private RadioGroup mRadioGroup;
		private EditText mEditText;

		private int mMode = BixolonLabelPrinter.MAXICODE_MODE0;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_maxicode;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;

			mEditText = (EditText) findViewById(R.id.editText1);

			mRadioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			mRadioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly DrawMaxicodeActivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(DrawMaxicodeActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				switch (checkedId)
				{
				case R.id.radio0:
					outerInstance.mEditText.Text = R.@string.mode0_data;
					outerInstance.mMode = BixolonLabelPrinter.MAXICODE_MODE0;
					break;

				case R.id.radio1:
					outerInstance.mEditText.Text = R.@string.mode2_data;
					outerInstance.mMode = BixolonLabelPrinter.MAXICODE_MODE2;
					break;

				case R.id.radio2:
					outerInstance.mEditText.Text = R.@string.mode3_data;
					outerInstance.mMode = BixolonLabelPrinter.MAXICODE_MODE3;
					break;

				case R.id.radio3:
					outerInstance.mEditText.Text = R.@string.mode4_data;
					outerInstance.mMode = BixolonLabelPrinter.MAXICODE_MODE4;
					break;
				}
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_maxicode, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			string data = mEditText.Text.ToString();

			EditText editText = (EditText) findViewById(R.id.editText2);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "200";
			}
			int horizontalPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText3);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "200";
			}
			int verticalPosition = int.Parse(editText.Text.ToString());

			MainActivity.mBixolonLabelPrinter.drawMaxicode(data, horizontalPosition, verticalPosition, mMode);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}
	}

}