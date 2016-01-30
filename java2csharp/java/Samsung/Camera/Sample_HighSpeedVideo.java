package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.Matrix;
import android.graphics.RectF;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.media.MediaRecorder;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.text.format.DateFormat;
import android.util.Log;
import android.util.Range;
import android.util.Size;
import android.view.OrientationEventListener;
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.Spinner;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCaptureSession;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraDevice;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.SCaptureRequest;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.sample.R;
import com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;

import java.io.File;
import java.io.IOException;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.TimeZone;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

/**
 * High Speed Video via Scene Mode
 */
public class Sample_HighSpeedVideo extends Activity implements AdapterView.OnItemSelectedListener {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_HighSpeedVideo";

    private SCamera mSCamera;
    private SCameraManager mSCameraManager;

    /**
     * A reference to the opened {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private SCameraDevice mSCameraDevice;
    private SCameraCaptureSession mSCameraSession;
    private SCameraCharacteristics mCharacteristics;

    /**
     * {@link com.samsung.android.sdk.camera.SCaptureRequest.Builder} for the camera preview and recording
     */
    private SCaptureRequest.Builder mPreviewBuilder;

    /**
     * Current Preview Size.
     */
    private Size mPreviewSize;

    /**
     * ID of the current {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private String mCameraId;

    /**
     * An {@link com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView} for camera preview.
     */
    private AutoFitTextureView mTextureView = null;

    /**
     * A camera related listener/callback will be posted in this handler.
     */
    private HandlerThread mBackgroundHandlerThread;
    private Handler mBackgroundHandler;

    /**
     * An orientation listener for jpeg orientation
     */
    private OrientationEventListener mOrientationListener;
    private int mLastOrientation = 0;

    /**
     * A {@link Semaphore} to prevent the app from exiting before closing the camera.
     */
    private Semaphore mCameraOpenCloseLock = new Semaphore(1);

    /**
     * Current app state.
     */
    private CAMERA_STATE mState = CAMERA_STATE.IDLE;

    /**
     * The {@link MediaRecorder} for recording audio and video.
     */
    private MediaRecorder mMediaRecorder;

    private VideoParameter mVideoParameter;

    /**
     * Button to record video
     */
    private Button mRecordButton;

    private List<VideoParameter> mVideoParameterList = new ArrayList<>();
    private Spinner mVideoSpinner;

    private long mRecordingStartTime;
    private List<SCaptureRequest> mRepeatingList;

    private enum CAMERA_STATE {
        IDLE, PREVIEW, RECORD_VIDEO, CLOSING
    }

    private synchronized void setState(CAMERA_STATE state) {
        mState = state;
    }

    private CAMERA_STATE getState() {
        return mState;
    }

    private static class VideoParameter {
        final Size mVideoSize;
        final Range<Integer> mFpsRange;

        VideoParameter(Size videoSize, Range<Integer> fpsRange) {
            mVideoSize = new Size(videoSize.getWidth(), videoSize.getHeight());
            mFpsRange = new Range<>(fpsRange.getLower(), fpsRange.getUpper());
        }

        @Override
        public String toString() {
            return mVideoSize.toString() + " @ " + mFpsRange.getUpper() + "FPS";
        }

        @Override
        public boolean equals(Object o) {
            return o instanceof VideoParameter &&
                    mVideoSize.equals(((VideoParameter) o).mVideoSize) &&
                    mFpsRange.equals(((VideoParameter) o).mFpsRange);
        }
    }

    /**
     * A {@link com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback} for {@link com.samsung.android.sdk.camera.SCameraCaptureSession#setRepeatingRequest(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)}
     */
    private SCameraCaptureSession.CaptureCallback mSessionCaptureCallback = new SCameraCaptureSession.CaptureCallback() {

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
            // Depends on the current state and capture result, app will take next action.
            switch (getState()) {

                case IDLE:
                case PREVIEW:
                case CLOSING:
                    // do nothing
                    break;
            }
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_highspeedvideo);
    }

    @Override
    public void onResume() {
        super.onResume();
        setState(CAMERA_STATE.IDLE);

        startBackgroundThread();

        // initialize SCamera
        mSCamera = new SCamera();
        try {
            mSCamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }

        setOrientationListener(true);

        if(!checkRequiredFeatures()) return;

        createUI();
        openCamera();
    }

    @Override
    public void onPause() {

        if(getState() == CAMERA_STATE.RECORD_VIDEO) stopRecordVideo(true);

        setState(CAMERA_STATE.CLOSING);

        setOrientationListener(false);

        stopBackgroundThread();

        closeCamera();

        super.onPause();
    }

    @SuppressWarnings("deprecation")
    private boolean checkRequiredFeatures() {
        try {
            mCameraId = null;

            // Find camera device that facing to given facing parameter.
            for(String id : mSCamera.getSCameraManager().getCameraIdList()) {
                SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(id);
                if(cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_BACK) {
                    mCameraId = id;
                    break;
                }
            }

            if(mCameraId == null) {
                showAlertDialog("No back-facing camera exist.", true);
                return false;
            }

            // acquires camera characteristics
            mCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(mCameraId);

            if(!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE)) {
                showAlertDialog("Required AF mode is not supported.", true);
                return false;
            }

            if(!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AVAILABLE_SCENE_MODES), SCameraCharacteristics.CONTROL_SCENE_MODE_HIGH_SPEED_VIDEO) ||
                    mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).getHighSpeedVideoSizes().length == 0 ||
                    mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).getHighSpeedVideoFpsRanges().length == 0)
            {
                showAlertDialog("High speed video recording scene mode is not supported.", true);
                return false;
            }

            mVideoParameterList.clear();

            for(Size videoSize : mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).getHighSpeedVideoSizes()) {
                for(Range<Integer> fpsRange : mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP).getHighSpeedVideoFpsRangesFor(videoSize)) {

                    //we will record constant fps video
                    if(fpsRange.getLower().equals(fpsRange.getUpper())) {
                        mVideoParameterList.add(new VideoParameter(videoSize, fpsRange));
                    }
                }
            }

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
            return false;
        }

        return true;
    }

    /**
     * Prepares an UI, like button, etc.
     */
    private void createUI() {

        ArrayAdapter<VideoParameter> videoParameterArrayAdapter = new ArrayAdapter<>(this, android.R.layout.simple_spinner_item, mVideoParameterList);

        mVideoSpinner = (Spinner) findViewById(R.id.videolist);
        mVideoSpinner.setAdapter(videoParameterArrayAdapter);
        mVideoSpinner.setOnItemSelectedListener(this);

        mVideoParameter = mVideoParameterList.get(0);
        mVideoSpinner.setSelection(0);

        mRecordButton = (Button) findViewById(R.id.record);
        mRecordButton.setEnabled(true);
        mRecordButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (getState() == CAMERA_STATE.PREVIEW) {
                    recordVideo();
                } else if(getState() == CAMERA_STATE.RECORD_VIDEO) {
                    stopRecordVideo(false);
                }
            }
        });

        mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

        // Set SurfaceTextureListener that handle life cycle of TextureView
        mTextureView.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
            @Override
            public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
                // "onSurfaceTextureAvailable" is called, which means that SCameraCaptureSession is not created.
                // We need to configure transform for TextureView and crate SCameraCaptureSession.
                configureTransform(width, height);
                createPreviewSession();
            }

            @Override
            public void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {
                // SurfaceTexture size changed, we need to configure transform for TextureView, again.
                configureTransform(width, height);
            }

            @Override
            public boolean onSurfaceTextureDestroyed(SurfaceTexture surface) {
                return true;
            }

            @Override
            public void onSurfaceTextureUpdated(SurfaceTexture surface) { }
        });
    }

    private void closeCamera() {
        try {
            mCameraOpenCloseLock.acquire();

            stopPreview();

            if (mSCameraSession != null) {
                mSCameraSession.close();
                mSCameraSession = null;
            }

            if (mSCameraDevice != null) {
                mSCameraDevice.close();
                mSCameraDevice = null;
            }

            if (null != mMediaRecorder) {
                mMediaRecorder.release();
                mMediaRecorder = null;
            }

            mSCameraManager = null;
            mSCamera = null;
        } catch (InterruptedException e) {
            Log.e(TAG, "Interrupted while trying to lock camera closing.", e);
        } finally {
            mCameraOpenCloseLock.release();
        }
    }

    /**
     * Opens a {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private void openCamera() {
        try {
            if(!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS)) {
                showAlertDialog("Time out waiting to lock camera opening.", true);
            }

            mSCameraManager = mSCamera.getSCameraManager();
            mMediaRecorder = new MediaRecorder();

            // Opening the camera device here
            mSCameraManager.openCamera(mCameraId, new SCameraDevice.StateCallback() {
                public void onOpened(SCameraDevice sCameraDevice) {
                    Log.v(TAG, "onOpened");
                    mCameraOpenCloseLock.release();
                    if (getState() == CAMERA_STATE.CLOSING)
                        return;
                    mSCameraDevice = sCameraDevice;
                    createPreviewSession();
                }

                @Override
                public void onDisconnected(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    if (getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("Camera disconnected.", true);
                }

                @Override
                public void onError(SCameraDevice sCameraDevice, int i) {
                    mCameraOpenCloseLock.release();
                    if (getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("Error while camera open.", true);
                }
            }, mBackgroundHandler);

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot open the camera.", true);
            Log.e(TAG, "Cannot open the camera.", e);
        } catch (InterruptedException e) {
            throw new RuntimeException("Interrupted while trying to lock camera opening.", e);
        }
    }

    /**
     * Create a {@link com.samsung.android.sdk.camera.SCameraCaptureSession} for preview.
     */
    @SuppressWarnings("deprecation")
    private void createPreviewSession() {

        if (null == mSCamera || null == mSCameraDevice || null == mSCameraManager || !mTextureView.isAvailable())
            return;

        try {
            mPreviewSize = mVideoParameter.mVideoSize;

            // Android Camera HAL3.2
            // Note that for the use case of multiple output streams, application must select one
            // unique size from this metadata to use. Otherwise a request error might occur.
            // The camera device will only support up to 2 output high speed streams
            // (processed non-stalling format defined in android.request.maxNumOutputStreams) in this mode.
            // This control will be effective only if all of below conditions are true:
            //
            // The application created no more than maxNumHighSpeedStreams processed non-stalling format output streams,
            // where maxNumHighSpeedStreams is calculated as min(2, android.request.maxNumOutputStreams[Processed (but not-stalling)]).
            // The stream sizes are selected from the sizes reported by android.control.availableHighSpeedVideoConfigurations.
            // No processed non-stalling or raw streams are configured.

            Log.e(TAG, "Preview size: " + mPreviewSize + " Video size: " + mVideoParameter.mVideoSize);

            // Set the aspect ratio to TextureView
            int orientation = getResources().getConfiguration().orientation;
            if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
                mTextureView.setAspectRatio(
                        mPreviewSize.getWidth(), mPreviewSize.getHeight());
            } else {
                mTextureView.setAspectRatio(
                        mPreviewSize.getHeight(), mPreviewSize.getWidth());
            }

            prepareMediaRecorder();

            SurfaceTexture texture = mTextureView.getSurfaceTexture();

            // Set default buffer size to camera preview size.
            texture.setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());

            Surface previewSurface = new Surface(texture);
            Surface recorderSurface = mMediaRecorder.getSurface();

            // Creates SCaptureRequest.Builder for preview and recording with output target.
            mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_RECORD);

            // {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} supports only 24fps.
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, mVideoParameter.mFpsRange);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_SCENE_MODE, SCaptureRequest.CONTROL_SCENE_MODE_HIGH_SPEED_VIDEO);
            mPreviewBuilder.addTarget(previewSurface);
            mPreviewBuilder.addTarget(recorderSurface);

            // limit preview fps up to 30.
            int requestListSize = mVideoParameter.mFpsRange.getUpper() > 30 ? mVideoParameter.mFpsRange.getUpper() / 30 : 1;

            mRepeatingList = new ArrayList<>();
            mRepeatingList.add(mPreviewBuilder.build());
            mPreviewBuilder.removeTarget(previewSurface);

            for(int i = 0; i < requestListSize - 1; i++) {
                mRepeatingList.add(mPreviewBuilder.build());
            }

            Log.e(TAG, "Request size: " + mRepeatingList.size());

            // Creates a SCameraCaptureSession here.
            List<Surface> outputSurface = Arrays.asList(previewSurface, recorderSurface);
            mSCameraDevice.createCaptureSession(outputSurface, new SCameraCaptureSession.StateCallback() {
                @Override
                public void onConfigured(SCameraCaptureSession sCameraCaptureSession) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    mSCameraSession = sCameraCaptureSession;
                    startPreview();
                }

                @Override
                public void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("Fail to create camera capture session.", true);
                }
            }, mBackgroundHandler);

        } catch (CameraAccessException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    /**
     * Starts a preview.
     */
    private void startPreview() {
        try {
            // Starts displaying the preview.
            mSCameraSession.setRepeatingBurst(mRepeatingList, mSessionCaptureCallback, mBackgroundHandler);
            setState(CAMERA_STATE.PREVIEW);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to start preview.", true);
        }
    }

    /**
     * Stop a preview.
     */
    private void stopPreview() {
        try {
            if (mSCameraSession != null)
                mSCameraSession.stopRepeating();
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to stop preview.", true);
        }
    }

    /**
     * Prepares the media recorder to begin recording.
     */
    private void prepareMediaRecorder() throws IOException {
        mMediaRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
        mMediaRecorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);
        mMediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
        mMediaRecorder.setOutputFile(new File(getExternalFilesDir(null), "temp.mp4").getAbsolutePath());

        int bitrate = 384000;
        if (mVideoParameter.mVideoSize.getWidth() * mVideoParameter.mVideoSize.getHeight() >= 1920 * 1080) {
            bitrate = 14000000;
        } else if (mVideoParameter.mVideoSize.getWidth() * mVideoParameter.mVideoSize.getHeight() >= 1280 * 720) {
            bitrate = 9730000;
        } else if (mVideoParameter.mVideoSize.getWidth() * mVideoParameter.mVideoSize.getHeight() >= 640 * 480) {
            bitrate = 2500000;
        } else if (mVideoParameter.mVideoSize.getWidth() * mVideoParameter.mVideoSize.getHeight() >= 320 * 240) {
            bitrate = 622000;
        }
        mMediaRecorder.setVideoEncodingBitRate(bitrate);

        mMediaRecorder.setVideoFrameRate(mVideoParameter.mFpsRange.getUpper());
        mMediaRecorder.setVideoSize(mVideoParameter.mVideoSize.getWidth(), mVideoParameter.mVideoSize.getHeight());
        mMediaRecorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
        mMediaRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AAC);
        mMediaRecorder.setOrientationHint(getJpegOrientation());
        mMediaRecorder.prepare();
    }

    private synchronized void recordVideo() {
        setState(CAMERA_STATE.RECORD_VIDEO);

        // UI
        mRecordButton.setText(R.string.button_title_stop);
        mVideoSpinner.setEnabled(false);

        // Start recording
        mMediaRecorder.start();

        mRecordingStartTime = System.currentTimeMillis();
    }

    private synchronized void stopRecordVideo(boolean isPausing) {

        // prevents terminated during that the operation to start.
        if (!isPausing && (System.currentTimeMillis() - mRecordingStartTime) < 1000) {
            return;
        }

        // UI
        mRecordButton.setText(R.string.button_title_record);
        mVideoSpinner.setEnabled(true);

        // Stop recording
        mMediaRecorder.stop();
        mMediaRecorder.reset();

        // Save recording file
        File dir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).getAbsolutePath() + "/Camera/");
        if(!dir.exists()) dir.mkdirs();

        final File file = new File(dir, createFileName() + "_hsv.mp4");
        new File(getExternalFilesDir(null), "temp.mp4").renameTo(file);

        MediaScannerConnection.scanFile(Sample_HighSpeedVideo.this,
                new String[]{file.getAbsolutePath()}, null,
                new MediaScannerConnection.OnScanCompletedListener() {
                    public void onScanCompleted(String path, Uri uri) {
                        Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                    }
                });

        Toast.makeText(Sample_HighSpeedVideo.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();

        if (!isPausing) {
            createPreviewSession();
        }
    }

    /**
     * Returns required orientation that the jpeg picture needs to be rotated to be displayed upright.
     */
    private int getJpegOrientation() {
        int degrees = mLastOrientation;

        if(mCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_FRONT) {
            degrees = -degrees;
        }

        return (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) + degrees + 360) % 360;
    }

    /**
     * Enable/Disable an orientation listener.
     */
    private void setOrientationListener(boolean isEnable) {
        if (mOrientationListener == null) {

            mOrientationListener = new OrientationEventListener(this) {
                @Override
                public void onOrientationChanged(int orientation) {
                    if(orientation == ORIENTATION_UNKNOWN) return;
                    mLastOrientation = (orientation + 45) / 90 * 90;
                }
            };
        }

        if(isEnable) {
            mOrientationListener.enable();
        } else {
            mOrientationListener.disable();
        }
    }

    /**
     * Configures requires transform {@link android.graphics.Matrix} to TextureView.
     */
    private void configureTransform(int viewWidth, int viewHeight) {

        if (null == mTextureView || null == mPreviewSize) {
            return;
        }
        int rotation = getWindowManager().getDefaultDisplay().getRotation();
        Matrix matrix = new Matrix();
        RectF viewRect = new RectF(0, 0, viewWidth, viewHeight);
        RectF bufferRect = new RectF(0, 0, mPreviewSize.getHeight(), mPreviewSize.getWidth());
        float centerX = viewRect.centerX();
        float centerY = viewRect.centerY();
        if (Surface.ROTATION_90 == rotation || Surface.ROTATION_270 == rotation) {
            bufferRect.offset(centerX - bufferRect.centerX(), centerY - bufferRect.centerY());
            matrix.setRectToRect(viewRect, bufferRect, Matrix.ScaleToFit.FILL);
            float scale = Math.max(
                    (float) viewHeight / mPreviewSize.getHeight(),
                    (float) viewWidth / mPreviewSize.getWidth());
            matrix.postScale(scale, scale, centerX, centerY);
            matrix.postRotate(90 * (rotation - 2), centerX, centerY);
        } else {
            matrix.postRotate(90 * rotation, centerX, centerY);
        }

        mTextureView.setTransform(matrix);
        mTextureView.getSurfaceTexture().setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());
    }

    /**
     * Starts background thread that callback from camera will posted.
     */
    private void startBackgroundThread() {
        mBackgroundHandlerThread = new HandlerThread("Background Thread");
        mBackgroundHandlerThread.start();
        mBackgroundHandler = new Handler(mBackgroundHandlerThread.getLooper());
    }

    /**
     * Stops background thread.
     */
    private void stopBackgroundThread() {
        if (mBackgroundHandlerThread != null) {
            mBackgroundHandlerThread.quitSafely();
            try {
                mBackgroundHandlerThread.join();
                mBackgroundHandlerThread = null;
                mBackgroundHandler = null;
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
    }

    @Override
    public void onNothingSelected(AdapterView<?> parent) { }

    @Override
    public void onItemSelected(AdapterView<?> adapterView, View view, int position, long id) {
        VideoParameter videoParameter = mVideoParameterList.get(position);

        if( !videoParameter.equals(mVideoParameter) ) {
            mVideoParameter = videoParameter;

            if(getState() == CAMERA_STATE.PREVIEW)
            {
                mMediaRecorder.reset();
                createPreviewSession();
            }
        }
    }

    private boolean contains(final int[] array, final int key) {
        for (final int i : array) {
            if (i == key) {
                return true;
            }
        }
        return false;
    }

    /**
     * Creates file name based on current time.
     */
    private String createFileName() {
        GregorianCalendar calendar = new GregorianCalendar();
        calendar.setTimeZone(TimeZone.getDefault());
        long dateTaken = calendar.getTimeInMillis();

        return DateFormat.format("yyyyMMdd_kkmmss", dateTaken).toString();
    }

    /**
     * Shows alert dialog.
     */
    private void showAlertDialog(String message, final boolean finishActivity) {

        final AlertDialog.Builder dialog = new AlertDialog.Builder(this);
        dialog.setMessage(message)
                .setIcon(android.R.drawable.ic_dialog_alert)
                .setTitle("Alert")
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        dialog.dismiss();
                        if (finishActivity) finish();
                    }
                }).setCancelable(false);

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                dialog.show();
            }
        });
    }
}
