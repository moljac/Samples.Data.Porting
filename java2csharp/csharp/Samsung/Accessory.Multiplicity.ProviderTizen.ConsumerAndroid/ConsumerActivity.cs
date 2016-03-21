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

namespace com.samsung.android.sdk.accessory.example.multiplicity.consumer
{


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
	using OnClickListener = android.view.View.OnClickListener;
	using BaseAdapter = android.widget.BaseAdapter;
	using CheckBox = android.widget.CheckBox;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	public class ConsumerActivity : Activity
	{
		private static TextView mTextView;
		private static TextView mTextView2;
		private static MessageAdapter mMessageAdapter;
		private static MessageAdapter2 mMessageAdapter2;
		private ListView mMessageListView;
		private ListView mMessageListView2;
		private bool mIsBound = false;
		private bool mIsBound2 = false;
		private ConsumerService1 mConsumerService1 = null;
		private ConsumerService2 mConsumerService2 = null;
		private CheckBox chkP1, chkP2;
		private bool mProvider1 = false;
		private bool mProvider2 = false;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;
			mTextView = (TextView) findViewById(R.id.tvStatus);
			mMessageListView = (ListView) findViewById(R.id.lvMessage);
			mMessageAdapter = new MessageAdapter(this);
			mMessageListView.Adapter = mMessageAdapter;
			chkP1 = (CheckBox) findViewById(R.id.checkBox1);
			chkP1.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			mTextView2 = (TextView) findViewById(R.id.tvStatus2);
			mMessageListView2 = (ListView) findViewById(R.id.lvMessage2);
			mMessageAdapter2 = new MessageAdapter2(this);
			mMessageListView2.Adapter = mMessageAdapter2;
			chkP2 = (CheckBox) findViewById(R.id.checkBox2);
			chkP2.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
			// Bind service
			mIsBound = bindService(new Intent(ConsumerActivity.this, typeof(ConsumerService1)), mConnection1, Context.BIND_AUTO_CREATE);
			mIsBound2 = bindService(new Intent(ConsumerActivity.this, typeof(ConsumerService2)), mConnection2, Context.BIND_AUTO_CREATE);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly ConsumerActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(ConsumerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.mProvider1 = ((CheckBox) v).Checked ? true : false;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly ConsumerActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(ConsumerActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.mProvider2 = ((CheckBox) v).Checked ? true : false;
			}
		}

		protected internal override void onDestroy()
		{
			// Clean up connections
			if (mIsBound == true && mConsumerService1 != null && mConsumerService2 != null)
			{
				if (mConsumerService1.closeConnection() && mConsumerService2.closeConnection() == false)
				{
					updateTextView("Disconnected");
					updateTextView2("Disconnected");
	//                Toast.makeText(getApplicationContext(), R.string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
					mMessageAdapter.clear();
					mMessageAdapter2.clear();
				}
			}
			// Un-bind service
			if (mIsBound)
			{
				unbindService(mConnection1);
				mIsBound = false;
			}
			if (mIsBound2)
			{
				unbindService(mConnection2);
				mIsBound2 = false;
			}
			base.onDestroy();
		}

		public virtual void mOnClick(View v)
		{
			switch (v.Id)
			{
				case R.id.buttonConnect:
				{
					if (mProvider1 == true)
					{
						if (mConsumerService1 != null && mIsBound == true)
						{
							mConsumerService1.findPeers();
						}
					}
					if (mProvider2 == true)
					{
						if (mConsumerService2 != null && mIsBound2 == true)
						{
							mConsumerService2.findPeers();
						}
					}
					break;
				}
				// case R.id.buttonConnect2: {
				// if (mIsBound == true && mConsumerService2 != null) {
				// mConsumerService2.findPeers();
				// }
				// break;
				// }
				// case R.id.buttonDisconnect1: {
				// if (mIsBound == true && mConsumerService1 != null) {
				// if (mConsumerService1.closeConnection() == false) {
				// updateTextView("Disconnected");
				// Toast.makeText(getApplicationContext(), R.string.ConnectionAlreadyDisconnected,
				// Toast.LENGTH_LONG).show();
				// mMessageAdapter.clear();
				// }
				// }
				// break;
				// }
				case R.id.buttonDisconnect:
				{
					if (mProvider1 == true)
					{
						if (mConsumerService1 != null && mIsBound == true)
						{
							if (mConsumerService1.closeConnection() == false)
							{
								updateTextView("Disconnected");
								Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
								mMessageAdapter.clear();
							}
						}
					}
					if (mProvider2 == true)
					{
						if (mConsumerService2 != null && mIsBound2 == true)
						{
							if (mConsumerService2.closeConnection() == false)
							{
								updateTextView2("Disconnected");
								Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected2, Toast.LENGTH_LONG).show();
								mMessageAdapter2.clear();
							}
						}
					}
					break;
				}
				case R.id.buttonSend:
				{
					if (mProvider1 == true)
					{
						if (mConsumerService1 != null && mIsBound == true)
						{
							if (mConsumerService1.sendData("Hello Accessory!"))
							{
							}
							else
							{
								Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
							}
						}
						else
						{
							Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected, Toast.LENGTH_LONG).show();
						}
					}
					if (mProvider2 == true)
					{
						if (mConsumerService2 != null && mIsBound2 == true)
						{
							if (mConsumerService2.sendData("Hello Accessory!"))
							{
							}
							else
							{
								Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected2, Toast.LENGTH_LONG).show();
							}
						}
						else
						{
							Toast.makeText(ApplicationContext, R.@string.ConnectionAlreadyDisconnected2, Toast.LENGTH_LONG).show();
						}
					}
					break;
				}
				// case R.id.buttonSend2: {
				// if (mIsBound == true && mConsumerService2 != null) {
				// if (mConsumerService2.sendData("Hello Accessory!")) {
				// } else {
				// Toast.makeText(getApplicationContext(), R.string.ConnectionAlreadyDisconnected,
				// Toast.LENGTH_LONG).show();
				// }
				// }
				// break;
				// }
				default:
			break;
			}
		}

		private readonly ServiceConnection mConnection1 = new ServiceConnectionAnonymousInnerClassHelper();

		private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper()
			{
			}

			public override void onServiceConnected(ComponentName className, IBinder service)
			{
				outerInstance.mConsumerService1 = ((ConsumerService1.LocalBinder) service).Service;
				updateTextView("onServiceConnected");
			}

			public override void onServiceDisconnected(ComponentName className)
			{
				outerInstance.mConsumerService1 = null;
				outerInstance.mIsBound = false;
				updateTextView("onServiceDisconnected");
			}
		}
		private readonly ServiceConnection mConnection2 = new ServiceConnectionAnonymousInnerClassHelper2();

		private class ServiceConnectionAnonymousInnerClassHelper2 : ServiceConnection
		{
			public ServiceConnectionAnonymousInnerClassHelper2()
			{
			}

			public override void onServiceConnected(ComponentName className, IBinder service)
			{
				outerInstance.mConsumerService2 = ((ConsumerService2.LocalBinder) service).Service;
				updateTextView2("onServiceConnected");
			}

			public override void onServiceDisconnected(ComponentName className)
			{
				outerInstance.mConsumerService2 = null;
				outerInstance.mIsBound2 = false;
				updateTextView2("onServiceDisconnected");
			}
		}

		public static void addMessage(string data)
		{
			mMessageAdapter.addMessage(new Message(data));
		}

		public static void addMessage2(string data)
		{
			mMessageAdapter2.addMessage(new Message(data));
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void updateTextView(final String str)
		public static void updateTextView(string str)
		{
			mTextView.Text = str;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void updateTextView2(final String str)
		public static void updateTextView2(string str)
		{
			mTextView2.Text = str;
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

		private class MessageAdapter2 : BaseAdapter
		{
			private readonly ConsumerActivity outerInstance;

			internal const int MAX_MESSAGES_TO_DISPLAY = 20;
			internal IList<Message> mMessages;

			public MessageAdapter2(ConsumerActivity outerInstance)
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
					outerInstance.mMessageListView2.Selection = Count - 1;
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