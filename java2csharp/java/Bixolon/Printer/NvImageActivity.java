package com.bixolon.printersample;

import java.io.FileNotFoundException;
import java.io.InputStream;
import java.util.ArrayList;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.BroadcastReceiver;
import android.content.ContentResolver;
import android.content.Context;
import android.content.DialogInterface;
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
import android.view.LayoutInflater;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.bixolon.printer.BixolonPrinter;

public class NvImageActivity extends Activity implements OnClickListener {

	private static final int REQUEST_CODE_ACTION_PICK = 1;

	private ImageView mImageView;
	private TextView mTextView;
	
	private ListView mListView;
	private ArrayList<String> mArrayList = new ArrayList<String>();
	private ArrayAdapter<String> mAdapter;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_nv_image);

		mImageView = (ImageView) findViewById(R.id.imageView1);
		mTextView = (TextView) findViewById(R.id.textView3);

		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button3);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button4);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button5);
		button.setOnClickListener(this);

		mAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_single_choice, mArrayList);

		mListView = (ListView) findViewById(R.id.listView1);
		mListView.setAdapter(mAdapter);
		mListView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);

		IntentFilter filter = new IntentFilter();
		filter.addAction(MainActivity.ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES);
		registerReceiver(mReceiver, filter);

		MainActivity.mBixolonPrinter.getDefinedNvImageKeyCodes();
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

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			String externalStorageState = Environment.getExternalStorageState();
			if (externalStorageState.equals(Environment.MEDIA_MOUNTED)) {
				Intent intent = new Intent(Intent.ACTION_PICK);
				intent.setType(android.provider.MediaStore.Images.Media.CONTENT_TYPE);
				startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
			}
			break;

		case R.id.button2:
			LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
			final View view = inflater.inflate(R.layout.dialog_nv_image, null);

			new AlertDialog.Builder(NvImageActivity.this).setView(view).setPositiveButton("OK", new DialogInterface.OnClickListener() {

				public void onClick(DialogInterface dialog, int which) {
					defineNvImage(view);
				}
			}).setNegativeButton("Cancel", new DialogInterface.OnClickListener() {

				public void onClick(DialogInterface dialog, int which) {
					// TODO Auto-generated method stub

				}
			}).show();
			break;

		case R.id.button3:
			int checkedItemPosition = mListView.getCheckedItemPosition();
			if (checkedItemPosition < 0 || mArrayList.size() == 0) {
				Toast.makeText(getApplicationContext(), "Please choose one key code.", Toast.LENGTH_SHORT).show();
			} else {
				MainActivity.mBixolonPrinter.printNvImage(Integer.parseInt(mArrayList.get(checkedItemPosition)), true);
			}
			break;

		case R.id.button4:
			checkedItemPosition = mListView.getCheckedItemPosition();
			if (checkedItemPosition < 0 || mArrayList.size() == 0) {
				Toast.makeText(getApplicationContext(), "Please choose one key code.", Toast.LENGTH_SHORT).show();
			} else {
				MainActivity.mBixolonPrinter.removeNvImage(Integer.parseInt(mArrayList.get(checkedItemPosition)));
				mListView.clearChoices();
			}
			break;

		case R.id.button5:
			MainActivity.mBixolonPrinter.removeAllNvImage();
			mListView.clearChoices();
			break;
		}
	}
	
	private void defineNvImage(View view) {
		try {
			EditText editText = (EditText) findViewById(R.id.editText1);
			final int level = Integer.parseInt(editText.getText().toString());

			editText = (EditText) view.findViewById(R.id.editText1);
			final int keyCode = Integer.parseInt(editText.getText().toString());
			
			final String pathName = mTextView.getText().toString();
			
			if (pathName != null && pathName.length() > 0) {
				MainActivity.mBixolonPrinter.defineNvImage(pathName, BixolonPrinter.BITMAP_WIDTH_NONE, level, keyCode);
			} else {
				BitmapDrawable drawable = (BitmapDrawable) getResources().getDrawable(R.drawable.bixolon);
				Bitmap bitmap = drawable.getBitmap();
				
				MainActivity.mBixolonPrinter.defineNvImage(bitmap, BixolonPrinter.BITMAP_WIDTH_NONE, level, keyCode);
			}
		} catch (NumberFormatException e) {
			e.printStackTrace();
			Toast.makeText(getApplicationContext(), "Please input key code or level.", Toast.LENGTH_SHORT).show();
		}
	}
	
	BroadcastReceiver mReceiver = new BroadcastReceiver() {

		@Override
		public void onReceive(Context context, Intent intent) {
			if (intent.getAction().equals(MainActivity.ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES)) {
				int[] keyCodes = intent.getIntArrayExtra(MainActivity.EXTRA_NAME_NV_KEY_CODES);
				mArrayList.clear();
				if (keyCodes != null) {
					for (int i = 0; i < keyCodes.length; i++) {
						mArrayList.add(Integer.toString(keyCodes[i]));
					}
				}
				mAdapter.notifyDataSetChanged();
			}

		}
	};
}
