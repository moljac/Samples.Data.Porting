package com.bxl.postest;

import java.nio.ByteBuffer;

import jpos.JposException;
import jpos.POSPrinterConst;
import android.content.ContentResolver;
import android.content.Intent;
import android.database.Cursor;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.support.annotation.Nullable;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.EditText;
import android.widget.SeekBar;
import android.widget.SeekBar.OnSeekBarChangeListener;
import android.widget.Spinner;
import android.widget.TextView;

public class POSPrinterBitmapFragment extends POSPrinterFragment
		implements View.OnClickListener, OnSeekBarChangeListener {
	
	private TextView bitmapPathTextView;
	private EditText widthEditText;
	private Spinner alignmentSpinner;
	private SeekBar brightnessSeekBar;
	private TextView brightnessTextView;
	
	@Override
	public View onCreateView(LayoutInflater inflater,
			@Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
		View view = inflater.inflate(R.layout.fragment_pos_printer_bitmap, container, false);
		
		bitmapPathTextView = (TextView) view.findViewById(R.id.textViewBitmapPath);
		widthEditText = (EditText) view.findViewById(R.id.editTextWidth);
		alignmentSpinner = (Spinner) view.findViewById(R.id.spinnerAlignment);
		
		view.findViewById(R.id.buttonGallery).setOnClickListener(this);
		view.findViewById(R.id.buttonPrintBitmap).setOnClickListener(this);
		
		deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
		deviceMessagesTextView.setMovementMethod(new ScrollingMovementMethod());
		deviceMessagesTextView.setVerticalScrollBarEnabled(true);
		
		brightnessTextView = (TextView) view.findViewById(R.id.textViewBrightness);
		brightnessSeekBar = (SeekBar) view.findViewById(R.id.seekBarBrightness);
		brightnessSeekBar.setOnSeekBarChangeListener(this);
		return view;
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonGallery:
			String externalStorageState = Environment.getExternalStorageState();
			if (externalStorageState.equals(Environment.MEDIA_MOUNTED)) {
				Intent intent = new Intent(Intent.ACTION_PICK);
				intent.setType(android.provider.MediaStore.Images.Media.CONTENT_TYPE);
				getActivity().startActivityForResult(intent, MainActivity.REQUEST_CODE_ACTION_PICK);
			}
			break;
			
		case R.id.buttonPrintBitmap:
			printBitmap();
			break;
		}
	}
	
	@Override
	public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
		brightnessTextView.setText(Integer.toString(progress));
	}

	@Override
	public void onStartTrackingTouch(SeekBar seekBar) {
		// TODO Auto-generated method stub
		
	}

	@Override
	public void onStopTrackingTouch(SeekBar seekBar) {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		if (requestCode == MainActivity.REQUEST_CODE_ACTION_PICK) {
			if (data != null) {
				Uri uri = data.getData();
				ContentResolver cr = getActivity().getContentResolver();
				Cursor c = cr.query(uri, new String[] {MediaStore.Images.Media.DATA}, null, null, null);
				if (c == null || c.getCount() == 0) {
					return;
				}
				
				c.moveToFirst();
				int columnIndex = c.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
				String text = c.getString(columnIndex);
				
				bitmapPathTextView.setText(text);
			}
		}
	}
	
	private void printBitmap() {
		String fileName = bitmapPathTextView.getText().toString();
		try {
			int width = Integer.valueOf(widthEditText.getText().toString());
			int alignment = getAlignment();
			
			ByteBuffer buffer = ByteBuffer.allocate(4);
			buffer.put((byte) POSPrinterConst.PTR_S_RECEIPT);
			buffer.put((byte) brightnessSeekBar.getProgress());
			buffer.put((byte) 0x00);
			buffer.put((byte) 0x00);
			
			posPrinter.printBitmap(buffer.getInt(0), fileName, width, alignment);
		} catch (NumberFormatException | JposException e) {
			e.printStackTrace();
			MessageDialogFragment.showDialog(getFragmentManager(), "Exception", e.getMessage());
			return;
		}
	}
	
	private int getAlignment() {
		switch (alignmentSpinner.getSelectedItemPosition()) {
		case 0:
			return POSPrinterConst.PTR_BM_LEFT;
			
		case 1:
			return POSPrinterConst.PTR_BM_CENTER;
			
		case 2:
			return POSPrinterConst.PTR_BM_RIGHT;
			
			default:
				return -1;
		}
	}
}
