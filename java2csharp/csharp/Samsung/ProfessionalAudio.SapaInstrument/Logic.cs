namespace com.samsung.audiosuite.sapainstrumentsample
{

	using SapaProcessor = com.samsung.android.sdk.professionalaudio.SapaProcessor;

	/// <summary>
	/// This class is responsible for common functionality for standalone mode and instance started from
	/// other audio apps.
	/// </summary>
	internal class Logic
	{

		// Commands for communication with native part
		private const string STOP_COMMAND = "/stop";
		private const string PLAY_COMMAND = "/play";

		/// <summary>
		/// This method send start playing command to native part.
		/// </summary>
		/// <param name="processor">
		///            Processor via which message is to be sent. </param>
		internal static void startPlaying(SapaProcessor processor)
		{
			processor.sendCommand(PLAY_COMMAND);
		}

		/// <summary>
		/// This method send stop playing command to native part.
		/// </summary>
		/// <param name="processor">
		///            Processor via which message is to be sent. </param>
		internal static void stopPlaying(SapaProcessor processor)
		{
			processor.sendCommand(STOP_COMMAND);
		}
	}

}