using System.Collections.Generic;

namespace com.opentok.android.demo.multiparty
{

	using Context = android.content.Context;
	using PagerAdapter = android.support.v4.view.PagerAdapter;
	using ViewPager = android.support.v4.view.ViewPager;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using RelativeLayout = android.widget.RelativeLayout;
	using LayoutParams = android.widget.RelativeLayout.LayoutParams;
	using ScrollView = android.widget.ScrollView;
	using TextView = android.widget.TextView;

	using OpenTokConfig = com.opentok.android.demo.config.OpenTokConfig;


	public class MySession : Session
	{

		private Context mContext;

		// Interface
		private ViewPager mSubscribersViewContainer;
		private ViewGroup mPreview;
		private TextView mMessageView;
		private ScrollView mMessageScroll;

		// Players status
		private List<MySubscriber> mSubscribers = new List<MySubscriber>();
		private Dictionary<Stream, MySubscriber> mSubscriberStream = new Dictionary<Stream, MySubscriber>();
		private Dictionary<string, MySubscriber> mSubscriberConnection = new Dictionary<string, MySubscriber>();

		private PagerAdapter mPagerAdapter = new PagerAdapterAnonymousInnerClassHelper();

		private class PagerAdapterAnonymousInnerClassHelper : PagerAdapter
		{
			public PagerAdapterAnonymousInnerClassHelper()
			{
			}


			public override bool isViewFromObject(View arg0, object arg1)
			{
				return ((MySubscriber) arg1).View == arg0;
			}

			public override int Count
			{
				get
				{
					return outerInstance.mSubscribers.Count;
				}
			}

			public override CharSequence getPageTitle(int position)
			{
				if (position < outerInstance.mSubscribers.Count)
				{
					return outerInstance.mSubscribers[position].Name;
				}
				else
				{
					return null;
				}
			}

			public override object instantiateItem(ViewGroup container, int position)
			{
				MySubscriber p = outerInstance.mSubscribers[position];
				container.addView(p.View);
				return p;
			}

			public override void setPrimaryItem(ViewGroup container, int position, object @object)
			{
				foreach (MySubscriber p in outerInstance.mSubscribers)
				{
					if (p == @object)
					{
						if (!p.SubscribeToVideo)
						{
							p.SubscribeToVideo = true;
						}
					}
					else
					{
						if (p.SubscribeToVideo)
						{
							p.SubscribeToVideo = false;
						}
					}
				}
			}

			public override void destroyItem(ViewGroup container, int position, object @object)
			{
				MySubscriber p = (MySubscriber) @object;
				container.removeView(p.View);
			}

			public override int getItemPosition(object @object)
			{
				for (int i = 0; i < outerInstance.mSubscribers.Count; i++)
				{
					if (outerInstance.mSubscribers[i] == @object)
					{
						return i;
					}
				}
				return POSITION_NONE;
			}

		}

		public MySession(Context context) : base(context, OpenTokConfig.API_KEY, OpenTokConfig.SESSION_ID)
		{
			this.mContext = context;
		}

		// public methods
		public virtual ViewPager PlayersViewContainer
		{
			set
			{
				this.mSubscribersViewContainer = value;
				this.mSubscribersViewContainer.Adapter = mPagerAdapter;
				mPagerAdapter.notifyDataSetChanged();
			}
		}

		public virtual void setMessageView(TextView et, ScrollView scroller)
		{
			this.mMessageView = et;
			this.mMessageScroll = scroller;
		}

		public virtual ViewGroup PreviewView
		{
			set
			{
				this.mPreview = value;
			}
		}

		public virtual void connect()
		{
			this.connect(OpenTokConfig.TOKEN);
		}

		public virtual void sendChatMessage(string message)
		{
			sendSignal("chat", message);
			presentMessage("Me", message);
		}

		// callbacks
		protected internal override void onConnected()
		{
			Publisher p = new Publisher(mContext, "MyPublisher");
			publish(p);

			// Add video preview
			RelativeLayout.LayoutParams lp = new RelativeLayout.LayoutParams(RelativeLayout.LayoutParams.MATCH_PARENT, RelativeLayout.LayoutParams.MATCH_PARENT);
			mPreview.addView(p.View, lp);
			p.setStyle(BaseVideoRenderer.STYLE_VIDEO_SCALE, BaseVideoRenderer.STYLE_VIDEO_FILL);

			presentText("Welcome to OpenTok Chat.");
		}

		protected internal override void onStreamReceived(Stream stream)
		{
			MySubscriber p = new MySubscriber(mContext, stream);

			// we can use connection data to obtain each user id
			p.UserId = stream.Connection.Data;

			// Subscribe audio only if we have more than one player
			if (mSubscribers.Count != 0)
			{
				p.SubscribeToVideo = false;
			}

			// Subscribe to this player
			this.subscribe(p);

			mSubscribers.Add(p);
			mSubscriberStream[stream] = p;
			mSubscriberConnection[stream.Connection.ConnectionId] = p;
			mPagerAdapter.notifyDataSetChanged();

			presentText("\n" + p.Name + " has joined the chat");
		}

		protected internal override void onStreamDropped(Stream stream)
		{
			MySubscriber p = mSubscriberStream[stream];
			if (p != null)
			{
				mSubscribers.Remove(p);
				mSubscriberStream.Remove(stream);
				mSubscriberConnection.Remove(stream.Connection.ConnectionId);
				mPagerAdapter.notifyDataSetChanged();

				presentText("\n" + p.Name + " has left the chat");
			}
		}

		protected internal override void onSignalReceived(string type, string data, Connection connection)
		{

			if (type != null && "chat".Equals(type))
			{
				string mycid = this.Connection.ConnectionId;
				string cid = connection.ConnectionId;
				if (!cid.Equals(mycid))
				{
					MySubscriber p = mSubscriberConnection[cid];
					if (p != null)
					{
						presentMessage(p.Name, data);
					}
				}
			}
		}

		private void presentMessage(string who, string message)
		{
			presentText("\n" + who + ": " + message);
		}

		private void presentText(string message)
		{
			mMessageView.Text = mMessageView.Text + message;
			mMessageScroll.post(() =>
			{
				int totalHeight = mMessageView.Height;
				mMessageScroll.smoothScrollTo(0, totalHeight);
			});
		}

	}

}