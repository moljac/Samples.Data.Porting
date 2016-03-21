using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.sample.mediabrowser
{


	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using Uri = android.net.Uri;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;
	using SmcItem = com.samsung.android.sdk.mediacontrol.SmcItem;


	public class ItemAdapter : BaseAdapter
	{
		private const int KILOBYTE = 1024;
		private static readonly int MEGABYTE = KILOBYTE * KILOBYTE;
		private static readonly int GIGABYTE = MEGABYTE * KILOBYTE;

		private LayoutInflater inflater;
		private IList<SmcItem> items;
		// private SimpleDateFormat dateFormat; // lint
		private DateFormat dateFormat;

		public ItemAdapter(Context context)
		{
			inflater = LayoutInflater.from(context);
			items = new List<SmcItem>();
			// dateFormat = new SimpleDateFormat("dd-MM-yyyy"); // lint
			dateFormat = DateFormat.DateInstance;
		}

		public virtual void add(IList<SmcItem> items)
		{
			((List<SmcItem>)this.items).AddRange(items);
			notifyDataSetChanged();
		}

		public virtual void clear()
		{
			items.Clear();
			notifyDataSetChanged();
		}

		public override int Count
		{
			get
			{
				return items.Count;
			}
		}

		public override SmcItem getItem(int position)
		{
			return items[position];
		}

		public override long getItemId(int position)
		{
			return position;
		}

		public override View getView(int position, View convertView, ViewGroup parent)
		{
			ImageView icon;
			TextView name, size, date;
			if (convertView == null)
			{
				convertView = inflater.inflate(R.layout.item, null);
				icon = (ImageView) convertView.findViewById(R.id.icon);
				name = (TextView) convertView.findViewById(R.id.name);
				size = (TextView) convertView.findViewById(R.id.size);
				date = (TextView) convertView.findViewById(R.id.date);
				convertView.setTag(R.id.icon, icon);
				convertView.setTag(R.id.name, name);
				convertView.setTag(R.id.size, size);
				convertView.setTag(R.id.date, date);
			}
			else
			{
				icon = (ImageView) convertView.getTag(R.id.icon);
				name = (TextView) convertView.getTag(R.id.name);
				size = (TextView) convertView.getTag(R.id.size);
				date = (TextView) convertView.getTag(R.id.date);
			}
			SmcItem item = getItem(position);
			//set title
			name.Text = item.Title;
			//set size
			if (item.MediaType == SmcItem.MEDIA_TYPE_ITEM_FOLDER)
			{
				size.Text = "<DIR>";
			}
			else
			{
				size.Text = getSize(item.FileSize);
			}
			//set date
			if (item.Date != null)
			{
				date.Text = dateFormat.format(item.Date);
			}
			else
			{
				date.Text = null;
			}
			//load icon
			Uri iconPath = item.Thumbnail;
			icon.Tag = iconPath;
			if (iconPath != null)
			{
				Bitmap b = IconCache.Instance.get(iconPath);
				if (b == null)
				{
					// Clear the image so we don't display stale icon.
					icon.ImageResource = getIconForItem(item);
					(new IconLoader(icon)).execute(iconPath);
				}
				else
				{
					icon.ImageBitmap = b;
				}
			}
			else
			{
				icon.ImageResource = getIconForItem(item);
			}
			return convertView;
		}


		/// <summary>
		/// Gets an icon resource for specific item type
		/// </summary>
		/// <param name="item"> an item </param>
		/// <returns> icon resource id </returns>
		private int getIconForItem(SmcItem item)
		{
			switch (item.MediaType)
			{
				case SmcItem.MEDIA_TYPE_ITEM_AUDIO:
					return R.drawable.ic_musicplayer;
				case SmcItem.MEDIA_TYPE_ITEM_FOLDER:
					return R.drawable.icon_myfiles;
				case SmcItem.MEDIA_TYPE_ITEM_IMAGE:
					return R.drawable.ic_gallery;
				case SmcItem.MEDIA_TYPE_ITEM_VIDEO:
					return R.drawable.ic_video_player;
				default:
					return R.drawable.ic_launcher;
			}
		}

		/// <summary>
		/// Convert size in bytes to string representation
		/// </summary>
		/// <param name="size"> size in bytes </param>
		/// <returns> size as string </returns>
		private static string getSize(long size)
		{
			if (size < 0)
			{
				return "";
			}
			else if (size < KILOBYTE)
			{
				return size + " B";
			}
			else if (size < MEGABYTE)
			{
				return approx((double) size / KILOBYTE, 2) + " kB";
			}
			else if (size < GIGABYTE)
			{
				return approx((double) size / MEGABYTE, 2) + " MB";
			}
			else
			{
				return approx((double) size / GIGABYTE, 2) + " GB";
			}
		}

		/// <summary>
		/// Approximate a number with specified precision. For example: approx(2.34278, 2) returns 2.34
		/// </summary>
		/// <param name="num">       number to approximate </param>
		/// <param name="precision"> number of digits after the dot </param>
		/// <returns> approximated number </returns>
		private static float approx(double num, int precision)
		{
			double factor = Math.Pow(10, precision);
			return (float)(((int)(num * factor)) / (factor));
		}
	}

}