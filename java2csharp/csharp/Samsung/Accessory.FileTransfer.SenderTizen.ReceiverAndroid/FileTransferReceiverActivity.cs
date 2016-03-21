using System;

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

namespace com.samsung.android.sdk.accessory.example.filetransfer.receiver
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using KeyEvent = android.view.KeyEvent;
	using ProgressBar = android.widget.ProgressBar;
	using Toast = android.widget.Toast;

	public class FileTransferReceiverActivity : Activity
	{
		private const string TAG = "FileTransferReceiverActivity";
		private static bool mIsUp = false;
		private static readonly string DEST_DIRECTORY = Environment.ExternalStorageDirectory.AbsolutePath;
		private int mTransId;
		private Context mCtxt;
		private string mDirPath;
		private string mFilePath;
		private AlertDialog mAlert;
		private ProgressBar mRecvProgressBar;
		private FileTransferReceiver mReceiverService;
		private ServiceConnection mServiceConnection = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}

			public override void onServiceDisconnected(ComponentName name)
			{
				Log.i(TAG, "Service disconnected");
				outerInstance.mReceiverService = null;
			}

			public override void onServiceConnected(ComponentName arg0, IBinder binder)
			{
				Log.d(TAG, "Service connected");
				outerInstance.mReceiverService = ((FileTransferReceiver.ReceiverBinder) binder).Service;
				outerInstance.mReceiverService.registerFileAction(outerInstance.FileAction);
			}
		}

		protected internal virtual void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.ft_receiver_activity;
			mIsUp = true;
			mCtxt = ApplicationContext;
			mRecvProgressBar = (ProgressBar) findViewById(R.id.RecvProgress);
			mRecvProgressBar.Max = 100;
			if (!Environment.ExternalStorageState.Equals(Environment.MEDIA_MOUNTED))
			{
				Toast.makeText(mCtxt, " No SDCARD Present", Toast.LENGTH_SHORT).show();
				finish();
			}
			else
			{
				mDirPath = Environment.ExternalStorageDirectory + File.separator + "FileTransferReceiver";
				File file = new File(mDirPath);
				if (file.mkdirs())
				{
					Toast.makeTextuniquetempvar.show();
				}
			}
			mCtxt.bindService(new Intent(ApplicationContext, typeof(FileTransferReceiver)), this.mServiceConnection, Context.BIND_AUTO_CREATE);
		}

		protected internal override void onStart()
		{
			mIsUp = true;
			base.onStart();
		}

		protected internal override void onStop()
		{
			mIsUp = false;
			base.onStop();
		}

		protected internal override void onPause()
		{
			mIsUp = false;
			base.onPause();
		}

		protected internal override void onResume()
		{
			mIsUp = true;
			base.onResume();
		}

		public virtual void onDestroy()
		{
			mIsUp = false;
			base.onDestroy();
		}

		public override void onBackPressed()
		{
			mIsUp = false;
			moveTaskToBack(true);
		}

		// for Android before 2.0, just in case
		public override bool onKeyDown(int keyCode, KeyEvent @event)
		{
			if (keyCode == KeyEvent.KEYCODE_BACK)
			{
				mIsUp = false;
				moveTaskToBack(true);
				return true;
			}
			return base.onKeyDown(keyCode, @event);
		}

		public static bool Up
		{
			get
			{
				return mIsUp;
			}
		}

		private FileTransferReceiver.FileAction FileAction
		{
			get
			{
				return new FileActionAnonymousInnerClassHelper(this);
			}
		}

		private class FileActionAnonymousInnerClassHelper : FileAction
		{
			private readonly FileTransferReceiverActivity outerInstance;

			public FileActionAnonymousInnerClassHelper(FileTransferReceiverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onFileActionError()
			{
				runOnUiThread(() =>
				{
					if (outerInstance.mAlert != null && outerInstance.mAlert.Showing)
					{
						outerInstance.mAlert.dismiss();
					}
					Toast.makeTextuniquetempvar.show();
					outerInstance.mRecvProgressBar.Progress = 0;
				});
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void onFileActionProgress(final long progress)
			public virtual void onFileActionProgress(long progress)
			{
				runOnUiThread(() =>
				{
					outerInstance.mRecvProgressBar.Progress = (int) progress;
				});
			}

			public virtual void onFileActionTransferComplete()
			{
				runOnUiThread(() =>
				{
					outerInstance.mRecvProgressBar.Progress = 0;
					outerInstance.mAlert.dismiss();
					Toast.makeText(outerInstance.mCtxt, "Receive Completed!", Toast.LENGTH_SHORT).show();
				});
			}

			public virtual void onFileActionTransferRequested(int id, string path)
			{
				outerInstance.mFilePath = path;
				outerInstance.mTransId = id;
				runOnUiThread(() =>
				{
					AlertDialog.Builder alertbox = new AlertDialog.Builder(outerInstance);
					alertbox.Message = "Do you want to receive file: " + outerInstance.mFilePath + " ?";
					alertbox.setPositiveButton("Accept", new OnClickListenerAnonymousInnerClassHelper(this));
					alertbox.setNegativeButton("Reject", new OnClickListenerAnonymousInnerClassHelper2(this));
					alertbox.Cancelable = false;
					outerInstance.mAlert = alertbox.create();
					outerInstance.mAlert.show();
				});
			}

			private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
			{
				private readonly FileActionAnonymousInnerClassHelper outerInstance;

				public OnClickListenerAnonymousInnerClassHelper(FileActionAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void onClick(DialogInterface arg0, int arg1)
				{
					outerInstance.outerInstance.mAlert.dismiss();
					try
					{
						string receiveFileName = StringHelperClass.SubstringSpecial(outerInstance.outerInstance.mFilePath, outerInstance.outerInstance.mFilePath.LastIndexOf("/", StringComparison.Ordinal), outerInstance.outerInstance.mFilePath.Length);
						outerInstance.outerInstance.mReceiverService.receiveFile(outerInstance.outerInstance.mTransId, DEST_DIRECTORY + receiveFileName, true);
						Log.i(TAG, "Transfer accepted");
						outerInstance.outerInstance.showQuitDialog();
					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(outerInstance.outerInstance.mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
					}
				}
			}

			private class OnClickListenerAnonymousInnerClassHelper2 : DialogInterface.OnClickListener
			{
				private readonly FileActionAnonymousInnerClassHelper outerInstance;

				public OnClickListenerAnonymousInnerClassHelper2(FileActionAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void onClick(DialogInterface arg0, int arg1)
				{
					outerInstance.outerInstance.mAlert.dismiss();
					try
					{
						outerInstance.outerInstance.mReceiverService.receiveFile(outerInstance.outerInstance.mTransId, DEST_DIRECTORY, false);
						Log.i(TAG, "Transfer rejected");
					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(outerInstance.outerInstance.mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
					}
				}
			}
		}

		private void showQuitDialog()
		{
			runOnUiThread(() =>
			{
				AlertDialog.Builder alertbox = new AlertDialog.Builder(FileTransferReceiverActivity.this);
				alertbox = new AlertDialog.Builder(FileTransferReceiverActivity.this);
				alertbox.Message = "Receiving file : [" + mFilePath + "] QUIT?";
				alertbox.setNegativeButton("OK", new OnClickListenerAnonymousInnerClassHelper3(this));
				alertbox.Cancelable = false;
				mAlert = alertbox.create();
				mAlert.show();
			});
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : DialogInterface.OnClickListener
		{
			private readonly FileTransferReceiverActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(FileTransferReceiverActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(DialogInterface arg0, int arg1)
			{
				try
				{
					outerInstance.mReceiverService.cancelFileTransfer(outerInstance.mTransId);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
					Toast.makeText(outerInstance.mCtxt, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
				}
				outerInstance.mAlert.dismiss();
				outerInstance.mRecvProgressBar.Progress = 0;
			}
		}
	}

}