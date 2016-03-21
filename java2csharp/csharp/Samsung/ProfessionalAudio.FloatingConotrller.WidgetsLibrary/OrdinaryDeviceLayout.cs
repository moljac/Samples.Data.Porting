namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using Resources = android.content.res.Resources;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;

	internal class OrdinaryDeviceLayout : DeviceActionsLayout
	{

		private static string TAG = "professionalaudioconnection:widget:j:OrdinaryDeviceLayout";
		private int perpendicular;
		private int along;

		protected internal OrdinaryDeviceLayout(Context context, DeviceActionsLayoutData data, SapaAppService sapaAppService, bool isExpanded, int orientation, ControlBar bar) : base(context, data, sapaAppService, isExpanded, orientation, bar)
		{
		}

		protected internal override ImageButton createDeviceButton()
		{
			ImageButton button = (ImageButton)LayoutInflater.from(Context).inflate(R.layout.app_view, this,false);
			button.ImageDrawable = this.mData.mInstanceIcon;

			if (mData.mActionList.size() > 0)
			{
				button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			}
			else
			{
				button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
			}

			Resources res = Resources;
			perpendicular = res.getDimensionPixelSize(R.dimen.max_app_ic_size);
			along = res.getDimensionPixelSize(R.dimen.max_app_ic_size);

			return button;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly OrdinaryDeviceLayout outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(OrdinaryDeviceLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				Log.d(TAG, "Device " + outerInstance.mData.mSapaApp + " clicked.");
				if (outerInstance.mIsExpanded)
				{
					outerInstance.collapse();
				}
				else
				{
					outerInstance.expand();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly OrdinaryDeviceLayout outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(OrdinaryDeviceLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.openAppActivity();
			}
		}

		protected internal override LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation)
		{
			return orientation == LinearLayout.HORIZONTAL ? new LinearLayout.LayoutParams(along, perpendicular) : new LinearLayout.LayoutParams(perpendicular, along);
		}

		internal override void expand()
		{
			if (mIsExpanded)
			{
				return;
			}
			mIsExpanded = true;
			swapExpandedDeviceLayouts();
			show();
			focusProperDevice();
			mParent.getInfo().setDeviceExpanded(mData.mSapaApp.InstanceId, true);
		}

		private void swapExpandedDeviceLayouts()
		{
			// make this layout the expanded one while simultaneously collapsing previously expanded if applies
			if (mParent.ExpandedLayout != null && mParent.ExpandedLayout != this)
			{
				mParent.ExpandedLayout.collapse();
			}
			mParent.ExpandedLayout = this;
		}

		private void focusProperDevice()
		{
			// focus properly on expanded device
			mParent.ExpandCalled = true;
			int previousExpandedX = PreviouslyExpandedX;
			int scrollTargetX = computeScrollTargetX(previousExpandedX);
			mParent.ScrollTargetX = scrollTargetX;
		}

		private int PreviouslyExpandedX
		{
			get
			{
				// return X coordinate of previously expanded OrdinaryDeviceLayout
				int previousX = 0;
				if (mParent.ExpandedLayout != null)
				{
					previousX = mParent.ExpandedLayout.Left;
				}
				return previousX;
			}
		}

		private int computeScrollTargetX(int prevActiveX)
		{
			// compute X coordinate of scrolling for parent horizontal view
			// depending on the positioning of the ControlBar
			int resX = this.Left - mParent.EndItemWidth;

			if (mParent.CurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_BOTTOM_LEFT || mParent.CurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_TOP_LEFT)
			{
				resX -= prevActiveX - mParent.EndItemWidth + this.Width;
			}

			return resX;
		}

		internal override void collapse()
		{
			if (!mIsExpanded)
			{
				return;
			}
			mIsExpanded = false;
			if (mParent.ExpandedLayout == this)
			{
				mParent.ExpandedLayout = null;
			}
			hide();
			mParent.getInfo().setDeviceExpanded(mData.mSapaApp.InstanceId, false);
		}
	}

}