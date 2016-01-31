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

	using R = com.opentok.android.demo.opentoksamples.R;
	using UIActivity = com.opentok.android.demo.opentoksamples.UIActivity;
	using PublisherCallbacks = com.opentok.android.demo.ui.fragments.PublisherControlFragment.PublisherCallbacks;

	public class PublisherStatusFragment : Fragment
	{

		private const string LOGTAG = "pub-status-fragment";
		private const int ANIMATION_DURATION = 500;
		private const int STATUS_ANIMATION_DURATION = 7000;

		private ImageButton archiving;
		private TextView statusText;
		private UIActivity openTokActivity;
		private bool mPubStatusWidgetVisible = false;

		private bool archivingOn = false;

		private RelativeLayout mPubStatusContainer;

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			Log.i(LOGTAG, "On attach Publisher status fragment");
			openTokActivity = (UIActivity) activity;
			if (!(activity is PublisherCallbacks))
			{
				throw new System.InvalidOperationException("Activity must implement fragment's callback");
			}
		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.inflate(R.layout.layout_fragment_pub_status, container, false);

			mPubStatusContainer = (RelativeLayout) openTokActivity.findViewById(R.id.fragment_pub_status_container);
			archiving = (ImageButton) rootView.findViewById(R.id.archiving);

			statusText = (TextView) rootView.findViewById(R.id.statusLabel);

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
			Log.i(LOGTAG, "On detach Publisher status fragment");
		}

		private Runnable mPubStatusWidgetTimerTask = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{
				outerInstance.showPubStatusWidget(false);
				outerInstance.openTokActivity.setPubViewMargins();
			}
		}

		public virtual void showPubStatusWidget(bool show)
		{
			showPubStatusWidget(show, true);
		}

		private void showPubStatusWidget(bool show, bool animate)
		{
			if (mPubStatusContainer != null)
			{
				mPubStatusContainer.clearAnimation();
				mPubStatusWidgetVisible = show;
				float dest = show ? 1.0f : 0.0f;
				AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
				aa.Duration = animate ? ANIMATION_DURATION : 1;
				aa.FillAfter = true;
				mPubStatusContainer.startAnimation(aa);

				if (show && archivingOn)
				{
					mPubStatusContainer.Visibility = View.VISIBLE;
				}
				else
				{
					mPubStatusContainer.Visibility = View.GONE;
				}
			}
		}

		public virtual void publisherClick()
		{
			if (!mPubStatusWidgetVisible)
			{
				showPubStatusWidget(true);
			}
			else
			{
				showPubStatusWidget(false);
			}

			initPubStatusUI();
		}

		public virtual void initPubStatusUI()
		{
			if (openTokActivity != null)
			{
				openTokActivity.Handler.removeCallbacks(mPubStatusWidgetTimerTask);
				openTokActivity.Handler.postDelayed(mPubStatusWidgetTimerTask, STATUS_ANIMATION_DURATION);
			}
		}

		public virtual void updateArchivingUI(bool archivingOn)
		{

			archiving = (ImageButton) openTokActivity.findViewById(R.id.archiving);
			this.archivingOn = archivingOn;
			if (archivingOn)
			{
				statusText.Text = R.@string.archivingOn;
				archiving.ImageResource = R.drawable.archiving_on;
				showPubStatusWidget(true);
				initPubStatusUI();
			}
			else
			{
				showPubStatusWidget(false);
			}
		}

		public virtual bool PubStatusWidgetVisible
		{
			get
			{
				return mPubStatusWidgetVisible;
			}
		}

		public virtual RelativeLayout PubStatusContainer
		{
			get
			{
				return mPubStatusContainer;
			}
		}

	}

}