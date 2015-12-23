namespace com.bixolon.labelprintersample
{

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;

	public class DrawCircleActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_circle;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly DrawCircleActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DrawCircleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View arg0)
			{
				outerInstance.printCircle();
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_draw_circle, menu);
			return true;
		}

		private void printCircle()
		{
			EditText editText = (EditText) findViewById(R.id.editText1);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "100";
			}
			int horizontalStartPosition = int.Parse(editText.Text.ToString());
			editText = (EditText) findViewById(R.id.editText2);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "200";
			}
			int verticalStartPosition = int.Parse(editText.Text.ToString());

			int size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER5;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio1:
				size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER7;
				break;
			case R.id.radio2:
				size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER9;
				break;
			case R.id.radio3:
				size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER11;
				break;
			case R.id.radio4:
				size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER13;
				break;
			case R.id.radio5:
				size = BixolonLabelPrinter.CIRCLE_SIZE_DIAMETER21;
				break;
			}

			editText = (EditText) findViewById(R.id.editText3);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "2";
			}
			int multiplier = int.Parse(editText.Text.ToString());

			MainActivity.mBixolonLabelPrinter.drawCircle(horizontalStartPosition, verticalStartPosition, size, multiplier);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}
	}

}