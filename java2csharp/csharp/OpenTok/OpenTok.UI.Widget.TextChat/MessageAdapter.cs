using System;
using System.Collections.Generic;

namespace com.opentok.android.ui.textchat.widget
{

	using Context = android.content.Context;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;



	internal class MessageAdapter : ArrayAdapter<ChatMessage>
	{

		private const int VIEW_TYPE_ROW_SENT = 0;
		private const int VIEW_TYPE_ROW_SENT_GROUP = 1;
		private const int VIEW_TYPE_ROW_RECEIVED = 2;
		private const int VIEW_TYPE_ROW_RECEIVED_GROUP = 3;


		private IList<ChatMessage> messagesList = new List<ChatMessage>();
		internal ViewHolder holder;

		private bool messagesGroup = false;

		private class ViewHolder
		{
			private readonly MessageAdapter outerInstance;

			public ViewHolder(MessageAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public TextView aliasText, messageText, timestampText;
			public ImageView arrowView;
			public View rowDividerView;
			public int viewType;
		}

		public MessageAdapter(Context context, int resource, IList<ChatMessage> entities) : base(context, resource, entities)
		{
			this.messagesList = entities;
		}


		public virtual IList<ChatMessage> MessagesList
		{
			get
			{
				return messagesList;
			}
		}

		public override int ViewTypeCount
		{
			get
			{
				return 4;
			}
		}

		public override int getItemViewType(int position)
		{
			if (messagesList[position].Status.Equals(ChatMessage.MessageStatus.SENT_MESSAGE))
			{
				if (checkMessageGroup(position))
				{
					return VIEW_TYPE_ROW_SENT_GROUP;
				}
				else
				{
					return VIEW_TYPE_ROW_SENT;
				}
			}
			else
			{
				if (messagesList[position].Status.Equals(ChatMessage.MessageStatus.RECEIVED_MESSAGE))
				{
					if (checkMessageGroup(position))
					{
						return VIEW_TYPE_ROW_RECEIVED_GROUP;
					}
					else
					{
						return VIEW_TYPE_ROW_RECEIVED;
					}
				}
			}

			return VIEW_TYPE_ROW_SENT; //by default
		}

		private bool checkMessageGroup(int position)
		{
			//check timestamp for message to group messages (multiple messages sent within a 2 minutes time limit)
			ChatMessage lastMsg = null;
			ChatMessage currentMsg = null;

			IList<ChatMessage> myList = new List<ChatMessage>();
			myList = this.MessagesList;
			if (myList.Count > 1 && position > 0)
			{
				lastMsg = messagesList[position - 1];
				currentMsg = messagesList[position];
				if (lastMsg != null && currentMsg != null && lastMsg.SenderId.Equals(currentMsg.SenderId))
				{
					//check time
					if (checkTimeMsg(lastMsg.Timestamp, currentMsg.Timestamp))
					{
						return true;
					}
				}
			}
			return false;
		}

		//Check the time between the current new message and the last added message
		private bool checkTimeMsg(long lastMsgTime, long newMsgTime)
		{
			if (lastMsgTime - newMsgTime <= TimeUnit.MINUTES.toMillis(2))
			{
				return true;
			}
			return false;
		}

		public override View getView(int position, View convertView, ViewGroup parent)
		{
			ViewHolder holder = null;
			ChatMessage message = this.messagesList[position];

			holder = new ViewHolder(this);
			int type = VIEW_TYPE_ROW_SENT;
			type = getItemViewType(position);

			if (convertView == null)
			{
				holder = new ViewHolder(this);

				switch (type)
				{
					case VIEW_TYPE_ROW_SENT:
						convertView = LayoutInflater.from(Context).inflate(R.layout.sent_msg_row_layout, parent, false);
						holder.viewType = VIEW_TYPE_ROW_SENT;
						break;
					case VIEW_TYPE_ROW_RECEIVED:
						convertView = LayoutInflater.from(Context).inflate(R.layout.received_msg_row_layout, parent, false);
						holder.viewType = VIEW_TYPE_ROW_RECEIVED;
						break;
					case VIEW_TYPE_ROW_SENT_GROUP:
						convertView = LayoutInflater.from(Context).inflate(R.layout.group_sent_msg_row_layout, parent, false);
						holder.viewType = VIEW_TYPE_ROW_SENT_GROUP;
						break;
					case VIEW_TYPE_ROW_RECEIVED_GROUP:
						convertView = LayoutInflater.from(Context).inflate(R.layout.group_received_msg_row_layout, parent, false);
						holder.viewType = VIEW_TYPE_ROW_RECEIVED_GROUP;
						break;
				}
				if (!messagesGroup)
				{
					holder.aliasText = (TextView) convertView.findViewById(R.id.msg_alias);
					holder.timestampText = (TextView) convertView.findViewById(R.id.msg_time);
					holder.arrowView = (ImageView) convertView.findViewById(R.id.arrow_row);

				}
				holder.rowDividerView = (View) convertView.findViewById(R.id.row_divider);
				holder.messageText = (TextView) convertView.findViewById(R.id.msg_text);
				convertView.Tag = holder;
			}
			else
			{
				holder = (ViewHolder) convertView.Tag;
			}
			if (holder.viewType != VIEW_TYPE_ROW_RECEIVED_GROUP && holder.viewType != VIEW_TYPE_ROW_SENT_GROUP)
			{
				//msg alias
				holder.aliasText.Text = message.SenderAlias;

				//msg time
				SimpleDateFormat ft = new SimpleDateFormat("hh:mm a");
				holder.timestampText.Text = ft.format(new DateTime(message.Timestamp)).ToString();
			}
			//msg txt
			holder.messageText.Text = message.Text;

			//divider
			if (position == this.MessagesList.Count - 1)
			{
				holder.rowDividerView.Visibility = View.VISIBLE;
			}
			else
			{
				holder.rowDividerView.Visibility = View.GONE;
			}

			return convertView;
		}

	}

}