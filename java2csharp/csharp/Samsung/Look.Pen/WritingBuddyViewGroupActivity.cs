namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using FrameLayout = android.widget.FrameLayout;
	using TextView = android.widget.TextView;

	using SlookWritingBuddy = com.samsung.android.sdk.look.writingbuddy.SlookWritingBuddy;

	public class WritingBuddyViewGroupActivity : Activity
	{

		private SlookWritingBuddy mWritingBuddy;

		private TextView mOutputTextView;

		/* State */
		private bool mIsEnabled = true;

		private int mType = SlookWritingBuddy.TYPE_EDITOR_TEXT;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_writingbuddy_viewgroup;

			FrameLayout fl = (FrameLayout) findViewById(R.id.input);
			mOutputTextView = (TextView) findViewById(R.id.text_output);

			mWritingBuddy = new SlookWritingBuddy(fl);

			mWritingBuddy.TextWritingListener = new TextWritingListenerAnonymousInnerClassHelper(this);

			Button enableButton = (Button) findViewById(R.id.btn_enable);
			enableButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			Button typeButton = (Button) findViewById(R.id.btn_type);
			typeButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class TextWritingListenerAnonymousInnerClassHelper : SlookWritingBuddy.TextWritingListener
		{
			private readonly WritingBuddyViewGroupActivity outerInstance;

			public TextWritingListenerAnonymousInnerClassHelper(WritingBuddyViewGroupActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onTextReceived(CharSequence arg0)
			{
				outerInstance.mOutputTextView.Text = arg0;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly WritingBuddyViewGroupActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(WritingBuddyViewGroupActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				if (outerInstance.mIsEnabled)
				{
					((Button) v).Text = "Enable";
					outerInstance.mIsEnabled = false;
				}
				else
				{
					((Button) v).Text = "Disable";
					outerInstance.mIsEnabled = true;
				}
				outerInstance.mWritingBuddy.Enabled = outerInstance.mIsEnabled;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly WritingBuddyViewGroupActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(WritingBuddyViewGroupActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				if (outerInstance.mType == SlookWritingBuddy.TYPE_EDITOR_TEXT)
				{
					outerInstance.mType = SlookWritingBuddy.TYPE_EDITOR_NUMBER;
					((Button) v).Text = "String+Number";
				}
				else
				{
					outerInstance.mType = SlookWritingBuddy.TYPE_EDITOR_TEXT;
					((Button) v).Text = "Number";
				}
				outerInstance.mWritingBuddy.EditorType = outerInstance.mType;
			}
		}
	}

}