package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.util.Log;

/**
 * @brief Log throwable objects
 */
public class LogUtils {

    /**
     * Log throwable object.
     *
     * @param tag
     * @param msg
     * @param ex
     */
    public static void throwable(String tag, String msg, Throwable ex) {
        StringBuilder stackTrace = new StringBuilder();
        stackTrace.append(msg)
                .append(": ")
                .append(ex.toString());

        if (null != ex.getMessage()) {
            stackTrace.append(" (")
                    .append(ex.getMessage())
                    .append(")");
        }

        stackTrace.append("\n");
        for (StackTraceElement ste : ex.getStackTrace()) {
            stackTrace.append("\tat ")
                    .append(ste.getClassName())
                    .append(".")
                    .append(ste.getMethodName())
                    .append(" (")
                    .append(ste.getFileName())
                    .append(":")
                    .append(ste.getLineNumber())
                    .append(")\n");
        }
        Log.e(tag, stackTrace.toString());
    }

}
