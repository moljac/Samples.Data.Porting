using System.Collections.Generic;

namespace com.bxl.postest
{


	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Nullable = android.support.annotation.Nullable;
	using Fragment = android.support.v4.app.Fragment;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using ExpandableListAdapter = android.widget.ExpandableListAdapter;
	using ExpandableListView = android.widget.ExpandableListView;
	using SimpleExpandableListAdapter = android.widget.SimpleExpandableListAdapter;

	public class DeviceCategoryListFragment : Fragment, ExpandableListView.OnChildClickListener, ExpandableListView.OnGroupClickListener
	{

		private const string KEY_TITLE = "title";
		private const string KEY_SUBTITLE = "subtitle";

		private ExpandableListView expandableListView;

		internal interface OnClickListener
		{
			bool onClick(int groupPosition, int childPosition);
		}

		private OnClickListener onClickListener;

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			try
			{
				onClickListener = (OnClickListener) activity;
			}
			catch (System.InvalidCastException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_device_category_list, container, false);

			IList<IDictionary<string, string>> groupData = new List<IDictionary<string, string>>();
			IList<IList<IDictionary<string, string>>> childData = new List<IList<IDictionary<string, string>>>();

			string[] deviceCategory = Resources.getStringArray(R.array.device_categories);
			string[] posPrinterChildren = Resources.getStringArray(R.array.pos_printer_children);

			for (int i = 0; i < deviceCategory.Length; i++)
			{
				IDictionary<string, string> groupMap = new Dictionary<string, string>();
				groupData.Add(groupMap);
				groupMap[KEY_TITLE] = deviceCategory[i];
				groupMap[KEY_SUBTITLE] = "";

				if (deviceCategory[i].Equals(getString(R.@string.pos_printer)))
				{
					IList<IDictionary<string, string>> children = new List<IDictionary<string, string>>();

					for (int j = 0; j < posPrinterChildren.Length; j++)
					{
						IDictionary<string, string> childMap = new Dictionary<string, string>();
						children.Add(childMap);
						childMap[KEY_TITLE] = posPrinterChildren[j];
						childMap[KEY_SUBTITLE] = "";
					}
					childData.Add(children);
				}
				else
				{
					IList<IDictionary<string, string>> children = new List<IDictionary<string, string>>();
					childData.Add(children);
				}
			}

			string[] from = new string[] {KEY_TITLE, KEY_SUBTITLE};
			int[] to = new int[] {android.R.id.text1, android.R.id.text2};
			ExpandableListAdapter adapter = new SimpleExpandableListAdapter(Activity, groupData, android.R.layout.simple_expandable_list_item_1, from, to, childData, android.R.layout.simple_expandable_list_item_2, from, to);

			expandableListView = (ExpandableListView) view;
			expandableListView.OnGroupClickListener = this;
			expandableListView.OnChildClickListener = this;
			expandableListView.Adapter = adapter;

			return view;
		}

		public override bool onGroupClick(ExpandableListView parent, View v, int groupPosition, long id)
		{
			switch (groupPosition)
			{
			case 0:
			case 1:
			case 3:
				return onClickListener.onClick(groupPosition, 0);
			}
			return false;
		}

		public override bool onChildClick(ExpandableListView parent, View v, int groupPosition, int childPosition, long id)
		{
			if (groupPosition == 2)
			{
				switch (childPosition)
				{
				case 0:
				case 1:
				case 2:
				case 3:
				case 4:
				case 5:
					return onClickListener.onClick(groupPosition, childPosition);
				}
			}
			return false;
		}
	}

}