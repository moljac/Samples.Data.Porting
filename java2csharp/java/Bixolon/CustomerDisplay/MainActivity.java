package com.bixolon.customerdisplaysample;

import java.util.ArrayList;

import android.app.AlertDialog;
import android.app.ListActivity;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ListView;

import com.bixolon.customerdisplay.BixolonCustomerDisplay;

public class MainActivity extends ListActivity implements View.OnClickListener {
	private final String[] ITEMS = {
			"move cursor",
			"initialize",
			"setInternationalCodeSet",
			"setCharacterFontTable",
			"setTime",
			"displayTime",
			"executeSelfTest",
			"write string",
			"sample"
	};
	
	private final String[] INTERNATIONAL_CODE_SET_ITEMS = {
			 "U.S.A",
			 "France",
			 "Germany",
			 "U.K",
			 "Denmark-1",
			 "Sweden",
			 "Italy",
			 "Spain-1",
			 "Japan",
			 "Norway",
			 "Denmark-2",
			 "Spain-2",
			 "Latin America",
			 "Korea",
			 "Slovenia Croatia",
			 "China"
	};
	
	private final String[] FONT_TABLE_ITEMS = {
			"PC437 (Standard Eropean)",
			"JIS (Japanese Katakana)",
			"PC850 (Multilingual)",
			"PC860 (Portuguese)",
			"PC863 (Canadian French)",
			"PC865 (Nordic)",
			"PC737 (Greek)",
			"WPC1250 (Central European Windows Code)",
			"WPC1251 (Cyrillic Windows Code)",
			"WPC1252 (Western European Windows Code)",
			"PC866 (Cyrillic-2: Russian)",
			"PC852 (Latin-2: Slavonic)",
			"PC858 (Euro)",
			"PC775 (Baltic)",
			"WPC1253 (Greek Windows Code)",
			"PC857 (Turkish)",
			"PC864 (Arabic)"
	};
	
	static BixolonCustomerDisplay mBcd;
	private boolean mIsConnected;
	
	private ListView mListView;
	
	private LayoutInflater mLayoutInflater;
	private View mView;
	
	private AlertDialog mMoveCursorDialog;
	private AlertDialog mInternationalCodeSetDialog;
	private AlertDialog mFontTableDialog;
	private AlertDialog mSetTimeDialog;
	
	@Override
	public void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		ArrayList<String> list = new ArrayList<String>();
		for (int i = 0; i < ITEMS.length; i++) {
			list.add(ITEMS[i]);
		}
		
		ArrayAdapter<String> adapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, list);
		mListView = (ListView) findViewById(android.R.id.list);
		mListView.setAdapter(adapter);
		mListView.setEnabled(false);
		
		mLayoutInflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
	}
	
	public void onDestroy() {
		if (mBcd != null) {
			mBcd.close();
		}
		super.onDestroy();
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;
	}
	
	@Override
	public boolean onPrepareOptionsMenu(Menu menu) {
		menu.getItem(0).setEnabled(!mIsConnected);
		menu.getItem(1).setEnabled(mIsConnected);
		return true;
	}
	
	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		switch (item.getItemId()) {
		case R.id.item1:
			mBcd = new BixolonCustomerDisplay(MainActivity.this);
			mIsConnected = mBcd.open();
			if (mIsConnected) {
				mListView.setEnabled(true);
			} else {
				mBcd = null;
			}
			return true;
			
		case R.id.item2:
			mBcd.close();
			mBcd = null;
			mIsConnected = false;
			return true;
		}
		return false;
	}
	
	@Override
	public void onListItemClick(ListView l, View v, int position, long id) {
		switch (position) {
		case 0:
			showMoveCursorDialog();
			break;
			
		case 1:
			mBcd.initialize();
			break;
			
		case 2:
			showInternationalCodeSetDialog();
			break;
			
		case 3:
			showFontTableDialog();
			break;
			
		case 4:
			showSetTimeDialog();
			break;
			
		case 5:
			mBcd.displayTime();
			break;
			
		case 6:
			mBcd.executeSelfTest();
			break;
			
		case 7:
			Intent intent = new Intent(MainActivity.this, WriteStringActivity.class);
			startActivity(intent);
			break;
			
		case 8:
			intent = new Intent(MainActivity.this, SampleActivity.class);
			startActivity(intent);
			break;
		}
	}
	
	@Override
	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_LEFT);
			break;
			
		case R.id.button2:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_RIGHT);
			break;
			
		case R.id.button3:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_DOWN);
			break;
			
		case R.id.button4:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_UP);
			break;
			
		case R.id.button5:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_HOME);
			break;
			
		case R.id.button6:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_LEFT_MOST);
			break;
			
		case R.id.button7:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_RIGHT_MOST);
			break;
			
		case R.id.button8:
			mBcd.moveCursor(BixolonCustomerDisplay.POSITION_BOTTOM);
			break;
			
		case R.id.button9:
			EditText editText = (EditText) mView.findViewById(R.id.editText1);
			String string = editText.getText().toString();
			int x = Integer.parseInt(string);
			
			editText = (EditText) mView.findViewById(R.id.editText2);
			string = editText.getText().toString();
			int y = Integer.parseInt(string);
			
			mBcd.moveCursor(x, y);
			break;
		}
	}
	
	private void showMoveCursorDialog() {
		if (mMoveCursorDialog == null) {
			mView = mLayoutInflater.inflate(R.layout.dialog_move_cursor, null);
			Button button = (Button) mView.findViewById(R.id.button1);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button2);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button3);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button4);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button5);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button6);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button7);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button8);
			button.setOnClickListener(this);
			button = (Button) mView.findViewById(R.id.button9);
			button.setOnClickListener(this);
			
			mMoveCursorDialog = new AlertDialog.Builder(MainActivity.this).setView(mView).setNegativeButton("Close", new DialogInterface.OnClickListener() {
				
				public void onClick(DialogInterface dialog, int which) {
					// TODO Auto-generated method stub
					
				}
			}).create();
		}
		
		mMoveCursorDialog.show();
		mBcd.setCursor(true);
	}
	
	private void showInternationalCodeSetDialog() {
		if (mInternationalCodeSetDialog == null) {
			mInternationalCodeSetDialog = new AlertDialog.Builder(MainActivity.this).setItems(INTERNATIONAL_CODE_SET_ITEMS, new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					switch (which) {
					case 0:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_USA);
						break;
					case 1:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_FRANCE);
						break;
					case 2:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_GERMANY);
						break;
					case 3:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_UK);
						break;
					case 4:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_DENMARK1);
						break;
					case 5:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_SWEDEN);
						break;
					case 6:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_ITALY);
						break;
					case 7:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_SPAIN1);
						break;
					case 8:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_JAPAN);
						break;
					case 9:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_NORWAY);
						break;
					case 10:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_DENMARK2);
						break;
					case 11:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_SPAIN2);
						break;
					case 12:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_LATIN_AMERICA);
						break;
					case 13:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_KOREA);
						break;
					case 14:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_SLOVENIA_CROATIA);
						break;
					case 15:
						mBcd.setInternationalCodeSet(BixolonCustomerDisplay.INTERNATIONAL_CODE_CHINA);
						break;
					}
					
				}
			}).create();
		}
		
		mInternationalCodeSetDialog.show();
	}
	
	private void showFontTableDialog() {
		if (mFontTableDialog == null) {
			mFontTableDialog = new AlertDialog.Builder(MainActivity.this).setItems(FONT_TABLE_ITEMS, new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					switch (which) {
					case 0:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_437_STANDARD_EUROPEAN);
						break;
					case 1:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_JAPANESE_KATAKANA);
						break;
					case 2:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_850_MULTILINGUAL);
						break;
					case 3:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_860_PORTUGUESE);
						break;
					case 4:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_863_CANADIAN_FRENCH);
						break;
					case 5:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_865_NORDIC);
						break;
					case 6:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_737_GREEK);
						break;
					case 7:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_1250_CENTRAL_EUROPEAN_WINDOWS);
						break;
					case 8:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_1251_CYRILLIC_WINDOWS);
						break;
					case 9:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_1252_WESTERN_EUROPEAN_WINDOWS);
						break;
					case 10:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_866_CYRILLIC2_RUSSIAN);
						break;
					case 11:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_852_LATIN2_SLAVONIC);
						break;
					case 12:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_858_EURO);
						break;
					case 13:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_775_BALTIC);
						break;
					case 14:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_1253_GREEK_WINDOWS);
						break;
					case 15:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_857_TURKISH);
						break;
					case 16:
						mBcd.setCharacterFontTable(BixolonCustomerDisplay.CP_864_ARABIC);
						break;
					}
				}
			}).create();
		}
		mFontTableDialog.show();
	}
	
	private void showSetTimeDialog() {
		if (mSetTimeDialog == null) {
			mView = mLayoutInflater.inflate(R.layout.dialog_set_time, null);
			mSetTimeDialog = new AlertDialog.Builder(MainActivity.this).setView(mView).setPositiveButton("Set", new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					EditText editText = (EditText) mView.findViewById(R.id.editText1);
					String string = editText.getText().toString();
					int hour = Integer.parseInt(string);
					
					editText = (EditText) mView.findViewById(R.id.editText2);
					string = editText.getText().toString();
					int minute = Integer.parseInt(string);
					
					mBcd.setTime(hour, minute);
				}
			}).setNegativeButton("Cancel", new DialogInterface.OnClickListener() {
				
				@Override
				public void onClick(DialogInterface dialog, int which) {
					// TODO Auto-generated method stub
					
				}
			}).create();
		}
		mSetTimeDialog.show();
	}
}
