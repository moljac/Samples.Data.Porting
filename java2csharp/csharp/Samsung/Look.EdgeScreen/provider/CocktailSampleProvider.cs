namespace com.example.edgescreen.provider
{

	using SlookCocktailManager = com.samsung.android.sdk.look.cocktailbar.SlookCocktailManager;
	using SlookCocktailProvider = com.samsung.android.sdk.look.cocktailbar.SlookCocktailProvider;

	using PendingIntent = android.app.PendingIntent;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Uri = android.net.Uri;
	using RemoteViews = android.widget.RemoteViews;

	public class CocktailSampleProvider : SlookCocktailProvider
	{
		public override void onUpdate(Context context, SlookCocktailManager cocktailBarManager, int[] cocktailIds)
		{
			RemoteViews rv = new RemoteViews(context.PackageName, R.layout.sample_panel);
			string str = context.Resources.getString(R.@string.vertical_text);
			rv.setTextViewText(R.id.text, str);
			setPendingIntent(context, rv);
			for (int i = 0; i < cocktailIds.Length; i++)
			{
				cocktailBarManager.updateCocktail(cocktailIds[i], rv);
			}
		}

		private void setPendingIntent(Context context, RemoteViews rv)
		{
			setPendingIntent(context, R.id.btn_phone, new Intent(Intent.ACTION_DIAL), rv);
			setPendingIntent(context, R.id.btn_camera, new Intent("android.media.action.IMAGE_CAPTURE"), rv);
			setPendingIntent(context, R.id.btn_internet, new Intent(Intent.ACTION_VIEW, Uri.parse("http://www.google.com")), rv);
		}

		private void setPendingIntent(Context context, int rscId, Intent intent, RemoteViews rv)
		{
			PendingIntent itemClickPendingIntent = PendingIntent.getActivity(context, 0, intent, PendingIntent.FLAG_UPDATE_CURRENT);
			rv.setOnClickPendingIntent(rscId, itemClickPendingIntent);
		}

	}

}