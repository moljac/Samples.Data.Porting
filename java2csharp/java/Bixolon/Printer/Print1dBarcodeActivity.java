package com.bixolon.printersample;

import com.bixolon.printer.BixolonPrinter;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.Toast;

public class Print1dBarcodeActivity extends Activity {
	private EditText mDataEdit;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_print_barcode);
		
		mDataEdit = (EditText) findViewById(R.id.editText1);
		
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		radioGroup.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() {
			
			public void onCheckedChanged(RadioGroup group, int checkedId) {
				switch (checkedId) {
				case R.id.radio0:	// UPC-A
					mDataEdit.setText("012345678905");
						break;
				case R.id.radio1:	// UPC-E
					mDataEdit.setText("012345678905");
						break;
				case R.id.radio2:	// EAN13
					mDataEdit.setText("4711234567899");
						break;
				case R.id.radio3:	// EAN8
					mDataEdit.setText("47112346");
						break;
				case R.id.radio4:	// CODE39
					mDataEdit.setText("*ANDY*");
						break;
				case R.id.radio5:	// ITF
					mDataEdit.setText("1234567895");
						break;
				case R.id.radio6:	// CODABAR
					mDataEdit.setText("A1234567B");
						break;
				case R.id.radio7:	// CODE93
					mDataEdit.setText("12341555");
						break;
				case R.id.radio8:	// CODE128
					mDataEdit.setText("12345");
						break;
				}
				
			}
		});
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(new View.OnClickListener() {

			public void onClick(View v) {
				printBarCode();
			}
		});
	}

	private void printBarCode() {
		int barCodeSystem = 0;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		switch (radioGroup.getCheckedRadioButtonId()) {
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
		
		String data = mDataEdit.getText().toString();
		if (data == null || data.length() == 0) {
			Toast.makeText(getApplicationContext(), "Input bar code data", Toast.LENGTH_SHORT).show();
			return;
		}
		
		int alignment = BixolonPrinter.ALIGNMENT_LEFT;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		switch (radioGroup.getCheckedRadioButtonId()) {
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
		String string = editText.getText().toString();
		if (string.length() == 0) {
			Toast.makeText(getApplicationContext(), "Please enter the width again.", Toast.LENGTH_SHORT).show();
			return;
		}
		int width = Integer.parseInt(string);
		
		editText = (EditText) findViewById(R.id.editText3);
		string = editText.getText().toString();
		if (string.length() == 0) {
			Toast.makeText(getApplicationContext(), "Please enter the height again.", Toast.LENGTH_SHORT).show();
			return;
		}
		int height = Integer.parseInt(string);
		
		int characterPosition = 0;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
		switch (radioGroup.getCheckedRadioButtonId()) {
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
		if (checkBox.isChecked()) {
			MainActivity.mBixolonPrinter.print1dBarcode(data, barCodeSystem, alignment, width, height, characterPosition, false);
			MainActivity.mBixolonPrinter.formFeed(true);
		} else {
			MainActivity.mBixolonPrinter.print1dBarcode(data, barCodeSystem, alignment, width, height, characterPosition, true);
		}
	}
}
