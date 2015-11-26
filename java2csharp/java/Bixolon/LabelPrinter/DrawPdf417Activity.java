package com.bixolon.labelprintersample;

import com.bixolon.labelprinter.BixolonLabelPrinter;

import android.os.Bundle;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.TextView;

public class DrawPdf417Activity extends Activity implements OnClickListener {
	private static final CharSequence[] ERROR_CORRECTION_LEVEL_ITEMS = {
		"EC level 0 (EC codeword 2)",
		"EC level 1 (EC codeword 4)",
		"EC level 2 (EC codeword 8)",
		"EC level 3 (EC codeword 16)",
		"EC level 4 (EC codeword 32)",
		"EC level 5 (EC codeword 64)",
		"EC level 6 (EC codeword 128)",
		"EC level 7 (EC codeword 265)",
		"EC level 8 (EC codeword 512)"
	};

	private AlertDialog mErrorCorrectionLevelDialog;
	private TextView mErrorCorrectionLevelTextView;
	private int mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL0;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_pdf417);
		
		mErrorCorrectionLevelTextView = (TextView) findViewById(R.id.textView5);

		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw_pdf417, menu);
		return true;
	}

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			showErrorCorrectionLevelDialog();
			break;

		case R.id.button2:
			printPdf417();
			break;
		}
	}
	
	private void showErrorCorrectionLevelDialog() {
		if (mErrorCorrectionLevelDialog == null) {

			mErrorCorrectionLevelDialog = new AlertDialog.Builder(DrawPdf417Activity.this).setTitle(R.string.error_correction_level)
					.setItems(ERROR_CORRECTION_LEVEL_ITEMS, new DialogInterface.OnClickListener() {
						
						public void onClick(DialogInterface dialog, int which) {
							switch (which) {
							case 0:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL0;
								break;
							case 1:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL1;
								break;
							case 2:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL2;
								break;
							case 3:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL3;
								break;
							case 4:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL4;
								break;
							case 5:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL5;
								break;
							case 6:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL6;
								break;
							case 7:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL7;
								break;
							case 8:
								mErrorCorrectionLevel = BixolonLabelPrinter.PDF417_ERROR_CORRECTION_LEVEL8;
								break;
							}
							mErrorCorrectionLevelTextView.setText(ERROR_CORRECTION_LEVEL_ITEMS[which]);
						}
					}).create();
		}
		mErrorCorrectionLevelDialog.show();
	}
	
	private void printPdf417() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		if(editText.getText().toString().equals("")){
			editText.setText("BIXOLON Label Printer");
		}
		String data = editText.getText().toString();
		
		editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int horizontalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("750");
		}
		int verticalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("30");
		}
		int maxRow = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText5);
		if(editText.getText().toString().equals("")){
			editText.setText("5");
		}
		int maxColumn = Integer.parseInt(editText.getText().toString());
		
		int compression = BixolonLabelPrinter.DATA_COMPRESSION_TEXT;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio1:
			compression = BixolonLabelPrinter.DATA_COMPRESSION_NUMERIC;
			break;
			
		case R.id.radio2:
			compression = BixolonLabelPrinter.DATA_COMPRESSION_BINARY;
			break;
		}
		
		int hri = BixolonLabelPrinter.PDF417_HRI_NOT_PRINTED;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		if (radioGroup.getCheckedRadioButtonId() == R.id.radio4) {
			hri = BixolonLabelPrinter.PDF417_HRI_BELOW_BARCODE;
		}
		
		int originPoint = BixolonLabelPrinter.BARCODE_ORIGIN_POINT_UPPER_LEFT;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
		if (radioGroup.getCheckedRadioButtonId() == R.id.radio5) {
			originPoint = BixolonLabelPrinter.BARCODE_ORIGIN_POINT_CENTER;
		}
		
		editText = (EditText) findViewById(R.id.editText6);
		if(editText.getText().toString().equals("")){
			editText.setText("3");
		}
		int width = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText7);
		if(editText.getText().toString().equals("")){
			editText.setText("10");
		}
		int height = Integer.parseInt(editText.getText().toString());
		
		int rotation = BixolonLabelPrinter.ROTATION_NONE;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup4);
		switch (radioGroup.getCheckedRadioButtonId()) {
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
		
		
		MainActivity.mBixolonLabelPrinter.drawPdf417(data, horizontalPosition, verticalPosition, maxRow, maxColumn, mErrorCorrectionLevel, compression,
				hri, originPoint, width, height, rotation);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}
}
