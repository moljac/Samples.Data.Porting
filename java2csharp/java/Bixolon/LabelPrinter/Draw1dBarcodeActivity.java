package com.bixolon.labelprintersample;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.TextView;

import com.bixolon.labelprinter.BixolonLabelPrinter;

public class Draw1dBarcodeActivity extends Activity implements View.OnClickListener, DialogInterface.OnClickListener {
	private static final String[] BARCODE_SELECTION_ITEMS = {
		"Code39",
		"Code128",
		"I2of5",
		"Codabar",
		"Code93",
		"UPC-A",
		"UPC-E",
		"EAN13",
		"EAN8",
		"UCC/EAN128"
	};
	
	private static final String[] HRI_ITEMS = {
		"Not printed",
		"Below the barcode (font size: 1)",
		"Above the barcode (font size: 1)",
		"Below the barcode (font size: 2)",
		"Above the barcode (font size: 2)",
		"Below the barcode (font size: 3)",
		"Above the barcode (font size: 3)",
		"Below the barcode (font size: 4)",
		"Above the barcode (font size: 4)"
	};
	
	private AlertDialog mBarcodeSelectionDialog;
	private AlertDialog mHriSelectionDialog;
	
	private int mBarcodeSelection = BixolonLabelPrinter.BARCODE_CODE39;
	private int mHri = BixolonLabelPrinter.HRI_NOT_PRINTED;
	
	private TextView mBarcodeTextView;
	private TextView mHriTextView;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw1d_barcode);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button3);
		button.setOnClickListener(this);
		
		mBarcodeTextView = (TextView) findViewById(R.id.textView3);
		mHriTextView = (TextView) findViewById(R.id.textView8);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw1d_barcode, menu);
		return true;
	}
	
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			if (mBarcodeSelectionDialog == null) {
				mBarcodeSelectionDialog = new AlertDialog.Builder(Draw1dBarcodeActivity.this)
					.setTitle(R.string.barcode_selection).setItems(BARCODE_SELECTION_ITEMS, this).create();
			}
			mBarcodeSelectionDialog.show();
			break;
			
		case R.id.button2:
			if (mHriSelectionDialog == null) {
				mHriSelectionDialog = new AlertDialog.Builder(Draw1dBarcodeActivity.this)
					.setTitle(R.string.hri).setItems(HRI_ITEMS, this).create();
			}
			mHriSelectionDialog.show();
			break;
			
		case R.id.button3:
			print1dBarcode();
			break;
		}
	}
	
	private void print1dBarcode() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		if(editText.getText().toString().equals("")){
			editText.setText("1234567890");
		}
		String data = editText.getText().toString();
		
		editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("78");
		}
		int horizontalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("196");
		}
		int verticalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("2");
		}
		int narrowBarWidth = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText5);
		if(editText.getText().toString().equals("")){
			editText.setText("6");
		}
		int wideBarWidth = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText6);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int height = Integer.parseInt(editText.getText().toString());
		
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		int rotation = BixolonLabelPrinter.ROTATION_NONE;
		switch (radioGroup.getCheckedRadioButtonId()) {
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
		
		editText = (EditText) findViewById(R.id.editText7);
		if(editText.getText().toString().equals("")){
			editText.setText("0");
		}
		int quietZoneWidth = Integer.parseInt(editText.getText().toString());
		
		MainActivity.mBixolonLabelPrinter.draw1dBarcode(data, horizontalPosition, verticalPosition, mBarcodeSelection,
				narrowBarWidth, wideBarWidth, height, rotation, mHri, quietZoneWidth);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}

	public void onClick(DialogInterface dialog, int which) {
		if (dialog == mBarcodeSelectionDialog) {
			switch (which) {
			case 0:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_CODE39;
				break;
			case 1:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_CODE128;
				break;
			case 2:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_I2OF5;
				break;
			case 3:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_CODABAR;
				break;
			case 4:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_CODE93;
				break;
			case 5:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_UPC_A;
				break;
			case 6:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_UPC_E;
				break;
			case 7:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_EAN13;
				break;
			case 8:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_EAN8;
				break;
			case 9:
				mBarcodeSelection = BixolonLabelPrinter.BARCODE_UCC_EAN128;
				break;
			}
			mBarcodeTextView.setText(BARCODE_SELECTION_ITEMS[which]);
		} else if (dialog == mHriSelectionDialog) {
			switch (which) {
			case 0:
				mHri = BixolonLabelPrinter.HRI_NOT_PRINTED;
				break;
			case 1:
				mHri = BixolonLabelPrinter.HRI_BELOW_FONT_SIZE_1;
				break;
			case 2:
				mHri = BixolonLabelPrinter.HRI_ABOVE_FONT_SIZE_1;
				break;
			case 3:
				mHri = BixolonLabelPrinter.HRI_BELOW_FONT_SIZE_2;
				break;
			case 4:
				mHri = BixolonLabelPrinter.HRI_ABOVE_FONT_SIZE_2;
				break;
			case 5:
				mHri = BixolonLabelPrinter.HRI_BELOW_FONT_SIZE_3;
				break;
			case 6:
				mHri = BixolonLabelPrinter.HRI_ABOVE_FONT_SIZE_3;
				break;
			case 7:
				mHri = BixolonLabelPrinter.HRI_BELOW_FONT_SIZE_4;
				break;
			case 8:
				mHri = BixolonLabelPrinter.HRI_ABOVE_FONT_SIZE_4;
				break;
			}
			mHriTextView.setText(HRI_ITEMS[which]);
		}
	}
}
