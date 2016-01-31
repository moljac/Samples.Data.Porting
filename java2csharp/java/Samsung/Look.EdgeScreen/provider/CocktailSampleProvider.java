package com.example.edgescreen.provider;

import com.example.edgescreen.R;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

import android.app.PendingIntent;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.widget.RemoteViews;

public class CocktailSampleProvider extends SlookCocktailProvider {
    @Override
    public void onUpdate(Context context, SlookCocktailManager cocktailBarManager, int[] cocktailIds) {
        RemoteViews rv = new RemoteViews(context.getPackageName(), R.layout.sample_panel);
        String str = context.getResources().getString(R.string.vertical_text);
        rv.setTextViewText(R.id.text, str);
        setPendingIntent(context, rv);
        for (int i = 0; i < cocktailIds.length; i++) {
            cocktailBarManager.updateCocktail(cocktailIds[i], rv);
        }
    }

    private void setPendingIntent(Context context, RemoteViews rv) {
        setPendingIntent(context, R.id.btn_phone, new Intent(Intent.ACTION_DIAL), rv);
        setPendingIntent(context, R.id.btn_camera, new Intent("android.media.action.IMAGE_CAPTURE"), rv);
        setPendingIntent(context, R.id.btn_internet, new Intent(Intent.ACTION_VIEW, Uri.parse("http://www.google.com")), rv);
    }

    private void setPendingIntent(Context context, int rscId, Intent intent, RemoteViews rv) {
        PendingIntent itemClickPendingIntent = PendingIntent.getActivity(context, 0, intent,
                PendingIntent.FLAG_UPDATE_CURRENT);
        rv.setOnClickPendingIntent(rscId, itemClickPendingIntent);
    }

}
