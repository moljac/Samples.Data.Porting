namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Bitmap = android.graphics.Bitmap;
	using BitmapDrawable = android.graphics.drawable.BitmapDrawable;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using ImageView = android.widget.ImageView;

	using SlookWritingBuddy = com.samsung.android.sdk.look.writingbuddy.SlookWritingBuddy;

	public class WritingBuddyEditTextActivity : Activity
	{

		private SlookWritingBuddy mWritingBuddy;

		private ImageView mOutputImageView;

		/* State */
		private bool mIsEnabled = true;

		private bool mEnableImage = false;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_writingbuddy_edittext;

			EditText editInput = (EditText) findViewById(R.id.edit_input);
			mOutputImageView = (ImageView) findViewById(R.id.image_output);

			mWritingBuddy = new SlookWritingBuddy(editInput);

			Button enableButton = (Button) findViewById(R.id.btn_enable);
			enableButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			Button modeButton = (Button) findViewById(R.id.btn_enable_image);
			modeButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly WritingBuddyEditTextActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(WritingBuddyEditTextActivity outerInstance)
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
			private readonly WritingBuddyEditTextActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(WritingBuddyEditTextActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				if (outerInstance.mEnableImage)
				{
					outerInstance.mEnableImage = false;
					outerInstance.mWritingBuddy.ImageWritingListener = null;
					((Button) v).Text = "Enable Image Listener";
				}
				else
				{
					outerInstance.mEnableImage = true;
					outerInstance.mWritingBuddy.ImageWritingListener = new ImageWritingListenerAnonymousInnerClassHelper(this);
					((Button) v).Text = "Disable Image Listener";
				}
			}

			private class ImageWritingListenerAnonymousInnerClassHelper : SlookWritingBuddy.ImageWritingListener
			{
				private readonly OnClickListenerAnonymousInnerClassHelper2 outerInstance;

				public ImageWritingListenerAnonymousInnerClassHelper(OnClickListenerAnonymousInnerClassHelper2 outerInstance)
				{
					this.outerInstance = outerInstance;
				}

				public virtual void onImageReceived(Bitmap arg0)
				{
					outerInstance.outerInstance.mOutputImageView.Background = new BitmapDrawable(Resources, arg0);
				}
			}
		}
	}

}