using System.Collections.Generic;

namespace com.firebase.drawing
{

	using Context = android.content.Context;
	using SharedPreferences = android.content.SharedPreferences;
	using Log = android.util.Log;

	using Firebase = com.firebase.client.Firebase;


	/// <summary>
	/// This static class handles the sync-state of the boards. Any changes to synced boards are automatically
	/// downloaded when the application is active, even when the user is not looking at that board.
	/// 
	/// Whether a board is synced on this device is kept in SharedPreferences on the device itself, since
	/// each user can have their own preference for what board(s) to keep synced.
	/// </summary>
	public class SyncedBoardManager
	{
		public const string PREFS_NAME = "DoodleBoardPrefs";
		public const string PREF_NAME = "SyncedBoards";
		public const string TAG = "AndroidDrawing";

		private static Context mContext;

		public static Context Context
		{
			set
			{
				mContext = value;
			}
		}

		public static void restoreSyncedBoards(Firebase boardsRef)
		{
			SharedPreferences preferences = mContext.getSharedPreferences(PREFS_NAME, 0);
			ISet<string> syncedBoards = preferences.getStringSet(PREF_NAME, new HashSet<string>());
			foreach (string key in syncedBoards)
			{
				Log.i(TAG, "Keeping board " + key + " synced");
				boardsRef.child(key).keepSynced(true);
			}
		}

		public static bool isSynced(string boardId)
		{
			SharedPreferences preferences = mContext.getSharedPreferences(PREFS_NAME, 0);
			ISet<string> syncedBoards = preferences.getStringSet(PREF_NAME, new HashSet<string>());
			return syncedBoards.Contains(boardId);
		}

		public static void toggle(Firebase boardsRef, string boardId)
		{
			SharedPreferences preferences = mContext.getSharedPreferences(PREFS_NAME, 0);
			ISet<string> syncedBoards = new HashSet<string>(preferences.getStringSet(PREF_NAME, new HashSet<string>()));
			if (syncedBoards.Contains(boardId))
			{
				syncedBoards.Remove(boardId);
				boardsRef.child(boardId).keepSynced(false);
			}
			else
			{
				syncedBoards.Add(boardId);
				boardsRef.child(boardId).keepSynced(true);
			}
			preferences.edit().putStringSet(PREF_NAME, syncedBoards).commit();
			Log.i(TAG, "Board " + boardId + " is now " + (syncedBoards.Contains(boardId) ? "" : "not ") + "synced");
		}

	}

}