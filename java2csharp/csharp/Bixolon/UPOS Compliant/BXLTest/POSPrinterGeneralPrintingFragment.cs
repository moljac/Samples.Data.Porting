using System;
using System.Collections.Generic;

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
	using TextView = android.widget.TextView;

	public class POSPrinterGeneralPrintingFragment : POSPrinterFragment, View.OnClickListener, ListDialogFragment.OnClickListener
	{

		private EditText sendToPrinterEditText;

		private static string ESCAPE_SEQUENCE = StringHelperClass.NewString(new sbyte[] {0x1b, 0x7c});
		private static string[] ESCAPE_SEQUENCES = new string[] {"ABCDEF\n", ESCAPE_SEQUENCE + "uCABCDEF\n", ESCAPE_SEQUENCE + "1CABCDEF\n", ESCAPE_SEQUENCE + "2CABCDEF\n", ESCAPE_SEQUENCE + "3CABCDEF\n", ESCAPE_SEQUENCE + "4CABCDEF\n", ESCAPE_SEQUENCE + "1hC" + ESCAPE_SEQUENCE + "1vCABCDEF\n", ESCAPE_SEQUENCE + "8hC" + ESCAPE_SEQUENCE + "1vCABCDEF\n", ESCAPE_SEQUENCE + "8hCABCDEF\n", ESCAPE_SEQUENCE + "N\n", ESCAPE_SEQUENCE + "rAABCDEF\n", ESCAPE_SEQUENCE + "lAABCDEF\n", ESCAPE_SEQUENCE + "8hCABCD\n"};

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_general_printing, container, false);

			sendToPrinterEditText = (EditText) view.findViewById(R.id.editTextSendToPrinter);

			view.findViewById(R.id.buttonAddEscapeSequence).OnClickListener = this;
			view.findViewById(R.id.buttonPrintNormal).OnClickListener = this;
			view.findViewById(R.id.buttonCutPaper).OnClickListener = this;
			view.findViewById(R.id.buttonCharacterSet).OnClickListener = this;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;
			return view;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonAddEscapeSequence:
				ListDialogFragment.showDialog(FragmentManager, "Escape sequences", ESCAPE_SEQUENCES);
				break;

			case R.id.buttonPrintNormal:
				try
				{
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, sendToPrinterEditText.Text.ToString());
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonCutPaper:
				try
				{
					posPrinter.cutPaper(90);
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
				break;

			case R.id.buttonCharacterSet:
				try
				{
					string characterSetList = posPrinter.CharacterSetList;
					List<string> arrayList = new List<string>();
					foreach (string token in characterSetList.Split(",", true))
					{
						arrayList.Add(token);
					}
					string[] items = arrayList.ToArray();
					ListDialogFragment.showDialog(FragmentManager, getString(R.@string.character_set), items);
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

		public virtual void onClick(string title, string text)
		{
			if (title.Equals("Escape sequences"))
			{
				sendToPrinterEditText.append(text);
			}
			else
			{
				try
				{
					int characterSet = Convert.ToInt32(text);
					posPrinter.CharacterSet = characterSet;

					switch (characterSet)
					{
					case 437:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page437));
						break;

					case 737:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page737));
						break;

					case 775:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page775));
						break;

					case 850:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page850));
						break;

					case 852:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page852));
						break;

					case 855:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page855));
						break;

					case 857:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page857));
						break;

					case 858:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page858));
						break;

					case 860:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page860));
						break;

					case 862:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page862));
						break;

					case 863:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page863));
						break;

					case 864:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page864));
						break;

					case 865:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page865));
						break;

					case 866:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page866));
						break;

					case 928:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page928));
						break;

					case 1250:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1250));
						break;

					case 1251:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1251));
						break;

					case 1252:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1252));
						break;

					case 1253:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1253));
						break;

					case 1254:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1254));
						break;

					case 1255:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1255));
						break;

					case 1256:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1256));
						break;

					case 1257:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1257));
						break;

					case 1258:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_windows1258));
						break;

					case 7065:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_farsi));
						break;

					case 7565:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_katakana));
						break;

					case 7572:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_khmer_cambodia));
						break;

					case 8411:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_thai11));
						break;

					case 8414:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_thai14));
						break;

					case 8416:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_thai16));
						break;

					case 8418:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_thai18));
						break;

					case 8442:
						posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.@string.code_page_thai42));
						break;
					}
				}
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java 'multi-catch' syntax:
				catch (NumberFormatException | JposException e)
				{
					e.printStackTrace();
					MessageDialogFragment.showDialog(FragmentManager, "Excepction", e.Message);
				}
			}
		}
	}

}