package com.bixolon.labelprintersample;

import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;

import com.bixolon.labelprinter.BixolonLabelPrinter;

public class DrawMaxicodeActivity extends Activity implements OnClickListener {

	private RadioGroup mRadioGroup;
	private EditText mEditText;
	
	private int mMode = BixolonLabelPrinter.MAXICODE_MODE0;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_maxicode);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		
		mEditText = (EditText) findViewById(R.id.editText1);
		
		mRadioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		mRadioGroup.setOnCheckedChangeListener(new OnCheckedChangeListener() {
			
			public void onCheckedChanged(RadioGroup group, int checkedId) {
				switch (checkedId) {
				case R.id.radio0:
					mEditText.setText(R.string.mode0_data);
					mMode = BixolonLabelPrinter.MAXICODE_MODE0;
					break;
					
				case R.id.radio1:
					mEditText.setText(R.string.mode2_data);
					mMode = BixolonLabelPrinter.MAXICODE_MODE2;
					break;
					
				case R.id.radio2:
					mEditText.setText(R.string.mode3_data);
					mMode = BixolonLabelPrinter.MAXICODE_MODE3;
					break;
					
				case R.id.radio3:
					mEditText.setText(R.string.mode4_data);
					mMode = BixolonLabelPrinter.MAXICODE_MODE4;
					break;
				}
			}
		});
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw_maxicode, menu);
		return true;
	}

	public void onClick(View v) {
		String data = mEditText.getText().toString();
		
		EditText editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("200");
		}
		int horizontalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("200");
		}
		int verticalPosition = Integer.parseInt(editText.getText().toString());
		
		MainActivity.mBixolonLabelPrinter.drawMaxicode(data, horizontalPosition, verticalPosition, mMode);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}
}
