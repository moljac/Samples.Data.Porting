package com.samsung.android.sdk.sample.mediabrowser;


import android.graphics.Bitmap;
import android.net.Uri;
import android.util.LruCache;

public class IconCache extends LruCache<Uri, Bitmap> {

    private static IconCache instance;

    public static IconCache getInstance() {
        if (instance == null) {
            long size = Runtime.getRuntime().maxMemory() / 8;
            instance = new IconCache((int) size);
        }
        return instance;
    }

    protected IconCache(int maxSize) {
        super(maxSize);
    }

    @Override
    protected int sizeOf(Uri key, Bitmap value) {
        return value.getByteCount();
    }
}
