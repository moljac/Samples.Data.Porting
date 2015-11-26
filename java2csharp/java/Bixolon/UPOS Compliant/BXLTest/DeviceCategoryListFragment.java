package com.bxl.postest;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import android.app.Activity;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ExpandableListAdapter;
import android.widget.ExpandableListView;
import android.widget.SimpleExpandableListAdapter;

public class DeviceCategoryListFragment extends Fragment
		implements ExpandableListView.OnChildClickListener, ExpandableListView.OnGroupClickListener {
	
	private static final String KEY_TITLE = "title";
	private static final String KEY_SUBTITLE = "subtitle";
	
	private ExpandableListView expandableListView;
	
	interface OnClickListener {
		boolean onClick(int groupPosition, int childPosition);
	}
	
	private OnClickListener onClickListener;
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		
		try {
			onClickListener = (OnClickListener) activity;
		} catch (ClassCastException e) {
			e.printStackTrace();
		}
	}
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_device_category_list, container, false);
		
		List<Map<String, String>> groupData = new ArrayList<>();
		List<List<Map<String, String>>> childData = new ArrayList<>();
		
		String[] deviceCategory = getResources().getStringArray(R.array.device_categories);
		String[] posPrinterChildren = getResources().getStringArray(R.array.pos_printer_children);
		
		for (int i = 0; i < deviceCategory.length; i++) {
			Map<String, String> groupMap = new HashMap<>();
			groupData.add(groupMap);
			groupMap.put(KEY_TITLE, deviceCategory[i]);
			groupMap.put(KEY_SUBTITLE, "");
			
			if (deviceCategory[i].equals(getString(R.string.pos_printer))) {
				List<Map<String, String>> children = new ArrayList<>();
				
				for (int j = 0; j < posPrinterChildren.length; j++) {
					Map<String, String> childMap = new HashMap<>();
					children.add(childMap);
					childMap.put(KEY_TITLE, posPrinterChildren[j]);
					childMap.put(KEY_SUBTITLE, "");
				}
				childData.add(children);
			} else {
				List<Map<String, String>> children = new ArrayList<>();
				childData.add(children);
			}
		}
		
		String[] from = new String[] {KEY_TITLE, KEY_SUBTITLE};
		int[] to = new int[] {android.R.id.text1, android.R.id.text2};
		ExpandableListAdapter adapter = new SimpleExpandableListAdapter(
				getActivity(), groupData, android.R.layout.simple_expandable_list_item_1, from, to,
				childData, android.R.layout.simple_expandable_list_item_2, from, to);
		
		expandableListView = (ExpandableListView) view;
		expandableListView.setOnGroupClickListener(this);
		expandableListView.setOnChildClickListener(this);
		expandableListView.setAdapter(adapter);
		
		return view;
	}

	@Override
	public boolean onGroupClick(ExpandableListView parent, View v,
			int groupPosition, long id) {
		switch (groupPosition) {
		case 0:
		case 1:
		case 3:
			return onClickListener.onClick(groupPosition, 0);
		}
		return false;
	}

	@Override
	public boolean onChildClick(ExpandableListView parent, View v,
			int groupPosition, int childPosition, long id) {
		if (groupPosition == 2) {
			switch (childPosition) {
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
