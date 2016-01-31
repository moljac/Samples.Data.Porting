package com.samsung.android.sdk.sample.mediabrowser;

import android.app.Activity;
import android.content.Intent;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ListView;
import android.widget.SearchView;
import android.widget.Toast;
import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.mediacontrol.*;
import com.samsung.android.sdk.sample.mediabrowser.devicepicker.DevicePicker;

import java.util.LinkedHashMap;
import java.util.List;
import java.util.Stack;

public class MediaBrowser extends Activity implements SmcProvider.ResponseListener,
        AdapterView.OnItemClickListener, AdapterView.OnItemLongClickListener {

    public static final int REQUEST_SIZE = 100;

    private DevicePicker mSourceDevicePicker;
    private DevicePicker mPlayerDevicePicker;

    private SmcProvider mProvider;
    private SmcDevice mPlayer;

    private ListView mListView;
    private ItemAdapter mItemAdapter;

    private Stack<SmcItem> mItemStack;
    private SmcItem mCurrentFolder;
    private SmcItem mItemToPlay;

    private Smc mSmcLib;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.main);
        mSourceDevicePicker = (DevicePicker) getFragmentManager().findFragmentById(R.id.sourcePicker);
        mSourceDevicePicker.setDeviceType(SmcDevice.TYPE_PROVIDER);
        mSourceDevicePicker.setDeviceSelectedListener(mSourceDevicePickerListener);

        mPlayerDevicePicker = (DevicePicker) getFragmentManager().findFragmentById(R.id.playerPicker);
        //mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_IMAGEVIEWER);
        mPlayerDevicePicker.setDeviceSelectedListener(mPlayerDevicePickerListener);

        mListView = (ListView) findViewById(R.id.listView);
        mListView.setOnItemClickListener(this);
        mItemAdapter = new ItemAdapter(this);
        mListView.setAdapter(mItemAdapter);
        mListView.setOnItemLongClickListener(this);
        mItemStack = new Stack<SmcItem>();

        mSmcLib = new Smc();
        try {
            mSmcLib.initialize(getBaseContext());
        } catch (SsdkUnsupportedException e) {
            e.printStackTrace();  //TODO Handle exceptions.
        }

    }


    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.menu, menu);
        MenuItem mi = menu.findItem(R.id.search);
        SearchView searchView = (SearchView) mi.getActionView();
        searchView.setIconifiedByDefault(true);
        searchView.setOnQueryTextListener(new SearchView.OnQueryTextListener() {
            @Override
            public boolean onQueryTextSubmit(String query) {
                search(query);
                return true;
            }

            @Override
            public boolean onQueryTextChange(String newText) {
                return false;
            }
        });
        mi.setOnActionExpandListener(new MenuItem.OnActionExpandListener() {
            @Override
            public boolean onMenuItemActionExpand(MenuItem item) {
                Toast.makeText(MediaBrowser.this, "Type * in search box to list all items", Toast.LENGTH_LONG).show();
                return true;
            }

            @Override
            public boolean onMenuItemActionCollapse(MenuItem item) {
                browse(mCurrentFolder);
                return true;
            }
        });
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
    	Intent intent;
    	switch (item.getItemId()) {
			case R.id.upload_audio:
				intent = new Intent(Intent.ACTION_GET_CONTENT).setType("audio/*");
	            startActivityForResult(intent, 0);
	            return true;
			case R.id.upload_image:
				intent = new Intent(Intent.ACTION_PICK).setType("image/*");
	            startActivityForResult(intent, 0);
	            return true;
			case R.id.upload_video:
				intent = new Intent(Intent.ACTION_PICK).setType("video/*");
	            startActivityForResult(intent, 0);
	            return true;
		}
        return super.onOptionsItemSelected(item);
    }

    @Override
    protected void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if(resultCode == RESULT_OK && data!=null){
            data.setClass(this, Uploader.class);
            startActivity(data);
        }
        else if (resultCode == RESULT_CANCELED) {
            Toast.makeText(this, "Upload canceled", Toast.LENGTH_SHORT).show();
        }
    }

    /**
     * This listener gets notified when source device is selected or deselected
     */
    private DevicePicker.DevicePickerResult mSourceDevicePickerListener = new DevicePicker.DevicePickerResult() {
        @Override
        public void onDeviceSelected(SmcDevice device) {
            mProvider = (SmcProvider) device;
            //set listeners and browse the root folder
            mProvider.setResponseListener(MediaBrowser.this);
            mProvider.setResponseListener(MediaBrowser.this);
            browse(mProvider.getRootFolder());
        }

        @Override
        public void onAllShareDisabled() {
            mProvider = null;
            //clear file list
            mItemAdapter.clear();
            findViewById(R.id.no_files).setVisibility(View.VISIBLE);
        }
    };

    /**
     * This listener gets notified when target device is selected or deselected
     */
    private DevicePicker.DevicePickerResult mPlayerDevicePickerListener = new DevicePicker.DevicePickerResult() {
        @Override
        public void onDeviceSelected(SmcDevice device) {
            mPlayer = device;
            //if there was an item requested to play before, then play it
            if (mItemToPlay != null) {
                playItem(mItemToPlay);
                mItemToPlay = null;
            }
        }

        @Override
        public void onAllShareDisabled() {
            if (mPlayer instanceof SmcImageViewer)
                ((SmcImageViewer) mPlayer).hide();
            else if (mPlayer instanceof SmcAvPlayer)
                ((SmcAvPlayer) mPlayer).stop();
            mPlayer = null;
        }
    };

    @Override
    public void onBackPressed() {
        if (!mItemStack.empty()) {
            browse(mItemStack.pop());
            return;
        }
        if(mPlayer instanceof SmcAvPlayer){
            ((SmcAvPlayer) mPlayer).stop();
        }else if(mPlayer instanceof SmcImageViewer){
            ((SmcImageViewer) mPlayer).hide();
        }
        super.onBackPressed();
    }



    @Override
    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        SmcItem item = mItemAdapter.getItem(position);
        if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_FOLDER) {
            mItemStack.push(mCurrentFolder);
            browse(item);
        } else {
            playItem(item);
        }
    }

    /**
     * This method plays an {@link SmcItem}. If current player is not compatible with the type of item, then it will show
     * device picker dialog;
     *
     * @param item an item to play
     */
    private void playItem(SmcItem item) {
        if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_IMAGE) {
            if (mPlayer instanceof SmcImageViewer) {
                ((SmcImageViewer) mPlayer).show(item);
            } else {
                Toast.makeText(this, "Please select ImageViewer", Toast.LENGTH_SHORT).show();
                //mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_IMAGEVIEWER);
                mPlayerDevicePicker.showPickerDialog();
                mItemToPlay = item;
            }
        } else if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_AUDIO || item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_VIDEO) {
            if (mPlayer instanceof SmcAvPlayer) {
                ((SmcAvPlayer) mPlayer).play(item, null);
            } else {
                Toast.makeText(this, "Please select AVPlayer", Toast.LENGTH_SHORT).show();
                //mPlayerDevicePicker.setDeviceType(SmcDevice.TYPE_AVPLAYER);
                mPlayerDevicePicker.showPickerDialog();
                mItemToPlay = item;
            }
        }
    }

    /**
     * Lists contents of a directory.
     *
     * @param item directory to list
     */
    private void browse(SmcItem item) {
        if (mProvider != null) {
            mCurrentFolder = item;
            mItemAdapter.clear();
            mProvider.browse(item, 0, REQUEST_SIZE);
            findViewById(R.id.progress).setVisibility(View.VISIBLE);
            findViewById(R.id.no_files).setVisibility(View.GONE);
        }
    }

    /**
     * Performs search in current provider.
     *
     * @param query
     */
    private void search(String query) {
        if (mProvider != null) {
            if(query.equals("*"))
                query = "";
            mItemAdapter.clear();
            SmcProvider.SearchCriteria sc = new SmcProvider.SearchCriteria(query);
            mProvider.search(sc, 0, REQUEST_SIZE);
            findViewById(R.id.progress).setVisibility(View.VISIBLE);
            findViewById(R.id.no_files).setVisibility(View.GONE);
        }
    }

    /**
     * Called when file listing is completed
     */
    private void onBrowseComplete() {
        findViewById(R.id.progress).setVisibility(View.GONE);
        findViewById(R.id.no_files).setVisibility(mItemAdapter.getCount() == 0 ? View.VISIBLE : View.GONE);
    }


    @Override
    public boolean onItemLongClick(AdapterView<?> parent, View view, int position, long id) {
        SmcItem item = mItemAdapter.getItem(position);
        LinkedHashMap<String, String> props = new LinkedHashMap<String, String>();
        props.put(getString(R.string.title), item.getTitle());
        if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_AUDIO) {
            props.put(getString(R.string.artist), item.getArtist());
            props.put(getString(R.string.album), item.getAlbumTitle());
            props.put(getString(R.string.genre), item.getGenre());
            props.put(getString(R.string.bitrate), item.getBitrate() + " B/s");
            props.put(getString(R.string.duration), item.getDuration() + "s");
        } else if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_IMAGE) {
            props.put(getString(R.string.resolution), item.getResolution());
            if (item.getLocation() != null)
                props.put(getString(R.string.location), item.getLocation().toString());
        } else if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_VIDEO) {
            props.put(getString(R.string.resolution), item.getResolution());
            props.put(getString(R.string.bitrate), item.getBitrate() + " B/s");
            props.put(getString(R.string.duration), item.getDuration() + "s");
        // } else if (item.getMediaType() == SmcItem.MEDIA_TYPE_ITEM_FOLDER) { // Prevent
        }
        props.put(getString(R.string.mime_type), item.getMimeType());
        PropertiesDialog dialog = new PropertiesDialog();
        Bundle b = new Bundle();
        b.putSerializable("props", props);
        dialog.setArguments(b);
        dialog.show(getFragmentManager(), "props");
        return true;
    }

    @Override
    public void onBrowse(SmcProvider smcProvider, List<SmcItem> items, int requestedStartIndex, int requestedCount, SmcItem requestedFolderItem, boolean endOfItems, int error) {
        mItemAdapter.add(items);
        if (endOfItems)
            onBrowseComplete();
        else {
            //if the callback didn't return full list of files, make request for the rest of them
            mProvider.browse(requestedFolderItem, requestedStartIndex + items.size(), REQUEST_SIZE);
        }
    }

    @Override
    public void onSearch(SmcProvider smcProvider, List<SmcItem> items, int requestedStartIndex, int requestedCount, SmcProvider.SearchCriteria searchCriteria, boolean endOfItems, int error) {
        mItemAdapter.add(items);
        if (endOfItems)
            onBrowseComplete();
        else {
            //if the callback didn't return full list of files, make request for the rest of them
            mProvider.search(searchCriteria, requestedStartIndex + items.size(), REQUEST_SIZE);
        }
    }

    @Override
    public void onUpload(SmcProvider smcProvider, SmcItem Item, int error) {
        //To change body of implemented methods use File | Settings | File Templates.
    }

    @Override
    public void onUploadCancel(SmcProvider smcProvider, SmcItem Item, int error) {
        //To change body of implemented methods use File | Settings | File Templates.
    }
}
