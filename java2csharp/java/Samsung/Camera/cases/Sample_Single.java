package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.ImageFormat;
import android.graphics.Matrix;
import android.graphics.PixelFormat;
import android.graphics.Point;
import android.graphics.Rect;
import android.graphics.RectF;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.params.BlackLevelPattern;
import android.hardware.camera2.params.Face;
import android.hardware.camera2.params.MeteringRectangle;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.ExifInterface;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.os.Looper;
import android.text.format.DateFormat;
import android.util.Log;
import android.util.Pair;
import android.util.Range;
import android.util.Rational;
import android.util.Size;
import android.util.SparseArray;
import android.util.SparseIntArray;
import android.view.OrientationEventListener;
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
import android.webkit.WebView;
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
import com.samsung.android.sdk.camera.SDngCreator;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.sample.R;
import com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;
import com.samsung.android.sdk.camera.sample.cases.util.FaceRectView;
import com.samsung.android.sdk.camera.sample.cases.util.SettingDialog;
import com.samsung.android.sdk.camera.sample.cases.util.SettingItem;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.Arrays;
import java.util.GregorianCalendar;
import java.util.HashSet;
import java.util.List;
import java.util.Set;
import java.util.TimeZone;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

public class Sample_Single extends Activity implements SettingDialog.OnCameraSettingUpdatedListener {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_Single";

    private SCamera mSCamera;
    private SCameraManager mSCameraManager;
    private SCameraDevice mSCameraDevice;
    private SCameraCaptureSession mSCameraSession;
    private SCameraCharacteristics mCharacteristics;
    private SCaptureRequest.Builder mPreviewBuilder;
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
     * Lens facing. Camera with this facing will be opened
     */
    private int mLensFacing;
    private List<Integer> mLensFacingList;

    /**
     * Image saving format.
     */
    private int mImageFormat;
    private List<Integer> mImageFormatList;

    /**
     * A {@link com.samsung.android.sdk.camera.sample.cases.util.SettingDialog} for camera setting UI.
     */
    private SettingDialog mSettingDialog;

    /**
     * An {@link com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView} for camera preview.
     */
    private AutoFitTextureView mTextureView;

    /**
     * A {@link com.samsung.android.sdk.camera.sample.cases.util.FaceRectView} for face detection UI.
     */
    private FaceRectView mFaceRectView;

    private ImageReader mJpegReader;
    private ImageReader mRawReader;
    private ImageSaver mImageSaver = new ImageSaver();

    /**
     * A camera related listener/callback will be posted in this handler.
     */
    private Handler mBackgroundHandler;
    private HandlerThread mBackgroundHandlerThread;
    /**
     * A image saving worker Runnable will be posted to this handler.
     */
    private Handler mImageSavingHandler;
    private HandlerThread mImageSavingHandlerThread;

    private BlockingQueue<SCaptureResult> mCaptureResultQueue;

    /**
     * An orientation listener for jpeg orientation
     */
    private OrientationEventListener mOrientationListener;
    private int mLastOrientation = 0;

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
    private static final int MAX_PREVIEW_WIDTH = 1920;
    /**
     * Maximum preview height app will use.
     */
    private static final int MAX_PREVIEW_HEIGHT = 1080;

    /**
     * Conversion from device rotation to DNG orientation
     */
    private static final SparseIntArray DNG_ORIENTATION = new SparseIntArray();
    static {
        DNG_ORIENTATION.append(0, ExifInterface.ORIENTATION_NORMAL);
        DNG_ORIENTATION.append(90, ExifInterface.ORIENTATION_ROTATE_90);
        DNG_ORIENTATION.append(180, ExifInterface.ORIENTATION_ROTATE_180);
        DNG_ORIENTATION.append(270, ExifInterface.ORIENTATION_ROTATE_270);
    }

    @Override
    public void onCameraSettingUpdated(int key, int value) {
        switch (key) {
            case SettingItem.SETTING_TYPE_REQUEST_KEY:
                if(getState() == CAMERA_STATE.PREVIEW) startPreview();
                break;
            case SettingItem.SETTING_TYPE_CAMERA_FACING:
                if(mLensFacing != value) {
                    mLensFacing = value;
                    closeCamera();
                    setState(CAMERA_STATE.IDLE);
                    openCamera(mLensFacing);
                }
                break;
            case SettingItem.SETTING_TYPE_IMAGE_FORMAT:
                if(mImageFormat != value) {
                    mImageFormat = value;
                }
                break;
        }
    }

    private enum CAMERA_STATE {
        IDLE, PREVIEW, WAIT_AF, WAIT_AE, TAKE_PICTURE, CLOSING
    }

    private synchronized void setState(CAMERA_STATE state) {
        mState = state;
    }

    private CAMERA_STATE getState() {
        return mState;
    }

    /**
     * A {@link com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback} for {@link com.samsung.android.sdk.camera.SCameraCaptureSession#setRepeatingRequest(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)}
     */
    private SCameraCaptureSession.CaptureCallback mSessionCaptureCallback = new SCameraCaptureSession.CaptureCallback() {

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
            // Remove comment, if you want to check request/result from console log.
            // dumpCaptureResultToLog(result);
            // dumpCaptureRequestToLog(request);

            // Depends on the current state and capture result, app will take next action.
            switch (getState()) {

                case IDLE:
                case TAKE_PICTURE:
                case CLOSING:
                    // do nothing
                    break;
                case PREVIEW:
                    if(result.get(SCaptureResult.STATISTICS_FACES) != null) {
                        processFace(result.get(SCaptureResult.STATISTICS_FACES),
                                result.get(SCaptureResult.SCALER_CROP_REGION));
                    }
                    break;

                // If AF is triggered and AF_STATE indicates AF process is finished, app will trigger AE pre-capture.
                case WAIT_AF: {
                    if(isAFTriggered) {
                        int afState = result.get(SCaptureResult.CONTROL_AF_STATE);
                        // Check if AF is finished.
                        if (SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState ||
                                SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState) {

                            // If AE mode is off or device is legacy device then skip AE pre-capture.
                            if(result.get(SCaptureResult.CONTROL_AE_MODE) != SCaptureResult.CONTROL_AE_MODE_OFF &&
                                    mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY) {
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
            if(mImageFormat == ImageFormat.JPEG) mImageSaver.save(reader.acquireNextImage(), createFileName() + ".jpg");
            else mImageSaver.save(reader.acquireNextImage(), createFileName() + ".dng");
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_single);
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

        mCaptureResultQueue = new LinkedBlockingQueue<SCaptureResult>();

        setOrientationListener(true);
        createUI();
        checkRequiredFeatures();
        openCamera(mLensFacing);
    }

    @Override
    public void onPause() {
        setState(CAMERA_STATE.CLOSING);

        if(mSettingDialog != null) {
            mSettingDialog.dismiss();
            mSettingDialog = null;
        }

        setOrientationListener(false);

        stopBackgroundThread();
        closeCamera();

        mSCamera = null;
        super.onPause();
    }

    /**
     * Prepares an UI, like button, dialog, etc.
     */
    private void createUI() {
        mSettingDialog = new SettingDialog(this);
        mSettingDialog.setOnCaptureRequestUpdatedListener(this);

        findViewById(R.id.picture).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // take picture is only works under preview state.
                if (getState() == CAMERA_STATE.PREVIEW) {

                    // No AF lock is required for AF modes OFF/EDOF.
                    if (mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) != SCaptureRequest.CONTROL_AF_MODE_OFF &&
                            mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) != SCaptureRequest.CONTROL_AF_MODE_EDOF) {
                        lockAF();

                    // No AE pre-capture is required for AE mode OFF or device is LEGACY.
                    } else if (mPreviewBuilder.get(SCaptureRequest.CONTROL_AE_MODE) != SCaptureRequest.CONTROL_AE_MODE_OFF &&
                            mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL) != SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY) {
                        triggerAE();

                    // If AE/AF is skipped, run still capture directly.
                    } else {
                        takePicture();
                    }
                }
            }
        });

        findViewById(R.id.setting).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if(getState() == CAMERA_STATE.PREVIEW)
                    mSettingDialog.show();
            }
        });

        findViewById(R.id.info).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                showInformationDialog();
            }
        });

        mTextureView = (AutoFitTextureView) findViewById(R.id.texture);
        mFaceRectView = (FaceRectView) findViewById(R.id.face);

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
            public void onSurfaceTextureUpdated(SurfaceTexture surface) {
            }
        });
    }

    /**
     * Closes a camera and release resources.
     */
    synchronized private void closeCamera() {
        try {
            mCameraOpenCloseLock.acquire();

            if (mSCameraSession != null) {
                mSCameraSession.close();
                mSCameraSession = null;
            }

            if (mSCameraDevice != null) {
                mSCameraDevice.close();
                mSCameraDevice = null;
            }

            if (mJpegReader != null) {
                mJpegReader.close();
                mJpegReader = null;
            }

            if (mRawReader != null) {
                mRawReader.close();
                mRawReader = null;
            }

            mSCameraManager = null;
        } catch (InterruptedException e) {
            Log.e(TAG, "Interrupted while trying to lock camera closing.", e);
        } finally {
            mCameraOpenCloseLock.release();
        }
    }

    private void checkRequiredFeatures() {
        try {
            // Find available lens facing value for this device
            Set<Integer> lensFacings = new HashSet<Integer>();
            for(String id : mSCamera.getSCameraManager().getCameraIdList()) {
                SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(id);
                lensFacings.add(cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING));
            }
            mLensFacingList = new ArrayList<Integer>(lensFacings);

            mLensFacing = mLensFacingList.get(mLensFacingList.size() - 1);

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
        }
    }

    /**
     * Opens a {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    synchronized private void openCamera(int facing) {
        try {
            if(!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS)) {
                showAlertDialog("Time out waiting to lock camera opening.", true);
            }

            mSCameraManager = mSCamera.getSCameraManager();

            mCameraId = null;

            // Find camera device that facing to given facing parameter.
            for(String id : mSCamera.getSCameraManager().getCameraIdList()) {
                SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(id);
                if(cameraCharacteristics.get(SCameraCharacteristics.LENS_FACING) == facing) {
                    mCameraId = id;
                    break;
                }
            }

            if(mCameraId == null) {
                showAlertDialog("No camera exist with given facing: " + facing, true);
                return;
            }

            // acquires camera characteristics
            mCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(mCameraId);

            StreamConfigurationMap streamConfigurationMap = mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

            // Acquires supported preview size list that supports SurfaceTexture
            mPreviewSize = streamConfigurationMap.getOutputSizes(SurfaceTexture.class)[0];
            for(Size option : streamConfigurationMap.getOutputSizes(SurfaceTexture.class)) {
                // Find maximum preview size that is not larger than MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT
                int areaCurrent = Math.abs( (mPreviewSize.getWidth() * mPreviewSize.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
                int areaNext = Math.abs( (option.getWidth() * option.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));

                if(areaCurrent > areaNext) mPreviewSize = option;
            }

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
            mJpegReader = ImageReader.newInstance(jpegSize.getWidth(), jpegSize.getHeight(), ImageFormat.JPEG, 1);
            mJpegReader.setOnImageAvailableListener(mImageCallback, mImageSavingHandler);

            if(contains(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES), SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_RAW)) {
                Size[] rawSizeList = streamConfigurationMap.getOutputSizes(ImageFormat.RAW_SENSOR);
                if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.M && 0 == rawSizeList.length) {
                    rawSizeList = streamConfigurationMap.getHighResolutionOutputSizes(ImageFormat.RAW_SENSOR);
                }
                Size rawSize = rawSizeList[0];

                mRawReader = ImageReader.newInstance(rawSize.getWidth(), rawSize.getHeight(), ImageFormat.RAW_SENSOR, 1);
                mRawReader.setOnImageAvailableListener(mImageCallback, mImageSavingHandler);

                mImageFormatList = Arrays.asList(ImageFormat.JPEG, ImageFormat.RAW_SENSOR);
            } else {
                if(mRawReader != null) {
                    mRawReader.close();
                    mRawReader = null;
                }
                mImageFormatList = Arrays.asList(ImageFormat.JPEG);
            }
            mImageFormat = ImageFormat.JPEG;

            // Set the aspect ratio to TextureView
            int orientation = getResources().getConfiguration().orientation;
            if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
                mTextureView.setAspectRatio(mPreviewSize.getWidth(), mPreviewSize.getHeight());
                mFaceRectView.setAspectRatio(mPreviewSize.getWidth(), mPreviewSize.getHeight());
            } else {
                mTextureView.setAspectRatio(mPreviewSize.getHeight(), mPreviewSize.getWidth());
                mFaceRectView.setAspectRatio(mPreviewSize.getHeight(), mPreviewSize.getWidth());
            }

            // calculate transform matrix for face rect view
            configureFaceRectTransform();

            // Opening the camera device here
            mSCameraManager.openCamera(mCameraId, new SCameraDevice.StateCallback() {
                public void onOpened(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    if (getState() == CAMERA_STATE.CLOSING)
                        return;
                    mSettingDialog.dismiss();
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
    synchronized private void createPreviewSession() {

        if (null == mSCamera
            || null == mSCameraDevice
            || null == mSCameraManager
            || null == mPreviewSize
            || !mTextureView.isAvailable())
            return;

        try {
            SurfaceTexture texture = mTextureView.getSurfaceTexture();

            // Set default buffer size to camera preview size.
            texture.setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());

            Surface surface = new Surface(texture);

            // Creates SCaptureRequest.Builder for preview with output target.
            mPreviewBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_PREVIEW);
            mPreviewBuilder.addTarget(surface);

            // Creates SCaptureRequest.Builder for still capture with output target.
            mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);

            // Setup SettingDialog. SettingDialog will add setting item depends on camera characteristics.
            // and updates builders as setting value changes.
            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    mSettingDialog.configure(mCharacteristics, mLensFacing, mLensFacingList, mImageFormat, mImageFormatList, mPreviewBuilder, mCaptureBuilder);
                }
            });

            // Creates a SCameraCaptureSession here.
            List<Surface> outputSurface = new ArrayList<Surface>();
            outputSurface.add(surface);
            outputSurface.add(mJpegReader.getSurface());
            if(mRawReader != null) outputSurface.add(mRawReader.getSurface());

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
                    setState(CAMERA_STATE.IDLE);
                }
            }, mBackgroundHandler);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to create camera capture session.", true);
        }
    }

    /**
     * Starts a preview.
     */
    synchronized private void startPreview() {
        if(mSCameraSession == null) return;

        try {
            // Starts displaying the preview.
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            setState(CAMERA_STATE.PREVIEW);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to start preview.", true);
        }
    }

    /**
     * Starts AF process by triggering {@link com.samsung.android.sdk.camera.SCaptureRequest#CONTROL_AF_TRIGGER_START}.
     */
    private void lockAF() {
        try {
            setState(CAMERA_STATE.WAIT_AF);
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
        // If we send TRIGGER_CANCEL. Lens move to its default position. This results in bad user experience.
        if(mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) == SCaptureRequest.CONTROL_AF_MODE_AUTO ||
                mPreviewBuilder.get(SCaptureRequest.CONTROL_AF_MODE) == SCaptureRequest.CONTROL_AF_MODE_MACRO) {
            setState(CAMERA_STATE.PREVIEW);
            return;
        }

        // Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
            mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    setState(CAMERA_STATE.PREVIEW);
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
     * Take picture.
     */
    private void takePicture() {
        if(getState() == CAMERA_STATE.CLOSING)
            return;

        try {
            // Sets orientation
            mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, getJpegOrientation());

            if(mImageFormat == ImageFormat.JPEG) mCaptureBuilder.addTarget(mJpegReader.getSurface());
            else mCaptureBuilder.addTarget(mRawReader.getSurface());

            mSCameraSession.capture(mCaptureBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                @Override
                public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {

                    try {
                        mCaptureResultQueue.put(result);
                    } catch (InterruptedException e) {
                        e.printStackTrace();
                    }

                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    unlockAF();
                }

                @Override
                public void onCaptureFailed(SCameraCaptureSession session, SCaptureRequest request, SCaptureFailure failure) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("JPEG Capture failed.", false);
                    unlockAF();
                }
            }, mBackgroundHandler);

            if(mImageFormat == ImageFormat.JPEG) mCaptureBuilder.removeTarget(mJpegReader.getSurface());
            else mCaptureBuilder.removeTarget(mRawReader.getSurface());

            setState(CAMERA_STATE.TAKE_PICTURE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to start preview.", true);
        }
    }

    /**
     * Process face information to draw face UI
     */
    private void processFace(final Face[] faces, final Rect zoomRect) {
        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                mFaceRectView.setFaceRect(faces, zoomRect);
                mFaceRectView.invalidate();
            }
        });
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
     * Configure required transform for {@link android.hardware.camera2.params.Face} to be displayed correctly in the screen.
     */
    private void configureFaceRectTransform() {
        int orientation = getResources().getConfiguration().orientation;
        int degrees = getWindowManager().getDefaultDisplay().getRotation() * 90;

        int result;
        if(mCharacteristics.get(SCameraCharacteristics.LENS_FACING) == SCameraCharacteristics.LENS_FACING_FRONT) {
            result = (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) + degrees) % 360;
            result = (360 - result) % 360;  // compensate the mirror
        } else {
            result = (mCharacteristics.get(SCameraCharacteristics.SENSOR_ORIENTATION) - degrees + 360) % 360;
        }
        mFaceRectView.setTransform(mPreviewSize,
                mCharacteristics.get(SCameraCharacteristics.LENS_FACING),
                result, orientation);
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

        mImageSavingHandlerThread = new HandlerThread("Saving Thread");
        mImageSavingHandlerThread.start();
        mImageSavingHandler = new Handler(mImageSavingHandlerThread.getLooper());
    }

    /**
     * Stops back ground thread.
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

        if (mImageSavingHandlerThread != null) {
            mImageSavingHandlerThread.quitSafely();
            try {
                mImageSavingHandlerThread.join();
                mImageSavingHandlerThread = null;
                mImageSavingHandler = null;
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

            if(image.getFormat() == ImageFormat.RAW_SENSOR) {
                SCaptureResult result = null;
                try {
                    result = mCaptureResultQueue.take();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }

                try {
                    final SDngCreator dngCreator = new SDngCreator(mCharacteristics, result);
                    dngCreator.setOrientation(DNG_ORIENTATION.get(getJpegOrientation()));

                    new Handler(Looper.myLooper()).post(new Runnable() {
                        @Override
                        public void run() {
                            ByteBuffer buffer = image.getPlanes()[0].getBuffer();
                            byte[] bytes = new byte[buffer.remaining()];
                            buffer.get(bytes);
                            FileOutputStream output = null;
                            try {
                                output = new FileOutputStream(file);
                                dngCreator.writeImage(output, image);
                            } catch (IOException e) {
                                e.printStackTrace();
                            } finally {
                                image.close();
                                dngCreator.close();
                                if (null != output) {
                                    try {
                                        output.close();
                                    } catch (IOException e) {
                                        e.printStackTrace();
                                    }
                                }
                            }

                            MediaScannerConnection.scanFile(Sample_Single.this,
                                    new String[]{file.getAbsolutePath()}, null,
                                    new MediaScannerConnection.OnScanCompletedListener() {
                                        public void onScanCompleted(String path, Uri uri) {
                                            Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                        }
                                    });

                            runOnUiThread(new Runnable() {
                                @Override
                                public void run() {
                                    Toast.makeText(Sample_Single.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
                                }
                            });
                        }
                    });
                } catch (IllegalArgumentException e) {
                    e.printStackTrace();
                    showAlertDialog("Fail to save DNG file.", false);
                    image.close();
                    return;
                }
            } else {
                new Handler(Looper.myLooper()).post(new Runnable() {
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

                        MediaScannerConnection.scanFile(Sample_Single.this,
                                new String[]{file.getAbsolutePath()}, null,
                                new MediaScannerConnection.OnScanCompletedListener() {
                                    public void onScanCompleted(String path, Uri uri) {
                                        Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                    }
                                });

                        runOnUiThread(new Runnable() {
                            @Override
                            public void run() {
                                Toast.makeText(Sample_Single.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
                            }
                        });
                    }
                });
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

    /**
     * Shows the camera device information into dialog.
     */
    private void showInformationDialog() {

        StringBuilder builder = new StringBuilder();

        builder.append("<html><body><pre>");
        if(mCharacteristics != null) {
            builder.append(String.format("Camera Id: %s\n", mCameraId));

            // Check supported hardware level.
            SparseArray<String> level = new SparseArray<String>();
            level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_FULL, "Full");
            level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LIMITED, "Limited");
            level.put(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL_LEGACY, "Legacy");

            builder.append(String.format("Supported H/W Level: %s\n",
                    level.get(mCharacteristics.get(SCameraCharacteristics.INFO_SUPPORTED_HARDWARE_LEVEL))
            ));

            // Available characteristics tag.
            builder.append("\nCharacteristics [\n");
            for(SCameraCharacteristics.Key<?> key :  mCharacteristics.getKeys()) {
                if(mCharacteristics.get(key) instanceof int[]) {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.toString((int[])mCharacteristics.get(key))));
                } else if(mCharacteristics.get(key) instanceof Range[]) {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.deepToString((Range[])mCharacteristics.get(key))));
                } else if(mCharacteristics.get(key) instanceof Size[] ) {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.deepToString((Size[]) mCharacteristics.get(key))));
                } else if(mCharacteristics.get(key) instanceof float[] ) {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.toString((float[])mCharacteristics.get(key))));
                } else if(mCharacteristics.get(key) instanceof StreamConfigurationMap) {
                    builder.append(String.format("\t%s --> [\n", key.getName()));
                    {
                        StreamConfigurationMap streamConfigurationMap = mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);
                        SparseArray<String> formatMap = new SparseArray<String>();
                        formatMap.put(ImageFormat.JPEG, "JPEG");
                        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                            formatMap.put(ImageFormat.PRIVATE, "PRIVATE");
                            formatMap.put(ImageFormat.DEPTH16, "DEPTH16");
                            formatMap.put(ImageFormat.DEPTH_POINT_CLOUD, "DEPTH_POINT_CLOUD");
                        }
                        formatMap.put(ImageFormat.NV16, "NV16");
                        formatMap.put(ImageFormat.NV21, "NV21");
                        formatMap.put(ImageFormat.RAW10, "RAW10");
                        formatMap.put(ImageFormat.RAW_SENSOR, "RAW_SENSOR");
                        formatMap.put(ImageFormat.RGB_565, "RGB_565");
                        formatMap.put(ImageFormat.UNKNOWN, "UNKNOWN");
                        formatMap.put(ImageFormat.YUV_420_888, "420_888");
                        formatMap.put(ImageFormat.YUY2, "YUY2");
                        formatMap.put(ImageFormat.YV12, "YV12");
                        formatMap.put(PixelFormat.RGBA_8888, "RGBA_8888");

                        for (int format : streamConfigurationMap.getOutputFormats()) {
                            builder.append(String.format("\t\t%s(0x%x) --> %s\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getOutputSizes(format))));
                            if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                                builder.append(String.format("\t\tHigh Resolution %s(0x%x) --> %s\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getHighResolutionOutputSizes(format))));
                            }
                        }

                        builder.append(String.format("\n\t\tHigh speed video fps --> %s\n", Arrays.deepToString(streamConfigurationMap.getHighSpeedVideoFpsRanges())));
                        builder.append(String.format("\t\tHigh speed video size --> %s\n", Arrays.deepToString(streamConfigurationMap.getHighSpeedVideoSizes())));

                        if(Build.VERSION.SDK_INT >= Build.VERSION_CODES.M) {
                            builder.append(String.format("\n\t\tInput formats [\n"));
                            for (int format : streamConfigurationMap.getInputFormats()) {
                                builder.append(String.format("\t\t\t%s(0x%x) --> %s\n", formatMap.get(format), format, Arrays.deepToString(streamConfigurationMap.getInputSizes(format))));
                            }
                        }

                        builder.append(String.format("\t\t]\n"));

                    }
                    builder.append("\t]\n");
                } else if(mCharacteristics.get(key) instanceof BlackLevelPattern) {
                    BlackLevelPattern pattern = mCharacteristics.get(SCameraCharacteristics.SENSOR_BLACK_LEVEL_PATTERN);
                    int[] patternArray = new int[BlackLevelPattern.COUNT];
                    pattern.copyTo(patternArray, 0);

                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.toString(patternArray)));
                } else if(mCharacteristics.get(key) instanceof boolean[]) {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), Arrays.toString((boolean[])mCharacteristics.get(key))));
                } else {
                    builder.append(String.format("\t%s --> %s\n", key.getName(), mCharacteristics.get(key).toString()));
                }
            }
            builder.append("]\n");

            // Available characteristics tag.
            builder.append("\nAvailable characteristics keys [\n");
            for(SCameraCharacteristics.Key<?> key :  mCharacteristics.getKeys()) {
                builder.append(String.format("\t%s\n", key.getName()));
            }
            builder.append("]\n");

            // Available request tag.
            builder.append("\nAvailable request keys [\n");
            for(SCaptureRequest.Key<?> key :  mCharacteristics.getAvailableCaptureRequestKeys()) {
                builder.append(String.format("\t%s\n", key.getName()));
            }
            builder.append("]\n");

            // Available result tag.
            builder.append("\nAvailable result keys [\n");
            for(SCaptureResult.Key<?> key :  mCharacteristics.getAvailableCaptureResultKeys()) {
                builder.append(String.format("\t%s\n", key.getName()));
            }
            builder.append("]\n");

            // Available capability.
            builder.append("\nAvailable capabilities [\n");
            SparseArray<String> capabilityName = new SparseArray<String>();
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BACKWARD_COMPATIBLE, "BACKWARD_COMPATIBLE");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_MANUAL_SENSOR, "MANUAL_SENSOR");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_MANUAL_POST_PROCESSING, "MANUAL_POST_PROCESSING");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_RAW, "RAW");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_READ_SENSOR_SETTINGS, "READ_SENSOR_SETTINGS");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_BURST_CAPTURE, "BURST_CAPTURE");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_DEPTH_OUTPUT, "DEPTH_OUTPUT");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_PRIVATE_REPROCESSING, "PRIVATE_REPROCESSING");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_YUV_REPROCESSING, "YUV_REPROCESSING");
            capabilityName.put(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_CONSTRAINED_HIGH_SPEED_VIDEO, "HIGH_SPEED_VIDEO");

            for(int value :  mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES)) {
                builder.append(String.format("\t%s\n", capabilityName.get(value)));
            }
            builder.append("]\n");

            {
                builder.append("\nSamsung extend tags\n");

                // RT-HDR.
                if(mCharacteristics.getKeys().contains(SCameraCharacteristics.LIVE_HDR_INFO_LEVEL_RANGE)) {
                    builder.append(String.format("\tRT-HDR: %s\n", mCharacteristics.get(SCameraCharacteristics.LIVE_HDR_INFO_LEVEL_RANGE).toString()));
                } else {
                    builder.append("\tRT-HDR: not available\n");
                }

                // Metering mode.
                builder.append("\tAvailable Metering mode: [\n");
                if(mCharacteristics.getKeys().contains(SCameraCharacteristics.METERING_AVAILABLE_MODES)) {
                    SparseArray<String> stringMap = new SparseArray<String>();
                    stringMap.put(SCameraCharacteristics.METERING_MODE_CENTER, "Center");
                    stringMap.put(SCameraCharacteristics.METERING_MODE_MATRIX, "Matrix");
                    stringMap.put(SCameraCharacteristics.METERING_MODE_SPOT, "Spot");
                    stringMap.put(SCameraCharacteristics.METERING_MODE_MANUAL, "Manual");

                    for(int mode : mCharacteristics.get(SCameraCharacteristics.METERING_AVAILABLE_MODES))
                    {
                        builder.append(String.format("\t\t%s\n", stringMap.get(mode)));
                    }
                } else {
                    builder.append("\t\tnot available\n");
                }
                builder.append("\t]\n");

                // PAF.
                builder.append("\tPhase AF: ");
                if(mCharacteristics.getKeys().contains(SCameraCharacteristics.PHASE_AF_INFO_AVAILABLE)) {
                    builder.append(mCharacteristics.get(SCameraCharacteristics.PHASE_AF_INFO_AVAILABLE)).append("\n");
                } else {
                    builder.append("not available\n");
                }

                // Stabilization operation mode.
                builder.append("\tStabilization modes: ");
                if(mCharacteristics.getKeys().contains(SCameraCharacteristics.LENS_INFO_AVAILABLE_OPTICAL_STABILIZATION_OPERATION_MODE)) {
                    builder.append(Arrays.toString(mCharacteristics.get(SCameraCharacteristics.LENS_INFO_AVAILABLE_OPTICAL_STABILIZATION_OPERATION_MODE))).append("\n");
                } else {
                    builder.append("not available\n");
                }
            }
        }

        builder.append("</pre></body></html>");

        View dialogView = getLayoutInflater().inflate(R.layout.information_dialog_single, null);
        ((WebView)dialogView.findViewById(R.id.information)).loadDataWithBaseURL(null, builder.toString(), "text/html", "utf-8", null);
        final AlertDialog.Builder dialog = new AlertDialog.Builder(this);
        dialog.setTitle("Information")
                .setView(dialogView)
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        dialog.dismiss();
                    }
                });

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                dialog.show();
            }
        });
    }

    /**
     * Dump {@link com.samsung.android.sdk.camera.SCaptureResult} to console log.
     */
    private void dumpCaptureResultToLog(SCaptureResult result) {

        Log.v(TAG, "Dump of SCaptureResult Frame#" + result.getFrameNumber() + " Seq.#" + result.getSequenceId());
        for(SCaptureResult.Key<?> key : result.getKeys()) {
            if(result.get(key) instanceof int[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((int[]) result.get(key)));
            } else if(result.get(key) instanceof float[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((float[])result.get(key)));
            } else if(result.get(key) instanceof long[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((long[])result.get(key)));
            } else if(result.get(key) instanceof MeteringRectangle[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((MeteringRectangle[]) result.get(key)));
            } else if(result.get(key) instanceof Rational[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Rational[]) result.get(key)));
            } else if(result.get(key) instanceof Face[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Face[]) result.get(key)));
            } else if(result.get(key) instanceof Point[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Point[]) result.get(key)));
            } else if(result.get(key) instanceof Pair) {
                Pair value = (Pair)result.get(key);
                Log.v(TAG, key.getName() + ": (" +  value.first + ", " + value.second + ")");
            } else {
                Log.v(TAG, key.getName() + ": " + result.get(key));
            }
        }
    }

    /**
     * Dump {@link com.samsung.android.sdk.camera.SCaptureRequest} to console log.
     */
    private void dumpCaptureRequestToLog(SCaptureRequest request) {

        Log.v(TAG, "Dump of SCaptureRequest");
        for(SCaptureRequest.Key<?> key : request.getKeys()) {
            if (request.get(key) instanceof int[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((int[]) request.get(key)));
            } else if (request.get(key) instanceof float[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((float[]) request.get(key)));
            } else if (request.get(key) instanceof long[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.toString((long[]) request.get(key)));
            } else if (request.get(key) instanceof MeteringRectangle[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((MeteringRectangle[]) request.get(key)));
            } else if (request.get(key) instanceof Rational[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Rational[]) request.get(key)));
            } else if (request.get(key) instanceof Face[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Face[]) request.get(key)));
            } else if (request.get(key) instanceof Point[]) {
                Log.v(TAG, key.getName() + ": " + Arrays.deepToString((Point[]) request.get(key)));
            } else {
                Log.v(TAG, key.getName() + ": " + request.get(key));
            }
        }
    }
}