package com.samsung.audiosuite.sapaeffectsample;

import android.util.Log;

import com.samsung.android.sdk.professionalaudio.SapaProcessor;

/**
 * This class is responsible for common functionality for standalone mode and instance started from
 * other audio apps.
 */
class Logic {

    private static final String TAG = "audiosuite:sapaeffectsample:j:Logic";

    // Commands for communication with native part
    private static final String VOLUME_COMMAND = "/volume:";

    final static int DEFAULT_VOLUME = -10;
    private final static int MAX_VOLUME = 0;
    private final static int MIN_VOLUME = -30;
    private final static int DELTA_VOLUME = 1;

    /**
     * This method is responsible to send volume message via given processor.
     * 
     * @param processor
     *            Processor via which message is to be sent.
     * @param volume
     *            Volume to be sent.
     */
    static void sendVolume(SapaProcessor processor, int volume) {
        processor.sendCommand(VOLUME_COMMAND + volume);
        Log.d(TAG, "Sending command: " + VOLUME_COMMAND + volume);
    }

    /**
     * This method checks whether given value is a minimum accepted value of volume.
     * 
     * @param volume
     *            volume to be checked.
     * @return true when given value of volume is not bigger from the minimum volume; false
     *         otherwise.
     */
    static boolean isMinVolume(int volume) {
        return volume <= MIN_VOLUME;
    }

    /**
     * This method checks whether given value is a maximum accepted value of volume.
     * 
     * @param volume
     *            volume to be checked.
     * @return true when given value of volume is not smaller from the maximum volume; false
     *         otherwise.
     */
    static boolean isMaxVolume(int volume) {
        return volume >= MAX_VOLUME;
    }

    /**
     * This method returns decreased by DELTA_VOLUME value of volume.
     * 
     * @param volume
     *            Value of volume to be decreased.
     * @return Decreased by DELTA_VOLUME value of volume.
     */
    static int decreaseVolume(int volume) {
        return volume - DELTA_VOLUME;
    }

    /**
     * This method returns increased by DELTA_VOLUME value of volume.
     * 
     * @param volume
     *            Value of volume to be increased.
     * @return Increased by DELTA_VOLUME value of volume.
     */
    static int increaseVolume(int volume) {
        return volume + DELTA_VOLUME;
    }

    /**
     * This method returns text with value of volume.
     * 
     * @param volume
     *            Value of volume to be shown.
     * @return Text with value of volume.
     */
    static String getVolumeText(int volume) {
        return ("" + volume + " dB");
    }
}
