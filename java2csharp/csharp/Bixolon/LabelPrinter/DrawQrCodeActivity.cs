namespace com.bixolon.labelprintersample
{

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	using Bundle = android.os.Bundle;
	using Activity = android.app.Activity;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;

	public class DrawQrCodeActivity : Activity, View.OnClickListener
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_qr_code;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_qr_code, menu);
			return true;
		}

		public virtual void onClick(View v)
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

			int model = BixolonLabelPrinter.QR_CODE_MODEL1;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			if (radioGroup.CheckedRadioButtonId == R.id.radio1)
			{
				model = BixolonLabelPrinter.QR_CODE_MODEL2;
			}

			int eccLevel = BixolonLabelPrinter.ECC_LEVEL_7;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio3:
				eccLevel = BixolonLabelPrinter.ECC_LEVEL_15;
				break;

			case R.id.radio4:
				eccLevel = BixolonLabelPrinter.ECC_LEVEL_25;
				break;

			case R.id.radio5:
				eccLevel = BixolonLabelPrinter.ECC_LEVEL_30;
				break;
			}

			editText = (EditText) findViewById(R.id.editText4);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "4";
			}
			int size = int.Parse(editText.Text.ToString());

			int rotation = BixolonLabelPrinter.ROTATION_NONE;
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio7:
				rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
				break;

			case R.id.radio8:
				rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
				break;

			case R.id.radio9:
				rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
				break;
			}

			MainActivity.mBixolonLabelPrinter.drawQrCode(data, horizontalPosition, verticalPosition, model, eccLevel, size, rotation);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}
	}

}