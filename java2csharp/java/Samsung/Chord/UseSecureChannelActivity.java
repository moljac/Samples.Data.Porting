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
import com.samsung.android.sdk.chord.example.adapter.NodeListAdapter;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.graphics.Color;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.util.Log;
import android.util.SparseIntArray;
import android.view.GestureDetector;
import android.view.MotionEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.View.OnTouchListener;
import android.view.inputmethod.InputMethodManager;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.ExpandableListView;
import android.widget.LinearLayout;
import android.widget.LinearLayout.LayoutParams;
import android.widget.TextView;
import android.widget.Toast;

import java.util.HashMap;
import java.util.List;

public class UseSecureChannelActivity extends Activity implements OnClickListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.use_secure_channel_activity);

        mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
        mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
        mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

        mDrawableConnected = getResources().getDrawable(R.drawable.ic_network_connected);
        mDrawableDisconnected = getResources().getDrawable(R.drawable.ic_network_disconnected);
        mDrawableConnected.setBounds(0, 0, mDrawableConnected.getIntrinsicWidth(),
                mDrawableConnected.getIntrinsicHeight());
        mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.getIntrinsicWidth(),
                mDrawableDisconnected.getIntrinsicHeight());

        mLogView = (ChordLogView) findViewById(R.id.log_textView);
        mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
        mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

        mWifi_channel_btn = (Button) findViewById(R.id.wifi_channel_btn);
        mWifi_channel_btn.setOnClickListener(this);
        mWifiDirect_channel_btn = (Button) findViewById(R.id.wifiDirect_channel_btn);
        mWifiDirect_channel_btn.setOnClickListener(this);
        mMobileAP_channel_btn = (Button) findViewById(R.id.mobileAP_channel_btn);
        mMobileAP_channel_btn.setOnClickListener(this);

        mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
        mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
        mJoinedNodeList_textView = (TextView) findViewById(R.id.joinedNodeList_textView);
        mJoinedNodeList_textView.setText(getString(R.string.joined_node_list, "Empty"));

        mNodeListAdapter = new NodeListAdapter(getBaseContext(), null);
        mNodeListAdapter.setCheckMode(false);
        mNodeListAdapter.setSecureChannelFrag(true);

        mPeer_listView = (ExpandableListView) findViewById(R.id.secure_peer_list);
        mPeer_listView.setAdapter(mNodeListAdapter);
        mPeer_listView.setGroupIndicator(null);
    }

    @Override
    public void onResume() {
        super.onResume();

        /**
         * [A] Initialize and start Chord!
         */
        if (mSchordManager_1 == null) {
            // mLogView.appendLog("\n[A] Initialize Chord!");
            initChord();
        }

    }

    @Override
    public void onDestroy() {
        /**
         * [F] Release Chord!
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

        boolean bJoined = false;
        int ifc = -1;

        switch (v.getId()) {

            case R.id.wifi_channel_btn:
                bJoined = mWifi_joined;
                ifc = SchordManager.INTERFACE_TYPE_WIFI;
                break;
            case R.id.wifiDirect_channel_btn:
                bJoined = mWifiDirect_joined;
                ifc = SchordManager.INTERFACE_TYPE_WIFI_P2P;
                break;
            case R.id.mobileAP_channel_btn:
                bJoined = mMobileAP_joined;
                ifc = SchordManager.INTERFACE_TYPE_WIFI_AP;
                break;
        }

        if (!bJoined) {
            showChannelInputDialog(ifc);
        } else {
            /**
             * [D] leave test channel
             */
            addLogView(ifc, "\n[C] Leave test channel!");
            leaveTestChannel(ifc);
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

        mSchordManager_1 = new SchordManager(this);
        mSchordManager_1.setLooper(getMainLooper());

        /*****************************************************
         * 2. You have to enable SecureMode to use Secured channel. It is false
         * as default. If you do not set and try to joinChannel with Secured
         * name, it will throw IllegalArgumentException.
         ****************************************************/
        mSchordManager_1.setSecureModeEnabled(true);

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

                switch (interfaceType) {
                    case SchordManager.INTERFACE_TYPE_WIFI:
                        if (mWifi_disconnected)
                            return;
                        break;
                    case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                        if (mWifiDirect_disconnected)
                            return;
                        break;
                    case SchordManager.INTERFACE_TYPE_WIFI_AP:
                        if (mMobileAP_disconnected)
                            return;
                        break;
                }
                startChord(interfaceType);
            }
        });

        List<Integer> ifcList = mSchordManager_1.getAvailableInterfaceTypes();
        for (Integer ifc : ifcList) {
            startChord(ifc);
            refreshInterfaceStatus(ifc, true);
        }

    }

    private void refreshInterfaceStatus(int interfaceType, boolean bConnected) {

        if (!bConnected) {
            if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                mWifi_state_view.setText(R.string.wifi_off);
                mWifi_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                mWifiDirect_state_view.setText(R.string.wifi_direct_off);
                mWifiDirect_state_view
                        .setCompoundDrawables(mDrawableDisconnected, null, null, null);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
                mMobileAP_state_view.setText(R.string.mobile_ap_off);
                mMobileAP_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
            }
        } else {
            if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                mWifi_state_view.setText(R.string.wifi_on);
                mWifi_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                mWifiDirect_state_view.setText(R.string.wifi_direct_on);
                mWifiDirect_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
                mMobileAP_state_view.setText(R.string.mobile_ap_on);
                mMobileAP_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
            }
        }

    }

    private void startChord(int interfaceType) {
        /**
         * 3. Start Chord using the first interface in the list of available
         * interfaces.
         */
        addLogView(interfaceType, "\n[A] Start Chord!");

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
            startManager.setSecureModeEnabled(true);

            switch (interfaceType) {
                case SchordManager.INTERFACE_TYPE_WIFI:
                    startManager.start(interfaceType, mWifi_ManagerListener);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                    startManager.start(interfaceType, mWifiDirect_ManagerListener);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_AP:
                    startManager.start(interfaceType, mMobileAP_ManagerListener);
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

            mWifi_channel_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                // Success to start by calling start() method
                mLogView.appendLog("    > onStarted(STARTED_BY_USER)");
            } else if (reason == STARTED_BY_RECONNECTION) {
                // Re-start by network re-connection.
                mLogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            /**
             * 9. Chord has stopped successfully
             */
            mWifi_bStarted = false;
            mWifi_channel_btn.setEnabled(false);

            if (!mWifiDirect_bStarted) {
                mMyNodeName_textView.setText("");
                mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            }

            mNodeListAdapter.removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mWifi_disconnected = true;
                mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
            } else {
                mWifi_joined = false;
                mWifi_channel_btn.setText(R.string.join_channel);
                mLogView.appendLog("    > onStopped(Error[" + reason + "])");
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

            mWifiDirect_channel_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");

            } else if (reason == STARTED_BY_RECONNECTION) {
                mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            mWifiDirect_bStarted = false;
            mWifiDirect_channel_btn.setEnabled(false);

            if (!mWifi_bStarted) {
                mMyNodeName_textView.setText("");
                mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            }

            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mWifiDirect_disconnected = true;
                mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
            } else {
                mWifiDirect_joined = false;
                mWifiDirect_channel_btn.setText(R.string.join_channel);
                mWifiDirect_LogView.appendLog("    > onStopped(Error[" + reason + "])");
            }
        }
    };

    private SchordManager.StatusListener mMobileAP_ManagerListener = new SchordManager.StatusListener() {

        public void onStarted(String nodeName, int reason) {
            mMobileAP_LogView.setVisibility(View.VISIBLE);
            mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));
            mMobileAP_channel_btn.setEnabled(true);

            if (reason == STARTED_BY_USER) {
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");

            } else if (reason == STARTED_BY_RECONNECTION) {
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            mMyNodeName_textView.setText("");
            mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            mMobileAP_channel_btn.setEnabled(false);

            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));

            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mMobileAP_disconnected = true;
                mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
            } else {
                mMobileAP_joined = false;
                mMobileAP_channel_btn.setText(R.string.join_channel);
                mMobileAP_LogView.appendLog("    > onStopped(Error[" + reason + "])");
            }
        }
    };

    private void joinTestChannel(int interfaceType, String channelName, boolean secureMode) {

        if (channelName == null || channelName.equals("")) {
            if (secureMode)
                channelName = SchordManager.SECURE_PREFIX + CHORD_SECURE_TEST_CHANNEL;
            else
                channelName = CHORD_SECURE_TEST_CHANNEL;
        }

        /**************************************************
         * 6. Join my channel
         **************************************************/
        addLogView(interfaceType, "    joinChannel(" + channelName + ")");

        SchordChannel channel = null;
        SchordManager currentManager = null;

        currentManager = getSchordManager(interfaceType);

        try {
            switch (interfaceType) {
                case SchordManager.INTERFACE_TYPE_WIFI:
                    channel = currentManager.joinChannel(channelName, mWifi_ChannelListener);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                    channel = currentManager.joinChannel(channelName, mWifiDirect_ChannelListener);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_AP:
                    channel = currentManager.joinChannel(channelName, mMobileAP_ChannelListener);
                    break;
            }

        } catch (Exception e) {
            addLogView(interfaceType, "    Fail to join -" + e.getMessage());
            return;
        }

        if (channel == null) {
            addLogView(interfaceType, "    Fail to joinChannel");
        } else {
            if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                mWifi_channelName = channelName;
                mWifi_joined = true;
                mWifi_channel_btn.setText(R.string.leave_channel);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                mWifiDirect_channelName = channelName;
                mWifiDirect_joined = true;
                mWifiDirect_channel_btn.setText(R.string.leave_channel);

            } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
                mMobileAP_channelName = channelName;
                mMobileAP_joined = true;
                mMobileAP_channel_btn.setText(R.string.leave_channel);
            }
        }

    }

    private void leaveTestChannel(int interfaceType) {

        /***************************************************
         * 7. Leave channel
         ***************************************************/
        addLogView(interfaceType, "    leaveChannel()");

        SchordManager currentManager = null;
        currentManager = getSchordManager(interfaceType);

        try {
            switch (interfaceType) {
                case SchordManager.INTERFACE_TYPE_WIFI:
                    currentManager.leaveChannel(mWifi_channelName);
                    mWifi_joined = false;
                    mWifi_channel_btn.setText(R.string.join_channel);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                    currentManager.leaveChannel(mWifiDirect_channelName);
                    mWifiDirect_joined = false;
                    mWifiDirect_channel_btn.setText(R.string.join_channel);
                    break;
                case SchordManager.INTERFACE_TYPE_WIFI_AP:
                    currentManager.leaveChannel(mMobileAP_channelName);
                    mMobileAP_joined = false;
                    mMobileAP_channel_btn.setText(R.string.join_channel);
                    break;
            }
            mNodeListAdapter.removeNodeGroup(getInterfaceName(interfaceType));
            setJoinedNodeCount();

        } catch (Exception e) {
            addLogView(interfaceType, "    Fail to join -" + e.getMessage());
        }
    }

    // ***************************************************
    // ChordChannelListener
    // ***************************************************
    private SchordChannel.StatusListener mWifi_ChannelListener = new SchordChannel.StatusListener() {

        int interfaceType = SchordManager.INTERFACE_TYPE_WIFI;

        /**
         * Called when a node leave event is raised on the channel.
         */
        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        /**
         * Called when a node join event is raised on the channel.
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(getInterfaceName(interfaceType) + fromNode)
                                + ", " + new String(payload[0]) + ")");
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

        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(getInterfaceName(interfaceType) + fromNode)
                                + ", " + new String(payload[0]) + ")");
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

        @Override
        public void onNodeLeft(String fromNode, String fromChannel) {
            onNodeCallbackCommon(false, interfaceType, fromNode);
        }

        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
            if (payloadType.equals(CHORD_SAMPLE_MESSAGE_TYPE)) {
                addLogView(
                        interfaceType,
                        "    > onDataReceived( Node "
                                + mNodeNumberMap.get(getInterfaceName(interfaceType) + fromNode)
                                + ", " + new String(payload[0]) + ")");
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
                mNodeListAdapter.addNode(interfaceName, fromNode,
                        mNodeNumberMap.get(interfaceName + fromNode));
                addLogView(interfaceType,
                        "    > onNodeJoined(Node" + mNodeNumberMap.get(interfaceName + fromNode)
                                + " : " + fromNode + ")");
            } else {
                mNodeNumber++;
                mNodeNumberMap.put(interfaceName + fromNode, mNodeNumber);
                mNodeListAdapter.addNode(interfaceName, fromNode, mNodeNumber);
                addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumber + " : "
                        + fromNode + ")");
            }

            sendData(interfaceType, fromNode);
        } else {
            addLogView(interfaceType,
                    "    > onNodeLeft(Node" + mNodeNumberMap.get(interfaceName + fromNode) + " : "
                            + fromNode + ")");

            mNodeListAdapter.removeNode(interfaceName, fromNode);
        }

        setJoinedNodeCount();
    }

    private void sendData(int interfaceType, String toNode) {
        SchordChannel channel = null;
        byte[][] payload = new byte[1][];

        channel = getJoinedChannelByIfcType(interfaceType);

        if (channel == null) {
            addLogView(interfaceType, "    Fail to get the joined Channel");
            return;
        }

        boolean isSecure = channel.isSecureChannel();
        if (isSecure) {
            payload[0] = "Encrypted data".getBytes();
        } else {
            payload[0] = "Non-encrypted data".getBytes();
        }
        channel.sendData(toNode, CHORD_SAMPLE_MESSAGE_TYPE, payload);
        addLogView(
                interfaceType,
                "    sendData( Node "
                        + mNodeNumberMap.get(getInterfaceName(interfaceType) + toNode) + ", "
                        + new String(payload[0]) + ")");
    }

    private void setJoinedNodeCount() {

        int nodeCnt = mNodeListAdapter.getGroupCount();

        if (nodeCnt == 0) {
            mJoinedNodeList_textView.setText(getString(R.string.joined_node_list, "Empty"));
        } else if (nodeCnt == 1) {
            mJoinedNodeList_textView
                    .setText(getString(R.string.joined_node_list, nodeCnt + " node"));
        } else {
            mJoinedNodeList_textView.setText(getString(R.string.joined_node_list, nodeCnt
                    + " nodes"));
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
        SchordChannel channel = null;
        SchordManager currentManager = null;

        currentManager = getSchordManager(ifcType);

        switch (ifcType) {
            case SchordManager.INTERFACE_TYPE_WIFI:
                channel = currentManager.getJoinedChannel(mWifi_channelName);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                channel = currentManager.getJoinedChannel(mWifiDirect_channelName);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_AP:
                channel = currentManager.getJoinedChannel(mMobileAP_channelName);
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

    private boolean checkChannelName(String channelName, boolean secure) {

        char c;
        if (channelName == null || channelName.isEmpty()) {
            return true;
        }

        if (secure) {
            if (channelName.charAt(0) == '#' && channelName.length() == 1) {
                return false;
            } else {
                c = channelName.charAt(1);
            }
        } else {
            c = channelName.charAt(0);
        }

        if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')) {
            return true;
        } else {
            return false;
        }

    }

    private void showChannelInputDialog(final int interfaceType) {
        // Dialog to input the name of channel to join.

        LinearLayout mlayout = new LinearLayout(this);
        LinearLayout.LayoutParams lp = new LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT);
        mlayout.setLayoutParams(lp);
        mlayout.setOrientation(LinearLayout.VERTICAL);

        final EditText input = new EditText(this);
        LinearLayout.LayoutParams lp2 = new LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT,
                LinearLayout.LayoutParams.WRAP_CONTENT);
        input.setLayoutParams(lp2);
        input.setHint(CHORD_SECURE_TEST_CHANNEL);
        input.setLongClickable(false);
        input.setMaxHeight(500);
        input.setOnTouchListener(new OnTouchListener() {
            private long lastTouchTime = -1;

            @Override
            public boolean onTouch(View arg0, MotionEvent arg1) {

                if (arg1.getAction() == MotionEvent.ACTION_DOWN) {

                    long thisTime = System.currentTimeMillis();
                    if (thisTime - lastTouchTime < 250) {
                        // Double tap
                        mGesture.onTouchEvent(arg1);
                        lastTouchTime = -1;
                        return true;
                    } else {
                        // Too slow
                        lastTouchTime = thisTime;
                    }
                }

                return false;
            }
        });

        // Set checkBox to use secure channel
        final CheckBox mCheck = new CheckBox(this);
        mCheck.setText("Use Secure Channel");
        mCheck.setTextColor(Color.BLACK);

        /*********************************************************************
         * 5. If you want to use secured channel, add prefix to your channel
         * name SECURE_PREFIX : Prefix to secured channel name
         *********************************************************************/

        // Get keyboard inputs to add prefix to secured channel name
        input.addTextChangedListener(new TextWatcher() {

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {

                String str = input.getText().toString();

                if (mCheck.isChecked()) {
                    if (str.length() == 1 && str.indexOf(SchordManager.SECURE_PREFIX) != 0) {
                        str = SchordManager.SECURE_PREFIX + str;
                        input.setText(str);
                        input.setSelection(input.length());
                        if (!checkChannelName(str, mCheck.isChecked())) {
                            Toast.makeText(
                                    getApplicationContext(),
                                    "channelName should always begin with an alphanumeric character",
                                    Toast.LENGTH_SHORT).show();
                        }

                    } else if (str.length() > 1 && str.indexOf(SchordManager.SECURE_PREFIX) != 0) {
                        mCheck.setChecked(false);
                        input.setText(str);
                        input.setSelection(input.length());
                    } else if (str.length() > 1 && !checkChannelName(str, mCheck.isChecked())) {
                        Toast.makeText(getApplicationContext(),
                                "channelName should always begin with an alphanumeric character",
                                Toast.LENGTH_SHORT).show();
                    }
                } else {
                    if (str.indexOf(SchordManager.SECURE_PREFIX) == 0) {
                        mCheck.setChecked(true);
                        input.setText(str);
                        input.setSelection(input.length());
                    } else if (str.length() >= 1 && !checkChannelName(str, mCheck.isChecked())) {
                        Toast.makeText(getApplicationContext(),
                                "channelName should always begin with an alphanumeric character",
                                Toast.LENGTH_SHORT).show();
                    }
                }

            }

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void afterTextChanged(Editable s) {

            }
        });

        // Get Change of CheckBox to add prefix to secured channel name
        mCheck.setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {

            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {

                String str = "";

                if (isChecked) {
                    input.setHint(SchordManager.SECURE_PREFIX + CHORD_SECURE_TEST_CHANNEL);
                    if (!(input.getText().toString().equals("")))
                        str = SchordManager.SECURE_PREFIX + input.getText().toString();
                } else {
                    input.setHint(CHORD_SECURE_TEST_CHANNEL);
                    if (!(input.getText().toString().equals("")))
                        str = input.getText().toString().substring(1);
                }
                input.setText(str);
                input.setSelection(input.length());
            }
        });

        mlayout.addView(input);
        mlayout.addView(mCheck);

        AlertDialog alertDialog = new AlertDialog.Builder(this, AlertDialog.THEME_HOLO_LIGHT)
                .setTitle(
                        getString(R.string.join_channel) + " [" + getInterfaceName(interfaceType)
                                + "]").setMessage(R.string.input_channel_name).setView(mlayout)
                .setPositiveButton(R.string.ok, new DialogInterface.OnClickListener() {

                    @Override
                    public void onClick(DialogInterface arg0, int arg1) {
                        /**
                         * [C] Join test channel
                         */
                        if (checkChannelName(input.getText().toString(), mCheck.isChecked())) {
                            addLogView(interfaceType, "\n[B] Join test channel!");
                            joinTestChannel(interfaceType, input.getText().toString(),
                                    mCheck.isChecked());

                            InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
                            imm.hideSoftInputFromWindow(input.getWindowToken(), 0);
                        } else {
                            Toast.makeText(
                                    getApplicationContext(),
                                    "channelName should always begin with an alphanumeric character",
                                    Toast.LENGTH_SHORT).show();
                        }
                    }
                }).setNegativeButton(R.string.cancel, new DialogInterface.OnClickListener() {

                    @Override
                    public void onClick(DialogInterface arg0, int arg1) {
                        InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
                        imm.hideSoftInputFromWindow(input.getWindowToken(), 0);
                    }
                }).create();
        alertDialog.show();
    }

    private GestureDetector mGesture = new GestureDetector(getApplication(),
            new GestureDetector.SimpleOnGestureListener() {

                @Override
                public boolean onDoubleTap(MotionEvent e) {
                    return true;
                }

            });

    private static final String CHORD_SECURE_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.SECURETESTCHANNEL";

    private static final String CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.MESSAGE_TYPE";

    private SchordManager mSchordManager_1 = null;

    private SchordManager mSchordManager_2 = null;

    private SchordManager mSchordManager_3 = null;

    private TextView mWifi_state_view = null;

    private TextView mWifiDirect_state_view = null;

    private TextView mMobileAP_state_view = null;

    private Drawable mDrawableConnected = null;

    private Drawable mDrawableDisconnected = null;

    private boolean mWifi_bStarted = false;

    private boolean mWifiDirect_bStarted = false;

    private Button mWifi_channel_btn = null;

    private Button mWifiDirect_channel_btn = null;

    private Button mMobileAP_channel_btn = null;

    private boolean mWifi_joined = false;

    private boolean mWifiDirect_joined = false;

    private boolean mMobileAP_joined = false;

    private boolean mWifi_disconnected = false;

    private boolean mWifiDirect_disconnected = false;

    private boolean mMobileAP_disconnected = false;

    private String mWifi_channelName = null;

    private String mWifiDirect_channelName = null;

    private String mMobileAP_channelName = null;

    private TextView mMyNodeName_textView = null;

    private TextView mJoinedNodeList_textView = null;

    private NodeListAdapter mNodeListAdapter = null;

    private ExpandableListView mPeer_listView = null;

    private ChordLogView mLogView = null;

    private ChordLogView mWifiDirect_LogView = null;

    private ChordLogView mMobileAP_LogView = null;

    private HashMap<String, Integer> mNodeNumberMap = new HashMap<String, Integer>();

    private int mNodeNumber = 0;

    private SparseIntArray mInterfaceMap = new SparseIntArray();

    private static final String TAG = "[Chord][Sample]";

    private static final String TAGClass = "UseSecureChannelFragment : ";

}
