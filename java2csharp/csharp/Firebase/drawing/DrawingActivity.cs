using System;
using System.Collections.Generic;

namespace com.firebase.drawing
{

	using Intent = android.content.Intent;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Paint = android.graphics.Paint;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using ViewGroup = android.view.ViewGroup;
	using Toast = android.widget.Toast;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;

	using DataSnapshot = com.firebase.client.DataSnapshot;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using ValueEventListener = com.firebase.client.ValueEventListener;


	public class DrawingActivity : ActionBarActivity, ColorPickerDialog.OnColorChangedListener
	{
		public const int THUMBNAIL_SIZE = 256;

		private static readonly int COLOR_MENU_ID = Menu.FIRST;
		private static readonly int CLEAR_MENU_ID = COLOR_MENU_ID + 1;
		private static readonly int PIN_MENU_ID = CLEAR_MENU_ID + 1;
		public const string TAG = "AndroidDrawing";

		private DrawingView mDrawingView;
		private Firebase mFirebaseRef; // Firebase base URL
		private Firebase mMetadataRef;
		private Firebase mSegmentsRef;
		private ValueEventListener mConnectedListener;
		private string mBoardId;
		private int mBoardWidth;
		private int mBoardHeight;

		/// <summary>
		/// Called when the activity is first created.
		/// </summary>
		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			Intent intent = Intent;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String url = intent.getStringExtra("FIREBASE_URL");
			string url = intent.getStringExtra("FIREBASE_URL");
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String boardId = intent.getStringExtra("BOARD_ID");
			string boardId = intent.getStringExtra("BOARD_ID");
			Log.i(TAG, "Adding DrawingView on " + url + " for boardId " + boardId);
			mFirebaseRef = new Firebase(url);
			mBoardId = boardId;
			mMetadataRef = mFirebaseRef.child("boardmetas").child(boardId);
			mSegmentsRef = mFirebaseRef.child("boardsegments").child(mBoardId);
			mMetadataRef.addValueEventListener(new ValueEventListenerAnonymousInnerClassHelper(this, boardId));
		}

		private class ValueEventListenerAnonymousInnerClassHelper : ValueEventListener
		{
			private readonly DrawingActivity outerInstance;

			private string boardId;

			public ValueEventListenerAnonymousInnerClassHelper(DrawingActivity outerInstance, string boardId)
			{
				this.outerInstance = outerInstance;
				this.boardId = boardId;
			}

			public override void onDataChange(DataSnapshot dataSnapshot)
			{
				if (outerInstance.mDrawingView != null)
				{
					((ViewGroup) outerInstance.mDrawingView.Parent).removeView(outerInstance.mDrawingView);
					outerInstance.mDrawingView.cleanup();
					outerInstance.mDrawingView = null;
				}
				IDictionary<string, object> boardValues = (IDictionary<string, object>) dataSnapshot.Value;
				if (boardValues != null && boardValues["width"] != null && boardValues["height"] != null)
				{
					outerInstance.mBoardWidth = ((long?) boardValues["width"]).intValue();
					outerInstance.mBoardHeight = ((long?) boardValues["height"]).intValue();

					outerInstance.mDrawingView = new DrawingView(outerInstance, outerInstance.mFirebaseRef.child("boardsegments").child(boardId), outerInstance.mBoardWidth, outerInstance.mBoardHeight);
					ContentView = outerInstance.mDrawingView;
				}
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				// No-op
			}
		}

		public override void onStart()
		{
			base.onStart();
			// Set up a notification to let us know when we're connected or disconnected from the Firebase servers
			mConnectedListener = mFirebaseRef.Root.child(".info/connected").addValueEventListener(new ValueEventListenerAnonymousInnerClassHelper2(this));
		}

		private class ValueEventListenerAnonymousInnerClassHelper2 : ValueEventListener
		{
			private readonly DrawingActivity outerInstance;

			public ValueEventListenerAnonymousInnerClassHelper2(DrawingActivity outerInstance)
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
			// Clean up our listener so we don't have it attached twice.
			mFirebaseRef.Root.child(".info/connected").removeEventListener(mConnectedListener);
			if (mDrawingView != null)
			{
				mDrawingView.cleanup();
			}
			updateThumbnail(mBoardWidth, mBoardHeight, mSegmentsRef, mMetadataRef);
		}


		public override bool onCreateOptionsMenu(Menu menu)
		{
			base.onCreateOptionsMenu(menu);
			// getMenuInflater().inflate(R.menu.menu_drawing, menu);

			menu.add(0, COLOR_MENU_ID, 0, "Color").setShortcut('3', 'c').setShowAsAction(MenuItem.SHOW_AS_ACTION_IF_ROOM);
			menu.add(0, CLEAR_MENU_ID, 2, "Clear").setShortcut('5', 'x');
			menu.add(0, PIN_MENU_ID, 3, "Keep in sync").setShortcut('6', 's').setIcon(android.R.drawable.ic_lock_lock).setCheckable(true).setChecked(SyncedBoardManager.isSynced(mBoardId));

			return true;
		}

		public override bool onPrepareOptionsMenu(Menu menu)
		{
			base.onPrepareOptionsMenu(menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			if (item.ItemId == COLOR_MENU_ID)
			{
				(new ColorPickerDialog(this, this, unchecked((int)0xFFFF0000))).show();
				return true;
			}
			else if (item.ItemId == CLEAR_MENU_ID)
			{
				mDrawingView.cleanup();
				mSegmentsRef.removeValue(new CompletionListenerAnonymousInnerClassHelper(this));

				return true;
			}
			else if (item.ItemId == PIN_MENU_ID)
			{
				SyncedBoardManager.toggle(mFirebaseRef.child("boardsegments"), mBoardId);
				item.Checked = SyncedBoardManager.isSynced(mBoardId);
				return true;
			}
			else
			{
				return base.onOptionsItemSelected(item);
			}
		}

		private class CompletionListenerAnonymousInnerClassHelper : Firebase.CompletionListener
		{
			private readonly DrawingActivity outerInstance;

			public CompletionListenerAnonymousInnerClassHelper(DrawingActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onComplete(FirebaseError firebaseError, Firebase firebase)
			{
				if (firebaseError != null)
				{
					throw firebaseError.toException();
				}
				outerInstance.mDrawingView = new DrawingView(outerInstance, outerInstance.mFirebaseRef.child("boardsegments").child(outerInstance.mBoardId), outerInstance.mBoardWidth, outerInstance.mBoardHeight);
				ContentView = outerInstance.mDrawingView;
				//mDrawingView.clear();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void updateThumbnail(int boardWidth, int boardHeight, com.firebase.client.Firebase segmentsRef, final com.firebase.client.Firebase metadataRef)
		public static void updateThumbnail(int boardWidth, int boardHeight, Firebase segmentsRef, Firebase metadataRef)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float scale = Math.min(1.0f * THUMBNAIL_SIZE / boardWidth, 1.0f * THUMBNAIL_SIZE / boardHeight);
			float scale = Math.Min(1.0f * THUMBNAIL_SIZE / boardWidth, 1.0f * THUMBNAIL_SIZE / boardHeight);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.Bitmap b = android.graphics.Bitmap.createBitmap(Math.round(boardWidth * scale), Math.round(boardHeight * scale), android.graphics.Bitmap.Config.ARGB_8888);
			Bitmap b = Bitmap.createBitmap(Math.Round(boardWidth * scale), Math.Round(boardHeight * scale), Bitmap.Config.ARGB_8888);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.Canvas buffer = new android.graphics.Canvas(b);
			Canvas buffer = new Canvas(b);

			buffer.drawRect(0, 0, b.Width, b.Height, DrawingView.paintFromColor(Color.WHITE, Paint.Style.FILL_AND_STROKE));
			Log.i(TAG, "Generating thumbnail of " + b.Width + "x" + b.Height);

			segmentsRef.addListenerForSingleValueEvent(new ValueEventListenerAnonymousInnerClassHelper3(metadataRef, scale, b, buffer));
		}

		private class ValueEventListenerAnonymousInnerClassHelper3 : ValueEventListener
		{
			private Firebase metadataRef;
			private float scale;
			private Bitmap b;
			private Canvas buffer;

			public ValueEventListenerAnonymousInnerClassHelper3(Firebase metadataRef, float scale, Bitmap b, Canvas buffer)
			{
				this.metadataRef = metadataRef;
				this.scale = scale;
				this.b = b;
				this.buffer = buffer;
			}

			public override void onDataChange(DataSnapshot dataSnapshot)
			{
				foreach (DataSnapshot segmentSnapshot in dataSnapshot.Children)
				{
					Segment segment = segmentSnapshot.getValue(typeof(Segment));
					buffer.drawPath(DrawingView.getPathForPoints(segment.Points, scale), DrawingView.paintFromColor(segment.Color));
				}
				string encoded = encodeToBase64(b);
				metadataRef.child("thumbnail").setValue(encoded, new CompletionListenerAnonymousInnerClassHelper2(this));
			}

			private class CompletionListenerAnonymousInnerClassHelper2 : Firebase.CompletionListener
			{
				private readonly ValueEventListenerAnonymousInnerClassHelper3 outerInstance;

				public CompletionListenerAnonymousInnerClassHelper2(ValueEventListenerAnonymousInnerClassHelper3 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void onComplete(FirebaseError firebaseError, Firebase firebase)
				{
					if (firebaseError != null)
					{
						Log.e(TAG, "Error updating thumbnail", firebaseError.toException());
					}
				}
			}

			public override void onCancelled(FirebaseError firebaseError)
			{

			}
		}

		public static string encodeToBase64(Bitmap image)
		{
			ByteArrayOutputStream baos = new ByteArrayOutputStream();
			image.compress(Bitmap.CompressFormat.PNG, 100, baos);
			sbyte[] b = baos.toByteArray();
			string imageEncoded = com.firebase.client.utilities.Base64.encodeBytes(b);

			return imageEncoded;
		}
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static android.graphics.Bitmap decodeFromBase64(String input) throws java.io.IOException
		public static Bitmap decodeFromBase64(string input)
		{
			sbyte[] decodedByte = com.firebase.client.utilities.Base64.decode(input);
			return BitmapFactory.decodeByteArray(decodedByte, 0, decodedByte.Length);
		}

		public virtual void colorChanged(int newColor)
		{
			mDrawingView.Color = newColor;
		}
	}

}