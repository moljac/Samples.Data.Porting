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
	using IFileCancelListener = com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.IFileCancelListener;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using StatFs = android.os.StatFs;
	using Log = android.util.Log;
	using SparseIntArray = android.util.SparseIntArray;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using OnItemSelectedListener = android.widget.AdapterView.OnItemSelectedListener;
	using Button = android.widget.Button;
	using ExpandableListView = android.widget.ExpandableListView;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class SendFilesActivity : Activity, NodeListAdapter.IFileCancelListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.send_files_activity;

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
			mNodeListAdapter = new NodeListAdapter(BaseContext, this);
			mNodeListAdapter.SecureChannelFrag = false;
			mNode_listView = (ExpandableListView) findViewById(R.id.node_listView);
			mNode_listView.Adapter = mNodeListAdapter;

			mSend_btn = (Button) findViewById(R.id.send_btn);
			mSend_version_spinner = (Spinner) findViewById(R.id.send_version_spinner);
			mSend_version_spinner.Selection = SEND_FILE;
			mMultifiles_limitCnt_spinner = (Spinner) findViewById(R.id.multifiles_limitCnt_spinner);
			mMultifiles_limitCnt_spinner.Selection = 0;

			mLogView = (ChordLogView) findViewById(R.id.log_textView);
			mWifiDirect_LogView = (ChordLogView) findViewById(R.id.wifiDirect_log_textView);
			mMobileAP_LogView = (ChordLogView) findViewById(R.id.mobileAP_log_textView);

			mSend_btn.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			// The spinner to set the version for sending a file
			mSend_version_spinner.OnItemSelectedListener = new OnItemSelectedListenerAnonymousInnerClassHelper(this);

			// The spinner to set the limit count for sending files simultaneously
			mMultifiles_limitCnt_spinner.OnItemSelectedListener = new OnItemSelectedListenerAnonymousInnerClassHelper2(this);

			mAlertDialogMap = new Dictionary<string, AlertDialog>();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SendFilesActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(SendFilesActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View arg0)
			{

				if (outerInstance.mNodeListAdapter.CheckedNodeList.Count == 0)
				{
					Toast.makeText(ApplicationContext, "Please select at least one node", Toast.LENGTH_SHORT).show();
					return;
				}

				// Call the activity for selecting files.
				Intent mIntent = new Intent(outerInstance, typeof(FileSelectActivity));
				startActivityForResult(mIntent, 0);
			}
		}

		private class OnItemSelectedListenerAnonymousInnerClassHelper : AdapterView.OnItemSelectedListener
		{
			private readonly SendFilesActivity outerInstance;

			public OnItemSelectedListenerAnonymousInnerClassHelper(SendFilesActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
			{
				if (parent.getItemAtPosition(position).ToString().Equals("sendFile"))
				{
					outerInstance.mSend_api = SEND_FILE;
					outerInstance.mMultifiles_limitCnt_spinner.Selection = 0;
					outerInstance.mMultifiles_limitCnt_spinner.Enabled = false;
				}
				else
				{
					outerInstance.mSend_api = SEND_MULTI_FILES;
					outerInstance.mMultifiles_limitCnt_spinner.Enabled = true;
				}
			}

			public override void onNothingSelected<T1>(AdapterView<T1> arg0)
			{

			}

		}

		private class OnItemSelectedListenerAnonymousInnerClassHelper2 : AdapterView.OnItemSelectedListener
		{
			private readonly SendFilesActivity outerInstance;

			public OnItemSelectedListenerAnonymousInnerClassHelper2(SendFilesActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
			{
				int multfiles_limitCnt = Convert.ToInt32(parent.getItemAtPosition(position).ToString());

				if (outerInstance.mSchordManager_1 != null)
				{
					outerInstance.mSchordManager_1.SendMultiFilesLimitCount = multfiles_limitCnt;
				}

				if (outerInstance.mSchordManager_2 != null)
				{
					outerInstance.mSchordManager_2.SendMultiFilesLimitCount = multfiles_limitCnt;
				}
			}

			public override void onNothingSelected<T1>(AdapterView<T1> arg0)
			{
			}
		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);

			// After selecting files to send
			switch (resultCode)
			{
				case Activity.RESULT_OK:
					List<string> fileList = data.getStringArrayListExtra("SELECTED_FILE");
					string trId = null;
					string[][] checkedNodeInfo = new string[1][];
					string toNode = null;
					string interfaceName = null;
					int interfaceType = -1;
					SchordChannel channel = null;

					for (int i = 0; i < mNodeListAdapter.CheckedNodeList.Count; i++)
					{

						// Get the list of checked nodes to send files.
						checkedNodeInfo = mNodeListAdapter.CheckedNodeList[i];
						interfaceName = checkedNodeInfo[i][0];
						toNode = checkedNodeInfo[i][1];

						interfaceType = getInterfaceType(interfaceName);
						channel = getJoinedChannelByIfcType(interfaceType);
						if (channel == null)
						{
							addLogView(interfaceType, "    Fail to get the joined Channel");
							return;
						}

						if (fileList.Count == 0)
						{
							Toast.makeText(this, "Please select at least one file.", Toast.LENGTH_SHORT).show();
							return;
						}

						if (mSend_api == SEND_FILE)
						{
							if (fileList.Count > 1)
							{
								Toast.makeText(this, "Don't select more than one file.", Toast.LENGTH_SHORT).show();
								return;
							}

							try
							{
								/// <summary>
								/// 6. Send a file to the selected node
								/// </summary>
								trId = channel.sendFile(toNode, MESSAGE_TYPE_FILE_NOTIFICATION, fileList[0], SHARE_FILE_TIMEOUT_MILISECONDS);
							}
							catch (FileNotFoundException e)
							{
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}
							catch (System.ArgumentException e)
							{
								addLogView(interfaceType, "	Fail to send file	:" + e.Message);
							}

							// Set the total count of files to send. (set to 1)
							mNodeListAdapter.setFileTotalCnt(interfaceName, toNode, 1, trId);

						}
						else if (mSend_api == SEND_MULTI_FILES)
						{
							/// <summary>
							/// 6. Send multiFile to the selected node
							/// </summary>

							try
							{
								trId = channel.sendMultiFiles(toNode, MESSAGE_TYPE_FILE_NOTIFICATION, fileList, SHARE_FILE_TIMEOUT_MILISECONDS);
							}
							catch (FileNotFoundException e)
							{
								Console.WriteLine(e.ToString());
								Console.Write(e.StackTrace);
							}
							catch (System.ArgumentException e)
							{
								addLogView(interfaceType, "	Fail to send multifiles	:" + e.Message);
							}

							// Set the total count of files to send.
							mNodeListAdapter.setFileTotalCnt(interfaceName, toNode, fileList.Count, trId);
						}

					}

					if (null == trId)
					{ // failed to send
						Toast.makeText(this, getString(R.@string.sending_ps_failed, fileList.Count), Toast.LENGTH_SHORT).show();
					}
					else
					{ // succeed to send

						for (int i = 0; i < mNodeListAdapter.CheckedNodeList.Count; i++)
						{
							checkedNodeInfo = mNodeListAdapter.CheckedNodeList[i];
							interfaceType = getInterfaceType(checkedNodeInfo[i][0]);

							if (mSend_api == SEND_FILE)
							{
								addLogView(getInterfaceType(checkedNodeInfo[i][0]), "    sendFile() : to Node" + mNodeNumberMap[checkedNodeInfo[i][0] + checkedNodeInfo[i][1]]);
							}
							else if (mSend_api == SEND_MULTI_FILES)
							{
								addLogView(getInterfaceType(checkedNodeInfo[i][0]), "    sendMultiFiles() : to Node" + mNodeNumberMap[checkedNodeInfo[i][0] + checkedNodeInfo[i][1]] + ", " + fileList.Count + "files");
							}
						}

					}

					break;
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

			mAlertDialogMap.Clear();
			mNodeNumberMap.Clear();
			mInterfaceMap.clear();

			base.onDestroy();
		}

		// **********************************************************************
		// From adapter
		// **********************************************************************
		public virtual void onFileCanceled(string interfaceName, string node, string trId, int index, bool bMulti)
		{

			int interfaceType = getInterfaceType(interfaceName);

			if (interfaceType != -1)
			{
				SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
				if (channel == null)
				{
					addLogView(interfaceType, "    Fail to get the joined Channel");
					return;
				}

				if (bMulti)
				{
					/// <summary>
					/// 7. Cancel the multiFile transfer
					/// </summary>
					channel.cancelMultiFiles(trId);
					addLogView(interfaceType, "    cancelMultiFiles()");
					mNodeListAdapter.removeCanceledProgress(interfaceName, node, trId);
				}
				else
				{
					/// <summary>
					/// 7. Cancel the file transfer
					/// </summary>
					channel.cancelFile(trId);
					addLogView(interfaceType, "    cancelFile()");
					mNodeListAdapter.removeProgress(index, trId);
				}
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

			/// <summary>
			///**************************************************
			/// 2. Set some values before start. It is recommended to use the
			/// application's name and an internal storage of each application as a
			/// directory path. If you want to use secured channel, you should enable
			/// SecureMode. Please refer UseSecureChannelFragment.java
			/// mSchordManager_1.enableSecureMode(true);
			/// ***************************************************
			/// </summary>
			mSchordManager_1.Looper = MainLooper;
			mSchordManager_1.TempDirectory = chordFilePath;

			/// <summary>
			/// Optional. If you need listening network changed, you can set callback
			/// before starting chord.
			/// </summary>
			mSchordManager_1.NetworkListener = new NetworkListenerAnonymousInnerClassHelper(this);

			IList<int?> ifcList = mSchordManager_1.AvailableInterfaceTypes;
			foreach (int? ifc in ifcList)
			{
				refreshInterfaceStatus(ifc.Value, true);
				startChord(ifc.Value);
			}

		}

		private class NetworkListenerAnonymousInnerClassHelper : SchordManager.NetworkListener
		{
			private readonly SendFilesActivity outerInstance;

			public NetworkListenerAnonymousInnerClassHelper(SendFilesActivity outerInstance)
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
			/// 3. Start Chord using the interface in the list of available
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
				startManager.TempDirectory = chordFilePath;

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

				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					// Stopped by network disconnected
					outerInstance.mWifi_disconnected = true;
					outerInstance.mLogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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

				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_P2P));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mWifiDirect_disconnected = true;
					outerInstance.mWifiDirect_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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
				outerInstance.mNodeListAdapter.removeNodeGroup(outerInstance.getInterfaceName(SchordManager.INTERFACE_TYPE_WIFI_AP));
				outerInstance.setJoinedNodeCount();

				if (NETWORK_DISCONNECTED == reason)
				{
					outerInstance.mMobileAP_disconnected = true;
					outerInstance.mMobileAP_LogView.appendLog("    > onStopped(NETWORK_DISCONNECTED)");
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
					channel = currentManager.joinChannel(CHORD_SEND_TEST_CHANNEL, mWifi_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_P2P:
					channel = currentManager.joinChannel(CHORD_SEND_TEST_CHANNEL, mWifiDirect_ChannelListener);
					break;
				case SchordManager.INTERFACE_TYPE_WIFI_AP:
					channel = currentManager.joinChannel(CHORD_SEND_TEST_CHANNEL, mMobileAP_ChannelListener);
					break;
			}

			if (channel == null)
			{
				addLogView(interfaceType, "    Fail to joinChannel");
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
			/// Called when a node join event is raised on the channel.
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

				outerInstance.onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId, totalCount, fileSize);
			}

			/// <summary>
			/// Called when the sending a file is completed.
			/// </summary>
			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", file" + index);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", file" + index);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the file transfer is finished to the node.
			/// </summary>
			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

				outerInstance.onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// </summary>
			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

				outerInstance.onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress, true, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress, true, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

				outerInstance.onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId, fileSize);
			}

			/// <summary>
			/// Called when the file transfer is completed to the node.
			/// </summary>
			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

				outerInstance.addLogView(interfaceType, "    > onFileSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", " + (new System.IO.FileInfo(fileName)).Name);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onFileReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + fileName);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// </summary>
			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

				outerInstance.onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress, false, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress, false, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// The following callBack is not used in this Fragment. Please refer to
			/// the HelloChordFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
			}

			/// <summary>
			/// The following callBack is not used in this Fragment. Please refer to
			/// the UdpFrameworkFragment.java
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
			/// Called when a node join event is raised on the channel.
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

				outerInstance.onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId, totalCount, fileSize);
			}

			/// <summary>
			/// Called when the sending a file is completed.
			/// </summary>
			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", file" + index);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", file" + index);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the file transfer is finished to the node.
			/// </summary>
			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

				outerInstance.onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// 
			/// </summary>
			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

				outerInstance.onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress, true, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress, true, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

				outerInstance.onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId, fileSize);
			}

			/// <summary>
			/// Called when the file transfer is completed to the node.
			/// </summary>
			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

				outerInstance.addLogView(interfaceType, "    > onFileSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", " + (new System.IO.FileInfo(fileName)).Name);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onFileReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + fileName);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// 
			/// </summary>
			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

				outerInstance.onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress, false, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress, false, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// The following callBack is not used in this Fragment. Please refer to
			/// the HelloChordFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
			{
			}

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
			/// Called when a node join event is raised on the channel.
			/// </summary>
			public override void onNodeJoined(string fromNode, string fromChannel)
			{
				outerInstance.onNodeCallbackCommon(true, interfaceType, fromNode);
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onMultiFilesWillReceive(string fromNode, string fromChannel, string fileName, string taskId, int totalCount, string fileType, long fileSize)
			{

				outerInstance.onMultiFilesWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, taskId, totalCount, fileSize);
			}

			/// <summary>
			/// Called when the sending a file is completed.
			/// </summary>
			public override void onMultiFilesSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", file" + index);
				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onMultiFilesReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", file" + index);

				outerInstance.mNodeListAdapter.removeProgress(index, taskId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the file transfer is finished to the node.
			/// </summary>
			public override void onMultiFilesFinished(string node, string channel, string taskId, int reason)
			{

				outerInstance.onMultiFilesFinishedCommon(interfaceType, interfaceName, node, taskId, reason);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// 
			/// </summary>
			public override void onMultiFilesFailed(string node, string channel, string fileName, string taskId, int index, int reason)
			{

				outerInstance.onMultiFilesFailedCommon(interfaceType, interfaceName, node, fileName, taskId, index, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onMultiFilesChunkSent(string toNode, string toChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, taskId, index, progress, true, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the receiving a file is completed from the node.
			/// </summary>
			public override void onMultiFilesChunkReceived(string fromNode, string fromChannel, string fileName, string taskId, int index, string fileType, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, taskId, index, progress, true, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the Share file notification is received. User can decide
			/// to receive or reject the file.
			/// </summary>
			public override void onFileWillReceive(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize)
			{

				outerInstance.onFileWillReceiveCommon(interfaceType, interfaceName, fromNode, fileName, exchangeId, fileSize);
			}

			/// <summary>
			/// Called when the file transfer is completed to the node.
			/// </summary>
			public override void onFileSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId)
			{

				outerInstance.addLogView(interfaceType, "    > onFileSent()  :  to Node" + outerInstance.mNodeNumberMap[interfaceName + toNode] + ", " + (new System.IO.FileInfo(fileName)).Name);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, string tmpFilePath)
			{

				outerInstance.addLogView(interfaceType, "    > onFileReceived()  :  from Node" + outerInstance.mNodeNumberMap[interfaceName + fromNode] + ", " + fileName);
				outerInstance.mNodeListAdapter.removeProgress(1, exchangeId);
				outerInstance.saveFile(fileName, tmpFilePath);
			}

			/// <summary>
			/// Called when the error is occurred while the file transfer is in
			/// progress.
			/// 
			/// </summary>
			public override void onFileFailed(string node, string channel, string fileName, string hash, string exchangeId, int reason)
			{

				outerInstance.onFileFailedCommon(interfaceType, interfaceName, node, fileName, exchangeId, reason);
			}

			/// <summary>
			/// Called when an individual chunk of the file is sent.
			/// </summary>
			public override void onFileChunkSent(string toNode, string toChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset, long chunkSize)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, toNode, exchangeId, 1, progress, false, true);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// Called when the file transfer is completed from the node.
			/// </summary>
			public override void onFileChunkReceived(string fromNode, string fromChannel, string fileName, string hash, string fileType, string exchangeId, long fileSize, long offset)
			{

				// Set the progressBar - add or update
				int progress = (int)(100 * offset / fileSize);
				outerInstance.mNodeListAdapter.setProgressUpdate(interfaceName, fromNode, exchangeId, 1, progress, false, false);
				outerInstance.expandeGroup();
			}

			/// <summary>
			/// The following callBack is not used in this Fragment. Please refer to
			/// the HelloChordFragment.java
			/// </summary>
			public override void onDataReceived(string fromNode, string fromChannel, string payloadType, sbyte[][] payload)
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
			}
			else
			{
				addLogView(interfaceType, "    > onNodeLeft(Node" + mNodeNumberMap[interfaceName + fromNode] + " : " + fromNode + ")");

				mNodeListAdapter.removeNode(interfaceName, fromNode);
			}

			setJoinedNodeCount();
		}

		private void onMultiFilesWillReceiveCommon(int interfaceType, string interfaceName, string fromNode, string fileName, string taskId, int totalCount, long fileSize)
		{

			addLogView(interfaceType, "    > onMultiFilesWillReceive()  :  from Node" + mNodeNumberMap[interfaceName + fromNode] + ", " + totalCount + "files");

			if (checkAvailableMemory(fileSize))
			{
				displayFileNotify(interfaceType, fromNode, getString(R.@string.file_ps_total_pd, fileName, totalCount), taskId, SEND_MULTI_FILES);

				// Set the total count of files to receive.
				mNodeListAdapter.setFileTotalCnt(interfaceName, fromNode, totalCount, taskId);
			}
			else
			{
				/// <summary>
				/// Because the external storage may be unavailable, you should
				/// verify that the volume is available before accessing it. But
				/// also, onMultiFilesFailed with ERROR_FILE_SEND_FAILED will be
				/// called while Chord got failed to write file.
				/// 
				/// </summary>

				SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
				channel.rejectMultiFiles(taskId);

				addLogView(interfaceType, "    > onMultiFilesWillReceive()\n     : There is not enough storage available. Reject receiving from Node" + mNodeNumberMap[interfaceName + fromNode]);
			}

		}

		private void onMultiFilesFinishedCommon(int interfaceType, string interfaceName, string node, string taskId, int reason)
		{

			string delimiter = "_";
			string sentOrReceived = null;

			if (taskId.Split(delimiter, true)[0].Equals(node))
			{
				sentOrReceived = "from";
			}
			else
			{
				sentOrReceived = "to";
			}

			addLogView(interfaceType, "    **************************************************");
			switch (reason)
			{
				case SchordChannel.StatusListener.ERROR_FILE_REJECTED:
				{
					addLogView(interfaceType, "    > onMultiFilesFinished()  :  REJECTED by Node" + mNodeNumberMap[interfaceName + node]);
					break;
				}
				case SchordChannel.StatusListener.ERROR_FILE_CANCELED:
				{
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
					addLogView(interfaceType, "    > onMultiFilesFinished()  :  " + sentOrReceived + " Node" + mNodeNumberMap[interfaceName + node]);
					break;
				default:

					addLogView(interfaceType, "    > onMultiFilesFinished()  :  Error[" + getErrorName(reason) + ":" + reason + "]");
					break;
			}
			addLogView(interfaceType, "    **************************************************\n");

		}

		private void onMultiFilesFailedCommon(int interfaceType, string interfaceName, string node, string fileName, string taskId, int index, int reason)
		{

			addLogView(interfaceType, "    **************************************************");
			switch (reason)
			{
				case SchordChannel.StatusListener.ERROR_FILE_REJECTED:
				{
					addLogView(interfaceType, "    > onMultiFilesFailed()  :  REJECTED by Node" + mNodeNumberMap[interfaceName + node]);
					break;
				}
				case SchordChannel.StatusListener.ERROR_FILE_CANCELED:
				{
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
				case SchordChannel.StatusListener.ERROR_FILE_TIMEOUT:
				{
					addLogView(interfaceType, "    > onMultiFilesFailed()  :  TIME OUT - Node" + mNodeNumberMap[interfaceName + node] + ", " + fileName);
					break;
				}
				default:
					addLogView(interfaceType, "    > onMultiFilesFailed()  :   Error[" + getErrorName(reason) + ":" + reason + "]");
					break;
			}
			addLogView(interfaceType, "    **************************************************\n");

			mNodeListAdapter.removeProgress(index, taskId);
			AlertDialog alertDialog = mAlertDialogMap[taskId];
			if (alertDialog != null)
			{
				alertDialog.dismiss();
				mAlertDialogMap.Remove(taskId);
			}

		}

		private void onFileWillReceiveCommon(int interfaceType, string interfaceName, string fromNode, string fileName, string exchangeId, long fileSize)
		{

			addLogView(interfaceType, "    > onFileWillReceive()  :  from Node" + mNodeNumberMap[interfaceName + fromNode] + ", " + fileName);

			if (checkAvailableMemory(fileSize))
			{
				displayFileNotify(interfaceType, fromNode, fileName, exchangeId, SEND_FILE);

				// Set the total count of files to receive.
				mNodeListAdapter.setFileTotalCnt(interfaceName, fromNode, 1, exchangeId);
			}
			else
			{
				/// <summary>
				/// Because the external storage may be unavailable, you should
				/// verify that the volume is available before accessing it. But
				/// also, onFileFailed with ERROR_FILE_SEND_FAILED will be called
				/// while Chord got failed to write file.
				/// 
				/// </summary>
				SchordChannel channel = getJoinedChannelByIfcType(interfaceType);
				channel.rejectFile(exchangeId);
				addLogView(interfaceType, "    > onFileWillReceive()\n     : There is not enough storage available. Reject receiving from Node" + mNodeNumberMap[interfaceName + fromNode]);
			}
		}

		private void onFileFailedCommon(int interfaceType, string interfaceName, string node, string fileName, string exchangeId, int reason)
		{

			addLogView(interfaceType, "    ******************************************");
			switch (reason)
			{
				case SchordChannel.StatusListener.ERROR_FILE_REJECTED:
				{
					addLogView(interfaceType, "    > onFileFailed()  :  REJECTED by Node" + mNodeNumberMap[interfaceName + node]);
					break;
				}
				case SchordChannel.StatusListener.ERROR_FILE_CANCELED:
				{
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
				case SchordChannel.StatusListener.ERROR_FILE_TIMEOUT:
				{
					addLogView(interfaceType, "    > onFileFailed()  :  TIME OUT " + fileName);
					break;
				}
				default:
					addLogView(interfaceType, "    > onFileFailed()  :  Error[" + reason + "] - Node" + mNodeNumberMap[interfaceName + node] + ", " + fileName);
					break;
			}
			addLogView(interfaceType, "    ******************************************\n");

			mNodeListAdapter.removeProgress(1, exchangeId);

			AlertDialog alertDialog = mAlertDialogMap[exchangeId];
			if (alertDialog != null)
			{
				alertDialog.dismiss();
				mAlertDialogMap.Remove(exchangeId);
			}

		}

		private string getErrorName(int errorType)
		{
			if (errorType == SchordChannel.StatusListener.ERROR_FILE_SEND_FAILED)
			{
				return "ERROR_FILE_SEND_FAILED";
			}
			else if (errorType == SchordChannel.StatusListener.ERROR_FILE_CREATE_FAILED)
			{
				return "ERROR_FILE_CREATE_FAILED";
			}
			else if (errorType == SchordChannel.StatusListener.ERROR_FILE_NO_RESOURCE)
			{
				return "ERROR_FILE_NO_RESOURCE";
			}

			return "UNKNOWN";
		}

		private string getInterfaceName(int interfaceType)
		{
			if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI)
			{
				return "Wi-Fi";
			}
			else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_AP)
			{
				return "Mobile AP";
			}
			else if (interfaceType == SchordManager.INTERFACE_TYPE_WIFI_P2P)
			{
				return "Wi-Fi Direct";
			}

			return "UNKNOWN";
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

		private bool checkAvailableMemory(long fileSize)
		{
			File targetdir = new File(chordFilePath);
			if (!targetdir.exists())
			{
				targetdir.mkdirs();
			}

			StatFs stat = new StatFs(chordFilePath);
			long blockSize = stat.BlockSize;
			long totalBlocks = stat.AvailableBlocks;
			long availableMemory = blockSize * totalBlocks;

			if (availableMemory < fileSize)
			{
				return false;
			}

			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void displayFileNotify(final int ifc, final String nodeName, final String fileName, final String trId, final int sendApi)
		private void displayFileNotify(int ifc, string nodeName, string fileName, string trId, int sendApi)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.samsung.android.sdk.chord.SchordChannel channel = getJoinedChannelByIfcType(ifc);
			SchordChannel channel = getJoinedChannelByIfcType(ifc);
			if (channel == null)
			{
				addLogView(ifc, "    Fail to get the joined Channel");
				return;
			}

			// for dialog whether accept the file transfer or not.
			AlertDialog alertDialog = (new AlertDialog.Builder(this)).setTitle("Receive Files").setMessage(getString(R.@string.from_ps_file_ps, nodeName + " [" + getInterfaceName(ifc) + "]", fileName)).setPositiveButton(R.@string.accept, new OnClickListenerAnonymousInnerClassHelper(this, ifc, nodeName, trId, sendApi, channel))
				   .setNegativeButton(R.@string.reject, new OnClickListenerAnonymousInnerClassHelper2(this, ifc, nodeName, trId, sendApi, channel))
				   .create();

			alertDialog.show();
			mAlertDialogMap[trId] = alertDialog;
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly SendFilesActivity outerInstance;

			private int ifc;
			private string nodeName;
			private string trId;
			private int sendApi;
			private SchordChannel channel;

			public OnClickListenerAnonymousInnerClassHelper(SendFilesActivity outerInstance, int ifc, string nodeName, string trId, int sendApi, SchordChannel channel)
			{
				this.outerInstance = outerInstance;
				this.ifc = ifc;
				this.nodeName = nodeName;
				this.trId = trId;
				this.sendApi = sendApi;
				this.channel = channel;
			}


			public override void onClick(DialogInterface arg0, int arg1)
			{
				/// <summary>
				/// 6. Accept the file transfer
				/// </summary>
				if (outerInstance.senderLeft(ifc, nodeName) || !outerInstance.mAlertDialogMap.ContainsKey(trId))
				{
					return;
				}

				if (sendApi == SEND_FILE)
				{
					channel.acceptFile(trId, 30 * 1000, 2, 300 * 1024);
					outerInstance.addLogView(ifc, "    acceptFile()");
				}
				else if (sendApi == SEND_MULTI_FILES)
				{
					channel.acceptMultiFiles(trId, 30 * 1000, 2, 300 * 1024);
					outerInstance.addLogView(ifc, "    acceptMultiFiles()");
				}
				outerInstance.mAlertDialogMap.Remove(trId);

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly SendFilesActivity outerInstance;

			private int ifc;
			private string nodeName;
			private string trId;
			private int sendApi;
			private SchordChannel channel;

			public OnClickListenerAnonymousInnerClassHelper2(SendFilesActivity outerInstance, int ifc, string nodeName, string trId, int sendApi, SchordChannel channel)
			{
				this.outerInstance = outerInstance;
				this.ifc = ifc;
				this.nodeName = nodeName;
				this.trId = trId;
				this.sendApi = sendApi;
				this.channel = channel;
			}


			public override void onClick(DialogInterface arg0, int arg1)
			{
				/// <summary>
				/// 6. Reject the file transfer
				/// </summary>
				if (outerInstance.senderLeft(ifc, nodeName) || !outerInstance.mAlertDialogMap.ContainsKey(trId))
				{
					return;
				}

				if (sendApi == SEND_FILE)
				{
					channel.rejectFile(trId);
					outerInstance.addLogView(ifc, "    rejectFile()");
				}
				else if (sendApi == SEND_MULTI_FILES)
				{
					channel.rejectMultiFiles(trId);
					outerInstance.addLogView(ifc, "    rejectMultiFiles()");
				}
				outerInstance.mAlertDialogMap.Remove(trId);

			}
		}

		private bool senderLeft(int interfaceType, string nodeName)
		{
			foreach (string sender in mNodeListAdapter.NodeList)
			{
				if (sender.Equals(nodeName))
				{
					return false;
				}
			}
			addLogView(interfaceType, "    The sender left.");

			return true;
		}

		private void saveFile(string fileName, string tmpFilePath)
		{
			string savedName = fileName;
			string name, ext;

			int i = savedName.LastIndexOf(".", StringComparison.Ordinal);
			if (i == -1)
			{
				name = savedName;
				ext = "";
			}
			else
			{
				name = savedName.Substring(0, i);
				ext = savedName.Substring(i);
			}

			File targetFile = new File(chordFilePath, savedName);
			int index = 0;
			while (targetFile.exists())
			{
				savedName = name + "_" + index + ext;
				targetFile = new File(chordFilePath, savedName);
				index++;
			}

			File srcFile = new File(tmpFilePath);
			srcFile.renameTo(targetFile);
		}

		private void expandeGroup()
		{
			// Expand the list of the progressBar
			int nodeCnt = mNodeListAdapter.GroupCount;
			for (int i = 0; i < nodeCnt; i++)
			{
				mNode_listView.expandGroup(i);
			}
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

		private const string CHORD_SEND_TEST_CHANNEL = "com.samsung.android.sdk.chord.example.SENDTESTCHANNEL";

		private const string MESSAGE_TYPE_FILE_NOTIFICATION = "FILE_NOTIFICATION_V2";

		private static readonly string chordFilePath = Environment.ExternalStorageDirectory.AbsolutePath + "/ChordExample";

		private const int SHARE_FILE_TIMEOUT_MILISECONDS = 1000 * 60 * 60;

		private const int SEND_FILE = 1;

		private const int SEND_MULTI_FILES = 2;

		private int mSend_api = 2;

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

		private bool mWifi_disconnected = false;

		private bool mWifiDirect_disconnected = false;

		private bool mMobileAP_disconnected = false;

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

		private Dictionary<string, AlertDialog> mAlertDialogMap = null;

		private Dictionary<string, int?> mNodeNumberMap = new Dictionary<string, int?>();

		private int mNodeNumber = 0;

		private SparseIntArray mInterfaceMap = new SparseIntArray();

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "SendFilesFragment : ";

	}

}