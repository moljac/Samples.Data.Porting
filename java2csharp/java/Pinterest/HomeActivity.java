package com.pinterest.android.pinsdk;

import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import com.pinterest.android.pdk.PDKCallback;
import com.pinterest.android.pdk.PDKClient;
import com.pinterest.android.pdk.PDKException;
import com.pinterest.android.pdk.PDKResponse;
import com.pinterest.android.pdk.PDKUser;
import com.pinterest.android.pdk.Utils;
import com.squareup.picasso.Picasso;


public class HomeActivity extends ActionBarActivity {

    private static boolean DEBUG = true;
    private Button pinsButton, boardsButton, followingButton, logoutButton, pathButton;
    private TextView nameTv;
    private ImageView profileIv;
    private final String USER_FIELDS = "id,image,counts,created_at,first_name,last_name,bio";
    PDKUser user;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_home);
        setTitle("Pinterest SDK Demo");

        nameTv = (TextView) findViewById(R.id.name_textview);
        profileIv = (ImageView) findViewById(R.id.profile_imageview);

        pinsButton = (Button) findViewById(R.id.pins_button);
        pinsButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onMyPins();
            }
        });

        boardsButton = (Button) findViewById(R.id.boards_button);
        boardsButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onMyBoards();
            }
        });

        followingButton = (Button) findViewById(R.id.following_button);
        followingButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onMyFollowing();
            }
        });

        pathButton = (Button) findViewById(R.id.path_button);
        pathButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onGetPath();
            }
        });

        logoutButton = (Button) findViewById(R.id.logout_button);
        logoutButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onLogout();
            }
        });

        getMe();
    }

    private void setUser() {
        nameTv.setText(user.getFirstName() + " " + user.getLastName());
        Picasso.with(this).load(user.getImageUrl()).into(profileIv);
    }

    private void  getMe() {
        PDKClient.getInstance().getMe(USER_FIELDS, new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                if (DEBUG) log(String.format("status: %d", response.getStatusCode()));
                user = response.getUser();
                setUser();
            }
            @Override
            public void onFailure(PDKException exception) {
                if (DEBUG)  log(exception.getDetailMessage());
                Toast.makeText(HomeActivity.this, "/me Request failed", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private void  onMyPins() {
        Intent i = new Intent(this, MyPinsActivity.class);
        startActivity(i);
    }

    private void  onMyBoards() {
        Intent i = new Intent(this, MyBoardsActivity.class);
        startActivity(i);
    }

    private void  onGetPath() {
        Intent i = new Intent(this, AnyPathActivity.class);
        startActivity(i);
    }

    private void  onMyFollowing() {
        Intent i = new Intent(this, FollowingActivity.class);
        startActivity(i);
    }

    private void  onLogout() {
        PDKClient.getInstance().logout();
        Intent i = new Intent(this, MainActivity.class);
        startActivity(i);
        finish();
    }

    private void showDialog(String raw) {
        RawResponseDialogFragment frag = RawResponseDialogFragment.newInstance(raw);
        frag.show(getSupportFragmentManager(), "RawResponseDialogFragment");
    }

    private void log(String msg) {
        if (!Utils.isEmpty(msg))
            Log.d(getClass().getName(), msg);
    }
}
