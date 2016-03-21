namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using ActivityNotFoundException = android.content.ActivityNotFoundException;
	using ComponentName = android.content.ComponentName;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	using SlookSmartClip = com.samsung.android.sdk.look.smartclip.SlookSmartClip;
	using SlookSmartClipMetaTag = com.samsung.android.sdk.look.smartclip.SlookSmartClipMetaTag;

	public class SmartClipActivity : Activity
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_smartclip;

			TextView tv = (TextView) findViewById(R.id.text_static);

			SlookSmartClip sc = new SlookSmartClip(tv);
			sc.clearAllMetaTag();
			sc.addMetaTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_URL, "http://www.samsung.com"));
			sc.addMetaTag(new SlookSmartClipMetaTag(SlookSmartClipMetaTag.TAG_TYPE_PLAIN_TEXT, "This is android textview."));

			Button gotoPinAll = (Button) findViewById(R.id.gotopinboard);
			gotoPinAll.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SmartClipActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(SmartClipActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				Intent intent = new Intent();
				intent.Component = new ComponentName("com.samsung.android.app.pinboard", "com.samsung.android.app.pinboard.ui.PinboardActivity");
				try
				{
					startActivity(intent);
				}
				catch (ActivityNotFoundException)
				{
					Toast.makeText(outerInstance, "Pinboard application is not installed.", Toast.LENGTH_SHORT).show();
				}
			}
		}

	}

}