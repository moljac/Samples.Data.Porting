using System.Collections.Generic;

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

	using R = com.opentok.android.demo.opentoksamples.R;
	using UIActivity = com.opentok.android.demo.opentoksamples.UIActivity;
	using SubscriberCallbacks = com.opentok.android.demo.ui.fragments.SubscriberControlFragment.SubscriberCallbacks;

	public class SubscriberQualityFragment : Fragment
	{

		private const string LOGTAG = "sub-quality-fragment";

		private const int ANIMATION_DURATION = 500;

		private bool mSubscriberWidgetVisible = false;
		private ImageButton congestionIndicator;
		private RelativeLayout mSubQualityContainer;
		private UIActivity openTokActivity;

		private CongestionLevel congestion = CongestionLevel.Low;

		public sealed class CongestionLevel
		{
			public static readonly CongestionLevel High = new CongestionLevel("High", InnerEnum.High, 2);
			public static readonly CongestionLevel Mid = new CongestionLevel("Mid", InnerEnum.Mid, 1);
			public static readonly CongestionLevel Low = new CongestionLevel("Low", InnerEnum.Low, 0);

			private static readonly IList<CongestionLevel> valueList = new List<CongestionLevel>();

			static CongestionLevel()
			{
				valueList.Add(High);
				valueList.Add(Mid);
				valueList.Add(Low);
			}

			public enum InnerEnum
			{
				High,
				Mid,
				Low
			}

			private readonly string nameValue;
			private readonly int ordinalValue;
			private readonly InnerEnum innerEnumValue;
			private static int nextOrdinal = 0;

			internal int congestionLevel;

			internal CongestionLevel(string name, InnerEnum innerEnum, int congestionLevel)
			{
				this.congestionLevel = congestionLevel;

				nameValue = name;
				ordinalValue = nextOrdinal++;
				innerEnumValue = innerEnum;
			}

			public int getCongestionLevel()
			{
				return congestionLevel;
			}

			public static IList<CongestionLevel> values()
			{
				return valueList;
			}

			public InnerEnum InnerEnumValue()
			{
				return innerEnumValue;
			}

			public int ordinal()
			{
				return ordinalValue;
			}

			public override string ToString()
			{
				return nameValue;
			}

			public static CongestionLevel valueOf(string name)
			{
				foreach (CongestionLevel enumInstance in CongestionLevel.values())
				{
					if (enumInstance.nameValue == name)
					{
						return enumInstance;
					}
				}
				throw new System.ArgumentException(name);
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

		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{

			View rootView = inflater.inflate(R.layout.layout_fragment_sub_quality, container, false);

			mSubQualityContainer = (RelativeLayout) openTokActivity.findViewById(R.id.fragment_sub_quality_container);

			congestionIndicator = (ImageButton) rootView.findViewById(R.id.congestionIndicator);

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
		}

		public virtual void showSubscriberWidget(bool show)
		{
			if (show)
			{
				switch (congestion.InnerEnumValue())
				{
					case com.opentok.android.demo.ui.fragments.SubscriberQualityFragment.CongestionLevel.InnerEnum.High:
						this.congestionIndicator.ImageResource = R.drawable.high_congestion;
						break;
					case com.opentok.android.demo.ui.fragments.SubscriberQualityFragment.CongestionLevel.InnerEnum.Mid:
						this.congestionIndicator.ImageResource = R.drawable.mid_congestion;
						break;
					case com.opentok.android.demo.ui.fragments.SubscriberQualityFragment.CongestionLevel.InnerEnum.Low:
						break;

					default:
						break;
				}
			}
			else
			{
				Log.i(LOGTAG, "Hidding subscriber quality");
			}

			showSubscriberWidget(show, true);

		}

		private void showSubscriberWidget(bool show, bool animate)
		{
			if (mSubQualityContainer != null)
			{
				mSubQualityContainer.clearAnimation();
				mSubscriberWidgetVisible = show;
				float dest = show ? 1.0f : 0.0f;
				AlphaAnimation aa = new AlphaAnimation(1.0f - dest, dest);
				aa.Duration = animate ? ANIMATION_DURATION : 1;
				aa.FillAfter = true;
				mSubQualityContainer.startAnimation(aa);

				if (show)
				{
					mSubQualityContainer.Visibility = View.VISIBLE;
				}
				else
				{
					mSubQualityContainer.Visibility = View.GONE;
				}
			}
		}

		public virtual CongestionLevel Congestion
		{
			get
			{
				return congestion;
			}
			set
			{
				this.congestion = value;
			}
		}


		public virtual bool SubscriberWidgetVisible
		{
			get
			{
				return mSubscriberWidgetVisible;
			}
		}

		public virtual RelativeLayout SubQualityContainer
		{
			get
			{
				return mSubQualityContainer;
			}
		}
	}

}