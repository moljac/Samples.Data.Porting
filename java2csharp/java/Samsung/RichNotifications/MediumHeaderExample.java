
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

public class MediumHeaderExample implements IExample {

    private final Context mContext;

    public MediumHeaderExample(Context ctx) {
        mContext = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(mContext);
        noti.setReadout("New Book notification from Bookify",
                "Kate Daniels New Album Magic Breaks is released");

        noti.setPrimaryTemplate(getSrnMediumHeaderTemplate());

        noti.setSecondaryTemplate(getMediumSecondaryTemplate());
        try{
        noti.addActionsWithPermissionCheck(getActions());
        } catch(Exception e)
        { 
          e.printStackTrace();  
        }
        noti.setAlertType(AlertType.SOUND_AND_VIBRATION);

        Bitmap appIconBitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.uber_icon);
        SrnImageAsset appIcon = new SrnImageAsset(mContext, "app_icon", appIconBitmap);
        noti.setIcon(appIcon);

        noti.setTitle("Bookify");

        return noti;
    }

    public SrnPrimaryTemplate getSrnMediumHeaderTemplate() {
        SrnStandardTemplate mMediumHeaderTemplate = new SrnStandardTemplate(HeaderSizeType.MEDIUM);
        mMediumHeaderTemplate.setBackgroundColor(Color.rgb(0, 255, 0));

        mMediumHeaderTemplate.setSubHeader("<b> New Book Release </b>");
        mMediumHeaderTemplate.setBody("<b>Kate Daniel</b>'s New Book <b>Magic Breaks</b> released");

        return mMediumHeaderTemplate;
    }

    public SrnSecondaryTemplate getMediumSecondaryTemplate() {
        SrnStandardSecondaryTemplate mediumSecondaryTemplate = new SrnStandardSecondaryTemplate();

        mediumSecondaryTemplate.setTitle("<b>Book Information</b>");
        Time today = new Time(Time.getCurrentTimezone());
        today.setToNow();

        mediumSecondaryTemplate.setSubHeader("<b>Release Date</b>:" + today.year + "/"
                + today.month + "/" + today.monthDay);

        mediumSecondaryTemplate.setBody("<b>Overview</b>: As the mate of the Beast Lord, Curran, "
                + "former mercenary Kate Daniels has more responsibilities than "
                + "it seems possible to juggle. Not only is she still struggling "
                + "to keep her investigative business afloat, she must now deal with "
                + "the affairs of the pack, including preparing her people for attack "
                + "from Roland, a cruel ancient being with god-like powers. "
                + "Since Kate¡¯s connection to Roland has come out into the open, no one is safe");

        Bitmap qrCodeBitmap = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.magic);
        SrnImageAsset qrCodeBig = new SrnImageAsset(mContext, "qr_code_big", qrCodeBitmap);
        mediumSecondaryTemplate.setImage(qrCodeBig);

        Bitmap commentBM = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.star_subtitle);
        SrnImageAsset commentIcon = new SrnImageAsset(mContext, "comment_icon", commentBM);
        mediumSecondaryTemplate.setSmallIcon1(commentIcon, "34/5");

        Bitmap likeBM = BitmapFactory.decodeResource(mContext.getResources(), R.drawable.like);
        SrnImageAsset likeIcon = new SrnImageAsset(mContext, "like_icon", likeBM);
        mediumSecondaryTemplate.setSmallIcon2(likeIcon, "425");

        return mediumSecondaryTemplate;
    }

    public List<SrnAction> getActions() {
        ArrayList<SrnAction> myActions = new ArrayList<SrnAction>();

        SrnHostAction primaryAction = new SrnHostAction("Read On Phone");
        Bitmap webActionBM = BitmapFactory.decodeResource(mContext.getResources(),
                R.drawable.voice_action);
        SrnImageAsset webActionIcon = new SrnImageAsset(mContext, "web_icon", webActionBM);

        Intent resultIntent = new Intent(mContext, MyCallbackActivity.class);
        resultIntent.setData(Uri.parse("http://musicmp3.ru/artist_taylor-swift__album_red.html#.U-Cj3WPzkjY"));

        primaryAction.setIcon(webActionIcon);
        primaryAction.setToast("Music will be played on Phone!");
        primaryAction.setCallbackIntent(CallbackIntent.getActivityCallback(resultIntent));
        myActions.add(primaryAction);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comment");

        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode()
                .setPrefillString("@name")
                .setCharacterLimit(140)
                .setKeyboardType(KeyboardType.NORMAL);

        keyboardAction.setRequestedInputMode(kInputMode);

        Intent keyboardIntent = new Intent(mContext, RichNotificationActivity.class);
        keyboardAction.setCallbackIntent(CallbackIntent.getActivityCallback(keyboardIntent));

        myActions.add(keyboardAction);

        return myActions;
    }
}
