namespace com.bxl.postest
{

	using JposException = jpos.JposException;
	using Bundle = android.os.Bundle;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using EditText = android.widget.EditText;
	using TextView = android.widget.TextView;


	public class POSPrinterFirmwareFragment : POSPrinterFragment, View.OnClickListener, FileListDialogFragment.OnClickListener
	{

		private EditText fileNameEditText;

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_firmware, container, false);

			fileNameEditText = (EditText) view.findViewById(R.id.editTextFileName);

			view.findViewById(R.id.buttonBrowse).OnClickListener = this;
			view.findViewById(R.id.buttonGo).OnClickListener = this;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);

			return view;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonBrowse:
				FileListDialogFragment.showDialog(FragmentManager);
				break;

			case R.id.buttonGo:
				try
				{
					posPrinter.updateFirmware(fileNameEditText.Text.ToString());
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

		public virtual void onClick(string text)
		{
			fileNameEditText.Text = text;
		}
	}

}