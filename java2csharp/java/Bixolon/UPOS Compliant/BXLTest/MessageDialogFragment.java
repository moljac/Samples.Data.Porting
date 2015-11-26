package com.bxl.postest;

import android.app.AlertDialog;
import android.app.Dialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.support.annotation.NonNull;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.FragmentManager;

public class MessageDialogFragment extends DialogFragment {
	
	private static final String KEY_TITLE = "title";
	private static final String KEY_MESSAGE = "message";
	private static final String TAG = MessageDialogFragment.class.getSimpleName();
	
	static void showDialog(FragmentManager manager, String title, String message) {
		MessageDialogFragment messageDialogFragment = new MessageDialogFragment();
		Bundle args = new Bundle();
		args.putString(KEY_TITLE, title);
		args.putString(KEY_MESSAGE, message);
		messageDialogFragment.setArguments(args);
		messageDialogFragment.show(manager, TAG);
	}
	
	@Override
	@NonNull
	public Dialog onCreateDialog(Bundle savedInstanceState) {
		Bundle args = getArguments();
		String title = args.getString(KEY_TITLE);
		String message = args.getString(KEY_MESSAGE);
		
		AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
		builder.setTitle(title).setMessage(message).setPositiveButton(R.string.ok,
				new DialogInterface.OnClickListener() {
					
					@Override
					public void onClick(DialogInterface dialog, int which) {
						// TODO Auto-generated method stub
						
					}
				});
		
		return builder.create();
	}
}
