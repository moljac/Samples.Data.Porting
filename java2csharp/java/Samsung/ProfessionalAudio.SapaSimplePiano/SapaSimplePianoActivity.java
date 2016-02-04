package com.samsung.android.sdk.professionalaudio.sample.simplepiano;

import android.app.Activity;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.Toast;

import android.content.res.AssetManager;
import android.os.Environment;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.OutputStream;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaService;

public class SapaSimplePianoActivity extends Activity {
	
	private SapaService mService;
	private SapaProcessor mProcessor;
	private static final String TAG = "SapaSimplePiano";

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_sapa_simple_piano);
		
		try{
			(new Sapa()).initialize(this);
			mService = new SapaService();
			mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
			mProcessor = new SapaProcessor(this, null, new SapaProcessor.StatusListener() {
						
				@Override
				public void onKilled() {
					Log.v(TAG, "SapaSimplePiano will be closed. because of the SapaProcessor was closed.");
					mService.stop(true);
					finish();
				}
			});
			mService.register(mProcessor);

			// copy sound font file to sdcard.
			copyAssets();

			mProcessor.sendCommand("START");
			mProcessor.activate();
			
		}catch (SsdkUnsupportedException e){
			e.printStackTrace();
			Toast.makeText(this, "Not support Professional Audio package", Toast.LENGTH_LONG).show();
			finish();
			return;
		}catch (IllegalArgumentException e){
			e.printStackTrace();
			Toast.makeText(this, "Error - invalid arguments. please check the log", Toast.LENGTH_LONG).show();
			finish();
			return;
		}catch (InstantiationException e){
			e.printStackTrace();
			Toast.makeText(this, "Error. please check the log", Toast.LENGTH_LONG).show();
			finish();
			return;
		}

		((Button)findViewById(R.id.play_sound_c1)).setOnClickListener(new OnClickListener() {
			
			@Override
			public void onClick(View v) {
				// play sound c3
				final int noteC3 = 60;
				final int velocity = 90;
				mProcessor.sendCommand(String.format("PLAY %d %d", noteC3, velocity));
			}
		});
	}

	@Override
	protected void onDestroy() {
		super.onDestroy();
		if(mService != null){
			if(mProcessor != null){
				mService.unregister(mProcessor);
				mProcessor = null;
			}
			mService.stop(false);
		}
	}

	private void copyAssets() {
		final String mSoundFontDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS).toString() + "/";
		String[] files = null;
		String mkdir = null ;
		AssetManager assetManager = getAssets();
		try {
			files = assetManager.list("");
		}
		catch (IOException e) {
			Log.e(TAG, e.getMessage());
			e.printStackTrace();
			return;
		}

		for(int i=0; i<files.length; i++)	{
			InputStream in = null;
			OutputStream out = null;
			try	{
				File tmp = new File(mSoundFontDir, files[i]);
				if(!tmp.exists()) {
					in = assetManager.open(files[i]);
				}

				if(in == null)	{
					continue;
				}

				mkdir = mSoundFontDir;
				File mpath = new File(mkdir);

				if(! mpath.isDirectory()) {
					mpath.mkdirs();
				}

				out = new FileOutputStream(mSoundFontDir + files[i]);

				final byte[] buffer = new byte[1024];
				int read;

				while((read = in.read(buffer)) != -1) {
					out.write(buffer, 0, read);
				}

				in.close();
				in = null;
				out.flush();
				out.close();
				out = null;
			}
			catch(Exception e) {
				e.printStackTrace();
			}
		}
	}

}
