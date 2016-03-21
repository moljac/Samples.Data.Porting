namespace com.example.edgescreen.list
{

	using SlookCocktailManager = com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
	using SlookCocktailProvider = com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

	using PendingIntent = android.app.PendingIntent;
	using CanceledException = android.app.PendingIntent.CanceledException;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Uri = android.net.Uri;
	using Log = android.util.Log;
	using RemoteViews = android.widget.RemoteViews;

	public class CocktailListAdapterProvider : SlookCocktailProvider
	{
		private const string TAG = "CocktailListAdapterProvider";

		public override void onUpdate(Context context, SlookCocktailManager cocktailManager, int[] cocktailIds)
		{
			Intent intent = new Intent(context, typeof(CocktailListAdapterService));
			intent.Data = Uri.parse(intent.toUri(Intent.URI_INTENT_SCHEME));

			RemoteViews views = new RemoteViews(context.PackageName, R.layout.widget_layout);

			views.setRemoteAdapter(R.id.widgetlist, intent);
			views.setEmptyView(R.id.widgetlist, R.id.emptylist);

			Intent itemClickIntent = new Intent(context, typeof(CocktailListAdapterProvider));
			itemClickIntent.Action = Constants.COCKTAIL_LIST_ADAPTER_CLICK_ACTION;

			PendingIntent itemClickPendingIntent = PendingIntent.getBroadcast(context, 1, itemClickIntent, PendingIntent.FLAG_UPDATE_CURRENT);
			views.setPendingIntentTemplate(R.id.widgetlist, itemClickPendingIntent);

			for (int i = 0; i < cocktailIds.Length; i++)
			{
				cocktailManager.updateCocktail(cocktailIds[i], views);
			}
		}

		public override void onReceive(Context context, Intent intent)
		{
			base.onReceive(context, intent);
			if (intent.Action == Constants.COCKTAIL_LIST_ADAPTER_CLICK_ACTION)
			{
				PendingIntent p = intent.getParcelableExtra(Constants.EXTRA_CONTENT_INTENT);
				if (p != null)
				{
					try
					{
						p.send();
					}
					catch (PendingIntent.CanceledException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}

		}

		public override void onEnabled(Context context)
		{
			base.onEnabled(context);
			Log.d(TAG, "onEnabled");
		}
	}

}