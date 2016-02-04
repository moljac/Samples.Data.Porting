package com.samsung.android.sdk.professionalaudio.sample.simplepluginclient;

import java.util.Iterator;
import java.util.List;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaPluginInfo;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaService;

public class SapaSimplePluginClient extends Activity {

	private SapaService mService;
	private SapaProcessor mProcessor;

	private static final String TAG = "SapaSimplePluginClient";
	
	@Override
	protected void onDestroy() {
		
		super.onDestroy();
		if(mService != null){
			if(mProcessor != null){
				mProcessor.deactivate();
				mService.unregister(mProcessor);
			}
			mService.stop(false);
		}
	}

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_sapa_plugin_simple_client);

		try {
			Sapa sapa = new Sapa();
			sapa.initialize(this);
			mService = new SapaService();
			mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);

		} catch (SsdkUnsupportedException e) {
			Toast.makeText(this, "Not support professional audio package", Toast.LENGTH_LONG).show();
			e.printStackTrace();
			finish();
			return;
		} catch (InstantiationException e) {
			Toast.makeText(this, "fail to instantiation SapaService", Toast.LENGTH_LONG).show();
			e.printStackTrace();
			finish();
			return;
		}

		((Button) findViewById(R.id.load_button)).setEnabled(true);
		
		((Button) findViewById(R.id.load_button)).setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				((TextView) findViewById(R.id.load_status_text)).setText("Loading... ");
				List<SapaPluginInfo> pluginList = mService.getAllPlugin();
				Iterator<SapaPluginInfo> iter = pluginList.iterator();
				while (iter.hasNext()) {
					SapaPluginInfo info = iter.next();
					if (info.getName().contentEquals("SapaSimplePlugin") == true) {
						// load SapaSimplePlugin
						try {
							mProcessor = new SapaProcessor(v.getContext(), info, new SapaProcessor.StatusListener() {
								
								@Override
								public void onKilled() {
									Log.v(TAG, "SapaSimplePluginClient will be closed. because of the SapaProcessor was closed.");
									mService.stop(true);
									finish();
								}
							});
							mService.register(mProcessor);
							((TextView) findViewById(R.id.load_status_text)).setText("Loading SUCCESS ");
							((Button) findViewById(R.id.activate_button)).setEnabled(true);
							((Button) findViewById(R.id.load_button)).setEnabled(false);
							return;
						} catch (InstantiationException e) {
							Toast.makeText(v.getContext(), "Fail to register SapaProcessor", Toast.LENGTH_LONG).show();
							((TextView) findViewById(R.id.load_status_text)).setText("Loading FAIL ");
							e.printStackTrace();
						}
						break;
					}
				}
				((TextView) findViewById(R.id.load_status_text)).setText("Loading FAIL ");
			}
		});

		((Button) findViewById(R.id.activate_button)).setEnabled(false);
		((Button) findViewById(R.id.activate_button)).setOnClickListener(new OnClickListener() {

			@Override
			public void onClick(View v) {
				if (mProcessor != null) {
					mProcessor.activate();
				}
			}
		});
	}
}
