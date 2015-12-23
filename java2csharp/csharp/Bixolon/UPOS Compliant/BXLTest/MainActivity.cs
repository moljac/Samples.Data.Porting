namespace com.bxl.postest
{

	using SettingsActivity = com.bxl.postest.settings.SettingsActivity;

	using JposConst = jpos.JposConst;
	using JposException = jpos.JposException;
	using POSPrinter = jpos.POSPrinter;
	using DirectIOEvent = jpos.events.DirectIOEvent;
	using DirectIOListener = jpos.events.DirectIOListener;
	using ErrorEvent = jpos.events.ErrorEvent;
	using ErrorListener = jpos.events.ErrorListener;
	using OutputCompleteEvent = jpos.events.OutputCompleteEvent;
	using OutputCompleteListener = jpos.events.OutputCompleteListener;
	using StatusUpdateEvent = jpos.events.StatusUpdateEvent;
	using StatusUpdateListener = jpos.events.StatusUpdateListener;
	using Intent = android.content.Intent;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using LayoutInflater = android.view.LayoutInflater;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using TextView = android.widget.TextView;

	public class MainActivity : ActionBarActivity, ErrorListener, OutputCompleteListener, StatusUpdateListener, DirectIOListener, DeviceCategoryListFragment.OnClickListener, ListDialogFragment.OnClickListener, FileListDialogFragment.OnClickListener
	{

		private static readonly string TAG_CASH_DRAWER_FRAGMENT = typeof(CashDrawerFragment).Name;
		private static readonly string TAG_MSR_FRAGMENT = typeof(MSRFragment).Name;
		private static readonly string TAG_POS_PRINTER_COMMON_FRAGMENT = typeof(POSPrinterCommonFragment).Name;
		private static readonly string TAG_POS_PRINTER_GENERAL_PRINTING_FRAGMENT = typeof(POSPrinterGeneralPrintingFragment).Name;
		private static readonly string TAG_POS_PRINTER_BAR_CODE_FRAGMENT = typeof(POSPrinterBarCodeFragment).Name;
		private static readonly string TAG_POS_PRINTER_BITMAP_FRAGMENT = typeof(POSPrinterBitmapFragment).Name;
		private static readonly string TAG_POS_PRINTER_PAGE_MODE_FRAGMENT = typeof(POSPrinterPageModeFragment).Name;
		private static readonly string TAG_POS_PRINTER_DIRECT_IO_FRAGMENT = typeof(POSPrinterDirectIOFragment).Name;
		private static readonly string TAG_POS_PRINTER_FIRMWARE_FRAGMENT = typeof(POSPrinterFirmwareFragment).Name;
		private static readonly string TAG_SMART_CARD_RW_FRAGMENT = typeof(SmartCardRWFragment).Name;

		internal const int REQUEST_CODE_SETTINGS = 1;
		internal const int REQUEST_CODE_ACTION_PICK = 2;

		internal const string PREFS_NAME = "LogicalNamePrefsFile";
		internal const string KEY_LOGICAL_NAME_CASH_DRAWER = "CashDrawer";
		internal const string KEY_LOGICAL_NAME_MSR = "MSR";
		internal const string KEY_LOGICAL_NAME_POS_PRINTER = "POSPrinter";
		internal const string KEY_LOGICAL_NAME_SMART_CARD_RW = "SmartCardRW";

		private POSPrinter posPrinter;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			if (findViewById(R.id.container) != null)
			{
				SupportFragmentManager.beginTransaction().add(R.id.container, new DeviceCategoryListFragment()).commit();
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();

			if (posPrinter != null)
			{
				try
				{
					posPrinter.close();
				}
				catch (JposException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.main, menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			// Handle action bar item clicks here. The action bar will
			// automatically handle clicks on the Home/Up button, so long
			// as you specify a parent activity in AndroidManifest.xml.
			int id = item.ItemId;
			if (id == R.id.action_settings)
			{
				Intent intent = new Intent(MainActivity.this, typeof(SettingsActivity));
				startActivityForResult(intent, REQUEST_CODE_SETTINGS);
				return true;
			}
			return base.onOptionsItemSelected(item);
		}

		internal virtual POSPrinter POSPrinter
		{
			get
			{
				if (posPrinter == null)
				{
					posPrinter = new POSPrinter(this);
					posPrinter.addErrorListener(this);
					posPrinter.addOutputCompleteListener(this);
					posPrinter.addStatusUpdateListener(this);
					posPrinter.addDirectIOListener(this);
				}
    
				return posPrinter;
			}
		}

		internal static string getPowerStateString(int powerState)
		{
			switch (powerState)
			{
			case JposConst.JPOS_PS_OFF_OFFLINE:
				return "OFFLINE";

			case JposConst.JPOS_PS_ONLINE:
				return "ONLINE";

				default:
					return "Unknown";
			}
		}

		internal static string getStatusString(int state)
		{
			switch (state)
			{
			case JposConst.JPOS_S_BUSY:
				return "JPOS_S_BUSY";

			case JposConst.JPOS_S_CLOSED:
				return "JPOS_S_CLOSED";

			case JposConst.JPOS_S_ERROR:
				return "JPOS_S_ERROR";

			case JposConst.JPOS_S_IDLE:
				return "JPOS_S_IDLE";

			default:
				return "Unknown State";
			}
		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			if (requestCode == REQUEST_CODE_SETTINGS)
			{
				if (posPrinter != null)
				{
					try
					{
						posPrinter.close();
					}
					catch (JposException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
				posPrinter = null;
			}
			else
			{
				Fragment fragment = SupportFragmentManager.findFragmentByTag(TAG_POS_PRINTER_BITMAP_FRAGMENT);
				if (fragment != null)
				{
					fragment.onActivityResult(requestCode, resultCode, data);
				}
				else
				{
					base.onActivityResult(requestCode, resultCode, data);
				}
			}
		}

		public override void statusUpdateOccurred(StatusUpdateEvent e)
		{
			Fragment fragment = Fragment;
			if (fragment is POSPrinterFragment)
			{
				((StatusUpdateListener) fragment).statusUpdateOccurred(e);
			}
		}

		public override void outputCompleteOccurred(OutputCompleteEvent e)
		{
			Fragment fragment = Fragment;
			if (fragment is POSPrinterFragment)
			{
				((OutputCompleteListener) fragment).outputCompleteOccurred(e);
			}
		}

		public override void errorOccurred(ErrorEvent e)
		{
			Fragment fragment = Fragment;
			if (fragment is POSPrinterFragment)
			{
				((ErrorListener) fragment).errorOccurred(e);
			}
		}

		public override void directIOOccurred(DirectIOEvent e)
		{
			Fragment fragment = Fragment;
			if (fragment is POSPrinterFragment)
			{
				((DirectIOListener) fragment).directIOOccurred(e);
			}
		}

		public virtual void onClick(string title, string text)
		{
			Fragment fragment = Fragment;
			if (fragment is ListDialogFragment.OnClickListener)
			{
				((ListDialogFragment.OnClickListener) fragment).onClick(title, text);
			}
		}

		public virtual void onClick(string text)
		{
			Fragment fragment = Fragment;
			if (fragment is FileListDialogFragment.OnClickListener)
			{
				((FileListDialogFragment.OnClickListener) fragment).onClick(text);
			}
		}

		public virtual bool onClick(int groupPosition, int childPosition)
		{
			switch (groupPosition)
			{
			case 0:
				replaceFragment(new CashDrawerFragment(), TAG_CASH_DRAWER_FRAGMENT);
				return true;

			case 1:
				replaceFragment(new MSRFragment(), TAG_MSR_FRAGMENT);
				return true;

			case 2:
				switch (childPosition)
				{
				case 0:
					replaceFragment(new POSPrinterCommonFragment(), TAG_POS_PRINTER_COMMON_FRAGMENT);
					break;

				case 1:
					replaceFragment(new POSPrinterGeneralPrintingFragment(), TAG_POS_PRINTER_GENERAL_PRINTING_FRAGMENT);
					break;

				case 2:
					replaceFragment(new POSPrinterBarCodeFragment(), TAG_POS_PRINTER_BAR_CODE_FRAGMENT);
					break;

				case 3:
					replaceFragment(new POSPrinterBitmapFragment(), TAG_POS_PRINTER_BITMAP_FRAGMENT);
					break;

				case 4:
					replaceFragment(new POSPrinterPageModeFragment(), TAG_POS_PRINTER_PAGE_MODE_FRAGMENT);
					break;

				case 5:
					replaceFragment(new POSPrinterDirectIOFragment(), TAG_POS_PRINTER_DIRECT_IO_FRAGMENT);
					break;

				case 6:
					replaceFragment(new POSPrinterFirmwareFragment(), TAG_POS_PRINTER_FIRMWARE_FRAGMENT);
				break;
				}
				return true;

			case 3:
				replaceFragment(new SmartCardRWFragment(), TAG_SMART_CARD_RW_FRAGMENT);
				return true;
			}
			return false;
		}

		private Fragment Fragment
		{
			get
			{
				if (findViewById(R.id.container) != null)
				{
					return SupportFragmentManager.findFragmentById(R.id.container);
				}
				else
				{
					return SupportFragmentManager.findFragmentById(R.id.fragmentDevice);
				}
			}
		}

		private void replaceFragment(Fragment fragment, string tag)
		{
			if (findViewById(R.id.container) != null)
			{
				SupportFragmentManager.beginTransaction().replace(R.id.container, fragment, tag).addToBackStack(null).commit();
			}
			else
			{
				SupportFragmentManager.beginTransaction().replace(R.id.fragmentDevice, fragment, tag).commit();
			}
		}

		/// <summary>
		/// A placeholder fragment containing a simple view.
		/// </summary>
		public class PlaceholderFragment : Fragment
		{

			public PlaceholderFragment()
			{
			}

			public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
			{
				View rootView = inflater.inflate(R.layout.fragment_main, container, false);
				TextView textView = (TextView) rootView.findViewById(R.id.textViewVersion);
				try
				{
					textView.Text = "v" + Activity.PackageManager.getPackageInfo(Activity.PackageName, 0).versionName;
				}
				catch (NameNotFoundException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				return rootView;
			}
		}
	}

}