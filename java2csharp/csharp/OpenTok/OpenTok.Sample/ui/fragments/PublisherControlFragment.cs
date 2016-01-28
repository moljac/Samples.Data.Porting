namespace com.opentok.android.demo.ui.fragments
{

	using Activity = android.app.Activity;
	using Fragment = android.app.Fragment;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using AlphaAnimation = android.view.animation.AlphaAnimation;
	using Button = android.widget.Button;
	using ImageButton = android.widget.ImageButton;
	using RelativeLayout = android.widget.RelativeLayout;

	using UIActivity = com.opentok.android.demo.opentoksamples.UIActivity;

	public class PublisherControlFragment : Fragment, View.OnClickListener
	{

		private const string LOGTAG = "pub-control-fragment";
		private const int ANIMATION_DURATION = 500;
		private const int PUBLISHER_CONTROLS_DURATION = 7000;

		private ImageButton mPublisherMute;
		private ImageButton mSwapCamera;
		private Button mEndCall;

		private PublisherCallbacks mCallbacks = sOpenTokCallbacks;
		private UIActivity openTokActivity;
		private bool mPubControlWidgetVisible = false;
		private RelativeLayout mPublisherContainer;

		public interface PublisherCallbacks
		{
			void onMutePublisher();

			void onSwapCamera();

			void onEndCall();
		}

		private static PublisherCallbacks sOpenTokCallbacks = new PublisherCallbacksAnonymousInnerClassHelper();

		private class PublisherCallbacksAnonymousInnerClassHelper : PublisherCallbacks
		{
			public PublisherCallbacksAnonymousInnerClassHelper()
			{
			}


			public virtual void onMutePublisher()
			{
			}

			public virtual void onSwapCamera()
			{
			}

			public virtual void onEndCall()
			{
			}
		}

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			Log.i(LOGTAG, "On attach Publisher control fragment");
			openTokActivity = (UIActivity) activity;
			if (!(activity is PublisherCallbacks))
			{
				throw new System.InvalidOperationException("Activity must implement fragment's callback");
			}

			mCallbacks = (PublisherCallbacks) activity;
		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			View rootView = inflater.inflate(com.opentok.android.demo.opentoksamples.R.layout.layout_fragment_pub_control, container, false);

			mPublisherContainer = (RelativeLayout) openTokActivity.findViewById(com.opentok.android.demo.opentoksamples.R.id.fragment_pub_container);

			mPublisherMute = (ImageButton) rootView.findViewById(com.opentok.android.demo.opentoksamples.R.id.mutePublisher);
			mPublisherMute.OnClickListener = this;

			mSwapCamera = (ImageButton) rootView.findViewById(com.opentok.android.demo.opentoksamples.R.id.swapCamera);
			mSwapCamera.OnClickListener = this;

			mEndCall = (Button) rootView.findViewById(com.opentok.android.demo.opentoksamples.R.id.endCall);
			mEndCall.OnClickListener = this;

			return rootView;
		}

		public override void onDetach()
		{
			base.onDetach();
			Log.i(LOGTAG, "On detach Publisher control fragment");
			mCallbacks = sOpenTokCallbacks;
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
				case com.opentok.android.demo.opentoksamples.R.id.mutePublisher:
					mutePublisher();
					break;

				case com.opentok.android.demo.opentoksamples.R.id.swapCamera:
					swapCamera();
					break;

				case com.opentok.android.demo.opentoksamples.R.id.endCall:
					endCall();
					break;
			}
		}

		public virtual void mutePublisher()
		{
			mCallbacks.onMutePublisher();

			mPublisherMute.ImageResource = openTokActivity.Publisher.PublishAudio ? com.opentok.android.demo.opentoksamples.R.drawable.unmute_pub : com.opentok.android.demo.opentoksamples.R.drawable.mute_pub;
		}

		public virtual void swapCamera()
		{
			mCallbacks.onSwapCamera();
		}

		public virtual void endCall()
		{
			mCallbacks.onEndCall();
		}

		public virtual void initPublisherUI()
		{
			if (openTokActivity != null)
			{
				openTokActivity.Handler.removeCallbacks(mPublisherWidgetTimerTask);
				openTokActivity.Handler.postDelayed(mPublisherWidgetTimerTask, PUBLISHER_CONTROLS_DURATION);
				mPublisherMute.ImageResource = openTokActivity.Publisher.PublishAudio ? com.opentok.android.demo.opentoksamples.R.drawable.unmute_pub : com.opentok.android.demo.opentoksamples.R.drawable.mute_pub;
			}
		}

		private Runnable mPublisherWidgetTimerTask = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{
				outerInstance.showPublisherWidget(false);
				outerInstance.openTokActivity.setPubViewMargins();
			}
		}

		public virtual void publisherClick()
		{
			if (!mPubControlWidgetVisible)
			{
				showPublisherWidget(true);
			}
			else
			{
				showPublisherWidget(false);
			}
			initPublisherUI();
		}

		public virtual void showPublisherWidget(bool show)
		{
			showPublisherWidget(show, true);
		}

		private void showPublisherWidget(bool show, bool animate)
		{
			if (mPublisherContainer != null)
			{
				mPublisherContainer.clearAnimation();
				mPubControlWidgetVisible = show;
				float dest = show ? 1.0f : 0.0f;
				AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
				aa.Duration = animate ? ANIMATION_DURATION : 1;
				aa.FillAfter = true;
				mPublisherContainer.startAnimation(aa);

				if (show)
				{
					mEndCall.Clickable = true;
					mSwapCamera.Clickable = true;
					mPublisherMute.Clickable = true;
					mPublisherContainer.Visibility = View.VISIBLE;
				}
				else
				{
					mEndCall.Clickable = false;
					mSwapCamera.Clickable = false;
					mPublisherMute.Clickable = false;
					mPublisherContainer.Visibility = View.GONE;
				}
			}
		}

		public virtual bool PubControlWidgetVisible
		{
			get
			{
				return mPubControlWidgetVisible;
			}
		}

		public virtual RelativeLayout PublisherContainer
		{
			get
			{
				return mPublisherContainer;
			}
		}

	}

}