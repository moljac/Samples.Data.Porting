/**
 * Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
 *
 * Mobile Communication Division,
 * Digital Media & Communications Business, Samsung Electronics Co., Ltd.
 *
 * This software and its documentation are confidential and proprietary
 * information of Samsung Electronics Co., Ltd.  No part of the software and
 * documents may be copied, reproduced, transmitted, translated, or reduced to
 * any electronic medium or machine-readable form without the prior written
 * consent of Samsung Electronics.
 *
 * Samsung Electronics makes no representations with respect to the contents,
 * and assumes no responsibility for any errors that might appear in the
 * software and documents. This publication and the contents hereof are subject
 * to change without notice.
 */

package com.samsung.android.sdk.chord.example;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.chord.Schord;
import com.samsung.android.sdk.chord.SchordChannel;
import com.samsung.android.sdk.chord.SchordManager;
import com.samsung.android.sdk.chord.SchordManager.NetworkListener;

import android.app.Activity;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.util.SparseIntArray;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import java.util.HashMap;
import java.util.List;

public class HelloChordActivity extends Activity implements OnClickListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.hello_chord_activity);

        mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
        mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
        mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

        mDrawableConnected = getResources().getDrawable(R.drawable.ic_network_connected);
        mDrawableDisconnected = getResources().getDrawable(R.drawable.ic_network_disconnected);
        mDrawableConnected.setBounds(0, 0, mDrawableConnected.getIntrinsicWidth(),
                mDrawableConnected.getIntrinsicHeight());
        mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.getIntrinsicWidth(),
                mDrawableDisconnected.getIntrinsicHeight());

        mWifi_startStop_btn = (Button) findViewById(R.id.start_stop_btn);
        mWifi_startStop_btn.setOnClickListener(this);
        mWifi_startStop_btn.setEnabled(false);
        mWifiDirect_startStop_btn = (Button) findViewById(R.id.wifiDirect_start_stop_btn);
        mWifiDirect_startStop_btn.setOnClickListener(this);
        mWifiDirect_startStop_btn.setEnabled(false);
        mMobileAP_startStop_btn = (Button) findViewById(R.id.mobileAP_start_stop_btn);
        mMobileAP_startStop_btn.setOnClickListener(this);
        mMobileAP_startStop_btn.setEnabled(false);

        mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
        mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));

        mLogView = (ChordLogView) findViewById(R.id.log_textView);
        mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
        mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);
    }

    @Override
    public void onResume() {
        super.onResume();

        /**
         * [A] Initialize Chord!
         */
        if (mSchordManager_1 == null) {
            // mLogView.appendLog("\n[A] Initialize Chord!");
            initChord();
        }
    }

    @Override
    public void onDestroy() {
        /**
         * [D] Release Chord!
         */
        if (mSchordManager_1 != null) {
            /**
             * If you registered NetworkListener, you should unregister it.
             */
            mSchordManager_1.setNetworkListener(null);

            mSchordManager_1.close();
            mSchordManager_1 = null;
        }

        if (mSchordManager_2 != null) {
            mSchordManager_2.close();
            mSchordManager_2 = null;
        }

        if (mSchordManager_3 != null) {
            mSchordManager_3.close();
            mSchordManager_3 = null;
        }

        mNodeNumberMap.clear();
        mInterfaceMap.clear();

        super.onDestroy();
    }

    @Override
    public void onClick(View v) {

        boolean bStarted = false;
        int ifc = -1;

        switch (v.getId()) {
            case R.id.start_stop_btn:
                bStarted = mWifi_bStarted;
                ifc = SchordManager.INTERFACE_TYPE_WIFI;
                break;
            case R.id.wifiDirect_start_stop_btn:
                bStarted = mWifiDirect_bStarted;
                ifc = SchordManager.INTERFACE_TYPE_WIFI_P2P;
                break;
            case R.id.mobileAP_start_stop_btn:
                bStarted = mMobileAP_bStarted;
                ifc = SchordManager.INTERFACE_TYPE_WIFI_AP;
                break;
        }

        if (!bStarted) {
            /**
             * [B] Start Chord
             */
            addLogView(ifc, "\n[A] Start Chord!");
            startChord(ifc);
        } else {
            /**
             * [C] Stop Chord
             */
            addLogView(ifc, "\n[B] Stop Chord!");
            stopChord(ifc);
        }

    }

    private void initChord() {

        /****************************************************
         * 1. GetInstance
         ****************************************************/
        Schord chord = new Schord();
        try {
            chord.initialize(this);
        } catch (SsdkUnsupportedException e) {
            if (e.getType() == SsdkUnsupportedException.VENDOR_NOT_SUPPORTED) {
                // Vender is not SAMSUNG
                return;
            }
        }
        
        Log.d(TAG, TAGClass + "initChord : VersionName( " + chord.getVersionName() + " ), VerionCode( " + chord.getVersionCode()+" )");
        
        mSchordManager_1 = new SchordManager(this);

        /****************************************************
         * 2. Set some values before start If you want to use secured channel,
         * you should enable SecureMode. Please refer
         * UseSecureChannelFragment.java mChordManager.enableSecureMode(true);
         * Once you will use sendFile or sendMultiFiles, you have to call
         * setTempDirectory mChordManager.setTempDirectory(Environment.
         * getExternalStorageDirectory().getAbsolutePath() + "/Chord");
         ****************************************************/
        mSchordManager_1.setLooper(getMainLooper());

        /**
         * Optional. If you need listening network changed, you can set callback
         * before starting chord.
         */
        mSchordManager_1.setNetworkListener(new NetworkListener() {

            @Override
            public void onDisconnected(int interfaceType) {
                Toast.makeText(getApplicationContext(),
                        getInterfaceName(interfaceType) + " is disconnected", Toast.LENGTH_SHORT)
                        .show();
                refreshInterfaceStatus(interfaceType, false);
            }

            @Override
            public void onConnected(int interfaceType) {
                Toast.makeText(getApplicationContext(),
                        getInterfaceName(interfaceType) + " is connected", Toast.LENGTH_SHORT)
                        .show();
                refreshInterfaceStatus(interfaceType, true);
            }
        });

        List<Integer> ifcList = mSchordManager_1.getAvailableInterfaceTypes();
        for (Integer ifc : ifcList) {
            refreshInterfaceStatus(ifc, true);
        }

    }

    private void refreshInterfaceStatus(int interfaceType, boolean bConnected) {

        if (!bConnected) {
            if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                mWifi_state_view.setText(R.string.wifi_off);
                mWifi_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
                mWifi_startStop_btn.setEnabled(false);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                mWifiDirect_state_view.setText(R.string.wifi_direct_off);
                mWifiDirect_state_view
                        .setCompoundDrawables(mDrawableDisconnected, null, null, null);
                mWifiDirect_startStop_btn.setEnabled(false);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
                mMobileAP_state_view.setText(R.string.mobile_ap_off);
                mMobileAP_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
                mMobileAP_startStop_btn.setEnabled(false);

            }
        } else {
            if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                mWifi_state_view.setText(R.string.wifi_on);
                mWifi_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
                mWifi_startStop_btn.setEnabled(true);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                mWifiDirect_state_view.setText(R.string.wifi_direct_on);
                mWifiDirect_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
                mWifiDirect_startStop_btn.setEnabled(true);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
                mMobileAP_state_view.setText(R.string.mobile_ap_on);
                mMobileAP_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
                mMobileAP_startStop_btn.setEnabled(true);
            }
        }

    }

    private void startChord(int interfaceType) {
        /**
         * 3. Start Chord using the selected interface in the list of available
         * interfaces. You can get a list of available network interface types
         * List<Integer> infList =
         * mSchordManager_1.getAvailableInterfaceTypes().isEmpty();
         * if(infList.isEmpty()) // there is no active interface!
         */
        int managerIndex = 0;
        SchordManager startManager = null;

        if (mInterfaceMap.get(interfaceType) == 0) {
            managerIndex = mInterfaceMap.size() + 1;
            mInterfaceMap.put(interfaceType, managerIndex);
        } else {
            managerIndex = mInterfaceMap.get(interfaceType);
        }

        switch (managerIndex) {
            case 1:
                startManager = mSchordManager_1;
                break;
            case 2:
                mSchordManager_2 = new SchordManager(this);
                startManager = mSchordManager_2;
                break;
            case 3:
                mSchordManager_3 = new SchordManager(this);
                startManager = mSchordManager_3;
                break;
        }

        try {
            Log.d(TAG, TAGClass + "start(" + getInterfaceName(interfaceType)
                    + ") with the SchordManager number : " + managerIndex);

            startManager.setLooper(getMainLooper());

            switch (interfaceType) {
                case SchordManager.INTERFACE_TYPE_WIFI:
                    startManager.start(interfaceType, mWifi_ManagerListener);
                    mWifi_startStop_btn.setEnabled(false);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                    startManager.start(interfaceType, mWifiDirect_ManagerListener);
                    mWifiDirect_startStop_btn.setEnabled(false);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_AP:
                    startManager.start(interfaceType, mMobileAP_ManagerListener);
                    mMobileAP_startStop_btn.setEnabled(false);
                    break;
            }
            addLogView(interfaceType, "    start(" + getInterfaceName(interfaceType) + ")");

        } catch (Exception e) {
            addLogView(interfaceType, "    Fail to start -" + e.getMessage());
            mInterfaceMap.delete(interfaceType);
        }
    }

    // ***************************************************
    // ChordManagerListener
    // ***************************************************
    private SchordManager.StatusListener mWifi_ManagerListener = new SchordManager.StatusListener() {

        @Override
        public void onStarted(String nodeName, int reason) {
            /**
             * 4. Chord has started successfully
             */
            mWifi_bStarted = true;
            mLogView.setVisibility(View.VISIBLE);

            if (!mWifiDirect_bStarted) {
                mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));
            }

            mWifi_startStop_btn.setText(R.string.stop);
            mWifi_startStop_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                // Success to start by calling start() method
                mLogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI);
            } else if (reason == STARTED_BY_RECONNECTION) {
                // Re-start by network re-connection.
                mLogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            /**
             * 8. Chord has stopped successfully
             */
            mWifi_bStarted = false;

            if (!mWifiDirect_bStarted) {
                mMyNodeName_textView.setText("");
                mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            }

            mWifi_startStop_btn.setText(R.string.start);

            if (STOPPED_BY_USER == reason) {
                // Success to stop by calling stop() method
                mLogView.appendLog("    > onStopped(STOPPED_BY_USER)");
                mWifi_startStop_btn.setEnabled(true);

            } else if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mWifi_startStop_btn.setEnabled(false);
            }
        }
    };

    private SchordManager.StatusListener mWifiDirect_ManagerListener = new SchordManager.StatusListener() {

        @Override
        public void onStarted(String nodeName, int reason) {
            mWifiDirect_bStarted = true;
            mWifiDirect_LogView.setVisibility(View.VISIBLE);

            if (!mWifi_bStarted) {
                mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));
            }

            mWifiDirect_startStop_btn.setText(R.string.stop);
            mWifiDirect_startStop_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                // Success to start by calling start() method
                mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_P2P);
            } else if (reason == STARTED_BY_RECONNECTION) {
                // Re-start by network re-connection.
                mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            mWifiDirect_bStarted = false;

            if (!mWifi_bStarted) {
                mMyNodeName_textView.setText("");
                mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            }

            mWifiDirect_startStop_btn.setText(R.string.start);

            if (STOPPED_BY_USER == reason) {
                // Success to stop by calling stop() method
                mWifiDirect_LogView.appendLog("    > onStopped(STOPPED_BY_USER)");
                mWifiDirect_startStop_btn.setEnabled(true);

            } else if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mWifiDirect_startStop_btn.setEnabled(false);
            }
        }
    };

    private SchordManager.StatusListener mMobileAP_ManagerListener = new SchordManager.StatusListener() {

        @Override
        public void onStarted(String nodeName, int reason) {
            mMobileAP_bStarted = true;

            mMobileAP_LogView.setVisibility(View.VISIBLE);
            mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));

            mMobileAP_startStop_btn.setText(R.string.stop);
            mMobileAP_startStop_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                // Success to start by calling start() method
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_AP);
            } else if (reason == STARTED_BY_RECONNECTION) {
                // Re-start by network re-connection.
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            mMobileAP_bStarted = false;

            mMyNodeName_textView.setText("");
            mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));

            mMobileAP_startStop_btn.setText(R.string.start);

            if (STOPPED_BY_USER == reason) {
                // Success to stop by calling stop() method
                mMobileAP_LogView.appendLog("    > onStopped(STOPPED_BY_USER)");
                mMobileAP_startStop_btn.setEnabled(true);

            } else if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mMobileAP_startStop_btn.setEnabled(false);
            }
        }
    };

    private void joinTestChannel(int interfaceType) {
        /**
         * 5. Join my channel
         */
        addLogView(interfaceType, "    joinChannel()");

        SchordChannel channel = null;
        SchordManager currentManager = null;

        currentManager = getSchordManager(interfaceType);

        switch (interfaceType) {
            case SchordManager.INTERFACE_TYPE_WIFI:
                channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL,
                        mWifi_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL,
                        mWifiDirect_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_AP:
                channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL,
                        mMobileAP_ChannelListener);
                break;
        }

        if (channel == null) {
            addLogView(interfaceType, "    Fail to joinChannel");
        }

    }

    private void stopChord(int ifc) {
        /**
         * 7. Stop Chord. You can call leaveChannel explicitly.
         * mSchordManager_1.leaveChannel(CHORD_HELLO_TEST_CHANNEL);
         */
        SchordManager currentManager = null;

        currentManager = getSchordManager(ifc);

        if (currentManager == null)
            return;

        currentManager.stop();

        switch (ifc) {
            case SchordManager.INTERFACE_TYPE_WIFI:
                mWifi_startStop_btn.setEnabled(false);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                mWifiDirect_startStop_btn.setEnabled(false);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_AP:
                mMobileAP_startStop_btn.setEnabled(false);
                break;
        }
        addLogView(ifc, "    stop(" + getInterfaceName(ifc) + ")");

    }

    // ***************************************************
    // ChordChannelListener
    // ***************************************************
    private SchordChannel.StatusListener mWifi_ChannelListener = new SchordChannel.StatusListener() {

        int interfaceType = SchordManager.INTERFACE_TYPE_WIFI;
        String interfaceName = getInterfaceName(interfaceType);

        /**
         * Called when a node leave event is raised on the channel.
         */
        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        /**
         * Called when a node join event is raised on the channel
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);

        }

        /**
         * Called when the data message received from the node.
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            /**
             * 6. Received data from other node
             */            
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(interfaceName + fromNode) + ", "
                                + new String(payload[0]) + ")");
            }
        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

        }

        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

        }

        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

        }

        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

        }

        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

        }

        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

        }

        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

        }

        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

        }

        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

        }

        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

        }

        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the UdpFrameworkFragment.java
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String serviceType) {

        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

        }

    };

    private SchordChannel.StatusListener mWifiDirect_ChannelListener = new SchordChannel.StatusListener() {

        int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_P2P;
        String interfaceName = getInterfaceName(interfaceType);

        /**
         * Called when a node leave event is raised on the channel.
         */
        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        /**
         * Called when a node join event is raised on the channel
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);

        }

        /**
         * Called when the data message received from the node.
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            /**
             * 6. Received data from other node
             */
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(interfaceName + fromNode) + ", "
                                + new String(payload[0]) + ")");
            }
        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

        }

        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

        }

        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

        }

        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

        }

        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

        }

        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

        }

        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

        }

        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

        }

        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

        }

        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

        }

        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the UdpFrameworkFragment.java
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String serviceType) {

        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

        }

    };

    private SchordChannel.StatusListener mMobileAP_ChannelListener = new SchordChannel.StatusListener() {

        int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_AP;
        String interfaceName = getInterfaceName(interfaceType);

        /**
         * Called when a node leave event is raised on the channel.
         */
        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        /**
         * Called when a node join event is raised on the channel
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);

        }

        /**
         * Called when the data message received from the node.
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            /**
             * 6. Received data from other node
             */           
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(interfaceName + fromNode) + ", "
                                + new String(payload[0]) + ")");
            }
        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

        }

        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

        }

        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

        }

        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

        }

        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

        }

        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

        }

        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

        }

        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

        }

        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

        }

        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

        }

        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

        }

        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

        }

        /**
         * The following callBacks are not used in this Fragment. Please refer
         * to the UdpFrameworkFragment.java
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String serviceType) {

        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

        }

    };

    private void onNodeCallbackCommon(boolean isJoin, int interfaceType, String fromNode) {

        String interfaceName = getInterfaceName(interfaceType);

        if (isJoin) {
            if (mNodeNumberMap.containsKey(interfaceName + fromNode)) {
                addLogView(interfaceType,
                        "    > onNodeJoined(Node" + mNodeNumberMap.get(interfaceName + fromNode)
                                + " : " + fromNode + ")");
            } else {
                mNodeNumber++;
                mNodeNumberMap.put(interfaceName + fromNode, mNodeNumber);
                addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumber + " : "
                        + fromNode + ")");
            }

            /**
             * 6. Send data to joined node
             */
            byte[][] payload = new byte[1][];
            payload[0] = "Hello chord!".getBytes();

            SchordChannel channel = getJoinedChannelByIfcType(interfaceType);

            if (channel == null) {
                addLogView(interfaceType, "    Fail to get the joined Channel");
                return;
            }

            try {
                channel.sendData(fromNode, CHORD_SAMPLE_MESSAGE_TYPE, payload);
            } catch (Exception e) {
                addLogView(interfaceType, "    " + e.getMessage());
                return;
            }

            addLogView(interfaceType,
                    "    sendData( Node " + mNodeNumberMap.get(interfaceName + fromNode) + ", "
                            + new String(payload[0]) + ")");

        } else {
            addLogView(interfaceType,
                    "    > onNodeLeft(Node" + mNodeNumberMap.get(interfaceName + fromNode) + " : "
                            + fromNode + ")");

        }
    }

    private String getInterfaceName(int interfaceType) {
        if (SchordManager.INTERFACE_TYPE_WIFI == interfaceType)
            return "Wi-Fi";
        else if (SchordManager.INTERFACE_TYPE_WIFI_AP == interfaceType)
            return "Mobile AP";
        else if (SchordManager.INTERFACE_TYPE_WIFI_P2P == interfaceType)
            return "Wi-Fi Direct";

        return "UNKNOWN";
    }

    private SchordManager getSchordManager(int interfaceType) {
        int managerIndex = 0;
        SchordManager manager = null;

        managerIndex = mInterfaceMap.get(interfaceType);

        switch (managerIndex) {
            case 1:
                manager = mSchordManager_1;
                break;
            case 2:
                manager = mSchordManager_2;
                break;
            case 3:
                manager = mSchordManager_3;
                break;
        }
        return manager;
    }

    private SchordChannel getJoinedChannelByIfcType(int ifcType) {
        int managerIndex = 0;
        SchordChannel channel = null;

        managerIndex = mInterfaceMap.get(ifcType);

        switch (managerIndex) {
            case 1:
                channel = mSchordManager_1.getJoinedChannel(CHORD_HELLO_TEST_CHANNEL);
                break;
            case 2:
                channel = mSchordManager_2.getJoinedChannel(CHORD_HELLO_TEST_CHANNEL);
                break;
            case 3:
                channel = mSchordManager_3.getJoinedChannel(CHORD_HELLO_TEST_CHANNEL);
                break;

        }

        return channel;
    }

    private void addLogView(int interfaceType, String str) {
        if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
            mLogView.appendLog(str);
        } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
            mWifiDirect_LogView.appendLog(str);
        } else {
            mMobileAP_LogView.appendLog(str);
        }
    }

    private static final String CHORD_HELLO_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.HELLOTESTCHANNEL";

    private static final String CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.MESSAGE_TYPE";

    private SchordManager mSchordManager_1 = null;

    private SchordManager mSchordManager_2 = null;

    private SchordManager mSchordManager_3 = null;

    private TextView mWifi_state_view = null;

    private TextView mWifiDirect_state_view = null;

    private TextView mMobileAP_state_view = null;

    private Drawable mDrawableConnected = null;

    private Drawable mDrawableDisconnected = null;

    private Button mWifi_startStop_btn = null;

    private Button mWifiDirect_startStop_btn = null;

    private Button mMobileAP_startStop_btn = null;

    private boolean mWifi_bStarted = false;

    private boolean mWifiDirect_bStarted = false;

    private boolean mMobileAP_bStarted = false;

    private TextView mMyNodeName_textView = null;

    private ChordLogView mLogView = null;

    private ChordLogView mWifiDirect_LogView = null;

    private ChordLogView mMobileAP_LogView = null;

    private HashMap<String, Integer> mNodeNumberMap = new HashMap<String, Integer>();

    private int mNodeNumber = 0;

    private SparseIntArray mInterfaceMap = new SparseIntArray();

    private static final String TAG = "[Chord][Sample]";

    private static final String TAGClass = "HelloChordFragment : ";

}
