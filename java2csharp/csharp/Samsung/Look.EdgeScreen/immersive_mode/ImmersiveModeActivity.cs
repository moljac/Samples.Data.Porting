namespace com.example.edgescreen.immersive_mode
{

	using Bundle = android.os.Bundle;
	using Activity = android.app.Activity;
	using Log = android.util.Log;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;

	using OnClickListener = android.view.View.OnClickListener;
	using android.view.animation;
	using ImageView = android.widget.ImageView;
	using TextView = android.widget.TextView;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Slook = com.samsung.android.sdk.look.Slook;
	using SlookCocktailSubWindow = com.samsung.android.sdk.look.cocktailbar.SlookCocktailSubWindow;

	public class ImmersiveModeActivity : Activity
	{

		private const string TAG = "ImmersiveModeActivity";
		private TextView mainText;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			Slook slook = new Slook();

			try
			{
				slook.initialize(this);
			}
			catch (SsdkUnsupportedException)
			{
				return;
			}

			if (slook.isFeatureEnabled(Slook.COCKTAIL_BAR))
			{
				// If device supports cocktail bar, you can set up the sub-window.
				ContentView = R.layout.activity_immersive_mode;
				SlookCocktailSubWindow.setSubContentView(this, R.layout.sub_view);
			}
			else
			{
				// Normal device
				ContentView = R.layout.activity_main;
			}

			mainText = (TextView)findViewById(R.id.main_text);
			ViewGroup subView = (ViewGroup)findViewById(R.id.sub_view_layout);

			if (subView != null)
			{
				int childCount = subView.ChildCount;
				for (int i = 0; i < childCount; i++)
				{
					if (subView.getChildAt(i) is ImageView)
					{
						subView.getChildAt(i).OnClickListener = mSubViewOnClickListener;
					}
				}
			}
		}

		internal View.OnClickListener mSubViewOnClickListener = new OnClickListenerAnonymousInnerClassHelper();

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			public OnClickListenerAnonymousInnerClassHelper()
			{
			}


			public override void onClick(View arg0)
			{
				playClickAnim(arg0);
				switch (arg0.Id)
				{
					case R.id.img01:
						Log.i(TAG, "clicked iamge icon 01.");
						outerInstance.mainText.Text = "Selected pencil icon.";
						break;
					case R.id.img02:
						Log.i(TAG, "clicked iamge icon 02.");
						outerInstance.mainText.Text = "Selected pen icon.";
						break;
					case R.id.img03:
						Log.i(TAG, "clicked iamge icon 03.");
						outerInstance.mainText.Text = "Selected high lighter icon.";
						break;
					case R.id.img04:
						Log.i(TAG, "clicked iamge icon 04.");
						outerInstance.mainText.Text = "Selected calligraphy brush icon.";
						break;
					case R.id.img05:
						Log.i(TAG, "clicked iamge icon 05.");
						outerInstance.mainText.Text = "Selected brush icon.";
						break;
					case R.id.img06:
						Log.i(TAG, "clicked iamge icon 06.");
						outerInstance.mainText.Text = "Selected marker icon.";
						break;
				}
			}
		}

		public static void playClickAnim(View targetView)
		{
			float SCALE_FACTOR = 0.3f;
			AnimationSet animSet = new AnimationSet(true);
			TranslateAnimation translateAnim = new TranslateAnimation(Animation.RELATIVE_TO_SELF, 0, Animation.RELATIVE_TO_SELF, -SCALE_FACTOR / 2, Animation.RELATIVE_TO_SELF, 0f, Animation.RELATIVE_TO_SELF, -SCALE_FACTOR / 2);
			animSet.addAnimation(translateAnim);
			ScaleAnimation anim = new ScaleAnimation(1f, 1f + SCALE_FACTOR, 1f, 1f + SCALE_FACTOR);

			animSet.addAnimation(anim);
			animSet.Interpolator = new AccelerateDecelerateInterpolator();
			animSet.Duration = 150;
			targetView.startAnimation(animSet);

		}

	}

}