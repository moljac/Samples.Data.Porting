package com.pinterest.android.pinsdk;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.pinterest.android.pdk.PDKCallback;
import com.pinterest.android.pdk.PDKClient;
import com.pinterest.android.pdk.PDKException;
import com.pinterest.android.pdk.PDKResponse;
import com.pinterest.android.pdk.PDKUser;


import java.util.ArrayList;
import java.util.List;


public class FollowingActivity extends ActionBarActivity {

    private PDKCallback myFollowingCallback;
    private PDKResponse myFollowingResponse;
    private ListView _listView;
    private FollowingAdapter _followingAdapter;
    private boolean _loading = false;
    private final String USER_FIELDS = "id,image,counts,created_at,first_name,last_name,bio";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_following);
        setTitle("My Following");
        _followingAdapter = new FollowingAdapter(this);
        _listView = (ListView) findViewById(R.id.listView);

        _listView.setAdapter(_followingAdapter);

        myFollowingCallback = new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                _loading = false;
                myFollowingResponse = response;
                _followingAdapter.setFollowingList(response.getUserList());
            }

            @Override
            public void onFailure(PDKException exception) {
                _loading = false;
                Log.e(getClass().getName(), exception.getDetailMessage());
            }
        };
        _loading = true;
    }

    private void fetchFollowers() {
        _followingAdapter.setFollowingList(null);
        PDKClient.getInstance().getPath("me/following/users/", myFollowingCallback);
    }

    private void loadNext() {
        if (!_loading && myFollowingResponse.hasNext()) {
            _loading = true;
            myFollowingResponse.loadNext(myFollowingCallback);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        fetchFollowers();
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_following, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_search_user:
                SearchUser();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    private void SearchUser() {
        Intent i = new Intent(this, SearchUserActivity.class);
        startActivity(i);
    }

    private void onUnfollowUser(int position) {
        String userId = _followingAdapter.getFollowingList().get(position).getUid();
        String path = "me/following/users/" + userId + "/";
        PDKClient.getInstance().deletePath(path, new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                Log.d(getClass().getName(), "Response: " + response.getData().toString());
                Toast.makeText(FollowingActivity.this, "User Unfollow success", Toast.LENGTH_SHORT).show();
            }

            @Override
            public void onFailure(PDKException exception) {
                Log.e(getClass().getName(), "error: " + exception.getDetailMessage());
                Toast.makeText(FollowingActivity.this, "User Unfollow failed", Toast.LENGTH_SHORT).show();
            }
        });
    }

    private class FollowingAdapter extends BaseAdapter {

        private List<PDKUser> _followingList;
        private Context _context;
        public FollowingAdapter(Context c) {
            _context = c;
        }

        public void setFollowingList(List<PDKUser> list) {
            if (_followingList == null) _followingList = new ArrayList<PDKUser>();
            if (list == null) _followingList.clear();
            else _followingList.addAll(list);
            notifyDataSetChanged();
        }

        public List<PDKUser> getFollowingList() {
            return _followingList;
        }

        @Override
        public int getCount() {
            return _followingList == null ? 0 : _followingList.size();
        }

        @Override
        public Object getItem(int position) {
            return position;
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        @Override
        public View getView(final int position, View convertView, ViewGroup parent) {
            ViewHolderItem viewHolder;

            //load more pins if about to reach end of list
            if (_followingList.size() - position < 5) {
                loadNext();
            }

            if (convertView == null){
                LayoutInflater inflater = ((Activity) _context).getLayoutInflater();
                convertView = inflater.inflate(R.layout.list_item_following, parent, false);

                viewHolder = new ViewHolderItem();
                viewHolder.textView = (TextView) convertView.findViewById(R.id.text_view);
                viewHolder.unfollowButton = (Button) convertView.findViewById(R.id.unfollow_button);

                convertView.setTag(viewHolder);

            } else {
                viewHolder = (ViewHolderItem) convertView.getTag();
            }

            PDKUser user = _followingList.get(position);
            if (user != null) {
                viewHolder.textView.setText(user.getFirstName());
                viewHolder.unfollowButton.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        onUnfollowUser(position);
                    }
                });
            }

            return convertView;
        }

        private class ViewHolderItem {
            TextView textView;
            Button unfollowButton;
        }
    }

}
