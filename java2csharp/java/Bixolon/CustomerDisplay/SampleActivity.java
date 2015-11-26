package com.bixolon.customerdisplaysample;

import java.util.Calendar;

import android.app.Activity;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;

import com.bixolon.customerdisplay.BixolonCustomerDisplay;

public class SampleActivity extends Activity implements OnClickListener {
	private BixolonCustomerDisplay mBcd = MainActivity.mBcd;
	
	private Button mOpenButton;
	private Button mCloseButton;
	private LinearLayout mLinearLayout;
	private EditText mWriteString1;
	private EditText mWriteString2;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_sample);
		
		mOpenButton = (Button) findViewById(R.id.button1);
		mCloseButton = (Button) findViewById(R.id.button2);
		mLinearLayout = (LinearLayout) findViewById(R.id.linearLayout1);
//		setEnableView(mLinearLayout, false);
		
		mWriteString1 = (EditText) findViewById(R.id.editText1);
		mWriteString2 = (EditText) findViewById(R.id.editText2);
		
		mOpenButton.setOnClickListener(this);
		mCloseButton.setOnClickListener(this);
		Button button = (Button) findViewById(R.id.button3);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button4);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button5);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button6);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button7);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button8);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button9);
		button.setOnClickListener(this);
	}
	
//	@Override
//	public void onDestroy() {
//		closeDisplay();
//		super.onDestroy();
//	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;
	}
	
	private void setEnableView(View view, boolean enabled) {
		if (view instanceof ViewGroup) {
			ViewGroup viewGroup = (ViewGroup) view;
			int childCount = viewGroup.getChildCount();
			
			for (int i = 0; i < childCount; i++) {
				View childView = viewGroup.getChildAt(i);
				if (childView instanceof ViewGroup) {
					setEnableView(childView, enabled);
				} else {
					viewGroup.getChildAt(i).setEnabled(enabled);
				}
			}
		}
	}
	
	private void readyToDisplay() {
		mBcd.selectPeripheralDevices(true);
		mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_USA);
		mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_437_STANDARD_EUROPEAN);
		mBcd.setScrollMode(BixolonCustomerDisplay.MODE_HORIZONTAL);
		mBcd.setCursor(false);
		mBcd.setDimmingControl(BixolonCustomerDisplay.BRIGHTNESS_100);
	}
	
	private void closeDisplay() {
		if (mBcd != null) {
			readyToDisplay();
			mBcd.clearScreen();
			mBcd.selectPeripheralDevices(false);
			mBcd.close();
		}
	}
	
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			mBcd = new BixolonCustomerDisplay(SampleActivity.this);
			if (mBcd.open()) {
				setEnableView(mLinearLayout, true);
				mOpenButton.setEnabled(false);
				mCloseButton.setEnabled(true);
			} else {
				mBcd = null;
			}
			break;
			
		case R.id.button2:
			closeDisplay();
			setEnableView(mLinearLayout, false);
			mOpenButton.setEnabled(true);
			mCloseButton.setEnabled(false);
			break;
			
		case R.id.button3:	// 1st line display
			readyToDisplay();
			mBcd.setBlinkInterval(0);
			mBcd.setReversedCharacterMode(false);
			mBcd.moveCursor(1, 1);
			mBcd.writeString(mWriteString1.getText().toString());
			mBcd.selectPeripheralDevices(false);
			break;
			
		case R.id.button4:	// clear 1st line
			readyToDisplay();
			mBcd.moveCursor(1, 20);
			mBcd.clearLine();
			mBcd.selectPeripheralDevices(false);
			break;
			
		case R.id.button5:	// 2nd line display
			readyToDisplay();
			mBcd.setBlinkInterval(0);
			mBcd.setReversedCharacterMode(false);
			mBcd.moveCursor(2, 1);
			mBcd.writeString(mWriteString2.getText().toString());
			mBcd.selectPeripheralDevices(false);
			break;
			
		case R.id.button6:	// clear 2nd line
			readyToDisplay();
			mBcd.moveCursor(2, 20);
			mBcd.clearLine();
			mBcd.selectPeripheralDevices(false);
			break;
			
		case R.id.button7:	// time
			readyToDisplay();
			Calendar c = Calendar.getInstance();
			int h = c.get(Calendar.HOUR_OF_DAY);
			int m = c.get(Calendar.MINUTE);
			mBcd.setTime(h, m);
			mBcd.selectPeripheralDevices(false);
			break;
			
		case R.id.button8:	// macro
			readyToDisplay();
			mBcd.startMacroDefinition();
			mBcd.clearScreen();
			mBcd.setBlinkInterval(0);
			mBcd.moveCursor(1, 1);
			mBcd.writeString("Save your money");
			mBcd.moveCursor(2, 1);
			mBcd.writeString(" with BIXOLON");
			mBcd.setBlinkInterval(10);
			mBcd.endMacroDefinition();
			mBcd.executeDefinedMacro(5, 60);
			break;
			
		case R.id.button9:	// clear screen
			readyToDisplay();
			mBcd.clearScreen();
			mBcd.selectPeripheralDevices(false);
			break;
		}
	}
}
