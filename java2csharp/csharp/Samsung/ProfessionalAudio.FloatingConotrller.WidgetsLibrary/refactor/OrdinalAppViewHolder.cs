using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Animator = android.animation.Animator;
	using Context = android.content.Context;
	using Color = android.graphics.Color;
	using ColorDrawable = android.graphics.drawable.ColorDrawable;
	using Drawable = android.graphics.drawable.Drawable;
	using LayerDrawable = android.graphics.drawable.LayerDrawable;
	using Log = android.util.Log;
	using Gravity = android.view.Gravity;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using Button = android.widget.Button;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	/// <summary>
	/// @brief ViewHolder representing ordinal application (instrument, effect)
	/// 
	/// - device_app_button      main button of the application to expand/collapse actions
	/// - device_root_layout     main layout to be expanded when main button is clicked
	/// - device_open_actions    list of buttons to open activities of the application (effect,
	///                          instrument)
	/// - device_actions         actions of the applications
	/// - device_volumes         volume up/down actions if present
	/// </summary>
	/// <seealso cref= android.support.v7.widget.RecyclerView.ViewHolder </seealso>
	public class OrdinalAppViewHolder : FcAdapter.BaseViewHolder
	{
		private static readonly string TAG = typeof(OrdinalAppViewHolder).Name;

		private readonly AppClickedListener mExpandActionsButtonListener;
		private readonly View.OnClickListener mOpenAppDirectlyButtonListener;

		internal ImageButton mDeviceAppButton;
		internal LinearLayout mDeviceRootLayout;
		internal LinearLayout mDeviceOpenActions;
		internal LinearLayout mDeviceActions;

		internal LinearLayout mDeviceVolumeActions;
		internal LinearLayout mDeviceVolumes;
		internal TextView mDeviceVolumesLabel;

		private FcModelItem mFcModelItem;
		private bool mIsFreeze = false;
		private Toast mToast = null;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public OrdinalAppViewHolder(final FcContext context, android.view.View itemView, FcAdapter adapter)
		public OrdinalAppViewHolder(FcContext context, View itemView, FcAdapter adapter) : base(itemView)
		{

			mDeviceAppButton = (ImageButton) itemView.findViewById(R.id.device_app_button);
			mDeviceRootLayout = (LinearLayout) itemView.findViewById(R.id.device_root_layout);
			mDeviceOpenActions = (LinearLayout) itemView.findViewById(R.id.device_open_actions);
			mDeviceActions = (LinearLayout) itemView.findViewById(R.id.device_actions);

			mDeviceVolumeActions = (LinearLayout) itemView.findViewById(R.id.device_volume_actions);
			mDeviceVolumes = (LinearLayout) itemView.findViewById(R.id.device_volumes);
			mDeviceVolumesLabel = (TextView) itemView.findViewById(R.id.device_volume_label);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final FcAnimator animator = new FcAnimator();
			FcAnimator animator = new FcAnimator();
			mExpandActionsButtonListener = new AppClickedListener(this, adapter);
			mOpenAppDirectlyButtonListener = new OnClickListenerAnonymousInnerClassHelper(this, context, animator);

			mDeviceAppButton.OnClickListener = mExpandActionsButtonListener;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly OrdinalAppViewHolder outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.FcContext context;
			private com.samsung.android.sdk.professionalaudio.widgets.refactor.FcAnimator animator;

			public OnClickListenerAnonymousInnerClassHelper(OrdinalAppViewHolder outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.FcContext context, com.samsung.android.sdk.professionalaudio.widgets.refactor.FcAnimator animator)
			{
				this.outerInstance = outerInstance;
				this.context = context;
				this.animator = animator;
			}

			public override void onClick(View v)
			{
				Log.d(TAG, "ordinal app view holder: device app clicked, opening activity directly");
				if (!outerInstance.mIsFreeze)
				{
					// This should be called only if there is a single return action and no more
					// other actions.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<FcActionItem> item = mFcModelItem.getReturnActions();
					IList<FcActionItem> item = outerInstance.mFcModelItem.ReturnActions;
					if (item.Count == 1)
					{
						Animator a = animator.createScaleAnimator(outerInstance.mDeviceAppButton, FcConstants.DEFAULT_SCALE_ANIM_DURATION, 0.3f, 0.2f);

						a.addListener(new FcAnimatorListenerAnonymousInnerClassHelper(this, item));

						a.start();

					}
					else
					{
						Log.w(TAG, "Return actions size should be equal to 1!");
					}
				}
				else
				{
					outerInstance.displayToast(outerInstance.mDeviceAppButton, R.@string.frozen_app_text);
				}
			}

			private class FcAnimatorListenerAnonymousInnerClassHelper : FcAnimator.FcAnimatorListener
			{
				private readonly OnClickListenerAnonymousInnerClassHelper outerInstance;

				private IList<FcActionItem> item;

				public FcAnimatorListenerAnonymousInnerClassHelper(OnClickListenerAnonymousInnerClassHelper outerInstance, IList<FcActionItem> item)
				{
					this.outerInstance = outerInstance;
					this.item = item;
				}

				public override void onAnimationEnd(Animator animation)
				{
					outerInstance.context.runOnMainThread(item[0].ActionRunnable);
				}
			}
		}

		private void displayToast(View v, int resId)
		{
			if (mToast == null)
			{
				mToast = Toast.makeText(v.Context, resId, Toast.LENGTH_SHORT);
			}
			else
			{
				mToast.Duration = Toast.LENGTH_SHORT;
				mToast.Text = resId;
			}
			mToast.show();
		}

		public override void prepareViewHolder(FcContext fcContext, FcAdapterModel model, FcModelItem item)
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "prepareViewHolder(" + item + ")");
			}
			Context context = fcContext.Context;
			cleanLayouts();

			mFcModelItem = item;
			mExpandActionsButtonListener.Item = mFcModelItem;

			if (item.Expanded)
			{
				mDeviceRootLayout.Visibility = View.VISIBLE;
				mDeviceRootLayout.LayoutParams.width = ViewGroup.LayoutParams.WRAP_CONTENT;
			}
			else
			{
				mDeviceRootLayout.Visibility = View.VISIBLE;
				mDeviceRootLayout.LayoutParams.width = 0;
			}
			mDeviceRootLayout.requestLayout();

			// Setting up device_app_button
			mDeviceAppButton.Background = getBackground(fcContext, item.Active);
			LinearLayout.LayoutParams @params = (LinearLayout.LayoutParams) mDeviceAppButton.LayoutParams;
			@params.gravity = Gravity.CENTER;

			int padding = context.Resources.getDimensionPixelSize(R.dimen.floating_controller_active_app_frame_stroke_width) + context.Resources.getDimensionPixelSize(R.dimen.floating_controller_ordinal_app_layout_distance_between_app_icon_and_active_indication_frame);

			Drawable deviceAppDrawable = mFcModelItem.Icon;
			if (model.isMultiInstance(item))
			{
				int number = model.getInstanceNumber(item);
				int iconSize = context.Resources.getDimensionPixelSize(R.dimen.ord_app_expand_action_button_width);
				deviceAppDrawable = DrawableTool.getDrawableWithNumber(deviceAppDrawable, number, iconSize, context);
			}

			mIsFreeze = model.isAppFreeze(item.InstanceId);

			if (mIsFreeze)
			{
				deviceAppDrawable = getFreezeDrawable(fcContext.Context, deviceAppDrawable);
			}

			int actionCount = item.CallActions.Count;
			actionCount += item.VolumeActions.Count;
			actionCount += item.ReturnActions.Count;

			if (actionCount > 1)
			{
				mDeviceAppButton.OnClickListener = mExpandActionsButtonListener;
			}
			else
			{
				mDeviceAppButton.OnClickListener = mOpenAppDirectlyButtonListener;
			}

			mDeviceAppButton.setPadding(padding, padding, padding, padding);
			mDeviceAppButton.ImageDrawable = deviceAppDrawable;

			// Open Actions
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<FcActionItem> returnActions = item.getReturnActions();
			IList<FcActionItem> returnActions = item.ReturnActions;
			int numberOfElements = returnActions.Count;

			for (int i = 0; i < numberOfElements; i++)
			{
				addActionButton(mDeviceOpenActions, returnActions[i], R.layout.fc_ordinal_open_app_button, i, numberOfElements, fcContext, false);
			}

			if (numberOfElements > 0)
			{
				mDeviceOpenActions.Visibility = View.VISIBLE;
			}
			else
			{
				mDeviceOpenActions.Visibility = View.GONE;
			}

			// Call Actions
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<FcActionItem> callActions = item.getCallActions();
			IList<FcActionItem> callActions = item.CallActions;
			numberOfElements = callActions.Count;
			string bypassOn = fcContext.Context.Resources.getString(R.@string.bypass_on);
			string bypassOff = fcContext.Context.Resources.getString(R.@string.bypass_off);

			for (int i = 0; i < numberOfElements; i++)
			{
				string itemId = callActions[i].Id;
				if (itemId.Equals(bypassOff))
				{
					addBypassAction(fcContext, mDeviceActions, callActions[i], R.@string.bypass_text_off);
				}
				else if (itemId.Equals(bypassOn))
				{
					addBypassAction(fcContext, mDeviceActions, callActions[i], R.@string.bypass_text_on);
				}
				else
				{
					addActionButton(mDeviceActions, callActions[i], R.layout.fc_ordinal_action_button, i, numberOfElements, fcContext, true);
				}
			}

			if (numberOfElements > 0)
			{
				mDeviceActions.Visibility = View.VISIBLE;
			}
			else
			{
				mDeviceActions.Visibility = View.GONE;
			}

			// Volume Actions
			if (item.hasVolumeActions())
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.List<FcActionItem> volumeActions = item.getVolumeActions();
				IList<FcActionItem> volumeActions = item.VolumeActions;
				numberOfElements = volumeActions.Count;

				// handle volume label anomaly
				if (item.Direction == LinearLayout.LAYOUT_DIRECTION_LTR)
				{
					for (int i = 0; i < numberOfElements; i++)
					{
						addActionButton(mDeviceVolumes, volumeActions[i], R.layout.fc_ordinal_action_button, i, numberOfElements, fcContext, true);
					}
				}
				else
				{
					for (int i = numberOfElements - 1; i >= 0; i--)
					{
						addActionButton(mDeviceVolumes, volumeActions[i], R.layout.fc_ordinal_action_button, numberOfElements - 1 - i, numberOfElements, fcContext, true);
					}
				}
				// this handles the requirement that volume label is always before volume actions in a layout
				float marginSize = fcContext.getDimensionPixelSize(R.dimen.ord_app_action_volume_layout_margin_ltr);
				if (item.Direction == LinearLayout.LAYOUT_DIRECTION_RTL)
				{
					marginSize = fcContext.getDimensionPixelSize(R.dimen.ord_app_action_volume_layout_margin_rtl);
				}
				reverseLayoutWithMargins(mDeviceVolumeActions, item.Direction, marginSize);

				mDeviceVolumeActions.Visibility = View.VISIBLE;
			}
			else
			{
				mDeviceVolumeActions.Visibility = View.GONE;
			}
		}

		private void reverseLayoutWithMargins(LinearLayout layout, int origDirection, float margin)
		{
			LinearLayout.LayoutParams deviceVolumeActionsParams = (LinearLayout.LayoutParams) layout.LayoutParams;
			if (origDirection == LinearLayout.LAYOUT_DIRECTION_RTL)
			{
				layout.LayoutDirection = LinearLayout.LAYOUT_DIRECTION_LTR;
				deviceVolumeActionsParams.MarginStart = 0;
				deviceVolumeActionsParams.MarginEnd = (int) margin;
			}
			else
			{
				deviceVolumeActionsParams.MarginStart = (int) margin;
				deviceVolumeActionsParams.MarginEnd = 0;
			}
		}

		private Drawable getBackground(FcContext fcContext, bool active)
		{
			return active ? fcContext.getDrawable(R.drawable.floating_controller_active_app_frame) : new ColorDrawable(Color.parseColor("#00ffffff"));
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void addBypassAction(FcContext fcContext, android.widget.LinearLayout linearLayout, final FcActionItem fcActionItem, int buttonText)
		private void addBypassAction(FcContext fcContext, LinearLayout linearLayout, FcActionItem fcActionItem, int buttonText)
		{
			Button bypassButton = (Button) LayoutInflater.from(fcContext.Context).inflate(R.layout.bypass_action_button, mDeviceActions, false);

	//        bypassButton.setBackground(fcActionItem.getIcon(fcContext));
			bypassButton.Enabled = fcActionItem.Enabled;
			bypassButton.Visibility = fcActionItem.Visible ? View.VISIBLE : View.GONE;
			bypassButton.Text = fcContext.Context.Resources.getString(buttonText);
			bypassButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this, fcActionItem);

			DrawableTool.setBackground(bypassButton, 0, 1, fcContext);
			linearLayout.addView(bypassButton);
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly OrdinalAppViewHolder outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.FcActionItem fcActionItem;

			public OnClickListenerAnonymousInnerClassHelper2(OrdinalAppViewHolder outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.FcActionItem fcActionItem)
			{
				this.outerInstance = outerInstance;
				this.fcActionItem = fcActionItem;
			}

			public override void onClick(View v)
			{
				fcActionItem.ActionRunnable.run();
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void addActionButton(final android.widget.LinearLayout linearLayout, final FcActionItem fcActionItem, final int resId, final int index, final int numberOfElements, final FcContext fcContext, final boolean adjustBackgroundToPositionInLayout)
		private void addActionButton(LinearLayout linearLayout, FcActionItem fcActionItem, int resId, int index, int numberOfElements, FcContext fcContext, bool adjustBackgroundToPositionInLayout)
		{
			ImageButton imageButton = (ImageButton) LayoutInflater.from(fcContext.Context).inflate(resId, mDeviceActions, false);
			imageButton.ImageDrawable = fcActionItem.getIcon(fcContext);
			imageButton.Enabled = fcActionItem.Enabled;
			imageButton.Visibility = fcActionItem.Visible ? View.VISIBLE : View.GONE;
			imageButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this, fcActionItem);

			if (adjustBackgroundToPositionInLayout)
			{
				DrawableTool.setBackground(imageButton, index, numberOfElements, fcContext);
			}

			linearLayout.addView(imageButton);
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly OrdinalAppViewHolder outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.FcActionItem fcActionItem;

			public OnClickListenerAnonymousInnerClassHelper3(OrdinalAppViewHolder outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.FcActionItem fcActionItem)
			{
				this.outerInstance = outerInstance;
				this.fcActionItem = fcActionItem;
			}

			public override void onClick(View v)
			{
				fcActionItem.ActionRunnable.run();
			}
		}

		protected internal override void cleanLayouts()
		{
			base.cleanLayouts();

			// First, remove views from layouts
			mDeviceOpenActions.removeAllViews();
			mDeviceActions.removeAllViews();
			mDeviceVolumes.removeAllViews();

			// Main layout could be in expanded/collapsed state
			mDeviceRootLayout.Visibility = View.GONE;
			mDeviceRootLayout.Alpha = 1.0f;
			mDeviceRootLayout.LayoutParams.width = ViewGroup.LayoutParams.WRAP_CONTENT;

			// Just to be sure, button should have the valid scale
			mDeviceAppButton.ScaleX = 1.0f;
			mDeviceAppButton.ScaleY = 1.0f;
			mDeviceAppButton.Enabled = true;

			// Hide action button layouts
			mDeviceOpenActions.Visibility = View.GONE;
			mDeviceActions.Visibility = View.GONE;
			mDeviceVolumeActions.Visibility = View.GONE;
		}

		public virtual Drawable getFreezeDrawable(Context context, Drawable baseDrawable)
		{
			Drawable[] drawables = new Drawable[]{baseDrawable, new ColorDrawable(Color.parseColor("#55000000")), context.Resources.getDrawable(R.drawable.freeze_multitrack_icon)};
			return new LayerDrawable(drawables);
		}

		public virtual void setScrollFocusFinished()
		{
			mFcModelItem.ScrollFocus = true;
		}

		public virtual bool needsScrollFocus()
		{
			return mFcModelItem.ScrollFocus;
		}

		public virtual bool AppButtonEnabled
		{
			set
			{
				mDeviceAppButton.Enabled = value;
			}
		}


		/// 
		private class AppClickedListener : View.OnClickListener
		{
			private readonly OrdinalAppViewHolder outerInstance;


			internal readonly FcAdapter mAdapter;
			internal FcModelItem mItem;

			/// <summary>
			/// @brief
			/// </summary>
			/// <param name="adapter"> </param>
			public AppClickedListener(OrdinalAppViewHolder outerInstance, FcAdapter adapter)
			{
				this.outerInstance = outerInstance;
				mAdapter = adapter;
			}

			/// <summary>
			/// @brief
			/// </summary>
			/// <param name="item"> </param>
			public virtual FcModelItem Item
			{
				set
				{
					mItem = value;
				}
			}

			public override void onClick(View v)
			{
				FcModelItem curItem = mItem;

				if (null == curItem)
				{
					Log.w(TAG, "Clicked on an app without bound item");
					return;
				}

				if (outerInstance.mIsFreeze)
				{
					outerInstance.displayToast(v, R.@string.frozen_app_text);
					return;
				}

				Log.d(TAG, "onClick for app " + mItem.InstanceId);

				lock (curItem)
				{
					if (curItem.Expanded)
					{
						curItem.Expanded = false;
						mAdapter.notifyItemChanged(curItem);
						return;
					}
				}

				bool invalidateAll = false;
				int previousIndex = -1;
				lock (curItem)
				{
					FcModelItem expandedItem = mAdapter.ExpandedItem;
					if (null != expandedItem)
					{
						expandedItem.Expanded = false;
						invalidateAll = true;
						previousIndex = mAdapter.getItemPosition(expandedItem);
					}
					curItem.Expanded = true;
					curItem.ScrollFocus = true;
				}

				if (invalidateAll)
				{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int nextIndex = mAdapter.getItemPosition(curItem);
					int nextIndex = mAdapter.getItemPosition(curItem);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int lowerIndex = Math.min(previousIndex, nextIndex);
					int lowerIndex = Math.Min(previousIndex, nextIndex);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int higherIndex = Math.max(previousIndex, nextIndex);
					int higherIndex = Math.Max(previousIndex, nextIndex);
					mAdapter.notifyItemRangeChanged(lowerIndex, higherIndex - lowerIndex + 1);
				}
				else
				{
					mAdapter.notifyItemChanged(curItem);
				}
			}
		}

	}

}