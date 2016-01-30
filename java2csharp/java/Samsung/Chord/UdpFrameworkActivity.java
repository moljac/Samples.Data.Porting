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
import com.samsung.android.sdk.chord.example.adapter.Item;
import com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.IFileCancelListener;
import com.samsung.android.sdk.chord.example.adapter.NodeListGridAdapter;

import android.app.Activity;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.Log;
import android.util.SparseIntArray;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ExpandableListView;
import android.widget.RadioGroup;
import android.widget.TextView;
import android.widget.Toast;

import java.util.Arrays;
import java.util.HashMap;
import java.util.List;

public class UdpFrameworkActivity extends Activity implements IFileCancelListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.udp_framework_activity);

        mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
        mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
        mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

        mDrawableConnected = getResources().getDrawable(R.drawable.ic_network_connected);
        mDrawableDisconnected = getResources().getDrawable(R.drawable.ic_network_disconnected);
        mDrawableConnected.setBounds(0, 0, mDrawableConnected.getIntrinsicWidth(),
                mDrawableConnected.getIntrinsicHeight());
        mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.getIntrinsicWidth(),
                mDrawableDisconnected.getIntrinsicHeight());

        mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
        mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
        mJoinedNodeList_textView = (TextView) findViewById(R.id.joinedNodeList_textView);
        mJoinedNodeList_textView.setText(getString(R.string.joined_node_list, "Empty"));

        // Set an adapter for the list of nodes and progressBars.
        mNode_listView = (ExpandableListView) findViewById(R.id.node_listView);
        mNodeListAdapter = new NodeListGridAdapter(getBaseContext(), response_needed);
        mNode_listView.setAdapter(mNodeListAdapter);
        mNodeListAdapter.setListener(this);

        mLogView = (ChordLogView) findViewById(R.id.log_textView);
        mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
        mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

        /* for UDP */
        greenIcon = BitmapFactory.decodeResource(getResources(), R.drawable.ic_udp_data_bg_green);
        greyIcon = BitmapFactory.decodeResource(getResources(), R.drawable.ic_udp_data_bg_gray);
        redIcon = BitmapFactory.decodeResource(getResources(), R.drawable.ic_udp_data_bg_red);

        messageSizeEditText = (EditText) findViewById(R.id.messageSize_editText);
        messageSizeEditText.setText(MSG_SIZE_BYTES);

        udpSendButton = (Button) findViewById(R.id.udp_send_btn);
        udpSendButton.setOnClickListener(new OnClickListener() {

            @Override
            public void onClick(View v) {
                sendUdpData();
            }
        });

        radioUdpMode = (RadioGroup) findViewById(R.id.radio_udp_mode_btn);
        radioUdpMode.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() {
            @Override
            public void onCheckedChanged(RadioGroup group, int checkedId) {
                if (checkedId == R.id.udp_reliable_btn) {
                    send_mode = MODE_RELIABLE;
                } else if (checkedId == R.id.udp_semi_reliable_btn) {
                    send_mode = MODE_SEMIRELIABLE;
                } else if (checkedId == R.id.udp_unreliable_btn) {
                    send_mode = MODE_UNRELIABLE;
                }
            }
        });
    }

    @Override
    public void onResume() {
        super.onResume();

        /**
         * [A] Initialize Chord!
         */
        if (mSchordManager_1 == null) {
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
        mNodeListAdapter.StopAllTimer();

        super.onDestroy();
    }

    @Override
    public void onFileCanceled(String chennel, String node, String trId, int index, boolean bMulti) {

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

        /****************************************************
         * 2. Set some values before start If you want to use secured channel,
         * you should enable SecureMode. Please refer
         * UseSecureChannelFragment.java
         * mSchordManager_1.enableSecureMode(true); Once you will use sendFile
         * or sendMultiFiles, you have to call setTempDirectory
         * mSchordManager_1.setTempDirectory(Environment.
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
        /**
         * 4. Chord has started successfully
         */
        @Override
        public void onStarted(String nodeName, int reason) {
            mWifi_bStarted = true;
            mLogView.setVisibility(View.VISIBLE);

            if (!mWifiDirect_bStarted) {
                mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));
            }

            if (reason == STARTED_BY_USER) {
                // Success to start by calling start() method
                mLogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI);
            } else if (reason == STARTED_BY_RECONNECTION) {
                // Re-start by network re-connection.
                mLogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }
        }

        /**
         * 9. Chord has stopped successfully
         */
        @Override
        public void onStopped(int reason) {

            mWifi_bStarted = false;
            if (!mWifiDirect_bStarted) {
                mMyNodeName_textView.setText("");
                mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));
            }

            // mNodeListAdapter
            // .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
            mNodeListAdapter
                    .removeReceiverDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
            mNodeListAdapter.removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mWifi_disconnected = true;
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

            if (reason == STARTED_BY_USER) {
                mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_P2P);
            } else if (reason == STARTED_BY_RECONNECTION) {
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

            // mNodeListAdapter
            // .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
            mNodeListAdapter
                    .removeReceiverDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mWifiDirect_disconnected = true;
            }
        }
    };

    private SchordManager.StatusListener mMobileAP_ManagerListener = new SchordManager.StatusListener() {

        public void onStarted(String nodeName, int reason) {
            mMobileAP_LogView.setVisibility(View.VISIBLE);
            mMyNodeName_textView.setText(getString(R.string.my_node_name, nodeName));

            if (reason == STARTED_BY_USER) {
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
                joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_AP);
            } else if (reason == STARTED_BY_RECONNECTION) {
                mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
            }

        }

        @Override
        public void onStopped(int reason) {
            mMyNodeName_textView.setText("");
            mMyNodeName_textView.setHint(getString(R.string.my_node_name, " "));

            // mNodeListAdapter
            // .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
            mNodeListAdapter
                    .removeReceiverDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
                mMobileAP_disconnected = true;
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
                channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL, mWifi_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL,
                        mWifiDirect_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_AP:
                channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL,
                        mMobileAP_ChannelListener);
                break;
        }

        if (channel == null) {
            addLogView(interfaceType, "    Fail to joinChannel");
        }
    }

    private void sendUdpData() {
        /**
         * 6. Send data to the selected node using UDP
         */
        char[] chars = null;
        String AppendMsg = null;

      try
        {

	    udpSendButton.setEnabled(false);
        if (mNodeListAdapter.getCheckedNodeList().size() == 0) {
            Toast.makeText(getApplicationContext(), "Select at least one node", Toast.LENGTH_SHORT)
                    .show();
            return;
        }

        // check the size value
        int message_size = checkMessageSize();
        if (message_size == -1)
            return;

        // check the reliable mode
        int reliableTime = send_mode;
        String sendMode = null;
        if (reliableTime == -1) {
            sendMode = "Reliable";
        } else if (reliableTime == 10) {
            sendMode = "Semi-Reliable";
        } else if (reliableTime == 0) {
            sendMode = "Unreliable";
        } else {
            sendMode = "UNKNOWN";
        }

        // make the message to send
 
	        if (message_size > 65522) {
	            // Because of .split() limit of 65535 bytes, new char[65535-13].
	            // 13 is for length of Message::19::
	            chars = new char[65522];
	        } else {
	            chars = new char[message_size];
	        }
	
	        Arrays.fill(chars, '*');
	
	        AppendMsg = new String(chars);

        String reqId = null;
        String checkedNodeInfo[][] = new String[1][];
        String toNode = null;
        String interfaceName = null;
        int interfaceType = -1;
        SchordChannel channel = null;
        byte[][] payload = new byte[1][];

        for (int j = 0; j < MSG_COUNT; j++) {

            for (int i = 0; i < mNodeListAdapter.getCheckedNodeList().size(); i++) {

                checkedNodeInfo = mNodeListAdapter.getCheckedNodeList().get(i);
                toNode = checkedNodeInfo[i][1];
                interfaceName = checkedNodeInfo[i][0];
                interfaceType = getInterfaceType(interfaceName);

                channel = getJoinedChannelByIfcType(interfaceType);

                if (channel == null) {
                    addLogView(interfaceType, "    Fail to get the joined Channel");
                    return;
                }

                String Message = "Message::" + j + "::" + AppendMsg;
                payload[0] = Message.getBytes();

                /* Initialization of Data Structures for 1st Message Sent */
                if (j == 0) {
                    addLogView(interfaceType, "    sendUdpData(" + sendMode + ") : to Node"
                            + mNodeNumberMap.get(interfaceName + toNode) + ", total 20msg");

                    int stream_id = mNodeListAdapter.getSendStreamId(interfaceName, toNode);

                    mNodeListAdapter.setPayloadtype(interfaceName, toNode,
                            CHORD_SAMPLE_MESSAGE_TYPE + ":" + stream_id + ":" + reliableTime);
                    mNodeListAdapter.setSendStreamId(interfaceName, toNode, stream_id + 1);
                    // mNodeListAdapter.setTotalMsgSent(interfaceName, toNode,
                    // 0);
                    // mNodeListAdapter.clearSenderMap(interfaceName, toNode);
                }

                try {
                    reqId = channel.sendUdpData(toNode, reliableTime, response_needed,
                            mNodeListAdapter.getPayloadtype(interfaceName, toNode), payload,
                            CHORD_SESSION_NAME_TEST);
                } catch (Exception ex) {
                    addLogView(interfaceType, "    sendUdpData(" + sendMode + ") : to Node"
                            + mNodeNumberMap.get(interfaceName + toNode) + ", Exception");
                }
            }
        }

    }
    
    finally
    {
    	chars = null;
    	AppendMsg = null;
    	udpSendButton.setEnabled(true);
    }

    }

    private void initializeReceiverGrid(String interfaceName, String nodeName) {
        for (int i = 0; i < MSG_COUNT; i++) {
            mNodeListAdapter.addToReceiverGrid(interfaceName, nodeName,
                    new Item(greyIcon, String.valueOf(i % 10)));
        }

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
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {

        }

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
         * Called when the udp data message received from the node.
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String sessionName) {

            onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload,
                    sessionName);
        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

            // onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
            // reqId);
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
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {

        }

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
         * Called when the udp data message received from the node.
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String sessionName) {

            onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload,
                    sessionName);
        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

            // onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
            // reqId);
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
         * The following callBacks are not used in this Fragment. Please refer
         * to the SendFilesFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {

        }

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
         * Called when the udp data message received from the node.
         */
        @Override
        public void onUdpDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload, String sessionName) {

            onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload,
                    sessionName);
        }

        @Override
        public void onUdpDataDelivered(String fromNode, String channelName, String reqId) {

            // onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
            // reqId);
        }

    };

    private void onNodeCallbackCommon(boolean isJoin, int interfaceType, String fromNode) {

        String interfaceName = getInterfaceName(interfaceType);

        if (isJoin) {
            if (fromNode.isEmpty() || fromNode == null) {
                return;
            }
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
        } else {
            addLogView(interfaceType,
                    "    > onNodeLeft(Node" + mNodeNumberMap.get(interfaceName + fromNode) + " : "
                            + fromNode + ")");

            mNodeListAdapter.removeNode(interfaceName, fromNode);
        }

        setJoinedNodeCount();
    }

    /* For Sender, Not Used */
    // private void onUdpDataDeliveredCommon(int interfaceType, String
    // interfaceName, String fromNode,
    // String reqId) {
    //
    // HashMap<String, Integer> Sendermap = null;
    // Sendermap = mNodeListAdapter.getSenderMap(interfaceName, fromNode);
    //
    // if (response_needed == true) {
    // if (Sendermap.size() > 0) {
    // if (Sendermap.containsKey(reqId)) {
    //
    // int MsgNum = Sendermap.get(reqId);
    // Sendermap.remove(reqId);
    //
    // if (MsgNum < mNodeListAdapter.getSenderGridSize(interfaceName, fromNode))
    // {
    // mNodeListAdapter.IncrementTotalMsgSent(interfaceName, fromNode);
    // mNodeListAdapter.updateSenderGrid(interfaceName, fromNode, MsgNum,
    // new Item(greenIcon, String.valueOf(MsgNum % 10)));
    //
    // if (mNodeListAdapter.getTotalMsgSent(interfaceName, fromNode) ==
    // mNodeListAdapter
    // .getSenderGridSize(interfaceName, fromNode)) {
    //
    // mNodeListAdapter.setSenderEndTime(interfaceName, fromNode,
    // System.currentTimeMillis());
    //
    // long totalTime = mNodeListAdapter.getSenderEndTime(interfaceName,
    // fromNode)
    // - mNodeListAdapter.getSenderStartTime(interfaceName, fromNode);
    //
    // int loss = (Sendermap.size() * 100)
    // / mNodeListAdapter.getSenderGridSize(interfaceName, fromNode);
    //
    // addLogView(interfaceType, "    Sending time " + totalTime + "ms, Loss "
    // + loss + "%");
    // }
    // } else {
    // addLogView(interfaceType, " \n Wrong Msg Number: " + MsgNum);
    // }
    // } else {
    // addLogView(interfaceType, "\n onUdpDataDelivered: ReqId Not in Map ");
    // }
    // } else {
    // addLogView(interfaceType, "\n onUdpDataDelivered: ReqId Not in Map ");
    // }
    //
    // }
    //
    // }

    private void onUdpDataReceivedCommon(int interfaceType, String interfaceName, String fromNode,
            String payloadType, byte[][] payload, String sessionName) {

        String Node = null;

        if (sessionName.equals(CHORD_SESSION_NAME_TEST)) {
            // Grid is maintained for CHORD_SESSION_NAME_TEST

            String payload_info[] = new String(payload[0]).split("::");
            String payload_type[] = payloadType.split(":");

            if (payload_type[0].equals(CHORD_SAMPLE_MESSAGE_TYPE)) {

                int MsgNumber = 0;
                int STREAM_ID = Integer.parseInt(payload_type[1]);
                int RELIABLE_MODE = Integer.parseInt(payload_type[2]);

                if (payload_info[0].equals("Message")) {

                    MsgNumber = Integer.parseInt(payload_info[1]);

                    for (int i = 0; i < mNodeListAdapter.getNodeList().size(); i++) {
                        Node = mNodeListAdapter.getNodeList().get(i);
                        if (fromNode.equals(Node)) {

                            if (mNodeListAdapter.getRecvStreamId(interfaceName, fromNode) < STREAM_ID) { // New
                                                                                                         // Stream
                                                                                                         // Received
                                mNodeListAdapter.setTotalMsgRecvd(interfaceName, fromNode, 0);
                                addLogView(interfaceType, "    > onUdpDataReceived() : from Node"
                                        + mNodeNumberMap.get(interfaceName + fromNode)
                                        + ", first msg)");

                                mNodeListAdapter.setReceiverStartTime(interfaceName, fromNode,
                                        System.currentTimeMillis());
                                mNodeListAdapter.removeReceiverGridData(interfaceName, fromNode);

                                initializeReceiverGrid(interfaceName, fromNode);

                                mNodeListAdapter
                                        .setRecvStreamId(interfaceName, fromNode, STREAM_ID);
                                mNodeListAdapter.IncrementTotalMsgRecvd(interfaceName, fromNode);
                                mNodeListAdapter.setMessageCount(interfaceName, fromNode);

                                if (MODE_RELIABLE != RELIABLE_MODE) {
                                    mNodeListAdapter.ReStartTimer(interfaceName, fromNode,
                                            MsgNumber);
                                }
                                mNodeListAdapter.updateReceiverGrid(interfaceName, fromNode,
                                        MsgNumber,
                                        new Item(greenIcon, String.valueOf(MsgNumber % 10)));

                                expandeGroup(mNodeListAdapter.getNodePosition(interfaceName,
                                        fromNode));
                                break;

                            } else if (mNodeListAdapter.getRecvStreamId(interfaceName, fromNode) == STREAM_ID) { // Same
                                                                                                                 // Stream
                                if (MsgNumber == MSG_COUNT - 1) {
                                    mNodeListAdapter
                                            .IncrementTotalMsgRecvd(interfaceName, fromNode);
                                    mNodeListAdapter.setReceiverEndTime(interfaceName, fromNode,
                                            System.currentTimeMillis());

                                    if (MODE_RELIABLE != RELIABLE_MODE) {
                                        for (int k = mNodeListAdapter.getMessgeCount(interfaceName,
                                                fromNode); k < MsgNumber; k++) {

                                            mNodeListAdapter.updateReceiverGrid(interfaceName,
                                                    fromNode, k,
                                                    new Item(redIcon, String.valueOf(k % 10)));
                                        }
                                    }

                                    long totalTime = mNodeListAdapter.getReceiverEndTime(
                                            interfaceName, fromNode)
                                            - mNodeListAdapter.getReceiverStartTime(interfaceName,
                                                    fromNode);

                                    int loss = mNodeListAdapter.getReceiverLossPer(interfaceName,
                                            fromNode, mNodeListAdapter.getTotalMsgRecvd(
                                                    interfaceName, fromNode));

                                    addLogView(interfaceType,
                                            "    > onUdpDataReceived() : from Node"
                                                    + mNodeNumberMap.get(interfaceName + fromNode)
                                                    + ", last msg)");

                                    addLogView(interfaceType,
                                            "    **************************************************");

                                    addLogView(interfaceType, "    : Receiving time " + totalTime
                                            + "ms, Loss " + loss + "%");

                                    addLogView(interfaceType,
                                            "    **************************************************");

                                    mNodeListAdapter.setTotalMsgRecvd(interfaceName, fromNode, 0);
                                    mNodeListAdapter.StopTimer(interfaceName, fromNode);
                                    mNodeListAdapter.setMessageCount(interfaceName, fromNode);
                                } else {
                                    mNodeListAdapter
                                            .IncrementTotalMsgRecvd(interfaceName, fromNode);

                                    if (MODE_RELIABLE != RELIABLE_MODE) {
                                        mNodeListAdapter.ReStartTimer(interfaceName, fromNode,
                                                MsgNumber);
                                    }
                                }

                                mNodeListAdapter.updateReceiverGrid(interfaceName, fromNode,
                                        MsgNumber,
                                        new Item(greenIcon, String.valueOf(MsgNumber % 10)));

                                expandeGroup(mNodeListAdapter.getNodePosition(interfaceName,
                                        fromNode));
                                break;
                            } else { // Old Stream
                                break;
                            }

                        } else {
                            continue;
                        }
                    }
                } else {
                    addLogView(interfaceType, "    Message Parsing failed");
                }
            } else {
                addLogView(interfaceType, "    Not Handled: Invalid Payload Type:"
                        + payload_type[0]);
            }
        } else {
            addLogView(interfaceType, "    Not Handled: Invalid Session name:" + sessionName);
        }
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

    private int getInterfaceType(String interfaceName) {
        if (interfaceName.equals("Wi-Fi")) {
            return SchordManager.INTERFACE_TYPE_WIFI;
        } else if (interfaceName.equals("Wi-Fi Direct")) {
            return SchordManager.INTERFACE_TYPE_WIFI_P2P;
        } else if (interfaceName.equals("Mobile AP")) {
            return SchordManager.INTERFACE_TYPE_WIFI_AP;
        }

        return -1;
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
                channel = mSchordManager_1.getJoinedChannel(CHORD_UDP_TEST_CHANNEL);
                break;
            case 2:
                channel = mSchordManager_2.getJoinedChannel(CHORD_UDP_TEST_CHANNEL);
                break;
            case 3:
                channel = mSchordManager_3.getJoinedChannel(CHORD_UDP_TEST_CHANNEL);
                break;

        }

        return channel;
    }

    private int checkMessageSize() {
        int message_size = -1;

        try {
            message_size = Integer.parseInt(messageSizeEditText.getText().toString());
        } catch (NumberFormatException e) {
            Toast.makeText(getApplicationContext(), "Enter Valid Message Size (1 ~ 65535)",
                    Toast.LENGTH_SHORT).show();
            return -1;
        }

        if (message_size <= 0 || message_size > 65535) {
            Toast.makeText(getApplicationContext(), "Enter Valid Message Size (1 ~ 65535)",
                    Toast.LENGTH_SHORT).show();
            return -1;
        }

        return message_size;
    }

    private void expandeGroup(int groupPos) {
        mNode_listView.expandGroup(groupPos);
        mNodeListAdapter.notifyDataSetChanged();
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

    public void restart_timer(final String interfaceName, final String Node, final int msg_count) {
        if (getApplicationContext() != null) {
            runOnUiThread(new Runnable() {

                @Override
                public void run() {
                    updateGrid(interfaceName, Node, msg_count + 1);
                    mNodeListAdapter.ReStartTimer(interfaceName, Node, msg_count + 1);
                }
            });
        }
    }

    public void lossCalculated(final String interfaceName, final String Node, final long RecvTime,
            final int loss) {

        if (getApplicationContext() != null) {
            runOnUiThread(new Runnable() {

                @Override
                public void run() {

                    addLogView(
                            getInterfaceType(interfaceName),
                            "    > Calculation Timer Expired : for Node"
                                    + mNodeNumberMap.get(interfaceName + Node));

                    addLogView(getInterfaceType(interfaceName),
                            "    **************************************************");

                    addLogView(getInterfaceType(interfaceName), "    : Receiving time " + RecvTime
                            + "ms, Loss " + loss + "%");

                    addLogView(getInterfaceType(interfaceName),
                            "    **************************************************");
                    // mNodeListAdapter.setFirstPacket(interfaceName, Node,
                    // false);
                }
            });
        }

    }

    public void updateGrid(final String interfaceName, final String Node, final int msg_count) {
        if (getApplicationContext() != null) {
            runOnUiThread(new Runnable() {

                @Override
                public void run() {
                    mNodeListAdapter.updateReceiverGrid(interfaceName, Node, msg_count, new Item(
                            redIcon, String.valueOf(msg_count % 10)));

                }
            });
        }
    }

    private static final String TAG = "[Chord][Sample]";

    private static final String TAGClass = "UdpFrameworkFragment : ";

    private static final String CHORD_UDP_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.UDPTESTCHANNEL";

    private static final String CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.STREAM_ID";

    private static final String CHORD_SESSION_NAME_TEST = "SESSION_TESTING";

    private final int MODE_RELIABLE = -1;

    private final int MODE_UNRELIABLE = 0;

    private final int MODE_SEMIRELIABLE = 10;

    private int send_mode = MODE_UNRELIABLE;

    private boolean response_needed = false;

    private SchordManager mSchordManager_1 = null;

    private SchordManager mSchordManager_2 = null;

    private SchordManager mSchordManager_3 = null;

    private TextView mWifi_state_view = null;

    private TextView mWifiDirect_state_view = null;

    private TextView mMobileAP_state_view = null;

    private Drawable mDrawableConnected = null;

    private Drawable mDrawableDisconnected = null;

    private boolean mWifi_disconnected = false;

    private boolean mWifiDirect_disconnected = false;

    private boolean mMobileAP_disconnected = false;

    private boolean mWifi_bStarted = false;

    private boolean mWifiDirect_bStarted = false;

    private TextView mMyNodeName_textView = null;

    private TextView mJoinedNodeList_textView = null;

    private ExpandableListView mNode_listView = null;

    private NodeListGridAdapter mNodeListAdapter = null;

    private ChordLogView mLogView = null;

    private ChordLogView mWifiDirect_LogView = null;

    private ChordLogView mMobileAP_LogView = null;

    private HashMap<String, Integer> mNodeNumberMap = new HashMap<String, Integer>();

    private int mNodeNumber = 0;

    private Bitmap greenIcon = null;

    private Bitmap greyIcon = null;

    private Bitmap redIcon = null;

    private RadioGroup radioUdpMode = null;

    private EditText messageSizeEditText = null;

    private Button udpSendButton = null;

    private int MSG_COUNT = 20;

    private String MSG_SIZE_BYTES = "1300";

    private SparseIntArray mInterfaceMap = new SparseIntArray();

}
