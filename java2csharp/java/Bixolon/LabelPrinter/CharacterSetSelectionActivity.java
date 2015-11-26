package com.bixolon.labelprintersample;

import com.bixolon.labelprinter.BixolonLabelPrinter;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.os.Bundle;
import android.view.Menu;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.TextView;

public class CharacterSetSelectionActivity extends Activity implements OnClickListener {
	private static final String[] INTERNATIONAL_CHARACTER_SET_ITEMS = {
		"U.S.A",
		"France",
		"Germany",
		"U.K",
		"Denmark I",
		"Sweden",
		"Italy",
		"Spain I",
		"Norway",
		"Denmark II",
		"Japan",
		"Spain II",
		"Latin America",
		"Korea",
		"Slovenia/Croatia",
		"China"
	};
	
	private static final int[] INTERNATIONAL_CHARACTER_SET = {
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_USA,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_FRANCE,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_GERMANY,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_UK,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_DENMARK1,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SWEDEN,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_ITALY,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SPAIN1,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_NORWAY,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_DENMARK2,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_JAPAN,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SPAIN2,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_LATIN_AMERICA,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_KOREA,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SLOVENIA_CROATIA,
		BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_CHINA
	};
	
	private static final String[] CODE_PAGE_ITEMS = {
		"CP437 (U.S.A)",
		"CP850 (Latin1)",
		"CP852 (Latin2)",
		"CP860 (Portuguese)",
		"CP863 (Canadian French)",
		"CP865 (Nordic)",
		"WCP1252 (Latin I)",
		"CP865 + WCP1252 (European Combined)",
		"CP857 (Turkish)",
		"CP737 (Greek)",
		"WCP1250 (Latin 2)",
		"WCP1253 (Greek)",
		"WCP1254 (Turkish)",
		"CP855 (Cyrillic)",
		"CP862 (Hebrew)",
		"CP866 (Cyrillic)",
		"WCP1251 (Cyrillic)",
		"WCP1255 (Hebrew)",
		"CP928 (Greek)",
		"CP864 (Arabic)",
		"CP775 (Baltic)",
		"WCP1257 (Baltic)",
		"CP858 (Latin 1 + Euro)"
	};
	
	private static final int[] CODE_PAGES = {
		BixolonLabelPrinter.CODE_PAGE_CP437_USA,
		BixolonLabelPrinter.CODE_PAGE_CP850_LATIN1,
		BixolonLabelPrinter.CODE_PAGE_CP852_LATIN2,
		BixolonLabelPrinter.CODE_PAGE_CP860_PORTUGUESE,
		BixolonLabelPrinter.CODE_PAGE_CP863_CANADIAN_FRENCH,
		BixolonLabelPrinter.CODE_PAGE_CP865_NORDIC,
		BixolonLabelPrinter.CODE_PAGE_WCP1252_LATIN1,
		BixolonLabelPrinter.CODE_PAGE_CP865_WCP1252_EUROPEAN_COMBINED,
		BixolonLabelPrinter.CODE_PAGE_CP857_TURKISH,
		BixolonLabelPrinter.CODE_PAGE_CP737_GREEK,
		BixolonLabelPrinter.CODE_PAGE_WCP1250_LATIN2,
		BixolonLabelPrinter.CODE_PAGE_WCP1253_GREEK,
		BixolonLabelPrinter.CODE_PAGE_WCP1254_TURKISH,
		BixolonLabelPrinter.CODE_PAGE_CP855_CYRILLIC,
		BixolonLabelPrinter.CODE_PAGE_CP862_HEBREW,
		BixolonLabelPrinter.CODE_PAGE_CP866_CYRILLIC,
		BixolonLabelPrinter.CODE_PAGE_WCP1251_CYRILLIC,
		BixolonLabelPrinter.CODE_PAGE_WCP1255_HEBREW,
		BixolonLabelPrinter.CODE_PAGE_CP928_GREEK,
		BixolonLabelPrinter.CODE_PAGE_CP864_ARABIC,
		BixolonLabelPrinter.CODE_PAGE_CP775_BALTIC,
		BixolonLabelPrinter.CODE_PAGE_WCP1257_BALTIC,
		BixolonLabelPrinter.CODE_PAGE_CP858_LATIN1_EURO
	};
	
	private AlertDialog mInternationalCharacterSetDialog;
	private AlertDialog mCodePageDialog;
	
	private TextView mInternationalCharacterSetTextView;
	private TextView mCodePageTextView;
	
	private int mInternationalCharacterSet;
	private int mCodePage;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_character_set_selection);
		
		Button button = (Button) findViewById(R.id.button1);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button2);
		button.setOnClickListener(this);
		button = (Button) findViewById(R.id.button3);
		button.setOnClickListener(this);
		
		mInternationalCharacterSetTextView = (TextView) findViewById(R.id.textView1);
		mCodePageTextView = (TextView) findViewById(R.id.textView2);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater()
				.inflate(R.menu.activity_character_set_selection, menu);
		return true;
	}

	public void onClick(View v) {
		switch (v.getId()) {
		case R.id.button1:
			showInternationalCharacterSetDialog();
			break;
			
		case R.id.button2:
			showCodePageDialog();
			break;
			
		case R.id.button3:
			MainActivity.mBixolonLabelPrinter.setCharacterSet(mInternationalCharacterSet, mCodePage);
			break;
		}
	}

	private void showInternationalCharacterSetDialog() {
		if (mInternationalCharacterSetDialog == null) {
			mInternationalCharacterSetDialog = new AlertDialog.Builder(CharacterSetSelectionActivity.this).setTitle(R.string.international_chracter_set)
				.setItems(INTERNATIONAL_CHARACTER_SET_ITEMS, new DialogInterface.OnClickListener() {
				
					public void onClick(DialogInterface dialog, int which) {
						mInternationalCharacterSet = INTERNATIONAL_CHARACTER_SET[which];
						mInternationalCharacterSetTextView.setText(INTERNATIONAL_CHARACTER_SET_ITEMS[which]);
					}
				}).create();
		}
		mInternationalCharacterSetDialog.show();
	}
	
	private void showCodePageDialog() {
		if (mCodePageDialog == null) {
			mCodePageDialog = new AlertDialog.Builder(CharacterSetSelectionActivity.this).setTitle(R.string.code_pages)
				.setItems(CODE_PAGE_ITEMS, new DialogInterface.OnClickListener() {
					
					public void onClick(DialogInterface dialog, int which) {
						mCodePage = CODE_PAGES[which];
						mCodePageTextView.setText(CODE_PAGE_ITEMS[which]);
					}
				}).create();
		}
		mCodePageDialog.show();
	}
}
