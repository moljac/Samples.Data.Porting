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
import com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.IFileCancelListener;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.os.Environment;
import android.os.StatFs;
import android.util.Log;
import android.util.SparseIntArray;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.Button;
import android.widget.ExpandableListView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

public class SendFilesActivity extends Activity implements IFileCancelListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.send_files_activity);

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
        mNodeListAdapter = new NodeListAdapter(getBaseContext(), this);
        mNodeListAdapter.setSecureChannelFrag(false);
        mNode_listView = (ExpandableListView) findViewById(R.id.node_listView);
        mNode_listView.setAdapter(mNodeListAdapter);

        mSend_btn = (Button) findViewById(R.id.send_btn);
        mSend_version_spinner = (Spinner) findViewById(R.id.send_version_spinner);
        mSend_version_spinner.setSelection(SEND_FILE);
        mMultifiles_limitCnt_spinner = (Spinner) findViewById(R.id.multifiles_limitCnt_spinner);
        mMultifiles_limitCnt_spinner.setSelection(0);

        mLogView = (ChordLogView) findViewById(R.id.log_textView);
        mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
        mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

        mSend_btn.setOnClickListener(new OnClickListener() {

            @Override
            public void onClick(View arg0) {

                if (mNodeListAdapter.getCheckedNodeList().isEmpty()) {
                    Toast.makeText(getApplicationContext(), "Please select at least one node",
                            Toast.LENGTH_SHORT).show();
                    return;
                }

                // Call the activity for selecting files.
                Intent mIntent = new Intent(SendFilesActivity.this, FileSelectActivity.class);
                startActivityForResult(mIntent, 0);
            }
        });

        // The spinner to set the version for sending a file
        mSend_version_spinner.setOnItemSelectedListener(new OnItemSelectedListener() {

            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                if (parent.getItemAtPosition(position).toString().equals("sendFile")) {
                    mSend_api = SEND_FILE;
                    mMultifiles_limitCnt_spinner.setSelection(0);
                    mMultifiles_limitCnt_spinner.setEnabled(false);
                } else {
                    mSend_api = SEND_MULTI_FILES;
                    mMultifiles_limitCnt_spinner.setEnabled(true);
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> arg0) {

            }

        });

        // The spinner to set the limit count for sending files simultaneously
        mMultifiles_limitCnt_spinner.setOnItemSelectedListener(new OnItemSelectedListener() {

            @Override
            public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
                int multfiles_limitCnt = Integer.valueOf(parent.getItemAtPosition(position)
                        .toString());

                if (mSchordManager_1 != null) {
                    mSchordManager_1.setSendMultiFilesLimitCount(multfiles_limitCnt);
                }

                if (mSchordManager_2 != null) {
                    mSchordManager_2.setSendMultiFilesLimitCount(multfiles_limitCnt);
                }
            }

            @Override
            public void onNothingSelected(AdapterView<?> arg0) {
            }
        });

        mAlertDialogMap = new HashMap<String, AlertDialog>();
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        // After selecting files to send
        switch (resultCode) {
            case Activity.RESULT_OK:
                ArrayList<String> fileList = data.getStringArrayListExtra("SELECTED_FILE");
                String trId = null;
                String checkedNodeInfo[][] = new String[1][];
                String toNode = null;
                String interfaceName = null;
                int interfaceType = -1;
                SchordChannel channel = null;

                for (int i = 0; i < mNodeListAdapter.getCheckedNodeList().size(); i++) {

                    // Get the list of checked nodes to send files.
                    checkedNodeInfo = mNodeListAdapter.getCheckedNodeList().get(i);
                    interfaceName = checkedNodeInfo[i][0];
                    toNode = checkedNodeInfo[i][1];

                    interfaceType = getInterfaceType(interfaceName);
                    channel = getJoinedChannelByIfcType(interfaceType);
                    if (channel == null) {
                        addLogView(interfaceType, "    Fail to get the joined Channel");
                        return;
                    }

                    if (fileList.isEmpty()) {
                        Toast.makeText(this, "Please select at least one file.", Toast.LENGTH_SHORT)
                                .show();
                        return;
                    }

                    if (mSend_api == SEND_FILE) {
                        if (fileList.size() > 1) {
                            Toast.makeText(this, "Don't select more than one file.",
                                    Toast.LENGTH_SHORT).show();
                            return;
                        }

                        try {
                            /**
                             * 6. Send a file to the selected node
                             */
                            trId = channel.sendFile(toNode, MESSAGE_TYPE_FILE_NOTIFICATION,
                                    fileList.get(0), SHARE_FILE_TIMEOUT_MILISECONDS);
                        } catch (FileNotFoundException e) {
                            e.printStackTrace();
                        } catch (IllegalArgumentException e) {
                            addLogView(interfaceType, "	Fail to send file	:" + e.getMessage());
                        }

                        // Set the total count of files to send. (set to 1)
                        mNodeListAdapter.setFileTotalCnt(interfaceName, toNode, 1, trId);

                    } else if (mSend_api == SEND_MULTI_FILES) {
                        /**
                         * 6. Send multiFile to the selected node
                         */

                        try {
                            trId = channel.sendMultiFiles(toNode, MESSAGE_TYPE_FILE_NOTIFICATION,
                                    fileList, SHARE_FILE_TIMEOUT_MILISECONDS);
                        } catch (FileNotFoundException e) {
                            e.printStackTrace();
                        } catch (IllegalArgumentException e) {
                            addLogView(interfaceType, "	Fail to send multifiles	:" + e.getMessage());
                        }

                        // Set the total count of files to send.
                        mNodeListAdapter.setFileTotalCnt(interfaceName, toNode, fileList.size(),
                                trId);
                    }

                }

                if (null == trId) { // failed to send
                    Toast.makeText(this, getString(R.string.sending_ps_failed, fileList.size()),
                            Toast.LENGTH_SHORT).show();
                } else { // succeed to send

                    for (int i = 0; i < mNodeListAdapter.getCheckedNodeList().size(); i++) {
                        checkedNodeInfo = mNodeListAdapter.getCheckedNodeList().get(i);
                        interfaceType = getInterfaceType(checkedNodeInfo[i][0]);

                        if (mSend_api == SEND_FILE) {
                            addLogView(
                                    getInterfaceType(checkedNodeInfo[i][0]),
                                    "    sendFile() : to Node"
                                            + mNodeNumberMap.get(checkedNodeInfo[i][0]
                                                    + checkedNodeInfo[i][1]));
                        } else if (mSend_api == SEND_MULTI_FILES) {
                            addLogView(
                                    getInterfaceType(checkedNodeInfo[i][0]),
                                    "    sendMultiFiles() : to Node"
                                            + mNodeNumberMap.get(checkedNodeInfo[i][0]
                                                    + checkedNodeInfo[i][1]) + ", "
                                            + fileList.size() + "files");
                        }
                    }

                }

                break;
        }
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

        mAlertDialogMap.clear();
        mNodeNumberMap.clear();
        mInterfaceMap.clear();

        super.onDestroy();
    }

    // **********************************************************************
    // From adapter
    // **********************************************************************
    @Override
    public void onFileCanceled(String interfaceName, String node, String trId, int index,
            boolean bMulti) {

        int interfaceType = getInterfaceType(interfaceName);

        if (interfaceType != -1) {
            SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
            if (channel == null) {
                addLogView(interfaceType, "    Fail to get the joined Channel");
                return;
            }

            if (bMulti) {
                /**
                 * 7. Cancel the multiFile transfer
                 */
                channel.cancelMultiFiles(trId);
                addLogView(interfaceType, "    cancelMultiFiles()");
                mNodeListAdapter.removeCanceledProgress(interfaceName, node, trId);
            } else {
                /**
                 * 7. Cancel the file transfer
                 */
                channel.cancelFile(trId);
                addLogView(interfaceType, "    cancelFile()");
                mNodeListAdapter.removeProgress(index, trId);
            }
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

        /****************************************************
         * 2. Set some values before start. It is recommended to use the
         * application's name and an internal storage of each application as a
         * directory path. If you want to use secured channel, you should enable
         * SecureMode. Please refer UseSecureChannelFragment.java
         * mSchordManager_1.enableSecureMode(true);
         ****************************************************/
        mSchordManager_1.setLooper(getMainLooper());
        mSchordManager_1.setTempDirectory(chordFilePath);

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
            refreshInterfaceStatus(ifc, true);
            startChord(ifc);
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
         * 3. Start Chord using the interface in the list of available
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
            startManager.setTempDirectory(chordFilePath);

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

            mNodeListAdapter.removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                // Stopped by network disconnected
                mWifi_disconnected = true;
                mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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

            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mWifiDirect_disconnected = true;
                mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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
            mNodeListAdapter
                    .removeNodeGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
            setJoinedNodeCount();

            if (NETWORK_DISCONNECTED == reason) {
                mMobileAP_disconnected = true;
                mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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
                channel = currentManager
                        .joinChannel(CHORD_SEND_TEST_CHANNEL, mWifi_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_P2P:
                channel = currentManager.joinChannel(CHORD_SEND_TEST_CHANNEL,
                        mWifiDirect_ChannelListener);
                break;
            case SchordManager.INTERFACE_TYPE_WIFI_AP:
                channel = currentManager.joinChannel(CHORD_SEND_TEST_CHANNEL,
                        mMobileAP_ChannelListener);
                break;
        }

        if (channel == null) {
            addLogView(interfaceType, "    Fail to joinChannel");
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
         * Called when a node join event is raised on the channel.
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

            onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId,
                    totalCount, fileSize);
        }

        /**
         * Called when the sending a file is completed.
         */
        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesSent()  :  to Node"
                            + mNodeNumberMap.get(interfaceName + toNode) + ", file" + index);
            mNodeListAdapter.removeProgress(index, taskId);
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", file" + index);
            mNodeListAdapter.removeProgress(index, taskId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the file transfer is finished to the node.
         */
        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

            onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         */
        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

            onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index,
                    reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress,
                    true, true);
            expandeGroup();
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress,
                    true, false);
            expandeGroup();
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

            onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId,
                    fileSize);
        }

        /**
         * Called when the file transfer is completed to the node.
         */
        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

            addLogView(interfaceType,
                    "    > onFileSent()  :  to Node" + mNodeNumberMap.get(interfaceName + toNode)
                            + ", " + new File(fileName).getName());
            mNodeListAdapter.removeProgress(1, exchangeId);
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onFileReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", " + fileName);
            mNodeListAdapter.removeProgress(1, exchangeId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         */
        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

            onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress,
                    false, true);
            expandeGroup();
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress,
                    false, false);
            expandeGroup();
        }

        /**
         * The following callBack is not used in this Fragment. Please refer to
         * the HelloChordFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
        }

        /**
         * The following callBack is not used in this Fragment. Please refer to
         * the UdpFrameworkFragment.java
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
         * Called when a node join event is raised on the channel.
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

            onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId,
                    totalCount, fileSize);
        }

        /**
         * Called when the sending a file is completed.
         */
        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesSent()  :  to Node"
                            + mNodeNumberMap.get(interfaceName + toNode) + ", file" + index);
            mNodeListAdapter.removeProgress(index, taskId);
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", file" + index);
            mNodeListAdapter.removeProgress(index, taskId);
            mNodeListAdapter.removeProgress(index, taskId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the file transfer is finished to the node.
         */
        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

            onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         **/
        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

            onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index,
                    reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress,
                    true, true);
            expandeGroup();
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress,
                    true, false);
            expandeGroup();
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

            onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId,
                    fileSize);
        }

        /**
         * Called when the file transfer is completed to the node.
         */
        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

            addLogView(interfaceType,
                    "    > onFileSent()  :  to Node" + mNodeNumberMap.get(interfaceName + toNode)
                            + ", " + new File(fileName).getName());
            mNodeListAdapter.removeProgress(1, exchangeId);
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onFileReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", " + fileName);
            mNodeListAdapter.removeProgress(1, exchangeId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         **/
        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

            onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress,
                    false, true);
            expandeGroup();
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress,
                    false, false);
            expandeGroup();
        }

        /**
         * The following callBack is not used in this Fragment. Please refer to
         * the HelloChordFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
        }

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
         * Called when a node join event is raised on the channel.
         */
        @Override
        public void onNodeJoined(String fromNode, String fromChannel) {
            onNodeCallbackCommon(true, interfaceType, fromNode);
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onMultiFilesWillReceive(String fromNode, String fromChannel, String fileName,
                String taskId, int totalCount, String fileType, long fileSize) {

            onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId,
                    totalCount, fileSize);
        }

        /**
         * Called when the sending a file is completed.
         */
        @Override
        public void onMultiFilesSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesSent()  :  to Node"
                            + mNodeNumberMap.get(interfaceName + toNode) + ", file" + index);
            mNodeListAdapter.removeProgress(index, taskId);
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onMultiFilesReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", file" + index);

            mNodeListAdapter.removeProgress(index, taskId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the file transfer is finished to the node.
         */
        @Override
        public void onMultiFilesFinished(String node, String channel, String taskId, int reason) {

            onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         **/
        @Override
        public void onMultiFilesFailed(String node, String channel, String fileName, String taskId,
                int index, int reason) {

            onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index,
                    reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onMultiFilesChunkSent(String toNode, String toChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset,
                long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress,
                    true, true);
            expandeGroup();
        }

        /**
         * Called when the receiving a file is completed from the node.
         */
        @Override
        public void onMultiFilesChunkReceived(String fromNode, String fromChannel, String fileName,
                String taskId, int index, String fileType, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress,
                    true, false);
            expandeGroup();
        }

        /**
         * Called when the Share file notification is received. User can decide
         * to receive or reject the file.
         */
        @Override
        public void onFileWillReceive(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize) {

            onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId,
                    fileSize);
        }

        /**
         * Called when the file transfer is completed to the node.
         */
        @Override
        public void onFileSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId) {

            addLogView(interfaceType,
                    "    > onFileSent()  :  to Node" + mNodeNumberMap.get(interfaceName + toNode)
                            + ", " + new File(fileName).getName());
            mNodeListAdapter.removeProgress(1, exchangeId);
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, String tmpFilePath) {

            addLogView(
                    interfaceType,
                    "    > onFileReceived()  :  from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode) + ", " + fileName);
            mNodeListAdapter.removeProgress(1, exchangeId);
            saveFile(fileName, tmpFilePath);
        }

        /**
         * Called when the error is occurred while the file transfer is in
         * progress.
         **/
        @Override
        public void onFileFailed(String node, String channel, String fileName, String hash,
                String exchangeId, int reason) {

            onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
        }

        /**
         * Called when an individual chunk of the file is sent.
         */
        @Override
        public void onFileChunkSent(String toNode, String toChannel, String fileName, String hash,
                String fileType, String exchangeId, long fileSize, long offset, long chunkSize) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress,
                    false, true);
            expandeGroup();
        }

        /**
         * Called when the file transfer is completed from the node.
         */
        @Override
        public void onFileChunkReceived(String fromNode, String fromChannel, String fileName,
                String hash, String fileType, String exchangeId, long fileSize, long offset) {

            // Set the progressBar - add or update
            int progress = (int) (100 * offset / fileSize);
            mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress,
                    false, false);
            expandeGroup();
        }

        /**
         * The following callBack is not used in this Fragment. Please refer to
         * the HelloChordFragment.java
         */
        @Override
        public void onDataReceived(String fromNode, String fromChannel, String payloadType,
                byte[][] payload) {
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
        } else {
            addLogView(interfaceType,
                    "    > onNodeLeft(Node" + mNodeNumberMap.get(interfaceName + fromNode) + " : "
                            + fromNode + ")");

            mNodeListAdapter.removeNode(interfaceName, fromNode);
        }

        setJoinedNodeCount();
    }

    private void onMultiFilesWillReceiveCommon(int interfaceType, String interfaceName,
            String fromNode, String fileName, String taskId, int totalCount, long fileSize) {

        addLogView(
                interfaceType,
                "    > onMultiFilesWillReceive()  :  from Node"
                        + mNodeNumberMap.get(interfaceName + fromNode) + ", " + totalCount
                        + "files");

        if (checkAvailableMemory(fileSize)) {
            displayFileNotify(interfaceType, fromNode,
                    getString(R.string.file_ps_total_pd, fileName, totalCount), taskId,
                    SEND_MULTI_FILES);

            // Set the total count of files to receive.
            mNodeListAdapter.setFileTotalCnt(interfaceName, fromNode, totalCount, taskId);
        } else {
            /**
             * Because the external storage may be unavailable, you should
             * verify that the volume is available before accessing it. But
             * also, onMultiFilesFailed with ERROR_FILE_SEND_FAILED will be
             * called while Chord got failed to write file.
             **/

            SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
            channel.rejectMultiFiles(taskId);

            addLogView(
                    interfaceType,
                    "    > onMultiFilesWillReceive()\n     : There is not enough storage available. Reject receiving from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode));
        }

    }

    private void onMultiFilesFinishedCommon(int interfaceType, String interfaceName, String node,
            String taskId, int reason) {

        String delimiter = "_";
        String sentOrReceived = null;

        if (taskId.split(delimiter)[0].equals(node)) {
            sentOrReceived = "from";
        } else {
            sentOrReceived = "to";
        }

        addLogView(interfaceType, "    **************************************************");
        switch (reason) {
            case SchordChannel.StatusListener.ERROR_FILE_REJECTED: {
                addLogView(interfaceType, "    > onMultiFilesFinished()  :  REJECTED by Node"
                        + mNodeNumberMap.get(interfaceName + node));
                break;
            }
            case SchordChannel.StatusListener.ERROR_FILE_CANCELED: {
                // String myNodeName = null;
                //
                // if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                // myNodeName = mSchordManager_1.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                // myNodeName = mSchordManager_2.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_AP) {
                // myNodeName = mMobileAP_ChordManager.getName();
                // }
                //
                // if (node.equals(myNodeName)) {
                // addLogView(interfaceType,
                // "    > onMultiFilesFinished()  :  CANCELED");
                // } else {
                // addLogView(interfaceType,
                // "    > onMultiFilesFinished()  :  CANCELED by Node"
                // + mNodeNumberMap.get(interfaceName + node));
                // }

                addLogView(interfaceType, "    > onMultiFilesFinished()  :  CANCELED");
                break;
            }
            case SchordChannel.StatusListener.ERROR_NONE:
                addLogView(interfaceType, "    > onMultiFilesFinished()  :  " + sentOrReceived
                        + " Node" + mNodeNumberMap.get(interfaceName + node));
                break;
            default:

                addLogView(interfaceType, "    > onMultiFilesFinished()  :  Error["
                        + getErrorName(reason) + ":" + reason + "]");
                break;
        }
        addLogView(interfaceType, "    **************************************************\n");

    }

    private void onMultiFilesFailedCommon(int interfaceType, String interfaceName, String node,
            String fileName, String taskId, int index, int reason) {

        addLogView(interfaceType, "    **************************************************");
        switch (reason) {
            case SchordChannel.StatusListener.ERROR_FILE_REJECTED: {
                addLogView(interfaceType, "    > onMultiFilesFailed()  :  REJECTED by Node"
                        + mNodeNumberMap.get(interfaceName + node));
                break;
            }
            case SchordChannel.StatusListener.ERROR_FILE_CANCELED: {
                // String myNodeName = null;
                //
                // if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                // myNodeName = mSchordManager_1.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                // myNodeName = mSchordManager_2.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_AP) {
                // myNodeName = mMobileAP_ChordManager.getName();
                // }
                //
                // if (node.equals(myNodeName)) {
                // addLogView(interfaceType,
                // "    > onMultiFilesFailed()  :  CANCELED");
                // } else {
                // addLogView(interfaceType,
                // "    > onMultiFilesFailed()  :  CANCELED by Node"
                // + mNodeNumberMap.get(interfaceName + node));
                // }

                addLogView(interfaceType, "    > onMultiFilesFailed()  :  CANCELED");
                break;
            }
            case SchordChannel.StatusListener.ERROR_FILE_TIMEOUT: {
                addLogView(interfaceType, "    > onMultiFilesFailed()  :  TIME OUT - Node"
                        + mNodeNumberMap.get(interfaceName + node) + ", " + fileName);
                break;
            }
            default:
                addLogView(interfaceType, "    > onMultiFilesFailed()  :   Error["
                        + getErrorName(reason) + ":" + reason + "]");
                break;
        }
        addLogView(interfaceType, "    **************************************************\n");

        mNodeListAdapter.removeProgress(index, taskId);
        AlertDialog alertDialog = mAlertDialogMap.get(taskId);
        if (alertDialog != null) {
            alertDialog.dismiss();
            mAlertDialogMap.remove(taskId);
        }

    }

    private void onFileWillReceiveCommon(int interfaceType, String interfaceName, String fromNode,
            String fileName, String exchangeId, long fileSize) {

        addLogView(
                interfaceType,
                "    > onFileWillReceive()  :  from Node"
                        + mNodeNumberMap.get(interfaceName + fromNode) + ", " + fileName);

        if (checkAvailableMemory(fileSize)) {
            displayFileNotify(interfaceType, fromNode, fileName, exchangeId, SEND_FILE);

            // Set the total count of files to receive.
            mNodeListAdapter.setFileTotalCnt(interfaceName, fromNode, 1, exchangeId);
        } else {
            /**
             * Because the external storage may be unavailable, you should
             * verify that the volume is available before accessing it. But
             * also, onFileFailed with ERROR_FILE_SEND_FAILED will be called
             * while Chord got failed to write file.
             **/
            SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
            channel.rejectFile(exchangeId);
            addLogView(
                    interfaceType,
                    "    > onFileWillReceive()\n     : There is not enough storage available. Reject receiving from Node"
                            + mNodeNumberMap.get(interfaceName + fromNode));
        }
    }

    private void onFileFailedCommon(int interfaceType, String interfaceName, String node,
            String fileName, String exchangeId, int reason) {

        addLogView(interfaceType, "    ******************************************");
        switch (reason) {
            case SchordChannel.StatusListener.ERROR_FILE_REJECTED: {
                addLogView(interfaceType, "    > onFileFailed()  :  REJECTED by Node"
                        + mNodeNumberMap.get(interfaceName + node));
                break;
            }
            case SchordChannel.StatusListener.ERROR_FILE_CANCELED: {
                // String myNodeName = null;
                //
                // if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
                // myNodeName = mSchordManager_1.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_P2P) {
                // myNodeName = mSchordManager_2.getName();
                // } else if (interfaceType ==
                // SchordManager.INTERFACE_TYPE_WIFI_AP) {
                // myNodeName = mMobileAP_ChordManager.getName();
                // }
                //
                // if (node.equals(myNodeName)) {
                // addLogView(interfaceType,
                // "    > onFileFailed()  :  CANCELED");
                // } else {
                // addLogView(interfaceType,
                // "    > onFileFailed()  :  CANCELED by Node"
                // + mNodeNumberMap.get(interfaceName + node));
                // }

                addLogView(interfaceType, "    > onFileFailed()  :  CANCELED");
                break;
            }
            case SchordChannel.StatusListener.ERROR_FILE_TIMEOUT: {
                addLogView(interfaceType, "    > onFileFailed()  :  TIME OUT " + fileName);
                break;
            }
            default:
                addLogView(interfaceType, "    > onFileFailed()  :  Error[" + reason + "] - Node"
                        + mNodeNumberMap.get(interfaceName + node) + ", " + fileName);
                break;
        }
        addLogView(interfaceType, "    ******************************************\n");

        mNodeListAdapter.removeProgress(1, exchangeId);

        AlertDialog alertDialog = mAlertDialogMap.get(exchangeId);
        if (alertDialog != null) {
            alertDialog.dismiss();
            mAlertDialogMap.remove(exchangeId);
        }

    }

    private String getErrorName(int errorType) {
        if (errorType == SchordChannel.StatusListener.ERROR_FILE_SEND_FAILED) {
            return "ERROR_FILE_SEND_FAILED";
        } else if (errorType == SchordChannel.StatusListener.ERROR_FILE_CREATE_FAILED) {
            return "ERROR_FILE_CREATE_FAILED";
        } else if (errorType == SchordChannel.StatusListener.ERROR_FILE_NO_RESOURCE) {
            return "ERROR_FILE_NO_RESOURCE";
        }

        return "UNKNOWN";
    }

    private String getInterfaceName(int interfaceType) {
        if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI) {
            return "Wi-Fi";
        } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP) {
            return "Mobile AP";
        } else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P) {
            return "Wi-Fi Direct";
        }

        return "UNKNOWN";
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
                channel = mSchordManager_1.getJoinedChannel(CHORD_SEND_TEST_CHANNEL);
                break;
            case 2:
                channel = mSchordManager_2.getJoinedChannel(CHORD_SEND_TEST_CHANNEL);
                break;
            case 3:
                channel = mSchordManager_3.getJoinedChannel(CHORD_SEND_TEST_CHANNEL);
                break;

        }

        return channel;
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

    private boolean checkAvailableMemory(long fileSize) {
        File targetdir = new File(chordFilePath);
        if (!targetdir.exists()) {
            targetdir.mkdirs();
        }

        StatFs stat = new StatFs(chordFilePath);
        long blockSize = stat.getBlockSize();
        long totalBlocks = stat.getAvailableBlocks();
        long availableMemory = blockSize * totalBlocks;

        if (availableMemory < fileSize) {
            return false;
        }

        return true;
    }

    private void displayFileNotify(final int ifc, final String nodeName, final String fileName,
            final String trId, final int sendApi) {

        final SchordChannel channel = getJoinedChannelByIfcType(ifc);
        if (channel == null) {
            addLogView(ifc, "    Fail to get the joined Channel");
            return;
        }

        // for dialog whether accept the file transfer or not.
        AlertDialog alertDialog = new AlertDialog.Builder(this)
                .setTitle("Receive Files")
                .setMessage(
                        getString(R.string.from_ps_file_ps, nodeName + " [" + getInterfaceName(ifc)
                                + "]", fileName))
                .setPositiveButton(R.string.accept, new DialogInterface.OnClickListener() {

                    @Override
                    public void onClick(DialogInterface arg0, int arg1) {
                        /**
                         * 6. Accept the file transfer
                         */
                        if (senderLeft(ifc, nodeName) || !mAlertDialogMap.containsKey(trId)) {
                            return;
                        }

                        if (sendApi == SEND_FILE) {
                            channel.acceptFile(trId, 30 * 1000, 2, 300 * 1024);
                            addLogView(ifc, "    acceptFile()");
                        } else if (sendApi == SEND_MULTI_FILES) {
                            channel.acceptMultiFiles(trId, 30 * 1000, 2, 300 * 1024);
                            addLogView(ifc, "    acceptMultiFiles()");
                        }
                        mAlertDialogMap.remove(trId);

                    }
                }).setNegativeButton(R.string.reject, new DialogInterface.OnClickListener() {

                    @Override
                    public void onClick(DialogInterface arg0, int arg1) {
                        /**
                         * 6. Reject the file transfer
                         */
                        if (senderLeft(ifc, nodeName) || !mAlertDialogMap.containsKey(trId)) {
                            return;
                        }

                        if (sendApi == SEND_FILE) {
                            channel.rejectFile(trId);
                            addLogView(ifc, "    rejectFile()");
                        } else if (sendApi == SEND_MULTI_FILES) {
                            channel.rejectMultiFiles(trId);
                            addLogView(ifc, "    rejectMultiFiles()");
                        }
                        mAlertDialogMap.remove(trId);

                    }
                }).create();

        alertDialog.show();
        mAlertDialogMap.put(trId, alertDialog);
    }

    private boolean senderLeft(int interfaceType, String nodeName) {
        for (String sender : mNodeListAdapter.getNodeList()) {
            if (sender.equals(nodeName)) {
                return false;
            }
        }
        addLogView(interfaceType, "    The sender left.");

        return true;
    }

    private void saveFile(String fileName, String tmpFilePath) {
        String savedName = fileName;
        String name, ext;

        int i = savedName.lastIndexOf(".");
        if (i == -1) {
            name = savedName;
            ext = "";
        } else {
            name = savedName.substring(0, i);
            ext = savedName.substring(i);
        }

        File targetFile = new File(chordFilePath, savedName);
        int index = 0;
        while (targetFile.exists()) {
            savedName = name + "_" + index + ext;
            targetFile = new File(chordFilePath, savedName);
            index++;
        }

        File srcFile = new File(tmpFilePath);
        srcFile.renameTo(targetFile);
    }

    private void expandeGroup() {
        // Expand the list of the progressBar
        int nodeCnt = mNodeListAdapter.getGroupCount();
        for (int i = 0; i < nodeCnt; i++) {
            mNode_listView.expandGroup(i);
        }
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

    private static final String CHORD_SEND_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.SENDTESTCHANNEL";

    private static final String MESSAGE_TYPE_FILE_NOTIFICATION = "FILE_NOTIFICATION_V2";

    private static final String chordFilePath = Environment.getExternalStorageDirectory()
            .getAbsolutePath() + "/ChordExample";

    private static final int SHARE_FILE_TIMEOUT_MILISECONDS = 1000 * 60 * 60;

    private static final int SEND_FILE = 1;

    private static final int SEND_MULTI_FILES = 2;

    private int mSend_api = 2;

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

    private boolean mWifi_disconnected = false;

    private boolean mWifiDirect_disconnected = false;

    private boolean mMobileAP_disconnected = false;

    private TextView mMyNodeName_textView = null;

    private TextView mJoinedNodeList_textView = null;

    private NodeListAdapter mNodeListAdapter = null;

    private ExpandableListView mNode_listView = null;

    private Button mSend_btn = null;

    private Spinner mSend_version_spinner = null;

    private Spinner mMultifiles_limitCnt_spinner = null;

    private ChordLogView mLogView = null;

    private ChordLogView mWifiDirect_LogView = null;

    private ChordLogView mMobileAP_LogView = null;

    private HashMap<String, AlertDialog> mAlertDialogMap = null;

    private HashMap<String, Integer> mNodeNumberMap = new HashMap<String, Integer>();

    private int mNodeNumber = 0;

    private SparseIntArray mInterfaceMap = new SparseIntArray();

    private static final String TAG = "[Chord][Sample]";

    private static final String TAGClass = "SendFilesFragment : ";

}
