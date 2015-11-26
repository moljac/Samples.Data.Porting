package com.bxl.postest;

import jpos.JposConst;
import jpos.JposException;
import jpos.POSPrinterConst;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.support.annotation.Nullable;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.TextView;

public class POSPrinterCommonFragment extends POSPrinterFragment
		implements View.OnClickListener, CompoundButton.OnCheckedChangeListener {
	
	private EditText logicalNameEditText;
	private TextView stateTextView;
	
	private CheckBox deviceEnabledCheckBox;
	private CheckBox asyncModeCheckBox;
	
	private CountDownTimer countDownTimer;
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_common, container, false);
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
		logicalNameEditText.setText(settings.getString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER,
				getString(R.string.pos_printer)));
		
		view.findViewById(R.id.buttonOpen).setOnClickListener(this);
		view.findViewById(R.id.buttonClaim).setOnClickListener(this);
		view.findViewById(R.id.buttonRelease).setOnClickListener(this);
		view.findViewById(R.id.buttonClose).setOnClickListener(this);
		view.findViewById(R.id.buttonInfo).setOnClickListener(this);
		view.findViewById(R.id.buttonCheckHealth).setOnClickListener(this);
		
		deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
		deviceEnabledCheckBox.setOnCheckedChangeListener(this);
		asyncModeCheckBox = (CheckBox) view.findViewById(R.id.checkBoxAsyncMode);
		asyncModeCheckBox.setOnCheckedChangeListener(this);
		try {
			deviceEnabledCheckBox.setChecked(posPrinter.getDeviceEnabled());
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		try {
			asyncModeCheckBox.setChecked(posPrinter.getAsyncMode());
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
		
		stateTextView = (TextView) view.findViewById(R.id.textViewState);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		return view;
	}
	
	@Override
	public void onDestroyView() {
		super.onDestroyView();
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER, logicalNameEditText.getText().toString());
		editor.commit();
	}
	
	@Override
	public void onResume() {
		super.onResume();
		
		countDownTimer = new CountDownTimer(Long.MAX_VALUE, 1000) {
			
			@Override
			public void onTick(long millisUntilFinished) {
				stateTextView.setText(MainActivity.getStatusString(posPrinter.getState()));
			}
			
			@Override
			public void onFinish() {
				// TODO Auto-generated method stub
				
			}
		}.start();
	}
	
	@Override
	public void onPause() {
		super.onPause();
		
		countDownTimer.cancel();
	}
	
	@Override
	public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
		switch (buttonView.getId()) {
		case R.id.checkBoxDeviceEnabled:
			try {
				posPrinter.setDeviceEnabled(isChecked);
			} catch (JposException e) {
				e.printStackTrace();
				try {
					posPrinter.setDeviceEnabled(!isChecked);
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
			}
			break;
			
		case R.id.checkBoxAsyncMode:
			try {
				posPrinter.setAsyncMode(isChecked);
			} catch (JposException e) {
				e.printStackTrace();
				try {
					posPrinter.setAsyncMode(!isChecked);
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
			}
			break;
		}
		
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonOpen:
			String logicalDeviceName = logicalNameEditText.getText().toString();
			try {
				posPrinter.open(logicalDeviceName);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClaim:
			try {
				posPrinter.claim(0);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonRelease:
			try {
				posPrinter.release();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClose:
			try {
				posPrinter.close();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonInfo:
			info();
			break;
			
		case R.id.buttonCheckHealth:
			checkHealth();
			break;
		}
	}
	
	private void info() {
		String message;
		try {
			message = "deviceServiceDescription: " + posPrinter.getDeviceServiceDescription()
					+ "\ndeviceServiceVersion: " + posPrinter.getDeviceServiceVersion()
					+ "\nphysicalDeviceDescription: " + posPrinter.getPhysicalDeviceDescription()
					+ "\nphysicalDeviceName: " + posPrinter.getPhysicalDeviceName()
					+ "\npowerState: " + MainActivity.getPowerStateString(posPrinter.getPowerState())
					+ "\ncapRecNearEndSensor: " + posPrinter.getCapRecNearEndSensor()
					+ "\nRecPapercut: " + posPrinter.getCapRecPapercut()
					+ "\ncapRecMarkFeed: " + getMarkFeedString(posPrinter.getCapRecMarkFeed())
					+ "\ncharacterSet: " + posPrinter.getCharacterSet()
					+ "\ncharacterSetList: " + posPrinter.getCharacterSetList()
					+ "\nfontTypefaceList: " + posPrinter.getFontTypefaceList()
					+ "\nrecLineChars: " + posPrinter.getRecLineChars()
					+ "\nrecLineCharsList: " + posPrinter.getRecLineCharsList()
					+ "\nrecLineSpacing: " + posPrinter.getRecLineSpacing()
					+ "\nrecLineWidth: " + posPrinter.getRecLineWidth();
			MessageDialogFragment.showDialog(getFragmentManager(), "Info", message);
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception",
					"Exception in Info: "+ e.getMessage());
		}
	}
	
	private String getMarkFeedString(int markFeed) {
		switch (markFeed) {
		case POSPrinterConst.PTR_MF_TO_TAKEUP:
			return "TAKEUP";
			
		case POSPrinterConst.PTR_MF_TO_CUTTER:
			return "CUTTER";
			
		case POSPrinterConst.PTR_MF_TO_CURRENT_TOF:
			return "CURRENT TOF";
			
		case POSPrinterConst.PTR_MF_TO_NEXT_TOF:
			return "NEXT TOF";
			
			default:
				return "Not support";
		}
	}
	
	private void checkHealth() {
		try {
			posPrinter.checkHealth(JposConst.JPOS_CH_INTERNAL);
			posPrinter.checkHealth(JposConst.JPOS_CH_EXTERNAL);
			MessageDialogFragment.showDialog(getFragmentManager(), "checkHealth", posPrinter.getCheckHealthText());
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
}
