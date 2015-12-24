using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.utils
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.content.pm.PackageManager.GET_META_DATA;
	using Activity = android.app.Activity;
	using Context = android.content.Context;
	using PackageManager = android.content.pm.PackageManager;
	using Bundle = android.os.Bundle;

	/// <summary>
	/// Utility functions </summary>
	public sealed class EngagementUtils
	{
	  private EngagementUtils()
	  {
		/* Prevent instantiation */
	  }

	  /// <summary>
	  /// Get application meta-data of the current package name. </summary>
	  /// <param name="context"> application context. </param>
	  /// <returns> meta-data, may be empty but never null. </returns>
	  public static Bundle getMetaData(Context context)
	  {
		Bundle config;
		try
		{
		  config = context.PackageManager.getApplicationInfo(context.PackageName, PackageManager.GET_META_DATA).metaData;
		  if (config == null)
		  {
			config = new Bundle();
		  }
		}
		catch (Exception)
		{
		  /*
		   * NameNotFoundException or in some rare scenario an undocumented "RuntimeException: Package
		   * manager has died.", probably caused by a system app process crash.
		   */
		  config = new Bundle();
		}
		return config;
	  }

	  /// <summary>
	  /// Get activity meta-data. </summary>
	  /// <param name="activity"> activity to get meta-data from. </param>
	  /// <returns> meta-data, may be empty but never null. </returns>
	  public static Bundle getActivityMetaData(Activity activity)
	  {
		Bundle config;
		try
		{
		  config = activity.PackageManager.getActivityInfo(activity.ComponentName, GET_META_DATA).metaData;
		  if (config == null)
		  {
			config = new Bundle();
		  }
		}
		catch (Exception)
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

}