/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file LocalPlayer.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.musicplayer.controller;

import java.io.IOException;

import android.media.MediaPlayer;
import android.net.Uri;
import com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;


/**
 * Controller for local media player on Android device.
 *
 * This class uses the Android MediaPlayer class for playback.
 *
*/
public class LocalPlayer implements ControllerType {


    // The controlled local player.
    private final MediaPlayer mPlayer;


    // Reference to PlayerController for providing responses and state updates.
    private final PlayerController mController;

    LocalPlayer(PlayerController controller, Uri content) {
        this.mController = controller;

        // Create and initialize Android MediaPlayer instance.
        mPlayer = new MediaPlayer();
        try {
            mPlayer.setDataSource(controller.mContext, content);
            mPlayer.prepare();
            controller.setDuration(mPlayer.getDuration() / 1000);
        } catch (IOException ignored) {
            // Ignored for application brevity
        }

    }

    @Override
    public void queryCurrentPosition() {
        mController.notifyCurrentPosition(mPlayer.getCurrentPosition() / 1000);
    }

    @Override
    public void play() {
        mPlayer.start();
        mController.setCurentState(PlayerState.PLAYING);
    }

    @Override
    public void pause() {
        if (mController.getCurrentState() == PlayerState.STOPPED) {
            mPlayer.start();
        }
        mPlayer.pause();
        mController.setCurentState(PlayerState.PAUSED);
    }

    @Override
    public void stop() {
        try {
            mPlayer.stop();
            mPlayer.prepare();

            // Contrary to documentation, MediaPlayer does not reset position
            // after hide & prepare.
            mPlayer.seekTo(0);
            mController.notifyCurrentPosition(0);
        } catch (IllegalStateException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
        mController.setCurentState(PlayerState.STOPPED);
    }

    @Override
    public void seek(int destination) {
        mPlayer.seekTo(destination * 1000);
    }

    @Override
    public void setVolume(int vol) {
        //not used here
    }

    @Override
    public void setMute(boolean mute) {
        float vol = mute?0:1;
        mPlayer.setVolume(vol, vol);
    }

    @Override
    public void release() {
        mPlayer.release();
    }

}
