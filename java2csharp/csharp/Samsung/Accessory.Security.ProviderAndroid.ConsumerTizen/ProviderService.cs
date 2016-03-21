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

namespace com.samsung.android.sdk.accessory.example.security.provider
{


	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using PackageInfo = android.content.pm.PackageInfo;
	using PackageManager = android.content.pm.PackageManager;
	using Signature = android.content.pm.Signature;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Binder = android.os.Binder;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using Toast = android.widget.Toast;
	using Log = android.util.Log;

	using com.samsung.android.sdk.accessory;

	public class ProviderService : SAAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mBinder = new LocalBinder(this);
		}

		private const string TAG = "SecuredProvider";
		private const int SECURED_CHANNEL_ID = 1111;
		private static readonly Type<ServiceConnection> SASOCKET_CLASS = typeof(ServiceConnection);
		private IBinder mBinder;
		private ServiceConnection mConnectionHandler = null;
		private Context mContext = null;
		internal Handler mHandler = new Handler();

		public ProviderService() : base(TAG, SASOCKET_CLASS)
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
			Log.d(TAG, "onFindPeerAgentResponse : result =" + result);
		}

		protected internal override void onServiceConnectionRequested(SAPeerAgent peerAgent)
		{
			if (peerAgent != null)
			{
				// Toast.makeText(getBaseContext(), R.string.ConnectionAcceptedMsg, Toast.LENGTH_SHORT).show();
				authenticatePeerAgent(peerAgent);
			}
		}

		protected internal override void onServiceConnectionResponse(SAPeerAgent peerAgent, SASocket socket, int result)
		{
			if (result == SAAgent.CONNECTION_SUCCESS)
			{
				if (socket != null)
				{
					mConnectionHandler = (ServiceConnection) socket;
				}
			}
			else if (result == SAAgent.CONNECTION_ALREADY_EXIST)
			{
				Log.e(TAG, "onServiceConnectionResponse, CONNECTION_ALREADY_EXIST");
			}
		}

		protected internal override void onAuthenticationResponse(SAPeerAgent peerAgent, SAAuthenticationToken authToken, int code)
		{
			if (authToken.AuthenticationType == SAAuthenticationToken.AUTHENTICATION_TYPE_CERTIFICATE_X509)
			{
				mContext = ApplicationContext;
				sbyte[] myAppKey = getApplicationCertificate(mContext);
				if (authToken.Key != null)
				{
					bool matched = true;
					if (authToken.Key.length != myAppKey.Length)
					{
						matched = false;
					}
					else
					{
						for (int i = 0; i < authToken.Key.length; i++)
						{
							if (authToken.Key[i] != myAppKey[i])
							{
								matched = false;
							}
						}
					}
					if (matched)
					{
						Log.d(TAG, "onAuthenticationResponse : authentication is matched");
						acceptServiceConnectionRequest(peerAgent);
						mHandler.post(() =>
						{
							Toast.makeText(BaseContext, R.@string.Aunthenticated, Toast.LENGTH_SHORT).show();
						});
					}
				}
			}
			else if (authToken.AuthenticationType == SAAuthenticationToken.AUTHENTICATION_TYPE_NONE)
			{
				Log.e(TAG, "onAuthenticationResponse : CERT_TYPE(NONE)");
			}
		}

		protected internal override void onError(SAPeerAgent peerAgent, string errorMessage, int errorCode)
		{
			base.onError(peerAgent, errorMessage, errorCode);
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

		private static sbyte[] getApplicationCertificate(Context context)
		{
			if (context == null)
			{
				return null;
			}
			sbyte[] cert = null;
			string packageName = context.PackageName;
			if (context != null)
			{
				try
				{
					PackageInfo pkgInfo = context.PackageManager.getPackageInfo(packageName, PackageManager.GET_SIGNATURES);
					if (pkgInfo == null)
					{
						return null;
					}
					Signature[] sigs = pkgInfo.signatures;
					if (sigs == null)
					{
					}
					else
					{
						CertificateFactory cf = CertificateFactory.getInstance("X.509");
						ByteArrayInputStream stream = new ByteArrayInputStream(sigs[0].toByteArray());
						X509Certificate x509cert = X509Certificate.getInstance(stream);
						cert = x509cert.PublicKey.Encoded;
					}
				}
				catch (PackageManager.NameNotFoundException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				catch (CertificateException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				catch (javax.security.cert.CertificateException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
			return cert;
		}

		public class LocalBinder : Binder
		{
			private readonly ProviderService outerInstance;

			public LocalBinder(ProviderService outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual ProviderService Service
			{
				get
				{
					return outerInstance;
				}
			}
		}

		public class ServiceConnection : SASocket
		{
			private readonly ProviderService outerInstance;

			public ServiceConnection(ProviderService outerInstance) : base(typeof(ServiceConnection).FullName)
			{
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
				this.outerInstance = outerInstance;
			}

			public override void onError(int channelId, string errorMessage, int errorCode)
			{
			}

			public override void onReceive(int channelId, sbyte[] data)
			{
				if (outerInstance.mConnectionHandler == null)
				{
					return;
				}
				DateTime calendar = new GregorianCalendar();
				SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy.MM.dd aa hh:mm:ss.SSS");
				string timeStr = " " + dateFormat.format(calendar);
				string strToUpdateUI = StringHelperClass.NewString(data);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String message = strToUpdateUI.concat(timeStr);
				string message = strToUpdateUI + timeStr;
				new Thread(() =>
				{
					try
					{
						outerInstance.mConnectionHandler.secureSend(SECURED_CHANNEL_ID, message.GetBytes());
					}
					catch (IOException e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
					}
				}).start();
			}

			protected internal override void onServiceConnectionLost(int reason)
			{
				outerInstance.mConnectionHandler = null;
				outerInstance.mHandler.post(() =>
				{
					Toast.makeText(BaseContext, R.@string.ConnectionTerminatedMsg, Toast.LENGTH_SHORT).show();
				});
			}
		}
	}

}