package com.tokbox.android.opentokrtc;

import android.app.Activity;
import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.util.Log;
import android.view.View;
import android.view.WindowManager;
import android.widget.EditText;

public class HomeActivity extends Activity {

    private static final String LOGTAG = "opentokrtc";

    private static final String LAST_CONFERENCE_DATA = "LAST_CONFERENCE_DATA";

    private String roomName;
    private String username;
    private EditText roomNameInput;
    private EditText usernameInput;

    /** Called when the activity is first created. */
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        this.getWindow().setFlags(WindowManager.LayoutParams.FLAG_FULLSCREEN,
                WindowManager.LayoutParams.FLAG_FULLSCREEN);

        //restore last used conference data
        restoreConferenceData();

        setContentView(R.layout.main_layout);

        roomNameInput = (EditText) findViewById(R.id.input_room_name);
        roomNameInput.setText(this.roomName);

        usernameInput = (EditText) findViewById(R.id.input_username);
        usernameInput.setText(this.username);

    }

    public void joinRoom(View v) {
        Log.i(LOGTAG, "join room button clicked.");

        roomName = roomNameInput.getText().toString();
        username = usernameInput.getText().toString();

        Intent enterChatRoomIntent = new Intent(this, ChatRoomActivity.class);
        enterChatRoomIntent.putExtra(ChatRoomActivity.ARG_ROOM_ID, roomName);
        enterChatRoomIntent.putExtra(ChatRoomActivity.ARG_USERNAME_ID, username);

        //save room name and username
        saveConferenceData();

        startActivity(enterChatRoomIntent);
    }

    private void saveConferenceData() {

        SharedPreferences settings = getApplicationContext()
                .getSharedPreferences(LAST_CONFERENCE_DATA, 0);
        SharedPreferences.Editor editor = settings.edit();
        editor.putString("roomName", roomName);
        editor.putString("username", username);

        editor.apply();
    }

    private void restoreConferenceData() {
        SharedPreferences settings = getApplicationContext()
                .getSharedPreferences(LAST_CONFERENCE_DATA, 0);
        roomName = settings.getString("roomName", "");
        username = settings.getString("username", "");
    }
}
