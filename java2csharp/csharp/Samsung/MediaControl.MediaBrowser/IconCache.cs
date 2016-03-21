namespace com.samsung.android.sdk.sample.mediabrowser
{


	using Bitmap = android.graphics.Bitmap;
	using Uri = android.net.Uri;
	using LruCache = android.util.LruCache;

	public class IconCache : LruCache<Uri, Bitmap>
	{

		private static IconCache instance;

		public static IconCache Instance
		{
			get
			{
				if (instance == null)
				{
					long size = Runtime.Runtime.maxMemory() / 8;
					instance = new IconCache((int) size);
				}
				return instance;
			}
		}

		protected internal IconCache(int maxSize) : base(maxSize)
		{
		}

		protected internal override int sizeOf(Uri key, Bitmap value)
		{
			return value.ByteCount;
		}
	}

}