package com.tokbox.android.opentokrtc;

import java.net.MalformedURLException;
import java.net.URI;
import java.net.URISyntaxException;
import java.net.URL;

import org.apache.http.HttpEntity;
import org.apache.http.HttpResponse;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpGet;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.util.EntityUtils;
import org.json.JSONObject;

import android.app.ActionBar;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.app.ProgressDialog;
import android.content.ComponentName;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.res.Configuration;
import android.graphics.BitmapFactory;
import android.media.AudioManager;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.support.v4.app.NotificationCompat;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.view.WindowManager;
import android.view.animation.AlphaAnimation;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ProgressBar;
import android.widget.RelativeLayout;
import android.widget.RelativeLayout.LayoutParams;
import android.widget.ScrollView;
import android.widget.TextView;

import com.opentok.android.SubscriberKit;
import com.tokbox.android.opentokrtc.fragments.PublisherControlFragment;
import com.tokbox.android.opentokrtc.fragments.PublisherStatusFragment;
import com.tokbox.android.opentokrtc.fragments.SubscriberControlFragment;
import com.tokbox.android.opentokrtc.fragments.SubscriberQualityFragment;
import com.tokbox.android.opentokrtc.services.ClearNotificationService;
import com.tokbox.android.opentokrtc.services.ClearNotificationService.ClearBinder;
import com.tokbox.android.ui.AudioLevelView;

public class ChatRoomActivity extends Activity implements
        SubscriberControlFragment.SubscriberCallbacks,
        PublisherControlFragment.PublisherCallbacks {

    private static final String LOGTAG = "ChatRoomActivity";

    private static final int ANIMATION_DURATION = 500;
    public static final String ARG_ROOM_ID = "roomId";
    public static final String ARG_USERNAME_ID = "usernameId";

    private String serverURL = null;
    private String mRoomName;
    private Room mRoom;
    private String mUsername = null;
    private boolean mSubscriberVideoOnly = false;
    private boolean mArchiving = false;

    private ProgressDialog mConnectingDialog;
    private AlertDialog mErrorDialog;
    private EditText mMessageEditText;
    private ViewGroup mPreview;
    private ViewPager mParticipantsView;
    private ImageView mLeftArrowImage;
    private ImageView mRightArrowImage;
    private ProgressBar mLoadingSub; // Spinning wheel for loading subscriber view
    private RelativeLayout mSubscriberAudioOnlyView;
    private RelativeLayout mMessageBox;
    private AudioLevelView mAudioLevelView;

    protected SubscriberControlFragment mSubscriberFragment;
    protected PublisherControlFragment mPublisherFragment;
    protected PublisherStatusFragment mPublisherStatusFragment;
    protected SubscriberQualityFragment mSubscriberQualityFragment;

    protected Handler mHandler = new Handler();
    private NotificationCompat.Builder mNotifyBuilder;
    private NotificationManager mNotificationManager;
    private ServiceConnection mConnection;
    private boolean mIsBound = false;

    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        this.getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);

        getWindow().addFlags(WindowManager.LayoutParams.FLAG_KEEP_SCREEN_ON);

        setContentView(R.layout.room_layout);

        //Custom title bar
        ActionBar actionBar = getActionBar();
        actionBar.setDisplayShowTitleEnabled(false);
        actionBar.setDisplayUseLogoEnabled(false);
        actionBar.setDisplayHomeAsUpEnabled(true);
        actionBar.setDisplayShowCustomEnabled(true);

        View cView = getLayoutInflater().inflate(R.layout.custom_title, null);
        actionBar.setCustomView(cView);

        mMessageBox = (RelativeLayout) findViewById(R.id.messagebox);
        mMessageEditText = (EditText) findViewById(R.id.message);

        mPreview = (ViewGroup) findViewById(R.id.publisherview);
        mParticipantsView = (ViewPager) findViewById(R.id.pager);
        mLeftArrowImage = (ImageView) findViewById(R.id.left_arrow);
        mRightArrowImage = (ImageView) findViewById(R.id.right_arrow);
        mSubscriberAudioOnlyView = (RelativeLayout) findViewById(R.id.audioOnlyView);
        mLoadingSub = (ProgressBar) findViewById(R.id.loadingSpinner);

        //Initialize
        mAudioLevelView = (AudioLevelView) findViewById(R.id.subscribermeter);
        mAudioLevelView.setIcons(BitmapFactory.decodeResource(getResources(),
                R.drawable.headset));

        Uri url = getIntent().getData();
        serverURL = getResources().getString(R.string.serverURL);

        if (url == null) {
            mRoomName = getIntent().getStringExtra(ARG_ROOM_ID);
            mUsername = getIntent().getStringExtra(ARG_USERNAME_ID);
        } else {
            mRoomName = url.getPathSegments().get(0);
        }

        TextView title = (TextView) findViewById(R.id.title);
        title.setText(mRoomName);

        if (savedInstanceState == null) {
            initSubscriberFragment();
            initPublisherFragment();
            initPublisherStatusFragment();
            initSubscriberQualityFragment();
        }

        mNotificationManager =
                (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
        initializeRoom();
    }

    @Override
    public void onConfigurationChanged(Configuration newConfig) {
        super.onConfigurationChanged(newConfig);

        // Remove publisher & subscriber views because we want to reuse them
        if (mRoom != null && mRoom.getCurrentParticipant() != null) {
            mRoom.getParticipantsViewContainer()
                    .removeView(mRoom.getCurrentParticipant().getView());
        }
        reloadInterface();
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

        //Pause implies go to audio only mode
        if (mRoom != null) {
            mRoom.onPause();

            if (mRoom != null && mRoom.getCurrentParticipant() != null) {
                mRoom.getParticipantsViewContainer()
                        .removeView(mRoom.getCurrentParticipant().getView());
            }
        }

        //Add notification to status bar which gets removed if the user force kills the application.
        mNotifyBuilder = new NotificationCompat.Builder(this)
                .setContentTitle("OpenTokRTC")
                .setContentText("Ongoing call")
                .setSmallIcon(R.drawable.ic_launcher).setOngoing(true);

        Intent notificationIntent = new Intent(this, ChatRoomActivity.class);
        notificationIntent
                .setFlags(Intent.FLAG_ACTIVITY_CLEAR_TOP | Intent.FLAG_ACTIVITY_SINGLE_TOP);
        notificationIntent.putExtra(ChatRoomActivity.ARG_ROOM_ID, mRoomName);
        PendingIntent intent = PendingIntent.getActivity(this, 0, notificationIntent, 0);
        mNotifyBuilder.setContentIntent(intent);

        //Creates a service which removes the notification after application is forced closed.
        if (mConnection == null) {
            mConnection = new ServiceConnection() {

                public void onServiceConnected(ComponentName className, IBinder binder) {
                    ((ClearBinder) binder).service.startService(
                            new Intent(ChatRoomActivity.this, ClearNotificationService.class));
                    NotificationManager mNotificationManager
                            = (NotificationManager) getSystemService(NOTIFICATION_SERVICE);
                    mNotificationManager.notify(ClearNotificationService.NOTIFICATION_ID,
                            mNotifyBuilder.build());
                }

                public void onServiceDisconnected(ComponentName className) {
                    mConnection = null;
                }

            };
        }
        if (!mIsBound) {
            bindService(new Intent(ChatRoomActivity.this,
                            ClearNotificationService.class), mConnection,
                    Context.BIND_AUTO_CREATE);
            mIsBound = true;
            startService(notificationIntent);
        }
    }

    @Override
    public void onResume() {
        super.onResume();
        super.onResume();
        //Resume implies restore video mode if it was enable before pausing app

        //If service is binded remove it, so that the next time onPause can bind service.
        if (mIsBound) {
            unbindService(mConnection);
            stopService(new Intent(ClearNotificationService.MY_SERVICE));
            mIsBound = false;
        }

        if (mRoom != null) {
            mRoom.onResume();
        }

        mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);
        reloadInterface();
    }

    @Override
    public void onStop() {
        super.onStop();
        if (mIsBound) {
            unbindService(mConnection);
            mIsBound = false;
        }

        if (this.isFinishing()) {
            mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);
            if (mRoom != null) {
                mRoom.disconnect();
            }
        }
    }

    @Override
    public void onDestroy() {
        mNotificationManager.cancel(ClearNotificationService.NOTIFICATION_ID);

        if (mIsBound) {
            unbindService(mConnection);
            mIsBound = false;
        }

        if (mRoom != null) {
            mRoom.disconnect();
        }

        super.onDestroy();
        finish();
    }

    @Override
    public void onBackPressed() {

        if (mRoom != null) {
            mRoom.disconnect();
        }

        super.onBackPressed();
    }

    public void reloadInterface() {
        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                if (mRoom != null && mRoom.getCurrentParticipant() != null) {
                    mRoom.getParticipantsViewContainer()
                            .addView(mRoom.getCurrentParticipant().getView());
                }
            }
        }, 500);
    }

    private void initializeRoom() {
        Log.i(LOGTAG, "initializing chat room fragment for room: " + mRoomName);
        setTitle(mRoomName);

        //Show connecting dialog
        mConnectingDialog = new ProgressDialog(this);
        mConnectingDialog.setTitle("Joining Room...");
        mConnectingDialog.setMessage("Please wait.");
        mConnectingDialog.setCancelable(false);
        mConnectingDialog.setIndeterminate(true);
        mConnectingDialog.show();

        GetRoomDataTask task = new GetRoomDataTask();
        task.execute(mRoomName, mUsername);
    }

    private class GetRoomDataTask extends AsyncTask<String, Void, Room> {

        protected HttpClient mHttpClient;

        protected HttpGet mHttpGet;

        protected boolean mDidCompleteSuccessfully;

        public GetRoomDataTask() {
            mHttpClient = new DefaultHttpClient();
        }

        @Override
        protected Room doInBackground(String... params) {
            String sessionId = null;
            String token = null;
            String apiKey = null;
            initializeGetRequest(params[0]);
            try {
                HttpResponse roomResponse = mHttpClient.execute(mHttpGet);
                HttpEntity roomEntity = roomResponse.getEntity();
                String temp = EntityUtils.toString(roomEntity);
                Log.i(LOGTAG, "retrieved room response: " + temp);
                JSONObject roomJson = new JSONObject(temp);
                sessionId = roomJson.getString("sid");
                token = roomJson.getString("token");
                apiKey = roomJson.getString("apiKey");
                mDidCompleteSuccessfully = true;
            } catch (Exception exception) {
                Log.e(LOGTAG,
                        "could not get room data: " + exception.getMessage());
                mDidCompleteSuccessfully = false;
                return null;
            }
            return new Room(ChatRoomActivity.this, params[0], sessionId, token,
                    apiKey, params[1]);
        }

        @Override
        protected void onPostExecute(final Room room) {
            if (mDidCompleteSuccessfully) {
                mConnectingDialog.dismiss();
                mRoom = room;
                mPreview.setOnClickListener(onViewClick);
                mRoom.setPreviewView(mPreview);
                mRoom.setParticipantsViewContainer(mParticipantsView, onViewClick);
                mRoom.setMessageView((TextView) findViewById(R.id.messageView),
                        (ScrollView) findViewById(R.id.scroller));
                mRoom.connect();
            } else {
                mConnectingDialog.dismiss();
                mConnectingDialog = null;
                showErrorDialog();
            }
        }

        protected void initializeGetRequest(String room) {
            URI roomURI;
            URL url;

            String urlStr = "https://opentokrtc.com/" + room + ".json";
            try {
                url = new URL(urlStr);
                roomURI = new URI(url.getProtocol(), url.getUserInfo(),
                        url.getHost(), url.getPort(), url.getPath(),
                        url.getQuery(), url.getRef());
            } catch (URISyntaxException exception) {
                Log.e(LOGTAG,
                        "the room URI is malformed: " + exception.getMessage());
                return;
            } catch (MalformedURLException exception) {
                Log.e(LOGTAG,
                        "the room URI is malformed: " + exception.getMessage());
                return;
            }
            mHttpGet = new HttpGet(roomURI);
        }
    }

    private DialogInterface.OnClickListener errorListener = new DialogInterface.OnClickListener() {
        public void onClick(DialogInterface dialog, int id) {
            finish();
        }
    };

    private void showErrorDialog() {
        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle(R.string.error_title);
        builder.setMessage(R.string.error);
        builder.setCancelable(false);
        builder.setPositiveButton("OK", errorListener);
        mErrorDialog = builder.create();
        mErrorDialog.show();
    }

    public void onClickSend(View v) {
        if (mMessageEditText.getText().toString().compareTo("") == 0) {
            Log.d("Send Message", "Cannot Send - Empty String");
        } else {
            mRoom.sendChatMessage(mMessageEditText.getText().toString());
            mMessageEditText.setText("");
        }
    }

    public void onClickTextChat(View v) {
        if (mMessageBox.getVisibility() == View.GONE) {
            mMessageBox.setVisibility(View.VISIBLE);
        } else {
            mMessageBox.setVisibility(View.GONE);
        }
    }

    public void onClickShareLink(View v) {
        String roomUrl = serverURL + mRoomName;
        String text = getString(R.string.sharingLink) + " " + roomUrl;
        Intent sendIntent = new Intent();
        sendIntent.setAction(Intent.ACTION_SEND);
        sendIntent.putExtra(Intent.EXTRA_TEXT, text);
        sendIntent.setType("text/plain");
        startActivity(sendIntent);
    }

    public void onPublisherViewClick(View v) {
        if (mRoom != null && mRoom.getCurrentParticipant() != null) {
            mRoom.getCurrentParticipant().getView()
                    .setOnClickListener(onViewClick);
        }
    }

    //Initialize fragments
    public void initPublisherFragment() {
        mPublisherFragment = new PublisherControlFragment();
        getFragmentManager().beginTransaction()
                .add(R.id.fragment_pub_container, mPublisherFragment)
                .commit();
    }

    public void initPublisherStatusFragment() {
        mPublisherStatusFragment = new PublisherStatusFragment();
        getFragmentManager().beginTransaction()
                .add(R.id.fragment_pub_status_container, mPublisherStatusFragment)
                .commit();
    }

    public void initSubscriberFragment() {
        mSubscriberFragment = new SubscriberControlFragment();
        getFragmentManager().beginTransaction()
                .add(R.id.fragment_sub_container, mSubscriberFragment).commit();
    }

    public void initSubscriberQualityFragment() {
        mSubscriberQualityFragment = new SubscriberQualityFragment();
        getFragmentManager()
                .beginTransaction()
                .add(R.id.fragment_sub_quality_container,
                        mSubscriberQualityFragment).commit();
    }

    public Room getRoom() {
        return mRoom;
    }

    public Handler getHandler() {
        return this.mHandler;
    }

    public ProgressBar getLoadingSub() {
        return mLoadingSub;
    }

    public void updateLoadingSub() {
        mRoom.loadSubscriberView();
    }

    //Callbacks
    @Override
    public void onMuteSubscriber() {
        if (mRoom.getCurrentParticipant() != null) {
            mRoom.getCurrentParticipant().setSubscribeToAudio(
                    !mRoom.getCurrentParticipant().getSubscribeToAudio());
        }
    }

    @Override
    public void onMutePublisher() {
        if (mRoom.getPublisher() != null) {
            mRoom.getPublisher().setPublishAudio(
                    !mRoom.getPublisher().getPublishAudio());
        }
    }

    @Override
    public void onSwapCamera() {
        if (mRoom.getPublisher() != null) {
            mRoom.getPublisher().swapCamera();
        }
    }

    @Override
    public void onEndCall() {
        if (mRoom != null) {
            mRoom.disconnect();
        }

        finish();
    }

    @Override
    public void onStatusPubBar() {
        setPublisherMargins();
    }

    @Override
    public void onStatusSubBar() {
        showArrowsOnSubscriber();
    }

    //Adjust publisher view if its control bar is hidden
    public void setPublisherMargins() {
        int bottomMargin = 0;
        RelativeLayout.LayoutParams params = (LayoutParams) mPreview
                .getLayoutParams();
        RelativeLayout.LayoutParams pubControlLayoutParams = (LayoutParams) mPublisherFragment
                .getPublisherContainer().getLayoutParams();
        RelativeLayout.LayoutParams pubStatusLayoutParams = (LayoutParams) mPublisherStatusFragment
                .getPubStatusContainer().getLayoutParams();

        if (mPublisherFragment.isPublisherWidgetVisible() && mArchiving) {
            bottomMargin = pubControlLayoutParams.height
                    + pubStatusLayoutParams.height + dpToPx(20);
        } else {
            if (mPublisherFragment.isPublisherWidgetVisible()) {
                bottomMargin = pubControlLayoutParams.height + dpToPx(20);
            } else {
                params.addRule(RelativeLayout.ALIGN_BOTTOM);
                bottomMargin = dpToPx(20);
            }
        }
        params.bottomMargin = bottomMargin;
        params.leftMargin = dpToPx(20);
        mPreview.setLayoutParams(params);

        setSubQualityMargins();
    }

    public void setSubQualityMargins() {
        if (mRoom.getParticipants() != null) {
            RelativeLayout.LayoutParams subQualityLayoutParams
                    = (LayoutParams) mSubscriberQualityFragment
                    .getSubQualityContainer().getLayoutParams();
            boolean pubControlBarVisible = mPublisherFragment
                    .isPublisherWidgetVisible();
            boolean pubStatusBarVisible = mPublisherStatusFragment
                    .isPubStatusWidgetVisible();
            RelativeLayout.LayoutParams pubControlLayoutParams = (LayoutParams) mPublisherFragment
                    .getPublisherContainer().getLayoutParams();
            RelativeLayout.LayoutParams pubStatusLayoutParams
                    = (LayoutParams) mPublisherStatusFragment
                    .getPubStatusContainer().getLayoutParams();

            int bottomMargin = 0;

            // control pub fragment
            if (getResources().getConfiguration().orientation
                    == Configuration.ORIENTATION_PORTRAIT) {
                if (pubControlBarVisible) {
                    bottomMargin = pubControlLayoutParams.height + dpToPx(10);
                }
                if (pubStatusBarVisible && mArchiving) {
                    bottomMargin = pubStatusLayoutParams.height + dpToPx(10);
                }
                if (bottomMargin == 0) {
                    bottomMargin = dpToPx(10);
                }
                subQualityLayoutParams.rightMargin = dpToPx(10);
            }

            subQualityLayoutParams.bottomMargin = bottomMargin;

            mSubscriberQualityFragment.getSubQualityContainer()
                    .setLayoutParams(subQualityLayoutParams);
        }
    }

    private OnClickListener onViewClick = new OnClickListener() {
        @Override
        public void onClick(View v) {
            boolean visible = false;

            if (mRoom.getPublisher() != null) {
                // check visibility of bars
                if (!mPublisherFragment.isPublisherWidgetVisible()) {
                    visible = true;
                }
                mPublisherFragment.publisherClick();
                if (mArchiving) {
                    mPublisherStatusFragment.publisherClick();
                }
                setPublisherMargins();

                if (mRoom.getCurrentParticipant() != null) {
                    mSubscriberFragment.showSubscriberWidget(visible);
                    mSubscriberFragment.initSubscriberUI();
                    showArrowsOnSubscriber();
                }
            }
        }
    };

    //Show next and last arrow on subscriber view if the number of subscribers is higher than 1
    public void showArrowsOnSubscriber() {
        boolean show = false;
        if (mRoom.getParticipants().size() > 1) {
            if (mLeftArrowImage.getVisibility() == View.GONE) {
                show = true;
            } else {
                show = false;
            }
            mLeftArrowImage.clearAnimation();
            mRightArrowImage.clearAnimation();
            float dest = show ? 1.0f : 0.0f;
            AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
            aa.setDuration(ANIMATION_DURATION);
            aa.setFillAfter(true);
            mLeftArrowImage.startAnimation(aa);
            mRightArrowImage.startAnimation(aa);
        }

        if (show) {
            mLeftArrowImage.setVisibility(View.VISIBLE);
            mRightArrowImage.setVisibility(View.VISIBLE);
            //to show all the controls
            if (mRoom.getPublisher() != null) {
                mPublisherStatusFragment.showPubStatusWidget(true);
            }
            mSubscriberFragment.showSubscriberWidget(true);
        } else {
            mLeftArrowImage.setVisibility(View.GONE);
            mRightArrowImage.setVisibility(View.GONE);
        }
    }

    public void nextParticipant(View view) {
        int nextPosition = mRoom.getCurrentPosition() + 1;
        mParticipantsView.setCurrentItem(nextPosition);

        //reload subscriber controls UI
        mSubscriberFragment.initSubscriberWidget();
        mSubscriberFragment.showSubscriberWidget(true);
    }

    public void lastParticipant(View view) {
        int nextPosition = mRoom.getCurrentPosition() - 1;
        mParticipantsView.setCurrentItem(nextPosition);

        //reload subscriber controls UI
        mSubscriberFragment.initSubscriberWidget();
        mSubscriberFragment.showSubscriberWidget(true);
    }

    //Show audio only icon when video quality changed and it is disabled for the subscriber
    public void setAudioOnlyView(boolean audioOnlyEnabled, Participant participant) {
        mSubscriberVideoOnly = audioOnlyEnabled;

        if (audioOnlyEnabled) {
            participant.getView().setVisibility(View.GONE);
            mSubscriberAudioOnlyView.setVisibility(View.VISIBLE);
            mSubscriberAudioOnlyView.setOnClickListener(onViewClick);

            // Audio only text for subscriber
            TextView subStatusText = (TextView) findViewById(R.id.subscriberName);
            subStatusText.setText(R.string.audioOnly);
            AlphaAnimation aa = new AlphaAnimation(1.0f, 0.0f);
            aa.setDuration(ANIMATION_DURATION);
            subStatusText.startAnimation(aa);

            participant
                    .setAudioLevelListener(new SubscriberKit.AudioLevelListener() {
                        @Override
                        public void onAudioLevelUpdated(
                                SubscriberKit subscriber, float audioLevel) {
                            mAudioLevelView.setMeterValue(audioLevel);
                        }
                    });
        } else {
            if (!mSubscriberVideoOnly) {
                participant.getView().setVisibility(View.VISIBLE);
                mSubscriberAudioOnlyView.setVisibility(View.GONE);
            }
        }
    }

    //Update publisher status bar when archiving stars/stops
    public void updateArchivingStatus(boolean archiving) {
        mArchiving = archiving;

        if (archiving) {
            mPublisherFragment.showPublisherWidget(false);
            mPublisherStatusFragment.updateArchivingUI(true);
            mPublisherFragment.showPublisherWidget(true);
            mPublisherFragment.initPublisherUI();
            setPublisherMargins();

            if (mRoom.getCurrentParticipant() != null) {
                mSubscriberFragment.showSubscriberWidget(true);
            }
        } else {
            mPublisherStatusFragment.updateArchivingUI(false);
            setPublisherMargins();
        }
    }


    //Convert dp to real pixels, according to the screen density.
    public int dpToPx(int dp) {
        double screenDensity = this.getResources().getDisplayMetrics().density;
        return (int) (screenDensity * (double) dp);
    }
}