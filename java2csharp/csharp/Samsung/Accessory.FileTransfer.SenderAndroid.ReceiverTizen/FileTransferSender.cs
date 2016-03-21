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

namespace com.samsung.android.sdk.accessory.example.filetransfer.sender
{

	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Log = android.util.Log;
	using Toast = android.widget.Toast;

	using com.samsung.android.sdk.accessory;
	using com.samsung.android.sdk.accessoryfiletransfer;
	using com.samsung.android.sdk.accessoryfiletransfer.SAFileTransfer;

	public class FileTransferSender : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mSenderBinder = new SenderBinder(this);
		}

		private const string TAG = "FileTransferSender";
		private static readonly Type<ServiceConnection> SASOCKET_CLASS = typeof(ServiceConnection);
		private int trId = -1;
		private int errCode = SAFileTransfer.ERROR_NONE;
		private SAPeerAgent mPeerAgent = null;
		private IBinder mSenderBinder;
		private SAFileTransfer mSAFileTransfer = null;
		private EventListener mCallback = null;
		private FileAction mFileAction = null;

		public FileTransferSender() : base(TAG, SASOCKET_CLASS)
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
			Log.d(TAG, "On Create of Sample FileTransferSender Service");
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
			mSAFileTransfer = new SAFileTransfer(FileTransferSender.this, mCallback);
		}

		private class EventListenerAnonymousInnerClassHelper : EventListener
		{
			private readonly FileTransferSender outerInstance;

			public EventListenerAnonymousInnerClassHelper(FileTransferSender outerInstance)
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
				outerInstance.errCode = errorCode;
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
				// No use at sender side
			}

			public override void onCancelAllCompleted(int errorCode)
			{
				if (errorCode == SAFileTransfer.ERROR_NONE)
				{
					outerInstance.mFileAction.onFileActionCancelAllComplete();
				}
				else if (errorCode == SAFileTransfer.ERROR_TRANSACTION_NOT_FOUND)
				{
					Toast.makeText(BaseContext, "onCancelAllCompleted : ERROR_TRANSACTION_NOT_FOUND.", Toast.LENGTH_SHORT).show();
				}
				else if (errorCode == SAFileTransfer.ERROR_NOT_SUPPORTED)
				{
					Toast.makeText(BaseContext, "onCancelAllCompleted : ERROR_NOT_SUPPORTED.", Toast.LENGTH_SHORT).show();
				}
				Log.e(TAG, "onCancelAllCompleted: Error Code " + errorCode);
			}
		}

		public override IBinder onBind(Intent arg0)
		{
			return mSenderBinder;
		}

		public override void onDestroy()
		{
			try
			{
				mSAFileTransfer.close();
				mSAFileTransfer = null;
			}
			catch (Exception e)
			{
				Log.e(TAG, e.Message);
			}
			base.onDestroy();
			Log.i(TAG, "FileTransferSender Service is Stopped.");
		}

		protected internal override void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result)
		{
			if (peerAgent != null)
			{
				mPeerAgent = peerAgent;
			}
			else
			{
				Log.e(TAG, "No peer Aget found:" + result);
				Toast.makeText(BaseContext, "No peer agent found.", Toast.LENGTH_SHORT).show();
			}
		}

		protected internal override void onPeerAgentUpdated(SAPeerAgent peerAgent, int result)
		{
			Log.d(TAG, "Peer agent updated- result: " + result + " trId: " + trId);
			mPeerAgent = peerAgent;
			if (result == SAAgent.PEER_AGENT_UNAVAILABLE)
			{
				if (errCode != SAFileTransfer.ERROR_CONNECTION_LOST)
				{
					try
					{
						cancelFileTransfer(trId);
					}
					catch (System.ArgumentException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						Toast.makeText(BaseContext, "IllegalArgumentException", Toast.LENGTH_SHORT).show();
					}
				}
			}
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peerAgent, SASocket socket, int result)
		{
			Log.i(TAG, "onServiceConnectionResponse: result - " + result);
			if (socket == null)
			{
				if (result == SAAgent.CONNECTION_ALREADY_EXIST)
				{
					Toast.makeText(BaseContext, "CONNECTION_ALREADY_EXIST", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Toast.makeText(BaseContext, "Connection could not be made. Please try again", Toast.LENGTH_SHORT).show();
				}
			}
			else
			{
				Toast.makeText(BaseContext, "Connection established for FT", Toast.LENGTH_SHORT).show();
			}
		}

		public virtual void connect()
		{
			if (mPeerAgent != null)
			{
				requestServiceConnection(mPeerAgent);
			}
			else
			{
				base.findPeerAgents();
				Toast.makeText(BaseContext, "No peer agent found yet. Please try again", Toast.LENGTH_SHORT).show();
			}
		}

		public virtual int sendFile(string mSelectedFileName)
		{
			if (mSAFileTransfer != null && mPeerAgent != null)
			{
				trId = mSAFileTransfer.send(mPeerAgent, mSelectedFileName);
				return trId;
			}
			else
			{
				Toast.makeText(BaseContext, "Peer could not be found. Try again.", Toast.LENGTH_SHORT).show();
				findPeerAgents();
				return -1;
			}
		}

		public virtual void cancelFileTransfer(int transId)
		{
			if (mSAFileTransfer != null)
			{
				mSAFileTransfer.cancel(transId);
			}
		}

		public virtual void cancelAllTransactions()
		{
			if (mSAFileTransfer != null)
			{
				mSAFileTransfer.cancelAll();
			}
		}

		public virtual void registerFileAction(FileAction action)
		{
			this.mFileAction = action;
		}

		public class SenderBinder : Binder
		{
			private readonly FileTransferSender outerInstance;

			public SenderBinder(FileTransferSender outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual FileTransferSender Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		public class ServiceConnection : SASocket
		{
			private readonly FileTransferSender outerInstance;

			public ServiceConnection(FileTransferSender outerInstance) : base(typeof(ServiceConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			protected internal override void onServiceConnectionLost(int reason)
			{
				Log.e(TAG, "onServiceConnectionLost: reason-" + reason);
				if (outerInstance.mSAFileTransfer != null)
				{
					outerInstance.mFileAction.onFileActionError();
				}
				outerInstance.mPeerAgent = null;
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
			}

			public override void onError(int channelId, string errorMessage, int errorCode)
			{
			}
		}

		public interface FileAction
		{
			void onFileActionError();

			void onFileActionProgress(long progress);

			void onFileActionTransferComplete();

			void onFileActionCancelAllComplete();
		}
	}

}