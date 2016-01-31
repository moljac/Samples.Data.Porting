
package com.example.edgescreen.list;

import com.example.edgescreen.R;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

import android.app.PendingIntent;
import android.app.PendingIntent.CanceledException;
import android.content.Context;
import android.content.Intent;
import android.net.Uri;
import android.util.Log;
import android.widget.RemoteViews;

public class CocktailListAdapterProvider extends SlookCocktailProvider {
    private static final String TAG = "CocktailListAdapterProvider";

    @Override
    public void onUpdate(Context context, SlookCocktailManager cocktailManager, int[] cocktailIds) {
        Intent intent = new Intent(context, CocktailListAdapterService.class);
        intent.setData(Uri.parse(intent.toUri(Intent.URI_INTENT_SCHEME)));

        RemoteViews views = new RemoteViews(context.getPackageName(), R.layout.widget_layout);

        views.setRemoteAdapter(R.id.widgetlist, intent);
        views.setEmptyView(R.id.widgetlist, R.id.emptylist);

        Intent itemClickIntent = new Intent(context, CocktailListAdapterProvider.class);
        itemClickIntent.setAction(Constants.COCKTAIL_LIST_ADAPTER_CLICK_ACTION);

        PendingIntent itemClickPendingIntent = PendingIntent.getBroadcast(context, 1,
                itemClickIntent, PendingIntent.FLAG_UPDATE_CURRENT);
        views.setPendingIntentTemplate(R.id.widgetlist, itemClickPendingIntent);

        for (int i = 0; i < cocktailIds.length; i++) {
            cocktailManager.updateCocktail(cocktailIds[i], views);
        }
    }

    @Override
    public void onReceive(Context context, Intent intent) {
        super.onReceive(context, intent);
        if (intent.getAction() == Constants.COCKTAIL_LIST_ADAPTER_CLICK_ACTION) {
            PendingIntent p = intent.getParcelableExtra(Constants.EXTRA_CONTENT_INTENT);
            if (p != null) {
                try {
                    p.send();
                } catch (CanceledException e) {
                    e.printStackTrace();
                }
            }
        }

    }

    @Override
    public void onEnabled(Context context) {
        super.onEnabled(context);
        Log.d(TAG, "onEnabled");
    }
}
