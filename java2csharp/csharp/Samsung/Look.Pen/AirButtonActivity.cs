using System.Collections.Generic;

namespace com.samsung.android.example.slookdemos
{


	using ListActivity = android.app.ListActivity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ListView = android.widget.ListView;
	using SimpleAdapter = android.widget.SimpleAdapter;

	public class AirButtonActivity : ListActivity
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ListAdapter = new SimpleAdapter(this, Data, android.R.layout.simple_list_item_1, new string[] {"title"}, new int[] {android.R.id.text1});
			ListView.TextFilterEnabled = true;
		}

		private IList<IDictionary<string, object>> Data
		{
			get
			{
				IList<IDictionary<string, object>> myData = new List<IDictionary<string, object>>();
    
				addItem(myData, "1. Simple", activityIntent("com.samsung.android.example.slookdemos.AirButtonSimpleActivity"));
				addItem(myData, "2. Default Adapter", activityIntent("com.samsung.android.example.slookdemos.AirButtonDefaultActivity"));
				addItem(myData, "3. SurfaceView", activityIntent("com.samsung.android.example.slookdemos.AirButtonSurfaceViewActivity"));
				addItem(myData, "4. SurfaceView-Manaual", activityIntent("com.samsung.android.example.slookdemos.AirButtonManualActivity"));
				return myData;
			}
		}

		private Intent activityIntent(string componentName)
		{
			Intent result = new Intent();
			result.setClassName(this, componentName);
			return result;
		}

		private void addItem(IList<IDictionary<string, object>> data, string name, object intent)
		{
			IDictionary<string, object> item = new Dictionary<string, object>();
			item["title"] = name;
			item["intent"] = intent;
			data.Add(item);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") protected void onListItemClick(android.widget.ListView l, android.view.View v, int position, long id)
		protected internal override void onListItemClick(ListView l, View v, int position, long id)
		{
			IDictionary<string, object> map = (IDictionary<string, object>) l.getItemAtPosition(position);
			Intent intent = (Intent) map["intent"];
			startActivity(intent);
		}

	}

}