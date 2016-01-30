package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.media.MediaScannerConnection;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.DocumentsContract;
import android.provider.MediaStore;
import android.util.DisplayMetrics;
import android.util.Log;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ImageView;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.image.SCameraImage;
import com.samsung.android.sdk.camera.image.SCameraImageCore;
import com.samsung.android.sdk.camera.image.SCameraImageMatrix;
import com.samsung.android.sdk.camera.sample.R;

import java.io.ByteArrayOutputStream;
import java.io.File;
import java.io.IOException;
import java.io.InputStream;
import java.util.GregorianCalendar;
import java.util.TimeZone;

public class Sample_Image extends Activity {
    /**
     * Tag for the {@link Log}.
     */
    private static final String TAG ="Sample_IPX";

    private SCamera mScamera;

    /**
     * Image buffer for processing image.
     */
    private byte[] buffer = null;
    private static ImageView mInputView;
    private static ImageView mOutputView;
    private static Bitmap mInputBitmap;
    private static Bitmap mOutputBitmap;
    private static SCameraImage mInputImage = null;
    private static SCameraImage mOutputImage = null;

    /**
     * Current Screen Width width app will use.
     */
    private int mScreenWidth = 0;
	
     /**

     * Current Screen Height app will use.

     */
    private int mScreenHeight = 0;

    private static final int SELECT_PICTURE = 1;

    /**
     * Prepares an UI, like button, dialog, etc. and
     */
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_ipx);
        Log.v(TAG, "onCreate");

        mInputView = (ImageView)findViewById(R.id.image_in);
        mOutputView = (ImageView)findViewById(R.id.image_out);

        // Get screen width.
        DisplayMetrics dm = new DisplayMetrics();
        getWindowManager().getDefaultDisplay().getMetrics(dm);
        mScreenWidth = dm.widthPixels;
        mScreenHeight = dm.heightPixels;

        // initialize SCamera
        mScamera = new SCamera();
        try {
            mScamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }
        if(!mScamera.isFeatureEnabled(SCamera.SCAMERA_IMAGE)) {
            showAlertDialog("This device does not support SCamera Image Processing Accelerator feature.", true);
            return;
        }
        Log.v(TAG, "SDK feature check passed");

        // initialize Gellery button
        ImageView openGalleryButton = (ImageView) findViewById(R.id.button1);
        openGalleryButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Intent intent = new Intent();
                intent.setType("image/*");
                intent.setAction(Intent.ACTION_GET_CONTENT);
                startActivityForResult(Intent.createChooser(intent, "Select Picture"), SELECT_PICTURE);

            }
        });

        makeBufferWithDefaultPath();
    }

    /**
     * Get selected image from gallery.
     */
    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (resultCode == RESULT_OK) {
            if (requestCode == SELECT_PICTURE) {
                Uri selectedImageUri = data.getData();

                if(!getContentResolver().getType(selectedImageUri).equals("image/jpeg")) {
                    Toast.makeText(this, "Only support JPEG file.", Toast.LENGTH_LONG).show();
                    return;
                }

                String imagePath = getPath(selectedImageUri);
                if(imagePath == null)
                {
                    Log.d(TAG, "imagepath from getPath null");
                    // get the id of the image selected by the user
                    String wholeID = DocumentsContract.getDocumentId(data.getData());
                    String id = wholeID.split(":")[1];

                    String[] projection = { MediaStore.Images.Media.DATA };
                    String whereClause = MediaStore.Images.Media._ID + "=?";
                    Cursor cursor = getContentResolver().query(getUri(), projection, whereClause, new String[]{id}, null);
                    if( cursor != null ){
                        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
                        if (cursor.moveToFirst()) {
                            imagePath = cursor.getString(column_index);
                        }

                        cursor.close();
                    } else {
                        imagePath = selectedImageUri.getPath();
                        Toast.makeText(this, imagePath, Toast.LENGTH_LONG).show();
                    }
                }

                Log.d(TAG, "selected file path "  + imagePath);

                if(mInputImage != null)
                    mInputImage.release();

                //Make SImage from selected Image and show to Screen.
                mInputImage = new SCameraImage(imagePath,SCameraImage.FORMAT_DEFAULT);
                mInputBitmap = getOutputBitmap(mInputImage);
                if(mInputBitmap == null)
                	makeBufferWithDefaultPath();
                else
                	mInputView.setImageBitmap(mInputBitmap);
                mOutputView.setImageBitmap(null);
            }
        }
	}
	
    private Uri getUri() {
        String state = Environment.getExternalStorageState();
        if(!state.equalsIgnoreCase(Environment.MEDIA_MOUNTED))
            return MediaStore.Images.Media.INTERNAL_CONTENT_URI;

        return MediaStore.Images.Media.EXTERNAL_CONTENT_URI;
    }

    private String getPath(Uri uri) {
        if (uri == null) {
            return null;
        }
        String[] projection = { MediaStore.Images.Media.DATA };
        Cursor cursor = getContentResolver().query(uri, projection, null, null, null);
        if (cursor != null) {
            int column_index = cursor
            .getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
            cursor.moveToFirst();

            String path = cursor.getString(column_index);
            cursor.close();

            return path;
        }
        return uri.getPath();
    }
    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_ipx, menu);
        return true;
    }

    /**
     * Each Image process working from selected item.
     */
    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.

        int id = item.getItemId();

        if(mOutputBitmap != null && id != R.id.Save_as_Jpeg && id != R.id.Save_as_Raw) {
            mOutputBitmap.recycle();
            mOutputBitmap = null;
        }

        if(mOutputImage != null && id != R.id.Save_as_Jpeg && id != R.id.Save_as_Raw) {
            mOutputImage.release();
            mOutputImage = null;
        }

        switch(id)
        {
            case R.id.Sobel:
            {
                Log.v(TAG, "Sobel");
                //Process sobel.
                try{
                	mOutputImage = SCameraImageCore.processSobel(mInputImage,100);

                	//Getting bitmap from output.
                	mOutputBitmap = getOutputBitmap(mOutputImage);

                	if(mOutputBitmap == null) {
                		Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                		mOutputImage.release();
                		return false;
                	}
                	else {
                		//Show output image
                		mOutputView.setImageBitmap(mOutputBitmap);
                	}
                }catch(IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                    return false;
                }
                return true;
            }

            case R.id.Median:
            {
            	Log.v(TAG, "Median");
            	//Process median.
            	try{
            		mOutputImage = SCameraImageCore.processMedian(mInputImage,5);
            		//Getting bitmap from output.
            		mOutputBitmap = getOutputBitmap(mOutputImage);

            		if(mOutputBitmap == null) {
            			Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
            			mOutputImage.release();
            			return false;
            		}
            		else {
            			//Show output image
            			mOutputView.setImageBitmap(mOutputBitmap);
            		}
            	}catch(IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                    return false;
            	}
                return true;
            }

            case R.id.Enhance_Contrast:
            {
                Log.v(TAG, "Enhance_Contrast");
                //Process Enhance_Contrast.
                try {
                	mOutputImage = SCameraImageCore.enhanceContrast(mInputImage,100, 0.5f);
                	//Getting bitmap from output.
                	mOutputBitmap = getOutputBitmap(mOutputImage);

                	if(mOutputBitmap == null) {
                		Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                		mOutputImage.release();
                		return false;
                	}
                	else {
                		//Show output image
                		mOutputView.setImageBitmap(mOutputBitmap);
                	}
                }catch(IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                    return false;
                }
                return true;
            }

            case R.id.Equalize_Histogram:
            {
            	Log.v(TAG, "Equalize_Histogram");
            	//Process Equalize_Histogram.
            	try{
            		mOutputImage = SCameraImageCore.equalizeHistogram(mInputImage);
            		//Getting bitmap from output.
            		mOutputBitmap = getOutputBitmap(mOutputImage);

            		if(mOutputBitmap == null) {
            			Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
            			mOutputImage.release();
            			return false;
            		}
            		else {
            			//Show output image
            			mOutputView.setImageBitmap(mOutputBitmap);
            		}
            	}catch(IllegalArgumentException e) {
                   Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                   return false;
            	}
                return true;
            }
            case R.id.Warp_Affine:
            {
            	Log.v(TAG, "Warp_Affine");
            	//Make matrix for Warp_Affine
            	try{
            		SCameraImageMatrix mat = SCameraImageCore.getDefaultAffineMatrix();
            		SCameraImageCore.calculateAffineScale(mat,0.5f,0.5f);
            		SCameraImageCore.calculateAffineSkew(mat,0.9f,0.8f);
            		SCameraImageCore.calculateAffineRotation(mat,30,400,600);
            		//Process Warp_Affine.
            		mOutputImage = SCameraImageCore.warpAffine(mInputImage,mat);
            		//Getting bitmap from output.
            		mOutputBitmap = getOutputBitmap(mOutputImage);
            		mat.release();
            		if(mOutputBitmap == null) {
            			Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
            			mOutputImage.release();
            			return false;
            		}
            		else {
            			//Show output image
            			mOutputView.setImageBitmap(mOutputBitmap);
            		}
            	}catch(IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
					return false;
            	}
                return true;
            }
            case R.id.Spatial_Filter:
            {
            	Log.v(TAG, "Spatial_Filter");
            	//Make matrix for Spatial_Filter
            	int N =7;
            	try{
            		SCameraImageMatrix matrix = new SCameraImageMatrix(N, N);
            		for(int i =0;i<N;i++)
            		{
            			for(int j =0;j<N;j++)
            			{
            				matrix.setAt(i,j,1);
            			}
            		}
            		//Process Spatial_Filter.
            		mOutputImage = SCameraImageCore.filterSpatial(mInputImage,matrix);
            		//Getting bitmap from output.
            		mOutputBitmap = getOutputBitmap(mOutputImage);

            		matrix.release();
            		if(mOutputBitmap == null) {
            			Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
            			mOutputImage.release();
            			return false;
            		}
            		else {
            			//Show output image
            			mOutputView.setImageBitmap(mOutputBitmap);
            		}
            	}catch(IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Failed to apply effect" , Toast.LENGTH_SHORT).show();
                    return false;
            	}
                return true;
            }

            case R.id.Save_as_Jpeg:
            {
                Log.v(TAG, "Save_as_Jpeg");
                if(mOutputImage == null){
                    Toast.makeText(getApplication().getBaseContext(), "No image to save", Toast.LENGTH_SHORT).show();
                    return true;
                }
                //Make filepath.
                File fileInjpg = new File(getFileName()+".jpg");
                //Save to file.
                try {
                    mOutputImage.saveAsJpeg(fileInjpg.getAbsolutePath(), 95);
                } catch (IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Save failed by " + e, Toast.LENGTH_SHORT).show();
                    return true;
                }
                Toast.makeText(getApplication().getBaseContext(), "Saved: " + fileInjpg, Toast.LENGTH_SHORT).show();

                MediaScannerConnection.scanFile(this,
                        new String[]{fileInjpg.getAbsolutePath()}, null,
                        new MediaScannerConnection.OnScanCompletedListener() {
                            public void onScanCompleted(String path, Uri uri) {
                                Log.i(TAG, "ExternalStorage Scanned " + path + "-> uri=" + uri);
                            }
                        });
                return true;
            }

            case R.id.Save_as_Raw:
            {
                Log.v(TAG, "Save_as_Raw");
                if(mOutputImage == null){
                    Toast.makeText(getApplication().getBaseContext(), "No image to save", Toast.LENGTH_SHORT).show();
                    return true;
                }
                //Make filepath.
                File fileInyuv = new File(getFileName()+".yuv");
                //Save to file.
                try {
                    mOutputImage.saveAsRaw(fileInyuv.getAbsolutePath());
                } catch (IllegalArgumentException e) {
                    Toast.makeText(getApplication().getBaseContext(), "Save failed by " + e, Toast.LENGTH_SHORT).show();
                    return true;
                }
                Toast.makeText(getApplication().getBaseContext(), "Saved: " + fileInyuv, Toast.LENGTH_SHORT).show();
                return true;
            }

        }
        return super.onOptionsItemSelected(item);
    }

    /**
     * Make file name for save image.
     */
    private String getFileName(){
        GregorianCalendar calendar = new GregorianCalendar();
        calendar.setTimeZone(TimeZone.getDefault());
        long dateTaken = calendar.getTimeInMillis();

        File dir = new File(Environment.getExternalStorageDirectory().getPath()+"/DCIM/Camera/");
        if (!dir.exists())
            dir.mkdirs();
        return Environment.getExternalStorageDirectory().getPath()+"/DCIM/Camera/"+String.valueOf(dateTaken);
    }

    /**
     * Load default image.
     */
    private void makeBufferWithDefaultPath() {
        Log.v(TAG, "makeBufferWithDefaultPath");
        buffer = null;
        InputStream inputStream = getResources().openRawResource(R.raw.flower_resize);
        ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream();

        int i;
        try {
            i = inputStream.read();
            while (i != -1) {
                byteArrayOutputStream.write(i);
                i = inputStream.read();
            }

            buffer = byteArrayOutputStream.toByteArray();
            inputStream.close();
        } catch (IOException e) {
            e.printStackTrace();
        }

        mInputImage = new SCameraImage(SCameraImage.FORMAT_DEFAULT, buffer);
        mInputBitmap = getOutputBitmap(mInputImage);
        mInputView.setImageBitmap(mInputBitmap);
    }

    /**
     * Getting bitmap for show UI. Proportion screen width, bitmap size calcurate.
     */
    private Bitmap getOutputBitmap(SCameraImage mImage){
        float scaleWidth = (float)mScreenWidth / (float)mImage.getWidth();
        float scaleHeight = (float)mScreenHeight/(float)mImage.getHeight();
        float scale = Math.min(scaleWidth, scaleHeight);
        try {
            if(scale < 1)
                return mImage.getBitmap((int)(scale * mImage.getWidth()), (int)(scale * mImage.getHeight()));
            else
                return mImage.getBitmap();
        } catch (OutOfMemoryError E) {
            Log.e(TAG,"Not enough memory to getbitmap");
            return null;
        } catch(IllegalArgumentException e) {
        	Toast.makeText(getApplication().getBaseContext(), "Failed to get bitmap." + e.getMessage(), Toast.LENGTH_SHORT).show();
        	return null;
        }
    }

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

    @Override
    protected void onDestroy() {
        Log.v(TAG, "onDestroy");
        if(mInputBitmap != null)
            mInputBitmap.recycle();
        if(mOutputBitmap != null)
            mOutputBitmap.recycle();
        if(mInputImage != null)
            mInputImage.release();
        buffer = null;

        mScamera = null;
        super.onPause();
    }
}
