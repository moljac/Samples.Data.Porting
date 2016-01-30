package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.filter.SCameraFilter;
import com.samsung.android.sdk.camera.filter.SCameraFilterInfo;
import com.samsung.android.sdk.camera.filter.SCameraFilterManager;
import com.samsung.android.sdk.camera.sample.R;

import java.io.File;
import java.io.FileFilter;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import java.util.List;

public class Sample_Filter extends Activity implements View.OnClickListener {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG = "Sample_Filter";

    // SCameraFilter only supports a 8192x8192 or less resolution.
    private static final int MAX_IMAGE_SIZE = 8192;

    private final int PROCESS_BITMAP = 1;
    private final int PROCESS_FILE = 2;

    private final String mCameraDirectory = Environment.getExternalStorageDirectory().getPath() + "/DCIM/Camera";

    private SCamera mSCamera = null;

    /**
     * list of retrieving available filters.
     */
    private List<SCameraFilterInfo> mFilterInfoList;

    /**
     * {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager} for creating and retrieving available filters
     */
    private SCameraFilterManager mSCameraFilterManager;


    /**
     * Button to processing for bitmap format
     */
    private Button mBitmapButton = null;

    /**
     * Button to processing for image file
     */
    private Button mFileButton = null;

    /**
     * ImageView to display a selected image.
     */
    private ImageView mInputImageView = null;

    /**
     * ImageView to display a processed image.
     */
    private ImageView mOutputImageView = null;

    /**
     * TextView to display a selected filter.
     */
    private TextView mSelectedFilterTextView = null;

    /**
     * ID of selected filter.
     */
    private int mFilterID = -1;

    /**
     * list of filter name
     */
    private String[] mFilterNames = null;

    /**
     * list of file in [/DCIM/Camera]
     */
    private String[] mFileNames = null;

    /**
     * file path of a processed image.
     */
    private String mInputImage = null;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_filter);

        mSelectedFilterTextView = (TextView) findViewById(R.id.selected_textview);

        findViewById(R.id.btn_select_image).setOnClickListener(this);
        findViewById(R.id.btn_select_filter).setOnClickListener(this);

        mBitmapButton = (Button) findViewById(R.id.btn_bitmap);
        mFileButton = (Button) findViewById(R.id.btn_file);

        mBitmapButton.setOnClickListener(this);
        mFileButton.setOnClickListener(this);

        mBitmapButton.setEnabled(false);
        mFileButton.setEnabled(false);

        mInputImageView = (ImageView) findViewById(R.id.input_image_view);
        mOutputImageView = (ImageView) findViewById(R.id.output_image_view);

        mSCamera = new SCamera();
        try {
            mSCamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }
        if (!mSCamera.isFeatureEnabled(SCamera.SCAMERA_FILTER)) {
            showAlertDialog("This device does not support SCamera Filter feature.", true);
            return;
        }

        // retrieving an {@link com.samsung.android.sdk.camera.filter.SCameraFilterManager}
        mSCameraFilterManager = mSCamera.getSCameraFilterManager();

        // retrieving available filters
        mFilterInfoList = mSCameraFilterManager.getAvailableFilters();

        if (mFilterInfoList.size() > 0)
            mSelectedFilterTextView.setText(mFilterInfoList.get(0).getName());

    }

    @Override
    protected void onDestroy() {
        mSCamera = null;
        super.onDestroy();
    }

    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.btn_select_image:
                selectImage();
                break;
            case R.id.btn_select_filter:
                selectFilter();
                break;
            case R.id.btn_bitmap:
                process(PROCESS_BITMAP);
                break;
            case R.id.btn_file:
                process(PROCESS_FILE);
                break;
        }
    }

    /**
     * select an image file for processing filter.
     */
    private void selectImage() {

        mFileNames = null;

        File cameraDirectory = new File(mCameraDirectory);
        if (!cameraDirectory.exists()) {
            cameraDirectory.mkdirs();
            return;
        }

        File[] cameraImages = cameraDirectory.listFiles(new FileFilter() {
            public boolean accept(File file) {
                return file.isFile() && file.getName().toLowerCase().endsWith(".jpg");
            }
        });
        if (cameraImages == null || cameraImages.length == 0) {
            return;
        }

        mFileNames = new String[cameraImages.length];

        for (int i = 0; i < cameraImages.length; i++) {
            mFileNames[i] = mCameraDirectory + "/" + cameraImages[i].getName();
        }

        new AlertDialog.Builder(Sample_Filter.this)
                .setTitle("Select Image")
                .setItems(mFileNames, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        String inputImage = mFileNames[which];

                        if (!setImageView(mInputImageView, inputImage)) {
                            showAlertDialog("This image resolution is greater than the " + MAX_IMAGE_SIZE + "x" + MAX_IMAGE_SIZE, false);
                            return;
                        }

                        mInputImage = inputImage;
                        mBitmapButton.setEnabled(true);
                        mFileButton.setEnabled(true);
                    }
                }).show();

    }

    @Override
    protected void onResume() {
        super.onResume();

        if(mInputImage != null) {
            File inputFile = new File(mInputImage);
            if(!inputFile.exists()) {
                mInputImage = null;
                mBitmapButton.setEnabled(false);
                mFileButton.setEnabled(false);
                mInputImageView.setImageDrawable(getDrawable(R.drawable.bg_image_frame));
                mOutputImageView.setImageDrawable(getDrawable(R.drawable.bg_image_frame));
            }
        }
    }

    /**
     * select an filter file for processing.
     */
    private void selectFilter() {

        int filterNum = mFilterInfoList.size();
        if (filterNum < 1)
            return;

        mFilterNames = new String[filterNum];

        for (int i = 0; i < filterNum; i++) {
            mFilterNames[i] = mFilterInfoList.get(i).getName();
        }

        new AlertDialog.Builder(Sample_Filter.this)
                .setTitle("Select Filter")
                .setItems(mFilterNames, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        mFilterID = which;
                        mSelectedFilterTextView.setText(mFilterNames[which]);
                    }
                }).show();
    }

    private void process(int type) {
        if (mInputImage == null) {
            Toast tempToast = Toast.makeText(Sample_Filter.this, "please select image.", Toast.LENGTH_SHORT);
            tempToast.show();
            return;
        }

        String outputPath = mCameraDirectory + "/FILTER_IMAGE.jpg";
        processFilter(mInputImage, outputPath, type);
        setImageView(mOutputImageView, outputPath);

        MediaScannerConnection.scanFile(this,
                new String[]{outputPath}, null,
                new MediaScannerConnection.OnScanCompletedListener() {
                    public void onScanCompleted(String path, Uri uri) {
                        Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                    }
                });
    }

    /**
     * display an image file.
     */
    private boolean setImageView(ImageView imageview, String filepath) {

        boolean bSCameraFilterSupported = true;

        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeFile(filepath, options);

        int photoWidth = options.outWidth;
        int photoHeight = options.outHeight;

        int targetWidth = photoWidth;
        int targetHeight = photoHeight;

        int photoSize = (photoWidth > photoHeight) ? photoWidth : photoHeight;

        if (photoSize> MAX_IMAGE_SIZE) {
            bSCameraFilterSupported = false;
            return bSCameraFilterSupported;
        }

        if (photoSize > 1920) {
            int scale = (photoSize / 1920) + 1;
            targetWidth = photoWidth / scale;
            targetHeight = photoHeight / scale;
        }

        int scaleFactor = Math.min(photoWidth / targetWidth, photoHeight / targetHeight);

        options.inJustDecodeBounds = false;
        options.inDither = false;
        options.inSampleSize = scaleFactor;
        Bitmap orgImage = BitmapFactory.decodeFile(filepath, options);
        imageview.setImageBitmap(orgImage);

        return bSCameraFilterSupported;
    }

    /**
     * save an bitmap object.
     */
    private void saveImage(Bitmap bitmap, String filename) {

        if (bitmap != null) {

            File file = new File(filename);
            OutputStream out = null;
            try {
                file.createNewFile();
                out = new FileOutputStream(file);
                bitmap.compress(Bitmap.CompressFormat.JPEG, 95, out);
                bitmap.recycle();
            } catch (Exception e) {
                e.printStackTrace();
            } finally {
                if(out != null) {
                    try {
                    out.close();
                    } catch (IOException e) {
                        e.printStackTrace();
                    }
                }
            }

        }
    }

    // ----------------------------------------------------------------------------

    /**
     * process a filter
     */
    private void processFilter(String inputFile, String outputFile, int type) {
        Log.e(TAG, "InputFile:  " + inputFile);
        Log.e(TAG, "OutputFile:  " + outputFile);
        Log.e(TAG, "Type:" + type);
        Log.e(TAG, "Filter:  " + mFilterID);

        SCameraFilter filter;

        // create a SCameraFilter using SCameraFilterManager.
        if (mFilterID == -1)
            filter = mSCameraFilterManager.createFilter(mFilterInfoList.get(0));
        else
            filter = mSCameraFilterManager.createFilter(mFilterInfoList.get(mFilterID));

        switch (type) {
            case PROCESS_BITMAP:
                Bitmap inputBitmap = BitmapFactory.decodeFile(inputFile);

                // process filter using bitmap object
                Bitmap outputBitmap = filter.processImage(inputBitmap);
                if (outputBitmap == null) {
                    return;
                }

                // save a bitmap object
                saveImage(outputBitmap, outputFile);

                inputBitmap.recycle();
                outputBitmap.recycle();
                break;
            case PROCESS_FILE:
                // process filter using image file
                filter.processImage(inputFile, outputFile);
                break;
        }
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
