package com.bixolon.labelprintersample;

import com.bixolon.labelprinter.BixolonLabelPrinter;

import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.RadioGroup;

public class DrawDataMatrixActivity extends Activity {

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_data_matrix);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(new OnClickListener() {
			
			public void onClick(View arg0) {
				printDataMatrix();
			}
		});
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_draw_data_matrix, menu);
		return true;
	}
	
	private void printDataMatrix() {
		EditText editText = (EditText) findViewById(R.id.editText1);
		if(editText.getText().toString().equals("")){
			editText.setText("BIXOLON Label Printer");
		}
		String data = editText.getText().toString();
		
		editText = (EditText) findViewById(R.id.editText2);
		if(editText.getText().toString().equals("")){
			editText.setText("200");
		}
		int horizontalPosition = Integer.parseInt(editText.getText().toString());
		editText = (EditText) findViewById(R.id.editText3);
		if(editText.getText().toString().equals("")){
			editText.setText("100");
		}
		int verticalPosition = Integer.parseInt(editText.getText().toString());
		
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("2");
		}
		int size = Integer.parseInt(editText.getText().toString());
		
		CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
		boolean isReversed = checkBox.isChecked();
		
		int rotation = BixolonLabelPrinter.ROTATION_NONE;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio2:
			rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
			break;
			
		case R.id.radio3:
			rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
			break;
			
		case R.id.radio4:
			rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
			break;
		}
		
		MainActivity.mBixolonLabelPrinter.drawDataMatrix(data, horizontalPosition, verticalPosition, size, isReversed, rotation);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}

}
