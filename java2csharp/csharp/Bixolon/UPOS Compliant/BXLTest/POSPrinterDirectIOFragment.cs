namespace com.bxl.postest
{

	using JposException = jpos.JposException;
	using Bundle = android.os.Bundle;
	using ScrollingMovementMethod = android.text.method.ScrollingMovementMethod;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;

	public class POSPrinterDirectIOFragment : POSPrinterFragment, View.OnClickListener
	{

		private static readonly string[] DATA_STRING = new string[] {"0x10 0x04 0x02", "0x10 0x04 0x04", "0x1d 0x49 0x41", "0x1d 0x49 0x42", "0x1d 0x49 0x43", "0x1d 0x49 0x45"};

		private static readonly sbyte[][] DATA = new sbyte[][]
		{
			new sbyte[] {0x10, 0x04, 0x02},
			new sbyte[] {0x10, 0x04, 0x04},
			new sbyte[] {0x1d, 0x49, 0x41},
			new sbyte[] {0x1d, 0x49, 0x42},
			new sbyte[] {0x1d, 0x49, 0x43},
			new sbyte[] {0x1d, 0x49, 0x45}
		};

		private Spinner dataSpinner;

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_direct_io, container, false);

			view.findViewById(R.id.buttonDirectIO).OnClickListener = this;
			view.findViewById(R.id.buttonBatteryStatus).OnClickListener = this;

			dataSpinner = (Spinner) view.findViewById(R.id.spinnerData);
			ArrayAdapter<CharSequence> adapter = new ArrayAdapter<CharSequence>(Activity, android.R.layout.simple_spinner_item, DATA_STRING);
			adapter.DropDownViewResource = android.R.layout.simple_spinner_dropdown_item;
			dataSpinner.Adapter = adapter;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;

			return view;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonDirectIO:
				int selectedItemPosition = dataSpinner.SelectedItemPosition;

				try
				{
					posPrinter.directIO(1, null, DATA[selectedItemPosition]);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
				}
				break;

			case R.id.buttonBatteryStatus:
				try
				{
					posPrinter.directIO(2, null, null);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
				}
				break;
			}
		}
	}

}