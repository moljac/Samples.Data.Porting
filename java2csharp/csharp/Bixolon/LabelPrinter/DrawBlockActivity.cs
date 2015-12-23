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
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;

	public class DrawBlockActivity : Activity
	{
		private RadioGroup mRadioGroup;
		private EditText mEditText;
		private EditText mExOrEditText7;
		private EditText mExOrEditText8;
		private EditText mExOrEditText9;
		private EditText mExOrEditText10;

		private int mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_OVERWRITING;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_block_two;

			mEditText = (EditText) findViewById(R.id.editText5);

			mExOrEditText7 = (EditText) findViewById(R.id.editText7);
			mExOrEditText8 = (EditText) findViewById(R.id.editText8);
			mExOrEditText9 = (EditText) findViewById(R.id.editText9);
			mExOrEditText10 = (EditText) findViewById(R.id.editText10);


			mRadioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			mRadioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly DrawBlockActivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(DrawBlockActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				switch (checkedId)
				{
				case R.id.radio0:
					outerInstance.mEditText.Enabled = false;
					outerInstance.exclusiveOrOption(false);
					outerInstance.mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_OVERWRITING;
					break;

				case R.id.radio1:
					outerInstance.mEditText.Enabled = false;
					outerInstance.exclusiveOrOption(true);
					outerInstance.mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_EXCLUSIVE_OR;
					break;

				case R.id.radio2:
					outerInstance.mEditText.Enabled = false;
					outerInstance.exclusiveOrOption(false);
					outerInstance.mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_DELETE;
					break;

				case R.id.radio3:
					outerInstance.mEditText.Enabled = true;
					outerInstance.exclusiveOrOption(false);
					outerInstance.mOption = BixolonLabelPrinter.BLOCK_OPTION_SLOPE;
					break;

				case R.id.radio4:
					outerInstance.mEditText.Enabled = true;
					outerInstance.exclusiveOrOption(false);
					outerInstance.mOption = BixolonLabelPrinter.BLOCK_OPTION_BOX;
					break;
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly DrawBlockActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(DrawBlockActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View arg0)
			{
				outerInstance.printBlock();
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_draw_block, menu);
			return true;
		}

		private void exclusiveOrOption(bool enabled)
		{

			mExOrEditText7.Enabled = enabled;
			mExOrEditText8.Enabled = enabled;
			mExOrEditText9.Enabled = enabled;
			mExOrEditText10.Enabled = enabled;
		}



		private void printBlock()
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
				editText.Text = "100";
			}
			int verticalStartPosition = int.Parse(editText.Text.ToString());
			editText = (EditText) findViewById(R.id.editText3);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "800";
			}
			int horizontalEndPosition = int.Parse(editText.Text.ToString());
			editText = (EditText) findViewById(R.id.editText4);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "120";
			}
			int verticalEndPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText5);
			if (editText.Text.ToString().Equals(""))
			{
				editText.Text = "10";
			}
			int thickness = int.Parse(editText.Text.ToString());


			if (mOption == BixolonLabelPrinter.BLOCK_OPTION_LINE_EXCLUSIVE_OR)
			{

				editText = (EditText) findViewById(R.id.editText7);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "200";
				}
				int horizontalStartPositionSquare2 = int.Parse(editText.Text.ToString());
				editText = (EditText) findViewById(R.id.editText8);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "10";
				}
				int verticalStartPositionSquare2 = int.Parse(editText.Text.ToString());
				editText = (EditText) findViewById(R.id.editText9);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "220";
				}
				int horizontalEndPositionSquare2 = int.Parse(editText.Text.ToString());
				editText = (EditText) findViewById(R.id.editText10);
				if (editText.Text.ToString().Equals(""))
				{
					editText.Text = "300";
				}
				int verticalEndPositionSquare2 = int.Parse(editText.Text.ToString());

				MainActivity.mBixolonLabelPrinter.drawTowBlock(horizontalStartPosition, verticalStartPosition, horizontalEndPosition, verticalEndPosition, mOption, horizontalStartPositionSquare2, verticalStartPositionSquare2, horizontalEndPositionSquare2, verticalEndPositionSquare2, mOption);
				MainActivity.mBixolonLabelPrinter.print(1, 1);


			}
			else
			{
				MainActivity.mBixolonLabelPrinter.drawBlock(horizontalStartPosition, verticalStartPosition, horizontalEndPosition, verticalEndPosition, mOption, thickness);
				MainActivity.mBixolonLabelPrinter.print(1, 1);
			}


		}
	}

}