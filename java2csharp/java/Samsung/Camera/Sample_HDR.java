package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.res.Configuration;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.ImageFormat;
import android.graphics.Matrix;
import android.graphics.RectF;
import android.graphics.SurfaceTexture;
import android.hardware.camera2.CameraAccessException;
import android.hardware.camera2.params.StreamConfigurationMap;
import android.media.Image;
import android.media.ImageReader;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Build;
import android.os.Bundle;
import android.os.Environment;
import android.os.Handler;
import android.os.HandlerThread;
import android.text.format.DateFormat;
import android.util.Log;
import android.util.Size;
import android.view.OrientationEventListener;
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
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
import com.samsung.android.sdk.camera.processor.SCameraHdrProcessor;
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
import java.util.List;
import java.util.TimeZone;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

public class Sample_HDR extends Activity {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_HDR";

    private SCamera mSCamera;
    private SCameraManager mSCameraManager;
    private SCameraDevice mSCameraDevice;
    private SCameraCaptureSession mSCameraSession;
    private SCaptureRequest.Builder mPreviewBuilder;
    private SCaptureRequest.Builder mCaptureBuilder;

    /**
     * ID of the current {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private String mCameraId;
    private SCameraCharacteristics mCharacteristics;

    /**
     * HDR(High Dynamic Range) processor
     */
    private SCameraHdrProcessor mProcessor;

    /**
     * An {@link com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView} for camera preview.
     */
    private AutoFitTextureView mTextureView;
    private ImageReader mImageReader;
    private ImageSaver mImageSaver = new ImageSaver();

    /**
     * Input image list to produce high dynamic range image
     */
    private List<Image> mInputImageList = new ArrayList<Image>();

    private Size mPreviewSize;

    /**
     * A camera related listener/callback will be posted in this handler.
     */    
    private Handler mBackgroundHandler;
    private HandlerThread mBackgroundHandlerThread;

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
     * Number of input images to produce high dynamic range image from device
     */
    private int mDeviceHdrInputCnt = 0;

    /**
     * AE compensation step from device. Smallest step by which the exposure compensation can be changed.
     */
    private float mDeviceAeStep = 0;


    private int mReceivedImgCnt = 0;

    /**
     * AE compensation values of input images. {short, medium, long}
     */
    private final int AE_COMPENSATIONS[] = {-2, 0, 2};
    private final byte IDX_SHORT_AE = 0;
    private final byte IDX_MEDIUM_AE = 1;
    private final byte IDX_LONG_AE = 2;

    private enum CAMERA_STATE {
        IDLE, PREVIEW, WAIT_AF, WAIT_AE_LOCK, TAKE_HDR_PICTURE, CLOSING
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
    private class HdrSessionCaptureCallback extends SCameraCaptureSession.CaptureCallback {
        private CaptureHelper mResultListener = null;

        public void setResultListener(CaptureHelper listener) {
            mResultListener = listener;
        }

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
            // Depends on the current state and capture result, app will take next action.
            switch (getState()) {
                case IDLE:
                case PREVIEW:
                case CLOSING:
                    // do nothing
                    break;
                case TAKE_HDR_PICTURE:
                    if(mResultListener != null) {
                        mResultListener.checkPresetResult(result);
                    }
                    break;

                case WAIT_AF: {
                    if(isAFTriggered) {
                        Integer afState = result.get(SCaptureResult.CONTROL_AF_STATE);
                        // Check if AF is finished. If finished, take hdr picture.						
                        if(null == afState ||
                                SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState ||
                                SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState) {
                            isAFTriggered = false;
                            lockAE();
                        }
                    }
                    break;
                }

                case WAIT_AE_LOCK: {
                    if(result.get(SCaptureResult.CONTROL_AE_STATE) != null &&
                            result.get(SCaptureResult.CONTROL_AE_STATE) == SCaptureResult.CONTROL_AE_STATE_LOCKED) {
                        takeHdrPicture();
                    }
                    break;
                }
            }
        }
    }

    private HdrSessionCaptureCallback mSessionCaptureCallback = new HdrSessionCaptureCallback();

    /**
     * A {@link android.media.ImageReader.OnImageAvailableListener} for still capture.
     */
    private ImageReader.OnImageAvailableListener mImageCallback = new ImageReader.OnImageAvailableListener() {

        @Override
        public void onImageAvailable(ImageReader reader) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;

            mReceivedImgCnt++;

            Image image = reader.acquireNextImage();
            mInputImageList.add(image);

            // if we received enough images to produce hdr, request process to hdr processor.
            if(mReceivedImgCnt == mDeviceHdrInputCnt) {
                mReceivedImgCnt = 0;
                mProcessor.requestMultiProcess(mInputImageList);
            }
        }
    };

    private Bitmap decodeToBitmap(ByteBuffer jpegData, int sampleSize) {
        byte[] jpegDataArray = new byte[jpegData.remaining()];
        jpegData.get(jpegDataArray);
        jpegData.rewind();

        BitmapFactory.Options option = new BitmapFactory.Options();
        option.inSampleSize = sampleSize;

        return BitmapFactory.decodeByteArray(jpegDataArray, 0, jpegDataArray.length, option);
    }

    /**
     * Callback to receive result from hdr processor.
     */
    private SCameraHdrProcessor.EventCallback mProcessorCallback = new SCameraHdrProcessor.EventCallback() {

        /**
         * Called when {@link SCameraHdrProcessor#requestMultiProcess} is finished.
         */
        @Override
        public void onProcessCompleted(Image result) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;

            // decode result image to bitmap
            int sampleSize = 4;
            ByteBuffer jpegData = result.getPlanes()[0].getBuffer();
            Bitmap resultImg = decodeToBitmap(jpegData, sampleSize);

            // save result image to file
            mImageSaver.save(result, createFileName() + "_hdr.jpg");

            // decode input images
            Bitmap shortExpImg = decodeToBitmap(mInputImageList.get(IDX_SHORT_AE).getPlanes()[0].getBuffer(), sampleSize);
            Bitmap mediumExpImg = decodeToBitmap(mInputImageList.get(IDX_MEDIUM_AE).getPlanes()[0].getBuffer(), sampleSize);
            Bitmap longExpImg = decodeToBitmap(mInputImageList.get(IDX_LONG_AE).getPlanes()[0].getBuffer(), sampleSize);

            // show result dialog
            showResultDialog(shortExpImg, mediumExpImg, longExpImg, resultImg);
            clearInputImages();
            unlockAF();

            // done. re-start preview with default ae
            startPreview();
            setState(CAMERA_STATE.PREVIEW);
        }

        /**
         * Called when error occurred.
         */
        @Override
        public void onError(int code) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;

            StringBuilder builder = new StringBuilder();
            builder.append("Fail to create result: ");

            switch(code) {
                case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_DECODING_FAIL: {
                    builder.append("decoding fail");
                    break;
                }

                case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_ENCODING_FAIL: {
                    builder.append("encoding fail");
                    break;
                }

                case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_PROCESSING_FAIL: {
                    builder.append("processing fail");
                    break;
                }

                case SCameraHdrProcessor.NATIVE_PROCESSOR_MSG_UNKNOWN_ERROR: {
                    builder.append("unknown error");
                    break;
                }
            }
            showAlertDialog(builder.toString(), false);
            clearInputImages();
            unlockAF();
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_hdr);
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
        setState(CAMERA_STATE.CLOSING);

        setOrientationListener(false);

        stopBackgroundThread();

        deinitProcessor();
        closeCamera();

        super.onPause();
    }

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

            if(mCameraId == null) {
                showAlertDialog("No back-facing camera exist.", true);
                return false;
            }

            mCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(mCameraId);

            if(!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_CONTINUOUS_PICTURE)) {
                showAlertDialog("Required AF mode is not supported.", true);
                return false;
            }

            if(!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR)) {
                showAlertDialog("This device does not support SCamera Processor feature.", true);
                return false;
            }

            SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();
            if(!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_HDR)) {
                showAlertDialog("This device does not support HDR Processor.", true);
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

        mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_HDR);
    }

    private void initProcessor() {
        SCameraProcessorParameter parameter = mProcessor.getParameters();

        mDeviceHdrInputCnt = parameter.get(SCameraHdrProcessor.MULTI_INPUT_COUNT_RANGE).getUpper();

        parameter.set(SCameraHdrProcessor.STILL_INPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraHdrProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraHdrProcessor.STILL_SIZE, new Size(mImageReader.getWidth(), mImageReader.getHeight()));

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
     * Prepares an UI, like button, dialog, etc.
     */
    private void createUI() {
        findViewById(R.id.picture).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // take picture is only works under preview state. lock af first to take hdr picture.		
                if (getState() == CAMERA_STATE.PREVIEW)
                    lockAF();
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

    /**
     * Closes a camera and release resources.
     */
    private void closeCamera() {
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

            if (mImageReader != null) {
                mImageReader.close();
                mImageReader = null;
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

            // Acquires camera characteristics
            SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);

            // Acquires ae step from device. Smallest step by which the exposure compensation can be changed.
            mDeviceAeStep = characteristics.get(SCameraCharacteristics.CONTROL_AE_COMPENSATION_STEP).floatValue();

            StreamConfigurationMap streamConfigurationMap = characteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

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
            mImageReader = ImageReader.newInstance(jpegSize.getWidth(), jpegSize.getHeight(), ImageFormat.JPEG, mProcessor.getParameters().get(SCameraHdrProcessor.MULTI_INPUT_COUNT_RANGE).getUpper());
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

            // Initialize hdr processor
            initProcessor();

            // Opening the camera device here
            mSCameraManager.openCamera(mCameraId, new SCameraDevice.StateCallback() {
                public void onOpened(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    mSCameraDevice = sCameraDevice;
                    createPreviewSession();
                }

                @Override
                public void onDisconnected(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("Camera disconnected.", true);
                }

                @Override
                public void onError(SCameraDevice sCameraDevice, int i) {
                    mCameraOpenCloseLock.release();
                    if(getState() == CAMERA_STATE.CLOSING)
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
    private void createPreviewSession() {
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
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mPreviewBuilder.addTarget(surface);

            mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
            mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_CONTINUOUS_PICTURE);
            mCaptureBuilder.addTarget(mImageReader.getSurface());

            // HAL Workaround
            mPreviewBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);
            mCaptureBuilder.set(SCaptureRequest.METERING_MODE, SCaptureRequest.METERING_MODE_MATRIX);

            // Creates a SCameraCaptureSession here.
            List<Surface> outputSurface = Arrays.asList(surface, mImageReader.getSurface());
            mSCameraDevice.createCaptureSession(outputSurface, new SCameraCaptureSession.StateCallback() {
                @Override
                public void onConfigured(SCameraCaptureSession sCameraCaptureSession) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    mSCameraSession = sCameraCaptureSession;
                    startPreview();
                    setState(CAMERA_STATE.PREVIEW);
                }

                @Override
                public void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession) {
                    if(getState() == CAMERA_STATE.CLOSING)
                        return;
                    showAlertDialog("Fail to create camera session.", true);
                }
            }, mBackgroundHandler);

        } catch (CameraAccessException e) {
            showAlertDialog("Fail to create session. " + e.getMessage(), true);
        }
    }

    /**
     * Starts a preview.
     */
    private void startPreview() {
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, false);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, 0);
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
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

    private void lockAE() {
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            setState(CAMERA_STATE.WAIT_AE_LOCK);
        } catch(CameraAccessException e) {
            showAlertDialog("Fail to lock AE", true);
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

    private void clearInputImages() {
        for(Image i : mInputImageList) {
            i.close();
        }
        mInputImageList.clear();
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

                    MediaScannerConnection.scanFile(Sample_HDR.this,
                            new String[]{file.getAbsolutePath()}, null,
                            new MediaScannerConnection.OnScanCompletedListener() {
                                public void onScanCompleted(String path, Uri uri) {
                                    Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                }
                            });

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(Sample_HDR.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
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
     * Shows hdr result into dialog.
     */
    private void showResultDialog(Object... args) {
        final AlertDialog.Builder dialog = new AlertDialog.Builder(this);

        View dialogView = getLayoutInflater().inflate(R.layout.result_dialog_hdr, null);

        ImageView shortExpImage = (ImageView)dialogView.findViewById(R.id.shortExpImage);
        shortExpImage.setImageBitmap((Bitmap)args[0]);

        ImageView mediumExpImage = (ImageView)dialogView.findViewById(R.id.mediumExpImage);
        mediumExpImage.setImageBitmap((Bitmap)args[1]);

        ImageView longExpImage = (ImageView)dialogView.findViewById(R.id.longExpImage);
        longExpImage.setImageBitmap((Bitmap)args[2]);

        ImageView resultImage = (ImageView) dialogView.findViewById(R.id.resultHDRImage);
        resultImage.setImageBitmap((Bitmap) args[3]);

        dialog.setView(dialogView)
            .setTitle("Capture result")
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
     * take multi pictures with different AE to produce high dynamic range image
     */
    private void takeHdrPicture() {
        setState(CAMERA_STATE.TAKE_HDR_PICTURE);
        clearInputImages();

        // 1st.short exposure. make helper to capture short exposure image
        ExposureCaptureHelper shortCapture = new ExposureCaptureHelper();
        shortCapture.setAeBias((int) (AE_COMPENSATIONS[IDX_SHORT_AE] / mDeviceAeStep));

        // 2nd.medium exposure. make helper to capture medium exposure image
        ExposureCaptureHelper mediumCapture = new ExposureCaptureHelper();
        mediumCapture.setAeBias((int) (AE_COMPENSATIONS[IDX_MEDIUM_AE] / mDeviceAeStep));

        // 3rd.long exposure. make helper to capture long exposure image
        ExposureCaptureHelper longCapture = new ExposureCaptureHelper();
        longCapture.setAeBias((int) (AE_COMPENSATIONS[IDX_LONG_AE] / mDeviceAeStep));

        // set order
        shortCapture.setNext(mediumCapture);
        mediumCapture.setNext(longCapture);
        longCapture.setNext(null);

        // start capture
        shortCapture.start();
    }

    /**
     * A {@link com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback} for {@link com.samsung.android.sdk.camera.SCameraCaptureSession#capture(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)}
     */
    private class HdrCaptureCallback extends SCameraCaptureSession.CaptureCallback {
        private CaptureHelper mResultListener = null;

        private void setResultListener(CaptureHelper listener) {
            mResultListener = listener;
        }

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;
            if(mResultListener != null) {
                mResultListener.checkCaptureResult(result);
            }
        }
    }

    private HdrCaptureCallback mHdrCaptureCallback = new HdrCaptureCallback();

    /**
     * CaptureHelper interface
     */
    private interface CaptureHelper {
        public void start();
        public void preset();
        public void checkPresetResult(STotalCaptureResult result);
        public void capture();
        public void checkCaptureResult(STotalCaptureResult result);
        public void setNext(CaptureHelper next);
    }

    /**
     * ExposureCaptureHelper to support capture image with specific exposure compensation
     */
    private class ExposureCaptureHelper implements CaptureHelper {
        private CaptureHelper mNext = null;
        int mAeBias = 0;

        @Override
        public void start() {
            preset();
        }

        @Override
        public void preset() {
            try {
                mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
                mPreviewBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, mAeBias);

                mSessionCaptureCallback.setResultListener(ExposureCaptureHelper.this);
                mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            } catch (CameraAccessException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void checkPresetResult(STotalCaptureResult result) {
            if(result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) != null &&
                    result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) == mAeBias) {
                if(result.get(SCaptureResult.CONTROL_AE_STATE) != null &&
                        result.get(SCaptureResult.CONTROL_AE_STATE) == SCaptureResult.CONTROL_AE_STATE_LOCKED) {
                    mSessionCaptureCallback.setResultListener(null);
                    capture();
                }
            }
        }

        @Override
        public void capture() {
            // Orientation
            mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, getJpegOrientation());

            mCaptureBuilder.set(SCaptureRequest.CONTROL_AE_LOCK, true);
            mCaptureBuilder.set(SCaptureRequest.CONTROL_AE_EXPOSURE_COMPENSATION, mAeBias);

            mHdrCaptureCallback.setResultListener(ExposureCaptureHelper.this);
            try {
                mSCameraSession.capture(mCaptureBuilder.build(), mHdrCaptureCallback, mBackgroundHandler);
            } catch (CameraAccessException e) {
                e.printStackTrace();
            }

            if(mNext != null) {
                mNext.start();
            }
        }

        @Override
        public void checkCaptureResult(STotalCaptureResult result) {
            if(result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION) != null) {
                int aeBias = result.get(SCaptureResult.CONTROL_AE_EXPOSURE_COMPENSATION);
                Log.d(TAG, "aeBias of captured image : " + aeBias);
            }
        }

        @Override
        public void setNext(CaptureHelper next) {
            mNext = next;
        }

        public void setAeBias(int aeBias) {
            mAeBias = aeBias;
        }
    }
}
