package com.example.filter_test;

import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.OutputStream;
import android.app.Activity;
import android.content.CursorLoader;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.Bitmap.CompressFormat;
import android.graphics.Bitmap.Config;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;
import android.net.Uri;
import android.os.Build;
import android.provider.MediaStore;

public class AnimationUtils {

    /**
     * Get the real file path from URI registered in media store
     * 
     * @param contentUri
     *            URI registered in media store
     */
    public static String getRealPathFromURI(Activity activity, Uri contentUri) {

        String releaseNumber = Build.VERSION.RELEASE;

        if (releaseNumber != null) {
            /* ICS Version */
            if (releaseNumber.length() > 0 && releaseNumber.charAt(0) == '4') {
                String[] proj = { MediaStore.Images.Media.DATA };
                String strFileName = "";
                CursorLoader cursorLoader = new CursorLoader(activity, contentUri, proj, null, null, null);
                if (cursorLoader != null) {
                    Cursor cursor = cursorLoader.loadInBackground();
                    if (cursor != null) {
                        int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
                        cursor.moveToFirst();
                        if (cursor.getCount() > 0) {
                            strFileName = cursor.getString(column_index);
                        }

                        cursor.close();
                    }
                }
                return strFileName;
            }
            /* GB Version */
            else if (releaseNumber.startsWith("2.3")) {
                String[] proj = { MediaStore.Images.Media.DATA };
                String strFileName = "";
                Cursor cursor = activity.managedQuery(contentUri, proj, null, null, null);
                if (cursor != null) {
                    int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

                    cursor.moveToFirst();
                    if (cursor.getCount() > 0) {
                        strFileName = cursor.getString(column_index);
                    }

                    cursor.close();
                }
                return strFileName;
            }
        }

        // ---------------------
        // Undefined Version
        // ---------------------
        /* GB, ICS Common */
        String[] proj = { MediaStore.Images.Media.DATA };
        String strFileName = "";
        Cursor cursor = activity.managedQuery(contentUri, proj, null, null, null);
        if (cursor != null) {
            int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

            // Use the Cursor manager in ICS
            activity.startManagingCursor(cursor);

            cursor.moveToFirst();
            if (cursor.getCount() > 0) {
                strFileName = cursor.getString(column_index);
            }

            // cursor.close(); // If the cursor close use , This application is terminated .(In ICS Version)
            activity.stopManagingCursor(cursor);
        }
        return strFileName;
    }

    /**
     * Save jpeg image with background color
     * 
     * @param strFileName
     *            Save file path
     * @param bitmap
     *            Input bitmap
     * @param nQuality
     *            Jpeg quality for saving
     * @param nBackgroundColor
     *            background color
     * @return whether success or not
     */
    public static boolean saveBitmapJPEGWithBackgroundColor(String strFileName, Bitmap bitmap, int nQuality,
            int nBackgroundColor) {
        boolean bSuccess1 = false;
        boolean bSuccess2 = false;
        boolean bSuccess3;
        File saveFile = new File(strFileName);

        if (saveFile.exists()) {
            if (!saveFile.delete()) {
                return false;
            }
        }

        int nA = (nBackgroundColor >> 24) & 0xff;

        // If Background color alpha is 0, Background color substitutes as white
        if (nA == 0) {
            nBackgroundColor = 0xFFFFFFFF;
        }

        Rect rect = new Rect(0, 0, bitmap.getWidth(), bitmap.getHeight());
        Bitmap newBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Config.ARGB_8888);
        Canvas canvas = new Canvas(newBitmap);
        canvas.drawColor(nBackgroundColor);
        canvas.drawBitmap(bitmap, rect, rect, new Paint());

        // Quality limitation min/max
        if (nQuality < 10) {
            nQuality = 10;
        } else if (nQuality > 100) {
            nQuality = 100;
        }

        OutputStream out = null;

        try {
            bSuccess1 = saveFile.createNewFile();
        } catch (IOException e1) {
            // TODO Auto-generated catch block
            e1.printStackTrace();
        }

        try {
            out = new FileOutputStream(saveFile);
            bSuccess2 = newBitmap.compress(CompressFormat.JPEG, nQuality, out);
        } catch (Exception e) {
            e.printStackTrace();
        }

        try {
            if (out != null) {
                out.flush();
                out.close();
                bSuccess3 = true;
            } else {
                bSuccess3 = false;
            }

        } catch (IOException e) {
            e.printStackTrace();
            bSuccess3 = false;
        } finally {
            if (out != null) {
                try {
                    out.close();
                } catch (IOException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                }
            }
        }

        return (bSuccess1 && bSuccess2 && bSuccess3);
    }

    /**
     * Save PNG image with background color
     * 
     * @param strFileName
     *            Save file path
     * @param bitmap
     *            Input bitmap
     * @param nQuality
     *            Jpeg quality for saving
     * @param nBackgroundColor
     *            background color
     * @return whether success or not
     */
    public static boolean saveBitmapPNGWithBackgroundColor(String strFileName, Bitmap bitmap, int nBackgroundColor) {
        boolean bSuccess1 = false;
        boolean bSuccess2 = false;
        boolean bSuccess3;
        File saveFile = new File(strFileName);

        if (saveFile.exists()) {
            if (!saveFile.delete()) {
                return false;
            }
        }

        int nA = (nBackgroundColor >> 24) & 0xff;

        // If Background color alpha is 0, Background color substitutes as white
        if (nA == 0) {
            nBackgroundColor = 0xFFFFFFFF;
        }

        Rect rect = new Rect(0, 0, bitmap.getWidth(), bitmap.getHeight());
        Bitmap newBitmap = Bitmap.createBitmap(bitmap.getWidth(), bitmap.getHeight(), Config.ARGB_8888);
        Canvas canvas = new Canvas(newBitmap);
        canvas.drawColor(nBackgroundColor);
        canvas.drawBitmap(bitmap, rect, rect, new Paint());

        OutputStream out = null;

        try {
            bSuccess1 = saveFile.createNewFile();
        } catch (IOException e1) {
            // TODO Auto-generated catch block
            e1.printStackTrace();
        }

        try {
            out = new FileOutputStream(saveFile);
            bSuccess2 = newBitmap.compress(CompressFormat.PNG, 100, out);
        } catch (Exception e) {
            e.printStackTrace();
        }

        try {
            if (out != null) {
                out.flush();
                out.close();
                bSuccess3 = true;
            } else {
                bSuccess3 = false;
            }

        } catch (IOException e) {
            e.printStackTrace();
            bSuccess3 = false;
        } finally {
            if (out != null) {
                try {
                    out.close();
                } catch (IOException e) {
                    // TODO Auto-generated catch block
                    e.printStackTrace();
                }
            }
        }

        return (bSuccess1 && bSuccess2 && bSuccess3);
    }

    // Check whether valid image file or not
    public static boolean isValidImagePath(String strImagePath) {
        if (strImagePath == null) {
            return false;
        }
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        BitmapFactory.decodeFile(strImagePath, options);

        return (options.outMimeType != null);
    }

    // Check whether valid file name or not
    public static boolean isValidSaveName(String fileName) {

        int len = fileName.length();
        for (int i = 0; i < len; i++) {
            char c = fileName.charAt(i);

            if (c == '\\' || c == ':' || c == '/' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>'
                    || c == '|' || c == '\t' || c == '\n') {
                return false;
            }
        }
        return true;
    }

    /****************************************************************************************************************
     * Get the image bitmap that resizing to maximum size of limit.
     * Parameter :
     * - context : Context
     * - uri : Image URI
     * - bContentStreamImage : Gallery contents stream file(true)/file path(false)
     * - nMaxResizedWidth : The maximum allowable width of resizing image.
     * - nMaxResizedHeight : The maximum allowable height of resizing image.
     * Return :
     * - Resizing bitmap
     */
    public static Bitmap getSafeResizingBitmap(String strImagePath, int nMaxResizedWidth, int nMaxResizedHeight) {
        // ==========================================
        // Bitmap Option
        // ==========================================
        BitmapFactory.Options options = getBitmapSize(strImagePath);

        if (options == null) {
            return null;
        }

        // ==========================================
        // Bitmap Scaling
        // ==========================================
        int nSampleSize = getSafeResizingSampleSize(options.outWidth, options.outHeight, nMaxResizedWidth,
                nMaxResizedHeight);

        // ==========================================
        // Load the bitmap including actually data.
        // ==========================================
        options.inJustDecodeBounds = false;
        options.inSampleSize = nSampleSize;
        options.inDither = false;
        options.inPreferredConfig = Bitmap.Config.ARGB_8888;
        options.inPurgeable = true;

        Bitmap photo = BitmapFactory.decodeFile(strImagePath, options);

        return photo;
    }

    /****************************************************************************************************************
     * Get the option for obtain the image size of bitmap.
     * Parameter :
     * - context : Context
     * - uri : Image URI
     * - bContentStreamImage : Gallery contents stream file(true)/File path(false)
     * Return :
     * - BitmapFactory.Options
     */
    public static BitmapFactory.Options getBitmapSize(String strImagePath) {
        // ==========================================
        // Load the temporary bitmap for getting size.
        // ==========================================
        BitmapFactory.Options options = new BitmapFactory.Options();
        options.inJustDecodeBounds = true;
        // Bitmap photo = BitmapFactory.decodeFile(strPath, options);
        BitmapFactory.decodeFile(strImagePath, options);

        return options;
    }

    /****************************************************************************************************************
     * Get the sampling size for load the bitmap. (If you load the bitmap file of big size, This application is
     * terminated.)
     * Parameter :
     * - nOrgWidth : The width of the original image (Value of outWidth of BitmapFactory.Options)
     * - nOrgHeight : The height of the original image (Value of outHeight of BitmapFactory.Options)
     * - nMaxWidth : The width of the image of maximum size. (width under 3M. ex.3000)
     * - nMaxHeight : The height of the image of maximum size. (height under 3M. ex.1000) *
     * Return :
     * - Sampling size (If no need to resize, return 1). Throttled much larger.
     * - If more than x.5 times , divide x+1 times.
     */
    public static int getSafeResizingSampleSize(int nOrgWidth, int nOrgHeight, int nMaxWidth, int nMaxHeight) {
        int size = 1;
        float fsize;
        float fWidthScale = 0;
        float fHeightScale = 0;

        if (nOrgWidth > nMaxWidth || nOrgHeight > nMaxHeight) {
            if (nOrgWidth > nMaxWidth) {
                fWidthScale = (float) nOrgWidth / (float) nMaxWidth;
            }
            if (nOrgHeight > nMaxHeight) {
                fHeightScale = (float) nOrgHeight / (float) nMaxHeight;
            }

            if (fWidthScale >= fHeightScale) {
                fsize = fWidthScale;
            } else {
                fsize = fHeightScale;
            }

            size = (int) fsize;
        }

        return size;
    }

}
