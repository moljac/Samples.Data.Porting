/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.gcm
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementNativePushToken.Type.GCM;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	using EngagementNativePushAgent = com.microsoft.azure.engagement.nativepush.EngagementNativePushAgent;

	/// <summary>
	/// This class is required to communicate the GCM registration id to the Engagement Push service and
	/// to receive GCM messages. Add this to your AndroidManifest.xml:
	/// 
	/// <pre>
	/// {@code
	/// <receiver android:name="com.microsoft.azure.engagement.gcm.EngagementGCMReceiver" android:permission="com.google.android.c2dm.permission.SEND">
	///   <intent-filter>
	///     <action android:name="com.google.android.c2dm.intent.REGISTRATION" />
	///     <action android:name="com.google.android.c2dm.intent.RECEIVE" />
	///     <category android:name="<your_package_name>" />
	///   </intent-filter>
	/// </receiver>
	/// }
	/// </pre>
	/// 
	/// Please ensure you have the following permissions:
	/// 
	/// <pre>
	/// {@code
	/// <uses-permission android:name="com.google.android.c2dm.permission.RECEIVE" />
	/// <uses-permission android:name="<your_package_name>.permission.C2D_MESSAGE" />
	/// <permission android:name="<your_package_name>.permission.C2D_MESSAGE" android:protectionLevel="signature" />
	/// }
	/// </pre>
	/// 
	/// You also have to integrate <seealso cref="EngagementGCMEnabler"/> if you don't already manage the
	/// registration intent yourself. </summary>
	/// <seealso cref= EngagementGCMEnabler </seealso>
	public class EngagementGCMReceiver : BroadcastReceiver
	{
	  /// <summary>
	  /// Action when we receive token </summary>
	  private const string INTENT_ACTION_REGISTRATION = "com.google.android.c2dm.intent.REGISTRATION";

	  /// <summary>
	  /// Token key in intent result </summary>
	  private const string INTENT_EXTRA_REGISTRATION = "registration_id";

	  /// <summary>
	  /// Action when we receive a push </summary>
	  public const string INTENT_ACTION_RECEIVE = "com.google.android.c2dm.intent.RECEIVE";

	  public override void onReceive(Context context, Intent intent)
	  {
		/* Registration result action */
		string action = intent.Action;
		if (INTENT_ACTION_REGISTRATION.Equals(action))
		{
		  /* Handle register if successful (otherwise we'll retry next time process is started) */
		  string registrationId = intent.getStringExtra(INTENT_EXTRA_REGISTRATION);
		  if (registrationId != null)
		  {
			/* Send registration id to the Engagement Push service */
			EngagementNativePushAgent nativePushAgent = EngagementNativePushAgent.getInstance(context);
			nativePushAgent.registerNativePush(new EngagementNativePushToken(registrationId, GCM));
		  }
		}

		/* Received message action */
		else if (INTENT_ACTION_RECEIVE.Equals(action))
		{
		  EngagementNativePushAgent.getInstance(context).onPushReceived(intent.Extras);
		}
	  }
	}

}