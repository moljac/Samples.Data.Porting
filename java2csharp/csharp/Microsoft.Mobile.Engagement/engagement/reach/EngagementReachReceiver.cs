/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE_DLC;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE_PUSH;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;


	/// <summary>
	/// Integrating this class in the AndroidManifest.xml is required for the Reach SDK to be able to
	/// process incoming contents and handle system notifications.<br/>
	/// Add the following section in the AndroidManifest.xml:
	/// 
	/// <pre>
	/// {@code
	/// <receiver android:name="com.microsoft.azure.engagement.reach.EngagementReachReceiver" android:exported="false">
	///   <intent-filter>
	///     <action android:name="android.intent.action.BOOT_COMPLETED"/>
	///     <action android:name="com.microsoft.azure.engagement.intent.action.AGENT_CREATED"/>
	///     <action android:name="com.microsoft.azure.engagement.intent.action.MESSAGE"/>
	///     <action android:name="com.microsoft.azure.engagement.reach.intent.action.ACTION_NOTIFICATION"/>
	///     <action android:name="com.microsoft.azure.engagement.reach.intent.action.EXIT_NOTIFICATION"/>
	///     <action android:name="com.microsoft.azure.engagement.reach.intent.action.DOWNLOAD_TIMEOUT"/>
	///   </intent-filter>
	/// </receiver>
	/// }
	/// </pre>
	/// </summary>
	public class EngagementReachReceiver : BroadcastReceiver
	{
	  public override void onReceive(Context context, Intent intent)
	  {
		/* Boot: restore system notifications */
		string action = intent.Action;
		if (Intent.ACTION_BOOT_COMPLETED.Equals(action))
		{
		  EngagementReachAgent.getInstance(context).onDeviceBoot();
		}

		/* Just ensure the reach agent is loaded for checking pending contents in SQLite */
		else if (EngagementAgent.INTENT_ACTION_AGENT_CREATED.Equals(action))
		{
		  EngagementReachAgent.getInstance(context);
		}

		/* Notification actioned e.g. clicked (from the system notification) */
		else if (EngagementReachAgent.INTENT_ACTION_ACTION_NOTIFICATION.Equals(action))
		{
		  onNotificationActioned(context, intent);
		}

		/* System notification exited (clear button) */
		else if (EngagementReachAgent.INTENT_ACTION_EXIT_NOTIFICATION.Equals(action))
		{
		  onNotificationExited(context, intent);
		}

		/* Called when download takes too much time to complete */
		else if (EngagementReachAgent.INTENT_ACTION_DOWNLOAD_TIMEOUT.Equals(action))
		{
		  onDownloadTimeout(context, intent);
		}

		/* Called when we receive GCM or ADM push with azme parameters or message download completes. */
		else if (EngagementIntents.INTENT_ACTION_MESSAGE.Equals(action))
		{
		  onMessage(context, intent);
		}
	  }

	  /// <summary>
	  /// Called when a push message is received or message download completes. </summary>
	  /// <param name="context"> context. </param>
	  /// <param name="intent"> intent. </param>
	  private void onMessage(Context context, Intent intent)
	  {
		string type = intent.getStringExtra(INTENT_EXTRA_TYPE);
		if (INTENT_EXTRA_TYPE_PUSH.Equals(type))
		{
		  EngagementReachAgent.getInstance(context).onContentReceived(intent.Extras);
		}
		else if (INTENT_EXTRA_TYPE_DLC.Equals(type))
		{
		  EngagementReachAgent.getInstance(context).onMessageDownloaded(intent.Extras);
		}
	  }

	  /// <summary>
	  /// Called when a system notification for a content has been actioned. </summary>
	  /// <param name="context"> context. </param>
	  /// <param name="intent"> intent describing the content. </param>
	  private void onNotificationActioned(Context context, Intent intent)
	  {
		/* Get content */
		EngagementReachAgent reachAgent = EngagementReachAgent.getInstance(context);
		EngagementReachInteractiveContent content = reachAgent.getContent(intent);

		/* If content retrieved successfully */
		if (content != null)

		  /* Tell reach to start the content activity */
		{
		  content.actionNotification(context, true);
		}
	  }

	  /// <summary>
	  /// Called when a notification has been exited (clear button from notification panel). </summary>
	  /// <param name="context"> context. </param>
	  /// <param name="intent"> intent containing the content identifier to exit. </param>
	  private void onNotificationExited(Context context, Intent intent)
	  {
		/* Get content */
		EngagementReachAgent reachAgent = EngagementReachAgent.getInstance(context);
		EngagementReachInteractiveContent content = reachAgent.getContent(intent);

		/* Exit it if found */
		if (content != null)
		{
		  content.exitNotification(context);
		}
	  }

	  /// <summary>
	  /// Called when download times out. </summary>
	  /// <param name="context"> application context. </param>
	  /// <param name="intent"> timeout intent containing content identifier. </param>
	  private void onDownloadTimeout(Context context, Intent intent)
	  {
		/* Delegate to agent */
		EngagementReachAgent reachAgent = EngagementReachAgent.getInstance(context);
		EngagementReachInteractiveContent content = reachAgent.getContent(intent);
		if (content != null)
		{
		  reachAgent.onDownloadTimeout(content);
		}
	  }
	}

}