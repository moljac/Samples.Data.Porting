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

namespace com.samsung.android.sdk.accessory.example.security.consumer
{

	using Intent = android.content.Intent;
	using Handler = android.os.Handler;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Toast = android.widget.Toast;
	using Log = android.util.Log;

	using com.samsung.android.sdk.accessory;

	public class ConsumerService : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mBinder = new LocalBinder(this);
		}

		private const string TAG = "SecuredConsumer";
		private const int SECURED_CHANNEL_ID = 1111;
		private static readonly Type<ServiceConnection> SASOCKET_CLASS = typeof(ServiceConnection);
		private IBinder mBinder;
		private ServiceConnection mConnectionHandler = null;
		internal Handler mHandler = new Handler();

		public ConsumerService() : base(TAG, SASOCKET_CLASS)
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
			SA mAccessory = new SA();
			try
			{
				mAccessory.initialize(this);
			}
			catch (SsdkUnsupportedException e)
			{
				// try to handle SsdkUnsupportedException
				if (processUnsupportedException(e) == true)
				{
					return;
				}
			}
			catch (Exception e1)
			{
				Console.WriteLine(e1.ToString());
				Console.Write(e1.StackTrace);
				/*
				 * Your application can not use Samsung Accessory SDK. Your application should work smoothly
				 * without using this SDK, or you may want to notify user and close your application gracefully
				 * (release resources, stop Service threads, close UI thread, etc.)
				 */
				stopSelf();
			}
		}

		public override IBinder onBind(Intent intent)
		{
			return mBinder;
		}

		protected internal override void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result)
		{
			if ((result == SAAgent.PEER_AGENT_FOUND) && (peerAgent != null))
			{
				requestServiceConnection(peerAgent);
			}
			else if (result == SAAgent.FINDPEER_DEVICE_NOT_CONNECTED)
			{
				Toast.makeText(ApplicationContext, "FINDPEER_DEVICE_NOT_CONNECTED", Toast.LENGTH_LONG).show();
				updateTextView("Disconnected");
			}
			else if (result == SAAgent.FINDPEER_SERVICE_NOT_FOUND)
			{
				Toast.makeText(ApplicationContext, "FINDPEER_SERVICE_NOT_FOUND", Toast.LENGTH_LONG).show();
				updateTextView("Disconnected");
			}
			else
			{
				Toast.makeText(ApplicationContext, R.@string.NoPeersFound, Toast.LENGTH_LONG).show();
			}
		}

		protected internal override void onServiceConnectionRequested(SAPeerAgent peerAgent)
		{
			if (peerAgent != null)
			{
				acceptServiceConnectionRequest(peerAgent);
			}
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peerAgent, SASocket socket, int result)
		{
			if (result == SAAgent.CONNECTION_SUCCESS)
			{
				this.mConnectionHandler = (ServiceConnection) socket;
				updateTextView("Connected");
			}
			else if (result == SAAgent.CONNECTION_ALREADY_EXIST)
			{
				updateTextView("Connected");
				Toast.makeText(BaseContext, "CONNECTION_ALREADY_EXIST", Toast.LENGTH_LONG).show();
			}
			else if (result == SAAgent.CONNECTION_DUPLICATE_REQUEST)
			{
				Toast.makeText(BaseContext, "CONNECTION_DUPLICATE_REQUEST", Toast.LENGTH_LONG).show();
			}
			else
			{
				Toast.makeText(BaseContext, R.@string.ConnectionFailure, Toast.LENGTH_LONG).show();
			}
		}

		protected internal override void onError(SAPeerAgent peerAgent, string errorMessage, int errorCode)
		{
			base.onError(peerAgent, errorMessage, errorCode);
		}

		protected internal override void onPeerAgentUpdated(SAPeerAgent peerAgent, int result)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final SAPeerAgent peer = peerAgent;
			SAPeerAgent peer = peerAgent;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int status = result;
			int status = result;
			mHandler.post(() =>
			{
				if (peer != null)
				{
					if (status == SAAgent.PEER_AGENT_AVAILABLE)
					{
						Toast.makeText(ApplicationContext, "PEER_AGENT_AVAILABLE", Toast.LENGTH_LONG).show();
					}
					else
					{
						Toast.makeText(ApplicationContext, "PEER_AGENT_UNAVAILABLE", Toast.LENGTH_LONG).show();
					}
				}
			});
		}

		public class ServiceConnection : SASocket
		{
			private readonly ConsumerService outerInstance;

			public ServiceConnection(ConsumerService outerInstance) : base(typeof(ServiceConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			public override void onError(int channelId, string errorMessage, int errorCode)
			{
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String message = new String(data);
				string message = StringHelperClass.NewString(data);
				outerInstance.addMessage("Received: ", message);
			}

			protected internal override void onServiceConnectionLost(int reason)
			{
				outerInstance.updateTextView("Disconnected");
				outerInstance.closeConnection();
			}
		}

		public class LocalBinder : Binder
		{
			private readonly ConsumerService outerInstance;

			public LocalBinder(ConsumerService outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual ConsumerService Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		public virtual void findPeers()
		{
			findPeerAgents();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public boolean sendData(final String data)
		public virtual bool sendData(string data)
		{
			bool retvalue = false;
			if (mConnectionHandler != null)
			{
				try
				{
					mConnectionHandler.secureSend(SECURED_CHANNEL_ID, data.GetBytes());
					retvalue = true;
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				addMessage("Sent: ", data);
			}
			return retvalue;
		}

		public virtual bool closeConnection()
		{
			if (mConnectionHandler != null)
			{
				mConnectionHandler.close();
				mConnectionHandler = null;
				return true;
			}
			else
			{
				return false;
			}
		}

		private bool processUnsupportedException(SsdkUnsupportedException e)
		{
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
			int errType = e.Type;
			if (errType == SsdkUnsupportedException.VENDOR_NOT_SUPPORTED || errType == SsdkUnsupportedException.DEVICE_NOT_SUPPORTED)
			{
				/*
				 * Your application can not use Samsung Accessory SDK. You application should work smoothly
				 * without using this SDK, or you may want to notify user and close your app gracefully (release
				 * resources, stop Service threads, close UI thread, etc.)
				 */
				stopSelf();
			}
			else if (errType == SsdkUnsupportedException.LIBRARY_NOT_INSTALLED)
			{
				Log.e(TAG, "You need to install Samsung Accessory SDK to use this application.");
			}
			else if (errType == SsdkUnsupportedException.LIBRARY_UPDATE_IS_REQUIRED)
			{
				Log.e(TAG, "You need to update Samsung Accessory SDK to use this application.");
			}
			else if (errType == SsdkUnsupportedException.LIBRARY_UPDATE_IS_RECOMMENDED)
			{
				Log.e(TAG, "We recommend that you update your Samsung Accessory SDK before using this application.");
				return false;
			}
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void updateTextView(final String str)
		private void updateTextView(string str)
		{
			mHandler.post(() =>
			{
				ConsumerActivity.updateTextView(str);
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void addMessage(final String prefix, final String data)
		private void addMessage(string prefix, string data)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String strToUI = prefix.concat(data);
			string strToUI = prefix + data;
			mHandler.post(() =>
			{
				ConsumerActivity.addMessage(strToUI);
			});
		}
	}

}