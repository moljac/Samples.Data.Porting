using System;

namespace com.bxl.pdftest
{


	using JposException = jpos.JposException;
	using POSPrinter = jpos.POSPrinter;
	using POSPrinterConst = jpos.POSPrinterConst;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;
	using View = android.view.View;
	using EditText = android.widget.EditText;
	using RadioGroup = android.widget.RadioGroup;
	using Toast = android.widget.Toast;

	public class MainActivity : ActionBarActivity, View.OnClickListener, ListDialogFragment.OnClickListener
	{

		private static readonly string PATH = Environment.ExternalStorageDirectory.AbsolutePath + "/BIXOLON";

		private const string CONFIG_FILE_NAME = "jpos.xml";

		private POSPrinter posPrinter;
		private EditText logicalNameEditText;
		private EditText widthEditText;
		private EditText pageEditText;
		private EditText brightnessEditText;
		private RadioGroup alignmentRadioGroup;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			logicalNameEditText = (EditText) findViewById(R.id.editTextLogicalName);

			widthEditText = (EditText) findViewById(R.id.editTextWidth);
			pageEditText = (EditText) findViewById(R.id.editTextPage);
			brightnessEditText = (EditText) findViewById(R.id.editTextBrightness);

			alignmentRadioGroup = (RadioGroup) findViewById(R.id.radioGroupAlignment);

			findViewById(R.id.buttonOCE).OnClickListener = this;
			findViewById(R.id.buttonPrintPDF).OnClickListener = this;
			findViewById(R.id.buttonClose).OnClickListener = this;

			createConfigFile();

			if (posPrinter == null)
			{
				posPrinter = new POSPrinter(this);
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			try
			{
				posPrinter.close();
			}
			catch (JposException e)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public virtual void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonOCE:
				try
				{
					posPrinter.open(logicalNameEditText.Text.ToString());
					posPrinter.claim(0);
					posPrinter.DeviceEnabled = true;
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);

					try
					{
						posPrinter.close();
					}
					catch (JposException e1)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e1.ToString());
						Console.Write(e1.StackTrace);
					}

					Toast.makeText(ApplicationContext, e.Message, Toast.LENGTH_SHORT).show();
				}
				break;

			case R.id.buttonPrintPDF:
				showFileList();
				break;

			case R.id.buttonClose:
				try
				{
					posPrinter.close();
				}
				catch (JposException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					Toast.makeText(ApplicationContext, e.Message, Toast.LENGTH_SHORT).show();
				}
				break;
			}
		}

		public virtual void onClick(string item)
		{
			printPDF(item);
		}

		private void createConfigFile()
		{
			System.IO.FileStream fis = null;

			try
			{
				fis = openFileInput(CONFIG_FILE_NAME);
			}
			catch (FileNotFoundException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
				createNewConfigFile();
			}

			if (fis != null)
			{
				try
				{
					fis.Close();
				}
				catch (IOException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private void createNewConfigFile()
		{
			System.IO.Stream @is = Resources.openRawResource(R.raw.jpos);
			System.IO.FileStream fos = null;

			int available = 0;
			sbyte[] buffer = null;

			try
			{
				available = @is.available();
				buffer = new sbyte[available];
				@is.Read(buffer, 0, buffer.Length);

				fos = openFileOutput(CONFIG_FILE_NAME, MODE_PRIVATE);
				fos.Write(buffer, 0, buffer.Length);
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, e.Message, Toast.LENGTH_SHORT).show();
			}
			finally
			{
				try
				{
					@is.Close();
				}
				catch (IOException e)
				{
					// TODO Auto-generated catch block
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				if (fos != null)
				{
					try
					{
						fos.Close();
					}
					catch (IOException e)
					{
						// TODO Auto-generated catch block
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}
			}
		}

		private void showFileList()
		{
			File file = new File(PATH);
			File[] files = file.listFiles(new FileFilterAnonymousInnerClassHelper(this));

			if (files != null && files.Length > 0)
			{
				string[] items = new string[files.Length];
				for (int i = 0; i < items.Length; i++)
				{
					items[i] = files[i].AbsolutePath;
				}
				ListDialogFragment.showDialog(SupportFragmentManager, "PDF file list", items);
			}
			else
			{
				Toast.makeText(ApplicationContext, "No PDF file", Toast.LENGTH_SHORT).show();
			}
		}

		private class FileFilterAnonymousInnerClassHelper : FileFilter
		{
			private readonly MainActivity outerInstance;

			public FileFilterAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool accept(File pathname)
			{
				if (pathname.Directory)
				{
					return false;
				}

				string name = pathname.Name;
				int lastIndex = name.LastIndexOf('.');
				if (lastIndex < 0)
				{
					return false;
				}

				return name.Substring(lastIndex).Equals(".pdf", StringComparison.CurrentCultureIgnoreCase);
			}
		}

		private void printPDF(string fileName)
		{
			try
			{
				int width = Convert.ToInt32(widthEditText.Text.ToString());
				int page = Convert.ToInt32(pageEditText.Text.ToString());
				int brightness = Convert.ToInt32(brightnessEditText.Text.ToString());

				int alignment = POSPrinterConst.PTR_PDF_LEFT;
				switch (alignmentRadioGroup.CheckedRadioButtonId)
				{
				case R.id.radioCenter:
					alignment = POSPrinterConst.PTR_PDF_CENTER;
					break;

				case R.id.radioRight:
					alignment = POSPrinterConst.PTR_PDF_RIGHT;
					break;
				}

				posPrinter.printPDF(POSPrinterConst.PTR_S_RECEIPT, fileName, width, alignment, page, brightness);
			}
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java 'multi-catch' syntax:
			catch (NumberFormatException | JposException e)
			{
				e.printStackTrace();
				Toast.makeText(ApplicationContext, e.Message, Toast.LENGTH_SHORT).show();
			}
		}
	}

}