package com.bixolon.printersample;

import java.io.File;
import java.util.ArrayList;
import java.util.Arrays;

import android.app.ListActivity;
import android.content.Intent;
import android.os.Bundle;
import android.os.Environment;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

public class FileExplorerActivity extends ListActivity {
	private ArrayAdapter<String> mAdapter;
	private ArrayList<String> mArrayList = new ArrayList<String>();
	
	private TextView mTextView;
	private String mCurrentPath;
	
	private static final String PARENT_DIRECTORY_PATH = "..";
	private static final String DIRECTORY_DELIMITER = "/";

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_file_explorer);
		
		mCurrentPath = Environment.getExternalStorageDirectory().getPath() + DIRECTORY_DELIMITER;
		mTextView = (TextView) findViewById(R.id.textView1);
		mTextView.setText(mCurrentPath);

		mAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, mArrayList);
		setListAdapter(mAdapter);
		
		createDirectoryList(new File(mCurrentPath));
	}
	
	@Override
	public void onListItemClick(ListView l, View v, int position, long id) {
		String path = mCurrentPath + mArrayList.get(position);
		File file = new File(path);
		
		if (file.isDirectory()) {
			if (path.indexOf(PARENT_DIRECTORY_PATH) > 0) {
				mCurrentPath = file.getParentFile().getParent() + DIRECTORY_DELIMITER;
			} else {
				mCurrentPath = path;
			}
			createDirectoryList(new File(mCurrentPath));
			
			mTextView.setText(mCurrentPath);
			mAdapter.notifyDataSetChanged();
		} else {
			if (!path.endsWith(".fls") && !path.endsWith(".bin")) {
				Toast.makeText(this, "Must choose .fls or .bin file", Toast.LENGTH_SHORT).show();
			} else {
				Intent data = new Intent();
				data.putExtra(MainActivity.FIRMWARE_FILE_NAME, path);
				setResult(MainActivity.RESULT_CODE_SELECT_FIRMWARE, data);
				finish();
			}
		}
	}
	
	private void createDirectoryList(File file) {
		mArrayList.clear();
		mArrayList.add(PARENT_DIRECTORY_PATH);
		File[] files = file.listFiles();
		if (files != null) {
			Arrays.sort(files);
			
			for (int i = 0; i < files.length; i++) {
				if (files[i].isDirectory()) {
					mArrayList.add(files[i].getName() + DIRECTORY_DELIMITER);
				} else {
					mArrayList.add(files[i].getName());
				}
			}
		}
	}
}
