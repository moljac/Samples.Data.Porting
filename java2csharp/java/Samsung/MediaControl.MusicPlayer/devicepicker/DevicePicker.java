/**
 *
 * Sample source code for AllShare Framework SDK
 *
 * Copyright (C) 2013 Samsung Electronics Co., Ltd.
 * All Rights Reserved.
 *
 * @file DevicePicker.java
 */

package com.samsung.android.sdk.sample.musicplayer.devicepicker;

import android.app.Activity;
import android.app.Fragment;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.ImageView;
import com.samsung.android.sdk.mediacontrol.Smc;
import com.samsung.android.sdk.mediacontrol.SmcDevice;
import com.samsung.android.sdk.mediacontrol.SmcDeviceFinder;
import com.samsung.android.sdk.sample.musicplayer.R;

/**
 * AllShare icon fragment.
 * <p/>
 * Fragment that displays AllShare icon with number of available devices.
 * <p/>
 * If AllShare is connected (active), the icon is blue and clicking on it
 * disconnects the AllShare device.
 * <p/>
 * If AllShare is not connected and there are any available devices,
 * then clicking on the icon displays the device selection dialog and conveys
 * the selected device to host activity.
 * <p/>
 * If there are no detected devices, clicking on icon rescans the network.
 *
 * @version 4
 */
public class DevicePicker extends Fragment implements OnClickListener,SmcDeviceFinder.StatusListener, SmcDeviceFinder.DeviceListener {


    /**
     * Callback interface for device selection events.
     */
    public interface DevicePickerResult {

        /**
         * User has selected an AllShare device
         *
         * @param device the selected device
         */
        void onDeviceSelected(SmcDevice device);

        /**
         * User clicked to disable AllShare
         */
        void onAllShareDisabled();
    }

    /**
     * The type of device we are interested in
     */
    private int mType = SmcDevice.TYPE_AVPLAYER;

    /**
     * Listener to be notified of events
     */
    private DevicePickerResult mPickerListener;

    /**
     * Device finder instance
     */
    private SmcDeviceFinder mDeviceFinder;

    /**
     * The ImageView displaying AllShare icon
     */
    private ImageView mIcon;

    /**
     * Flag indicating if AllShare is currently active
     */
    private boolean mActive;

    private String mDeviceId;

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
                             Bundle savedInstanceState) {

        // Set view, remember ImageView for icon and setup onclick listener.
        View v = inflater.inflate(R.layout.device_picker, container, false);
        mIcon = (ImageView) v.findViewById(R.id.devicePickerIcon);
        mIcon.setOnClickListener(this);
        return v;
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        
        /*
        if(savedInstanceState!=null){
            mDeviceId = savedInstanceState.getString("deviceId");
        }
        */
    }

    @Override
    public void onSaveInstanceState(Bundle outState) {
        super.onSaveInstanceState(outState);
   
        /*
        if(mDeviceId!=null)
            outState.putString("deviceId", mDeviceId);
        */
    }

    /**
     *
     * Set the type of device.
     * <p/>
     * This has two effects:
     * <ul>
     * <li>Only devices of this type are counted when displaying number
     * of devices on AllShare icon.
     * <li>Only devices of this type are shown in displayed device select dialog.
     * </ul>
     *
     * @param type The type of device to use
     */
    public void setDeviceType(int type) {
        mType = type;
    }


    /**
     * Sets the listener for event notifications.
     *
     * @param listener the new listener
     */
    public void setDeviceSelectedListener(DevicePickerResult listener) {
        mPickerListener = listener;
        restoreDevice();
    }

    @Override
    public void onActivityCreated(Bundle savedInstanceState) {
        super.onActivityCreated(savedInstanceState);

        // The service provider needs to be created after device type is set
        // It could also be created in onStart or onResume, but we the sooner
        // we create it, the sooner we get devices.
        SmcDeviceFinder df = new SmcDeviceFinder(getActivity());
        df.setStatusListener(this);
        df.start();

    }


    private void restoreDevice(){
        if(mDeviceFinder != null && mDeviceId!=null && mPickerListener != null) {
            SmcDevice d = mDeviceFinder.getDevice(mType, mDeviceId);
            if(d!=null){
                mPickerListener.onDeviceSelected(d);
                setActive(true);
            }
        }
    }

    /**
     * Changes the active state of picker.
     * <p/>
     * Picker should be active if AllShare is actively running,
     * i.e. device is connected and used.
     *
     * @param newState new active state
     */
    public void setActive(boolean newState) {
        if (newState == mActive) {
            // No change in state, do nothing
            return;
        }
        mActive = newState;
        mIcon.setImageResource(
                newState ?
                        R.drawable.allshare_icons_active :
                        R.drawable.allshare_icons_inactive);
        updateButtonCounter();
    }

    @Override
    public void onClick(View v) {
        if (v != mIcon) {
            return;
        }

        if(mDeviceFinder != null) {

            int numDevices = mDeviceFinder.
                    getDeviceList(mType).size();

            // If no devices found, try refreshing the list.
            if (numDevices == 0) {
                mDeviceFinder.rescan();
            }

            // If we are already active, disable allshare
            if (mActive) {
                setActive(false);
                if (mPickerListener != null) {
                    mPickerListener.onAllShareDisabled();
                }
                return;
            }
        }

        // Devices are available, and we are not connected
        // Ask user to select device
        showPickerDialog();
    }

    @Override
    public void onDetach() {
        if (mDeviceFinder != null) {
            mDeviceFinder.stop();
            mDeviceFinder = null;
        }
        super.onDetach();
    }

    ///////////////////////////////////////////////////////////////////////////
    // These methods handle device finder start hide event.
    ///////////////////////////////////////////////////////////////////////////

    @Override
    public void onStarted(SmcDeviceFinder deviceFinder, int error) {
        if (error == Smc.SUCCESS) {
            mDeviceFinder = deviceFinder;
            mDeviceFinder.setDeviceListener(mType, this);
            mDeviceFinder.rescan();
            updateButtonCounter();
            restoreDevice();
        }
    }

    @Override
    public void onStopped(SmcDeviceFinder deviceFinder) {
        if (mDeviceFinder == deviceFinder) {
            mDeviceFinder.setDeviceListener(mType, null);
            mDeviceFinder.setStatusListener(null);
            mDeviceFinder = null;
        }
    }

    ///////////////////////////////////////////////////////////////////////////
    // These methods handle devices appearing and disappearing in network.
    ///////////////////////////////////////////////////////////////////////////

    @Override
    public void onDeviceAdded(SmcDeviceFinder deviceFinder, SmcDevice smcDevice) {
        // We aren't interested in individual devices, only in their number
        updateButtonCounter();
    }

    @Override
    public void onDeviceRemoved(SmcDeviceFinder deviceFinder, SmcDevice smcDevice, int error) {
        // We aren't interested in individual devices, only in their number
        updateButtonCounter();
        //if current device has been removed
        if (smcDevice.getId().equals(mDeviceId)) {
            setActive(false);
            if (mPickerListener != null) {
                mPickerListener.onAllShareDisabled();
            }
        }
    }



    /**
     * Methods that selects which icon to display, based on number of
     * available devices in network.
     */
    private void updateButtonCounter() {
        if (mDeviceFinder != null) {
            int numDevices =
                mDeviceFinder.getDeviceList(mType).size();

            mIcon.getDrawable().setLevel(numDevices);
            if (numDevices==0) {
                setActive(false);
            }
        }
    }

    public void showPickerDialog() {
        Intent intent = new Intent(getActivity(), DeviceSelectActivity.class);
        intent.putExtra("deviceType", mType);
        startActivityForResult(intent, 0);
    }

    /**
     * Callback when user has selected device in device select activity.
     */
    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        if (resultCode == Activity.RESULT_OK) {
            mDeviceId = data.getStringExtra("deviceId");
            int type = data.getIntExtra("deviceType", -1);

            if (mDeviceFinder != null && mPickerListener != null) {
                SmcDevice d = mDeviceFinder.getDevice(type, mDeviceId);
                if(d!=null){
                    mPickerListener.onDeviceSelected(d);
                    setActive(true);
                }
            }
        }
    }
}
