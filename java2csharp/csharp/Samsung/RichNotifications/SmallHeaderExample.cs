using System;
using System.Collections.Generic;

namespace com.samsung.android.richnotification.sample
{


	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Color = android.graphics.Color;
	using Uri = android.net.Uri;
	using Time = android.text.format.Time;

	using SrnAction = com.samsung.android.sdk.richnotification.SrnAction;
	using CallbackIntent = com.samsung.android.sdk.richnotification.SrnAction.CallbackIntent;
	using SrnImageAsset = com.samsung.android.sdk.richnotification.SrnImageAsset;
	using SrnRichNotification = com.samsung.android.sdk.richnotification.SrnRichNotification;
	using AlertType = com.samsung.android.sdk.richnotification.SrnRichNotification.AlertType;
	using SrnHostAction = com.samsung.android.sdk.richnotification.actions.SrnHostAction;
	using SrnRemoteInputAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction;
	using InputModeFactory = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.InputModeFactory;
	using KeyboardInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode;
	using KeyboardType = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode.KeyboardType;
	using SrnPrimaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
	using SrnSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
	using SrnStandardSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;
	using SrnStandardTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
	using HeaderSizeType = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

	public class SmallHeaderExample : IExample
	{

		private readonly Context mContext;

		public SmallHeaderExample(Context ctx)
		{
			mContext = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(mContext);
			noti.setReadout("New Album notification from Songify", "Taylor Swift New Album Red is released");
			noti.PrimaryTemplate = SmallHeaderTemplate;

			noti.SecondaryTemplate = SmallSecondaryTemplate;

			noti.Title = "Songify";
			try
			{
				noti.addActionsWithPermissionCheck(Actions);
			}
			catch (Exception e)
			{
				  Console.WriteLine(e.ToString());
				  Console.Write(e.StackTrace);
			}
			noti.AlertType = SrnRichNotification.AlertType.VIBRATION;

			//        Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.getResources(),
			//                R.drawable.uber_icon);
			//        SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
			//        noti.setIcon(appIcon);

			return noti;
		}

		public virtual SrnPrimaryTemplate SmallHeaderTemplate
		{
			get
			{
				SrnStandardTemplate smallHeaderTemplate = new SrnStandardTemplate(SrnStandardTemplate.HeaderSizeType.SMALL);
    
				smallHeaderTemplate.SubHeader = "<b> New Album Release </b>";
				smallHeaderTemplate.Body = "<b>Taylor Swift</b>'s New Album <b>Red</b> released";
				smallHeaderTemplate.BackgroundColor = Color.rgb(0, 0, 255);
    
				return smallHeaderTemplate;
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
    
				smallSecTemplate.Body = "<b>Tracks in Red</b>: State Of Grace, Red, Treacherous, " + "I Knew You Were Trouble, All Too Well, 22, I Almost Do, " + "We Are Never Ever Getting Back Together, Stay Stay Stay, " + "The Last Time, Holy Ground, Sad Beautiful Tragic, The Lucky One, " + "Everything Has Changed, Starlight, Begin Again, Bonus Tracks, " + "The Moment I Knew, Come Back... Be Here, Girl At Home";
    
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
    
				SrnHostAction primaryAction = new SrnHostAction("Listen On Phone");
				Bitmap listenBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.listen);
				SrnImageAsset listenIcon = new SrnImageAsset(mContext, "web_icon", listenBitmap);
    
				string url = "http://musicmp3.ru/artist_taylor-swift__album_red.html#.U-Cj3WPzkjY";
				Intent resultIntent = new Intent(mContext, typeof(MyCallbackActivity));
				resultIntent.Data = Uri.parse(url);
    
				primaryAction.Icon = listenIcon;
				primaryAction.Toast = "Music will be played on Phone!";
				primaryAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(resultIntent);
				myActions.Add(primaryAction);
    
				SrnHostAction action2 = new SrnHostAction("Watch On Phone");
				string yturl = "http://www.youtube.com/watch?v=Smu1jse33bQ";
				Intent videoIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(yturl));
				videoIntent.Data = Uri.parse(url);
    
				Bitmap watctBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.watch);
				SrnImageAsset watchIcon = new SrnImageAsset(mContext, "web_icon", watctBitmap);
				action2.Icon = watchIcon;
				action2.Toast = "Youtube App will be launched on Phone!";
				action2.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(videoIntent);
				myActions.Add(action2);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode().setPrefillString("@name").setCharacterLimit(140).setKeyboardType(SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL);
    
				keyboardAction.RequestedInputMode = kInputMode;
    
				Intent keyboardIntent = new Intent("com.samsung.android.richnotification.sample.callback_broadcast");
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getBroadcastCallback(keyboardIntent);
    
				myActions.Add(keyboardAction);
    
				return myActions;
			}
		}
	}

}