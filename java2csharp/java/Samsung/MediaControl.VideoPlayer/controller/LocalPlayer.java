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

package com.samsung.android.sdk.sample.videoplayer.controller;

import java.io.IOException;

import android.media.MediaPlayer;
import android.media.MediaPlayer.TrackInfo;
import android.media.TimedText;
import android.media.MediaPlayer.OnTimedTextListener;
import android.net.Uri;
import android.util.Log;
import android.view.SurfaceHolder;
import android.view.SurfaceHolder.Callback;
import android.view.SurfaceView;

import com.samsung.android.sdk.sample.videoplayer.controller.PlayerController.PlayerState;

/**
 * Controller for local media player on Android device.
 *
 * This class uses the Android MediaPlayer class for playback.
 *
*/
public class LocalPlayer implements ControllerType, Callback, OnTimedTextListener,
        MediaPlayer.OnCompletionListener {

    // Reference to PlayerController for providing responses and state updates.
    private final PlayerController mController;
    private SurfaceHolder mSurfaceHolder;

    // The controlled local player.
    private final MediaPlayer mPlayer;

    private boolean mIsPlayerReleased = false;

    LocalPlayer(PlayerController controller, Uri content, Uri subtitlesUri, SurfaceView videoView) {
        mController = controller;

        // Create and initialize Android MediaPlayer instance.
        mPlayer = new MediaPlayer();
        mPlayer.setOnCompletionListener(this);

        mSurfaceHolder = videoView.getHolder();
        mSurfaceHolder.addCallback(this);

        boolean isSurfaceValid = mSurfaceHolder.getSurface().isValid();

        if (isSurfaceValid) {
            mPlayer.setDisplay(mSurfaceHolder);
        }

        try {
            mPlayer.setDataSource(controller.mContext, content);

            mPlayer.prepare();
            if (subtitlesUri != null) {
                mPlayer.addTimedTextSource(subtitlesUri.getPath(),
                        MediaPlayer.MEDIA_MIMETYPE_TEXT_SUBRIP);
                mPlayer.setOnTimedTextListener(this);
            }
            controller.setDuration(mPlayer.getDuration() / 1000);
        } catch (IOException ignored) {
            Log.e("VideoPlayer", ignored.getMessage());
        }
    }

    @Override
    public void queryCurrentPosition() {
        mController.notifyCurrentPosition(mPlayer.getCurrentPosition() / 1000);
    }

    @Override
    public void play() {
        mPlayer.start();

        TrackInfo[] trackInfo = mPlayer.getTrackInfo();
        int timedTextTrack = findTimedTextTrack(trackInfo);

        if (timedTextTrack > 0) {
            mPlayer.selectTrack(timedTextTrack);
        }

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

            // Contrary to documentation, MediaPlayer does not reset position after hide & prepare.
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
        mIsPlayerReleased = true;
        mSurfaceHolder.removeCallback(this);
    }

    @Override
    public void surfaceCreated(SurfaceHolder holder) {
        mPlayer.setDisplay(holder);

        // Forces player to redraw its content on new surface.
        if (mController.getCurrentState() == PlayerState.PAUSED) {
            mPlayer.seekTo(mPlayer.getCurrentPosition());
        }
    }

    @Override
    public void surfaceChanged(SurfaceHolder holder, int format, int width, int height) {
        mPlayer.setDisplay(holder);

        // Forces player to redraw its content on new surface.
        if (mController.getCurrentState() == PlayerState.PAUSED) {
            mPlayer.seekTo(mPlayer.getCurrentPosition());
        }
    }

    @Override
    public void surfaceDestroyed(SurfaceHolder holder) {
        // The surface only gets destroyed when activity is destroyed.
        // At this point the player is already released, so no need to clean up here.
        if (!mIsPlayerReleased) {
            mPlayer.setDisplay(null);
        }
    }

    /**
     * Listener that is invoked where a suitable subtitle should be loaded.
     * When there no text should be displayed - then TimedText is a null.
     */
    @Override
    public void onTimedText(MediaPlayer mediaPlayer, TimedText text) {
        if (text != null) {
            mController.notifySubtitles(text.getText());
        }
    }

    @Override
    public void onCompletion(MediaPlayer mediaPlayer) {
        if (mPlayer == mediaPlayer) {
            mController.setCurentState(PlayerState.STOPPED);
        }
    }

    /**
     * Finds if there is a TimedTrack attached.
     */
    private int findTimedTextTrack(TrackInfo[] trackInfo) {
        int index = -1;
        for (int i = 0; i < trackInfo.length; i++) {
            if (trackInfo[i].getTrackType() == TrackInfo.MEDIA_TRACK_TYPE_TIMEDTEXT) {
                return i;
            }
        }
        return index;
    }
}
