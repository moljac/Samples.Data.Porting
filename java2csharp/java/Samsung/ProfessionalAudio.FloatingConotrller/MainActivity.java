package com.example.floatingcontrollersample;

import com.samsung.android.sdk.professionalaudio.widgets.FloatingController;
import com.samsung.android.sdk.professionalaudio.app.SapaActionDefinerInterface;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;

import android.app.Activity;
import android.os.Bundle;

public class MainActivity extends Activity implements SapaActionDefinerInterface {

    // Floating controller.
    FloatingController mFloatingController;

    SapaServiceConnectListener mListener = new SapaServiceConnectListener() {

        @Override
        public void onServiceConnected() {
            mFloatingController.setSapaAppService(mBridge);
        }

		@Override
		public void onServiceDisconnected() {
			
		}
    };

    SapaAppService mBridge;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        mBridge = new SapaAppService(this);
        mBridge.addConnectionListener(mListener);
        mBridge.connect();
        mFloatingController = ((FloatingController) findViewById(R.id.jam_control));
    }
    
    @Override
    protected void onDestroy() {
        mBridge.removeConnectionListener(mListener);
        mBridge.disconnect();
        super.onDestroy();
    }


	@Override
	public Runnable getActionDefinition(SapaApp arg0, String arg1) {
		return null;
	}

}
