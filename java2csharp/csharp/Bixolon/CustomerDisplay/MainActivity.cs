using System.Collections.Generic;

namespace com.bixolon.customerdisplaysample
{

	using AlertDialog = android.app.AlertDialog;
	using ListActivity = android.app.ListActivity;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using LayoutInflater = android.view.LayoutInflater;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ListView = android.widget.ListView;

	using BixolonCustomerDisplay = com.bixolon.customerdisplay.BixolonCustomerDisplay;

	public class MainActivity : ListActivity, View.OnClickListener
	{
		private readonly string[] ITEMS = new string[] {"move cursor", "initialize", "setInternationalCodeSet", "setCharacterFontTable", "setTime", "displayTime", "executeSelfTest", "write string", "sample"};

		private readonly string[] INTERNATIONAL_CODE_SET_ITEMS = new string[] {"U.S.A", "France", "Germany", "U.K", "Denmark-1", "Sweden", "Italy", "Spain-1", "Japan", "Norway", "Denmark-2", "Spain-2", "Latin America", "Korea", "Slovenia Croatia", "China"};

		private readonly string[] FONT_TABLE_ITEMS = new string[] {"PC437 (Standard Eropean)", "JIS (Japanese Katakana)", "PC850 (Multilingual)", "PC860 (Portuguese)", "PC863 (Canadian French)", "PC865 (Nordic)", "PC737 (Greek)", "WPC1250 (Central European Windows Code)", "WPC1251 (Cyrillic Windows Code)", "WPC1252 (Western European Windows Code)", "PC866 (Cyrillic-2: Russian)", "PC852 (Latin-2: Slavonic)", "PC858 (Euro)", "PC775 (Baltic)", "WPC1253 (Greek Windows Code)", "PC857 (Turkish)", "PC864 (Arabic)"};

		internal static BixolonCustomerDisplay mBcd;
		private bool mIsConnected;

		private ListView mListView;

		private LayoutInflater mLayoutInflater;
		private View mView;

		private AlertDialog mMoveCursorDialog;
		private AlertDialog mInternationalCodeSetDialog;
		private AlertDialog mFontTableDialog;
		private AlertDialog mSetTimeDialog;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			List<string> list = new List<string>();
			for (int i = 0; i < ITEMS.Length; i++)
			{
				list.Add(ITEMS[i]);
			}

			ArrayAdapter<string> adapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, list);
			mListView = (ListView) findViewById(android.R.id.list);
			mListView.Adapter = adapter;
			mListView.Enabled = false;

			mLayoutInflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
		}

		public virtual void onDestroy()
		{
			if (mBcd != null)
			{
				mBcd.close();
			}
			base.onDestroy();
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.activity_main, menu);
			return true;
		}

		public override bool onPrepareOptionsMenu(Menu menu)
		{
			menu.getItem(0).Enabled = !mIsConnected;
			menu.getItem(1).Enabled = mIsConnected;
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			switch (item.ItemId)
			{
			case R.id.item1:
				mBcd = new BixolonCustomerDisplay(MainActivity.this);
				mIsConnected = mBcd.open();
				if (mIsConnected)
				{
					mListView.Enabled = true;
				}
				else
				{
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

		public override void onListItemClick(ListView l, View v, int position, long id)
		{
			switch (position)
			{
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
				Intent intent = new Intent(MainActivity.this, typeof(WriteStringActivity));
				startActivity(intent);
				break;

			case 8:
				intent = new Intent(MainActivity.this, typeof(SampleActivity));
				startActivity(intent);
				break;
			}
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
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
				string @string = editText.Text.ToString();
				int x = int.Parse(@string);

				editText = (EditText) mView.findViewById(R.id.editText2);
				@string = editText.Text.ToString();
				int y = int.Parse(@string);

				mBcd.moveCursor(x, y);
				break;
			}
		}

		private void showMoveCursorDialog()
		{
			if (mMoveCursorDialog == null)
			{
				mView = mLayoutInflater.inflate(R.layout.dialog_move_cursor, null);
				Button button = (Button) mView.findViewById(R.id.button1);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button2);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button3);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button4);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button5);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button6);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button7);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button8);
				button.OnClickListener = this;
				button = (Button) mView.findViewById(R.id.button9);
				button.OnClickListener = this;

				mMoveCursorDialog = (new AlertDialog.Builder(MainActivity.this)).setView(mView).setNegativeButton("Close", new OnClickListenerAnonymousInnerClassHelper(this))
			   .create();
			}

			mMoveCursorDialog.show();
			mBcd.Cursor = true;
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		private void showInternationalCodeSetDialog()
		{
			if (mInternationalCodeSetDialog == null)
			{
				mInternationalCodeSetDialog = (new AlertDialog.Builder(MainActivity.this)).setItems(INTERNATIONAL_CODE_SET_ITEMS, new OnClickListenerAnonymousInnerClassHelper2(this))
			   .create();
			}

			mInternationalCodeSetDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_USA;
					break;
				case 1:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_FRANCE;
					break;
				case 2:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_GERMANY;
					break;
				case 3:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_UK;
					break;
				case 4:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_DENMARK1;
					break;
				case 5:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_SWEDEN;
					break;
				case 6:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_ITALY;
					break;
				case 7:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_SPAIN1;
					break;
				case 8:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_JAPAN;
					break;
				case 9:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_NORWAY;
					break;
				case 10:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_DENMARK2;
					break;
				case 11:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_SPAIN2;
					break;
				case 12:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_LATIN_AMERICA;
					break;
				case 13:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_KOREA;
					break;
				case 14:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_SLOVENIA_CROATIA;
					break;
				case 15:
					mBcd.InternationalCodeSet = BixolonCustomerDisplay.INTERNATIONAL_CODE_CHINA;
					break;
				}

			}
		}

		private void showFontTableDialog()
		{
			if (mFontTableDialog == null)
			{
				mFontTableDialog = (new AlertDialog.Builder(MainActivity.this)).setItems(FONT_TABLE_ITEMS, new OnClickListenerAnonymousInnerClassHelper3(this))
			   .create();
			}
			mFontTableDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : DialogInterface.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				switch (which)
				{
				case 0:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_437_STANDARD_EUROPEAN;
					break;
				case 1:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_JAPANESE_KATAKANA;
					break;
				case 2:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_850_MULTILINGUAL;
					break;
				case 3:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_860_PORTUGUESE;
					break;
				case 4:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_863_CANADIAN_FRENCH;
					break;
				case 5:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_865_NORDIC;
					break;
				case 6:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_737_GREEK;
					break;
				case 7:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_1250_CENTRAL_EUROPEAN_WINDOWS;
					break;
				case 8:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_1251_CYRILLIC_WINDOWS;
					break;
				case 9:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_1252_WESTERN_EUROPEAN_WINDOWS;
					break;
				case 10:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_866_CYRILLIC2_RUSSIAN;
					break;
				case 11:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_852_LATIN2_SLAVONIC;
					break;
				case 12:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_858_EURO;
					break;
				case 13:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_775_BALTIC;
					break;
				case 14:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_1253_GREEK_WINDOWS;
					break;
				case 15:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_857_TURKISH;
					break;
				case 16:
					mBcd.CharacterFontTable = BixolonCustomerDisplay.CP_864_ARABIC;
					break;
				}
			}
		}

		private void showSetTimeDialog()
		{
			if (mSetTimeDialog == null)
			{
				mView = mLayoutInflater.inflate(R.layout.dialog_set_time, null);
				mSetTimeDialog = (new AlertDialog.Builder(MainActivity.this)).setView(mView).setPositiveButton("Set", new OnClickListenerAnonymousInnerClassHelper4(this))
			   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper5(this))
			   .create();
			}
			mSetTimeDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : DialogInterface.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				EditText editText = (EditText) outerInstance.mView.findViewById(R.id.editText1);
				string @string = editText.Text.ToString();
				int hour = int.Parse(@string);

				editText = (EditText) outerInstance.mView.findViewById(R.id.editText2);
				@string = editText.Text.ToString();
				int minute = int.Parse(@string);

				mBcd.setTime(hour, minute);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : DialogInterface.OnClickListener
		{
			private readonly MainActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper5(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}
	}

}