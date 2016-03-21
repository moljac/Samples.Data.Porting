using System.Collections.Generic;
using System.Text;

namespace com.sinch.android.rtc.sample.messaging
{

	using Message = com.sinch.android.rtc.messaging.Message;
	using MessageClient = com.sinch.android.rtc.messaging.MessageClient;
	using MessageClientListener = com.sinch.android.rtc.messaging.MessageClientListener;
	using MessageDeliveryInfo = com.sinch.android.rtc.messaging.MessageDeliveryInfo;
	using MessageFailureInfo = com.sinch.android.rtc.messaging.MessageFailureInfo;

	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ListView = android.widget.ListView;
	using Toast = android.widget.Toast;

	public class MessagingActivity : BaseActivity, MessageClientListener
	{

		private static readonly string TAG = typeof(MessagingActivity).Name;

		private MessageAdapter mMessageAdapter;
		private EditText mTxtRecipient;
		private EditText mTxtTextBody;
		private Button mBtnSend;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.messaging;

			mTxtRecipient = (EditText) findViewById(R.id.txtRecipient);
			mTxtTextBody = (EditText) findViewById(R.id.txtTextBody);

			mMessageAdapter = new MessageAdapter(this);
			ListView messagesList = (ListView) findViewById(R.id.lstMessages);
			messagesList.Adapter = mMessageAdapter;

			mBtnSend = (Button) findViewById(R.id.btnSend);
			mBtnSend.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MessagingActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MessagingActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.sendMessage();
			}
		}

		public override void onDestroy()
		{
			if (SinchServiceInterface != null)
			{
				SinchServiceInterface.removeMessageClientListener(this);
				SinchServiceInterface.stopClient();
			}
			base.onDestroy();
		}

		public override void onServiceConnected()
		{
			SinchServiceInterface.addMessageClientListener(this);
			ButtonEnabled = true;
		}

		public override void onServiceDisconnected()
		{
			ButtonEnabled = false;
		}

		private void sendMessage()
		{
			string recipient = mTxtRecipient.Text.ToString();
			string textBody = mTxtTextBody.Text.ToString();
			if (recipient.Length == 0)
			{
				Toast.makeText(this, "No recipient added", Toast.LENGTH_SHORT).show();
				return;
			}
			if (textBody.Length == 0)
			{
				Toast.makeText(this, "No text message", Toast.LENGTH_SHORT).show();
				return;
			}

			SinchServiceInterface.sendMessage(recipient, textBody);
			mTxtTextBody.Text = "";
		}

		private bool ButtonEnabled
		{
			set
			{
				mBtnSend.Enabled = value;
			}
		}

		public override void onIncomingMessage(MessageClient client, Message message)
		{
			mMessageAdapter.addMessage(message, MessageAdapter.DIRECTION_INCOMING);
		}

		public override void onMessageSent(MessageClient client, Message message, string recipientId)
		{
			mMessageAdapter.addMessage(message, MessageAdapter.DIRECTION_OUTGOING);
		}

		public override void onShouldSendPushData(MessageClient client, Message message, IList<PushPair> pushPairs)
		{
			// Left blank intentionally
		}

		public override void onMessageFailed(MessageClient client, Message message, MessageFailureInfo failureInfo)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append("Sending failed: ").Append(failureInfo.SinchError.Message);

			Toast.makeText(this, sb.ToString(), Toast.LENGTH_LONG).show();
			Log.d(TAG, sb.ToString());
		}

		public override void onMessageDelivered(MessageClient client, MessageDeliveryInfo deliveryInfo)
		{
			Log.d(TAG, "onDelivered");
		}
	}

}