package com.bxl.postest.settings;

import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.FragmentManager;
import android.text.InputFilter;
import android.text.InputType;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.view.inputmethod.EditorInfo;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.LinearLayout.LayoutParams;
import android.widget.Spinner;
import android.widget.TextView;

import com.bxl.postest.R;

public class EntryDialogFragment extends DialogFragment implements OnItemSelectedListener {
	
	private static final String KEY_TITLE = "title";
	private static final String KEY_ENTRY_INFO = "entry_info";
	
	interface OnClickListener {
		void onClick(boolean isModified, EntryInfo entryInfo);
	}
	
	private OnClickListener onClickListener;
	
	private LinearLayout addressLinearLayout;
	private EditText logicalNameEditText;
	private String delimiter;
	
	private static final int LAYOUT_NONE = 0;
	private static final int LAYOUT_MAC_ADDRESS = 1;
	private static final int LAYOUT_IP_ADDRESS = 2;
	
	static void showDialog(FragmentManager manager, String title, EntryInfo entryInfo) {
		Bundle args = new Bundle();
		args.putString(KEY_TITLE, title);
		args.putParcelable(KEY_ENTRY_INFO, entryInfo);
		
		EntryDialogFragment dialogFragment = new EntryDialogFragment();
		dialogFragment.setArguments(args);
		dialogFragment.show(manager, EntryDialogFragment.class.getSimpleName());
	}
	
	@Override
	public void onAttach(Activity activity) {
		super.onAttach(activity);
		
		try {
			onClickListener = (OnClickListener) activity;
		} catch (ClassCastException e) {
			e.printStackTrace();
			throw new ClassCastException(activity.toString()
					+ " must implement NoticeDialogListener");
		}
	}
	
	@Override
	public Dialog onCreateDialog(Bundle savedInstanceState) {
		EntryInfo entryInfo = getArguments().getParcelable(KEY_ENTRY_INFO);
		final boolean isModifying = (entryInfo != null);
		
		LayoutInflater inflater = getActivity().getLayoutInflater();
		
		View view = inflater.inflate(R.layout.dialog_entry, (ViewGroup) getActivity().findViewById(android.R.id.content).getRootView(), false);
		
		logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
		final Spinner deviceCategorySpinner = (Spinner) view.findViewById(R.id.spinnerDeviceCategory);
		deviceCategorySpinner.setSelection(2);	// POSPrinter
		
		final Spinner productNameSpinner = (Spinner) view.findViewById(R.id.spinnerProductName);
		productNameSpinner.setOnItemSelectedListener(this);
		
		final Spinner deviceBusSpinner = (Spinner) view.findViewById(R.id.spinnerDeviceBus);
		deviceBusSpinner.setOnItemSelectedListener(this);
		addressLinearLayout = (LinearLayout) view.findViewById(R.id.linearLayoutAddress);
		
		if (isModifying) {
			logicalNameEditText.setText(entryInfo.getLogicalName());
			logicalNameEditText.setEnabled(false);
			logicalNameEditText.setInputType(InputType.TYPE_NULL);
			
			String[] deviceCategories = getResources().getStringArray(R.array.device_categories);
			for (int i = 0; i < deviceCategories.length; i++) {
				if (deviceCategories[i].equals(entryInfo.getDeviceCategory())) {
					deviceCategorySpinner.setSelection(i);
					break;
				}
			}
			deviceCategorySpinner.setEnabled(false);
			
			String[] productNames = getResources().getStringArray(R.array.product_name);
			for (int i = 0; i < productNames.length; i++) {
				if (productNames[i].equals(entryInfo.getProductName())) {
					productNameSpinner.setSelection(i);
					break;
				}
			}
			productNameSpinner.setEnabled(false);
			
			String[] deviceBuses = getResources().getStringArray(R.array.device_bus);
			for (int i = 0; i < deviceBuses.length; i++) {
				if (deviceBuses[i].equals(entryInfo.getDeviceBus())) {
					deviceBusSpinner.setSelection(i);
					break;
				}
			}
		}
		
		String title = getArguments().getString(KEY_TITLE);
		AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
		builder.setView(view).setTitle(title).setPositiveButton("OK",
				new DialogInterface.OnClickListener() {
			
					@Override
					public void onClick(DialogInterface dialog, int which) {
						EntryInfo entryInfo = new EntryInfo(
								logicalNameEditText.getText().toString(),
								(String) deviceCategorySpinner.getSelectedItem(),
								(String) productNameSpinner.getSelectedItem(),
								(String) deviceBusSpinner.getSelectedItem(),
								getAddressText());
						onClickListener.onClick(isModifying, entryInfo);
					}
		}).setNegativeButton("Cancel",
				new DialogInterface.OnClickListener() {
			
					@Override
					public void onClick(DialogInterface dialog, int which) {
						// TODO Auto-generated method stub
						
					}
		});
		return builder.create();
	}

	@Override
	public void onItemSelected(AdapterView<?> parent, View view, int position, long id) {
		switch (parent.getId()) {
		case R.id.spinnerDeviceBus:
			switch (position) {
			case 0:	// Bluetooth
			case 4:	// Wi-Fi Direct
				setAddressLayout(LAYOUT_MAC_ADDRESS, true);
				delimiter = ":";
				
				String[] deviceBuses = getResources().getStringArray(R.array.device_bus);
				EntryInfo entryInfo = getArguments().getParcelable(KEY_ENTRY_INFO);
				if (entryInfo != null && entryInfo.getDeviceBus().equals(deviceBuses[position])) {
					setAddressText(entryInfo.getAddress());
				}
				break;
				
			case 1:	// Ethernet
			case 3:	// Wi-Fi
				setAddressLayout(LAYOUT_IP_ADDRESS, false);
				delimiter = ".";
				
				deviceBuses = getResources().getStringArray(R.array.device_bus);
				entryInfo = getArguments().getParcelable(KEY_ENTRY_INFO);
				if (entryInfo != null && entryInfo.getDeviceBus().equals(deviceBuses[position])) {
					setAddressText(entryInfo.getAddress());
				}
				break;
				
			case 2:	// USB
			default:
				setAddressLayout(LAYOUT_NONE, false);
				break;
			}
			break;
			
		case R.id.spinnerProductName:
			EntryInfo entryInfo = getArguments().getParcelable(KEY_ENTRY_INFO);
			if (entryInfo == null) {
				String text = ((TextView) view).getText().toString();
				logicalNameEditText.setText(text);
				logicalNameEditText.setSelection(text.length());
			}
			break;
		}
	}

	@Override
	public void onNothingSelected(AdapterView<?> parent) {
		// TODO Auto-generated method stub
		
	}
	
	private String getAddressText() {
		StringBuffer buffer = new StringBuffer();
		int childCount = addressLinearLayout.getChildCount();
		
		for (int i = 0; i < childCount; i++) {
			EditText editText = (EditText) addressLinearLayout.getChildAt(i);
			buffer.append(editText.getText().toString());
			
			if (i < childCount - 1) {
				buffer.append(delimiter);
			}
		}
		return buffer.toString();
	}
	
	private void setAddressText(String address) {
		int index = 0;
		for (String text : address.split("\\:|\\.")) {
			EditText editText = (EditText) addressLinearLayout.getChildAt(index++);
			if (editText != null) {
				editText.setText(text);
			}
		}
	}
	
	private void setAddressLayout(int layout, boolean inputTypeClassText) {
		addressLinearLayout.removeAllViews();
		int count = 0;
		int maxInputLength = 0;
		
		if (layout == LAYOUT_MAC_ADDRESS) {
			count = 6;
			maxInputLength = 2;
		} else if (layout == LAYOUT_IP_ADDRESS) {
			count = 4;
			maxInputLength = 3;
		}
		
		if (count > 0) {
			for (int i = 0; i < count; i++) {
				EditText editText = new EditText(getActivity());
				if (inputTypeClassText) {
					editText.setInputType(InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD);
				} else {
					editText.setInputType(InputType.TYPE_CLASS_NUMBER);
				}
				
				InputFilter[] filters = new InputFilter[1];
				filters[0] = new InputFilter.LengthFilter(maxInputLength);
				editText.setFilters(filters);
				
				if (i == count - 1) {
					editText.setImeOptions(EditorInfo.IME_ACTION_DONE);
				} else {
					editText.setImeOptions(EditorInfo.IME_ACTION_NEXT);
				}
				
				addressLinearLayout.addView(editText,
						new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT, 1f));
			}
		}
	}
}
