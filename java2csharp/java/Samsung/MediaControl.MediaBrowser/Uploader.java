package com.samsung.android.sdk.sample.mediabrowser;

import android.app.Activity;
import android.database.Cursor;
import android.os.Bundle;
import android.provider.MediaStore;
import android.view.View;
import android.webkit.MimeTypeMap;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import com.samsung.android.sdk.mediacontrol.Smc;
import com.samsung.android.sdk.mediacontrol.SmcDevice;
import com.samsung.android.sdk.mediacontrol.SmcItem;
import com.samsung.android.sdk.mediacontrol.SmcProvider;
import com.samsung.android.sdk.sample.mediabrowser.R;
import com.samsung.android.sdk.sample.mediabrowser.devicepicker.DevicePicker;

import java.util.List;

public class Uploader extends Activity implements DevicePicker.DevicePickerResult, SmcProvider.EventListener, SmcProvider.ResponseListener {
    DevicePicker mTargetDevicePicker;
    Button mUploadButton;
    private SmcProvider provider;
    private boolean uploadInProgress;
    private SmcItem itemToUpload;
    private ProgressBar progressBar;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.uploader);

        mTargetDevicePicker = (DevicePicker) getFragmentManager().findFragmentById(R.id.targetPicker);
        mTargetDevicePicker.setDeviceType(SmcDevice.TYPE_PROVIDER);
        mTargetDevicePicker.setDeviceSelectedListener(this);
        mUploadButton = (Button) findViewById(R.id.button);
        mUploadButton.setOnClickListener(uploadClickListener);
        progressBar = (ProgressBar) findViewById(R.id.progress);


        SmcItem.LocalContent content = getLocalContent();

        if (content == null) {
            Toast.makeText(this, "Content not supported", Toast.LENGTH_SHORT).show();
            this.finish();
        }
        else {
            itemToUpload = new SmcItem(content);
            ((TextView)findViewById(R.id.header)).setText("File: "+itemToUpload.getUri().toString());
        }
    }

    private SmcItem.LocalContent getLocalContent(){
        SmcItem.LocalContent content = null;
        if (getIntent().getData() != null) {
	        if(getIntent().getData().getScheme().equals("content")){
	            Cursor c = getContentResolver().query(getIntent().getData(), new String[]{MediaStore.MediaColumns.DATA, MediaStore.MediaColumns.TITLE, MediaStore.MediaColumns.MIME_TYPE}, null, null, null);
	            if (c != null) {
                    if(c.moveToFirst() && c.getString(2) != null)
                        content = new SmcItem.LocalContent(c.getString(0), c.getString(2)).setTitle(c.getString(1));
                    c.close();
	            }
	        }else{
	            String path = getIntent().getDataString();
	            if(path!=null){
	                String name = path.substring(path.lastIndexOf('/')+1);
	                String extension = name.substring(name.lastIndexOf('.')+1);
	                content = new SmcItem.LocalContent(path, MimeTypeMap.getSingleton().getMimeTypeFromExtension(extension)).setTitle(name);
	            }
	        }
        }
        return content;
    }




    @Override
    public void onDeviceSelected(SmcDevice device) {
        mUploadButton.setEnabled(true);
        provider = (SmcProvider) device;
        provider.setEventListener(this);
        provider.setResponseListener(this);
        mUploadButton.setEnabled(provider.isUploadable());
        if(!provider.isUploadable()){
            Toast.makeText(this, "This provider doesn't support file upload", Toast.LENGTH_SHORT).show();
        }
    }

    @Override
    public void onAllShareDisabled() {
        mUploadButton.setEnabled(false);
        cancelUpload();
        provider.setResponseListener(null);
        provider.setEventListener(null);
        provider = null;
    }


    private View.OnClickListener uploadClickListener = new View.OnClickListener() {
        @Override
        public void onClick(View v) {
            if(itemToUpload==null){
                Toast.makeText(Uploader.this, "Content not supported", Toast.LENGTH_SHORT).show();
            }
            if(uploadInProgress){
                cancelUpload();
            }else{
                startUpload();
            }
        }
    };

    private void startUpload(){
        if(provider!=null){
            provider.upload(itemToUpload);
            uploadInProgress = true;
            mUploadButton.setText("Cancel");
        }else{
            Toast.makeText(this, "Please select target device", Toast.LENGTH_SHORT).show();
        }
    }

    private void cancelUpload(){
        if(provider!=null && uploadInProgress){
            provider.uploadCancel(itemToUpload);
            uploadInProgress = false;
            progressBar.setProgress(0);
            mUploadButton.setText(R.string.upload);
        }else{
            Toast.makeText(this, "Please select target device", Toast.LENGTH_SHORT).show();
        }
    }

    @Override
    public void onContentUpdated(SmcProvider smcProvider, int error) {

    }

    @Override
    public void onUploadProgressUpdated(SmcProvider smcProvider, long receivedSize, long totalSize, SmcItem smcItem, int error) {
        progressBar.setMax((int) totalSize);
        progressBar.setProgress((int) receivedSize);
    }

    @Override
    public void onUploadCompleted(SmcProvider smcProvider, SmcItem smcItem) {
        mUploadButton.setText(R.string.upload);
        progressBar.setProgress(0);
        uploadInProgress = false;
        Toast.makeText(this, "File uploaded successfully", Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onBrowse(SmcProvider smcProvider, List<SmcItem> smcItems, int requestedStartIndex, int requestedCount, SmcItem requestedFolder, boolean endOfItems, int error) {

    }

    @Override
    public void onSearch(SmcProvider smcProvider, List<SmcItem> smcItems, int requestedStartIndex, int requestedCount, SmcProvider.SearchCriteria searchCriteria, boolean endOfItems, int error) {

    }

    @Override
    public void onUpload(SmcProvider smcProvider, SmcItem smcItem, int error) {
        if(error != Smc.SUCCESS)
            Toast.makeText(this, "Upload error: "+error, Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onUploadCancel(SmcProvider smcProvider, SmcItem smcItem, int error) {
        Toast.makeText(this, "Upload canceled", Toast.LENGTH_SHORT).show();
    }
}
