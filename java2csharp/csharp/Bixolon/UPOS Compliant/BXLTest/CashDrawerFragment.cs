namespace com.bxl.postest
{

	using CashDrawer = jpos.CashDrawer;
	using CashDrawerConst = jpos.CashDrawerConst;
	using JposConst = jpos.JposConst;
	using JposException = jpos.JposException;
	using StatusUpdateEvent = jpos.events.StatusUpdateEvent;
	using StatusUpdateListener = jpos.events.StatusUpdateListener;
	using Context = android.content.Context;
	using SharedPreferences = android.content.SharedPreferences;
	using Bundle = android.os.Bundle;
	using CountDownTimer = android.os.CountDownTimer;
	using Nullable = android.support.annotation.Nullable;
	using Fragment = android.support.v4.app.Fragment;
	using ScrollingMovementMethod = android.text.method.ScrollingMovementMethod;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;

	public class CashDrawerFragment : Fragment, View.OnClickListener, CompoundButton.OnCheckedChangeListener, StatusUpdateListener
	{

		private CashDrawer cashDrawer;

		private EditText logicalNameEditText;
		private TextView stateTextView;
		private TextView deviceMessagesTextView;

		private CheckBox deviceEnabledCheckBox;

		private CountDownTimer countDownTimer;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			cashDrawer = new CashDrawer();
			cashDrawer.addStatusUpdateListener(this);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_cash_drawer, container, false);

			SharedPreferences settings = Activity.getSharedPreferences(MainActivity.PREFS_NAME, Context.MODE_PRIVATE);
			logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
			logicalNameEditText.Text = settings.getString(MainActivity.KEY_LOGICAL_NAME_POS_PRINTER, getString(R.@string.cash_drawer));

			view.findViewById(R.id.buttonOpen).OnClickListener = this;
			view.findViewById(R.id.buttonClaim).OnClickListener = this;
			view.findViewById(R.id.buttonRelease).OnClickListener = this;
			view.findViewById(R.id.buttonClose).OnClickListener = this;
			view.findViewById(R.id.buttonInfo).OnClickListener = this;
			view.findViewById(R.id.buttonCheckHealth).OnClickListener = this;

			deviceEnabledCheckBox = (CheckBox) view.findViewById(R.id.checkBoxDeviceEnabled);
			deviceEnabledCheckBox.OnCheckedChangeListener = this;

			view.findViewById(R.id.buttonOpenDrawer).OnClickListener = this;
			view.findViewById(R.id.buttonGetDrawerOpened).OnClickListener = this;

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
			editor.putString(MainActivity.KEY_LOGICAL_NAME_CASH_DRAWER, logicalNameEditText.Text.ToString());
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
			private readonly CashDrawerFragment outerInstance;

			public CountDownTimerAnonymousInnerClassHelper(CashDrawerFragment outerInstance, UnknownType MAX_VALUE) : base(MAX_VALUE, 1000)
			{
				this.outerInstance = outerInstance;
			}


			public override void onTick(long millisUntilFinished)
			{
				outerInstance.stateTextView.Text = MainActivity.getStatusString(outerInstance.cashDrawer.State);
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
				cashDrawer.close();
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
				cashDrawer.DeviceEnabled = isChecked;
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				try
				{
					cashDrawer.DeviceEnabled = !isChecked;
				}
				catch (JposException e1)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e1.ToString());
					Console.Write(e1.StackTrace);
				}
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
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
					cashDrawer.open(logicalDeviceName);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}

				try
				{
					deviceEnabledCheckBox.Checked = cashDrawer.DeviceEnabled;
				}
				catch (JposException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				break;

			case R.id.buttonClaim:
				try
				{
					cashDrawer.claim(0);
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
					cashDrawer.release();
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
					cashDrawer.close();
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

			case R.id.buttonOpenDrawer:
				try
				{
					cashDrawer.openDrawer();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonGetDrawerOpened:
				try
				{
					if (cashDrawer.DrawerOpened)
					{
						deviceMessagesTextView.append("Cash drawer is open.\n");
					}
					else
					{
						deviceMessagesTextView.append("Cash drawer is closed.\n");
					}
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

		public override void statusUpdateOccurred(StatusUpdateEvent e)
		{
			string text = "Status Update Event: ";
			switch (e.Status)
			{
			case CashDrawerConst.CASH_SUE_DRAWERCLOSED:
				text += "Drawer Closed\n";
				break;

			case CashDrawerConst.CASH_SUE_DRAWEROPEN:
				text += "Drawer Opened\n";
				break;

			default:
				text += "Unknown Status: " + e.Status + "\n";
				break;
			}
			deviceMessagesTextView.append(text);
		}

		private void info()
		{
			string message;
			try
			{
				message = "deviceServiceDescription: " + cashDrawer.DeviceServiceDescription + "\ndeviceServiceVersion: " + cashDrawer.DeviceServiceVersion + "\nphysicalDeviceDescription: " + cashDrawer.PhysicalDeviceDescription + "\nphysicalDeviceName: " + cashDrawer.PhysicalDeviceName + "\npowerState: " + MainActivity.getPowerStateString(cashDrawer.PowerState);
				MessageDialogFragment.showDialog(FragmentManager, "Info", message);
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", "Exception in Info: " + e.Message);
			}
		}

		private void checkHealth()
		{
			try
			{
				cashDrawer.checkHealth(JposConst.JPOS_CH_INTERNAL);
				MessageDialogFragment.showDialog(FragmentManager, "checkHealth", cashDrawer.CheckHealthText);
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