namespace com.bixolon.labelprintersample
{

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;

	public class DrawDataMatrixActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_data_matrix;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly DrawDataMatrixActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DrawDataMatrixActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View arg0)
			{
				outerInstance.printDataMatrix();
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_draw_data_matrix, menu);
			return true;
		}

		private void printDataMatrix()
		{
			EditText editText = (EditText) findViewById(R.id.editText1);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "BIXOLON Label Printer";
			}
			string data = editText.Text.ToString();

			editText = (EditText) findViewById(R.id.editText2);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "200";
			}
			int horizontalPosition = int.Parse(editText.Text.ToString());
			editText = (EditText) findViewById(R.id.editText3);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "100";
			}
			int verticalPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText4);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "2";
			}
			int size = int.Parse(editText.Text.ToString());

			CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
			bool isReversed = checkBox.Checked;

			int rotation = BixolonLabelPrinter.ROTATION_NONE;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio2:
				rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
				break;

			case R.id.radio3:
				rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
				break;

			case R.id.radio4:
				rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
				break;
			}

			MainActivity.mBixolonLabelPrinter.drawDataMatrix(data, horizontalPosition, verticalPosition, size, isReversed, rotation);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}

	}

}