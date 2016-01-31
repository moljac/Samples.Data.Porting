/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file ControllerType.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.videoplayer.controller;

/**
 * Interface for controlling various types of players (remote or local).
 *
*/
public interface ControllerType {

    /**
     * Queries for current position in media asynchronously.
     */
    void queryCurrentPosition();

    /**
     * Starts playback of media, or resumes if media was paused.
     */
    void play();

    /**
     * Pauses playback of media.
     */
    void pause();

    /**
     * Stops and resets playback.
     */
    void stop();

    /**
     * Seeks to given position in media.
     *
     * @param destination target in seconds
     */
    void seek(int destination);

    /**
     * Sets a new volume.
     * @param vol a volume to set between 0 and 100
     */
    void setVolume(int vol);

    /**
     * Mutes or unmutes a device
     * @param mute if true, a device will be muted
     */
    void setMute(boolean mute);

    /**
     * Releases all resources used by player.
     *
     * The player is not usable after calling release.
     */
    void release();
}
