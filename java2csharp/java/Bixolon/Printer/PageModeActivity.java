package com.bixolon.printersample;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.graphics.Bitmap;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.RelativeLayout;
import android.widget.Toast;

import com.bixolon.printer.BixolonPrinter;

public class PageModeActivity extends Activity implements OnClickListener {
	
	private AlertDialog mSampleDialog;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_page_mode);

		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button3);
		button.setOnClickListener(this);
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			setPageMode();
			break;

		case R.id.button2:
			printSample1();
			break;

		case R.id.button3:
			printSample2();
			break;
		}
	}

	private void setPageMode() {
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		if (radioGroup.getCheckedRadioButtonId() == R.id.radio0) {
			MainActivity.mBixolonPrinter.setStandardMode();
		} else {
			MainActivity.mBixolonPrinter.setPageMode();

			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			switch (radioGroup.getCheckedRadioButtonId()) {
			case R.id.radio2:
				MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_0_DEGREE_ROTATION);
				break;
			case R.id.radio3:
				MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_90_DEGREE_ROTATION);
				break;
			case R.id.radio4:
				MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_180_DEGREE_ROTATION);
				break;
			case R.id.radio5:
				MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_270_DEGREE_ROTATION);
				break;
			}

			EditText editText = (EditText) findViewById(R.id.editText1);
			String string = editText.getText().toString();
			if (string.length() == 0) {
				Toast.makeText(getApplicationContext(), "Please enter the horizontal start position again", Toast.LENGTH_SHORT).show();
				return;
			}
			int x = Integer.parseInt(string);

			editText = (EditText) findViewById(R.id.editText2);
			string = editText.getText().toString();
			if (string.length() == 0) {
				Toast.makeText(getApplicationContext(), "Please enter the vertical start position again", Toast.LENGTH_SHORT).show();
				return;
			}
			int y = Integer.parseInt(string);

			editText = (EditText) findViewById(R.id.editText3);
			string = editText.getText().toString();
			if (string.length() == 0) {
				Toast.makeText(getApplicationContext(), "Please enter the horizontal print area again", Toast.LENGTH_SHORT).show();
				return;
			}
			int width = Integer.parseInt(string);

			editText = (EditText) findViewById(R.id.editText4);
			string = editText.getText().toString();
			if (string.length() == 0) {
				Toast.makeText(getApplicationContext(), "Please enter the vertical print area again", Toast.LENGTH_SHORT).show();
				return;
			}
			int height = Integer.parseInt(string);

			MainActivity.mBixolonPrinter.setPrintArea(x, y, width, height);
		}
	}

	private void printSample1() {
		MainActivity.mBixolonPrinter.setPageMode();
		MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_180_DEGREE_ROTATION);
		MainActivity.mBixolonPrinter.setAbsoluteVerticalPrintPosition(0);
		MainActivity.mBixolonPrinter.setAbsolutePrintPosition(0);
		MainActivity.mBixolonPrinter.printText("Page mode\nsample",
				BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A,
				BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
		MainActivity.mBixolonPrinter.setAbsoluteVerticalPrintPosition(0);
		MainActivity.mBixolonPrinter.setAbsolutePrintPosition(128);

		BitmapDrawable drawable = (BitmapDrawable) getResources().getDrawable(R.drawable.bixolon);
		Bitmap bitmap = drawable.getBitmap(); 
		MainActivity.mBixolonPrinter.printBitmap(bitmap, BixolonPrinter.ALIGNMENT_LEFT, 128, 70, false);
		MainActivity.mBixolonPrinter.setAbsoluteVerticalPrintPosition(0);
		MainActivity.mBixolonPrinter.setAbsolutePrintPosition(256);
		MainActivity.mBixolonPrinter.printQrCode("www.bixolon.com", BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.QR_CODE_MODEL2, 4, false);
		MainActivity.mBixolonPrinter.formFeed(true);
	}

	private void printSample2() {
		if (mSampleDialog == null) {
			LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_page_mode_sample, null);
			
			mSampleDialog = new AlertDialog.Builder(PageModeActivity.this).setView(view).setPositiveButton("OK", new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					RelativeLayout layout = (RelativeLayout) view.findViewById(R.id.relativeLayout1);
					layout.buildDrawingCache();
					Bitmap bitmap = layout.getDrawingCache();
					
					MainActivity.mBixolonPrinter.setPageMode();
					MainActivity.mBixolonPrinter.setPrintDirection(BixolonPrinter.DIRECTION_180_DEGREE_ROTATION);
					MainActivity.mBixolonPrinter.setPrintArea(0, 0, 384, 840);
					MainActivity.mBixolonPrinter.setAbsoluteVerticalPrintPosition(100);
					MainActivity.mBixolonPrinter.printBitmap(bitmap, BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.BITMAP_WIDTH_FULL, 88, false);
					MainActivity.mBixolonPrinter.formFeed(true);
				}
			}).setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					// TODO Auto-generated method stub
					
				}
			}).create();
		}
		
		mSampleDialog.show();
	}
}
