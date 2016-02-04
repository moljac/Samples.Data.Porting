package com.samsung.audiosuite.sapamidisample;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.IBinder;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;

import com.samsung.android.sdk.professionalaudio.widgets.FloatingController;

import java.lang.ref.WeakReference;

public class MainActivity extends Activity
    implements View.OnTouchListener {

	static final String FLOATING_ALIGNMENT_STATE_TAG = "floating_alignment_state_tag";
	private static final String TAG = "audiosuite:sapamidisample:j:MainActivity";

	//Reference to local service.
	WeakReference<MainService> mService;

	//Floating controller.
	FloatingController mFloatingController;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		Log.d(TAG, "onCreate");
		setContentView(R.layout.activity_main);
		super.onCreate(savedInstanceState);
		mService = null;
		//binding to local service.
		bindService(new Intent(this, MainService.class), mConnection,
				0);

		//Views are being set from the layout.

		mFloatingController = (FloatingController)findViewById(R.id.jam_control);
		SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
		int barAlignment = preferences.getInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.getBarAlignment());
		/**This method is used to set floating controller position
		 To set position you must used one of below value:
		 HORIZONTAL POSITION
		 5 - BOTTOM RIGHT
		 6 - BOTTOM LEFT
		 4 - TOP RIGHT
		 7 - TOP LEFT

		 VERTICAL POSITION
		 15 - BOTTOM RIGHT
		 16 - BOTTOM LEFT
		 14 - TOP RIGHT
		 17 - TOP LEFT
		 */
		mFloatingController.loadBarState(barAlignment);
        ViewGroup vg = (ViewGroup) findViewById(R.id.layout_keys);

        int size = vg.getChildCount();
        for (int i = 0; i < size; ++i) {
            View v = vg.getChildAt(i);
            if (v instanceof KeyButton) {
                v.setOnTouchListener(this);
            }
        }

	}
	
    @Override
    protected void onResume() {
        Log.d(TAG, "onResume");
        super.onResume();
    }

	@Override
	protected void onDestroy() {
		Log.d(TAG, "onDestroy");
		unbindService(mConnection);
		SharedPreferences preferences = getPreferences(Context.MODE_PRIVATE);
		preferences.edit()
				.putInt(FLOATING_ALIGNMENT_STATE_TAG, mFloatingController.getBarAlignment())
				.apply();
		super.onDestroy();
	}

    @Override
    public boolean onTouch(View v, MotionEvent ev) {

        KeyButton k = (KeyButton) v;

        try{
        switch (ev.getAction()) {
            case MotionEvent.ACTION_DOWN:
                if(mService != null && mService.get() != null)
                MainActivity.this.mService.get().sendNoteOn(k.getNote(), k.getVelocity());
                break;
            case MotionEvent.ACTION_UP:
                if(mService != null && mService.get() != null)
                MainActivity.this.mService.get().sendNoteOff(k.getNote(), k.getVelocity());
                break;
        }
        }catch(NullPointerException e){
            ;
        }

        return false;
    }

	private ServiceConnection mConnection = new ServiceConnection() {

		@Override
		public void onServiceConnected(ComponentName className, IBinder binder) {
			Log.d(TAG, "onServiceConnected");
			MainActivity.this.mService = new WeakReference<MainService>(((MainService.LocalBinder) binder)
					.getMainService(MainActivity.this));
			//Connection bridge is set to the FloatingController.
			//You can have only one AudioAppConnectionBridge in the application
			//so you need to pass it from service to FloatingController.

			if(mService != null && mService.get() != null && mFloatingController != null){
			    try{
			        MainActivity.this.mFloatingController.setSapaAppService(MainActivity.this.mService.get().getSapaAppService());
			    }catch(NullPointerException e){
                    ;
                }
			}
			
			if(mService != null && mService.get() != null){
			    try{
			        Log.d(TAG, "Connection bridge set " + MainActivity.this.mService.get().getSapaAppService());
			    }catch(NullPointerException e){
                    ;
                }
			}
		}

		@Override
		public void onServiceDisconnected(ComponentName name) {
			Log.d(TAG, "onServiceDisconnected");
			MainActivity.this.mService = null;
		}

	};
}
