package com.bxl.postest;

import jpos.JposConst;
import jpos.POSPrinter;
import jpos.POSPrinterConst;
import jpos.events.DirectIOEvent;
import jpos.events.DirectIOListener;
import jpos.events.ErrorEvent;
import jpos.events.ErrorListener;
import jpos.events.OutputCompleteEvent;
import jpos.events.OutputCompleteListener;
import jpos.events.StatusUpdateEvent;
import jpos.events.StatusUpdateListener;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.text.Layout;
import android.widget.TextView;

public abstract class POSPrinterFragment extends Fragment
		implements StatusUpdateListener, OutputCompleteListener, ErrorListener, DirectIOListener {
	
	protected POSPrinter posPrinter;
	
	protected TextView deviceMessagesTextView;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		posPrinter = ((MainActivity) getActivity()).getPOSPrinter();
	}

	@Override
	public void statusUpdateOccurred(final StatusUpdateEvent e) {
		getActivity().runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				deviceMessagesTextView.append("SUE: " + getSUEMessage(e.getStatus()) + "\n");
				scroll();
			}
		});
	}

	@Override
	public void outputCompleteOccurred(final OutputCompleteEvent e) {
		getActivity().runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				deviceMessagesTextView.append("OCE: " + e.getOutputID() + "\n");
				scroll();
			}
		});
	}

	@Override
	public void errorOccurred(final ErrorEvent e) {
		getActivity().runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				deviceMessagesTextView.append("Error: " + e + "\n");
				scroll();
			}
		});
	}
	
	@Override
	public void directIOOccurred(final DirectIOEvent e) {
		getActivity().runOnUiThread(new Runnable() {
			
			@Override
			public void run() {
				deviceMessagesTextView.append("DirectIO: " + e + "(" + getBatterStatusString(e.getData()) + ")\n");
				
				if (e.getObject() != null) {
					deviceMessagesTextView.append(new String((byte[]) e.getObject()) + "\n");
				}
				
				scroll();
			}
		});
	}
	
	private void scroll() {
		Layout layout = deviceMessagesTextView.getLayout();
		if (layout != null) {
			int y = layout.getLineTop(
					deviceMessagesTextView.getLineCount()) - deviceMessagesTextView.getHeight();
			if (y > 0) {
				deviceMessagesTextView.scrollTo(0, y);
				deviceMessagesTextView.invalidate();
			}
		}
	}
	
	private String getSUEMessage(int status){
		switch(status){
		case JposConst.JPOS_SUE_POWER_OFF_OFFLINE:
			return "Power off";
			
		case POSPrinterConst.PTR_SUE_COVER_OPEN:
			return "Cover Open";
			
		case POSPrinterConst.PTR_SUE_COVER_OK:
			return "Cover OK";
			
		case POSPrinterConst.PTR_SUE_REC_EMPTY:
			return "Receipt Paper Empty";
			
		case POSPrinterConst.PTR_SUE_REC_NEAREMPTY:
			return "Receipt Paper Near Empty";
			
		case POSPrinterConst.PTR_SUE_REC_PAPEROK:
			return "Receipt Paper OK";
			
		case POSPrinterConst.PTR_SUE_IDLE:
			return "Printer Idle";
			
			default:
				return "Unknown";
		}
	}
	
	private String getBatterStatusString(int status) {
		switch (status) {
		case 0x30:
			return "Full";
			
		case 0x31:
			return "High";
			
		case 0x32:
			return "Middle";
			
		case 0x33:
			return "Low";
			
			default:
				return "Unknwon";
		}
	}
}
