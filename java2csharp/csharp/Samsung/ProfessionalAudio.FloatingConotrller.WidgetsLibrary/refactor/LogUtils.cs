using System;
using System.Text;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Log = android.util.Log;

	/// <summary>
	/// @brief Log throwable objects
	/// </summary>
	public class LogUtils
	{

		/// <summary>
		/// Log throwable object.
		/// </summary>
		/// <param name="tag"> </param>
		/// <param name="msg"> </param>
		/// <param name="ex"> </param>
		public static void throwable(string tag, string msg, Exception ex)
		{
			StringBuilder stackTrace = new StringBuilder();
			stackTrace.Append(msg).Append(": ").Append(ex.ToString());

			if (null != ex.Message)
			{
				stackTrace.Append(" (").Append(ex.Message).Append(")");
			}

			stackTrace.Append("\n");
			foreach (StackTraceElement ste in ex.StackTrace)
			{
				stackTrace.Append("\tat ").Append(ste.ClassName).Append(".").Append(ste.MethodName).Append(" (").Append(ste.FileName).Append(":").Append(ste.LineNumber).Append(")\n");
			}
			Log.e(tag, stackTrace.ToString());
		}

	}

}