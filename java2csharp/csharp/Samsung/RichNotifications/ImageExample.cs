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
	using SrnQRTemplate = com.samsung.android.sdk.richnotification.templates.SrnQRTemplate;
	using SrnSecondaryTemplate = com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;

	public class ImageExample : IExample
	{

		private readonly Context context;

		public ImageExample(Context ctx)
		{
			context = ctx;
		}

		public virtual SrnRichNotification createRichNoti()
		{
			SrnRichNotification noti = new SrnRichNotification(context);
			noti.setReadout("New QR Code", "You have a new QR code.");

			noti.PrimaryTemplate = ImageTemplate;

			noti.SecondaryTemplate = ImageSecondaryTemplate;
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

			Bitmap appIconBitmap = BitmapFactory.decodeResource(context.Resources, R.drawable.uber_icon);
			SrnImageAsset appIcon = new SrnImageAsset(context, "app_icon", appIconBitmap);
			noti.Icon = appIcon;

			return noti;
		}

		public virtual SrnPrimaryTemplate ImageTemplate
		{
			get
			{
				SrnQRTemplate mImageTemplate = new SrnQRTemplate();
				mImageTemplate.BackgroundColor = Color.rgb(0, 255, 0);
    
				mImageTemplate.SubHeader = "<b> Beautiful </b>";
    
				Bitmap imageBitmap = BitmapFactory.decodeResource(context.Resources, R.drawable.ash);
				SrnImageAsset imageAsset = new SrnImageAsset(context, "large_image", imageBitmap);
				mImageTemplate.Image = imageAsset;
    
				return mImageTemplate;
			}
		}

		public virtual SrnSecondaryTemplate ImageSecondaryTemplate
		{
			get
			{
				SrnQRSecondaryTemplate qrSecTemplate = new SrnQRSecondaryTemplate();
    
				Bitmap qrCodeBitmap = BitmapFactory.decodeResource(context.Resources, R.drawable.ash2);
				SrnImageAsset qrCodeBig = new SrnImageAsset(context, "qr_code_big", qrCodeBitmap);
				qrSecTemplate.Image = qrCodeBig;
    
				qrSecTemplate.addListItem("Profession", "Actress, Model");
				qrSecTemplate.addListItem("Star", "Scorpio");
				qrSecTemplate.addListItem("Height", "5.7");
				qrSecTemplate.addListItem("Nationality", "Indian");
    
				Bitmap commentBM = BitmapFactory.decodeResource(context.Resources, R.drawable.star_subtitle);
				SrnImageAsset commentIcon = new SrnImageAsset(context, "comment_icon", commentBM);
				qrSecTemplate.setSmallIcon1(commentIcon, "99999999+");
    
				Bitmap likeBM = BitmapFactory.decodeResource(context.Resources, R.drawable.like);
				SrnImageAsset likeIcon = new SrnImageAsset(context, "like_icon", likeBM);
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
				Bitmap webActionBM = BitmapFactory.decodeResource(context.Resources, R.drawable.voice_action);
				SrnImageAsset webActionIcon = new SrnImageAsset(context, "web_icon", webActionBM);
    
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
				Bitmap dialerBM = BitmapFactory.decodeResource(context.Resources, R.drawable.call);
				SrnImageAsset dialerIcon = new SrnImageAsset(context, "dialer_icon", dialerBM);
				remoteLaunchAction.Icon = dialerIcon;
				Intent remoteLaunchIntentResult = new Intent(context, typeof(RichNotificationActivity));
				remoteLaunchAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(remoteLaunchIntentResult);
				myActions.Add(remoteLaunchAction);
    
				SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Keyboard");
				Intent keyboardIntent = new Intent(context, typeof(RichNotificationActivity));
    
				SrnRemoteInputAction.KeyboardInputMode kInputMode = SrnRemoteInputAction.InputModeFactory.createKeyboardInputMode();
				kInputMode.PrefillString = "@name";
				kInputMode.CharacterLimit = 140;
				kInputMode.KeyboardType = SrnRemoteInputAction.KeyboardInputMode.KeyboardType.NORMAL;
				keyboardAction.RequestedInputMode = kInputMode;
				keyboardAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(keyboardIntent);
				myActions.Add(keyboardAction);
    
				SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Single Select");
				SrnRemoteInputAction.SingleSelectInputMode sInputMode = SrnRemoteInputAction.InputModeFactory.createSingleSelectInputMode();
				Intent singleSelectIntent = new Intent(context, typeof(RichNotificationActivity));
    
				sInputMode.addChoice("Black", "color_black");
				sInputMode.addChoice("White", "color_white");
				sInputMode.addChoice("Red", "color_red");
				singelSelectAction.CallbackIntent = SrnAction.CallbackIntent.getActivityCallback(singleSelectIntent);
    
				singelSelectAction.RequestedInputMode = sInputMode;
				singelSelectAction.Description = "Select your favorite color.";
				myActions.Add(singelSelectAction);
    
				SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
				Intent multipleSelectIntent = new Intent(context, typeof(RichNotificationActivity));
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