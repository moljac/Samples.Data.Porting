namespace com.samsung.audiosuite.sapaeffectsample
{

	using Log = android.util.Log;

	using SapaProcessor = com.samsung.android.sdk.professionalaudio.SapaProcessor;

	/// <summary>
	/// This class is responsible for common functionality for standalone mode and instance started from
	/// other audio apps.
	/// </summary>
	internal class Logic
	{

		private const string TAG = "audiosuite:sapaeffectsample:j:Logic";

		// Commands for communication with native part
		private const string VOLUME_COMMAND = "/volume:";

		internal const int DEFAULT_VOLUME = -10;
		private const int MAX_VOLUME = 0;
		private const int MIN_VOLUME = -30;
		private const int DELTA_VOLUME = 1;

		/// <summary>
		/// This method is responsible to send volume message via given processor.
		/// </summary>
		/// <param name="processor">
		///            Processor via which message is to be sent. </param>
		/// <param name="volume">
		///            Volume to be sent. </param>
		internal static void sendVolume(SapaProcessor processor, int volume)
		{
			processor.sendCommand(VOLUME_COMMAND + volume);
			Log.d(TAG, "Sending command: " + VOLUME_COMMAND + volume);
		}

		/// <summary>
		/// This method checks whether given value is a minimum accepted value of volume.
		/// </summary>
		/// <param name="volume">
		///            volume to be checked. </param>
		/// <returns> true when given value of volume is not bigger from the minimum volume; false
		///         otherwise. </returns>
		internal static bool isMinVolume(int volume)
		{
			return volume <= MIN_VOLUME;
		}

		/// <summary>
		/// This method checks whether given value is a maximum accepted value of volume.
		/// </summary>
		/// <param name="volume">
		///            volume to be checked. </param>
		/// <returns> true when given value of volume is not smaller from the maximum volume; false
		///         otherwise. </returns>
		internal static bool isMaxVolume(int volume)
		{
			return volume >= MAX_VOLUME;
		}

		/// <summary>
		/// This method returns decreased by DELTA_VOLUME value of volume.
		/// </summary>
		/// <param name="volume">
		///            Value of volume to be decreased. </param>
		/// <returns> Decreased by DELTA_VOLUME value of volume. </returns>
		internal static int decreaseVolume(int volume)
		{
			return volume - DELTA_VOLUME;
		}

		/// <summary>
		/// This method returns increased by DELTA_VOLUME value of volume.
		/// </summary>
		/// <param name="volume">
		///            Value of volume to be increased. </param>
		/// <returns> Increased by DELTA_VOLUME value of volume. </returns>
		internal static int increaseVolume(int volume)
		{
			return volume + DELTA_VOLUME;
		}

		/// <summary>
		/// This method returns text with value of volume.
		/// </summary>
		/// <param name="volume">
		///            Value of volume to be shown. </param>
		/// <returns> Text with value of volume. </returns>
		internal static string getVolumeText(int volume)
		{
			return ("" + volume + " dB");
		}
	}

}