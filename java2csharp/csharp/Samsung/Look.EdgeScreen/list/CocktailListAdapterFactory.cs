using System;

namespace com.example.edgescreen.list
{

	using SlookCocktailManager = com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;

	using PendingIntent = android.app.PendingIntent;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using RemoteViews = android.widget.RemoteViews;
	using RemoteViewsFactory = android.widget.RemoteViewsService.RemoteViewsFactory;

	public class CocktailListAdapterFactory : RemoteViewsFactory
	{
		internal const string TAG = "CocktailListAdapter ";

		private Context mContext;

		private DBHelper mDbHelper;

		public CocktailListAdapterFactory(Context context)
		{
			Log.d(TAG, "CocktailListAdapterFactory constructor ");
			mContext = context;
			mDbHelper = DBHelper.Instance;
			Minute;
		}

		public override int Count
		{
			get
			{
				// TODO Auto-generated method stub
    
				int count = mDbHelper.Size;
				return count;
			}
		}

		public override long getItemId(int position)
		{
			// TODO Auto-generated method stub
			return position;
		}

		public override RemoteViews LoadingView
		{
			get
			{
				// TODO Auto-generated method stub
				return null;
			}
		}

		public override RemoteViews getViewAt(int position)
		{
			// TODO Auto-generated method stub
			RemoteViews contentView = new RemoteViews(mContext.PackageName, R.layout.widget_item);
			Bundle extras = new Bundle();
			Intent fillInIntent = new Intent();
			if ((position % 2) == 0)
			{
				PendingIntent pIntent = PendingIntent.getActivity(mContext, 0, new Intent(Intent.ACTION_DIAL), PendingIntent.FLAG_UPDATE_CURRENT);
				extras.putParcelable(Constants.EXTRA_CONTENT_INTENT, pIntent);
				fillInIntent.putExtras(extras);
				contentView.setOnClickFillInIntent(R.id.widget_item_layout, fillInIntent);
			}
			else
			{
				Intent intent = new Intent(Intent.ACTION_VIEW);
				intent.Type = "vnd.android-dir/mms-sms";
				PendingIntent pIntent = PendingIntent.getActivity(mContext, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT);
				extras.putParcelable(Constants.EXTRA_CONTENT_INTENT, pIntent);
				fillInIntent.putExtras(extras);
				contentView.setOnClickFillInIntent(R.id.widget_item_layout, fillInIntent);
			}
			try
			{
				contentView.setTextViewText(R.id.tv_item, mDbHelper.getData(position));
			}
			catch (System.IndexOutOfRangeException)
			{

			}
			return contentView;
		}

		public override int ViewTypeCount
		{
			get
			{
				// TODO Auto-generated method stub
				return 1;
			}
		}

		public override bool hasStableIds()
		{
			// TODO Auto-generated method stub
			return false;
		}

		public override void onCreate()
		{
			// TODO Auto-generated method stub
			IntentFilter filter = new IntentFilter();
			filter.addAction(Intent.ACTION_TIME_TICK);
			mContext.registerReceiver(mClockReceiver, filter);
		}

		public override void onDataSetChanged()
		{
			// TODO Auto-generated method stub

		}

		public override void onDestroy()
		{
			// TODO Auto-generated method stub
			mContext.unregisterReceiver(mClockReceiver);
		}

		internal BroadcastReceiver mClockReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}

			public virtual void onReceive(Context context, Intent intent)
			{
				string action = intent.Action;
				if (action.Equals(Intent.ACTION_TIME_TICK))
				{
					outerInstance.Minute;
					SlookCocktailManager mgr = SlookCocktailManager.getInstance(outerInstance.mContext);
					int[] cocktailIds = mgr.getCocktailIds(new ComponentName(context, typeof(CocktailListAdapterProvider)));
					for (int i = 0; i < cocktailIds.Length; i++)
					{
						mgr.notifyCocktailViewDataChanged(cocktailIds[i], R.id.widgetlist);
					}
				}
			}
		}

		private void getMinute()
		{
			DateTime cal = new DateTime();
			int min = cal.Minute;
			if (min == 0)
			{
				mDbHelper.clearData();
				mDbHelper.addData("0Min");
			}
			else
			{
				mDbHelper.addData(string.Format("{0:D2}Min", min));
			}
		}

	}

}