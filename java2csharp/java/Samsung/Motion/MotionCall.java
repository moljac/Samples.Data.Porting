
package com.samsung.sdk.motion.test;

import com.samsung.android.sdk.motion.Smotion;
import com.samsung.android.sdk.motion.SmotionCall;
import com.samsung.android.sdk.motion.SmotionCall.Info;

import android.os.Looper;

class MotionCall {
    private SmotionCall mCall = null;

    MotionCall(Looper looper, Smotion motion) {
        mCall = new SmotionCall(looper, motion);
    }

    void start() {
        initialize();
        mCall.start(changeListener);
    }

    void stop() {
        initialize();
        mCall.stop();
    }

    void initialize() {
        StringBuffer sb = new StringBuffer();
        sb.append("Ready");
        MotionTest.displayData(0, sb.toString());
    }

    private void displayData(Info info) {
        StringBuffer sb = new StringBuffer();
        long timestamp = System.currentTimeMillis();
        switch (info.getCallPosition()) {
            case SmotionCall.POSITION_LEFT:
                sb.append("Left");
                break;
            case SmotionCall.POSITION_RIGHT:
                sb.append("Right");
                break;
            default:
                break;
        }
        MotionTest.displayData(timestamp, sb.toString());
    }

    private SmotionCall.ChangeListener changeListener = new SmotionCall.ChangeListener() {

        @Override
        public void onChanged(Info info) {
            // TODO Auto-generated method stub
            MotionTest.playSound();
            displayData(info);
        }
    };
}
