package com.bxl.postest;

import jpos.JposException;
import android.os.Bundle;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Spinner;
import android.widget.TextView;

public class POSPrinterDirectIOFragment extends POSPrinterFragment implements OnClickListener {
	
	private static final String[] DATA_STRING = {
		"0x10 0x04 0x02",
		"0x10 0x04 0x04",
		"0x1d 0x49 0x41",
		"0x1d 0x49 0x42",
		"0x1d 0x49 0x43",
		"0x1d 0x49 0x45"
	};
	
	private static final byte[][] DATA = {
		{0x10, 0x04, 0x02},
		{0x10, 0x04, 0x04},
		{0x1d, 0x49, 0x41},
		{0x1d, 0x49, 0x42},
		{0x1d, 0x49, 0x43},
		{0x1d, 0x49, 0x45},
	};
	
	private Spinner dataSpinner;
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container,
			Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_direct_io, container, false);
		
		view.findViewById(R.id.buttonDirectIO).setOnClickListener(this);
		view.findViewById(R.id.buttonBatteryStatus).setOnClickListener(this);
		
		dataSpinner = (Spinner) view.findViewById(R.id.spinnerData);
		ArrayAdapter<CharSequence> adapter = new ArrayAdapter<CharSequence>(getActivity(),
				android.R.layout.simple_spinner_item, DATA_STRING);
		adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
		dataSpinner.setAdapter(adapter);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		
		return view;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonDirectIO:
			int selectedItemPosition = dataSpinner.getSelectedItemPosition();
			
			try {
				posPrinter.directIO(1, null, DATA[selectedItemPosition]);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
			}
			break;
			
		case R.id.buttonBatteryStatus:
			try {
				posPrinter.directIO(2, null, null);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
			}
			break;
		}
	}
}
