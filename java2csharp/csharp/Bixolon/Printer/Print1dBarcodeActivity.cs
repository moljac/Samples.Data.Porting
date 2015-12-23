namespace com.bixolon.printersample
{

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using Toast = android.widget.Toast;

	public class Print1dBarcodeActivity : Activity
	{
		private EditText mDataEdit;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_print_barcode;

			mDataEdit = (EditText) findViewById(R.id.editText1);

			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			radioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly Print1dBarcodeActivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(Print1dBarcodeActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				switch (checkedId)
				{
				case R.id.radio0: // UPC-A
					outerInstance.mDataEdit.Text = "012345678905";
						break;
				case R.id.radio1: // UPC-E
					outerInstance.mDataEdit.Text = "012345678905";
						break;
				case R.id.radio2: // EAN13
					outerInstance.mDataEdit.Text = "4711234567899";
						break;
				case R.id.radio3: // EAN8
					outerInstance.mDataEdit.Text = "47112346";
						break;
				case R.id.radio4: // CODE39
					outerInstance.mDataEdit.Text = "*ANDY*";
						break;
				case R.id.radio5: // ITF
					outerInstance.mDataEdit.Text = "1234567895";
						break;
				case R.id.radio6: // CODABAR
					outerInstance.mDataEdit.Text = "A1234567B";
						break;
				case R.id.radio7: // CODE93
					outerInstance.mDataEdit.Text = "12341555";
						break;
				case R.id.radio8: // CODE128
					outerInstance.mDataEdit.Text = "12345";
						break;
				}

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly Print1dBarcodeActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Print1dBarcodeActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				outerInstance.printBarCode();
			}
		}

		private void printBarCode()
		{
			int barCodeSystem = 0;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio0:
				barCodeSystem = BixolonPrinter.BAR_CODE_UPC_A;
					break;
			case R.id.radio1:
				barCodeSystem = BixolonPrinter.BAR_CODE_UPC_E;
					break;
			case R.id.radio2:
				barCodeSystem = BixolonPrinter.BAR_CODE_EAN13;
					break;
			case R.id.radio3:
				barCodeSystem = BixolonPrinter.BAR_CODE_EAN8;
					break;
			case R.id.radio4:
				barCodeSystem = BixolonPrinter.BAR_CODE_CODE39;
					break;
			case R.id.radio5:
				barCodeSystem = BixolonPrinter.BAR_CODE_ITF;
					break;
			case R.id.radio6:
				barCodeSystem = BixolonPrinter.BAR_CODE_CODABAR;
					break;
			case R.id.radio7:
				barCodeSystem = BixolonPrinter.BAR_CODE_CODE93;
					break;
			case R.id.radio8:
				barCodeSystem = BixolonPrinter.BAR_CODE_CODE128;
					break;
			}

			string data = mDataEdit.Text.ToString();
			if (data == null || data.Length == 0)
			{
				Toast.makeText(ApplicationContext, "Input bar code data", Toast.LENGTH_SHORT).show();
				return;
			}

			int alignment = BixolonPrinter.ALIGNMENT_LEFT;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio9:
				alignment = BixolonPrinter.ALIGNMENT_LEFT;
				break;
			case R.id.radio10:
				alignment = BixolonPrinter.ALIGNMENT_CENTER;
				break;
			case R.id.radio11:
				alignment = BixolonPrinter.ALIGNMENT_RIGHT;
				break;
			}

			EditText editText = (EditText) findViewById(R.id.editText2);
			string @string = editText.Text.ToString();
			if (@string.Length == 0)
			{
				Toast.makeText(ApplicationContext, "Please enter the width again.", Toast.LENGTH_SHORT).show();
				return;
			}
			int width = int.Parse(@string);

			editText = (EditText) findViewById(R.id.editText3);
			@string = editText.Text.ToString();
			if (@string.Length == 0)
			{
				Toast.makeText(ApplicationContext, "Please enter the height again.", Toast.LENGTH_SHORT).show();
				return;
			}
			int height = int.Parse(@string);

			int characterPosition = 0;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio12:
				characterPosition = BixolonPrinter.HRI_CHARACTER_NOT_PRINTED;
				break;
			case R.id.radio13:
				characterPosition = BixolonPrinter.HRI_CHARACTERS_ABOVE_BAR_CODE;
				break;
			case R.id.radio14:
				characterPosition = BixolonPrinter.HRI_CHARACTERS_BELOW_BAR_CODE;
				break;
			case R.id.radio15:
				characterPosition = BixolonPrinter.HRI_CHARACTERS_ABOVE_AND_BELOW_BAR_CODE;
				break;
			}

			CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
			if (checkBox.Checked)
			{
				MainActivity.mBixolonPrinter.print1dBarcode(data, barCodeSystem, alignment, width, height, characterPosition, false);
				MainActivity.mBixolonPrinter.formFeed(true);
			}
			else
			{
				MainActivity.mBixolonPrinter.print1dBarcode(data, barCodeSystem, alignment, width, height, characterPosition, true);
			}
		}
	}

}