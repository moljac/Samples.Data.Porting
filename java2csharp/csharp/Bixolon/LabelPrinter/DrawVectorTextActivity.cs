namespace com.bixolon.labelprintersample
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using TextView = android.widget.TextView;

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	public class DrawVectorTextActivity : Activity, View.OnClickListener, DialogInterface.OnClickListener
	{
		private static readonly string[] FONT_SELECTION_ITEMS = new string[] {"ASCII (1 byte code)", "KS5601 (2 byte code)", "BIG5 (2 byte code)", "GB2312 (2 byte code)", "SHIFT-JIS (2 byte code)", "OCR-A (1 byte code)", "OCR-B (1 byte code)"};

		private int mFont = BixolonLabelPrinter.VECTOR_FONT_ASCII;
		private TextView mFontTextView;

		private AlertDialog mFontSelectionDialog;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_vector_text;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;

			mFontTextView = (TextView) findViewById(R.id.textView3);
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_vector_text, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				if (mFontSelectionDialog == null)
				{
					mFontSelectionDialog = (new AlertDialog.Builder(DrawVectorTextActivity.this)).setTitle(R.@string.font_selection).setItems(FONT_SELECTION_ITEMS, this).create();
				}
				mFontSelectionDialog.show();
				break;

			case R.id.button2:
				printLabel();
				break;
			}
		}

		private void printLabel()
		{
			EditText editText = (EditText) findViewById(R.id.editText1);
			string data = editText.Text.ToString();

			editText = (EditText) findViewById(R.id.editText2);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "50";
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
				editText.Text = "25";
			}
			int width = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText5);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "25";
			}
			int height = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText6);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "0";
			}
			int rightSpace = int.Parse(editText.Text.ToString());

			CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
			bool bold = checkBox.Checked;
			checkBox = (CheckBox) findViewById(R.id.checkBox2);
			bool reverse = checkBox.Checked;
			checkBox = (CheckBox) findViewById(R.id.checkBox3);
			bool italic = checkBox.Checked;

			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			int rotation = BixolonLabelPrinter.ROTATION_NONE;
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio1:
				rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
				break;
			case R.id.radio2:
				rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
				break;
			case R.id.radio3:
				rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
				break;
			}

			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			int alignment = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_LEFT;
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio5:
				rotation = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_RIGHT;
				break;
			case R.id.radio6:
				rotation = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_CENTER;
				break;
			}

			radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
			int direction = BixolonLabelPrinter.VECTOR_FONT_TEXT_DIRECTION_LEFT_TO_RIGHT;
			if (radioGroup.CheckedRadioButtonId == R.id.radio8)
			{
				direction = BixolonLabelPrinter.VECTOR_FONT_TEXT_DIRECTION_RIGHT_TO_LEET;
			}

			MainActivity.mBixolonLabelPrinter.drawVectorFontText(data, horizontalPosition, verticalPosition, mFont, width, height, rightSpace, bold, reverse, italic, rotation, alignment, direction);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}

		public virtual void onClick(DialogInterface dialog, int which)
		{
			switch (which)
			{
			case 0:
				mFont = BixolonLabelPrinter.VECTOR_FONT_ASCII;
				break;
			case 1:
				mFont = BixolonLabelPrinter.VECTOR_FONT_KS5601;
				break;
			case 2:
				mFont = BixolonLabelPrinter.VECTOR_FONT_BIG5;
				break;
			case 3:
				mFont = BixolonLabelPrinter.VECTOR_FONT_GB2312;
				break;
			case 4:
				mFont = BixolonLabelPrinter.VECTOR_FONT_SHIFT_JIS;
				break;
			case 5:
				mFont = BixolonLabelPrinter.VECTOR_FONT_OCR_A;
				break;
			case 6:
				mFont = BixolonLabelPrinter.VECTOR_FONT_OCR_B;
				break;
			}
			mFontTextView.Text = FONT_SELECTION_ITEMS[which];
		}
	}

}