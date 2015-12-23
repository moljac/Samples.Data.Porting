namespace com.bixolon.printersample
{


	using Activity = android.app.Activity;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using ContentResolver = android.content.ContentResolver;
	using Context = android.content.Context;
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
	using View = android.view.View;
	using Button = android.widget.Button;
	using CheckBox = android.widget.CheckBox;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;
	using RadioGroup = android.widget.RadioGroup;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class PrintBitmapAcitivity : Activity
	{
		private ImageView mImageView;
		private TextView mTextView;
		private EditText mWidthEdit;
		private RadioGroup mWidthRadioGroup;

		private int mAlignment;
		private bool mFormFeed;

		private const int REQUEST_CODE_ACTION_PICK = 1;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_print_bitmap;

			mImageView = (ImageView) findViewById(R.id.imageView1);
			mTextView = (TextView) findViewById(R.id.textView4);
			mWidthEdit = (EditText) findViewById(R.id.editText2);
			mWidthRadioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
			mWidthRadioGroup.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			IntentFilter filter = new IntentFilter();
			filter.addAction(MainActivity.ACTION_COMPLETE_PROCESS_BITMAP);
			registerReceiver(mReceiver, filter);
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
		{
			private readonly PrintBitmapAcitivity outerInstance;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(PrintBitmapAcitivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onCheckedChanged(RadioGroup group, int checkedId)
			{
				outerInstance.mWidthEdit.Enabled = checkedId == R.id.radio5;

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly PrintBitmapAcitivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(PrintBitmapAcitivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				string externalStorageState = Environment.ExternalStorageState;
				if (externalStorageState.Equals(Environment.MEDIA_MOUNTED))
				{
					Intent intent = new Intent(Intent.ACTION_PICK);
					intent.Type = MediaStore.Images.Media.CONTENT_TYPE;
					startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
				}

			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly PrintBitmapAcitivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(PrintBitmapAcitivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				outerInstance.printBitmap();
			}
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

		private void printBitmap()
		{
			string pathName = mTextView.Text.ToString();

			mAlignment = BixolonPrinter.ALIGNMENT_CENTER;
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			switch (radioGroup.CheckedRadioButtonId)
			{
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
			switch (radioGroup.CheckedRadioButtonId)
			{
			case R.id.radio3:
				width = BixolonPrinter.BITMAP_WIDTH_NONE;
				break;

			case R.id.radio4:
				width = BixolonPrinter.BITMAP_WIDTH_FULL;
				break;

			case R.id.radio5:
				string @string = mWidthEdit.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(ApplicationContext, "Please enter the width", Toast.LENGTH_SHORT).show();
				}
				else
				{
					width = int.Parse(@string);
				}
				break;
			}

			EditText editText = (EditText) findViewById(R.id.editText1);
			int level = int.Parse(editText.Text.ToString());

			CheckBox checkBox = (CheckBox) findViewById(R.id.checkBox1);
			mFormFeed = checkBox.Checked;

			checkBox = (CheckBox) findViewById(R.id.checkBox2);

			bool dither = ((CheckBox) findViewById(R.id.checkBox3)).Checked;
			bool compress = ((CheckBox) findViewById(R.id.checkBox4)).Checked;

			if (checkBox.Checked)
			{
				if (pathName != null && pathName.Length > 0)
				{
					MainActivity.mBixolonPrinter.printDotMatrixBitmap(pathName, mAlignment, width, level, false);
				}
				else
				{
					BitmapDrawable drawable = (BitmapDrawable) Resources.getDrawable(R.drawable.bixolon);
					Bitmap bitmap = drawable.Bitmap;

					MainActivity.mBixolonPrinter.printDotMatrixBitmap(bitmap, mAlignment, width, level, false);
				}
			}
			else
			{
				if (pathName != null && pathName.Length > 0)
				{
					MainActivity.mBixolonPrinter.printBitmap(pathName, mAlignment, width, level, dither, compress, true);
					/*
					 * You can choose the method. Please reference attached document.
					 * MainActivity.mBixolonPrinter.getMonoPixels(pathName, width, level);
					 */
				}
				else
				{
					BitmapDrawable drawable = (BitmapDrawable) Resources.getDrawable(R.drawable.bixolon);
					Bitmap bitmap = drawable.Bitmap;

					MainActivity.mBixolonPrinter.printBitmap(bitmap, mAlignment, width, level, dither, compress, true);
					/*
					 * You can choose the method. Please reference attached document.
					 * MainActivity.mBixolonPrinter.getMonoPixels(bitmap, width, level);
					 */
				}
			}
		}

		private void printBitmap(sbyte[] pixels, int width, int height)
		{
			MainActivity.mBixolonPrinter.printBitmap(pixels, mAlignment, width, height, false);
			if (mFormFeed)
			{
				MainActivity.mBixolonPrinter.formFeed(false);
			}
			MainActivity.mBixolonPrinter.cutPaper(0, true);
		}

		internal BroadcastReceiver mReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}


			public override void onReceive(Context context, Intent intent)
			{
				if (intent.Action.Equals(MainActivity.ACTION_COMPLETE_PROCESS_BITMAP))
				{
					sbyte[] pixels = intent.getByteArrayExtra(MainActivity.EXTRA_NAME_BITMAP_PIXELS);
					int width = intent.getIntExtra(MainActivity.EXTRA_NAME_BITMAP_WIDTH, 0);
					int height = intent.getIntExtra(MainActivity.EXTRA_NAME_BITMAP_HEIGHT, 0);

					outerInstance.printBitmap(pixels, width, height);
				}

			}
		}
	}

}