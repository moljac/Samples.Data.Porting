package com.bixolon.printersample;

import com.bixolon.printer.BixolonPrinter;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.SeekBar;

public class PrintTextActivity extends Activity {
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_pint_text);

		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(new View.OnClickListener() {

			public void onClick(View v) {
				printText();
			}
		});
	}
	
	private void printText() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		String text = editText.getText().toString();
		
		int alignment = BixolonPrinter.ALIGNMENT_LEFT;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio0:
			alignment = BixolonPrinter.ALIGNMENT_LEFT;
			break;
		case R.id.radio1:
			alignment = BixolonPrinter.ALIGNMENT_CENTER;
			break;
		case R.id.radio2:
			alignment = BixolonPrinter.ALIGNMENT_RIGHT;
			break;
		}
		
		int attribute = 0;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio3:
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_FONT_A;
			break;
		case R.id.radio4:
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_FONT_B;
			break;
		case R.id.radio5:
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_FONT_C;
			break;
		}
		
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio7:
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_UNDERLINE1;
			break;
		case R.id.radio8:
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_UNDERLINE2;
			break;
		}
		
		CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
		if (checkBox.isChecked()) {
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_EMPHASIZED;
		}
		
		checkBox = (CheckBox) findViewById(R.id.checkBox2);
		if (checkBox.isChecked()) {
			attribute |= BixolonPrinter.TEXT_ATTRIBUTE_REVERSE;
		}
		
		int size = 0;
		SeekBar seekBar = (SeekBar) findViewById(R.id.seekBar1);
		switch (seekBar.getProgress()) {
		case 0:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL1;
			break;
		case 1:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL2;
			break;
		case 2:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL3;
			break;
		case 3:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL4;
			break;
		case 4:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL5;
			break;
		case 5:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL6;
			break;
		case 6:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL7;
			break;
		case 7:
			size = BixolonPrinter.TEXT_SIZE_HORIZONTAL8;
			break;
		}
		
		seekBar = (SeekBar) findViewById(R.id.seekBar2);
		switch (seekBar.getProgress()) {
		case 0:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL1;
			break;
		case 1:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL2;
			break;
		case 2:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL3;
			break;
		case 3:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL4;
			break;
		case 4:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL5;
			break;
		case 5:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL6;
			break;
		case 6:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL7;
			break;
		case 7:
			size |= BixolonPrinter.TEXT_SIZE_VERTICAL8;
			break;
		}
		
		checkBox = (CheckBox) findViewById(R.id.checkBox3);
		boolean formFeed = checkBox.isChecked();
		
		checkBox = (CheckBox) findViewById(R.id.checkBox4);
		if (checkBox.isChecked()) {
			MainActivity.mBixolonPrinter.printDotMatrixText(text, alignment, attribute, size, false);
		} else {
			if (formFeed) {
				MainActivity.mBixolonPrinter.printText(text, alignment, attribute, size, false);
				MainActivity.mBixolonPrinter.formFeed(false);
			} else {
				MainActivity.mBixolonPrinter.printText(text, alignment, attribute, size, false);
			}
		}
		MainActivity.mBixolonPrinter.lineFeed(3, false);
		
		MainActivity.mBixolonPrinter.cutPaper(true);
		MainActivity.mBixolonPrinter.kickOutDrawer(BixolonPrinter.DRAWER_CONNECTOR_PIN5);
	}
}
