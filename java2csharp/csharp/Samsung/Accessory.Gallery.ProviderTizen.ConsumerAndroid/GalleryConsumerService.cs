using System;
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
 *     * Neither the name of Samsung Electronics Co., Ltd. nor the names of its contributors may be used to endorse or 
 *       promote products derived from this software without specific prior written permission.
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

namespace com.samsung.android.sdk.accessory.example.galleryconsumer
{


	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using Intent = android.content.Intent;
	using Binder = android.os.Binder;
	using IBinder = android.os.IBinder;
	using Messenger = android.os.Messenger;
	using Log = android.util.Log;

	using com.samsung.android.sdk.accessory;

	public class GalleryConsumerService : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mBinder = new LocalBinder(this);
		}

		public const int GALLERY_CHANNEL_ID = 104; // XML file provided the info
		private const string TAG = "GalleryConsumerService";
		private static readonly Type<GalleryConsumerConnection> SASOCKET_CLASS = typeof(GalleryConsumerConnection);
		private SASocket mConnectionHandler;
		private SA mAccessory;
		private IBinder mBinder;
		internal List<Messenger> mClients = new List<Messenger>();
		internal ImageListReceiver mImageListReceiverRegistered;

		public GalleryConsumerService() : base(TAG, SASOCKET_CLASS)
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
			mAccessory = new SA();
			try
			{
				mAccessory.initialize(this);
			}
			catch (SsdkUnsupportedException e)
			{
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
				 * Your application can not use Samsung Accessory SDK. You application should work smoothly
				 * without using this SDK, or you may want to notify user and close your app gracefully (release
				 * resources, stop Service threads, close UI thread, etc.)
				 */
				stopSelf();
			}
		}

		public override void onLowMemory()
		{
			closeConnection();
			base.onLowMemory();
		}

		public override void onDestroy()
		{
			base.onDestroy();
		}

		public override IBinder onBind(Intent intent)
		{
			return mBinder;
		}

		public virtual bool registerImageReciever(ImageListReceiver uImageReceiver)
		{
			mImageListReceiverRegistered = uImageReceiver;
			return true;
		}

		public virtual bool establishConnection(SAPeerAgent peerAgent)
		{
			if (peerAgent != null)
			{
				requestServiceConnection(peerAgent);
				return true;
			}
			return false;
		}

		public virtual void findPeers()
		{
			findPeerAgents();
		}

		public virtual bool requestImage(int index)
		{
			// width and height logic to be developed by consumer , current support max 320x240.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ImgReqMsg uRequest = new ImgReqMsg((long) index, 320, 240);
			ImageFetchModelImpl.ImgReqMsg uRequest = new ImageFetchModelImpl.ImgReqMsg((long) index, 320, 240);
			string uJsonStringToSend = "";
			try
			{
				uJsonStringToSend = uRequest.toJSON().ToString();
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}
			if (mConnectionHandler != null)
			{
				try
				{
					mConnectionHandler.send(GALLERY_CHANNEL_ID, uJsonStringToSend.GetBytes());
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			return true;
		}

		public virtual bool requestThumbNail(int countRequested)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBListReqMsg uRequest = new TBListReqMsg((long) countRequested);
			ImageFetchModelImpl.TBListReqMsg uRequest = new ImageFetchModelImpl.TBListReqMsg((long) countRequested);
			string uJsonStringToSend = "";
			try
			{
				uJsonStringToSend = uRequest.toJSON().ToString();
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return false;
			}
			if (mConnectionHandler != null)
			{
				try
				{
					mConnectionHandler.send(GALLERY_CHANNEL_ID, uJsonStringToSend.GetBytes());
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			return true;
		}

		public virtual bool closeConnection()
		{
			if (mConnectionHandler != null)
			{
				mConnectionHandler.close();
				mConnectionHandler = null;
			}
			return true;
		}

		protected internal override void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result)
		{
			if (result == PEER_AGENT_FOUND)
			{
				if (mImageListReceiverRegistered != null)
				{
					mImageListReceiverRegistered.onPeerFound(peerAgent);
				}
			}
		}

		protected internal override void onAuthenticationResponse(SAPeerAgent peerAgent, SAAuthenticationToken authToken, int code)
		{
			if (code == SAAgent.AUTHENTICATION_SUCCESS && authToken.AuthenticationType == SAAuthenticationToken.AUTHENTICATION_TYPE_CERTIFICATE_X509)
			{
				requestServiceConnection(peerAgent);
			}
		}

		protected internal override void onPeerAgentUpdated(SAPeerAgent peerAgent, int result)
		{
			if (result == PEER_AGENT_AVAILABLE)
			{
				authenticatePeerAgent(peerAgent);
			}
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peerAgent, SASocket socket, int result)
		{
			if (result == CONNECTION_SUCCESS)
			{ // SERVICE_CONNECTION_RESULT_OK
				this.mConnectionHandler = socket;
				// Toast.makeText(getBaseContext(), R.string.ConnectionEstablishedMsg, Toast.LENGTH_LONG).show();
				mImageListReceiverRegistered.onServiceConnectionResponse(result);
			}
		}

		protected internal override void onError(SAPeerAgent peerAgent, string error, int errorCode)
		{
		}

		private void onDataAvailableonChannel(long channelId, string data)
		{
			if (data.Contains(Model.THUMBNAIL_LIST_RSP))
			{
				handleThumbnails(data);
			}
			else if (data.Contains(Model.DOWNSCALE_IMG_RSP))
			{
				handleDownscaledImage(data);
			}
			else if (data.Contains(Model.THUMBNAIL_PUSH_IND))
			{
				mImageListReceiverRegistered.onThumbnailsPush();
			}
			else if (data.Contains(Model.THUMBNAIL_NEXT_IND))
			{
				mImageListReceiverRegistered.onThumbnailsNext();
			}
		}

		private void handleDownscaledImage(string resp)
		{
			ImageStructure image;
			JSONObject obj = null;
			try
			{
				obj = new JSONObject(resp);
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ImgRespMsg uResponse = new ImgRespMsg();
			ImageFetchModelImpl.ImgRespMsg uResponse = new ImageFetchModelImpl.ImgRespMsg();
			try
			{
				uResponse.fromJSON(obj);
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}
			if (uResponse.Result.Equals("success", StringComparison.CurrentCultureIgnoreCase))
			{ // success case
				// decode and update UI
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBModelJson img = uResponse.getDownscaledImg();
				ImageFetchModelImpl.TBModelJson img = uResponse.DownscaledImg;
				image = new ImageStructure(img.Id.ToString(), img.Data, img.Size.ToString(), img.Name, img.Width.ToString(), img.Height.ToString());
				mImageListReceiverRegistered.onImageReceived(image);
			}
		}

		private void handleThumbnails(string resp)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<ImageStructure> uNotificationList = new java.util.ArrayList<ImageStructure>();
			IList<ImageStructure> uNotificationList = new List<ImageStructure>();
			JSONObject obj = null;
			try
			{
				obj = new JSONObject(resp);
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBListRespMsg uResponse = new TBListRespMsg();
			ImageFetchModelImpl.TBListRespMsg uResponse = new ImageFetchModelImpl.TBListRespMsg();
			try
			{
				uResponse.fromJSON(obj);
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}
			if (uResponse.Result.Equals("success", StringComparison.CurrentCultureIgnoreCase))
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<TBModelJson> list = uResponse.getmsgTBList();
				IList<ImageFetchModelImpl.TBModelJson> list = uResponse.getmsgTBList();
				foreach (ImageFetchModelImpl.TBModelJson cur in list)
				{
					if (cur != null)
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ImageStructure ds = new ImageStructure(String.valueOf(cur.getId()), cur.getData(), String.valueOf(cur.getSize()), cur.getName(), String.valueOf(cur.getWidth()), String.valueOf(cur.getHeight()));
						ImageStructure ds = new ImageStructure(cur.Id.ToString(), cur.Data, cur.Size.ToString(), cur.Name, cur.Width.ToString(), cur.Height.ToString());
						uNotificationList.Add(ds);
					}
				}
			}
			mImageListReceiverRegistered.onThumbnailsReceived(uNotificationList); // to
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

		public interface ImageListReceiver
		{
			void onThumbnailsReceived(IList<ImageStructure> uList);

			void onImageReceived(ImageStructure image);

			void onPeerFound(SAPeerAgent uRemoteAgent);

			void onServiceConnectionResponse(int result);

			void onServiceConnectionLost(int errorcode);

			void onThumbnailsPush();

			void onThumbnailsNext();
		}

		public class LocalBinder : Binder
		{
			private readonly GalleryConsumerService outerInstance;

			public LocalBinder(GalleryConsumerService outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual GalleryConsumerService Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		// service connection helper inner class
		public class GalleryConsumerConnection : SASocket
		{
			private readonly GalleryConsumerService outerInstance;

			public GalleryConsumerConnection(GalleryConsumerService outerInstance) : base(typeof(GalleryConsumerConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String strToUpdateUI = new String(data);
				string strToUpdateUI = StringHelperClass.NewString(data);
				outerInstance.onDataAvailableonChannel(channelId, strToUpdateUI);
			}

			public override void onError(int channelId, string errorString, int error)
			{
			}

			public override void onServiceConnectionLost(int errorCode)
			{
				outerInstance.mConnectionHandler = null;
				outerInstance.mImageListReceiverRegistered.onServiceConnectionLost(errorCode);
			}
		}
	}

}