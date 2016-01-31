package com.samsung.android.sdk.sample.mediabrowser;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.AsyncTask;
import android.widget.ImageView;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;

/**
 * This class fetches an image from specified {@link Uri} and displays in {@link ImageView}
 */
public class IconLoader extends AsyncTask<Uri, Void, Bitmap> {
    private static final int MAX_ICON_SIZE = 100;
    private final ImageView mImageView;
    private IconCache mIconsCache;

    public IconLoader(ImageView imageView) {
        mImageView = imageView;
        mIconsCache = IconCache.getInstance();
    }

    @Override
    protected Bitmap doInBackground(Uri... params) {
        InputStream in = null;
        try {
            in = openConnection(params[0]);
            BitmapFactory.Options opt = new BitmapFactory.Options();
            opt.inJustDecodeBounds = true;
            opt.inPreferredConfig = Bitmap.Config.RGB_565;
            BitmapFactory.decodeStream(in, null, opt);
            in.close();
            int width = opt.outWidth;
            int height = opt.outHeight;
            int scale = 1;
            while(width>MAX_ICON_SIZE && height>MAX_ICON_SIZE){
                width/=2;
                height/=2;
                scale*=2;
            }
            opt.inJustDecodeBounds = false;
            opt.inSampleSize = scale;
            in = openConnection(params[0]);
            Bitmap bitmap = BitmapFactory.decodeStream(in, null, opt);
            if(bitmap == null)
                return null;

            // Add the bitmap to cache.
            if (mIconsCache != null) {
                mIconsCache.put(params[0], bitmap);
            }
            //return the bitmap only if target image view is still valid
            if (params[0].equals(mImageView.getTag()))
                return bitmap;
            else
                return null;
        } catch (IOException e) {
            // Failed to retrieve icon, ignore it
            return null;
        } finally {
            if (in != null)
                try {
                    in.close();
                } catch (IOException e) {
                }
        }
    }

    private InputStream openConnection(Uri uri) throws IOException {
        URL url = new URL(uri.toString());
        URLConnection conn = url.openConnection();
        return conn.getInputStream();
    }

    @Override
    protected void onPostExecute(Bitmap result) {
        if (result != null && mIconsCache != null) {
            mImageView.setImageBitmap(result);
        }
    }
}