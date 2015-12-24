/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using DownloadManager = android.app.DownloadManager;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	/// <summary>
	/// Integrating this class in the AndroidManifest.xml is required for the Reach SDK to be able to
	/// process big picture notifications.<br/>
	/// Add the following section in the AndroidManifest.xml:
	/// 
	/// <pre>
	/// {@code <receiver android:name="com.microsoft.azure.engagement.reach.EngagementReachDownloadReceiver">
	///   <intent-filter>
	///     <action android:name="android.intent.action.DOWNLOAD_COMPLETE"/>
	///   </intent-filter>
	/// </receiver>
	/// }
	/// </pre>
	/// </summary>
	public class EngagementReachDownloadReceiver : BroadcastReceiver
	{
	  public override void onReceive(Context context, Intent intent)
	  {
		/* Big picture downloaded */
		if (DownloadManager.ACTION_DOWNLOAD_COMPLETE.Equals(intent.Action))
		{
		  /* Get content by download id */
		  long downloadId = intent.getLongExtra(DownloadManager.EXTRA_DOWNLOAD_ID, 0);
		  EngagementReachAgent reachAgent = EngagementReachAgent.getInstance(context);
		  EngagementReachInteractiveContent content = reachAgent.getContentByDownloadId(downloadId);

		  /* Delegate to agent if content found */
		  if (content != null)
		  {
			reachAgent.onDownloadComplete(content);
		  }
		}
	  }
	}

}