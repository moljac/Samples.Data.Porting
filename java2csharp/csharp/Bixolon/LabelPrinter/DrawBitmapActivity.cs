namespace com.bixolon.labelprintersample
{


	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using MediaStore = android.provider.MediaStore;
	using Activity = android.app.Activity;
	using ContentResolver = android.content.ContentResolver;
	using Intent = android.content.Intent;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;

	public class DrawBitmapActivity : Activity, View.OnClickListener
	{
		private const int REQUEST_CODE_ACTION_PICK = 1;

		private ImageView mImageView;
		private TextView mTextView;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_draw_bitmap;

			mImageView = (ImageView) findViewById(R.id.imageView1);
			mTextView = (TextView) findViewById(R.id.textView5);

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.activity_draw_bitmap, menu);
			return true;
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

					try
					{
					BitmapFactory.Options opts = new BitmapFactory.Options();
					opts.inJustDecodeBounds = false;
					opts.inSampleSize = 1;
					opts.inPreferredConfig = Bitmap.Config.RGB_565;
					Bitmap bm = BitmapFactory.decodeStream(@is, null, opts);
					mImageView.ImageBitmap = bm;
					}
					catch (System.OutOfMemoryException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return;
					}

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
				printBitmap();
				break;

			case R.id.button2:
				pickAlbum();
				break;
			}
		}

		private void pickAlbum()
		{
			string externalStorageState = Environment.ExternalStorageState;
			if (externalStorageState.Equals(Environment.MEDIA_MOUNTED))
			{
				Intent intent = new Intent(Intent.ACTION_PICK);
				intent.Type = MediaStore.Images.Media.CONTENT_TYPE;
				startActivityForResult(intent, REQUEST_CODE_ACTION_PICK);
			}
		}

		private void printBitmap()
		{
			string pathName = mTextView.Text.ToString();
			Bitmap bitmap = null;
			if (pathName.Length == 0)
			{
				mImageView.buildDrawingCache();
				bitmap = mImageView.DrawingCache;
			}

			EditText editText = (EditText) findViewById(R.id.editText1);
			int horizontalStartPosition = int.Parse(editText.Text.ToString());
			editText = (EditText) findViewById(R.id.editText2);
			int verticalStartPosition = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText3);
			int width = int.Parse(editText.Text.ToString());

			editText = (EditText) findViewById(R.id.editText4);
			int level = int.Parse(editText.Text.ToString());

			if (bitmap == null)
			{
				MainActivity.mBixolonLabelPrinter.drawBitmap(pathName, horizontalStartPosition, verticalStartPosition, width, level);
			}
			else
			{
				MainActivity.mBixolonLabelPrinter.drawBitmap(bitmap, horizontalStartPosition, verticalStartPosition, width, level);
			}
			MainActivity.mBixolonLabelPrinter.print(1, 1);
		}

	}

}