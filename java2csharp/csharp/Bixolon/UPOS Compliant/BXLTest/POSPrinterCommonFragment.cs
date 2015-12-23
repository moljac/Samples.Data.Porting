namespace com.bxl.postest
{

	using JposConst = jpos.JposConst;
	using JposException = jpos.JposException;
	using POSPrinterConst = jpos.POSPrinterConst;
	using Context = android.content.Context;
	using SharedPreferences = android.content.SharedPreferences;
	using Bundle = android.os.Bundle;
	using CountDownTimer = android.os.CountDownTimer;
	using Nullable = android.support.annotation.Nullable;
	using ScrollingMovementMethod = android.text.method.ScrollingMovementMethod;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;

	public class POSPrinterCommonFragment : POSPrinterFragment, View.OnClickListener, CompoundButton.OnCheckedChangeListener
	{

		private EditText logicalNameEditText;
		private TextView stateTextView;

		private CheckBox deviceEnabledCheckBox;
		private CheckBox asyncModeCheckBox;

		private CountDownTimer countDownTimer;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_common, container, false);

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
			logicalNameEditText.Text = settings.getString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER, getString(R.@string.pos_printer));

			view.findViewById(R.id.buttonOpen).OnClickListener = this;
			view.findViewById(R.id.buttonClaim).OnClickListener = this;
			view.findViewById(R.id.buttonRelease).OnClickListener = this;
			view.findViewById(R.id.buttonClose).OnClickListener = this;
			view.findViewById(R.id.buttonInfo).OnClickListener = this;
			view.findViewById(R.id.buttonCheckHealth).OnClickListener = this;

			deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
			deviceEnabledCheckBox.OnCheckedChangeListener = this;
			asyncModeCheckBox = (CheckBox) view.findViewById(R.id.checkBoxAsyncMode);
			asyncModeCheckBox.OnCheckedChangeListener = this;
			try
			{
				deviceEnabledCheckBox.Checked = posPrinter.DeviceEnabled;
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			try
			{
				asyncModeCheckBox.Checked = posPrinter.AsyncMode;
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			stateTextView = (TextView) view.findViewById(R.id.textViewState);

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;
			return view;
		}

		public override void onDestroyView()
		{
			base.onDestroyView();

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			SharedPreferences.Editor editor = settings.edit();
			editor.putString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER, logicalNameEditText.Text.ToString());
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
			private readonly POSPrinterCommonFragment outerInstance;

			public CountDownTimerAnonymousInnerClassHelper(POSPrinterCommonFragment outerInstance, UnknownType MAX_VALUE) : base(MAX_VALUE, 1000)
			{
				this.outerInstance = outerInstance;
			}


			public override void onTick(long millisUntilFinished)
			{
				outerInstance.stateTextView.Text = MainActivity.getStatusString(outerInstance.posPrinter.State);
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

		public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
		{
			switch (buttonView.Id)
			{
			case R.id.checkBoxDeviceEnabled:
				try
				{
					posPrinter.DeviceEnabled = isChecked;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					try
					{
						posPrinter.DeviceEnabled = !isChecked;
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}
					MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
				}
				break;

			case R.id.checkBoxAsyncMode:
				try
				{
					posPrinter.AsyncMode = isChecked;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					try
					{
						posPrinter.AsyncMode = !isChecked;
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}
					MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
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
					posPrinter.open(logicalDeviceName);
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
					posPrinter.claim(0);
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
					posPrinter.release();
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
					posPrinter.close();
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
			}
		}

		private void info()
		{
			string message;
			try
			{
				message = "deviceServiceDescription: " + posPrinter.DeviceServiceDescription + "\ndeviceServiceVersion: " + posPrinter.DeviceServiceVersion + "\nphysicalDeviceDescription: " + posPrinter.PhysicalDeviceDescription + "\nphysicalDeviceName: " + posPrinter.PhysicalDeviceName + "\npowerState: " + MainActivity.getPowerStateString(posPrinter.PowerState) + "\ncapRecNearEndSensor: " + posPrinter.CapRecNearEndSensor + "\nRecPapercut: " + posPrinter.CapRecPapercut + "\ncapRecMarkFeed: " + getMarkFeedString(posPrinter.CapRecMarkFeed) + "\ncharacterSet: " + posPrinter.CharacterSet + "\ncharacterSetList: " + posPrinter.CharacterSetList + "\nfontTypefaceList: " + posPrinter.FontTypefaceList + "\nrecLineChars: " + posPrinter.RecLineChars + "\nrecLineCharsList: " + posPrinter.RecLineCharsList + "\nrecLineSpacing: " + posPrinter.RecLineSpacing + "\nrecLineWidth: " + posPrinter.RecLineWidth;
				MessageDialogFragment.showDialog(FragmentManager, "Info", message);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", "Exception in Info: " + e.Message);
			}
		}

		private string getMarkFeedString(int markFeed)
		{
			switch (markFeed)
			{
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

		private void checkHealth()
		{
			try
			{
				posPrinter.checkHealth(JposConst.JPOS_CH_INTERNAL);
				posPrinter.checkHealth(JposConst.JPOS_CH_EXTERNAL);
				MessageDialogFragment.showDialog(FragmentManager, "checkHealth", posPrinter.CheckHealthText);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
			}
		}
	}

}