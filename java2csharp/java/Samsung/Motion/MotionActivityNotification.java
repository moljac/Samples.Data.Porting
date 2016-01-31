
package com.samsung.sdk.motion.test;

import com.samsung.android.sdk.motion.Smotion;
import com.samsung.android.sdk.motion.SmotionActivityNotification;
import com.samsung.android.sdk.motion.SmotionActivityNotification.Info;
import com.samsung.android.sdk.motion.SmotionActivityNotification.InfoFilter;

import android.os.Looper;
import android.text.format.Time;

import java.util.Formatter;

public class MotionActivityNotification {

    private SmotionActivityNotification mActivityNotification;

    private InfoFilter mFilter;

    MotionActivityNotification(Looper looper, Smotion motion) {
        // TODO Auto-generated constructor stub
        mActivityNotification = new SmotionActivityNotification(looper, motion);
        mFilter = null;
        initialize();
    }

    void start() {
        initialize();
        if (mFilter != null) {
            mActivityNotification.start(mFilter, changeListener);
        }
    }

    void stop() {
        if (mFilter != null) {
            mActivityNotification.stop();
        }
        mFilter = null;
        initialize();
    }

    void addActivity(int activity_type) {
        if (mFilter == null) {
            mFilter = new InfoFilter();
        }
        mFilter.addActivity(activity_type);
    }

    void initialize() {
        StringBuffer sb = new StringBuffer();
        sb.append("Ready");
        MotionTest.displayData(0, sb.toString());
    }

    private String getStatus(int status) {
        String str = null;
        switch (status) {
            case SmotionActivityNotification.Info.STATUS_UNKNOWN:
                str = "Unknown";
                break;
            case SmotionActivityNotification.Info.STATUS_STATIONARY:
                str = "Stationary";
                break;
            case SmotionActivityNotification.Info.STATUS_WALK:
                str = "Walk";
                break;
            case SmotionActivityNotification.Info.STATUS_RUN:
                str = "Run";
                break;
            case SmotionActivityNotification.Info.STATUS_VEHICLE:
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
            case SmotionActivityNotification.Info.ACCURACY_LOW:
                str = "Low";
                break;
            case SmotionActivityNotification.Info.ACCURACY_MID:
                str = "Mid";
                break;
            case SmotionActivityNotification.Info.ACCURACY_HIGH:
                str = "High";
                break;
            default:
                break;
        }
        return str;
    }

    private void displayData(Info info) {
        StringBuffer sb = new StringBuffer();
        String str = null;
        long timestamp = info.getTimeStamp();
        Time time = new Time();
        time.set(timestamp);
        Formatter form = new Formatter();
        form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
        sb.append("[" + form.toString() + "] ");
        form.close();
        str = getStatus(info.getStatus());

        sb.append("(" + getAccuracy(info.getAccuracy()) + ")");
        if (str != null) {
            MotionTest.displayData(0, str, sb.toString());
        }
    }

    StringBuffer checkActivityStatus() {
        StringBuffer sb = new StringBuffer();
        sb.append("Status valid check\n");
        sb.append("Stationary : "
                + mActivityNotification
                .isActivitySupported(SmotionActivityNotification.Info.STATUS_STATIONARY)
                + "\n");
        sb.append("Walk : "
                + mActivityNotification
                .isActivitySupported(SmotionActivityNotification.Info.STATUS_WALK) + "\n");
        sb.append("Run : "
                + mActivityNotification
                .isActivitySupported(SmotionActivityNotification.Info.STATUS_STATIONARY)
                + "\n");
        sb.append("Vehicle : "
                + mActivityNotification
                .isActivitySupported(SmotionActivityNotification.Info.STATUS_RUN) + "\n");
        return sb;
    }

    private SmotionActivityNotification.ChangeListener changeListener = new SmotionActivityNotification.ChangeListener() {

        @Override
        public void onChanged(Info info) {
            // TODO Auto-generated method stub
            MotionTest.playSound();
            displayData(info);
        }
    };
}
