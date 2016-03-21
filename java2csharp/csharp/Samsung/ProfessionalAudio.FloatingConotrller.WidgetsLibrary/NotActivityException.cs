using System;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	internal class NotActivityException : Exception
	{
		/// 
		private const long serialVersionUID = -6125350060493181620L;
		private const string MSG = "Provided context is not an Activity";

		public override string Message
		{
			get
			{
				return MSG;
			}
		}
	}

}