using System;
using System.Collections.Generic;

namespace com.samsung.android.richnotification.sample
{


	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using Time = android.text.format.Time;

	using SrnAction = com.samsung.android.sdk.richnotification.SrnAction;
	using CallbackIntent = com.samsung.android.sdk.richnotification.SrnAction.CallbackIntent;
	using SrnImageAsset = com.samsung.android.sdk.richnotification.SrnImageAsset;
	using SrnRichNotification = com.samsung.android.sdk.richnotification.SrnRichNotification;
	using AlertType = com.samsung.android.sdk.richnotification.SrnRichNotification.AlertType;
	using SrnHostAction = com.samsung.android.sdk.richnotification.actions.SrnHostAction;
	using SrnRemoteBuiltInAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteBuiltInAction;
	using SrnRemoteInputAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction;
	using InputModeFactory = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.InputModeFactory;
	using KeyboardInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode;
	using KeyboardType = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode.KeyboardType;
	using SingleSelectInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
	using SrnLargeHeaderTemplate = com.samsung.android.sdk.richnotification.templates.SrnLargeHeaderTemplate;
	using SrnPrimaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
	using SrnSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
	using SrnStandardSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;

	public class LargeHeaderExample : IExample
	{

		private readonly Context mContext;

		public LargeHeaderExample(Context ctx)
		{
			mContext = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(mContext);
			noti.setReadout("Facebook post", "Your friend post, nice weather in SFO");

			noti.PrimaryTemplate = SrnLargeHeaderTemplate;

			noti.SecondaryTemplate = SmallSecondaryTemplate;
			try
			{
			noti.addActionsWithPermissionCheck(Actions);
			}
			 catch (Exception e)
			 {
			   Console.WriteLine(e.ToString());
			   Console.Write(e.StackTrace);
			 }
			noti.AlertType = SrnRichNotification.AlertType.SILENCE;

			Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.uber_icon);
			SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
			noti.Icon = appIcon;

			noti.Title = "Nice Weather";

			return noti;
		}

		public virtual SrnPrimaryTemplate SrnLargeHeaderTemplate
		{
			get
			{
				SrnLargeHeaderTemplate mLargeHeaderTemplate = new SrnLargeHeaderTemplate();
    
				Bitmap bg = BitmapFactory.decodeResource(mContext.Resources, R.drawable.weather);
				SrnImageAsset ibg = new SrnImageAsset(mContext, "l_h", bg);
    
				mLargeHeaderTemplate.BackgroundImage = ibg;
    
				return mLargeHeaderTemplate;
			}
		}

		public virtual SrnSecondaryTemplate SmallSecondaryTemplate
		{
			get
			{
				SrnStandardSecondaryTemplate smallSecTemplate = new SrnStandardSecondaryTemplate();
    
				smallSecTemplate.Title = "<b>Album Information</b>";
				Time today = new Time(Time.CurrentTimezone);
				today.setToNow();
    
				smallSecTemplate.SubHeader = "<b>Release Date</b>:" + today.year + "/" + today.month + "/" + today.monthDay;
    
				smallSecTemplate.Body = "<b>Tracks in Red</b>: State Of Grace, Red, Treacherous, " + "I Knew You Were Trouble, All Too Well, 22, I Almost Do, " + "We Are Never Ever Getting Back Together, Stay Stay Stay, " + "The Last Time, Holy Ground, Sad Beautiful Tragic, " + "The Lucky One, Everything Has Changed, Starlight, " + "Begin Again, Bonus Tracks, The Moment I Knew, Come Back... " + "Be Here, Girl At Home";
    
				Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.red);
				SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
				smallSecTemplate.Image = qrCodeBig;
    
				Bitmap commentBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.star_subtitle);
				SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
				smallSecTemplate.setSmallIcon1(commentIcon, "4/5");
    
				Bitmap likeBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.like);
				SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
				smallSecTemplate.setSmallIcon2(likeIcon, "999+");
    
				return smallSecTemplate;
			}
		}

		public virtual IList<SrnAction> Actions
		{
			get
			{
				List<SrnAction> myActions = new List<SrnAction>();
    
				SrnHostAction primaryAction = new SrnHostAction("See Forecast");
				Bitmap webActionBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.voice_action);
				SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);
    
				primaryAction.Icon = webActionIcon;
				primaryAction.Toast = "Check your phone!";
    
				Intent resultIntent = new Intent(Intent.ACTION_VIEW);
				resultIntent.Data = Uri.parse("http://www.weather.com");
    
				primaryAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(resultIntent);
    
				myActions.Add(primaryAction);
    
				SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");
    
				builtInAction.Type = SrnRemoteBuiltInAction.OperationType.CALL;
				builtInAction.Data = Uri.fromParts("tel", "+821095307811", null);
    
				myActions.Add(builtInAction);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");
    
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode();
				kInputMode.PrefillString = "@name";
				kInputMode.CharacterLimit = 140;
				kInputMode.KeyboardType = SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL;
    
				keyboardAction.RequestedInputMode = kInputMode;
    
				Intent keyboardIntent = new Intent(mContext, typeof(SrnRichNotification));
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(keyboardIntent);
    
				myActions.Add(keyboardAction);
    
				SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Survey");
    
				SrnRemoteInputAction.SingleSelectInputMode sInputMode = SrnRemoteInputAction.InputModeFactory.createSingleSelectInputMode();
				sInputMode.addChoice("Love This", "color_black");
				sInputMode.addChoice("Hate it", "color_white");
				sInputMode.addChoice("Am Cool", "color_red");
    
				singelSelectAction.RequestedInputMode = sInputMode;
				singelSelectAction.Description = "Is this weather good?";
    
				Intent singleSelectIntent = new Intent(mContext, typeof(SrnRichNotification));
				singelSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(singleSelectIntent);
    
				myActions.Add(singelSelectAction);
    
				return myActions;
			}
		}
	}

}