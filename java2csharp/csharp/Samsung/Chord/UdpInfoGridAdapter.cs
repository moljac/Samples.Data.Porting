using System.Collections.Generic;

namespace com.samsung.android.sdk.chord.example.adapter
{

	using Context = android.content.Context;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;

	public class UdpInfoGridAdapter : ArrayAdapter<Item>
	{

		internal LayoutInflater mInflater;

		internal int layoutResourceId;

		internal List<Item> data = new List<Item>();

		public UdpInfoGridAdapter(Context context, LayoutInflater inflater, List<Item> data) : base(context, 0, data)
		{

			this.mInflater = inflater;
			this.data = data;
		}

		public override View getView(int position, View convertView, ViewGroup parent)
		{
			View row = convertView;
			RecordHolder holder = null;

			if (row == null)
			{
				row = mInflater.inflate(R.layout.row_grid, null);
				holder = new RecordHolder();
				holder.txtTitle = (TextView) row.findViewById(R.id.item_text);
				holder.imageItem = (ImageView) row.findViewById(R.id.item_image);
				row.Tag = holder;
			}
			else
			{
				holder = (RecordHolder) row.Tag;
			}

			Item item = data[position];
			holder.txtTitle.Text = item.Text;
			holder.imageItem.ImageBitmap = item.Image;

			return row;

		}

		internal class RecordHolder
		{
			internal TextView txtTitle;

			internal ImageView imageItem;
		}

	}
}