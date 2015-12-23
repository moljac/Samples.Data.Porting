using System;

namespace com.bxl.postest
{

	using JposConst = jpos.JposConst;
	using JposException = jpos.JposException;
	using MSR = jpos.MSR;
	using MSRConst = jpos.MSRConst;
	using DataEvent = jpos.events.DataEvent;
	using DataListener = jpos.events.DataListener;
	using Context = android.content.Context;
	using SharedPreferences = android.content.SharedPreferences;
	using Bundle = android.os.Bundle;
	using CountDownTimer = android.os.CountDownTimer;
	using Nullable = android.support.annotation.Nullable;
	using Fragment = android.support.v4.app.Fragment;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using TextView = android.widget.TextView;

	public class MSRFragment : Fragment, View.OnClickListener, CompoundButton.OnCheckedChangeListener, RadioGroup.OnCheckedChangeListener, DataListener
	{

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

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			msr = new MSR();
			msr.addDataListener(this);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_msr, container, false);

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
			logicalNameEditText.Text = settings.getString(MainActivity.KEY_LOGICAL_NAME_MSR, getString(R.@string.msr));

			view.findViewById(R.id.buttonOpen).OnClickListener = this;
			view.findViewById(R.id.buttonClaim).OnClickListener = this;
			view.findViewById(R.id.buttonRelease).OnClickListener = this;
			view.findViewById(R.id.buttonClose).OnClickListener = this;
			view.findViewById(R.id.buttonInfo).OnClickListener = this;
			view.findViewById(R.id.buttonCheckHealth).OnClickListener = this;
			view.findViewById(R.id.buttonClearFields).OnClickListener = this;
			view.findViewById(R.id.buttonRefreshFields).OnClickListener = this;

			deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
			deviceEnabledCheckBox.OnCheckedChangeListener = this;
			autoDisableCheckBox = (CheckBox) view.findViewById(R.id.checkBoxAutoDisable);
			autoDisableCheckBox.OnCheckedChangeListener = this;
			dataEventEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDataEventEnabled);
			dataEventEnabledCheckBox.OnCheckedChangeListener = this;

			RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroupDataEncryptionAlgorithm);
			radioGroup.OnCheckedChangeListener = this;

			track1DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack1DataLength);
			track1DataTextView = (TextView) view.findViewById(R.id.textViewTrack1Data);
			track2DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack2DataLength);
			track2DataTextView = (TextView) view.findViewById(R.id.textViewTrack2Data);
			track3DataLengthTextView = (TextView) view.findViewById(R.id.textViewTrack3DataLength);
			track3DataTextView = (TextView) view.findViewById(R.id.textViewTrack3Data);

			stateTextView = (TextView) view.findViewById(R.id.textViewState);
			return view;
		}

		public override void onDestroyView()
		{
			base.onDestroyView();

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			SharedPreferences.Editor editor = settings.edit();
			editor.putString(MainActivity.KEY_LOGICAL_NAME_MSR, logicalNameEditText.Text.ToString());
			editor.commit();
		}

		public override void onResume()
		{
			base.onResume();

			countDownTimer = new CountDownTimerAnonymousInnerClassHelper(this, long.MaxValue)
			.start();
		}

		private class CountDownTimerAnonymousInnerClassHelper : CountDownTimer
		{
			private readonly MSRFragment outerInstance;

			public CountDownTimerAnonymousInnerClassHelper(MSRFragment outerInstance, UnknownType MAX_VALUE) : base(MAX_VALUE, 1000)
			{
				this.outerInstance = outerInstance;
			}


			public override void onTick(long millisUntilFinished)
			{
				outerInstance.stateTextView.Text = MainActivity.getStatusString(outerInstance.msr.State);
			}

			public override void onFinish()
			{
				// TODO Auto-generated method stub

			}
		}

		public override void onPause()
		{
			base.onPause();

			countDownTimer.cancel();
		}

		public override void onDestroy()
		{
			base.onDestroy();

			try
			{
				msr.close();
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			switch (buttonView.Id)
			{
			case R.id.checkBoxDeviceEnabled:
				try
				{
					msr.DeviceEnabled = isChecked;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					try
					{
						msr.DeviceEnabled = !isChecked;
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.checkBoxAutoDisable:
				try
				{
					msr.AutoDisable = isChecked;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					try
					{
						msr.AutoDisable = !isChecked;
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.checkBoxDataEventEnabled:
				try
				{
					msr.DataEventEnabled = isChecked;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					try
					{
						msr.DataEventEnabled = !isChecked;
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;
			}
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonOpen:
				string logicalDeviceName = logicalNameEditText.Text.ToString();
				try
				{
					msr.open(logicalDeviceName);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonClaim:
				try
				{
					msr.claim(0);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonRelease:
				try
				{
					msr.release();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonClose:
				try
				{
					msr.close();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonInfo:
				try
				{
					info();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonCheckHealth:
				checkHealth();
				break;

			case R.id.buttonRefreshFields:
				try
				{
					refreshFields();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonClearFields:
				track1DataTextView.Text = "";
				track2DataTextView.Text = "";
				track3DataTextView.Text = "";
				break;
			}
		}

		public override void onCheckedChanged(RadioGroup group, int checkedId)
		{
			switch (checkedId)
			{
			case R.id.radioNotEnabled:
				try
				{
					msr.DataEncryptionAlgorithm = MSRConst.MSR_DE_NONE;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.radioTripleDES:
				try
				{
					msr.DataEncryptionAlgorithm = MSRConst.MSR_DE_3DEA_DUKPT;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void dataOccurred(final jpos.events.DataEvent e)
		public override void dataOccurred(DataEvent e)
		{
			Activity.runOnUiThread(() =>
			{

				try
				{
					refreshFields();

					track1DataLengthTextView.Text = Convert.ToString(e.Status & 0xff);
					track2DataLengthTextView.Text = Convert.ToString((e.Status & 0xff00) >> 8);
					track3DataLengthTextView.Text = Convert.ToString((e.Status & 0xff0000) >> 16);

					deviceEnabledCheckBox.Checked = msr.DeviceEnabled;
					dataEventEnabledCheckBox.Checked = msr.DataEventEnabled;
					autoDisableCheckBox.Checked = msr.AutoDisable;
				}
				catch (JposException e)
				{
					e.printStackTrace();
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
			});
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void info() throws jpos.JposException
		private void info()
		{
			string message = "deviceServiceDescription: " + msr.DeviceServiceDescription + "\ndeviceServiceVersion: " + msr.DeviceServiceVersion + "\nphysicalDeviceDescription: " + msr.PhysicalDeviceDescription + "\nphysicalDeviceName: " + msr.PhysicalDeviceName + "\npowerState: " + MainActivity.getPowerStateString(msr.PowerState) + "\ncapDataEncryption: " + getDataEncryptionString(msr.CapDataEncryption) + "\ndataEncryptionAlgorithm: " + getDataEncryptionString(msr.DataEncryptionAlgorithm) + "\ntracksToRead: " + getTrackToReadString(msr.TracksToRead);
			MessageDialogFragment.showDialog(FragmentManager, "Info", message);
		}

		private string getDataEncryptionString(int dataEncryption)
		{
			switch (dataEncryption)
			{
			case MSRConst.MSR_DE_NONE:
				return "Data encryption is not enabled";

			case MSRConst.MSR_DE_3DEA_DUKPT:
				return "Triple DEA encryption";

				default:
					return "Additional encryption algorithms supported";
			}
		}

		private void checkHealth()
		{
			try
			{
				msr.checkHealth(JposConst.JPOS_CH_INTERNAL);
				MessageDialogFragment.showDialog(FragmentManager, "checkHealth", msr.CheckHealthText);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void refreshFields() throws jpos.JposException
		private void refreshFields()
		{
			track1DataTextView.Text = new string(msr.Track1Data);
			track2DataTextView.Text = new string(msr.Track2Data);
			track3DataTextView.Text = new string(msr.Track3Data);
		}

		private string getTrackToReadString(int tracksToRead)
		{
			switch (tracksToRead)
			{
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

}