using System;
using System.Collections.Generic;

namespace com.bixolon.printersample
{


	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using ContentResolver = android.content.ContentResolver;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using BitmapDrawable = android.graphics.drawable.BitmapDrawable;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using MediaStore = android.provider.MediaStore;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class NvImageActivity : Activity, View.OnClickListener
	{

		private const int REQUEST_CODE_ACTION_PICK = 1;

		private ImageView mImageView;
		private TextView mTextView;

		private ListView mListView;
		private List<string> mArrayList = new List<string>();
		private ArrayAdapter<string> mAdapter;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_nv_image;

			mImageView = (ImageView) findViewById(R.id.imageView1);
			mTextView = (TextView) findViewById(R.id.textView3);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button3);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button4);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button5);
			button.OnClickListener = this;

			mAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_single_choice, mArrayList);

			mListView = (ListView) findViewById(R.id.listView1);
			mListView.Adapter = mAdapter;
			mListView.ChoiceMode = ListView.CHOICE_MODE_SINGLE;

			IntentFilter filter = new IntentFilter();
			filter.addAction(MainActivity.ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES);
			registerReceiver(mReceiver, filter);

			MainActivity.mBixolonPrinter.DefinedNvImageKeyCodes;
		}

		public override void onDestroy()
		{
			unregisterReceiver(mReceiver);
			base.onDestroy();
		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);

			if (requestCode == REQUEST_CODE_ACTION_PICK)
			{
				if (data != null)
				{
					Uri uri = data.Data;
					System.IO.Stream @is = null;
					try
					{
						@is = ContentResolver.openInputStream(uri);
					}
					catch (FileNotFoundException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return;
					}

					BitmapFactory.Options opts = new BitmapFactory.Options();
					opts.inJustDecodeBounds = false;
					opts.inSampleSize = 1;
					opts.inPreferredConfig = Bitmap.Config.RGB_565;
					Bitmap bm = BitmapFactory.decodeStream(@is, null, opts);
					mImageView.ImageBitmap = bm;

					ContentResolver cr = ContentResolver;
					Cursor c = cr.query(uri, new string[] {MediaStore.Images.Media.DATA}, null, null, null);
					if (c == null || c.Count == 0)
					{
						return;
					}
					c.moveToFirst();
					int columnIndex = c.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
					string text = c.getString(columnIndex);
					mTextView.Text = text;
				}
			}
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				string externalStorageState = Environment.ExternalStorageState;
				if (externalStorageState.Equals(Environment.MEDIA_MOUNTED))
				{
					Intent intent = new Intent(Intent.ACTION_PICK);
					intent.Type = MediaStore.Images.Media.CONTENT_TYPE;
					startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
				}
				break;

			case R.id.button2:
				LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_nv_image, null);
				View view = inflater.inflate(R.layout.dialog_nv_image, null);

				(new AlertDialog.Builder(NvImageActivity.this)).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper(this, view))
			   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper2(this))
			   .show();
				break;

			case R.id.button3:
				int checkedItemPosition = mListView.CheckedItemPosition;
				if (checkedItemPosition < 0 || mArrayList.Count == 0)
				{
					Toast.makeText(ApplicationContext, "Please choose one key code.", Toast.LENGTH_SHORT).show();
				}
				else
				{
					MainActivity.mBixolonPrinter.printNvImage(int.Parse(mArrayList[checkedItemPosition]), true);
				}
				break;

			case R.id.button4:
				checkedItemPosition = mListView.CheckedItemPosition;
				if (checkedItemPosition < 0 || mArrayList.Count == 0)
				{
					Toast.makeText(ApplicationContext, "Please choose one key code.", Toast.LENGTH_SHORT).show();
				}
				else
				{
					MainActivity.mBixolonPrinter.removeNvImage(int.Parse(mArrayList[checkedItemPosition]));
					mListView.clearChoices();
				}
				break;

			case R.id.button5:
				MainActivity.mBixolonPrinter.removeAllNvImage();
				mListView.clearChoices();
				break;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly NvImageActivity outerInstance;

			private View view;

			public OnClickListenerAnonymousInnerClassHelper(NvImageActivity outerInstance, View view)
			{
				this.outerInstance = outerInstance;
				this.view = view;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				outerInstance.defineNvImage(view);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly NvImageActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(NvImageActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}

		private void defineNvImage(View view)
		{
			try
			{
				EditText editText = (EditText) findViewById(R.id.editText1);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int level = Integer.parseInt(editText.getText().toString());
				int level = int.Parse(editText.Text.ToString());

				editText = (EditText) view.findViewById(R.id.editText1);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int keyCode = Integer.parseInt(editText.getText().toString());
				int keyCode = int.Parse(editText.Text.ToString());

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String pathName = mTextView.getText().toString();
				string pathName = mTextView.Text.ToString();

				if (pathName != null && pathName.Length > 0)
				{
					MainActivity.mBixolonPrinter.defineNvImage(pathName, BixolonPrinter.BITMAP_WIDTH_NONE, level, keyCode);
				}
				else
				{
					BitmapDrawable drawable = (BitmapDrawable) Resources.getDrawable(R.drawable.bixolon);
					Bitmap bitmap = drawable.Bitmap;

					MainActivity.mBixolonPrinter.defineNvImage(bitmap, BixolonPrinter.BITMAP_WIDTH_NONE, level, keyCode);
				}
			}
			catch (System.FormatException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(ApplicationContext, "Please input key code or level.", Toast.LENGTH_SHORT).show();
			}
		}

		internal BroadcastReceiver mReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}


			public override void onReceive(Context context, Intent intent)
			{
				if (intent.Action.Equals(MainActivity.ACTION_GET_DEFINEED_NV_IMAGE_KEY_CODES))
				{
					int[] keyCodes = intent.getIntArrayExtra(MainActivity.EXTRA_NAME_NV_KEY_CODES);
					outerInstance.mArrayList.Clear();
					if (keyCodes != null)
					{
						for (int i = 0; i < keyCodes.Length; i++)
						{
							outerInstance.mArrayList.Add(Convert.ToString(keyCodes[i]));
						}
					}
					outerInstance.mAdapter.notifyDataSetChanged();
				}

			}
		}
	}

}