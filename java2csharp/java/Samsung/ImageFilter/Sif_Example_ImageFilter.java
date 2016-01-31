package com.example.filter_test;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.Intent;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.net.Uri;
import android.os.Bundle;
import android.text.InputType;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.ImageView;
import android.widget.ListView;
import android.widget.Toast;
import com.samsung.android.sdk.imagefilter.Sif;
import com.samsung.android.sdk.imagefilter.SifImageFilter;

public class Sif_Example_ImageFilter extends Activity {

    Context mContext = null;

    private final int REQUEST_CODE_SELECT_IMAGE_BACKGROUND = 106;

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

    boolean bShowListView = false;

    // Conversion array to map menu index to Image Filter Index
    private final int[] imageOperationByIndex = { SifImageFilter.FILTER_ORIGINAL, SifImageFilter.FILTER_GRAY,
            SifImageFilter.FILTER_SEPIA, SifImageFilter.FILTER_NEGATIVE, SifImageFilter.FILTER_BRIGHT,
            SifImageFilter.FILTER_DARK, SifImageFilter.FILTER_VINTAGE, SifImageFilter.FILTER_OLD_PHOTO,
            SifImageFilter.FILTER_FADED_COLOR, SifImageFilter.FILTER_VIGNETTE, SifImageFilter.FILTER_VIVID,
            SifImageFilter.FILTER_COLORIZE, SifImageFilter.FILTER_BLUR, SifImageFilter.FILTER_PENCIL_SKETCH,
            SifImageFilter.FILTER_FUSAIN, SifImageFilter.FILTER_PEN_SKETCH, SifImageFilter.FILTER_PASTEL_SKETCH,
            SifImageFilter.FILTER_COLOR_SKETCH, SifImageFilter.FILTER_PENCIL_PASTEL_SKETCH,
            SifImageFilter.FILTER_PENCIL_COLOR_SKETCH, SifImageFilter.FILTER_RETRO, SifImageFilter.FILTER_SUNSHINE,
            SifImageFilter.FILTER_DOWN_LIGHT, SifImageFilter.FILTER_BLUE_WASH, SifImageFilter.FILTER_NOSTALGIA,
            SifImageFilter.FILTER_YELLOW_GLOW, SifImageFilter.FILTER_SOFT_GLOW, SifImageFilter.FILTER_MOSAIC,
            SifImageFilter.FILTER_POPART, SifImageFilter.FILTER_MAGIC_PEN, SifImageFilter.FILTER_OIL_PAINT,
            SifImageFilter.FILTER_POSTERIZE, SifImageFilter.FILTER_CARTOONIZE, SifImageFilter.FILTER_CLASSIC };

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.general_purpose_image_filter);

        Sif imageFilterSdk = new Sif();
        try {
            imageFilterSdk.initialize(this);
        } catch (Exception e1) {
            // TODO Auto-generated catch block
            e1.printStackTrace();
        }

        mContext = this;

        mImageFilterBtn = (Button) findViewById(R.id.imagefilter);
        mImageFilterBtn.setOnClickListener(imagefilterBtnClickListener);
        mBackgroundImageBtn = (Button) findViewById(R.id.backgroundimage);
        mBackgroundImageBtn.setOnClickListener(backgroundimageBtnClickListener);
        mFilterLevelBtn = (Button) findViewById(R.id.filterlevel);
        mFilterLevelBtn.setOnClickListener(filterlevelBtnClickListener);

        mBackgroudnImageView = (ImageView) findViewById(R.id.imageview_background);

        arrayAdapter = ArrayAdapter.createFromResource(this, R.array.imageoperation,
                R.layout.general_purpose_filter_list);

        filterlistView = (ListView) findViewById(R.id.filter_list);
        filterlistView.setAdapter(arrayAdapter);
        filterlistView.setChoiceMode(ListView.CHOICE_MODE_SINGLE);

        // Don't show list view
        bShowListView = false;
        filterlistView.setVisibility(View.GONE);

        // initial background
        bmBackgroundBitmap = BitmapFactory.decodeResource(getResources(), R.drawable.baby);
        mBackgroudnImageView.setImageBitmap(bmBackgroundBitmap);

        // getting Get the original image
        // Bitmap backgroundBitmap = BitmapFactory.decodeResource(getResources(), R.drawable.baby);
        // Bitmap filteredBitmap = SifImageFilter.filterImageCopy(backgroundBitmap, SifImageFilter.FILTER_SEPIA,
        // SifImageFilter.LEVEL_MEDIUM);
        // Apply image filtering

        filterlistView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView parent, View view, int position, long id) {

                if (bmBackgroundBitmap == null) {
                    Toast.makeText(mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT)
                            .show();
                    return;
                }

                mImageOperationIndex = position;
                mImageOperation = imageOperationByIndex[mImageOperationIndex];

                if (mImageOperation == SifImageFilter.FILTER_ORIGINAL) {
                    mBackgroudnImageView.setImageBitmap(bmBackgroundBitmap);
                } else {
                    bmFilteredBitmap = bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

                    if (mImageOperation != SifImageFilter.FILTER_ORIGINAL) {
                        bmFilteredBitmap = SifImageFilter.filterImageCopy(bmBackgroundBitmap, mImageOperation,
                                mImageOperationLevel);
                        if (bmFilteredBitmap != null) {
                            mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                        } else {
                            Toast.makeText(mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
                        }
                    }

                    if (bmFilteredBitmap != null) {
                        bmFilteredBitmap = bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
                        SifImageFilter.setImageTransparency(bmFilteredBitmap, mTransparency);
                        mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                    }
                    // reset the transparency
                    mTransparency = 0;
                }
            }
        });
    }

    // set image filter
    private final OnClickListener imagefilterBtnClickListener = new OnClickListener() {
        @Override
        public void onClick(View v) {
            if (v.equals(mImageFilterBtn)) {
                // make image filter dialog
                bShowListView = !bShowListView;
                if (bShowListView) {
                    filterlistView.setVisibility(View.VISIBLE);
                    mImageFilterBtn.setText("Select Filter");
                } else {
                    filterlistView.setVisibility(View.GONE);
                    mImageFilterBtn.setText("Show Filter List");
                }
            }
        }
    };

    // set background image
    private final OnClickListener backgroundimageBtnClickListener = new OnClickListener() {
        @Override
        public void onClick(View v) {
            if (v.equals(mBackgroundImageBtn)) {
                callGalleryForInputImage(REQUEST_CODE_SELECT_IMAGE_BACKGROUND);
            }
        }
    };

    // set image filter
    private final OnClickListener filterlevelBtnClickListener = new OnClickListener() {
        @Override
        public void onClick(View v) {
            if (v.equals(mFilterLevelBtn)) {
                // make image filter dialog
                showSetFilterLevelMenu();
            }
        }
    };

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        menu.add("Set Image Transparency");
        return super.onCreateOptionsMenu(menu);
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // set transparency
        showSetFilterTransparency();
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);

        if (resultCode == RESULT_OK) {
            if (data == null) {
                return;
            }

            if (requestCode == REQUEST_CODE_SELECT_IMAGE_BACKGROUND) {
                Uri imageFileUri = data.getData();
                String strBackgroundImagePath = AnimationUtils.getRealPathFromURI(this, imageFileUri);

                // Check Valid Image File
                if (!AnimationUtils.isValidImagePath(strBackgroundImagePath)) {
                    Toast.makeText(this, "Invalid image path or web image", Toast.LENGTH_LONG).show();
                    return;
                }

                // Set background image on the image view
                bmBackgroundBitmap = AnimationUtils.getSafeResizingBitmap(strBackgroundImagePath,
                        mBackgroudnImageView.getWidth() / 2, mBackgroudnImageView.getHeight() / 2);

                if (bmBackgroundBitmap != null) {
                    // mBackgroudnImageView.setImageBitmap(bmBackgroundBitmap);
                    bmFilteredBitmap = bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

                    if (mImageOperation != SifImageFilter.FILTER_ORIGINAL) {
                        bmFilteredBitmap = SifImageFilter.filterImageCopy(bmBackgroundBitmap, mImageOperation,
                                mImageOperationLevel);
                        if (bmFilteredBitmap != null) {
                            mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                        } else {
                            Toast.makeText(mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
                        }
                    }

                    if (bmFilteredBitmap != null) {
                        bmFilteredBitmap = bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
                        SifImageFilter.setImageTransparency(bmFilteredBitmap, mTransparency);
                        mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                    }
                    // reset the transparency
                    mTransparency = 0;
                } else {
                    Toast.makeText(mContext, "Fail to set Background Image Path.", Toast.LENGTH_LONG).show();
                }
            }
        }
    }

    // ==================================================
    // Call Gallery
    // ==================================================
    private void callGalleryForInputImage(int nRequestCode) {
        Intent galleryIntent;
        galleryIntent = new Intent();
        galleryIntent.setAction(Intent.ACTION_GET_CONTENT);
        galleryIntent.setType("image/*");
        startActivityForResult(galleryIntent, nRequestCode);
    }

    private void showSetFilterLevelMenu() {

        if (bmBackgroundBitmap == null) {
            Toast.makeText(mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT).show();
            return;
        }
        AlertDialog dlg = new AlertDialog.Builder(this)
                .setIcon(getResources().getDrawable(android.R.drawable.ic_dialog_alert))
                .setTitle("Set Filter Level")
                .setSingleChoiceItems(R.array.imageoperation_level, mImageOperationLevelIndex,
                        new DialogInterface.OnClickListener() {
                            @Override
                            public void onClick(DialogInterface dialog, int whichButton) {
                                mImageOperationLevelIndex = whichButton;
                                mImageOperationLevel = mImageOperationLevelIndex;

                                if (bmBackgroundBitmap != null) {
                                    bmFilteredBitmap = bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);
                                }

                                if (mImageOperation != SifImageFilter.FILTER_ORIGINAL) {
                                    bmFilteredBitmap = SifImageFilter.filterImageCopy(bmBackgroundBitmap,
                                            mImageOperation, mImageOperationLevel);
                                    if (bmFilteredBitmap != null) {
                                        mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                                    } else {
                                        Toast.makeText(mContext, "Fail to apply image filter", Toast.LENGTH_LONG)
                                                .show();
                                    }
                                }

                                bmFilteredBitmap = bmFilteredBitmap.copy(Bitmap.Config.ARGB_8888, false);
                                SifImageFilter.setImageTransparency(bmFilteredBitmap, mTransparency);
                                mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                                // reset the transparency
                                mTransparency = 0;

                                dialog.dismiss();
                            }
                        }).create();
        dlg.show();
    }

    private void showSetFilterTransparency() {

        if (bmBackgroundBitmap == null) {
            Toast.makeText(mContext, "Input image does not selected!, select Image firtsly", Toast.LENGTH_SHORT).show();
            return;
        }
        final EditText text = new EditText(mContext);
        text.setInputType(InputType.TYPE_CLASS_NUMBER | InputType.TYPE_NUMBER_FLAG_DECIMAL);
        text.setText(Integer.toString(mTransparency));
        text.selectAll();
        AlertDialog dlg = new AlertDialog.Builder(this).setTitle("Set Image Transparency").setView(text)
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        int transparency;
                        try {
                            transparency = Integer.parseInt(text.getText().toString());
                        } catch (Exception e) {
                            Toast.makeText(mContext, "Invalid Value !", Toast.LENGTH_SHORT).show();
                            return;
                        }
                        if (transparency > 255 || transparency < 0) {
                            Toast.makeText(mContext, "Invalid Value !", Toast.LENGTH_SHORT).show();
                            return;
                        }
                        mTransparency = transparency;

                        bmFilteredBitmap = bmBackgroundBitmap.copy(Bitmap.Config.ARGB_8888, true);

                        if (mImageOperation != SifImageFilter.FILTER_ORIGINAL) {
                            bmFilteredBitmap = SifImageFilter.filterImageCopy(bmBackgroundBitmap, mImageOperation,
                                    mImageOperationLevel);
                            if (bmFilteredBitmap != null) {
                                mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                            } else {
                                Toast.makeText(mContext, "Fail to apply image filter", Toast.LENGTH_LONG).show();
                            }
                        }

                        if (bmFilteredBitmap != null) {
                            SifImageFilter.setImageTransparency(bmFilteredBitmap, mTransparency);
                            mBackgroudnImageView.setImageBitmap(bmFilteredBitmap);
                        }
                        // reset the transparency
                        mTransparency = 0;
                    }
                }).create();
        dlg.show();
    }
}
