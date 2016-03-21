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
	using MultiSelectInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.MultiSelectInputMode;
	using SingleSelectInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
	using SrnPrimaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
	using SrnSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
	using SrnStandardSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;
	using SrnStandardTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
	using HeaderSizeType = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

	public class FullScreenExample : IExample
	{

		private readonly Context context;

		public FullScreenExample(Context ctx)
		{
			context = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(context);
			noti.setReadout("New Full Header", "FullScreen.");

			noti.PrimaryTemplate = FullScreenTemplate;

			noti.SecondaryTemplate = FullSecondaryTemplate;
			try
			{
			noti.addActionsWithPermissionCheck(Actions);
			}
			catch (Exception e)
			{
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}
			noti.AlertType = SrnRichNotification.AlertType.SOUND;

			Bitmap appIconBitmap = BitmapFactory.decodeResource(context.Resources, R.drawable.uber_icon);
			SrnImageAsset appIcon = new SrnImageAsset(context, "app_icon", appIconBitmap);
			noti.Icon = appIcon;

			noti.Title = "Movify";

			return noti;
		}

		public virtual SrnPrimaryTemplate FullScreenTemplate
		{
			get
			{
				SrnStandardTemplate mFullScreenTemplate = new SrnStandardTemplate(SrnStandardTemplate.HeaderSizeType.FULL_SCREEN);
				Bitmap bg = BitmapFactory.decodeResource(context.Resources, R.drawable.hercules);
				SrnImageAsset ibg = new SrnImageAsset(context, "full_h", bg);
    
				mFullScreenTemplate.BackgroundImage = ibg;
    
				mFullScreenTemplate.SubHeader = "<b> New Movie Release </b>";
				mFullScreenTemplate.Body = "<b>Brett Ratner</b>'s New Movie <b>Hercules</b> released";
    
				return mFullScreenTemplate;
			}
		}

		public virtual SrnSecondaryTemplate FullSecondaryTemplate
		{
			get
			{
				SrnStandardSecondaryTemplate fullSecondaryTemplate = new SrnStandardSecondaryTemplate();
    
				fullSecondaryTemplate.Title = "<b>Movie Information</b>";
				Time today = new Time(Time.CurrentTimezone);
				today.setToNow();
    
				fullSecondaryTemplate.SubHeader = "<b>Release Date</b>:" + today.year + "/" + today.month + "/" + today.monthDay;
    
				fullSecondaryTemplate.Body = "<b>Synopsis</b>: Having endured his legendary twelve labors, " + "Hercules, the Greek demigod, has his life as a sword-for-hire " + "tested when the King of Thrace and his daughter seek his aid " + "in defeating a tyrannical warlord.";
    
				Bitmap commentBM = BitmapFactory.decodeResource(context.Resources, R.drawable.star_subtitle);
				SrnImageAsset commentIcon = new SrnImageAsset(context, "comment_icon", commentBM);
				fullSecondaryTemplate.setSmallIcon1(commentIcon, "5/5");
    
				Bitmap likeBM = BitmapFactory.decodeResource(context.Resources, R.drawable.like);
				SrnImageAsset likeIcon = new SrnImageAsset(context, "like_icon", likeBM);
				fullSecondaryTemplate.setSmallIcon2(likeIcon, "999+");
    
				return fullSecondaryTemplate;
    
			}
		}

		public virtual IList<SrnAction> Actions
		{
			get
			{
				List<SrnAction> myActions = new List<SrnAction>();
    
				SrnHostAction primaryAction = new SrnHostAction("View On IMDB");
				Bitmap webActionBM = BitmapFactory.decodeResource(context.Resources, R.drawable.voice_action);
    
				SrnImageAsset webActionIcon = new SrnImageAsset(context, "web_icon", webActionBM);
    
				primaryAction.Icon = webActionIcon;
				primaryAction.Toast = "Music will be played on Phone!";
    
				string url = "http://www.imdb.com/title/tt1267297/";
				Intent resultIntent = new Intent(context, typeof(MyCallbackActivity));
				resultIntent.Data = Uri.parse(url);
				primaryAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(resultIntent);
    
				myActions.Add(primaryAction);
    
				SrnHostAction action2 = new SrnHostAction("Watch Trailier On Phone");
				action2.Icon = webActionIcon;
				action2.Toast = "Youtube App will be launched on Phone!";
    
				string yturl = "http://www.youtube.com/watch?v=GFqY089piQ4";
				Intent videoIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(yturl));
				videoIntent.Data = Uri.parse(url);
    
				action2.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(videoIntent);
    
				myActions.Add(action2);
    
				SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");
    
				builtInAction.Type = SrnRemoteBuiltInAction.OperationType.CALL;
				builtInAction.Data = Uri.fromParts("tel", "+14157364480", null);
    
				myActions.Add(builtInAction);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comments");
    
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode();
				kInputMode.PrefillString = "@name";
				kInputMode.CharacterLimit = 140;
				kInputMode.KeyboardType = SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL;
				keyboardAction.RequestedInputMode = kInputMode;
    
				Intent keyboardIntent = new Intent(context, typeof(RichNotificationActivity));
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(keyboardIntent);
    
				myActions.Add(keyboardAction);
    
				SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Survey");
    
				SrnRemoteInputAction.SingleSelectInputMode sInputMode = SrnRemoteInputAction.InputModeFactory.createSingleSelectInputMode();
				Bitmap bitmap = BitmapFactory.decodeResource(context.Resources, R.drawable.ic_launcher);
    
				sInputMode.addChoice("Yes", "color_black", new SrnImageAsset(context, "black_icon", bitmap));
				sInputMode.addChoice("No", "color_white", new SrnImageAsset(context, "black_icon", bitmap));
				sInputMode.addChoice("I don't remember", "color_red", new SrnImageAsset(context, "black_icon", bitmap));
    
				singelSelectAction.RequestedInputMode = sInputMode;
				singelSelectAction.Description = "Did you watch the movie";
    
				Intent singleSelectIntent = new Intent(context, typeof(RichNotificationActivity));
				singelSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(singleSelectIntent);
    
				myActions.Add(singelSelectAction);
    
				SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
				SrnRemoteInputAction.MultiSelectInputMode mInputMode = SrnRemoteInputAction.InputModeFactory.createMultiSelectInputMode();
				multipeSelectAction.Description = "Select your favorite Actors.";
				mInputMode.addChoice("Dwayne Johnson", "color_black");
				mInputMode.addChoice("Ian McShane", "color_white");
				mInputMode.addChoice("John Hurt", "color_red");
				mInputMode.addChoice("Rufus Sewell", "color_blue");
    
				multipeSelectAction.RequestedInputMode = mInputMode;
    
				Intent multipleSelectIntent = new Intent(context, typeof(RichNotificationActivity));
				multipeSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(multipleSelectIntent);
    
				myActions.Add(multipeSelectAction);
    
				return myActions;
			}
		}

	}

}