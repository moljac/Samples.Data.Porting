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

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using Message = android.os.Message;
	using Base64 = android.util.Base64;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using ImageView = android.widget.ImageView;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using ImageListReceiver = com.samsung.android.sdk.accessory.example.galleryconsumer.GalleryConsumerService.ImageListReceiver;
	using LocalBinder = com.samsung.android.sdk.accessory.example.galleryconsumer.GalleryConsumerService.LocalBinder;
	using com.samsung.android.sdk.accessory;

	public class GalleryConsumerActivity : Activity, View.OnClickListener, ImageListReceiver
	{
		public const int MSG_INITIATE_CONNECTION = 6;
		public const int MSG_THUMBNAIL_RECEIVED = 1986;
		public const int MSG_IMAGE_RECEIVED = 1987;
		public const int INITIAL_IMAGE_INDEX = -1;
		private const string TAG = "GalleryConsumerActivity";
		internal int mNextIndex = INITIAL_IMAGE_INDEX;
		internal int mIndextb1 = INITIAL_IMAGE_INDEX;
		internal int mIndextb2 = INITIAL_IMAGE_INDEX;
		internal int mIndextb3 = INITIAL_IMAGE_INDEX;
		internal GalleryConsumerService mBackendService = null;
		internal IList<ImageStructure> mDTBListReceived = null;
		internal ImageStructure mImage = null;
		internal string mDownscaledImage = "";
		internal bool mIsBound = false;
		internal bool mReConnect = false;
		internal Button mFetch;
		internal Button mNext;
		internal Button mConnect;
		internal Button mClose;
		internal ImageView mTb1;
		internal ImageView mTb2;
		internal ImageView mTb3;
		internal TextView mTxt1;
		internal TextView mTxt2;
		internal TextView mTxt3;
		internal Handler mHandler = new HandlerAnonymousInnerClassHelper(new ImageReceivedHandlerCallback(this));

		private class HandlerAnonymousInnerClassHelper : Handler
		{
			public HandlerAnonymousInnerClassHelper(ImageReceivedHandlerCallback new) : base(new ImageReceivedHandlerCallback)
			{
			}

		}
		private static object mListLock = new object();
		private ServiceConnection mConnection = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}

			public virtual void onServiceConnected(ComponentName className, IBinder service)
			{
				LocalBinder binder = (LocalBinder) service;
				outerInstance.mBackendService = binder.Service;
				outerInstance.mBackendService.registerImageReciever(outerInstance);
				outerInstance.mIsBound = true;
				outerInstance.mBackendService.findPeers();
			}

			public virtual void onServiceDisconnected(ComponentName className)
			{
				Log.e(TAG, "Gallery Service Disconnected");
				outerInstance.mBackendService = null;
				outerInstance.mIsBound = false;
			}
		}

		protected internal override void onResume()
		{
			if (mReConnect == true && mBackendService != null)
			{
				mBackendService.findPeers();
			}
			base.onResume();
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;
			// Bind to the consumer service on create itself
			doBindServiceToConsumerService();
			mFetch = (Button) findViewById(R.id.btnFetch);
			mFetch.OnClickListener = this;
			mNext = (Button) findViewById(R.id.btnNext);
			mNext.OnClickListener = this;
			mConnect = (Button) findViewById(R.id.btnConnect);
			mConnect.OnClickListener = this;
			mClose = (Button) findViewById(R.id.btnClose);
			mClose.OnClickListener = this;
			mTb1 = (ImageView) findViewById(R.id.imageView1);
			mTb1.OnClickListener = this;
			mTb2 = (ImageView) findViewById(R.id.imageView2);
			mTb2.OnClickListener = this;
			mTb3 = (ImageView) findViewById(R.id.imageView3);
			mTb3.OnClickListener = this;
			mTxt1 = (TextView) findViewById(R.id.textView1);
			mTxt1.OnClickListener = this;
			mTxt2 = (TextView) findViewById(R.id.textView2);
			mTxt2.OnClickListener = this;
			mTxt3 = (TextView) findViewById(R.id.textView3);
			mTxt3.OnClickListener = this;
		}

		protected internal virtual void onDestroy()
		{
			Log.i(TAG, "onDestroy");
			closeConnection();
			doUnbindService();
			base.onDestroy();
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
				case R.id.btnConnect:
				{
					if (mIsBound)
					{
						mBackendService.findPeers();
					}
				}
					break;
				case R.id.btnFetch:
				{
					if (mIsBound)
					{
						requestThumbnails();
					}
				}
					break;
				case R.id.btnNext:
				{
					if (mIsBound)
					{
						requestNext();
					}
				}
					break;
				case R.id.btnClose:
				{
					if (mIsBound)
					{
						closeConnection();
					}
					clearThumbnails();
					mNextIndex = INITIAL_IMAGE_INDEX;
				}
					break;
				case R.id.imageView1:
				{
					requestImage(mIndextb1);
				}
					break;
				case R.id.imageView2:
				{
					requestImage(mIndextb2);
				}
					break;
				case R.id.imageView3:
				{
					requestImage(mIndextb3);
				}
					break;
				default:
			break;
			}
		}

		public virtual void onImageReceived(ImageStructure img)
		{
			mImage = img;
			Message msg = Message.obtain();
			msg.what = MSG_IMAGE_RECEIVED;
			mHandler.sendMessage(msg);
		}

		public virtual void onThumbnailsReceived(IList<ImageStructure> uListReceived)
		{
			lock (mListLock)
			{
				// consumer can use this list to hold previous thumbnails as cache
				Log.d(TAG, "onThumbnailsReceived Enter");
				mDTBListReceived = uListReceived;
				Message msg = Message.obtain();
				msg.what = MSG_THUMBNAIL_RECEIVED;
				mHandler.sendMessage(msg);
			}
		}

		public virtual void onPeerFound(SAPeerAgent uRemoteAgent)
		{
			if (uRemoteAgent != null)
			{
				if (mIsBound = true)
				{
					mBackendService.establishConnection(uRemoteAgent);
				}
			}
			else
			{
				Toast.makeText(ApplicationContext, R.@string.NoPeersFound, Toast.LENGTH_LONG).show();
			}
		}

		public virtual void onServiceConnectionResponse(int result)
		{
			if (result == SAAgent.CONNECTION_SUCCESS)
			{
				Toast.makeText(ApplicationContext, R.@string.ConnectionEstablishedMsg, Toast.LENGTH_LONG).show();
	//            mHandler.post(new Runnable() {
	//                @Override
	//                public void run() {
	//                    Toast.makeText(getBaseContext(), R.string.ConnectionEstablishedMsg, Toast.LENGTH_SHORT).show();
	//                }
	//            });
			}
		}

		public virtual void onServiceConnectionLost(int errorcode)
		{
			if (errorcode == SASocket.CONNECTION_LOST_DEVICE_DETACHED)
			{
				mReConnect = true;
			}
			mHandler.post(() =>
			{
				Toast.makeText(BaseContext, R.@string.ConnectionLostMsg, Toast.LENGTH_SHORT).show();
			});
		}

		public virtual void onThumbnailsPush()
		{
			if (mIsBound)
			{
				requestThumbnails();
			}
		}

		public virtual void onThumbnailsNext()
		{
			if (mIsBound)
			{
				requestNext();
			}
		}

		internal virtual void doBindServiceToConsumerService()
		{
			mIsBound = bindService(new Intent(this, typeof(GalleryConsumerService)), mConnection, Context.BIND_AUTO_CREATE);
		}

		internal virtual void doUnbindService()
		{
			if (mIsBound == true)
			{
				unbindService(mConnection);
				mIsBound = false;
			}
		}

		internal virtual void closeConnection()
		{
			if (mIsBound == true)
			{
				mBackendService.closeConnection();
			}
		}

		private void requestThumbnails()
		{
			mBackendService.requestThumbNail(INITIAL_IMAGE_INDEX);
			mNextIndex = INITIAL_IMAGE_INDEX;
		}

		private void requestNext()
		{
			if (mNextIndex != -1)
			{
				mBackendService.requestThumbNail(mNextIndex);
			}
			else
			{
				requestThumbnails();
			}
		}

		private void requestImage(int index)
		{
			if (index != INITIAL_IMAGE_INDEX)
			{
				mBackendService.requestImage(index);
			}
		}

		private void clearThumbnails()
		{
			mIndextb1 = INITIAL_IMAGE_INDEX;
			mIndextb2 = INITIAL_IMAGE_INDEX;
			mIndextb3 = INITIAL_IMAGE_INDEX;
			mTb1.ImageResource = R.drawable.ic_launcher;
			mTb2.ImageResource = R.drawable.ic_launcher;
			mTb3.ImageResource = R.drawable.ic_launcher;
			mTxt1.Text = " ";
			mTxt2.Text = " ";
			mTxt3.Text = " ";
		}

		private bool handleRecievedImage()
		{
			// parse the structure and render on Image VIEW
			mDownscaledImage = mImage.mData;
			sbyte[] decodedstream;

			try
			{
				decodedstream = Base64.decode(mDownscaledImage, Base64.NO_WRAP);
			}
			catch (System.ArgumentException e)
			{
				Log.e(TAG, "handleReceivedThumbnails: mDownscaledImage " + mDownscaledImage);
				Log.e(TAG, "handleReceivedThumbnails decode failed " + e.Message);
				return false;
			}

			Bitmap bitmap = BitmapFactory.decodeByteArray(decodedstream, 0, decodedstream.Length);
			// try new full screen approach
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.app.AlertDialog.Builder thumbDialog = new android.app.AlertDialog.Builder(GalleryConsumerActivity.this);
			AlertDialog.Builder thumbDialog = new AlertDialog.Builder(GalleryConsumerActivity.this);
			ImageView thumbView = new ImageView(GalleryConsumerActivity.this);
			thumbView.MinimumWidth = 320;
			thumbView.MinimumWidth = 240;
			thumbView.BackgroundResource = 0x00000;
			if (bitmap != null)
			{
				thumbView.ImageBitmap = bitmap;
			}
			LinearLayout layout = new LinearLayout(GalleryConsumerActivity.this);
			layout.Orientation = LinearLayout.VERTICAL;
			layout.addView(thumbView);
			thumbDialog.View = layout;
			thumbDialog.show();
			Toast.makeText(ApplicationContext, mImage.mDisplayname, Toast.LENGTH_LONG).show();
			return true;
		}

		private bool handleReceivedThumbnails()
		{
			lock (mListLock)
			{
				int size = mDTBListReceived.Count;
				if (mDTBListReceived.Count > 0)
				{
					// last elements id for reference
					try
					{
						mNextIndex = int.Parse(mDTBListReceived[size - 1].mId);
					}
					catch (System.IndexOutOfRangeException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
					while (size > 0)
					{
						ImageStructure thb1 = mDTBListReceived[size - 1];
						sbyte[] decodedstream;
						try
						{
							decodedstream = Base64.decode(thb1.mData, Base64.NO_WRAP);
						}
						catch (System.ArgumentException e)
						{
							Log.e(TAG, "handleReceivedThumbnails: mData " + thb1.mData);
							Log.e(TAG, "handleReceivedThumbnails decode failed " + e.Message);
							return false;
						}
						Bitmap bitmap = BitmapFactory.decodeByteArray(decodedstream, 0, decodedstream.Length);
						if (bitmap != null)
						{
							switch (size)
							{
								case 3:
								{
									mTb1.ImageBitmap = bitmap;
									mIndextb1 = int.Parse(thb1.mId);
									mTxt1.Text = thb1.mDisplayname;
								}
									break;
								case 2:
								{
									mTb2.ImageBitmap = bitmap;
									mIndextb2 = int.Parse(thb1.mId);
									mTxt2.Text = thb1.mDisplayname;
								}
									break;
								case 1:
								{
									mTb3.ImageBitmap = bitmap;
									mIndextb3 = int.Parse(thb1.mId);
									mTxt3.Text = thb1.mDisplayname;
								}
									break;
							}
						}
						size--;
					}
				}
				return true;
			}
		}

		internal class ImageReceivedHandlerCallback : Handler.Callback
		{
			private readonly GalleryConsumerActivity outerInstance;

			public ImageReceivedHandlerCallback(GalleryConsumerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool handleMessage(Message msg)
			{
				if (msg.what == MSG_THUMBNAIL_RECEIVED)
				{
					outerInstance.clearThumbnails();
					outerInstance.handleReceivedThumbnails();
				}
				else if (msg.what == MSG_IMAGE_RECEIVED)
				{
					outerInstance.handleRecievedImage();
				}
				return true;
			}
		}
	}

}