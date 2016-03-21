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

namespace com.samsung.android.sdk.sample.musicplayer.controller
{

	using MediaPlayer = android.media.MediaPlayer;
	using Uri = android.net.Uri;
	using PlayerState = com.samsung.android.sdk.sample.musicplayer.controller.PlayerController.PlayerState;


	/// <summary>
	/// Controller for local media player on Android device.
	/// 
	/// This class uses the Android MediaPlayer class for playback.
	/// 
	/// </summary>
	public class LocalPlayer : ControllerType
	{


		// The controlled local player.
		private readonly MediaPlayer mPlayer;


		// Reference to PlayerController for providing responses and state updates.
		private readonly PlayerController mController;

		internal LocalPlayer(PlayerController controller, Uri content)
		{
			this.mController = controller;

			// Create and initialize Android MediaPlayer instance.
			mPlayer = new MediaPlayer();
			try
			{
				mPlayer.setDataSource(controller.mContext, content);
				mPlayer.prepare();
				controller.Duration = mPlayer.Duration / 1000;
			}
			catch (IOException)
			{
				// Ignored for application brevity
			}

		}

		public virtual void queryCurrentPosition()
		{
			mController.notifyCurrentPosition(mPlayer.CurrentPosition / 1000);
		}

		public virtual void play()
		{
			mPlayer.start();
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

				// Contrary to documentation, MediaPlayer does not reset position
				// after hide & prepare.
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
		}

	}

}