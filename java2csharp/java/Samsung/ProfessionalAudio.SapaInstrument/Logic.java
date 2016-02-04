package com.samsung.audiosuite.sapainstrumentsample;

import com.samsung.android.sdk.professionalaudio.SapaProcessor;

/**
 * This class is responsible for common functionality for standalone mode and instance started from
 * other audio apps.
 */
class Logic {

    // Commands for communication with native part
    private static final String STOP_COMMAND = "/stop";
    private static final String PLAY_COMMAND = "/play";

    /**
     * This method send start playing command to native part.
     * 
     * @param processor
     *            Processor via which message is to be sent.
     */
    static void startPlaying(SapaProcessor processor) {
        processor.sendCommand(PLAY_COMMAND);
    }

    /**
     * This method send stop playing command to native part.
     * 
     * @param processor
     *            Processor via which message is to be sent.
     */
    static void stopPlaying(SapaProcessor processor) {
        processor.sendCommand(STOP_COMMAND);
    }
}
