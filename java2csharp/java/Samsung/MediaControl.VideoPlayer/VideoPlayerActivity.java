/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file VideoPlayerActivity.java
 * @date March 12, 2013
 *
 */

package com.samsung.android.sdk.sample.videoplayer;

import android.app.Activity;
import android.app.ProgressDialog;
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
import android.view.SurfaceView;
import android.view.View;
import android.view.View.OnClickListener;
import android.webkit.MimeTypeMap;
import android.widget.ImageButton;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.TextView;
import android.widget.Toast;
import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.mediacontrol.Smc;
import com.samsung.android.sdk.mediacontrol.SmcDevice;
import com.samsung.android.sdk.sample.videoplayer.controller.PlayerController;
import com.samsung.android.sdk.sample.videoplayer.controller.PlayerController.PlayerState;
import com.samsung.android.sdk.sample.videoplayer.devicepicker.DevicePicker;

import java.io.*;

/**
 * Main activity class of sample music player.
 *
 */
public class VideoPlayerActivity extends Activity implements OnSeekBarChangeListener,
        DevicePicker.DevicePickerResult, OnClickListener,
        PlayerController.PlayerControllerEventListener, OnScanCompletedListener {

    private static final String ASSETS_SUBDIR = "AllShareVideoPlayer";
    private static final String VIDEO_FILE = "AllShare_Video.mp4";
    private static final String SUBTITLES_FILE = "AllShare_Subtitles.srt";
    private static final String[] ALL_ASSETS = {VIDEO_FILE, SUBTITLES_FILE};

    // Activity UI related fields.
    private boolean mIsActivityVisible = false;
    private SurfaceView mVideoView;
    private TextView mTitleText;
    private TextView mPositionText;
    private TextView mDurationText;
    private TextView mSubtitlesText;
    private SeekBar mSeekBar;
    private ImageButton mPlayButton;
    private ImageButton mMuteButton;
    private DevicePicker mDevicePicker;
    private ProgressDialog mProgressDialog;

    // Non-UI fields.
    private PlayerController mPlayerController;
    private VideoPlayerState mState;
    private Handler mUpdateEventHandler = new Handler(new PositionUpdater());

    private Smc mSmcLib;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.video_player_layout);
        setupViewElements();

        mSmcLib = new Smc();

        try {
            mSmcLib.initialize(getBaseContext());
        } catch (SsdkUnsupportedException e) {
            e.printStackTrace();  //TODO Handle exceptions.
        }
                       
        Log.d("media control lib version", mSmcLib.getVersionName());
        

        // Prepares assets used by sample.
        extractAssets();

        // Restores saved state, after e.g. rotating.
        if (savedInstanceState != null) {
            mState = new VideoPlayerState(savedInstanceState);
        } else {
            mState = new VideoPlayerState();

            // Initializes state with built-in content.
            File storageDir = new File(Environment.getExternalStorageDirectory(), ASSETS_SUBDIR);
            File video = new File(storageDir, VIDEO_FILE);
            File subtitles = new File(storageDir, SUBTITLES_FILE);

            // Gets media information (title, artist, cover)
            MediaScannerConnection.scanFile(getApplicationContext(),
                                            new String[] {video.getAbsolutePath()},
                                            null, this);

            Uri mediaUri = Uri.fromFile(video);
            Uri subtitlesUri = Uri.fromFile(subtitles);
            mState.mMediaUri = mediaUri;

            mState.mSubtitlesUri = subtitlesUri;

            mState.mTitle = "Sample album";
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

        mIsActivityVisible = true;

        // Sets up the mPlayerController and related elements.
        setupPlayer();
    }

    @Override
    protected void onStop() {
        super.onStop();

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
        mVideoView = (SurfaceView) findViewById(R.id.video);
        mTitleText = (TextView) findViewById(R.id.titleText);
        mPositionText = (TextView) findViewById(R.id.positionText);
        mDurationText = (TextView) findViewById(R.id.durationText);
        mSubtitlesText = (TextView) findViewById(R.id.subtitlesText);
        mSeekBar = (SeekBar) findViewById(R.id.videoPosition);
        mPlayButton = (ImageButton) findViewById(R.id.playPause);
        mMuteButton = (ImageButton) findViewById(R.id.mute);
        mSeekBar.setOnSeekBarChangeListener(this);
        mPlayButton.setOnClickListener(this);
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
        mMuteButton.setOnClickListener(this);

        mDevicePicker = (DevicePicker) getFragmentManager().findFragmentById(R.id.playerPicker);
        mDevicePicker.setDeviceType(SmcDevice.TYPE_AVPLAYER);
        mDevicePicker.setDeviceSelectedListener(this);
    }

    /**
     * Sets up the media player controller and media information in UI.
     */
    private void setupPlayer() {
        // Displays artist, album name and cover art.
        mTitleText.setText(mState.mTitle);

        // Starts PlayerController and setups local or remote mPlayerController.
        mPlayerController = new PlayerController(this, this, mState.mMediaUri, mState.mSubtitlesUri,
                mState.mMimeType, mState.mPlayback, mVideoView);
     
        if (mState.mDeviceId == null || mState.mDeviceId.length() == 0) {
            mPlayerController.setLocalPlayer();
        } else {
            mPlayerController.setRemotePlayer(mState.mDeviceId, SmcDevice.TYPE_AVPLAYER);
            mDevicePicker.setActive(true);
        }


        // Seeks to given mPosition if requested.
        if (mState.mPosition != 0) {
            mPlayerController.seek(mState.mPosition);
        }

        // Starts playback if requested.
        if (mState.mPlayback == PlayerState.PLAYING) {
            mPlayerController.play();
        }
        setMute(mState.mMute);

        // Sets seek bar to media length (in seconds).
        // First update via mUpdateEventHandler will set current mPosition.
        mSeekBar.setMax(mState.mDuration);
        mDurationText.setText(convertTime2String(mState.mDuration));
        mPositionText.setText(convertTime2String(mState.mPosition));
    }

    private void updateStatusMessage(int message) {
        if (mIsActivityVisible) {
            Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
        }
    }

    private void updateInformationUi() {
        runOnUiThread(new Runnable(){
            @Override
            public void run() {
                mTitleText.setText(mState.mTitle);
                mSeekBar.setMax(mState.mDuration);
                mDurationText.setText(convertTime2String(mState.mDuration));
            }
        });
    }

    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the device DevicePicker events.
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
     * User selected to disable AllShare, so disconnect and switch to local player controller.
     */
    @Override
    public void onAllShareDisabled() {
        mState.mDeviceId = null;
        mPlayerController.stop();
        mPlayerController.setLocalPlayer();
    }

    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the seek bar events.
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
        // Do nothing.
    }

    @Override
    public void onStartTrackingTouch(SeekBar seekBar) {
        // Blocks seek bar refreshing to avoid flickering.
        mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
    }

    @Override
    public void onStopTrackingTouch(SeekBar seekBar) {
        // Seeks to requested position.
        int position = seekBar.getProgress();
        mPlayerController.seek(position);
        mState.mPosition = position;
        // Re-enables current position refreshing.
        mUpdateEventHandler.sendEmptyMessage(PositionUpdater.START_POLLING);
    }

    // ////////////////////////////////////////////////////////////////////////
    // These methods handle the PlayerController events.
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onCurrentPositionUpdated(int position) {
        if (mState.mPlayback == PlayerState.STOPPED) {
            // We might have received a position update after stopping.
            // Ignore it in this case and set to zero.
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
            //mAdjustingSeekPosition = false;
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
    public void onSubtitleAppears(String sub) {
        mSubtitlesText.setText(sub);
    }

    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if(mPlayerController.isRemotePlayer() && (keyCode==KeyEvent.KEYCODE_VOLUME_DOWN || keyCode == KeyEvent.KEYCODE_VOLUME_UP)){
            changeVolume(keyCode == KeyEvent.KEYCODE_VOLUME_UP?1:-1);
            return true;
        }
        return super.onKeyDown(keyCode, event);
    }



    // ////////////////////////////////////////////////////////////////////////
    // This method handles the click events.
    // ////////////////////////////////////////////////////////////////////////

    @Override
    public void onClick(View v) {
        switch (v.getId()) {
        case R.id.playPause:
            if (mState.mPlayback == PlayerState.PLAYING) {
                mPlayerController.pause();
                break;
            }
            // Player is stopped or paused, so start playing content.
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

    private void changeVolume(int diff){
        if(mPlayerController.getMute())
            setMute(false);
        mPlayerController.setVolume(mPlayerController.getVolume() + diff);
    }

    /**
     * A class responsible for updating the current position.
     *
     * Send it the START_POLLING empty message to start updating the
     * current mPosition, and STOP_POLLING if you want to hide it
     * (in case of activity end, player controller hide or simply user moves the seek bar).
     *
     */
    private class PositionUpdater implements Runnable, Handler.Callback {
        private static final int START_POLLING = 1;
        private static final int STOP_POLLING = 2;

        @Override
        public boolean handleMessage(Message msg) {
            switch(msg.what) {
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
            PlayerController player = VideoPlayerActivity.this.mPlayerController;
            if (player != null) {
                player.queryCurrentPositionAsync();
                mUpdateEventHandler.postDelayed(this, 500);
            }
        }
    }

    /**
     * Internal class for storing complete mPlayerController state.
     * Used for saving state between e.g. rotations.
     */
    private static class VideoPlayerState {
        private static final String MEDIA_URI = "VideoPlayerState.mMediaUri";
        private static final String SUBTITLES_URI = "VideoPlayerState.mSubtitlesUri";
        private static final String TITLE = "VideoPlayerState.mTitleText";
        private static final String MIME_TYPE = "VideoPlayerState.mMimeType";
        private static final String PLAYER_STATE = "VideoPlayerState.mState";
        private static final String POSITION = "VideoPlayerState.mPosition";
        private static final String DURATION = "VideoPlayerState.mDuration";
        private static final String MUTE = "MusicPlayer.mute";
        private static final String DEVICE_ID = "VideoPlayerState.mDeviceId";

        private Uri mMediaUri;
        private Uri mSubtitlesUri;
        private String mTitle;
        private String mMimeType;
        private PlayerState mPlayback = PlayerState.STOPPED;
        private int mPosition = 0;
        private int mDuration = 0;
        boolean mMute;
        private String mDeviceId;

        private VideoPlayerState() {
        }

        /**
         * Constructs {@link VideoPlayerState} from bundle.
         *
         * @param savedState the bundle containing the saved mState
         */
        private VideoPlayerState(Bundle savedState) {
            mMediaUri = Uri.parse(savedState.getString(MEDIA_URI));
            mSubtitlesUri = Uri.parse(savedState.getString(SUBTITLES_URI));
            mTitle = savedState.getString(TITLE);
            mMimeType = savedState.getString(MIME_TYPE);
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
        private void writeToBundle(Bundle savedState) {
            savedState.putString(MEDIA_URI, mMediaUri.toString());

            if (mSubtitlesUri != null) {
                savedState.putString(SUBTITLES_URI, mSubtitlesUri.toString());
            }

            savedState.putString(TITLE, mTitle);
            savedState.putString(MIME_TYPE, mMimeType);
            savedState.putString(PLAYER_STATE, mPlayback.toString());
            savedState.putInt(POSITION, mPosition);
            savedState.putInt(DURATION, mDuration);
            savedState.putInt(MUTE, mMute?1:0);
            savedState.putString(DEVICE_ID, mDeviceId);
        }
    }

    @Override
    public void onScanCompleted(String arg0, Uri uri) {
        Cursor cursor = getApplicationContext().getContentResolver().query(uri, null, null, null, null);
        if (cursor == null) {
            return;
        }
        if (cursor.moveToFirst()){

            int titleColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Video.Media.TITLE);
            int durationColumn = cursor
                    .getColumnIndex(android.provider.MediaStore.Video.Media.DURATION);

            mState.mTitle = cursor.getString(titleColumn);
            // Divided by 1000 to store duration in seconds.
            mState.mDuration = cursor.getInt(durationColumn) / 1000;

            updateInformationUi();
        }
        cursor.close();
    }

    /**
     * Extracts the media used by this sample to SD card or equivalent.
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

        for (String filename: ALL_ASSETS) {
            InputStream in = null;
            OutputStream out = null;
            try {
                File destination = new File(destDir, filename);

                if (destination.exists() == false) {

                    in = assetManager.open(filename);
                    out = new FileOutputStream(destination);

                    byte[] buffer = new byte[16 * 1024];
                    int read;

                    while ((read = in.read(buffer)) != -1) {
                        out.write(buffer, 0, read);
                    }
                }
            } catch(IOException ignored) {
                Toast.makeText(this, R.string.unable_to_load_asset, Toast.LENGTH_LONG).show();
//                finish();
            } finally {
                if (in != null) {
                    try {
                        in.close();
                    } catch (IOException ignored) {
                        // Error during close, ignoring.
                    }
                }
                if (out != null) {
                    try {
                        out.close();
                    } catch (IOException ignored) {
                        // Error during close, ignoring.
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
