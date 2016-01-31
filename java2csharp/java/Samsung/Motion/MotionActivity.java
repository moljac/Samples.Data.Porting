
package com.samsung.sdk.motion.test;

import com.samsung.android.sdk.motion.Smotion;
import com.samsung.android.sdk.motion.SmotionActivity;
import com.samsung.android.sdk.motion.SmotionActivity.Info;

import android.annotation.SuppressLint;
import android.os.Handler;
import android.os.Looper;
import android.text.format.Time;

import java.util.Formatter;
import java.util.Timer;
import java.util.TimerTask;

class MotionActivity {

    private SmotionActivity mActivity = null;

    private int mMode = SmotionActivity.Info.MODE_REALTIME;

    private String[] mQueue = new String[2];

    private Timer mTimer;

    private SmotionActivity.Info mInfo[];

    private long mInterval;

    private boolean mIsPeriodicMode;

    private boolean mIsStarting = false;

    MotionActivity(Looper looper, Smotion motion) {
        mActivity = new SmotionActivity(looper, motion);
        initialize();
    }

    void start(int mode, boolean isPeriodicMode, long interval) {
        initialize();
        mMode = mode;
        mIsPeriodicMode = isPeriodicMode;
        mInterval = interval;
        mActivity.start(mMode, changeListener);
        if (mIsPeriodicMode) {
            startTimer();
        }
        mIsStarting = true;
        MotionTest.displayData(0, getMode(mode), "");
    }

    void stop() {
        if (mIsStarting) {
            mActivity.stop();
        }
        if (mIsPeriodicMode) {
            stopTimer();
        }
        mIsStarting = false;
        initialize();
    }

    void initialize() {
        StringBuffer sb = new StringBuffer();
        sb.append("Ready");
        MotionTest.displayData(0, sb.toString());

        mInfo = new Info[1];
        mQueue = new String[2];
    }

    void updateInfo() {
        if (mActivity == null)
            return;
        mActivity.updateInfo();
    }

    boolean isUpdateInfoBatchModeSupport() {
        return mActivity.isUpdateInfoBatchModeSupport();
    }

    private void displayData(int mode, Info[] info) {
        // TODO Auto-generated method stub
        String str = getMode(mMode);
        StringBuffer sb = new StringBuffer();
        if (mIsPeriodicMode && (mode == SmotionActivity.Info.MODE_REALTIME)) {
            sb.append("Periodic Mode : " + mInterval / 1000 + " sec" + "\n");
        }

        sb.append("<" + getMode(mode) + ">");
        if (MotionTest.mIsUpdateInfo) {
            if (mMode == SmotionActivity.Info.MODE_ALL) {
                if (isUpdateInfoBatchModeSupport()) {
                    sb.append(" - Update" + " All" + " Data");
                } else {
                    sb.append(" - Update " + getMode(mode) + " Data");
                }
            } else {
                sb.append(" - Update " + getMode(mode) + " Data");
            }
            MotionTest.mIsUpdateInfo = false;
        }
        sb.append("\n");
        for (int i = 0; i < info.length; i++) {
            long timestamp = 0;
            if (mIsPeriodicMode) {
                if (mode == SmotionActivity.Info.MODE_REALTIME) {
                    timestamp = System.currentTimeMillis();
                } else if (mode == SmotionActivity.Info.MODE_BATCH) {
                    timestamp = info[i].getTimeStamp();
                }
            } else {
                timestamp = info[i].getTimeStamp();
            }

            Time time = new Time();
            time.set(timestamp);
            Formatter form = new Formatter();
            form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
            sb.append("[" + form.toString() + "] ");
            form.close();
            sb.append(getStatus(info[i].getStatus()));
            sb.append("(" + getAccuracy(info[i].getAccuracy()) + ")");
            sb.append("\n");
        }

        switch (mode) {
            case SmotionActivity.Info.MODE_REALTIME:
                mQueue[0] = sb.toString();
                break;

            case SmotionActivity.Info.MODE_BATCH:
                mQueue[1] = sb.toString();
                break;
            default:
                break;
        }

        sb = new StringBuffer();
        for (int i = 0; i < mQueue.length; i++) {
            if (mQueue[i] != null) {
                sb.append(mQueue[i] + "\n");
            }
        }
        if (str != null) {
            MotionTest.displayData(0, str, sb.toString());
        }
    }

    private String getStatus(int status) {
        String str = null;
        switch (status) {
            case SmotionActivity.Info.STATUS_UNKNOWN:
                str = "Unknown";
                break;
            case SmotionActivity.Info.STATUS_STATIONARY:
                str = "Stationary";
                break;
            case SmotionActivity.Info.STATUS_WALK:
                str = "Walk";
                break;
            case SmotionActivity.Info.STATUS_RUN:
                str = "Run";
                break;
            case SmotionActivity.Info.STATUS_VEHICLE:
                str = "Vehicle";
                break;
            default:
                break;
        }
        return str;
    }

    private String getAccuracy(int accuracy) {
        String str = null;
        switch (accuracy) {
            case SmotionActivity.Info.ACCURACY_LOW:
                str = "Low";
                break;
            case SmotionActivity.Info.ACCURACY_MID:
                str = "Mid";
                break;
            case SmotionActivity.Info.ACCURACY_HIGH:
                str = "High";
                break;
            default:
                break;
        }
        return str;
    }

    private String getMode(int mode) {
        String str = null;
        switch (mode) {
            case SmotionActivity.Info.MODE_REALTIME:
                str = "Real time";
                break;
            case SmotionActivity.Info.MODE_BATCH:
                str = "Batch";
                break;
            case SmotionActivity.Info.MODE_ALL:
                str = "ALL";
                break;
            default:
                break;
        }
        return str;
    }

    StringBuffer checkActivityStatus() {
        StringBuffer sb = new StringBuffer();
        sb.append("Status valid check\n");
        sb.append("Stationary : "
                + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_STATIONARY) + "\n");
        sb.append("Walk : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_WALK)
                + "\n");
        sb.append("Run : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_STATIONARY)
                + "\n");
        sb.append("Vehicle : " + mActivity.isActivitySupported(SmotionActivity.Info.STATUS_RUN)
                + "\n");
        return sb;
    }

    private void startTimer() {
        if (mTimer == null) {
            mTimer = new Timer();
            mTimer.schedule(new MyTimer(), 0, mInterval);
        }
    }

    private void stopTimer() {
        if (mTimer != null) {
            mTimer.cancel();
            mTimer = null;
        }
    }

    private SmotionActivity.ChangeListener changeListener = new SmotionActivity.ChangeListener() {

        @Override
        public void onChanged(int mode, Info[] infoArray) {
            if (mIsPeriodicMode && (mode == SmotionActivity.Info.MODE_REALTIME))
                return;
            MotionTest.playSound();
            displayData(mode, infoArray);

        }
    };

    class MyTimer extends TimerTask {

        @Override
        public void run() {
            // TODO Auto-generated method stub
            mInfo[0] = mActivity.getInfo();
            handler.sendEmptyMessage(0);
        }
    }

    @SuppressLint("HandlerLeak")
    private final Handler handler = new Handler() {

        @Override
        public void handleMessage(android.os.Message msg) {
            // TODO Auto-generated method stub
            if (mInfo[0] != null) {
                MotionTest.playSound();
                displayData(SmotionActivity.Info.MODE_REALTIME, mInfo);
            }
        }
    };
}
