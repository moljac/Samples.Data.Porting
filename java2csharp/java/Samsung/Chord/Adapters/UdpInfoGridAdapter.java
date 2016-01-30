
package com.samsung.android.sdk.chord.example.adapter;

import java.util.ArrayList;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.ImageView;
import android.widget.TextView;

import com.samsung.android.sdk.chord.example.R;

public class UdpInfoGridAdapter extends ArrayAdapter<Item> {

    LayoutInflater mInflater;

    int layoutResourceId;

    ArrayList<Item> data = new ArrayList<Item>();

    public UdpInfoGridAdapter(Context context, LayoutInflater inflater, ArrayList<Item> data) {
        super(context, 0, data);

        this.mInflater = inflater;
        this.data = data;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {
        View row = convertView;
        RecordHolder holder = null;

        if (row == null) {
            row = mInflater.inflate(R.layout.row_grid, null);
            holder = new RecordHolder();
            holder.txtTitle = (TextView) row.findViewById(R.id.item_text);
            holder.imageItem = (ImageView) row.findViewById(R.id.item_image);
            row.setTag(holder);
        } else {
            holder = (RecordHolder) row.getTag();
        }

        Item item = data.get(position);
        holder.txtTitle.setText(item.getText());
        holder.imageItem.setImageBitmap(item.getImage());

        return row;

    }

    static class RecordHolder {
        TextView txtTitle;

        ImageView imageItem;
    }

}