/*    
 * Copyright (c) 2015 Samsung Electronics Co., Ltd. All rights reserved. 
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that 
 * the following conditions are met:
 * 
 *     * Redistributions of source code must retain the above copyright notice, 
 *       this list of conditions and the following disclaimer. 
 *     * Redistributions in binary form must reproduce the above copyright notice, 
 *       this list of conditions and the following disclaimer in the documentation and/or 
 *       other materials provided with the distribution. 
 *     * Neither the name of Samsung Electronics Co., Ltd. nor the names of its contributors may be used to endorse
 *       or promote products derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A
 * PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION)
 * HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE
 * POSSIBILITY OF SUCH DAMAGE.
 */

package com.samsung.android.sdk.accessory.example.filetransfer.sender;

import java.io.File;
import java.util.ArrayList;
import java.util.List;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.media.MediaRecorder;
import android.os.Bundle;
import android.os.Environment;
import android.os.IBinder;
import android.util.Log;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.widget.Button;
import android.widget.ProgressBar;
import android.widget.Toast;

import com.samsung.android.sdk.accessory.example.filetransfer.sender.FileTransferSender.*;

public class FileTransferSenderActivity extends Activity implements OnClickListener {
    private static final String TAG = "FileTransferSenderActivity";
    private static final String SRC_PATH =
                Environment.getExternalStorageDirectory().getAbsolutePath() + "/src.aaa";
    private Button mBtnSend;
    private Button mBtnConn;
    private Button mBtnCancel;
    private Button mBtnCancelAll;
    private ProgressBar mSentProgressBar;
    private Context mCtxt;
    private String mDirPath;
    private long currentTransId;
    private long mFileSize;
    private List<Long> mTransactions = new ArrayList<Long>();
    private FileTransferSender mSenderService;
    private ServiceConnection mServiceConnection = new ServiceConnection() {
        @Override
        public void onServiceDisconnected(ComponentName name) {
            Log.i(TAG, "Service disconnected");
            mSenderService = null;
        }

        @Override
        public void onServiceConnected(ComponentName arg0, IBinder binder) {
            Log.d(TAG, "Service connected");
            mSenderService = ((SenderBinder) binder).getService();
            mSenderService.registerFileAction(getFileAction());
        }
    };

    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.ft_sender_activity);
        mCtxt = getApplicationContext();
        mBtnSend = (Button) findViewById(R.id.Send);
        mBtnSend.setOnClickListener(this);
        mBtnConn = (Button) findViewById(R.id.connectButton);
        mBtnConn.setOnClickListener(this);
        mBtnCancel = (Button) findViewById(R.id.cancel);
        mBtnCancel.setOnClickListener(this);
        mBtnCancelAll = (Button) findViewById(R.id.cancelAll);
        mBtnCancelAll.setOnClickListener(this);
        if (!Environment.getExternalStorageState().equals(Environment.MEDIA_MOUNTED)) {
            Toast.makeText(mCtxt, " No SDCARD Present", Toast.LENGTH_SHORT).show();
            finish();
        } else {
            mDirPath = Environment.getExternalStorageDirectory() + File.separator + "FileTransferSender";
            File file = new File(mDirPath);
            if (file.mkdirs()) {
                Toast.makeText(mCtxt, " Stored in " + mDirPath, Toast.LENGTH_LONG).show();
            }
        }
        mCtxt.bindService(new Intent(getApplicationContext(), FileTransferSender.class),
                    this.mServiceConnection, Context.BIND_AUTO_CREATE);
        mSentProgressBar = (ProgressBar) findViewById(R.id.fileTransferProgressBar);
        mSentProgressBar.setMax(100);
    }

    public void onDestroy() {
        getApplicationContext().unbindService(mServiceConnection);
        super.onDestroy();
    }

    public void onError(MediaRecorder mr, int what, int extra) {
        Toast.makeText(mCtxt, " MAX SERVER DIED ", Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onBackPressed() {
        moveTaskToBack(true);
    }

    // for Android before 2.0, just in case
    @Override
    public boolean onKeyDown(int keyCode, KeyEvent event) {
        if (keyCode == KeyEvent.KEYCODE_BACK) {
            moveTaskToBack(true);
            return true;
        }
        return super.onKeyDown(keyCode, event);
    }

    public void onClick(View v) {
        if (v.equals(mBtnSend)) {
            File file = new File(SRC_PATH);
            mFileSize = file.length();
            Toast.makeText(mCtxt, SRC_PATH + " selected " + " size " + mFileSize + " bytes", Toast.LENGTH_SHORT).show();
            if (isSenderServiceBound()) {
                try {
                    int trId = mSenderService.sendFile(SRC_PATH);
                    mTransactions.add((long) trId);
                    currentTransId = trId;
                } catch (IllegalArgumentException e) {
                    e.printStackTrace();
                    Toast.makeText(mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
                }
            }
        } else if (v.equals(mBtnCancel)) {
            if (mSenderService != null) {
                try {
                    mSenderService.cancelFileTransfer((int) currentTransId);
                    mTransactions.remove(currentTransId);
                } catch (IllegalArgumentException e) {
                    e.printStackTrace();
                    Toast.makeText(mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
                }
            } else {
                Toast.makeText(mCtxt, "no binding to service", Toast.LENGTH_SHORT).show();
            }
        } else if (v.equals(mBtnCancelAll)) {
            if (mSenderService != null) {
                mSenderService.cancelAllTransactions();
                mTransactions.clear();
            } else {
                Toast.makeText(mCtxt, "no binding to service", Toast.LENGTH_SHORT).show();
            }
        } else if (v.equals(mBtnConn)) {
            if (mSenderService != null) {
                mSenderService.connect();
            } else {
                Toast.makeText(getApplicationContext(), "Service not Bound", Toast.LENGTH_SHORT).show();
            }
        }
    }

    private FileAction getFileAction() {
        return new FileAction() {
            @Override
            public void onFileActionError() {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        mSentProgressBar.setProgress(0);
                        mTransactions.remove(currentTransId);
                        Toast.makeText(mCtxt, "Error", Toast.LENGTH_SHORT).show();
                    }
                });
            }

            @Override
            public void onFileActionProgress(final long progress) {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        mSentProgressBar.setProgress((int) progress);
                    }
                });
            }

            @Override
            public void onFileActionTransferComplete() {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        mSentProgressBar.setProgress(0);
                        mTransactions.remove(currentTransId);
                        Toast.makeText(mCtxt, "Transfer Completed!", Toast.LENGTH_SHORT).show();
                    }
                });
            }

            @Override
            public void onFileActionCancelAllComplete() {
                runOnUiThread(new Runnable() {
                    @Override
                    public void run() {
                        mSentProgressBar.setProgress(0);
                        mTransactions.remove(currentTransId);
                    }
                });
            }
        };
    }

    private boolean isSenderServiceBound() {
        return this.mSenderService != null;
    }
}
