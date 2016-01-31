/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file MusicPlayerActivity.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.musicplayer;

import android.app.Activity;
import android.app.ProgressDialog;
import android.content.ContentUris;
import android.content.DialogInterface;
import android.content.res.AssetManager;
import android.database.Cursor;
import android.media.MediaScannerConnection;
import android.media.MediaScannerConnection.OnScanCompletedListener;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.Message;
import android.util.Log;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.webkit.MimeTypeMap;
import android.widget.*;
import android.widget.SeekBar.OnSeekBarChangeListener;
import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.mediacontrol.Smc;
import com.samsung.android.sdk.mediacontrol.SmcDevice;
import com.samsung.android.sdk.sample.musicplayer.controller.PlayerController;
import com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;
import com.samsung.android.sdk.sample.musicplayer.devicepicker.DevicePicker;

import java.io.*;

/**
 * Main activity class of sample music player.
 */
public class MusicPlayerActivity extends Activity implements OnSeekBarChangeListener,
        DevicePicker.DevicePickerResult, OnClickListener, PlayerController.PlayerControllerEvents, OnScanCompletedListener {

    private static final String ASSETS_SUBDIR = "AllShareMusicPlayer";
    private static final String MUSIC_FILE = "samsung.mp3";
    private static final String[] ALL_ASSETS = {MUSIC_FILE};

    // activity UI related fields
    private boolean mIsActivityVisible = false;
    private ImageView mCoverArt;
    private TextView mTitleText;
    private TextView mArtistText;
    private TextView mPositionText;
    private TextView mDurationText;
    private SeekBar mSeekBar;
    private ImageButton mPlayButton;
    private ImageButton mMuteButton;
    private DevicePicker mDevicePicker;
    private ProgressDialog mProgressDialog;

    // and non-UI fields
    private PlayerController mPlayerController;
    private Handler mUpdateEventHandler = new Handler(new PositionUpdater());
    private MusicPlayerState mState;

    private Smc mSmcLib;

    /**
     * Called when the activity is first created.
     */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.music_player_layout);
        setupViewElements();

        mSmcLib = new Smc();

        try {
            mSmcLib.initialize(getBaseContext());
        } catch (SsdkUnsupportedException e) {
            e.printStackTrace();  //TODO Handle exceptions.
        }

        Log.d("media control lib version", mSmcLib.getVersionName());

        // Prepare assets used by sample
        extractAssets();

        // Restore saved state, after e.g. rotating
        if (savedInstanceState != null) {
            mState = new MusicPlayerState(savedInstanceState);
        } else {
            mState = new MusicPlayerState();

            // Initialize state with built-in content
            File storageDir = new File(Environment.getExternalStorageDirectory(), ASSETS_SUBDIR);
            File audio = new File(storageDir, MUSIC_FILE);

            //To get media information (title, artist, cover)
            MediaScannerConnection.scanFile(getApplicationContext(),
                    new String[]{audio.getAbsolutePath()},
                    null, this);

            Uri mediaUri = Uri.fromFile(audio);
            mState.mMediaUri = mediaUri;
            String path = mediaUri.getPath();
            if (path != null && path.lastIndexOf(".") != -1) {
                String ext = path.substring(path.lastIndexOf(".") + 1);
                mState.mMimeType = MimeTypeMap.getSingleton().getMimeTypeFromExtension(ext);
            }
        }

    }

    @Override
    protected void onStart() {
        super.onStart();

        Log.d("MusicPlayer", "onStart");

        mIsActivityVisible = true;

        // Setup the player and related elements
        setupPlayer();
    }

    @Override
    protected void onStop() {
        super.onStop();

        Log.d("MusicPlayer", "onHide");

        mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
        mIsActivityVisible = false;
        if(isFinishing() || !mPlayerController.isRemotePlayer()){
            mPlayerController.stop();
            mPlayerController.release();
            mPlayerController = null;
        } else {
            mPlayerController.pause(); // make pause in case of going to home screen
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        mState.writeToBundle(outState);

        super.onSaveInstanceState(outState);
    }

    /**
     * Sets the global references to UI elements and event handlers for those elements.
     */
    private void setupViewElements() {
        mTitleText = (TextView) findViewById(R.id.titleText);
        mArtistText = (TextView) findViewById(R.id.artistText);
        mCoverArt = (ImageView) findViewById(R.id.coverImage);
        mSeekBar = (SeekBar) findViewById(R.id.musicPosition);
        mPlayButton = (ImageButton) findViewById(R.id.playPause);
        mMuteButton = (ImageButton) findViewById(R.id.mute);
        mPositionText = (TextView) findViewById(R.id.positionText);
        mDurationText = (TextView) findViewById(R.id.durationText);

        mSeekBar.setOnSeekBarChangeListener(this);
        mPlayButton.setOnClickListener(this);
        mMuteButton.setOnClickListener(this);
        mPositionText.setText("00:00");

        mProgressDialog = new ProgressDialog(this);
        mProgressDialog.setMessage("Buffering...");
        mProgressDialog.setCancelable(true);
        mProgressDialog.setOnCancelListener(new DialogInterface.OnCancelListener() {
            @Override
            public void onCancel(DialogInterface dialog) {
                mPlayerController.stop();
            }
        });
        
        View stopButton = findViewById(R.id.stop);
        stopButton.setOnClickListener(this);

        mDevicePicker = (DevicePicker) getFragmentManager().findFragmentById(R.id.playerPicker);
        mDevicePicker.setDeviceType(SmcDevice.TYPE_AVPLAYER);
        mDevicePicker.setDeviceSelectedListener(this);
    }

    /**
     * Sets up the media player controller and media information in UI.
     */
    private void setupPlayer() {
        // Display artist, album name and cover art.
        mTitleText.setText(mState.mTitle);
        mArtistText.setText(mState.mArtist);
        mCoverArt.setImageURI(mState.mAlbumArtUri);

        // Start PlayerController and setup local or remote player.
        mPlayerController = new PlayerController(this, this, mState.mMediaUri, mState.mMimeType, mState.mPlayback);

        Log.d("MusicPlayer", "setupPlayer");

        mPlayerController.setLocalPlayer();
        if (mState.mDeviceId == null || mState.mDeviceId.length() == 0) {
            mPlayerController.setLocalPlayer();
        } else {
            mPlayerController.setRemotePlayer(mState.mDeviceId, SmcDevice.TYPE_AVPLAYER);
            mDevicePicker.setActive(true);
        }

        // Seek to given position if requested
        if (mState.mPosition != 0) {
            mPlayerController.seek(mState.mPosition);
        }

        // Starts playback if requested.
        if (mState.mPlayback == PlayerController.PlayerState.PLAYING) {
            mPlayerController.play();
        }

        setMute(mState.mMute);

        // Set seek bar to media length (in seconds).
        // First update via mUpdateEventHandler will set current position.
        mSeekBar.setMax(mState.mDuration);
        mDurationText.setText(convertTime2String(mState.mDuration));
        mPositionText.setText(convertTime2String(mState.mPosition));
        onStateChanged(mState.mPlayback);
    }

    private void updateStatusMessage(int message) {
        if (mIsActivityVisible) {
            Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
        }
    }

    private void updateInformationUi() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                mTitleText.setText(mState.mTitle);
                mArtistText.setText(mState.mArtist);
                mCoverArt.setImageURI(mState.mAlbumArtUri);
                mSeekBar.setMax(mState.mDuration);
                mDurationText.setText(convertTime2String(mState.mDuration));
            }
        });
    }

    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the device picker events
    // ////////////////////////////////////////////////////////////////////////

    /**
     * User selected remote player, connect to that device and start playback.
     */
    @Override
    public void onDeviceSelected(SmcDevice device) {
        if (mState.mDeviceId == null) {
            updateStatusMessage(R.string.allshare_connecting);
            mState.mDeviceId = device.getId();
            mPlayerController.setRemotePlayer(device.getId(), SmcDevice.TYPE_AVPLAYER);
        }
    }

    /**
     * User selected to disable AllShare, so disconnect
     * and switch to local player.
     */
    @Override
    public void onAllShareDisabled() {
        mState.mDeviceId = null;
        mPlayerController.stop();
        mPlayerController.setLocalPlayer();
    }


    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the seek bar events
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onProgressChanged(SeekBar seekBar, int progress,
                                  boolean fromUser) {
        // Do nothing
    }

    @Override
    public void onStartTrackingTouch(SeekBar seekBar) {
        // Block refreshing seek bar, to avoid flickering.
        mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
    }

    @Override
    public void onStopTrackingTouch(SeekBar seekBar) {
        // Seek to requested position
        int position = seekBar.getProgress();
        mPlayerController.seek(position);
        mState.mPosition = position;
        // Re-enable refreshing current position.
        mUpdateEventHandler.sendEmptyMessage(PositionUpdater.START_POLLING);
    }

    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the PlayerController events
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onCurrentPositionUpdated(int position) {
        if (mState.mPlayback == PlayerState.STOPPED) {
            // We might have received a position update after stopping
            // ignore it in that case and set to zero
            mSeekBar.setProgress(0);
            mState.mPosition = 0;
            mPositionText.setText(convertTime2String(0));
            return;
        }
        if(mState.mPlayback == PlayerState.BUFFERING || mState.mPlayback == PlayerState.INITIALIZING)
            return;

        mSeekBar.setProgress(position);
        mPositionText.setText(convertTime2String(position));
        mState.mPosition = position;
    }

    @Override
    public void onStateChanged(PlayerState currentState) {
        mState.mPlayback = currentState;
        if (currentState == PlayerState.PLAYING) {
            mPlayButton.setImageResource(R.drawable.ic_media_pause);
            mUpdateEventHandler.sendEmptyMessage(PositionUpdater.START_POLLING);
        } else {
            mPlayButton.setImageResource(R.drawable.ic_media_play);
            mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
        }

        if (currentState == PlayerState.BUFFERING || currentState == PlayerState.INITIALIZING) {
            mProgressDialog.show();
        } else {
            mProgressDialog.dismiss();
        }
        if (currentState == PlayerState.STOPPED) {
            onCurrentPositionUpdated(0);
        }
    }

    @Override
    public void onRemoteDisconnected() {
        updateStatusMessage(R.string.allshare_disconnected);
        mState.mDeviceId = null;
        mDevicePicker.setActive(false);
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (mPlayerController.isRemotePlayer() && (keyCode == KeyEvent.KEYCODE_VOLUME_DOWN || keyCode == KeyEvent.KEYCODE_VOLUME_UP)) {
            changeVolume(keyCode == KeyEvent.KEYCODE_VOLUME_UP ? 1 : -1);
            return true;
        }
        return super.onKeyDown(keyCode, event);
    }

    // ////////////////////////////////////////////////////////////////////////
    // This method handles the click events
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.playPause:
                if (mState.mPlayback == PlayerState.PLAYING) {
                    mPlayerController.pause();
                    break;
                }
                // player is stopped or paused, so start playing content
                mPlayerController.play();
                break;
            case R.id.stop:
                mPlayerController.stop();
                break;
            case R.id.mute:
                setMute(!mPlayerController.getMute());
                break;
        }
    }

    private void setMute(boolean mute) {
        mPlayerController.setMute(mute);
        mMuteButton.setImageResource(mute ? R.drawable.volume_muted : R.drawable.volume_on);
        mState.mMute = mute;
    }

    private void changeVolume(int diff) {
        if (mPlayerController.getMute())
            setMute(false);
        mPlayerController.setVolume(mPlayerController.getVolume() + diff);
    }

    /**
     * A class responsible for updating the current position.
     * <p/>
     * Send it the START_POLLING empty message to start updating the
     * current position, and STOP_POLLING if you want to hide it
     * (in case of activity end, player hide or simply user moves the seek bar).
     */
    private class PositionUpdater implements Runnable, Handler.Callback {
        static final int START_POLLING = 1;
        static final int STOP_POLLING = 2;

        @Override
        public boolean handleMessage(Message msg) {
            switch (msg.what) {
                case START_POLLING:
                    mUpdateEventHandler.post(this);
                    break;
                case STOP_POLLING:
                    mUpdateEventHandler.removeCallbacks(this);
                    break;
            }
            return false;
        }

        @Override
        public void run() {
            PlayerController player = MusicPlayerActivity.this.mPlayerController;
            if (player != null) {
                player.queryCurrentPositionAsync();
                mUpdateEventHandler.postDelayed(this, 500);
            }
        }

    }

    /**
     * Internal class for storing complete player state.
     * Used for saving state between e.g. rotations.
     */
    private static class MusicPlayerState {
        private static final String MEDIA_URI = "MusicPlayer.mediaUri";
        private static final String COVER_URI = "MusicPlayer.albumArtUri";
        private static final String TITLE = "MusicPlayer.title";
        private static final String ARTIST = "MusicPlayer.artist";
        private static final String MIMETYPE = "MusicPlayer.mimeType";
        private static final String PLAYER_STATE = "MusicPlayer.state";
        private static final String POSITION = "MusicPlayer.position";
        private static final String DURATION = "MusicPlayer.duration";
        private static final String MUTE = "MusicPlayer.mute";
        private static final String DEVICE_ID = "MusicPlayer.deviceId";

        Uri mMediaUri;
        Uri mAlbumArtUri;
        String mTitle;
        String mArtist;
        String mMimeType;
        PlayerState mPlayback = PlayerState.STOPPED;
        int mPosition = 0;
        int mDuration = 0;
        boolean mMute;
        String mDeviceId;

        private MusicPlayerState() {

        }

        /**
         * Constructs {@link MusicPlayerState} from bundle.
         *
         * @param savedState the bundle containing the saved mState
         */
        private MusicPlayerState(Bundle savedState) {

            mMediaUri = Uri.parse(savedState.getString(MEDIA_URI));
            mAlbumArtUri = Uri.parse(savedState.getString(COVER_URI));
            mTitle = savedState.getString(TITLE);
            mArtist = savedState.getString(ARTIST);
            mMimeType = savedState.getString(MIMETYPE);
            mPlayback = PlayerState.valueOf(savedState.getString(PLAYER_STATE));
            mPosition = savedState.getInt(POSITION);
            mDuration = savedState.getInt(DURATION);
            mMute = savedState.getInt(MUTE)==1;
            mDeviceId = savedState.getString(DEVICE_ID);
        }

        /**
         * Saves the current state to bundle
         *
         * @param savedState the bundle we should save to
         */
        void writeToBundle(Bundle savedState) {
            savedState.putString(MEDIA_URI, mMediaUri != null ? mMediaUri.toString() : null);
            savedState.putString(COVER_URI, mAlbumArtUri != null ? mAlbumArtUri.toString() : null);
            savedState.putString(TITLE, mTitle);
            savedState.putString(ARTIST, mArtist);
            savedState.putString(MIMETYPE, mMimeType);
            savedState.putString(PLAYER_STATE, mPlayback != null ? mPlayback.toString() : null);
            savedState.putInt(POSITION, mPosition);
            savedState.putInt(DURATION, mDuration);
            savedState.putInt(MUTE, mMute ? 1 : 0);
            savedState.putString(DEVICE_ID, mDeviceId);
        }
    }

    @Override
    public void onScanCompleted(String arg0, Uri uri) {
        Cursor cursor = getApplicationContext().getContentResolver().query(uri, null, null, null, null);
        if (cursor == null) {
            return;
        }
        if (cursor.moveToFirst()) {

            int albumColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Audio.Media.ALBUM_ID);
            int titleColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Audio.Media.TITLE);
            int artistColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Audio.Media.ARTIST);
            int durationColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Audio.Media.DURATION);

            mState.mTitle = cursor.getString(titleColumn);
            mState.mArtist = cursor.getString(artistColumn);
            Long albumId = cursor.getLong(albumColumn);

            Uri albumArtStore = Uri.parse("content://media/external/audio/albumart");
            Uri albumArt = ContentUris.withAppendedId(albumArtStore, albumId);
            mState.mAlbumArtUri = albumArt;
            //Divided by 1000 to store duration in seconds.
            mState.mDuration = cursor.getInt(durationColumn) / 1000;

            updateInformationUi();
        }
        cursor.close();
    }

    /**
     * Extract the media used by this sample to SD card or equivalent.
     */
    private void extractAssets() {
        AssetManager assetManager = getAssets();
        File destDir = new File(Environment.getExternalStorageDirectory(), ASSETS_SUBDIR);
        if (!destDir.exists()) {
            if(!destDir.mkdirs()){
                Toast.makeText(this, R.string.unable_to_load_asset, Toast.LENGTH_LONG).show();
                return;
            }
        }

        for (String filename : ALL_ASSETS) {

            InputStream in = null;
            OutputStream out = null;
            try {
                File destination = new File(destDir, filename);

                if (!destination.exists()) {

                    in = assetManager.open(filename);
                    out = new FileOutputStream(destination);

                    byte[] buffer = new byte[16 * 1024];
                    int read;

                    while ((read = in.read(buffer)) != -1) {
                        out.write(buffer, 0, read);
                    }
                }
            } catch (IOException ignored) {
                Toast.makeText(this, R.string.unable_to_load_asset, Toast.LENGTH_LONG).show();
                this.finish();
            } finally {
                if (in != null) {
                    try {
                        in.close();
                    } catch (IOException ignored) {
                        // Error during close, ignoring
                    }
                }
                if (out != null) {
                    try {
                        out.close();
                    } catch (IOException ignored) {
                        // Error during close, ignoring
                    }
                }
            }
        }
    }

    /**
     * Formats the time in seconds as mm:ss or hh:mm:ss string.
     *
     * @param time time to format
     * @return formatted time
     */
    private static String convertTime2String(int time) {
        int hour = time / 3600;
        time %= 3600;
        int min = time / 60;
        int sec = time % 60;

        String result;

        if (hour >= 1) {
            result = String.format("%02d:%02d:%02d", hour, min, sec);
        } else {
            result = String.format("%02d:%02d", min, sec);
        }

        return result;
    }


}
