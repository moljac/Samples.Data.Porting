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

namespace com.samsung.android.sdk.chord.example.adapter
{


	using Context = android.content.Context;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using BaseExpandableListAdapter = android.widget.BaseExpandableListAdapter;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using OnCheckedChangeListener = android.widget.CompoundButton.OnCheckedChangeListener;
	using ImageView = android.widget.ImageView;
	using LinearLayout = android.widget.LinearLayout;
	using ProgressBar = android.widget.ProgressBar;
	using TextView = android.widget.TextView;

	public class NodeListAdapter : BaseExpandableListAdapter
	{

		public interface IFileCancelListener
		{
			void onFileCanceled(string channel, string node, string trId, int index, bool bMulti);
		}

		public NodeListAdapter(Context context, IFileCancelListener fileCancelListener)
		{
			mContext = context;
			mInflater = LayoutInflater.from(mContext);
			mNodeInfoList = new CopyOnWriteArrayList<NodeInfo>();
			mFileCancelListener = fileCancelListener;
		}

		/* Information about the joined node */
		internal class NodeInfo
		{
			private readonly NodeListAdapter outerInstance;


			internal string interfaceName = null;

			internal string nodeName = null;

			internal int nodeNumber = 0;

			internal bool bChecked = false;

			internal List<ProgressInfo> progressInfoList = new List<ProgressInfo>();

			internal NodeInfo(NodeListAdapter outerInstance, string interfaceName, string nodeName, int nodeNumber, bool bChecked)
			{
				this.outerInstance = outerInstance;
				this.interfaceName = interfaceName;
				this.nodeName = nodeName;
				this.bChecked = bChecked;
				this.nodeNumber = nodeNumber;
			}
		}

		/* Information about the progressBar of the file transfer */
		internal class ProgressInfo
		{
			private readonly NodeListAdapter outerInstance;

			public ProgressInfo(NodeListAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal bool bMulti = false;

			internal bool bSend = false;

			internal string trId = null;

			internal int fileIndex = 0;

			internal int progress = 0;
		}

		internal class ProgressViewHolder
		{
			private readonly NodeListAdapter outerInstance;

			public ProgressViewHolder(NodeListAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal ImageView loadingicon_imageView;

			internal Button fileCancel_btn;

			internal ProgressBar progressbar;

			internal TextView fileCnt_textView;
		}

		internal class NodeViewHolder
		{
			private readonly NodeListAdapter outerInstance;

			public NodeViewHolder(NodeListAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal TextView mNodename_textView;

			internal CheckBox mbSend_checkbox;
		}

		public virtual void addNode(string interfaceName, string nodeName, int nodeNumber)
		{
			NodeInfo nodeInfo = new NodeInfo(this, this, interfaceName, nodeName, nodeNumber, false);
			mNodeInfoList.Add(nodeInfo);

			notifyDataSetChanged();
		}

		public virtual void removeNode(string interfaceName, string nodeName)
		{
			int position = getNodePosition(interfaceName, nodeName);
			if (position == -1)
			{
				Log.d(TAG, TAGClass + "removeNode() : no such a node - " + nodeName);
				return;
			}
			mNodeInfoList.RemoveAt(position);

			notifyDataSetChanged();
		}

		public virtual void removeNodeGroup(string interfaceName)
		{

			IList<NodeInfo> nodeList = mNodeInfoList;
			foreach (NodeInfo nodeInfo in nodeList)
			{
				if (nodeInfo.interfaceName.Equals(interfaceName))
				{
					Log.d(TAG, TAGClass + "removeNodeGroup(" + interfaceName + ")");
					removeNode(interfaceName, nodeInfo.nodeName);
				}
			}

			notifyDataSetChanged();
		}

		public virtual void removeNodeAll()
		{
			mNodeInfoList.Clear();
			notifyDataSetChanged();
		}

		/* Get the list of nodes that you checked. */
		public virtual List<string[][]> CheckedNodeList
		{
			get
			{
				List<string[][]> checkedList = new List<string[][]>();
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: string[][] checkedNodeInfo = new string[mNodeInfoList.Count][2];
				string[][] checkedNodeInfo = RectangularArrays.ReturnRectangularStringArray(mNodeInfoList.Count, 2);
				int i = 0;
    
				foreach (NodeInfo nodeInfo in mNodeInfoList)
				{
					if (nodeInfo.bChecked)
					{
						checkedNodeInfo[i][0] = nodeInfo.interfaceName;
						checkedNodeInfo[i][1] = nodeInfo.nodeName;
						checkedList.Add(checkedNodeInfo);
						i++;
					}
				}
    
				return checkedList;
			}
		}

		public virtual List<string> NodeList
		{
			get
			{
				List<string> nodeNameList = new List<string>();
				foreach (NodeInfo nodeInfo in mNodeInfoList)
				{
					nodeNameList.Add(nodeInfo.nodeName);
				}
    
				return nodeNameList;
			}
		}

		public virtual bool CheckMode
		{
			set
			{
				bCheckMode = value;
			}
		}

		public virtual bool SecureChannelFrag
		{
			set
			{
				mIsSecureChannelFrag = value;
			}
		}

		/* Set the total count of files. */
		public virtual void setFileTotalCnt(string interfaceName, string node, int fileTotalCnt, string trId)
		{

			int nodePosition = getNodePosition(interfaceName, node);
			if (nodePosition == -1)
			{
				Log.d(TAG, TAGClass + "setFileTotalCnt() : no such a node - " + node);
				return;
			}

			fileCntHashMap[trId] = fileTotalCnt;

		}

		/* Add or Update the progressBar */
		public virtual void setProgressUpdate(string interfaceName, string node, string trId, int fileIndex, int progress, bool bMulti, bool bSend)
		{

			int nodePosition = getNodePosition(interfaceName, node);
			if (nodePosition == -1)
			{
				Log.d(TAG, TAGClass + "setProgressUpdate(fileIdx[" + fileIndex + "]) : no such a node - " + node);
				return;
			}

			int progressPosition = getProgressPosition(nodePosition, fileIndex, trId);

			if (progressPosition == -1)
			{ // Add the progressBar
				if (null != fileCancelHashMap[trId])
				{
					Log.d(TAG, TAGClass + "fileChunk is received/sent after click the cancel button.");
					fileCancelHashMap.Remove(trId);
					return;
				}

				ProgressInfo progressInfo = new ProgressInfo(this, this);
				progressInfo.trId = trId;
				progressInfo.bMulti = bMulti;
				progressInfo.fileIndex = fileIndex;
				progressInfo.progress = progress;
				progressInfo.bSend = bSend;

				addProgress(nodePosition, progressInfo);

			}
			else
			{ // Update the progressBar - save the percentage of the file
					 // transfer
				List<ProgressInfo> progressList = mNodeInfoList[nodePosition].progressInfoList;
				progressList[progressPosition].progress = progress;

				notifyDataSetChanged();
			}
		}

		/* Add the progressBar */
		public virtual void addProgress(int nodePosition, ProgressInfo progressInfo)
		{
			List<ProgressInfo> progressInfoList = mNodeInfoList[nodePosition].progressInfoList;
			progressInfoList.Add(progressInfo);

			notifyDataSetChanged();
		}

		/* Remove the progressBar */
		public virtual void removeProgress(int fileIndex, string trId)
		{
			for (int i = 0; i < GroupCount; i++)
			{
				NodeInfo node = mNodeInfoList[i];
				for (int j = 0; j < getChildrenCount(i); j++)
				{

					ProgressInfo progressInfo = node.progressInfoList[j];
					if (progressInfo.fileIndex == fileIndex && progressInfo.trId.Equals(trId))
					{
						mNodeInfoList[i].progressInfoList.RemoveAt(j);
					}
				}
			}

			notifyDataSetChanged();
		}

		/* Remove the canceled file transfer's progressBar. */
		public virtual void removeCanceledProgress(string interfaceName, string nodeName, string trId)
		{
			int nodePosition = getNodePosition(interfaceName, nodeName);
			if (nodePosition == -1)
			{
				Log.d(TAG, TAGClass + "removeCanceledProgress() : no such a node - " + nodeName);
				return;
			}

			// Find the canceled file transfer's trId
			for (IEnumerator<ProgressInfo> iterator = mNodeInfoList[nodePosition].progressInfoList.GetEnumerator(); iterator.MoveNext();)
			{
				ProgressInfo progressInfo = iterator.Current;
				if (progressInfo.trId.Equals(trId))
				{
					iterator.remove();
				}
			}

			notifyDataSetChanged();
		}

		/* Get the node's position. */
		private int getNodePosition(string interfaceName, string nodeName)
		{

			for (int i = 0; i < GroupCount; i++)
			{
				if (mNodeInfoList[i].interfaceName.Equals(interfaceName) && mNodeInfoList[i].nodeName.Equals(nodeName))
				{
					return i;
				}
			}

			return -1;
		}

		/* Get the progressBar's position. */
		private int getProgressPosition(int nodePosition, int fileIndex, string trId)
		{
			NodeInfo node = mNodeInfoList[nodePosition];
			for (int i = 0; i < getChildrenCount(nodePosition); i++)
			{

				ProgressInfo progressInfo = node.progressInfoList[i];
				if (progressInfo.fileIndex == fileIndex && progressInfo.trId.Equals(trId))
				{
					return i;
				}
			}

			return -1;
		}

		public override object getChild(int nodePosition, int progressPosition)
		{

			return mNodeInfoList[nodePosition].progressInfoList[progressPosition];
		}

		public override long getChildId(int nodePosition, int progressPosition)
		{

			return progressPosition;
		}

		public override View getChildView(int nodePosition, int progressPosition, bool isLastProgress, View view, ViewGroup parent)
		{

			View v = view;
			if (null == v)
			{
				mProgressViewHolder = new ProgressViewHolder(this, this);
				v = mInflater.inflate(R.layout.progress_list_item, null);
				mProgressViewHolder.loadingicon_imageView = (ImageView) v.findViewById(R.id.loadingicon_imageView);
				mProgressViewHolder.fileCancel_btn = (Button) v.findViewById(R.id.fileCancel_btn);
				mProgressViewHolder.progressbar = (ProgressBar) v.findViewById(R.id.progressBar);
				mProgressViewHolder.fileCnt_textView = (TextView) v.findViewById(R.id.fileCnt_textView);
				v.Tag = mProgressViewHolder;

			}
			else
			{
				mProgressViewHolder = (ProgressViewHolder) v.Tag;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NodeInfo nodeInfo = mNodeInfoList.get(nodePosition);
			NodeInfo nodeInfo = mNodeInfoList[nodePosition];
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ProgressInfo progressInfo = nodeInfo.progressInfoList.get(progressPosition);
			ProgressInfo progressInfo = nodeInfo.progressInfoList[progressPosition];

			// Set the progressBar's icon to distinguish between sending and
			// receiving the file transfer.
			if (progressInfo.bSend)
			{
				mProgressViewHolder.loadingicon_imageView.BackgroundResource = R.drawable.ic_file_sending;

			}
			else
			{
				mProgressViewHolder.loadingicon_imageView.BackgroundResource = R.drawable.ic_file_receiving;
			}

			// Set the the percentage of the file transfer and count of files.
			mProgressViewHolder.progressbar.Progress = progressInfo.progress;
			mProgressViewHolder.fileCnt_textView.Text = "(" + progressInfo.fileIndex + "/" + fileCntHashMap[progressInfo.trId] + ")";

			// When you click a button of the file transfer ..
			mProgressViewHolder.fileCancel_btn.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, v, nodeInfo, progressInfo);

			return v;

		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly NodeListAdapter outerInstance;

			private View v;
			private com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.NodeInfo nodeInfo;
			private com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.ProgressInfo progressInfo;

			public OnClickListenerAnonymousInnerClassHelper(NodeListAdapter outerInstance, View v, com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.NodeInfo nodeInfo, com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.ProgressInfo progressInfo)
			{
				this.outerInstance = outerInstance;
				this.v = v;
				this.nodeInfo = nodeInfo;
				this.progressInfo = progressInfo;
			}


			public override void onClick(View v)
			{
				outerInstance.mFileCancelListener.onFileCanceled(nodeInfo.interfaceName, nodeInfo.nodeName, progressInfo.trId, progressInfo.fileIndex, progressInfo.bMulti);
				outerInstance.fileCancelHashMap[progressInfo.trId] = true;
				Log.d(TAG, TAGClass + "onClick() : " + progressInfo.trId);
			}
		}

		public override int getChildrenCount(int nodePosition)
		{

			return mNodeInfoList[nodePosition].progressInfoList.Count;
		}

		public override object getGroup(int nodePosition)
		{

			return mNodeInfoList[nodePosition];
		}

		public override int GroupCount
		{
			get
			{
    
				return mNodeInfoList.Count;
			}
		}

		public override long getGroupId(int nodePosition)
		{

			return nodePosition;
		}

		public override View getGroupView(int position, bool isExpanded, View view, ViewGroup parent)
		{

			View v = view;
			if (null == v)
			{
				mNodeViewHolder = new NodeViewHolder(this, this);

				v = mInflater.inflate(R.layout.node_list_item, parent, false);
				mNodeViewHolder.mNodename_textView = (TextView) v.findViewById(R.id.nodeName_textview);
				mNodeViewHolder.mbSend_checkbox = (CheckBox) v.findViewById(R.id.bSend_checkbox);

				v.Tag = mNodeViewHolder;

			}
			else
			{
				mNodeViewHolder = (NodeViewHolder) v.Tag;
			}

			if (mIsSecureChannelFrag)
			{
				LinearLayout mNodeListItem = (LinearLayout) v.findViewById(R.id.nodeListItem_layout);
				mNodeListItem.setPadding(20, 15, 15, 15);
			}

			// set a name of the node.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NodeInfo nodeInfo = mNodeInfoList.get(position);
			NodeInfo nodeInfo = mNodeInfoList[position];
			mNodeViewHolder.mNodename_textView.Text = "Node" + nodeInfo.nodeNumber + " : " + nodeInfo.nodeName + " [" + nodeInfo.interfaceName + "]";

			if (bCheckMode)
			{
				mNodeViewHolder.mbSend_checkbox.Visibility = View.VISIBLE;

				mNodeViewHolder.mbSend_checkbox.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this, nodeInfo);
			}
			else
			{
				mNodeViewHolder.mbSend_checkbox.Visibility = View.GONE;
			}

			// set the checkBox
			if (nodeInfo.bChecked)
			{
				mNodeViewHolder.mbSend_checkbox.Checked = true;
			}
			else
			{
				mNodeViewHolder.mbSend_checkbox.Checked = false;
			}

			return v;
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
		{
			private readonly NodeListAdapter outerInstance;

			private com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.NodeInfo nodeInfo;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(NodeListAdapter outerInstance, com.samsung.android.sdk.chord.example.adapter.NodeListAdapter.NodeInfo nodeInfo)
			{
				this.outerInstance = outerInstance;
				this.nodeInfo = nodeInfo;
			}


			public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
			{
				if (isChecked && !nodeInfo.bChecked)
				{
					nodeInfo.bChecked = true;
				}
				else if (!isChecked && nodeInfo.bChecked)
				{
					nodeInfo.bChecked = false;
				}
			}
		}

		public override bool hasStableIds()
		{

			return true;
		}

		public override bool isChildSelectable(int arg0, int arg1)
		{

			return true;
		}

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "NodeListAdapter : ";

		private LayoutInflater mInflater = null;

		private Context mContext = null;

		private NodeViewHolder mNodeViewHolder = null;

		private ProgressViewHolder mProgressViewHolder = null;

		private IList<NodeInfo> mNodeInfoList = null;

		private bool bCheckMode = true;

		private bool mIsSecureChannelFrag = false;

		private IFileCancelListener mFileCancelListener = null;

		private Dictionary<string, int?> fileCntHashMap = new Dictionary<string, int?>();

		private Dictionary<string, bool?> fileCancelHashMap = new Dictionary<string, bool?>();

	}

}