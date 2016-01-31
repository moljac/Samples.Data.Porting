
package com.samsung.android.example.slookdemos;

import android.app.Activity;
import android.net.Uri;
import android.os.Bundle;
import android.view.View;
import android.widget.Button;
import android.widget.Toast;

import com.samsung.android.sdk.look.Slook;
import com.samsung.android.sdk.look.airbutton.SlookAirButton;
import com.samsung.android.sdk.look.airbutton.SlookAirButton.ItemSelectListener;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonFrequentContactAdapter;
import com.samsung.android.sdk.look.airbutton.SlookAirButtonRecentMediaAdapter;

public class AirButtonDefaultActivity extends Activity {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_airbutton_default);

        Button btnRecipient = (Button) findViewById(R.id.btn_default_recipient);
        createRecipientListWidgetFromView(btnRecipient);
        Button btnImage = (Button) findViewById(R.id.btn_default_image);
        createImageListWidgetFromView(btnImage);

        Slook slook = new Slook();
        if(!slook.isFeatureEnabled(Slook.AIRBUTTON)) {
            Toast.makeText(this, "This model doesn't support AirButton",
                    Toast.LENGTH_LONG).show();
        } else {
            Toast.makeText(this, "Please hover and push the Spen button on each button",
                Toast.LENGTH_SHORT).show();
        }

    }

    public SlookAirButton createImageListWidgetFromView(View v) {

        SlookAirButton airButtonWidget = new SlookAirButton(v,
                new SlookAirButtonRecentMediaAdapter(v, null), SlookAirButton.UI_TYPE_LIST);
        airButtonWidget.setItemSelectListener(new ItemSelectListener() {

            public void onItemSelected(View arg0, int arg1, Object arg2) {
                if (arg1 < 0) {
                    return;
                }
                Uri uri = (Uri) arg2;
                Toast.makeText(AirButtonDefaultActivity.this, uri.toString(), Toast.LENGTH_SHORT)
                        .show();
            }
        });
        airButtonWidget.setGravity(SlookAirButton.GRAVITY_LEFT);
        airButtonWidget.setDirection(SlookAirButton.DIRECTION_UPPER);
        airButtonWidget.setPosition(0, -50);

        return airButtonWidget;
    }

    public SlookAirButton createRecipientListWidgetFromView(View v) {
        Bundle option = new Bundle();
        option.putString("MIME_TYPE", "vnd.android.cursor.item/phone_v2");
        SlookAirButton airButtonWidget = new SlookAirButton(v,
                new SlookAirButtonFrequentContactAdapter(v, null), SlookAirButton.UI_TYPE_LIST);
        // airButtonWidget.steDir
        airButtonWidget.setDirection(SlookAirButton.DIRECTION_UPPER);
        airButtonWidget.setItemSelectListener(new ItemSelectListener() {

            public void onItemSelected(View arg0, int arg1, Object arg2) {
                Bundle bundle = (Bundle) arg2;
				if (bundle != null) {
					String name = bundle
							.getString(SlookAirButtonFrequentContactAdapter.DISPLAY_NAME);
					String data = bundle
							.getString(SlookAirButtonFrequentContactAdapter.DATA);

					Toast.makeText(AirButtonDefaultActivity.this,
							name + ":" + data, Toast.LENGTH_SHORT).show();
				}
            }
        });

        return airButtonWidget;
    }
}
