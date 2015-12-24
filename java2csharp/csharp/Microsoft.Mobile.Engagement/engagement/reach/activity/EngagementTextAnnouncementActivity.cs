/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{

	/// <summary>
	/// Activity displaying a plain text Engagement announcement. Add this in the AndroidManifest.xml
	/// file to use it:
	/// 
	/// <pre>
	/// {@code <activity
	///   android:name="com.microsoft.azure.engagement.reach.activity.EngagementTextAnnouncementActivity"
	///   android:theme="@android:style/Theme.Light">
	///     <intent-filter>
	///       <action android:name="com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT" />
	///       <category android:name="android.intent.category.DEFAULT" />
	///       <data android:mimeType="text/plain" />
	///     </intent-filter>
	/// </activity>}
	/// </pre>
	/// </summary>
	public class EngagementTextAnnouncementActivity : EngagementAnnouncementActivity
	{
	  protected internal override string LayoutName
	  {
		  get
		  {
			return "engagement_text_announcement";
		  }
	  }
	}

}