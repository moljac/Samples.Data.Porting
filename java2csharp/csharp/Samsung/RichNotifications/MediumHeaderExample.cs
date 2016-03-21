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

	public class MediumHeaderExample : IExample
	{

		private readonly Context mContext;

		public MediumHeaderExample(Context ctx)
		{
			mContext = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(mContext);
			noti.setReadout("New Book notification from Bookify", "Kate Daniels New Album Magic Breaks is released");

			noti.PrimaryTemplate = SrnMediumHeaderTemplate;

			noti.SecondaryTemplate = MediumSecondaryTemplate;
			try
			{
			noti.addActionsWithPermissionCheck(Actions);
			}
			catch (Exception e)
			{
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}
			noti.AlertType = SrnRichNotification.AlertType.SOUND_AND_VIBRATION;

			Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.uber_icon);
			SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
			noti.Icon = appIcon;

			noti.Title = "Bookify";

			return noti;
		}

		public virtual SrnPrimaryTemplate SrnMediumHeaderTemplate
		{
			get
			{
				SrnStandardTemplate mMediumHeaderTemplate = new SrnStandardTemplate(SrnStandardTemplate.HeaderSizeType.MEDIUM);
				mMediumHeaderTemplate.BackgroundColor = Color.rgb(0, 255, 0);
    
				mMediumHeaderTemplate.SubHeader = "<b> New Book Release </b>";
				mMediumHeaderTemplate.Body = "<b>Kate Daniel</b>'s New Book <b>Magic Breaks</b> released";
    
				return mMediumHeaderTemplate;
			}
		}

		public virtual SrnSecondaryTemplate MediumSecondaryTemplate
		{
			get
			{
				SrnStandardSecondaryTemplate mediumSecondaryTemplate = new SrnStandardSecondaryTemplate();
    
				mediumSecondaryTemplate.Title = "<b>Book Information</b>";
				Time today = new Time(Time.CurrentTimezone);
				today.setToNow();
    
				mediumSecondaryTemplate.SubHeader = "<b>Release Date</b>:" + today.year + "/" + today.month + "/" + today.monthDay;
    
				mediumSecondaryTemplate.Body = "<b>Overview</b>: As the mate of the Beast Lord, Curran, " + "former mercenary Kate Daniels has more responsibilities than " + "it seems possible to juggle. Not only is she still struggling " + "to keep her investigative business afloat, she must now deal with " + "the affairs of the pack, including preparing her people for attack " + "from Roland, a cruel ancient being with god-like powers. " + "Since Kate��s connection to Roland has come out into the open, no one is safe";
    
				Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.magic);
				SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
				mediumSecondaryTemplate.Image = qrCodeBig;
    
				Bitmap commentBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.star_subtitle);
				SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
				mediumSecondaryTemplate.setSmallIcon1(commentIcon, "34/5");
    
				Bitmap likeBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.like);
				SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
				mediumSecondaryTemplate.setSmallIcon2(likeIcon, "425");
    
				return mediumSecondaryTemplate;
			}
		}

		public virtual IList<SrnAction> Actions
		{
			get
			{
				List<SrnAction> myActions = new List<SrnAction>();
    
				SrnHostAction primaryAction = new SrnHostAction("Read On Phone");
				Bitmap webActionBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.voice_action);
				SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);
    
				Intent resultIntent = new Intent(mContext, typeof(MyCallbackActivity));
				resultIntent.Data = Uri.parse("http://musicmp3.ru/artist_taylor-swift__album_red.html#.U-Cj3WPzkjY");
    
				primaryAction.Icon = webActionIcon;
				primaryAction.Toast = "Music will be played on Phone!";
				primaryAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(resultIntent);
				myActions.Add(primaryAction);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");
    
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode().setPrefillString("@name").setCharacterLimit(140).setKeyboardType(SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL);
    
				keyboardAction.RequestedInputMode = kInputMode;
    
				Intent keyboardIntent = new Intent(mContext, typeof(RichNotificationActivity));
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(keyboardIntent);
    
				myActions.Add(keyboardAction);
    
				return myActions;
			}
		}
	}

}