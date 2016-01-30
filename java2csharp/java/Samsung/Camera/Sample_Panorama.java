package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.ImageFormat;
import android.graphics.Matrix;
import android.graphics.Point;
import android.graphics.RectF;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.Image;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.text.format.DateFormat;
import android.util.Log;
import android.util.Size;
import android.view.Gravity;
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
import android.widget.Button;
import android.widget.FrameLayout;
import android.widget.ImageView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCaptureSession;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraDevice;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.SCaptureRequest;
import com.samsung.android.sdk.camera.SCaptureResult;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor;
import com.samsung.android.sdk.camera.processor.SCameraProcessorManager;
import com.samsung.android.sdk.camera.processor.SCameraProcessorParameter;
import com.samsung.android.sdk.camera.sample.R;
import com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;
import com.samsung.android.sdk.camera.sample.cases.util.RectView;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.Arrays;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.TimeZone;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

public class Sample_Panorama extends Activity {

    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_Panorama";

    private SCamera mSCamera;
    private SCameraManager mSCameraManager;
    private SCameraDevice mSCameraDevice;
    private SCameraCaptureSession mSCameraSession;
    private SCaptureRequest.Builder mPreviewBuilder;
    private SCameraPanoramaProcessor mProcessor;

    /**
     * Current Preview Size.
     */
    private Size mPreviewSize;

    /**
     * ID of the current {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private String mCameraId;

    /**
     * An {@link AutoFitTextureView} for camera preview.
     */
    private AutoFitTextureView mTextureView;

    /**
     * A {@link Button} to start and stop taking picture.
     */
    private Button mPictureButton;

    /**
     * An {@link ImageView} for panorama stitching preview.
     */
    private ImageView mPanoramaPreview;

    /**
     * A {@link com.samsung.android.sdk.camera.sample.cases.util.RectView} for panorama tracing rect.
     */
    private RectView mPanoramaRectView;

    /**
     * Temp data to be used panorama processing in callback.
     */
    private final PanoramaData mPanoramaData = new PanoramaData();

    /**
     * Scale factor for panorama preview.
     */
    private static final float PANORAMA_PREVIEW_SCALE = 0.8f;

    private HandlerThread mBackgroundHandlerThread;
    private Handler mBackgroundHandler;

    private ImageSaver mImageSaver = new ImageSaver();

    private Semaphore mCameraOpenCloseLock = new Semaphore(1);

    /**
     * True if {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER} is triggered.
     */
    private boolean isAFTriggered;

    /**
     * True if {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_LOCK} and {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AWB_LOCK} are requested.
     */
    private boolean isAEAWBLocked;

    /**
     * Current app state.
     */
    private CAMERA_STATE mState = CAMERA_STATE.IDLE;

    /**
     * Maximum preview width app will use.
     */
    private static final int MAX_PREVIEW_WIDTH = 1920;
    /**
     * Maximum preview height app will use.
     */
    private static final int MAX_PREVIEW_HEIGHT = 1080;

    private enum CAMERA_STATE {
        IDLE, PREVIEW, WAIT_AF, WAIT_AEAWB_LOCK, TAKE_PICTURE, PROCESSING
    }

    /**
     * A {@link com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback} for {@link com.samsung.android.sdk.camera.SCameraCaptureSession#setRepeatingRequest(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)}
     */
    private SCameraCaptureSession.CaptureCallback mSessionCaptureCallback = new SCameraCaptureSession.CaptureCallback() {

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {

            // Depends on the current state and capture result, app will take next action.
            switch (mState) {
                case IDLE:
                case PREVIEW:
                case TAKE_PICTURE:
                    // do nothing
                    break;

                // If AF is triggered and AF_STATE indicates AF process is finished, app will lock AE/AWB
                case WAIT_AF: {
                    if(isAFTriggered) {
                        Integer afState = result.get(SCaptureResult.CONTROL_AF_STATE);
                        if(null == afState || // in this way, app will compatible with legacy device
                            SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState ||
                            SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState) {
                            lockAEAWB();
                            isAFTriggered = false;
                        }
                    }
                    break;
                }

                // If AE/AWB is locked and AE_STATE/AWB_STATE indicates lock is completed, app will take a picture.
                case WAIT_AEAWB_LOCK: {
                    if(isAEAWBLocked) {
                        Integer aeState = result.get(SCaptureResult.CONTROL_AE_STATE);
                        Integer awbState = result.get(SCaptureResult.CONTROL_AWB_STATE);
                        if (null == aeState || null == awbState || // in this way, app will compatible with legacy device
                            (SCaptureResult.CONTROL_AE_STATE_LOCKED == aeState &&
                            (SCaptureResult.CONTROL_AWB_STATE_LOCKED == awbState ||
                            SCaptureResult.CONTROL_AWB_STATE_CONVERGED == awbState))) {
                            startTakePicture();
                            isAEAWBLocked = false;
                        }
                    }
                    break;
                }
            }
        }
    };

    /**
     * Temp data to be used in {@link com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor.EventCallback}.
     */
    private static class PanoramaData {
        public int direction;
        public Point baseRectOffset, traceRectOffset;
        public Bitmap livePreview;

        public PanoramaData() {
            clearData();
        }

        public void clearData() {
            direction = 0;
            baseRectOffset = new Point();
            traceRectOffset = new Point();
            livePreview = null;
        }
    }

    /**
     * A {@link com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor.EventCallback} that handles events related to panorama processing.
     */
    private SCameraPanoramaProcessor.EventCallback mProcessorCallback = new SCameraPanoramaProcessor.EventCallback() {

        @Override
        public void onError(int code) {
            StringBuilder builder = new StringBuilder();
            builder.append("Fail to create result: ");

            switch(code) {
                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_DECODING_FAIL: {
                    builder.append("decoding fail");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_ENCODING_FAIL: {
                    builder.append("encoding fail");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PROCESSING_FAIL: {
                    builder.append("processing fail");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_UNKNOWN_ERROR: {
                    builder.append("unknown error");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_MAX_FRAME_COUNT: {
                    builder.append("reached to max frame count");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_NO_DIRECTION: {
                    builder.append("direction was not found");
                    break;
                }

                case SCameraPanoramaProcessor.NATIVE_PROCESSOR_MSG_PANORAMA_ERROR_TRACING: {
                    builder.append("tracing fail");
                    break;
                }
            }

            showAlertDialog(builder.toString(), false);
            endTakePicture();
        }

        @Override
        public void onRectChanged(int x, int y) {
            synchronized(mPanoramaData) {
                mPanoramaData.traceRectOffset.set(x, y);
            }

            // Draw tracing rect.
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    synchronized(mPanoramaData) {
                        if (null == mPanoramaRectView || View.INVISIBLE == mPanoramaRectView.getVisibility())
                            return;

                        // Merge coordinates.
                        Size rectSize = mPanoramaRectView.getRectSize();
                        int offsetX = mPanoramaData.baseRectOffset.x + (int) (rectSize.getWidth() * (mPanoramaData.traceRectOffset.x / 1000.0));
                        int offsetY = mPanoramaData.baseRectOffset.y - (int) (rectSize.getHeight() * (mPanoramaData.traceRectOffset.y / 1000.0));

                        mPanoramaRectView.setRectOffset(offsetX, offsetY);
                        mPanoramaRectView.invalidate();
                    }
                }
            });
        }

        @Override
        public void onDirectionChanged(int direction) {
            synchronized (mPanoramaData) {
                mPanoramaData.direction = direction;
            }
        }

        @Override
        public void onStitchingProgressed(int progress) {
            // Stitching progress.
        }

        @Override
        public void onLivePreviewDataStitched(Bitmap data) {
            synchronized (mPanoramaData) {
                mPanoramaData.livePreview = data;
            }

            // Draw panorama preview.
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    synchronized (mPanoramaData) {
                        if(null == mPanoramaPreview || null == mPanoramaRectView)
                            return;

                        // Scale panorama preview data.
                        int scaledWidth = (int)(mPanoramaData.livePreview.getWidth() * PANORAMA_PREVIEW_SCALE);
                        int scaledHeight = (int)(mPanoramaData.livePreview.getHeight() * PANORAMA_PREVIEW_SCALE);
                        Bitmap scaledLivePreview = Bitmap.createScaledBitmap(mPanoramaData.livePreview, scaledWidth, scaledHeight, true);
                        mPanoramaPreview.setImageBitmap(scaledLivePreview);

                        // Setup tracing rect size.
                        Size rectSize = mPanoramaRectView.getRectSize();
                        if (rectSize.getWidth() < 1 || rectSize.getHeight() < 1) {
                            mPanoramaRectView.setRectSize(scaledLivePreview.getWidth(), scaledLivePreview.getHeight());
                        }

                        // Setup preview and tracing rect gravity.
                        if (mPanoramaData.direction > 0) {
                            if (View.INVISIBLE == mPanoramaPreview.getVisibility()) {
                                FrameLayout.LayoutParams params = (FrameLayout.LayoutParams)mPanoramaPreview.getLayoutParams();

                                switch (mPanoramaData.direction) {
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
                                        params.gravity = Gravity.CENTER_VERTICAL | Gravity.END;
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
                                        params.gravity = Gravity.CENTER_HORIZONTAL | Gravity.BOTTOM;
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
                                        params.gravity = Gravity.CENTER_VERTICAL | Gravity.START;
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
                                        params.gravity = Gravity.CENTER_HORIZONTAL | Gravity.TOP;
                                        break;
                                }

                                mPanoramaPreview.setLayoutParams(params);
                                mPanoramaPreview.setVisibility(View.VISIBLE);
                            }

                            if (View.INVISIBLE == mPanoramaRectView.getVisibility()) {
                                switch (mPanoramaData.direction) {
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
                                        mPanoramaRectView.setRectGravity(RectView.GRAVITY_END | RectView.GRAVITY_CENTER_VERTICAL);
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
                                        mPanoramaRectView.setRectGravity(RectView.GRAVITY_BOTTOM | RectView.GRAVITY_CENTER_HORIZONTAL);
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
                                        mPanoramaRectView.setRectGravity(RectView.GRAVITY_START | RectView.GRAVITY_CENTER_VERTICAL);
                                        break;
                                    case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
                                        mPanoramaRectView.setRectGravity(RectView.GRAVITY_TOP | RectView.GRAVITY_CENTER_HORIZONTAL);
                                        break;
                                }

                                mPanoramaRectView.setVisibility(View.VISIBLE);
                            }

                            // Change base rect coordinates.
                            switch (mPanoramaData.direction) {
                                case SCameraPanoramaProcessor.PANORAMA_DIRECTION_LEFT:
                                    mPanoramaData.baseRectOffset.x = -(scaledLivePreview.getWidth() - rectSize.getWidth());
                                    break;
                                case SCameraPanoramaProcessor.PANORAMA_DIRECTION_UP:
                                    mPanoramaData.baseRectOffset.y = -(scaledLivePreview.getHeight() - rectSize.getHeight());
                                    break;
                                case SCameraPanoramaProcessor.PANORAMA_DIRECTION_RIGHT:
                                    mPanoramaData.baseRectOffset.x = scaledLivePreview.getWidth() - rectSize.getWidth();
                                    break;
                                case SCameraPanoramaProcessor.PANORAMA_DIRECTION_DOWN:
                                    mPanoramaData.baseRectOffset.y = scaledLivePreview.getHeight() - rectSize.getHeight();
                                    break;
                            }
                        }
                    }
                }
            });
        }

        @Override
        public void onMaxFramesCaptured() {
            // Will be stopped.
        }

        @Override
        public void onMovingTooFast() {
            // Move slowly.
        }

        @Override
        public void onProcessCompleted(Image result) {
            mImageSaver.save(result, createFileName() + "_panorama.jpg");
            endTakePicture();
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_panorama);
    }

    @Override
    public void onResume() {
        super.onResume();

        startBackgroundThread();

        //Initialize SCamera.
        mSCamera = new SCamera();
        try {
            mSCamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }

        if(!checkRequiredFeatures()) return;
        createProcessor();
        createUI();
        openCamera();
    }

    @Override
    public void onPause() {

        stopBackgroundThread();

        if(CAMERA_STATE.TAKE_PICTURE == mState)
            endTakePicture();

        deinitProcessor();
        closeCamera();

        super.onPause();
    }

    /**
     * @return true, If device supports required feature.
     */
    private boolean checkRequiredFeatures() {
        try {
            mCameraId = null;
            for(String id : mSCamera.getSCameraManager().getCameraIdList()) {
                SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(id);
                if(cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_BACK) {
                    mCameraId = id;
                    break;
                }
            }

            if(null == mCameraId) {
                showAlertDialog("No back-facing camera exist.", true);
                return false;
            }

            SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(mCameraId);

            if(!contains(cameraCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE)) {
                showAlertDialog("Required AF mode is not supported.", true);
                return false;
            }

            if(!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR)) {
                showAlertDialog("This device does not support SCamera Processor feature.", true);
                return false;
            }

            SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();
            if(!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_PANORAMA)) {
                showAlertDialog("This device does not support Panorama Processor.", true);
                return false;
            }

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
            return false;
        }

        return true;
    }

    /**
     * Create {@link com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor}.
     */
    private void createProcessor() {
        SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();

        mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_PANORAMA);
    }

    /**
     * Initialize {@link com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor}.
     */
    private void initProcessor() {
        SCameraProcessorParameter parameter = mProcessor.getParameters();

        parameter.set(SCameraPanoramaProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraPanoramaProcessor.STREAM_SIZE, mPreviewSize);
        parameter.set(SCameraPanoramaProcessor.CAMERA_ID, Integer.parseInt(mCameraId));

        mProcessor.setParameters(parameter);
        mProcessor.initialize();
        mProcessor.setEventCallback(mProcessorCallback, mBackgroundHandler);
    }

    /**
     * Deinitialize {@link com.samsung.android.sdk.camera.processor.SCameraPanoramaProcessor}.
     */
    private void deinitProcessor() {
        if(null != mProcessor) {
            mProcessor.deinitialize();
            mProcessor.close();
            mProcessor = null;
        }
    }

    /**
     * Prepares an UI, like button, dialog, etc.
     */
    private void createUI() {
        mPictureButton = (Button) findViewById(R.id.picture);
        mPictureButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (CAMERA_STATE.PREVIEW == mState) {
                    lockAF();
                } else if (CAMERA_STATE.TAKE_PICTURE == mState) {
                    stopTakePicture();
                }
            }
        });

        mTextureView = (AutoFitTextureView) findViewById(R.id.texture);
        mTextureView.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
            @Override
            public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
                configureTransform(width, height);
                createPreviewSession();
            }

            @Override
            public void onSurfaceTextureSizeChanged(SurfaceTexture surface, int width, int height) {
                configureTransform(width, height);
            }

            @Override
            public boolean onSurfaceTextureDestroyed(SurfaceTexture surface) {
                return true;
            }

            @Override
            public void onSurfaceTextureUpdated(SurfaceTexture surface) { }
        });

        mPanoramaPreview = (ImageView) findViewById(R.id.panorama_preview);
        mPanoramaRectView = (RectView) findViewById(R.id.panorama_rect);
    }

    /**
     * Closes a camera and release resources.
     */
    private void closeCamera() {
        try {
            mCameraOpenCloseLock.acquire();

            if (null != mSCameraSession) {
                mSCameraSession.close();
                mSCameraSession = null;
            }

            if (null != mSCameraDevice) {
                mSCameraDevice.close();
                mSCameraDevice = null;
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

            SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);
            StreamConfigurationMap streamConfigurationMap = characteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

            // Acquires supported preview size list that supports YUV420_888.
            // Input Surface from panorama processor will have a format of ImageFormat.YUV420_888
            mPreviewSize = streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)[0];

            for(Size option : streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)) {
                // preview size must be supported by panorama processor
                if(!contains(mProcessor.getParameters().get(SCameraPanoramaProcessor.STREAM_SIZE_LIST), option)) continue;

                int areaCurrent = Math.abs( (mPreviewSize.getWidth() * mPreviewSize.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
                int areaNext = Math.abs( (option.getWidth() * option.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));

                if(areaCurrent > areaNext) mPreviewSize = option;
            }

            int orientation = getResources().getConfiguration().orientation;
            if (Configuration.ORIENTATION_LANDSCAPE == orientation) {
                mTextureView.setAspectRatio(
                        mPreviewSize.getWidth(), mPreviewSize.getHeight());
            } else {
                mTextureView.setAspectRatio(
                        mPreviewSize.getHeight(), mPreviewSize.getWidth());
            }

            initProcessor();

            mSCameraManager.openCamera(mCameraId, new SCameraDevice.StateCallback() {
                public void onOpened(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    mSCameraDevice = sCameraDevice;
                    createPreviewSession();
                }

                @Override
                public void onDisconnected(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    showAlertDialog("Camera disconnected.", true);
                }

                @Override
                public void onError(SCameraDevice sCameraDevice, int i) {
                    mCameraOpenCloseLock.release();
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
    private void createPreviewSession() {
        if (null == mSCamera
            || null == mSCameraDevice
            || null == mSCameraManager
            || null == mPreviewSize
            || !mTextureView.isAvailable())
            return;

        try {
            SurfaceTexture texture = mTextureView.getSurfaceTexture();
            texture.setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());

            Surface surface = new Surface(texture);

            mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mPreviewBuilder.addTarget(surface);
            mPreviewBuilder.addTarget(mProcessor.getInputSurface());

            //HAL Workaround
            mPreviewBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);

            List<Surface> outputSurface = Arrays.asList(surface, mProcessor.getInputSurface());
            mSCameraDevice.createCaptureSession(outputSurface, new SCameraCaptureSession.StateCallback() {
                @Override
                public void onConfigured(SCameraCaptureSession sCameraCaptureSession) {
                    mSCameraSession = sCameraCaptureSession;
                    startPreview();
                }

                @Override
                public void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession) {
                    showAlertDialog("Fail to create camera session.", true);
                }
            }, mBackgroundHandler);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to session. " + e.getMessage(), true);
        }
    }

    /**
     * Starts a preview.
     */
    private void startPreview() {
        try {
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            mState = CAMERA_STATE.PREVIEW;
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to start preview.", true);
        }
    }

    /**
     * Starts AF process by triggering {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER_START}.
     */
    private void lockAF() {
        try {
            mState = CAMERA_STATE.WAIT_AF;
            isAFTriggered = false;

            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_START);
            mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                    isAFTriggered = true;
                }
            }, mBackgroundHandler);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to trigger AF", true);
        }
    }

    /**
     * Unlock AF.
     */
    private void unlockAF() {
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
            mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                    mState = CAMERA_STATE.PREVIEW;
                }
            }, mBackgroundHandler);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to cancel AF", false);
        }
    }

    /**
     * Starts AE/AWB lock by request {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_LOCK} and {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AWB_LOCK}.
     */
    private void lockAEAWB() {
        try {
            mState = CAMERA_STATE.WAIT_AEAWB_LOCK;

            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AWB_LOCK, true);
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);

            isAEAWBLocked = true;
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to lock AE/AWB", true);
        }
    }

    /**
     * Unlock AE/AWB.
     */
    private void unlockAEAWB() {
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, false);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AWB_LOCK, false);
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to unlock AE/AWB", true);
        }
    }

    /**
     * Starts taking picture.
     */
    private void startTakePicture() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if(null != mPictureButton)
                    mPictureButton.setText(R.string.button_title_stop);
                initPanoramaUI();
            }
        });

        mPanoramaData.clearData();
        mProcessor.start();

        mState = CAMERA_STATE.TAKE_PICTURE;
    }

    /**
     * Stops taking picture.
     */
    private void stopTakePicture() {
        mState = CAMERA_STATE.PROCESSING;
        mProcessor.stop();
    }

    /**
     * End taking picture.
     */
    private void endTakePicture() {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                if(null != mPictureButton)
                    mPictureButton.setText(R.string.button_title_picture);
                hidePanoramaUI();
            }
        });

        unlockAEAWB();
        unlockAF();
    }

    /**
     * Initialize panorama ui.
     */
    private void initPanoramaUI() {
        if(null != mPanoramaPreview)
            mPanoramaPreview.setImageBitmap(null);
        if(null != mPanoramaRectView)
            mPanoramaRectView.clearRect();
    }

    /**
     * Hide panorama ui.
     */
    private void hidePanoramaUI() {
        if(null != mPanoramaPreview)
            mPanoramaPreview.setVisibility(View.INVISIBLE);
        if(null != mPanoramaRectView)
            mPanoramaRectView.setVisibility(View.INVISIBLE);
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
     * Starts back ground thread that callback from camera will posted.
     */
    private void startBackgroundThread() {
        mBackgroundHandlerThread = new HandlerThread("Background Thread");
        mBackgroundHandlerThread.start();
        mBackgroundHandler = new Handler(mBackgroundHandlerThread.getLooper());
    }

    /**
     * Stops back ground thread.
     */
    private void stopBackgroundThread() {
        if (null != mBackgroundHandlerThread) {
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

    /**
     * Saves {@link android.media.Image} to file.
     */
    private class ImageSaver {
        void save(final Image image, String filename) {

            File dir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).getAbsolutePath() + "/Camera/");
            if(!dir.exists()) dir.mkdirs();

            final File file = new File(dir, filename);

            mBackgroundHandler.post(new Runnable() {
                @Override
                public void run() {
                    ByteBuffer buffer = image.getPlanes()[0].getBuffer();
                    byte[] bytes = new byte[buffer.remaining()];
                    buffer.get(bytes);
                    FileOutputStream output = null;
                    try {
                        output = new FileOutputStream(file);
                        output.write(bytes);
                    } catch (IOException e) {
                        e.printStackTrace();
                    } finally {
                        image.close();
                        if (null != output) {
                            try {
                                output.close();
                            } catch (IOException e) {
                                e.printStackTrace();
                            }
                        }
                    }

                    MediaScannerConnection.scanFile(Sample_Panorama.this,
                            new String[]{file.getAbsolutePath()}, null,
                            new MediaScannerConnection.OnScanCompletedListener() {
                                public void onScanCompleted(String path, Uri uri) {
                                    Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                }
                            });

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(Sample_Panorama.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
                        }
                    });
                }
            });
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

    private <T> boolean contains(final T[] array, final T key) {
        for (final T i : array) {
            if (i.equals(key)) {
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
