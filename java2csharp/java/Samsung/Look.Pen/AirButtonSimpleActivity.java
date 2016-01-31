
package com.samsung.android.example.slookdemos;

import java.util.ArrayList;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.samsung.android.sdk.look.Slook;
import com.samsung.android.sdk.look.airbutton.SlookAirButton;
import com.samsung.android.sdk.look.airbutton.SlookAirButton.ItemSelectListener;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter.AirButtonItem;

public class AirButtonSimpleActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_airbutton_simple);

        Button btnMenu = (Button) findViewById(R.id.btn_menu);
        createMenuWidgetFromView(btnMenu);
        Button btnRecipient = (Button) findViewById(R.id.btn_recipient);
        createRecipientListWidgetFromView(btnRecipient);
        Button btnImage = (Button) findViewById(R.id.btn_image);
        createImageListWidgetFromView(btnImage);
        Button btnText = (Button) findViewById(R.id.btn_text);
        createTextListWidgetFromView(btnText);

        Slook slook = new Slook();
        if(!slook.isFeatureEnabled(Slook.AIRBUTTON)) {
            Toast.makeText(this, "This model doesn't support AirButton",
                    Toast.LENGTH_LONG).show();
        } else {
            Toast.makeText(this, "Please hover and press the S-pen side-button on each button",
                    Toast.LENGTH_SHORT).show();
        }
    }

    private ItemSelectListener mCallback = new ItemSelectListener() {
        public void onItemSelected(View arg0, int arg1, Object arg2) {
            Toast.makeText(AirButtonSimpleActivity.this, "Item index = " + arg1, Toast.LENGTH_SHORT)
                    .show();
        }
    };

    public SlookAirButton createImageListWidgetFromView(View v) {

        SlookAirButton airButtonWidget = new SlookAirButton(v, getAdapterImageList(),
                SlookAirButton.UI_TYPE_LIST);
        airButtonWidget.setItemSelectListener(mCallback);
        airButtonWidget.setGravity(SlookAirButton.GRAVITY_LEFT);
        airButtonWidget.setDirection(SlookAirButton.DIRECTION_UPPER);
        airButtonWidget.setPosition(0, -50);

        return airButtonWidget;
    }

    public SlookAirButton createTextListWidgetFromView(View v) {      
        SlookAirButton airButtonWidget = new SlookAirButton(v, getAdapterStringList(), SlookAirButton.UI_TYPE_LIST);
        airButtonWidget.setItemSelectListener(mCallback);

        airButtonWidget.setPosition(0, 50);

        return airButtonWidget;
    }

    public SlookAirButton createRecipientListWidgetFromView(View v) {
        SlookAirButton airButtonWidget = new SlookAirButton(v, getAdapterRecipientList(),
                SlookAirButton.UI_TYPE_LIST);
        airButtonWidget.setDirection(SlookAirButton.DIRECTION_LOWER);
        airButtonWidget.setItemSelectListener(mCallback);

        return airButtonWidget;
    }

    public SlookAirButton createMenuWidgetFromView(View v) {
        SlookAirButton airButtonWidget = new SlookAirButton(v, getAdapterMenuList(),
                SlookAirButton.UI_TYPE_MENU);
        airButtonWidget.setDirection(SlookAirButton.DIRECTION_RIGHT);
        airButtonWidget.setItemSelectListener(mCallback);

        return airButtonWidget;
    }

    public SlookAirButtonAdapter getAdapterImageList() {
        ArrayList<AirButtonItem> itemList = new ArrayList<AirButtonItem>();
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic01), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic02), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic03), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic04), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic05), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic06), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic07), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic08), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic09), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic10), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic11), null, null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.pic12), null, null));

        return new SlookAirButtonAdapter(itemList);
    }

    public SlookAirButtonAdapter getAdapterStringList() {
        ArrayList<AirButtonItem> stringList = new ArrayList<AirButtonItem>();
        stringList.add(new AirButtonItem(null, "You can come here at 5:00", null));
        stringList.add(new AirButtonItem(null, "Why?", null));
        stringList.add(new AirButtonItem(null, "Please send your e-mail address", null));
        stringList.add(new AirButtonItem(null, "Ok. No problem", null));
        stringList.add(new AirButtonItem(null, "kkkkkkk", null));
        stringList.add(new AirButtonItem(null, "I'm a boy", null));
        stringList.add(new AirButtonItem(null, "You are a girl", null));
        stringList.add(new AirButtonItem(null, "How about this weekend?", null));
        stringList.add(new AirButtonItem(null, "You are so sexy!", null));
        stringList.add(new AirButtonItem(null, "Haha it's really good", null));
        stringList.add(new AirButtonItem(null, "What you really want to?", null));
        stringList.add(new AirButtonItem(null, "I wanna watch movie", null));
        stringList.add(new AirButtonItem(null, "No...", null));
        stringList.add(new AirButtonItem(null, "ASAP", null));
        stringList.add(new AirButtonItem(null, "Really? I can't agree with you", null));

        return new SlookAirButtonAdapter(stringList);
    }

    public SlookAirButtonAdapter getAdapterRecipientList() {
        ArrayList<AirButtonItem> itemList = new ArrayList<AirButtonItem>();
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Alexander Hamilton", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Oliver Wolcott Jr", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Samuel Dexter", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Albert Gallatin", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "George W. Campbell", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Richard Rush", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Richard Rush", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "William J. Duane", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Thomas Ewing", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "George M. Bibb", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "William M. Meredith", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Howell Cobb", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.recipient),
                "Salmon P. Chase", null));

        return new SlookAirButtonAdapter(itemList);
    }

    public SlookAirButtonAdapter getAdapterMenuList() {
        ArrayList<AirButtonItem> itemList = new ArrayList<AirButtonItem>();
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_add), "Add",
                null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_archive),
                "Help", null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_edit), "Edit",
                null));
        itemList.add(new AirButtonItem(getResources().getDrawable(R.drawable.ic_menu_help), "Help",
                null));
        ;

        return new SlookAirButtonAdapter(itemList);
    }
}
