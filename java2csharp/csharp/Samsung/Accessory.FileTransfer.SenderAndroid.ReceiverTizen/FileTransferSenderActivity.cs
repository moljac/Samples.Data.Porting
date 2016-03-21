using System.Collections.Generic;

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

namespace com.samsung.android.sdk.accessory.example.filetransfer.sender
{


	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using MediaRecorder = android.media.MediaRecorder;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using KeyEvent = android.view.KeyEvent;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using ProgressBar = android.widget.ProgressBar;
	using Toast = android.widget.Toast;

	public class FileTransferSenderActivity : Activity, View.OnClickListener
	{
		private const string TAG = "FileTransferSenderActivity";
		private static readonly string SRC_PATH = Environment.ExternalStorageDirectory.AbsolutePath + "/src.aaa";
		private Button mBtnSend;
		private Button mBtnConn;
		private Button mBtnCancel;
		private Button mBtnCancelAll;
		private ProgressBar mSentProgressBar;
		private Context mCtxt;
		private string mDirPath;
		private long currentTransId;
		private long mFileSize;
		private IList<long?> mTransactions = new List<long?>();
		private FileTransferSender mSenderService;
		private ServiceConnection mServiceConnection = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}

			public override void onServiceDisconnected(ComponentName name)
			{
				Log.i(TAG, "Service disconnected");
				outerInstance.mSenderService = null;
			}

			public override void onServiceConnected(ComponentName arg0, IBinder binder)
			{
				Log.d(TAG, "Service connected");
				outerInstance.mSenderService = ((FileTransferSender.SenderBinder) binder).Service;
				outerInstance.mSenderService.registerFileAction(outerInstance.FileAction);
			}
		}

		protected internal virtual void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.ft_sender_activity;
			mCtxt = ApplicationContext;
			mBtnSend = (Button) findViewById(R.id.Send);
			mBtnSend.OnClickListener = this;
			mBtnConn = (Button) findViewById(R.id.connectButton);
			mBtnConn.OnClickListener = this;
			mBtnCancel = (Button) findViewById(R.id.cancel);
			mBtnCancel.OnClickListener = this;
			mBtnCancelAll = (Button) findViewById(R.id.cancelAll);
			mBtnCancelAll.OnClickListener = this;
			if (!Environment.ExternalStorageState.Equals(Environment.MEDIA_MOUNTED))
			{
				Toast.makeText(mCtxt, " No SDCARD Present", Toast.LENGTH_SHORT).show();
				finish();
			}
			else
			{
				mDirPath = Environment.ExternalStorageDirectory + File.separator + "FileTransferSender";
				File file = new File(mDirPath);
				if (file.mkdirs())
				{
					Toast.makeTextuniquetempvar.show();
				}
			}
			mCtxt.bindService(new Intent(ApplicationContext, typeof(FileTransferSender)), this.mServiceConnection, Context.BIND_AUTO_CREATE);
			mSentProgressBar = (ProgressBar) findViewById(R.id.fileTransferProgressBar);
			mSentProgressBar.Max = 100;
		}

		public virtual void onDestroy()
		{
			ApplicationContext.unbindService(mServiceConnection);
			base.onDestroy();
		}

		public virtual void onError(MediaRecorder mr, int what, int extra)
		{
			Toast.makeText(mCtxt, " MAX SERVER DIED ", Toast.LENGTH_SHORT).show();
		}

		public override void onBackPressed()
		{
			moveTaskToBack(true);
		}

		// for Android before 2.0, just in case
		public override bool onKeyDown(int keyCode, KeyEvent @event)
		{
			if (keyCode == KeyEvent.KEYCODE_BACK)
			{
				moveTaskToBack(true);
				return true;
			}
			return base.onKeyDown(keyCode, @event);
		}

		public virtual void onClick(View v)
		{
			if (v.Equals(mBtnSend))
			{
				File file = new File(SRC_PATH);
				mFileSize = file.length();
				Toast.makeTextuniquetempvar.show();
				if (SenderServiceBound)
				{
					try
					{
						int trId = mSenderService.sendFile(SRC_PATH);
						mTransactions.Add((long) trId);
						currentTransId = trId;
					}
					catch (System.ArgumentException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
					}
				}
			}
			else if (v.Equals(mBtnCancel))
			{
				if (mSenderService != null)
				{
					try
					{
						mSenderService.cancelFileTransfer((int) currentTransId);
						mTransactions.RemoveAt(currentTransId);
					}
					catch (System.ArgumentException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
					}
				}
				else
				{
					Toast.makeText(mCtxt, "no binding to service", Toast.LENGTH_SHORT).show();
				}
			}
			else if (v.Equals(mBtnCancelAll))
			{
				if (mSenderService != null)
				{
					mSenderService.cancelAllTransactions();
					mTransactions.Clear();
				}
				else
				{
					Toast.makeText(mCtxt, "no binding to service", Toast.LENGTH_SHORT).show();
				}
			}
			else if (v.Equals(mBtnConn))
			{
				if (mSenderService != null)
				{
					mSenderService.connect();
				}
				else
				{
					Toast.makeText(ApplicationContext, "Service not Bound", Toast.LENGTH_SHORT).show();
				}
			}
		}

		private FileTransferSender.FileAction FileAction
		{
			get
			{
				return new FileActionAnonymousInnerClassHelper(this);
			}
		}

		private class FileActionAnonymousInnerClassHelper : FileAction
		{
			private readonly FileTransferSenderActivity outerInstance;

			public FileActionAnonymousInnerClassHelper(FileTransferSenderActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onFileActionError()
			{
				runOnUiThread(() =>
				{
					outerInstance.mSentProgressBar.Progress = 0;
					outerInstance.mTransactions.RemoveAt(outerInstance.currentTransId);
					Toast.makeText(outerInstance.mCtxt, "Error", Toast.LENGTH_SHORT).show();
				});
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void onFileActionProgress(final long progress)
			public virtual void onFileActionProgress(long progress)
			{
				runOnUiThread(() =>
				{
					outerInstance.mSentProgressBar.Progress = (int) progress;
				});
			}

			public virtual void onFileActionTransferComplete()
			{
				runOnUiThread(() =>
				{
					outerInstance.mSentProgressBar.Progress = 0;
					outerInstance.mTransactions.RemoveAt(outerInstance.currentTransId);
					Toast.makeText(outerInstance.mCtxt, "Transfer Completed!", Toast.LENGTH_SHORT).show();
				});
			}

			public virtual void onFileActionCancelAllComplete()
			{
				runOnUiThread(() =>
				{
					outerInstance.mSentProgressBar.Progress = 0;
					outerInstance.mTransactions.RemoveAt(outerInstance.currentTransId);
				});
			}
		}

		private bool SenderServiceBound
		{
			get
			{
				return this.mSenderService != null;
			}
		}
	}

}