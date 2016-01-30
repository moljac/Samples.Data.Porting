package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.ImageFormat;
import android.graphics.Matrix;
import android.graphics.RectF;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaRecorder;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.text.format.DateFormat;
import android.util.Log;
import android.util.Range;
import android.util.Size;
import android.util.TypedValue;
import android.view.Gravity;
import android.view.OrientationEventListener;
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.LinearLayout;
import android.widget.SeekBar;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCaptureSession;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraDevice;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.SCaptureFailure;
import com.samsung.android.sdk.camera.SCaptureRequest;
import com.samsung.android.sdk.camera.SCaptureResult;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.filter.SCameraFilter;
import com.samsung.android.sdk.camera.filter.SCameraFilterInfo;
import com.samsung.android.sdk.camera.filter.SCameraFilterManager;
import com.samsung.android.sdk.camera.processor.SCameraEffectProcessor;
import com.samsung.android.sdk.camera.processor.SCameraProcessorManager;
import com.samsung.android.sdk.camera.processor.SCameraProcessorParameter;
import com.samsung.android.sdk.camera.sample.R;
import com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.GregorianCalendar;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.TimeZone;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

public class Sample_Effect extends Activity implements AdapterView.OnItemSelectedListener {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_Effect";

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
     * {@link com.samsung.android.sdk.camera.SCaptureRequest.Builder} for the still capture
     */
    private SCaptureRequest.Builder mCaptureBuilder;

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

    private ImageReader mImageReader;
    private ImageSaver mImageSaver = new ImageSaver();

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
     * True if {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER} is triggered.
     */
    private boolean isAFTriggered;
    /**
     * True if {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AE_PRECAPTURE_TRIGGER} is triggered.
     */
    private boolean isAETriggered;

    /**
     * Current app state.
     */
    private CAMERA_STATE mState = CAMERA_STATE.IDLE;

    /**
     * Maximum preview width app will use.
     */
    private static final int MAX_PREVIEW_WIDTH = 1280;
    /**
     * Maximum preview height app will use.
     */
    private static final int MAX_PREVIEW_HEIGHT = 720;

    /**
     * Maximum preview FPS app will use.
     */
    private static final int MAX_PREVIEW_FPS = 24;

    /**
     * {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} for applies the filter effect on image data
     */
    private SCameraEffectProcessor mProcessor;

    /**
     * {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager} for creating and retrieving available filters
     */
    static private SCameraFilterManager mSCameraFilterManager;

    /**
     * The {@link android.media.MediaRecorder} for recording audio and video.
     */
    private MediaRecorder mMediaRecorder;

    /**
     * The {@link android.util.Size} of video recording.
     */
    private Size mVideoSize;

    /**
     * {@link com.samsung.android.sdk.camera.filter.SCameraFilter} for contains information of filter.
     */
    private SCameraFilter mFilter;

    /**
     * list of retrieving available filters.
     */
    private List<SCameraFilterInfo> mFilterInfoList;

    /**
     * list of the information available with the filter of the parameter.
     */
    private Map<String, List<FilterParameterInfo>> mFilterParameterInfoList = new HashMap<String, List<FilterParameterInfo>>();


    /**
     * Button to capture
     */
    private Button mPictureButton;

    /**
     * Button to record video
     */
    private Button mRecordButton;

    private ArrayAdapter<String> mEffectAdapter = null;

    private List<View> mParameterViewList = new ArrayList<View>();

    private long mRecordingStartTime;

    private enum CAMERA_STATE {
        IDLE, PREVIEW, WAIT_AF, WAIT_AE, TAKE_PICTURE, RECORD_VIDEO, CLOSING
    }

    private synchronized void setState(CAMERA_STATE state) {
        mState = state;
    }

    private CAMERA_STATE getState() {
        return mState;
    }

    private static class FilterParameterInfo {
        final String mParameterName;
        final Range<Integer> mParameterRange;

        FilterParameterInfo(String name, Range<Integer> range) {
            mParameterName = name;
            mParameterRange = range;
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
                case TAKE_PICTURE:
                case CLOSING:
                    // do nothing
                    break;

                // If AF is triggered and AF_STATE indicates AF process is finished, app will trigger AE pre-capture.
                case WAIT_AF: {
                    if(isAFTriggered) {
                        int afState = result.get(SCaptureResult.CONTROL_AF_STATE);
                        // Check if AF is finished.
                        if (SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState ||
                                SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState) {

                            // If device is legacy device then skip AE pre-capture.
                            if(mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY) {
                                triggerAE();
                            } else {
                                takePicture();
                            }
                            isAFTriggered = false;
                        }
                    }
                    break;
                }

                // If AE is triggered and AE_STATE indicates AE pre-capture process is finished, app will take a picture.
                case WAIT_AE: {
                    if(isAETriggered) {
                        Integer aeState = result.get(SCaptureResult.CONTROL_AE_STATE);
                        if(null == aeState || // Legacy device might have null AE_STATE. However, this should not be happened as we skip triggerAE() for legacy device
                                SCaptureResult.CONTROL_AE_STATE_CONVERGED == aeState ||
                                SCaptureResult.CONTROL_AE_STATE_FLASH_REQUIRED == aeState ||
                                SCaptureResult.CONTROL_AE_STATE_LOCKED == aeState) {
                            takePicture();
                            isAETriggered = false;
                        }
                    }
                    break;
                }
            }
        }
    };

    /**
     * A {@link android.media.ImageReader.OnImageAvailableListener} for still capture.
     */
    private ImageReader.OnImageAvailableListener mImageCallback = new ImageReader.OnImageAvailableListener() {
        @Override
        public void onImageAvailable(ImageReader reader) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;
            Image image = reader.acquireNextImage();
            // process effect using image data, must be called after {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#startStreamProcessing()}, but before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#stopStreamProcessing()}.
            mProcessor.requestProcess(image);
            // after using the image object, should be called the close().
            image.close();
        }
    };

    private SCameraEffectProcessor.EventCallback mProcessorCallback = new SCameraEffectProcessor.EventCallback() {

        @Override
        public void onProcessCompleted(Image image) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;
            mImageSaver.save(image, createFileName() + "_effect.jpg");
            unlockAF();
        }

        @Override
        public void onError(int i) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;
            showAlertDialog("Fail to create result: " + i, false);
            unlockAF();
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_effect);
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
        createProcessor();
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
        deinitProcessor();

        Spinner spinner = (Spinner) findViewById(R.id.effectlist);
        spinner.setOnItemSelectedListener(null);

        super.onPause();
    }

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

            if(!mSCamera.isFeatureEnabled(SCamera.SCAMERA_FILTER)) {
                showAlertDialog("This device does not support SCamera Filter feature.", true);
                return false;
            }

            if(!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR)) {
                showAlertDialog("This device does not support SCamera Processor feature.", true);
                return false;
            }

            SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();
            if(!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_EFFECT)) {
                showAlertDialog("This device does not support Effect Processor.", true);
                return false;
            }

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
            return false;
        }

        return true;
    }

    private void createProcessor() {
        SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();

        // create an {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor}
        mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_EFFECT);

        // retrieving an {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager}
        if(mSCameraFilterManager == null)
            mSCameraFilterManager = mSCamera.getSCameraFilterManager();

        // retrieving available filters
        mFilterInfoList = mSCameraFilterManager.getAvailableFilters();

        List<String> filterName = new ArrayList<String>();
        for(SCameraFilterInfo filterInfo : mFilterInfoList) filterName.add(filterInfo.getName());

        mEffectAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_spinner_item, filterName);

        // Add filter parameter info, refer {@link com.samsung.android.sdk.camera.filter.SCameraFilter#setParameter(String, Number)}
        mFilterParameterInfoList.put("Beauty", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(0, 4))));
        mFilterParameterInfoList.put("Brightness", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(-100, 100))));
        mFilterParameterInfoList.put("Contrast", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(-100, 100))));
        mFilterParameterInfoList.put("Saturate", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(-100, 100))));
        mFilterParameterInfoList.put("Temperature", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(-100, 100))));
        mFilterParameterInfoList.put("Tint Control", Arrays.asList(new FilterParameterInfo("intensity", new Range<Integer>(-100, 100))));
        mFilterParameterInfoList.put("Highlights and Shadows", Arrays.asList(new FilterParameterInfo("highlight", new Range<Integer>(-100, 100)),
                new FilterParameterInfo("shadow", new Range<Integer>(-100, 100))));
    }


    private void initProcessor() {

        SCameraProcessorParameter parameter = mProcessor.getParameters();

        parameter.set(SCameraEffectProcessor.STILL_INPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraEffectProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraEffectProcessor.STILL_SIZE, new Size(mImageReader.getWidth(), mImageReader.getHeight()));
        parameter.set(SCameraEffectProcessor.STREAM_SIZE, mPreviewSize);

        // changes the settings for this processor. must be called before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#initialize()}.
        mProcessor.setParameters(parameter);
        mProcessor.initialize();
        mProcessor.setEventCallback(mProcessorCallback, mBackgroundHandler);
    }

    private void deinitProcessor() {
        if(mProcessor != null) {
            mProcessor.deinitialize();
            mProcessor.close();
            mProcessor = null;
        }
    }

    /**
     * Prepares an UI, like button, etc.
     */
    private void createUI() {

        Spinner spinner = (Spinner) findViewById(R.id.effectlist);
        spinner.setAdapter(mEffectAdapter);
        spinner.setOnItemSelectedListener(this);

        mPictureButton = (Button) findViewById(R.id.picture);
        mPictureButton.setEnabled(true);
        mPictureButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // take picture is only works under preview state.
                if (getState() == CAMERA_STATE.PREVIEW) {
                    lockAF();
                }
            }
        });

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

            if (mImageReader != null) {
                mImageReader.close();
                mImageReader = null;
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

            SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);
            StreamConfigurationMap streamConfigurationMap = characteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

            // Acquires supported preview size list that supports YUV420_888.
            // Input Surface from EffectProcessor will have a format of ImageFormat.YUV420_888
            mPreviewSize = streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)[0];
            for(Size option : streamConfigurationMap.getOutputSizes(ImageFormat.YUV_420_888)) {
                // preview size must be supported by effect processor
                if(!contains(mProcessor.getParameters().get(SCameraEffectProcessor.STREAM_SIZE_LIST), option)) continue;

                // Find maximum preview size that is not larger than MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT and closest to MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT.
                if(option.getWidth() > MAX_PREVIEW_WIDTH || option.getHeight() > MAX_PREVIEW_HEIGHT) continue;

                int areaCurrent = Math.abs( (mPreviewSize.getWidth() * mPreviewSize.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
                int areaNext = Math.abs( (option.getWidth() * option.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));

                if(areaCurrent > areaNext) mPreviewSize = option;
            }

            mVideoSize = streamConfigurationMap.getOutputSizes(MediaRecorder.class)[streamConfigurationMap.getOutputSizes(MediaRecorder.class).length - 1];
            for(Size option : streamConfigurationMap.getOutputSizes(MediaRecorder.class)) {
                if(option.getWidth() == option.getHeight() * mPreviewSize.getWidth() / mPreviewSize.getHeight() && option.getWidth() <= mPreviewSize.getWidth()) {
                    mVideoSize = option;
                    break;
                }
            }

            mMediaRecorder = new MediaRecorder();

            // Acquires supported size for JPEG format
            Size[] jpegSizeList = null;
            jpegSizeList = streamConfigurationMap.getOutputSizes(ImageFormat.JPEG);
            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && 0 == jpegSizeList.length) {
                // If device has 'SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BURST_CAPTURE' getOutputSizes can return zero size list
                // for a format value in getOutputFormats.
                jpegSizeList = streamConfigurationMap.getHighResolutionOutputSizes(ImageFormat.JPEG);
            }
            Size jpegSize = jpegSizeList[0];

            // Configures an ImageReader
            mImageReader = ImageReader.newInstance(jpegSize.getWidth(), jpegSize.getHeight(), ImageFormat.JPEG, 1);
            mImageReader.setOnImageAvailableListener(mImageCallback, mBackgroundHandler);

            // Set the aspect ratio to TextureView
            int orientation = getResources().getConfiguration().orientation;
            if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
                mTextureView.setAspectRatio(
                        mPreviewSize.getWidth(), mPreviewSize.getHeight());
            } else {
                mTextureView.setAspectRatio(
                        mPreviewSize.getHeight(), mPreviewSize.getWidth());
            }

            initProcessor();
            prepareMediaRecorder();

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
        } catch (IOException e) {
            throw new RuntimeException("Fail to prepare media recorder", e);
        }
    }

    /**
     * Create a {@link com.samsung.android.sdk.camera.SCameraCaptureSession} for preview.
     */
    private void createPreviewSession() {
        if (null == mSCamera || null == mSCameraDevice || null == mSCameraManager || null == mPreviewSize || !mTextureView.isAvailable())
            return;

        try {
            SurfaceTexture texture = mTextureView.getSurfaceTexture();

            // Set default buffer size to camera preview size.
            texture.setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());

            Surface surface = new Surface(texture);

            // set a surface of UI preview, must be called before {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor#initialize()}.
            mProcessor.setOutputSurface(surface);

            // retrieving a surface of camera preview, this must be set to preview request.
            Surface cameraSurface = mProcessor.getInputSurface();

            // Creates SCaptureRequest.Builder for preview and recording with output target.
            mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_RECORD);

            // {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} supports only 24fps.
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_TARGET_FPS_RANGE, Range.create(MAX_PREVIEW_FPS, MAX_PREVIEW_FPS));
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mPreviewBuilder.addTarget(cameraSurface);

            // Creates SCaptureRequest.Builder for still capture with output target.
            mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
            mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mCaptureBuilder.addTarget(mImageReader.getSurface());

            // Creates a SCameraCaptureSession here.
            List<Surface> outputSurface = Arrays.asList(cameraSurface, mImageReader.getSurface());
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
        }
    }

    /**
     * Starts a preview.
     */
    private void startPreview() {
        try {
            // Starts displaying the preview.
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            setState(CAMERA_STATE.PREVIEW);
            // must be called after setRepeatingRequest(include surface of camera preview).
            mProcessor.startStreamProcessing();
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

            if (mProcessor != null && getState() == CAMERA_STATE.PREVIEW)
                mProcessor.stopStreamProcessing();
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to stop preview.", true);
        }
    }

    /**
     * Starts AF process by triggering {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER_START}.
     */
    private void lockAF() {
        try {
            setState(CAMERA_STATE.WAIT_AF);
            mRecordButton.setEnabled(false);
            isAFTriggered = false;

            // Set AF trigger to SCaptureRequest.Builder
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_START);

            // App should send AF triggered request for only a single capture.
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
            // Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
            mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    setState(CAMERA_STATE.PREVIEW);
                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            mRecordButton.setEnabled(true);
                        }
                    });
                }
            }, mBackgroundHandler);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to cancel AF", false);
        }
    }

    /**
     * Starts AE pre-capture
     */
    private void triggerAE() {
        try {
            setState(CAMERA_STATE.WAIT_AE);
            isAETriggered = false;

            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER, SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER_START);

            // App should send AE triggered request for only a single capture.
            mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                    isAETriggered = true;
                }
            }, mBackgroundHandler);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER, SCaptureRequest.CONTROL_AE_PRECAPTURE_TRIGGER_IDLE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to trigger AE", true);
        }
    }

    /**
     * Prepares the mediarecorder to begin recording.
     */
    private void prepareMediaRecorder() throws IOException {
        mMediaRecorder.setAudioSource(MediaRecorder.AudioSource.MIC);
        mMediaRecorder.setVideoSource(MediaRecorder.VideoSource.SURFACE);
        mMediaRecorder.setOutputFormat(MediaRecorder.OutputFormat.MPEG_4);
        mMediaRecorder.setOutputFile(new File(getExternalFilesDir(null), "temp.mp4").getAbsolutePath());

        int bitrate = 384000;
        if (mVideoSize.getWidth() * mVideoSize.getHeight() >= 1920 * 1080) {
            bitrate = 14000000;
        } else if (mVideoSize.getWidth() * mVideoSize.getHeight() >= 1280 * 720) {
            bitrate = 9730000;
        } else if (mVideoSize.getWidth() * mVideoSize.getHeight() >= 640 * 480) {
            bitrate = 2500000;
        } else if (mVideoSize.getWidth() * mVideoSize.getHeight() >= 320 * 240) {
            bitrate = 622000;
        }
        mMediaRecorder.setVideoEncodingBitRate(bitrate);

        mMediaRecorder.setVideoFrameRate(MAX_PREVIEW_FPS);
        mMediaRecorder.setVideoSize(mVideoSize.getWidth(), mVideoSize.getHeight());
        mMediaRecorder.setVideoEncoder(MediaRecorder.VideoEncoder.H264);
        mMediaRecorder.setAudioEncoder(MediaRecorder.AudioEncoder.AAC);
        mMediaRecorder.setOrientationHint(getJpegOrientation());
        mMediaRecorder.prepare();
    }

    private synchronized void recordVideo() {
        setState(CAMERA_STATE.RECORD_VIDEO);

        // UI
        mRecordButton.setText(R.string.button_title_stop);
        mPictureButton.setEnabled(false);

        // set the surface for recording, {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} performs an operation for recording.
        mProcessor.setRecordingSurface(mMediaRecorder.getSurface());
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
        mPictureButton.setEnabled(true);

        // {@link com.samsung.android.sdk.camera.processor.SCameraEffectProcessor} stop an operation for recording.
        mProcessor.setRecordingSurface(null);
        // Stop recording
        mMediaRecorder.stop();
        mMediaRecorder.reset();

        // Save recording file
        File dir = new File(Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DCIM).getAbsolutePath() + "/Camera/");
        if(!dir.exists()) dir.mkdirs();

        final File file = new File(dir, createFileName() + "_effect.mp4");
        new File(getExternalFilesDir(null), "temp.mp4").renameTo(file);

        MediaScannerConnection.scanFile(Sample_Effect.this,
                new String[]{file.getAbsolutePath()}, null,
                new MediaScannerConnection.OnScanCompletedListener() {
                    public void onScanCompleted(String path, Uri uri) {
                        Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                    }
                });

        Toast.makeText(Sample_Effect.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();

        if (!isPausing) {
            try {
                prepareMediaRecorder();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }

        setState(CAMERA_STATE.PREVIEW);
    }

    /**
     * Take picture.
     */
    private void takePicture() {
        try {
            // Sets orientation
            mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, getJpegOrientation());

            mSCameraSession.capture(mCaptureBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureFailed(SCameraCaptureSession session, SCaptureRequest request, SCaptureFailure failure) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("JPEG Capture failed.", false);
                    unlockAF();
                }
            }, mBackgroundHandler);
            setState(CAMERA_STATE.TAKE_PICTURE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to take picture.", true);
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
    public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {

        SCameraFilterInfo filterInfo = mFilterInfoList.get(position);

        Log.v(TAG, "Filter selected: " + filterInfo.getName());

        // create a SCameraFilter using SCameraFilterManager.
        mFilter = mSCameraFilterManager.createFilter(filterInfo);

        LinearLayout layout = (LinearLayout) findViewById(R.id.parameterbox);

        for(View v : mParameterViewList) {
            layout.removeView(v);
            if(v instanceof SeekBar) {
                ((SeekBar) v).setOnSeekBarChangeListener(null);
            }
        }
        mParameterViewList.clear();

        if (mFilterParameterInfoList.get(filterInfo.getName()) != null) {
            List<FilterParameterInfo> filterParameterInfoList = mFilterParameterInfoList.get(filterInfo.getName());

            for (FilterParameterInfo info : filterParameterInfoList) {
                final String paramName = info.mParameterName;
                final Range<Integer> paramRange = info.mParameterRange;

                TextView parameterLabel = new TextView(this);
                parameterLabel.setText(info.mParameterName);

                LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(
                        LinearLayout.LayoutParams.WRAP_CONTENT,
                        LinearLayout.LayoutParams.WRAP_CONTENT
                );
                layoutParams.gravity = Gravity.CENTER;
                parameterLabel.setLayoutParams(layoutParams);

                SeekBar seekBar = new SeekBar(this);
                seekBar.setMax(info.mParameterRange.getUpper() - info.mParameterRange.getLower());
                seekBar.setProgress((info.mParameterRange.getUpper() - info.mParameterRange.getLower()) / 2);

                // set a key and value of filter parameters.
                mFilter.setParameter(info.mParameterName, seekBar.getProgress() + info.mParameterRange.getLower());

                layoutParams = new LinearLayout.LayoutParams(
                        (int) TypedValue.applyDimension(TypedValue.COMPLEX_UNIT_DIP, 60, getResources().getDisplayMetrics()),
                        LinearLayout.LayoutParams.WRAP_CONTENT
                );
                layoutParams.gravity = Gravity.CENTER;
                seekBar.setLayoutParams(layoutParams);

                seekBar.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
                    @Override
                    public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                        Log.e(TAG, String.format("Parameter(%s) --> %d", paramName, progress + paramRange.getLower()));

                        // set a key and value of filter parameters.
                        mFilter.setParameter(paramName, progress + paramRange.getLower());

                        // set a filter parameters.
                        SCameraProcessorParameter param = mProcessor.getParameters();
                        param.set(SCameraEffectProcessor.FILTER_EFFECT, mFilter);
                        mProcessor.setParameters(param);
                    }

                    @Override
                    public void onStartTrackingTouch(SeekBar seekBar) {
                    }

                    @Override
                    public void onStopTrackingTouch(SeekBar seekBar) {
                    }
                });

                layout.addView(parameterLabel);
                layout.addView(seekBar);
                mParameterViewList.add(parameterLabel);
                mParameterViewList.add(seekBar);
            }

        }

        // set a filter parameters.
        SCameraProcessorParameter parameter = mProcessor.getParameters();
        parameter.set(SCameraEffectProcessor.FILTER_EFFECT, mFilter);
        mProcessor.setParameters(parameter);
    }

    @Override
    public void onNothingSelected(AdapterView<?> parent) { }


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
                        // after using the image object, should be called the close().
                        image.close();
                        if (null != output) {
                            try {
                                output.close();
                            } catch (IOException e) {
                                e.printStackTrace();
                            }
                        }
                    }

                    MediaScannerConnection.scanFile(Sample_Effect.this,
                            new String[]{file.getAbsolutePath()}, null,
                            new MediaScannerConnection.OnScanCompletedListener() {
                                public void onScanCompleted(String path, Uri uri) {
                                    Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                }
                            });

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(Sample_Effect.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
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