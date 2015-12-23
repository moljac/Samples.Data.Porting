using System;

namespace com.bxl.postest
{

	using JposException = jpos.JposException;
	using POSPrinterConst = jpos.POSPrinterConst;
	using Bundle = android.os.Bundle;
	using Nullable = android.support.annotation.Nullable;
	using ScrollingMovementMethod = android.text.method.ScrollingMovementMethod;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using EditText = android.widget.EditText;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;

	public class POSPrinterPageModeFragment : POSPrinterFragment, View.OnClickListener
	{

		private TextView pageModeAreaTextView;
		private EditText pageModeHorizontalPositionEditText;
		private EditText pageModeVerticalPositionEditText;
		private EditText pageModePrintAreaEditText1;
		private EditText pageModePrintAreaEditText2;
		private EditText pageModePrintAreaEditText3;
		private EditText pageModePrintAreaEditText4;
		private Spinner pageModePrintDirectionSpinner;
		private Spinner pageModeCommandSpinner;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_page_mode, container, false);

			pageModeAreaTextView = (TextView) view.findViewById(R.id.textViewPageModeArea);
			try
			{
				pageModeAreaTextView.Text = posPrinter.PageModeArea;
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			pageModeHorizontalPositionEditText = (EditText) view.findViewById(R.id.editTextPageModeHorizontalPosition);
			try
			{
				pageModeHorizontalPositionEditText.Text = Convert.ToString(posPrinter.PageModeHorizontalPosition);
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			pageModeVerticalPositionEditText = (EditText) view.findViewById(R.id.editTextPageModeVerticalPosition);
			try
			{
				pageModeVerticalPositionEditText.Text = Convert.ToString(posPrinter.PageModeVerticalPosition);
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			pageModePrintAreaEditText1 = (EditText) view.findViewById(R.id.editTextPageModePrintArea1);
			pageModePrintAreaEditText2 = (EditText) view.findViewById(R.id.editTextPageModePrintArea2);
			pageModePrintAreaEditText3 = (EditText) view.findViewById(R.id.editTextPageModePrintArea3);
			pageModePrintAreaEditText4 = (EditText) view.findViewById(R.id.editTextPageModePrintArea4);
			try
			{
				string[] pageModePrintArea = posPrinter.PageModePrintArea.split(",");
				pageModePrintAreaEditText1.Text = pageModePrintArea[0];
				pageModePrintAreaEditText2.Text = pageModePrintArea[1];
				pageModePrintAreaEditText3.Text = pageModePrintArea[2];
				pageModePrintAreaEditText4.Text = pageModePrintArea[3];
			}
			catch (JposException e1)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
			}

			pageModePrintDirectionSpinner = (Spinner) view.findViewById(R.id.spinnerPageModePrintDirection);
			try
			{
				switch (posPrinter.PageModePrintDirection)
				{
				case POSPrinterConst.PTR_PD_LEFT_TO_RIGHT:
					pageModePrintDirectionSpinner.Selection = 0;
					break;

				case POSPrinterConst.PTR_PD_BOTTOM_TO_TOP:
					pageModePrintDirectionSpinner.Selection = 1;
					break;

				case POSPrinterConst.PTR_PD_RIGHT_TO_LEFT:
					pageModePrintDirectionSpinner.Selection = 2;
					break;

				case POSPrinterConst.PTR_PD_TOP_TO_BOTTOM:
					pageModePrintDirectionSpinner.Selection = 3;
					break;
				}
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			pageModeCommandSpinner = (Spinner) view.findViewById(R.id.spinnerPageModeCommand);

			view.findViewById(R.id.buttonClearPrintArea).OnClickListener = this;
			view.findViewById(R.id.buttonUpdateProperties).OnClickListener = this;
			view.findViewById(R.id.buttonSendPageModeCommand).OnClickListener = this;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;
			return view;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonClearPrintArea:
				clearPrintArea();
				break;

			case R.id.buttonUpdateProperties:
				updateProperties();
				break;

			case R.id.buttonSendPageModeCommand:
				sendPageModeCommand();
				break;
			}
		}

		private void clearPrintArea()
		{
			try
			{
				posPrinter.clearPrintArea();
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
			}
		}

		private void updateProperties()
		{
			try
			{
				pageModeAreaTextView.Text = posPrinter.PageModeArea;

				string area = pageModePrintAreaEditText1.Text.ToString() + "," + pageModePrintAreaEditText2.Text.ToString() + "," + pageModePrintAreaEditText3.Text.ToString() + "," + pageModePrintAreaEditText4.Text.ToString();
				posPrinter.PageModePrintArea = area;

				int direction = PageModePrintDirection;
				posPrinter.PageModePrintDirection = direction;

				int position = Convert.ToInt32(pageModeHorizontalPositionEditText.Text.ToString());
				posPrinter.PageModeHorizontalPosition = position;

				position = Convert.ToInt32(pageModeVerticalPositionEditText.Text.ToString());
				posPrinter.PageModeVerticalPosition = position;
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
			}
		}

		private void sendPageModeCommand()
		{
			try
			{
				switch (pageModeCommandSpinner.SelectedItemPosition)
				{
				case 0:
					posPrinter.pageModePrint(POSPrinterConst.PTR_PM_PAGE_MODE);
					break;

				case 1:
					posPrinter.pageModePrint(POSPrinterConst.PTR_PM_NORMAL);
					break;

				case 2:
					posPrinter.pageModePrint(POSPrinterConst.PTR_PM_CANCEL);
					break;
				}
			}
			catch (JposException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
			}
		}

		private int PageModePrintDirection
		{
			get
			{
				switch (pageModePrintDirectionSpinner.SelectedItemPosition)
				{
				case 0:
					return POSPrinterConst.PTR_PD_LEFT_TO_RIGHT;
    
				case 1:
					return POSPrinterConst.PTR_PD_BOTTOM_TO_TOP;
    
				case 2:
					return POSPrinterConst.PTR_PD_RIGHT_TO_LEFT;
    
				case 3:
					return POSPrinterConst.PTR_PD_TOP_TO_BOTTOM;
    
					default:
						return -1;
				}
			}
		}
	}

}