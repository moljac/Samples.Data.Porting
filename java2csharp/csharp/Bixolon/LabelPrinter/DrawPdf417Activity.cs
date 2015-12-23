namespace com.bixolon.labelprintersample
{

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	using Bundle = android.os.Bundle;
	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using TextView = android.widget.TextView;

	public class DrawPdf417Activity : Activity, View.OnClickListener
	{
		private static readonly CharSequence[] ERROR_CORRECTION_LEVEL_ITEMS = new CharSequence[] {"EC level 0 (EC codeword 2)", "EC level 1 (EC codeword 4)", "EC level 2 (EC codeword 8)", "EC level 3 (EC codeword 16)", "EC level 4 (EC codeword 32)", "EC level 5 (EC codeword 64)", "EC level 6 (EC codeword 128)", "EC level 7 (EC codeword 265)", "EC level 8 (EC codeword 512)"};

		private AlertDialog mErrorCorrectionLevelDialog;
		private TextView mErrorCorrectionLevelTextView;
		private int mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL0;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_pdf417;

			mErrorCorrectionLevelTextView = (TextView) findViewById(R.id.textView5);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_pdf417, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				showErrorCorrectionLevelDialog();
				break;

			case R.id.button2:
				printPdf417();
				break;
			}
		}

		private void showErrorCorrectionLevelDialog()
		{
			if (mErrorCorrectionLevelDialog == null)
			{

				mErrorCorrectionLevelDialog = (new AlertDialog.Builder(DrawPdf417Activity.this)).setTitle(R.@string.error_correction_level).setItems(ERROR_CORRECTION_LEVEL_ITEMS, new OnClickListenerAnonymousInnerClassHelper(this))
					   .create();
			}
			mErrorCorrectionLevelDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly DrawPdf417Activity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DrawPdf417Activity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL0;
					break;
				case 1:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL1;
					break;
				case 2:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL2;
					break;
				case 3:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL3;
					break;
				case 4:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL4;
					break;
				case 5:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL5;
					break;
				case 6:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL6;
					break;
				case 7:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL7;
					break;
				case 8:
					outerInstance.mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL8;
					break;
				}
				outerInstance.mErrorCorrectionLevelTextView.Text = ERROR_CORRECTION_LEVEL_ITEMS[which];
			}
		}

		private void printPdf417()
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
				editText.Text = "100";
			}
			int horizontalPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText3);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "750";
			}
			int verticalPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText4);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "30";
			}
			int maxRow = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText5);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "5";
			}
			int maxColumn = int.Parse(editText.Text.ToString());

			int compression = BixolonLabelPrinter.DATA_COMPRESSION_TEXT;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio1:
				compression = BixolonLabelPrinter.DATA_COMPRESSION_NUMERIC;
				break;

			case R.id.radio2:
				compression = BixolonLabelPrinter.DATA_COMPRESSION_BINARY;
				break;
			}

			int hri = BixolonLabelPrinter.PDF417_HRI_NOT_PRINTED;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			if (radioGroup.CheckedRadioButtonId == R.id.radio4)
			{
				hri = BixolonLabelPrinter.PDF417_HRI_BELOW_BARCODE;
			}

			int originPoint = BixolonLabelPrinter.BARCODE_ORIGIN_POINT_UPPER_LEFT;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
			if (radioGroup.CheckedRadioButtonId == R.id.radio5)
			{
				originPoint = BixolonLabelPrinter.BARCODE_ORIGIN_POINT_CENTER;
			}

			editText = (EditText) findViewById(R.id.editText6);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "3";
			}
			int width = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText7);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "10";
			}
			int height = int.Parse(editText.Text.ToString());

			int rotation = BixolonLabelPrinter.ROTATION_NONE;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup4);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio8:
				rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
				break;

			case R.id.radio9:
				rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
				break;

			case R.id.radio10:
				rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
				break;
			}


			MainActivity.mBixolonLabelPrinter.drawPdf417(data, horizontalPosition, verticalPosition, maxRow, maxColumn, mErrorCorrectionLevel, compression, hri, originPoint, width, height, rotation);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}
	}

}