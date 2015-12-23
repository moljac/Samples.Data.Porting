namespace com.firebase.androidchat
{

	/// <summary>
	/// @author greg
	/// @since 6/21/13
	/// </summary>
	public class Chat
	{

		private string message;
		private string author;

		// Required default constructor for Firebase object mapping
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private Chat()
		private Chat()
		{
		}

		internal Chat(string message, string author)
		{
			this.message = message;
			this.author = author;
		}

		public virtual string Message
		{
			get
			{
				return message;
			}
		}

		public virtual string Author
		{
			get
			{
				return author;
			}
		}
	}

}