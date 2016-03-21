/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file LocalPlayer.java
/// @date March 12, 2013
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.videoplayer.controller
{

	using MediaPlayer = android.media.MediaPlayer;
	using TrackInfo = android.media.MediaPlayer.TrackInfo;
	using TimedText = android.media.TimedText;
	using OnTimedTextListener = android.media.MediaPlayer.OnTimedTextListener;
	using Uri = android.net.Uri;
	using Log = android.util.Log;
	using SurfaceHolder = android.view.SurfaceHolder;
	using Callback = android.view.SurfaceHolder.Callback;
	using SurfaceView = android.view.SurfaceView;

	using PlayerState = com.samsung.android.sdk.sample.videoplayer.controller.PlayerController.PlayerState;

	/// <summary>
	/// Controller for local media player on Android device.
	/// 
	/// This class uses the Android MediaPlayer class for playback.
	/// 
	/// </summary>
	public class LocalPlayer : ControllerType, SurfaceHolder.Callback, MediaPlayer.OnTimedTextListener, MediaPlayer.OnCompletionListener
	{

		// Reference to PlayerController for providing responses and state updates.
		private readonly PlayerController mController;
		private SurfaceHolder mSurfaceHolder;

		// The controlled local player.
		private readonly MediaPlayer mPlayer;

		private bool mIsPlayerReleased = false;

		internal LocalPlayer(PlayerController controller, Uri content, Uri subtitlesUri, SurfaceView videoView)
		{
			mController = controller;

			// Create and initialize Android MediaPlayer instance.
			mPlayer = new MediaPlayer();
			mPlayer.OnCompletionListener = this;

			mSurfaceHolder = videoView.Holder;
			mSurfaceHolder.addCallback(this);

			bool isSurfaceValid = mSurfaceHolder.Surface.Valid;

			if (isSurfaceValid)
			{
				mPlayer.Display = mSurfaceHolder;
			}

			try
			{
				mPlayer.setDataSource(controller.mContext, content);

				mPlayer.prepare();
				if (subtitlesUri != null)
				{
					mPlayer.addTimedTextSource(subtitlesUri.Path, MediaPlayer.MEDIA_MIMETYPE_TEXT_SUBRIP);
					mPlayer.OnTimedTextListener = this;
				}
				controller.Duration = mPlayer.Duration / 1000;
			}
			catch (IOException ignored)
			{
				Log.e("VideoPlayer", ignored.Message);
			}
		}

		public virtual void queryCurrentPosition()
		{
			mController.notifyCurrentPosition(mPlayer.CurrentPosition / 1000);
		}

		public virtual void play()
		{
			mPlayer.start();

			MediaPlayer.TrackInfo[] trackInfo = mPlayer.TrackInfo;
			int timedTextTrack = findTimedTextTrack(trackInfo);

			if (timedTextTrack > 0)
			{
				mPlayer.selectTrack(timedTextTrack);
			}

			mController.CurentState = PlayerState.PLAYING;
		}

		public virtual void pause()
		{
			if (mController.CurrentState == PlayerState.STOPPED)
			{
				mPlayer.start();
			}
			mPlayer.pause();
			mController.CurentState = PlayerState.PAUSED;
		}

		public virtual void stop()
		{
			try
			{
				mPlayer.stop();
				mPlayer.prepare();

				// Contrary to documentation, MediaPlayer does not reset position after hide & prepare.
				mPlayer.seekTo(0);
				mController.notifyCurrentPosition(0);
			}
			catch (System.InvalidOperationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			mController.CurentState = PlayerState.STOPPED;
		}

		public virtual void seek(int destination)
		{
			mPlayer.seekTo(destination * 1000);
		}

		public virtual int Volume
		{
			set
			{
				//not used here
			}
		}

		public virtual bool Mute
		{
			set
			{
				float vol = value?0:1;
				mPlayer.setVolume(vol, vol);
			}
		}

		public virtual void release()
		{
			mPlayer.release();
			mIsPlayerReleased = true;
			mSurfaceHolder.removeCallback(this);
		}

		public override void surfaceCreated(SurfaceHolder holder)
		{
			mPlayer.Display = holder;

			// Forces player to redraw its content on new surface.
			if (mController.CurrentState == PlayerState.PAUSED)
			{
				mPlayer.seekTo(mPlayer.CurrentPosition);
			}
		}

		public override void surfaceChanged(SurfaceHolder holder, int format, int width, int height)
		{
			mPlayer.Display = holder;

			// Forces player to redraw its content on new surface.
			if (mController.CurrentState == PlayerState.PAUSED)
			{
				mPlayer.seekTo(mPlayer.CurrentPosition);
			}
		}

		public override void surfaceDestroyed(SurfaceHolder holder)
		{
			// The surface only gets destroyed when activity is destroyed.
			// At this point the player is already released, so no need to clean up here.
			if (!mIsPlayerReleased)
			{
				mPlayer.Display = null;
			}
		}

		/// <summary>
		/// Listener that is invoked where a suitable subtitle should be loaded.
		/// When there no text should be displayed - then TimedText is a null.
		/// </summary>
		public override void onTimedText(MediaPlayer mediaPlayer, TimedText text)
		{
			if (text != null)
			{
				mController.notifySubtitles(text.Text);
			}
		}

		public override void onCompletion(MediaPlayer mediaPlayer)
		{
			if (mPlayer == mediaPlayer)
			{
				mController.CurentState = PlayerState.STOPPED;
			}
		}

		/// <summary>
		/// Finds if there is a TimedTrack attached.
		/// </summary>
		private int findTimedTextTrack(MediaPlayer.TrackInfo[] trackInfo)
		{
			int index = -1;
			for (int i = 0; i < trackInfo.Length; i++)
			{
				if (trackInfo[i].TrackType == MediaPlayer.TrackInfo.MEDIA_TRACK_TYPE_TIMEDTEXT)
				{
					return i;
				}
			}
			return index;
		}
	}

}