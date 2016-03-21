/// 
/// <summary>
/// Sample source code for AllShare Framework SDK
/// 
/// Copyright (C) 2013 Samsung Electronics Co., Ltd.
/// All Rights Reserved.
/// 
/// @file ControllerType.java
/// @date March 12, 2013
/// 
/// </summary>

namespace com.samsung.android.sdk.sample.videoplayer.controller
{

	/// <summary>
	/// Interface for controlling various types of players (remote or local).
	/// 
	/// </summary>
	public interface ControllerType
	{

		/// <summary>
		/// Queries for current position in media asynchronously.
		/// </summary>
		void queryCurrentPosition();

		/// <summary>
		/// Starts playback of media, or resumes if media was paused.
		/// </summary>
		void play();

		/// <summary>
		/// Pauses playback of media.
		/// </summary>
		void pause();

		/// <summary>
		/// Stops and resets playback.
		/// </summary>
		void stop();

		/// <summary>
		/// Seeks to given position in media.
		/// </summary>
		/// <param name="destination"> target in seconds </param>
		void seek(int destination);

		/// <summary>
		/// Sets a new volume. </summary>
		/// <param name="vol"> a volume to set between 0 and 100 </param>
		int Volume {set;}

		/// <summary>
		/// Mutes or unmutes a device </summary>
		/// <param name="mute"> if true, a device will be muted </param>
		bool Mute {set;}

		/// <summary>
		/// Releases all resources used by player.
		/// 
		/// The player is not usable after calling release.
		/// </summary>
		void release();
	}

}