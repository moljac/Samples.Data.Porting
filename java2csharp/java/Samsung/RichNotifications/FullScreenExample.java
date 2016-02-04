
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
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.MultiSelectInputMode;
import com.samsung.android.sdk.richnotification.actions.SrnRemoteInputAction.SingleSelectInputMode;
import com.samsung.android.sdk.richnotification.templates.SrnPrimaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardSecondaryTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate;
import com.samsung.android.sdk.richnotification.templates.SrnStandardTemplate.HeaderSizeType;

public class FullScreenExample implements IExample {

    private final Context context;

    public FullScreenExample(Context ctx) {
        context = ctx;
    }

    @Override
    public SrnRichNotification createRichNoti() {
        SrnRichNotification noti = new SrnRichNotification(context);
        noti.setReadout("New Full Header", "FullScreen.");

        noti.setPrimaryTemplate(getFullScreenTemplate());

        noti.setSecondaryTemplate(getFullSecondaryTemplate());
        try{
        noti.addActionsWithPermissionCheck(getActions());
        } catch(Exception e)
        { 
          e.printStackTrace();  
        }
        noti.setAlertType(AlertType.SOUND);

        Bitmap appIconBitmap = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.uber_icon);
        SrnImageAsset appIcon = new SrnImageAsset(context, "app_icon", appIconBitmap);
        noti.setIcon(appIcon);

        noti.setTitle("Movify");

        return noti;
    }

    public SrnPrimaryTemplate getFullScreenTemplate() {
        SrnStandardTemplate mFullScreenTemplate = new SrnStandardTemplate(
                HeaderSizeType.FULL_SCREEN);
        Bitmap bg = BitmapFactory.decodeResource(context.getResources(), R.drawable.hercules);
        SrnImageAsset ibg = new SrnImageAsset(context, "full_h", bg);

        mFullScreenTemplate.setBackgroundImage(ibg);

        mFullScreenTemplate.setSubHeader("<b> New Movie Release </b>");
        mFullScreenTemplate.setBody("<b>Brett Ratner</b>'s New Movie <b>Hercules</b> released");

        return mFullScreenTemplate;
    }

    public SrnSecondaryTemplate getFullSecondaryTemplate() {
        SrnStandardSecondaryTemplate fullSecondaryTemplate = new SrnStandardSecondaryTemplate();

        fullSecondaryTemplate.setTitle("<b>Movie Information</b>");
        Time today = new Time(Time.getCurrentTimezone());
        today.setToNow();

        fullSecondaryTemplate.setSubHeader("<b>Release Date</b>:" + today.year + "/" + today.month
                + "/" + today.monthDay);

        fullSecondaryTemplate
                .setBody("<b>Synopsis</b>: Having endured his legendary twelve labors, "
                        + "Hercules, the Greek demigod, has his life as a sword-for-hire "
                        + "tested when the King of Thrace and his daughter seek his aid "
                        + "in defeating a tyrannical warlord.");

        Bitmap commentBM = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.star_subtitle);
        SrnImageAsset commentIcon = new SrnImageAsset(context, "comment_icon", commentBM);
        fullSecondaryTemplate.setSmallIcon1(commentIcon, "5/5");

        Bitmap likeBM = BitmapFactory.decodeResource(context.getResources(), R.drawable.like);
        SrnImageAsset likeIcon = new SrnImageAsset(context, "like_icon", likeBM);
        fullSecondaryTemplate.setSmallIcon2(likeIcon, "999+");

        return fullSecondaryTemplate;

    }

    public List<SrnAction> getActions() {
        ArrayList<SrnAction> myActions = new ArrayList<SrnAction>();

        SrnHostAction primaryAction = new SrnHostAction("View On IMDB");
        Bitmap webActionBM = BitmapFactory.decodeResource(context.getResources(),
                R.drawable.voice_action);

        SrnImageAsset webActionIcon = new SrnImageAsset(context, "web_icon", webActionBM);

        primaryAction.setIcon(webActionIcon);
        primaryAction.setToast("Music will be played on Phone!");

        String url = "http://www.imdb.com/title/tt1267297/";
        Intent resultIntent = new Intent(context, MyCallbackActivity.class);
        resultIntent.setData(Uri.parse(url));
        primaryAction.setCallbackIntent(CallbackIntent.getActivityCallback(resultIntent));

        myActions.add(primaryAction);

        SrnHostAction action2 = new SrnHostAction("Watch Trailier On Phone");
        action2.setIcon(webActionIcon);
        action2.setToast("Youtube App will be launched on Phone!");

        String yturl = "http://www.youtube.com/watch?v=GFqY089piQ4";
        Intent videoIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(yturl));
        videoIntent.setData(Uri.parse(url));

        action2.setCallbackIntent(CallbackIntent.getActivityCallback(videoIntent));

        myActions.add(action2);

        SrnRemoteBuiltInAction builtInAction = new SrnRemoteBuiltInAction("Call");

        builtInAction.setType(SrnRemoteBuiltInAction.OperationType.CALL);
        builtInAction.setData(Uri.fromParts("tel", "+14157364480", null));

        myActions.add(builtInAction);

        SrnRemoteInputAction keyboardAction = new SrnRemoteInputAction("Comments");

        KeyboardInputMode kInputMode = InputModeFactory.createKeyboardInputMode();
        kInputMode.setPrefillString("@name");
        kInputMode.setCharacterLimit(140);
        kInputMode.setKeyboardType(KeyboardType.NORMAL);
        keyboardAction.setRequestedInputMode(kInputMode);

        Intent keyboardIntent = new Intent(context, RichNotificationActivity.class);
        keyboardAction.setCallbackIntent(CallbackIntent.getActivityCallback(keyboardIntent));

        myActions.add(keyboardAction);

        SrnRemoteInputAction singelSelectAction = new SrnRemoteInputAction("Survey");

        SingleSelectInputMode sInputMode = InputModeFactory.createSingleSelectInputMode();
        Bitmap bitmap = BitmapFactory
                .decodeResource(context.getResources(), R.drawable.ic_launcher);

        sInputMode
                .addChoice("Yes", "color_black", new SrnImageAsset(context, "black_icon", bitmap));
        sInputMode.addChoice("No", "color_white", new SrnImageAsset(context, "black_icon", bitmap));
        sInputMode.addChoice("I don't remember", "color_red", new SrnImageAsset(context,
                "black_icon", bitmap));

        singelSelectAction.setRequestedInputMode(sInputMode);
        singelSelectAction.setDescription("Did you watch the movie");

        Intent singleSelectIntent = new Intent(context, RichNotificationActivity.class);
        singelSelectAction
                .setCallbackIntent(CallbackIntent.getActivityCallback(singleSelectIntent));

        myActions.add(singelSelectAction);

        SrnRemoteInputAction multipeSelectAction = new SrnRemoteInputAction("Multi Select");
        MultiSelectInputMode mInputMode = InputModeFactory.createMultiSelectInputMode();
        multipeSelectAction.setDescription("Select your favorite Actors.");
        mInputMode.addChoice("Dwayne Johnson", "color_black");
        mInputMode.addChoice("Ian McShane", "color_white");
        mInputMode.addChoice("John Hurt", "color_red");
        mInputMode.addChoice("Rufus Sewell", "color_blue");

        multipeSelectAction.setRequestedInputMode(mInputMode);

        Intent multipleSelectIntent = new Intent(context, RichNotificationActivity.class);
        multipeSelectAction.setCallbackIntent(CallbackIntent
                .getActivityCallback(multipleSelectIntent));

        myActions.add(multipeSelectAction);

        return myActions;
    }

}
