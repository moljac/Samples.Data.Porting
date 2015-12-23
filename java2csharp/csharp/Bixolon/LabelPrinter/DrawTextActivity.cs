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
	using SeekBar = android.widget.SeekBar;
	using OnSeekBarChangeListener = android.widget.SeekBar.OnSeekBarChangeListener;
	using TextView = android.widget.TextView;

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	public class DrawTextActivity : Activity, View.OnClickListener, SeekBar.OnSeekBarChangeListener, DialogInterface.OnClickListener
	{
		private static readonly CharSequence[] FONT_SELECTION_ITEMS = new CharSequence[] {"6pt (9x15)", "8pt (12x20)", "10pt (16x25)", "12pt (19x30)", "15pt (24x38)", "20pt (32x50)", "30pt (48x76)", "14pt (22x34)", "18pt (28x44)", "24pt (37x58)", "Korean1 (16x16, ascii 9x15)", "Korean2 (24x24, ascii 12x24)", "Korean3 (20x20, ascii 12x20)", "Korean4 (26x26, ascii 16x30)", "Korean5 (20x26, ascii 16x30)", "Korean6 (38x38, ascii 22x34)", "GB2312 (24x24, ascii 12x24)", "BIG5 (24x24, ascii 12x24)", "Shift JIS (24x24, ascii 12x24)"};

		private SeekBar mHorizontalMultiplierSeekBar;
		private SeekBar mVerticalMultiplierSeekBar;
		private TextView mFontTextView;

		private int mFontSize = BixolonLabelPrinter.FONT_SIZE_12;
		private int mHorizontalMultiplier = 1;
		private int mVerticalMultiplier = 1;

		private AlertDialog mFontSelectionDialog;

		private EditText editText;


		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_text;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;

			mFontTextView = (TextView) findViewById(R.id.textView3);

			mHorizontalMultiplierSeekBar = (SeekBar) findViewById(R.id.seekBar1);
			mHorizontalMultiplierSeekBar.OnSeekBarChangeListener = this;
			mVerticalMultiplierSeekBar = (SeekBar) findViewById(R.id.seekBar2);
			mVerticalMultiplierSeekBar.OnSeekBarChangeListener = this;


		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_text1, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				if (mFontSelectionDialog == null)
				{
					mFontSelectionDialog = (new AlertDialog.Builder(DrawTextActivity.this)).setTitle(R.@string.font_selection).setItems(FONT_SELECTION_ITEMS, this).create();
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
				editText.Text = "0";
			}
			int rightSpace = int.Parse(editText.Text.ToString());

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

			CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
			bool reverse = checkBox.Checked;

			checkBox = (CheckBox) findViewById(R.id.checkBox2);
			bool bold = checkBox.Checked;

			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			int alignment = BixolonLabelPrinter.TEXT_ALIGNMENT_NONE;
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio5:
				alignment = BixolonLabelPrinter.TEXT_ALIGNMENT_LEFT;
				break;
			case R.id.radio6:
				alignment = BixolonLabelPrinter.TEXT_ALIGNMENT_RIGHT;
				break;
			case R.id.radio7:
				alignment = BixolonLabelPrinter.TEXT_ALIGNMENT_RIGHT_TO_LEFT;
				break;
			}

			MainActivity.mBixolonLabelPrinter.drawText(data, horizontalPosition, verticalPosition, mFontSize, mHorizontalMultiplier, mVerticalMultiplier, rightSpace, rotation, reverse, bold, alignment);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}

		public virtual void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			if (seekBar == mHorizontalMultiplierSeekBar)
			{
				mHorizontalMultiplier = progress + 1;
			}
			else if (seekBar == mVerticalMultiplierSeekBar)
			{
				mVerticalMultiplier = progress + 1;
			}
		}

		public virtual void onStartTrackingTouch(SeekBar seekBar)
		{
			// TODO Auto-generated method stub

		}

		public virtual void onStopTrackingTouch(SeekBar seekBar)
		{
			// TODO Auto-generated method stub

		}

		public virtual void onClick(DialogInterface dialog, int which)
		{
			switch (which)
			{
			case 0:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_6;
				break;
			case 1:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_8;
				break;
			case 2:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_10;
				break;
			case 3:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_12;
				break;
			case 4:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_15;
				break;
			case 5:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_20;
				break;
			case 6:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_30;
				break;
			case 7:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_14;
				break;
			case 8:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_18;
				break;
			case 9:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_24;
				break;
			case 10:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN1;
				break;
			case 11:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN2;
				break;
			case 12:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN3;
				break;
			case 13:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN4;
				break;
			case 14:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN5;
				break;
			case 15:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_KOREAN6;
				break;
			case 16:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_GB2312;
				break;
			case 17:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_BIG5;
				break;
			case 18:
				mFontSize = BixolonLabelPrinter.FONT_SIZE_SHIFT_JIS;
				break;
			}
			mFontTextView.Text = FONT_SELECTION_ITEMS[which];
		}
	}

}