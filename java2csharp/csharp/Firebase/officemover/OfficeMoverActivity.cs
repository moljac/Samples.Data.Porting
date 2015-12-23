using System.Collections.Generic;

namespace com.firebase.officemover
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using Menu = android.view.Menu;
	using MenuInflater = android.view.MenuInflater;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using EditText = android.widget.EditText;
	using FrameLayout = android.widget.FrameLayout;
	using PopupMenu = android.widget.PopupMenu;
	using Toast = android.widget.Toast;

	using AuthData = com.firebase.client.AuthData;
	using ChildEventListener = com.firebase.client.ChildEventListener;
	using DataSnapshot = com.firebase.client.DataSnapshot;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using ValueEventListener = com.firebase.client.ValueEventListener;
	using OfficeLayout = com.firebase.officemover.model.OfficeLayout;
	using OfficeThing = com.firebase.officemover.model.OfficeThing;



	/// <summary>
	/// @author Jenny Tong (mimming)
	/// 
	/// This is the main Activity for Office Mover. It manages the Firebase client and all of the
	/// listeners.
	/// </summary>
	public class OfficeMoverActivity : Activity
	{
		private static readonly string TAG = typeof(OfficeMoverActivity).Name;

		//TODO: Update to your Firebase
		public const string FIREBASE = "https://<your-firebase>.firebaseio.com/";

		// How often (in ms) we push write updates to Firebase
		private const int UPDATE_THROTTLE_DELAY = 40;

		// The Firebase client
		private Firebase mFirebaseRef;

		// The office layout
		private OfficeLayout mOfficeLayout;

		// The currently selected thing in the office
		private OfficeThing mSelectedThing;

		// A list of elements to be written to Firebase on the next push
		private Dictionary<string, OfficeThing> mStuffToUpdate = new Dictionary<string, OfficeThing>();

		// View stuff
		private OfficeCanvasView mOfficeCanvasView;
		private FrameLayout mOfficeFloorView;
		private Menu mActionMenu;

		public abstract class ThingChangeListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public ThingChangeListener(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public abstract void thingChanged(string key, OfficeThing officeThing);
		}

		public abstract class SelectedThingChangeListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public SelectedThingChangeListener(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public abstract void thingChanged(OfficeThing officeThing);
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_office_mover;

			// Initialize Firebase
			mFirebaseRef = new Firebase(FIREBASE);

			// Process authentication
			Bundle extras = Intent.Extras;
			string authToken;
			if (extras != null)
			{
				authToken = extras.getString(LoginActivity.AUTH_TOKEN_EXTRA);
			}
			else
			{
				Log.w(TAG, "Users must be authenticated to do this activity. Redirecting to login activity.");
				Intent loginIntent = new Intent(ApplicationContext, typeof(LoginActivity));
				loginIntent.Flags = Intent.FLAG_ACTIVITY_CLEAR_TOP;
				startActivity(loginIntent);
				finish();
				return;
			}

			mFirebaseRef.authWithOAuthToken("google", authToken, new AuthResultHandlerAnonymousInnerClassHelper(this));

			// Initialize the view stuff
			mOfficeLayout = new OfficeLayout();
			mOfficeCanvasView = (OfficeCanvasView) findViewById(R.id.office_canvas);
			mOfficeCanvasView.OfficeLayout = mOfficeLayout;
			mOfficeFloorView = (FrameLayout) findViewById(R.id.office_floor);

			// Listen for floor changes
			mFirebaseRef.child("background").addValueEventListener(new ValueEventListenerAnonymousInnerClassHelper(this));

			// Listen for furniture changes
			mFirebaseRef.child("furniture").addChildEventListener(new ChildEventListenerAnonymousInnerClassHelper(this));

			// Handles menu changes that happen when an office thing is selected or de-selected
			mOfficeCanvasView.ThingFocusChangeListener = new SelectedThingChangeListenerAnonymousInnerClassHelper(this);

			// Triggers whenever an office thing changes on the screen. This binds the
			// user interface to the scheduler that throttles updates to Firebase
			mOfficeCanvasView.ThingChangedListener = new ThingChangeListenerAnonymousInnerClassHelper(this);

			// A scheduled executor that throttles updates to Firebase to about 40ms each.
			// This prevents the high frequency change events from swamping Firebase.
			ScheduledExecutorService firebaseUpdateScheduler = Executors.newScheduledThreadPool(1);
			firebaseUpdateScheduler.scheduleAtFixedRate(() =>
			{
				if (mStuffToUpdate != null && mStuffToUpdate.Count > 0)
				{
					foreach (OfficeThing officeThing in mStuffToUpdate.Values)
					{
						updateOfficeThing(officeThing.Key, officeThing);
						mStuffToUpdate.Remove(officeThing.Key);
					}
				}
			}, UPDATE_THROTTLE_DELAY, UPDATE_THROTTLE_DELAY, TimeUnit.MILLISECONDS);
		}

		private class AuthResultHandlerAnonymousInnerClassHelper : Firebase.AuthResultHandler
		{
			private readonly OfficeMoverActivity outerInstance;

			public AuthResultHandlerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onAuthenticated(AuthData authData)
			{
				Log.v(TAG, "Authentication worked");
			}

			public override void onAuthenticationError(FirebaseError firebaseError)
			{
				Log.e(TAG, "Authentication failed: " + firebaseError.Message);
				Toast.makeText(ApplicationContext, "Authentication failed. Please try again", Toast.LENGTH_SHORT).show();
			}
		}

		private class ValueEventListenerAnonymousInnerClassHelper : ValueEventListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public ValueEventListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onDataChange(DataSnapshot dataSnapshot)
			{
				string floor = dataSnapshot.getValue(typeof(string));
				if (floor == null || floor.Equals("none"))
				{
					outerInstance.mOfficeFloorView.Background = null;
				}
				else if (floor.Equals("carpet"))
				{
					outerInstance.mOfficeFloorView.Background = Resources.getDrawable(R.drawable.floor_carpet);
				}
				else if (floor.Equals("grid"))
				{
					outerInstance.mOfficeFloorView.Background = Resources.getDrawable(R.drawable.floor_grid);
				}
				else if (floor.Equals("tile"))
				{
					outerInstance.mOfficeFloorView.Background = Resources.getDrawable(R.drawable.floor_tile);
				}
				else if (floor.Equals("wood"))
				{
					outerInstance.mOfficeFloorView.Background = Resources.getDrawable(R.drawable.floor_wood);
				}
				outerInstance.mOfficeFloorView.invalidate();
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				Log.v(TAG, "Floor update canceled: " + firebaseError.Message);

			}
		}

		private class ChildEventListenerAnonymousInnerClassHelper : ChildEventListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public ChildEventListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onChildAdded(DataSnapshot dataSnapshot, string s)
			{

				string key = dataSnapshot.Key;
				OfficeThing existingThing = dataSnapshot.getValue(typeof(OfficeThing));

				Log.v(TAG, "New thing added " + existingThing);

				outerInstance.addUpdateThingToLocalModel(key, existingThing);
			}

			public override void onChildChanged(DataSnapshot dataSnapshot, string s)
			{
				string key = dataSnapshot.Key;
				OfficeThing existingThing = dataSnapshot.getValue(typeof(OfficeThing));

				Log.v(TAG, "Thing changed " + existingThing);

				outerInstance.addUpdateThingToLocalModel(key, existingThing);
			}

			public override void onChildRemoved(DataSnapshot dataSnapshot)
			{
				string key = dataSnapshot.Key;

				Log.v(TAG, "Thing removed " + key);

				outerInstance.removeThingFromLocalModel(key);
			}

			public override void onChildMoved(DataSnapshot dataSnapshot, string s)
			{
				string key = dataSnapshot.Key;
				OfficeThing existingThing = dataSnapshot.getValue(typeof(OfficeThing));

				Log.v(TAG, "Thing moved " + existingThing);

				outerInstance.addUpdateThingToLocalModel(key, existingThing);
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				Log.w(TAG, "Furniture move was canceled: " + firebaseError.Message);
			}
		}

		private class SelectedThingChangeListenerAnonymousInnerClassHelper : SelectedThingChangeListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public SelectedThingChangeListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void thingChanged(OfficeThing officeThing)
			{
				outerInstance.mSelectedThing = officeThing;

				if (outerInstance.mActionMenu != null)
				{
					// Clean things up, if they're there
					outerInstance.mActionMenu.removeItem(R.id.action_delete);
					outerInstance.mActionMenu.removeItem(R.id.action_edit);
					outerInstance.mActionMenu.removeItem(R.id.action_rotate);

					// If I have a new thing, add menu items back to it
					if (officeThing != null)
					{
						outerInstance.mActionMenu.add(Menu.NONE, R.id.action_delete, Menu.NONE, getString(R.@string.action_delete));

						// Only desks can be edited
						if (officeThing.Type.Equals("desk"))
						{
							outerInstance.mActionMenu.add(Menu.NONE, R.id.action_edit, Menu.NONE, getString(R.@string.action_edit));
						}

						outerInstance.mActionMenu.add(Menu.NONE, R.id.action_rotate, Menu.NONE, getString(R.@string.action_rotate));
					}
				}
			}
		}

		private class ThingChangeListenerAnonymousInnerClassHelper : ThingChangeListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public ThingChangeListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance) : base(outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void thingChanged(string key, OfficeThing officeThing)
			{
				outerInstance.mStuffToUpdate[key] = officeThing;
				outerInstance.mOfficeCanvasView.invalidate();
			}
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			int id = item.ItemId;
			switch (id)
			{
				case R.id.action_new_thing:
					renderNewThingPopup();
					break;
				case R.id.change_floor:
					renderChangeCarpetPopup();
					break;
				case R.id.action_rotate:
					if (mSelectedThing != null)
					{
						int rotation = mSelectedThing.Rotation;

						if (rotation >= 270)
						{
							mSelectedThing.Rotation = 0;
						}
						else
						{
							mSelectedThing.Rotation = rotation + 90;
						}
						updateOfficeThing(mSelectedThing.Key, mSelectedThing);
					}
					break;
				case R.id.action_delete:
					deleteOfficeThing(mSelectedThing.Key, mSelectedThing);
					break;
				case R.id.action_edit:
					AlertDialog.Builder builder = new AlertDialog.Builder(this);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.EditText entry = new android.widget.EditText(this);
					EditText entry = new EditText(this);

					builder.setMessage(getString(R.@string.edit_desk_name_description)).setTitle(getString(R.@string.edit_desk_name_title)).setView(entry);

					builder.setPositiveButton(getString(R.@string.edit_desk_name_save), new OnClickListenerAnonymousInnerClassHelper(this, id, entry));
					builder.show();
					break;
			}
			return base.onOptionsItemSelected(item);
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly OfficeMoverActivity outerInstance;

			private int id;
			private EditText entry;

			public OnClickListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance, int id, EditText entry)
			{
				this.outerInstance = outerInstance;
				this.id = id;
				this.entry = entry;
			}

			public virtual void onClick(DialogInterface dialog, int id)
			{
				string text = entry.Text.ToString();
				outerInstance.mSelectedThing.Name = text;
				outerInstance.updateOfficeThing(outerInstance.mSelectedThing.Key, outerInstance.mSelectedThing);
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.office_mover, menu);
			mActionMenu = menu;
			return true;
		}

		/// <summary>
		/// The add item popup menu
		/// </summary>
		private void renderNewThingPopup()
		{
			View menuItemView = findViewById(R.id.action_new_thing);
			PopupMenu popup = new PopupMenu(this, menuItemView);
			MenuInflater inflater = popup.MenuInflater;
			inflater.inflate(R.menu.add_office_thing, popup.Menu);
			popup.OnMenuItemClickListener = new OnMenuItemClickListenerAnonymousInnerClassHelper(this);
			popup.show();
		}

		private class OnMenuItemClickListenerAnonymousInnerClassHelper : PopupMenu.OnMenuItemClickListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public OnMenuItemClickListenerAnonymousInnerClassHelper(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool onMenuItemClick(MenuItem item)
			{
				string menuName = Resources.getResourceName(item.ItemId);
				if (menuName.Contains("action_add_"))
				{
					string newThingName = menuName.Split("action_add_", true)[1];
					addOfficeThing(newThingName);
				}
				else
				{
					Log.e(TAG, "Attempted to add unknown thing " + menuName);
				}
				return true;
			}

			/// <summary>
			/// Saves a new thing to Firebase, which is then picked up and displayed by
			/// the view
			/// </summary>
			/// <param name="thingType"> The type of furniture to add to Firebase </param>
			private void addOfficeThing(string thingType)
			{
				if (null == thingType)
				{
					throw new System.ArgumentException("Typeless office things are not allowed");
				}

				OfficeThing newThing = new OfficeThing();
				newThing.Type = thingType;
				newThing.setzIndex(outerInstance.mOfficeLayout.HighestzIndex + 1);
				newThing.Rotation = 0;
				newThing.Name = "";
				newThing.Left = OfficeCanvasView.LOGICAL_WIDTH / 2;
				newThing.Top = OfficeCanvasView.LOGICAL_HEIGHT / 2;

				Log.w(TAG, "Added thing to firebase " + newThing);

				Firebase newThingFirebaseRef = outerInstance.mFirebaseRef.child("furniture").push();
				newThingFirebaseRef.setValue(newThing, new CompletionListenerAnonymousInnerClassHelper(this));
			}

			private class CompletionListenerAnonymousInnerClassHelper : Firebase.CompletionListener
			{
				private readonly OnMenuItemClickListenerAnonymousInnerClassHelper outerInstance;

				public CompletionListenerAnonymousInnerClassHelper(OnMenuItemClickListenerAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public override void onComplete(FirebaseError firebaseError, Firebase firebase)
				{
					if (firebaseError != null)
					{
						Log.w(TAG, "Add failed! " + firebaseError.Message);
					}
				}
			}
		}

		/// <summary>
		/// The change floor pattern popup menu
		/// </summary>
		private void renderChangeCarpetPopup()
		{
			View menuItemView = findViewById(R.id.change_floor);
			PopupMenu popup = new PopupMenu(this, menuItemView);
			MenuInflater inflater = popup.MenuInflater;
			inflater.inflate(R.menu.change_floor, popup.Menu);
			popup.OnMenuItemClickListener = new OnMenuItemClickListenerAnonymousInnerClassHelper2(this);
			popup.show();
		}

		private class OnMenuItemClickListenerAnonymousInnerClassHelper2 : PopupMenu.OnMenuItemClickListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public OnMenuItemClickListenerAnonymousInnerClassHelper2(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool onMenuItemClick(MenuItem item)
			{
				string menuName = Resources.getResourceName(item.ItemId);
				if (menuName.Contains("action_floor_"))
				{
					string newFloor = menuName.Split("action_floor_", true)[1];
					if (newFloor.Equals("none"))
					{
						outerInstance.mFirebaseRef.child("background").removeValue();
					}
					else
					{
						outerInstance.mFirebaseRef.child("background").Value = newFloor;
					}
				}
				else
				{
					Log.e(TAG, "Attempted change carpet to unknown value " + menuName);
				}
				return true;
			}
		}

		public virtual void updateOfficeThing(string key, OfficeThing officeThing)
		{
			if (null == key || null == officeThing)
			{
				throw new System.ArgumentException();
			}

			// re-apply the cached key, just in case
			officeThing.Key = key;

			mFirebaseRef.child("furniture").child(key).setValue(officeThing, new CompletionListenerAnonymousInnerClassHelper2(this));
		}

		private class CompletionListenerAnonymousInnerClassHelper2 : Firebase.CompletionListener
		{
			private readonly OfficeMoverActivity outerInstance;

			public CompletionListenerAnonymousInnerClassHelper2(OfficeMoverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onComplete(FirebaseError firebaseError, Firebase firebase)
			{
				if (firebaseError != null)
				{
					Log.w(TAG, "Update failed! " + firebaseError.Message);
				}
			}
		}

		public virtual void deleteOfficeThing(string key, OfficeThing officeThing)
		{
			if (null == key || null == officeThing)
			{
				throw new System.ArgumentException();
			}

			mFirebaseRef.child("furniture").child(key).removeValue();
		}

		/// <summary>
		/// Adds a thing to the local model used in rendering
		/// </summary>
		/// <param name="key"> </param>
		/// <param name="officeThing"> </param>
		public virtual void addUpdateThingToLocalModel(string key, OfficeThing officeThing)
		{
			officeThing.Key = key;
			mOfficeLayout[key] = officeThing;
			mOfficeCanvasView.invalidate();
		}

		/// <summary>
		/// Removes a thing from the local model used in rendering
		/// </summary>
		/// <param name="key"> </param>
		public virtual void removeThingFromLocalModel(string key)
		{
			mOfficeLayout.Remove(key);
			mOfficeCanvasView.invalidate();
		}

		public virtual bool signOut(MenuItem item)
		{
			Intent signOutIntent = new Intent(this, typeof(LoginActivity));
			signOutIntent.putExtra("SIGNOUT", true);
			startActivity(signOutIntent);
			return true;
		}
	}
}