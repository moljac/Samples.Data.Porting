package com.bxl.postest;

import java.io.File;
import java.util.Arrays;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.Context;
import android.os.Bundle;
import android.os.Environment;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.FragmentManager;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;

public class FileListDialogFragment extends DialogFragment implements View.OnClickListener, OnItemClickListener {
	
	private static final String TAG = FileListDialogFragment.class.getSimpleName();
	
	static void showDialog(FragmentManager manager) {
		FileListDialogFragment fileListDialogFragment = new FileListDialogFragment();
		fileListDialogFragment.show(manager, TAG);
	}
	
	interface OnClickListener {
		public void onClick(String text);
	}
	
	private OnClickListener onClickListener;
	
	private File[] files;
	private FileAdapter fileAdapter;
	
	private TextView pathTextView;
	private Button okButton;
	
	private String filePath;
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		
		try {
			onClickListener = (OnClickListener) activity;
		} catch (ClassCastException e) {
			e.printStackTrace();
			throw new ClassCastException(activity.toString() + " must implement OnClickListener");
		}
	}
	
	@Override
	public Dialog onCreateDialog(Bundle savedInstanceState) {
		AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());

		LayoutInflater inflater = getActivity().getLayoutInflater();
		View view = inflater.inflate(R.layout.dialog_file_list,
				(ViewGroup) getActivity().findViewById(android.R.id.content).getRootView(), false);
		
		ListView listView = (ListView) view.findViewById(R.id.listView1);
		listView.setOnItemClickListener(this);
		fileAdapter = new FileAdapter(getActivity());
		listView.setAdapter(fileAdapter);
		
		pathTextView = (TextView) view.findViewById(R.id.textViewTitle);
		pathTextView.setOnClickListener(this);
		
		okButton = (Button) view.findViewById(R.id.buttonOk);
		okButton.setOnClickListener(this);
		view.findViewById(R.id.buttonCancel).setOnClickListener(this);
		
		builder.setView(view).setTitle(getString(R.string.open));
		
		updateFileList(new File(Environment.getExternalStorageDirectory().getAbsolutePath()));
		
		return builder.create();
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.textViewTitle:
			String path = pathTextView.getText().toString();
			if (!path.equals(Environment.getExternalStorageDirectory().getAbsolutePath())) {
				String parentPath = path.substring(0, path.lastIndexOf('/'));
				updateFileList(new File(parentPath));
			}
			break;
			
		case R.id.buttonOk:
			dismiss();
			onClickListener.onClick(filePath);
			break;
			
		case R.id.buttonCancel:
			dismiss();
			break;
		}
	}

	@Override
	public void onItemClick(AdapterView<?> parent, View view, int position,
			long id) {
		if (files[position].isDirectory()) {
			updateFileList(files[position]);
		} else {
			view.setSelected(true);
			okButton.setEnabled(true);
			filePath = files[position].getAbsolutePath();
		}
	}
	
	private void updateFileList(File file) {
		files = file.listFiles();
		Arrays.sort(files);
		
		if (pathTextView != null) {
			pathTextView.setText(file.getAbsolutePath());
		}
		
		if (fileAdapter != null) {
			fileAdapter.notifyDataSetInvalidated();
		}
		
		okButton.setEnabled(false);
	}
	
	private class FileAdapter extends BaseAdapter {
		
		final LayoutInflater layoutInflater;
		
		public FileAdapter(Context context) {
			layoutInflater = LayoutInflater.from(context);
		}

		@Override
		public int getCount() {
			if (files != null) {
				return files.length;
			} else {
				return -1;
			}
		}

		@Override
		public Object getItem(int position) {
			return files[position];
		}

		@Override
		public long getItemId(int position) {
			return position;
		}

		@Override
		public View getView(int position, View convertView, ViewGroup parent) {
			if (convertView == null) {
				convertView = layoutInflater.inflate(R.layout.file_list_item, parent, false);
			}
			
			File file = files[position];
			TextView textView = (TextView) convertView.findViewById(R.id.textView1);
			
			if (file.isDirectory()) {
				textView.setCompoundDrawablesWithIntrinsicBounds(R.drawable.ic_directory, 0, 0, 0);
			} else {
				textView.setCompoundDrawablesWithIntrinsicBounds(R.drawable.ic_file, 0, 0, 0);
			}
			
			textView.setText(file.getName());
			
			return convertView;
		}
		
	}
}
