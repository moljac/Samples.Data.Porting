using System;

/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file RemotePlayer.java
/// @date March 12, 2013
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.musicplayer.controller
{

	using Uri = android.net.Uri;
	using com.samsung.android.sdk.mediacontrol;
	using PlayerState = com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;

	using Log = android.util.Log;

	/// <summary>
	/// Controller for AllShare controlled media player.
	/// <p/>
	/// This class handles all interactions with AllShare Service and remote
	/// device.
	/// <p/>
	/// The resource to be played has to be Uri of a local file.
	/// <p/>
	/// This sample has basic error recovery, in case of any errors it switches to
	/// local player.
	/// </summary>
	public class RemotePlayer : ControllerType, SmcAvPlayer.EventListener, SmcAvPlayer.ResponseListener, SmcDeviceFinder.StatusListener
	{

		// Reference to PlayerController for providing responses and state updates
		private readonly PlayerController mController;

		// The Uri to be played on remote AllShare device.
		private readonly Uri mContentUri;

		// The mime type of resource.
		private readonly string mMimeType;

		// The unique id of remote AllShare device.
		private readonly string mDeviceId;

		// The device type of remote AllShare device.
		private readonly int mDeviceType;

		// Remote controlled AllShare device.
		private SmcAvPlayer mPlayer;

		// Playback position that should be set when player connects.
		private int mPosition;

		// The destination playback state, used before player is connected.
		private PlayerState mStateToGo;

		// The current playback state.
		private PlayerState mCurrentState = PlayerState.STOPPED;

		// Device finder reference
		private SmcDeviceFinder mDeviceFinder;

		private bool mFirstPlay = true;


		internal RemotePlayer(PlayerController controller, Uri contentUri, string mimetype, string deviceId, int deviceType)
		{
			this.mController = controller;
			this.mContentUri = contentUri;
			this.mMimeType = mimetype;
			this.mDeviceId = deviceId;
			this.mDeviceType = deviceType;

			// Connect to AllShare Service. Processing continues when
			// service instance is received in onCreated callback method.
			SmcDeviceFinder df = new SmcDeviceFinder(controller.mContext);
			df.StatusListener = this;
			df.start();
		}

		/////////////////////////////////////////////////////////////////////////////////
		// SmcDeviceFinder.StatusListener implementation
		/////////////////////////////////////////////////////////////////////////////////

		public override void onStarted(SmcDeviceFinder deviceFinder, int error)
		{
			mDeviceFinder = deviceFinder;
			mPlayer = (SmcAvPlayer) mDeviceFinder.getDevice(mDeviceType, mDeviceId);
			if (mPlayer == null)
			{
				mController.setLocalPlayer();
				return;
			}

			if (mController.CurrentState != PlayerState.STOPPED)
			{
				registerListeners();
			}

			mPlayer.requestVolumeInfo();
			mPlayer.requestStateInfo();
			mPlayer.requestMuteInfo();
		}

		public override void onStopped(SmcDeviceFinder deviceFinder)
		{
			if (mDeviceFinder == deviceFinder)
			{
				mDeviceFinder.StatusListener = null;
				mDeviceFinder = null;
			}
		}
		// End of SmcDeviceFinder.StatusListener implementation.

		private void registerListeners()
		{
			mPlayer.EventListener = this;
			mPlayer.ResponseListener = this;
		}

		private void unregisterListeners()
		{
			mPlayer.EventListener = null;
			mPlayer.ResponseListener = null;
		}

		/// <summary>
		/// Asynchronously call for current playback media position.
		/// The response is returned via onGetPlayPositionResponse callback method.
		/// </summary>
		public virtual void queryCurrentPosition()
		{
			if (mPlayer != null && mCurrentState == PlayerState.PLAYING)
			{
				mPlayer.requestPlayPosition();
			}
		}

		/// <summary>
		/// Starts or resumes madia playback/
		/// </summary>
		public virtual void play()
		{
			if (mPlayer == null)
			{
				// Player is not yet connected, store that we should start playback when it connects.
				mStateToGo = PlayerState.PLAYING;
				return;
			}

			switch (mCurrentState)
			{
				case PlayerState.STOPPED:
					// Player was stopped, we have to provide content and starting position.
					playItem();
					break;
				case PlayerState.PAUSED:
					// Player was paused, resume playback.
					mPlayer.resume();
					break;
				default:
					break;
			}
		}

		private void playItem()
		{
			registerListeners();

			State = PlayerState.INITIALIZING;

			SmcItem item = new SmcItem(new SmcItem.LocalContent(mContentUri.Path,mMimeType));
			// If seek() was called before player connected, it stored the position
			// and we can start playback from that position.
			SmcAvPlayer.PlayInfo info = new SmcAvPlayer.PlayInfo(mPosition);
			mPlayer.play(item, info);
			Log.d("MusicPlayer", "Starting playback at: " + mPosition);
		}

		public virtual void pause()
		{
			if (mPlayer != null)
			{
				mPlayer.pause();
			}
			else
			{
				// Player is not yet connected, store that we should pause
				// playback when it connects.
				mStateToGo = PlayerState.PAUSED;
			}
		}

		public virtual void stop()
		{
			if (mPlayer != null)
			{
				mPlayer.stop();
				mPosition = 0;
			}
			else
			{
				// Player is not yet connected, store that we should hide
				// playback when it connects.
				mStateToGo = PlayerState.STOPPED;
			}
		}

		public virtual void seek(int destination)
		{
			if (mPosition == destination)
			{
				return;
			}
			// Store the requested position, it will be used if player
			// is not yet connected.
			mPosition = destination;
			if (mPlayer != null)
			{
				mPlayer.seek(destination);
				State = PlayerState.BUFFERING;
			}
			Log.d("MusicPlayer", "Seek: " + destination + ", " + mPlayer);
		}

		public virtual int Volume
		{
			set
			{
				if (mPlayer != null)
				{
					mPlayer.Volume = value;
				}
			}
		}

		public virtual bool Mute
		{
			set
			{
				if (mPlayer != null)
				{
					mPlayer.Mute = value;
				}
			}
		}

		public virtual void release()
		{
			// Disconnect from remote player
			if (mPlayer != null)
			{
				unregisterListeners();
			}

			// Release the AllShare Service instance.
			if (mDeviceFinder != null)
			{
				mDeviceFinder.setDeviceListener(SmcDevice.TYPE_AVPLAYER, null);
				mDeviceFinder.stop();
			}

		}

		/// <summary>
		/// Callback when AllShare device state changes
		/// </summary>
		public override void onDeviceChanged(SmcAvPlayer device, int state, int err)
		{
			Log.d("MusicPlayer", "onDeviceChanged: " + state);

			if (err != Smc.SUCCESS)
			{
				// Error has occurred, switch to local player.
				mController.setLocalPlayer();
				return;
			}

			// We use simplified state set, so map from states used in service
			// to states used locally.
			switch (state)
			{
				case SmcAvPlayer.STATE_PAUSED:
					State = PlayerState.PAUSED;
					break;
				case SmcAvPlayer.STATE_PLAYING:
					State = PlayerState.PLAYING;
					if (mFirstPlay)
					{
						mPlayer.seek(mPosition);
						mFirstPlay = false;
					}

					mPosition = 0;

					break;
				case SmcAvPlayer.STATE_BUFFERING:
					State = PlayerState.BUFFERING;
					break;
				case SmcAvPlayer.STATE_STOPPED:
					if (mCurrentState == PlayerState.INITIALIZING)
					{
						playItem();
					}
					else
					{
						State = PlayerState.STOPPED;
					}
					break;
				case SmcAvPlayer.STATE_CONTENT_CHANGED:
					//mController.setLocalPlayer();
					//TODO
					break;
				default:
					State = PlayerState.STOPPED;
				break;
			}
		}

		/// <summary>
		/// Callback for response to requestPlayPosition device method.
		/// </summary>
		public override void onRequestPlayPosition(SmcAvPlayer device, long position, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred
				return;
			}
			// Notify controller of current position
			if (position != 0)
			{
				mController.notifyCurrentPosition((int) position);
			}
			if (Math.Abs(position - mPosition) < 2 && mCurrentState == PlayerState.BUFFERING)
			{
				State = PlayerState.PLAYING;
			}
		}

		public override void onRequestStateInfo(SmcAvPlayer device, int state, int err)
		{
			onDeviceChanged(device, state, err);
		}

		public override void onRequestMediaInfo(SmcAvPlayer device, SmcAvPlayer.MediaInfo mediaInfo, int error)
		{
			//Not used in this sample.
		}

		/// <summary>
		/// Callback for response to pause() device method.
		/// </summary>
		public override void onPause(SmcAvPlayer device, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred.
				return;
			}

			// Notify controller of state change
			State = PlayerState.PAUSED;
		}

		/// <summary>
		/// Callback for response to play() device method.
		/// </summary>
		public override void onPlay(SmcAvPlayer device, SmcItem ai, SmcAvPlayer.PlayInfo ci, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred.
				return;
			}

			// Notify controller of state change
			State = PlayerState.PLAYING;
		}

		/// <summary>
		/// Callback for response to resume() device method.
		/// </summary>
		public override void onResume(SmcAvPlayer device, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred.
				return;
			}

			// Notify controller of state change
			State = PlayerState.PLAYING;
		}

		/// <summary>
		/// Callback for response to seek() device method.
		/// </summary>
		public override void onSeek(SmcAvPlayer device, long requestedPosition, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred.
				return;
			}

			// Action successful, no further processing needed.
		}

		/// <summary>
		/// Callback for response to hide() device method.
		/// </summary>
		public override void onStop(SmcAvPlayer device, int err)
		{
			if (err != Smc.SUCCESS)
			{
				// Error has occurred.
				return;
			}

			// Notify controller of state change
			mController.notifyCurrentPosition(0);

			unregisterListeners();

			State = PlayerState.STOPPED;
			mPosition = 0;
		}

		public override void onRequestVolumeInfo(SmcAvPlayer device, int i, int error)
		{
			mController.notifyCurrentVolume(i);
		}


		public override void onRequestMuteInfo(SmcAvPlayer device, bool b, int error)
		{

		}


		/// <summary>
		/// Provide common processing for state changing logic.
		/// </summary>
		/// <param name="newState"> the new playback state </param>
		private PlayerState State
		{
			set
			{
				Log.d("MusicPlayer", "New state: " + value);
    
				// Notify controller of state change
				mController.CurentState = value;
    
				// Store the current state locally
				mCurrentState = value;
    
				if (value == mStateToGo)
				{
					// We have reached the state we were supposed to reach, so we can
					// clear the flag
					mStateToGo = null;
				}
				else if (value == PlayerState.PLAYING && mStateToGo == PlayerState.PAUSED)
				{
					// If paused state is requested, we have to go through playing
					// This is detected here and pause is finally called.
					pause();
				}
				else if (mStateToGo == PlayerState.PLAYING)
				{
					play();
				}
			}
		}


		public override void onSetVolume(SmcAvPlayer device, int level, int error)
		{

		}


		public override void onSetMute(SmcAvPlayer device, bool onOff, int error)
		{

		}


	}

}