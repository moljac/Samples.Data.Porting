package com.bxl.pdftest;

import java.io.File;
import java.io.FileFilter;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;

import jpos.JposException;
import jpos.POSPrinter;
import jpos.POSPrinterConst;
import android.os.Bundle;
import android.os.Environment;
import android.support.v7.app.ActionBarActivity;
import android.view.View;
import android.widget.EditText;
import android.widget.RadioGroup;
import android.widget.Toast;

public class MainActivity extends ActionBarActivity
		implements View.OnClickListener, ListDialogFragment.OnClickListener {
	
	private static final String PATH = Environment.getExternalStorageDirectory().getAbsolutePath()
			+ "/BIXOLON";
	
	private static final String CONFIG_FILE_NAME = "jpos.xml";
	
	private POSPrinter posPrinter;
	private EditText logicalNameEditText;
	private EditText widthEditText;
	private EditText pageEditText;
	private EditText brightnessEditText;
	private RadioGroup alignmentRadioGroup;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		logicalNameEditText = (EditText) findViewById(R.id.editTextLogicalName);
		
		widthEditText = (EditText) findViewById(R.id.editTextWidth);
		pageEditText = (EditText) findViewById(R.id.editTextPage);
		brightnessEditText = (EditText) findViewById(R.id.editTextBrightness);
		
		alignmentRadioGroup = (RadioGroup) findViewById(R.id.radioGroupAlignment);
		
		findViewById(R.id.buttonOCE).setOnClickListener(this);
		findViewById(R.id.buttonPrintPDF).setOnClickListener(this);
		findViewById(R.id.buttonClose).setOnClickListener(this);
		
		createConfigFile();
		
		if (posPrinter == null) {
			posPrinter = new POSPrinter(this);
		}
	}
	
	@Override
	protected void onDestroy() {
		super.onDestroy();
		try {
			posPrinter.close();
		} catch (JposException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}
	}

	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.buttonOCE:
			try {
				posPrinter.open(logicalNameEditText.getText().toString());
				posPrinter.claim(0);
				posPrinter.setDeviceEnabled(true);
			} catch (JposException e) {
				e.printStackTrace();
				
				try {
					posPrinter.close();
				} catch (JposException e1) {
					// TODO Auto-generated catch block
					e1.printStackTrace();
				}
				
				Toast.makeText(getApplicationContext(), e.getMessage(), Toast.LENGTH_SHORT).show();
			}
			break;
			
		case R.id.buttonPrintPDF:
			showFileList();
			break;
			
		case R.id.buttonClose:
			try {
				posPrinter.close();
			} catch (JposException e) {
				e.printStackTrace();
				Toast.makeText(getApplicationContext(), e.getMessage(), Toast.LENGTH_SHORT).show();
			}
			break;
		}
	}
		
	@Override
	public void onClick(String item) {
		printPDF(item);
	}
	
	private void createConfigFile() {
		FileInputStream fis = null;
		
		try {
			fis = openFileInput(CONFIG_FILE_NAME);
		} catch (FileNotFoundException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
			createNewConfigFile();
		}
		
		if (fis != null) {
			try {
				fis.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
		}
	}
	
	private void createNewConfigFile() {
		InputStream is = getResources().openRawResource(R.raw.jpos);
		FileOutputStream fos = null;
		
		int available = 0;
		byte[] buffer = null;
		
		try {
			available = is.available();
			buffer = new byte[available];
			is.read(buffer);
			
			fos = openFileOutput(CONFIG_FILE_NAME, MODE_PRIVATE);
			fos.write(buffer);
		} catch (IOException e) {
			e.printStackTrace();
			Toast.makeText(this, e.getMessage(), Toast.LENGTH_SHORT).show();
		} finally {
			try {
				is.close();
			} catch (IOException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}
			
			if (fos != null) {
				try {
					fos.close();
				} catch (IOException e) {
					// TODO Auto-generated catch block
					e.printStackTrace();
				}
			}
		}
	}
		
	private void showFileList() {
		File file = new File(PATH);
		File[] files = file.listFiles(new FileFilter() {
			
			@Override
			public boolean accept(File pathname) {
				if (pathname.isDirectory()) {
					return false;
				}
				
				String name = pathname.getName();
				int lastIndex = name.lastIndexOf('.');
				if (lastIndex < 0) {
					return false;
				}
				
				return name.substring(lastIndex).equalsIgnoreCase(".pdf");
			}
		});
		
		if (files != null && files.length > 0) {
			String[] items = new String[files.length];
			for (int i = 0; i < items.length; i++) {
				items[i] = files[i].getAbsolutePath();
			}
			ListDialogFragment.showDialog(getSupportFragmentManager(), "PDF file list", items);
		} else {
			Toast.makeText(getApplicationContext(), "No PDF file", Toast.LENGTH_SHORT).show();
		}
	}
		
	private void printPDF(String fileName) {
		try {
			int width = Integer.valueOf(widthEditText.getText().toString());
			int page = Integer.valueOf(pageEditText.getText().toString());
			int brightness = Integer.valueOf(brightnessEditText.getText().toString());
			
			int alignment = POSPrinterConst.PTR_PDF_LEFT;
			switch (alignmentRadioGroup.getCheckedRadioButtonId()) {
			case R.id.radioCenter:
				alignment = POSPrinterConst.PTR_PDF_CENTER;
				break;
				
			case R.id.radioRight:
				alignment = POSPrinterConst.PTR_PDF_RIGHT;
				break;
			}
			
			posPrinter.printPDF(POSPrinterConst.PTR_S_RECEIPT, fileName, width, alignment, page, brightness);
		} catch (NumberFormatException | JposException e) {
			e.printStackTrace();
			Toast.makeText(getApplicationContext(), e.getMessage(), Toast.LENGTH_SHORT).show();
		}
	}
}
