namespace com.opentok.android.ui.textchat.widget
{

	/// <summary>
	/// Defines the chat message object that you pass into the
	/// <seealso cref="com.opentok.android.ui.textchat.widget.TextChatFragment#addMessage(ChatMessage msg)"/>
	/// method.
	/// </summary>
	public class ChatMessage
	{

		/// <summary>
		/// Defines the status of the message (whether it was a sent or received message).
		/// </summary>
		internal enum MessageStatus
		{
			/// <summary>
			/// The status for a sent message.
			/// </summary>
			SENT_MESSAGE,
			/// <summary>
			/// The status for a received message.
			/// </summary>
			RECEIVED_MESSAGE
		}

		protected internal string senderId;
		protected internal string senderAlias;
		protected internal string text;
		protected internal long timestamp;
		internal MessageStatus status;
		private UUID id;

		/// <summary>
		/// Construct a chat message that includes a message string, a sender identifier, and a sender
		/// alias.
		/// </summary>
		/// <param name="senderId"> The unique ID string for the sender of the of the message. The
		/// TextChatFragment uses this ID to group messages from the same sender in the user interface.
		/// </param>
		/// <param name="senderAlias"> The string (alias) identifying the sender of the message.
		/// </param>
		/// <param name="text"> The text of the message. </param>
		public ChatMessage(string senderId, string senderAlias, string text)
		{
			if (senderAlias == null || senderId == null)
			{
				throw new System.ArgumentException("The sender alias and the sender id cannot be null");
			}
			this.senderId = senderId;
			this.senderAlias = senderAlias;
			this.text = text;
			this.id = UUID.randomUUID();
		}

		internal ChatMessage(string senderId, string sender, string text, MessageStatus status)
		{
			if (sender == null || senderId == null)
			{
				throw new System.ArgumentException("The senderId and sender values cannot be null.");
			}
			this.senderId = senderId;
			this.senderAlias = sender;
			this.text = text;
			this.id = UUID.randomUUID();
			this.status = status;
		}

		/// <summary>
		/// Returns the unique ID of the sender.
		/// </summary>
		public virtual string SenderId
		{
			get
			{
				return senderId;
			}
			set
			{
				this.senderId = value;
			}
		}


		/// <summary>
		/// Returns the sender alias for the message.
		/// </summary>
		public virtual string SenderAlias
		{
			get
			{
				return senderAlias;
			}
		}

		/// <summary>
		/// Sets the sender alias for the message.
		/// </summary>
		public virtual string Sender
		{
			set
			{
				if (value == null)
				{
					throw new System.ArgumentException("The sender value cannot be null.");
				}
				this.senderAlias = senderAlias;
			}
		}

		/// <summary>
		/// Returns the text of the message.
		/// </summary>
		public virtual string Text
		{
			get
			{
				return text;
			}
			set
			{
				this.text = value;
			}
		}


		/// <summary>
		/// Returns the sent/received status of the message.
		/// </summary>
		internal virtual MessageStatus Status
		{
			get
			{
				return status;
			}
			set
			{
				this.status = value;
			}
		}


		/// <summary>
		/// Sets the UNIX timestamp for the message.
		/// </summary>
		public virtual long Timestamp
		{
			set
			{
				timestamp = value;
			}
			get
			{
				return timestamp;
			}
		}


		/// <summary>
		/// Returns the unique identifier for the message. (This is a unique identifier for
		/// the message, not the sender.)
		/// </summary>
		public virtual UUID Id
		{
			get
			{
				return id;
			}
		}

	}

}