using System;

/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file MusicPlayerActivity.java
/// @date March 12, 2013
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.musicplayer
{

	using Activity = android.app.Activity;
	using ProgressDialog = android.app.ProgressDialog;
	using ContentUris = android.content.ContentUris;
	using DialogInterface = android.content.DialogInterface;
	using AssetManager = android.content.res.AssetManager;
	using Cursor = android.database.Cursor;
	using MediaScannerConnection = android.media.MediaScannerConnection;
	using OnScanCompletedListener = android.media.MediaScannerConnection.OnScanCompletedListener;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Handler = android.os.Handler;
	using Message = android.os.Message;
	using Log = android.util.Log;
	using KeyEvent = android.view.KeyEvent;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using MimeTypeMap = android.webkit.MimeTypeMap;
	using android.widget;
	using OnSeekBarChangeListener = android.widget.SeekBar.OnSeekBarChangeListener;
	using Smc = com.samsung.android.sdk.mediacontrol.Smc;
	using SmcDevice = com.samsung.android.sdk.mediacontrol.SmcDevice;
	using PlayerController = com.samsung.android.sdk.sample.musicplayer.controller.PlayerController;
	using PlayerState = com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;
	using DevicePicker = com.samsung.android.sdk.sample.musicplayer.devicepicker.DevicePicker;

	/// <summary>
	/// Main activity class of sample music player.
	/// </summary>
	public class MusicPlayerActivity : Activity, OnSeekBarChangeListener, DevicePicker.DevicePickerResult, View.OnClickListener, PlayerController.PlayerControllerEvents, MediaScannerConnection.OnScanCompletedListener
	{
		private bool InstanceFieldsInitialized = false;

		public MusicPlayerActivity()
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			mUpdateEventHandler = new Handler(new PositionUpdater(this));
		}


		private const string ASSETS_SUBDIR = "AllShareMusicPlayer";
		private const string MUSIC_FILE = "samsung.mp3";
		private static readonly string[] ALL_ASSETS = new string[] {MUSIC_FILE};

		// activity UI related fields
		private bool mIsActivityVisible = false;
		private ImageView mCoverArt;
		private TextView mTitleText;
		private TextView mArtistText;
		private TextView mPositionText;
		private TextView mDurationText;
		private SeekBar mSeekBar;
		private ImageButton mPlayButton;
		private ImageButton mMuteButton;
		private DevicePicker mDevicePicker;
		private ProgressDialog mProgressDialog;

		// and non-UI fields
		private PlayerController mPlayerController;
		private Handler mUpdateEventHandler;
		private MusicPlayerState mState;

		private Smc mSmcLib;

		/// <summary>
		/// Called when the activity is first created.
		/// </summary>
		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.music_player_layout;
			setupViewElements();

			mSmcLib = new Smc();

			try
			{
				mSmcLib.initialize(BaseContext);
			}
			catch (SsdkUnsupportedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace); //TODO Handle exceptions.
			}

			Log.d("media control lib version", mSmcLib.VersionName);

			// Prepare assets used by sample
			extractAssets();

			// Restore saved state, after e.g. rotating
			if (savedInstanceState != null)
			{
				mState = new MusicPlayerState(savedInstanceState);
			}
			else
			{
				mState = new MusicPlayerState();

				// Initialize state with built-in content
				File storageDir = new File(Environment.ExternalStorageDirectory, ASSETS_SUBDIR);
				File audio = new File(storageDir, MUSIC_FILE);

				//To get media information (title, artist, cover)
				MediaScannerConnection.scanFile(ApplicationContext, new string[]{audio.AbsolutePath}, null, this);

				Uri mediaUri = Uri.fromFile(audio);
				mState.mMediaUri = mediaUri;
				string path = mediaUri.Path;
				if (path != null && path.LastIndexOf(".", StringComparison.Ordinal) != -1)
				{
					string ext = path.Substring(path.LastIndexOf(".", StringComparison.Ordinal) + 1);
					mState.mMimeType = MimeTypeMap.Singleton.getMimeTypeFromExtension(ext);
				}
			}

		}

		protected internal override void onStart()
		{
			base.onStart();

			Log.d("MusicPlayer", "onStart");

			mIsActivityVisible = true;

			// Setup the player and related elements
			setupPlayer();
		}

		protected internal override void onStop()
		{
			base.onStop();

			Log.d("MusicPlayer", "onHide");

			mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
			mIsActivityVisible = false;
			if (Finishing || !mPlayerController.RemotePlayer)
			{
				mPlayerController.stop();
				mPlayerController.release();
				mPlayerController = null;
			}
			else
			{
				mPlayerController.pause(); // make pause in case of going to home screen
			}
		}

		protected internal override void onSaveInstanceState(Bundle outState)
		{
			mState.writeToBundle(outState);

			base.onSaveInstanceState(outState);
		}

		/// <summary>
		/// Sets the global references to UI elements and event handlers for those elements.
		/// </summary>
		private void setupViewElements()
		{
			mTitleText = (TextView) findViewById(R.id.titleText);
			mArtistText = (TextView) findViewById(R.id.artistText);
			mCoverArt = (ImageView) findViewById(R.id.coverImage);
			mSeekBar = (SeekBar) findViewById(R.id.musicPosition);
			mPlayButton = (ImageButton) findViewById(R.id.playPause);
			mMuteButton = (ImageButton) findViewById(R.id.mute);
			mPositionText = (TextView) findViewById(R.id.positionText);
			mDurationText = (TextView) findViewById(R.id.durationText);

			mSeekBar.OnSeekBarChangeListener = this;
			mPlayButton.OnClickListener = this;
			mMuteButton.OnClickListener = this;
			mPositionText.Text = "00:00";

			mProgressDialog = new ProgressDialog(this);
			mProgressDialog.Message = "Buffering...";
			mProgressDialog.Cancelable = true;
			mProgressDialog.OnCancelListener = new OnCancelListenerAnonymousInnerClassHelper(this);

			View stopButton = findViewById(R.id.stop);
			stopButton.OnClickListener = this;

			mDevicePicker = (DevicePicker) FragmentManager.findFragmentById(R.id.playerPicker);
			mDevicePicker.DeviceType = SmcDevice.TYPE_AVPLAYER;
			mDevicePicker.DeviceSelectedListener = this;
		}

		private class OnCancelListenerAnonymousInnerClassHelper : DialogInterface.OnCancelListener
		{
			private readonly MusicPlayerActivity outerInstance;

			public OnCancelListenerAnonymousInnerClassHelper(MusicPlayerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCancel(DialogInterface dialog)
			{
				outerInstance.mPlayerController.stop();
			}
		}

		/// <summary>
		/// Sets up the media player controller and media information in UI.
		/// </summary>
		private void setupPlayer()
		{
			// Display artist, album name and cover art.
			mTitleText.Text = mState.mTitle;
			mArtistText.Text = mState.mArtist;
			mCoverArt.ImageURI = mState.mAlbumArtUri;

			// Start PlayerController and setup local or remote player.
			mPlayerController = new PlayerController(this, this, mState.mMediaUri, mState.mMimeType, mState.mPlayback);

			Log.d("MusicPlayer", "setupPlayer");

			mPlayerController.setLocalPlayer();
			if (mState.mDeviceId == null || mState.mDeviceId.Length == 0)
			{
				mPlayerController.setLocalPlayer();
			}
			else
			{
				mPlayerController.setRemotePlayer(mState.mDeviceId, SmcDevice.TYPE_AVPLAYER);
				mDevicePicker.Active = true;
			}

			// Seek to given position if requested
			if (mState.mPosition != 0)
			{
				mPlayerController.seek(mState.mPosition);
			}

			// Starts playback if requested.
			if (mState.mPlayback == PlayerController.PlayerState.PLAYING)
			{
				mPlayerController.play();
			}

			Mute = mState.mMute;

			// Set seek bar to media length (in seconds).
			// First update via mUpdateEventHandler will set current position.
			mSeekBar.Max = mState.mDuration;
			mDurationText.Text = convertTime2String(mState.mDuration);
			mPositionText.Text = convertTime2String(mState.mPosition);
			onStateChanged(mState.mPlayback);
		}

		private void updateStatusMessage(int message)
		{
			if (mIsActivityVisible)
			{
				Toast.makeText(this, message, Toast.LENGTH_SHORT).show();
			}
		}

		private void updateInformationUi()
		{
			runOnUiThread(() =>
			{
				mTitleText.Text = mState.mTitle;
				mArtistText.Text = mState.mArtist;
				mCoverArt.ImageURI = mState.mAlbumArtUri;
				mSeekBar.Max = mState.mDuration;
				mDurationText.Text = convertTime2String(mState.mDuration);
			});
		}

		// ////////////////////////////////////////////////////////////////////////
		// These methods handle the device picker events
		// ////////////////////////////////////////////////////////////////////////

		/// <summary>
		/// User selected remote player, connect to that device and start playback.
		/// </summary>
		public virtual void onDeviceSelected(SmcDevice device)
		{
			if (mState.mDeviceId == null)
			{
				updateStatusMessage(R.@string.allshare_connecting);
				mState.mDeviceId = device.Id;
				mPlayerController.setRemotePlayer(device.Id, SmcDevice.TYPE_AVPLAYER);
			}
		}

		/// <summary>
		/// User selected to disable AllShare, so disconnect
		/// and switch to local player.
		/// </summary>
		public virtual void onAllShareDisabled()
		{
			mState.mDeviceId = null;
			mPlayerController.stop();
			mPlayerController.setLocalPlayer();
		}


		// ////////////////////////////////////////////////////////////////////////
		// These methods handle the seek bar events
		// ////////////////////////////////////////////////////////////////////////

		public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			// Do nothing
		}

		public override void onStartTrackingTouch(SeekBar seekBar)
		{
			// Block refreshing seek bar, to avoid flickering.
			mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
		}

		public override void onStopTrackingTouch(SeekBar seekBar)
		{
			// Seek to requested position
			int position = seekBar.Progress;
			mPlayerController.seek(position);
			mState.mPosition = position;
			// Re-enable refreshing current position.
			mUpdateEventHandler.sendEmptyMessage(PositionUpdater.START_POLLING);
		}

		// ////////////////////////////////////////////////////////////////////////
		// These methods handle the PlayerController events
		// ////////////////////////////////////////////////////////////////////////

		public virtual void onCurrentPositionUpdated(int position)
		{
			if (mState.mPlayback == PlayerController.PlayerState.STOPPED)
			{
				// We might have received a position update after stopping
				// ignore it in that case and set to zero
				mSeekBar.Progress = 0;
				mState.mPosition = 0;
				mPositionText.Text = convertTime2String(0);
				return;
			}
			if (mState.mPlayback == PlayerController.PlayerState.BUFFERING || mState.mPlayback == PlayerController.PlayerState.INITIALIZING)
			{
				return;
			}

			mSeekBar.Progress = position;
			mPositionText.Text = convertTime2String(position);
			mState.mPosition = position;
		}

		public virtual void onStateChanged(PlayerController.PlayerState currentState)
		{
			mState.mPlayback = currentState;
			if (currentState == PlayerController.PlayerState.PLAYING)
			{
				mPlayButton.ImageResource = R.drawable.ic_media_pause;
				mUpdateEventHandler.sendEmptyMessage(PositionUpdater.START_POLLING);
			}
			else
			{
				mPlayButton.ImageResource = R.drawable.ic_media_play;
				mUpdateEventHandler.sendEmptyMessage(PositionUpdater.STOP_POLLING);
			}

			if (currentState == PlayerController.PlayerState.BUFFERING || currentState == PlayerController.PlayerState.INITIALIZING)
			{
				mProgressDialog.show();
			}
			else
			{
				mProgressDialog.dismiss();
			}
			if (currentState == PlayerController.PlayerState.STOPPED)
			{
				onCurrentPositionUpdated(0);
			}
		}

		public virtual void onRemoteDisconnected()
		{
			updateStatusMessage(R.@string.allshare_disconnected);
			mState.mDeviceId = null;
			mDevicePicker.Active = false;
		}

		public override bool onKeyDown(int keyCode, KeyEvent @event)
		{
			if (mPlayerController.RemotePlayer && (keyCode == KeyEvent.KEYCODE_VOLUME_DOWN || keyCode == KeyEvent.KEYCODE_VOLUME_UP))
			{
				changeVolume(keyCode == KeyEvent.KEYCODE_VOLUME_UP ? 1 : -1);
				return true;
			}
			return base.onKeyDown(keyCode, @event);
		}

		// ////////////////////////////////////////////////////////////////////////
		// This method handles the click events
		// ////////////////////////////////////////////////////////////////////////

		public override void onClick(View v)
		{
			switch (v.Id)
			{
				case R.id.playPause:
					if (mState.mPlayback == PlayerController.PlayerState.PLAYING)
					{
						mPlayerController.pause();
						break;
					}
					// player is stopped or paused, so start playing content
					mPlayerController.play();
					break;
				case R.id.stop:
					mPlayerController.stop();
					break;
				case R.id.mute:
					Mute = !mPlayerController.Mute;
					break;
			}
		}

		private bool Mute
		{
			set
			{
				mPlayerController.Mute = value;
				mMuteButton.ImageResource = value ? R.drawable.volume_muted : R.drawable.volume_on;
				mState.mMute = value;
			}
		}

		private void changeVolume(int diff)
		{
			if (mPlayerController.Mute)
			{
				Mute = false;
			}
			mPlayerController.Volume = mPlayerController.Volume + diff;
		}

		/// <summary>
		/// A class responsible for updating the current position.
		/// <p/>
		/// Send it the START_POLLING empty message to start updating the
		/// current position, and STOP_POLLING if you want to hide it
		/// (in case of activity end, player hide or simply user moves the seek bar).
		/// </summary>
		private class PositionUpdater : Runnable, Handler.Callback
		{
			private readonly MusicPlayerActivity outerInstance;

			public PositionUpdater(MusicPlayerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal const int START_POLLING = 1;
			internal const int STOP_POLLING = 2;

			public override bool handleMessage(Message msg)
			{
				switch (msg.what)
				{
					case START_POLLING:
						outerInstance.mUpdateEventHandler.post(this);
						break;
					case STOP_POLLING:
						outerInstance.mUpdateEventHandler.removeCallbacks(this);
						break;
				}
				return false;
			}

			public override void run()
			{
				PlayerController player = outerInstance.mPlayerController;
				if (player != null)
				{
					player.queryCurrentPositionAsync();
					outerInstance.mUpdateEventHandler.postDelayed(this, 500);
				}
			}

		}

		/// <summary>
		/// Internal class for storing complete player state.
		/// Used for saving state between e.g. rotations.
		/// </summary>
		private class MusicPlayerState
		{
			internal const string MEDIA_URI = "MusicPlayer.mediaUri";
			internal const string COVER_URI = "MusicPlayer.albumArtUri";
			internal const string TITLE = "MusicPlayer.title";
			internal const string ARTIST = "MusicPlayer.artist";
			internal const string MIMETYPE = "MusicPlayer.mimeType";
			internal const string PLAYER_STATE = "MusicPlayer.state";
			internal const string POSITION = "MusicPlayer.position";
			internal const string DURATION = "MusicPlayer.duration";
			internal const string MUTE = "MusicPlayer.mute";
			internal const string DEVICE_ID = "MusicPlayer.deviceId";

			internal Uri mMediaUri;
			internal Uri mAlbumArtUri;
			internal string mTitle;
			internal string mArtist;
			internal string mMimeType;
			internal PlayerController.PlayerState mPlayback = PlayerController.PlayerState.STOPPED;
			internal int mPosition = 0;
			internal int mDuration = 0;
			internal bool mMute;
			internal string mDeviceId;

			internal MusicPlayerState()
			{

			}

			/// <summary>
			/// Constructs <seealso cref="MusicPlayerState"/> from bundle.
			/// </summary>
			/// <param name="savedState"> the bundle containing the saved mState </param>
			internal MusicPlayerState(Bundle savedState)
			{

				mMediaUri = Uri.parse(savedState.getString(MEDIA_URI));
				mAlbumArtUri = Uri.parse(savedState.getString(COVER_URI));
				mTitle = savedState.getString(TITLE);
				mArtist = savedState.getString(ARTIST);
				mMimeType = savedState.getString(MIMETYPE);
				mPlayback = Enum.Parse(typeof(PlayerController.PlayerState), savedState.getString(PLAYER_STATE));
				mPosition = savedState.getInt(POSITION);
				mDuration = savedState.getInt(DURATION);
				mMute = savedState.getInt(MUTE) == 1;
				mDeviceId = savedState.getString(DEVICE_ID);
			}

			/// <summary>
			/// Saves the current state to bundle
			/// </summary>
			/// <param name="savedState"> the bundle we should save to </param>
			internal virtual void writeToBundle(Bundle savedState)
			{
				savedState.putString(MEDIA_URI, mMediaUri != null ? mMediaUri.ToString() : null);
				savedState.putString(COVER_URI, mAlbumArtUri != null ? mAlbumArtUri.ToString() : null);
				savedState.putString(TITLE, mTitle);
				savedState.putString(ARTIST, mArtist);
				savedState.putString(MIMETYPE, mMimeType);
				savedState.putString(PLAYER_STATE, mPlayback != null ? mPlayback.ToString() : null);
				savedState.putInt(POSITION, mPosition);
				savedState.putInt(DURATION, mDuration);
				savedState.putInt(MUTE, mMute ? 1 : 0);
				savedState.putString(DEVICE_ID, mDeviceId);
			}
		}

		public override void onScanCompleted(string arg0, Uri uri)
		{
			Cursor cursor = ApplicationContext.ContentResolver.query(uri, null, null, null, null);
			if (cursor == null)
			{
				return;
			}
			if (cursor.moveToFirst())
			{

				int albumColumn = cursor.getColumnIndex(android.provider.MediaStore.Audio.Media.ALBUM_ID);
				int titleColumn = cursor.getColumnIndex(android.provider.MediaStore.Audio.Media.TITLE);
				int artistColumn = cursor.getColumnIndex(android.provider.MediaStore.Audio.Media.ARTIST);
				int durationColumn = cursor.getColumnIndex(android.provider.MediaStore.Audio.Media.DURATION);

				mState.mTitle = cursor.getString(titleColumn);
				mState.mArtist = cursor.getString(artistColumn);
				long? albumId = cursor.getLong(albumColumn);

				Uri albumArtStore = Uri.parse("content://media/external/audio/albumart");
				Uri albumArt = ContentUris.withAppendedId(albumArtStore, albumId);
				mState.mAlbumArtUri = albumArt;
				//Divided by 1000 to store duration in seconds.
				mState.mDuration = cursor.getInt(durationColumn) / 1000;

				updateInformationUi();
			}
			cursor.close();
		}

		/// <summary>
		/// Extract the media used by this sample to SD card or equivalent.
		/// </summary>
		private void extractAssets()
		{
			AssetManager assetManager = Assets;
			File destDir = new File(Environment.ExternalStorageDirectory, ASSETS_SUBDIR);
			if (!destDir.exists())
			{
				if (!destDir.mkdirs())
				{
					Toast.makeText(this, R.@string.unable_to_load_asset, Toast.LENGTH_LONG).show();
					return;
				}
			}

			foreach (string filename in ALL_ASSETS)
			{

				System.IO.Stream @in = null;
				System.IO.Stream @out = null;
				try
				{
					File destination = new File(destDir, filename);

					if (!destination.exists())
					{

						@in = assetManager.open(filename);
						@out = new System.IO.FileStream(destination, System.IO.FileMode.Create, System.IO.FileAccess.Write);

						sbyte[] buffer = new sbyte[16 * 1024];
						int read;

						while ((read = @in.Read(buffer, 0, buffer.Length)) != -1)
						{
							@out.Write(buffer, 0, read);
						}
					}
				}
				catch (IOException)
				{
					Toast.makeText(this, R.@string.unable_to_load_asset, Toast.LENGTH_LONG).show();
					this.finish();
				}
				finally
				{
					if (@in != null)
					{
						try
						{
							@in.Close();
						}
						catch (IOException)
						{
							// Error during close, ignoring
						}
					}
					if (@out != null)
					{
						try
						{
							@out.Close();
						}
						catch (IOException)
						{
							// Error during close, ignoring
						}
					}
				}
			}
		}

		/// <summary>
		/// Formats the time in seconds as mm:ss or hh:mm:ss string.
		/// </summary>
		/// <param name="time"> time to format </param>
		/// <returns> formatted time </returns>
		private static string convertTime2String(int time)
		{
			int hour = time / 3600;
			time %= 3600;
			int min = time / 60;
			int sec = time % 60;

			string result;

			if (hour >= 1)
			{
				result = string.Format("{0:D2}:{1:D2}:{2:D2}", hour, min, sec);
			}
			else
			{
				result = string.Format("{0:D2}:{1:D2}", min, sec);
			}

			return result;
		}


	}

}