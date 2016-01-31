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
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.RelativeLayout;

public class PublisherControlFragment extends Fragment implements
        View.OnClickListener {

    private static final String LOGTAG = "demo-UI-pub-control-fragment";

    // Animation constants
    private static final int ANIMATION_DURATION = 500;
    private static final int PUBLISHER_CONTROLS_DURATION = 7000;

    private ImageButton mPublisherMute;
    private ImageButton mSwapCamera;
    private Button mEndCall;
    private RelativeLayout mPublisherContainer;
    private PublisherCallbacks mCallbacks = sOpenTokCallbacks;
    protected boolean mPublisherWidgetVisible = false;

    private ChatRoomActivity chatRoomActivity;

    public interface PublisherCallbacks {

        public void onMutePublisher();

        public void onSwapCamera();

        public void onEndCall();

        public void onStatusPubBar();
    }

    private static PublisherCallbacks sOpenTokCallbacks = new PublisherCallbacks() {

        @Override
        public void onMutePublisher() {
        }

        @Override
        public void onSwapCamera() {
        }

        @Override
        public void onEndCall() {
        }

        @Override
        public void onStatusPubBar() {
        }

    };

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);

        Log.i(LOGTAG, "On attach Publisher control fragment");
        chatRoomActivity = (ChatRoomActivity) activity;
        if (!(activity instanceof PublisherCallbacks)) {
            throw new IllegalStateException(
                    "Activity must implement fragment's callback");
        }

        mCallbacks = (PublisherCallbacks) activity;
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

        View rootView = inflater.inflate(R.layout.layout_fragment_pub_control,
                container, false);

        mPublisherContainer = (RelativeLayout) chatRoomActivity
                .findViewById(R.id.fragment_pub_container);

        mPublisherMute = (ImageButton) rootView
                .findViewById(R.id.mutePublisher);
        mPublisherMute.setOnClickListener(this);

        mSwapCamera = (ImageButton) rootView.findViewById(R.id.swapCamera);
        mSwapCamera.setOnClickListener(this);

        mEndCall = (Button) rootView.findViewById(R.id.endCall);
        mEndCall.setOnClickListener(this);

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
            case R.id.mutePublisher:
                mutePublisher();
                break;

            case R.id.swapCamera:
                swapCamera();
                break;

            case R.id.endCall:
                endCall();
                break;
        }
    }

    public void updateStatusPubBar() {
        mCallbacks.onStatusPubBar();
    }

    public void swapCamera() {
        mCallbacks.onSwapCamera();
    }

    public void endCall() {
        mCallbacks.onEndCall();
    }

    public void mutePublisher() {
        mCallbacks.onMutePublisher();

        mPublisherMute.setImageResource(chatRoomActivity.getRoom().getPublisher()
                .getPublishAudio() ? R.drawable.unmute_pub : R.drawable.mute_pub);
    }

    //Initialize publisher control bar: mute/unmute, endCall and swap camera buttons
    public void initPublisherUI() {
        chatRoomActivity.getHandler().removeCallbacks(
                mPublisherWidgetTimerTask);
        chatRoomActivity.getHandler().postDelayed(mPublisherWidgetTimerTask,
                PUBLISHER_CONTROLS_DURATION);
        mPublisherMute.setImageResource(chatRoomActivity.getRoom().getPublisher()
                .getPublishAudio() ? R.drawable.unmute_pub : R.drawable.mute_pub);
    }

    private Runnable mPublisherWidgetTimerTask = new Runnable() {
        @Override
        public void run() {
            showPublisherWidget(false);
            updateStatusPubBar();
        }
    };

    public void showPublisherWidget(boolean show) {
        showPublisherWidget(show, true);
    }

    //Set animation to show and hide the publisher control bar
    private void showPublisherWidget(boolean show, boolean animate) {
        mPublisherContainer.clearAnimation();
        mPublisherWidgetVisible = show;
        float dest = show ? 1.0f : 0.0f;
        AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
        aa.setDuration(animate ? ANIMATION_DURATION : 1);
        aa.setFillAfter(true);
        mPublisherContainer.startAnimation(aa);

        if (show) {
            mEndCall.setClickable(true);
            mSwapCamera.setClickable(true);
            mPublisherMute.setClickable(true);
            mPublisherContainer.setVisibility(View.VISIBLE);
        } else {
            mEndCall.setClickable(false);
            mSwapCamera.setClickable(false);
            mPublisherMute.setClickable(false);
            mPublisherContainer.setVisibility(View.GONE);
        }
    }

    public void publisherClick() {
        if (!mPublisherWidgetVisible) {
            showPublisherWidget(true);
        } else {
            showPublisherWidget(false);
        }

        initPublisherUI();
    }

    public boolean isPublisherWidgetVisible() {
        return mPublisherWidgetVisible;
    }

    public RelativeLayout getPublisherContainer() {
        return mPublisherContainer;
    }
}
