using System.Collections.Generic;

namespace com.samsung.android.sdk.chord.example.adapter
{


	using Context = android.content.Context;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseExpandableListAdapter = android.widget.BaseExpandableListAdapter;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using OnCheckedChangeListener = android.widget.CompoundButton.OnCheckedChangeListener;
	using GridView = android.widget.GridView;
	using TextView = android.widget.TextView;


	public class NodeListGridAdapter : BaseExpandableListAdapter
	{

		public static int LOSS_CALCULATION_DURATION_MILLISECONDS = 3000;

		public NodeListGridAdapter(Context context, bool isResponseEnabled)
		{
			mContext = context;
			mInflater = LayoutInflater.from(mContext);
			mNodeInfoList = new CopyOnWriteArrayList<NodeInfo>();

		}

		internal class NodeInfo
		{

			internal string interfaceName = null;

			internal string nodeName = null;

			internal int nodeNumber = 0;

			internal bool bChecked = false;

			// private long StartSendTime = 0;

			// private long EndSendTime = 0;

			internal long StartRecvTime = 0;

			internal long EndRecvTime = 0;

			internal int TotalMsgRecvd = 0;

			internal Timer myTimer = null;

			// private int TotalMsgSent = 0;

			internal bool bIsFrstPckt = false;

			internal int recv_stream_id = -1;

			internal int send_stream_id = 0;

			internal int message_count = 0;

			internal string payload_type = null;

			internal Dictionary<string, int?> Sendermap = null;

			internal List<GridData> gridList = new List<GridData>();

			internal NodeInfo(string interfaceName, string nodeName, int nodeNumber, bool bChecked)
			{
				this.interfaceName = interfaceName;
				this.nodeName = nodeName;
				this.nodeNumber = nodeNumber;
				this.bChecked = bChecked;
				// this.StartSendTime = 0;
				// this.EndSendTime = 0;
				this.StartRecvTime = 0;
				this.EndRecvTime = 0;
				this.TotalMsgRecvd = 0;
				// this.TotalMsgSent = 0;
				Sendermap = new Dictionary<string, int?>();
			}

		}

		internal class GridData
		{
			private readonly NodeListGridAdapter outerInstance;

			public GridData(NodeListGridAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			// private ArrayList<Item> senderData = new ArrayList<Item>();
			internal List<Item> receiverData = new List<Item>();
		}

		internal class GridViewHolder
		{
			private readonly NodeListGridAdapter outerInstance;

			public GridViewHolder(NodeListGridAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal GridView receiverStatisticsView;
			internal UdpInfoGridAdapter receiverInfoAdapter;
		}

		internal class NodeViewHolder
		{
			private readonly NodeListGridAdapter outerInstance;

			public NodeViewHolder(NodeListGridAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal TextView mNodename_textView;
			internal CheckBox mbSend_checkbox;
		}

		public virtual UdpFrameworkActivity Listener
		{
			set
			{
				mListener = value;
			}
		}

		public virtual int getNodePosition(string interfaceName, string nodeName)
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

		public virtual void addNode(string interfaceName, string nodeName, int nodeNum)
		{
			NodeInfo nodeInfo = new NodeInfo(interfaceName, nodeName, nodeNum, false);
			mNodeInfoList.Add(nodeInfo);

			notifyDataSetChanged();
		}

		public virtual void removeNode(string interfaceName, string nodeName)
		{
			int position = getNodePosition(interfaceName, nodeName);

			StopTimer(interfaceName, nodeName);

			try
			{
				mNodeInfoList.RemoveAt(position);
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "removeNode(" + position + ") : no such a node - " + nodeName);
			}

			notifyDataSetChanged();
		}

		public virtual void removeNodeGroup(string interfaceName)
		{
			IList<NodeInfo> nodeList = mNodeInfoList;
			foreach (NodeInfo nodeInfo in nodeList)
			{
				if (nodeInfo.interfaceName.Equals(interfaceName))
				{
					removeNode(interfaceName, nodeInfo.nodeName);
				}
			}

			notifyDataSetChanged();
		}

		public virtual void removeNodeAll()
		{
			StopAllTimer();
			mNodeInfoList.Clear();
			notifyDataSetChanged();
		}

		public virtual void removeReceiverDataGroup(string interfaceName)
		{
			IList<NodeInfo> nodeList = mNodeInfoList;

			foreach (NodeInfo nodeInfo in nodeList)
			{
				if (nodeInfo.interfaceName.Equals(interfaceName))
				{
					removeReceiverGridData(interfaceName, nodeInfo.nodeName);
				}
			}

			notifyDataSetChanged();
		}

		public virtual void setReceiverStartTime(string interfaceName, string node, long value)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.StartRecvTime = value;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setReceiverStartTime(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual long getReceiverStartTime(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].StartRecvTime;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getReceiverStartTime(" + pos + ") : no such a node - " + node);
			}

			return -1;

		}

		public virtual void setReceiverEndTime(string interfaceName, string node, long value)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.EndRecvTime = value;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setReceiverEndTime(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual long getReceiverEndTime(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].EndRecvTime;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getReceiverEndTime(" + pos + ") : no such a node - " + node);
			}

			return -1;

		}

		public virtual void setRecvStreamId(string interfaceName, string node, int stream_id)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.recv_stream_id = stream_id;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setRecvStreamId(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual int getRecvStreamId(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].recv_stream_id;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getRecvStreamId(" + pos + ") : no such a node - " + node);
			}

			return -1;
		}

		public virtual void setSendStreamId(string interfaceName, string node, int stream_id)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.send_stream_id = stream_id;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setSendStreamId(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual int getSendStreamId(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].send_stream_id;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getSendStreamId(" + pos + ") : no such a node - " + node);
			}

			return -1;
		}

		public virtual void setMessageCount(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.message_count = 0;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setMessageCount(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual int getMessgeCount(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].message_count;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getMessgeCount(" + pos + ") : no such a node - " + node);
			}

			return -1;
		}

		public virtual void setPayloadtype(string interfaceName, string node, string payloadtype)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.payload_type = payloadtype;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setPayloadtype(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual string getPayloadtype(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].payload_type;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getPayloadtype(" + pos + ") : no such a node - " + node);
			}

			return null;
		}

		public virtual void setTotalMsgRecvd(string interfaceName, string node, int value)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.TotalMsgRecvd = value;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setTotalMsgRecvd(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual int getTotalMsgRecvd(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].TotalMsgRecvd;
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getTotalMsgRecvd(" + pos + ") : no such a node - " + node);
			}

			return -1;
		}

		public virtual void IncrementTotalMsgRecvd(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.TotalMsgRecvd++;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "IncrementTotalMsgRecvd(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual void setFirstPacket(string interfaceName, string node, bool flag)
		{
			int pos = getNodePosition(interfaceName, node);
			NodeInfo nodeInfo = null;

			try
			{
				nodeInfo = mNodeInfoList[pos];
				nodeInfo.bIsFrstPckt = flag;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "setFirstPacket(" + pos + ") : no such a node - " + node);
			}

		}

		public virtual bool getFirstPacket(string interfaceName, string node)
		{
			int pos = getNodePosition(interfaceName, node);

			try
			{
				return mNodeInfoList[pos].bIsFrstPckt;

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getFirstPacket(" + pos + ") : no such a node - " + node);
			}

			return false;

		}

		public virtual int getReceiverLossPer(string interfaceName, string node, int TotalMsgRecvd)
		{
			int nodePos = getNodePosition(interfaceName, node);
			int loss = 0;

			try
			{
				GridData gridData = mNodeInfoList[nodePos].gridList[GridPosition];
				if (gridData != null)
				{
					if (gridData.receiverData.Count > 0)
					{
						loss = (gridData.receiverData.Count - TotalMsgRecvd) * 100 / gridData.receiverData.Count;
					}
					else
					{
						return 100;
					}
				}
			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "getReceiverLossPer(" + nodePos + ") : no such a node - " + node);
			}

			return loss;
		}

		public virtual void addToReceiverGrid(string interfaceName, string node, Item item)
		{
			int nodePos = getNodePosition(interfaceName, node);
			GridData gridData = null;

			try
			{
				if (mNodeInfoList[nodePos].gridList.Count == 0)
				{
					GridData g_data = new GridData(this, this);
					mNodeInfoList[nodePos].gridList.Add(g_data);
				}

				gridData = mNodeInfoList[nodePos].gridList[GridPosition];
				if (gridData != null)
				{
					gridData.receiverData.Add(item);
					notifyDataSetChanged();
				}

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "addToReceiverGrid(" + nodePos + ") : no such a node - " + node);
			}

		}

		public virtual void removeReceiverGridData(string interfaceName, string node)
		{
			int nodePos = getNodePosition(interfaceName, node);
			GridData gridData = null;

			// Log.d(TAG, TAGClass + "####### mNodeInfoList.size, nodePosition : " +
			// mNodeInfoList.size()
			// + ", " + nodePos);

			try
			{
				if (mNodeInfoList[nodePos].gridList.Count == 0)
				{
					return;
				}

				gridData = mNodeInfoList[nodePos].gridList[GridPosition];
				if (gridData != null && gridData.receiverData.Count > 0)
				{
					gridData.receiverData.Clear();
					notifyDataSetChanged();
				}

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "removeReceiverGridData(" + nodePos + ") : no such a node - " + node);
			}

		}

		public virtual void updateReceiverGrid(string interfaceName, string node, int pos, Item item)
		{
			int nodePos = getNodePosition(interfaceName, node);
			GridData gridData = null;

			try
			{
				if (mNodeInfoList[nodePos].gridList.Count == 0)
				{
					return;
				}

				gridData = mNodeInfoList[nodePos].gridList[GridPosition];
				if (gridData != null && gridData.receiverData.Count != 0)
				{
					gridData.receiverData[pos] = item;
					notifyDataSetChanged();
				}

			}
			catch (System.IndexOutOfRangeException)
			{
				Log.d(TAG, TAGClass + "removeReceiverGridData(" + nodePos + ") : no such a node - " + node);
			}

		}

		public virtual void StartTimer(string interfaceName, string node, int MsgNumber)
		{
			lock (this)
			{
				int pos = getNodePosition(interfaceName, node);
				NodeInfo nodeInfo = null;
        
				try
				{
					nodeInfo = mNodeInfoList[pos];
				}
				catch (System.IndexOutOfRangeException)
				{
					Log.d(TAG, TAGClass + "StartTimer(" + pos + ") : no such a node - " + node);
					return;
				}
        
				nodeInfo.myTimer = new Timer();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String InterfaceName = interfaceName;
				string InterfaceName = interfaceName;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String Node = node;
				string Node = node;
        
				if (MsgNumber > nodeInfo.message_count)
				{
					for (int i = nodeInfo.message_count; i < MsgNumber; i++)
					{
						if (null != mListener)
						{
							mListener.updateGrid(InterfaceName, Node, i);
						}
					}
				}
        
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int msg_count = MsgNumber;
				int msg_count = MsgNumber;
				nodeInfo.message_count = msg_count + 1;
        
				nodeInfo.myTimer.scheduleAtFixedRate(new TimerTaskAnonymousInnerClassHelper(this, InterfaceName, Node, msg_count), LOSS_CALCULATION_DURATION_MILLISECONDS, LOSS_CALCULATION_DURATION_MILLISECONDS);
        
			}
		}

		private class TimerTaskAnonymousInnerClassHelper : TimerTask
		{
			private readonly NodeListGridAdapter outerInstance;

			private string InterfaceName;
			private string Node;
			private int msg_count;

			public TimerTaskAnonymousInnerClassHelper(NodeListGridAdapter outerInstance, string InterfaceName, string Node, int msg_count)
			{
				this.outerInstance = outerInstance;
				this.InterfaceName = InterfaceName;
				this.Node = Node;
				this.msg_count = msg_count;
			}

			public virtual void run()
			{

				if (18 <= msg_count)
				{
					outerInstance.setReceiverEndTime(InterfaceName, Node, DateTimeHelperClass.CurrentUnixTimeMillis());
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long totalTime = getReceiverEndTime(InterfaceName, Node) - getReceiverStartTime(InterfaceName, Node);
					long totalTime = outerInstance.getReceiverEndTime(InterfaceName, Node) - outerInstance.getReceiverStartTime(InterfaceName, Node);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int loss = getReceiverLossPer(InterfaceName, Node, getTotalMsgRecvd(InterfaceName, Node));
					int loss = outerInstance.getReceiverLossPer(InterfaceName, Node, outerInstance.getTotalMsgRecvd(InterfaceName, Node));

					if (outerInstance.mListener != null)
					{
						outerInstance.mListener.lossCalculated(InterfaceName, Node, totalTime, loss);
						outerInstance.mListener.updateGrid(InterfaceName, Node, msg_count + 1);
					}

					outerInstance.StopTimer(InterfaceName, Node);
					outerInstance.setMessageCount(InterfaceName, Node);
				}
				else
				{
					if (outerInstance.mListener != null)
					{
						outerInstance.mListener.restart_timer(InterfaceName, Node, msg_count);
					}
				}
			}

		}

		public virtual void StopTimer(string interfaceName, string node)
		{
			lock (this)
			{
				int pos = getNodePosition(interfaceName, node);
        
				try
				{
					NodeInfo nodeInfo = mNodeInfoList[pos];
        
					if (nodeInfo.myTimer != null)
					{
						nodeInfo.myTimer.cancel();
						nodeInfo.myTimer.purge();
						nodeInfo.myTimer = null;
					}
        
				}
				catch (System.IndexOutOfRangeException)
				{
					Log.d(TAG, TAGClass + "StopTimer(" + pos + ") : no such a node - " + node);
				}
        
			}
		}

		public virtual void StopAllTimer()
		{
			lock (this)
			{
				foreach (NodeInfo nodeInfo in mNodeInfoList)
				{
					if (nodeInfo.myTimer != null)
					{
						nodeInfo.myTimer.cancel();
						nodeInfo.myTimer.purge();
						nodeInfo.myTimer = null;
					}
				}
        
			}
		}

		public virtual void ReStartTimer(string interfaceName, string node, int MsgNumber)
		{
			lock (this)
			{
				int pos = getNodePosition(interfaceName, node);
        
				try
				{
					NodeInfo nodeInfo = mNodeInfoList[pos];
        
					if (nodeInfo.myTimer != null)
					{
						nodeInfo.myTimer.cancel();
						nodeInfo.myTimer.purge();
						nodeInfo.myTimer = null;
					}
					StartTimer(interfaceName, node, MsgNumber);
        
				}
				catch (System.IndexOutOfRangeException)
				{
					Log.d(TAG, TAGClass + "ReStartTimer(" + pos + ") : no such a node - " + node);
				}
        
			}
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

		private int GridPosition
		{
			get
			{
				return 0;
			}
		}

		public override void notifyDataSetChanged()
		{
			base.notifyDataSetChanged();

			if (mGridViewHolder != null)
			{
				mGridViewHolder.receiverInfoAdapter.notifyDataSetChanged();
			}

		}

		/* Sender, Not Used */
		// public void removeSenderDataGroup(String interfaceName) {
		// List<NodeInfo> nodeList = mNodeInfoList;
		// for (NodeInfo nodeInfo : nodeList) {
		// if (nodeInfo.interfaceName.equals(interfaceName)) {
		// removeSenderGridData(interfaceName, nodeInfo.nodeName);
		// }
		// }
		// notifyDataSetChanged();
		// }
		//
		// public void clearSenderMap(String interfaceName, String node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.Sendermap.clear();
		// }
		//
		// public void setSenderMap(String interfaceName, String node, String key,
		// int value) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.Sendermap.put(key, value);
		// }
		//
		// public HashMap<String, Integer> getSenderMap(String interfaceName, String
		// node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// return nodeInfo.Sendermap;
		// }
		//
		// public void setSenderStartTime(String interfaceName, String node, long
		// value) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.StartSendTime = value;
		// }
		//
		// public void setSenderEndTime(String interfaceName, String node, long
		// value) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.EndSendTime = value;
		// }
		//
		// public long getSenderStartTime(String interfaceName, String node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// return nodeInfo.StartSendTime;
		// }
		//
		// public long getSenderEndTime(String interfaceName, String node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// return nodeInfo.EndSendTime;
		// }
		//
		// public void setTotalMsgSent(String interfaceName, String node, int value)
		// {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.TotalMsgSent = value;
		// }
		//
		// public void IncrementTotalMsgSent(String interfaceName, String node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// nodeInfo.TotalMsgSent++;
		// }
		//
		// public int getTotalMsgSent(String interfaceName, String node) {
		// int pos = getNodePosition(interfaceName, node);
		// NodeInfo nodeInfo = mNodeInfoList.get(pos);
		// return nodeInfo.TotalMsgSent;
		// }
		//
		// public void addToSenderGrid(String interfaceName, String node, Item item)
		// {
		// int nodePos = getNodePosition(interfaceName, node);
		// if (mNodeInfoList.get(nodePos).gridList.size() == 0) {
		// GridData g_data = new GridData();
		// mNodeInfoList.get(nodePos).gridList.add(g_data);
		// }
		// if (nodePos != -1) {
		// GridData gridData =
		// mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
		// if (gridData != null) {
		// gridData.senderData.add(item);
		// notifyDataSetChanged();
		// }
		// }
		// }
		//
		// public void removeSenderGridData(String interfaceName, String node) {
		// int nodePos = getNodePosition(interfaceName, node);
		// if (mNodeInfoList.get(nodePos).gridList.size() == 0)
		// return;
		// if (nodePos != -1) {
		// GridData gridData =
		// mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
		// if (gridData != null && !gridData.senderData.isEmpty()) {
		// gridData.senderData.clear();
		// notifyDataSetChanged();
		// }
		// mNodeInfoList.get(nodePos).gridList.remove(getGridPosition());
		// }
		// }
		//
		// public void updateSenderGrid(String interfaceName, String node, int pos,
		// Item item) {
		// int nodePos = getNodePosition(interfaceName, node);
		// if (nodePos != -1) {
		// GridData gridData =
		// mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
		// if (gridData != null) {
		// gridData.senderData.set(pos, item);
		// notifyDataSetChanged();
		// }
		// }
		// }
		//
		// public int getSenderGridSize(String interfaceName, String node) {
		// int nodePos = getNodePosition(interfaceName, node);
		// if (nodePos != -1) {
		// GridData gridData =
		// mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
		// if (gridData != null) {
		// return gridData.senderData.size();
		// }
		// }
		// return -1;
		// }

		public override object getChild(int groupPosition, int gridPosition)
		{
			return mNodeInfoList[groupPosition].gridList[gridPosition];
		}

		public override long getChildId(int groupPosition, int gridPosition)
		{
			return gridPosition;
		}

		public override View getChildView(int nodePosition, int progressPosition, bool isLastChild, View view, ViewGroup parent)
		{

			View v = view;

			if (null == v)
			{
				mGridViewHolder = new GridViewHolder(this, this);

				v = mInflater.inflate(R.layout.grid_list_item, null);
				mGridViewHolder.receiverStatisticsView = (GridView) v.findViewById(R.id.receiver_gridView);
				v.Tag = mGridViewHolder;

			}
			else
			{
				mGridViewHolder = (GridViewHolder) v.Tag;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NodeInfo nodeInfo = mNodeInfoList.get(nodePosition);
			NodeInfo nodeInfo = mNodeInfoList[nodePosition];

			GridData g_data = new GridData(this, this);
			mNodeInfoList[nodePosition].gridList.Add(g_data);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GridData data = nodeInfo.gridList.get(progressPosition);
			GridData data = nodeInfo.gridList[progressPosition];

			mGridViewHolder.receiverInfoAdapter = new UdpInfoGridAdapter(mContext, mInflater, data.receiverData);
			mGridViewHolder.receiverStatisticsView.Adapter = mGridViewHolder.receiverInfoAdapter;

			return v;

		}

		public override int getChildrenCount(int groupPosition)
		{
			return 1;
		}

		public override object getGroup(int groupPosition)
		{
			return mNodeInfoList[groupPosition];
		}

		public override int GroupCount
		{
			get
			{
				return mNodeInfoList.Count;
			}
		}

		public override long getGroupId(int groupPosition)
		{
			return groupPosition;
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

			// set a name of the node.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final NodeInfo nodeInfo = mNodeInfoList.get(position);
			NodeInfo nodeInfo = mNodeInfoList[position];

			mNodeViewHolder.mNodename_textView.Text = "Node" + nodeInfo.nodeNumber + " : " + nodeInfo.nodeName + " [" + nodeInfo.interfaceName + "] ";

			// set the checkBox

			if (bCheckMode)
			{
				mNodeViewHolder.mbSend_checkbox.Visibility = View.VISIBLE;

				mNodeViewHolder.mbSend_checkbox.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this, nodeInfo);
			}
			else
			{
				mNodeViewHolder.mbSend_checkbox.Visibility = View.GONE;
			}

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
			private readonly NodeListGridAdapter outerInstance;

			private com.samsung.android.sdk.chord.example.adapter.NodeListGridAdapter.NodeInfo nodeInfo;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(NodeListGridAdapter outerInstance, com.samsung.android.sdk.chord.example.adapter.NodeListGridAdapter.NodeInfo nodeInfo)
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

		public override bool isChildSelectable(int groupPosition, int childPosition)
		{
			return true;
		}

		private const string TAG = "[Chord][Sample]";

		private const string TAGClass = "NodeListGridAdapter : ";

		private LayoutInflater mInflater = null;

		private Context mContext = null;

		private IList<NodeInfo> mNodeInfoList = null;

		private bool bCheckMode = true;

		private GridViewHolder mGridViewHolder = null;

		private NodeViewHolder mNodeViewHolder = null;

		private UdpFrameworkActivity mListener = null;

	}

}