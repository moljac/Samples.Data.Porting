namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;

	using SlookPointerIcon = com.samsung.android.sdk.look.SlookPointerIcon;

	public class PointerIconActivity : Activity
	{

		private bool mIsDefault = false;

		private TextView mHoverTextView;

		private SlookPointerIcon mPointerIcon = new SlookPointerIcon();

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_pointericon;

			Button changeIcon = (Button) findViewById(R.id.btn_changeicon);

			mHoverTextView = (TextView) findViewById(R.id.text_hoverarea);
			mPointerIcon.setHoverIcon(mHoverTextView, Resources.getDrawable(R.drawable.pointericon));
			changeIcon.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly PointerIconActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(PointerIconActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onClick(View v)
			{
				if (outerInstance.mIsDefault)
				{
					outerInstance.mPointerIcon.setHoverIcon(outerInstance.mHoverTextView, Resources.getDrawable(R.drawable.pointericon));
					outerInstance.mIsDefault = false;
					((Button) v).Text = "Change default icon";
				}
				else
				{
					outerInstance.mPointerIcon.setHoverIcon(outerInstance.mHoverTextView, null);
					outerInstance.mIsDefault = true;
					((Button) v).Text = "Change special icon";
				}
			}
		}
	}

}