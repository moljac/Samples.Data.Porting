
package com.example.edgescreen.list;

import com.example.edgescreen.R;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;

import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.util.Log;
import android.widget.RemoteViews;
import android.widget.RemoteViewsService.RemoteViewsFactory;

import java.util.Calendar;

public class CocktailListAdapterFactory implements RemoteViewsFactory {
    static final String TAG = "CocktailListAdapter ";

    private Context mContext;

    private DBHelper mDbHelper;

    public CocktailListAdapterFactory(Context context) {
        Log.d(TAG, "CocktailListAdapterFactory constructor ");
        mContext = context;
        mDbHelper = DBHelper.getInstance();
        getMinute();
    }

    @Override
    public int getCount() {
        // TODO Auto-generated method stub

        int count = mDbHelper.getSize();
        return count;
    }

    @Override
    public long getItemId(int position) {
        // TODO Auto-generated method stub
        return position;
    }

    @Override
    public RemoteViews getLoadingView() {
        // TODO Auto-generated method stub
        return null;
    }

    @Override
    public RemoteViews getViewAt(int position) {
        // TODO Auto-generated method stub
        RemoteViews contentView = new RemoteViews(mContext.getPackageName(), R.layout.widget_item);
        Bundle extras = new Bundle();
        Intent fillInIntent = new Intent();
        if ((position % 2) == 0) {
            PendingIntent pIntent = PendingIntent.getActivity(mContext, 0, new Intent(
                    Intent.ACTION_DIAL), PendingIntent.FLAG_UPDATE_CURRENT);
            extras.putParcelable(Constants.EXTRA_CONTENT_INTENT, pIntent);
            fillInIntent.putExtras(extras);
            contentView.setOnClickFillInIntent(R.id.widget_item_layout, fillInIntent);
        } else {
            Intent intent = new Intent(Intent.ACTION_VIEW);
            intent.setType("vnd.android-dir/mms-sms");
            PendingIntent pIntent = PendingIntent.getActivity(mContext, 0, intent,
                    PendingIntent.FLAG_UPDATE_CURRENT);
            extras.putParcelable(Constants.EXTRA_CONTENT_INTENT, pIntent);
            fillInIntent.putExtras(extras);
            contentView.setOnClickFillInIntent(R.id.widget_item_layout, fillInIntent);
        }
        try {
            contentView.setTextViewText(R.id.tv_item, mDbHelper.getData(position));
        } catch (IndexOutOfBoundsException e) {

        }
        return contentView;
    }

    @Override
    public int getViewTypeCount() {
        // TODO Auto-generated method stub
        return 1;
    }

    @Override
    public boolean hasStableIds() {
        // TODO Auto-generated method stub
        return false;
    }

    @Override
    public void onCreate() {
        // TODO Auto-generated method stub
        IntentFilter filter = new IntentFilter();
        filter.addAction(Intent.ACTION_TIME_TICK);
        mContext.registerReceiver(mClockReceiver, filter);
    }

    @Override
    public void onDataSetChanged() {
        // TODO Auto-generated method stub

    }

    @Override
    public void onDestroy() {
        // TODO Auto-generated method stub
        mContext.unregisterReceiver(mClockReceiver);
    }

    BroadcastReceiver mClockReceiver = new BroadcastReceiver() {
        public void onReceive(Context context, Intent intent) {
            String action = intent.getAction();
            if (action.equals(Intent.ACTION_TIME_TICK)) {
                getMinute();
                SlookCocktailManager mgr = SlookCocktailManager.getInstance(mContext);
                int[] cocktailIds = mgr.getCocktailIds(new ComponentName(context,
                        CocktailListAdapterProvider.class));
                for (int i = 0; i < cocktailIds.length; i++) {
                    mgr.notifyCocktailViewDataChanged(cocktailIds[i], R.id.widgetlist);
                }
            }
        }
    };

    private void getMinute() {
        Calendar cal = Calendar.getInstance();
        int min = cal.get(Calendar.MINUTE);
        if (min == 0) {
            mDbHelper.clearData();
            mDbHelper.addData("0Min");
        } else {
            mDbHelper.addData(String.format("%02dMin", min));
        }
    }

}
