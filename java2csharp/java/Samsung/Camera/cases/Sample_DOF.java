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
import android.widget.Button;
import android.widget.ImageView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCaptureSession;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraDevice;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.SCameraMetadata;
import com.samsung.android.sdk.camera.SCaptureRequest;
import com.samsung.android.sdk.camera.SCaptureResult;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.processor.SCameraDepthOfFieldProcessor;
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

public class Sample_DOF extends Activity {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_DOF";

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
     * Depth of field processor
     */
    private SCameraDepthOfFieldProcessor mProcessor;

    /**
     * An {@link com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView} for camera preview.
     */
    private AutoFitTextureView mTextureView;
    private ImageReader mImageReader;
    private ImageSaver mImageSaver = new ImageSaver();

    /**
     * Input image list to produce depth of field image
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
     * Number of input images to produce depth of field image from device
     */
    private int mDeviceDofInputCnt = 0;

    private int mReceivedImgCnt = 0;

    /**
     * distance value of infinity focus
     */
    private static final float INFINITY_FOCUS_DISTANCE = 0.0f;

    private static final byte IDX_AUTO_FOCUS_IMG = 0;
    private static final byte IDX_INFINITY_FOCUS_IMG = 1;

    private enum CAMERA_STATE {
        IDLE, PREVIEW, TAKE_DOF_PICTURE, CLOSING
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
    private class DofSessionCaptureCallback extends SCameraCaptureSession.CaptureCallback {
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
                case TAKE_DOF_PICTURE:
                    if(mResultListener != null) {
                        mResultListener.checkPresetResult(result);
                    }
                    break;
            }
        }
    }

    private DofSessionCaptureCallback mSessionCaptureCallback = new DofSessionCaptureCallback();

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

            // if we received enough images to produce depth of field image, request process to depth of field processor.
            if(mReceivedImgCnt == mDeviceDofInputCnt) {
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
     * Callback to receive result from depth of field processor.
     */
    private SCameraDepthOfFieldProcessor.EventCallback mProcessorCallback = new SCameraDepthOfFieldProcessor.EventCallback() {

        /**
         * Called as {@link SCameraDepthOfFieldProcessor#requestMultiProcess} is finished.
         *
         * @param result a Image object holds result image.
         * @param error error code when processing
         */
        @Override
        public void onProcessCompleted(Image result, int error) {
            if(getState() == CAMERA_STATE.CLOSING)
                return;

            // result image is produced, but error occurs when processing depth of field image.
            if(error != SCameraDepthOfFieldProcessor.PROCESSING_NO_ERROR) {
                processingDofError(error);
            }

            // decode result image to bitmap
            int sampleSize = 4;
            ByteBuffer jpegData = result.getPlanes()[0].getBuffer();
            Bitmap resultImg = decodeToBitmap(jpegData, sampleSize);

            // save result image to file
            mImageSaver.save(result, createFileName() + "_dof.jpg");

            // decode input images
            Bitmap autoFocusImg = decodeToBitmap(mInputImageList.get(IDX_AUTO_FOCUS_IMG).getPlanes()[0].getBuffer(), sampleSize);
            Bitmap infinityFocusImg = decodeToBitmap(mInputImageList.get(IDX_INFINITY_FOCUS_IMG).getPlanes()[0].getBuffer(), sampleSize);

            // show result dialog
            showResultDialog(autoFocusImg, infinityFocusImg, resultImg);
            clearInputImages();
            unlockAF();

           // done. re-start preview with default parameter
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
                case SCameraDepthOfFieldProcessor.NATIVE_PROCESSOR_MSG_DECODING_FAIL: {
                    builder.append("decoding fail");
                    break;
                }

                case SCameraDepthOfFieldProcessor.NATIVE_PROCESSOR_MSG_ENCODING_FAIL: {
                    builder.append("encoding fail");
                    break;
                }

                case SCameraDepthOfFieldProcessor.NATIVE_PROCESSOR_MSG_PROCESSING_FAIL: {
                    builder.append("processing fail");
                    break;
                }

                case SCameraDepthOfFieldProcessor.NATIVE_PROCESSOR_MSG_UNKNOWN_ERROR: {
                    builder.append("unknown error");
                    break;
                }
            }
            showAlertDialog(builder.toString(), false);
            clearInputImages();
            unlockAF();
            setState(CAMERA_STATE.PREVIEW);
        }

        /**
         * Show error code when processing depth of field
         * @param code error code when processing
         */
        private void processingDofError(int code) {
            final StringBuilder builder = new StringBuilder();

            switch(code) {
                case SCameraDepthOfFieldProcessor.PROCESSING_ERROR_AF: {
                    builder.append(getResources().getText(R.string.depth_of_field_error_af));
                    break;
                }

                case SCameraDepthOfFieldProcessor.PROCESSING_ERROR_INF: {
                    builder.append(getResources().getText(R.string.depth_of_field_error_inf));
                    break;
                }

                case SCameraDepthOfFieldProcessor.PROCESSING_ERROR_SEGMENTATION: {
                    builder.append(getResources().getText(R.string.depth_of_field_error_segmentation));
                    break;
                }
            }

            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    Log.e(TAG, "runOnUiThread - run");
                    Toast.makeText(Sample_DOF.this, builder.toString(), Toast.LENGTH_SHORT).show();
                }
            });
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_dof);
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

    private void createProcessor() {
        SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();

        mProcessor = processorManager.createProcessor(SCameraProcessorManager.PROCESSOR_TYPE_DEPTH_OF_FIELD);
    }

    private void initProcessor() {
        SCameraProcessorParameter parameter = mProcessor.getParameters();

        mDeviceDofInputCnt = parameter.get(SCameraDepthOfFieldProcessor.MULTI_INPUT_COUNT_RANGE).getUpper();

        parameter.set(SCameraDepthOfFieldProcessor.STILL_INPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraDepthOfFieldProcessor.STILL_OUTPUT_FORMAT, ImageFormat.JPEG);
        parameter.set(SCameraDepthOfFieldProcessor.STILL_SIZE, new Size(mImageReader.getWidth(), mImageReader.getHeight()));

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

            if(!mCharacteristics.getAvailableCaptureRequestKeys().contains(SCaptureRequest.LENS_FOCUS_DISTANCE) ||
                    !contains(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES), SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_MANUAL_SENSOR) ||
                    !(mCharacteristics.get(SCameraCharacteristics.LENS_INFO_MINIMUM_FOCUS_DISTANCE) > 0.0f)) {
                showAlertDialog("Required SCaptureRequest.Key is not supported.", true);
                return false;
            }

            if(!contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_AUTO) ||
                    !contains(mCharacteristics.get(SCameraCharacteristics.CONTROL_AF_AVAILABLE_MODES), SCameraCharacteristics.CONTROL_AF_MODE_OFF) ) {
                showAlertDialog("Required AF mode is not supported.", true);
                return false;
            }

            if(!mSCamera.isFeatureEnabled(SCamera.SCAMERA_PROCESSOR)) {
                showAlertDialog("This device does not support SCamera Processor feature.", true);
                return false;
            }

            SCameraProcessorManager processorManager = mSCamera.getSCameraProcessorManager();
            if(!processorManager.isProcessorAvailable(SCameraProcessorManager.PROCESSOR_TYPE_DEPTH_OF_FIELD)) {
                showAlertDialog("This device does not support DOF Processor.", true);
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
     * Prepares an UI, like button, dialog, etc.
     */
    private void createUI() {
        Button button = (Button) findViewById(R.id.picture);
        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                // take picture is only works under preview state.		
                if (getState() == CAMERA_STATE.PREVIEW) {
                    mBackgroundHandler.post(new Runnable() {
                        @Override
                        public void run() {
                            setState(CAMERA_STATE.TAKE_DOF_PICTURE);
                            takeDofPicture();
                        }
                    });
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

            // acquires camera characteristics
            SCameraCharacteristics characteristics = mSCameraManager.getCameraCharacteristics(mCameraId);
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
            mImageReader = ImageReader.newInstance(jpegSize.getWidth(), jpegSize.getHeight(), ImageFormat.JPEG, mProcessor.getParameters().get(SCameraDepthOfFieldProcessor.MULTI_INPUT_COUNT_RANGE).getUpper());
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

            // Initialize depth of field processor
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
            mPreviewBuilder.addTarget(surface);

            mCaptureBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_STILL_CAPTURE);
            mCaptureBuilder.addTarget(mImageReader.getSurface());

            // remove af default lens distance
            mPreviewBuilder.set(SCaptureRequest.LENS_FOCUS_DISTANCE, null);
            mCaptureBuilder.set(SCaptureRequest.LENS_FOCUS_DISTANCE, null);

            // set default AF trigger
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCameraMetadata.CONTROL_AF_TRIGGER_IDLE);
            mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCameraMetadata.CONTROL_AF_TRIGGER_IDLE);

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
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_AUTO);
            mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to create preview session. " + e.getMessage(), true);
        }
    }

    /**
     * Unlock AF.
     */
    private void unlockAF() {
        // Triggers CONTROL_AF_TRIGGER_CANCEL to return to initial AF state.
        try {
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_CANCEL);
            mSCameraSession.capture(mPreviewBuilder.build(), null, mBackgroundHandler);
            mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to cancel AF", false);
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

            MediaScannerConnection.scanFile(Sample_DOF.this,
                    new String[]{file.getAbsolutePath()}, null,
                    new MediaScannerConnection.OnScanCompletedListener() {
                        public void onScanCompleted(String path, Uri uri) {
                            Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                        }
                    });

            runOnUiThread(new Runnable() {
                @Override
                public void run() {
                    Toast.makeText(Sample_DOF.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
                }
            });
        }
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

    private boolean contains(final int[] array, final int key) {
        for (final int i : array) {
            if (i == key) {
                return true;
            }
        }
        return false;
    }
	
    /**
     * Shows depth of field result into dialog.
     */
    private void showResultDialog(Object... args) {
        final AlertDialog.Builder dialog = new AlertDialog.Builder(this);

        View dialogView = getLayoutInflater().inflate(R.layout.result_dialog_dof, null);

        ImageView autoFocusImage = (ImageView)dialogView.findViewById(R.id.autoFocusImage);
        autoFocusImage.setImageBitmap((Bitmap)args[0]);

        ImageView infinityFocusImage = (ImageView)dialogView.findViewById(R.id.infinityFocusImage);
        infinityFocusImage.setImageBitmap((Bitmap)args[1]);

        ImageView resultImage = (ImageView) dialogView.findViewById(R.id.resultDOFImage);
        resultImage.setImageBitmap((Bitmap)args[2]);

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
     * take multi pictures with different focus distance to produce depth of field image.
     * one with auto focus and the other with infinity focus
     */
    private void takeDofPicture() {
        clearInputImages();

        // 1st.auto focus. make helper to capture with auto focus
        AutoFocusCaptureHelper afCapture = new AutoFocusCaptureHelper();

        // 2nd.infinity focus. make helper to capture with infinity focus
        ManualFocusCaptureHelper infCapture = new ManualFocusCaptureHelper();
        infCapture.setFocusDistance(INFINITY_FOCUS_DISTANCE);

        // set order
        afCapture.setNext(infCapture);
        infCapture.setNext(null);

        // start capture
        afCapture.start();
    }

    /**
     * A {@link com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback} for {@link com.samsung.android.sdk.camera.SCameraCaptureSession#capture(com.samsung.android.sdk.camera.SCaptureRequest, com.samsung.android.sdk.camera.SCameraCaptureSession.CaptureCallback, android.os.Handler)}
     */
    private class DofCaptureCallback extends SCameraCaptureSession.CaptureCallback {
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

    private DofCaptureCallback mDofCaptureCallback = new DofCaptureCallback();

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
     * CaptureHelper to support capture image with auto focus
     */
    private class AutoFocusCaptureHelper implements CaptureHelper {
        private CaptureHelper mNext = null;
        private boolean isAFTriggered = false;

        @Override
        public void start() {
            preset();
        }

        @Override
        public void preset() {
            try {
                isAFTriggered = false;

                // Set AF trigger to SCaptureRequest.Builder
                mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_START);
                mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                    @Override
                    public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                        if(getState() == CAMERA_STATE.CLOSING)
                            return;
                        mSessionCaptureCallback.setResultListener(AutoFocusCaptureHelper.this);
                        isAFTriggered = true;
                    }
                }, mBackgroundHandler);
                mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_TRIGGER, SCaptureRequest.CONTROL_AF_TRIGGER_IDLE);
            } catch (CameraAccessException e) {
                showAlertDialog("Fail to trigger AF", true);
            }
        }

        @Override
        public void checkPresetResult(STotalCaptureResult result) {
            if(isAFTriggered) {
                Integer afState = result.get(SCaptureResult.CONTROL_AF_STATE);
                if(null == afState ||
                        SCaptureResult.CONTROL_AF_STATE_FOCUSED_LOCKED == afState ||
                        SCaptureResult.CONTROL_AF_STATE_NOT_FOCUSED_LOCKED == afState) {
                    isAFTriggered = false;
                    mSessionCaptureCallback.setResultListener(null);
                    capture();
                }
            }
        }

        @Override
        public void capture() {
            // Orientation
            mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, getJpegOrientation());

            mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_AUTO);

            mDofCaptureCallback.setResultListener(AutoFocusCaptureHelper.this);
            try {
                mSCameraSession.capture(mCaptureBuilder.build(), mDofCaptureCallback, mBackgroundHandler);
            } catch (CameraAccessException e) {
                e.printStackTrace();
            }

            if(mNext != null) {
                mNext.start();
            }
        }

        @Override
        public void checkCaptureResult(STotalCaptureResult result) {
            if(result.get(SCaptureResult.LENS_FOCUS_DISTANCE) != null ) {
                mDofCaptureCallback.setResultListener(null);
                float distance = result.get(SCaptureResult.LENS_FOCUS_DISTANCE);
                Log.d(TAG, "focus distance of captured image : " + distance);
            }
        }

        @Override
        public void setNext(CaptureHelper next) {
            mNext = next;
        }
    }

    /**
     * CaptureHelper to support capture image with manual focus
     */
    private class ManualFocusCaptureHelper implements CaptureHelper {
        private CaptureHelper mNext = null;
        boolean mRequestTriggered = false;
        float mFocusDistance = 0.0f; // 0.0f means infinity focus

        @Override
        public void start() {
            preset();
        }

        @Override
        public void preset() {
            try {
                // set af mode off to use manual focus
                mPreviewBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_OFF);
                // set focus distance
                mPreviewBuilder.set(SCaptureRequest.LENS_FOCUS_DISTANCE, mFocusDistance);

                mSessionCaptureCallback.setResultListener(ManualFocusCaptureHelper.this);
                mSCameraSession.capture(mPreviewBuilder.build(), new SCameraCaptureSession.CaptureCallback() {
                    @Override
                    public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
                        mRequestTriggered = true;
                    }
                }, mBackgroundHandler);
                mSCameraSession.setRepeatingRequest(mPreviewBuilder.build(), mSessionCaptureCallback, mBackgroundHandler);
            } catch (CameraAccessException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void checkPresetResult(STotalCaptureResult result) {
            if(result.get(SCaptureResult.LENS_STATE) != null) {
                // There is no guarantee that the focus distance value in the capture result will be the same value in the capture request, after lens moved.
                // The 'LENS_STATE' value should be used to determine whether the lens is moved to requested focus point.
                if(result.get(SCaptureResult.LENS_STATE) == SCameraMetadata.LENS_STATE_STATIONARY && mRequestTriggered) {
                    mSessionCaptureCallback.setResultListener(null);
                    capture();
                }
            }
        }

        @Override
        public void capture() {
            // Orientation
            mCaptureBuilder.set(SCaptureRequest.JPEG_ORIENTATION, getJpegOrientation());

            // set af mode off to use manual focus
            mCaptureBuilder.set(SCaptureRequest.CONTROL_AF_MODE, SCaptureRequest.CONTROL_AF_MODE_OFF);
            // set focus distance
            mCaptureBuilder.set(SCaptureRequest.LENS_FOCUS_DISTANCE, mFocusDistance);

            mDofCaptureCallback.setResultListener(ManualFocusCaptureHelper.this);
            try {
                mSCameraSession.capture(mCaptureBuilder.build(), mDofCaptureCallback, mBackgroundHandler);
            } catch (CameraAccessException e) {
                e.printStackTrace();
            }

            mCaptureBuilder.set(SCaptureRequest.LENS_FOCUS_DISTANCE, null);

            if(mNext != null) {
                mNext.start();
            }
        }

        @Override
        public void checkCaptureResult(STotalCaptureResult result) {
            if(result.get(SCaptureResult.LENS_FOCUS_DISTANCE) != null ) {
                mDofCaptureCallback.setResultListener(null);
                float distance = result.get(SCaptureResult.LENS_FOCUS_DISTANCE);
                Log.d(TAG, "focus distance of captured image : " + distance);
            }
        }

        @Override
        public void setNext(CaptureHelper next) {
            mNext = next;
        }

        public void setFocusDistance(float focusDistance) {
            mFocusDistance = focusDistance;
        }
    }
}
