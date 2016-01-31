package com.tokbox.android.opentokrtc.fragments;

import com.tokbox.android.opentokrtc.ChatRoomActivity;
import com.tokbox.android.opentokrtc.R;

import android.app.Activity;
import android.app.Fragment;
import android.os.Bundle;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AlphaAnimation;
import android.widget.ImageButton;
import android.widget.RelativeLayout;
import android.widget.TextView;

public class SubscriberControlFragment extends Fragment implements
        View.OnClickListener {

    private static final String LOGTAG = "demo-UI-sub-control-fragment";

    // Animation constants
    private static final int ANIMATION_DURATION = 500;

    private static final int SUBSCRIBER_CONTROLS_DURATION = 7000;

    //Interface
    private boolean mSubscriberWidgetVisible = false;

    private ImageButton mSubscriberMute;

    private TextView mSubscriberName;

    private RelativeLayout mSubContainer;

    private SubscriberCallbacks mCallbacks = sOpenTokCallbacks;

    private ChatRoomActivity chatRoomActivity;

    public interface SubscriberCallbacks {

        public void onMuteSubscriber();

        public void onStatusSubBar();
    }

    private static SubscriberCallbacks sOpenTokCallbacks = new SubscriberCallbacks() {

        @Override
        public void onMuteSubscriber() {
        }

        @Override
        public void onStatusSubBar() {
        }
    };

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);
        Log.i(LOGTAG, "On attach Subscriber control fragment");

        chatRoomActivity = (ChatRoomActivity) activity;
        if (!(activity instanceof SubscriberCallbacks)) {
            throw new IllegalStateException(
                    "Activity must implement fragment's callback");
        }

        mCallbacks = (SubscriberCallbacks) activity;

    }

    @Override
    public void onActivityCreated(Bundle savedInstanceState) {
        super.onActivityCreated(savedInstanceState);
        Log.i(LOGTAG, "onActivityCreated");
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState) {
        Log.v(LOGTAG, "onCreateView");

        View rootView = inflater.inflate(R.layout.layout_fragment_sub_control,
                container, false);

        mSubContainer = (RelativeLayout) chatRoomActivity
                .findViewById(R.id.fragment_sub_container);

        mSubscriberMute = (ImageButton) rootView
                .findViewById(R.id.muteSubscriber);
        mSubscriberMute.setOnClickListener(this);

        mSubscriberName = (TextView) rootView.findViewById(R.id.subscriberName);

        showSubscriberWidget(mSubscriberWidgetVisible, false);

        return rootView;
    }

    @Override
    public void onDetach() {
        super.onDetach();
        Log.i(LOGTAG, "onDetach");

        mCallbacks = sOpenTokCallbacks;
    }

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.muteSubscriber:
                muteSubscriber();
                break;
        }
    }

    //Initialize subscriber status bar
    public void initSubscriberUI() {
        chatRoomActivity.getHandler().removeCallbacks(
                mSubscriberWidgetTimerTask);
        chatRoomActivity.getHandler().postDelayed(mSubscriberWidgetTimerTask,
                SUBSCRIBER_CONTROLS_DURATION);
        mSubscriberName.setText(chatRoomActivity.getRoom().getCurrentParticipant().getStream()
                .getName());
    }

    public void initSubscriberWidget() {
        mSubscriberMute.setImageResource(chatRoomActivity.getRoom().getCurrentParticipant()
                .getSubscribeToAudio() ? R.drawable.unmute_sub : R.drawable.mute_sub);
    }

    private Runnable mSubscriberWidgetTimerTask = new Runnable() {
        @Override
        public void run() {
            showSubscriberWidget(false);
            updateStatusSubBar();
        }
    };

    public void showSubscriberWidget(boolean show) {
        showSubscriberWidget(show, true);
    }

    //Set animation to show and hide the subscriber control bar  
    private void showSubscriberWidget(boolean show, boolean animate) {
        mSubContainer.clearAnimation();
        mSubscriberWidgetVisible = show;
        float dest = show ? 1.0f : 0.0f;
        AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
        aa.setDuration(animate ? ANIMATION_DURATION : 1);
        aa.setFillAfter(true);
        mSubContainer.startAnimation(aa);

        if (show) {
            mSubscriberMute.setClickable(true);
            mSubContainer.setVisibility(View.VISIBLE);
        } else {
            mSubscriberMute.setClickable(false);
            mSubContainer.setVisibility(View.GONE);
        }
    }

    public void updateStatusSubBar() {
        mCallbacks.onStatusSubBar();
    }

    public void muteSubscriber() {
        mCallbacks.onMuteSubscriber();

        mSubscriberMute.setImageResource(chatRoomActivity.getRoom().getCurrentParticipant()
                .getSubscribeToAudio() ? R.drawable.unmute_sub : R.drawable.mute_sub);
    }

}
