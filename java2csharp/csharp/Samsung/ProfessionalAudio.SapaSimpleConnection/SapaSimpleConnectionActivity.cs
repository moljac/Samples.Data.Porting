using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.sample.simpleconnection
{

	using TargetApi = android.annotation.TargetApi;
	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using AndroidRuntimeException = android.util.AndroidRuntimeException;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using OnItemSelectedListener = android.widget.AdapterView.OnItemSelectedListener;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;
	using ToggleButton = android.widget.ToggleButton;



	public class SapaSimpleConnectionActivity : Activity, AdapterView.OnItemSelectedListener
	{
		internal SapaService mService;

		internal ToggleButton mOnOffButton;
		internal Button mButtonCon, mButtonAudio, mButtonMidi, mButtonDisconnAll, mGetConn;
		internal Button mSyncStart, mSyncStop, mSetPosition;
		internal ListView mInputPortList, mOutputPortList, mConnected;
		internal TextView mInputText, mOutputText;
		internal Spinner spinner;
		internal int selectedSpinnerValue = 0;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			List<string> availableList = new List<string>();

			try
			{
				Sapa sapa = new Sapa();
				sapa.initialize(this);
				mService = new SapaService();

				availableList.Add("Default");
				if (sapa.isFeatureEnabled(Sapa.SUPPORT_HIGH_LATENCY))
				{
					availableList.Add("High");
				}
				if (sapa.isFeatureEnabled(Sapa.SUPPORT_MID_LATENCY))
				{
					availableList.Add("Mid");
				}
				if (sapa.isFeatureEnabled(Sapa.SUPPORT_LOW_LATENCY))
				{
					availableList.Add("Low");
				}
			}
			catch (SsdkUnsupportedException e1)
			{
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
				Toast.makeText(this, "Not support Professional Audio package", Toast.LENGTH_LONG).show();
				finish();
				return;
			}
			catch (InstantiationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "Fail to create", Toast.LENGTH_LONG).show();
				finish();
				return;
			}

			mSyncStart = (Button) this.findViewById(R.id.sync_start);
			mSyncStart.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mSyncStop = (Button) this.findViewById(R.id.sync_stop);
			mSyncStop.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			mSetPosition = (Button) this.findViewById(R.id.set_position);
			mSetPosition.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);

			mButtonCon = (Button) findViewById(R.id.connect_button);
			mButtonCon.OnClickListener = new OnClickListenerAnonymousInnerClassHelper4(this, e);

			mButtonDisconnAll = (Button) findViewById(R.id.disconn_button);
			mButtonDisconnAll.OnClickListener = new OnClickListenerAnonymousInnerClassHelper5(this);

			mOnOffButton = (ToggleButton) findViewById(R.id.service_button);
			mOnOffButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, e);

			mButtonAudio = (Button) findViewById(R.id.audio_tab_button);
			mButtonAudio.OnClickListener = new OnClickListenerAnonymousInnerClassHelper6(this);

			mButtonMidi = (Button) findViewById(R.id.midi_tab_button);
			mButtonMidi.OnClickListener = new OnClickListenerAnonymousInnerClassHelper7(this);

			mInputText = (TextView) findViewById(R.id.inputport_text);
			mOutputText = (TextView) findViewById(R.id.outputport_text);

			mInputPortList = (ListView) findViewById(R.id.inputPort_listView);
			mInputPortList.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);

			mOutputPortList = (ListView) findViewById(R.id.outputPort_listView);
			mOutputPortList.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper2(this);

			mConnected = (ListView) findViewById(R.id.connected_ListView);
			mConnected.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper3(this);

			mGetConn = (Button) findViewById(R.id.getconn_button);
			mGetConn.OnClickListener = new OnClickListenerAnonymousInnerClassHelper8(this);

			TextView tv = new TextView(ApplicationContext);

			tv.TextSize = 20;
			tv.Text = "Input Port List";

			mInputPortList.addHeaderView(tv);

			TextView tv1 = new TextView(ApplicationContext);
			tv1.TextSize = 20;
			tv1.Text = "OutPut Port List";

			mOutputPortList.addHeaderView(tv1);

			TextView tv2 = new TextView(ApplicationContext);
			tv2.TextSize = 20;
			tv2.Text = "Connected Port List";

			mConnected.addHeaderView(tv2);

			mInputPortList.Adapter = null;
			mOutputPortList.Adapter = null;
			mConnected.Adapter = null;

			if (mService.Started == true)
			{
				mOnOffButton.Checked = true;
			}
			else
			{
				mOnOffButton.Checked = false;
			}



			spinner = (Spinner)findViewById(R.id.latency_spinner);
			string[] items = new string[availableList.Count];
			availableList.toArray(items);
			ArrayAdapter<string> spinnerArrayAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_spinner_dropdown_item, items);

			spinner.OnItemSelectedListener = this;
			spinner.Adapter = spinnerArrayAdapter;
		}

		private class OnClickListenerAnonymousInnerClassHelper : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.mService.startTransport();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.mService.stopTransport();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.mService.setTransportPosition(1, 1, 0);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			private InstantiationException e;

			public OnClickListenerAnonymousInnerClassHelper4(SapaSimpleConnectionActivity outerInstance, InstantiationException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public virtual void onClick(View v)
			{
				try
				{
				outerInstance.mService.connect(new SapaPortConnection(outerInstance.mInputText.Text.ToString(), outerInstance.mOutputText.Text.ToString()));
				}
				catch (AndroidRuntimeException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					Toast.makeText(v.Context, e.Message, Toast.LENGTH_LONG).show();
				}
				outerInstance.ConnectedPortList;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper5(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.disconnectAll();
				outerInstance.ConnectedPortList;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			private InstantiationException e;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimpleConnectionActivity outerInstance, InstantiationException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				if (outerInstance.mOnOffButton.Checked)
				{
					try
					{
						int startParam = SapaService.START_PARAM_DEFAULT_LATENCY;
						if (outerInstance.selectedSpinnerValue == 1)
						{
							startParam = SapaService.START_PARAM_HIGH_LATENCY;
						}
						else if (outerInstance.selectedSpinnerValue == 2)
						{
							startParam = SapaService.START_PARAM_MID_LATENCY;
						}
						else if (outerInstance.selectedSpinnerValue == 3)
						{
							startParam = SapaService.START_PARAM_LOW_LATENCY;
						}
					outerInstance.mService.start(startParam);
					}
					catch (AndroidRuntimeException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						//ignore.
					}
					if (outerInstance.mService.Started == false)
					{
						Toast.makeText(v.Context, "Fail to start the service", Toast.LENGTH_LONG).show();
						outerInstance.mOnOffButton.Checked = false;
					}
				}
				else
				{
					outerInstance.mService.stop(false);
					if (outerInstance.mService.Started == true)
					{
						Toast.makeText(v.Context, "Fail to stop the service. Maybe some processor is using.", Toast.LENGTH_LONG).show();
						outerInstance.mOnOffButton.Checked = true;
					}
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper6 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper6(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				// Call Get port list with "Audio"
				outerInstance.mInputText.Text = null;
				outerInstance.mOutputText.Text = null;
				outerInstance.getPortList("audio");
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper7 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper7(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.mInputText.Text = null;
				outerInstance.mOutputText.Text = null;
				outerInstance.getPortList("midi");
			}
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @TargetApi(11) public void onItemClick(android.widget.AdapterView<?> arg0, android.view.View arg1, int arg2, long arg3)
			public virtual void onItemClick<T1>(AdapterView<T1> arg0, View arg1, int arg2, long arg3)
			{
				if (arg2 > 0)
				{
					outerInstance.mInputText.Text = outerInstance.mInputPortList.getItemAtPosition(arg2).ToString();
				}
				else
				{
					outerInstance.mInputPortList.setItemChecked(0, false);
				}
			}
		}

		private class OnItemClickListenerAnonymousInnerClassHelper2 : AdapterView.OnItemClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper2(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> arg0, View arg1, int arg2, long arg3)
			{
				if (arg2 > 0)
				{
					outerInstance.mOutputText.Text = outerInstance.mOutputPortList.getItemAtPosition(arg2).ToString();
				}
				else
				{
					outerInstance.mOutputPortList.setItemChecked(0, false);
				}
			}
		}

		private class OnItemClickListenerAnonymousInnerClassHelper3 : AdapterView.OnItemClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper3(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick<T1>(AdapterView<T1> arg0, View arg1, int arg2, long arg3)
			{
				if (arg2 > 0)
				{
					outerInstance.showDiallog(arg2);
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper8 : Button.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper8(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				outerInstance.ConnectedPortList;
			}
		}

		public override void onItemSelected<T1>(AdapterView<T1> arg0, View arg1, int arg2, long arg3)
		{
			selectedSpinnerValue = arg2;
		}

		protected internal override void onResume()
		{
			base.onResume();

			if (mService.Started == true)
			{
				mOnOffButton.Checked = true;
			}
			else
			{
				mOnOffButton.Checked = false;
			}
		}

		public virtual bool disconnectAll()
		{
			mService.disconnectAllConnection();
			return true;
		}

		public virtual bool getPortList(string type)
		{
			IList<SapaPort> portList = mService.AllPort;
			if (portList != null)
			{
				int length = portList.Count;

				IList<string> listInputContents = new List<string>(length);
				IList<string> listOutputContents = new List<string>(length);

				IEnumerator<SapaPort> iter = portList.GetEnumerator();
				while (iter.MoveNext())
				{
					SapaPort port = iter.Current;
					if (port.InOutType == SapaPort.INOUT_TYPE_OUT)
					{
						int etype = type.StartsWith("midi", StringComparison.Ordinal) ? SapaPort.SIGNAL_TYPE_MIDI : SapaPort.SIGNAL_TYPE_AUDIO;
						if (etype == port.SignalType)
						{
							listInputContents.Add(port.FullName);
						}
					}
					else
					{
						int etype = type.StartsWith("midi", StringComparison.Ordinal) ? SapaPort.SIGNAL_TYPE_MIDI : SapaPort.SIGNAL_TYPE_AUDIO;
						if (etype == port.SignalType)
						{
							listOutputContents.Add(port.FullName);
						}
					}
				}

				mInputPortList.Adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_activated_1, listInputContents);
				mInputPortList.TextFilterEnabled = true;
				mInputPortList.ChoiceMode = ListView.CHOICE_MODE_SINGLE;

				mOutputPortList.Adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_activated_1, listOutputContents);
				mOutputPortList.TextFilterEnabled = true;
				mOutputPortList.ChoiceMode = ListView.CHOICE_MODE_SINGLE;
			}
			return true;
		}

		public virtual bool ConnectedPortList
		{
			get
			{
				IList<SapaPortConnection> connectionList = mService.AllConnection;
    
				if (connectionList != null)
				{
					int length = connectionList.Count;
					IList<string> listConnContents = new List<string>(length);
    
					IEnumerator<SapaPortConnection> iter = connectionList.GetEnumerator();
					while (iter.MoveNext())
					{
						SapaPortConnection one = iter.Current;
						listConnContents.Add(one.SourcePortName + " " + one.DestinationPortName);
					}
    
					mConnected.Adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, listConnContents);
				}
				return true;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void showDiallog(final int position)
		public virtual void showDiallog(int position)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(this);

			builder.Title = "Confirm";
			builder.Message = "Disconnect ?";

			builder.setPositiveButton("YES", new OnClickListenerAnonymousInnerClassHelper(this, position));

			builder.setNegativeButton("NO", new OnClickListenerAnonymousInnerClassHelper2(this));

			AlertDialog alert = builder.create();
			alert.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			private int position;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimpleConnectionActivity outerInstance, int position)
			{
				this.outerInstance = outerInstance;
				this.position = position;
			}

			public virtual void onClick(DialogInterface dialog, int which)
			{
				// Do nothing but close the dialog
				outerInstance.disconnect(position);
				dialog.dismiss();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly SapaSimpleConnectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(SapaSimpleConnectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				// Do nothing
				dialog.dismiss();
			}
		}

		public virtual void disconnect(int position)
		{
			string connection = mConnected.getItemAtPosition(position).ToString();
			string[] arrayConnection;
			arrayConnection = connection.Split(" ", true);
			try
			{
			mService.disconnect(new SapaPortConnection(arrayConnection[0].ToString(), arrayConnection[1].ToString()));
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_LONG).show();
			}
			ConnectedPortList;
		}

		protected internal override void onStart()
		{
			base.onStart();
			ConnectedPortList;
		}

		public override void onNothingSelected<T1>(AdapterView<T1> arg0)
		{
			// TODO Auto-generated method stub

		}
	}

}