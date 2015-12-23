namespace com.firebase.drawing
{

	/// <summary>
	/// @author greg
	/// @since 6/26/13
	/// </summary>
	public class Point
	{
		internal int x;
		internal int y;

		// Required default constructor for Firebase serialization / deserialization
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private Point()
		private Point()
		{
		}

		public Point(int x, int y)
		{
			this.x = x;
			this.y = y;
		}

		public virtual int X
		{
			get
			{
				return x;
			}
		}

		public virtual int Y
		{
			get
			{
				return y;
			}
		}
	}

}