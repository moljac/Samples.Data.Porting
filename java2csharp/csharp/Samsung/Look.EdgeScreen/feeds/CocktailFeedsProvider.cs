using System;

namespace com.example.edgescreen.feeds
{

	using SlookCocktailManager = com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
	using SlookCocktailProvider = com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

	using Context = android.content.Context;
	using RemoteViews = android.widget.RemoteViews;


	public class CocktailFeedsProvider : SlookCocktailProvider
	{
		public override void onUpdate(Context context, SlookCocktailManager cocktailBarManager, int[] cocktailIds)
		{
			// create RemoteViews
			RemoteViews rv = new RemoteViews(context.PackageName, R.layout.feeds_panel_layout);
			string str = context.Resources.getString(R.@string.feeds_contents);

			// set text in RemoteViews
			rv.setTextViewText(R.id.text, str + " at " + CurrentDate);

			// update cocktail
			for (int i = 0; i < cocktailIds.Length; i++)
			{
				cocktailBarManager.updateCocktail(cocktailIds[i], rv);
			}
		}

		private string CurrentDate
		{
			get
			{
				long now = DateTimeHelperClass.CurrentUnixTimeMillis();
				DateTime date = new DateTime(now);
    
				SimpleDateFormat format = new SimpleDateFormat("yyyy-MM-dd");
				return format.format(date);
			}
		}
	}

}