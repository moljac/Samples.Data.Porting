package com.samsung.android.sdk.professionalaudio.sample.simpleclient;

import android.app.Activity;
import android.os.Bundle;
import android.util.AndroidRuntimeException;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaService;

public class SapaSimpleClientActivity extends Activity {
	
	private final static String TAG = "SapaSimpleClient"; 

	Button mPlayButton, mActivateButton, mDeactivateButton;

	SapaService mService;
	SapaProcessor mClient;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_apasimple_client);
		
		try {
			Sapa sapa = new Sapa();
			sapa.initialize(this);
			mService = new SapaService();
			mService.stop(true);
			mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
			mClient = new SapaProcessor(this, null, new SapaProcessor.StatusListener() {
				
				@Override
				public void onKilled() {
					Log.v(TAG, "SapaSimpleClient will be closed. because of the SapaProcessor was closed.");
					mService.stop(true);
					finish();
				}
			});
			mService.register(mClient);
			
		} catch (SsdkUnsupportedException e) {
			e.printStackTrace();
			Toast.makeText(this, "not support professional audio",
					Toast.LENGTH_LONG).show();
			finish();
			return;
		} catch (InstantiationException e) {
			e.printStackTrace();
			Toast.makeText(this, "fail to instantiate", Toast.LENGTH_LONG)
					.show();
			finish();
			return;
		} catch (AndroidRuntimeException e){
			e.printStackTrace();
			Toast.makeText(this, "fail to start", Toast.LENGTH_LONG)
					.show();
			finish();
			return;
		}

		mPlayButton = (Button) this.findViewById(R.id.play_button);
		mPlayButton.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				if (mClient != null) {
					mClient.activate();
					mPlayButton.setText(R.string.playing_sound);
					mPlayButton.setEnabled(false);
				}
			}
		});

		mActivateButton = (Button) this.findViewById(R.id.button_activate);
		mActivateButton.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				if (mClient != null)
					mClient.activate();
			}
		});

		mDeactivateButton = (Button) this.findViewById(R.id.button_deactivate);
		mDeactivateButton.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				if (mClient != null)
					mClient.deactivate();
			}
		});
	}

	@Override
	protected void onDestroy() {
		super.onDestroy();
		if(mService != null){
			if(mClient != null){
				mService.unregister(mClient);
			}
			mService.stop(true);
		}
	}
}
