package com.opentok.android.ui.textchat.sample;

import android.app.FragmentTransaction;
import android.app.NotificationManager;
import android.content.ServiceConnection;
import android.os.Bundle;
import android.support.v4.app.FragmentActivity;
import android.support.v4.app.NotificationCompat;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;

import com.opentok.android.Connection;
import com.opentok.android.OpentokError;
import com.opentok.android.Session;
import com.opentok.android.Stream;
import com.opentok.android.ui.textchat.widget.ChatMessage;
import com.opentok.android.ui.textchat.widget.TextChatFragment;

public class TextChatActivity extends FragmentActivity implements Session.SignalListener, Session.SessionListener, TextChatFragment.TextChatListener {

    private static final String LOGTAG = "demo-text-chat";
    private static final String SIGNAL_TYPE = "TextChat";

    // Replace with a generated Session ID
    public static final String SESSION_ID = "";
    // Replace with a generated token (from the dashboard or using an OpenTok server SDK)
    public static final String TOKEN = "";
    // Replace with your OpenTok API key
    public static final String API_KEY = "";

    private Session mSession;

    private ProgressBar mLoadingBar;

    private boolean resumeHasRun = false;

    private boolean mIsBound = false;
    private NotificationCompat.Builder mNotifyBuilder;
    private NotificationManager mNotificationManager;
    private ServiceConnection mConnection;

    private RelativeLayout mTextChatViewContainer;
    private TextChatFragment mTextChatFragment;

    private FragmentTransaction mFragmentTransaction;

    private boolean msgError = false;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.layout_textchat_activity);

        mLoadingBar = (ProgressBar) findViewById(R.id.load_spinner);
        mLoadingBar.setVisibility(View.VISIBLE);

        sessionConnect();
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case android.R.id.home:
                onBackPressed();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    @Override
    public void onPause() {
        super.onPause();

        if (mSession != null) {
            mSession.onPause();
        }
    }

    @Override
    public void onResume() {
        super.onResume();
        if (mSession != null) {
            mSession.onResume();
        }
    }

    @Override
    public void onStop() {
        super.onStop();
        if (isFinishing() && mSession != null) {
            mSession.disconnect();
        }
    }

    @Override
    public void onDestroy() {
        if (mSession != null) {
            mSession.disconnect();
        }
        super.onDestroy();
        finish();
    }

    @Override
    public void onBackPressed() {
        if (mSession != null) {
            mSession.disconnect();
        }
        super.onBackPressed();
    }

    private void sessionConnect() {
        if (mSession == null) {
            mSession = new Session(TextChatActivity.this,
                    API_KEY, SESSION_ID);
            mSession.setSignalListener(this);
            mSession.setSessionListener(this);
            mSession.connect(TOKEN);
        }
    }

    // Initialize a TextChatFragment instance and add it to the UI
    private void loadTextChatFragment(){
        int containerId = R.id.fragment_textchat_container;
        mFragmentTransaction = getFragmentManager().beginTransaction();
        mTextChatFragment = (TextChatFragment)this.getFragmentManager().findFragmentByTag("TextChatFragment");

        if (mTextChatFragment == null) {
            mTextChatFragment = new TextChatFragment();
            mTextChatFragment.setMaxTextLength(1050);
            mTextChatFragment.setTextChatListener(this);
            mTextChatFragment.setSenderInfo(mSession.getConnection().getConnectionId(), mSession.getConnection().getData());

            mFragmentTransaction.add(containerId, mTextChatFragment, "TextChatFragment").commit();
        }
    }

    @Override
    public boolean onMessageReadyToSend(ChatMessage msg) {
        Log.d(LOGTAG, "TextChat listener: onMessageReadyToSend: " + msg.getText());

        if (mSession != null) {
            mSession.sendSignal(SIGNAL_TYPE, msg.getText());
        }
        return msgError;
    }

    @Override
    public void onSignalReceived(Session session, String type, String data, Connection connection) {
        Log.d(LOGTAG, "onSignalReceived. Type: " + type + " data: " + data);
        ChatMessage msg = null;
        if (!connection.getConnectionId().equals(mSession.getConnection().getConnectionId())) {
            // The signal was sent from another participant. The sender ID is set to the sender's
            // connection ID. The sender alias is the value added as connection data when you
            // created the user's token.
            msg = new ChatMessage(connection.getConnectionId(), connection.getData(), data);
            // Add the new ChatMessage to the text-chat component
            mTextChatFragment.addMessage(msg);
        }
    }


    @Override
    public void onConnected(Session session) {
        Log.d(LOGTAG, "The session is connected.");

        mLoadingBar.setVisibility(View.GONE);
        //loading text-chat ui component
        loadTextChatFragment();
    }

    @Override
    public void onDisconnected(Session session) {
        Log.d(LOGTAG, "The session disconnected.");
    }

    @Override
    public void onError(Session session, OpentokError opentokError) {
        Log.d(LOGTAG, "Session error. OpenTokError: " + opentokError.getErrorCode() + " - " + opentokError.getMessage());
        OpentokError.ErrorCode errorCode = opentokError.getErrorCode();
        msgError = true;
    }

    @Override
    public void onStreamReceived(Session session, Stream stream) {
    }

    @Override
    public void onStreamDropped(Session session, Stream stream) {
    }

}
