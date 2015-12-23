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
	using AdapterView = android.widget.AdapterView;
	using EditText = android.widget.EditText;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;

	public class POSPrinterBarCodeFragment : POSPrinterFragment, View.OnClickListener, AdapterView.OnItemSelectedListener
	{

		private EditText widthEditText;
		private EditText heightEditText;

		private Spinner symbologySpinner;
		private Spinner alignmentSpinner;
		private Spinner textPositionSpinner;

		private EditText dataEditText;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_bar_code, container, false);

			widthEditText = (EditText) view.findViewById(R.id.editTextWidth);
			heightEditText = (EditText) view.findViewById(R.id.editTextHeight);

			symbologySpinner = (Spinner) view.findViewById(R.id.spinnerSymbology);
			symbologySpinner.OnItemSelectedListener = this;

			alignmentSpinner = (Spinner) view.findViewById(R.id.spinnerAlignment);
			textPositionSpinner = (Spinner) view.findViewById(R.id.spinnerTextPosition);

			dataEditText = (EditText) view.findViewById(R.id.editTextBarCodeData);

			view.findViewById(R.id.buttonPrintBarCode).OnClickListener = this;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;
			return view;
		}

		public override void onClick(View v)
		{
			string data = dataEditText.Text.ToString();
			int symbology = Symbology;

			try
			{
				int height = 0;
				if (heightEditText.Enabled)
				{
					height = Convert.ToInt32(heightEditText.Text.ToString());
				}

				int width = 0;
				if (widthEditText.Enabled)
				{
					width = Convert.ToInt32(widthEditText.Text.ToString());
				}

				int alignment = Alignment;
				int textPosition = TextPosition;

				if (symbology == POSPrinterConst.PTR_BCS_MAXICODE)
				{
					string dummyHeader = StringHelperClass.NewString(new sbyte[] {(sbyte)'[', (sbyte)')', (sbyte)'>', 0x1e, (sbyte)'0', (sbyte)'1', 0x1d, (sbyte)'9', (sbyte)'6', (sbyte)'1', (sbyte)'2', (sbyte)'3', (sbyte)'4', (sbyte)'5', (sbyte)'6', (sbyte)'7', (sbyte)'8', (sbyte)'9', 0x1d, (sbyte)'0', (sbyte)'0', (sbyte)'7', 0x1d, (sbyte)'2', (sbyte)'5', (sbyte)'0', 0x1d});
					data = dummyHeader + data;
				}

				posPrinter.printBarCode(POSPrinterConst.PTR_S_RECEIPT, data, symbology, height, width, alignment, textPosition);
			}
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java 'multi-catch' syntax:
			catch (NumberFormatException | JposException e)
			{
				e.printStackTrace();
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
			}
		}

		public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			string[] barCodeData = Resources.getStringArray(R.array.bar_code_data);
			dataEditText.Text = barCodeData[position];
			switch (position)
			{
			case 0:
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			case 7:
			case 8:
				widthEditText.Hint = R.@string.bar_code_width_hint;
				widthEditText.Enabled = true;
				heightEditText.Hint = R.@string.bar_code_height_hint;
				heightEditText.Enabled = true;
				break;

			case 9:
				widthEditText.Hint = R.@string.pdf417_width_hint;
				widthEditText.Enabled = true;
				heightEditText.Hint = R.@string.pdf417_height_hint;
				heightEditText.Enabled = true;
				break;

			case 10:
				widthEditText.Hint = R.@string.qrcode_size_hint;
				widthEditText.Enabled = true;
				heightEditText.Text = "";
				heightEditText.Hint = "";
				heightEditText.Enabled = false;
				break;

			case 11:
				widthEditText.Text = "";
				widthEditText.Hint = "";
				widthEditText.Enabled = false;
				heightEditText.Text = "";
				heightEditText.Hint = "";
				heightEditText.Enabled = false;
				break;

			case 12:
				widthEditText.Hint = R.@string.datamatrix_size_hint;
				widthEditText.Enabled = true;
				heightEditText.Text = "";
				heightEditText.Hint = "";
				heightEditText.Enabled = false;
				break;
			}
		}

		public override void onNothingSelected<T1>(AdapterView<T1> parent)
		{
			// TODO Auto-generated method stub

		}

		private int Symbology
		{
			get
			{
				switch (symbologySpinner.SelectedItemPosition)
				{
				case 0:
					return POSPrinterConst.PTR_BCS_UPCA;
    
				case 1:
					return POSPrinterConst.PTR_BCS_UPCE;
    
				case 2:
					return POSPrinterConst.PTR_BCS_EAN8;
    
				case 3:
					return POSPrinterConst.PTR_BCS_EAN13;
    
				case 4:
					return POSPrinterConst.PTR_BCS_ITF;
    
				case 5:
					return POSPrinterConst.PTR_BCS_Codabar;
    
				case 6:
					return POSPrinterConst.PTR_BCS_Code39;
    
				case 7:
					return POSPrinterConst.PTR_BCS_Code93;
    
				case 8:
					return POSPrinterConst.PTR_BCS_Code128;
    
				case 9:
					return POSPrinterConst.PTR_BCS_PDF417;
    
				case 10:
					return POSPrinterConst.PTR_BCS_QRCODE;
    
				case 11:
					return POSPrinterConst.PTR_BCS_MAXICODE;
    
				case 12:
					return POSPrinterConst.PTR_BCS_DATAMATRIX;
    
					default:
						return -1;
				}
			}
		}

		private int Alignment
		{
			get
			{
				switch (alignmentSpinner.SelectedItemPosition)
				{
				case 0:
					return POSPrinterConst.PTR_BC_LEFT;
    
				case 1:
					return POSPrinterConst.PTR_BC_CENTER;
    
				case 2:
					return POSPrinterConst.PTR_BC_RIGHT;
    
					default:
						return -1;
				}
			}
		}

		private int TextPosition
		{
			get
			{
				switch (textPositionSpinner.SelectedItemPosition)
				{
				case 0:
					return POSPrinterConst.PTR_BC_TEXT_NONE;
    
				case 1:
					return POSPrinterConst.PTR_BC_TEXT_ABOVE;
    
				case 2:
					return POSPrinterConst.PTR_BC_TEXT_BELOW;
    
					default:
						return -1;
				}
			}
		}
	}

}