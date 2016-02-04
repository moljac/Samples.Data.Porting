
package com.samsung.android.richnotification.sample;

import java.util.ArrayList;
import java.util.List;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.net.Uri;

import com.samsung.android.sdk.richnotification.SrnAction;
import com.samsung.android.sdk.richnotification.SrnAction.CallbackIntent;
import com.samsung.android.sdk.richnotification.SrnImageAsset;
import com.samsung.android.sdk.richnotification.SrnRichNotification;
import com.samsung.android.sdk.richnotification.SrnRichNotification.AlertType;
import com.samsung.android.sdk.richnotification.actions.SrnHostAction;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteBuiltInAction;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.InputModeFactory;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode.KeyboardType;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.MultiSelectInputMode;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteLaunchAction;
import com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnQRSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnQRTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;

public class ImageExample implements IExample {

    private final Context context;

    public ImageExample(Context ctx) {
        context = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(context);
        noti.setReadout("New QR Code", "You have a new QR code.");

        noti.setPrimaryTemplate(getImageTemplate());

        noti.setSecondaryTemplate(getImageSecondaryTemplate());
        try{
        noti.addActionsWithPermissionCheck(getActions());
        }catch(Exception e)
        { 
            e.printStackTrace();  
         }
        noti.setAlertType(AlertType.VIBRATION);

        Bitmap appIconBitmap = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.uber_icon);
        SrnImageAsset appIcon = new SrnImageAsset(context, "app_icon", appIconBitmap);
        noti.setIcon(appIcon);

        return noti;
    }

    public SrnPrimaryTemplate getImageTemplate() {
        SrnQRTemplate mImageTemplate = new SrnQRTemplate();
        mImageTemplate.setBackgroundColor(Color.rgb(0, 255, 0));

        mImageTemplate.setSubHeader("<b> Beautiful </b>");

        Bitmap imageBitmap = BitmapFactory.decodeResource(context.getResources(), R.drawable.ash);
        SrnImageAsset imageAsset = new SrnImageAsset(context, "large_image", imageBitmap);
        mImageTemplate.setImage(imageAsset);

        return mImageTemplate;
    }

    public SrnSecondaryTemplate getImageSecondaryTemplate() {
        SrnQRSecondaryTemplate qrSecTemplate = new SrnQRSecondaryTemplate();

        Bitmap qrCodeBitmap = BitmapFactory.decodeResource(context.getResources(), R.drawable.ash2);
        SrnImageAsset qrCodeBig = new SrnImageAsset(context, "qr_code_big", qrCodeBitmap);
        qrSecTemplate.setImage(qrCodeBig);

        qrSecTemplate.addListItem("Profession", "Actress, Model");
        qrSecTemplate.addListItem("Star", "Scorpio");
        qrSecTemplate.addListItem("Height", "5.7");
        qrSecTemplate.addListItem("Nationality", "Indian");

        Bitmap commentBM = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.star_subtitle);
        SrnImageAsset commentIcon = new SrnImageAsset(context, "comment_icon", commentBM);
        qrSecTemplate.setSmallIcon1(commentIcon, "99999999+");

        Bitmap likeBM = BitmapFactory.decodeResource(context.getResources(), R.drawable.like);
        SrnImageAsset likeIcon = new SrnImageAsset(context, "like_icon", likeBM);
        qrSecTemplate.setSmallIcon2(likeIcon, "99999999+");

        return qrSecTemplate;
    }

    public List<SrnAction> getActions() {
        ArrayList<SrnAction> myActions = new ArrayList<SrnAction>();

        SrnHostAction primaryAction = new SrnHostAction("Launch Web");
        Bitmap webActionBM = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.voice_action);
        SrnImageAsset webActionIcon = new SrnImageAsset(context, "web_icon", webActionBM);

        String url = "http://www.samsung.com";
        Intent resultIntent = new Intent(Intent.ACTION_VIEW);
        resultIntent.setData(Uri.parse(url));

        primaryAction.setIcon(webActionIcon);
        primaryAction.setToast("Check your phone!");

        primaryAction.setCallbackIntent(CallbackIntent.getActivityCallback(resultIntent));
        myActions.add(primaryAction);

        SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");

        builtInAction.setType(SrnRemoteBuiltInAction.OperationType.CALL);
        builtInAction.setData(Uri.fromParts("tel", "+14157364480", null));
        myActions.add(builtInAction);

        SrnRemoteLaunchAction remoteLaunchAction = new SrnRemoteLaunchAction("Launch Dialer");
        remoteLaunchAction.setPackage("com.samsung.dialer");
        Bitmap dialerBM = BitmapFactory.decodeResource(context.getResources(), R.drawable.call);
        SrnImageAsset dialerIcon = new SrnImageAsset(context, "dialer_icon", dialerBM);
        remoteLaunchAction.setIcon(dialerIcon);
        Intent remoteLaunchIntentResult = new Intent(context, RichNotificationActivity.class);
        remoteLaunchAction.setCallbackIntent(CallbackIntent
                .getActivityCallback(remoteLaunchIntentResult));
        myActions.add(remoteLaunchAction);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Keyboard");
        Intent keyboardIntent = new Intent(context, RichNotificationActivity.class);

        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode();
        kInputMode.setPrefillString("@name");
        kInputMode.setCharacterLimit(140);
        kInputMode.setKeyboardType(KeyboardType.NORMAL);
        keyboardAction.setRequestedInputMode(kInputMode);
        keyboardAction.setCallbackIntent(CallbackIntent.getActivityCallback(keyboardIntent));
        myActions.add(keyboardAction);

        SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Single Select");
        SingleSelectInputMode sInputMode = InputModeFactory.createSingleSelectInputMode();
        Intent singleSelectIntent = new Intent(context, RichNotificationActivity.class);

        sInputMode.addChoice("Black", "color_black");
        sInputMode.addChoice("White", "color_white");
        sInputMode.addChoice("Red", "color_red");
        singelSelectAction
                .setCallbackIntent(CallbackIntent.getActivityCallback(singleSelectIntent));

        singelSelectAction.setRequestedInputMode(sInputMode);
        singelSelectAction.setDescription("Select your favorite color.");
        myActions.add(singelSelectAction);

        SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
        Intent multipleSelectIntent = new Intent(context, RichNotificationActivity.class);
        MultiSelectInputMode mInputMode = InputModeFactory.createMultiSelectInputMode();

        multipeSelectAction.setDescription("Select your favorite colors.");
        mInputMode.addChoice("Black", "color_black");
        mInputMode.addChoice("White", "color_white");
        mInputMode.addChoice("Red", "color_red");
        mInputMode.addChoice("Blue", "color_blue");
        multipeSelectAction.setCallbackIntent(CallbackIntent
                .getActivityCallback(multipleSelectIntent));
        multipeSelectAction.setRequestedInputMode(mInputMode);
        myActions.add(multipeSelectAction);

        return myActions;
    }

}
