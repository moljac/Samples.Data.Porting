using System;

namespace com.opentok.android.demo.multiparty
{

	using Context = android.content.Context;


	public class MySubscriber : Subscriber
	{

		private string userId;
		private string name;

		public MySubscriber(Context context, Stream stream) : base(context, stream)
		{
			// With the userId we can query our own database
			// to extract player information
			Name = "User" + ((int)(new Random(1).NextDouble() * 1000));
		}

		public virtual string UserId
		{
			get
			{
				return userId;
			}
			set
			{
				this.userId = value;
			}
		}


		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}



	}

}