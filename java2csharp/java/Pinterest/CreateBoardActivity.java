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

public class CreateBoardActivity extends ActionBarActivity {

    EditText boardName, boardDesc;
    Button saveButton;
    TextView responseView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_create_board);
        setTitle("New Board");
        boardName = (EditText) findViewById(R.id.board_create_name);
        boardDesc = (EditText) findViewById(R.id.board_create_desc);
        responseView = (TextView) findViewById(R.id.board_response_view);
        saveButton = (Button) findViewById(R.id.save_button);
        saveButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onSaveBoard();
            }
        });

    }

    private void onSaveBoard() {
        String bName = boardName.getText().toString();
        if (!Utils.isEmpty(bName)) {
            PDKClient.getInstance().createBoard(bName, boardDesc.getText().toString(), new PDKCallback() {
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
            Toast.makeText(this, "Board name cannot be empty", Toast.LENGTH_SHORT).show();
        }
    }
}
