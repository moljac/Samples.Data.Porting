/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.nativepush
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_ACTION_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementIntents.INTENT_EXTRA_TYPE_PUSH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.microsoft.azure.engagement.EngagementNativePushToken.typeFromInt;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using SharedPreferences = android.content.SharedPreferences;
	using Editor = android.content.SharedPreferences.Editor;
	using Bundle = android.os.Bundle;

	using EngagementADMEnabler = com.microsoft.azure.engagement.adm.EngagementADMEnabler;
	using EngagementADMReceiver = com.microsoft.azure.engagement.adm.EngagementADMReceiver;
	using EngagementGCMEnabler = com.microsoft.azure.engagement.gcm.EngagementGCMEnabler;
	using EngagementGCMReceiver = com.microsoft.azure.engagement.gcm.EngagementGCMReceiver;

	/// <summary>
	/// <para>
	/// This agent manages the life cycle of the registration identifier (called token in Engagement)
	/// used in native push.
	/// </para>
	/// <para>
	/// The token is associated to a Native Push service. A device may only have one token at any time
	/// but you can safely integrate several services: at most 1 service is available at runtime.
	/// </para>
	/// <para>
	/// Engagement currently supports GCM (Google Cloud Messaging) and ADM (Amazon Device Messaging).
	/// Engagement cannot work with GCM if your own application code uses deprecated C2DM: the device can
	/// only have one registration identifier at any time. Please migrate your application to GCM in that
	/// case.
	/// </para>
	/// <para>
	/// You don't need to use this class directly, you can instead integrate the following classes.
	/// </para>
	/// <ul>
	/// <li>For GCM:
	/// <ul>
	/// <li><seealso cref="EngagementGCMEnabler"/></li>
	/// <li><seealso cref="EngagementGCMReceiver"/></li>
	/// </ul>
	/// </li>
	/// <li>For ADM:
	/// <ul>
	/// <li><seealso cref="EngagementADMEnabler"/></li>
	/// <li><seealso cref="EngagementADMReceiver"/></li>
	/// </ul>
	/// </li>
	/// </ul>
	/// </summary>
	public class EngagementNativePushAgent
	{
	  /// <summary>
	  /// Native push payload extra parameter to detect our namespace (value is irrelevant and is likely
	  /// empty string).
	  /// </summary>
	  private const string INTENT_EXTRA_AZME = "azme";

	  /// <summary>
	  /// Storage file name </summary>
	  private const string STORAGE_FILE = "engagement.nativepush";

	  /// <summary>
	  /// Storage key: application identifier for which we sent the token. </summary>
	  private const string APP_ID = "appId";

	  /// <summary>
	  /// Storage key: token value </summary>
	  private const string TOKEN_VALUE = "val";

	  /// <summary>
	  /// Storage key: token type </summary>
	  private const string TOKEN_TYPE = "type";

	  /// <summary>
	  /// Storage key: sent timestamp in ms since epoch </summary>
	  private const string SENT = "sent";

	  /// <summary>
	  /// Storage key: new registration identifier value </summary>
	  private const string NEW_TOKEN_VALUE = "newVal";

	  /// <summary>
	  /// Storage key: new registration identifier type </summary>
	  private const string NEW_TOKEN_TYPE = "newType";

	  /// <summary>
	  /// We re-send the same registration identifier only if some time has elapsed, specified by this
	  /// constant (in ms). Value: 1 day.
	  /// </summary>
	  private const long SENT_EXPIRY = 86400000L;

	  /// <summary>
	  /// Unique instance </summary>
	  private static EngagementNativePushAgent sInstance;

	  /// <summary>
	  /// Get the unique instance. </summary>
	  /// <param name="context"> any valid context. </param>
	  public static EngagementNativePushAgent getInstance(Context context)
	  {
		/* Always check this even if we instantiate once to trigger null pointer in all cases */
		if (sInstance == null)
		{
		  sInstance = new EngagementNativePushAgent(context.ApplicationContext);
		}
		return sInstance;
	  }

	  /// <summary>
	  /// Context </summary>
	  private readonly Context mContext;

	  /// <summary>
	  /// Storage file </summary>
	  private readonly SharedPreferences mStorage;

	  /// <summary>
	  /// Engagement agent </summary>
	  private readonly EngagementAgent mEngagementAgent;

	  /// <summary>
	  /// Application identifier </summary>
	  private string mAppId;

	  /// <summary>
	  /// Init. </summary>
	  /// <param name="context"> application context. </param>
	  private EngagementNativePushAgent(Context context)
	  {
		/* Init */
		mContext = context;
		mEngagementAgent = EngagementAgent.getInstance(context);
		mStorage = context.getSharedPreferences(STORAGE_FILE, 0);
	  }

	  /// <summary>
	  /// Calls <seealso cref="EngagementAgent#registerNativePush(EngagementNativePushToken)"/> only if the token
	  /// is a new one or some time has elapsed since the last time we sent it. </summary>
	  /// <param name="token"> token to register. </param>
	  public virtual void registerNativePush(EngagementNativePushToken token)
	  {
		/* If application identifier is unknown */
		if (mAppId == null)
		{
		  /*
		   * Keep state until onAppIdGot is called. Possibly in the next application launch so it must
		   * be persisted.
		   */
		  SharedPreferences.Editor edit = mStorage.edit();
		  edit.putString(NEW_TOKEN_VALUE, token.Token);
		  edit.putInt(NEW_TOKEN_TYPE, token.getType().ordinal());
		  edit.commit();
		}
		else
		{
		  /* Get state */
		  string oldAppId = mStorage.getString(APP_ID, null);
		  string oldTokenValue = mStorage.getString(TOKEN_VALUE, null);
		  EngagementNativePushToken.Type oldType = EngagementNativePushToken.typeFromInt(mStorage.getInt(TOKEN_TYPE, -1));
		  long sent = mStorage.getLong(SENT, 0);
		  long elapsedSinceSent = DateTimeHelperClass.CurrentUnixTimeMillis() - sent;

		  /* If registrationId changed or enough time elapsed since we sent it */
		  if (oldAppId == null || !oldAppId.Equals(mAppId) || oldTokenValue == null || !oldTokenValue.Equals(token.Token) || oldType == null || !oldType.Equals(token.getType()) || elapsedSinceSent >= SENT_EXPIRY)
		  {
			/* Send registration identifier */
			mEngagementAgent.registerNativePush(token);

			/* Update state */
			SharedPreferences.Editor edit = mStorage.edit();
			edit.clear();
			edit.putString(APP_ID, mAppId);
			edit.putString(TOKEN_VALUE, token.Token);
			edit.putInt(TOKEN_TYPE, token.getType().ordinal());
			edit.putLong(SENT, DateTimeHelperClass.CurrentUnixTimeMillis());
			edit.commit();
		  }
		}
	  }

	  /// <summary>
	  /// Notify when the application identifier is known. </summary>
	  /// <param name="appId"> application identifier. </param>
	  public virtual void onAppIdGot(string appId)
	  {
		/* Keep identifier */
		mAppId = appId;

		/* Send pending token if any */
		string value = mStorage.getString(NEW_TOKEN_VALUE, null);
		EngagementNativePushToken.Type type = typeFromInt(mStorage.getInt(NEW_TOKEN_TYPE, -1));
		if (value != null && type != null)
		{
		  registerNativePush(new EngagementNativePushToken(value, type));
		}
	  }

	  /// <summary>
	  /// Notify when a push is received. </summary>
	  /// <param name="extras"> push parameters. </param>
	  public virtual void onPushReceived(Bundle extras)
	  {
		/* Relay push to listeners if its an azme push */
		if (extras.containsKey(INTENT_EXTRA_AZME))
		{
		  Intent intent = new Intent(INTENT_ACTION_MESSAGE);
		  intent.Package = mContext.PackageName;
		  extras.putString(INTENT_EXTRA_TYPE, INTENT_EXTRA_TYPE_PUSH);
		  intent.putExtras(extras);
		  mContext.sendBroadcast(intent);
		}
	  }
	}

}