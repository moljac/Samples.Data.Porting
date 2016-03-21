using System;

namespace com.example.filter_test
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using InputType = android.text.InputType;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;
	using ListView = android.widget.ListView;
	using Toast = android.widget.Toast;
	using Sif = com.samsung.android.sdk.imagefilter.Sif;
	using SifImageFilter = com.samsung.android.sdk.imagefilter.SifImageFilter;

	public class Sif_Example_ImageFilter : Activity
	{

		internal Context mContext = null;

		private readonly int REQUEST_CODE_SELECT_IMAGE_BACKGROUND = 106;

		private Button mBackgroundImageBtn;

		private Button mImageFilterBtn;

		private Button mFilterLevelBtn;

		private ImageView mBackgroudnImageView;

		private Bitmap bmBackgroundBitmap = null;

		private Bitmap bmFilteredBitmap = null;

		private int mImageOperation = SifImageFilter.FILTER_ORIGINAL;

		private int mImageOperationIndex = 0;

		private int mImageOperationLevel = SifImageFilter.LEVEL_MEDIUM;

		private int mImageOperationLevelIndex = 2;

		private int mTransparency = 0;

		private ArrayAdapter<CharSequence> arrayAdapter;

		private ListView filterlistView;

		internal bool bShowListView = false;

		// Conversion array to map menu index to Image Filter Index
		private readonly int[] imageOperationByIndex = new int[] {SifImageFilter.FILTER_ORIGINAL, SifImageFilter.FILTER_GRAY, SifImageFilter.FILTER_SEPIA, SifImageFilter.FILTER_NEGATIVE, SifImageFilter.FILTER_BRIGHT, SifImageFilter.FILTER_DARK, SifImageFilter.FILTER_VINTAGE, SifImageFilter.FILTER_OLD_PHOTO, SifImageFilter.FILTER_FADED_COLOR, SifImageFilter.FILTER_VIGNETTE, SifImageFilter.FILTER_VIVID, SifImageFilter.FILTER_COLORIZE, SifImageFilter.FILTER_BLUR, SifImageFilter.FILTER_PENCIL_SKETCH, SifImageFilter.FILTER_FUSAIN, SifImageFilter.FILTER_PEN_SKETCH, SifImageFilter.FILTER_PASTEL_SKETCH, SifImageFilter.FILTER_COLOR_SKETCH, SifImageFilter.FILTER_PENCIL_PASTEL_SKETCH, SifImageFilter.FILTER_PENCIL_COLOR_SKETCH, SifImageFilter.FILTER_RETRO, SifImageFilter.FILTER_SUNSHINE, SifImageFilter.FILTER_DOWN_LIGHT, SifImageFilter.FILTER_BLUE_WASH, SifImageFilter.FILTER_NOSTALGIA, SifImageFilter.FILTER_YELLOW_GLOW, SifImageFilter.FILTER_SOFT_GLOW, SifImageFilter.FILTER_MOSAIC, SifImageFilter.FILTER_POPART, SifImageFilter.FILTER_MAGIC_PEN, SifImageFilter.FILTER_OIL_PAINT, SifImageFilter.FILTER_POSTERIZE, SifImageFilter.FILTER_CARTOONIZE, SifImageFilter.FILTER_CLASSIC};

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.general_purpose_image_filter;

			Sif imageFilterSdk = new Sif();
			try
			{
				imageFilterSdk.initialize(this);
			}
			catch (Exception e1)
			{
				// TODO Auto-generated catch block
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
			}

			mContext = this;

			mImageFilterBtn = (Button) findViewById(R.id.imagefilter);
			mImageFilterBtn.OnClickListener = imagefilterBtnClickListener;
			mBackgroundImageBtn = (Button) findViewById(R.id.backgroundimage);
			mBackgroundImageBtn.OnClickListener = backgroundimageBtnClickListener;
			mFilterLevelBtn = (Button) findViewById(R.id.filterlevel);
			mFilterLevelBtn.OnClickListener = filterlevelBtnClickListener;

			mBackgroudnImageView = (ImageView) findViewById(R.id.imageview_background);

			arrayAdapter = ArrayAdapter.createFromResource(this, R.array.imageoperation, R.layout.general_purpose_filter_list);

			filterlistView = (ListView) findViewById(R.id.filter_list);
			filterlistView.Adapter = arrayAdapter;
			filterlistView.ChoiceMode = ListView.CHOICE_MODE_SINGLE;

			// Don't show list view
			bShowListView = false;
			filterlistView.Visibility = View.GONE;

			// initial background
			bmBackgroundBitmap = BitmapFactory.decodeResource(Resources, R.drawable.baby);
			mBackgroudnImageView.ImageBitmap = bmBackgroundBitmap;

			// getting Get the original image
			// Bitmap backgroundBitmap = BitmapFactory.decodeResource(getResources(), R.drawable.baby);
			// Bitmap filteredBitmap = SifImageFilter.filterImageCopy(backgroundBitmap, SifImageFilter.FILTER_SEPIA,
			// SifImageFilter.LEVEL_MEDIUM);
			// Apply image filtering

			filterlistView.OnItemClickListener = new OnItemClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemClickListenerAnonymousInnerClassHelper : AdapterView.OnItemClickListener
		{
			private readonly Sif_Example_ImageFilter outerInstance;

			public OnItemClickListenerAnonymousInnerClassHelper(Sif_Example_ImageFilter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemClick(AdapterView parent, View view, int position, long id)
			{

				if (outerInstance.bmBackgroundBitmap == null)
				{
					Toast.makeText(outerInstance.mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT).show();
					return;
				}

				outerInstance.mImageOperationIndex = position;
				outerInstance.mImageOperation = outerInstance.imageOperationByIndex[outerInstance.mImageOperationIndex];

				if (outerInstance.mImageOperation == SifImageFilter.FILTER_ORIGINAL)
				{
					outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmBackgroundBitmap;
				}
				else
				{
					outerInstance.bmFilteredBitmap = outerInstance.bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

					if (outerInstance.mImageOperation != SifImageFilter.FILTER_ORIGINAL)
					{
						outerInstance.bmFilteredBitmap = SifImageFilter.filterImageCopy(outerInstance.bmBackgroundBitmap, outerInstance.mImageOperation, outerInstance.mImageOperationLevel);
						if (outerInstance.bmFilteredBitmap != null)
						{
							outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
						}
						else
						{
							Toast.makeText(outerInstance.mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
						}
					}

					if (outerInstance.bmFilteredBitmap != null)
					{
						outerInstance.bmFilteredBitmap = outerInstance.bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
						SifImageFilter.setImageTransparency(outerInstance.bmFilteredBitmap, outerInstance.mTransparency);
						outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
					}
					// reset the transparency
					outerInstance.mTransparency = 0;
				}
			}
		}

		// set image filter
		private readonly View.OnClickListener imagefilterBtnClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}

			public override void onClick(View v)
			{
				if (v.Equals(outerInstance.mImageFilterBtn))
				{
					// make image filter dialog
					outerInstance.bShowListView = !outerInstance.bShowListView;
					if (outerInstance.bShowListView)
					{
						outerInstance.filterlistView.Visibility = View.VISIBLE;
						outerInstance.mImageFilterBtn.Text = "Select Filter";
					}
					else
					{
						outerInstance.filterlistView.Visibility = View.GONE;
						outerInstance.mImageFilterBtn.Text = "Show Filter List";
					}
				}
			}
		}

		// set background image
		private readonly View.OnClickListener backgroundimageBtnClickListener = new OnClickListenerAnonymousInnerClassHelper2();

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper2()
			{
			}

			public override void onClick(View v)
			{
				if (v.Equals(outerInstance.mBackgroundImageBtn))
				{
					outerInstance.callGalleryForInputImage(outerInstance.REQUEST_CODE_SELECT_IMAGE_BACKGROUND);
				}
			}
		}

		// set image filter
		private readonly View.OnClickListener filterlevelBtnClickListener = new OnClickListenerAnonymousInnerClassHelper3();

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper3()
			{
			}

			public override void onClick(View v)
			{
				if (v.Equals(outerInstance.mFilterLevelBtn))
				{
					// make image filter dialog
					outerInstance.showSetFilterLevelMenu();
				}
			}
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			menu.add("Set Image Transparency");
			return base.onCreateOptionsMenu(menu);
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			// set transparency
			showSetFilterTransparency();
			return base.onOptionsItemSelected(item);
		}

		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);

			if (resultCode == RESULT_OK)
			{
				if (data == null)
				{
					return;
				}

				if (requestCode == REQUEST_CODE_SELECT_IMAGE_BACKGROUND)
				{
					Uri imageFileUri = data.Data;
					string strBackgroundImagePath = AnimationUtils.getRealPathFromURI(this, imageFileUri);

					// Check Valid Image File
					if (!AnimationUtils.isValidImagePath(strBackgroundImagePath))
					{
						Toast.makeText(this, "Invalid image path or web image", Toast.LENGTH_LONG).show();
						return;
					}

					// Set background image on the image view
					bmBackgroundBitmap = AnimationUtils.getSafeResizingBitmap(strBackgroundImagePath, mBackgroudnImageView.Width / 2, mBackgroudnImageView.Height / 2);

					if (bmBackgroundBitmap != null)
					{
						// mBackgroudnImageView.setImageBitmap(bmBackgroundBitmap);
						bmFilteredBitmap = bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

						if (mImageOperation != SifImageFilter.FILTER_ORIGINAL)
						{
							bmFilteredBitmap = SifImageFilter.filterImageCopy(bmBackgroundBitmap, mImageOperation, mImageOperationLevel);
							if (bmFilteredBitmap != null)
							{
								mBackgroudnImageView.ImageBitmap = bmFilteredBitmap;
							}
							else
							{
								Toast.makeText(mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
							}
						}

						if (bmFilteredBitmap != null)
						{
							bmFilteredBitmap = bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
							SifImageFilter.setImageTransparency(bmFilteredBitmap, mTransparency);
							mBackgroudnImageView.ImageBitmap = bmFilteredBitmap;
						}
						// reset the transparency
						mTransparency = 0;
					}
					else
					{
						Toast.makeText(mContext, "Fail to set Background Image Path.", Toast.LENGTH_LONG).show();
					}
				}
			}
		}

		// ==================================================
		// Call Gallery
		// ==================================================
		private void callGalleryForInputImage(int nRequestCode)
		{
			Intent galleryIntent;
			galleryIntent = new Intent();
			galleryIntent.Action = Intent.ACTION_GET_CONTENT;
			galleryIntent.Type = "image/*";
			startActivityForResult(galleryIntent, nRequestCode);
		}

		private void showSetFilterLevelMenu()
		{

			if (bmBackgroundBitmap == null)
			{
				Toast.makeText(mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT).show();
				return;
			}
			AlertDialog dlg = (new AlertDialog.Builder(this)).setIcon(Resources.getDrawable(android.R.drawable.ic_dialog_alert)).setTitle("Set Filter Level").setSingleChoiceItems(R.array.imageoperation_level, mImageOperationLevelIndex, new OnClickListenerAnonymousInnerClassHelper(this))
						   .create();
			dlg.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly Sif_Example_ImageFilter outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(Sif_Example_ImageFilter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int whichButton)
			{
				outerInstance.mImageOperationLevelIndex = whichButton;
				outerInstance.mImageOperationLevel = outerInstance.mImageOperationLevelIndex;

				if (outerInstance.bmBackgroundBitmap != null)
				{
					outerInstance.bmFilteredBitmap = outerInstance.bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);
				}

				if (outerInstance.mImageOperation != SifImageFilter.FILTER_ORIGINAL)
				{
					outerInstance.bmFilteredBitmap = SifImageFilter.filterImageCopy(outerInstance.bmBackgroundBitmap, outerInstance.mImageOperation, outerInstance.mImageOperationLevel);
					if (outerInstance.bmFilteredBitmap != null)
					{
						outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
					}
					else
					{
						Toast.makeText(outerInstance.mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
					}
				}

				outerInstance.bmFilteredBitmap = outerInstance.bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
				SifImageFilter.setImageTransparency(outerInstance.bmFilteredBitmap, outerInstance.mTransparency);
				outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
				// reset the transparency
				outerInstance.mTransparency = 0;

				dialog.dismiss();
			}
		}

		private void showSetFilterTransparency()
		{

			if (bmBackgroundBitmap == null)
			{
				Toast.makeText(mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT).show();
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.EditText text = new android.widget.EditText(mContext);
			EditText text = new EditText(mContext);
			text.InputType = InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL;
			text.Text = Convert.ToString(mTransparency);
			text.selectAll();
			AlertDialog dlg = (new AlertDialog.Builder(this)).setTitle("Set Image Transparency").setView(text).setPositiveButton(android.R.@string.ok, new OnClickListenerAnonymousInnerClassHelper2(this, text))
				   .create();
			dlg.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
		{
			private readonly Sif_Example_ImageFilter outerInstance;

			private EditText text;

			public OnClickListenerAnonymousInnerClassHelper2(Sif_Example_ImageFilter outerInstance, EditText text)
			{
				this.outerInstance = outerInstance;
				this.text = text;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
				int transparency;
				try
				{
					transparency = int.Parse(text.Text.ToString());
				}
				catch (Exception)
				{
					Toast.makeText(outerInstance.mContext, "Invalid Value !", Toast.LENGTH_SHORT).show();
					return;
				}
				if (transparency > 255 || transparency < 0)
				{
					Toast.makeText(outerInstance.mContext, "Invalid Value !", Toast.LENGTH_SHORT).show();
					return;
				}
				outerInstance.mTransparency = transparency;

				outerInstance.bmFilteredBitmap = outerInstance.bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

				if (outerInstance.mImageOperation != SifImageFilter.FILTER_ORIGINAL)
				{
					outerInstance.bmFilteredBitmap = SifImageFilter.filterImageCopy(outerInstance.bmBackgroundBitmap, outerInstance.mImageOperation, outerInstance.mImageOperationLevel);
					if (outerInstance.bmFilteredBitmap != null)
					{
						outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
					}
					else
					{
						Toast.makeText(outerInstance.mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
					}
				}

				if (outerInstance.bmFilteredBitmap != null)
				{
					SifImageFilter.setImageTransparency(outerInstance.bmFilteredBitmap, outerInstance.mTransparency);
					outerInstance.mBackgroudnImageView.ImageBitmap = outerInstance.bmFilteredBitmap;
				}
				// reset the transparency
				outerInstance.mTransparency = 0;
			}
		}
	}

}