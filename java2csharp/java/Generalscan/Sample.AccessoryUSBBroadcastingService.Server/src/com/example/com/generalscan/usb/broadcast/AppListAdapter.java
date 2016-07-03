package com.example.com.generalscan.usb.broadcast;

import java.util.ArrayList;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.ImageView;
import android.widget.TextView;

public class AppListAdapter extends BaseAdapter {
	private ArrayList<AppInfo> appList;

	private LayoutInflater mLayoutInflater;

	private Context mContext;

	public AppListAdapter(Context context, ArrayList<AppInfo> appList) {
		this.appList = appList;
		mContext = context;
		mLayoutInflater = LayoutInflater.from(context);
	}

	@Override
	public int getCount() {
		// TODO Auto-generated method stub
		return appList.size();
	}

	@Override
	public Object getItem(int arg0) {
		// TODO Auto-generated method stub
		return appList.get(arg0);
	}

	@Override
	public long getItemId(int arg0) {
		// TODO Auto-generated method stub
		return arg0;
	}

	@Override
	public View getView(int arg0, View convertView, ViewGroup arg2) {
		// 获取list_item布局文件的视图
		if (convertView == null) {
			convertView = mLayoutInflater.inflate(R.layout.list_app_item, null);
		}
		// 获取控件对象
		TextView tv = (TextView) convertView.findViewById(R.id.tvAppName);
		ImageView img = (ImageView) convertView.findViewById(R.id.imageView1);
		tv.setText(appList.get(arg0).getName());
		img.setImageDrawable(appList.get(arg0).getDrawable());

		convertView.setTag(appList.get(arg0).getPackageName());
		return convertView;
	}

}
