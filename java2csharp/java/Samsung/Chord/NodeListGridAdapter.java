
package com.samsung.android.sdk.chord.example.adapter;

import com.samsung.android.sdk.chord.example.R;
import com.samsung.android.sdk.chord.example.UdpFrameworkActivity;

import android.content.Context;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.GridView;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Timer;
import java.util.TimerTask;
import java.util.concurrent.CopyOnWriteArrayList;

public class NodeListGridAdapter extends BaseExpandableListAdapter {

    public static int LOSS_CALCULATION_DURATION_MILLISECONDS = 3000;

    public NodeListGridAdapter(Context context, boolean isResponseEnabled) {
        mContext = context;
        mInflater = LayoutInflater.from(mContext);
        mNodeInfoList = new CopyOnWriteArrayList<NodeInfo>();

    }

    static class NodeInfo {

        String interfaceName = null;

        String nodeName = null;

        int nodeNumber = 0;

        boolean bChecked = false;

        // private long StartSendTime = 0;

        // private long EndSendTime = 0;

        private long StartRecvTime = 0;

        private long EndRecvTime = 0;

        private int TotalMsgRecvd = 0;

        private Timer myTimer = null;

        // private int TotalMsgSent = 0;

        private boolean bIsFrstPckt = false;

        private int recv_stream_id = -1;

        private int send_stream_id = 0;

        private int message_count = 0;

        private String payload_type = null;

        HashMap<String, Integer> Sendermap = null;

        ArrayList<GridData> gridList = new ArrayList<GridData>();

        NodeInfo(String interfaceName, String nodeName, int nodeNumber, boolean bChecked) {
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
            Sendermap = new HashMap<String, Integer>();
        }

    }

    class GridData {
        // private ArrayList<Item> senderData = new ArrayList<Item>();
        private ArrayList<Item> receiverData = new ArrayList<Item>();
    }

    class GridViewHolder {
        private GridView receiverStatisticsView;
        private UdpInfoGridAdapter receiverInfoAdapter;
    }

    class NodeViewHolder {
        TextView mNodename_textView;
        CheckBox mbSend_checkbox;
    }

    public void setListener(UdpFrameworkActivity listener) {
        mListener = listener;
    }

    public int getNodePosition(String interfaceName, String nodeName) {
        for (int i = 0; i < getGroupCount(); i++) {
            if (mNodeInfoList.get(i).interfaceName.equals(interfaceName)
                    && mNodeInfoList.get(i).nodeName.equals(nodeName)) {
                return i;
            }
        }

        return -1;
    }

    public void addNode(String interfaceName, String nodeName, int nodeNum) {
        NodeInfo nodeInfo = new NodeInfo(interfaceName, nodeName, nodeNum, false);
        mNodeInfoList.add(nodeInfo);

        notifyDataSetChanged();
    }

    public void removeNode(String interfaceName, String nodeName) {
        int position = getNodePosition(interfaceName, nodeName);

        StopTimer(interfaceName, nodeName);

        try {
            mNodeInfoList.remove(position);
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "removeNode(" + position + ") : no such a node - " + nodeName);
        }

        notifyDataSetChanged();
    }

    public void removeNodeGroup(String interfaceName) {
        List<NodeInfo> nodeList = mNodeInfoList;
        for (NodeInfo nodeInfo : nodeList) {
            if (nodeInfo.interfaceName.equals(interfaceName)) {
                removeNode(interfaceName, nodeInfo.nodeName);
            }
        }

        notifyDataSetChanged();
    }

    public void removeNodeAll() {
        StopAllTimer();
        mNodeInfoList.clear();
        notifyDataSetChanged();
    }

    public void removeReceiverDataGroup(String interfaceName) {
        List<NodeInfo> nodeList = mNodeInfoList;

        for (NodeInfo nodeInfo : nodeList) {
            if (nodeInfo.interfaceName.equals(interfaceName)) {
                removeReceiverGridData(interfaceName, nodeInfo.nodeName);
            }
        }

        notifyDataSetChanged();
    }

    public void setReceiverStartTime(String interfaceName, String node, long value) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.StartRecvTime = value;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setReceiverStartTime(" + pos + ") : no such a node - " + node);
        }

    }

    public long getReceiverStartTime(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).StartRecvTime;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getReceiverStartTime(" + pos + ") : no such a node - " + node);
        }

        return -1;

    }

    public void setReceiverEndTime(String interfaceName, String node, long value) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.EndRecvTime = value;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setReceiverEndTime(" + pos + ") : no such a node - " + node);
        }

    }

    public long getReceiverEndTime(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).EndRecvTime;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getReceiverEndTime(" + pos + ") : no such a node - " + node);
        }

        return -1;

    }

    public void setRecvStreamId(String interfaceName, String node, int stream_id) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.recv_stream_id = stream_id;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setRecvStreamId(" + pos + ") : no such a node - " + node);
        }

    }

    public int getRecvStreamId(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).recv_stream_id;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getRecvStreamId(" + pos + ") : no such a node - " + node);
        }

        return -1;
    }

    public void setSendStreamId(String interfaceName, String node, int stream_id) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.send_stream_id = stream_id;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setSendStreamId(" + pos + ") : no such a node - " + node);
        }

    }

    public int getSendStreamId(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).send_stream_id;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getSendStreamId(" + pos + ") : no such a node - " + node);
        }

        return -1;
    }

    public void setMessageCount(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.message_count = 0;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setMessageCount(" + pos + ") : no such a node - " + node);
        }

    }

    public int getMessgeCount(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).message_count;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getMessgeCount(" + pos + ") : no such a node - " + node);
        }

        return -1;
    }

    public void setPayloadtype(String interfaceName, String node, String payloadtype) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.payload_type = payloadtype;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setPayloadtype(" + pos + ") : no such a node - " + node);
        }

    }

    public String getPayloadtype(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).payload_type;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getPayloadtype(" + pos + ") : no such a node - " + node);
        }

        return null;
    }

    public void setTotalMsgRecvd(String interfaceName, String node, int value) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.TotalMsgRecvd = value;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setTotalMsgRecvd(" + pos + ") : no such a node - " + node);
        }

    }

    public int getTotalMsgRecvd(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).TotalMsgRecvd;
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getTotalMsgRecvd(" + pos + ") : no such a node - " + node);
        }

        return -1;
    }

    public void IncrementTotalMsgRecvd(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.TotalMsgRecvd++;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "IncrementTotalMsgRecvd(" + pos + ") : no such a node - " + node);
        }

    }

    public void setFirstPacket(String interfaceName, String node, boolean flag) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
            nodeInfo.bIsFrstPckt = flag;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "setFirstPacket(" + pos + ") : no such a node - " + node);
        }

    }

    public boolean getFirstPacket(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            return mNodeInfoList.get(pos).bIsFrstPckt;

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getFirstPacket(" + pos + ") : no such a node - " + node);
        }

        return false;

    }

    public int getReceiverLossPer(String interfaceName, String node, int TotalMsgRecvd) {
        int nodePos = getNodePosition(interfaceName, node);
        int loss = 0;

        try {
            GridData gridData = mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
            if (gridData != null) {
                if (gridData.receiverData.size() > 0) {
                    loss = (gridData.receiverData.size() - TotalMsgRecvd) * 100
                            / gridData.receiverData.size();
                } else {
                    return 100;
                }
            }
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "getReceiverLossPer(" + nodePos + ") : no such a node - " + node);
        }

        return loss;
    }

    public void addToReceiverGrid(String interfaceName, String node, Item item) {
        int nodePos = getNodePosition(interfaceName, node);
        GridData gridData = null;

        try {
            if (mNodeInfoList.get(nodePos).gridList.size() == 0) {
                GridData g_data = new GridData();
                mNodeInfoList.get(nodePos).gridList.add(g_data);
            }

            gridData = mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
            if (gridData != null) {
                gridData.receiverData.add(item);
                notifyDataSetChanged();
            }

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "addToReceiverGrid(" + nodePos + ") : no such a node - " + node);
        }

    }

    public void removeReceiverGridData(String interfaceName, String node) {
        int nodePos = getNodePosition(interfaceName, node);
        GridData gridData = null;

        // Log.d(TAG, TAGClass + "####### mNodeInfoList.size, nodePosition : " +
        // mNodeInfoList.size()
        // + ", " + nodePos);

        try {
            if (mNodeInfoList.get(nodePos).gridList.size() == 0)
                return;

            gridData = mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
            if (gridData != null && !gridData.receiverData.isEmpty()) {
                gridData.receiverData.clear();
                notifyDataSetChanged();
            }

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "removeReceiverGridData(" + nodePos + ") : no such a node - "
                    + node);
        }

    }

    public void updateReceiverGrid(String interfaceName, String node, int pos, Item item) {
        int nodePos = getNodePosition(interfaceName, node);
        GridData gridData = null;

        try {
            if (mNodeInfoList.get(nodePos).gridList.size() == 0)
                return;

            gridData = mNodeInfoList.get(nodePos).gridList.get(getGridPosition());
            if (gridData != null && gridData.receiverData.size() != 0) {
                gridData.receiverData.set(pos, item);
                notifyDataSetChanged();
            }

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "removeReceiverGridData(" + nodePos + ") : no such a node - "
                    + node);
        }

    }

    public synchronized void StartTimer(String interfaceName, String node, int MsgNumber) {
        int pos = getNodePosition(interfaceName, node);
        NodeInfo nodeInfo = null;

        try {
            nodeInfo = mNodeInfoList.get(pos);
        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "StartTimer(" + pos + ") : no such a node - " + node);
            return;
        }

        nodeInfo.myTimer = new Timer();
        final String InterfaceName = interfaceName;
        final String Node = node;

        if (MsgNumber > nodeInfo.message_count) {
            for (int i = nodeInfo.message_count; i < MsgNumber; i++) {
                if (null != mListener) {
                    mListener.updateGrid(InterfaceName, Node, i);
                }
            }
        }

        final int msg_count = MsgNumber;
        nodeInfo.message_count = msg_count + 1;

        nodeInfo.myTimer.scheduleAtFixedRate(new TimerTask() {
            public void run() {

                if (18 <= msg_count) {
                    setReceiverEndTime(InterfaceName, Node, System.currentTimeMillis());
                    final long totalTime = getReceiverEndTime(InterfaceName, Node)
                            - getReceiverStartTime(InterfaceName, Node);
                    final int loss = getReceiverLossPer(InterfaceName, Node,
                            getTotalMsgRecvd(InterfaceName, Node));

                    if (mListener != null) {
                        mListener.lossCalculated(InterfaceName, Node, totalTime, loss);
                        mListener.updateGrid(InterfaceName, Node, msg_count + 1);
                    }

                    StopTimer(InterfaceName, Node);
                    setMessageCount(InterfaceName, Node);
                } else {
                    if (mListener != null) {
                        mListener.restart_timer(InterfaceName, Node, msg_count);
                    }
                }
            }

        }, LOSS_CALCULATION_DURATION_MILLISECONDS, LOSS_CALCULATION_DURATION_MILLISECONDS);

    }

    public synchronized void StopTimer(String interfaceName, String node) {
        int pos = getNodePosition(interfaceName, node);

        try {
            NodeInfo nodeInfo = mNodeInfoList.get(pos);

            if (nodeInfo.myTimer != null) {
                nodeInfo.myTimer.cancel();
                nodeInfo.myTimer.purge();
                nodeInfo.myTimer = null;
            }

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "StopTimer(" + pos + ") : no such a node - " + node);
        }

    }

    public synchronized void StopAllTimer() {
        for (NodeInfo nodeInfo : mNodeInfoList) {
            if (nodeInfo.myTimer != null) {
                nodeInfo.myTimer.cancel();
                nodeInfo.myTimer.purge();
                nodeInfo.myTimer = null;
            }
        }

    }

    public synchronized void ReStartTimer(String interfaceName, String node, int MsgNumber) {
        int pos = getNodePosition(interfaceName, node);

        try {
            NodeInfo nodeInfo = mNodeInfoList.get(pos);

            if (nodeInfo.myTimer != null) {
                nodeInfo.myTimer.cancel();
                nodeInfo.myTimer.purge();
                nodeInfo.myTimer = null;
            }
            StartTimer(interfaceName, node, MsgNumber);

        } catch (IndexOutOfBoundsException e) {
            Log.d(TAG, TAGClass + "ReStartTimer(" + pos + ") : no such a node - " + node);
        }

    }

    /* Get the list of nodes that you checked. */
    public ArrayList<String[][]> getCheckedNodeList() {
        ArrayList<String[][]> checkedList = new ArrayList<String[][]>();
        String checkedNodeInfo[][] = new String[mNodeInfoList.size()][2];

        int i = 0;
        for (NodeInfo nodeInfo : mNodeInfoList) {
            if (nodeInfo.bChecked) {
                checkedNodeInfo[i][0] = nodeInfo.interfaceName;
                checkedNodeInfo[i][1] = nodeInfo.nodeName;
                checkedList.add(checkedNodeInfo);
                i++;
            }
        }

        return checkedList;
    }

    public ArrayList<String> getNodeList() {
        ArrayList<String> nodeNameList = new ArrayList<String>();

        for (NodeInfo nodeInfo : mNodeInfoList) {
            nodeNameList.add(nodeInfo.nodeName);
        }

        return nodeNameList;
    }

    public void setCheckMode(boolean checkMode) {
        bCheckMode = checkMode;
    }

    private int getGridPosition() {
        return 0;
    }

    @Override
    public void notifyDataSetChanged() {
        super.notifyDataSetChanged();

        if (mGridViewHolder != null) {
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

    @Override
    public Object getChild(int groupPosition, int gridPosition) {
        return mNodeInfoList.get(groupPosition).gridList.get(gridPosition);
    }

    @Override
    public long getChildId(int groupPosition, int gridPosition) {
        return gridPosition;
    }

    @Override
    public View getChildView(int nodePosition, int progressPosition, boolean isLastChild,
            View view, ViewGroup parent) {

        View v = view;

        if (null == v) {
            mGridViewHolder = new GridViewHolder();

            v = mInflater.inflate(R.layout.grid_list_item, null);
            mGridViewHolder.receiverStatisticsView = (GridView) v
                    .findViewById(R.id.receiver_gridView);
            v.setTag(mGridViewHolder);

        } else {
            mGridViewHolder = (GridViewHolder) v.getTag();
        }

        final NodeInfo nodeInfo = mNodeInfoList.get(nodePosition);

        GridData g_data = new GridData();
        mNodeInfoList.get(nodePosition).gridList.add(g_data);
        final GridData data = nodeInfo.gridList.get(progressPosition);

        mGridViewHolder.receiverInfoAdapter = new UdpInfoGridAdapter(mContext, mInflater,
                data.receiverData);
        mGridViewHolder.receiverStatisticsView.setAdapter(mGridViewHolder.receiverInfoAdapter);

        return v;

    }

    @Override
    public int getChildrenCount(int groupPosition) {
        return 1;
    }

    @Override
    public Object getGroup(int groupPosition) {
        return mNodeInfoList.get(groupPosition);
    }

    @Override
    public int getGroupCount() {
        return mNodeInfoList.size();
    }

    @Override
    public long getGroupId(int groupPosition) {
        return groupPosition;
    }

    @Override
    public View getGroupView(int position, boolean isExpanded, View view, ViewGroup parent) {
        View v = view;

        if (null == v) {
            mNodeViewHolder = new NodeViewHolder();

            v = mInflater.inflate(R.layout.node_list_item, parent, false);
            mNodeViewHolder.mNodename_textView = (TextView) v.findViewById(R.id.nodeName_textview);
            mNodeViewHolder.mbSend_checkbox = (CheckBox) v.findViewById(R.id.bSend_checkbox);

            v.setTag(mNodeViewHolder);

        } else {
            mNodeViewHolder = (NodeViewHolder) v.getTag();
        }

        // set a name of the node.
        final NodeInfo nodeInfo = mNodeInfoList.get(position);

        mNodeViewHolder.mNodename_textView.setText("Node" + nodeInfo.nodeNumber + " : "
                + nodeInfo.nodeName + " [" + nodeInfo.interfaceName + "] ");

        // set the checkBox

        if (bCheckMode) {
            mNodeViewHolder.mbSend_checkbox.setVisibility(View.VISIBLE);

            mNodeViewHolder.mbSend_checkbox
                    .setOnCheckedChangeListener(new OnCheckedChangeListener() {

                        @Override
                        public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                            if (isChecked && !nodeInfo.bChecked) {
                                nodeInfo.bChecked = true;
                            } else if (!isChecked && nodeInfo.bChecked) {
                                nodeInfo.bChecked = false;
                            }
                        }
                    });
        } else {
            mNodeViewHolder.mbSend_checkbox.setVisibility(View.GONE);
        }

        if (nodeInfo.bChecked) {
            mNodeViewHolder.mbSend_checkbox.setChecked(true);
        } else {
            mNodeViewHolder.mbSend_checkbox.setChecked(false);
        }
        return v;
    }

    @Override
    public boolean hasStableIds() {
        return true;
    }

    @Override
    public boolean isChildSelectable(int groupPosition, int childPosition) {
        return true;
    }

    private static final String TAG = "[Chord][Sample]";

    private static final String TAGClass = "NodeListGridAdapter : ";

    private LayoutInflater mInflater = null;

    private Context mContext = null;

    private List<NodeInfo> mNodeInfoList = null;

    private boolean bCheckMode = true;

    private GridViewHolder mGridViewHolder = null;

    private NodeViewHolder mNodeViewHolder = null;

    private UdpFrameworkActivity mListener = null;

}
