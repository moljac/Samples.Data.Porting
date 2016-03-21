package com.pinterest.android.pinsdk;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.ActionBarActivity;
import android.util.Log;
import android.view.ContextMenu;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuInflater;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.BaseAdapter;
import android.widget.ListView;
import android.widget.TextView;

import com.pinterest.android.pdk.PDKBoard;
import com.pinterest.android.pdk.PDKCallback;
import com.pinterest.android.pdk.PDKClient;
import com.pinterest.android.pdk.PDKException;
import com.pinterest.android.pdk.PDKResponse;


import java.util.ArrayList;
import java.util.List;


public class MyBoardsActivity extends ActionBarActivity {

    private PDKCallback myBoardsCallback;
    private PDKResponse myBoardsResponse;
    private ListView _listView;
    private BoardsAdapter _boardsAdapter;
    private boolean _loading = false;
    private static final String BOARD_FIELDS = "id,name,description,creator,image,counts,created_at";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_my_boards);
        setTitle("My Boards");
        _boardsAdapter = new BoardsAdapter(this);
        _listView = (ListView) findViewById(R.id.listView);

        _listView.setAdapter(_boardsAdapter);
        _listView.setOnCreateContextMenuListener(new View.OnCreateContextMenuListener() {

            @Override
            public void onCreateContextMenu(ContextMenu menu, View v,
                ContextMenu.ContextMenuInfo menuInfo) {
                MenuInflater inflater = getMenuInflater();
                inflater.inflate(R.menu.context_menu_boards, menu);
            }
        });

        myBoardsCallback = new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                _loading = false;
                myBoardsResponse = response;
                _boardsAdapter.setBoardList(response.getBoardList());
            }

            @Override
            public void onFailure(PDKException exception) {
                _loading = false;
                Log.e(getClass().getName(), exception.getDetailMessage());
            }
        };
        _loading = true;
    }

    private void fetchBoards() {
        _boardsAdapter.setBoardList(null);
        PDKClient.getInstance().getMyBoards(BOARD_FIELDS, myBoardsCallback);
    }

    private void loadNext() {
        if (!_loading && myBoardsResponse.hasNext()) {
            _loading = true;
            myBoardsResponse.loadNext(myBoardsCallback);
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        fetchBoards();
    }

    @Override
    public boolean onContextItemSelected(MenuItem item) {
        AdapterView.AdapterContextMenuInfo info = (AdapterView.AdapterContextMenuInfo) item.getMenuInfo();
        switch (item.getItemId()) {
            case R.id.action_board_delete:
                deleteBoard(info.position);
                return true;
            default:
                return super.onContextItemSelected(item);
        }
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu_my_boards, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case R.id.action_new_board:
                createNewBoard();
                return true;
            default:
                return super.onOptionsItemSelected(item);
        }
    }

    private void createNewBoard() {
        Intent i = new Intent(this, CreateBoardActivity.class);
        startActivity(i);
    }

    private void deleteBoard(int position) {
        PDKClient.getInstance().deleteBoard(_boardsAdapter.getBoardList().get(position).getUid(), new PDKCallback() {
            @Override
            public void onSuccess(PDKResponse response) {
                Log.d(getClass().getName(), "Response: " + response.getStatusCode());
                fetchBoards();
            }

            @Override
            public void onFailure(PDKException exception) {
                Log.e(getClass().getName(), "error: " + exception.getDetailMessage());
            }
        });
    }

    private class BoardsAdapter extends BaseAdapter {

        private List<PDKBoard> _boardList;
        private Context _context;
        public BoardsAdapter(Context c) {
            _context = c;
        }

        public void setBoardList(List<PDKBoard> list) {
            if (_boardList == null) _boardList = new ArrayList<PDKBoard>();
            if (list == null) _boardList.clear();
            else _boardList.addAll(list);
            notifyDataSetChanged();
        }

        public List<PDKBoard> getBoardList() {
            return _boardList;
        }
        @Override
        public int getCount() {
            return _boardList == null ? 0 : _boardList.size();
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
        public View getView(int position, View convertView, ViewGroup parent) {
            ViewHolderItem viewHolder;

            //load more pins if about to reach end of list
            if (_boardList.size() - position < 5) {
                loadNext();
            }

            if (convertView == null){
                LayoutInflater inflater = ((Activity) _context).getLayoutInflater();
                convertView = inflater.inflate(android.R.layout.simple_list_item_1, parent, false);

                viewHolder = new ViewHolderItem();
                viewHolder.textViewItem = (TextView) convertView.findViewById(android.R.id.text1);

                convertView.setTag(viewHolder);

            } else {
                viewHolder = (ViewHolderItem) convertView.getTag();
            }

            PDKBoard boardItem = _boardList.get(position);
            if (boardItem != null) {
                viewHolder.textViewItem.setText(boardItem.getName());
            }

            return convertView;
        }

        private class ViewHolderItem {
            TextView textViewItem;
        }
    }
}
