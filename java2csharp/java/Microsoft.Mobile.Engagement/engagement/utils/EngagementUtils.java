/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.utils;

import static android.content.pm.PackageManager.GET_META_DATA;
import android.app.Activity;
import android.content.Context;
import android.content.pm.PackageManager;
import android.os.Bundle;

/** Utility functions */
public final class EngagementUtils
{
  private EngagementUtils()
  {
    /* Prevent instantiation */
  }

  /**
   * Get application meta-data of the current package name.
   * @param context application context.
   * @return meta-data, may be empty but never null.
   */
  public static Bundle getMetaData(Context context)
  {
    Bundle config;
    try
    {
      config = context.getPackageManager().getApplicationInfo(context.getPackageName(),
        PackageManager.GET_META_DATA).metaData;
      if (config == null)
        config = new Bundle();
    }
    catch (Exception e)
    {
      /*
       * NameNotFoundException or in some rare scenario an undocumented "RuntimeException: Package
       * manager has died.", probably caused by a system app process crash.
       */
      config = new Bundle();
    }
    return config;
  }

  /**
   * Get activity meta-data.
   * @param activity activity to get meta-data from.
   * @return meta-data, may be empty but never null.
   */
  public static Bundle getActivityMetaData(Activity activity)
  {
    Bundle config;
    try
    {
      config = activity.getPackageManager().getActivityInfo(activity.getComponentName(),
        GET_META_DATA).metaData;
      if (config == null)
        config = new Bundle();
    }
    catch (Exception e)
    {
      /*
       * NameNotFoundException or in some rare scenario an undocumented "RuntimeException: Package
       * manager has died.", probably caused by a system app process crash.
       */
      config = new Bundle();
    }
    return config;
  }
}
