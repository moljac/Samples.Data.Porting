using System;
using System.Threading;

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

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using Toast = android.widget.Toast;

	using com.samsung.android.sdk.accessory;
	using com.samsung.android.sdk.accessoryfiletransfer;
	using com.samsung.android.sdk.accessoryfiletransfer.SAFileTransfer;

	public class FileTransferReceiver : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mReceiverBinder = new ReceiverBinder(this);
		}

		private const string TAG = "FileTransferReceiver";
		private Context mContext;
		private IBinder mReceiverBinder;
		private static readonly Type<ServiceConnection> SASOCKET_CLASS = typeof(ServiceConnection);
		private ServiceConnection mConnection = null;
		private SAFileTransfer mSAFileTransfer = null;
		private EventListener mCallback;
		private FileAction mFileAction = null;

		public FileTransferReceiver() : base(TAG, SASOCKET_CLASS)
		{
			if (!InstanceFieldsInitialized)
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		public override void onCreate()
		{
			base.onCreate();
			mContext = ApplicationContext;
			Log.d(TAG, "On Create of Sample FileTransferReceiver Service");
			mCallback = new EventListenerAnonymousInnerClassHelper(this);
			SAft saft = new SAft();
			try
			{
				saft.initialize(this);
			}
			catch (SsdkUnsupportedException e)
			{
				if (e.Type == SsdkUnsupportedException.DEVICE_NOT_SUPPORTED)
				{
					Toast.makeText(BaseContext, "Cannot initialize, DEVICE_NOT_SUPPORTED", Toast.LENGTH_SHORT).show();
				}
				else if (e.Type == SsdkUnsupportedException.LIBRARY_NOT_INSTALLED)
				{
					Toast.makeText(BaseContext, "Cannot initialize, LIBRARY_NOT_INSTALLED.", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(BaseContext, "Cannot initialize, UNKNOWN.", Toast.LENGTH_SHORT).show();
				}
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}
			catch (Exception e1)
			{
				Toast.makeText(BaseContext, "Cannot initialize, SAft.", Toast.LENGTH_SHORT).show();
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
				return;
			}
			mSAFileTransfer = new SAFileTransfer(FileTransferReceiver.this, mCallback);
		}

		private class EventListenerAnonymousInnerClassHelper : EventListener
		{
			private readonly FileTransferReceiver outerInstance;

			public EventListenerAnonymousInnerClassHelper(FileTransferReceiver outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onProgressChanged(int transId, int progress)
			{
				Log.d(TAG, "onProgressChanged : " + progress + " for transaction : " + transId);
				if (outerInstance.mFileAction != null)
				{
					outerInstance.mFileAction.onFileActionProgress(progress);
				}
			}

			public override void onTransferCompleted(int transId, string fileName, int errorCode)
			{
				Log.d(TAG, "onTransferCompleted: tr id : " + transId + " file name : " + fileName + " error : " + errorCode);
				if (errorCode == SAFileTransfer.ERROR_NONE)
				{
					outerInstance.mFileAction.onFileActionTransferComplete();
				}
				else
				{
					outerInstance.mFileAction.onFileActionError();
				}
			}

			public override void onTransferRequested(int id, string fileName)
			{
				Log.d(TAG, "onTransferRequested: id- " + id + " file name: " + fileName);
				if (FileTransferReceiverActivity.Up)
				{
					Log.d(TAG, "Activity is up");
					outerInstance.mFileAction.onFileActionTransferRequested(id, fileName);
				}
				else
				{
					Log.d(TAG, "Activity is not up, invoke activity");
					outerInstance.mContext.startActivity(new Intent()
								.setClass(outerInstance.mContext, typeof(FileTransferReceiverActivity)).setFlags(Intent.FLAG_ACTIVITY_NEW_TASK).setAction("incomingFT").putExtra("tx", id).putExtra("fileName", fileName));
					int counter = 0;
					while (counter < 10)
					{
						counter++;
						try
						{
							Thread.Sleep(500);
						}
						catch (InterruptedException e)
						{
							Console.WriteLine(e.ToString());
							Console.Write(e.StackTrace);
						}
						if (outerInstance.mFileAction != null)
						{
							outerInstance.mFileAction.onFileActionTransferRequested(id, fileName);
							break;
						}
					}
				}
			}

			public override void onCancelAllCompleted(int errorCode)
			{
				outerInstance.mFileAction.onFileActionError();
				Log.e(TAG, "onCancelAllCompleted: Error Code " + errorCode);
			}
		}

		public override IBinder onBind(Intent arg0)
		{
			return mReceiverBinder;
		}

		public override void onDestroy()
		{
			mSAFileTransfer.close();
			mSAFileTransfer = null;
			base.onDestroy();
			Log.i(TAG, "FileTransferReceiver Service is Stopped.");
		}

		protected internal override void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result)
		{
			if (mConnection == null)
			{
				Log.d(TAG, "onFindPeerAgentResponse : mConnection is null");
			}
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peer, SASocket socket, int result)
		{
			Log.i(TAG, "onServiceConnectionResponse: result - " + result);
			if (result == SAAgent.CONNECTION_SUCCESS)
			{
				if (socket != null)
				{
					mConnection = (ServiceConnection) socket;
					Toast.makeText(BaseContext, "Connection established for FT", Toast.LENGTH_SHORT).show();
				}
			}
		}

		public virtual void receiveFile(int transId, string path, bool bAccept)
		{
			Log.d(TAG, "receiving file : transId: " + transId + "bAccept : " + bAccept);
			if (mSAFileTransfer != null)
			{
				if (bAccept)
				{
					mSAFileTransfer.receive(transId, path);
				}
				else
				{
					mSAFileTransfer.reject(transId);
				}
			}
		}

		public virtual void cancelFileTransfer(int transId)
		{
			if (mSAFileTransfer != null)
			{
				mSAFileTransfer.cancel(transId);
			}
		}

		public virtual void registerFileAction(FileAction action)
		{
			this.mFileAction = action;
		}

		public class ReceiverBinder : Binder
		{
			private readonly FileTransferReceiver outerInstance;

			public ReceiverBinder(FileTransferReceiver outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual FileTransferReceiver Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		public class ServiceConnection : SASocket
		{
			private readonly FileTransferReceiver outerInstance;

			public ServiceConnection(FileTransferReceiver outerInstance) : base(typeof(ServiceConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			protected internal override void onServiceConnectionLost(int reason)
			{
				Log.e(TAG, "onServiceConnectionLost: reason-" + reason);
				outerInstance.mConnection = null;
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
				try
				{
					Log.i(TAG, "onReceive: channelId" + channelId + "data: " + StringHelperClass.NewString(data, "UTF-8"));
				}
				catch (UnsupportedEncodingException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}

			public override void onError(int channelId, string errorMessage, int errorCode)
			{
				outerInstance.mFileAction.onFileActionError();
				Log.e(TAG, "Connection is not alive ERROR: " + errorMessage + "  " + errorCode);
			}
		}

		public interface FileAction
		{
			void onFileActionError();

			void onFileActionProgress(long progress);

			void onFileActionTransferComplete();

			void onFileActionTransferRequested(int id, string path);
		}
	}

}