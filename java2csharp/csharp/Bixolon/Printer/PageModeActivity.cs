namespace com.bixolon.printersample
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Bitmap = android.graphics.Bitmap;
	using BitmapDrawable = android.graphics.drawable.BitmapDrawable;
	using Bundle = android.os.Bundle;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using RelativeLayout = android.widget.RelativeLayout;
	using Toast = android.widget.Toast;

	using BixolonPrinter = com.bixolon.printer.BixolonPrinter;

	public class PageModeActivity : Activity, View.OnClickListener
	{

		private AlertDialog mSampleDialog;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_page_mode;

			Button button = (Button) findViewById(R.id.button1);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button2);
			button.OnClickListener = this;
			button = (Button) findViewById(R.id.button3);
			button.OnClickListener = this;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.button1:
				setPageMode();
				break;

			case R.id.button2:
				printSample1();
				break;

			case R.id.button3:
				printSample2();
				break;
			}
		}

		private void setPageMode()
		{
			RadioGroup radioGroup = (RadioGroup) findViewById(R.id.radioGroup1);
			if (radioGroup.CheckedRadioButtonId == R.id.radio0)
			{
				MainActivity.mBixolonPrinter.setStandardMode();
			}
			else
			{
				MainActivity.mBixolonPrinter.setPageMode();

				radioGroup = (RadioGroup) findViewById(R.id.radioGroup2);
				switch (radioGroup.CheckedRadioButtonId)
				{
				case R.id.radio2:
					MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_0_DEGREE_ROTATION;
					break;
				case R.id.radio3:
					MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_90_DEGREE_ROTATION;
					break;
				case R.id.radio4:
					MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_180_DEGREE_ROTATION;
					break;
				case R.id.radio5:
					MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_270_DEGREE_ROTATION;
					break;
				}

				EditText editText = (EditText) findViewById(R.id.editText1);
				string @string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(ApplicationContext, "Please enter the horizontal start position again", Toast.LENGTH_SHORT).show();
					return;
				}
				int x = int.Parse(@string);

				editText = (EditText) findViewById(R.id.editText2);
				@string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(ApplicationContext, "Please enter the vertical start position again", Toast.LENGTH_SHORT).show();
					return;
				}
				int y = int.Parse(@string);

				editText = (EditText) findViewById(R.id.editText3);
				@string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(ApplicationContext, "Please enter the horizontal print area again", Toast.LENGTH_SHORT).show();
					return;
				}
				int width = int.Parse(@string);

				editText = (EditText) findViewById(R.id.editText4);
				@string = editText.Text.ToString();
				if (@string.Length == 0)
				{
					Toast.makeText(ApplicationContext, "Please enter the vertical print area again", Toast.LENGTH_SHORT).show();
					return;
				}
				int height = int.Parse(@string);

				MainActivity.mBixolonPrinter.setPrintArea(x, y, width, height);
			}
		}

		private void printSample1()
		{
			MainActivity.mBixolonPrinter.setPageMode();
			MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_180_DEGREE_ROTATION;
			MainActivity.mBixolonPrinter.AbsoluteVerticalPrintPosition = 0;
			MainActivity.mBixolonPrinter.AbsolutePrintPosition = 0;
			MainActivity.mBixolonPrinter.printText("Page mode\nsample", BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.TEXT_ATTRIBUTE_FONT_A, BixolonPrinter.TEXT_SIZE_HORIZONTAL1 | BixolonPrinter.TEXT_SIZE_VERTICAL1, false);
			MainActivity.mBixolonPrinter.AbsoluteVerticalPrintPosition = 0;
			MainActivity.mBixolonPrinter.AbsolutePrintPosition = 128;

			BitmapDrawable drawable = (BitmapDrawable) Resources.getDrawable(R.drawable.bixolon);
			Bitmap bitmap = drawable.Bitmap;
			MainActivity.mBixolonPrinter.printBitmap(bitmap, BixolonPrinter.ALIGNMENT_LEFT, 128, 70, false);
			MainActivity.mBixolonPrinter.AbsoluteVerticalPrintPosition = 0;
			MainActivity.mBixolonPrinter.AbsolutePrintPosition = 256;
			MainActivity.mBixolonPrinter.printQrCode("www.bixolon.com", BixolonPrinter.ALIGNMENT_LEFT, BixolonPrinter.QR_CODE_MODEL2, 4, false);
			MainActivity.mBixolonPrinter.formFeed(true);
		}

		private void printSample2()
		{
			if (mSampleDialog == null)
			{
				LayoutInflater inflater = (LayoutInflater) getSystemService(LAYOUT_INFLATER_SERVICE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = inflater.inflate(R.layout.dialog_page_mode_sample, null);
				View view = inflater.inflate(R.layout.dialog_page_mode_sample, null);

				mSampleDialog = (new AlertDialog.Builder(PageModeActivity.this)).setView(view).setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper(this, view))
			   .setNegativeButton("Cancel", new OnClickListenerAnonymousInnerClassHelper2(this))
			   .create();
			}

			mSampleDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly PageModeActivity outerInstance;

			private View view;

			public OnClickListenerAnonymousInnerClassHelper(PageModeActivity outerInstance, View view)
			{
				this.outerInstance = outerInstance;
				this.view = view;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				RelativeLayout layout = (RelativeLayout) view.findViewById(R.id.relativeLayout1);
				layout.buildDrawingCache();
				Bitmap bitmap = layout.DrawingCache;

				MainActivity.mBixolonPrinter.setPageMode();
				MainActivity.mBixolonPrinter.PrintDirection = BixolonPrinter.DIRECTION_180_DEGREE_ROTATION;
				MainActivity.mBixolonPrinter.setPrintArea(0, 0, 384, 840);
				MainActivity.mBixolonPrinter.AbsoluteVerticalPrintPosition = 100;
				MainActivity.mBixolonPrinter.printBitmap(bitmap, BixolonPrinter.ALIGNMENT_CENTER, BixolonPrinter.BITMAP_WIDTH_FULL, 88, false);
				MainActivity.mBixolonPrinter.formFeed(true);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly PageModeActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(PageModeActivity outerInstance)
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