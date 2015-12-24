using System;
using System.Threading;

/// <summary>
/// Modified MIT License
/// 
/// Copyright 2015 OneSignal
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// 1. The above copyright notice and this permission notice shall be included in
/// all copies or substantial portions of the Software.
/// 
/// 2. All copies of substantial portions of the Software may only be used in connection
/// with services provided by OneSignal.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
/// THE SOFTWARE.
/// </summary>

namespace com.onesignal.example
{

	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using TextView = android.widget.TextView;

	using NotificationOpenedHandler = com.onesignal.OneSignal.NotificationOpenedHandler;
	using IdsAvailableHandler = com.onesignal.OneSignal.IdsAvailableHandler;

	public class MainActivity : Activity
	{

	   internal static Activity currentActivity;

	   protected internal override void onCreate(Bundle savedInstanceState)
	   {
		  base.onCreate(savedInstanceState);
		  ContentView = R.layout.activity_main;
		  currentActivity = this;

		  // Enable Logging below to debug issues. (LogCat level, Visual level);
		  // OneSignal.setLogLevel(OneSignal.LOG_LEVEL.DEBUG, OneSignal.LOG_LEVEL.DEBUG);

		  // Pass in your app's Context, Google Project number, OneSignal App ID, and a NotificationOpenedHandler
		  OneSignal.init(this, "703322744261", "5eb5a37e-b458-11e3-ac11-000c2940e62c", new ExampleNotificationOpenedHandler(this, this));
	   }

	   // activity_main.xml defines the link to this method from the button.
	   public virtual void sendTag(View view)
	   {
		  OneSignal.sendTag("key", "valueAndroid");
	   }

	   // activity_main.xml defines the link to this method from the button.
	   public virtual void getIds(View view)
	   {
		  OneSignal.idsAvailable(new IdsAvailableHandlerAnonymousInnerClassHelper(this));
	   }

	   private class IdsAvailableHandlerAnonymousInnerClassHelper : OneSignal.IdsAvailableHandler
	   {
		   private readonly MainActivity outerInstance;

		   public IdsAvailableHandlerAnonymousInnerClassHelper(MainActivity outerInstance)
		   {
			   this.outerInstance = outerInstance;
		   }

		   public override void idsAvailable(string userId, string registrationId)
		   {
			  TextView textView = (TextView) findViewById(R.id.textViewIds);
			  string labelStr = "UserId: " + userId + "\n\nRegistrationId: ";
			  if (registrationId != null)
			  {
				 labelStr += registrationId;
			  }
			  else
			  {
				 labelStr += "Did not register. See Android LogCat for errors.";
			  }
			  textView.Text = labelStr;
			  Log.i("OneSignalExample", labelStr + "\n");
		   }
	   }

	   // onPause and onResume hooks are required so OneSignal knows when to create a notification and when to just call your callback and playtime for segmentation.
	   // To save on adding these hooks to every Activity in your app it might be worth extending android.app.Activity to include this logic if you have many Activity classes in your App.
	   // Anther option is to use this library https://github.com/BoD/android-activitylifecyclecallbacks-compat
	   protected internal override void onPause()
	   {
		  base.onPause();
		  OneSignal.onPaused();
	   }

	   protected internal override void onResume()
	   {
		  base.onResume();
		  OneSignal.onResumed();
	   }

	   // NotificationOpenedHandler is implemented in its own class instead of adding implements to MainActivity so we don't hold on to a reference of our first activity if it gets recreated.
	   private class ExampleNotificationOpenedHandler : OneSignal.NotificationOpenedHandler
	   {
		   private readonly MainActivity outerInstance;

		   public ExampleNotificationOpenedHandler(MainActivity outerInstance)
		   {
			   this.outerInstance = outerInstance;
		   }

		  /// <summary>
		  /// Called when a notification is opened from the Android status bar or a new one comes in while the app is in focus.
		  /// </summary>
		  /// <param name="message">
		  ///           The message string the user seen/should see in the Android status bar. </param>
		  /// <param name="additionalData">
		  ///           The additionalData key value pair section you entered in on onesignal.com. </param>
		  /// <param name="isActive">
		  ///           Was the app in the foreground when the notification was received. </param>
		  public override void notificationOpened(string message, JSONObject additionalData, bool isActive)
		  {
			 string messageTitle = "OneSignal Example", messageBody = message;

			 try
			 {
				if (additionalData != null)
				{
				   if (additionalData.has("title"))
				   {
					  messageTitle = additionalData.getString("title");
				   }
				   if (additionalData.has("actionSelected"))
				   {
					  messageBody += "\nPressed ButtonID: " + additionalData.getString("actionSelected");
				   }

				   messageBody = message + "\n\nFull additionalData:\n" + additionalData.ToString();
				}
			 }
			 catch (JSONException)
			 {
			 }

			 SafeAlertDialog(messageTitle, messageBody);
		  }
	   }

	   // AlertDialogs do not show on Android 2.3 if they are trying to be displayed while the activity is pause.
	   // We sleep for 500ms to wait for the activity to be ready before displaying.
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private static void SafeAlertDialog(final String msgTitle, final String msgBody)
	   private static void SafeAlertDialog(string msgTitle, string msgBody)
	   {
		  new Thread(() =>
		  {
				try
				{
					Thread.Sleep(500);
				}
				catch (Exception)
				{
				}

			MainActivity.currentActivity.runOnUiThread(() =>
			{
			  (new AlertDialog.Builder(MainActivity.currentActivity)).setTitle(msgTitle).setMessage(msgBody).setCancelable(true).setPositiveButton("OK", null).create().show();
			});
		  }).start();
	   }
	}
}