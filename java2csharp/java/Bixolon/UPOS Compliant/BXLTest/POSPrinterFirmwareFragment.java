package com.bxl.postest;

import jpos.JposException;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.TextView;


public class POSPrinterFirmwareFragment extends POSPrinterFragment
		implements View.OnClickListener, FileListDialogFragment.OnClickListener {
	
	private EditText fileNameEditText;
	
	@Override
	public View onCreateView(LayoutInflater inflater, ViewGroup container,
			Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_firmware, container, false);
		
		fileNameEditText = (EditText) view.findViewById(R.id.editTextFileName);
		
		view.findViewById(R.id.buttonBrowse).setOnClickListener(this);
		view.findViewById(R.id.buttonGo).setOnClickListener(this);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		
		return view;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonBrowse:
			FileListDialogFragment.showDialog(getFragmentManager());
			break;
			
		case R.id.buttonGo:
			try {
				posPrinter.updateFirmware(fileNameEditText.getText().toString());
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
		}
	}
	
	public void onClick(String text) {
		fileNameEditText.setText(text);
	}
}
