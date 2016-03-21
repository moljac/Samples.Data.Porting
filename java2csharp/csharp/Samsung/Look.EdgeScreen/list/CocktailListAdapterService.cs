namespace com.example.edgescreen.list
{

	using Intent = android.content.Intent;
	using RemoteViewsService = android.widget.RemoteViewsService;

	public class CocktailListAdapterService : RemoteViewsService
	{

		public override RemoteViewsFactory onGetViewFactory(Intent intent)
		{
			return new CocktailListAdapterFactory(this.ApplicationContext);
		}
	}

}