using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.adm
{

	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;

	using EngagementNativePushAgent = com.microsoft.azure.engagement.nativepush.EngagementNativePushAgent;
	using EngagementUtils = com.microsoft.azure.engagement.utils.EngagementUtils;

	/// <summary>
	/// This broadcast receiver is required for ADM to work. Add this section in your AndroidManifest.xml
	/// file:
	/// 
	/// <pre>
	/// {@code <receiver android:name="com.microsoft.azure.engagement.adm.EngagementADMEnabler"
	///   android:exported="false">
	///   <intent-filter>
	///     <action android:name="com.microsoft.azure.engagement.intent.action.APPID_GOT" />
	///   </intent-filter>
	/// </receiver>}
	/// </pre>
	/// 
	/// Additionally and unless you manage ADM initialization yourself, you must configure ADM like this:
	/// 
	/// <pre>
	/// {@code <meta-data android:name="engagement:adm:register" android:value="true" />}
	/// </pre>
	/// 
	/// If not already done, configure ADM as being optional or required in your application (inside
	/// application tag).
	/// 
	/// <pre>
	/// {@code <amazon:enable-feature android:name="com.amazon.device.messaging" android:required="false" />}
	/// </pre>
	/// 
	/// For this to work, you need to add the following attribute to the root manifest tag:
	/// 
	/// <pre>
	/// {@code xmlns:amazon="http://schemas.amazon.com/apk/res/android"}
	/// </pre>
	/// 
	/// If not already done, you also need to store your ADM API Key as an Asset: create a file named
	/// <tt>api_key.text</tt> in the assets folder of your Android project and put the API Key there,
	/// without any whitespace. API Key is not needed in release if you let Amazon sign your application.
	/// <para>
	/// Note that this receiver is mandatory whether you configure it to call {@code ADM.startRegister}
	/// or not.
	/// </para>
	/// <para>
	/// Engagement does not need the ADM lib to be in build path, it uses reflection to gracefully
	/// degrade if ADM is unavailable.
	/// </para>
	/// <para>
	/// You must also integrate <seealso cref="EngagementADMReceiver"/>.
	/// </para> </summary>
	/// <seealso cref= EngagementADMReceiver </seealso>
	public class EngagementADMEnabler : BroadcastReceiver
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
		   * Request ADM registration identifier if enabled, this is asynchronous, the response is made
		   * via a broadcast intent with the <tt>com.amazon.device.messaging.intent.REGISTRATION</tt>
		   * action.
		   */
		  if (EngagementUtils.getMetaData(context).getBoolean("engagement:adm:register"))
		  {
			try
			{
			  Type admClass = Type.GetType("com.amazon.device.messaging.ADM");
			  object adm = admClass.GetConstructor(typeof(Context)).newInstance(context);
			  admClass.GetMethod("startRegister").invoke(adm);
			}
			catch (Exception)
			{
			  /* Abort if ADM not available */
			}
		  }
		}
	  }
	}

}