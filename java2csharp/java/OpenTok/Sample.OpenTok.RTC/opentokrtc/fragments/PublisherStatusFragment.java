package com.tokbox.android.opentokrtc.fragments;

import com.tokbox.android.opentokrtc.ChatRoomActivity;
import com.tokbox.android.opentokrtc.R;

import android.app.Activity;
import android.app.Fragment;
import android.content.res.Configuration;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.AlphaAnimation;
import android.widget.ImageButton;
import android.widget.RelativeLayout;
import android.widget.TextView;

public class PublisherStatusFragment extends Fragment {

    private static final String LOGTAG = "demo-UI-pub-status-fragment";

    // Animation constants
    private static final int ANIMATION_DURATION = 500;

    private static final int STATUS_ANIMATION_DURATION = 7000;

    //Interface
    private ImageButton archiving;

    private TextView statusText;

    private RelativeLayout mPubStatusContainer;

    private boolean mPubStatusWidgetVisible = false;

    private boolean mArchiving = false;

    private ChatRoomActivity chatRoomActivity;

    @Override
    public void onAttach(Activity activity) {
        super.onAttach(activity);

        Log.i(LOGTAG, "On attach Publisher status fragment");
        chatRoomActivity = (ChatRoomActivity) activity;
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.layout_fragment_pub_status,
                container, false);

        mPubStatusContainer = (RelativeLayout) chatRoomActivity
                .findViewById(R.id.fragment_pub_status_container);
        archiving = (ImageButton) rootView.findViewById(R.id.archiving);

        statusText = (TextView) rootView.findViewById(R.id.statusLabel);

        if (chatRoomActivity.getResources().getConfiguration().orientation
                == Configuration.ORIENTATION_LANDSCAPE) {
            RelativeLayout.LayoutParams params = (RelativeLayout.LayoutParams) container
                    .getLayoutParams();

            DisplayMetrics metrics = new DisplayMetrics();
            chatRoomActivity.getWindowManager().getDefaultDisplay()
                    .getMetrics(metrics);

            params.width = metrics.widthPixels - chatRoomActivity.dpToPx(48);
            container.setLayoutParams(params);
        }

        return rootView;
    }

    @Override
    public void onDetach() {
        super.onDetach();
        Log.i(LOGTAG, "On detach Publisher status fragment");
    }

    //Initialize publisher status bar
    public void initPubStatusUI() {
        chatRoomActivity.getHandler()
                .removeCallbacks(mPubStatusWidgetTimerTask);
        chatRoomActivity.getHandler().postDelayed(mPubStatusWidgetTimerTask,
                STATUS_ANIMATION_DURATION);
    }

    private Runnable mPubStatusWidgetTimerTask = new Runnable() {
        @Override
        public void run() {
            if (mArchiving) {
                showPubStatusWidget(false);
                chatRoomActivity.setPublisherMargins();
            }
        }
    };

    public void showPubStatusWidget(boolean show) {
        showPubStatusWidget(show, true);
    }

    //Set animation to show and hide the publisher status bar
    private void showPubStatusWidget(boolean show, boolean animate) {
        mPubStatusContainer.clearAnimation();
        mPubStatusWidgetVisible = show;
        float dest = show ? 1.0f : 0.0f;
        AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
        aa.setDuration(animate ? ANIMATION_DURATION : 1);
        aa.setFillAfter(true);
        mPubStatusContainer.startAnimation(aa);

        if (show && mArchiving) {
            mPubStatusContainer.setVisibility(View.VISIBLE);
        } else {
            mPubStatusContainer.setVisibility(View.GONE);
        }
    }

    public void publisherClick() {
        if (!mPubStatusWidgetVisible) {
            showPubStatusWidget(true);
        } else {
            showPubStatusWidget(false);
        }

        initPubStatusUI();
    }

    //Update archiving status icon
    public void updateArchivingUI(boolean archivingOn) {
        archiving = (ImageButton) chatRoomActivity.findViewById(R.id.archiving);
        this.mArchiving = archivingOn;

        if (archivingOn) {
            statusText.setText(R.string.archivingOn);
            archiving.setImageResource(R.drawable.archiving_on);
            showPubStatusWidget(true);
            initPubStatusUI();
        } else {
            showPubStatusWidget(false);
        }
    }

    public boolean isPubStatusWidgetVisible() {
        return mPubStatusWidgetVisible;
    }

    public RelativeLayout getPubStatusContainer() {
        return mPubStatusContainer;
    }

}
