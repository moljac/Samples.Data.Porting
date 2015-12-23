namespace com.bixolon.labelprintersample
{

	using BixolonLabelPrinter = com.bixolon.labelprinter.BixolonLabelPrinter;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;

	public class CharacterSetSelectionActivity : Activity, View.OnClickListener
	{
		private static readonly string[] INTERNATIONAL_CHARACTER_SET_ITEMS = new string[] {"U.S.A", "France", "Germany", "U.K", "Denmark I", "Sweden", "Italy", "Spain I", "Norway", "Denmark II", "Japan", "Spain II", "Latin America", "Korea", "Slovenia/Croatia", "China"};

		private static readonly int[] INTERNATIONAL_CHARACTER_SET = new int[] {BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_USA, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_FRANCE, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_GERMANY, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_UK, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_DENMARK1, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SWEDEN, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_ITALY, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SPAIN1, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_NORWAY, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_DENMARK2, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_JAPAN, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SPAIN2, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_LATIN_AMERICA, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_KOREA, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_SLOVENIA_CROATIA, BixolonLabelPrinter.INTERNATIONAL_CHARACTER_SET_CHINA};

		private static readonly string[] CODE_PAGE_ITEMS = new string[] {"CP437 (U.S.A)", "CP850 (Latin1)", "CP852 (Latin2)", "CP860 (Portuguese)", "CP863 (Canadian French)", "CP865 (Nordic)", "WCP1252 (Latin I)", "CP865 + WCP1252 (European Combined)", "CP857 (Turkish)", "CP737 (Greek)", "WCP1250 (Latin 2)", "WCP1253 (Greek)", "WCP1254 (Turkish)", "CP855 (Cyrillic)", "CP862 (Hebrew)", "CP866 (Cyrillic)", "WCP1251 (Cyrillic)", "WCP1255 (Hebrew)", "CP928 (Greek)", "CP864 (Arabic)", "CP775 (Baltic)", "WCP1257 (Baltic)", "CP858 (Latin 1 + Euro)"};

		private static readonly int[] CODE_PAGES = new int[] {BixolonLabelPrinter.CODE_PAGE_CP437_USA, BixolonLabelPrinter.CODE_PAGE_CP850_LATIN1, BixolonLabelPrinter.CODE_PAGE_CP852_LATIN2, BixolonLabelPrinter.CODE_PAGE_CP860_PORTUGUESE, BixolonLabelPrinter.CODE_PAGE_CP863_CANADIAN_FRENCH, BixolonLabelPrinter.CODE_PAGE_CP865_NORDIC, BixolonLabelPrinter.CODE_PAGE_WCP1252_LATIN1, BixolonLabelPrinter.CODE_PAGE_CP865_WCP1252_EUROPEAN_COMBINED, BixolonLabelPrinter.CODE_PAGE_CP857_TURKISH, BixolonLabelPrinter.CODE_PAGE_CP737_GREEK, BixolonLabelPrinter.CODE_PAGE_WCP1250_LATIN2, BixolonLabelPrinter.CODE_PAGE_WCP1253_GREEK, BixolonLabelPrinter.CODE_PAGE_WCP1254_TURKISH, BixolonLabelPrinter.CODE_PAGE_CP855_CYRILLIC, BixolonLabelPrinter.CODE_PAGE_CP862_HEBREW, BixolonLabelPrinter.CODE_PAGE_CP866_CYRILLIC, BixolonLabelPrinter.CODE_PAGE_WCP1251_CYRILLIC, BixolonLabelPrinter.CODE_PAGE_WCP1255_HEBREW, BixolonLabelPrinter.CODE_PAGE_CP928_GREEK, BixolonLabelPrinter.CODE_PAGE_CP864_ARABIC, BixolonLabelPrinter.CODE_PAGE_CP775_BALTIC, BixolonLabelPrinter.CODE_PAGE_WCP1257_BALTIC, BixolonLabelPrinter.CODE_PAGE_CP858_LATIN1_EURO};

		private AlertDialog mInternationalCharacterSetDialog;
		private AlertDialog mCodePageDialog;

		private TextView mInternationalCharacterSetTextView;
		private TextView mCodePageTextView;

		private int mInternationalCharacterSet;
		private int mCodePage;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_character_set_selection;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button3);
			button.OnClickListener = this;

			mInternationalCharacterSetTextView = (TextView) findViewById(R.id.textView1);
			mCodePageTextView = (TextView) findViewById(R.id.textView2);
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_character_set_selection, menu);
			return true;
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
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

		private void showInternationalCharacterSetDialog()
		{
			if (mInternationalCharacterSetDialog == null)
			{
				mInternationalCharacterSetDialog = (new AlertDialog.Builder(CharacterSetSelectionActivity.this)).setTitle(R.@string.international_chracter_set).setItems(INTERNATIONAL_CHARACTER_SET_ITEMS, new OnClickListenerAnonymousInnerClassHelper(this))
				   .create();
			}
			mInternationalCharacterSetDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly CharacterSetSelectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(CharacterSetSelectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				outerInstance.mInternationalCharacterSet = INTERNATIONAL_CHARACTER_SET[which];
				outerInstance.mInternationalCharacterSetTextView.Text = INTERNATIONAL_CHARACTER_SET_ITEMS[which];
			}
		}

		private void showCodePageDialog()
		{
			if (mCodePageDialog == null)
			{
				mCodePageDialog = (new AlertDialog.Builder(CharacterSetSelectionActivity.this)).setTitle(R.@string.code_pages).setItems(CODE_PAGE_ITEMS, new OnClickListenerAnonymousInnerClassHelper2(this))
				   .create();
			}
			mCodePageDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly CharacterSetSelectionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(CharacterSetSelectionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				outerInstance.mCodePage = CODE_PAGES[which];
				outerInstance.mCodePageTextView.Text = CODE_PAGE_ITEMS[which];
			}
		}
	}

}