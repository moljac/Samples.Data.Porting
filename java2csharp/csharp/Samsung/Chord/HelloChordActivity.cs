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

	using Activity = android.app.Activity;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using SparseIntArray = android.util.SparseIntArray;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class HelloChordActivity : Activity, View.OnClickListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.hello_chord_activity;

			mWifi_state_view = (TextView) findViewById(R.id.wifi_state_view);
			mWifiDirect_state_view = (TextView) findViewById(R.id.wifiDirect_state_view);
			mMobileAP_state_view = (TextView) findViewById(R.id.mobileAP_state_view);

			mDrawableConnected = Resources.getDrawable(R.drawable.ic_network_connected);
			mDrawableDisconnected = Resources.getDrawable(R.drawable.ic_network_disconnected);
			mDrawableConnected.setBounds(0, 0, mDrawableConnected.IntrinsicWidth, mDrawableConnected.IntrinsicHeight);
			mDrawableDisconnected.setBounds(0, 0, mDrawableDisconnected.IntrinsicWidth, mDrawableDisconnected.IntrinsicHeight);

			mWifi_startStop_btn = (Button) findViewById(R.id.start_stop_btn);
			mWifi_startStop_btn.OnClickListener = this;
			mWifi_startStop_btn.Enabled = false;
			mWifiDirect_startStop_btn = (Button) findViewById(R.id.wifiDirect_start_stop_btn);
			mWifiDirect_startStop_btn.OnClickListener = this;
			mWifiDirect_startStop_btn.Enabled = false;
			mMobileAP_startStop_btn = (Button) findViewById(R.id.mobileAP_start_stop_btn);
			mMobileAP_startStop_btn.OnClickListener = this;
			mMobileAP_startStop_btn.Enabled = false;

			mMyNodeName_textView = (TextView) findViewById(R.id.myNodeName_textView);
			mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");

			mLogView = (ChordLogView) findViewById(R.id.log_textView);
			mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
			mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);
		}

		public override void onResume()
		{
			base.onResume();

			/// <summary>
			/// [A] Initialize Chord!
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

			base.onDestroy();
		}

		public override void onClick(View v)
		{

			bool bStarted = false;
			int ifc = -1;

			switch (v.Id)
			{
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

			if (!bStarted)
			{
				/// <summary>
				/// [B] Start Chord
				/// </summary>
				addLogView(ifc, "\n[A] Start Chord!");
				startChord(ifc);
			}
			else
			{
				/// <summary>
				/// [C] Stop Chord
				/// </summary>
				addLogView(ifc, "\n[B] Stop Chord!");
				stopChord(ifc);
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

			Log.d(TAG, TAGClass + "initChord : VersionName( " + chord.VersionName + " ), VerionCode( " + chord.VersionCode + " )");

			mSchordManager_1 = new SchordManager(this);

			/// <summary>
			///**************************************************
			/// 2. Set some values before start If you want to use secured channel,
			/// you should enable SecureMode. Please refer
			/// UseSecureChannelFragment.java mChordManager.enableSecureMode(true);
			/// Once you will use sendFile or sendMultiFiles, you have to call
			/// setTempDirectory mChordManager.setTempDirectory(Environment.
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
				refreshInterfaceStatus(ifc.Value, true);
			}

		}

		private class NetworkListenerAnonymousInnerClassHelper : SchordManager.NetworkListener
		{
			private readonly HelloChordActivity outerInstance;

			public NetworkListenerAnonymousInnerClassHelper(HelloChordActivity outerInstance)
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
					mWifi_startStop_btn.Enabled = false;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
				{
					mWifiDirect_state_view.Text = R.@string.wifi_direct_off;
					mWifiDirect_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
					mWifiDirect_startStop_btn.Enabled = false;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
				{
					mMobileAP_state_view.Text = R.@string.mobile_ap_off;
					mMobileAP_state_view.setCompoundDrawables(mDrawableDisconnected, null, null, null);
					mMobileAP_startStop_btn.Enabled = false;

				}
			}
			else
			{
				if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
				{
					mWifi_state_view.Text = R.@string.wifi_on;
					mWifi_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
					mWifi_startStop_btn.Enabled = true;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
				{
					mWifiDirect_state_view.Text = R.@string.wifi_direct_on;
					mWifiDirect_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
					mWifiDirect_startStop_btn.Enabled = true;

				}
				else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
				{
					mMobileAP_state_view.Text = R.@string.mobile_ap_on;
					mMobileAP_state_view.setCompoundDrawables(mDrawableConnected, null, null, null);
					mMobileAP_startStop_btn.Enabled = true;
				}
			}

		}

		private void startChord(int interfaceType)
		{
			/// <summary>
			/// 3. Start Chord using the selected interface in the list of available
			/// interfaces. You can get a list of available network interface types
			/// List<Integer> infList =
			/// mSchordManager_1.getAvailableInterfaceTypes().isEmpty();
			/// if(infList.isEmpty()) // there is no active interface!
			/// </summary>
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
						mWifi_startStop_btn.Enabled = false;
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_P2P:
						startManager.start(interfaceType, mWifiDirect_ManagerListener);
						mWifiDirect_startStop_btn.Enabled = false;
						break;
					case SchordManager.INTERFACE_TYPE_WIFI_AP:
						startManager.start(interfaceType, mMobileAP_ManagerListener);
						mMobileAP_startStop_btn.Enabled = false;
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

				outerInstance.mWifi_startStop_btn.Text = R.@string.stop;
				outerInstance.mWifi_startStop_btn.Enabled = true;

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

			public override void onStopped(int reason)
			{
				/// <summary>
				/// 8. Chord has stopped successfully
				/// </summary>
				outerInstance.mWifi_bStarted = false;

				if (!outerInstance.mWifiDirect_bStarted)
				{
					outerInstance.mMyNodeName_textView.Text = "";
					outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");
				}

				outerInstance.mWifi_startStop_btn.Text = R.@string.start;

				if (STOPPED_BY_USER == reason)
				{
					// Success to stop by calling stop() method
					outerInstance.mLogView.appendLog("    > onStopped(STOPPED_BY_USER)");
					outerInstance.mWifi_startStop_btn.Enabled = true;

				}
				else if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mWifi_startStop_btn.Enabled = false;
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

				outerInstance.mWifiDirect_startStop_btn.Text = R.@string.stop;
				outerInstance.mWifiDirect_startStop_btn.Enabled = true;

				if (reason == STARTED_BY_USER)
				{
					// Success to start by calling start() method
					outerInstance.mWifiDirect_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
					outerInstance.joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_P2P);
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					// Re-start by network re-connection.
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

				outerInstance.mWifiDirect_startStop_btn.Text = R.@string.start;

				if (STOPPED_BY_USER == reason)
				{
					// Success to stop by calling stop() method
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(STOPPED_BY_USER)");
					outerInstance.mWifiDirect_startStop_btn.Enabled = true;

				}
				else if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mWifiDirect_startStop_btn.Enabled = false;
				}
			}
		}

		private SchordManager.StatusListener mMobileAP_ManagerListener = new StatusListenerAnonymousInnerClassHelper3();

		private class StatusListenerAnonymousInnerClassHelper3 : SchordManager.StatusListener
		{
			public StatusListenerAnonymousInnerClassHelper3()
			{
			}


			public override void onStarted(string nodeName, int reason)
			{
				outerInstance.mMobileAP_bStarted = true;

				outerInstance.mMobileAP_LogView.Visibility = View.VISIBLE;
				outerInstance.mMyNodeName_textView.Text = getString(R.@string.my_node_name, nodeName);

				outerInstance.mMobileAP_startStop_btn.Text = R.@string.stop;
				outerInstance.mMobileAP_startStop_btn.Enabled = true;

				if (reason == STARTED_BY_USER)
				{
					// Success to start by calling start() method
					outerInstance.mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_USER)");
					outerInstance.joinTestChannel(SchordManager.INTERFACE_TYPE_WIFI_AP);
				}
				else if (reason == STARTED_BY_RECONNECTION)
				{
					// Re-start by network re-connection.
					outerInstance.mMobileAP_LogView.appendLog("    > onStarted(STARTED_BY_RECONNECTION)");
				}

			}

			public override void onStopped(int reason)
			{
				outerInstance.mMobileAP_bStarted = false;

				outerInstance.mMyNodeName_textView.Text = "";
				outerInstance.mMyNodeName_textView.Hint = getString(R.@string.my_node_name, " ");

				outerInstance.mMobileAP_startStop_btn.Text = R.@string.start;

				if (STOPPED_BY_USER == reason)
				{
					// Success to stop by calling stop() method
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(STOPPED_BY_USER)");
					outerInstance.mMobileAP_startStop_btn.Enabled = true;

				}
				else if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
					outerInstance.mMobileAP_startStop_btn.Enabled = false;
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
					channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL, mWifi_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_P2P:
					channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL, mWifiDirect_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_AP:
					channel = currentManager.joinChannel(CHORD_HELLO_TEST_CHANNEL, mMobileAP_ChannelListener);
					break;
			}

			if (channel == null)
			{
				addLogView(interfaceType, "    Fail to joinChannel");
			}

		}

		private void stopChord(int ifc)
		{
			/// <summary>
			/// 7. Stop Chord. You can call leaveChannel explicitly.
			/// mSchordManager_1.leaveChannel(CHORD_HELLO_TEST_CHANNEL);
			/// </summary>
			SchordManager currentManager = null;

			currentManager = getSchordManager(ifc);

			if (currentManager == null)
			{
				return;
			}

			currentManager.stop();

			switch (ifc)
			{
				case SchordManager.INTERFACE_TYPE_WIFI:
					mWifi_startStop_btn.Enabled = false;
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_P2P:
					mWifiDirect_startStop_btn.Enabled = false;
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_AP:
					mMobileAP_startStop_btn.Enabled = false;
					break;
			}
			addLogView(ifc, "    stop(" + getInterfaceName(ifc) + ")");

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
			/// Called when the data message received from the node.
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				/// <summary>
				/// 6. Received data from other node
				/// </summary>
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + new string(payload[0]) + ")");
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
			/// Called when the data message received from the node.
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				/// <summary>
				/// 6. Received data from other node
				/// </summary>
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + new string(payload[0]) + ")");
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
			/// Called when the data message received from the node.
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
				/// <summary>
				/// 6. Received data from other node
				/// </summary>
				if (payloadType.Equals(CHORD_SAMPLE_MESSAGE_TYPE))
				{
					outerInstance.addLogView(interfaceType, "    > onDataReceived( Node " + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + new string(payload[0]) + ")");
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
					addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");
				}
				else
				{
					mNodeNumber++;
					mNodeNumberMap[interfaceName + fromNode] = mNodeNumber;
					addLogView(interfaceType, "    > onNodeJoined(Node" + mNodeNumber + " : " + fromNode + ")");
				}

				/// <summary>
				/// 6. Send data to joined node
				/// </summary>
				sbyte[][] payload = new sbyte[1][];
				payload[0] = "Hello chord!".GetBytes();

				SchordChannel channel = getJoinedChannelByIfcType(interfaceType);

				if (channel == null)
				{
					addLogView(interfaceType, "    Fail to get the joined Channel");
					return;
				}

				try
				{
					channel.sendData(fromNode, CHORD_SAMPLE_MESSAGE_TYPE, payload);
				}
				catch (Exception e)
				{
					addLogView(interfaceType, "    " + e.Message);
					return;
				}

				addLogView(interfaceType, "    sendData( Node " + mNodeNumberMap[interfaceName + fromNode] + ", " + new string(payload[0]) + ")");

			}
			else
			{
				addLogView(interfaceType, "    > onNodeLeft(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");

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
			int managerIndex = 0;
			SchordChannel channel = null;

			managerIndex = mInterfaceMap.get(ifcType);

			switch (managerIndex)
			{
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

		private const string CHORD_HELLO_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.HELLOTESTCHANNEL";

		private const string CHORD_SAMPLE_MESSAGE_TYPE = "com.samsung.android.sdk.chord.example.MESSAGE_TYPE";

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

		private bool mWifi_bStarted = false;

		private bool mWifiDirect_bStarted = false;

		private bool mMobileAP_bStarted = false;

		private TextView mMyNodeName_textView = null;

		private ChordLogView mLogView = null;

		private ChordLogView mWifiDirect_LogView = null;

		private ChordLogView mMobileAP_LogView = null;

		private Dictionary<string, int?> mNodeNumberMap = new Dictionary<string, int?>();

		private int mNodeNumber = 0;

		private SparseIntArray mInterfaceMap = new SparseIntArray();

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "HelloChordFragment : ";

	}

}