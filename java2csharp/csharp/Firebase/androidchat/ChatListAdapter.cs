namespace com.firebase.androidchat
{

	using Activity = android.app.Activity;
	using Color = android.graphics.Color;
	using View = android.view.View;
	using TextView = android.widget.TextView;

	using Query = com.firebase.client.Query;

	/// <summary>
	/// @author greg
	/// @since 6/21/13
	/// 
	/// This class is an example of how to use FirebaseListAdapter. It uses the <code>Chat</code> class to encapsulate the
	/// data for each individual chat message
	/// </summary>
	public class ChatListAdapter : FirebaseListAdapter<Chat>
	{

		// The mUsername for this client. We use this to indicate which messages originated from this user
		private string mUsername;

		public ChatListAdapter(Query @ref, Activity activity, int layout, string mUsername) : base(@ref, typeof(Chat), layout, activity)
		{
			this.mUsername = mUsername;
		}

		/// <summary>
		/// Bind an instance of the <code>Chat</code> class to our view. This method is called by <code>FirebaseListAdapter</code>
		/// when there is a data change, and we are given an instance of a View that corresponds to the layout that we passed
		/// to the constructor, as well as a single <code>Chat</code> instance that represents the current data to bind.
		/// </summary>
		/// <param name="view"> A view instance corresponding to the layout we passed to the constructor. </param>
		/// <param name="chat"> An instance representing the current state of a chat message </param>
		protected internal override void populateView(View view, Chat chat)
		{
			// Map a Chat object to an entry in our listview
			string author = chat.Author;
			TextView authorText = (TextView) view.findViewById(R.id.author);
			authorText.Text = author + ": ";
			// If the message was sent by this user, color it differently
			if (author != null && author.Equals(mUsername))
			{
				authorText.TextColor = Color.RED;
			}
			else
			{
				authorText.TextColor = Color.BLUE;
			}
			((TextView) view.findViewById(R.id.message)).Text = chat.Message;
		}
	}

}