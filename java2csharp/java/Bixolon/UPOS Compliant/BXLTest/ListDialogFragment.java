package com.bxl.postest;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.FragmentManager;

public class ListDialogFragment extends DialogFragment {
	
	private static final String KEY_TITLE = "title";
	private static final String KEY_ITEMS = "items";
	private static final String TAG = ListDialogFragment.class.getSimpleName();
	
	interface OnClickListener {
		void onClick(String title, String text);
	}
	
	private OnClickListener onClickListener;
	
	static void showDialog(FragmentManager manager, String title, String[] items) {
		ListDialogFragment listDialogFragment = new ListDialogFragment();
		Bundle args = new Bundle();
		args.putString(KEY_TITLE, title);
		args.putStringArray(KEY_ITEMS, items);
		listDialogFragment.setArguments(args);
		listDialogFragment.show(manager, TAG);
	}
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		
		try {
			onClickListener = (OnClickListener) activity;
		} catch (ClassCastException e) {
			e.printStackTrace();
			throw new ClassCastException(activity.toString()
					+ " must implement ListDialogFragment.OnClickListener");
		}
	}

	@Override
	@NonNull
	public Dialog onCreateDialog(Bundle savedInstanceState) {
		Bundle args = getArguments();
		final String title = args.getString(KEY_TITLE);
		final String[] items = args.getStringArray(KEY_ITEMS);
		
		AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
		builder.setTitle(title).setItems(items, new DialogInterface.OnClickListener() {

			@Override
			public void onClick(DialogInterface dialog, int which) {
				onClickListener.onClick(title, items[which]);
			}
		});
		return builder.create();
	}
}
