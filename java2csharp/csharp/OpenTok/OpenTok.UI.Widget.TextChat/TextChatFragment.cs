using System;
using System.Collections.Generic;

namespace com.opentok.android.ui.textchat.widget
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Fragment = android.app.Fragment;
	using Context = android.content.Context;
	using Handler = android.os.Handler;
	using Editable = android.text.Editable;
	using TextWatcher = android.text.TextWatcher;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;


	/// <summary>
	/// A Fragment for adding and controling the text chat user interface.
	/// </summary>
	public class TextChatFragment : Fragment
	{

		private const string LOG_TAG = "TextChatFragment";

		private Context mContext;
		internal Handler mHandler;

		private List<ChatMessage> mMsgsList = new List<ChatMessage>();
		private MessageAdapter mMessageAdapter;

		private ListView mListView;
		private Button mSendButton;
		private EditText mMsgEditText;
		private TextView mMsgCharsView;
		private TextView mMsgNotificationView;
		private View mMsgDividerView;

		private int maxTextLength = 1000; // By default the maximum length is 1000.

		private string senderId;
		private string senderAlias;

		public TextChatFragment()
		{
			//Init the sender information for the output messages
			this.senderId = UUID.randomUUID().ToString();
			this.senderAlias = "me";
			Log.i(LOG_TAG, "senderstuff  " + this.senderId + this.senderAlias);
		}

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			mContext = activity.ApplicationContext;
		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			View rootView = inflater.inflate(R.layout.textchat_fragment_layout, container, false);

			mListView = (ListView) rootView.findViewById(R.id.msgs_list);
			mSendButton = (Button) rootView.findViewById(R.id.send_button);
			mMsgCharsView = (TextView) rootView.findViewById(R.id.characteres_msg);
			mMsgCharsView.Text = maxTextLength.ToString();
			mMsgNotificationView = (TextView) rootView.findViewById(R.id.new_msg_notification);
			mMsgEditText = (EditText) rootView.findViewById(R.id.edit_msg);
			mMsgDividerView = (View) rootView.findViewById(R.id.divider_notification);
			mMsgEditText.addTextChangedListener(mTextEditorWatcher);

			mSendButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mMessageAdapter = new MessageAdapter(Activity, R.layout.sent_msg_row_layout, mMsgsList);

			mListView.Adapter = mMessageAdapter;
			mMsgNotificationView.OnTouchListener = new OnTouchListenerAnonymousInnerClassHelper(this);

			return rootView;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly TextChatFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(TextChatFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				outerInstance.sendMessage();
			}
		}

		private class OnTouchListenerAnonymousInnerClassHelper : View.OnTouchListener
		{
			private readonly TextChatFragment outerInstance;

			public OnTouchListenerAnonymousInnerClassHelper(TextChatFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool onTouch(View v, MotionEvent @event)
			{
				outerInstance.showMsgNotification(false);
				outerInstance.mListView.smoothScrollToPosition(outerInstance.mMessageAdapter.MessagesList.Count - 1);
				return false;
			}
		}

		/// <summary>
		/// An interface for receiving events when a text chat message is ready to send.
		/// </summary>
		public interface TextChatListener
		{
			/// <summary>
			/// Called when a message in the TextChatFragment is ready to send. A message is
			/// ready to send when the user clicks the Send button in the TextChatFragment
			/// user interface.
			/// </summary>
			bool onMessageReadyToSend(ChatMessage msg);
		}

		private TextChatListener textChatListener;

		/// <summary>
		/// Set the object that receives events for this TextChatListener.
		/// </summary>
		public virtual void setTextChatListener(TextChatListener textChatListener)
		{
			this.textChatListener = textChatListener;
		}

		/// <summary>
		/// Set the maximum length of a text chat message (in characters).
		/// </summary>
		public virtual int MaxTextLength
		{
			set
			{
				maxTextLength = value;
			}
		}

		/// <summary>
		/// Set the sender alias and the sender ID of the outgoing messages.
		/// </summary>
		public virtual void setSenderInfo(string senderId, string senderAlias)
		{
			if (senderAlias == null || senderId == null)
			{
				throw new System.ArgumentException("The sender alias and the sender id cannot be null");
			}
			this.senderAlias = senderAlias;
			this.senderId = senderId;
		}
		/// <summary>
		/// Add a message to the TextChatListener received message list.
		/// </summary>
		public virtual void addMessage(ChatMessage msg)
		{
			Log.i(LOG_TAG, "New message " + msg.Text + " is ready to be added.");

			if (msg != null)
			{

				//check the origin of the message
				if (msg.SenderId != this.senderId)
				{
					msg.Status = ChatMessage.MessageStatus.RECEIVED_MESSAGE;
				}

				bool visible = NewMessageVisible;
				mMsgNotificationView.TextColor = Resources.getColor(R.color.text);
				mMsgNotificationView.Text = "New messages";
				showMsgNotification(visible);

				//generate message timestamp
				DateTime date = DateTime.Now;
				if (msg.Timestamp == 0)
				{
					msg.Timestamp = date.Ticks;
				}

				mMessageAdapter.add(msg);
			}
		}

		// Called when the user clicks the send button.
		private void sendMessage()
		{
			//checkMessage
			mMsgEditText.Enabled = false;
			string msgStr = mMsgEditText.Text.ToString();
			if (msgStr.Length > 0)
			{

				if (msgStr.Length > maxTextLength)
				{
					showError();
				}
				else
				{
					ChatMessage myMsg = new ChatMessage(senderId, senderAlias, msgStr, ChatMessage.MessageStatus.SENT_MESSAGE);
					bool msgError = onMessageReadyToSend(myMsg);

					if (msgError)
					{
						Log.d(LOG_TAG, "Error to send the message");
						showError();

					}
					else
					{
						mMsgEditText.Enabled = true;
						mMsgEditText.Focusable = true;
						mMsgEditText.Text = "";
						mMsgCharsView.TextColor = Resources.getColor(R.color.info);
						mListView.smoothScrollToPosition(mMessageAdapter.Count);

						//add the message to the component
						addMessage(myMsg);
					}

				}

			}
			else
			{
				mMsgEditText.Enabled = true;
			}
		}
		// Add a notification about a new message
		private void showMsgNotification(bool visible)
		{
			if (visible)
			{
				mMsgDividerView.Visibility = View.VISIBLE;
				mMsgNotificationView.Visibility = View.VISIBLE;
			}
			else
			{
				mMsgNotificationView.Visibility = View.GONE;
				mMsgDividerView.Visibility = View.GONE;
			}
		}

		// To check if the next item is visible in the list
		private bool NewMessageVisible
		{
			get
			{
				int last = mListView.LastVisiblePosition;
				int transpose = 0;
				View currentBottomView;
				currentBottomView = mListView.getChildAt(last);
    
				if (mListView.Count > 1)
				{
					while (currentBottomView == null)
					{
						transpose++;
						currentBottomView = mListView.getChildAt(last - transpose);
					}
					if (last == mListView.Count - 1 && currentBottomView.Bottom <= mListView.Height)
					{
						mListView.ScrollContainer = false;
						return false;
					}
					else
					{
						mListView.ScrollContainer = true;
						return true;
					}
				}
				return false;
			}
		}


		private void showError()
		{
			mMsgEditText.Enabled = true;
			mMsgEditText.Focusable = true;
			mMsgNotificationView.Text = "Unable to send message. Retry";
			mMsgNotificationView.TextColor = Color.RED;
			showMsgNotification(true);
		}

		/// <summary>
		/// Called when a message in the TextChatFragment is ready to send. A message is
		/// ready to send when the user clicks the Send button in the TextChatFragment
		/// user interface.
		/// 
		/// If you subclass the TextChatFragment class and implement this method,
		/// you do not need to set a TextChatListener.
		/// </summary>
		protected internal virtual bool onMessageReadyToSend(ChatMessage msg)
		{
			if (this.textChatListener != null)
			{
				Log.d(LOG_TAG, "onMessageReadyToSend");
				return this.textChatListener.onMessageReadyToSend(msg);
			}
			return false;
		}

		// Count down the characters left.
		private TextWatcher mTextEditorWatcher = new TextWatcherAnonymousInnerClassHelper();

		private class TextWatcherAnonymousInnerClassHelper : TextWatcher
		{
			public TextWatcherAnonymousInnerClassHelper()
			{
			}


			public virtual void beforeTextChanged(CharSequence s, int start, int count, int after)
			{
			}

			public virtual void onTextChanged(CharSequence s, int start, int before, int count)
			{
				int chars_left = outerInstance.maxTextLength - s.length();

				outerInstance.mMsgCharsView.Text = (outerInstance.maxTextLength - s.length()).ToString();
				if (chars_left < 10)
				{
					outerInstance.mMsgCharsView.TextColor = Color.RED;
				}
			}

			public override void afterTextChanged(Editable s)
			{
			}
		}

	}

}