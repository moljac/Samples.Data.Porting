using System.Collections.Generic;

namespace com.sinch.android.rtc.sample.messaging
{

	using Activity = android.app.Activity;
	using Pair = android.util.Pair;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using TextView = android.widget.TextView;

	using Message = com.sinch.android.rtc.messaging.Message;


	public class MessageAdapter : BaseAdapter
	{

		public const int DIRECTION_INCOMING = 0;

		public const int DIRECTION_OUTGOING = 1;

		private IList<Pair<Message, int?>> mMessages;

		private SimpleDateFormat mFormatter;

		private LayoutInflater mInflater;

		public MessageAdapter(Activity activity)
		{
			mInflater = activity.LayoutInflater;
			mMessages = new List<Pair<Message, int?>>();
			mFormatter = new SimpleDateFormat("HH:mm");
		}

		public virtual void addMessage(Message message, int direction)
		{
			mMessages.Add(new Pair(message, direction));
			notifyDataSetChanged();
		}

		public override int Count
		{
			get
			{
				return mMessages.Count;
			}
		}

		public override object getItem(int i)
		{
			return mMessages[i];
		}

		public override long getItemId(int i)
		{
			return 0;
		}

		public override int ViewTypeCount
		{
			get
			{
				return 2;
			}
		}

		public override int getItemViewType(int i)
		{
			return mMessages[i].second;
		}

		public override View getView(int i, View convertView, ViewGroup viewGroup)
		{
			int direction = getItemViewType(i);

			if (convertView == null)
			{
				int res = 0;
				if (direction == DIRECTION_INCOMING)
				{
					res = R.layout.message_right;
				}
				else if (direction == DIRECTION_OUTGOING)
				{
					res = R.layout.message_left;
				}
				convertView = mInflater.inflate(res, viewGroup, false);
			}

			Message message = mMessages[i].first;
			string name = message.SenderId;

			TextView txtSender = (TextView) convertView.findViewById(R.id.txtSender);
			TextView txtMessage = (TextView) convertView.findViewById(R.id.txtMessage);
			TextView txtDate = (TextView) convertView.findViewById(R.id.txtDate);

			txtSender.Text = name;
			txtMessage.Text = message.TextBody;
			txtDate.Text = mFormatter.format(message.Timestamp);

			return convertView;
		}
	}

}