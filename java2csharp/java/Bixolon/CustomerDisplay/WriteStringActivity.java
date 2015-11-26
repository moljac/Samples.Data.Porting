package com.bixolon.customerdisplaysample;

import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.RadioGroup;

import com.bixolon.customerdisplay.BixolonCustomerDisplay;

public class WriteStringActivity extends Activity implements OnClickListener {
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_write_string);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_write_string, menu);
		return true;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.getCheckedRadioButtonId()) {
			case R.id.radio0:
				MainActivity.mBcd.setScrollMode(BixolonCustomerDisplay.MODE_OVERWRITE);
				break;
			case R.id.radio1:
				MainActivity.mBcd.setScrollMode(BixolonCustomerDisplay.MODE_VERTICAL);
				break;
			case R.id.radio2:
				MainActivity.mBcd.setScrollMode(BixolonCustomerDisplay.MODE_HORIZONTAL);
				break;
			}
			
			EditText editText = (EditText) findViewById(R.id.editText3);
			String string = editText.getText().toString();
			if (string.length() > 0) {
				int interval = Integer.parseInt(string);
				MainActivity.mBcd.setBlinkInterval(interval);
			}
			
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			switch (radioGroup.getCheckedRadioButtonId()) {
			case R.id.radio3:
				MainActivity.mBcd.setDimmingControl(BixolonCustomerDisplay.BRIGHTNESS_20);
				break;
			case R.id.radio4:
				MainActivity.mBcd.setDimmingControl(BixolonCustomerDisplay.BRIGHTNESS_40);
				break;
			case R.id.radio5:
				MainActivity.mBcd.setDimmingControl(BixolonCustomerDisplay.BRIGHTNESS_60);
				break;
			case R.id.radio6:
				MainActivity.mBcd.setDimmingControl(BixolonCustomerDisplay.BRIGHTNESS_100);
				break;
			}
			
			radioGroup = (RadioGroup) findViewById(R.id.radioGroup3);
			switch (radioGroup.getCheckedRadioButtonId()) {
			case R.id.radio7:
				MainActivity.mBcd.clearLineBlinking(BixolonCustomerDisplay.UPPER_LINE);
				MainActivity.mBcd.clearLineBlinking(BixolonCustomerDisplay.LOWER_LINE);
				break;
			case R.id.radio8:
				MainActivity.mBcd.setLineBlinking(BixolonCustomerDisplay.UPPER_LINE);
				break;
			case R.id.radio9:
				MainActivity.mBcd.setLineBlinking(BixolonCustomerDisplay.LOWER_LINE);
				break;
			}
			
			editText = (EditText) findViewById(R.id.editText1);
			String text = editText.getText().toString();
			MainActivity.mBcd.writeString(text, BixolonCustomerDisplay.UPPER_LINE);
			
			editText = (EditText) findViewById(R.id.editText2);
			text = editText.getText().toString();
			MainActivity.mBcd.writeString(text, BixolonCustomerDisplay.LOWER_LINE);
			break;
			
		case R.id.button2:
			MainActivity.mBcd.clearScreen();
			break;
		}
	}
}
