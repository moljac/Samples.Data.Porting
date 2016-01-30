package com.samsung.android.sdk.camera.sample.cases;

import android.annotation.TargetApi;
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
import android.media.ImageWriter;
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
import android.view.Surface;
import android.view.TextureView;
import android.view.View;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCaptureSession;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraDevice;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.SCaptureRequest;
import com.samsung.android.sdk.camera.STotalCaptureResult;
import com.samsung.android.sdk.camera.params.InputConfiguration;
import com.samsung.android.sdk.camera.sample.R;
import com.samsung.android.sdk.camera.sample.cases.util.AutoFitTextureView;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.GregorianCalendar;
import java.util.List;
import java.util.TimeZone;
import java.util.concurrent.BlockingQueue;
import java.util.concurrent.LinkedBlockingQueue;
import java.util.concurrent.Semaphore;
import java.util.concurrent.TimeUnit;

@TargetApi(23)
public class Sample_ZSL extends Activity {
    private static final String TAG = Sample_ZSL.class.getSimpleName();

    private SCamera mSCamera;
    private SCameraManager mSCameraManager;
    private SCameraDevice mSCameraDevice;
    private SCameraCaptureSession mSCameraSession;
    private SCameraCharacteristics mCharacteristics;
    private SCaptureRequest.Builder mZslBuilder;

    /**
     * ID of the current {@link com.samsung.android.sdk.camera.SCameraDevice}.
     */
    private String mCameraId;

    private HandlerThread mBackgroundHandlerThread;
    private Handler mBackgroundHandler;
    private HandlerThread mReaderHandlerThread;
    private Handler mReaderHandler;

    private Semaphore mCameraOpenCloseLock = new Semaphore(1);

    private Size mPreviewSize;

    private static final int MAX_PREVIEW_WIDTH = 1920;
    private static final int MAX_PREVIEW_HEIGHT = 1080;

    private AutoFitTextureView mTextureView;

    private ImageReader mZslReader;
    private ImageReader mJpegReader;
    private ImageWriter mReprocessWriter;

    private ImageSaver mImageSaver = new ImageSaver();

    private ImageReader.OnImageAvailableListener mJpegImageListener = new ImageReader.OnImageAvailableListener() {
        @Override
        public void onImageAvailable(ImageReader reader) {
            Image image = reader.acquireNextImage();
            mImageSaver.save(image, createFileName() + ".jpg");
        }
    };

    //Simple ZSL image manager, It always captures image 3 frame ahead from current time. -_-;;;
    private class ImageManager extends SCameraCaptureSession.CaptureCallback implements ImageReader.OnImageAvailableListener {
        class CaptureImage {
            Image mImage;
            STotalCaptureResult mResult;

            CaptureImage(Image image, STotalCaptureResult result) {
                mImage = image;
                mResult = result;
            }
        };

        private BlockingQueue<STotalCaptureResult> mCaptureResultQueue = new LinkedBlockingQueue<STotalCaptureResult>();
        private BlockingQueue<CaptureImage> mCapturedImageQueue = new LinkedBlockingQueue<CaptureImage>();

        @Override
        public void onCaptureCompleted(SCameraCaptureSession session, SCaptureRequest request, STotalCaptureResult result) {
            try {
                mCaptureResultQueue.put(result);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void onImageAvailable(ImageReader reader) {
            STotalCaptureResult result = null;
            try {
                result = mCaptureResultQueue.take();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }

            addImage(reader.acquireNextImage(), result);
        }

        synchronized void addImage(Image image, STotalCaptureResult result) {
            try {
                mCapturedImageQueue.put(new CaptureImage(image, result));
                while(mCapturedImageQueue.size() > 3) mCapturedImageQueue.take().mImage.close();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }

        synchronized CaptureImage get() {
            try {
                return mCapturedImageQueue.take();
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            return null;
        }
    };
    private ImageManager mImageManager = new ImageManager();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_zsl);
    }

    @Override
    protected void onResume() {
        super.onResume();

        // initialize SCamera
        mSCamera = new SCamera();
        try {
            mSCamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }

        if(!checkRequiredFeatures()) return;

        mSCameraManager = mSCamera.getSCameraManager();

        mTextureView = (AutoFitTextureView) findViewById(R.id.texture);

        // Set SurfaceTextureListener that handle life cycle of TextureView
        mTextureView.setSurfaceTextureListener(new TextureView.SurfaceTextureListener() {
            @Override
            public void onSurfaceTextureAvailable(SurfaceTexture surface, int width, int height) {
                // "onSurfaceTextureAvailable" is called, which means that CameraCaptureSession is not created.
                // We need to configure transform for TextureView and crate CameraCaptureSession.
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

        findViewById(R.id.picture).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                takePicture();
            }
        });
        startBackgroundThread();

        openCamera();
    }

    private void takePicture() {
        ImageManager.CaptureImage image = mImageManager.get();
        try {
            SCaptureRequest.Builder builder = mSCameraDevice.createReprocessCaptureRequest(image.mResult);
            builder.addTarget(mJpegReader.getSurface());
            mReprocessWriter.queueInputImage(image.mImage);

            mSCameraSession.capture(builder.build(), null, mBackgroundHandler);
        } catch (CameraAccessException e) {
            e.printStackTrace();
        }
    }

    @Override
    protected void onPause() {
        stopBackgroundThread();
        closeCamera();

        super.onPause();
    }

    synchronized private void createPreviewSession() {

        if (null == mSCameraDevice
                || null == mSCameraManager
                || null == mPreviewSize
                || !mTextureView.isAvailable())
            return;

        try {
            SurfaceTexture texture = mTextureView.getSurfaceTexture();

            // Set default buffer size to camera preview size.
            texture.setDefaultBufferSize(mPreviewSize.getWidth(), mPreviewSize.getHeight());

            Surface surface = new Surface(texture);

            // Creates CaptureRequest.Builder for preview with output target.
            mZslBuilder = mSCameraDevice.createCaptureRequest(SCameraDevice.TEMPLATE_ZERO_SHUTTER_LAG);
            mZslBuilder.addTarget(surface);
            mZslBuilder.addTarget(mZslReader.getSurface());

            // Creates a CameraCaptureSession here.
            List<Surface> outputSurface = new ArrayList<Surface>();
            outputSurface.add(mZslReader.getSurface());
            outputSurface.add(surface);
            outputSurface.add(mJpegReader.getSurface());

            mSCameraDevice.createReprocessableCaptureSession(new InputConfiguration(mZslReader.getWidth(), mZslReader.getHeight(), mZslReader.getImageFormat()),
                    outputSurface, new SCameraCaptureSession.StateCallback() {
                        @Override
                        public void onConfigured(SCameraCaptureSession sCameraCaptureSession) {
                            mSCameraSession = sCameraCaptureSession;

                            // Configures an ImageWriter
                            mReprocessWriter = ImageWriter.newInstance(mSCameraSession.getInputSurface(), 2);
                            mReprocessWriter.setOnImageReleasedListener(new ImageWriter.OnImageReleasedListener() {
                                @Override
                                public void onImageReleased(ImageWriter writer) {
                                    Log.d(TAG, "onImageReleased");
                                }
                            }, mBackgroundHandler);
                            startPreview();
                        }

                        @Override
                        public void onConfigureFailed(SCameraCaptureSession sCameraCaptureSession) {
                            showAlertDialog("Fail to create camera capture session.", true);
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
            mSCameraSession.setRepeatingRequest(mZslBuilder.build(), mImageManager, mBackgroundHandler);
        } catch (CameraAccessException e) {
            showAlertDialog("Fail to start preview.", true);
        }
    }

    synchronized private void openCamera() {
        try {
            if(!mCameraOpenCloseLock.tryAcquire(3000, TimeUnit.MILLISECONDS)) {
                showAlertDialog("Time out waiting to lock camera opening.", true);
            }



            // acquires camera characteristics
            mCharacteristics = mSCameraManager.getCameraCharacteristics("0");

            StreamConfigurationMap streamConfigurationMap = mCharacteristics.get(SCameraCharacteristics.SCALER_STREAM_CONFIGURATION_MAP);

            // Acquires supported preview size list that supports SurfaceTexture
            mPreviewSize = streamConfigurationMap.getOutputSizes(SurfaceTexture.class)[0];
            for(Size option : streamConfigurationMap.getOutputSizes(SurfaceTexture.class)) {
                // Find maximum preview size that is not larger than MAX_PREVIEW_WIDTH/MAX_PREVIEW_HEIGHT
                int areaCurrent = Math.abs( (mPreviewSize.getWidth() * mPreviewSize.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));
                int areaNext = Math.abs( (option.getWidth() * option.getHeight()) - (MAX_PREVIEW_WIDTH * MAX_PREVIEW_HEIGHT));

                if(areaCurrent > areaNext) mPreviewSize = option;
            }

            // Acquires supported size for opaque format
            Size privateSize = streamConfigurationMap.getOutputSizes(ImageFormat.PRIVATE)[0];

            // Configures an ImageReader
            mZslReader = ImageReader.newInstance(privateSize.getWidth(), privateSize.getHeight(), ImageFormat.PRIVATE, 6);
            mJpegReader = ImageReader.newInstance(mZslReader.getWidth(), mZslReader.getHeight(), ImageFormat.JPEG, 2);

            mZslReader.setOnImageAvailableListener(mImageManager, mReaderHandler);
            mJpegReader.setOnImageAvailableListener(mJpegImageListener, mReaderHandler);

            // Set the aspect ratio to TextureView
            int orientation = getResources().getConfiguration().orientation;
            if (orientation == Configuration.ORIENTATION_LANDSCAPE) {
                mTextureView.setAspectRatio(mPreviewSize.getWidth(), mPreviewSize.getHeight());
            } else {
                mTextureView.setAspectRatio(mPreviewSize.getHeight(), mPreviewSize.getWidth());
            }

            // Opening the camera device here
            mSCameraManager.openCamera("0", new SCameraDevice.StateCallback() {
                public void onOpened(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    mSCameraDevice = sCameraDevice;
                    createPreviewSession();
                }

                @Override
                public void onDisconnected(SCameraDevice sCameraDevice) {
                    mCameraOpenCloseLock.release();
                    Log.e(TAG, "Camera Disconnected.");
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

            mSCameraManager = null;
        } catch (InterruptedException e) {
            Log.e(TAG, "Interrupted while trying to lock camera closing.", e);
        } finally {
            mCameraOpenCloseLock.release();
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
     * Starts back ground thread that callback from camera will posted.
     */
    private void startBackgroundThread() {
        mBackgroundHandlerThread = new HandlerThread("Background Thread");
        mBackgroundHandlerThread.start();
        mBackgroundHandler = new Handler(mBackgroundHandlerThread.getLooper());

        mReaderHandlerThread = new HandlerThread("Reader Thread");
        mReaderHandlerThread.start();
        mReaderHandler = new Handler(mReaderHandlerThread.getLooper());
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

        if (mReaderHandlerThread != null) {
            mReaderHandlerThread.quitSafely();
            try {
                mReaderHandlerThread.join();
                mReaderHandlerThread = null;
                mReaderHandler = null;
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

                    MediaScannerConnection.scanFile(Sample_ZSL.this,
                            new String[]{file.getAbsolutePath()}, null,
                            new MediaScannerConnection.OnScanCompletedListener() {
                                public void onScanCompleted(String path, Uri uri) {
                                    Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                                }
                            });

                    runOnUiThread(new Runnable() {
                        @Override
                        public void run() {
                            Toast.makeText(Sample_ZSL.this, "Saved: " + file.getName(), Toast.LENGTH_SHORT).show();
                        }
                    });
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
     * Convert int array to Integer list.
     */
    private List<Integer> asList(int[] array) {
        List<Integer> list = new ArrayList<Integer>();

        for (int value : array)  list.add(value);
        return list;
    }

    private boolean checkRequiredFeatures() {
        if(Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            showAlertDialog("Device running Android prior to M is not compatible with the reprocessing APIs.", true);
            Log.e(TAG, "Device running Android prior to M is not compatible with the reprocessing APIs.");

            return false;
        }

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

            if(!asList(mCharacteristics.get(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES))
                    .contains(SCameraCharacteristics.REQUEST_AVAILABLE_CAPABILITIES_PRIVATE_REPROCESSING)) {
                showAlertDialog("PRIVATE format reprocessing is not supported.", true);
                Log.e(TAG, "PRIVATE format reprocessing is not supported.");

                return false;
            }
        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
            return false;
        }

        return true;
    }
}
