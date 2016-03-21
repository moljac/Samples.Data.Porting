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
	using NodeListAdapter = com.samsung.android.sdk.chord.example.adapter.NodeListAdapter;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Color = android.graphics.Color;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using Editable = android.text.Editable;
	using TextWatcher = android.text.TextWatcher;
	using Log = android.util.Log;
	using SparseIntArray = android.util.SparseIntArray;
	using GestureDetector = android.view.GestureDetector;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using OnTouchListener = android.view.View.OnTouchListener;
	using InputMethodManager = android.view.inputmethod.InputMethodManager;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using EditText = android.widget.EditText;
	using ExpandableListView = android.widget.ExpandableListView;
	using LinearLayout = android.widget.LinearLayout;
	using LayoutParams = android.widget.LinearLayout.LayoutParams;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class UseSecureChannelActivity : Activity, View.OnClickListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.use_secure_channel_activity;

			mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
			mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
			mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

			mDrawableConnected = Resources.getDrawable(R.drawable.ic_network_connected);
			mDrawableDisconnected = Resources.getDrawable(R.drawable.ic_network_disconnected);
			mDrawableConnected.setBounds(0, 0, mDrawableConnected.IntrinsicWidth, mDrawableConnected.IntrinsicHeight);
			mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.IntrinsicWidth, mDrawableDisconnected.IntrinsicHeight);

			mLogView = (ChordLogView) findViewById(R.id.log_textView);
			mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
			mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

			mWifi_channel_btn = (Button) findViewById(R.id.wifi_channel_btn);
			mWifi_channel_btn.OnClickListener = this;
			mWifiDirect_channel_btn = (Button) findViewById(R.id.wifiDirect_channel_btn);
			mWifiDirect_channel_btn.OnClickListener = this;
			mMobileAP_channel_btn = (Button) findViewById(R.id.mobileAP_channel_btn);
			mMobileAP_channel_btn.OnClickListener = this;

			mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
			mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
			mJoinedNodeList_textView = (TextView) findViewById(R.id.joinedNodeList_textView);
			mJoinedNodeList_textView.Text = getString(R.@string.joined_node_list, "Empty");

			mNodeListAdapter = new NodeListAdapter(BaseContext, null);
			mNodeListAdapter.CheckMode = false;
			mNodeListAdapter.SecureChannelFrag = true;

			mPeer_listView = (ExpandableListView) findViewById(R.id.secure_peer_list);
			mPeer_listView.Adapter = mNodeListAdapter;
			mPeer_listView.GroupIndicator = null;
		}

		public override void onResume()
		{
			base.onResume();

			/// <summary>
			/// [A] Initialize and start Chord!
			/// </summary>
			if (mSchordManager_1 == null)
			{
				// mLogView.appendLog("\n[A] Initialize Chord!");
				initChord();
			}

		}

		public override void onDestroy()
		{
			/// <summary>
			/// [F] Release Chord!
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

			base.onDestroy();
		}

		public override void onClick(View v)
		{

			bool bJoined = false;
			int ifc = -1;

			switch (v.Id)
			{

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

			if (!bJoined)
			{
				showChannelInputDialog(ifc);
			}
			else
			{
				/// <summary>
				/// [D] leave test channel
				/// </summary>
				addLogView(ifc, "\n[C] Leave test channel!");
				leaveTestChannel(ifc);
			}

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
			mSchordManager_1.Looper = MainLooper;

			/// <summary>
			///***************************************************
			/// 2. You have to enable SecureMode to use Secured channel. It is false
			/// as default. If you do not set and try to joinChannel with Secured
			/// name, it will throw IllegalArgumentException.
			/// ***************************************************
			/// </summary>
			mSchordManager_1.SecureModeEnabled = true;

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
			private readonly UseSecureChannelActivity outerInstance;

			public NetworkListenerAnonymousInnerClassHelper(UseSecureChannelActivity outerInstance)
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
				startManager.SecureModeEnabled = true;

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


			public override void onStarted(string nodeName, int reason)
			{
				/// <summary>
				/// 4. Chord has started successfully
				/// </summary>
				outerInstance.mWifi_bStarted = true;
				outerInstance.mLogView.Visibility = View.VISIBLE;

				if (!outerInstance.mWifiDirect_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = getString(R.@string.my_node_name, nodeName);
				}

				outerInstance.mWifi_channel_btn.Enabled = true;

				if (reason == STARTED_BY_USER)
				{
					// Success to start by calling start() method
					outerInstance.mLogView.appendLog("    > onStarted(STARTED_BY_USER)");
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					// Re-start by network re-connection.
					outerInstance.mLogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}

			}

			public override void onStopped(int reason)
			{
				/// <summary>
				/// 9. Chord has stopped successfully
				/// </summary>
				outerInstance.mWifi_bStarted = false;
				outerInstance.mWifi_channel_btn.Enabled = false;

				if (!outerInstance.mWifiDirect_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = "";
					outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
				}

				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mWifi_disconnected = true;
					outerInstance.mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
				}
				else
				{
					outerInstance.mWifi_joined = false;
					outerInstance.mWifi_channel_btn.Text = R.@string.join_channel;
					outerInstance.mLogView.appendLog("    > onStopped(Error[" + reason + "])");
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

				outerInstance.mWifiDirect_channel_btn.Enabled = true;

				if (reason == STARTED_BY_USER)
				{
					outerInstance.mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");

				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					outerInstance.mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}

			}

			public override void onStopped(int reason)
			{
				outerInstance.mWifiDirect_bStarted = false;
				outerInstance.mWifiDirect_channel_btn.Enabled = false;

				if (!outerInstance.mWifi_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = "";
					outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
				}

				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mWifiDirect_disconnected = true;
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
				}
				else
				{
					outerInstance.mWifiDirect_joined = false;
					outerInstance.mWifiDirect_channel_btn.Text = R.@string.join_channel;
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(Error[" + reason + "])");
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
				outerInstance.mMobileAP_channel_btn.Enabled = true;

				if (reason == STARTED_BY_USER)
				{
					outerInstance.mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");

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
				outerInstance.mMobileAP_channel_btn.Enabled = false;

				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));

				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mMobileAP_disconnected = true;
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
				}
				else
				{
					outerInstance.mMobileAP_joined = false;
					outerInstance.mMobileAP_channel_btn.Text = R.@string.join_channel;
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(Error[" + reason + "])");
				}
			}
		}

		private void joinTestChannel(int interfaceType, string channelName, bool secureMode)
		{

			if (channelName == null || channelName.Equals(""))
			{
				if (secureMode)
				{
					channelName = SchordManager.SECURE_PREFIX + CHORD_SECURE_TEST_CHANNEL;
				}
				else
				{
					channelName = CHORD_SECURE_TEST_CHANNEL;
				}
			}

			/// <summary>
			///************************************************
			/// 6. Join my channel
			/// *************************************************
			/// </summary>
			addLogView(interfaceType, "    joinChannel(" + channelName + ")");

			SchordChannel channel = null;
			SchordManager currentManager = null;

			currentManager = getSchordManager(interfaceType);

			try
			{
				switch (interfaceType)
				{
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

			}
			catch (Exception e)
			{
				addLogView(interfaceType, "    Fail to join -" + e.Message);
				return;
			}

			if (channel == null)
			{
				addLogView(interfaceType, "    Fail to joinChannel");
			}
			else
			{
				if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
				{
					mWifi_channelName = channelName;
					mWifi_joined = true;
					mWifi_channel_btn.Text = R.@string.leave_channel;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
				{
					mWifiDirect_channelName = channelName;
					mWifiDirect_joined = true;
					mWifiDirect_channel_btn.Text = R.@string.leave_channel;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
				{
					mMobileAP_channelName = channelName;
					mMobileAP_joined = true;
					mMobileAP_channel_btn.Text = R.@string.leave_channel;
				}
			}

		}

		private void leaveTestChannel(int interfaceType)
		{

			/// <summary>
			///*************************************************
			/// 7. Leave channel
			/// **************************************************
			/// </summary>
			addLogView(interfaceType, "    leaveChannel()");

			SchordManager currentManager = null;
			currentManager = getSchordManager(interfaceType);

			try
			{
				switch (interfaceType)
				{
					case SchordManager.INTERFACE_TYPE_WIFI:
						currentManager.leaveChannel(mWifi_channelName);
						mWifi_joined = false;
						mWifi_channel_btn.Text = R.@string.join_channel;
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_P2P:
						currentManager.leaveChannel(mWifiDirect_channelName);
						mWifiDirect_joined = false;
						mWifiDirect_channel_btn.Text = R.@string.join_channel;
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_AP:
						currentManager.leaveChannel(mMobileAP_channelName);
						mMobileAP_joined = false;
						mMobileAP_channel_btn.Text = R.@string.join_channel;
						break;
				}
				mNodeListAdapter.removeNodeGroup(getInterfaceName(interfaceType));
				setJoinedNodeCount();

			}
			catch (Exception e)
			{
				addLogView(interfaceType, "    Fail to join -" + e.Message);
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

			/// <summary>
			/// Called when a node leave event is raised on the channel.
			/// </summary>
			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when a node join event is raised on the channel.
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[outerInstance.getInterfaceName(interfaceType) + fromNode] + ", " + new string(payload[0]) + ")");
				}
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
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
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the UdpFrameworkFragment.java
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string serviceType)
			{
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

			}

		}

		private SchordChannel.StatusListener mWifiDirect_ChannelListener = new StatusListenerAnonymousInnerClassHelper2();

		private class StatusListenerAnonymousInnerClassHelper2 : SchordChannel.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper2()
			{
			}


			internal int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_P2P;

			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[outerInstance.getInterfaceName(interfaceType) + fromNode] + ", " + new string(payload[0]) + ")");
				}
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
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
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the UdpFrameworkFragment.java
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string serviceType)
			{
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

			}

		}

		private SchordChannel.StatusListener mMobileAP_ChannelListener = new StatusListenerAnonymousInnerClassHelper3();

		private class StatusListenerAnonymousInnerClassHelper3 : SchordChannel.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper3()
			{
			}


			internal int interfaceType = SchordManager.INTERFACE_TYPE_WIFI_AP;

			public override void onNodeLeft(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(false, interfaceType, fromNode);
			}

			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[outerInstance.getInterfaceName(interfaceType) + fromNode] + ", " + new string(payload[0]) + ")");
				}
			}

			/// <summary>
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the SendFilesFragment.java
			/// </summary>
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
			/// The following callBacks are not used in this Fragment. Please refer
			/// to the UdpFrameworkFragment.java
			/// </summary>
			public override void onUdpDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload, string serviceType)
			{
			}

			public override void onUdpDataDelivered(string fromNode, string channelName, string reqId)
			{

			}

		}

		private void onNodeCallbackCommon(bool isJoin, int interfaceType, string fromNode)
		{

			string interfaceName = getInterfaceName(interfaceType);

			if (isJoin)
			{
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

				sendData(interfaceType, fromNode);
			}
			else
			{
				addLogView(interfaceType, "    > onNodeLeft(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");

				mNodeListAdapter.removeNode(interfaceName, fromNode);
			}

			setJoinedNodeCount();
		}

		private void sendData(int interfaceType, string toNode)
		{
			SchordChannel channel = null;
			sbyte[][] payload = new sbyte[1][];

			channel = getJoinedChannelByIfcType(interfaceType);

			if (channel == null)
			{
				addLogView(interfaceType, "    Fail to get the joined Channel");
				return;
			}

			bool isSecure = channel.SecureChannel;
			if (isSecure)
			{
				payload[0] = "Encrypted data".GetBytes();
			}
			else
			{
				payload[0] = "Non-encrypted data".GetBytes();
			}
			channel.sendData(toNode, CHORD_SAMPLE_MESSAGE_TYPE, payload);
			addLogView(interfaceType, "    sendData( Node " + mNodeNumberMap[getInterfaceName(interfaceType) + toNode] + ", " + new string(payload[0]) + ")");
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
			SchordChannel channel = null;
			SchordManager currentManager = null;

			currentManager = getSchordManager(ifcType);

			switch (ifcType)
			{
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

		private bool checkChannelName(string channelName, bool secure)
		{

			char c;
			if (channelName == null || channelName.Length == 0)
			{
				return true;
			}

			if (secure)
			{
				if (channelName[0] == '#' && channelName.Length == 1)
				{
					return false;
				}
				else
				{
					c = channelName[1];
				}
			}
			else
			{
				c = channelName[0];
			}

			if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z'))
			{
				return true;
			}
			else
			{
				return false;
			}

		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void showChannelInputDialog(final int interfaceType)
		private void showChannelInputDialog(int interfaceType)
		{
			// Dialog to input the name of channel to join.

			LinearLayout mlayout = new LinearLayout(this);
			LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
			mlayout.LayoutParams = lp;
			mlayout.Orientation = LinearLayout.VERTICAL;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.EditText input = new android.widget.EditText(this);
			EditText input = new EditText(this);
			LinearLayout.LayoutParams lp2 = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
			input.LayoutParams = lp2;
			input.Hint = CHORD_SECURE_TEST_CHANNEL;
			input.LongClickable = false;
			input.MaxHeight = 500;
			input.OnTouchListener = new OnTouchListenerAnonymousInnerClassHelper(this);

			// Set checkBox to use secure channel
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.CheckBox mCheck = new android.widget.CheckBox(this);
			CheckBox mCheck = new CheckBox(this);
			mCheck.Text = "Use Secure Channel";
			mCheck.TextColor = Color.BLACK;

			/// <summary>
			///*******************************************************************
			/// 5. If you want to use secured channel, add prefix to your channel
			/// name SECURE_PREFIX : Prefix to secured channel name
			/// ********************************************************************
			/// </summary>

			// Get keyboard inputs to add prefix to secured channel name
			input.addTextChangedListener(new TextWatcherAnonymousInnerClassHelper(this, input, mCheck));

			// Get Change of CheckBox to add prefix to secured channel name
			mCheck.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this, input);

			mlayout.addView(input);
			mlayout.addView(mCheck);

			AlertDialog alertDialog = (new AlertDialog.Builder(this, AlertDialog.THEME_HOLO_LIGHT)).setTitleuniquetempvar.setMessage(R.@string.input_channel_name).setView(mlayout).setPositiveButton(R.@string.ok, new OnClickListenerAnonymousInnerClassHelper(this, interfaceType, input, mCheck))
				   .setNegativeButton(R.@string.cancel, new OnClickListenerAnonymousInnerClassHelper2(this, input))
				   .create();
			alertDialog.show();
		}

		private class OnTouchListenerAnonymousInnerClassHelper : View.OnTouchListener
		{
			private readonly UseSecureChannelActivity outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper(UseSecureChannelActivity outerInstance)
			{
				this.outerInstance = outerInstance;
				lastTouchTime = -1;
			}

			private long lastTouchTime;

			public override bool onTouch(View arg0, MotionEvent arg1)
			{

				if (arg1.Action == MotionEvent.ACTION_DOWN)
				{

					long thisTime = DateTimeHelperClass.CurrentUnixTimeMillis();
					if (thisTime - lastTouchTime < 250)
					{
						// Double tap
						mGesture.onTouchEvent(arg1);
						lastTouchTime = -1;
						return true;
					}
					else
					{
						// Too slow
						lastTouchTime = thisTime;
					}
				}

				return false;
			}
		}

		private class TextWatcherAnonymousInnerClassHelper : TextWatcher
		{
			private readonly UseSecureChannelActivity outerInstance;

			private EditText input;
			private CheckBox mCheck;

			public TextWatcherAnonymousInnerClassHelper(UseSecureChannelActivity outerInstance, EditText input, CheckBox mCheck)
			{
				this.outerInstance = outerInstance;
				this.input = input;
				this.mCheck = mCheck;
			}


			public override void onTextChanged(CharSequence s, int start, int before, int count)
			{

				string str = input.Text.ToString();

				if (mCheck.Checked)
				{
					if (str.Length == 1 && str.IndexOf(SchordManager.SECURE_PREFIX) != 0)
					{
						str = SchordManager.SECURE_PREFIX + str;
						input.Text = str;
						input.Selection = input.length();
						if (!outerInstance.checkChannelName(str, mCheck.Checked))
						{
							Toast.makeText(ApplicationContext, "channelName should always begin with an alphanumeric character", Toast.LENGTH_SHORT).show();
						}

					}
					else if (str.Length > 1 && str.IndexOf(SchordManager.SECURE_PREFIX) != 0)
					{
						mCheck.Checked = false;
						input.Text = str;
						input.Selection = input.length();
					}
					else if (str.Length > 1 && !outerInstance.checkChannelName(str, mCheck.Checked))
					{
						Toast.makeText(ApplicationContext, "channelName should always begin with an alphanumeric character", Toast.LENGTH_SHORT).show();
					}
				}
				else
				{
					if (str.IndexOf(SchordManager.SECURE_PREFIX) == 0)
					{
						mCheck.Checked = true;
						input.Text = str;
						input.Selection = input.length();
					}
					else if (str.Length >= 1 && !outerInstance.checkChannelName(str, mCheck.Checked))
					{
						Toast.makeText(ApplicationContext, "channelName should always begin with an alphanumeric character", Toast.LENGTH_SHORT).show();
					}
				}

			}

			public override void beforeTextChanged(CharSequence s, int start, int count, int after)
			{

			}

			public override void afterTextChanged(Editable s)
			{

			}
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
		{
			private readonly UseSecureChannelActivity outerInstance;

			private EditText input;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(UseSecureChannelActivity outerInstance, EditText input)
			{
				this.outerInstance = outerInstance;
				this.input = input;
			}


			public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
			{

				string str = "";

				if (isChecked)
				{
					input.Hint = SchordManager.SECURE_PREFIX + CHORD_SECURE_TEST_CHANNEL;
					if (!(input.Text.ToString().Equals("")))
					{
						str = SchordManager.SECURE_PREFIX + input.Text.ToString();
					}
				}
				else
				{
					input.Hint = CHORD_SECURE_TEST_CHANNEL;
					if (!(input.Text.ToString().Equals("")))
					{
						str = input.Text.ToString().Substring(1);
					}
				}
				input.Text = str;
				input.Selection = input.length();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly UseSecureChannelActivity outerInstance;

			private int interfaceType;
			private EditText input;
			private CheckBox mCheck;

			public OnClickListenerAnonymousInnerClassHelper(UseSecureChannelActivity outerInstance, int interfaceType, EditText input, CheckBox mCheck)
			{
				this.outerInstance = outerInstance;
				this.interfaceType = interfaceType;
				this.input = input;
				this.mCheck = mCheck;
			}


			public override void onClick(DialogInterface arg0, int arg1)
			{
				/// <summary>
				/// [C] Join test channel
				/// </summary>
				if (outerInstance.checkChannelName(input.Text.ToString(), mCheck.Checked))
				{
					outerInstance.addLogView(interfaceType, "\n[B] Join test channel!");
					outerInstance.joinTestChannel(interfaceType, input.Text.ToString(), mCheck.Checked);

					InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
					imm.hideSoftInputFromWindow(input.WindowToken, 0);
				}
				else
				{
					Toast.makeText(ApplicationContext, "channelName should always begin with an alphanumeric character", Toast.LENGTH_SHORT).show();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly UseSecureChannelActivity outerInstance;

			private EditText input;

			public OnClickListenerAnonymousInnerClassHelper2(UseSecureChannelActivity outerInstance, EditText input)
			{
				this.outerInstance = outerInstance;
				this.input = input;
			}


			public override void onClick(DialogInterface arg0, int arg1)
			{
				InputMethodManager imm = (InputMethodManager) getSystemService(Context.INPUT_METHOD_SERVICE);
				imm.hideSoftInputFromWindow(input.WindowToken, 0);
			}
		}

		private GestureDetector mGesture = new GestureDetector(Application, new SimpleOnGestureListenerAnonymousInnerClassHelper());

		private class SimpleOnGestureListenerAnonymousInnerClassHelper : GestureDetector.SimpleOnGestureListener
		{
			public SimpleOnGestureListenerAnonymousInnerClassHelper()
			{
			}


			public override bool onDoubleTap(MotionEvent e)
			{
				return true;
			}

		}

		private const string CHORD_SECURE_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.SECURETESTCHANNEL";

		private const string CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.MESSAGE_TYPE";

		private SchordManager mSchordManager_1 = null;

		private SchordManager mSchordManager_2 = null;

		private SchordManager mSchordManager_3 = null;

		private TextView mWifi_state_view = null;

		private TextView mWifiDirect_state_view = null;

		private TextView mMobileAP_state_view = null;

		private Drawable mDrawableConnected = null;

		private Drawable mDrawableDisconnected = null;

		private bool mWifi_bStarted = false;

		private bool mWifiDirect_bStarted = false;

		private Button mWifi_channel_btn = null;

		private Button mWifiDirect_channel_btn = null;

		private Button mMobileAP_channel_btn = null;

		private bool mWifi_joined = false;

		private bool mWifiDirect_joined = false;

		private bool mMobileAP_joined = false;

		private bool mWifi_disconnected = false;

		private bool mWifiDirect_disconnected = false;

		private bool mMobileAP_disconnected = false;

		private string mWifi_channelName = null;

		private string mWifiDirect_channelName = null;

		private string mMobileAP_channelName = null;

		private TextView mMyNodeName_textView = null;

		private TextView mJoinedNodeList_textView = null;

		private NodeListAdapter mNodeListAdapter = null;

		private ExpandableListView mPeer_listView = null;

		private ChordLogView mLogView = null;

		private ChordLogView mWifiDirect_LogView = null;

		private ChordLogView mMobileAP_LogView = null;

		private Dictionary<string, int?> mNodeNumberMap = new Dictionary<string, int?>();

		private int mNodeNumber = 0;

		private SparseIntArray mInterfaceMap = new SparseIntArray();

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "UseSecureChannelFragment : ";

	}

}