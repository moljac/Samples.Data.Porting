package com.example.com.generalscan.usb.broadcast;

import android.app.ListActivity;
import android.os.Bundle;
import android.provider.Settings.System;
import android.view.View;
import android.widget.ListView;

public class AppListActivity extends ListActivity{

	AppListAdapter mAppListAdapter ;
	AppFilter mAppFilter;
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		 mAppFilter = new AppFilter(this);
		AppListAdapter mAppListAdapter = new AppListAdapter(this,mAppFilter.getAppList());
		this.setListAdapter(mAppListAdapter);
	}
	@Override
	protected void onListItemClick(ListView l, View v, int position, long id) {
		super.onListItemClick(l, v, position, id);
		String packetName = mAppFilter.getAppList().get(position).getPackageName();
		AppFilter.startAPP(this,packetName);
		USBSaveShared.SavePacketName(this, packetName);
		USBSaveShared.SaveAppName(this, mAppFilter.getAppList().get(position).getName());
		this.setResult(1);
		this.finish();
	}

	
}
