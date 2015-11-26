package com.bixolon.labelprintersample;

import com.bixolon.labelprinter.BixolonLabelPrinter;

import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;

public class DrawQrCodeActivity extends Activity implements OnClickListener {

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_draw_qr_code);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_draw_qr_code, menu);
		return true;
	}

	public void onClick(View v) {
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
		
		int model = BixolonLabelPrinter.QR_CODE_MODEL1;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		if (radioGroup.getCheckedRadioButtonId() == R.id.radio1) {
			model = BixolonLabelPrinter.QR_CODE_MODEL2;
		}
		
		int eccLevel = BixolonLabelPrinter.ECC_LEVEL_7;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio3:
			eccLevel = BixolonLabelPrinter.ECC_LEVEL_15;
			break;
			
		case R.id.radio4:
			eccLevel = BixolonLabelPrinter.ECC_LEVEL_25;
			break;
			
		case R.id.radio5:
			eccLevel = BixolonLabelPrinter.ECC_LEVEL_30;
			break;
		}
		
		editText = (EditText) findViewById(R.id.editText4);
		if(editText.getText().toString().equals("")){
			editText.setText("4");
		}
		int size = Integer.parseInt(editText.getText().toString());
		
		int rotation = BixolonLabelPrinter.ROTATION_NONE;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio7:
			rotation = BixolonLabelPrinter.ROTATION_90_DEGREES;
			break;
			
		case R.id.radio8:
			rotation = BixolonLabelPrinter.ROTATION_180_DEGREES;
			break;
			
		case R.id.radio9:
			rotation = BixolonLabelPrinter.ROTATION_270_DEGREES;
			break;
		}
		
		MainActivity.mBixolonLabelPrinter.drawQrCode(data, horizontalPosition, verticalPosition, model, eccLevel, size, rotation);
		MainActivity.mBixolonLabelPrinter.print(1, 1);
	}
}
