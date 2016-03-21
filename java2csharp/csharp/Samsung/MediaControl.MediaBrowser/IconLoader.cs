namespace com.samsung.android.sdk.sample.mediabrowser
{

	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using AsyncTask = android.os.AsyncTask;
	using ImageView = android.widget.ImageView;


	/// <summary>
	/// This class fetches an image from specified <seealso cref="Uri"/> and displays in <seealso cref="ImageView"/>
	/// </summary>
	public class IconLoader : AsyncTask<Uri, Void, Bitmap>
	{
		private const int MAX_ICON_SIZE = 100;
		private readonly ImageView mImageView;
		private IconCache mIconsCache;

		public IconLoader(ImageView imageView)
		{
			mImageView = imageView;
			mIconsCache = IconCache.Instance;
		}

		protected internal override Bitmap doInBackground(params Uri[] @params)
		{
			System.IO.Stream @in = null;
			try
			{
				@in = openConnection(@params[0]);
				BitmapFactory.Options opt = new BitmapFactory.Options();
				opt.inJustDecodeBounds = true;
				opt.inPreferredConfig = Bitmap.Config.RGB_565;
				BitmapFactory.decodeStream(@in, null, opt);
				@in.Close();
				int width = opt.outWidth;
				int height = opt.outHeight;
				int scale = 1;
				while (width > MAX_ICON_SIZE && height > MAX_ICON_SIZE)
				{
					width /= 2;
					height /= 2;
					scale *= 2;
				}
				opt.inJustDecodeBounds = false;
				opt.inSampleSize = scale;
				@in = openConnection(@params[0]);
				Bitmap bitmap = BitmapFactory.decodeStream(@in, null, opt);
				if (bitmap == null)
				{
					return null;
				}

				// Add the bitmap to cache.
				if (mIconsCache != null)
				{
					mIconsCache.put(@params[0], bitmap);
				}
				//return the bitmap only if target image view is still valid
				if (@params[0].Equals(mImageView.Tag))
				{
					return bitmap;
				}
				else
				{
					return null;
				}
			}
			catch (IOException)
			{
				// Failed to retrieve icon, ignore it
				return null;
			}
			finally
			{
				if (@in != null)
				{
					try
					{
						@in.Close();
					}
					catch (IOException)
					{
					}
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private java.io.InputStream openConnection(android.net.Uri uri) throws java.io.IOException
		private System.IO.Stream openConnection(Uri uri)
		{
			URL url = new URL(uri.ToString());
			URLConnection conn = url.openConnection();
			return conn.InputStream;
		}

		protected internal override void onPostExecute(Bitmap result)
		{
			if (result != null && mIconsCache != null)
			{
				mImageView.ImageBitmap = result;
			}
		}
	}
}