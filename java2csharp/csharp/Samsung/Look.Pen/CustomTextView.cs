namespace com.samsung.android.example.slookdemos
{

	using Context = android.content.Context;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;
	using TextView = android.widget.TextView;

	using SlookSmartClip = com.samsung.android.sdk.look.smartclip.SlookSmartClip;
	using SlookSmartClipCroppedArea = com.samsung.android.sdk.look.smartclip.SlookSmartClipCroppedArea;
	using SlookSmartClipDataElement = com.samsung.android.sdk.look.smartclip.SlookSmartClipDataElement;
	using SlookSmartClipMetaTag = com.samsung.android.sdk.look.smartclip.SlookSmartClipMetaTag;

	public class CustomTextView : TextView
	{

		private SlookSmartClip mSmartClip;

		public CustomTextView(Context context) : base(context)
		{
			init();
		}

		public CustomTextView(Context context, AttributeSet attrs) : base(context, attrs)
		{
			init();
		}

		public CustomTextView(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			init();
		}

		internal virtual void init()
		{
			mSmartClip = new SlookSmartClip(this);

			mSmartClip.DataExtractionListener = new DataExtractionListenerAnonymousInnerClassHelper(this);
		}

		private class DataExtractionListenerAnonymousInnerClassHelper : SlookSmartClip.DataExtractionListener
		{
			private readonly CustomTextView outerInstance;

			public DataExtractionListenerAnonymousInnerClassHelper(CustomTextView outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual int onExtractSmartClipData(View view, SlookSmartClipDataElement resultElement, SlookSmartClipCroppedArea arg2)
			{

				resultElement.addTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_PLAIN_TEXT, "This is custom textview."));
				resultElement.addTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_APP_DEEP_LINK, "slookdemos://SmartClip"));
				return SlookSmartClip.DataExtractionListener.EXTRACTION_DEFAULT;
			}

		}

	}

}