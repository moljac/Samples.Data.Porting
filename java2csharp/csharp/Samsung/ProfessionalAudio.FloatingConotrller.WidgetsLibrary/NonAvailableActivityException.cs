using System;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	internal class NonAvailableActivityException : Exception
	{

		/// 
		private const long serialVersionUID = -1452800299632702425L;
		private const string MSG = "Activity non available. It has been cleared by GC";

		public override string Message
		{
			get
			{
				return MSG;
			}
		}

	}

}