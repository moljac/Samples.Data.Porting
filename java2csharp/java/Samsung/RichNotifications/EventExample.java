
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
import com.samsung.android.sdk.richnotification.SrnRichNotification.PopupType;
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
import com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

public class EventExample implements IExample {

    private final Context mContext;

    public EventExample(Context ctx) {
        mContext = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(mContext);

        noti.setReadout("New Event", "Today there is an event.");

        Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.uber_icon);
        SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
        noti.setIcon(appIcon);

        noti.setTitle("Eventify");

        noti.setPrimaryTemplate(getEventTemplate());

        noti.setSecondaryTemplate(getEventSecondaryTemplate());
        try{
        noti.addActionsWithPermissionCheck(getActions());
        } catch(Exception e)
        { 
          e.printStackTrace();  
        }
        noti.setAlertType(AlertType.SOUND_AND_VIBRATION, PopupType.NORMAL);

        return noti;
    }

    public SrnPrimaryTemplate getEventTemplate() {
        SrnStandardTemplate mMediumHeaderTemplate = new SrnStandardTemplate(HeaderSizeType.MEDIUM);
        mMediumHeaderTemplate.setBackgroundColor(Color.rgb(0, 255, 0));

        mMediumHeaderTemplate.setSubHeader("<b> Scheduled Event </b>");
        mMediumHeaderTemplate.setBody("Scheduled Meeting 10mins from now");

        return mMediumHeaderTemplate;
    }

    public SrnSecondaryTemplate getEventSecondaryTemplate() {
        SrnQRSecondaryTemplate qrSecTemplate = new SrnQRSecondaryTemplate();

        Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.map);
        SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
        qrSecTemplate.setImage(qrCodeBig);

        qrSecTemplate.addListItem("Attendee", "Chitra Sampath Kumar");
        qrSecTemplate.addListItem("Attendee", "Taehee Lee");
        qrSecTemplate.addListItem("Attendee", "Hunje Yun");
        qrSecTemplate.addListItem("Attendee", "Minsuk Choi");
        qrSecTemplate.addListItem("Attendee", "Jihwa Park");
        qrSecTemplate.addListItem("Attendee", "Junho Lee");

        Bitmap commentBM = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.like);
        SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
        qrSecTemplate.setSmallIcon1(commentIcon, "99999999+");

        Bitmap likeBM = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.unlike);
        SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
        qrSecTemplate.setSmallIcon2(likeIcon, "99999999+");

        return qrSecTemplate;
    }

    public List<SrnAction> getActions() {
        ArrayList<SrnAction> myActions = new ArrayList<SrnAction>();

        SrnHostAction primaryAction = new SrnHostAction("Launch Web");
        Bitmap webActionBM = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.voice_action);
        SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);

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

        Bitmap dialerBM = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.call);
        SrnImageAsset dialerIcon = new SrnImageAsset(mContext, "dialer_icon", dialerBM);
        remoteLaunchAction.setIcon(dialerIcon);
        Intent remoteLaunchIntentResult = new Intent(mContext, RichNotificationActivity.class);
        remoteLaunchAction.setCallbackIntent(CallbackIntent
                .getActivityCallback(remoteLaunchIntentResult));
        myActions.add(remoteLaunchAction);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Keyboard");
        Intent keyboardIntent = new Intent(
                "com.samsung.android.richnotification.sample.callback_broadcast");

        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode();
        kInputMode.setPrefillString("@name");
        kInputMode.setCharacterLimit(140);
        kInputMode.setKeyboardType(KeyboardType.NORMAL);
        keyboardAction.setRequestedInputMode(kInputMode);
        keyboardAction.setCallbackIntent(CallbackIntent.getBroadcastCallback(keyboardIntent));
        myActions.add(keyboardAction);

        SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Single Select");
        SingleSelectInputMode sInputMode = InputModeFactory.createSingleSelectInputMode();
        Intent singleSelectIntent = new Intent(mContext, RichNotificationActivity.class);
        Bitmap bitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.ic_launcher);

        sInputMode.addChoice("Black", "color_black", new SrnImageAsset(mContext, "black_icon",
                bitmap));
        sInputMode.addChoice("White", "color_white", new SrnImageAsset(mContext, "white_icon",
                bitmap));
        sInputMode.addChoice("Red", "color_red", new SrnImageAsset(mContext, "white_icon", bitmap));
        singelSelectAction
                .setCallbackIntent(CallbackIntent.getActivityCallback(singleSelectIntent));
        singelSelectAction.setRequestedInputMode(sInputMode);
        singelSelectAction.setDescription("Select your favorite color.");
        myActions.add(singelSelectAction);

        SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
        Intent multipleSelectIntent = new Intent(mContext, RichNotificationActivity.class);
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
