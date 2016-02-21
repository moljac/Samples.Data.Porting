package com.sinch.android.rtc.sample.messaging;

import android.app.Service;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;

import com.sinch.android.rtc.*;
import com.sinch.android.rtc.messaging.MessageClientListener;
import com.sinch.android.rtc.messaging.WritableMessage;

public class SinchService extends Service {

    private static final String APP_KEY = "enter-application-key";
    private static final String APP_SECRET = "enter-application-secret";
    private static final String ENVIRONMENT = "sandbox.sinch.com";

    private static final String TAG = SinchService.class.getSimpleName();

    private final SinchServiceInterface mServiceInterface = new SinchServiceInterface();

    private SinchClient mSinchClient = null;
    private StartFailedListener mListener;

    public class SinchServiceInterface extends Binder {

        public boolean isStarted() {
            return SinchService.this.isStarted();
        }

        public void startClient(String userName) {
            start(userName);
        }

        public void stopClient() {
            stop();
        }

        public void setStartListener(StartFailedListener listener) {
            mListener = listener;
        }

        public void sendMessage(String recipientUserId, String textBody) {
            SinchService.this.sendMessage(recipientUserId, textBody);
        }

        public void addMessageClientListener(MessageClientListener listener) {
            SinchService.this.addMessageClientListener(listener);
        }

        public void removeMessageClientListener(MessageClientListener listener) {
            SinchService.this.removeMessageClientListener(listener);
        }
    }

    @Override
    public void onCreate() {
        super.onCreate();
    }

    @Override
    public void onDestroy() {
        if (mSinchClient != null && mSinchClient.isStarted()) {
            mSinchClient.terminate();
        }
        super.onDestroy();
    }

    @Override
    public IBinder onBind(Intent intent) {
        return mServiceInterface;
    }

    private boolean isStarted() {
        return (mSinchClient != null && mSinchClient.isStarted());
    }

    public void sendMessage(String recipientUserId, String textBody) {
        if (isStarted()) {
            WritableMessage message = new WritableMessage(recipientUserId, textBody);
            mSinchClient.getMessageClient().send(message);
        }
    }

    public void addMessageClientListener(MessageClientListener listener) {
        if (mSinchClient != null) {
            mSinchClient.getMessageClient().addMessageClientListener(listener);
        }
    }

    public void removeMessageClientListener(MessageClientListener listener) {
        if (mSinchClient != null) {
            mSinchClient.getMessageClient().removeMessageClientListener(listener);
        }
    }

    private void start(String userName) {
        if (mSinchClient == null) {
            mSinchClient = Sinch.getSinchClientBuilder().context(getApplicationContext()).userId(userName)
                    .applicationKey(APP_KEY)
                    .applicationSecret(APP_SECRET)
                    .environmentHost(ENVIRONMENT).build();

            mSinchClient.setSupportMessaging(true);
            mSinchClient.startListeningOnActiveConnection();

            mSinchClient.addSinchClientListener(new MySinchClientListener());
            mSinchClient.start();
        }
    }

    private void stop() {
        if (mSinchClient != null) {
            mSinchClient.terminate();
            mSinchClient = null;
        }
    }

    public interface StartFailedListener {

        void onStartFailed(SinchError error);

        void onStarted();
    }

    private class MySinchClientListener implements SinchClientListener {

        @Override
        public void onClientFailed(SinchClient client, SinchError error) {
            if (mListener != null) {
                mListener.onStartFailed(error);
            }
            mSinchClient.terminate();
            mSinchClient = null;
        }

        @Override
        public void onClientStarted(SinchClient client) {
            Log.d(TAG, "SinchClient started");
            if (mListener != null) {
                mListener.onStarted();
            }
        }

        @Override
        public void onClientStopped(SinchClient client) {
            Log.d(TAG, "SinchClient stopped");
        }

        @Override
        public void onLogMessage(int level, String area, String message) {
            switch (level) {
                case Log.DEBUG:
                    Log.d(area, message);
                    break;
                case Log.ERROR:
                    Log.e(area, message);
                    break;
                case Log.INFO:
                    Log.i(area, message);
                    break;
                case Log.VERBOSE:
                    Log.v(area, message);
                    break;
                case Log.WARN:
                    Log.w(area, message);
                    break;
            }
        }

        @Override
        public void onRegistrationCredentialsRequired(SinchClient client,
                ClientRegistration clientRegistration) {
        }
    }
}
