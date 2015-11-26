package com.bxl.postest;

import com.bxl.postest.settings.SettingsActivity;

import jpos.JposConst;
import jpos.JposException;
import jpos.POSPrinter;
import jpos.events.DirectIOEvent;
import jpos.events.DirectIOListener;
import jpos.events.ErrorEvent;
import jpos.events.ErrorListener;
import jpos.events.OutputCompleteEvent;
import jpos.events.OutputCompleteListener;
import jpos.events.StatusUpdateEvent;
import jpos.events.StatusUpdateListener;
import android.content.Intent;
import android.content.pm.PackageManager.NameNotFoundException;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.support.v7.app.ActionBarActivity;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

public class MainActivity extends ActionBarActivity 
		implements ErrorListener, OutputCompleteListener, StatusUpdateListener, DirectIOListener,
		DeviceCategoryListFragment.OnClickListener, ListDialogFragment.OnClickListener, FileListDialogFragment.OnClickListener {
	
	private static final String TAG_CASH_DRAWER_FRAGMENT
			= CashDrawerFragment.class.getSimpleName();
	private static final String TAG_MSR_FRAGMENT
			= MSRFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_COMMON_FRAGMENT
			= POSPrinterCommonFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_GENERAL_PRINTING_FRAGMENT
			= POSPrinterGeneralPrintingFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_BAR_CODE_FRAGMENT
			= POSPrinterBarCodeFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_BITMAP_FRAGMENT
			= POSPrinterBitmapFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_PAGE_MODE_FRAGMENT
			= POSPrinterPageModeFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_DIRECT_IO_FRAGMENT
	= POSPrinterDirectIOFragment.class.getSimpleName();
	private static final String TAG_POS_PRINTER_FIRMWARE_FRAGMENT
			= POSPrinterFirmwareFragment.class.getSimpleName();
	private static final String TAG_SMART_CARD_RW_FRAGMENT
			= SmartCardRWFragment.class.getSimpleName();
	
	static final int REQUEST_CODE_SETTINGS = 1;
	static final int REQUEST_CODE_ACTION_PICK = 2;
	
	static final String PREFS_NAME = "LogicalNamePrefsFile";
	static final String KEY_LOGICAL_NAME_CASH_DRAWER = "CashDrawer";
	static final String KEY_LOGICAL_NAME_MSR = "MSR";
	static final String KEY_LOGICAL_NAME_POS_PRINTER = "POSPrinter";
	static final String KEY_LOGICAL_NAME_SMART_CARD_RW = "SmartCardRW";
	
	private POSPrinter posPrinter;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		if (findViewById(R.id.container) != null) {
			getSupportFragmentManager().beginTransaction()
					.add(R.id.container, new DeviceCategoryListFragment()).commit();
		}
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		
		if (posPrinter != null) {
			try {
				posPrinter.close();
			} catch (JposException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
	
	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			Intent intent = new Intent(MainActivity.this, SettingsActivity.class);
			startActivityForResult(intent, REQUEST_CODE_SETTINGS);
			return true;
		}
		return super.onOptionsItemSelected(item);
	}
	
	POSPrinter getPOSPrinter() {
		if (posPrinter == null) {
			posPrinter = new POSPrinter(this);
			posPrinter.addErrorListener(this);
			posPrinter.addOutputCompleteListener(this);
			posPrinter.addStatusUpdateListener(this);
			posPrinter.addDirectIOListener(this);
		}
		
		return posPrinter;
	}
	
	static String getPowerStateString(int powerState) {
		switch (powerState) {
		case JposConst.JPOS_PS_OFF_OFFLINE:
			return "OFFLINE";
			
		case JposConst.JPOS_PS_ONLINE:
			return "ONLINE";
			
			default:
				return "Unknown";
		}
	}

	static String getStatusString(int state){
		switch (state){
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
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == REQUEST_CODE_SETTINGS) {
			if (posPrinter != null) {
				try {
					posPrinter.close();
				} catch (JposException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
			posPrinter = null;
		} else {
			Fragment fragment = getSupportFragmentManager().findFragmentByTag(TAG_POS_PRINTER_BITMAP_FRAGMENT);
			if (fragment != null) {
				fragment.onActivityResult(requestCode, resultCode, data);
			} else {
				super.onActivityResult(requestCode, resultCode, data);
			}
		}
	}

	@Override
	public void statusUpdateOccurred(StatusUpdateEvent e) {
		Fragment fragment = getFragment();
		if (fragment instanceof POSPrinterFragment) {
			((StatusUpdateListener) fragment).statusUpdateOccurred(e);
		}
	}

	@Override
	public void outputCompleteOccurred(OutputCompleteEvent e) {
		Fragment fragment = getFragment();
		if (fragment instanceof POSPrinterFragment) {
			((OutputCompleteListener) fragment).outputCompleteOccurred(e);
		}
	}

	@Override
	public void errorOccurred(ErrorEvent e) {
		Fragment fragment = getFragment();
		if (fragment instanceof POSPrinterFragment) {
			((ErrorListener) fragment).errorOccurred(e);
		}
	}
	
	@Override
	public void directIOOccurred(DirectIOEvent e) {
		Fragment fragment = getFragment();
		if (fragment instanceof POSPrinterFragment) {
			((DirectIOListener) fragment).directIOOccurred(e);
		}
	}

	@Override
	public void onClick(String title, String text) {
		Fragment fragment = getFragment();
		if (fragment instanceof ListDialogFragment.OnClickListener) {
			((ListDialogFragment.OnClickListener) fragment).onClick(title, text);
		}
	}
	
	@Override
	public void onClick(String text) {
		Fragment fragment = getFragment();
		if (fragment instanceof FileListDialogFragment.OnClickListener) {
			((FileListDialogFragment.OnClickListener) fragment).onClick(text);
		}
	}

	@Override
	public boolean onClick(int groupPosition, int childPosition) {
		switch (groupPosition) {
		case 0:
			replaceFragment(new CashDrawerFragment(), TAG_CASH_DRAWER_FRAGMENT);
			return true;
			
		case 1:
			replaceFragment(new MSRFragment(), TAG_MSR_FRAGMENT);
			return true;
			
		case 2:
			switch (childPosition) {
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
			}
			return true;
			
		case 3:
			replaceFragment(new SmartCardRWFragment(), TAG_SMART_CARD_RW_FRAGMENT);
			return true;
		}
		return false;
	}
	
	private Fragment getFragment() {
		if (findViewById(R.id.container) != null) {
			return getSupportFragmentManager().findFragmentById(R.id.container);
		} else {
			return getSupportFragmentManager().findFragmentById(R.id.fragmentDevice);
		}
	}
	
	private void replaceFragment(Fragment fragment, String tag) {
		if (findViewById(R.id.container) != null) {
			getSupportFragmentManager().beginTransaction().replace(R.id.container, fragment, tag)
					.addToBackStack(null).commit();
		} else {
			getSupportFragmentManager().beginTransaction().replace(R.id.fragmentDevice, fragment, tag).commit();
		}
	}
	
	/**
	 * A placeholder fragment containing a simple view.
	 */
	public static class PlaceholderFragment extends Fragment {

		public PlaceholderFragment() {
		}

		@Override
		public View onCreateView(LayoutInflater inflater, ViewGroup container,
				Bundle savedInstanceState) {
			View rootView = inflater.inflate(R.layout.fragment_main, container,
					false);
			TextView textView = (TextView) rootView.findViewById(R.id.textViewVersion);
			try {
				textView.setText("v" + getActivity().getPackageManager()
						.getPackageInfo(getActivity().getPackageName(), 0).versionName);
			} catch (NameNotFoundException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			return rootView;
		}
	}
}
