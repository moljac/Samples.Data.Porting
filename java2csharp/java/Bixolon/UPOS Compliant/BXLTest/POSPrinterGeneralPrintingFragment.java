package com.bxl.postest;

import java.util.ArrayList;

import jpos.JposException;
import jpos.POSPrinterConst;
import android.os.Bundle;
import android.support.annotation.Nullable;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.TextView;

public class POSPrinterGeneralPrintingFragment extends POSPrinterFragment
		implements View.OnClickListener, ListDialogFragment.OnClickListener {
	
	private EditText sendToPrinterEditText;
	
	private static String ESCAPE_SEQUENCE = new String(new byte[] {0x1b, 0x7c});
	private static String[] ESCAPE_SEQUENCES = new String[] {
			"ABCDEF\n",
			ESCAPE_SEQUENCE + "uCABCDEF\n",
			ESCAPE_SEQUENCE + "1CABCDEF\n",
			ESCAPE_SEQUENCE + "2CABCDEF\n",
			ESCAPE_SEQUENCE + "3CABCDEF\n",
			ESCAPE_SEQUENCE + "4CABCDEF\n",
			ESCAPE_SEQUENCE + "1hC" + ESCAPE_SEQUENCE + "1vCABCDEF\n",
			ESCAPE_SEQUENCE + "8hC" + ESCAPE_SEQUENCE + "1vCABCDEF\n",
			ESCAPE_SEQUENCE + "8hCABCDEF\n",
			ESCAPE_SEQUENCE + "N\n",
			ESCAPE_SEQUENCE + "rAABCDEF\n",
			ESCAPE_SEQUENCE + "lAABCDEF\n",
			ESCAPE_SEQUENCE + "8hCABCD\n"
	};
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_general_printing, container, false);
		
		sendToPrinterEditText = (EditText) view.findViewById(R.id.editTextSendToPrinter);
		
		view.findViewById(R.id.buttonAddEscapeSequence).setOnClickListener(this);
		view.findViewById(R.id.buttonPrintNormal).setOnClickListener(this);
		view.findViewById(R.id.buttonCutPaper).setOnClickListener(this);
		view.findViewById(R.id.buttonCharacterSet).setOnClickListener(this);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		return view;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonAddEscapeSequence:
			ListDialogFragment.showDialog(getFragmentManager(), "Escape sequences", ESCAPE_SEQUENCES);
			break;
			
		case R.id.buttonPrintNormal:
			try {
				posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, sendToPrinterEditText.getText().toString());
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonCutPaper:
			try {
				posPrinter.cutPaper(90);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			break;
			
		case R.id.buttonCharacterSet:
			try {
				String characterSetList = posPrinter.getCharacterSetList();
				ArrayList<String> arrayList = new ArrayList<String>();
				for (String token : characterSetList.split(",")) {
					arrayList.add(token);
				}
				String[] items = arrayList.toArray(new String[arrayList.size()]);
				ListDialogFragment.showDialog(getFragmentManager(), getString(R.string.character_set), items);
			} catch (JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
			
			break;
		}
	}
	
	@Override
	public void onClick(String title, String text) {
		if (title.equals("Escape sequences")) {
			sendToPrinterEditText.append(text);
		} else {
			try {
				int characterSet = Integer.valueOf(text);
				posPrinter.setCharacterSet(characterSet);
				
				switch (characterSet) {
				case 437:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page437));
					break;
					
				case 737:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page737));
					break;
					
				case 775:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page775));
					break;
					
				case 850:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page850));
					break;
					
				case 852:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page852));
					break;
					
				case 855:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page855));
					break;
					
				case 857:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page857));
					break;
					
				case 858:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page858));
					break;
					
				case 860:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page860));
					break;
					
				case 862:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page862));
					break;
					
				case 863:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page863));
					break;
					
				case 864:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page864));
					break;
					
				case 865:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page865));
					break;
					
				case 866:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page866));
					break;
					
				case 928:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page928));
					break;
					
				case 1250:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1250));
					break;
					
				case 1251:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1251));
					break;
					
				case 1252:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1252));
					break;
					
				case 1253:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1253));
					break;
					
				case 1254:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1254));
					break;
					
				case 1255:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1255));
					break;
					
				case 1256:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1256));
					break;
					
				case 1257:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1257));
					break;
					
				case 1258:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_windows1258));
					break;
					
				case 7065:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_farsi));
					break;
					
				case 7565:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_katakana));
					break;
					
				case 7572:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_khmer_cambodia));
					break;
					
				case 8411:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_thai11));
					break;
					
				case 8414:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_thai14));
					break;
					
				case 8416:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_thai16));
					break;
					
				case 8418:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_thai18));
					break;
					
				case 8442:
					posPrinter.printNormal(POSPrinterConst.PTR_S_RECEIPT, getString(R.string.code_page_thai42));
					break;
				}
			} catch (NumberFormatException | JposException e) {
				e.printStackTrace();
				MessageDialogFragment.showDialog(getFragmentManager(), "Excepction", e.getMessage());
			}
		}
	}
}
