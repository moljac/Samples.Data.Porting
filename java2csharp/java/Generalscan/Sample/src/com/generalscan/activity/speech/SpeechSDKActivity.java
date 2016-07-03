package com.generalscan.activity.speech;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnKeyListener;
import android.view.Window;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;

import com.generalscan.sdk.R;
import com.generalscan.speech.OnFinishSpeech;
import com.generalscan.speech.TTSInterface;

public class SpeechSDKActivity extends Activity {

	private Button myBtnSpeekContent;// 发音内容
	private EditText myEdtName;
	private EditText myEdtBarcode;
	private EditText myEdtCount;

	private CheckBox myCheckBoxSpeek;

	private Button myBtnSettings;
	private Button myBtnClean;
	private Context myContext;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);
		setContentView(R.layout.speech_main);
		myContext = this;
		// 查找控件
		findViewByid();
		// 设置button事件
		setListener();

	}

	private void findViewByid() {

		myBtnSpeekContent = (Button) findViewById(R.id.speak);
		myEdtName = (EditText) findViewById(R.id.edtName);
		myEdtBarcode = (EditText) findViewById(R.id.edtBarcode);
		myEdtCount = (EditText) findViewById(R.id.edtCount);
		myBtnSettings = (Button) findViewById(R.id.btnSetting);
		myBtnClean = (Button) findViewById(R.id.btnClear);
		myCheckBoxSpeek = (CheckBox) findViewById(R.id.checkBox1);
	}

	private void setListener() {

		// 配置发音内容
		myBtnSpeekContent.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {

				Intent intent = new Intent(myContext, SettingsSpeech.class);

				myContext.startActivity(intent);
			}

		});
		// 显示系统设置TTS界面
		myBtnSettings.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				TTSInterface.TTSSettings();
			}

		});

		// 播放完指定内容之后监听的信息
		TTSInterface.SetOnFinishSpeech(new OnFinishSpeech() {

			@Override
			public void FinishSpeech(String finishContent) {

				// if (finishContent.equals(String.valueOf(R.id.edtName))) {
				// myEdtName.setText("");
				// }
				// if (finishContent.equals(String.valueOf(R.id.edtBarcode))) {
				// myEdtBarcode.setText("");
				// }
				// if (finishContent.equals(String.valueOf(R.id.edtCount))) {
				// myEdtCount.setText("");
				// }
			}

		});

		// 设置按回车键后，发音内容
		myEdtName.setOnKeyListener(myOnKeyListener);
		myEdtBarcode.setOnKeyListener(myOnKeyListener);
		myEdtCount.setOnKeyListener(myOnKeyListener);

		myBtnClean.setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View arg0) {

				myEdtName.setText("");

				myEdtBarcode.setText("");

				myEdtCount.setText("");

			}

		});
	}

	/**
	 * 按键监听器
	 */
	private OnKeyListener myOnKeyListener = new OnKeyListener() {

		@Override
		public boolean onKey(View v, int keyCode, KeyEvent event) {
			// 判断是否发音
			if (myCheckBoxSpeek.isChecked()) {
				if (keyCode == KeyEvent.KEYCODE_ENTER) {
					if (event.getAction() == KeyEvent.ACTION_DOWN) {
						String speech = "";
						switch (v.getId()) {
						case R.id.edtName:
							speech = DataShare.getName(myContext);

							TTSInterface.Play(
									Replace(speech, myEdtName.getText()
											.toString()), String
											.valueOf(R.id.edtName));

							myEdtBarcode.requestFocus();

							break;
						case R.id.edtBarcode:
							speech = DataShare.getBarcode(myContext);

							TTSInterface.Play(
									Replace(speech, myEdtBarcode.getText()
											.toString()), String
											.valueOf(R.id.edtBarcode));
							myEdtCount.requestFocus();
							break;
						case R.id.edtCount:
							speech = DataShare.getCount(myContext);

							TTSInterface.Play(
									Replace(speech, myEdtCount.getText()
											.toString()), String
											.valueOf(R.id.edtCount));
							myEdtName.requestFocus();
							break;
						}

						return true;

					}
					return true;
				}
			}
			return false;
		}
	};
	/**
	 * 更换对应的内容
	 * @param speech
	 * @param newString
	 * @return
	 */
	private String Replace(String speech, String newString) {
		try {
			speech = speech.replace("?", newString);
		} catch (Exception ex) {

		}
		return speech;
	}

	/**
	 * 开启TTS服务，在onResume中设置
	 */
	@Override
	protected void onResume() {
		TTSInterface.StartTTS(this); // 校验TTS引擎安装及资源状态
		super.onResume();
	}
	/**
	 * 设置ActivityResult(必须的)
	 */
	@Override
	protected void onActivityResult(int requestCode, int resultCode, Intent data) {
		TTSInterface.TTSActivityResult(requestCode, resultCode);
	}
	/**
	 * 停止TTS服务
	 */
	@Override
	protected void onDestroy() {
		TTSInterface.Stop();
		super.onDestroy();
	}

	
}