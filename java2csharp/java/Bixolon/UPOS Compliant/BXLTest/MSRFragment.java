package com.bxl.postest;

import jpos.JposConst;
import jpos.JposException;
import jpos.MSR;
import jpos.MSRConst;
import jpos.events.DataEvent;
import jpos.events.DataListener;
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

public class MSRFragment extends Fragment
		implements View.OnClickListener, CompoundButton.OnCheckedChangeListener,
		RadioGroup.OnCheckedChangeListener, DataListener {
	
	private MSR msr;
	
	private EditText logicalNameEditText;
	private TextView stateTextView;
	
	private CheckBox deviceEnabledCheckBox;
	private CheckBox autoDisableCheckBox;
	private CheckBox dataEventEnabledCheckBox;
	
	private TextView track1DataLengthTextView;
	private TextView track1DataTextView;
	private TextView track2DataLengthTextView;
	private TextView track2DataTextView;
	private TextView track3DataLengthTextView;
	private TextView track3DataTextView;
	
	private CountDownTimer countDownTimer;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		msr = new MSR();
		msr.addDataListener(this);
	}

	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_msr, container, false);
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
		logicalNameEditText.setText(settings.getString(MainActivity.KEY_LOGICAL_NAME_MSR, getString(R.string.msr)));
		
		view.findViewById(R.id.buttonOpen).setOnClickListener(this);
		view.findViewById(R.id.buttonClaim).setOnClickListener(this);
		view.findViewById(R.id.buttonRelease).setOnClickListener(this);
		view.findViewById(R.id.buttonClose).setOnClickListener(this);
		view.findViewById(R.id.buttonInfo).setOnClickListener(this);
		view.findViewById(R.id.buttonCheckHealth).setOnClickListener(this);
		view.findViewById(R.id.buttonClearFields).setOnClickListener(this);
		view.findViewById(R.id.buttonRefreshFields).setOnClickListener(this);
		
		deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
		deviceEnabledCheckBox.setOnCheckedChangeListener(this);
		autoDisableCheckBox = (CheckBox) view.findViewById(R.id.checkBoxAutoDisable);
		autoDisableCheckBox.setOnCheckedChangeListener(this);
		dataEventEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDataEventEnabled);
		dataEventEnabledCheckBox.setOnCheckedChangeListener(this);
		
		RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroupDataEncryptionAlgorithm);
		radioGroup.setOnCheckedChangeListener(this);
		
		track1DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack1DataLength);
		track1DataTextView = (TextView) view.findViewById(R.id.textViewTrack1Data);
		track2DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack2DataLength);
		track2DataTextView = (TextView) view.findViewById(R.id.textViewTrack2Data);
		track3DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack3DataLength);
		track3DataTextView = (TextView) view.findViewById(R.id.textViewTrack3Data);
		
		stateTextView = (TextView) view.findViewById(R.id.textViewState);
		return view;
	}
	
	@Override
	public void onDestroyView() {
		super.onDestroyView();
		
		SharedPreferences settings = getActivity().getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
		SharedPreferences.Editor editor = settings.edit();
		editor.putString(MainActivity.KEY_LOGICAL_NAME_MSR, logicalNameEditText.getText().toString());
		editor.commit();
	}
	
	@Override
	public void onResume() {
		super.onResume();
		
		countDownTimer = new CountDownTimer(Long.MAX_VALUE, 1000) {
			
			@Override
			public void onTick(long millisUntilFinished) {
				stateTextView.setText(MainActivity.getStatusString(msr.getState()));
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
			msr.close();
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}
	
	@Override
	public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
		switch (buttonView.getId()) {
		case R.id.checkBoxDeviceEnabled:
			try {
				msr.setDeviceEnabled(isChecked);
			} catch (JposException e) {
				e.printStackTrace();
				try {
					msr.setDeviceEnabled(!isChecked);
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.checkBoxAutoDisable:
			try {
				msr.setAutoDisable(isChecked);
			} catch (JposException e) {
				e.printStackTrace();
				try {
					msr.setAutoDisable(!isChecked);
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.checkBoxDataEventEnabled:
			try {
				msr.setDataEventEnabled(isChecked);
			} catch (JposException e) {
				e.printStackTrace();
				try {
					msr.setDataEventEnabled(!isChecked);
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
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
				msr.open(logicalDeviceName);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClaim:
			try {
				msr.claim(0);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonRelease:
			try {
				msr.release();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClose:
			try {
				msr.close();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonInfo:
			try {
				info();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonCheckHealth:
			checkHealth();
			break;
			
		case R.id.buttonRefreshFields:
			try {
				refreshFields();
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonClearFields:
			track1DataTextView.setText("");
			track2DataTextView.setText("");
			track3DataTextView.setText("");
			break;
		}
	}
	
	@Override
	public void onCheckedChanged(RadioGroup group, int checkedId) {
		switch (checkedId) {
		case R.id.radioNotEnabled:
			try {
				msr.setDataEncryptionAlgorithm(MSRConst.MSR_DE_NONE);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.radioTripleDES:
			try {
				msr.setDataEncryptionAlgorithm(MSRConst.MSR_DE_3DEA_DUKPT);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
		}
	}
	
	@Override
	public void dataOccurred(final DataEvent e) {
		getActivity().runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				try {
					refreshFields();
					
					track1DataLengthTextView.setText(Integer.toString(e.getStatus() & 0xff));
					track2DataLengthTextView.setText(Integer.toString((e.getStatus() & 0xff00) >> 8));
					track3DataLengthTextView.setText(Integer.toString((e.getStatus() & 0xff0000) >> 16));
					
					deviceEnabledCheckBox.setChecked(msr.getDeviceEnabled());
					dataEventEnabledCheckBox.setChecked(msr.getDataEventEnabled());
					autoDisableCheckBox.setChecked(msr.getAutoDisable());
				} catch (JposException e) {
					e.printStackTrace();
					MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
				}
			}
		});
	}
	
	private void info() throws JposException {
		String message = "deviceServiceDescription: " + msr.getDeviceServiceDescription()
				+ "\ndeviceServiceVersion: " + msr.getDeviceServiceVersion()
				+ "\nphysicalDeviceDescription: " + msr.getPhysicalDeviceDescription()
				+ "\nphysicalDeviceName: " + msr.getPhysicalDeviceName()
				+ "\npowerState: " + MainActivity.getPowerStateString(msr.getPowerState())
				+ "\ncapDataEncryption: " + getDataEncryptionString(msr.getCapDataEncryption())
				+ "\ndataEncryptionAlgorithm: " + getDataEncryptionString(msr.getDataEncryptionAlgorithm())
				+ "\ntracksToRead: " + getTrackToReadString(msr.getTracksToRead());
		MessageDialogFragment.showDialog(getFragmentManager(), "Info", message);
	}
	
	private String getDataEncryptionString(int dataEncryption) {
		switch (dataEncryption) {
		case MSRConst.MSR_DE_NONE:
			return "Data encryption is not enabled";
			
		case MSRConst.MSR_DE_3DEA_DUKPT:
			return "Triple DEA encryption";
			
			default:
				return "Additional encryption algorithms supported";
		}
	}
	
	private void checkHealth() {
		try {
			msr.checkHealth(JposConst.JPOS_CH_INTERNAL);
			MessageDialogFragment.showDialog(getFragmentManager(), "checkHealth", msr.getCheckHealthText());
		} catch (JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
		}
	}
	
	private void refreshFields() throws JposException {
		track1DataTextView.setText(new String(msr.getTrack1Data()));
		track2DataTextView.setText(new String(msr.getTrack2Data()));
		track3DataTextView.setText(new String(msr.getTrack3Data()));
	}
	
	private String getTrackToReadString(int tracksToRead) {
		switch (tracksToRead) {
		case MSRConst.MSR_TR_1:
			return "Track 1";
			
		case MSRConst.MSR_TR_2:
			return "Track 2";
			
		case MSRConst.MSR_TR_3:
			return "Track 3";
			
		case MSRConst.MSR_TR_1_2:
			return "Track 1 and 2";
			
		case MSRConst.MSR_TR_2_3:
			return "Track 2 and 3";
			
		case MSRConst.MSR_TR_1_2_3:
			return "Track 1, 2 and 3";
			
			default:
				return "MSR does not support reading track data";
		}
	}
}
