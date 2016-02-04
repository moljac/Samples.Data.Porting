package com.samsung.android.sdk.professionalaudio.sample.simpleconnection;

import android.annotation.TargetApi;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.util.AndroidRuntimeException;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.professionalaudio.Sapa;
import com.samsung.android.sdk.professionalaudio.SapaPort;
import com.samsung.android.sdk.professionalaudio.SapaPortConnection;
import com.samsung.android.sdk.professionalaudio.SapaService;

import java.util.ArrayList;
import java.util.Iterator;
import java.util.List;
import java.util.Vector;

public class SapaSimpleConnectionActivity extends Activity implements OnItemSelectedListener {
	SapaService mService;

	ToggleButton mOnOffButton;
	Button mButtonCon, mButtonAudio, mButtonMidi, mButtonDisconnAll, mGetConn;
	Button mSyncStart, mSyncStop, mSetPosition;
	ListView mInputPortList, mOutputPortList, mConnected;
	TextView mInputText, mOutputText;
	Spinner spinner;
	int selectedSpinnerValue = 0;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);

		Vector<String> availableList = new Vector<String>();
        
		try {
			Sapa sapa = new Sapa();
			sapa.initialize(this);
			mService = new SapaService();
			
			availableList.add(new String("Default"));
	        if(sapa.isFeatureEnabled(Sapa.SUPPORT_HIGH_LATENCY)){
	            availableList.add(new String("High"));
	        }
	        if(sapa.isFeatureEnabled(Sapa.SUPPORT_MID_LATENCY)){
	            availableList.add(new String("Mid"));
	        }
	        if(sapa.isFeatureEnabled(Sapa.SUPPORT_LOW_LATENCY)){
	            availableList.add(new String("Low"));
	        }
		} catch (SsdkUnsupportedException e1) {
			e1.printStackTrace();
			Toast.makeText(this, "Not support Professional Audio package", Toast.LENGTH_LONG).show();
			finish();
			return;
		} catch (InstantiationException e) {
			e.printStackTrace();
			Toast.makeText(this, "Fail to create", Toast.LENGTH_LONG).show();
			finish();
			return;
		}

		mSyncStart = (Button) this.findViewById(R.id.sync_start);
		mSyncStart.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				mService.startTransport();
			}
		});

		mSyncStop = (Button) this.findViewById(R.id.sync_stop);
		mSyncStop.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				mService.stopTransport();
			}
		});

		mSetPosition = (Button) this.findViewById(R.id.set_position);
		mSetPosition.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				mService.setTransportPosition(1, 1, 0);
			}
		});

		mButtonCon = (Button) findViewById(R.id.connect_button);
		mButtonCon.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				try{
				mService.connect(new SapaPortConnection(mInputText
						.getText().toString(), mOutputText.getText().toString()));
				}catch(AndroidRuntimeException e){
					e.printStackTrace();
					Toast.makeText(v.getContext(), e.getMessage(), Toast.LENGTH_LONG).show();
				}
				getConnectedPortList();
			}
		});

		mButtonDisconnAll = (Button) findViewById(R.id.disconn_button);
		mButtonDisconnAll.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				disconnectAll();
				getConnectedPortList();
			}
		});

		mOnOffButton = (ToggleButton) findViewById(R.id.service_button);
		mOnOffButton.setOnClickListener(new OnClickListener() {
			@Override
			public void onClick(View v) {
				if (mOnOffButton.isChecked()) {
					try{
					    int startParam = SapaService.START_PARAM_DEFAULT_LATENCY;
					    if(selectedSpinnerValue == 1){
					        startParam = SapaService.START_PARAM_HIGH_LATENCY;
					    } else if(selectedSpinnerValue == 2){
					        startParam = SapaService.START_PARAM_MID_LATENCY;
					    } else if(selectedSpinnerValue == 3){
					        startParam = SapaService.START_PARAM_LOW_LATENCY;
					    }
					mService.start(startParam);
					} catch(AndroidRuntimeException e){
						e.printStackTrace();
						//ignore.
					}
					if(mService.isStarted() == false){
						Toast.makeText(v.getContext(), "Fail to start the service", Toast.LENGTH_LONG).show();
						mOnOffButton.setChecked(false);
					}
				} else {
					mService.stop(false);
					if(mService.isStarted() == true){
						Toast.makeText(v.getContext(), "Fail to stop the service. Maybe some processor is using.", Toast.LENGTH_LONG).show();
						mOnOffButton.setChecked(true);
					}
				}
			}
		});

		mButtonAudio = (Button) findViewById(R.id.audio_tab_button);
		mButtonAudio.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				// Call Get port list with "Audio"
				mInputText.setText(null);
				mOutputText.setText(null);
				getPortList("audio");
			}
		});

		mButtonMidi = (Button) findViewById(R.id.midi_tab_button);
		mButtonMidi.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				mInputText.setText(null);
				mOutputText.setText(null);
				getPortList("midi");
			}
		});

		mInputText = (TextView) findViewById(R.id.inputport_text);
		mOutputText = (TextView) findViewById(R.id.outputport_text);

		mInputPortList = (ListView) findViewById(R.id.inputPort_listView);
		mInputPortList
				.setOnItemClickListener(new AdapterView.OnItemClickListener() {
					@TargetApi(11)
					public void onItemClick(AdapterView<?> arg0, View arg1,
							int arg2, long arg3) {
						if (arg2 > 0) {
							mInputText.setText(mInputPortList
									.getItemAtPosition(arg2).toString());
						} else {
							mInputPortList.setItemChecked(0, false);
						}
					}
				});

		mOutputPortList = (ListView) findViewById(R.id.outputPort_listView);
		mOutputPortList
				.setOnItemClickListener(new AdapterView.OnItemClickListener() {
					@Override
					public void onItemClick(AdapterView<?> arg0, View arg1,
							int arg2, long arg3) {
						if (arg2 > 0) {
							mOutputText.setText(mOutputPortList
									.getItemAtPosition(arg2).toString());
						} else {
							mOutputPortList.setItemChecked(0, false);
						}
					}
				});

		mConnected = (ListView) findViewById(R.id.connected_ListView);
		mConnected
				.setOnItemClickListener(new AdapterView.OnItemClickListener() {
					@Override
					public void onItemClick(AdapterView<?> arg0, View arg1,
							int arg2, long arg3) {
						if (arg2 > 0) {
							showDiallog(arg2);
						}
					}
				});

		mGetConn = (Button) findViewById(R.id.getconn_button);
		mGetConn.setOnClickListener(new Button.OnClickListener() {
			public void onClick(View v) {
				getConnectedPortList();
			}
		});

		TextView tv = new TextView(getApplicationContext());

		tv.setTextSize(20);
		tv.setText("Input Port List");

		mInputPortList.addHeaderView(tv);

		TextView tv1 = new TextView(getApplicationContext());
		tv1.setTextSize(20);
		tv1.setText("OutPut Port List");

		mOutputPortList.addHeaderView(tv1);

		TextView tv2 = new TextView(getApplicationContext());
		tv2.setTextSize(20);
		tv2.setText("Connected Port List");

		mConnected.addHeaderView(tv2);

		mInputPortList.setAdapter(null);
		mOutputPortList.setAdapter(null);
		mConnected.setAdapter(null);

		if (mService.isStarted() == true) {
			mOnOffButton.setChecked(true);
		} else {
			mOnOffButton.setChecked(false);
		}
		
		
        
		spinner = (Spinner)findViewById(R.id.latency_spinner);
		String[] items = new String[availableList.size()];
		availableList.toArray(items);
        ArrayAdapter<String> spinnerArrayAdapter = new ArrayAdapter<String>(this,
                android.R.layout.simple_spinner_dropdown_item,
                    items);
        
        spinner.setOnItemSelectedListener(this);
        spinner.setAdapter(spinnerArrayAdapter);
	}
	
    @Override
    public void onItemSelected(AdapterView<?> arg0, View arg1, int arg2, long arg3) {
        selectedSpinnerValue = arg2;
    }

	@Override
	protected void onResume() {
		super.onResume();
		
		if(mService.isStarted() == true){
			mOnOffButton.setChecked(true);
		} else {
			mOnOffButton.setChecked(false);
		}
	}

	public boolean disconnectAll() {
		mService.disconnectAllConnection();
		return true;
	}

	public boolean getPortList(String type) {
		List<SapaPort> portList = mService.getAllPort();
		if (portList != null) {
			int length = portList.size();

			List<String> listInputContents = new ArrayList<String>(length);
			List<String> listOutputContents = new ArrayList<String>(length);

			Iterator<SapaPort> iter = portList.iterator();
			while (iter.hasNext()) {
				SapaPort port = iter.next();
				if (port.getInOutType() == SapaPort.INOUT_TYPE_OUT) {
					int etype = type.startsWith("midi") ? SapaPort.SIGNAL_TYPE_MIDI
							: SapaPort.SIGNAL_TYPE_AUDIO;
					if (etype == port.getSignalType()) {
						listInputContents.add(port.getFullName());
					}
				} else {
					int etype = type.startsWith("midi") ? SapaPort.SIGNAL_TYPE_MIDI
							: SapaPort.SIGNAL_TYPE_AUDIO;
					if (etype == port.getSignalType()) {
						listOutputContents.add(port.getFullName());
					}
				}
			}

			mInputPortList.setAdapter(new ArrayAdapter<String>(this,
					android.R.layout.simple_list_item_activated_1,
					listInputContents));
			mInputPortList.setTextFilterEnabled(true);
			mInputPortList.setChoiceMode(ListView.CHOICE_MODE_SINGLE);

			mOutputPortList.setAdapter(new ArrayAdapter<String>(this,
					android.R.layout.simple_list_item_activated_1,
					listOutputContents));
			mOutputPortList.setTextFilterEnabled(true);
			mOutputPortList.setChoiceMode(ListView.CHOICE_MODE_SINGLE);
		}
		return true;
	}

	public boolean getConnectedPortList() {
		List<SapaPortConnection> connectionList = mService
				.getAllConnection();

		if (connectionList != null) {
			int length = connectionList.size();
			List<String> listConnContents = new ArrayList<String>(length);

			Iterator<SapaPortConnection> iter = connectionList
					.iterator();
			while (iter.hasNext()) {
				SapaPortConnection one = iter.next();
				listConnContents.add(one.getSourcePortName() + " "
						+ one.getDestinationPortName());
			}

			mConnected.setAdapter(new ArrayAdapter<String>(this,
					android.R.layout.simple_list_item_1, listConnContents));
		}
		return true;
	}

	public void showDiallog(final int position) {
		AlertDialog.Builder builder = new AlertDialog.Builder(this);

		builder.setTitle("Confirm");
		builder.setMessage("Disconnect ?");

		builder.setPositiveButton("YES", new DialogInterface.OnClickListener() {
			public void onClick(DialogInterface dialog, int which) {
				// Do nothing but close the dialog
				disconnect(position);
				dialog.dismiss();
			}
		});

		builder.setNegativeButton("NO", new DialogInterface.OnClickListener() {
			@Override
			public void onClick(DialogInterface dialog, int which) {
				// Do nothing
				dialog.dismiss();
			}
		});

		AlertDialog alert = builder.create();
		alert.show();
	}

	public void disconnect(int position) {
		String connection = mConnected.getItemAtPosition(position).toString();
		String[] arrayConnection;
		arrayConnection = connection.split(" ");
		try{
		mService.disconnect(new SapaPortConnection(arrayConnection[0]
				.toString(), arrayConnection[1].toString()));
		}catch(Exception e){
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_LONG).show();
		}
		getConnectedPortList();
	}

	@Override
	protected void onStart() {
		super.onStart();
		getConnectedPortList();
	}

    @Override
    public void onNothingSelected(AdapterView<?> arg0) {
        // TODO Auto-generated method stub
        
    }
}
