using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using LayoutTransition = android.animation.LayoutTransition;
	using SuppressLint = android.annotation.SuppressLint;
	using Activity = android.app.Activity;
	using ActivityNotFoundException = android.content.ActivityNotFoundException;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ApplicationInfo = android.content.pm.ApplicationInfo;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Resources = android.content.res.Resources;
	using NotFoundException = android.content.res.Resources.NotFoundException;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using LayoutDirection = android.util.LayoutDirection;
	using Log = android.util.Log;
	using Gravity = android.view.Gravity;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using AnimationSet = android.view.animation.AnimationSet;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaConnectionNotSetException = com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
	using SapaUndeclaredActionException = com.samsung.android.sdk.professionalaudio.app.SapaUndeclaredActionException;


	/// <summary>
	/// This class represents view of actions of one application. It's content is set according to the
	/// given device action info.
	/// 
	/// 
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("ViewConstructor") abstract class DeviceActionsLayout extends android.widget.LinearLayout
	internal abstract class DeviceActionsLayout : LinearLayout
	{

		public const string APP_RET_BUTTONS = "app_ret_buttons";
		public const string APP_RET_BUTTONS_OPTIONS = "app_ret_buttons_options";
		private static string TAG = "professionalaudioconnection:widget:j:DeviceActionsLayout";
		protected internal DeviceActionsLayoutData mData;
		private SapaAppService mSapaAppService;
		protected internal bool mIsExpanded = false;
		private int mAnimationTime;
		protected internal List<View> mActionButtons;
		private ImageButton mAppButton;
		protected internal ControlBar mParent;
		private Runnable updateThread;
		protected internal int mDeviceIconMaxSize;

		private volatile int mPrevLayoutDir = -1;
		protected internal static int BUTTON_WIDTH;
		protected internal static int BUTTON_HEIGHT;

		private readonly int FIRST = 0;
		private readonly int INNER = 1;
		private readonly int LAST = 2;
		private readonly int ONLY = 3;
		private View mButtonsView;
		private LinearLayout mActionButtonsLayout;

		/// <returns> True if actions are expanded (visible), false otherwise. </returns>
		protected internal virtual bool Expanded
		{
			get
			{
				return this.mIsExpanded;
			}
		}

		/// <returns> Uid of the device. </returns>
		public virtual SapaApp SapaApp
		{
			get
			{
				return this.mData.mSapaApp;
			}
		}

		public virtual bool equalsActionsOfInstance(DeviceActionsLayoutData data)
		{
			return this.mData.mSapaApp.Equals(data.mSapaApp) && this.mData.mActionList.Equals(data.mActionList) && this.mData.mInstanceIcon.Equals(data.mInstanceIcon) && this.mData.mInstancePackageName.Equals(data.mInstancePackageName);
		}

		/// <summary>
		/// This method updates values on the layout. As a result the layout is the representation of the
		/// given device action info.
		/// </summary>
		/// <param name="data">
		///            DeviceActionsLayoutData object with parameters for the view to create
		///  </param>
		public virtual void updateInfo(DeviceActionsLayoutData data)
		{
			this.mData.mSapaApp = data.mSapaApp;
			this.mData.mActionList = data.mActionList;
			this.mData.mInstanceIcon = data.mInstanceIcon;
			this.mData.mInstancePackageName = data.mInstancePackageName;
			this.mData.loadActionMap();
			this.prepareActions(mParent.Orientation, true);
		}

		/// <summary>
		/// This method sets the orietation of the layout.
		/// </summary>
		public override int Orientation
		{
			set
			{
				// if (this.getOrientation() != value) {
				this.forceOrientationSet(value);
				// }
			}
		}

		private void forceOrientationSet(int orientation)
		{
			lock (this)
			{
				// this.mActions.setOrientation(orientation);
				base.Orientation = orientation;
				int level = calculateLevel();
				if (mAppButton != null)
				{
					mAppButton.Background.Level = level;
					mAppButton.LayoutParams = getAppBtnLayoutParams(orientation);
				}
				if (mActionButtons != null)
				{
					foreach (View actionButton in this.mActionButtons)
					{
						if (actionButton.Background != null)
						{
							actionButton.Background.Level = level;
						}
					}
				}
			}
		}

		public virtual int AnimationTime
		{
			get
			{
				return this.mAnimationTime + ControlBar.ANIM_TIME;
			}
		}

		/// <summary>
		/// This Ctor creates device info that represents given device action info and calls actions by
		/// jam connection service. Layout is created with buttons of actions hidden.
		/// </summary>
		/// <param name="context">
		///            Context in which layout is created. </param>
		/// <param name="data">
		///            DeviceActionsLayoutData object with parameters for the view to create </param>
		/// <param name="sapaAppService"> 
		///            Instance of SapaAppService of this application </param>
		/// <param name="orientation">
		///            Orientation of the bar. </param>
		/// <param name="controlbar">
		///            Local instance of ControlBar </param>
		protected internal DeviceActionsLayout(Context context, DeviceActionsLayoutData data, SapaAppService sapaAppService, int orientation, ControlBar controlbar) : this(context, data, sapaAppService, false, orientation, controlbar)
		{
		}

		/// <summary>
		/// This Ctor creates device info that represents given device action info and calls actions by
		/// jam connection service.
		/// </summary>
		/// <param name="context">
		///            Context in which layout is created. </param>
		/// <param name="data">
		///            DeviceActionsLayoutData object with parameters for the view to create </param>
		/// <param name="sapaAppService"> 
		///            Instance of SapaAppService of this application </param>
		/// <param name="isExpanded">
		///             Boolean indicating whether the bar should be expanded </param>
		/// <param name="orientation">
		///            Orientation of the bar. </param>
		/// <param name="controlbar">
		///            Reference to parent controlbar widget </param>
		protected internal DeviceActionsLayout(Context context, DeviceActionsLayoutData data, SapaAppService sapaAppService, bool isExpanded, int orientation, ControlBar controlbar) : base(context)
		{
			initAppIconMaxSize();
			mParent = controlbar;

			this.mData = data;
			this.HorizontalGravity = Gravity.CENTER_HORIZONTAL;
			this.VerticalGravity = Gravity.CENTER_VERTICAL;
			this.Orientation = orientation;
			this.LayoutDirection = LayoutDirection.INHERIT;


			// mActionButtons = new ArrayList<ImageButton>();
			this.mIsExpanded = isExpanded;
			mButtonsView = createOpenButtonLayout();
			this.mAppButton = createDeviceButton();
			string appName = AppName;
			if (appName != null && appName.Length > 0)
			{
				this.mAppButton.ContentDescription = Resources.getString(R.@string.app_btn_options, appName);
			}
			this.LayoutParams = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			this.addView(this.mAppButton, getAppBtnLayoutParams(Orientation));
			this.mSapaAppService = sapaAppService;
			createActionList();
			int buttonsNo = this.mData.mActionList != null ? (this.mData.mActionList.size() + 1) : 0;
			this.mAnimationTime = ControlBar.ANIM_TIME * buttonsNo;
			if (this.mIsExpanded)
			{
				show();
			}
		}

		protected internal virtual LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation)
		{
			return orientation == LinearLayout.HORIZONTAL ? new LinearLayout.LayoutParams(90, 80) : new LinearLayout.LayoutParams(80, 90);
		}

		private void createActionList()
		{
			mActionButtons = new List<View>();

			mActionButtonsLayout = (LinearLayout) LayoutInflater.from(Context).inflate(R.layout.open_buttons_layout, this, false);
			createActionButtons();
			if (this is MainDeviceLayout)
			{
				mActionButtons.Add(mActionButtonsLayout);
				mActionButtons.Add(mButtonsView);
			}
			else
			{
				mActionButtons.Add(mButtonsView);
				mActionButtons.Add(mActionButtonsLayout);
			}
			checkInvisibles();
		}

		private void initAppIconMaxSize()
		{
			Resources res = Resources;
			mDeviceIconMaxSize = res.getDimensionPixelSize(R.dimen.max_app_ic_size);
		}

		protected internal virtual void show()
		{
			show(ShowMode.ENABLED_TRANSITION);
		}

		protected internal virtual void show(ShowMode mode)
		{
			int size = mActionButtons.Count;

			if (mPrevLayoutDir != mParent.LayoutDirection && mActionButtonsLayout.ChildCount > 1)
			{
				createActionButtons();
				mPrevLayoutDir = mParent.LayoutDirection;
				checkInvisibles();
			}

			int level = calculateLevel();
			foreach (View v in mActionButtons)
			{
				if (v.Background != null)
				{
					v.Background.Level = level;
				}

				// This is a hack, not really a good design. It's a hack for bad design of
				// FloatingController. The hack sets alpha to 0 to all ordinary device layouts
				// when they got shown on the controller bar. The reason is to make alpha animation
				// (fade in) to work. APPEAR animation is run after the CHANGING_APPEAR. The latter one
				// is displayed by default with alpha 1, so we need to set it to 0, so the APPEAR
				// animation will animate it from 0 to 1.
				//
				if (!(this is MainDeviceLayout) && mode != ShowMode.DISABLED_TRANSITION)
				{
					v.Alpha = 0f;
				}
				else
				{
					v.Alpha = 1f;
				}
			}

			mParent.addSubViews(mActionButtons.subList(0, 1), this);

			if (mActionButtons.Count > 1)
			{
				mParent.addSubViews(mActionButtons.subList(1, size), mActionButtons[0]);
			}
		}

		private void createActionButtons()
		{
			mActionButtonsLayout.removeAllViews();
			if (mParent.LayoutDirection == LAYOUT_DIRECTION_LTR)
			{
				for (int i = 0, size = mData.mActionList.size(); i <= size - 1; ++i)
				{
					mActionButtonsLayout.addView(createActionView(mData.mActionList.get(i), R.layout.action_button));
				}
			}
			else
			{
				for (int i = mData.mActionList.size() - 1; i >= 0; --i)
				{
					mActionButtonsLayout.addView(createActionView(mData.mActionList.get(i), R.layout.action_button));
				}
			}

		}

		protected internal virtual void hide()
		{
			if (this.Parent != null) //for (View button : mActionButtons)
			{
				mParent.clearSubViews(mActionButtons.Count, this);
			}
		}

		private void removeActionView(View v)
		{
			if (v.Shown)
			{
				int index = mActionButtonsLayout.indexOfChild(v);
				mActionButtonsLayout.removeView(v);
			}
		}

		private View createActionView(SapaActionInfo info, int res)
		{
			ImageButton button = null;
			try
			{
				button = (ImageButton) LayoutInflater.from(Context).inflate(res, mActionButtonsLayout, false);
				button.ImageDrawable = info.getIcon(Context);

				if (!info.Enabled)
				{
					button.Enabled = false;
				}

				button.OnClickListener = new OnActionClickListener(this, this.mData.mSapaApp, info.Id);
				string actionName = info.getName(Context);
				if (actionName != null)
				{
					button.ContentDescription = actionName;
				}

			}
			catch (NameNotFoundException)
			{
				Log.w(TAG, "Action " + info.Id + " could not be shown.");

			}
			catch (Resources.NotFoundException)
			{
				Log.w(TAG, "Action " + info.Id + " could not be shown.");
			}

			button.Tag = info;
			if (info.Visible)
			{
				button.Visibility = View.VISIBLE;
			}
			else
			{
				button.Visibility = View.GONE;
			}
			return button;
		}

		private void replaceActionOnView(View v, SapaActionInfo info)
		{
			int order = mActionButtonsLayout.indexOfChild(v);
			if (info == null || order == -1)
			{
				return;
			}
			View newView = createActionView(info, R.layout.action_button);
			if (mParent.Shown)
			{
				mActionButtonsLayout.removeView(v);
				mActionButtonsLayout.addView(newView, order);
			}
		}

		protected internal override void onLayout(bool changed, int l, int t, int r, int b)
		{
			base.onLayout(changed, l, t, r, b);
			int level = calculateLevel();
			if (mActionButtons != null)
			{
				foreach (View v in mActionButtons)
				{
				v.Background.Level = level;
				}
			}
			mAppButton.Background.Level = level;
		}

		/// <summary>
		/// This method prepares view of actions (without application icon).
		/// </summary>
		internal virtual void prepareActions(int orientation, bool addView)
		{
			if (!Expanded)
			{
				createActionList();
				return;
			}
			LayoutTransition trans = null;
			if (mParent != null)
			{
				trans = mParent.mDevicesLayout.LayoutTransition;
				mParent.mDevicesLayout.LayoutTransition = null;
			}
			List<View> toBeRemoved = new List<View>();

			for (int i = 0, size = mActionButtonsLayout.ChildCount; i < size; ++i)
			{
				View v = mActionButtonsLayout.getChildAt(i);
				SapaActionInfo info = (SapaActionInfo) v.Tag;
				if (!mData.mActionMap.ContainsKey(info.Id))
				{
					toBeRemoved.Add(v);
				}
			}
			if (toBeRemoved.Count > 0)
			{
				int sizeToRemove = toBeRemoved.Count;
				for (int i = 0; i < sizeToRemove; ++i)
				{
					removeActionView(toBeRemoved[i]);
				}
			}
			HashSet<string> ids = new HashSet<string>();
			for (int i = 0, size = mActionButtonsLayout.ChildCount; i < size; ++i)
			{
				View v = mActionButtonsLayout.getChildAt(i);
				SapaActionInfo info = (SapaActionInfo) v.Tag;
				ids.Add(info.Id);
				if (!info.Equals(mData.mActionMap[info.Id]))
				{
					replaceActionOnView(v, mData.mActionMap[info.Id]);
				}
			}
			IList<View> viewsToAdd = new List<View>();
			int i = -1;
			foreach (string actionId in mData.mActionMap.Keys)
			{
				++i;
				if (ids.Contains(actionId))
				{
					continue;
				}
				if (i == 1)
				{
					viewsToAdd.Add(createActionView(mData.mActionMap[actionId], R.layout.action_button));
				}
				else
				{
					viewsToAdd.Add(createActionView(mData.mActionMap[actionId], R.layout.action_button));
				}
			}
			if (viewsToAdd.Count > 0)
			{

				View viewToUpdate = mActionButtons[mActionButtons.Count - 1];
				foreach (View v in viewsToAdd)
				{
					mActionButtons.Add(v);
				}
				replaceActionOnView(viewToUpdate, (SapaActionInfo) viewToUpdate.Tag);
				replaceActionOnView(viewsToAdd[0], (SapaActionInfo) viewsToAdd[0].Tag);
				if (viewsToAdd.Count > 1)
				{
					replaceActionOnView(viewsToAdd[viewsToAdd.Count - 1], (SapaActionInfo) viewsToAdd[viewsToAdd.Count - 1].Tag);
				}

				int size = mActionButtons.Count;
				mParent.addSubViews(mActionButtons.subList(0, 1), this);
				mParent.addSubViews(mActionButtons.subList(1, size), mActionButtons[0]);

			}
			checkInvisibles();
			if (mParent != null && trans != null)
			{
				mParent.mDevicesLayout.LayoutTransition = trans;
			}
			if (mParent != null)
			{
				this.forceOrientationSet(mParent.Orientation);
			}
		}

		private string AppName
		{
			get
			{
				string name = null;
				ApplicationInfo ai = null;
				try
				{
					ai = Context.PackageManager.getApplicationInfo(mData.mInstancePackageName, 0);
				}
				catch (NameNotFoundException e1)
				{
					Log.e(TAG, e1.Message);
				}
    
				if (ai != null)
				{
					name = Context.PackageManager.getApplicationLabel(ai).ToString();
				}
				return name;
			}
		}

		protected internal virtual void openAppActivity()
		{
			openAppActivity(1);
		}

		protected internal virtual void openAppActivity(int mode)
		{
			if (DeviceActionsLayout.this.mData.mInstancePackageName.Length != 0)
			{
				Intent intent;
				try
				{
					intent = DeviceActionsLayout.this.mSapaAppService.getLaunchIntent(DeviceActionsLayout.this.mData.mSapaApp);
					if (intent != null)
					{
						if (Context != null)
						{
							intent.ExtrasClassLoader = typeof(SapaAppInfo).ClassLoader;
							intent.putExtra("Edit_mode", mode);
							Context.startActivity(intent);

							// send broadcast
							Intent bi = new Intent("com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP");
							bi.putExtra("com.samsung.android.sdk.professionalaudio.switchTo.instanceID", DeviceActionsLayout.this.mData.mSapaApp.InstanceId);
							bi.putExtra("com.samsung.android.sdk.professionalaudio.switchTo.packageName", DeviceActionsLayout.this.mData.mSapaApp.PackageName);
							Context.sendBroadcast(bi, "com.samsung.android.sdk.professionalaudio.permission.USE_CONNECTION_SERVICE");

							Context context = Context;
							if (context is Activity && context.PackageName.contentEquals(DeviceActionsLayout.this.mData.mSapaApp.PackageName) == false)
							{
									((Activity) context).finish();
							}
						}
						else
						{
							Log.w(TAG, "Fail to swith because of the context is null.");
						}
					}
					else
					{
						Log.w(TAG, "Fail to swith because of the launchIntent is null.");
					}
				}
				catch (SapaConnectionNotSetException)
				{
					Log.w(TAG, "Application can not be opened from ControlBar due to connection problem.");
				}
				catch (IllegalAccessException)
				{
					Log.w(TAG, "Application can not be opened from FloatingController because of its internal error.");
				}
				catch (ActivityNotFoundException)
				{
					Log.w(TAG, "Application can not be opened from FloatingController because of not existing activity");
				}
			}
		}

		/// <summary>
		/// This method creates button to jump to applicaction. It sets its looks as well as bahaviour.
		/// </summary>
		/// <returns> Button to open application which actions are respresented by this layout. </returns>
		private View createOpenButtonLayout()
		{
			Bundle bundle = mData.mAppInfo.Configuration;
			LinearLayout layout = (LinearLayout)LayoutInflater.from(Context).inflate(R.layout.open_buttons_layout, this, false);
			if (bundle != null && bundle.getIntArray(APP_RET_BUTTONS) != null && bundle.getIntArray(APP_RET_BUTTONS_OPTIONS) != null)
			{
				int[] openRes = bundle.getIntArray(APP_RET_BUTTONS);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int[] openOptions = bundle.getIntArray(APP_RET_BUTTONS_OPTIONS);
				int[] openOptions = bundle.getIntArray(APP_RET_BUTTONS_OPTIONS);
				for (int i = 0; i < openRes.Length; ++i)
				{
					ImageButton openButton = (ImageButton) LayoutInflater.from(Context).inflate(R.layout.open_btn_view, layout, false);
					Drawable drawable = getDrawableFromApp(mData.mAppInfo, openRes[i]);
					if (drawable != null)
					{
						openButton.ImageDrawable = drawable;
					}
					string appName = AppName;
					if (appName != null && appName.Length > 0)
					{
						openButton.ContentDescription = Resources.getString(R.@string.open_app_btn_desc, appName);
					}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int mode = openOptions[i];
					int mode = openOptions[i];
					openButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, mode);
					layout.addView(openButton);
				}
				return layout;
			}
			else
			{
				ImageButton openButton = (ImageButton) LayoutInflater.from(Context).inflate(R.layout.open_btn_view, this, false);

				string appName = AppName;
				if (appName != null && appName.Length > 0)
				{
					openButton.ContentDescription = Resources.getString(R.@string.open_app_btn_desc, appName);
				}
				openButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
				return openButton;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly DeviceActionsLayout outerInstance;

			private int mode;

			public OnClickListenerAnonymousInnerClassHelper(DeviceActionsLayout outerInstance, int mode)
			{
				this.outerInstance = outerInstance;
				this.mode = mode;
			}

			public override void onClick(View v)
			{
				outerInstance.openAppActivity(mode);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly DeviceActionsLayout outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(DeviceActionsLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				outerInstance.openAppActivity();
			}
		}

		private Drawable getDrawableFromApp(SapaAppInfo appInfo, int openRe)
		{
			Context context = Context;
			try
			{
				return context.PackageManager.getResourcesForApplication(appInfo.PackageName).getDrawable(openRe);
			}
			catch (NameNotFoundException e)
			{
				Log.e(TAG, "Exception:", e);
			}
			return null;
		}

		/// <summary>
		/// This method creates button to show and hide its buttons of actions. It sets its looks as well
		/// as bahaviour.
		/// </summary>
		/// <returns> Button to hide and show action of represented application. </returns>
		protected internal abstract ImageButton createDeviceButton();

		internal virtual void expand()
		{
			if (mIsExpanded)
			{
				return;
			}
			DeviceActionsLayout.this.mIsExpanded = true;
			show();
			mParent.ExpandCalled = true;
			mParent.getInfo().setDeviceExpanded(mData.mSapaApp.InstanceId, true);
		}

		internal virtual void collapse()
		{
			if (!mIsExpanded)
			{
				return;
			}
			DeviceActionsLayout.this.mIsExpanded = false;
			hide();
			mParent.getInfo().setDeviceExpanded(mData.mSapaApp.InstanceId, false);
		}

		protected internal override void onVisibilityChanged(View changedView, int visibility)
		{
			base.onVisibilityChanged(changedView, visibility);
			if (visibility == View.VISIBLE)
			{
				prepareActions(mParent.Orientation, true);
				if (updateThread != null)
				{
					post(updateThread);
				}
				updateThread = null;
			}
		}

		public enum ShowMode
		{
			ENABLED_TRANSITION,
			DISABLED_TRANSITION,
		}

		public virtual void refresh()
		{
			updateThread = () =>
			{
				LayoutTransition trans = null;
				if (mParent != null)
				{
					trans = mParent.mDevicesLayout.LayoutTransition;
					mParent.mDevicesLayout.LayoutTransition = null;

					if (mParent.getInfo().isDevExpanded(mData.mSapaApp.InstanceId))
					{
						if (mIsExpanded)
						{
							hide();
						}
						show(ShowMode.DISABLED_TRANSITION);
						mIsExpanded = true;
					}
					else
					{
						if (mIsExpanded)
						{
							hide();
						}
						mIsExpanded = false;
					}
					if (trans != null)
					{
						mParent.mDevicesLayout.LayoutTransition = trans;
					}
				}
			};

			if (Shown)
			{
				post(updateThread);
				updateThread = null;
			}
		}

		private void checkInvisibles()
		{
			if (mActionButtons == null)
			{
				return;
			}
			for (int i = 0, size = mActionButtonsLayout.ChildCount; i < size; ++i)
			{
				View v = mActionButtonsLayout.getChildAt(i);
				SapaActionInfo info = (SapaActionInfo) v.Tag;
				if (info != null && info.Visible)
				{
					mActionButtonsLayout.getChildAt(i).Visibility = VISIBLE;
				}
				else
				{
					mActionButtonsLayout.getChildAt(i).Visibility = GONE;
				}
			}
		}


		// TODO get rid of it
		protected internal virtual int calculateLevel()
		{
			return (mParent.Orientation * 2) + mParent.LayoutDirection;
		}

		/// <summary>
		/// This class is used to handle onclick on action's buttons.
		/// 
		/// </summary>
		private class OnActionClickListener : View.OnClickListener
		{
			private readonly DeviceActionsLayout outerInstance;


			internal string actionId;
			internal SapaApp mSapaApp;

			/// <param name="sapaApp"> 
			///             SapaApp object for instance of application to which chosen action belongs </param>
			/// <param name="actionId">
			///            Id of action that is represented on button. </param>
			public OnActionClickListener(DeviceActionsLayout outerInstance, SapaApp sapaApp, string actionId)
			{
				this.outerInstance = outerInstance;
				this.actionId = actionId;
				this.mSapaApp = sapaApp;
			}

			/// <summary>
			/// As the result of onClick action is called on the device using jam connection service.
			/// </summary>
			public override void onClick(View v)
			{
				if (outerInstance.mSapaAppService != null)
				{
					try
					{
						outerInstance.mSapaAppService.callAction(this.mSapaApp, this.actionId);
					}
					catch (SapaConnectionNotSetException)
					{
						Log.w(TAG, "Action could not be called due to connection problem.");
					}
					catch (SapaUndeclaredActionException)
					{
						Log.w(TAG, "Attempt to call undeclared action.");
					}
				}
			}
		}
	}

}