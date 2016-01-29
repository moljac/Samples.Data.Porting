namespace com.opentok.android.demo.ui.fragments
{

	using Activity = android.app.Activity;
	using Fragment = android.app.Fragment;
	using Configuration = android.content.res.Configuration;
	using Bundle = android.os.Bundle;
	using DisplayMetrics = android.util.DisplayMetrics;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using AlphaAnimation = android.view.animation.AlphaAnimation;
	using ImageButton = android.widget.ImageButton;
	using RelativeLayout = android.widget.RelativeLayout;
	using TextView = android.widget.TextView;

	using UIActivity = com.opentok.android.demo.opentoksamples.UIActivity;

	public class SubscriberControlFragment : Fragment, View.OnClickListener
	{

		private const string LOGTAG = "sub-control-fragment";

		private bool mSubscriberWidgetVisible = false;
		private ImageButton mSubscriberMute;
		private TextView mSubscriberName;
		private RelativeLayout mSubContainer;

		// Animation constants
		private const int ANIMATION_DURATION = 500;
		private const int SUBSCRIBER_CONTROLS_DURATION = 7000;

		private SubscriberCallbacks mCallbacks = sOpenTokCallbacks;
		private UIActivity openTokActivity;

		public interface SubscriberCallbacks
		{
			void onMuteSubscriber();
		}

		private static SubscriberCallbacks sOpenTokCallbacks = new SubscriberCallbacksAnonymousInnerClassHelper();

		private class SubscriberCallbacksAnonymousInnerClassHelper : SubscriberCallbacks
		{
			public SubscriberCallbacksAnonymousInnerClassHelper()
			{
			}


			public virtual void onMuteSubscriber()
			{
			}

		}

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);
			Log.i(LOGTAG, "On attach Subscriber control fragment");
			openTokActivity = (UIActivity) activity;
			if (!(activity is SubscriberCallbacks))
			{
				throw new System.InvalidOperationException("Activity must implement fragment's callback");
			}

			mCallbacks = (SubscriberCallbacks) activity;

		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			View rootView = inflater.inflate(com.opentok.android.demo.opentoksamples.R.layout.layout_fragment_sub_control, container, false);

			mSubContainer = (RelativeLayout) openTokActivity.findViewById(com.opentok.android.demo.opentoksamples.R.id.fragment_sub_container);

			showSubscriberWidget(mSubscriberWidgetVisible, false);

			mSubscriberMute = (ImageButton) rootView.findViewById(com.opentok.android.demo.opentoksamples.R.id.muteSubscriber);
			mSubscriberMute.OnClickListener = this;

			mSubscriberName = (TextView) rootView.findViewById(com.opentok.android.demo.opentoksamples.R.id.subscriberName);

			if (openTokActivity.Resources.Configuration.orientation == Configuration.ORIENTATION_LANDSCAPE)
			{
				RelativeLayout.LayoutParams @params = (RelativeLayout.LayoutParams) container.LayoutParams;

				DisplayMetrics metrics = new DisplayMetrics();
				openTokActivity.WindowManager.DefaultDisplay.getMetrics(metrics);

				@params.width = metrics.widthPixels - openTokActivity.dpToPx(48);
				container.LayoutParams = @params;
			}

			return rootView;
		}

		public override void onDetach()
		{
			base.onDetach();

			Log.i(LOGTAG, "On detach Subscriber control fragment");
			mCallbacks = sOpenTokCallbacks;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
				case com.opentok.android.demo.opentoksamples.R.id.muteSubscriber:
					muteSubscriber();
					break;
			}
		}

		private Runnable mSubscriberWidgetTimerTask = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{
				outerInstance.showSubscriberWidget(false);
			}
		}

		public virtual void showSubscriberWidget(bool show)
		{
			showSubscriberWidget(show, true);
		}

		private void showSubscriberWidget(bool show, bool animate)
		{
			if (mSubContainer != null)
			{
				mSubContainer.clearAnimation();
				mSubscriberWidgetVisible = show;
				float dest = show ? 1.0f : 0.0f;
				AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
				aa.Duration = animate ? ANIMATION_DURATION : 1;
				aa.FillAfter = true;
				mSubContainer.startAnimation(aa);

				if (show)
				{
					if (mSubscriberMute != null)
					{
						mSubscriberMute.Clickable = true;
					}

					mSubContainer.Visibility = View.VISIBLE;

				}
				else
				{
					if (mSubscriberMute != null)
					{
						mSubscriberMute.Clickable = false;
					}

					mSubContainer.Visibility = View.GONE;
				}
			}
		}

		public virtual void subscriberClick()
		{
			if (!mSubscriberWidgetVisible)
			{
				showSubscriberWidget(true);
			}
			else
			{
				showSubscriberWidget(false);
			}

			initSubscriberUI();
		}

		public virtual void muteSubscriber()
		{
			mCallbacks.onMuteSubscriber();

			mSubscriberMute.ImageResource = openTokActivity.Subscriber.SubscribeToAudio ? com.opentok.android.demo.opentoksamples.R.drawable.unmute_sub : com.opentok.android.demo.opentoksamples.R.drawable.mute_sub;
		}

		public virtual void initSubscriberUI()
		{
			if (openTokActivity != null)
			{
				openTokActivity.Handler.removeCallbacks(mSubscriberWidgetTimerTask);
				openTokActivity.Handler.postDelayed(mSubscriberWidgetTimerTask, SUBSCRIBER_CONTROLS_DURATION);
				mSubscriberName.Text = openTokActivity.Subscriber.Stream.Name;
				mSubscriberMute.ImageResource = openTokActivity.Subscriber.SubscribeToAudio ? com.opentok.android.demo.opentoksamples.R.drawable.unmute_sub : com.opentok.android.demo.opentoksamples.R.drawable.mute_sub;
			}
		}

	}

}