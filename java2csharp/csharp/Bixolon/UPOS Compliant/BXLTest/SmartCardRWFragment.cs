namespace com.bxl.postest
{

	using BXLUtility = com.bxl.util.BXLUtility;

	using JposConst = jpos.JposConst;
	using JposException = jpos.JposException;
	using SmartCardRW = jpos.SmartCardRW;
	using SmartCardRWConst = jpos.SmartCardRWConst;
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

	public class SmartCardRWFragment : Fragment, View.OnClickListener, CompoundButton.OnCheckedChangeListener, RadioGroup.OnCheckedChangeListener
	{

		private SmartCardRW smartCardRW;

		private EditText logicalNameEditText;
		private TextView stateTextView;

		private TextView readDataTextView;

		private CountDownTimer countDownTimer;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			smartCardRW = new SmartCardRW();
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_smart_card_rw, container, false);

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
			logicalNameEditText.Text = settings.getString(MainActivity.KEY_LOGICAL_NAME_SMART_CARD_RW, getString(R.@string.smart_card_rw));

			view.findViewById(R.id.buttonOpen).OnClickListener = this;
			view.findViewById(R.id.buttonClaim).OnClickListener = this;
			view.findViewById(R.id.buttonRelease).OnClickListener = this;
			view.findViewById(R.id.buttonClose).OnClickListener = this;
			view.findViewById(R.id.buttonInfo).OnClickListener = this;
			view.findViewById(R.id.buttonCheckHealth).OnClickListener = this;
			view.findViewById(R.id.buttonReadData).OnClickListener = this;

			CheckBox checkBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
			checkBox.OnCheckedChangeListener = this;

			RadioGroup radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup1);
			radioGroup.OnCheckedChangeListener = this;
			radioGroup = (RadioGroup) view.findViewById(R.id.radioGroup2);
			radioGroup.OnCheckedChangeListener = this;

			readDataTextView = (TextView) view.findViewById(R.id.textViewReadData);
			stateTextView = (TextView) view.findViewById(R.id.textViewState);
			return view;
		}

		public override void onDestroyView()
		{
			base.onDestroyView();

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			SharedPreferences.Editor editor = settings.edit();
			editor.putString(MainActivity.KEY_LOGICAL_NAME_SMART_CARD_RW, logicalNameEditText.Text.ToString());
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
			private readonly SmartCardRWFragment outerInstance;

			public CountDownTimerAnonymousInnerClassHelper(SmartCardRWFragment outerInstance, UnknownType MAX_VALUE) : base(MAX_VALUE, 1000)
			{
				this.outerInstance = outerInstance;
			}


			public override void onTick(long millisUntilFinished)
			{
				outerInstance.stateTextView.Text = MainActivity.getStatusString(outerInstance.smartCardRW.State);
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
				smartCardRW.close();
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
			try
			{
				if (buttonView.Id == R.id.checkBoxDeviceEnabled)
				{
					smartCardRW.DeviceEnabled = isChecked;
					if (isChecked)
					{
						smartCardRW.SCSlot = 0x01 << (sizeof(int) - 1);
					}
				}
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
			}
		}

		public override void onCheckedChanged(RadioGroup group, int checkedId)
		{
			try
			{
				switch (checkedId)
				{
				case R.id.radioSmartCard:
					smartCardRW.SCSlot = 0x01 << (sizeof(int) - 1);
					break;

				case R.id.radioSam1:
					smartCardRW.SCSlot = 0x01 << (sizeof(int) - 2);
					break;

				case R.id.radioSam2:
					smartCardRW.SCSlot = 0x01 << (sizeof(int) - 3);
					break;

				case R.id.radioApdu:
					smartCardRW.IsoEmvMode = SmartCardRWConst.SC_CMODE_EMV;
					break;

				case R.id.radioTpdu:
					smartCardRW.IsoEmvMode = SmartCardRWConst.SC_CMODE_ISO;
					break;
				}
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
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
					smartCardRW.open(logicalDeviceName);
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
					smartCardRW.claim(0);
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
					smartCardRW.release();
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
					smartCardRW.close();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonInfo:
				info();
				break;

			case R.id.buttonCheckHealth:
				checkHealth();
				break;

			case R.id.buttonReadData:
				string[] data = new string[] {StringHelperClass.NewString(new sbyte[] {0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39, 0x3a, 0x3b, 0x3c, 0x3d, 0x3e, 0x3f, 0x40})};
				int[] count = new int[1];

				try
				{
					smartCardRW.readData(SmartCardRWConst.SC_READ_DATA, count, data);
					readDataTextView.Text = BXLUtility.toHexString(data[0].GetBytes());
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					readDataTextView.Text = "";
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;
			}
		}

		private void info()
		{
			try
			{
				string message = "deviceServiceDescription: " + smartCardRW.DeviceServiceDescription + "\ndeviceServiceVersion: " + smartCardRW.DeviceServiceVersion + "\nphysicalDeviceDescription: " + smartCardRW.PhysicalDeviceDescription + "\nphysicalDeviceName: " + smartCardRW.PhysicalDeviceName + "\npowerState: " + smartCardRW.PowerState + "\ninterfaceMode: " + smartCardRW.InterfaceMode + "\nisoEmvMode: " + smartCardRW.IsoEmvMode + "\ntransactionInProgress: " + smartCardRW.TransactionInProgress + "\ntransmissionProtocol: " + smartCardRW.TransmissionProtocol;
				MessageDialogFragment.showDialog(FragmentManager, "Info", message);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
			}
		}

		private void checkHealth()
		{
			try
			{
				smartCardRW.checkHealth(JposConst.JPOS_CH_INTERNAL);
				MessageDialogFragment.showDialog(FragmentManager, "Info", smartCardRW.CheckHealthText);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
			}
		}
	}

}