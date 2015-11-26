package com.bxl.postest;

import com.bxl.util.BXLUtility;

import jpos.JposConst;
import jpos.JposException;
import jpos.SmartCardRW;
import jpos.SmartCardRWConst;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.support.annotation.Nullable;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.TextView;

public class SmartCardRWFragment extends Fragment
		implements View.OnClickListener, CompoundButton.OnCheckedChangeListener, RadioGroup.OnCheckedChangeListener {
	
	private SmartCardRW smartCardRW;
	
	private EditText logicalNameEditText;
	private TextView stateTextView;
	
	private TextView readDataTextView;
	
	private CountDownTimer countDownTimer;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		smartCardRW = new SmartCardRW();
	}
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_smart_card_rw, container, false);
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
		logicalNameEditText.setText(settings.getString(MainActivity.KEY_LOGICAL_NAME_SMART_CARD_RW, getString(R.string.smart_card_rw)));
		
		view.findViewById(R.id.buttonOpen).setOnClickListener(this);
		view.findViewById(R.id.buttonClaim).setOnClickListener(this);
		view.findViewById(R.id.buttonRelease).setOnClickListener(this);
		view.findViewById(R.id.buttonClose).setOnClickListener(this);
		view.findViewById(R.id.buttonInfo).setOnClickListener(this);
		view.findViewById(R.id.buttonCheckHealth).setOnClickListener(this);
		view.findViewById(R.id.buttonReadData).setOnClickListener(this);
		
		CheckBox checkBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
		checkBox.setOnCheckedChangeListener(this);
		
		RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
		radioGroup.setOnCheckedChangeListener(this);
		radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup2);
		radioGroup.setOnCheckedChangeListener(this);
		
		readDataTextView = (TextView) view.findViewById(R.id.textViewReadData);
		stateTextView = (TextView) view.findViewById(R.id.textViewState);
		return view;
	}
	
	@Override
	public void onDestroyView() {
		super.onDestroyView();
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(MainActivity.KEY_LOGICAL_NAME_SMART_CARD_RW, logicalNameEditText.getText().toString());
		editor.commit();
	}
	
	@Override
	public void onResume() {
		super.onResume();
		
		countDownTimer = new CountDownTimer(Long.MAX_VALUE, 1000) {
			
			@Override
			public void onTick(long millisUntilFinished) {
				stateTextView.setText(MainActivity.getStatusString(smartCardRW.getState()));
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
	public void onDestroy() {
		super.onDestroy();
		
		try {
			smartCardRW.close();
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	@Override
	public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
		try {
			if (buttonView.getId() == R.id.checkBoxDeviceEnabled) {
				smartCardRW.setDeviceEnabled(isChecked);
				if (isChecked) {
					smartCardRW.setSCSlot(0x01 << (Integer.SIZE - 1));
				}
			}
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
		}
	}
	
	@Override
	public void onCheckedChanged(RadioGroup group, int checkedId) {
		try {
			switch (checkedId) {
			case R.id.radioSmartCard:
				smartCardRW.setSCSlot(0x01 << (Integer.SIZE - 1));
				break;
				
			case R.id.radioSam1:
				smartCardRW.setSCSlot(0x01 << (Integer.SIZE - 2));
				break;
				
			case R.id.radioSam2:
				smartCardRW.setSCSlot(0x01 << (Integer.SIZE - 3));
				break;
				
			case R.id.radioApdu:
				smartCardRW.setIsoEmvMode(SmartCardRWConst.SC_CMODE_EMV);
				break;
				
			case R.id.radioTpdu:
				smartCardRW.setIsoEmvMode(SmartCardRWConst.SC_CMODE_ISO);
				break;
			}
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
		}
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonOpen:
			String logicalDeviceName = logicalNameEditText.getText().toString();
			try {
				smartCardRW.open(logicalDeviceName);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClaim:
			try {
				smartCardRW.claim(0);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonRelease:
			try {
				smartCardRW.release();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClose:
			try {
				smartCardRW.close();
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
			
		case R.id.buttonReadData:
			String[] data = new String[] {
					new String(new byte[] {
							0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38,
							0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40})
			};
			int[] count = new int[1];
			
			try {
				smartCardRW.readData(SmartCardRWConst.SC_READ_DATA, count, data);
				readDataTextView.setText(BXLUtility.toHexString(data[0].getBytes()));
			} catch (JposException e) {
				e.printStackTrace();
				readDataTextView.setText("");
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
		}
	}
	
	private void info() {
		try {
			String message = "deviceServiceDescription: " + smartCardRW.getDeviceServiceDescription()
					+ "\ndeviceServiceVersion: " + smartCardRW.getDeviceServiceVersion()
					+ "\nphysicalDeviceDescription: " + smartCardRW.getPhysicalDeviceDescription()
					+ "\nphysicalDeviceName: " + smartCardRW.getPhysicalDeviceName()
					+ "\npowerState: " + smartCardRW.getPowerState()
					+ "\ninterfaceMode: " + smartCardRW.getInterfaceMode()
					+ "\nisoEmvMode: " + smartCardRW.getIsoEmvMode()
					+ "\ntransactionInProgress: " + smartCardRW.getTransactionInProgress()
					+ "\ntransmissionProtocol: " + smartCardRW.getTransmissionProtocol();
			MessageDialogFragment.showDialog(getFragmentManager(), "Info", message);
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
		}
	}
	
	private void checkHealth() {
		try {
			smartCardRW.checkHealth(JposConst.JPOS_CH_INTERNAL);
			MessageDialogFragment.showDialog(getFragmentManager(), "Info", smartCardRW.getCheckHealthText());
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
		}
	}
}
