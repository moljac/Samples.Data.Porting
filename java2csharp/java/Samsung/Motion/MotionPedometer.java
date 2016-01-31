
package com.samsung.sdk.motion.test;

import com.samsung.android.sdk.motion.Smotion;
import com.samsung.android.sdk.motion.SmotionPedometer;
import com.samsung.android.sdk.motion.SmotionPedometer.Info;

import android.annotation.SuppressLint;
import android.os.Handler;
import android.os.Looper;

import java.util.Timer;
import java.util.TimerTask;

class MotionPedometer {

    private SmotionPedometer mPedometer = null;

    private final static String[] sResults = {
            "Calorie", "Distance", "Speed", "Total Count", "Run Flat Count", "Walk Flat Count",
            "Run Up Count", "Run Down Count", "Walk Up Count", "Walk Down Count"
    };

    private int mMode = MotionTest.MODE_PEDOMETER;

    private Timer mTimer;

    private SmotionPedometer.Info mInfo;

    private long mInterval = 10000;

    private boolean mIsUpDownAvailable;

    MotionPedometer(Looper looper, Smotion motion, boolean isUpDownAvailable) {
        mPedometer = new SmotionPedometer(looper, motion);
        mIsUpDownAvailable = isUpDownAvailable;
        initialize();
    }

    void start(int mode) {
        mMode = mode;

        initialize();
        mPedometer.start(changeListener);
        if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC) {
            startTimer();
        }
    }

    void stop() {
        mPedometer.stop();
        if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC) {
            stopTimer();
        }
        initialize();
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

    void initialize() {
        String status = "Ready";
        StringBuffer sb = new StringBuffer();
        for (int i = 0; i < sResults.length; i++) {
            if (i >= 6) {
                if (mIsUpDownAvailable) {
                    sb.append(sResults[i] + " : \n");
                }
            } else {
                sb.append(sResults[i] + " : \n");
            }
        }
        if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC
                || MotionTest.mTestMode == MotionTest.MODE_PEDOMETER_PERIODIC) {
            sb.append("Interval : ");
        }
        MotionTest.displayData(0, status, sb.toString());
    }

    private String getStatus(int status) {
        String str = null;
        switch (status) {
            case SmotionPedometer.Info.STATUS_WALK_UP:
                str = "Walk Up";
                break;
            case SmotionPedometer.Info.STATUS_WALK_DOWN:
                str = "Walk Down";
                break;
            case SmotionPedometer.Info.STATUS_WALK_FLAT:
                str = "Walk";
                break;
            case SmotionPedometer.Info.STATUS_RUN_DOWN:
                str = "Run Down";
                break;
            case SmotionPedometer.Info.STATUS_RUN_UP:
                str = "Run Up";
                break;
            case SmotionPedometer.Info.STATUS_RUN_FLAT:
                str = "Run";
                break;
            case SmotionPedometer.Info.STATUS_STOP:
                str = "Stop";
                break;
            case SmotionPedometer.Info.STATUS_UNKNOWN:
                str = "Unknown";
                break;
            default:
                break;
        }
        return str;
    }

    private SmotionPedometer.ChangeListener changeListener = new SmotionPedometer.ChangeListener() {

        @Override
        public void onChanged(Info info) {
            // TODO Auto-generated method stub
            if (mMode == MotionTest.MODE_PEDOMETER) {
                displayData(info);
            }
        }
    };

    private void displayData(Info info) {
        // TODO Auto-generated method stub
        long timestamp = System.currentTimeMillis();
        StringBuffer sb = new StringBuffer();
        double calorie = info.getCalorie();
        double distance = info.getDistance();
        double speed = info.getSpeed();
        long totalCount = info.getCount(SmotionPedometer.Info.COUNT_TOTAL);
        long runCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_FLAT);
        long walkCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_FLAT);
        long runUpCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_UP);
        long runDownCount = info.getCount(SmotionPedometer.Info.COUNT_RUN_DOWN);
        long walkUpCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_UP);
        long walkDownCount = info.getCount(SmotionPedometer.Info.COUNT_WALK_DOWN);

        sb.append(sResults[0] + " : " + calorie + "\n");
        sb.append(sResults[1] + " : " + distance + "\n");
        sb.append(sResults[2] + " : " + speed + "\n");
        sb.append(sResults[3] + " : " + totalCount + "\n");
        sb.append(sResults[4] + " : " + runCount + "\n");
        sb.append(sResults[5] + " : " + walkCount + "\n");
        if (mIsUpDownAvailable) {
            sb.append(sResults[6] + " : " + runUpCount + "\n");
            sb.append(sResults[7] + " : " + runDownCount + "\n");
            sb.append(sResults[8] + " : " + walkUpCount + "\n");
            sb.append(sResults[9] + " : " + walkDownCount + "\n");
        }
        if (mMode == MotionTest.MODE_PEDOMETER_PERIODIC
                || MotionTest.mTestMode == MotionTest.MODE_PEDOMETER_PERIODIC) {
            sb.append("Interval : " + mInterval / 1000 + " sec");
        }
        String str = getStatus(info.getStatus());

        if (str != null) {
            MotionTest.displayData(timestamp, str, sb.toString());
        }
    }

    class MyTimer extends TimerTask {

        @Override
        public void run() {
            // TODO Auto-generated method stub
            mInfo = mPedometer.getInfo();
            handler.sendEmptyMessage(0);
        }
    }

    @SuppressLint("HandlerLeak")
    private final Handler handler = new Handler() {
        @Override
        public void handleMessage(android.os.Message msg) {
            // TODO Auto-generated method stub
            if (mInfo != null) {
                MotionTest.playSound();
                displayData(mInfo);
            }
        }
    };
}
