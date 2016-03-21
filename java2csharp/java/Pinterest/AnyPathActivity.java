package com.pinterest.android.pinsdk;

import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.text.method.ScrollingMovementMethod;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

import com.pinterest.android.pdk.PDKCallback;
import com.pinterest.android.pdk.PDKClient;
import com.pinterest.android.pdk.PDKException;
import com.pinterest.android.pdk.PDKResponse;
import com.pinterest.android.pdk.Utils;


import java.util.HashMap;


public class AnyPathActivity extends ActionBarActivity implements
    AdapterView.OnItemSelectedListener {

    private EditText pathText, fieldsText;
    private Spinner spinner;
    private Button getButton;
    private TextView responseView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_any_path);
        setTitle("Get Path..");

        pathText = (EditText) findViewById(R.id.path_edittext);
        fieldsText = (EditText) findViewById(R.id.fields_edittext);
        responseView = (TextView) findViewById(R.id.path_response_view);
        responseView.setMovementMethod(new ScrollingMovementMethod());
        getButton = (Button) findViewById(R.id.get_button);
        getButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onGet();
            }
        });

        spinner = (Spinner) findViewById(R.id.spinner);
        spinner.setOnItemSelectedListener(this);
        ArrayAdapter<CharSequence> adapter = ArrayAdapter.createFromResource(this,
            R.array.paths_array, android.R.layout.simple_spinner_item);
        adapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        spinner.setAdapter(adapter);
    }

    public void onItemSelected(AdapterView<?> parent, View view,
        int pos, long id) {
        switch (pos) {
            case 0:
                pathText.setText("pins/158400111869919209/");
                break;
            case 1:
                pathText.setText("boards/158400180583006164/");
                break;
            case 2:
                pathText.setText("me/likes/");
                break;
            case 3:
                pathText.setText("me/following/interests/");
                break;
            case 4:
                pathText.setText("boards/158400180583006164/pins/");
                break;
            case 5:
                pathText.setText("users/8en/");
                break;
        }
    }

    public void onNothingSelected(AdapterView<?> parent) {
    }

    private void onGet() {
        String path = pathText.getText().toString();
        String fields = fieldsText.getText().toString();
        HashMap params = new HashMap<String, String>();
        params.put(PDKClient.PDK_QUERY_PARAM_FIELDS, fields);
        if (!Utils.isEmpty(path)) {
            PDKClient
                .getInstance().getPath(path, params, new PDKCallback() {
                @Override
                public void onSuccess(PDKResponse response) {
                    Log.d(getClass().getName(), response.getData().toString());
                    responseView.setText(response.getData().toString());
                }

                @Override
                public void onFailure(PDKException exception) {
                    Log.e(getClass().getName(), exception.getDetailMessage());
                    responseView.setText(exception.getDetailMessage());
                }
            });
        }
    }
}
