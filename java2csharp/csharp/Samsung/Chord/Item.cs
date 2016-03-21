namespace com.samsung.android.sdk.chord.example.adapter
{

	using Bitmap = android.graphics.Bitmap;

	public class Item
	{

		internal Bitmap image;

		internal string text;

		public Item(Bitmap img, string txt)
		{
			this.image = img;
			this.text = txt;
		}

		public virtual Bitmap Image
		{
			get
			{
				return image;
			}
			set
			{
				this.image = value;
			}
		}


		public virtual string Text
		{
			get
			{
				return text;
			}
			set
			{
				this.text = value;
			}
		}


	}

}