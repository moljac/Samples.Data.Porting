/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.v11;

import static android.R.dimen.notification_large_icon_height;
import static android.R.dimen.notification_large_icon_width;
import static android.app.AlarmManager.ELAPSED_REALTIME_WAKEUP;

import java.io.File;
import java.io.IOException;
import java.io.InputStream;

import android.annotation.TargetApi;
import android.app.AlarmManager;
import android.app.DownloadManager;
import android.app.PendingIntent;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.res.Resources;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Build;
import android.os.SystemClock;

import com.microsoft.azure.engagement.reach.EngagementReachAgent;
import com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent;

/** Notification utilities depending on API level 11. */
@TargetApi(Build.VERSION_CODES.HONEYCOMB)
public final class EngagementNotificationUtilsV11
{
  /** Download timeout in ms */
  private static final int DOWNLOAD_TIMEOUT = 10000;

  private EngagementNotificationUtilsV11()
  {
    /* Utils pattern: prevent instances */
  }

  /**
   * Scale a bitmap down to fit as a large icon in a system notification.
   * @param context any application context.
   * @param bitmap the bitmap to scale.
   * @return bitmap to use, may be the original one if scaling is not needed or a memory problem
   *         occurred.
   */
  public static Bitmap scaleBitmapForLargeIcon(Context context, Bitmap bitmap)
  {
    /* Get large icon dimensions on this device (it depends on density) */
    Resources resources = context.getResources();
    double maxWidth = resources.getDimension(notification_large_icon_width);
    double maxHeight = resources.getDimension(notification_large_icon_height);
    int width = bitmap.getWidth();
    int height = bitmap.getHeight();

    /* If one of the dimensions is too large, scale down */
    if (width > maxWidth || height > maxHeight)
    {
      double widthRatio = width / maxWidth;
      double heightRatio = height / maxHeight;
      double ratio = Math.max(widthRatio, heightRatio);
      int dstWidth = (int) (Math.round(width / ratio));
      int dstHeight = (int) (Math.round(height / ratio));
      try
      {
        bitmap = Bitmap.createScaledBitmap(bitmap, dstWidth, dstHeight, true);
      }
      catch (OutOfMemoryError e)
      {
        /* Abort, return original bitmap */
      }
    }
    return bitmap;
  }

  /**
   * Schedule the download of the big picture associated with the content.
   * @param context any application context.
   * @param content content with big picture notification.
   */
  public static void downloadBigPicture(Context context, EngagementReachInteractiveContent content)
  {
    /* Set up download request */
    DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
    Uri uri = Uri.parse(content.getNotificationBigPicture());
    DownloadManager.Request request = new DownloadManager.Request(uri);
    request.setNotificationVisibility(DownloadManager.Request.VISIBILITY_HIDDEN);
    request.setVisibleInDownloadsUi(false);

    /* Create intermediate directories */
    File dir = context.getExternalFilesDir("engagement");
    dir = new File(dir, "big-picture");
    dir.mkdirs();

    /* Set destination */
    long contentId = content.getLocalId();
    request.setDestinationUri(Uri.fromFile(new File(dir, String.valueOf(contentId))));

    /* Submit download */
    long id = downloadManager.enqueue(request);
    content.setDownloadId(context, id);

    /* Set up timeout on download */
    Intent intent = new Intent(EngagementReachAgent.INTENT_ACTION_DOWNLOAD_TIMEOUT);
    intent.putExtra(EngagementReachAgent.INTENT_EXTRA_CONTENT_ID, contentId);
    intent.setPackage(context.getPackageName());
    PendingIntent operation = PendingIntent.getBroadcast(context, (int) contentId, intent, 0);
    long triggerAtMillis = SystemClock.elapsedRealtime() + DOWNLOAD_TIMEOUT;
    AlarmManager alarmManager = (AlarmManager) context.getSystemService(Context.ALARM_SERVICE);
    alarmManager.set(ELAPSED_REALTIME_WAKEUP, triggerAtMillis, operation);
  }

  /**
   * Decode downloaded big picture.
   * @param context any application context.
   * @param downloadId downloaded picture identifier.
   * @return decoded bitmap if successful, null on error.
   */
  public static Bitmap getBigPicture(Context context, long downloadId)
  {
    /* Decode bitmap */
    InputStream stream = null;
    try
    {
      /*
       * Query download manager to get file. FIXME For an unknown reason, using the file descriptor
       * fails after APK build with ProGuard, we use stream instead.
       */
      DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
      Uri uri = downloadManager.getUriForDownloadedFile(downloadId);
      ContentResolver contentResolver = context.getContentResolver();
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
      int maxDim = Math.max(options.outWidth, options.outHeight);

      /* Compute sub sample size (it must be a power of 2) */
      while (maxDim > 2048)
      {
        options.inSampleSize <<= 1;
        maxDim >>= 1;
      }

      /* Decode actual bitmap */
      options.inJustDecodeBounds = false;
      stream.close();
      stream = contentResolver.openInputStream(uri);
      return BitmapFactory.decodeStream(stream, null, options);
    }
    catch (Throwable t)
    {
      /* Abort, causes are likely FileNotFoundException or OutOfMemoryError */
      return null;
    }
    finally
    {
      /* Silently close stream */
      if (stream != null)
        try
        {
          stream.close();
        }
        catch (IOException e)
        {
        }
    }
  }

  /**
   * Cancel download or erase already downloaded file for the specified content.
   * @param context any application context.
   * @param downloadId download identifier.
   */
  public static void deleteDownload(Context context, long downloadId)
  {
    DownloadManager downloadManager = (DownloadManager) context.getSystemService(Context.DOWNLOAD_SERVICE);
    downloadManager.remove(downloadId);
  }
}
