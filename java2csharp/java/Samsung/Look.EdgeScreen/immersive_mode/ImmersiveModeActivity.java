package com.example.edgescreen.immersive_mode;

import com.example.edgescreen.R;
import android.os.Bundle;
import android.app.Activity;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;

import android.view.View.OnClickListener;
import android.view.animation.*;
import android.widget.ImageView;
import android.widget.TextView;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.look.Slook;
import com.samsung.android.sdk.look.cocktailbar.SlookCocktailSubWindow;

public class ImmersiveModeActivity extends Activity {

    private static final String TAG = "ImmersiveModeActivity";
    private TextView mainText;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        Slook slook = new Slook();

        try {
            slook.initialize(this);
        } catch (SsdkUnsupportedException e) {
            return;
        }
        
        if (slook.isFeatureEnabled(Slook.COCKTAIL_BAR)) {
            // If device supports cocktail bar, you can set up the sub-window.
            setContentView(R.layout.activity_immersive_mode);
            SlookCocktailSubWindow.setSubContentView(this, R.layout.sub_view);
        } else {
        	// Normal device
            setContentView(R.layout.activity_main);
        }

        mainText = (TextView)findViewById(R.id.main_text);
        ViewGroup subView = (ViewGroup)findViewById(R.id.sub_view_layout);

        if (subView != null) {
            int childCount = subView.getChildCount();
            for (int i = 0; i < childCount; i++) {
                if (subView.getChildAt(i) instanceof ImageView) {
                    subView.getChildAt(i).setOnClickListener(mSubViewOnClickListener);
                }
            }
        }
    }

    View.OnClickListener mSubViewOnClickListener = new OnClickListener() {

        @Override
        public void onClick(View arg0) {
            playClickAnim(arg0);
            switch (arg0.getId()) {
                case R.id.img01:
                    Log.i(TAG, "clicked iamge icon 01.");
                    mainText.setText("Selected pencil icon.");
                    break;
                case R.id.img02:
                    Log.i(TAG, "clicked iamge icon 02.");
                    mainText.setText("Selected pen icon.");
                    break;
                case R.id.img03:
                    Log.i(TAG, "clicked iamge icon 03.");
                    mainText.setText("Selected high lighter icon.");
                    break;
                case R.id.img04:
                    Log.i(TAG, "clicked iamge icon 04.");
                    mainText.setText("Selected calligraphy brush icon.");
                    break;
                case R.id.img05:
                    Log.i(TAG, "clicked iamge icon 05.");
                    mainText.setText("Selected brush icon.");
                    break;
                case R.id.img06:
                    Log.i(TAG, "clicked iamge icon 06.");
                    mainText.setText("Selected marker icon.");
                    break;
            }
        }
    };

    public static void playClickAnim(View targetView) {
        float SCALE_FACTOR = 0.3f;
        AnimationSet animSet = new AnimationSet(true);
        TranslateAnimation translateAnim = new TranslateAnimation(Animation.RELATIVE_TO_SELF, 0,
                Animation.RELATIVE_TO_SELF, -SCALE_FACTOR / 2, Animation.RELATIVE_TO_SELF, 0f,
                Animation.RELATIVE_TO_SELF, -SCALE_FACTOR / 2);
        animSet.addAnimation(translateAnim);
        ScaleAnimation anim = new ScaleAnimation(1f, 1f + SCALE_FACTOR, 1f, 1f + SCALE_FACTOR);

        animSet.addAnimation(anim);
        animSet.setInterpolator(new AccelerateDecelerateInterpolator());
        animSet.setDuration(150);
        targetView.startAnimation(animSet);

    }

}
