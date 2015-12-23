using System.Collections.Generic;

namespace com.firebase.drawing
{


	/// <summary>
	/// @author greg
	/// @since 6/26/13
	/// </summary>
	public class Segment
	{

		private IList<Point> points = new List<Point>();
		private int color;

		// Required default constructor for Firebase serialization / deserialization
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private Segment()
		private Segment()
		{
		}

		public Segment(int color)
		{
			this.color = color;
		}

		public virtual void addPoint(int x, int y)
		{
			Point p = new Point(x, y);
			points.Add(p);
		}

		public virtual IList<Point> Points
		{
			get
			{
				return points;
			}
		}

		public virtual int Color
		{
			get
			{
				return color;
			}
		}
	}

}