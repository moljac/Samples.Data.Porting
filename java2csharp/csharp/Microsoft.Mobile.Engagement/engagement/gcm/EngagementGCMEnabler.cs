using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.gcm
{

	using PendingIntent = android.app.PendingIntent;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	using EngagementNativePushAgent = com.microsoft.azure.engagement.nativepush.EngagementNativePushAgent;
	using EngagementUtils = com.microsoft.azure.engagement.utils.EngagementUtils;

	/// <summary>
	/// This broadcast receiver is required for GCM to work. Add this section in your AndroidManifest.xml
	/// file:
	/// 
	/// <pre>
	/// {@code <receiver android:name="com.microsoft.azure.engagement.gcm.EngagementGCMEnabler" android:exported="false">
	///   <intent-filter>
	///     <action android:name="com.microsoft.azure.engagement.intent.action.APPID_GOT" />
	///   </intent-filter>
	/// </receiver>}
	/// </pre>
	/// 
	/// Additionally and unless you send the <tt>com.google.android.c2dm.intent.REGISTER</tt> yourself,
	/// you must configure the GCM sender like this:
	/// 
	/// <pre>
	/// {@code <meta-data android:name="engagement:gcm:sender" android:value="<projectID\n>" />}
	/// </pre>
	/// 
	/// Warning: the <tt>\n</tt> in the sender value is required, otherwise a parsing problem occurs. <br/>
	/// <br/>
	/// Note that this receiver is mandatory whether you configure it to use the sender for you or not.
	/// You must also integrate <seealso cref="EngagementGCMReceiver"/>. </summary>
	/// <seealso cref= EngagementGCMReceiver </seealso>
	public class EngagementGCMEnabler : BroadcastReceiver
	{
	  public override void onReceive(Context context, Intent intent)
	  {
		/* Once the application identifier is known */
		if ("com.microsoft.azure.engagement.intent.action.APPID_GOT".Equals(intent.Action))
		{
		  /* Init the native push agent */
		  string appId = intent.getStringExtra("appId");
		  EngagementNativePushAgent.getInstance(context).onAppIdGot(appId);

		  /*
		   * Request GCM registration identifier, this is asynchronous, the response is made via a
		   * broadcast intent with the <tt>com.google.android.c2dm.intent.REGISTRATION</tt> action.
		   */
		  string sender = EngagementUtils.getMetaData(context).getString("engagement:gcm:sender");
		  if (sender != null)
		  {
			/* Launch registration process */
			Intent registrationIntent = new Intent("com.google.android.c2dm.intent.REGISTER");
			registrationIntent.Package = "com.google.android.gsf";
			registrationIntent.putExtra("app", PendingIntent.getBroadcast(context, 0, new Intent(), 0));
			registrationIntent.putExtra("sender", sender.Trim());
			try
			{
			  context.startService(registrationIntent);
			}
			catch (Exception)
			{
			  /* Abort if the GCM service can't be accessed. */
			}
		  }
		}
	  }
	}

}