using System.Collections;
using System.Collections.Generic;

namespace com.firebase.drawing
{

	using Intent = android.content.Intent;
	using DataSetObserver = android.database.DataSetObserver;
	using Bundle = android.os.Bundle;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using CheckBox = android.widget.CheckBox;
	using ImageView = android.widget.ImageView;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using DataSnapshot = com.firebase.client.DataSnapshot;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using ServerValue = com.firebase.client.ServerValue;
	using ValueEventListener = com.firebase.client.ValueEventListener;


	public class BoardListActivity : ActionBarActivity
	{

		public const string TAG = "AndroidDrawing";
		private static string FIREBASE_URL = "https://doodleboard.firebaseio.com/";

		private Firebase mRef;
		private Firebase mBoardsRef;
		private Firebase mSegmentsRef;
		private FirebaseListAdapter<Hashtable> mBoardListAdapter;
		private ValueEventListener mConnectedListener;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			mRef = new Firebase(FIREBASE_URL);
			mBoardsRef = mRef.child("boardmetas");
			mBoardsRef.keepSynced(true); // keep the board list in sync
			mSegmentsRef = mRef.child("boardsegments");
			SyncedBoardManager.restoreSyncedBoards(mSegmentsRef);
			ContentView = R.layout.activity_board_list;
		}

		protected internal override void onStart()
		{
			base.onStart();

			// Set up a notification to let us know when we're connected or disconnected from the Firebase servers
			mConnectedListener = mRef.Root.child(".info/connected").addValueEventListener(new ValueEventListenerAnonymousInnerClassHelper(this));

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ListView boardList = (android.widget.ListView) this.findViewById(R.id.BoardList);
			ListView boardList = (ListView) this.findViewById(R.id.BoardList);
			mBoardListAdapter = new FirebaseListAdapterAnonymousInnerClassHelper(this, mBoardsRef, typeof(Hashtable), R.layout.board_in_list);
			boardList.Adapter = mBoardListAdapter;
			boardList.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
			mBoardListAdapter.registerDataSetObserver(new DataSetObserverAnonymousInnerClassHelper(this, boardList));
		}

		private class ValueEventListenerAnonymousInnerClassHelper : ValueEventListener
		{
			private readonly BoardListActivity outerInstance;

			public ValueEventListenerAnonymousInnerClassHelper(BoardListActivity outerInstance)
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
					Toast.makeText(outerInstance, "Disconnected from Firebase", Toast.LENGTH_LONG).show();
				}
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				// No-op
			}
		}

		private class FirebaseListAdapterAnonymousInnerClassHelper : FirebaseListAdapter<Hashtable>
		{
			private readonly BoardListActivity outerInstance;

			public FirebaseListAdapterAnonymousInnerClassHelper(BoardListActivity outerInstance, com.firebase.client.Firebase mBoardsRef, UnknownType class, UnknownType board_in_list)
			{
				private readonly BoardListActivity.FirebaseListAdapterAnonymousInnerClassHelper outerInstance;

//JAVA TO C# CONVERTER WARNING: The following constructor is declared outside of its associated class:
//ORIGINAL LINE: public ()
				public (BoardListActivity.FirebaseListAdapterAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				base(outerInstance.outerInstance.mBoardsRef, class, board_in_list, outerInstance);
				this.outerInstance = outerInstance;
			}

			protected void populateView(View v, Hashtable model)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String key = BoardListActivity.this.mBoardListAdapter.getModelKey(model);
				string key = outerInstance.mBoardListAdapter.getModelKey(model);
				((TextView)v.findViewById(R.id.board_title)).Text = key;

				// show if the board is synced and listen for clicks to toggle that state
				CheckBox checkbox = (CheckBox) v.findViewById(R.id.keepSynced);
				checkbox.Checked = SyncedBoardManager.isSynced(key);
				checkbox.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, key);

				// display the board's thumbnail if it is available
				ImageView thumbnailView = (ImageView) v.findViewById(R.id.board_thumbnail);
				if (model.get("thumbnail") != null)
				{
					try
					{
						thumbnailView.ImageBitmap = DrawingActivity.decodeFromBase64(model.get("thumbnail").ToString());
						thumbnailView.Visibility = View.VISIBLE;
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
				else
				{
					thumbnailView.Visibility = View.INVISIBLE;
				}
			}
		}

		private static class OnItemClickListenerAnonymousInnerClassHelper extends AdapterView.OnItemClickListener
		{
			private final BoardListActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(BoardListActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: @Override public void onItemClick(android.widget.AdapterView<?> parent, android.view.View view, int position, long id)
			public void onItemClick(AdapterView<?> parent, View view, int position, long id)
			{
				openBoard(mBoardListAdapter.getModelKey(position));
			}
		}

		private static class DataSetObserverAnonymousInnerClassHelper extends DataSetObserver
		{
			private final BoardListActivity outerInstance;

			private ListView boardList;

			public DataSetObserverAnonymousInnerClassHelper(BoardListActivity outerInstance, ListView boardList)
			{
				this.outerInstance = outerInstance;
				this.boardList = boardList;
			}

			public void onChanged()
			{
				base.onChanged();
				boardList.Selection = mBoardListAdapter.Count - 1;
			}
		}

		protected void onStop()
		{
			base.onStop();
			// Clean up our listener so we don't have it attached twice.
			mRef.Root.child(".info/connected").removeEventListener(mConnectedListener);
			mBoardListAdapter.cleanup();

		}

		private void createBoard()
		{
			// create a new board
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.firebase.client.Firebase newBoardRef = mBoardsRef.push();
			Firebase newBoardRef = mBoardsRef.push();
			IDictionary<string, object> newBoardValues = new Dictionary<string, object>();
			newBoardValues["createdAt"] = ServerValue.TIMESTAMP;
			android.graphics.Point size = new android.graphics.Point();
			WindowManager.DefaultDisplay.getSize(size);
			newBoardValues["width"] = size.x;
			newBoardValues["height"] = size.y;
			newBoardRef.setValue(newBoardValues, new CompletionListenerAnonymousInnerClassHelper(this, newBoardRef));
		}

		private void openBoard(string key)
		{
			Log.i(TAG, "Opening board " + key);
			Toast.makeTextuniquetempvar.show();
			Intent intent = new Intent(this, typeof(DrawingActivity));
			intent.putExtra("FIREBASE_URL", FIREBASE_URL);
			intent.putExtra("BOARD_ID", key);
			startActivity(intent);
		}

		public bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.menu_board_list, menu);
			return true;
		}

		public bool onOptionsItemSelected(MenuItem item)
		{
			// Handle action bar item clicks here. The action bar will
			// automatically handle clicks on the Home/Up button, so long
			// as you specify a parent activity in AndroidManifest.xml.
			int id = item.ItemId;

			Log.i(TAG, "Selected item " + id);

			//noinspection SimplifiableIfStatement
			if (id == R.id.action_settings)
			{
				return true;
			}

			if (id == R.id.action_new_board)
			{
				createBoard();
			}


			return base.onOptionsItemSelected(item);
		}

	}

}