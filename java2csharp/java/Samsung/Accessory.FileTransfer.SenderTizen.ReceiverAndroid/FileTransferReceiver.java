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

package com.samsung.android.sdk.accessory.example.filetransfer.receiver;

import java.io.UnsupportedEncodingException;

import android.content.Context;
import android.content.Intent;
import android.os.Binder;
import android.os.IBinder;
import android.util.Log;
import android.widget.Toast;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.accessory.*;
import com.samsung.android.sdk.accessoryfiletransfer.*;
import com.samsung.android.sdk.accessoryfiletransfer.SAFileTransfer.*;

public class FileTransferReceiver extends SAAgent {
    private static final String TAG = "FileTransferReceiver";
    private Context mContext;
    private final IBinder mReceiverBinder = new ReceiverBinder();
    private static final Class<ServiceConnection> SASOCKET_CLASS = ServiceConnection.class;
    private ServiceConnection mConnection = null;
    private SAFileTransfer mSAFileTransfer = null;
    private EventListener mCallback;
    private FileAction mFileAction = null;

    public FileTransferReceiver() {
        super(TAG, SASOCKET_CLASS);
    }

    @Override
    public void onCreate() {
        super.onCreate();
        mContext = getApplicationContext();
        Log.d(TAG, "On Create of Sample FileTransferReceiver Service");
        mCallback = new EventListener() {
            @Override
            public void onProgressChanged(int transId, int progress) {
                Log.d(TAG, "onProgressChanged : " + progress + " for transaction : " + transId);
                if (mFileAction != null) {
                    mFileAction.onFileActionProgress(progress);
                }
            }

            @Override
            public void onTransferCompleted(int transId, String fileName, int errorCode) {
                Log.d(TAG, "onTransferCompleted: tr id : " + transId + " file name : " + fileName + " error : "
                            + errorCode);
                if (errorCode == SAFileTransfer.ERROR_NONE) {
                    mFileAction.onFileActionTransferComplete();
                } else {
                    mFileAction.onFileActionError();
                }
            }

            @Override
            public void onTransferRequested(int id, String fileName) {
                Log.d(TAG, "onTransferRequested: id- " + id + " file name: " + fileName);
                if (FileTransferReceiverActivity.isUp()) {
                    Log.d(TAG, "Activity is up");
                    mFileAction.onFileActionTransferRequested(id, fileName);
                } else {
                    Log.d(TAG, "Activity is not up, invoke activity");
                    mContext.startActivity(new Intent()
                                .setClass(mContext, FileTransferReceiverActivity.class)
                                .setFlags(Intent.FLAG_ACTIVITY_NEW_TASK)
                                .setAction("incomingFT").putExtra("tx", id)
                                .putExtra("fileName", fileName));
                    int counter = 0;
                    while (counter < 10) {
                        counter++;
                        try {
                            Thread.sleep(500);
                        } catch (InterruptedException e) {
                            e.printStackTrace();
                        }
                        if (mFileAction != null) {
                            mFileAction.onFileActionTransferRequested(id, fileName);
                            break;
                        }
                    }
                }
            }

            @Override
            public void onCancelAllCompleted(int errorCode) {
                mFileAction.onFileActionError();
                Log.e(TAG, "onCancelAllCompleted: Error Code " + errorCode);
            }
        };
        SAft saft = new SAft();
        try {
            saft.initialize(this);
        } catch (SsdkUnsupportedException e) {
            if (e.getType() == SsdkUnsupportedException.DEVICE_NOT_SUPPORTED) {
                Toast.makeText(getBaseContext(), "Cannot initialize, DEVICE_NOT_SUPPORTED", Toast.LENGTH_SHORT).show();
            } else if (e.getType() == SsdkUnsupportedException.LIBRARY_NOT_INSTALLED) {
                Toast.makeText(getBaseContext(), "Cannot initialize, LIBRARY_NOT_INSTALLED.", Toast.LENGTH_SHORT).show();
            } else {
                Toast.makeText(getBaseContext(), "Cannot initialize, UNKNOWN.", Toast.LENGTH_SHORT).show();
            }
            e.printStackTrace();
            return;
        } catch (Exception e1) {
            Toast.makeText(getBaseContext(), "Cannot initialize, SAft.", Toast.LENGTH_SHORT).show();
            e1.printStackTrace();
            return;
        }
        mSAFileTransfer = new SAFileTransfer(FileTransferReceiver.this, mCallback);
    }

    @Override
    public IBinder onBind(Intent arg0) {
        return mReceiverBinder;
    }

    @Override
    public void onDestroy() {
        mSAFileTransfer.close();
        mSAFileTransfer = null;
        super.onDestroy();
        Log.i(TAG, "FileTransferReceiver Service is Stopped.");
    }

    @Override
    protected void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result) {
        if (mConnection == null) {
            Log.d(TAG, "onFindPeerAgentResponse : mConnection is null");
        }
    }

    @Override
    protected void onServiceConnectionResponse(SAPeerAgent peer, SASocket socket, int result) {
        Log.i(TAG, "onServiceConnectionResponse: result - " + result);
        if (result == SAAgent.CONNECTION_SUCCESS) {
            if (socket != null) {
                mConnection = (ServiceConnection) socket;
                Toast.makeText(getBaseContext(), "Connection established for FT", Toast.LENGTH_SHORT).show();
            }
        }
    }

    public void receiveFile(int transId, String path, boolean bAccept) {
        Log.d(TAG, "receiving file : transId: " + transId + "bAccept : " + bAccept);
        if (mSAFileTransfer != null) {
            if (bAccept) {
                mSAFileTransfer.receive(transId, path);
            } else {
                mSAFileTransfer.reject(transId);
            }
        }
    }

    public void cancelFileTransfer(int transId) {
        if (mSAFileTransfer != null) {
            mSAFileTransfer.cancel(transId);
        }
    }

    public void registerFileAction(FileAction action) {
        this.mFileAction = action;
    }

    public class ReceiverBinder extends Binder {
        public FileTransferReceiver getService() {
            return FileTransferReceiver.this;
        }
    }

    public class ServiceConnection extends SASocket {
        public ServiceConnection() {
            super(ServiceConnection.class.getName());
        }

        @Override
        protected void onServiceConnectionLost(int reason) {
            Log.e(TAG, "onServiceConnectionLost: reason-" + reason);
            mConnection = null;
        }

        @Override
        public void onReceive(int channelId, byte[] data) {
            try {
                Log.i(TAG, "onReceive: channelId" + channelId + "data: " + new String(data, "UTF-8"));
            } catch (UnsupportedEncodingException e) {
                e.printStackTrace();
            }
        }

        @Override
        public void onError(int channelId, String errorMessage, int errorCode) {
            mFileAction.onFileActionError();
            Log.e(TAG, "Connection is not alive ERROR: " + errorMessage + "  " + errorCode);
        }
    }

    public interface FileAction {
        void onFileActionError();

        void onFileActionProgress(long progress);

        void onFileActionTransferComplete();

        void onFileActionTransferRequested(int id, String path);
    }
}
