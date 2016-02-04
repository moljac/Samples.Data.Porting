
package com.samsung.android.richnotification.sample;

import java.util.ArrayList;
import java.util.List;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.text.format.Time;

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
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
import com.samsung.android.sdk.richnotification.templates.SrnLargeHeaderTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;

public class LargeHeaderExample implements IExample {

    private final Context mContext;

    public LargeHeaderExample(Context ctx) {
        mContext = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(mContext);
        noti.setReadout("Facebook post", "Your friend post, nice weather in SFO");

        noti.setPrimaryTemplate(getSrnLargeHeaderTemplate());

        noti.setSecondaryTemplate(getSmallSecondaryTemplate());
        try{
        noti.addActionsWithPermissionCheck(getActions());
         } catch(Exception e)
         { 
           e.printStackTrace();  
         }
        noti.setAlertType(AlertType.SILENCE);

        Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.uber_icon);
        SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
        noti.setIcon(appIcon);

        noti.setTitle("Nice Weather");

        return noti;
    }

    public SrnPrimaryTemplate getSrnLargeHeaderTemplate() {
        SrnLargeHeaderTemplate mLargeHeaderTemplate = new SrnLargeHeaderTemplate();

        Bitmap bg = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.weather);
        SrnImageAsset ibg = new SrnImageAsset(mContext, "l_h", bg);

        mLargeHeaderTemplate.setBackgroundImage(ibg);

        return mLargeHeaderTemplate;
    }

    public SrnSecondaryTemplate getSmallSecondaryTemplate() {
        SrnStandardSecondaryTemplate smallSecTemplate = new SrnStandardSecondaryTemplate();

        smallSecTemplate.setTitle("<b>Album Information</b>");
        Time today = new Time(Time.getCurrentTimezone());
        today.setToNow();

        smallSecTemplate.setSubHeader("<b>Release Date</b>:" + today.year + "/" + today.month + "/"
                + today.monthDay);

        smallSecTemplate.setBody("<b>Tracks in Red</b>: State Of Grace, Red, Treacherous, "
                + "I Knew You Were Trouble, All Too Well, 22, I Almost Do, "
                + "We Are Never Ever Getting Back Together, Stay Stay Stay, "
                + "The Last Time, Holy Ground, Sad Beautiful Tragic, "
                + "The Lucky One, Everything Has Changed, Starlight, "
                + "Begin Again, Bonus Tracks, The Moment I Knew, Come Back... "
                + "Be Here, Girl At Home");

        Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.red);
        SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
        smallSecTemplate.setImage(qrCodeBig);

        Bitmap commentBM = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.star_subtitle);
        SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
        smallSecTemplate.setSmallIcon1(commentIcon, "4/5");

        Bitmap likeBM = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.like);
        SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
        smallSecTemplate.setSmallIcon2(likeIcon, "999+");

        return smallSecTemplate;
    }

    public List<SrnAction> getActions() {
        ArrayList<SrnAction> myActions = new ArrayList<SrnAction>();

        SrnHostAction primaryAction = new SrnHostAction("See Forecast");
        Bitmap webActionBM = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.voice_action);
        SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);

        primaryAction.setIcon(webActionIcon);
        primaryAction.setToast("Check your phone!");
        
        Intent resultIntent = new Intent(Intent.ACTION_VIEW);
        resultIntent.setData(Uri.parse( "http://www.weather.com"));

        primaryAction.setCallbackIntent(CallbackIntent.getActivityCallback(resultIntent));
        
        myActions.add(primaryAction);

        SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");

        builtInAction.setType(SrnRemoteBuiltInAction.OperationType.CALL);
        builtInAction.setData(Uri.fromParts("tel", "+821095307811", null));
        
        myActions.add(builtInAction);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");

        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode();
        kInputMode.setPrefillString("@name");
        kInputMode.setCharacterLimit(140);
        kInputMode.setKeyboardType(KeyboardType.NORMAL);

        keyboardAction.setRequestedInputMode(kInputMode);

        Intent keyboardIntent = new Intent(mContext, SrnRichNotification.class);
        keyboardAction.setCallbackIntent(CallbackIntent.getActivityCallback(keyboardIntent));

        myActions.add(keyboardAction);

        SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Survey");

        SingleSelectInputMode sInputMode = InputModeFactory.createSingleSelectInputMode();
        sInputMode.addChoice("Love This", "color_black");
        sInputMode.addChoice("Hate it", "color_white");
        sInputMode.addChoice("Am Cool", "color_red");

        singelSelectAction.setRequestedInputMode(sInputMode);
        singelSelectAction.setDescription("Is this weather good?");

        Intent singleSelectIntent = new Intent(mContext, SrnRichNotification.class);
        singelSelectAction
                .setCallbackIntent(CallbackIntent.getActivityCallback(singleSelectIntent));

        myActions.add(singelSelectAction);

        return myActions;
    }
}
