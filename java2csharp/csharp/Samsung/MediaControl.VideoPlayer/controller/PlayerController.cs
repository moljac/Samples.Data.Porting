/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file PlayerController.java
/// @date March 12, 2013
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.videoplayer.controller
{

	using Context = android.content.Context;
	using Uri = android.net.Uri;
	using SurfaceView = android.view.SurfaceView;


	/// <summary>
	/// Class for controlling media playback.
	/// 
	/// Abstraction layer that separates differences between local and remote
	/// players. Switches between local and remote player by calling either
	/// <seealso cref="#setLocalPlayer()"/> or <seealso cref="#setRemotePlayer(String, int)"/>.
	/// 
	/// Notifies the listener when playback state is changed and offers basic
	/// playback controls.
	/// </summary>

	public class PlayerController
	{
		/// <summary>
		/// Possible playback states of player.
		/// </summary>
		public enum PlayerState
		{
			STOPPED,
			PAUSED,
			PLAYING,
			BUFFERING,
			INITIALIZING
		}

		/// <summary>
		/// Callback interface for notifying about changes and asynchronous calls.
		/// </summary>
		public interface PlayerControllerEventListener
		{

			/// <summary>
			/// Called in response to <seealso cref="PlayerController#queryCurrentPositionAsync()"/>. </summary>
			/// <param name="position"> current media position. </param>
			void onCurrentPositionUpdated(int position);

			/// <summary>
			/// Called when state of playback changes </summary>
			/// <param name="currentState"> the state player is currently in. </param>
			void onStateChanged(PlayerState currentState);

			/// <summary>
			/// Called when remote player has disconnected.
			/// </summary>
			void onRemoteDisconnected();

			/// <summary>
			/// Called when local player pass subtitles for being
			/// displayed </summary>
			/// <param name="sub"> Text of subtitle to be displayed </param>
			void onSubtitleAppears(string sub);
		}

		/// <summary>
		/// Context, needed for connection to AllShare Service.
		/// </summary>
		internal readonly Context mContext;

		// Listener that should be notified of any events.
		private readonly PlayerControllerEventListener mEventListener;

		// The local URI (file://) that should be played.
		private readonly Uri mContentUri;

		// The local subtitles URI (file://) that should be played.
		private readonly Uri mSubtitlesUri;

		// Mime type of resource to be played.
		private readonly string mMimeType;


		// Current player instance.
		private ControllerType mControllerType;

		// The surface that should display the video.
		private readonly SurfaceView mVideoView;

		// Current playback state.
		private PlayerState mCurrentState = PlayerState.STOPPED;

		private int mDuration;
		private int mPosition;
		private int mVolume;
		private bool mMute;

		public PlayerController(Context context, PlayerControllerEventListener listener, Uri contentUri, Uri subtitlesUri, string mimeType, PlayerState playerState, SurfaceView videoView)
		{
			mContext = context;
			mEventListener = listener;
			mContentUri = contentUri;
			mSubtitlesUri = subtitlesUri;
			mMimeType = mimeType;
			mCurrentState = playerState;
			mVideoView = videoView;
		}

		/// <summary>
		/// Queries for the current state of playback. </summary>
		/// <returns> current state </returns>
		public virtual PlayerState CurrentState
		{
			get
			{
				return mCurrentState;
			}
		}

		/// <summary>
		/// Sets the current state of playback.
		/// 
		/// Intended to be called from <seealso cref="ControllerType"/> implementations.
		/// Calls <seealso cref="com.samsung.android.sdk.sample.videoplayer.controller.PlayerController.PlayerControllerEventListener#onStateChanged(PlayerState)"/> if
		/// new state is different from previous.
		/// 
		/// Function is safe to call if new state is identical to previous state.
		/// </summary>
		/// <param name="newState"> the new playback state. </param>
		internal virtual PlayerState CurentState
		{
			set
			{
				if (value == mCurrentState)
				{
					// State has not changed, do nothing.
					return;
				}
    
				// Store the new state
				mCurrentState = value;
    
				// Notify listener
				mEventListener.onStateChanged(value);
			}
		}

		/// <summary>
		/// Queries for the movie duration.
		/// 
		/// Note, that remote players might set this value some time after
		/// opening media. </summary>
		/// <returns> current media duration, in seconds. </returns>
		public virtual int Duration
		{
			get
			{
				return mDuration;
			}
			set
			{
				mDuration = value;
			}
		}


		/// <summary>
		/// Queries for current position in media.
		/// 
		/// In response to this method, the method <seealso cref="com.samsung.android.sdk.sample.videoplayer.controller.PlayerController.PlayerControllerEventListener#onCurrentPositionUpdated(int)"/>
		/// will be called with current position as argument.
		/// 
		/// Note that the callback method may be called synchronously
		/// or asynchronously, the user should not depend on queryCurrentPositionAsync()
		/// finishing before calling onCurrentPositionUpdated()
		/// </summary>
		public virtual void queryCurrentPositionAsync()
		{
			// Relay to current ControllerType implementation
			mControllerType.queryCurrentPosition();
		}

		/// <summary>
		/// Conveys answer to query for current media position.
		/// 
		/// Intended to be called from <seealso cref="ControllerType"/> implementations.
		/// </summary>
		/// <param name="position"> the current media position in seconds </param>
		internal virtual void notifyCurrentPosition(int position)
		{
			// Store it locally for use in restorePosition
			mPosition = position;

			// Provide response to listener
			mEventListener.onCurrentPositionUpdated(position);
		}

		/// <summary>
		/// Conveys answer to query for current volume.
		/// 
		/// Intended to be called from <seealso cref="ControllerType"/> implementations.
		/// </summary>
		/// <param name="volume"> the current volume between 0 and 100 </param>
		internal virtual void notifyCurrentVolume(int volume)
		{
			this.mVolume = volume;
		}

		internal virtual void notifySubtitles(string subtitle)
		{
			mEventListener.onSubtitleAppears(subtitle);
		}

		/// <summary>
		/// Start playback of media, or resumes if media was paused.
		/// </summary>
		public virtual void play()
		{
			// Delegate to current ControllerType instance.
			mControllerType.play();
		}

		/// <summary>
		/// Pauses playback of media.
		/// </summary>
		public virtual void pause()
		{
			// Delegate to current ControllerType instance.
			mControllerType.pause();
		}

		/// <summary>
		/// Stops and resets playback.
		/// </summary>
		public virtual void stop()
		{
			// Delegate to current ControllerType instance.
			mControllerType.stop();
		}

		/// <summary>
		/// Switches playback to remote AllShare device.
		/// </summary>
		/// <param name="deviceId"> the unique identifier of device </param>
		/// <param name="deviceType"> the device controllerType </param>
		public virtual void setRemotePlayer(string deviceId, int deviceType)
		{
			// Releases previous player.
			if (mControllerType != null)
			{
				mControllerType.release();
			}

			ControllerType previousControllerType = mControllerType;

			// Connects to remote player.
			mControllerType = new RemotePlayer(this, mContentUri, mSubtitlesUri, mMimeType, deviceId, deviceType);
			restorePosition();

			// Releases previous player.
			if (previousControllerType != null)
			{
				previousControllerType.release();
			}
		}

		public virtual bool RemotePlayer
		{
			get
			{
				return mControllerType is RemotePlayer;
			}
		}

		/// <summary>
		/// Switches playback to local media player.
		/// </summary>
		public virtual void setLocalPlayer()
		{
			if (mControllerType is LocalPlayer)
			{
				// We are already local, do nothing.
				return;
			}

			ControllerType previousControllerType = mControllerType;

			mControllerType = new LocalPlayer(this, mContentUri, mSubtitlesUri, mVideoView);
			restorePosition();

			// Releases previous player.
			if (previousControllerType != null)
			{
				previousControllerType.release();

				// Notify listener
				mEventListener.onRemoteDisconnected();
			}
		}

		/// <summary>
		/// Seeks to given position in media.
		/// </summary>
		/// <param name="destination"> target in seconds </param>
		public virtual void seek(int destination)
		{
			// Relay to current ControllerType implementation
			mControllerType.seek(destination);
		}

		/// <summary>
		/// Sets a volume with value between 0 and 100 </summary>
		/// <param name="volume"> a volume to set </param>
		public virtual int Volume
		{
			set
			{
				mControllerType.Volume = value;
				mVolume = value;
			}
			get
			{
				return mVolume;
			}
		}


		/// <summary>
		/// Mute or unmute the player </summary>
		/// <param name="mMute"> true to mute the player </param>
		public virtual bool Mute
		{
			set
			{
				this.mMute = value;
				mControllerType.Mute = value;
			}
			get
			{
				return mMute;
			}
		}


		/// <summary>
		/// Release all resources used by player.
		/// 
		/// The player is not usable after calling release.
		/// </summary>
		public virtual void release()
		{
			mControllerType.release();
			mControllerType = null;
		}

		/// <summary>
		/// Restore the playback position and state of player.
		/// 
		/// Used for seamless transition between local and remote player.
		/// 
		/// Note, the position is only updated when the caller class queries
		/// for position, so it depends on caller to do it regularly.
		/// </summary>
		private void restorePosition()
		{
			mControllerType.seek(mPosition);
			mControllerType.Volume = mVolume;
			mControllerType.Mute = mMute;
			if (mCurrentState == PlayerState.PLAYING)
			{
				mControllerType.play();
			}
		}
	}

}