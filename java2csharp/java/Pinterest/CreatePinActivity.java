package com.pinterest.android.pinsdk;

import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
import android.widget.TextView;
import android.widget.Toast;

import com.pinterest.android.pdk.PDKCallback;
import com.pinterest.android.pdk.PDKClient;
import com.pinterest.android.pdk.PDKException;
import com.pinterest.android.pdk.PDKResponse;
import com.pinterest.android.pdk.Utils;


public class CreatePinActivity extends ActionBarActivity {

    EditText imageUrl, link, boardId, note;
    Button saveButton, selectImagebutton;
    TextView responseView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_create_pin);
        setTitle("New Pin");
        imageUrl = (EditText) findViewById(R.id.pin_create_url);
        link = (EditText) findViewById(R.id.pin_create_link);
        note = (EditText) findViewById(R.id.pin_create_note);
        responseView = (TextView) findViewById(R.id.pin_response_view);
        boardId = (EditText) findViewById(R.id.create_pin_board_id);
        saveButton = (Button) findViewById(R.id.save_button);
        saveButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onSavePin();
            }
        });
    }

    private void onSavePin() {
        String pinImageUrl = imageUrl.getText().toString();
        String board = boardId.getText().toString();
        String noteText = note.getText().toString();
        if (!Utils.isEmpty(noteText) &&!Utils.isEmpty(board) && !Utils.isEmpty(pinImageUrl)) {
            PDKClient
                .getInstance().createPin(noteText, board, pinImageUrl, link.getText().toString(), new PDKCallback() {
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
        } else {
            Toast.makeText(this, "Required fields cannot be empty", Toast.LENGTH_SHORT).show();
        }
    }
}
