package com.bixolon.printersample;

import java.io.FileNotFoundException;
import java.io.InputStream;

import android.app.Activity;
import android.content.BroadcastReceiver;
import android.content.ContentResolver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.Cursor;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.drawable.BitmapDrawable;
import android.net.Uri;
import android.os.Bundle;
import android.os.Environment;
import android.provider.MediaStore;
import android.view.View;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.RadioGroup;
import android.widget.TextView;
import android.widget.Toast;

import com.bixolon.printer.BixolonPrinter;

public class PrintBitmapAcitivity extends Activity {
	private ImageView mImageView;
	private TextView mTextView;
	private EditText mWidthEdit;
	private RadioGroup mWidthRadioGroup;
	
	private int mAlignment;
	private boolean mFormFeed;
	
	private static final int REQUEST_CODE_ACTION_PICK = 1;

	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_print_bitmap);
		
		mImageView = (ImageView) findViewById(R.id.imageView1);
		mTextView = (TextView) findViewById(R.id.textView4);
		mWidthEdit = (EditText) findViewById(R.id.editText2);
		mWidthRadioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		mWidthRadioGroup.setOnCheckedChangeListener(new RadioGroup.OnCheckedChangeListener() {
			
			public void onCheckedChanged(RadioGroup group, int checkedId) {
				mWidthEdit.setEnabled(checkedId == R.id.radio5);
				
			}
		});
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(new View.OnClickListener() {
			
			public void onClick(View v) {
				String externalStorageState = Environment.getExternalStorageState();
				if (externalStorageState.equals(Environment.MEDIA_MOUNTED)) {
					Intent intent = new Intent(Intent.ACTION_PICK);
					intent.setType(android.provider.MediaStore.Images.Media.CONTENT_TYPE);
					startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
				}
				
			}
		});
		
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(new View.OnClickListener() {
			
			public void onClick(View v) {
				printBitmap();
			}
		});
		
		IntentFilter filter = new IntentFilter();
		filter.addAction(MainActivity.ACTION_COMPLETE_PROCESS_BITMAP);
		registerReceiver(mReceiver, filter);
	}
	
	@Override
	public void onDestroy() {
		unregisterReceiver(mReceiver);
		super.onDestroy();
	}
	
	@Override
	public void onActivityResult(int requestCode, int resultCode, Intent data) {
		super.onActivityResult(requestCode, resultCode, data);
		
		if (requestCode == REQUEST_CODE_ACTION_PICK) {
			if (data != null) {
				Uri uri = data.getData();
				InputStream is = null;
				try {
					is = getContentResolver().openInputStream(uri);
				} catch (FileNotFoundException e) {
					e.printStackTrace();
					return;
				}
				
				BitmapFactory.Options opts = new BitmapFactory.Options();
				opts.inJustDecodeBounds = false;
				opts.inSampleSize = 1;
				opts.inPreferredConfig = Bitmap.Config.RGB_565;
				Bitmap bm = BitmapFactory.decodeStream(is, null, opts);
				mImageView.setImageBitmap(bm);
				
				ContentResolver cr = getContentResolver();
				Cursor c = cr.query(uri, new String[] {MediaStore.Images.Media.DATA}, null, null, null);
				if (c == null || c.getCount() == 0) {
					return;
				}
				c.moveToFirst();
				int columnIndex = c.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
				String text = c.getString(columnIndex);
				mTextView.setText(text);
			}
		}
	}
	
	private void printBitmap() {
		String pathName = mTextView.getText().toString();
		
		mAlignment = BixolonPrinter.ALIGNMENT_CENTER;
		RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio0:
			mAlignment = BixolonPrinter.ALIGNMENT_LEFT;
			break;
			
		case R.id.radio1:
			mAlignment = BixolonPrinter.ALIGNMENT_CENTER;
			break;
			
		case R.id.radio2:
			mAlignment = BixolonPrinter.ALIGNMENT_RIGHT;
			break;
		}
		
		int width = 0;
		radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
		switch (radioGroup.getCheckedRadioButtonId()) {
		case R.id.radio3:
			width = BixolonPrinter.BITMAP_WIDTH_NONE;
			break;
			
		case R.id.radio4:
			width = BixolonPrinter.BITMAP_WIDTH_FULL;
			break;
			
		case R.id.radio5:
			String string = mWidthEdit.getText().toString();
			if (string.length() == 0) {
				Toast.makeText(getApplicationContext(), "Please enter the width", Toast.LENGTH_SHORT).show();
			} else {
				width = Integer.parseInt(string);
			}
			break;
		}
		
		EditText editText = (EditText) findViewById(R.id.editText1);
		int level = Integer.parseInt(editText.getText().toString());
		
		CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
		mFormFeed = checkBox.isChecked();
		
		checkBox = (CheckBox) findViewById(R.id.checkBox2);
		
		boolean dither = ((CheckBox) findViewById(R.id.checkBox3)).isChecked();
		boolean compress = ((CheckBox) findViewById(R.id.checkBox4)).isChecked();
		
		if (checkBox.isChecked()) {
			if (pathName != null && pathName.length() > 0) {
				MainActivity.mBixolonPrinter.printDotMatrixBitmap(pathName, mAlignment, width, level, false);
			} else {
				BitmapDrawable drawable = (BitmapDrawable) getResources().getDrawable(R.drawable.bixolon);
				Bitmap bitmap = drawable.getBitmap();
				
				MainActivity.mBixolonPrinter.printDotMatrixBitmap(bitmap, mAlignment, width, level, false);
			}
		} else {
			if (pathName != null && pathName.length() > 0) {
				MainActivity.mBixolonPrinter.printBitmap(pathName, mAlignment, width, level, dither, compress, true);
				/*
				 * You can choose the method. Please reference attached document.
				 * MainActivity.mBixolonPrinter.getMonoPixels(pathName, width, level);
				 */
			} else {
				BitmapDrawable drawable = (BitmapDrawable) getResources().getDrawable(R.drawable.bixolon);
				Bitmap bitmap = drawable.getBitmap();
				
				MainActivity.mBixolonPrinter.printBitmap(bitmap, mAlignment, width, level, dither, compress, true);
				/*
				 * You can choose the method. Please reference attached document.
				 * MainActivity.mBixolonPrinter.getMonoPixels(bitmap, width, level);
				 */
			}
		}
	}
	
	private void printBitmap(byte[] pixels, int width, int height) {
		MainActivity.mBixolonPrinter.printBitmap(pixels, mAlignment, width, height, false);
		if (mFormFeed) {
			MainActivity.mBixolonPrinter.formFeed(false);
		}
		MainActivity.mBixolonPrinter.cutPaper(0, true);
	}
	
	BroadcastReceiver mReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			if (intent.getAction().equals(MainActivity.ACTION_COMPLETE_PROCESS_BITMAP)) {
				byte[] pixels = intent.getByteArrayExtra(MainActivity.EXTRA_NAME_BITMAP_PIXELS);
				int width = intent.getIntExtra(MainActivity.EXTRA_NAME_BITMAP_WIDTH, 0);
				int height = intent.getIntExtra(MainActivity.EXTRA_NAME_BITMAP_HEIGHT, 0);
				
				printBitmap(pixels, width, height);
			}

		}
	};
}
