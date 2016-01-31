
package com.samsung.android.example.slookdemos;

import java.util.ArrayList;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

import android.app.Activity;
import android.opengl.GLSurfaceView;
import android.os.Bundle;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Toast;

import com.samsung.android.sdk.look.Slook;
import com.samsung.android.sdk.look.airbutton.SlookAirButton;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter.AirButtonItem;

public class AirButtonSurfaceViewActivity extends Activity {

    private SlookAirButton mAirButton;

    private GLSurfaceView mGlSurfaceView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        mGlSurfaceView = new GLSurfaceView(this);
        mGlSurfaceView.setRenderer(new GLSurfaceView.Renderer() {

            public void onSurfaceCreated(GL10 gl, EGLConfig config) {
            }

            public void onSurfaceChanged(GL10 gl, int width, int height) {
                gl.glViewport(0, 0, width, height);

            }

            public void onDrawFrame(GL10 gl) {
                gl.glClearColor((float) 1.0, (float) 0.5, (float) 0.5, (float) 1.0);
                gl.glClear(GL10.GL_COLOR_BUFFER_BIT | GL10.GL_DEPTH_BUFFER_BIT);
            }
        });
        setContentView(mGlSurfaceView);
        mGlSurfaceView.setOnHoverListener(new View.OnHoverListener() {
            
            public boolean onHover(View v, MotionEvent event) {
                if(event.getAction()==MotionEvent.ACTION_HOVER_MOVE){
                    if((event.getButtonState()&MotionEvent.BUTTON_SECONDARY)!=0){
                        Log.e("newkkc79","event");
                    }
                }
                return false;
            }
        });
        mAirButton = new SlookAirButton(mGlSurfaceView, getAdapterMenuList(),
                SlookAirButton.UI_TYPE_MENU);
        mAirButton.setGravity(SlookAirButton.GRAVITY_HOVER_POINT);

        Slook slook = new Slook();
        if(!slook.isFeatureEnabled(Slook.AIRBUTTON)) {
            Toast.makeText(this, "This model doesn't support AirButton",
                    Toast.LENGTH_LONG).show();
        } else {
            Toast.makeText(this, "Please hover and push the Spen button on each button",
                    Toast.LENGTH_SHORT).show();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        mGlSurfaceView.onResume();
    }

    @Override
    protected void onPause() {
        super.onPause();
        mGlSurfaceView.onPause();
    }

    public SlookAirButtonAdapter getAdapterMenuList() {
        ArrayList<AirButtonItem> itemList = new ArrayList<AirButtonItem>();
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_archive),
                "Help", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_edit), "Edit",
                null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_help), "Help",
                null));

        return new SlookAirButtonAdapter(itemList);
    }

}
