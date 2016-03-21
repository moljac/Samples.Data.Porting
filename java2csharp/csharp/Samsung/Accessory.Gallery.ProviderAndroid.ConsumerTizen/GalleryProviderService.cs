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

namespace com.samsung.android.sdk.accessory.example.galleryprovider
{


	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using Intent = android.content.Intent;
	using Cursor = android.database.Cursor;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Binder = android.os.Binder;
	using Handler = android.os.Handler;
	using HandlerThread = android.os.HandlerThread;
	using IBinder = android.os.IBinder;
	using Looper = android.os.Looper;
	using MediaStore = android.provider.MediaStore;
	using Base64 = android.util.Base64;
	using Log = android.util.Log;
	using SparseArray = android.util.SparseArray;
	using Toast = android.widget.Toast;

	using com.samsung.android.sdk.accessory;

	public class GalleryProviderService : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mBinder = new LocalBinder(this);
		}

		private const string TAG = "GalleryProviderService";
		private const int GALLERY_CHANNEL_ID = 104;
		private static readonly Type<GalleryProviderConnection> SASOCKET_CLASS = typeof(GalleryProviderConnection);
		private SA mAccessory;
		private IBinder mBinder;
		private HandlerThread mThread;
		private Looper mLooper;
		private Handler mBackgroundHandler;
		private string mResult = "failure";
		private const int INITIAL_IMAGE_INDEX = -1;
		private const int REASON_OK = 0;
		// private static final int REASON_BITMAP_ENCODING_FAILURE = 1;
		private const int REASON_IMAGE_ID_INVALID = 2;
		private const int REASON_EOF_IMAGE = 3;
		private const int REASON_DATABASE_ERROR = 4;
		private int mReason = REASON_IMAGE_ID_INVALID;
		internal string[] mProjection = new string[] {MediaStore.Images.Media._ID, MediaStore.Images.Media.DATA, MediaStore.Images.Media.SIZE, MediaStore.Images.Media.DISPLAY_NAME, MediaStore.Images.Media.WIDTH, MediaStore.Images.Media.HEIGHT};
		internal SparseArray<GalleryProviderConnection> mConnectionsMap = null;
		internal string mImgData = "";
		internal IList<ImageFetchModelImpl.TBModelJson> mTb = new List<ImageFetchModelImpl.TBModelJson>();

		public GalleryProviderService() : base(TAG, SASOCKET_CLASS)
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
			mThread = new HandlerThread("GalleryProvider");
			mThread.start();
			mLooper = mThread.Looper;
			if (mLooper != null)
			{
				mBackgroundHandler = new Handler(mLooper);
			}
			else
			{
				throw new Exception("Could not get Looper from Handler Thread");
			}
		}

		public override IBinder onBind(Intent intent)
		{
			return mBinder;
		}

		public override void onLowMemory()
		{
			closeConnection();
			base.onLowMemory();
		}

		public override void onDestroy()
		{
			base.onDestroy();
			mAccessory = null;
		}

		public virtual bool closeConnection()
		{
			if (mConnectionsMap != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<Integer> listConnections = new java.util.ArrayList<Integer>(mConnectionsMap.size());
				IList<int?> listConnections = new List<int?>(mConnectionsMap.size());
				foreach (Integer s in listConnections)
				{
					mConnectionsMap.get(s).close();
					mConnectionsMap.remove(s);
				}
			}
			return true;
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peerAgent, SASocket socket, int result)
		{
			if (result == CONNECTION_SUCCESS && socket != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final GalleryProviderConnection myConnection = (GalleryProviderConnection) socket;
				GalleryProviderConnection myConnection = (GalleryProviderConnection) socket;
				if (mConnectionsMap == null)
				{
					mConnectionsMap = new SparseArray<GalleryProviderConnection>();
				}
				myConnection.mConnectionId = (int)(DateTimeHelperClass.CurrentUnixTimeMillis() & 255);
				mConnectionsMap.put(myConnection.mConnectionId, myConnection);
				mBackgroundHandler.post(() =>
				{
					Toast.makeText(BaseContext, R.@string.ConnectionEstablishedMsg, Toast.LENGTH_SHORT).show();
				});
			}
		}

		protected internal override void onFindPeerAgentResponse(SAPeerAgent peerAgent, int result)
		{
		}

		protected internal override void onPeerAgentUpdated(SAPeerAgent peerAgent, int result)
		{
		}

		protected internal override void onError(SAPeerAgent peerAgent, string error, int errorCode)
		{
			Log.e(TAG, "ERROR: " + errorCode + ": " + error);
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
				;
				return false;
			}
			return true;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void onDataAvailableonChannel(final GalleryProviderConnection connection, final long channelId, final String data)
		private void onDataAvailableonChannel(GalleryProviderConnection connection, long channelId, string data)
		{
			mBackgroundHandler.post(() =>
			{
				if (data.Contains(Model.THUMBNAIL_LIST_REQ))
				{
					sendThumbnails(connection, data);
				}
				else if (data.Contains(Model.DOWNSCALE_IMG_REQ))
				{
					sendDownscaledImage(connection, data);
				}
			});
		}

		private void publishMediaStoreInfo(Cursor imageCursor)
		{
			for (int j = 0; j < imageCursor.Count; j++)
			{
				imageCursor.moveToNext();
			}
			imageCursor.moveToFirst();
		}

		private bool pullThumbnails(Cursor imageCursor)
		{
			string data = "";
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long img_id = imageCursor.getLong(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media._ID));
			long img_id = imageCursor.getLong(imageCursor.getColumnIndex(MediaStore.Images.Media._ID));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.Bitmap bm = android.provider.MediaStore.Images.Thumbnails.getThumbnail(getApplicationContext().getContentResolver(), img_id, android.provider.MediaStore.Images.Thumbnails.MICRO_KIND, null);
			Bitmap bm = MediaStore.Images.Thumbnails.getThumbnail(ApplicationContext.ContentResolver, img_id, MediaStore.Images.Thumbnails.MICRO_KIND, null);
			if (bm == null)
			{
				return false;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.ByteArrayOutputStream stream = new java.io.ByteArrayOutputStream();
			ByteArrayOutputStream stream = new ByteArrayOutputStream();
			bm.compress(Bitmap.CompressFormat.JPEG, 80, stream);
			data = Base64.encodeToString(stream.toByteArray(), Base64.NO_WRAP);
			try
			{
				stream.close();
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long img_size = imageCursor.getLong(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media.SIZE));
			long img_size = imageCursor.getLong(imageCursor.getColumnIndex(MediaStore.Images.Media.SIZE));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String name = imageCursor.getString(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media.DISPLAY_NAME));
			string name = imageCursor.getString(imageCursor.getColumnIndex(MediaStore.Images.Media.DISPLAY_NAME));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = imageCursor.getInt(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media.WIDTH));
			int width = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media.WIDTH));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = imageCursor.getInt(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media.HEIGHT));
			int height = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media.HEIGHT));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBModelJson msg = new TBModelJson(img_id, name, data, img_size, width, height);
			ImageFetchModelImpl.TBModelJson msg = new ImageFetchModelImpl.TBModelJson(img_id, name, data, img_size, width, height);
			mTb.Add(msg);
			return true;
		}

		private void sendThumbnails(GalleryProviderConnection connection, string request)
		{
			bool ret = true;
			mResult = "failure";
			mReason = REASON_IMAGE_ID_INVALID;
			int count = 0;
			if (mTb.Count > 0)
			{
				mTb.Clear();
			}
			JSONObject obj = null;
			try
			{
				obj = new JSONObject(request);
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
//ORIGINAL LINE: final TBListReqMsg uRequest = new TBListReqMsg();
			ImageFetchModelImpl.TBListReqMsg uRequest = new ImageFetchModelImpl.TBListReqMsg();
			try
			{
				uRequest.fromJSON(obj);
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
//ORIGINAL LINE: final long id = uRequest.getID();
			long id = uRequest.ID;
			Cursor imageCursor = ContentResolver.query(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, mProjection, null, null, null);
			if (imageCursor == null)
			{
				mReason = REASON_DATABASE_ERROR;
				sendTbListMsg(connection);
				return;
			}
			imageCursor.moveToFirst();
			publishMediaStoreInfo(imageCursor);
			if (id != INITIAL_IMAGE_INDEX)
			{
				for (int i = 0; i < imageCursor.Count; i++)
				{
					if (id == imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media._ID)))
					{
						ret = imageCursor.moveToNext();
						break;
					}
					if (imageCursor.moveToNext() == false)
					{
						ret = false;
						break;
					}
				}
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int size = imageCursor.getCount();
			int size = imageCursor.Count;
			if ((ret == true) && (size > 0))
			{
				do
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean status = pullThumbnails(imageCursor);
					bool status = pullThumbnails(imageCursor);
					if (status == true)
					{
						count++;
					}
				} while (count < 3 && imageCursor.moveToNext());
				mResult = "success";
				mReason = REASON_OK;
			} // check to ignore in case id is last record in DB
			else
			{
				mReason = REASON_EOF_IMAGE;
			}
			if (!imageCursor.Closed)
			{
				imageCursor.close();
			}
			sendTbListMsg(connection);
		}

		private void sendTbListMsg(GalleryProviderConnection connection)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBListRespMsg uRMessage = new TBListRespMsg(mResult, mReason, mTb.size(), mTb);
			ImageFetchModelImpl.TBListRespMsg uRMessage = new ImageFetchModelImpl.TBListRespMsg(mResult, mReason, mTb.Count, mTb);
			string uJsonStringToSend = "";
			try
			{
				uJsonStringToSend = uRMessage.toJSON().ToString();
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			if (mConnectionsMap != null)
			{
				try
				{
					connection.send(GALLERY_CHANNEL_ID, uJsonStringToSend.GetBytes());
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private void sendDownscaledImage(GalleryProviderConnection connection, string request)
		{
			// put a upper cap like say 320x240 for image
			mImgData = "";
			mResult = "failure";
			mReason = REASON_IMAGE_ID_INVALID;
			int orgWidth = 0, orgHeight = 0;
			long orgSize = 0;
			string orgName = "";
			JSONObject obj = null;
			try
			{
				obj = new JSONObject(request);
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
//ORIGINAL LINE: final ImgReqMsg uMessage = new ImgReqMsg();
			ImageFetchModelImpl.ImgReqMsg uMessage = new ImageFetchModelImpl.ImgReqMsg();
			try
			{
				uMessage.fromJSON(obj);
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
//ORIGINAL LINE: final long id = uMessage.getID();
			long id = uMessage.ID;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int width = uMessage.getWidth();
			int width = uMessage.Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int height = uMessage.getHeight();
			int height = uMessage.Height;
			Cursor imageCursor = ContentResolver.query(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, mProjection, MediaStore.Images.Media._ID + " = " + id, null, null);
			if (imageCursor != null && imageCursor.moveToFirst())
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String path = imageCursor.getString(imageCursor.getColumnIndex(android.provider.MediaStore.Images.Media.DATA));
				string path = imageCursor.getString(imageCursor.getColumnIndex(MediaStore.Images.Media.DATA));
				orgWidth = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media.WIDTH));
				orgHeight = imageCursor.getInt(imageCursor.getColumnIndex(MediaStore.Images.Media.HEIGHT));
				orgName = imageCursor.getString(imageCursor.getColumnIndex(MediaStore.Images.Media.DISPLAY_NAME));
				orgSize = imageCursor.getLong(imageCursor.getColumnIndex(MediaStore.Images.Media.SIZE));
				if (!imageCursor.Closed)
				{
					imageCursor.close();
				}
				pullDownscaledImg(path, width, height);
			}
			else
			{
				mResult = "failure";
				mReason = REASON_IMAGE_ID_INVALID;
			}
			sendImgRsp(connection, id, orgName, orgSize, orgWidth, orgHeight);
		}

		private void sendImgRsp(GalleryProviderConnection connection, long id, string orgName, long orgSize, int orgWidth, int orgHeight)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final TBModelJson msg = new TBModelJson(id, orgName, mImgData, orgSize, orgWidth, orgHeight);
			ImageFetchModelImpl.TBModelJson msg = new ImageFetchModelImpl.TBModelJson(id, orgName, mImgData, orgSize, orgWidth, orgHeight);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final ImgRespMsg uresponse = new ImgRespMsg(mResult, mReason, msg);
			ImageFetchModelImpl.ImgRespMsg uresponse = new ImageFetchModelImpl.ImgRespMsg(mResult, mReason, msg);
			string uJsonStringToSend = "";
			try
			{
				uJsonStringToSend = uresponse.toJSON().ToString();
			}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final org.json.JSONException e)
			catch (JSONException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			if (mConnectionsMap != null)
			{
				try
				{
					connection.send(GALLERY_CHANNEL_ID, uJsonStringToSend.GetBytes());
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private void pullDownscaledImg(string path, int width, int height)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.BitmapFactory.Options opt = new android.graphics.BitmapFactory.Options();
			BitmapFactory.Options opt = new BitmapFactory.Options();
			opt.inScaled = false;
			opt.inSampleSize = 4; // logic based on original and requested size.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.Bitmap scaledbitmap = android.graphics.Bitmap.createScaledBitmap(android.graphics.BitmapFactory.decodeFile(path, opt), width, height, false);
			Bitmap scaledbitmap = Bitmap.createScaledBitmap(BitmapFactory.decodeFile(path, opt), width, height, false);
			if (scaledbitmap != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.ByteArrayOutputStream stream = new java.io.ByteArrayOutputStream();
				ByteArrayOutputStream stream = new ByteArrayOutputStream();
				scaledbitmap.compress(Bitmap.CompressFormat.JPEG, 80, stream);
				mImgData = Base64.encodeToString(stream.toByteArray(), Base64.NO_WRAP);
				try
				{
					stream.close();
				}
//JAVA TO C# CONVERTER WARNING: 'final' catch parameters are not available in C#:
//ORIGINAL LINE: catch (final java.io.IOException e)
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			mResult = "success"; // success
			mReason = REASON_OK; // ok
		}

		public class LocalBinder : Binder
		{
			private readonly GalleryProviderService outerInstance;

			public LocalBinder(GalleryProviderService outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual GalleryProviderService Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		public class GalleryProviderConnection : SASocket
		{
			private readonly GalleryProviderService outerInstance;

			public const string TAG = "GalleryProviderConnection";
			internal int mConnectionId;

			public GalleryProviderConnection(GalleryProviderService outerInstance) : base(typeof(GalleryProviderConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String strToUpdateUI = new String(data);
				string strToUpdateUI = StringHelperClass.NewString(data);
				outerInstance.onDataAvailableonChannel(this, channelId, strToUpdateUI);
			}

			public override void onError(int channelId, string errorString, int error)
			{
			}

			public override void onServiceConnectionLost(int errorCode)
			{
				if (outerInstance.mConnectionsMap != null)
				{
					outerInstance.mConnectionsMap.remove(mConnectionId);
					outerInstance.mBackgroundHandler.post(() =>
					{
						Toast.makeText(BaseContext, R.@string.ConnectionLostMsg, Toast.LENGTH_SHORT).show();
					});
				}
			}
		}
	}

}