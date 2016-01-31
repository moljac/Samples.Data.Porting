/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file RemotePlayer.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.musicplayer.controller;

import android.net.Uri;
import com.samsung.android.sdk.mediacontrol.*;
import com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;

import android.util.Log;

/**
 * Controller for AllShare controlled media player.
 * <p/>
 * This class handles all interactions with AllShare Service and remote
 * device.
 * <p/>
 * The resource to be played has to be Uri of a local file.
 * <p/>
 * This sample has basic error recovery, in case of any errors it switches to
 * local player.
 */
public class RemotePlayer implements ControllerType, SmcAvPlayer.EventListener, SmcAvPlayer.ResponseListener, SmcDeviceFinder.StatusListener {

    // Reference to PlayerController for providing responses and state updates
    private final PlayerController mController;

    // The Uri to be played on remote AllShare device.
    private final Uri mContentUri;

    // The mime type of resource.
    private final String mMimeType;

    // The unique id of remote AllShare device.
    private final String mDeviceId;

    // The device type of remote AllShare device.
    private final int mDeviceType;

    // Remote controlled AllShare device.
    private SmcAvPlayer mPlayer;

    // Playback position that should be set when player connects.
    private int mPosition;

    // The destination playback state, used before player is connected.
    private PlayerState mStateToGo;

    // The current playback state.
    private PlayerState mCurrentState = PlayerState.STOPPED;

    // Device finder reference
    private SmcDeviceFinder mDeviceFinder;

    private boolean mFirstPlay = true;


    RemotePlayer(PlayerController controller, Uri contentUri,
                 String mimetype, String deviceId, int deviceType) {
        this.mController = controller;
        this.mContentUri = contentUri;
        this.mMimeType = mimetype;
        this.mDeviceId = deviceId;
        this.mDeviceType = deviceType;

        // Connect to AllShare Service. Processing continues when
        // service instance is received in onCreated callback method.
        SmcDeviceFinder df = new SmcDeviceFinder(controller.mContext);
        df.setStatusListener(this);
        df.start();
    }

    /////////////////////////////////////////////////////////////////////////////////
    // SmcDeviceFinder.StatusListener implementation
    /////////////////////////////////////////////////////////////////////////////////

    @Override
    public void onStarted(SmcDeviceFinder deviceFinder, int error) {
        mDeviceFinder = deviceFinder;
        mPlayer = (SmcAvPlayer) mDeviceFinder.getDevice(mDeviceType, mDeviceId);
        if(mPlayer == null) {
            mController.setLocalPlayer();
            return;
        }

        if (mController.getCurrentState() != PlayerState.STOPPED) {
            registerListeners();
        }

        mPlayer.requestVolumeInfo();
        mPlayer.requestStateInfo();
        mPlayer.requestMuteInfo();
    }

    @Override
    public void onStopped(SmcDeviceFinder deviceFinder) {
        if(mDeviceFinder == deviceFinder) {
            mDeviceFinder.setStatusListener(null);
            mDeviceFinder = null;
        }
    }
    // End of SmcDeviceFinder.StatusListener implementation.

    private void registerListeners() {
        mPlayer.setEventListener(this);
        mPlayer.setResponseListener(this);
    }

    private void unregisterListeners() {
        mPlayer.setEventListener(null);
        mPlayer.setResponseListener(null);
    }

    /**
     * Asynchronously call for current playback media position.
     * The response is returned via onGetPlayPositionResponse callback method.
     */
    @Override
    public void queryCurrentPosition() {
        if (mPlayer != null && mCurrentState == PlayerState.PLAYING) {
            mPlayer.requestPlayPosition();
        }
    }

    /**
     * Starts or resumes madia playback/
     */
    @Override
    public void play() {
        if (mPlayer == null) {
            // Player is not yet connected, store that we should start playback when it connects.
            mStateToGo = PlayerState.PLAYING;
            return;
        }

        switch (mCurrentState) {
            case STOPPED:
                // Player was stopped, we have to provide content and starting position.
                playItem();
                break;
            case PAUSED:
                // Player was paused, resume playback.
                mPlayer.resume();
                break;
            default:
            	break;
        }
    }

    private void playItem(){
        registerListeners();

        setState(PlayerState.INITIALIZING);

        SmcItem item = new SmcItem(new SmcItem.LocalContent(mContentUri.getPath(),mMimeType));
        // If seek() was called before player connected, it stored the position
        // and we can start playback from that position.
        SmcAvPlayer.PlayInfo info = new SmcAvPlayer.PlayInfo(mPosition);
        mPlayer.play(item, info);
        Log.d("MusicPlayer", "Starting playback at: "+mPosition);
    }

    @Override
    public void pause() {
        if (mPlayer != null) {
            mPlayer.pause();
        } else {
            // Player is not yet connected, store that we should pause
            // playback when it connects.
            mStateToGo = PlayerState.PAUSED;
        }
    }

    @Override
    public void stop() {
        if (mPlayer != null) {
            mPlayer.stop();
            mPosition = 0;
        } else {
            // Player is not yet connected, store that we should hide
            // playback when it connects.
            mStateToGo = PlayerState.STOPPED;
        }
    }

    @Override
    public void seek(int destination) {
        if(mPosition==destination)
            return;
        // Store the requested position, it will be used if player
        // is not yet connected.
        mPosition = destination;
        if (mPlayer != null) {
            mPlayer.seek(destination);
            setState(PlayerState.BUFFERING);
        }
        Log.d("MusicPlayer", "Seek: "+destination+", "+mPlayer);
    }

    @Override
    public void setVolume(int vol) {
        if (mPlayer != null)
            mPlayer.setVolume(vol);
    }

    @Override
    public void setMute(boolean mute) {
        if (mPlayer != null)
            mPlayer.setMute(mute);
    }

    @Override
    public void release() {
        // Disconnect from remote player
        if (mPlayer != null) {
            unregisterListeners();
        }

        // Release the AllShare Service instance.
        if(mDeviceFinder != null) {
            mDeviceFinder.setDeviceListener(SmcDevice.TYPE_AVPLAYER, null);
            mDeviceFinder.stop();
        }

    }

    /**
     * Callback when AllShare device state changes
     */
    @Override
    public void onDeviceChanged(SmcAvPlayer device, int state, int err) {
        Log.d("MusicPlayer", "onDeviceChanged: " + state);

        if (err != Smc.SUCCESS) {
            // Error has occurred, switch to local player.
            mController.setLocalPlayer();
            return;
        }

        // We use simplified state set, so map from states used in service
        // to states used locally.
        switch (state) {
            case SmcAvPlayer.STATE_PAUSED:
                setState(PlayerState.PAUSED);
                break;
            case SmcAvPlayer.STATE_PLAYING:
                setState(PlayerState.PLAYING);
                if(mFirstPlay){
                    mPlayer.seek(mPosition);
                    mFirstPlay = false;
                }
                
                mPosition = 0;
                
                break;
            case SmcAvPlayer.STATE_BUFFERING:
                setState(PlayerState.BUFFERING);
                break;
            case SmcAvPlayer.STATE_STOPPED:
                if(mCurrentState==PlayerState.INITIALIZING){
                    playItem();
                }else{
                    setState(PlayerState.STOPPED);
                }
                break;
            case SmcAvPlayer.STATE_CONTENT_CHANGED:
                //mController.setLocalPlayer();
                //TODO
                break;
            default:
                setState(PlayerState.STOPPED);
        }
    }

    /**
     * Callback for response to requestPlayPosition device method.
     */
    @Override
    public void onRequestPlayPosition(SmcAvPlayer device, long position, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred
            return;
        }
        // Notify controller of current position
        if(position!=0)
            mController.notifyCurrentPosition((int) position);
        if(Math.abs(position - mPosition)<2 && mCurrentState == PlayerState.BUFFERING){
            setState(PlayerState.PLAYING);
        }
    }

    @Override
    public void onRequestStateInfo(SmcAvPlayer device, int state, int err) {
        onDeviceChanged(device, state, err);
    }

    @Override
    public void onRequestMediaInfo(SmcAvPlayer device, SmcAvPlayer.MediaInfo mediaInfo, int error) {
        //Not used in this sample.
    }

    /**
     * Callback for response to pause() device method.
     */
    @Override
    public void onPause(SmcAvPlayer device, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred.
            return;
        }

        // Notify controller of state change
        setState(PlayerState.PAUSED);
    }

    /**
     * Callback for response to play() device method.
     */
    @Override
    public void onPlay(SmcAvPlayer device, SmcItem ai, SmcAvPlayer.PlayInfo ci, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred.
            return;
        }

        // Notify controller of state change
        setState(PlayerState.PLAYING);
    }

    /**
     * Callback for response to resume() device method.
     */
    @Override
    public void onResume(SmcAvPlayer device, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred.
            return;
        }

        // Notify controller of state change
        setState(PlayerState.PLAYING);
    }

    /**
     * Callback for response to seek() device method.
     */
    @Override
    public void onSeek(SmcAvPlayer device, long requestedPosition, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred.
            return;
        }

        // Action successful, no further processing needed.
    }

    /**
     * Callback for response to hide() device method.
     */
    @Override
    public void onStop(SmcAvPlayer device, int err) {
        if (err != Smc.SUCCESS) {
            // Error has occurred.
            return;
        }

        // Notify controller of state change
        mController.notifyCurrentPosition(0);

        unregisterListeners();

        setState(PlayerState.STOPPED);
        mPosition = 0;
    }

    @Override
    public void onRequestVolumeInfo(SmcAvPlayer device, int i, int error) {
        mController.notifyCurrentVolume(i);
    }


    @Override
    public void onRequestMuteInfo(SmcAvPlayer device, boolean b, int error) {

    }


    /**
     * Provide common processing for state changing logic.
     *
     * @param newState the new playback state
     */
    private void setState(PlayerState newState) {
        Log.d("MusicPlayer", "New state: "+newState);

        // Notify controller of state change
        mController.setCurentState(newState);

        // Store the current state locally
        mCurrentState = newState;

        if (newState == mStateToGo) {
            // We have reached the state we were supposed to reach, so we can
            // clear the flag
            mStateToGo = null;
        } else if (newState == PlayerState.PLAYING && mStateToGo == PlayerState.PAUSED) {
            // If paused state is requested, we have to go through playing
            // This is detected here and pause is finally called.
            pause();
        } else if(mStateToGo==PlayerState.PLAYING){
            play();
        }
    }


	@Override
	public void onSetVolume(SmcAvPlayer device, int level, int error) {
		
	}


	@Override
	public void onSetMute(SmcAvPlayer device, boolean onOff, int error) {
		
	}


}
