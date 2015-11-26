package com.bxl.postest;

import jpos.JposException;
import jpos.POSPrinterConst;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

public class POSPrinterBarCodeFragment extends POSPrinterFragment
		implements View.OnClickListener, AdapterView.OnItemSelectedListener {
	
	private EditText widthEditText;
	private EditText heightEditText;
	
	private Spinner symbologySpinner;
	private Spinner alignmentSpinner;
	private Spinner textPositionSpinner;
	
	private EditText dataEditText;
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_bar_code, container, false);
		
		widthEditText = (EditText) view.findViewById(R.id.editTextWidth);
		heightEditText = (EditText) view.findViewById(R.id.editTextHeight);
		
		symbologySpinner = (Spinner) view.findViewById(R.id.spinnerSymbology);
		symbologySpinner.setOnItemSelectedListener(this);
		
		alignmentSpinner = (Spinner) view.findViewById(R.id.spinnerAlignment);
		textPositionSpinner = (Spinner) view.findViewById(R.id.spinnerTextPosition);
		
		dataEditText = (EditText) view.findViewById(R.id.editTextBarCodeData);
		
		view.findViewById(R.id.buttonPrintBarCode).setOnClickListener(this);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		return view;
	}

	@Override
	public void onClick(View v) {
		String data = dataEditText.getText().toString();
		int symbology = getSymbology();
		
		try {
			int height = 0;
			if (heightEditText.isEnabled()) {
				height = Integer.valueOf(heightEditText.getText().toString());
			}
			
			int width = 0;
			if (widthEditText.isEnabled()) {
				width = Integer.valueOf(widthEditText.getText().toString());
			}
			
			int alignment = getAlignment();
			int textPosition = getTextPosition();
			
			if (symbology == POSPrinterConst.PTR_BCS_MAXICODE) {
				String dummyHeader = new String(new byte[] { '[', ')', '>', 0x1e, '0', '1',
						0x1d, '9', '6', '1', '2', '3', '4', '5', '6', '7', '8',
						'9', 0x1d, '0', '0', '7', 0x1d, '2', '5', '0', 0x1d });
				data = dummyHeader + data;
			}
			
			posPrinter.printBarCode(POSPrinterConst.PTR_S_RECEIPT,
					data, symbology, height, width, alignment, textPosition);
		} catch (NumberFormatException | JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
	
	@Override
	public void onItemSelected(AdapterView<?> parent, View view, int position,
			long id) {
		String[] barCodeData = getResources().getStringArray(R.array.bar_code_data);
		dataEditText.setText(barCodeData[position]);
		switch (position) {
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
		case 5:
		case 6:
		case 7:
		case 8:
			widthEditText.setHint(R.string.bar_code_width_hint);
			widthEditText.setEnabled(true);
			heightEditText.setHint(R.string.bar_code_height_hint);
			heightEditText.setEnabled(true);
			break;
			
		case 9:
			widthEditText.setHint(R.string.pdf417_width_hint);
			widthEditText.setEnabled(true);
			heightEditText.setHint(R.string.pdf417_height_hint);
			heightEditText.setEnabled(true);
			break;
			
		case 10:
			widthEditText.setHint(R.string.qrcode_size_hint);
			widthEditText.setEnabled(true);
			heightEditText.setText("");
			heightEditText.setHint("");
			heightEditText.setEnabled(false);
			break;
			
		case 11:
			widthEditText.setText("");
			widthEditText.setHint("");
			widthEditText.setEnabled(false);
			heightEditText.setText("");
			heightEditText.setHint("");
			heightEditText.setEnabled(false);
			break;
			
		case 12:
			widthEditText.setHint(R.string.datamatrix_size_hint);
			widthEditText.setEnabled(true);
			heightEditText.setText("");
			heightEditText.setHint("");
			heightEditText.setEnabled(false);
			break;
		}
	}

	@Override
	public void onNothingSelected(AdapterView<?> parent) {
		// TODO Auto-generated method stub
		
	}

	private int getSymbology() {
		switch (symbologySpinner.getSelectedItemPosition()) {
		case 0:
			return POSPrinterConst.PTR_BCS_UPCA;
			
		case 1:
			return POSPrinterConst.PTR_BCS_UPCE;
			
		case 2:
			return POSPrinterConst.PTR_BCS_EAN8;
			
		case 3:
			return POSPrinterConst.PTR_BCS_EAN13;
			
		case 4:
			return POSPrinterConst.PTR_BCS_ITF;
			
		case 5:
			return POSPrinterConst.PTR_BCS_Codabar;
			
		case 6:
			return POSPrinterConst.PTR_BCS_Code39;
			
		case 7:
			return POSPrinterConst.PTR_BCS_Code93;
			
		case 8:
			return POSPrinterConst.PTR_BCS_Code128;
			
		case 9:
			return POSPrinterConst.PTR_BCS_PDF417;
			
		case 10:
			return POSPrinterConst.PTR_BCS_QRCODE;
			
		case 11:
			return POSPrinterConst.PTR_BCS_MAXICODE;
			
		case 12:
			return POSPrinterConst.PTR_BCS_DATAMATRIX;
			
			default:
				return -1;
		}
	}
	
	private int getAlignment() {
		switch (alignmentSpinner.getSelectedItemPosition()) {
		case 0:
			return POSPrinterConst.PTR_BC_LEFT;
			
		case 1:
			return POSPrinterConst.PTR_BC_CENTER;
			
		case 2:
			return POSPrinterConst.PTR_BC_RIGHT;
			
			default:
				return -1;
		}
	}
	
	private int getTextPosition() {
		switch (textPositionSpinner.getSelectedItemPosition()) {
		case 0:
			return POSPrinterConst.PTR_BC_TEXT_NONE;
		
		case 1:
			return POSPrinterConst.PTR_BC_TEXT_ABOVE;
			
		case 2:
			return POSPrinterConst.PTR_BC_TEXT_BELOW;
			
			default:
				return -1;
		}
	}
}
