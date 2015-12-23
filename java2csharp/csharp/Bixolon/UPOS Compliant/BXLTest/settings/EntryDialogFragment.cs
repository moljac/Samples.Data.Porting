using System.Text;

namespace com.bxl.postest.settings
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;
	using InputFilter = android.text.InputFilter;
	using InputType = android.text.InputType;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using EditorInfo = android.view.inputmethod.EditorInfo;
	using AdapterView = android.widget.AdapterView;
	using OnItemSelectedListener = android.widget.AdapterView.OnItemSelectedListener;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using LayoutParams = android.widget.LinearLayout.LayoutParams;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;

	public class EntryDialogFragment : DialogFragment, AdapterView.OnItemSelectedListener
	{

		private const string KEY_TITLE = "title";
		private const string KEY_ENTRY_INFO = "entry_info";

		internal interface OnClickListener
		{
			void onClick(bool isModified, EntryInfo entryInfo);
		}

		private OnClickListener onClickListener;

		private LinearLayout addressLinearLayout;
		private EditText logicalNameEditText;
		private string delimiter;

		private const int LAYOUT_NONE = 0;
		private const int LAYOUT_MAC_ADDRESS = 1;
		private const int LAYOUT_IP_ADDRESS = 2;

		internal static void showDialog(FragmentManager manager, string title, EntryInfo entryInfo)
		{
			Bundle args = new Bundle();
			args.putString(KEY_TITLE, title);
			args.putParcelable(KEY_ENTRY_INFO, entryInfo);

			EntryDialogFragment dialogFragment = new EntryDialogFragment();
			dialogFragment.Arguments = args;
			dialogFragment.show(manager, typeof(EntryDialogFragment).Name);
		}

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			try
			{
				onClickListener = (OnClickListener) activity;
			}
			catch (System.InvalidCastException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				throw new System.InvalidCastException(activity.ToString() + " must implement NoticeDialogListener");
			}
		}

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			EntryInfo entryInfo = Arguments.getParcelable(KEY_ENTRY_INFO);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean isModifying = (entryInfo != null);
			bool isModifying = (entryInfo != null);

			LayoutInflater inflater = Activity.LayoutInflater;

			View view = inflater.inflate(R.layout.dialog_entry, (ViewGroup) Activity.findViewById(android.R.id.content).RootView, false);

			logicalNameEditText = (EditText) view.findViewById(R.id.editTextLogicalName);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.Spinner deviceCategorySpinner = (android.widget.Spinner) view.findViewById(com.bxl.postest.R.id.spinnerDeviceCategory);
			Spinner deviceCategorySpinner = (Spinner) view.findViewById(R.id.spinnerDeviceCategory);
			deviceCategorySpinner.Selection = 2; // POSPrinter

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.Spinner productNameSpinner = (android.widget.Spinner) view.findViewById(com.bxl.postest.R.id.spinnerProductName);
			Spinner productNameSpinner = (Spinner) view.findViewById(R.id.spinnerProductName);
			productNameSpinner.OnItemSelectedListener = this;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.Spinner deviceBusSpinner = (android.widget.Spinner) view.findViewById(com.bxl.postest.R.id.spinnerDeviceBus);
			Spinner deviceBusSpinner = (Spinner) view.findViewById(R.id.spinnerDeviceBus);
			deviceBusSpinner.OnItemSelectedListener = this;
			addressLinearLayout = (LinearLayout) view.findViewById(R.id.linearLayoutAddress);

			if (isModifying)
			{
				logicalNameEditText.Text = entryInfo.LogicalName;
				logicalNameEditText.Enabled = false;
				logicalNameEditText.InputType = InputType.TYPE_NULL;

				string[] deviceCategories = Resources.getStringArray(R.array.device_categories);
				for (int i = 0; i < deviceCategories.Length; i++)
				{
					if (deviceCategories[i].Equals(entryInfo.DeviceCategory))
					{
						deviceCategorySpinner.Selection = i;
						break;
					}
				}
				deviceCategorySpinner.Enabled = false;

				string[] productNames = Resources.getStringArray(R.array.product_name);
				for (int i = 0; i < productNames.Length; i++)
				{
					if (productNames[i].Equals(entryInfo.ProductName))
					{
						productNameSpinner.Selection = i;
						break;
					}
				}
				productNameSpinner.Enabled = false;

				string[] deviceBuses = Resources.getStringArray(R.array.device_bus);
				for (int i = 0; i < deviceBuses.Length; i++)
				{
					if (deviceBuses[i].Equals(entryInfo.DeviceBus))
					{
						deviceBusSpinner.Selection = i;
						break;
					}
				}
			}

			string title = Arguments.getString(KEY_TITLE);
			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.setView(view).setTitle(title).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper(this, entryInfo, isModifying, deviceCategorySpinner, productNameSpinner, deviceBusSpinner))
		   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper2(this));
			return builder.create();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly EntryDialogFragment outerInstance;

			private com.bxl.postest.settings.EntryInfo entryInfo;
			private bool isModifying;
			private Spinner deviceCategorySpinner;
			private Spinner productNameSpinner;
			private Spinner deviceBusSpinner;

			public OnClickListenerAnonymousInnerClassHelper(EntryDialogFragment outerInstance, com.bxl.postest.settings.EntryInfo entryInfo, bool isModifying, Spinner deviceCategorySpinner, Spinner productNameSpinner, Spinner deviceBusSpinner)
			{
				this.outerInstance = outerInstance;
				this.entryInfo = entryInfo;
				this.isModifying = isModifying;
				this.deviceCategorySpinner = deviceCategorySpinner;
				this.productNameSpinner = productNameSpinner;
				this.deviceBusSpinner = deviceBusSpinner;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				EntryInfo entryInfo = new EntryInfo(outerInstance.logicalNameEditText.Text.ToString(), (string) deviceCategorySpinner.SelectedItem, (string) productNameSpinner.SelectedItem, (string) deviceBusSpinner.SelectedItem, outerInstance.AddressText);
				outerInstance.onClickListener.onClick(isModifying, entryInfo);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly EntryDialogFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(EntryDialogFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			switch (parent.Id)
			{
			case R.id.spinnerDeviceBus:
				switch (position)
				{
				case 0: // Bluetooth
				case 4: // Wi-Fi Direct
					setAddressLayout(LAYOUT_MAC_ADDRESS, true);
					delimiter = ":";

					string[] deviceBuses = Resources.getStringArray(R.array.device_bus);
					EntryInfo entryInfo = Arguments.getParcelable(KEY_ENTRY_INFO);
					if (entryInfo != null && entryInfo.DeviceBus.Equals(deviceBuses[position]))
					{
						AddressText = entryInfo.Address;
					}
					break;

				case 1: // Ethernet
				case 3: // Wi-Fi
					setAddressLayout(LAYOUT_IP_ADDRESS, false);
					delimiter = ".";

					deviceBuses = Resources.getStringArray(R.array.device_bus);
					entryInfo = Arguments.getParcelable(KEY_ENTRY_INFO);
					if (entryInfo != null && entryInfo.DeviceBus.Equals(deviceBuses[position]))
					{
						AddressText = entryInfo.Address;
					}
					break;

				case 2: // USB
				default:
					setAddressLayout(LAYOUT_NONE, false);
					break;
				}
				break;

			case R.id.spinnerProductName:
				EntryInfo entryInfo = Arguments.getParcelable(KEY_ENTRY_INFO);
				if (entryInfo == null)
				{
					string text = ((TextView) view).Text.ToString();
					logicalNameEditText.Text = text;
					logicalNameEditText.Selection = text.Length;
				}
				break;
			}
		}

		public override void onNothingSelected<T1>(AdapterView<T1> parent)
		{
			// TODO Auto-generated method stub

		}

		private string AddressText
		{
			get
			{
				StringBuilder buffer = new StringBuilder();
				int childCount = addressLinearLayout.ChildCount;
    
				for (int i = 0; i < childCount; i++)
				{
					EditText editText = (EditText) addressLinearLayout.getChildAt(i);
					buffer.Append(editText.Text.ToString());
    
					if (i < childCount - 1)
					{
						buffer.Append(delimiter);
					}
				}
				return buffer.ToString();
			}
			set
			{
				int index = 0;
				foreach (string text in value.Split("\\:|\\.", true))
				{
					EditText editText = (EditText) addressLinearLayout.getChildAt(index++);
					if (editText != null)
					{
						editText.Text = text;
					}
				}
			}
		}


		private void setAddressLayout(int layout, bool inputTypeClassText)
		{
			addressLinearLayout.removeAllViews();
			int count = 0;
			int maxInputLength = 0;

			if (layout == LAYOUT_MAC_ADDRESS)
			{
				count = 6;
				maxInputLength = 2;
			}
			else if (layout == LAYOUT_IP_ADDRESS)
			{
				count = 4;
				maxInputLength = 3;
			}

			if (count > 0)
			{
				for (int i = 0; i < count; i++)
				{
					EditText editText = new EditText(Activity);
					if (inputTypeClassText)
					{
						editText.InputType = InputType.TYPE_CLASS_TEXT | InputType.TYPE_TEXT_VARIATION_VISIBLE_PASSWORD;
					}
					else
					{
						editText.InputType = InputType.TYPE_CLASS_NUMBER;
					}

					InputFilter[] filters = new InputFilter[1];
					filters[0] = new InputFilter.LengthFilter(maxInputLength);
					editText.Filters = filters;

					if (i == count - 1)
					{
						editText.ImeOptions = EditorInfo.IME_ACTION_DONE;
					}
					else
					{
						editText.ImeOptions = EditorInfo.IME_ACTION_NEXT;
					}

					addressLinearLayout.addView(editText, new LinearLayout.LayoutParams(LinearLayout.LayoutParams.WRAP_CONTENT, LinearLayout.LayoutParams.WRAP_CONTENT, 1f));
				}
			}
		}
	}

}