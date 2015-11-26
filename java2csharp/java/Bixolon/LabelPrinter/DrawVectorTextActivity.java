package com.bixolon.labelprintersample;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.TextView;

import com.bixolon.labelprinter.BixolonLabelPrinter;

public class DrawVectorTextActivity extends Activity implements View.OnClickListener, DialogInterface.OnClickListener {
	private static final String[] FONT_SELECTION_ITEMS = {
		"ASCII (1 byte code)",
		"KS5601 (2 byte code)",
		"BIG5 (2 byte code)",
		"GB2312 (2 byte code)",
		"SHIFT-JIS (2 byte code)",
		"OCR-A (1 byte code)",
		"OCR-B (1 byte code)"
	};
	
	private int mFont = BixolonLabelPrinter.VECTOR_FONT_ASCII;
	private TextView mFontTextView;
	
	private AlertDialog mFontSelectionDialog;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_vector_text);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
		
		mFontTextView = (TextView) findViewById(R.id.textView3);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw_vector_text, menu);
		return true;
	}

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			if (mFontSelectionDialog == null) {
				mFontSelectionDialog = new AlertDialog.Builder(DrawVectorTextActivity.this)
					.setTitle(R.string.font_selection).setItems(FONT_SELECTION_ITEMS, this).create();
			}
			mFontSelectionDialog.show();
			break;
			
		case R.id.button2:
			printLabel();
			break;
		}
	}
	
	private void printLabel() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		String data = editText.getText().toString();
		
		editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("50");
		}
		int horizontalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int verticalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("25");
		}
		int width = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText5);
		if(editText.getText().toString().equals("")){
			editText.setText("25");
		}
		int height = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText6);
		if(editText.getText().toString().equals("")){
			editText.setText("0");
		}
		int rightSpace = Integer.parseInt(editText.getText().toString());
		
		CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
		boolean bold = checkBox.isChecked();
		checkBox = (CheckBox) findViewById(R.id.checkBox2);
		boolean reverse = checkBox.isChecked();
		checkBox = (CheckBox) findViewById(R.id.checkBox3);
		boolean italic = checkBox.isChecked();
		
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
		
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		int alignment = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_LEFT;
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio5:
			rotation = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_RIGHT;
			break;
		case R.id.radio6:
			rotation = BixolonLabelPrinter.VECTOR_FONT_TEXT_ALIGNMENT_CENTER;
			break;
		}
		
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
		int direction = BixolonLabelPrinter.VECTOR_FONT_TEXT_DIRECTION_LEFT_TO_RIGHT;
		if (radioGroup.getCheckedRadioButtonId() == R.id.radio8) {
			direction = BixolonLabelPrinter.VECTOR_FONT_TEXT_DIRECTION_RIGHT_TO_LEET;
		}
		
		MainActivity.mBixolonLabelPrinter.drawVectorFontText(data, horizontalPosition, verticalPosition, mFont, width, height, rightSpace,
				bold, reverse, italic, rotation, alignment, direction);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}

	public void onClick(DialogInterface dialog, int which) {
		switch (which) {
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
		mFontTextView.setText(FONT_SELECTION_ITEMS[which]);
	}
}
