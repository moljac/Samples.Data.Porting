using System;

namespace com.bxl.postest
{


	using JposException = jpos.JposException;
	using POSPrinterConst = jpos.POSPrinterConst;
	using ContentResolver = android.content.ContentResolver;
	using Intent = android.content.Intent;
	using Cursor = android.database.Cursor;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using MediaStore = android.provider.MediaStore;
	using Nullable = android.support.annotation.Nullable;
	using ScrollingMovementMethod = android.text.method.ScrollingMovementMethod;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using EditText = android.widget.EditText;
	using SeekBar = android.widget.SeekBar;
	using OnSeekBarChangeListener = android.widget.SeekBar.OnSeekBarChangeListener;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;

	public class POSPrinterBitmapFragment : POSPrinterFragment, View.OnClickListener, SeekBar.OnSeekBarChangeListener
	{

		private TextView bitmapPathTextView;
		private EditText widthEditText;
		private Spinner alignmentSpinner;
		private SeekBar brightnessSeekBar;
		private TextView brightnessTextView;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public android.view.View onCreateView(android.view.LayoutInflater inflater, @Nullable android.view.ViewGroup container, @Nullable android.os.Bundle savedInstanceState)
		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View view = inflater.inflate(R.layout.fragment_pos_printer_bitmap, container, false);

			bitmapPathTextView = (TextView) view.findViewById(R.id.textViewBitmapPath);
			widthEditText = (EditText) view.findViewById(R.id.editTextWidth);
			alignmentSpinner = (Spinner) view.findViewById(R.id.spinnerAlignment);

			view.findViewById(R.id.buttonGallery).OnClickListener = this;
			view.findViewById(R.id.buttonPrintBitmap).OnClickListener = this;

			deviceMessagesTextView = (TextView) view.findViewById(R.id.textViewDeviceMessages);
			deviceMessagesTextView.MovementMethod = new ScrollingMovementMethod();
			deviceMessagesTextView.VerticalScrollBarEnabled = true;

			brightnessTextView = (TextView) view.findViewById(R.id.textViewBrightness);
			brightnessSeekBar = (SeekBar) view.findViewById(R.id.seekBarBrightness);
			brightnessSeekBar.OnSeekBarChangeListener = this;
			return view;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.buttonGallery:
				string externalStorageState = Environment.ExternalStorageState;
				if (externalStorageState.Equals(Environment.MEDIA_MOUNTED))
				{
					Intent intent = new Intent(Intent.ACTION_PICK);
					intent.Type = MediaStore.Images.Media.CONTENT_TYPE;
					Activity.startActivityForResult(intent, MainActivity.REQUEST_CODE_ACTION_PICK);
				}
				break;

			case R.id.buttonPrintBitmap:
				printBitmap();
				break;
			}
		}

		public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
		{
			brightnessTextView.Text = Convert.ToString(progress);
		}

		public override void onStartTrackingTouch(SeekBar seekBar)
		{
			// TODO Auto-generated method stub

		}

		public override void onStopTrackingTouch(SeekBar seekBar)
		{
			// TODO Auto-generated method stub

		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			if (requestCode == MainActivity.REQUEST_CODE_ACTION_PICK)
			{
				if (data != null)
				{
					Uri uri = data.Data;
					ContentResolver cr = Activity.ContentResolver;
					Cursor c = cr.query(uri, new string[] {MediaStore.Images.Media.DATA}, null, null, null);
					if (c == null || c.Count == 0)
					{
						return;
					}

					c.moveToFirst();
					int columnIndex = c.getColumnIndexOrThrow(MediaStore.Images.Media.DATA);
					string text = c.getString(columnIndex);

					bitmapPathTextView.Text = text;
				}
			}
		}

		private void printBitmap()
		{
			string fileName = bitmapPathTextView.Text.ToString();
			try
			{
				int width = Convert.ToInt32(widthEditText.Text.ToString());
				int alignment = Alignment;

				ByteBuffer buffer = ByteBuffer.allocate(4);
				buffer.put((sbyte) POSPrinterConst.PTR_S_RECEIPT);
				buffer.put((sbyte) brightnessSeekBar.Progress);
				buffer.put((sbyte) 0x00);
				buffer.put((sbyte) 0x00);

				posPrinter.printBitmap(buffer.getInt(0), fileName, width, alignment);
			}
//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java 'multi-catch' syntax:
			catch (NumberFormatException | JposException e)
			{
				e.printStackTrace();
				MessageDialogFragment.showDialog(FragmentManager, "Exception", e.Message);
				return;
			}
		}

		private int Alignment
		{
			get
			{
				switch (alignmentSpinner.SelectedItemPosition)
				{
				case 0:
					return POSPrinterConst.PTR_BM_LEFT;
    
				case 1:
					return POSPrinterConst.PTR_BM_CENTER;
    
				case 2:
					return POSPrinterConst.PTR_BM_RIGHT;
    
					default:
						return -1;
				}
			}
		}
	}

}