using System;
using System.Collections.Generic;

/// <summary>
/// Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
/// 
/// Mobile Communication Division,
/// Digital Media & Communications Business, Samsung Electronics Co., Ltd.
/// 
/// This software and its documentation are confidential and proprietary
/// information of Samsung Electronics Co., Ltd.  No part of the software and
/// documents may be copied, reproduced, transmitted, translated, or reduced to
/// any electronic medium or machine-readable form without the prior written
/// consent of Samsung Electronics.
/// 
/// Samsung Electronics makes no representations with respect to the contents,
/// and assumes no responsibility for any errors that might appear in the
/// software and documents. This publication and the contents hereof are subject
/// to change without notice.
/// </summary>

namespace com.samsung.android.sdk.chord.example
{

	using NetworkListener = com.samsung.android.sdk.chord.SchordManager.NetworkListener;
	using Item = com.samsung.android.sdk.chord.example.adapter.Item;
	using IFileCancelListener = com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.IFileCancelListener;
	using NodeListGridAdapter = com.samsung.android.sdk.chord.example.adapter.NodeListGridAdapter;

	using Activity = android.app.Activity;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using SparseIntArray = android.util.SparseIntArray;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ExpandableListView = android.widget.ExpandableListView;
	using RadioGroup = android.widget.RadioGroup;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class UdpFrameworkActivity : Activity, IFileCancelListener
	{
		private bool InstanceFieldsInitialized = false;

		public UdpFrameworkActivity()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			send_mode = MODE_UNRELIABLE;
		}


		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.udp_framework_activity;

			mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
			mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
			mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

			mDrawableConnected = Resources.getDrawable(R.drawable.ic_network_connected);
			mDrawableDisconnected = Resources.getDrawable(R.drawable.ic_network_disconnected);
			mDrawableConnected.setBounds(0, 0, mDrawableConnected.IntrinsicWidth, mDrawableConnected.IntrinsicHeight);
			mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.IntrinsicWidth, mDrawableDisconnected.IntrinsicHeight);

			mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
			mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
			mJoinedNodeList_textView = (TextView) findViewById(R.id.joinedNodeList_textView);
			mJoinedNodeList_textView.Text = getString(R.@string.joined_node_list, "Empty");

			// Set an adapter for the list of nodes and progressBars.
			mNode_listView = (ExpandableListView) findViewById(R.id.node_listView);
			mNodeListAdapter = new NodeListGridAdapter(BaseContext, response_needed);
			mNode_listView.Adapter = mNodeListAdapter;
			mNodeListAdapter.Listener = this;

			mLogView = (ChordLogView) findViewById(R.id.log_textView);
			mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
			mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

			/* for UDP */
			greenIcon = BitmapFactory.decodeResource(Resources, R.drawable.ic_udp_data_bg_green);
			greyIcon = BitmapFactory.decodeResource(Resources, R.drawable.ic_udp_data_bg_gray);
			redIcon = BitmapFactory.decodeResource(Resources, R.drawable.ic_udp_data_bg_red);

			messageSizeEditText = (EditText) findViewById(R.id.messageSize_editText);
			messageSizeEditText.Text = MSG_SIZE_BYTES;

			udpSendButton = (Button) findViewById(R.id.udp_send_btn);
			udpSendButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			radioUdpMode = (RadioGroup) findViewById(R.id.radio_udp_mode_btn);
			radioUdpMode.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly UdpFrameworkActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(UdpFrameworkActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				outerInstance.sendUdpData();
			}
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly UdpFrameworkActivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(UdpFrameworkActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCheckedChanged(RadioGroup group, int checkedId)
			{
				if (checkedId == R.id.udp_reliable_btn)
				{
					outerInstance.send_mode = outerInstance.MODE_RELIABLE;
				}
				else if (checkedId == R.id.udp_semi_reliable_btn)
				{
					outerInstance.send_mode = outerInstance.MODE_SEMIRELIABLE;
				}
				else if (checkedId == R.id.udp_unreliable_btn)
				{
					outerInstance.send_mode = outerInstance.MODE_UNRELIABLE;
				}
			}
		}

		public override void onResume()
		{
			base.onResume();

			/// <summary>
			/// [A] Initialize Chord!
			/// </summary>
			if (mSchordManager_1 == null)
			{
				initChord();
			}
		}

		public override void onDestroy()
		{
			/// <summary>
			/// [D] Release Chord!
			/// </summary>
			if (mSchordManager_1 != null)
			{
				/// <summary>
				/// If you registered NetworkListener, you should unregister it.
				/// </summary>
				mSchordManager_1.NetworkListener = null;

				mSchordManager_1.close();
				mSchordManager_1 = null;
			}

			if (mSchordManager_2 != null)
			{
				mSchordManager_2.close();
				mSchordManager_2 = null;
			}

			if (mSchordManager_3 != null)
			{
				mSchordManager_3.close();
				mSchordManager_3 = null;
			}

			mNodeNumberMap.Clear();
			mInterfaceMap.clear();
			mNodeListAdapter.StopAllTimer();

			base.onDestroy();
		}

		public virtual void onFileCanceled(string chennel, string node, string trId, int index, bool bMulti)
		{

		}

		private void initChord()
		{

			/// <summary>
			///**************************************************
			/// 1. GetInstance
			/// ***************************************************
			/// </summary>
			Schord chord = new Schord();
			try
			{
				chord.initialize(this);
			}
			catch (SsdkUnsupportedException e)
			{
				if (e.Type == SsdkUnsupportedException.VENDOR_NOT_SUPPORTED)
				{
					// Vender is not SAMSUNG
					return;
				}
			}
			mSchordManager_1 = new SchordManager(this);

			/// <summary>
			///**************************************************
			/// 2. Set some values before start If you want to use secured channel,
			/// you should enable SecureMode. Please refer
			/// UseSecureChannelFragment.java
			/// mSchordManager_1.enableSecureMode(true); Once you will use sendFile
			/// or sendMultiFiles, you have to call setTempDirectory
			/// mSchordManager_1.setTempDirectory(Environment.
			/// getExternalStorageDirectory().getAbsolutePath() + "/Chord");
			/// ***************************************************
			/// </summary>
			mSchordManager_1.Looper = MainLooper;

			/// <summary>
			/// Optional. If you need listening network changed, you can set callback
			/// before starting chord.
			/// </summary>
			mSchordManager_1.NetworkListener = new NetworkListenerAnonymousInnerClassHelper(this);

			IList<int?> ifcList = mSchordManager_1.AvailableInterfaceTypes;
			foreach (int? ifc in ifcList)
			{
				startChord(ifc.Value);
				refreshInterfaceStatus(ifc.Value, true);
			}

		}

		private class NetworkListenerAnonymousInnerClassHelper : SchordManager.NetworkListener
		{
			private readonly UdpFrameworkActivity outerInstance;

			public NetworkListenerAnonymousInnerClassHelper(UdpFrameworkActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onDisconnected(int interfaceType)
			{
				Toast.makeTextuniquetempvar.show();
				outerInstance.refreshInterfaceStatus(interfaceType, false);
			}

			public override void onConnected(int interfaceType)
			{
				Toast.makeTextuniquetempvar.show();
				outerInstance.refreshInterfaceStatus(interfaceType, true);

				switch (interfaceType)
				{
					case SchordManager.INTERFACE_TYPE_WIFI:
						if (outerInstance.mWifi_disconnected)
						{
							return;
						}
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_P2P:
						if (outerInstance.mWifiDirect_disconnected)
						{
							return;
						}
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_AP:
						if (outerInstance.mMobileAP_disconnected)
						{
							return;
						}
						break;
				}
				outerInstance.startChord(interfaceType);
			}
		}

		private void refreshInterfaceStatus(int interfaceType, bool bConnected)
		{

			if (!bConnected)
			{
				if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
				{
					mWifi_state_view.Text = R.@string.wifi_off;
					mWifi_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
				{
					mWifiDirect_state_view.Text = R.@string.wifi_direct_off;
					mWifiDirect_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
				{
					mMobileAP_state_view.Text = R.@string.mobile_ap_off;
					mMobileAP_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
				}
			}
			else
			{
				if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
				{
					mWifi_state_view.Text = R.@string.wifi_on;
					mWifi_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
				{
					mWifiDirect_state_view.Text = R.@string.wifi_direct_on;
					mWifiDirect_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
				{
					mMobileAP_state_view.Text = R.@string.mobile_ap_on;
					mMobileAP_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
				}
			}

		}

		private void startChord(int interfaceType)
		{
			/// <summary>
			/// 3. Start Chord using the first interface in the list of available
			/// interfaces.
			/// </summary>
			addLogView(interfaceType, "\n[A] Start Chord!");

			int managerIndex = 0;
			SchordManager startManager = null;

			if (mInterfaceMap.get(interfaceType) == 0)
			{
				managerIndex = mInterfaceMap.size() + 1;
				mInterfaceMap.put(interfaceType, managerIndex);
			}
			else
			{
				managerIndex = mInterfaceMap.get(interfaceType);
			}

			switch (managerIndex)
			{
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

			try
			{
				Log.d(TAG, TAGClass + "start(" + getInterfaceName(interfaceType) + ") with the SchordManager number : " + managerIndex);

				startManager.Looper = MainLooper;

				switch (interfaceType)
				{
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

			}
			catch (Exception e)
			{
				addLogView(interfaceType, "    Fail to start -" + e.Message);
				mInterfaceMap.delete(interfaceType);
			}

		}

		// ***************************************************
		// ChordManagerListener
		// ***************************************************
		private SchordManager.StatusListener mWifi_ManagerListener = new StatusListenerAnonymousInnerClassHelper();

		private class StatusListenerAnonymousInnerClassHelper : SchordManager.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper()
			{
			}

				/// <summary>
				/// 4. Chord has started successfully
				/// </summary>
			public override void onStarted(string nodeName, int reason)
			{
				outerInstance.mWifi_bStarted = true;
				outerInstance.mLogView.Visibility = View.VISIBLE;

				if (!outerInstance.mWifiDirect_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = getString(R.@string.my_node_name, nodeName);
				}

				if (reason == STARTED_BY_USER)
				{
					// Success to start by calling start() method
					outerInstance.mLogView.appendLog("    > onStarted(STARTED_BY_USER)");
					outerInstance.joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI);
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					// Re-start by network re-connection.
					outerInstance.mLogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}
			}

			/// <summary>
			/// 9. Chord has stopped successfully
			/// </summary>
			public override void onStopped(int reason)
			{

				outerInstance.mWifi_bStarted = false;
				if (!outerInstance.mWifiDirect_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = "";
					outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
				}

				// mNodeListAdapter
				// .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
				outerInstance.mNodeListAdapter.removeReceiverDataGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mWifi_disconnected = true;
				}
			}
		}

		private SchordManager.StatusListener mWifiDirect_ManagerListener = new StatusListenerAnonymousInnerClassHelper2();

		private class StatusListenerAnonymousInnerClassHelper2 : SchordManager.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper2()
			{
			}


			public override void onStarted(string nodeName, int reason)
			{
				outerInstance.mWifiDirect_bStarted = true;
				outerInstance.mWifiDirect_LogView.Visibility = View.VISIBLE;

				if (!outerInstance.mWifi_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = getString(R.@string.my_node_name, nodeName);
				}

				if (reason == STARTED_BY_USER)
				{
					outerInstance.mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
					outerInstance.joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_P2P);
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					outerInstance.mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}

			}

			public override void onStopped(int reason)
			{

				outerInstance.mWifiDirect_bStarted = false;
				if (!outerInstance.mWifi_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = "";
					outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
				}

				// mNodeListAdapter
				// .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
				outerInstance.mNodeListAdapter.removeReceiverDataGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mWifiDirect_disconnected = true;
				}
			}
		}

		private SchordManager.StatusListener mMobileAP_ManagerListener = new StatusListenerAnonymousInnerClassHelper3();

		private class StatusListenerAnonymousInnerClassHelper3 : SchordManager.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper3()
			{
			}


			public virtual void onStarted(string nodeName, int reason)
			{
				outerInstance.mMobileAP_LogView.Visibility = View.VISIBLE;
				outerInstance.mMyNodeName_textView.Text = getString(R.@string.my_node_name, nodeName);

				if (reason == STARTED_BY_USER)
				{
					outerInstance.mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
					outerInstance.joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_AP);
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					outerInstance.mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}

			}

			public override void onStopped(int reason)
			{
				outerInstance.mMyNodeName_textView.Text = "";
				outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");

				// mNodeListAdapter
				// .removeSenderDataGroup(getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
				outerInstance.mNodeListAdapter.removeReceiverDataGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mMobileAP_disconnected = true;
				}
			}
		}

		private void joinTestChannel(int interfaceType)
		{
			/// <summary>
			/// 5. Join my channel
			/// </summary>
			addLogView(interfaceType, "    joinChannel()");

			SchordChannel channel = null;
			SchordManager currentManager = null;

			currentManager = getSchordManager(interfaceType);

			switch (interfaceType)
			{
				case SchordManager.INTERFACE_TYPE_WIFI:
					channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL, mWifi_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_P2P:
					channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL, mWifiDirect_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_AP:
					channel = currentManager.joinChannel(CHORD_UDP_TEST_CHANNEL, mMobileAP_ChannelListener);
					break;
			}

			if (channel == null)
			{
				addLogView(interfaceType, "    Fail to joinChannel");
			}
		}

		private void sendUdpData()
		{
			/// <summary>
			/// 6. Send data to the selected node using UDP
			/// </summary>
			char[] chars = null;
			string AppendMsg = null;

		  try
		  {

			udpSendButton.Enabled = false;
			if (mNodeListAdapter.CheckedNodeList.Count == 0)
			{
				Toast.makeText(ApplicationContext, "Select at least one node", Toast.LENGTH_SHORT).show();
				return;
			}

			// check the size value
			int message_size = checkMessageSize();
			if (message_size == -1)
			{
				return;
			}

			// check the reliable mode
			int reliableTime = send_mode;
			string sendMode = null;
			if (reliableTime == -1)
			{
				sendMode = "Reliable";
			}
			else if (reliableTime == 10)
			{
				sendMode = "Semi-Reliable";
			}
			else if (reliableTime == 0)
			{
				sendMode = "Unreliable";
			}
			else
			{
				sendMode = "UNKNOWN";
			}

			// make the message to send

				if (message_size > 65522)
				{
					// Because of .split() limit of 65535 bytes, new char[65535-13].
					// 13 is for length of Message::19::
					chars = new char[65522];
				}
				else
				{
					chars = new char[message_size];
				}

				Arrays.fill(chars, '*');

				AppendMsg = new string(chars);

			string reqId = null;
			string[][] checkedNodeInfo = new string[1][];
			string toNode = null;
			string interfaceName = null;
			int interfaceType = -1;
			SchordChannel channel = null;
			sbyte[][] payload = new sbyte[1][];

			for (int j = 0; j < MSG_COUNT; j++)
			{

				for (int i = 0; i < mNodeListAdapter.CheckedNodeList.Count; i++)
				{

					checkedNodeInfo = mNodeListAdapter.CheckedNodeList[i];
					toNode = checkedNodeInfo[i][1];
					interfaceName = checkedNodeInfo[i][0];
					interfaceType = getInterfaceType(interfaceName);

					channel = getJoinedChannelByIfcType(interfaceType);

					if (channel == null)
					{
						addLogView(interfaceType, "    Fail to get the joined Channel");
						return;
					}

					string Message = "Message::" + j + "::" + AppendMsg;
					payload[0] = Message.GetBytes();

					/* Initialization of Data Structures for 1st Message Sent */
					if (j == 0)
					{
						addLogView(interfaceType, "    sendUdpData(" + sendMode + ") : to Node" + mNodeNumberMap[interfaceName + toNode] + ", total 20msg");

						int stream_id = mNodeListAdapter.getSendStreamId(interfaceName, toNode);

						mNodeListAdapter.setPayloadtype(interfaceName, toNode, CHORD_SAMPLE_MESSAGE_TYPE + ":" + stream_id + ":" + reliableTime);
						mNodeListAdapter.setSendStreamId(interfaceName, toNode, stream_id + 1);
						// mNodeListAdapter.setTotalMsgSent(interfaceName, toNode,
						// 0);
						// mNodeListAdapter.clearSenderMap(interfaceName, toNode);
					}

					try
					{
						reqId = channel.sendUdpData(toNode, reliableTime, response_needed, mNodeListAdapter.getPayloadtype(interfaceName, toNode), payload, CHORD_SESSION_NAME_TEST);
					}
					catch (Exception)
					{
						addLogView(interfaceType, "    sendUdpData(" + sendMode + ") : to Node" + mNodeNumberMap[interfaceName + toNode] + ", Exception");
					}
				}
			}

		  }

		finally
		{
			chars = null;
			AppendMsg = null;
			udpSendButton.Enabled = true;
		}

		}

		private void initializeReceiverGrid(string interfaceName, string nodeName)
		{
			for (int i = 0; i < MSG_COUNT; i++)
			{
				mNodeListAdapter.addToReceiverGrid(interfaceName, nodeName, new Item(greyIcon, (i % 10).ToString()));
			}

		}

		// ***************************************************
		// ChordChannelListener
		// ***************************************************
		private SchordChannel.StatusListener mWifi_ChannelListener = new StatusListenerAnonymousInnerClassHelper();

		private class StatusListenerAnonymousInnerClassHelper : SchordChannel.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper()
			{
			}


			internal int interfaceType = SchordManager.INTERFACE_TYPE_WIFI;

			internal string interfaceName = outerInstance.getInterfaceName(interfaceType);

			/// <summary>
			/// Called when a node leave event is raised on the channel.
			/// </summary>
			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when a node join event is raised on the channel
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{

			}

			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

			}

			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

			}

			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

			}

			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

			}

			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

			}

			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

			}

			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

			}

			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

			}

			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

			}

			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

			}

			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

			}

			/// <summary>
			/// Called when the udp data message received from the node.
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string sessionName)
			{

				outerInstance.onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload, sessionName);
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

				// onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
				// reqId);
			}

		}

		private SchordChannel.StatusListener mWifiDirect_ChannelListener = new StatusListenerAnonymousInnerClassHelper2();

		private class StatusListenerAnonymousInnerClassHelper2 : SchordChannel.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper2()
			{
			}


			internal int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_P2P;

			internal string interfaceName = outerInstance.getInterfaceName(interfaceType);

			/// <summary>
			/// Called when a node leave event is raised on the channel.
			/// </summary>
			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when a node join event is raised on the channel
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{

			}

			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

			}

			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

			}

			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

			}

			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

			}

			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

			}

			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

			}

			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

			}

			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

			}

			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

			}

			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

			}

			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

			}

			/// <summary>
			/// Called when the udp data message received from the node.
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string sessionName)
			{

				outerInstance.onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload, sessionName);
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

				// onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
				// reqId);
			}

		}

		private SchordChannel.StatusListener mMobileAP_ChannelListener = new StatusListenerAnonymousInnerClassHelper3();

		private class StatusListenerAnonymousInnerClassHelper3 : SchordChannel.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper3()
			{
			}


			internal int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_AP;

			internal string interfaceName = outerInstance.getInterfaceName(interfaceType);

			/// <summary>
			/// Called when a node leave event is raised on the channel.
			/// </summary>
			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when a node join event is raised on the channel
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{

			}

			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

			}

			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

			}

			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

			}

			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

			}

			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

			}

			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

			}

			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

			}

			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

			}

			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

			}

			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

			}

			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

			}

			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

			}

			/// <summary>
			/// Called when the udp data message received from the node.
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string sessionName)
			{

				outerInstance.onUdpDataReceivedCommon(interfaceType, interfaceName, fromNode, payloadType, payload, sessionName);
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

				// onUdpDataDeliveredCommon(interfaceType, interfaceName, fromNode,
				// reqId);
			}

		}

		private void onNodeCallbackCommon(bool isJoin, int interfaceType, string fromNode)
		{

			string interfaceName = getInterfaceName(interfaceType);

			if (isJoin)
			{
				if (fromNode.Length == 0 || fromNode == null)
				{
					return;
				}
				if (mNodeNumberMap.ContainsKey(interfaceName + fromNode))
				{
					mNodeListAdapter.addNode(interfaceName, fromNode, mNodeNumberMap[interfaceName + fromNode].Value);
					addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");
				}
				else
				{
					mNodeNumber++;
					mNodeNumberMap[interfaceName + fromNode] = mNodeNumber;
					mNodeListAdapter.addNode(interfaceName, fromNode, mNodeNumber);
					addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumber + " : " + fromNode + ")");
				}
			}
			else
			{
				addLogView(interfaceType, "    > onNodeLeft(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");

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

		private void onUdpDataReceivedCommon(int interfaceType, string interfaceName, string fromNode, string payloadType, sbyte[][] payload, string sessionName)
		{

			string Node = null;

			if (sessionName.Equals(CHORD_SESSION_NAME_TEST))
			{
				// Grid is maintained for CHORD_SESSION_NAME_TEST

				string[] payload_info = (new string(payload[0])).Split("::", true);
				string[] payload_type = payloadType.Split(":", true);

				if (payload_type[0].Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{

					int MsgNumber = 0;
					int STREAM_ID = int.Parse(payload_type[1]);
					int RELIABLE_MODE = int.Parse(payload_type[2]);

					if (payload_info[0].Equals("Message"))
					{

						MsgNumber = int.Parse(payload_info[1]);

						for (int i = 0; i < mNodeListAdapter.NodeList.Count; i++)
						{
							Node = mNodeListAdapter.NodeList[i];
							if (fromNode.Equals(Node))
							{

								if (mNodeListAdapter.getRecvStreamId(interfaceName, fromNode) < STREAM_ID)
								{ // New
																											 // Stream
																											 // Received
									mNodeListAdapter.setTotalMsgRecvd(interfaceName, fromNode, 0);
									addLogView(interfaceType, "    > onUdpDataReceived() : from Node" + mNodeNumberMap[interfaceName + fromNode] + ", first msg)");

									mNodeListAdapter.setReceiverStartTime(interfaceName, fromNode, DateTimeHelperClass.CurrentUnixTimeMillis());
									mNodeListAdapter.removeReceiverGridData(interfaceName, fromNode);

									initializeReceiverGrid(interfaceName, fromNode);

									mNodeListAdapter.setRecvStreamId(interfaceName, fromNode, STREAM_ID);
									mNodeListAdapter.IncrementTotalMsgRecvd(interfaceName, fromNode);
									mNodeListAdapter.setMessageCount(interfaceName, fromNode);

									if (MODE_RELIABLE != RELIABLE_MODE)
									{
										mNodeListAdapter.ReStartTimer(interfaceName, fromNode, MsgNumber);
									}
									mNodeListAdapter.updateReceiverGrid(interfaceName, fromNode, MsgNumber, new Item(greenIcon, (MsgNumber % 10).ToString()));

									expandeGroup(mNodeListAdapter.getNodePosition(interfaceName, fromNode));
									break;

								}
								else if (mNodeListAdapter.getRecvStreamId(interfaceName, fromNode) == STREAM_ID)
								{ // Same
																													 // Stream
									if (MsgNumber == MSG_COUNT - 1)
									{
										mNodeListAdapter.IncrementTotalMsgRecvd(interfaceName, fromNode);
										mNodeListAdapter.setReceiverEndTime(interfaceName, fromNode, DateTimeHelperClass.CurrentUnixTimeMillis());

										if (MODE_RELIABLE != RELIABLE_MODE)
										{
											for (int k = mNodeListAdapter.getMessgeCount(interfaceName, fromNode); k < MsgNumber; k++)
											{

												mNodeListAdapter.updateReceiverGrid(interfaceName, fromNode, k, new Item(redIcon, (k % 10).ToString()));
											}
										}

										long totalTime = mNodeListAdapter.getReceiverEndTime(interfaceName, fromNode) - mNodeListAdapter.getReceiverStartTime(interfaceName, fromNode);

										int loss = mNodeListAdapter.getReceiverLossPer(interfaceName, fromNode, mNodeListAdapter.getTotalMsgRecvd(interfaceName, fromNode));

										addLogView(interfaceType, "    > onUdpDataReceived() : from Node" + mNodeNumberMap[interfaceName + fromNode] + ", last msg)");

										addLogView(interfaceType, "    **************************************************");

										addLogView(interfaceType, "    : Receiving time " + totalTime + "ms, Loss " + loss + "%");

										addLogView(interfaceType, "    **************************************************");

										mNodeListAdapter.setTotalMsgRecvd(interfaceName, fromNode, 0);
										mNodeListAdapter.StopTimer(interfaceName, fromNode);
										mNodeListAdapter.setMessageCount(interfaceName, fromNode);
									}
									else
									{
										mNodeListAdapter.IncrementTotalMsgRecvd(interfaceName, fromNode);

										if (MODE_RELIABLE != RELIABLE_MODE)
										{
											mNodeListAdapter.ReStartTimer(interfaceName, fromNode, MsgNumber);
										}
									}

									mNodeListAdapter.updateReceiverGrid(interfaceName, fromNode, MsgNumber, new Item(greenIcon, (MsgNumber % 10).ToString()));

									expandeGroup(mNodeListAdapter.getNodePosition(interfaceName, fromNode));
									break;
								}
								else
								{ // Old Stream
									break;
								}

							}
							else
							{
								continue;
							}
						}
					}
					else
					{
						addLogView(interfaceType, "    Message Parsing failed");
					}
				}
				else
				{
					addLogView(interfaceType, "    Not Handled: Invalid Payload Type:" + payload_type[0]);
				}
			}
			else
			{
				addLogView(interfaceType, "    Not Handled: Invalid Session name:" + sessionName);
			}
		}

		private void setJoinedNodeCount()
		{
			int nodeCnt = mNodeListAdapter.GroupCount;

			if (nodeCnt == 0)
			{
				mJoinedNodeList_textView.Text = getString(R.@string.joined_node_list, "Empty");
			}
			else if (nodeCnt == 1)
			{
				mJoinedNodeList_textView.Text = getString(R.@string.joined_node_list, nodeCnt + " node");
			}
			else
			{
				mJoinedNodeList_textView.Text = getString(R.@string.joined_node_list, nodeCnt + " nodes");
			}

		}

		private int getInterfaceType(string interfaceName)
		{
			if (interfaceName.Equals("Wi-Fi"))
			{
				return SchordManager.INTERFACE_TYPE_WIFI;
			}
			else if (interfaceName.Equals("Wi-Fi Direct"))
			{
				return SchordManager.INTERFACE_TYPE_WIFI_P2P;
			}
			else if (interfaceName.Equals("Mobile AP"))
			{
				return SchordManager.INTERFACE_TYPE_WIFI_AP;
			}

			return -1;
		}

		private string getInterfaceName(int interfaceType)
		{
			if (SchordManager.INTERFACE_TYPE_WIFI == interfaceType)
			{
				return "Wi-Fi";
			}
			else if (SchordManager.INTERFACE_TYPE_WIFI_AP == interfaceType)
			{
				return "Mobile AP";
			}
			else if (SchordManager.INTERFACE_TYPE_WIFI_P2P == interfaceType)
			{
				return "Wi-Fi Direct";
			}

			return "UNKNOWN";
		}

		private SchordManager getSchordManager(int interfaceType)
		{
			int managerIndex = 0;
			SchordManager manager = null;

			managerIndex = mInterfaceMap.get(interfaceType);

			switch (managerIndex)
			{
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

		private SchordChannel getJoinedChannelByIfcType(int ifcType)
		{
			int managerIndex = 0;
			SchordChannel channel = null;

			managerIndex = mInterfaceMap.get(ifcType);

			switch (managerIndex)
			{
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

		private int checkMessageSize()
		{
			int message_size = -1;

			try
			{
				message_size = int.Parse(messageSizeEditText.Text.ToString());
			}
			catch (System.FormatException)
			{
				Toast.makeText(ApplicationContext, "Enter Valid Message Size (1 ~ 65535)", Toast.LENGTH_SHORT).show();
				return -1;
			}

			if (message_size <= 0 || message_size > 65535)
			{
				Toast.makeText(ApplicationContext, "Enter Valid Message Size (1 ~ 65535)", Toast.LENGTH_SHORT).show();
				return -1;
			}

			return message_size;
		}

		private void expandeGroup(int groupPos)
		{
			mNode_listView.expandGroup(groupPos);
			mNodeListAdapter.notifyDataSetChanged();
		}

		private void addLogView(int interfaceType, string str)
		{
			if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
			{
				mLogView.appendLog(str);
			}
			else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
			{
				mWifiDirect_LogView.appendLog(str);
			}
			else
			{
				mMobileAP_LogView.appendLog(str);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void restart_timer(final String interfaceName, final String Node, final int msg_count)
		public virtual void restart_timer(string interfaceName, string Node, int msg_count)
		{
			if (ApplicationContext != null)
			{
				runOnUiThread(() =>
				{

					updateGrid(interfaceName, Node, msg_count + 1);
					mNodeListAdapter.ReStartTimer(interfaceName, Node, msg_count + 1);
				});
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void lossCalculated(final String interfaceName, final String Node, final long RecvTime, final int loss)
		public virtual void lossCalculated(string interfaceName, string Node, long RecvTime, int loss)
		{

			if (ApplicationContext != null)
			{
				runOnUiThread(() =>
				{


					addLogView(getInterfaceType(interfaceName), "    > Calculation Timer Expired : for Node" + mNodeNumberMap[interfaceName + Node]);

					addLogView(getInterfaceType(interfaceName), "    **************************************************");

					addLogView(getInterfaceType(interfaceName), "    : Receiving time " + RecvTime + "ms, Loss " + loss + "%");

					addLogView(getInterfaceType(interfaceName), "    **************************************************");
					// mNodeListAdapter.setFirstPacket(interfaceName, Node,
					// false);
				});
			}

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void updateGrid(final String interfaceName, final String Node, final int msg_count)
		public virtual void updateGrid(string interfaceName, string Node, int msg_count)
		{
			if (ApplicationContext != null)
			{
				runOnUiThread(() =>
				{

					mNodeListAdapter.updateReceiverGrid(interfaceName, Node, msg_count, new Item(redIcon, (msg_count % 10).ToString()));

				});
			}
		}

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "UdpFrameworkFragment : ";

		private const string CHORD_UDP_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.UDPTESTCHANNEL";

		private const string CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.STREAM_ID";

		private const string CHORD_SESSION_NAME_TEST = "SESSION_TESTING";

		private readonly int MODE_RELIABLE = -1;

		private readonly int MODE_UNRELIABLE = 0;

		private readonly int MODE_SEMIRELIABLE = 10;

		private int send_mode;

		private bool response_needed = false;

		private SchordManager mSchordManager_1 = null;

		private SchordManager mSchordManager_2 = null;

		private SchordManager mSchordManager_3 = null;

		private TextView mWifi_state_view = null;

		private TextView mWifiDirect_state_view = null;

		private TextView mMobileAP_state_view = null;

		private Drawable mDrawableConnected = null;

		private Drawable mDrawableDisconnected = null;

		private bool mWifi_disconnected = false;

		private bool mWifiDirect_disconnected = false;

		private bool mMobileAP_disconnected = false;

		private bool mWifi_bStarted = false;

		private bool mWifiDirect_bStarted = false;

		private TextView mMyNodeName_textView = null;

		private TextView mJoinedNodeList_textView = null;

		private ExpandableListView mNode_listView = null;

		private NodeListGridAdapter mNodeListAdapter = null;

		private ChordLogView mLogView = null;

		private ChordLogView mWifiDirect_LogView = null;

		private ChordLogView mMobileAP_LogView = null;

		private Dictionary<string, int?> mNodeNumberMap = new Dictionary<string, int?>();

		private int mNodeNumber = 0;

		private Bitmap greenIcon = null;

		private Bitmap greyIcon = null;

		private Bitmap redIcon = null;

		private RadioGroup radioUdpMode = null;

		private EditText messageSizeEditText = null;

		private Button udpSendButton = null;

		private int MSG_COUNT = 20;

		private string MSG_SIZE_BYTES = "1300";

		private SparseIntArray mInterfaceMap = new SparseIntArray();

	}

}