package com.bxl.postest;

import jpos.JposException;
import jpos.POSPrinterConst;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

public class POSPrinterPageModeFragment extends POSPrinterFragment implements View.OnClickListener {
	
	private TextView pageModeAreaTextView;
	private EditText pageModeHorizontalPositionEditText;
	private EditText pageModeVerticalPositionEditText;
	private EditText pageModePrintAreaEditText1;
	private EditText pageModePrintAreaEditText2;
	private EditText pageModePrintAreaEditText3;
	private EditText pageModePrintAreaEditText4;
	private Spinner pageModePrintDirectionSpinner;
	private Spinner pageModeCommandSpinner;

	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_page_mode, container, false);
		
		pageModeAreaTextView = (TextView) view.findViewById(R.id.textViewPageModeArea);
		try {
			pageModeAreaTextView.setText(posPrinter.getPageModeArea());
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		pageModeHorizontalPositionEditText = (EditText) view.findViewById(R.id.editTextPageModeHorizontalPosition);
		try {
			pageModeHorizontalPositionEditText.setText(Integer.toString(posPrinter.getPageModeHorizontalPosition()));
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		pageModeVerticalPositionEditText = (EditText) view.findViewById(R.id.editTextPageModeVerticalPosition);
		try {
			pageModeVerticalPositionEditText.setText(Integer.toString(posPrinter.getPageModeVerticalPosition()));
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		pageModePrintAreaEditText1 = (EditText) view.findViewById(R.id.editTextPageModePrintArea1);
		pageModePrintAreaEditText2 = (EditText) view.findViewById(R.id.editTextPageModePrintArea2);
		pageModePrintAreaEditText3 = (EditText) view.findViewById(R.id.editTextPageModePrintArea3);
		pageModePrintAreaEditText4 = (EditText) view.findViewById(R.id.editTextPageModePrintArea4);
		try {
			String[] pageModePrintArea = posPrinter.getPageModePrintArea().split(",");
			pageModePrintAreaEditText1.setText(pageModePrintArea[0]);
			pageModePrintAreaEditText2.setText(pageModePrintArea[1]);
			pageModePrintAreaEditText3.setText(pageModePrintArea[2]);
			pageModePrintAreaEditText4.setText(pageModePrintArea[3]);
		} catch (JposException e1) {
			// TODO Auto-generated catch block
			e1.printStackTrace();
		}
		
		pageModePrintDirectionSpinner = (Spinner) view.findViewById(R.id.spinnerPageModePrintDirection);
		try {
			switch (posPrinter.getPageModePrintDirection()) {
			case POSPrinterConst.PTR_PD_LEFT_TO_RIGHT:
				pageModePrintDirectionSpinner.setSelection(0);
				break;
				
			case POSPrinterConst.PTR_PD_BOTTOM_TO_TOP:
				pageModePrintDirectionSpinner.setSelection(1);
				break;
				
			case POSPrinterConst.PTR_PD_RIGHT_TO_LEFT:
				pageModePrintDirectionSpinner.setSelection(2);
				break;
				
			case POSPrinterConst.PTR_PD_TOP_TO_BOTTOM:
				pageModePrintDirectionSpinner.setSelection(3);
				break;
			}
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		pageModeCommandSpinner = (Spinner) view.findViewById(R.id.spinnerPageModeCommand);
		
		view.findViewById(R.id.buttonClearPrintArea).setOnClickListener(this);
		view.findViewById(R.id.buttonUpdateProperties).setOnClickListener(this);
		view.findViewById(R.id.buttonSendPageModeCommand).setOnClickListener(this);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		return view;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonClearPrintArea:
			clearPrintArea();
			break;
			
		case R.id.buttonUpdateProperties:
			updateProperties();
			break;
			
		case R.id.buttonSendPageModeCommand:
			sendPageModeCommand();
			break;
		}
	}
	
	private void clearPrintArea() {
		try {
			posPrinter.clearPrintArea();
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
	
	private void updateProperties() {
		try {
			pageModeAreaTextView.setText(posPrinter.getPageModeArea());
			
			String area = pageModePrintAreaEditText1.getText().toString() + ","
					+ pageModePrintAreaEditText2.getText().toString() + ","
					+ pageModePrintAreaEditText3.getText().toString() + ","
					+ pageModePrintAreaEditText4.getText().toString();
			posPrinter.setPageModePrintArea(area);
			
			int direction = getPageModePrintDirection();
			posPrinter.setPageModePrintDirection(direction);
			
			int position = Integer.valueOf(pageModeHorizontalPositionEditText.getText().toString());
			posPrinter.setPageModeHorizontalPosition(position);
			
			position = Integer.valueOf(pageModeVerticalPositionEditText.getText().toString());
			posPrinter.setPageModeVerticalPosition(position);
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
	
	private void sendPageModeCommand() {
		try {
			switch (pageModeCommandSpinner.getSelectedItemPosition()) {
			case 0:
				posPrinter.pageModePrint(POSPrinterConst.PTR_PM_PAGE_MODE);
				break;
				
			case 1:
				posPrinter.pageModePrint(POSPrinterConst.PTR_PM_NORMAL);
				break;
				
			case 2:
				posPrinter.pageModePrint(POSPrinterConst.PTR_PM_CANCEL);
				break;
			}
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
	
	private int getPageModePrintDirection() {
		switch (pageModePrintDirectionSpinner.getSelectedItemPosition()) {
		case 0:
			return POSPrinterConst.PTR_PD_LEFT_TO_RIGHT;
			
		case 1:
			return POSPrinterConst.PTR_PD_BOTTOM_TO_TOP;
			
		case 2:
			return POSPrinterConst.PTR_PD_RIGHT_TO_LEFT;
			
		case 3:
			return POSPrinterConst.PTR_PD_TOP_TO_BOTTOM;
			
			default:
				return -1;
		}
	}
}
