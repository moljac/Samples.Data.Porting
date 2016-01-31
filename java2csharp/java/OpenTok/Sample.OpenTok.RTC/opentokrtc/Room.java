package com.tokbox.android.opentokrtc;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.Iterator;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import android.content.Context;
import android.os.Handler;
import android.support.v4.view.PagerAdapter;
import android.support.v4.view.ViewPager;
import android.util.Log;
import android.view.SurfaceView;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.RelativeLayout;
import android.widget.RelativeLayout.LayoutParams;
import android.widget.ScrollView;
import android.widget.TextView;
import android.widget.Toast;

import com.opentok.android.BaseVideoRenderer;
import com.opentok.android.Connection;
import com.opentok.android.OpentokError;
import com.opentok.android.Publisher;
import com.opentok.android.Session;
import com.opentok.android.Stream;

public class Room extends Session {

    private static final String LOGTAG = "Room";

    private Context mContext;

    private String apikey;
    private String sessionId;
    private String token;

    private Publisher mPublisher;
    private Participant mCurrentParticipant;
    private int mCurrentPosition;
    private String mPublisherName = null;
    private HashMap<Stream, Participant> mParticipantStream = new HashMap<Stream, Participant>();
    private HashMap<String, Participant> mParticipantConnection
            = new HashMap<String, Participant>();
    private ArrayList<Participant> mParticipants = new ArrayList<Participant>();

    private ViewGroup mPreview;
    private TextView mMessageView;
    private ScrollView mMessageScroll;
    private ViewPager mParticipantsViewContainer;
    private OnClickListener onSubscriberUIClick;

    private Handler mHandler;

    private ChatRoomActivity mActivity;

    private PagerAdapter mPagerAdapter = new PagerAdapter() {

        @Override
        public boolean isViewFromObject(View arg0, Object arg1) {
            return ((Participant) arg1).getView() == arg0;
        }

        @Override
        public int getCount() {
            return mParticipants.size();
        }

        @Override
        public CharSequence getPageTitle(int position) {
            if (position < mParticipants.size()) {
                return mParticipants.get(position).getName();
            } else {
                return null;
            }
        }

        @Override
        public Object instantiateItem(ViewGroup container, int position) {
            Participant p = mParticipants.get(position);
            container.addView(p.getView());
            return p;
        }

        @Override
        public void setPrimaryItem(ViewGroup container, int position,
                Object object) {
            for (Participant p : mParticipants) {
                if (p == object) {
                    mCurrentParticipant = p;
                    if (!p.getSubscribeToVideo()) {
                        p.setSubscribeToVideo(true);
                    }
                    if (p.getSubscriberVideoOnly()) {
                        mActivity.setAudioOnlyView(true, p);
                    }
                } else {
                    if (p.getSubscribeToVideo()) {
                        p.setSubscribeToVideo(false);
                    }
                }
            }
        }

        @Override
        public void destroyItem(ViewGroup container, int position, Object object) {
            Participant p = (Participant) object;
            container.removeView(p.getView());
        }

        @Override
        public int getItemPosition(Object object) {
            for (int i = 0; i < mParticipants.size(); i++) {
                if (mParticipants.get(i) == object) {
                    return i;
                }
            }
            return POSITION_NONE;
        }
    };

    public Room(Context context, String roomName, String sessionId, String token, String apiKey,
            String username) {
        super(context, apiKey, sessionId);
        this.apikey = apiKey;
        this.sessionId = sessionId;
        this.token = token;
        this.mContext = context;
        this.mPublisherName = username;
        this.mHandler = new Handler(context.getMainLooper());
        this.mActivity = (ChatRoomActivity) this.mContext;
    }

    public void setParticipantsViewContainer(ViewPager container,
            OnClickListener onSubscriberUIClick) {
        this.mParticipantsViewContainer = container;
        this.onSubscriberUIClick = onSubscriberUIClick;
        mPagerAdapter.notifyDataSetChanged();
    }

    public void setMessageView(TextView et, ScrollView scroller) {
        this.mMessageView = et;
        this.mMessageScroll = scroller;
    }

    public void setPreviewView(ViewGroup preview) {
        this.mPreview = preview;
    }

    public void connect() {
        this.connect(token);
    }

    public void sendChatMessage(String message) {
        JSONObject json = new JSONObject();
        try {
            json.put("name", mPublisherName);
            json.put("text", message);
            sendSignal("chat", json.toString());
            presentMessage("Me", message);
        } catch (JSONException e) {
            e.printStackTrace();
        }
    }

    public void loadSubscriberView() {
        //stop loading spinning
        if (mActivity.getLoadingSub().getVisibility() == View.VISIBLE) {
            mActivity.getLoadingSub().setVisibility(View.GONE);
        }

        //show control bars
        mActivity.mSubscriberFragment.showSubscriberWidget(true);
        mActivity.mSubscriberFragment.initSubscriberUI();
        if (mPublisher != null) {
            mActivity.mPublisherFragment.showPublisherWidget(true);
            mActivity.setPublisherMargins();
        }
        mActivity.showArrowsOnSubscriber();
    }

    public Publisher getPublisher() {
        return mPublisher;
    }

    public Participant getCurrentParticipant() {
        return mCurrentParticipant;
    }

    public ArrayList<Participant> getParticipants() {
        return mParticipants;
    }

    public PagerAdapter getPagerAdapter() {
        return mPagerAdapter;
    }

    public int getCurrentPosition() {
        return mCurrentPosition;
    }

    public ViewPager getParticipantsViewContainer() {
        return mParticipantsViewContainer;
    }

    private void presentMessage(String who, String message) {
        presentText("\n" + who + ": " + message);
    }

    private void presentText(String message) {
        mMessageView.setText(mMessageView.getText() + message);
        mMessageScroll.post(new Runnable() {
            @Override
            public void run() {
                int totalHeight = mMessageView.getHeight();
                mMessageScroll.smoothScrollTo(0, totalHeight);
            }
        });
    }

    //Callbacks
    @Override
    public void onPause() {
        super.onPause();
        if (mPublisher != null) {
            mPreview.setVisibility(View.GONE);
        }
    }

    @Override
    public void onResume() {
        super.onResume();
        mHandler.postDelayed(new Runnable() {

            @Override
            public void run() {
                if (mPublisher != null) {
                    mPreview.setVisibility(View.VISIBLE);
                    mPreview.removeView(mPublisher.getView());
                    RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(
                            LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
                    mPreview.addView(mPublisher.getView(), lp);
                }
            }
        }, 500);
    }

    @Override
    protected void onConnected() {
        Publisher p = new Publisher(mContext, "Android");
        mPublisher = p;
        mPublisher.setName(mPublisherName);
        publish(p);

        // Add video preview
        RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(
                LayoutParams.MATCH_PARENT, LayoutParams.MATCH_PARENT);
        SurfaceView v = (SurfaceView) p.getView();
        v.setZOrderOnTop(true);

        mPreview.addView(v, lp);
        p.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE,
                BaseVideoRenderer.STYLE_VIDEO_FILL);

        presentText((mActivity.getResources().getString(R.string.welcome_text_chat)));
        if (mPublisherName != null && !mPublisherName.isEmpty()) {
            sendChatMessage(
                    mActivity.getResources().getString((R.string.nick)) + " " + mPublisherName);
        }
    }

    @Override
    protected void onStreamReceived(Stream stream) {

        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                mActivity.mPublisherFragment.showPublisherWidget(true);
                mActivity.mPublisherFragment.initPublisherUI();
            }
        }, 0);

        Participant p = new Participant(mContext, stream);

        //We can use connection data to obtain each user id
        p.setUserId(stream.getConnection().getData());

        //Subscribe to audio only if we have more than 1 subscribed participant
        if (mParticipants.size() != 0) {
            p.setSubscribeToVideo(false);
        } else {
            // start loading spinning
            mActivity.getLoadingSub().setVisibility(View.VISIBLE);
        }

        p.getView().setOnClickListener(this.onSubscriberUIClick);

        //Subscribe to this participant
        this.subscribe(p);

        mParticipants.add(p);
        mParticipantStream.put(stream, p);
        mParticipantConnection.put(stream.getConnection().getConnectionId(), p);

        presentText("\n" + p.getName() + " has joined the chat");

        this.mParticipantsViewContainer.setAdapter(mPagerAdapter);
        mPagerAdapter.notifyDataSetChanged();

    }

    @Override
    protected void onStreamDropped(Stream stream) {
        Participant p = mParticipantStream.get(stream);
        if (p != null) {
            mParticipants.remove(p);
            mParticipantStream.remove(stream);
            mParticipantConnection.remove(stream.getConnection().getConnectionId());
            mPagerAdapter.notifyDataSetChanged();
            mCurrentParticipant = null;

            presentText("\n" + p.getName() + " has left the chat");
            mActivity.showArrowsOnSubscriber();
            mActivity.mSubscriberFragment.showSubscriberWidget(false);
            mActivity.mSubscriberQualityFragment.showSubscriberWidget(false);
        }

    }

    @Override
    protected void onSignalReceived(String type, String data,
            Connection connection) {
        Log.d(LOGTAG, "Received signal:" + type + " data:" + data + "connection: " + connection);

        if (connection != null) {
            String mycid = this.getConnection().getConnectionId();
            String cid = connection.getConnectionId();
            if (!cid.equals(mycid)) {
                if ("chat".equals(type)) {
                    //Text message
                    Participant p = mParticipantConnection.get(cid);
                    if (p != null) {
                        JSONObject json;
                        try {
                            json = new JSONObject(data);
                            String text = json.getString("text");
                            String name = json.getString("name");
                            p.setName(name);
                            presentMessage(p.getName(), text);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                } else if ("name".equals(type)) {
                    //Name change message
                    Participant p = mParticipantConnection.get(cid);
                    if (p != null) {
                        try {
                            String oldName = p.getName();
                            JSONArray jsonArray = new JSONArray(data);
                            String name = jsonArray.getString(1);
                            p.setName(name);
                            presentText("\n" + oldName + " is now known as " + name);
                        } catch (JSONException e) {
                            e.printStackTrace();
                        }
                    }
                }
            }
        } else if ("initialize".equals(type)) {
            // Initialize message
            try {
                JSONObject json = new JSONObject(data);
                JSONObject users = json.getJSONObject("users");
                Iterator<?> it = users.keys();
                while (it.hasNext()) {
                    String pcid = (String) it.next();
                    Participant p = mParticipantConnection.get(pcid);
                    if (p != null) {
                        p.setName(users.getString(pcid));
                    }
                }
            } catch (JSONException e) {
                e.printStackTrace();
            }
        }
    }

    @Override
    protected void onArchiveStarted(String id, String name) {
        super.onArchiveStarted(id, name);
        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                mActivity.updateArchivingStatus(true);
            }
        }, 0);
    }

    @Override
    protected void onArchiveStopped(String id) {
        super.onArchiveStopped(id);
        mHandler.postDelayed(new Runnable() {
            @Override
            public void run() {
                mActivity.updateArchivingStatus(false);
            }
        }, 0);
    }

    @Override
    protected void onError(OpentokError error) {
        super.onError(error);
        Toast.makeText(this.mContext, error.getMessage(), Toast.LENGTH_SHORT).show();
    }

}