/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import android.app.ActivityManager;
import android.app.ActivityManager.RunningAppProcessInfo;
import android.content.Context;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.ServiceInfo;
import android.os.Process;

import com.microsoft.azure.engagement.activity.EngagementActivity;

/**
 * Utility functions used by various classes of the Engagement SDK.
 */
public final class EngagementAgentUtils
{
  /** Service class name */
  private static final String SERVICE_CLASS = "com.microsoft.azure.engagement.service.EngagementService";

  private EngagementAgentUtils()
  {
    /* Prevent instantiation */
  }

  /**
   * Get the Engagement service intent to bind to.
   * @param context any application context.
   * @return an explicit intent that can used to bind to the service.
   */
  public static Intent getServiceIntent(Context context)
  {
    /* Build the base intent */
    Intent intent = new Intent();
    intent.setClassName(context, SERVICE_CLASS);
    return intent;
  }

  /**
   * Return <tt>true</tt> if the caller runs in a process dedicated to the Engagement service.<br/>
   * Return <tt>false</tt> otherwise, e.g. if it's the application process (even if the Engagement
   * service is running in it) or another process.<br/>
   * This method is useful when the <b>android:process</b> attribute has been set on the Engagement
   * service, if this method return <tt>true</tt>, application initialization must not be done in
   * that process. This method is used by {@link EngagementApplication}.
   * @param context the application context.
   * @return <tt>true</tt> if the caller is running in a process dedicated to the Engagement
   *         service, <tt>false</tt> otherwise.
   * @see EngagementApplication
   */
  public static boolean isInDedicatedEngagementProcess(Context context)
  {
    /* Get our package info */
    PackageInfo packageInfo;
    try
    {
      packageInfo = context.getPackageManager().getPackageInfo(context.getPackageName(),
        PackageManager.GET_SERVICES);
    }
    catch (Exception e)
    {
      /*
       * NameNotFoundException (uninstalling?) or in some rare scenario an undocumented
       * "RuntimeException: Package manager has died.", probably caused by a system app process
       * crash.
       */
      return false;
    }

    /* Get main process name */
    String mainProcess = packageInfo.applicationInfo.processName;

    /* Get embedded Engagement process name */
    String engagementProcess = null;
    if (packageInfo.services != null)
      for (ServiceInfo serviceInfo : packageInfo.services)
        if (SERVICE_CLASS.equals(serviceInfo.name))
        {
          engagementProcess = serviceInfo.processName;
          break;
        }

    /* If the embedded Engagement service runs on its own process */
    if (engagementProcess != null && !engagementProcess.equals(mainProcess))
    {
      /* The result is to check if the current process is the engagement process */
      ActivityManager activityManager = (ActivityManager) context.getSystemService(Context.ACTIVITY_SERVICE);
      for (RunningAppProcessInfo rapInfo : activityManager.getRunningAppProcesses())
        if (rapInfo.pid == Process.myPid())
          return rapInfo.processName.equals(engagementProcess);
    }

    /* Otherwise engagement is not running in a separate process (or not running at all) */
    return false;
  }

  /**
   * Build an Engagement alias for an Android Activity class. This implementation takes the simple
   * name of the class and removes the "Activity" suffix if any (e.g. "com.mycompany.MainActivity"
   * becomes "Main").<br/>
   * This method is used by {@link EngagementActivity} and its variants.
   * @return an activity name suitable to be reported by the Engagement service.
   */
  public static String buildEngagementActivityName(Class<?> activityClass)
  {
    String name = activityClass.getSimpleName();
    String suffix = "Activity";
    if (name.endsWith(suffix) && name.length() > suffix.length())
      return name.substring(0, name.length() - suffix.length());
    else
      return name;
  }
}
