/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file PlayerController.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.musicplayer.controller;

import android.content.Context;
import android.net.Uri;

import android.util.Log;

/**
 * Class for controlling media playback.
 *
 * Abstraction layer that separates differences between local and remote
 * players. Switches between local and remote player by calling either
 * {@link #setLocalPlayer()} or {@link #setRemotePlayer(String, int)}.
 *
 * Notifies the listener when playback state is changed and offers basic
 * playback controls.
 */

public class PlayerController {
    /**
     * Possible playback states of player.
     */
    public static enum PlayerState {STOPPED, PAUSED, PLAYING, BUFFERING, INITIALIZING}

    /**
     * Callback interface for notyfing about changes and asynchronous calls.
     */
    public interface PlayerControllerEvents {

        /**
         * Called in response to {@link PlayerController#queryCurrentPositionAsync()}.
         * @param position current media mPosition.
         */
        void onCurrentPositionUpdated(int position);

        /**
         * Called when state of playback changes
         * @param currentState the state player is currently in.
         */
        void onStateChanged(PlayerState currentState);

        /**
         * Called when remote player has disconnected.
         */
        void onRemoteDisconnected();
    }

    /**
     * Context, needed for connectiong to AllShare Service.
     */
    final Context mContext;

    // Listener that should be notified of any events.
    private final PlayerControllerEvents mEventListener;


    // The local URI (file://) that should be played.
    private final Uri mContentUri;


    // Mime type of resource to be played.
    private final String mMimeType;


    // Current player instance.
    private ControllerType mControllerType;

    // Current playback state.
    private PlayerState mCurrentState = PlayerState.STOPPED;

    private int mDuration;
    private int mPosition;
    private int mVolume;
    private boolean mMute;

    public PlayerController(Context context, PlayerControllerEvents listener, Uri contentUri,
                            String mimetype, PlayerState playerState) {
        mContext = context;
        mEventListener = listener;
        mContentUri = contentUri;
        mMimeType = mimetype;
        mCurrentState = playerState;
    }

    /**
     * Queries for the current state of playback.
     * @return current state
     */
    public PlayerState getCurrentState() {
        return mCurrentState;
    }

    /**
     * Sets the current state of playback.
     *
     * Intended to be called from {@link ControllerType} implementations.
     * Calls {@link PlayerControllerEvents#onStateChanged(PlayerState)} if
     * new state is different from previous.
     *
     * Function is safe to call if new state is identical to previous state.
     *
     * @param newState the new playback state.
     */
    void setCurentState(PlayerState newState) {
        if (newState == mCurrentState) {
            // State has not changed, do nothing.
            return;
        }

        // Store the new state
        mCurrentState = newState;

        // Notify listener
        mEventListener.onStateChanged(newState);
    }

    /**
     * Queries for the movie duration.
     *
     * Note, that remote players might set this value some time after
     * opening media.
     * @return current media duration, in seconds.
     */
    public int getDuration() {
        return mDuration;
    }

    /**
     * Sets the current movie duration.
     *
     * Intended to be called from {@link ControllerType} implementations.
     *
     * @param duration the new movie duration.
     */
    void setDuration(int duration) {
        mDuration = duration;
    }

    /**
     * Queries for current position in media.
     *
     * In response to this method, the method {@link PlayerControllerEvents#onCurrentPositionUpdated(int)}
     * will be called with current position as argument.
     *
     * Note that the callback method may be called synchronously
     * or asynchronously, the user should not depend on queryCurrentPositionAsync()
     * finishing before calling onCurrentPositionUpdated(int)
     */
    public void queryCurrentPositionAsync() {
        // Relay to current ControllerType implementation
        mControllerType.queryCurrentPosition();
    }

    /**
     * Conveys answer to query for current media position.
     *
     * Intended to be called from {@link ControllerType} implementations.
     *
     * @param position the current media position in seconds
     */
    void notifyCurrentPosition(int position) {
        // Store it locally for use in restorePosition
        mPosition = position;

        // Provide response to listener
        mEventListener.onCurrentPositionUpdated(position);
    }

    /**
     * Conveys answer to query for current volume.
     *
     * Intended to be called from {@link ControllerType} implementations.
     *
     * @param volume the current volume between 0 and 100
     */
    void notifyCurrentVolume(int volume){
        this.mVolume = volume;
    }

    /**
     * Start playback of media, or resumes if media was paused.
     */
    public void play() {
        // Relay to current ControllerType implementation
        mControllerType.play();
    }

    /**
     * Pauses playback of media.
     */
    public void pause() {
        // Relay to current ControllerType implementation
        mControllerType.pause();
    }

    /**
     * Stops and resets playback.
     */
    public void stop() {
        // Relay to current ControllerType implementation
        mControllerType.stop();
    }

    /**
     * Switches playback to remote AllShare device.
     *
     * @param deviceId the unique identifier of device
     * @param devicetype the device type
     */
    public void setRemotePlayer(String deviceId, int devicetype) {
        // Release previous player
        if (mControllerType != null) {
            mControllerType.release();
        }

        Log.d("MediaPlayer", "setRemotePlayer");

        ControllerType previousControllerType = mControllerType;

        // Connect to remote player
        mControllerType = new RemotePlayer(this, mContentUri, mMimeType, deviceId, devicetype);

        // Restore current state and position
        restorePosition();

        // Releases previous player.
        if (previousControllerType != null) {
            previousControllerType.release();
        }
    }

    public boolean isRemotePlayer(){
        return mControllerType instanceof RemotePlayer;
    }

    /**
     * Switches playback to local media player.
     */
    public void setLocalPlayer() {
        if (mControllerType instanceof LocalPlayer) {
            // We are already local, do nothing
            return;
        }

        Log.d("MusicPlayerActivity", "setLocalPlayer");

        ControllerType previousControllerType = mControllerType;

        mControllerType = new LocalPlayer(this, mContentUri);
        restorePosition();

        // Releases previous player.
        if (previousControllerType != null) {
            previousControllerType.release();

            // Notify listener
            mEventListener.onRemoteDisconnected();
        }
    }

    /**
     * Seeks to given position in media.
     *
     * @param destination target in seconds
     */
    public void seek(int destination) {
        // Relay to current ControllerType implementation
        mControllerType.seek(destination);
    }

    /**
     * Sets a volume with value between 0 and 100
     * @param volume a volume to set
     */
    public void setVolume(int volume){
        mControllerType.setVolume(volume);
        mVolume = volume;
    }

    /**
     * Get current volume
     * @return The volume of the player
     */
    public int getVolume(){
        return mVolume;
    }

    /**
     * Mute or unmute the player
     * @param mMute true to mute the player
     */
    public void setMute(boolean mMute){
        this.mMute = mMute;
        mControllerType.setMute(mMute);
    }

    /**
     * Get current mute state
     * @return true if the player is muted
     */
    public boolean getMute(){
        return mMute;
    }

    /**
     * Release all resources used by player.
     *
     * The player is not usable after calling release.
     */
    public void release() {
        mControllerType.release();
        mControllerType = null;
    }

    /**
     * Restore the playback position and state of player.
     *
     * Used for seamless transition between local and remote player.
     *
     * Note, the position is only updated when the caller class queries
     * for position, so it depends on caller to do it regularly.
     */
    private void restorePosition() {
        mControllerType.seek(mPosition);
        mControllerType.setVolume(mVolume);
        mControllerType.setMute(mMute);
        if (mCurrentState == PlayerState.PLAYING) {
            mControllerType.play();
        }
    }

}
