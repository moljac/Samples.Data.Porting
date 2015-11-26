package com.bixolon.labelprintersample;

import com.bixolon.labelprinter.BixolonLabelPrinter;

import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;

public class DrawBlockActivity extends Activity {
	private RadioGroup mRadioGroup;
	private EditText mEditText;
	private EditText mExOrEditText7;
	private EditText mExOrEditText8;
	private EditText mExOrEditText9;
	private EditText mExOrEditText10;
	
	private int mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_OVERWRITING;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_block_two);
		
		mEditText = (EditText) findViewById(R.id.editText5);
		
		mExOrEditText7 = (EditText) findViewById(R.id.editText7);
		mExOrEditText8 = (EditText) findViewById(R.id.editText8);
		mExOrEditText9 = (EditText) findViewById(R.id.editText9);
		mExOrEditText10 = (EditText) findViewById(R.id.editText10);
		
		
		mRadioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		mRadioGroup.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			
			public void onCheckedChanged(RadioGroup group, int checkedId) {
				switch (checkedId) {
				case R.id.radio0:
					mEditText.setEnabled(false);
					exclusiveOrOption(false);		
					mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_OVERWRITING;
					break;
					
				case R.id.radio1:
					mEditText.setEnabled(false);
					exclusiveOrOption(true);			
					mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_EXCLUSIVE_OR;
					break;
					
				case R.id.radio2:
					mEditText.setEnabled(false);
					exclusiveOrOption(false);	
					mOption = BixolonLabelPrinter.BLOCK_OPTION_LINE_DELETE;
					break;
					
				case R.id.radio3:
					mEditText.setEnabled(true);
					exclusiveOrOption(false);	
					mOption = BixolonLabelPrinter.BLOCK_OPTION_SLOPE;
					break;
					
				case R.id.radio4:
					mEditText.setEnabled(true);
					exclusiveOrOption(false);	
					mOption = BixolonLabelPrinter.BLOCK_OPTION_BOX;
					break;
				}
			}
		});
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(new OnClickListener() {
			
			public void onClick(View arg0) {
				printBlock();
			}
		});
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw_block, menu);
		return true;
	}
	
	private void exclusiveOrOption(boolean enabled){
		
		mExOrEditText7.setEnabled(enabled);
		mExOrEditText8.setEnabled(enabled);
		mExOrEditText9.setEnabled(enabled);
		mExOrEditText10.setEnabled(enabled);
	}
	
	
	
	private void printBlock() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int horizontalStartPosition = Integer.parseInt(editText.getText().toString());
		editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int verticalStartPosition = Integer.parseInt(editText.getText().toString());
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("800");
		}
		int horizontalEndPosition = Integer.parseInt(editText.getText().toString());
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("120");
		}
		int verticalEndPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText5);
		if(editText.getText().toString().equals("")){
			editText.setText("10");
		}
		int thickness = Integer.parseInt(editText.getText().toString());
		
		
		if(mOption == BixolonLabelPrinter.BLOCK_OPTION_LINE_EXCLUSIVE_OR){
			
			editText = (EditText) findViewById(R.id.editText7);
			if(editText.getText().toString().equals("")){
				editText.setText("200");
			}
			int horizontalStartPositionSquare2 = Integer.parseInt(editText.getText().toString());
			editText = (EditText) findViewById(R.id.editText8);
			if(editText.getText().toString().equals("")){
				editText.setText("10");
			}
			int verticalStartPositionSquare2 = Integer.parseInt(editText.getText().toString());
			editText = (EditText) findViewById(R.id.editText9);
			if(editText.getText().toString().equals("")){
				editText.setText("220");
			}
			int horizontalEndPositionSquare2 = Integer.parseInt(editText.getText().toString());
			editText = (EditText) findViewById(R.id.editText10);
			if(editText.getText().toString().equals("")){
				editText.setText("300");
			}
			int verticalEndPositionSquare2 = Integer.parseInt(editText.getText().toString());
			
			MainActivity.mBixolonLabelPrinter.drawTowBlock(horizontalStartPosition, verticalStartPosition, horizontalEndPosition, verticalEndPosition, mOption,
					horizontalStartPositionSquare2, verticalStartPositionSquare2, horizontalEndPositionSquare2, verticalEndPositionSquare2, mOption);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
			
			
		}else{
			MainActivity.mBixolonLabelPrinter.drawBlock(horizontalStartPosition, verticalStartPosition, horizontalEndPosition, verticalEndPosition, mOption, thickness);
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}
		
		
	}
}
