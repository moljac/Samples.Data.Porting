package com.pinterest.android.pinsdk;

import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.View;
import android.widget.Button;
import android.widget.EditText;
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


import java.util.HashMap;


public class SearchUserActivity extends ActionBarActivity {

    EditText searchBox;
    Button searchButton, followButton;
    ImageView userImageView;
    TextView userName;
    PDKUser user;
    private final String USER_FIELDS = "id,image,first_name,last_name";
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_search_user);

        searchBox = (EditText) findViewById(R.id.search_user);
        searchButton = (Button) findViewById(R.id.search_button);
        searchButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onSearch();
            }
        });
        userName = (TextView) findViewById(R.id.user_name);
        userImageView = (ImageView) findViewById(R.id.user_imageview);

        followButton = (Button) findViewById(R.id.follow_button);
        followButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                onFollow();
            }
        });
        followButton.setVisibility(View.GONE);
        userImageView.setVisibility(View.GONE);
        userName.setVisibility(View.GONE);
    }

    private void onSearch() {
        if (!Utils.isEmpty(searchBox.getText().toString())) {
            PDKClient.getInstance().getUser(searchBox.getText().toString(), USER_FIELDS, new PDKCallback() {
                    @Override
                    public void onSuccess(PDKResponse response) {
                        Log.d(getClass().getName(), "Response: " + response.getStatusCode());
                        user = response.getUser();
                        showUser();
                    }

                    @Override
                    public void onFailure(PDKException exception) {
                        Log.e(getClass().getName(), "error: " + exception.getDetailMessage());
                    }
                }
            );
        }
    }

    private void onFollow() {
        String path = "me/following/users/";
        HashMap<String, String> param = new HashMap<String, String>();
        param.put("user", user.getUid());
        PDKClient.getInstance().postPath(path, param, new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                Log.d(getClass().getName(), "Response: " + response.getData().toString());
                Toast.makeText(SearchUserActivity.this, "User Follow success", Toast.LENGTH_SHORT)
                    .show();
            }

            @Override
            public void onFailure(PDKException exception) {
                Log.e(getClass().getName(), "error: " + exception.getDetailMessage());
                Toast.makeText(SearchUserActivity.this, "User Follow failed", Toast.LENGTH_SHORT)
                    .show();
            }
        });
    }

    private void showUser() {
        if (user != null) {
            followButton.setVisibility(View.VISIBLE);
            userImageView.setVisibility(View.VISIBLE);
            userName.setVisibility(View.VISIBLE);

            userName.setText(user.getFirstName() + " " + user.getLastName() );
            Picasso.with(this).load(user.getImageUrl()).into(userImageView);
        }
    }
}
