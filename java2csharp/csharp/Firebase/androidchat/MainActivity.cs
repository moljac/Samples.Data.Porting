using System;

namespace com.firebase.androidchat
{

	using ListActivity = android.app.ListActivity;
	using SharedPreferences = android.content.SharedPreferences;
	using DataSetObserver = android.database.DataSetObserver;
	using Bundle = android.os.Bundle;
	using KeyEvent = android.view.KeyEvent;
	using View = android.view.View;
	using EditorInfo = android.view.inputmethod.EditorInfo;
	using EditText = android.widget.EditText;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using DataSnapshot = com.firebase.client.DataSnapshot;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using ValueEventListener = com.firebase.client.ValueEventListener;

	public class MainActivity : ListActivity
	{

		// TODO: change this to your own Firebase URL
		private const string FIREBASE_URL = "https://android-chat.firebaseio-demo.com";

		private string mUsername;
		private Firebase mFirebaseRef;
		private ValueEventListener mConnectedListener;
		private ChatListAdapter mChatListAdapter;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			// Make sure we have a mUsername
			setupUsername();

			Title = "Chatting as " + mUsername;

			// Setup our Firebase mFirebaseRef
			mFirebaseRef = (new Firebase(FIREBASE_URL)).child("chat");

			// Setup our input methods. Enter key on the keyboard or pushing the send button
			EditText inputText = (EditText) findViewById(R.id.messageInput);
			inputText.OnEditorActionListener = new OnEditorActionListenerAnonymousInnerClassHelper(this);

			findViewById(R.id.sendButton).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

		}

		private class OnEditorActionListenerAnonymousInnerClassHelper : TextView.OnEditorActionListener
		{
			private readonly MainActivity outerInstance;

			public OnEditorActionListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool onEditorAction(TextView textView, int actionId, KeyEvent keyEvent)
			{
				if (actionId == EditorInfo.IME_NULL && keyEvent.Action == KeyEvent.ACTION_DOWN)
				{
					outerInstance.sendMessage();
				}
				return true;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.sendMessage();
			}
		}

		public override void onStart()
		{
			base.onStart();
			// Setup our view and list adapter. Ensure it scrolls to the bottom as data changes
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ListView listView = getListView();
			ListView listView = ListView;
			// Tell our list adapter that we only want 50 messages at a time
			mChatListAdapter = new ChatListAdapter(mFirebaseRef.limit(50), this, R.layout.chat_message, mUsername);
			listView.Adapter = mChatListAdapter;
			mChatListAdapter.registerDataSetObserver(new DataSetObserverAnonymousInnerClassHelper(this, listView));

			// Finally, a little indication of connection status
			mConnectedListener = mFirebaseRef.Root.child(".info/connected").addValueEventListener(new ValueEventListenerAnonymousInnerClassHelper(this));
		}

		private class DataSetObserverAnonymousInnerClassHelper : DataSetObserver
		{
			private readonly MainActivity outerInstance;

			private ListView listView;

			public DataSetObserverAnonymousInnerClassHelper(MainActivity outerInstance, ListView listView)
			{
				this.outerInstance = outerInstance;
				this.listView = listView;
			}

			public override void onChanged()
			{
				base.onChanged();
				listView.Selection = outerInstance.mChatListAdapter.Count - 1;
			}
		}

		private class ValueEventListenerAnonymousInnerClassHelper : ValueEventListener
		{
			private readonly MainActivity outerInstance;

			public ValueEventListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onDataChange(DataSnapshot dataSnapshot)
			{
				bool connected = (bool?) dataSnapshot.Value;
				if (connected)
				{
					Toast.makeText(outerInstance, "Connected to Firebase", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(outerInstance, "Disconnected from Firebase", Toast.LENGTH_SHORT).show();
				}
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				// No-op
			}
		}

		public override void onStop()
		{
			base.onStop();
			mFirebaseRef.Root.child(".info/connected").removeEventListener(mConnectedListener);
			mChatListAdapter.cleanup();
		}

		private void setupUsername()
		{
			SharedPreferences prefs = Application.getSharedPreferences("ChatPrefs", 0);
			mUsername = prefs.getString("username", null);
			if (mUsername == null)
			{
				Random r = new Random();
				// Assign a random user name if we don't have one saved.
				mUsername = "JavaUser" + r.Next(100000);
				prefs.edit().putString("username", mUsername).commit();
			}
		}

		private void sendMessage()
		{
			EditText inputText = (EditText) findViewById(R.id.messageInput);
			string input = inputText.Text.ToString();
			if (!input.Equals(""))
			{
				// Create our 'model', a Chat object
				Chat chat = new Chat(input, mUsername);
				// Create a new, auto-generated child of that chat location, and save our chat data there
				mFirebaseRef.push().Value = chat;
				inputText.Text = "";
			}
		}
	}

}