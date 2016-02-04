
package com.samsung.android.richnotification.sample;

import java.util.ArrayList;
import java.util.List;

import android.content.Context;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.net.Uri;
import android.text.format.Time;

import com.samsung.android.sdk.richnotification.SrnAction;
import com.samsung.android.sdk.richnotification.SrnAction.CallbackIntent;
import com.samsung.android.sdk.richnotification.SrnImageAsset;
import com.samsung.android.sdk.richnotification.SrnRichNotification;
import com.samsung.android.sdk.richnotification.SrnRichNotification.AlertType;
import com.samsung.android.sdk.richnotification.actions.SrnHostAction;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.InputModeFactory;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.KeyboardInputMode.KeyboardType;
import com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

public class SmallHeaderExample implements IExample {

    private final Context mContext;

    public SmallHeaderExample(Context ctx) {
        mContext = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(mContext);
        noti.setReadout("New Album notification from Songify",
                "Taylor Swift New Album Red is released");
        noti.setPrimaryTemplate(getSmallHeaderTemplate());

        noti.setSecondaryTemplate(getSmallSecondaryTemplate());

        noti.setTitle("Songify");
        try{
            noti.addActionsWithPermissionCheck(getActions());
        } catch(Exception e)
        { 
              e.printStackTrace();  
        }
        noti.setAlertType(AlertType.VIBRATION);

        //        Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.getResources(),
        //                R.drawable.uber_icon);
        //        SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
        //        noti.setIcon(appIcon);

        return noti;
    }

    public SrnPrimaryTemplate getSmallHeaderTemplate() {
        SrnStandardTemplate smallHeaderTemplate = new SrnStandardTemplate(HeaderSizeType.SMALL);

        smallHeaderTemplate.setSubHeader("<b> New Album Release </b>");
        smallHeaderTemplate.setBody("<b>Taylor Swift</b>'s New Album <b>Red</b> released");
        smallHeaderTemplate.setBackgroundColor(Color.rgb(0, 0, 255));

        return smallHeaderTemplate;
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
                + "The Last Time, Holy Ground, Sad Beautiful Tragic, The Lucky One, "
                + "Everything Has Changed, Starlight, Begin Again, Bonus Tracks, "
                + "The Moment I Knew, Come Back... Be Here, Girl At Home");

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

        SrnHostAction primaryAction = new SrnHostAction("Listen On Phone");
        Bitmap listenBitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.listen);
        SrnImageAsset listenIcon = new SrnImageAsset(mContext, "web_icon", listenBitmap);

        String url = "http://musicmp3.ru/artist_taylor-swift__album_red.html#.U-Cj3WPzkjY";
        Intent resultIntent = new Intent(mContext, MyCallbackActivity.class);
        resultIntent.setData(Uri.parse(url));

        primaryAction.setIcon(listenIcon);
        primaryAction.setToast("Music will be played on Phone!");
        primaryAction.setCallbackIntent(CallbackIntent.getActivityCallback(resultIntent));
        myActions.add(primaryAction);

        SrnHostAction action2 = new SrnHostAction("Watch On Phone");
        String yturl = "http://www.youtube.com/watch?v=Smu1jse33bQ";
        Intent videoIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(yturl));
        videoIntent.setData(Uri.parse(url));

        Bitmap watctBitmap = BitmapFactory
                .decodeResource(mContext.getResources(), R.drawable.watch);
        SrnImageAsset watchIcon = new SrnImageAsset(mContext, "web_icon", watctBitmap);
        action2.setIcon(watchIcon);
        action2.setToast("Youtube App will be launched on Phone!");
        action2.setCallbackIntent(CallbackIntent.getActivityCallback(videoIntent));
        myActions.add(action2);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");
        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode()
                .setPrefillString("@name")
                .setCharacterLimit(140)
                .setKeyboardType(KeyboardType.NORMAL);
        
        keyboardAction.setRequestedInputMode(kInputMode);
        
        Intent keyboardIntent = new Intent(
                "com.samsung.android.richnotification.sample.callback_broadcast");
        keyboardAction.setCallbackIntent(CallbackIntent.getBroadcastCallback(keyboardIntent));
        
        myActions.add(keyboardAction);

        return myActions;
    }
}
