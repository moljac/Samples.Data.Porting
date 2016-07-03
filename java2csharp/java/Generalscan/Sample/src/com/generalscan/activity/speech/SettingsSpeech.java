package com.generalscan.activity.speech;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.Window;
import android.widget.Button;
import android.widget.EditText;

import com.generalscan.sdk.R;

public class SettingsSpeech extends Activity {
	
	private Button myBtnBack;// 发音内容
	private EditText myEdtName;
	private EditText myEdtBarcode;
	private EditText myEdtCount;

	private Activity myActivity;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.speech_content);
		myActivity = this;
		//查找控件
		findViewByid();
		// 设置button事件
		setListener();
		//
		setValue();
	}
	
	private void findViewByid() {

		myBtnBack = (Button) findViewById(R.id.btnBack);
		myEdtName = (EditText) findViewById(R.id.edtName);
		myEdtBarcode = (EditText) findViewById(R.id.edtBarcode);
		myEdtCount = (EditText) findViewById(R.id.edtCount); 


	}

	private void setListener() {
		myBtnBack.setOnClickListener(new OnClickListener()
		{

			@Override
			public void onClick(View arg0) {
				DataShare.SetName(myActivity, myEdtName.getText().toString());
				DataShare.SetBarcode(myActivity, myEdtBarcode.getText().toString());
				DataShare.SetCount(myActivity, myEdtCount.getText().toString());
				myActivity.finish();
			}
			
		});
	}
	private void setValue() {
		myEdtName.setText(DataShare.getName(myActivity));
		myEdtBarcode.setText(DataShare.getBarcode(myActivity));
		myEdtCount.setText(DataShare.getCount(myActivity));
		
	}
}
