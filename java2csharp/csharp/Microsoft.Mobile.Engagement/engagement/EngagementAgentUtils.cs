using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

	using ActivityManager = android.app.ActivityManager;
	using RunningAppProcessInfo = android.app.ActivityManager.RunningAppProcessInfo;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using PackageInfo = android.content.pm.PackageInfo;
	using PackageManager = android.content.pm.PackageManager;
	using ServiceInfo = android.content.pm.ServiceInfo;
	using Process = android.os.Process;

	using EngagementActivity = com.microsoft.azure.engagement.activity.EngagementActivity;

	/// <summary>
	/// Utility functions used by various classes of the Engagement SDK.
	/// </summary>
	public sealed class EngagementAgentUtils
	{
	  /// <summary>
	  /// Service class name </summary>
	  private const string SERVICE_CLASS = "com.microsoft.azure.engagement.service.EngagementService";

	  private EngagementAgentUtils()
	  {
		/* Prevent instantiation */
	  }

	  /// <summary>
	  /// Get the Engagement service intent to bind to. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <returns> an explicit intent that can used to bind to the service. </returns>
	  public static Intent getServiceIntent(Context context)
	  {
		/* Build the base intent */
		Intent intent = new Intent();
		intent.setClassName(context, SERVICE_CLASS);
		return intent;
	  }

	  /// <summary>
	  /// Return <tt>true</tt> if the caller runs in a process dedicated to the Engagement service.<br/>
	  /// Return <tt>false</tt> otherwise, e.g. if it's the application process (even if the Engagement
	  /// service is running in it) or another process.<br/>
	  /// This method is useful when the <b>android:process</b> attribute has been set on the Engagement
	  /// service, if this method return <tt>true</tt>, application initialization must not be done in
	  /// that process. This method is used by <seealso cref="EngagementApplication"/>. </summary>
	  /// <param name="context"> the application context. </param>
	  /// <returns> <tt>true</tt> if the caller is running in a process dedicated to the Engagement
	  ///         service, <tt>false</tt> otherwise. </returns>
	  /// <seealso cref= EngagementApplication </seealso>
	  public static bool isInDedicatedEngagementProcess(Context context)
	  {
		/* Get our package info */
		PackageInfo packageInfo;
		try
		{
	  packageInfo = context.PackageManager.getPackageInfo(context.PackageName, PackageManager.GET_SERVICES);
		}
		catch (Exception)
		{
		  /*
		   * NameNotFoundException (uninstalling?) or in some rare scenario an undocumented
		   * "RuntimeException: Package manager has died.", probably caused by a system app process
		   * crash.
		   */
		  return false;
		}

		/* Get main process name */
		string mainProcess = packageInfo.applicationInfo.processName;

		/* Get embedded Engagement process name */
		string engagementProcess = null;
		if (packageInfo.services != null)
		{
		  foreach (ServiceInfo serviceInfo in packageInfo.services)
		  {
			if (SERVICE_CLASS.Equals(serviceInfo.name))
			{
			  engagementProcess = serviceInfo.processName;
			  break;
			}
		  }
		}

		/* If the embedded Engagement service runs on its own process */
		if (engagementProcess != null && !engagementProcess.Equals(mainProcess))
		{
		  /* The result is to check if the current process is the engagement process */
		  ActivityManager activityManager = (ActivityManager) context.getSystemService(Context.ACTIVITY_SERVICE);
		  foreach (ActivityManager.RunningAppProcessInfo rapInfo in activityManager.RunningAppProcesses)
		  {
			if (rapInfo.pid == Process.myPid())
			{
			  return rapInfo.processName.Equals(engagementProcess);
			}
		  }
		}

		/* Otherwise engagement is not running in a separate process (or not running at all) */
		return false;
	  }

	  /// <summary>
	  /// Build an Engagement alias for an Android Activity class. This implementation takes the simple
	  /// name of the class and removes the "Activity" suffix if any (e.g. "com.mycompany.MainActivity"
	  /// becomes "Main").<br/>
	  /// This method is used by <seealso cref="EngagementActivity"/> and its variants. </summary>
	  /// <returns> an activity name suitable to be reported by the Engagement service. </returns>
	  public static string buildEngagementActivityName(Type activityClass)
	  {
		string name = activityClass.Name;
		string suffix = "Activity";
		if (name.EndsWith(suffix, StringComparison.Ordinal) && name.Length > suffix.Length)
		{
		  return name.Substring(0, name.Length - suffix.Length);
		}
		else
		{
		  return name;
		}
	  }
	}

}