/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import static android.app.Activity.RESULT_CANCELED;
import static android.app.Activity.RESULT_OK;
import static android.util.Base64.DEFAULT;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.util.Base64;

/**
 * This is an helper broadcast receiver used to process data push. To process datapush, you override
 * {@link #onDataPushBase64Received(Context, String, byte[], String)} if it's a base64 data push
 * (including file uploads), otherwise you override
 * {@link #onDataPushStringReceived(Context, String, String)}.<br/>
 * To use it in your application you need to add the following section in your AndroidManifest.xml
 * file:
 * 
 * <pre>
 * {@code <receiver android:name="<your_sub_class_name>" android:exported="false">
 *   <intent-filter>
 *     <action android:name="com.microsoft.azure.engagement.reach.intent.action.DATA_PUSH"/>
 *   </intent-filter>
 * </receiver>}
 * </pre>
 */
public abstract class EngagementReachDataPushReceiver extends BroadcastReceiver
{
  @Override
  public void onReceive(Context context, Intent intent)
  {
    /* Handle push message */
    String category = intent.getStringExtra("category");
    String body = intent.getStringExtra("body");
    String type = intent.getStringExtra("type");
    if (body == null || type == null)
      return;

    /* If text then use onDataPushStringReceived function */
    Boolean result;
    if (type.equals("text/plain"))
      result = onDataPushStringReceived(context, category, body);

    /* If base64 or binary file then use onDataPushBinaryReceived function */
    else if (type.equals("text/base64"))
      result = onDataPushBase64Received(context, category, Base64.decode(body, DEFAULT), body);

    /* Unknown type */
    else
      return;

    /* Set result if defined */
    if (result == null)
      return;
    int code;
    if (result)
      code = RESULT_OK;
    else
      code = RESULT_CANCELED;
    setResult(code, null, null);
  }

  /**
   * This function is called when a text data push has been received.
   * @param context the context in which the receiver is running.
   * @param category category name you defined on data push form.
   * @param body your content.
   * @return true to acknowledge the content, false to cancel, null to delegate result to another
   *         broadcast receiver, note that the first broadcast receiver that returns a non null
   *         value sets the result for good.
   **/
  protected Boolean onDataPushStringReceived(Context context, String category, String body)
  {
    /* Optional callback */
    return null;
  }

  /**
   * This function is called when a datapush of type base64 has been received.
   * @param context the context in which the receiver is running.
   * @param category category name you defined on data push form.
   * @param decodedBody your base64 content decoded.
   * @param encodedBody your content still encoded in base64.
   * @return true to acknowledge the content, false to cancel, null to delegate result to another
   *         broadcast receiver, note that the first broadcast receiver that returns a non null
   *         value sets the result for good.
   **/
  protected Boolean onDataPushBase64Received(Context context, String category, byte[] decodedBody,
    String encodedBody)
  {
    /* Optional callback */
    return null;
  }

}
