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

namespace com.samsung.android.sdk.accessory.example.security.consumer
{


	// import com.samsung.android.sdk.accessory.example.securityaccessory.consumer.R;

	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using Bundle = android.os.Bundle;
	using IBinder = android.os.IBinder;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	public class ConsumerActivity : Activity
	{
		private static TextView mTextView;
		private static MessageAdapter mMessageAdapter;
		private const string sEncryptedData = "Secured timestamp";
		private bool mIsBound = false;
		private ListView mMessageListView;
		private ConsumerService mConsumerService = null;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;
			mTextView = (TextView) findViewById(R.id.tvStatus);
			mMessageListView = (ListView) findViewById(R.id.lvMessage);
			mMessageAdapter = new MessageAdapter(this);
			mMessageListView.Adapter = mMessageAdapter;
			// Bind service
			mIsBound = bindService(new Intent(ConsumerActivity.this, typeof(ConsumerService)), mConnection, Context.BIND_AUTO_CREATE);
		}

		protected internal override void onDestroy()
		{
			// Clean up connections
			if (mIsBound == true && mConsumerService != null)
			{
				if (mConsumerService.closeConnection() == false)
				{
					updateTextView("Disconnected");
					Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
					mMessageAdapter.clear();
				}
			}
			// Un-bind service
			if (mIsBound)
			{
				unbindService(mConnection);
				mIsBound = false;
			}
			base.onDestroy();
		}

		public virtual void mOnClick(View v)
		{
			switch (v.Id)
			{
				case R.id.buttonConnect:
				{
					if (mIsBound == true && mConsumerService != null)
					{
						mConsumerService.findPeers();
					}
					break;
				}
				case R.id.buttonDisconnect:
				{
					if (mIsBound == true && mConsumerService != null)
					{
						if (mConsumerService.closeConnection() == false)
						{
							updateTextView("Disconnected");
							Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
							mMessageAdapter.clear();
						}
					}
					break;
				}
				case R.id.buttonSend:
				{
					if (mIsBound == true && mConsumerService != null)
					{
						if (mConsumerService.sendData(sEncryptedData))
						{
						}
						else
						{
							Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
						}
					}
					break;
				}
				default:
			break;
			}
		}

		private readonly ServiceConnection mConnection = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}

			public override void onServiceConnected(ComponentName className, IBinder service)
			{
				outerInstance.mConsumerService = ((ConsumerService.LocalBinder) service).Service;
				updateTextView("onServiceConnected");
			}

			public override void onServiceDisconnected(ComponentName className)
			{
				outerInstance.mConsumerService = null;
				outerInstance.mIsBound = false;
				updateTextView("onServiceDisconnected");
			}
		}

		public static void addMessage(string data)
		{
			mMessageAdapter.addMessage(new Message(data));
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void updateTextView(final String str)
		public static void updateTextView(string str)
		{
			mTextView.Text = str;
		}

		private class MessageAdapter : BaseAdapter
		{
			private readonly ConsumerActivity outerInstance;

			internal const int MAX_MESSAGES_TO_DISPLAY = 20;
			internal IList<Message> mMessages;

			public MessageAdapter(ConsumerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
				mMessages = Collections.synchronizedList(new List<Message>());
			}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: void addMessage(final Message msg)
			internal virtual void addMessage(Message msg)
			{
				runOnUiThread(() =>
				{
					if (mMessages.Count == MAX_MESSAGES_TO_DISPLAY)
					{
						mMessages.RemoveAt(0);
						mMessages.Add(msg);
					}
					else
					{
						mMessages.Add(msg);
					}
					notifyDataSetChanged();
					outerInstance.mMessageListView.Selection = Count - 1;
				});
			}

			internal virtual void clear()
			{
				runOnUiThread(() =>
				{
					mMessages.Clear();
					notifyDataSetChanged();
				});
			}

			public override int Count
			{
				get
				{
					return mMessages.Count;
				}
			}

			public override object getItem(int position)
			{
				return mMessages[position];
			}

			public override long getItemId(int position)
			{
				return 0;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				LayoutInflater inflator = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
				View messageRecordView = null;
				if (inflator != null)
				{
					messageRecordView = inflator.inflate(R.layout.message, null);
					TextView tvData = (TextView) messageRecordView.findViewById(R.id.tvData);
					Message message = (Message) getItem(position);
					tvData.Text = message.data;
				}
				return messageRecordView;
			}
		}

		private sealed class Message
		{
			internal string data;

			public Message(string data) : base()
			{
				this.data = data;
			}
		}
	}

}