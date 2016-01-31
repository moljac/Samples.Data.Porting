
package com.example.edgescreen.feeds;

import com.example.edgescreen.R;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

import android.content.Context;
import android.widget.RemoteViews;

import java.text.SimpleDateFormat;
import java.util.Date;

public class CocktailFeedsProvider extends SlookCocktailProvider {
    @Override
    public void onUpdate(Context context, SlookCocktailManager cocktailBarManager, int[] cocktailIds) {
        // create RemoteViews
        RemoteViews rv = new RemoteViews(context.getPackageName(), R.layout.feeds_panel_layout);
        String str = context.getResources().getString(R.string.feeds_contents);

        // set text in RemoteViews
        rv.setTextViewText(R.id.text, str + " at " + getCurrentDate());

        // update cocktail
        for (int i = 0; i < cocktailIds.length; i++) {
            cocktailBarManager.updateCocktail(cocktailIds[i], rv);
        }
    }

    private String getCurrentDate() {
        long now = System.currentTimeMillis();
        Date date = new Date(now);

        SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd");
        return format.format(date);
    }
}
