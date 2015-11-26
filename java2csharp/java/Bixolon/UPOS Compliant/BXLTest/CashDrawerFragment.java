package com.bxl.postest;

import jpos.CashDrawer;
import jpos.CashDrawerConst;
import jpos.JposConst;
import jpos.JposException;
import jpos.events.StatusUpdateEvent;
import jpos.events.StatusUpdateListener;
import android.content.Context;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.os.CountDownTimer;
import android.support.annotation.Nullable;
import android.support.v4.app.Fragment;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.TextView;

public class CashDrawerFragment extends Fragment
		implements View.OnClickListener, CompoundButton.OnCheckedChangeListener, StatusUpdateListener {
	
	private CashDrawer cashDrawer;
	
	private EditText logicalNameEditText;
	private TextView stateTextView;
	private TextView deviceMessagesTextView;
	
	private CheckBox deviceEnabledCheckBox;
	
	private CountDownTimer countDownTimer;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		cashDrawer = new CashDrawer();
		cashDrawer.addStatusUpdateListener(this);
	}
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_cash_drawer, container, false);
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
		logicalNameEditText.setText(settings.getString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER, getString(R.string.cash_drawer)));
		
		view.findViewById(R.id.buttonOpen).setOnClickListener(this);
		view.findViewById(R.id.buttonClaim).setOnClickListener(this);
		view.findViewById(R.id.buttonRelease).setOnClickListener(this);
		view.findViewById(R.id.buttonClose).setOnClickListener(this);
		view.findViewById(R.id.buttonInfo).setOnClickListener(this);
		view.findViewById(R.id.buttonCheckHealth).setOnClickListener(this);
		
		deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
		deviceEnabledCheckBox.setOnCheckedChangeListener(this);
		
		view.findViewById(R.id.buttonOpenDrawer).setOnClickListener(this);
		view.findViewById(R.id.buttonGetDrawerOpened).setOnClickListener(this);
		
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
		editor.putString(MainActivity.KEY_LOGICAL_NAME_CASH_DRAWER, logicalNameEditText.getText().toString());
		editor.commit();
	}
	
	@Override
	public void onResume() {
		super.onResume();
		
		countDownTimer = new CountDownTimer(Long.MAX_VALUE, 1000) {
			
			@Override
			public void onTick(long millisUntilFinished) {
				stateTextView.setText(MainActivity.getStatusString(cashDrawer.getState()));
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
			cashDrawer.close();
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	@Override
	public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
		try {
			cashDrawer.setDeviceEnabled(isChecked);
		} catch (JposException e) {
			e.printStackTrace();
			try {
				cashDrawer.setDeviceEnabled(!isChecked);
			} catch (JposException e1) {
				// TODO Auto-generated catch block
				e1.printStackTrace();
			}
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonOpen:
			String logicalDeviceName = logicalNameEditText.getText().toString();
			try {
				cashDrawer.open(logicalDeviceName);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			
			try {
				deviceEnabledCheckBox.setChecked(cashDrawer.getDeviceEnabled());
			} catch (JposException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			break;
			
		case R.id.buttonClaim:
			try {
				cashDrawer.claim(0);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonRelease:
			try {
				cashDrawer.release();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClose:
			try {
				cashDrawer.close();
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
			
		case R.id.buttonOpenDrawer:
			try {
				cashDrawer.openDrawer();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonGetDrawerOpened:
			try {
				if (cashDrawer.getDrawerOpened()) {
					deviceMessagesTextView.append("Cash drawer is open.\n");
				} else {
					deviceMessagesTextView.append("Cash drawer is closed.\n");
				}
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
		}
	}
	
	@Override
	public void statusUpdateOccurred(StatusUpdateEvent e) {
		String text = "Status Update Event: ";
		switch(e.getStatus()){
		case CashDrawerConst.CASH_SUE_DRAWERCLOSED:
			text += "Drawer Closed\n";
			break;
			
		case CashDrawerConst.CASH_SUE_DRAWEROPEN:
			text += "Drawer Opened\n";
			break;
			
		default:
			text += "Unknown Status: " + e.getStatus() + "\n";
			break;
        }
		deviceMessagesTextView.append(text);
	}
	
	private void info() {
		String message;
		try {
			message = "deviceServiceDescription: " + cashDrawer.getDeviceServiceDescription()
					+ "\ndeviceServiceVersion: " + cashDrawer.getDeviceServiceVersion()
					+ "\nphysicalDeviceDescription: " + cashDrawer.getPhysicalDeviceDescription()
					+ "\nphysicalDeviceName: " + cashDrawer.getPhysicalDeviceName()
					+ "\npowerState: " + MainActivity.getPowerStateString(cashDrawer.getPowerState());
			MessageDialogFragment.showDialog(getFragmentManager(), "Info", message);
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception",
					"Exception in Info: "+ e.getMessage());
		}
	}
	
	private void checkHealth() {
		try {
			cashDrawer.checkHealth(JposConst.JPOS_CH_INTERNAL);
			MessageDialogFragment.showDialog(getFragmentManager(), "checkHealth",
					cashDrawer.getCheckHealthText());
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
		}
	}
}
