package com.samsung.android.sdk.sample.mediabrowser;


import android.content.Context;
import android.graphics.Bitmap;
import android.net.Uri;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;
import com.samsung.android.sdk.mediacontrol.SmcItem;

import java.text.DateFormat;
import java.util.ArrayList;
import java.util.List;

public class ItemAdapter extends BaseAdapter {
    private static final int KILOBYTE = 1024;
    private static final int MEGABYTE = KILOBYTE * KILOBYTE;
    private static final int GIGABYTE = MEGABYTE * KILOBYTE;

    private LayoutInflater inflater;
    private List<SmcItem> items;
    // private SimpleDateFormat dateFormat; // lint
    private DateFormat dateFormat;

    public ItemAdapter(Context context) {
        inflater = LayoutInflater.from(context);
        items = new ArrayList<SmcItem>();
        // dateFormat = new SimpleDateFormat("dd-MM-yyyy"); // lint
        dateFormat = DateFormat.getDateInstance();
    }

    public void add(List<SmcItem> items) {
        this.items.addAll(items);
        notifyDataSetChanged();
    }

    public void clear() {
        items.clear();
        notifyDataSetChanged();
    }

    @Override
    public int getCount() {
        return items.size();
    }

    @Override
    public SmcItem getItem(int position) {
        return items.get(position);
    }

    @Override
    public long getItemId(int position) {
        return position;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        ImageView icon;
        TextView name, size, date;
        if (convertView == null) {
            convertView = inflater.inflate(R.layout.item, null);
            icon = (ImageView) convertView.findViewById(R.id.icon);
            name = (TextView) convertView.findViewById(R.id.name);
            size = (TextView) convertView.findViewById(R.id.size);
            date = (TextView) convertView.findViewById(R.id.date);
            convertView.setTag(R.id.icon, icon);
            convertView.setTag(R.id.name, name);
            convertView.setTag(R.id.size, size);
            convertView.setTag(R.id.date, date);
        } else {
            icon = (ImageView) convertView.getTag(R.id.icon);
            name = (TextView) convertView.getTag(R.id.name);
            size = (TextView) convertView.getTag(R.id.size);
            date = (TextView) convertView.getTag(R.id.date);
        }
        SmcItem item = getItem(position);
        //set title
        name.setText(item.getTitle());
        //set size
        if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_FOLDER)
            size.setText("<DIR>");
        else
            size.setText(getSize(item.getFileSize()));
        //set date
        if (item.getDate() != null)
            date.setText(dateFormat.format(item.getDate()));
        else
            date.setText(null);
        //load icon
        Uri iconPath = item.getThumbnail();
        icon.setTag(iconPath);
        if (iconPath != null) {
            Bitmap b = IconCache.getInstance().get(iconPath);
            if (b == null) {
                // Clear the image so we don't display stale icon.
                icon.setImageResource(getIconForItem(item));
                new IconLoader(icon).execute(iconPath);
            } else {
                icon.setImageBitmap(b);
            }
        } else {
            icon.setImageResource(getIconForItem(item));
        }
        return convertView;
    }


    /**
     * Gets an icon resource for specific item type
     *
     * @param item an item
     * @return icon resource id
     */
    private int getIconForItem(SmcItem item) {
        switch (item.getMediaType()) {
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

    /**
     * Convert size in bytes to string representation
     *
     * @param size size in bytes
     * @return size as string
     */
    private static String getSize(long size) {
        if (size < 0)
            return "";
        else if (size < KILOBYTE)
            return size + " B";
        else if (size < MEGABYTE)
            return approx((double) size / KILOBYTE, 2) + " kB";
        else if (size < GIGABYTE)
            return approx((double) size / MEGABYTE, 2) + " MB";
        else
            return approx((double) size / GIGABYTE, 2) + " GB";
    }

    /**
     * Approximate a number with specified precision. For example: approx(2.34278, 2) returns 2.34
     *
     * @param num       number to approximate
     * @param precision number of digits after the dot
     * @return approximated number
     */
    private static float approx(double num, int precision) {
        double factor = Math.pow(10, precision);
        return (float) (((int) (num * factor)) / (factor));
    }
}
