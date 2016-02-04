package com.samsung.audiosuite.sapamidisample;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;
import android.util.SparseArray;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaProcessor;
import com.samsung.android.sdk.professionalaudio.SapaService;
import com.samsung.android.sdk.professionalaudio.app.SapaActionDefinerInterface;
import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;

import java.nio.ByteBuffer;

public class MainService extends Service implements
		SapaServiceConnectListener, SapaActionDefinerInterface {

	private static final String TAG = "audiosuite:sapamidisample:j:MainService";

	//Commands for communication with native part
    private static final String COMMAND_SHUSH = "cmd:shush";

	//Binder for activity
	private LocalBinder mBinder;

	private SapaAppService mSapaAppService;

	//SapaAppInfo package representing this application
	private SapaAppInfo mMyInfo;

	//Reference to the activity of application
	//private WeakReference<MainActivity> mActivity;

	private SapaService mSapaService;
	private SapaProcessor mSapaProcessor;

    //Only note on/off that is why 3 bytes allocated
    private ByteBuffer mMidiEvent = ByteBuffer.allocateDirect(3);

	SparseArray<SapaActionInfo> mActionArray;
	
	private String mCallerPackageName;

	@Override
	public void onCreate() {
		Log.d(TAG, "onCreate");
		super.onCreate();
		mBinder = new LocalBinder();
		this.mSapaAppService = null;
	}

	@Override
	public int onStartCommand(Intent intent, int flags, int startId) {
		if (intent != null) {
			//When application is activated from ProfessionalAudio system it receives SapaAppInfo object describing it.
			//To obtain this object static method getSapaAppInfo() is to be used.
			SapaAppInfo info = SapaAppInfo.getAppInfo(intent);
			mCallerPackageName = intent.getStringExtra("com.samsung.android.sdk.professionalaudio.key.callerpackagename");
			//Toast.makeText(this, mCallerPackageName, Toast.LENGTH_SHORT).show();
			if (info != null) {
				//Received SapaAppInfo is saved to class field.
				mMyInfo = info;
			}
		}

		//Connection to remote service of ProfessionalAudio system is set.
		if(mSapaAppService == null){
			connectConnectionBridge();
		}

		return super.onStartCommand(intent, flags, startId);
	}

	@Override
	public IBinder onBind(Intent intent) {
		Log.d(TAG, "onBind");
		//Binder for local activity.
		return mBinder;
	}

	@Override
	public void onDestroy() {
		Log.d(TAG, "onDestroy");

		//Native part of application is being deactivated.
		if (mSapaService != null && mSapaProcessor != null) {
			mSapaProcessor.deactivate();
			mSapaService.unregister(mSapaProcessor);
			this.mSapaProcessor = null;
		}

		if(this.mSapaAppService != null){
    		try {
    		    if (this.mMyInfo!=null && this.mMyInfo.getApp() != null) {
    			//Application needs to declare that it was successfully deactivated.
    			this.mSapaAppService.removeFromActiveApps(this.mMyInfo
    					.getApp());
    		    }
    
    			//Action definer is being removed.
    			this.mSapaAppService.removeActionDefiner();
    		} catch (SapaConnectionNotSetException e) {
    			Log.e(TAG,
    					"Instance could not be removed from active list because of connection exception.");
    		}
    		//Connection with remote service is finished.
    		this.mSapaAppService.disconnect();
    		this.mSapaAppService = null;
		}
		
		this.mMyInfo = null;
	

		super.onDestroy();
	}

	@Override
	public void onServiceConnected() {
		Log.d(TAG, "onServiceConnected");
		try {
		    if(this.mMyInfo == null){
		        mMyInfo = mSapaAppService.getInstalledApp(this.getPackageName());
		    }
			if (mMyInfo != null) {

				//Actions are being set in SapaAppInfo representing this application.
				mActionArray = new SparseArray<SapaActionInfo>();
                mActionArray.put(0, new SapaActionInfo(
                            COMMAND_SHUSH,
                            R.drawable.ctrl_btn_stop_default,
                            getPackageName()));
				mMyInfo.setActions(mActionArray);

				//Native part is being initialised.
				Sapa sapa = new Sapa();
				sapa.initialize(this);
				mSapaService = new SapaService();
				mSapaProcessor = new SapaProcessor(this, null, new SapaProcessorStateListener(mMyInfo.getApp()));
				mSapaService.register(mSapaProcessor);
				mSapaProcessor.activate();

				//Information about ports is being set in SapaAppInfo representing this app.
				//It can not be done before activating SapaProcessor.
				mMyInfo.setPortFromSapaProcessor(mSapaProcessor);

				//Application needs to declare that it was successfully activated.
				if(mSapaAppService != null){
				    this.mSapaAppService.addActiveApp(this.mMyInfo);
				}
			}
		} catch (SapaConnectionNotSetException e) {
			Log.e(TAG,
					"App could not be added to active as connection has not been made.");
		} catch (IllegalArgumentException e) {
			Log.e(TAG, "Initialisation of Sapa is not possible due to invalid context of application");
		} catch (SsdkUnsupportedException e) {
			Log.e(TAG, "Initialisation of Sapa is not possible as Sapa is not available on the device");
		} catch (InstantiationException e) {
			Log.e(TAG, "SapaService can not be instantiate");
		}
	}

	@Override
	public Runnable getActionDefinition(SapaApp sapaApp, String actionId) {
		Log.d(TAG, "getActionDefinition: actionId=" + actionId);
        if (actionId.equals(COMMAND_SHUSH)) {
            return new Runnable() {
                @Override
                public void run() {
                    MainService.this.shush();
                }
            };
        }
        return null;
	}

	SapaAppService getSapaAppService(){
		return mSapaAppService;
	}

	private void connectConnectionBridge() {
		Log.d(TAG, "Connect using bridge");
		mSapaAppService = new SapaAppService(this);

		//MainService starts listening for establishment of connection.
		//MainService needs to implement AudioServiceConnectListener to be able to listen for this event.
		//When event occurs onServiceConnected() method is called.
		mSapaAppService.addConnectionListener(this);

		//MainService declares that it defines actions for this application.
		//MainService needs to implement AudioActionDefinerInterface.
		mSapaAppService.setActionDefiner(this);
		
		mSapaAppService.addAppStateListener(mSapaAppStateListener);

		//Connection to AudioConnectionService starts being created.
		mSapaAppService.connect();
	}
	
	private SapaAppStateListener mSapaAppStateListener = new SapaAppStateListener() {
        
        @Override
        public void onTransportMasterChanged(SapaApp arg0) {
            // TODO Auto-generated method stub
            
        }
        
        @Override
        public void onAppUninstalled(SapaApp arg0) {
            // TODO Auto-generated method stub
            
        }
        
        @Override
        public void onAppInstalled(SapaApp arg0) {
            // TODO Auto-generated method stub
            
        }
        
        @Override
        public void onAppDeactivated(SapaApp sapaApp) {
            Log.d(TAG, "onAppDeactivated: " + sapaApp.getInstanceId() + " was deactivated.");
            if(sapaApp.getPackageName().contentEquals(mCallerPackageName)){
                Log.d(TAG, "Caller is closed.");
                stopSelf();
            }
            
        }
        
        @Override
        public void onAppChanged(SapaApp arg0) {
        }
        
        @Override
        public void onAppActivated(SapaApp arg0) {
            // TODO Auto-generated method stub
            
        }
    };


    void sendNoteOn(int note, int velocity) {
        mMidiEvent.put(0, (byte) 0x90);
        mMidiEvent.put(1, (byte) note);
        mMidiEvent.put(2, (byte) velocity);
        //mSapaProcessor.sendCommand(new String(mMidiEvent.array()));
        mSapaProcessor.sendStream(mMidiEvent, 0x90);
    }

    void sendNoteOff(int note, int velocity) {
        mMidiEvent.put(0, (byte) 0x80);
        mMidiEvent.put(1, (byte) note);
        mMidiEvent.put(2, (byte) velocity);
        //mSapaProcessor.sendCommand(new String(mMidiEvent.array()));
        mSapaProcessor.sendStream(mMidiEvent, 0x80);
    }

    void shush() {
        mSapaProcessor.sendCommand(COMMAND_SHUSH);
    }
    
    class SapaProcessorStateListener implements SapaProcessor.StatusListener{
        
        private SapaApp mSapaApp;
        
        public SapaProcessorStateListener(SapaApp sapaApp){
            mSapaApp = sapaApp;
        }

        @Override
        public void onKilled() {
            Log.d(TAG, mSapaApp.getInstanceId() + " was killed.");
            
            
            try {
                mSapaProcessor = null;
                
                // remove from active apps // This will notify to the Soundcamp.
                mSapaAppService.removeFromActiveApps(mSapaApp);
                
                MainService.this.stopSelf();
                
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
        
    }

	class LocalBinder extends Binder {

		MainService getMainService(MainActivity activity) {
			Log.d(TAG, "getMainService");
			//MainService.this.mActivity = new WeakReference<MainActivity>(activity);
			return MainService.this;
		}
	}

	@Override
	public void onServiceDisconnected() {
		Log.d(TAG, "Nothing to be done");
	}
}
