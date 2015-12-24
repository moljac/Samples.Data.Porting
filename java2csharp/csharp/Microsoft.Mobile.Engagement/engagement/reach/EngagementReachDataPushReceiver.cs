/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.Activity.RESULT_CANCELED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.app.Activity.RESULT_OK;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.util.Base64.DEFAULT;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Base64 = android.util.Base64;

	/// <summary>
	/// This is an helper broadcast receiver used to process data push. To process datapush, you override
	/// <seealso cref="#onDataPushBase64Received(Context, String, byte[], String)"/> if it's a base64 data push
	/// (including file uploads), otherwise you override
	/// <seealso cref="#onDataPushStringReceived(Context, String, String)"/>.<br/>
	/// To use it in your application you need to add the following section in your AndroidManifest.xml
	/// file:
	/// 
	/// <pre>
	/// {@code <receiver android:name="<your_sub_class_name>" android:exported="false">
	///   <intent-filter>
	///     <action android:name="com.microsoft.azure.engagement.reach.intent.action.DATA_PUSH"/>
	///   </intent-filter>
	/// </receiver>}
	/// </pre>
	/// </summary>
	public abstract class EngagementReachDataPushReceiver : BroadcastReceiver
	{
	  public override void onReceive(Context context, Intent intent)
	  {
		/* Handle push message */
		string category = intent.getStringExtra("category");
		string body = intent.getStringExtra("body");
		string type = intent.getStringExtra("type");
		if (body == null || type == null)
		{
		  return;
		}

		/* If text then use onDataPushStringReceived function */
		bool? result;
		if (type.Equals("text/plain"))
		{
		  result = onDataPushStringReceived(context, category, body);
		}

		/* If base64 or binary file then use onDataPushBinaryReceived function */
		else if (type.Equals("text/base64"))
		{
		  result = onDataPushBase64Received(context, category, Base64.decode(body, DEFAULT), body);
		}

		/* Unknown type */
		else
		{
		  return;
		}

		/* Set result if defined */
		if (result == null)
		{
		  return;
		}
		int code;
		if (result.Value)
		{
		  code = RESULT_OK;
		}
		else
		{
		  code = RESULT_CANCELED;
		}
		setResult(code, null, null);
	  }

	  /// <summary>
	  /// This function is called when a text data push has been received. </summary>
	  /// <param name="context"> the context in which the receiver is running. </param>
	  /// <param name="category"> category name you defined on data push form. </param>
	  /// <param name="body"> your content. </param>
	  /// <returns> true to acknowledge the content, false to cancel, null to delegate result to another
	  ///         broadcast receiver, note that the first broadcast receiver that returns a non null
	  ///         value sets the result for good.
	  ///  </returns>
	  protected internal virtual bool? onDataPushStringReceived(Context context, string category, string body)
	  {
		/* Optional callback */
		return null;
	  }

	  /// <summary>
	  /// This function is called when a datapush of type base64 has been received. </summary>
	  /// <param name="context"> the context in which the receiver is running. </param>
	  /// <param name="category"> category name you defined on data push form. </param>
	  /// <param name="decodedBody"> your base64 content decoded. </param>
	  /// <param name="encodedBody"> your content still encoded in base64. </param>
	  /// <returns> true to acknowledge the content, false to cancel, null to delegate result to another
	  ///         broadcast receiver, note that the first broadcast receiver that returns a non null
	  ///         value sets the result for good.
	  ///  </returns>
	  protected internal virtual bool? onDataPushBase64Received(Context context, string category, sbyte[] decodedBody, string encodedBody)
	  {
		/* Optional callback */
		return null;
	  }

	}

}