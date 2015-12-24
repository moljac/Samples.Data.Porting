/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.adm
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementNativePushToken.Type.ADM;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	using EngagementNativePushAgent = com.microsoft.azure.engagement.nativepush.EngagementNativePushAgent;

	/// <summary>
	/// This class is required to communicate the ADM registration id to the Engagement Push service and
	/// to receive ADM messages. Add this to your AndroidManifest.xml:
	/// 
	/// <pre>
	/// {@code
	/// <receiver android:name="com.microsoft.azure.engagement.gcm.EngagementADMReceiver" android:permission="com.google.android.c2dm.permission.SEND">
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
	/// <uses-permission android:name="android.permission.WAKE_LOCK"/>
	/// <uses-permission android:name="com.amazon.device.messaging.permission.RECEIVE" />
	/// <uses-permission android:name="<your_package_name>.permission.RECEIVE_ADM_MESSAGE" />
	/// <permission android:name="<your_package_name>.permission.RECEIVE_ADM_MESSAGE" android:protectionLevel="signature" />
	/// }
	/// </pre>
	/// 
	/// You also have to integrate <seealso cref="EngagementADMEnabler"/> if you don't already manage the ADM
	/// integration yourself. </summary>
	/// <seealso cref= EngagementADMEnabler </seealso>
	public class EngagementADMReceiver : BroadcastReceiver
	{
	  /// <summary>
	  /// Action when we receive token </summary>
	  private const string INTENT_ACTION_REGISTRATION = "com.amazon.device.messaging.intent.REGISTRATION";

	  /// <summary>
	  /// Token key in intent result </summary>
	  private const string INTENT_EXTRA_REGISTRATION = "registration_id";

	  /// <summary>
	  /// Action when we receive a push </summary>
	  public const string INTENT_ACTION_RECEIVE = "com.amazon.device.messaging.intent.RECEIVE";

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
			EngagementNativePushAgent nativePushAgent = EngagementNativePushAgent.getInstance(context);
			nativePushAgent.registerNativePush(new EngagementNativePushToken(registrationId, ADM));
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