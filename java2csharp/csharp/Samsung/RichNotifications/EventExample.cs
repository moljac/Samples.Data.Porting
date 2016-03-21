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

	using SrnAction = com.samsung.android.sdk.richnotification.SrnAction;
	using CallbackIntent = com.samsung.android.sdk.richnotification.SrnAction.CallbackIntent;
	using SrnImageAsset = com.samsung.android.sdk.richnotification.SrnImageAsset;
	using SrnRichNotification = com.samsung.android.sdk.richnotification.SrnRichNotification;
	using AlertType = com.samsung.android.sdk.richnotification.SrnRichNotification.AlertType;
	using PopupType = com.samsung.android.sdk.richnotification.SrnRichNotification.PopupType;
	using SrnHostAction = com.samsung.android.sdk.richnotification.actions.SrnHostAction;
	using SrnRemoteBuiltInAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteBuiltInAction;
	using SrnRemoteInputAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction;
	using InputModeFactory = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.InputModeFactory;
	using KeyboardInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode;
	using KeyboardType = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode.KeyboardType;
	using MultiSelectInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.MultiSelectInputMode;
	using SingleSelectInputMode = com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
	using SrnRemoteLaunchAction = com.samsung.android.sdk.richnotification.actions.SrnRemoteLaunchAction;
	using SrnPrimaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
	using SrnQRSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnQRSecondaryTemplate;
	using SrnSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
	using SrnStandardTemplate = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
	using HeaderSizeType = com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

	public class EventExample : IExample
	{

		private readonly Context mContext;

		public EventExample(Context ctx)
		{
			mContext = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(mContext);

			noti.setReadout("New Event", "Today there is an event.");

			Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.uber_icon);
			SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
			noti.Icon = appIcon;

			noti.Title = "Eventify";

			noti.PrimaryTemplate = EventTemplate;

			noti.SecondaryTemplate = EventSecondaryTemplate;
			try
			{
			noti.addActionsWithPermissionCheck(Actions);
			}
			catch (Exception e)
			{
			  Console.WriteLine(e.ToString());
			  Console.Write(e.StackTrace);
			}
			noti.setAlertType(SrnRichNotification.AlertType.SOUND_AND_VIBRATION, SrnRichNotification.PopupType.NORMAL);

			return noti;
		}

		public virtual SrnPrimaryTemplate EventTemplate
		{
			get
			{
				SrnStandardTemplate mMediumHeaderTemplate = new SrnStandardTemplate(SrnStandardTemplate.HeaderSizeType.MEDIUM);
				mMediumHeaderTemplate.BackgroundColor = Color.rgb(0, 255, 0);
    
				mMediumHeaderTemplate.SubHeader = "<b> Scheduled Event </b>";
				mMediumHeaderTemplate.Body = "Scheduled Meeting 10mins from now";
    
				return mMediumHeaderTemplate;
			}
		}

		public virtual SrnSecondaryTemplate EventSecondaryTemplate
		{
			get
			{
				SrnQRSecondaryTemplate qrSecTemplate = new SrnQRSecondaryTemplate();
    
				Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.map);
				SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
				qrSecTemplate.Image = qrCodeBig;
    
				qrSecTemplate.addListItem("Attendee", "Chitra Sampath Kumar");
				qrSecTemplate.addListItem("Attendee", "Taehee Lee");
				qrSecTemplate.addListItem("Attendee", "Hunje Yun");
				qrSecTemplate.addListItem("Attendee", "Minsuk Choi");
				qrSecTemplate.addListItem("Attendee", "Jihwa Park");
				qrSecTemplate.addListItem("Attendee", "Junho Lee");
    
				Bitmap commentBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.like);
				SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
				qrSecTemplate.setSmallIcon1(commentIcon, "99999999+");
    
				Bitmap likeBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.unlike);
				SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
				qrSecTemplate.setSmallIcon2(likeIcon, "99999999+");
    
				return qrSecTemplate;
			}
		}

		public virtual IList<SrnAction> Actions
		{
			get
			{
				List<SrnAction> myActions = new List<SrnAction>();
    
				SrnHostAction primaryAction = new SrnHostAction("Launch Web");
				Bitmap webActionBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.voice_action);
				SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);
    
				string url = "http://www.samsung.com";
				Intent resultIntent = new Intent(Intent.ACTION_VIEW);
				resultIntent.Data = Uri.parse(url);
    
				primaryAction.Icon = webActionIcon;
				primaryAction.Toast = "Check your phone!";
    
				primaryAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(resultIntent);
				myActions.Add(primaryAction);
    
				SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");
    
				builtInAction.Type = SrnRemoteBuiltInAction.OperationType.CALL;
				builtInAction.Data = Uri.fromParts("tel", "+14157364480", null);
				myActions.Add(builtInAction);
    
				SrnRemoteLaunchAction remoteLaunchAction = new SrnRemoteLaunchAction("Launch Dialer");
				remoteLaunchAction.Package = "com.samsung.dialer";
    
				Bitmap dialerBM = BitmapFactory.decodeResource(mContext.Resources, R.drawable.call);
				SrnImageAsset dialerIcon = new SrnImageAsset(mContext, "dialer_icon", dialerBM);
				remoteLaunchAction.Icon = dialerIcon;
				Intent remoteLaunchIntentResult = new Intent(mContext, typeof(RichNotificationActivity));
				remoteLaunchAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(remoteLaunchIntentResult);
				myActions.Add(remoteLaunchAction);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Keyboard");
				Intent keyboardIntent = new Intent("com.samsung.android.richnotification.sample.callback_broadcast");
    
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode();
				kInputMode.PrefillString = "@name";
				kInputMode.CharacterLimit = 140;
				kInputMode.KeyboardType = SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL;
				keyboardAction.RequestedInputMode = kInputMode;
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getBroadcastCallback(keyboardIntent);
				myActions.Add(keyboardAction);
    
				SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Single Select");
				SrnRemoteInputAction.SingleSelectInputMode sInputMode = SrnRemoteInputAction.InputModeFactory.createSingleSelectInputMode();
				Intent singleSelectIntent = new Intent(mContext, typeof(RichNotificationActivity));
				Bitmap bitmap = BitmapFactory.decodeResource(mContext.Resources, R.drawable.ic_launcher);
    
				sInputMode.addChoice("Black", "color_black", new SrnImageAsset(mContext, "black_icon", bitmap));
				sInputMode.addChoice("White", "color_white", new SrnImageAsset(mContext, "white_icon", bitmap));
				sInputMode.addChoice("Red", "color_red", new SrnImageAsset(mContext, "white_icon", bitmap));
				singelSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(singleSelectIntent);
				singelSelectAction.RequestedInputMode = sInputMode;
				singelSelectAction.Description = "Select your favorite color.";
				myActions.Add(singelSelectAction);
    
				SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
				Intent multipleSelectIntent = new Intent(mContext, typeof(RichNotificationActivity));
				SrnRemoteInputAction.MultiSelectInputMode mInputMode = SrnRemoteInputAction.InputModeFactory.createMultiSelectInputMode();
    
				multipeSelectAction.Description = "Select your favorite colors.";
				mInputMode.addChoice("Black", "color_black");
				mInputMode.addChoice("White", "color_white");
				mInputMode.addChoice("Red", "color_red");
				mInputMode.addChoice("Blue", "color_blue");
				multipeSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(multipleSelectIntent);
				multipeSelectAction.RequestedInputMode = mInputMode;
				myActions.Add(multipeSelectAction);
    
				return myActions;
			}
		}
	}

}