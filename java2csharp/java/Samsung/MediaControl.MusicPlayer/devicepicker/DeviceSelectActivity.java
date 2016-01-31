/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file DeviceSelectActivity.java
 *
 */

package com.samsung.android.sdk.sample.musicplayer.devicepicker;

import android.app.ListActivity;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.AsyncTask;
import android.os.Bundle;
import android.util.LruCache;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.mediacontrol.Smc;
import com.samsung.android.sdk.mediacontrol.SmcDevice;
import com.samsung.android.sdk.mediacontrol.SmcDeviceFinder;
import com.samsung.android.sdk.sample.musicplayer.R;

import java.io.IOException;
import java.io.InputStream;
import java.net.URL;
import java.net.URLConnection;
import java.util.ArrayList;
import java.util.List;

/**
 * Activity enabling user to select a remote AllShare device for playback.
 */
public class DeviceSelectActivity extends ListActivity implements OnClickListener, SmcDeviceFinder.StatusListener, SmcDeviceFinder.DeviceListener {


    /**
     * Current list of AllShare devices
     */
    private List<SmcDevice> mDevices = new ArrayList<SmcDevice>();

    /**
     * Type of AllShare devices that should be presented to user
     */
    private int mType;

    private SmcDeviceFinder mDeviceFinder;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        mType = getIntent().getIntExtra("deviceType", SmcDevice.TYPE_AVPLAYER);

        setContentView(R.layout.device_select);

        TextView noDevicesTextView = (TextView) findViewById(R.id.no_devices);
        noDevicesTextView.setVisibility(View.GONE);

        if (isFrameworkInstalled()) {
            noDevicesTextView.setText(R.string.device_picker_no_devices);
        } else {
            noDevicesTextView.setText(R.string.device_picker_framework_not_installed);
            findViewById(R.id.refresh_button).setVisibility(View.GONE);
        }

        getListView().setEmptyView(noDevicesTextView);

        setListAdapter(new DevicesAdapter());

        findViewById(R.id.cancel_button).setOnClickListener(this);
        findViewById(R.id.refresh_button).setOnClickListener(this);

        SmcDeviceFinder df = new SmcDeviceFinder(this);
        df.setStatusListener(this);
        df.start();
    }


    @Override
    protected void onPause() {
        super.onPause();
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        // Disconnect from AllShare Service

        if(mDeviceFinder != null) {
            mDeviceFinder.stop();
        }
    }


    @Override
    public void onClick(View v) {
        switch (v.getId()) {
            case R.id.cancel_button:
                // Finish activity without returning result.
                finish();
                break;
            case R.id.refresh_button:
                // Rescan network for AllShare devices.
                    if (mDeviceFinder != null)
                        mDeviceFinder.rescan();
                break;
        }
    }

    @Override
    protected void onListItemClick(ListView l, View v, int position, long id) {
        SmcDevice device = mDevices.get(position);
        Intent ret = new Intent();
        ret.putExtra("deviceId", device.getId());
        ret.putExtra("deviceName", device.getName());
        ret.putExtra("deviceType", device.getDeviceType());
        setResult(RESULT_OK, ret);
        finish();
    }


    @Override
    public void onStarted(SmcDeviceFinder deviceFinder, int error) {
        mDeviceFinder = deviceFinder;
        mDeviceFinder.setDeviceListener(mType, this);
        mDeviceFinder.rescan();
        refreshDevicesList();
    }

    @Override
    public void onStopped(SmcDeviceFinder deviceFinder) {
        // Remove listeners so that no references remain
        // and GC can collect the service and this activity.
        if (mDeviceFinder == deviceFinder) {
            mDeviceFinder.setDeviceListener(mType, null);
            mDeviceFinder.setStatusListener(null);
            mDeviceFinder = null;
        }
    }


    ///////////////////////////////////////////////////////////////////////////
    // This methods handle devices appearing and disappearing in network
    ///////////////////////////////////////////////////////////////////////////

    @Override
    public void onDeviceAdded(SmcDeviceFinder deviceFinder, SmcDevice smcDevice) {
        refreshDevicesList();
    }

    @Override
    public void onDeviceRemoved(SmcDeviceFinder deviceFinder, SmcDevice smcDevice, int error) {
        refreshDevicesList();
    }


    /**
     * Refresh the list of displayed devices.
     */
    private void refreshDevicesList() {
        mDevices = mDeviceFinder.getDeviceList(mType);
        ((BaseAdapter) getListAdapter()).notifyDataSetChanged();
    }




    /**
     * Adapter for displaying devices in a list.
     */
    private class DevicesAdapter extends BaseAdapter {
        /**
         * Cache for device icons
         */
        private LruCache<Uri, Bitmap> mIconsCache;

        public DevicesAdapter() {
            // We create a 1MB cache for icons, with item size being the bitmap size in bytes.
            mIconsCache = new LruCache<Uri, Bitmap>(1024 * 1024) {
                @Override
                protected int sizeOf(Uri key, Bitmap value) {
                    return value.getByteCount();
                }
            };
        }

        @Override
        public int getCount() {
            return mDevices.size();
        }

        @Override
        public SmcDevice getItem(int position) {
            return mDevices.get(position);
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            if (convertView == null) {
                convertView = getLayoutInflater().inflate(R.layout.device_item, null);
            }

            TextView label = (TextView) convertView.findViewById(R.id.deviceName);
            ImageView icon = (ImageView) convertView.findViewById(R.id.deviceIcon);

            SmcDevice device = mDevices.get(position);

            label.setText(device.getName());

            Uri iconPath = device.getIconUri();
            icon.setTag(iconPath);
            if (iconPath != null) {
                Bitmap b = mIconsCache.get(iconPath);
                if (b == null) {
                    // Clear the image so we don't display stale icon.
                    icon.setImageResource(R.drawable.ic_launcher);
                    new IconLoader(icon).execute(iconPath);
                } else {
                    icon.setImageBitmap(b);
                }
            } else {
                icon.setImageResource(R.drawable.ic_launcher);
            }

            return convertView;
        }

        private class IconLoader extends AsyncTask<Uri, Void, Bitmap> {
            private final ImageView mImageView;

            IconLoader(ImageView imageView) {
                mImageView = imageView;
            }

            @Override
            protected Bitmap doInBackground(Uri... params) {
                InputStream in = null;
                try {
                    URL url = new URL(params[0].toString());
                    URLConnection conn = url.openConnection();
                    in = conn.getInputStream();
                    Bitmap bitmap = BitmapFactory.decodeStream(in);

                    // Add the bitmap to cache.
                    if (mIconsCache != null) {
                        mIconsCache.put(params[0], bitmap);
                    }
                    //return the bitmap only if target image view is still valid
                    if(params[0].equals(mImageView.getTag()))
                        return bitmap;
                    else
                        return null;
                } catch (IOException e) {
                    // Failed to retrieve icon, ignore it
                    return null;
                }finally{
                    if(in!=null)
                        try { in.close(); } catch (IOException e) {}
                }
            }

            @Override
            protected void onPostExecute(Bitmap result) {
                if (result != null && mIconsCache != null) {
                    mImageView.setImageBitmap(result);
                }
            }
        }
    }

    /**
     * Returns true if AllShare Framework is installed on device.
     */
    private boolean isFrameworkInstalled() {
        try {
            Smc smc = new Smc();
            smc.initialize(this);
            //if no error
            return true;
        } catch (SsdkUnsupportedException e) {
            return false;
        }
    }


}
