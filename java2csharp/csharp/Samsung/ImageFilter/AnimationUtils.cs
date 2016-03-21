using System;

namespace com.example.filter_test
{

	using Activity = android.app.Activity;
	using CursorLoader = android.content.CursorLoader;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using CompressFormat = android.graphics.Bitmap.CompressFormat;
	using Config = android.graphics.Bitmap.Config;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Canvas = android.graphics.Canvas;
	using Paint = android.graphics.Paint;
	using Rect = android.graphics.Rect;
	using Uri = android.net.Uri;
	using Build = android.os.Build;
	using MediaStore = android.provider.MediaStore;

	public class AnimationUtils
	{

		/// <summary>
		/// Get the real file path from URI registered in media store
		/// </summary>
		/// <param name="contentUri">
		///            URI registered in media store </param>
		public static string getRealPathFromURI(Activity activity, Uri contentUri)
		{

			string releaseNumber = Build.VERSION.RELEASE;

			if (releaseNumber != null)
			{
				/* ICS Version */
				if (releaseNumber.Length > 0 && releaseNumber[0] == '4')
				{
					string[] proj = new string[] {MediaStore.Images.Media.DATA};
					string strFileName = "";
					CursorLoader cursorLoader = new CursorLoader(activity, contentUri, proj, null, null, null);
					if (cursorLoader != null)
					{
						Cursor cursor = cursorLoader.loadInBackground();
						if (cursor != null)
						{
							int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
							cursor.moveToFirst();
							if (cursor.Count > 0)
							{
								strFileName = cursor.getString(column_index);
							}

							cursor.close();
						}
					}
					return strFileName;
				}
				/* GB Version */
				else if (releaseNumber.StartsWith("2.3", StringComparison.Ordinal))
				{
					string[] proj = new string[] {MediaStore.Images.Media.DATA};
					string strFileName = "";
					Cursor cursor = activity.managedQuery(contentUri, proj, null, null, null);
					if (cursor != null)
					{
						int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

						cursor.moveToFirst();
						if (cursor.Count > 0)
						{
							strFileName = cursor.getString(column_index);
						}

						cursor.close();
					}
					return strFileName;
				}
			}

			// ---------------------
			// Undefined Version
			// ---------------------
			/* GB, ICS Common */
			string[] proj = new string[] {MediaStore.Images.Media.DATA};
			string strFileName = "";
			Cursor cursor = activity.managedQuery(contentUri, proj, null, null, null);
			if (cursor != null)
			{
				int column_index = cursor.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);

				// Use the Cursor manager in ICS
				activity.startManagingCursor(cursor);

				cursor.moveToFirst();
				if (cursor.Count > 0)
				{
					strFileName = cursor.getString(column_index);
				}

				// cursor.close(); // If the cursor close use , This application is terminated .(In ICS Version)
				activity.stopManagingCursor(cursor);
			}
			return strFileName;
		}

		/// <summary>
		/// Save jpeg image with background color
		/// </summary>
		/// <param name="strFileName">
		///            Save file path </param>
		/// <param name="bitmap">
		///            Input bitmap </param>
		/// <param name="nQuality">
		///            Jpeg quality for saving </param>
		/// <param name="nBackgroundColor">
		///            background color </param>
		/// <returns> whether success or not </returns>
		public static bool saveBitmapJPEGWithBackgroundColor(string strFileName, Bitmap bitmap, int nQuality, int nBackgroundColor)
		{
			bool bSuccess1 = false;
			bool bSuccess2 = false;
			bool bSuccess3;
			File saveFile = new File(strFileName);

			if (saveFile.exists())
			{
				if (!saveFile.delete())
				{
					return false;
				}
			}

			int nA = (nBackgroundColor >> 24) & 0xff;

			// If Background color alpha is 0, Background color substitutes as white
			if (nA == 0)
			{
				nBackgroundColor = unchecked((int)0xFFFFFFFF);
			}

			Rect rect = new Rect(0, 0, bitmap.Width, bitmap.Height);
			Bitmap newBitmap = Bitmap.createBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.ARGB_8888);
			Canvas canvas = new Canvas(newBitmap);
			canvas.drawColor(nBackgroundColor);
			canvas.drawBitmap(bitmap, rect, rect, new Paint());

			// Quality limitation min/max
			if (nQuality < 10)
			{
				nQuality = 10;
			}
			else if (nQuality > 100)
			{
				nQuality = 100;
			}

			System.IO.Stream @out = null;

			try
			{
				bSuccess1 = saveFile.createNewFile();
			}
			catch (IOException e1)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
			}

			try
			{
				@out = new System.IO.FileStream(saveFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				bSuccess2 = newBitmap.compress(Bitmap.CompressFormat.JPEG, nQuality, @out);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			try
			{
				if (@out != null)
				{
					@out.Flush();
					@out.Close();
					bSuccess3 = true;
				}
				else
				{
					bSuccess3 = false;
				}

			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				bSuccess3 = false;
			}
			finally
			{
				if (@out != null)
				{
					try
					{
						@out.Close();
					}
					catch (IOException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}

			return (bSuccess1 && bSuccess2 && bSuccess3);
		}

		/// <summary>
		/// Save PNG image with background color
		/// </summary>
		/// <param name="strFileName">
		///            Save file path </param>
		/// <param name="bitmap">
		///            Input bitmap </param>
		/// <param name="nQuality">
		///            Jpeg quality for saving </param>
		/// <param name="nBackgroundColor">
		///            background color </param>
		/// <returns> whether success or not </returns>
		public static bool saveBitmapPNGWithBackgroundColor(string strFileName, Bitmap bitmap, int nBackgroundColor)
		{
			bool bSuccess1 = false;
			bool bSuccess2 = false;
			bool bSuccess3;
			File saveFile = new File(strFileName);

			if (saveFile.exists())
			{
				if (!saveFile.delete())
				{
					return false;
				}
			}

			int nA = (nBackgroundColor >> 24) & 0xff;

			// If Background color alpha is 0, Background color substitutes as white
			if (nA == 0)
			{
				nBackgroundColor = unchecked((int)0xFFFFFFFF);
			}

			Rect rect = new Rect(0, 0, bitmap.Width, bitmap.Height);
			Bitmap newBitmap = Bitmap.createBitmap(bitmap.Width, bitmap.Height, Bitmap.Config.ARGB_8888);
			Canvas canvas = new Canvas(newBitmap);
			canvas.drawColor(nBackgroundColor);
			canvas.drawBitmap(bitmap, rect, rect, new Paint());

			System.IO.Stream @out = null;

			try
			{
				bSuccess1 = saveFile.createNewFile();
			}
			catch (IOException e1)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
			}

			try
			{
				@out = new System.IO.FileStream(saveFile, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				bSuccess2 = newBitmap.compress(Bitmap.CompressFormat.PNG, 100, @out);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			try
			{
				if (@out != null)
				{
					@out.Flush();
					@out.Close();
					bSuccess3 = true;
				}
				else
				{
					bSuccess3 = false;
				}

			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				bSuccess3 = false;
			}
			finally
			{
				if (@out != null)
				{
					try
					{
						@out.Close();
					}
					catch (IOException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}

			return (bSuccess1 && bSuccess2 && bSuccess3);
		}

		// Check whether valid image file or not
		public static bool isValidImagePath(string strImagePath)
		{
			if (strImagePath == null)
			{
				return false;
			}
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.inJustDecodeBounds = true;
			BitmapFactory.decodeFile(strImagePath, options);

			return (options.outMimeType != null);
		}

		// Check whether valid file name or not
		public static bool isValidSaveName(string fileName)
		{

			int len = fileName.Length;
			for (int i = 0; i < len; i++)
			{
				char c = fileName[i];

				if (c == '\\' || c == ':' || c == '/' || c == '*' || c == '?' || c == '"' || c == '<' || c == '>' || c == '|' || c == '\t' || c == '\n')
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		///**************************************************************************************************************
		/// Get the image bitmap that resizing to maximum size of limit.
		/// Parameter :
		/// - context : Context
		/// - uri : Image URI
		/// - bContentStreamImage : Gallery contents stream file(true)/file path(false)
		/// - nMaxResizedWidth : The maximum allowable width of resizing image.
		/// - nMaxResizedHeight : The maximum allowable height of resizing image.
		/// Return :
		/// - Resizing bitmap
		/// </summary>
		public static Bitmap getSafeResizingBitmap(string strImagePath, int nMaxResizedWidth, int nMaxResizedHeight)
		{
			// ==========================================
			// Bitmap Option
			// ==========================================
			BitmapFactory.Options options = getBitmapSize(strImagePath);

			if (options == null)
			{
				return null;
			}

			// ==========================================
			// Bitmap Scaling
			// ==========================================
			int nSampleSize = getSafeResizingSampleSize(options.outWidth, options.outHeight, nMaxResizedWidth, nMaxResizedHeight);

			// ==========================================
			// Load the bitmap including actually data.
			// ==========================================
			options.inJustDecodeBounds = false;
			options.inSampleSize = nSampleSize;
			options.inDither = false;
			options.inPreferredConfig = Bitmap.Config.ARGB_8888;
			options.inPurgeable = true;

			Bitmap photo = BitmapFactory.decodeFile(strImagePath, options);

			return photo;
		}

		/// <summary>
		///**************************************************************************************************************
		/// Get the option for obtain the image size of bitmap.
		/// Parameter :
		/// - context : Context
		/// - uri : Image URI
		/// - bContentStreamImage : Gallery contents stream file(true)/File path(false)
		/// Return :
		/// - BitmapFactory.Options
		/// </summary>
		public static BitmapFactory.Options getBitmapSize(string strImagePath)
		{
			// ==========================================
			// Load the temporary bitmap for getting size.
			// ==========================================
			BitmapFactory.Options options = new BitmapFactory.Options();
			options.inJustDecodeBounds = true;
			// Bitmap photo = BitmapFactory.decodeFile(strPath, options);
			BitmapFactory.decodeFile(strImagePath, options);

			return options;
		}

		/// <summary>
		///**************************************************************************************************************
		/// Get the sampling size for load the bitmap. (If you load the bitmap file of big size, This application is
		/// terminated.)
		/// Parameter :
		/// - nOrgWidth : The width of the original image (Value of outWidth of BitmapFactory.Options)
		/// - nOrgHeight : The height of the original image (Value of outHeight of BitmapFactory.Options)
		/// - nMaxWidth : The width of the image of maximum size. (width under 3M. ex.3000)
		/// - nMaxHeight : The height of the image of maximum size. (height under 3M. ex.1000) *
		/// Return :
		/// - Sampling size (If no need to resize, return 1). Throttled much larger.
		/// - If more than x.5 times , divide x+1 times.
		/// </summary>
		public static int getSafeResizingSampleSize(int nOrgWidth, int nOrgHeight, int nMaxWidth, int nMaxHeight)
		{
			int size = 1;
			float fsize;
			float fWidthScale = 0;
			float fHeightScale = 0;

			if (nOrgWidth > nMaxWidth || nOrgHeight > nMaxHeight)
			{
				if (nOrgWidth > nMaxWidth)
				{
					fWidthScale = (float) nOrgWidth / (float) nMaxWidth;
				}
				if (nOrgHeight > nMaxHeight)
				{
					fHeightScale = (float) nOrgHeight / (float) nMaxHeight;
				}

				if (fWidthScale >= fHeightScale)
				{
					fsize = fWidthScale;
				}
				else
				{
					fsize = fHeightScale;
				}

				size = (int) fsize;
			}

			return size;
		}

	}

}