using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.v11
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.R.dimen.notification_large_icon_height;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.R.dimen.notification_large_icon_width;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.AlarmManager.ELAPSED_REALTIME_WAKEUP;


	using TargetApi = android.annotation.TargetApi;
	using AlarmManager = android.app.AlarmManager;
	using DownloadManager = android.app.DownloadManager;
	using PendingIntent = android.app.PendingIntent;
	using ContentResolver = android.content.ContentResolver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Resources = android.content.res.Resources;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using Build = android.os.Build;
	using SystemClock = android.os.SystemClock;


	/// <summary>
	/// Notification utilities depending on API level 11. </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TargetApi(Build.VERSION_CODES.HONEYCOMB) public final class EngagementNotificationUtilsV11
	public sealed class EngagementNotificationUtilsV11
	{
	  /// <summary>
	  /// Download timeout in ms </summary>
	  private const int DOWNLOAD_TIMEOUT = 10000;

	  private EngagementNotificationUtilsV11()
	  {
		/* Utils pattern: prevent instances */
	  }

	  /// <summary>
	  /// Scale a bitmap down to fit as a large icon in a system notification. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="bitmap"> the bitmap to scale. </param>
	  /// <returns> bitmap to use, may be the original one if scaling is not needed or a memory problem
	  ///         occurred. </returns>
	  public static Bitmap scaleBitmapForLargeIcon(Context context, Bitmap bitmap)
	  {
		/* Get large icon dimensions on this device (it depends on density) */
		Resources resources = context.Resources;
		double maxWidth = resources.getDimension(notification_large_icon_width);
		double maxHeight = resources.getDimension(notification_large_icon_height);
		int width = bitmap.Width;
		int height = bitmap.Height;

		/* If one of the dimensions is too large, scale down */
		if (width > maxWidth || height > maxHeight)
		{
		  double widthRatio = width / maxWidth;
		  double heightRatio = height / maxHeight;
		  double ratio = Math.Max(widthRatio, heightRatio);
		  int dstWidth = (int)(Math.Round(width / ratio));
		  int dstHeight = (int)(Math.Round(height / ratio));
		  try
		  {
			bitmap = Bitmap.createScaledBitmap(bitmap, dstWidth, dstHeight, true);
		  }
		  catch (System.OutOfMemoryException)
		  {
			/* Abort, return original bitmap */
		  }
		}
		return bitmap;
	  }

	  /// <summary>
	  /// Schedule the download of the big picture associated with the content. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="content"> content with big picture notification. </param>
	  public static void downloadBigPicture(Context context, EngagementReachInteractiveContent content)
	  {
		/* Set up download request */
		DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		Uri uri = Uri.parse(content.NotificationBigPicture);
		DownloadManager.Request request = new DownloadManager.Request(uri);
		request.NotificationVisibility = DownloadManager.Request.VISIBILITY_HIDDEN;
		request.VisibleInDownloadsUi = false;

		/* Create intermediate directories */
		File dir = context.getExternalFilesDir("engagement");
		dir = new File(dir, "big-picture");
		dir.mkdirs();

		/* Set destination */
		long contentId = content.LocalId;
		request.DestinationUri = Uri.fromFile(new File(dir, contentId.ToString()));

		/* Submit download */
		long id = downloadManager.enqueue(request);
		content.setDownloadId(context, id);

		/* Set up timeout on download */
		Intent intent = new Intent(EngagementReachAgent.INTENT_ACTION_DOWNLOAD_TIMEOUT);
		intent.putExtra(EngagementReachAgent.INTENT_EXTRA_CONTENT_ID, contentId);
		intent.Package = context.PackageName;
		PendingIntent operation = PendingIntent.getBroadcast(context, (int) contentId, intent, 0);
		long triggerAtMillis = SystemClock.elapsedRealtime() + DOWNLOAD_TIMEOUT;
		AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
		alarmManager.set(ELAPSED_REALTIME_WAKEUP, triggerAtMillis, operation);
	  }

	  /// <summary>
	  /// Decode downloaded big picture. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="downloadId"> downloaded picture identifier. </param>
	  /// <returns> decoded bitmap if successful, null on error. </returns>
	  public static Bitmap getBigPicture(Context context, long downloadId)
	  {
		/* Decode bitmap */
		System.IO.Stream stream = null;
		try
		{
		  /*
		   * Query download manager to get file. FIXME For an unknown reason, using the file descriptor
		   * fails after APK build with ProGuard, we use stream instead.
		   */
		  DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		  Uri uri = downloadManager.getUriForDownloadedFile(downloadId);
		  ContentResolver contentResolver = context.ContentResolver;
		  stream = contentResolver.openInputStream(uri);

		  /*
		   * Bitmaps larger than 2048 in any dimension are likely to cause OutOfMemoryError in the
		   * NotificationManager or even here. Plus some devices simply don't accept such images: the
		   * notification manager can drop the notification without any way to check that via API. To
		   * avoid the problem, sub sample the image in an efficient way (not using Bitmap matrix
		   * scaling after decoding the bitmap with original size: it could run out of memory when
		   * decoding the full image). FIXME There is center cropping applied by the NotificationManager
		   * on the bitmap we provide, we can't avoid it, see
		   * https://code.google.com/p/android/issues/detail?id=58318.
		   */
		  BitmapFactory.Options options = new BitmapFactory.Options();
		  options.inJustDecodeBounds = true;
		  options.inSampleSize = 1;
		  options.inPreferQualityOverSpeed = true;

		  /* Decode dimensions */
		  BitmapFactory.decodeStream(stream, null, options);
		  int maxDim = Math.Max(options.outWidth, options.outHeight);

		  /* Compute sub sample size (it must be a power of 2) */
		  while (maxDim > 2048)
		  {
			options.inSampleSize <<= 1;
			maxDim >>= 1;
		  }

		  /* Decode actual bitmap */
		  options.inJustDecodeBounds = false;
		  stream.Close();
		  stream = contentResolver.openInputStream(uri);
		  return BitmapFactory.decodeStream(stream, null, options);
		}
		catch (Exception)
		{
		  /* Abort, causes are likely FileNotFoundException or OutOfMemoryError */
		  return null;
		}
		finally
		{
		  /* Silently close stream */
		  if (stream != null)
		  {
			try
			{
			  stream.Close();
			}
			catch (IOException)
			{
			}
		  }
		}
	  }

	  /// <summary>
	  /// Cancel download or erase already downloaded file for the specified content. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="downloadId"> download identifier. </param>
	  public static void deleteDownload(Context context, long downloadId)
	  {
		DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
		downloadManager.remove(downloadId);
	  }
	}

}