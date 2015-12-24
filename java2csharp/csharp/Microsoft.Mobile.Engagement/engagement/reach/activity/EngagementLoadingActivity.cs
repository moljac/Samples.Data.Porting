/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.reach.EngagementReachAgent.INTENT_EXTRA_CONTENT_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.utils.EngagementResourcesUtils.getId;
	using TargetApi = android.annotation.TargetApi;
	using Color = android.graphics.Color;
	using ColorDrawable = android.graphics.drawable.ColorDrawable;
	using Build = android.os.Build;
	using Bundle = android.os.Bundle;
	using Window = android.view.Window;

	using EngagementActivity = com.microsoft.azure.engagement.activity.EngagementActivity;

	/// <summary>
	/// Activity displaying a loading screen before displaying a content. Add this in the
	/// AndroidManifest.xml file to use it:
	/// 
	/// <pre>
	/// {@code <activity
	///   android:name="com.microsoft.azure.engagement.reach.activity.EngagementLoadingActivity"
	///   android:theme="@android:style/Theme.Dialog">
	///   <intent-filter>
	///     <action android:name="com.microsoft.azure.engagement.reach.intent.action.LOADING"/>
	///     <category android:name="android.intent.category.DEFAULT"/>
	///   </intent-filter>
	/// </activity>}
	/// </pre>
	/// </summary>
	public class EngagementLoadingActivity : EngagementActivity
	{
	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		configureWindow();
		ContentView = getId(this, LayoutName, "layout");
	  }

	  /// <summary>
	  /// Configure window features, theme etc... before setContentView is called. Override this method
	  /// to customize window.
	  /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TargetApi(android.os.Build.VERSION_CODES.ICE_CREAM_SANDWICH) protected void configureWindow()
	  protected internal virtual void configureWindow()
	  {
		/* Remove activity title */
		requestWindowFeature(Window.FEATURE_NO_TITLE);

		/*
		 * Cumulative theme to android:theme="@android:style/Theme.Dialog" in Manifest for changing
		 * progress bar look and feel to better match device version. You still need to keep
		 * android:theme="@android:style/Theme.Dialog" in Manifest otherwise it would be full screen and
		 * not semi-transparent, themes seem to combine correctly only when the first one is in Manifest
		 * and the second one in code.
		 */
		if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.ICE_CREAM_SANDWICH)
		{
		  Theme = android.R.style.Theme_DeviceDefault_Dialog;
		}

		/* Remove dialog frame, we can't do it in layout */
		Window.BackgroundDrawable = new ColorDrawable(Color.TRANSPARENT);
	  }

	  protected internal override void onUserLeaveHint()
	  {
		finish();
	  }

	  protected internal override void onResume()
	  {
		long id = Intent.getLongExtra(INTENT_EXTRA_CONTENT_ID, -1);
		if (!EngagementReachAgent.getInstance(this).isLoading(id))
		{
		  finish();
		}
		base.onResume();
	  }

	  protected internal override void onPause()
	  {
		if (Finishing)
		{
		  long id = Intent.getLongExtra(INTENT_EXTRA_CONTENT_ID, -1);
		  EngagementReachAgent.getInstance(this).exitLoading(id);
		}
		base.onPause();
	  }

	  /// <summary>
	  /// Get layout name </summary>
	  protected internal virtual string LayoutName
	  {
		  get
		  {
			return "engagement_loading";
		  }
	  }
	}

}