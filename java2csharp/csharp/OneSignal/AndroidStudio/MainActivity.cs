namespace com.onesignal.example
{

	using Activity = android.app.Activity;
	using AlertDialog = android.support.v7.app.AlertDialog;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Bundle = android.os.Bundle;
	using TextView = android.widget.TextView;

	using NotificationOpenedHandler = com.onesignal.OneSignal.NotificationOpenedHandler;

	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	public class MainActivity : AppCompatActivity
	{

	   private static Activity currentActivity;

	   protected internal override void onCreate(Bundle savedInstanceState)
	   {
		  base.onCreate(savedInstanceState);
		  ContentView = R.layout.activity_main;

		  currentActivity = this;

		  // NOTE: Please update your Google Project number and OneSignal id to yours below.
		  // Pass in your app's Context, Google Project number, your OneSignal App ID, and NotificationOpenedHandler
		  OneSignal.init(this, "703322744261", "b2f7f966-d8cc-11e4-bed1-df8f05be55ba", new ExampleNotificationOpenedHandler(this, this));

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
			  string text = "OneSignal UserID:\n" + userId + "\n\n";

			  if (registrationId != null)
			  {
				 text += "Google Registration Id:\n" + registrationId;
			  }
			  else
			  {
				 text += "Google Registration Id:\nCould not subscribe for push";
			  }

			  TextView textView = (TextView)findViewById(R.id.debug_view);
			  textView.Text = text;
		   }
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
		  /// Callback to implement in your app to handle when a notification is opened from the Android status bar or
		  /// a new one comes in while the app is running.
		  /// This method is located in this activity as an example, you may have any class you wish implement NotificationOpenedHandler and define this method.
		  /// </summary>
		  /// <param name="message">        The message string the user seen/should see in the Android status bar. </param>
		  /// <param name="additionalData"> The additionalData key value pair section you entered in on onesignal.com. </param>
		  /// <param name="isActive">       Was the app in the foreground when the notification was received. </param>
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

			 (new AlertDialog.Builder(MainActivity.currentActivity)).setTitle(messageTitle).setMessage(messageBody).setCancelable(true).setPositiveButton("OK", null).create().show();
		  }
	   }

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

	}

}