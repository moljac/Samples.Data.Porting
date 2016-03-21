using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using AnimatorSet = android.animation.AnimatorSet;
	using LayoutTransition = android.animation.LayoutTransition;
	using ObjectAnimator = android.animation.ObjectAnimator;
	using SuppressLint = android.annotation.SuppressLint;
	using Activity = android.app.Activity;
	using ClipData = android.content.ClipData;
	using Context = android.content.Context;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Drawable = android.graphics.drawable.Drawable;
	using Bundle = android.os.Bundle;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using HorizontalScrollView = android.widget.HorizontalScrollView;
	using ImageButton = android.widget.ImageButton;
	using ImageView = android.widget.ImageView;
	using LinearLayout = android.widget.LinearLayout;

	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaAppStateListener = com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
	using SapaConnectionNotSetException = com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
	using SapaServiceConnectListener = com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;


	/// 
	/// <summary>
	/// This class represents the bar that exposes actions of all active devices on jam connection
	/// service. It shall be used only as a part of JamControl.
	/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("ViewConstructor") abstract class ControlBar extends android.widget.LinearLayout implements com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener, com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener
	internal abstract class ControlBar : LinearLayout, SapaServiceConnectListener, SapaAppStateListener
	{

		protected internal const int ANIM_TIME = 100;

		private const string TAG = "professionalaudioconnection:widget:j:ControlBar";
		private const long HIDE_ANIM_DURATION = 300;
		private const long SHOW_ANIM_DURATION = 300;

		protected internal Info mInfo;
		protected internal SortedDictionary<string, DeviceActionsLayout> mActionsLayouts;
		protected internal IDictionary<string, DeviceActionsLayoutData> mDevicesActions;
		protected internal HorizontalScrollView mHorizontalView;
		protected internal LinearLayout mDevicesLayout;
		private ImageButton mBarHandler;
		private object mLock = new object();

		private ImageView mEnd;
		private IDictionary<int?, int?> mEndImageRes;

		// this set of variables are to support FloatingController focus changes
		private bool mExpandCalled;
		private int mScrollTargetX;
		private int mCurrentPosition;
		private int mPreviousPosition;

		private DeviceActionsLayout mExpandedLayout;

		protected internal SapaAppService mSapaAppService;
		protected internal DeviceActionsLayout mMainDeviceLayout;
		protected internal bool mRemoveMain = false;
		private IList<string> mOrderingList;

		/// <summary>
		/// This method changes orientation of the bar.
		/// </summary>
		/// <param name="orientation">
		///            LinearLayout orientation to be set. </param>
		public override int Orientation
		{
			set
			{
				lock (this)
				{
            
					base.Orientation = value;
					this.mDevicesLayout.Orientation = value;
            
					//if (mEndImageRes != null && mEndImageRes.containsKey(getOrientation()))
					 //   this.mEnd.setImageResource(mEndImageRes.get(value));
					updateDrawables();
					if (mMainDeviceLayout != null)
					{
						mMainDeviceLayout.Orientation = value;
					}
					//if (value == LinearLayout.HORIZONTAL) mEnd.setLayoutParams(new LayoutParams(
					//        LayoutParams.WRAP_CONTENT, DeviceActionsLayout.BUTTON_HEIGHT));
					//else mEnd.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_HEIGHT,
					 //       LayoutParams.WRAP_CONTENT));
					foreach (DeviceActionsLayout layout in this.mActionsLayouts.Values)
					{
						layout.Orientation = value;
					}
				}
			}
		}

		/// <summary>
		/// This method exposes information about which devices shall have their actions shown.
		/// </summary>
		/// <returns> Map of device uid with information if it's bar shall be expanded. </returns>
		public virtual IDictionary<string, bool?> DevicesExpanded
		{
			get
			{
				lock (this)
				{
					IDictionary<string, bool?> devsExpanded = new Dictionary<string, bool?>();
					foreach (DeviceActionsLayout layout in this.mActionsLayouts.Values)
					{
						devsExpanded[layout.SapaApp.InstanceId] = layout.Expanded;
					}
					return devsExpanded;
				}
			}
		}

		/// <summary>
		/// This method exposes information whether whole bar is expanded or not.
		/// </summary>
		/// <returns> true if bar is expanded, false otherwise. </returns>
		public virtual bool BarExpanded
		{
			get
			{
				return mInfo.mListHidden;
			}
		}

		/// <summary>
		/// This ctor is used mostly when ControlBar is inflated from xml layout
		/// </summary>
		/// <param name="context">
		///            The context for which the view is being inflated. If it is not an Activity the
		///            exception may be thrown. </param>
		/// <param name="attrs">
		///            Attributes given in xml declaration of ControlBar </param>
		/// <exception cref="NotActivityException">
		///             Exception thrown if the context of this view is not an activity </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public ControlBar(android.content.Context context, android.util.AttributeSet attrs) throws NotActivityException
		public ControlBar(Context context, AttributeSet attrs) : base(context, attrs)
		{
			init(context, new Info(), R.layout.controlbar);
		}

		/// <summary>
		/// This ctor starts control bar in state (which parts are extended which not) given by
		/// parameters. </summary>
		/// <param name="activity"> 
		///            Context which MUST be an instance of the activity in which the ControlBar will be visible </param>
		/// <param name="info">
		///            ControlBar.Info object with some settings </param>
		/// <param name="layoutResource">
		///            A resource defining layout for this ControlBar </param>
		/// <exception cref="NotActivityException"> 
		///            This exception is throw if the Context is not an Activity </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected ControlBar(android.content.Context activity, Info info, int layoutResource) throws NotActivityException
		protected internal ControlBar(Context activity, Info info, int layoutResource) : base(activity)
		{
			init(activity, info, layoutResource);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: ControlBar(android.content.Context activity, Info info) throws NotActivityException
		internal ControlBar(Context activity, Info info) : base(activity)
		{
			init(activity, info, R.layout.controlbar);
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private synchronized void init(android.content.Context activity, Info info, int res) throws NotActivityException
		private void init(Context activity, Info info, int res)
		{
			lock (this)
			{
				if (!(activity is Activity))
				{
					throw new NotActivityException();
				}
				LayoutInflater.from(Context).inflate(res, this, true);
				LayoutDirection = LAYOUT_DIRECTION_INHERIT;
				mInfo = info;
				this.mActionsLayouts = new SortedDictionary<string, DeviceActionsLayout>();
				this.setControlBar();
        
				this.setDevicesLayout();
        
				initHorizontalScrolling();
        
				this.mBarHandler.OnLongClickListener = new MyLongTounchListener(this);
        
				this.mBarHandler.Id = Context.GetHashCode();
        
				this.mEnd = (ImageView) findViewById(R.id.end);
				//if (mEndImageRes != null && mEndImageRes.containsKey(getOrientation())) this.mEnd
				  //      .setImageResource(mEndImageRes.get(getOrientation()));
			   // else this.mEnd.setImageResource(android.R.color.transparent);
				if (mInfo.mListHidden || this.mActionsLayouts == null || this.mActionsLayouts.Count == 0)
				{
					this.mEnd.Visibility = LinearLayout.GONE;
				}
				else
				{
					this.mEnd.Visibility = LinearLayout.VISIBLE;
				}
				//if (getOrientation() == LinearLayout.HORIZONTAL) mEnd.setLayoutParams(new LayoutParams(
				 //       LayoutParams.WRAP_CONTENT, DeviceActionsLayout.BUTTON_HEIGHT));
				//else mEnd.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_HEIGHT,
				 //       LayoutParams.WRAP_CONTENT));
        
				this.mSapaAppService = null;
				this.Orientation = Orientation;
        
			}
		}

		private void initHorizontalScrolling()
		{
			mHorizontalView = (HorizontalScrollView) findViewById(R.id.main_scroll_view);

			mPreviousPosition = mCurrentPosition;

			mHorizontalView.addOnLayoutChangeListener(new OnLayoutChangeListenerAnonymousInnerClassHelper(this));
		}

		private class OnLayoutChangeListenerAnonymousInnerClassHelper : OnLayoutChangeListener
		{
			private readonly ControlBar outerInstance;

			public OnLayoutChangeListenerAnonymousInnerClassHelper(ControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onLayoutChange(View view, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom)
			{
				if (outerInstance.mExpandCalled)
				{
					if (outerInstance.mPreviousPosition != outerInstance.mCurrentPosition)
					{
						if (outerInstance.mCurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_BOTTOM_LEFT || outerInstance.mCurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_TOP_LEFT)
						{
							outerInstance.mHorizontalView.fullScroll(HorizontalScrollView.FOCUS_RIGHT);
						}
						else
						{
							outerInstance.mHorizontalView.fullScroll(HorizontalScrollView.FOCUS_LEFT);
						}
					}
					else
					{
						outerInstance.mHorizontalView.scrollTo(outerInstance.mScrollTargetX, 0);
					}
					outerInstance.mPreviousPosition = outerInstance.mCurrentPosition;
					outerInstance.mExpandCalled = false;
				}
			}
		}

		public virtual int CurrentPosition
		{
			set
			{
				mCurrentPosition = value;
			}
			get
			{
				return mCurrentPosition;
			}
		}


		public virtual bool ExpandCalled
		{
			set
			{
				this.mExpandCalled = value;
			}
		}

		public virtual int ScrollTargetX
		{
			set
			{
				this.mScrollTargetX = value;
			}
		}

		public virtual DeviceActionsLayout ExpandedLayout
		{
			set
			{
				mExpandedLayout = value;
			}
			get
			{
				return mExpandedLayout;
			}
		}


		public virtual int EndItemWidth
		{
			get
			{
				return mEnd.Width;
			}
		}

		internal virtual void setSapaAppService(SapaAppService sapaAppService, string mainAppPackageName)
		{
			Log.d(TAG, "setSapaservice " + sapaAppService);
			SapaAppService previous = this.mSapaAppService;
			this.mSapaAppService = sapaAppService;
			if (sapaAppService != null)
			{
				mInfo.mMainApplicationPackage = mainAppPackageName;
				this.mSapaAppService.addConnectionListener(this);
				this.mSapaAppService.addAppStateListener(this);
				if (Context != null)
				{
					try
					{
					((Activity) Context).runOnUiThread(() =>
					{
						lock (ControlBar.this)
						{
							ControlBar.this.initializeActions();
							ControlBar.this.updateBar();
							ControlBar.this.fillBar();
						}
					});
					}
					catch (System.NullReferenceException)
					{
						;
					}
				};
			}
			else if (previous != null)
			{
				previous.removeConnectionListener(this);
				previous.removeAppStateListener(this);
			}
		}

		internal virtual void updateSapaAppService(SapaAppService sapaAppService)
		{
			setSapaAppService(sapaAppService, mInfo.mMainApplicationPackage);
		}

		internal virtual SapaAppService SapaAppService
		{
			get
			{
				return this.mSapaAppService;
			}
		}

		internal virtual IDictionary<int?, int?> EndImageMap
		{
			set
			{
				this.mEndImageRes = value;
			}
		}

		internal virtual void removeListeners()
		{

			if (mSapaAppService == null)
			{
				return;
			}
			this.mSapaAppService.removeConnectionListener(this);
			this.mSapaAppService.removeAppStateListener(this);
		}

		private void setDevicesLayout()
		{
			mDevicesLayout = (LinearLayout) findViewById(R.id.main_panel);

			LayoutTransition transition = new LayoutTransition();
			transition.AnimateParentHierarchy = false;
			transition.enableTransitionType(LayoutTransition.DISAPPEARING);
			{
				// Fade out
				transition.setAnimator(LayoutTransition.DISAPPEARING, ObjectAnimator.ofFloat(null, View.ALPHA, 1, 0));

				transition.setStartDelay(LayoutTransition.DISAPPEARING, 0);
				transition.setDuration(LayoutTransition.DISAPPEARING, HIDE_ANIM_DURATION / 2);

				transition.setStartDelay(LayoutTransition.CHANGE_DISAPPEARING, HIDE_ANIM_DURATION / 2);
				transition.setDuration(LayoutTransition.CHANGE_DISAPPEARING, HIDE_ANIM_DURATION / 2);
			}

			transition.enableTransitionType(LayoutTransition.APPEARING);
			{
				// Fade in
				transition.setAnimator(LayoutTransition.APPEARING, ObjectAnimator.ofFloat(null, View.ALPHA, 0, 1));

				transition.setStartDelay(LayoutTransition.CHANGE_APPEARING, 0);
				transition.setDuration(LayoutTransition.CHANGE_APPEARING, SHOW_ANIM_DURATION / 2);

				transition.setStartDelay(LayoutTransition.APPEARING, SHOW_ANIM_DURATION / 2);
				transition.setDuration(LayoutTransition.APPEARING, SHOW_ANIM_DURATION / 2);
			}

			mDevicesLayout.LayoutTransition = transition;
		}

		protected internal override void onLayout(bool changed, int l, int t, int r, int b)
		{
			base.onLayout(changed, l, t, r, b);
			updateDrawables();
		}

		/// <summary>
		/// Class responsible for responding on long click.
		/// </summary>
		private sealed class MyLongTounchListener : OnLongClickListener
		{
			private readonly ControlBar outerInstance;

			public MyLongTounchListener(ControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool onLongClick(View view)
			{
				ClipData data = ClipData.newPlainText("", "");
				DragShadowBuilder shadowBuilder = new View.DragShadowBuilder(outerInstance.mMainDeviceLayout != null ? outerInstance.mMainDeviceLayout : outerInstance.mBarHandler);
				view.startDrag(data, shadowBuilder, outerInstance.mBarHandler, 0);
				return true;
			}
		}

		/// <summary>
		/// This method fills control bar and sets its behaviour.
		/// </summary>
		private void setControlBar()
		{
			this.setBarHandler();
			// this.addView(this.mBarHandler,0);
		}

		/*
		 * this method sets levels of drawables depending on orientation as follows 0 horizontal left to
		 * right 1 horizontal right to left 2 vertical left to right 3 vertical right to left (used as
		 * bottom to top)
		 * 
		 * This solution was used as temporary one
		 */
		private void updateDrawables()
		{
			this.mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;
			Drawable bg = mDevicesLayout.Background;
			if (bg != null)
			{
				bg.Level = Orientation * 2 + LayoutDirection;
			}
			/*for(int i=0; i<mDevicesLayout.getChildCount(); ++i){
			    View v = mDevicesLayout.getChildAt(i);
			    if(v.getTag()!=null && v.getTag() instanceof DeviceActionsLayoutData) break;
			    v.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
			}*/
		}

		/*
		 * This method sets bar handler.
		 */
		private void setBarHandler()
		{
			this.mBarHandler = (ImageButton) findViewById(R.id.barhandler);

			this.mBarHandler.BackgroundResource = mInfo.mHandleDrawableRes;
			// TODO remove formula
			this.mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;
			this.mBarHandler.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly ControlBar outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(ControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (!outerInstance.mInfo.mListHidden)
				{
					outerInstance.hide();
				}
				else
				{
					outerInstance.expand();
				}
				outerInstance.mInfo.mListHidden = !outerInstance.mInfo.mListHidden;
			}
		}

		public virtual void expand()
		{
			mInfo.restoreState();
			refresh();
		}

		public virtual void hide()
		{
			if (mInfo.mListHidden)
			{
				return;
			}
			mInfo.saveState();
			ControlBar.this.mEnd.Visibility = LinearLayout.GONE;
			ControlBar.this.mBarHandler.Activated = false;
			if (ControlBar.this.mMainDeviceLayout != null)
			{
				ControlBar.this.mMainDeviceLayout.collapse();
			}
			if (this.mDevicesLayout != null)
			{
				foreach (DeviceActionsLayout dev in mActionsLayouts.Values)
				{
				dev.collapse();
				this.mDevicesLayout.removeView(dev);
				}
			}
			//mDevicesLayout.requestLayout();
		}

		private void show()
		{
			if (ControlBar.this.mMainDeviceLayout != null || (ControlBar.this.mDevicesActions != null && ControlBar.this.mDevicesActions.Count > 0))
			{
				// ControlBar.this.mBarHandler.setBackgroundResource(mHandleDrawableRes);
				ControlBar.this.mEnd.Visibility = LinearLayout.VISIBLE;
				ControlBar.this.mBarHandler.Activated = true;
			}
			else
			{
				mInfo.mListHidden = true;
				return;
			}
			if (ControlBar.this.mMainDeviceLayout != null)
			{
				ControlBar.this.mMainDeviceLayout.expand();
			}
			if (ControlBar.this.mDevicesActions != null)
			{

				foreach (string instanceId in mOrderingList)
				{
					DeviceActionsLayoutData info = mDevicesActions[instanceId];
					if (info == null)
					{
						continue;
					}
					DeviceActionsLayout layout = ControlBar.this.mActionsLayouts[info.mSapaApp.InstanceId];
					Log.d(TAG, "info " + info);
					if (layout != null)
					{
						if (!mInfo.mListHidden && mDevicesLayout.indexOfChild(layout) == -1)
						{
							ControlBar.this.mDevicesLayout.addView(layout, ControlBar.this.EndIndex);
							bool? oExpanded = mInfo.mDevsExpanded[info.mSapaApp.InstanceId];
							bool expanded = oExpanded != null ? oExpanded : false;
							if (expanded)
							{
								layout.expand();
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// This method removes all views from bar besides handler.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private void clearBar()
		private void clearBar()
		{
			if (this.mDevicesLayout != null)
			{
				foreach (DeviceActionsLayout dev in mActionsLayouts.Values)
				{
				dev.collapse();
				this.mDevicesLayout.removeView(dev);
				}
			}
		}

		protected internal override void onVisibilityChanged(View changedView, int visibility)
		{

			if (visibility == View.VISIBLE && mUpdateThread != null)
			{
				post(mUpdateThread);
				mUpdateThread = null;
			}
			base.onVisibilityChanged(changedView, visibility);
		}

		private void updateMainLayout()
		{
			if (!Shown)
			{
				return;
			}
			if (mMainDeviceLayout != null)
			{
				bool addView = true;
				for (int i = 1; i < mDevicesLayout.ChildCount - 1; ++i)
				{
					View v = mDevicesLayout.getChildAt(i);
					if (v.Tag == null)
					{
						continue;
					}
					if ((v.Tag is DeviceActionsLayoutData) && ((DeviceActionsLayoutData) v.Tag).Equals(mMainDeviceLayout.Tag))
					{
						addView = false;
						break;
					}
				}

				if (addView)
				{
					addMainDevLayout();
				}
				if (!mInfo.mListHidden)
				{
					mMainDeviceLayout.expand();
				}
			}
			if ((mRemoveMain))
			{
				mMainDeviceLayout.collapse();
				ViewGroup parent = (ViewGroup)mMainDeviceLayout.Parent;
				parent.removeView(mMainDeviceLayout);
				mMainDeviceLayout = null;
				mRemoveMain = false;
			}
		}

		protected internal virtual ViewGroup MainDevParent
		{
			get
			{
				return (ViewGroup) mDevicesLayout;
			}
		}

		protected internal virtual void removeMainDevLayout()
		{

		}

		/// <summary>
		/// This method sets content of control bar according to the list of active devices from
		/// connection service.
		/// </summary>
		private void fillBar()
		{
			Log.d(TAG, "\t\tActive:");

			if (Context != null && Shown)
			{
				if (ControlBar.this.mDevicesActions != null)
				{
					foreach (DeviceActionsLayoutData info in ControlBar.this.mDevicesActions.Values)
					{
					Log.d(TAG, "action \t\t\t\t\t" + info);
					}
				}
				else
				{
					Log.d(TAG, "actions null");
				}
				updateMainLayout();
				if (ControlBar.this.mDevicesActions != null)
				{
					foreach (DeviceActionsLayoutData info in ControlBar.this.mDevicesActions.Values)
					{
						DeviceActionsLayout layout = ControlBar.this.mActionsLayouts[info.mSapaApp.InstanceId];
						if (layout != null)
						{
							if (!layout.equalsActionsOfInstance(info))
							{
								layout.updateInfo(info);
							}
						}
						else
						{
							bool? oExpanded = false;
							bool expanded = oExpanded != null ? oExpanded : false;
							// check
							if (info.mInstancePackageName.Equals(mInfo.mMainApplicationPackage))
							{
								if (ControlBar.this.mMainDeviceLayout == null)
								{
									createMainAppLayout(info, expanded);
								}
							}
							else
							{
								DeviceActionsLayout newLayout = createAppLayout(info, expanded);
								ControlBar.this.mActionsLayouts[info.mSapaApp.InstanceId] = newLayout;
								if (!mInfo.mListHidden)
								{
									ControlBar.this.mDevicesLayout.addView(newLayout, ControlBar.this.EndIndex);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// This method sets local devices actions layout map according to the list on connection
		/// service.
		/// </summary>
		private void updateBar()
		{
			IList<string> toRemove = new List<string>();
			foreach (string deviceUid in ControlBar.this.mActionsLayouts.Keys)
			{
				if (!ControlBar.this.mDevicesActions.ContainsKey(deviceUid))
				{
					toRemove.Add(deviceUid);
				}
			}
			foreach (string key in toRemove)
			{
				DeviceActionsLayout dev = ControlBar.this.mActionsLayouts.Remove(key);
				dev.collapse();
				ControlBar.this.mDevicesLayout.removeView(dev);

			}
		}

		private int EndIndex
		{
			get
			{
				return ControlBar.this.mDevicesLayout.ChildCount - 1;
			}
		}

		/// 
		/// <returns> true if the list of devices actions has changed, false otherwise </returns>
		private bool initializeActions()
		{
			lock (this)
			{
				if (this.mSapaAppService != null)
				{
					IDictionary<string, DeviceActionsLayoutData> devicesActions = new Dictionary<string, DeviceActionsLayoutData>();
					IList<string> orderingList = null;
					lock (mLock)
					{
						IList<SapaAppInfo> list;
						try
						{
							list = this.mSapaAppService.AllActiveApp;
							if (list != null)
							{
								foreach (SapaAppInfo info in list)
								{
									Log.d(TAG, "app instanceId:" + info.App.InstanceId);
									Bundle bundle = info.Configuration;
        
									if (bundle != null)
									{
										orderingList = bundle.getStringArrayList("track_order");
									}
									try
									{
										devicesActions[info.App.InstanceId] = new DeviceActionsLayoutData(info, Context, isMultiInstance(info));
									}
									catch (NameNotFoundException)
									{
										Log.d(TAG, "Actions of " + info.App.InstanceId);
									}
        
									if (info.PackageName.Equals(mInfo.mMainApplicationPackage))
									{
										if (ControlBar.this.mMainDeviceLayout == null)
										{
											createMainAppLayout(info, false);
										}
									}
								}
        
								if (!devicesActions.Equals(this.mDevicesActions))
								{
									if (this.mDevicesActions == null || !this.mDevicesActions.Equals(devicesActions))
									{
										this.mDevicesActions = devicesActions;
										mOrderingList = orderingList;
										return true;
									}
								}
							}
						}
						catch (SapaConnectionNotSetException)
						{
							//devicesActions = new HashMap<String, DeviceActionsLayoutData>();
							Log.w(TAG, "Devices were not obtained due to problems with connection");
						}
					}
				}
				return false;
			}
		}

		internal abstract void addSubViews<T1>(IList<T1> subViews, View view) where T1 : android.view.View;

		internal abstract void clearSubViews(int number, View view);

		private void createMainAppLayout(SapaAppInfo info, bool expanded)
		{
			try
			{
				DeviceActionsLayoutData data = new DeviceActionsLayoutData(info, Context, isMultiInstance(info));
				createMainAppLayout(data, expanded);
			}
			catch (NameNotFoundException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private DeviceActionsLayout createAppLayout(DeviceActionsLayoutData info, bool expanded)
		{
			DeviceActionsLayout newLayout = null;
			newLayout = new OrdinaryDeviceLayout(ControlBar.this.Context, info, ControlBar.this.mSapaAppService, expanded, ControlBar.this.Orientation, this);
			newLayout.LayoutParams = new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH, DeviceActionsLayout.BUTTON_HEIGHT);
			return newLayout;
		}

		private void createMainAppLayout(DeviceActionsLayoutData info, bool expanded)
		{
			DeviceActionsLayout newLayout = null;
			newLayout = new MainDeviceLayout(ControlBar.this.Context, info, ControlBar.this.mSapaAppService, expanded, ControlBar.this.Orientation, this);
			newLayout.LayoutParams = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			newLayout.getChildAt(0).OnLongClickListener = new MyLongTounchListener(this);
			ControlBar.this.mMainDeviceLayout = newLayout;
			ControlBar.this.mMainDeviceLayout.Tag = info;
			addMainDevLayout();
		}

		protected internal virtual void addMainDevLayout()
		{
			Log.d(TAG, "mainLayout add");
			if (mDevicesLayout.Shown)
			{
				ControlBar.this.mDevicesLayout.addView(ControlBar.this.mMainDeviceLayout, 1);
			}
		}

		/// <summary>
		/// This method logs content of devices actions list.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private synchronized void logReceivedAppActions()
		private void logReceivedAppActions()
		{
			lock (this)
			{
				if (this.mDevicesActions != null)
				{
					foreach (DeviceActionsLayoutData appAction in this.mDevicesActions.Values)
					{
						Log.d(TAG, "DeviceId: " + appAction.mSapaApp);
					}
				}
				else
				{
					Log.d(TAG, "no devices");
				}
			}
		}

		public override void onAppActivated(SapaApp sapaApp)
		{
			lock (this)
			{
				Log.d(TAG, "onAppActivated &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
				try
				{
        
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.samsung.android.sdk.professionalaudio.app.SapaAppInfo appInfo = this.mSapaAppService.getActiveApp(sapaApp);
					SapaAppInfo appInfo = this.mSapaAppService.getActiveApp(sapaApp);
        
					if (appInfo.PackageName.Equals(mInfo.mMainApplicationPackage))
					{
						if (mMainDeviceLayout == null)
						{
							post(() =>
							{
        
								createMainAppLayout(appInfo, false);
							});
        
						}
						return;
					}
					DeviceActionsLayoutData data = new DeviceActionsLayoutData(appInfo, Context, isMultiInstance(appInfo));
					this.mDevicesActions[sapaApp.InstanceId] = data;
					mInfo.mDevsExpanded[sapaApp.InstanceId] = false;
				}
				catch (SapaConnectionNotSetException)
				{
					Log.w(TAG, "AppInfo of activated instance " + sapaApp.InstanceId + " could not be obtained");
				}
				catch (NameNotFoundException)
				{
					Log.w(TAG, "SapaAppInfo of activated instance " + sapaApp.InstanceId + " could not be obtained as it's icon could not be found");
				}
				ControlBar.this.refresh();
				this.logActive();
			}
		}

		private bool isMultiInstance(SapaAppInfo appInfo)
		{
			if (!appInfo.MultiInstanceEnabled)
			{
				return false;
			}
			IList<SapaAppInfo> list = null;
			try
			{
				list = mSapaAppService.AllActiveApp;
				foreach (SapaAppInfo info in list)
				{
					if (info.App.InstanceId.Equals(appInfo.App.InstanceId))
					{
						continue;
					}
					if (info.PackageName.Equals(appInfo.PackageName))
					{
						return true;
					}
				}
			}
			catch (SapaConnectionNotSetException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}

			return false;
		}

		private void logActive()
		{
			lock (this)
			{
				Log.d(TAG, "\t\tActive:");
				foreach (string id in this.mActionsLayouts.Keys)
				{
					Log.d(TAG, "\t\t\t\t\t" + id);
				}
			}
		}

		public override void onAppDeactivated(SapaApp sapaApp)
		{
			lock (this)
			{
				Log.d(TAG, "onAppDeactivated &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
				if (mMainDeviceLayout != null && (DeviceActionsLayoutData) mMainDeviceLayout.Tag != null && ((DeviceActionsLayoutData) mMainDeviceLayout.Tag).mSapaApp != null && ((DeviceActionsLayoutData) mMainDeviceLayout.Tag).mSapaApp.InstanceId != null && sapaApp != null && ((DeviceActionsLayoutData) mMainDeviceLayout.Tag).mSapaApp.InstanceId.Equals(sapaApp.InstanceId))
				{
					Log.d(TAG, "main to nul");
					mRemoveMain = true;
					post(() =>
					{
        
						updateMainLayout();
					});
        
				}
				// this.mActionsLayouts.remove(instanceId);
				if (sapaApp != null)
				{
					if (mDevicesActions != null)
					{
						this.mDevicesActions.Remove(sapaApp.InstanceId);
					}
					if (mInfo != null && mInfo.mDevsExpanded != null)
					{
						mInfo.mDevsExpanded.Remove(sapaApp.InstanceId);
					}
				}
				if (Context != null)
				{
					try
					{
					((Activity) Context).runOnUiThread(() =>
					{
						lock (ControlBar.this)
						{
							ControlBar.this.updateBar();
							ControlBar.this.fillBar();
							if (ControlBar.this.mDevicesActions.Count == 0)
							{
								hide();
							}
						}
					});
					}
					catch (System.NullReferenceException)
					{
						;
					}
				}
        
				this.logActive();
			}
		}

		public override void onAppChanged(SapaApp sapaApp)
		{
			lock (this)
			{
				Log.d(TAG, "onInstanceChanged &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        
				SapaAppInfo appInfo;
				try
				{
					appInfo = ControlBar.this.mSapaAppService.getActiveApp(sapaApp);
					DeviceActionsLayoutData data = new DeviceActionsLayoutData(appInfo, Context, isMultiInstance(appInfo));
					if (!sapaApp.PackageName.Equals(mInfo.mMainApplicationPackage))
					{
						ControlBar.this.mDevicesActions[sapaApp.InstanceId] = data;
					}
        
        
					((Activity)Context).runOnUiThread(new Updater(this, sapaApp.InstanceId, data));
        
				}
				catch (SapaConnectionNotSetException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
				catch (NameNotFoundException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		internal class Updater : Runnable
		{
			private readonly ControlBar outerInstance;


			internal string mInstanceId;
			internal DeviceActionsLayoutData mData;

			internal Updater(ControlBar outerInstance, string instanceId, DeviceActionsLayoutData data)
			{
				this.outerInstance = outerInstance;
				this.mInstanceId = instanceId;
				this.mData = data;
			}

			public override void run()
			{
				lock (outerInstance)
				{
					if (mData.mInstancePackageName.Equals(outerInstance.mInfo.mMainApplicationPackage))
					{
						outerInstance.mMainDeviceLayout.updateInfo(mData);
					}
					else if (outerInstance.mActionsLayouts != null && outerInstance.mActionsLayouts[mInstanceId] != null)
					{
						outerInstance.mActionsLayouts[mInstanceId].updateInfo(mData);
					}
				}
			}

		}

		private Runnable mUpdateThread;

		public virtual void refresh()
		{
			mUpdateThread = () =>
			{

				LayoutTransition trans = null;
				updateMainLayout();
				if (mDevicesLayout != null)
				{
					trans = mDevicesLayout.LayoutTransition;
					mDevicesLayout.LayoutTransition = null;
				}
				if (mInfo.mListHidden)
				{
					hide();
				}
				else
				{
					updateBar();
					fillBar();
					show();

					if (mMainDeviceLayout != null)
					{
						mMainDeviceLayout.refresh();
					}
					if (mActionsLayouts != null)
					{
						foreach (DeviceActionsLayout lay in mActionsLayouts.Values)
						{
							lay.refresh();
						}
					}
				}
				mDevicesLayout.LayoutTransition = trans;

				updateDrawables();
				mDevicesLayout.invalidate();
			};
			if (Shown)
			{
				post(mUpdateThread);
				mUpdateThread = null;
			}
		}

		public virtual Info getInfo()
		{
			return mInfo;
		}

		private void checkMainApp()
		{
			if (mSapaAppService == null || mInfo.mMainApplicationPackage == null || mInfo.mMainApplicationPackage.Length == 0)
			{
				return;
			}
			try
			{
				foreach (SapaAppInfo info in mSapaAppService.AllInstalledApp)
				{
					if (info.PackageName.Equals(mInfo.mMainApplicationPackage) && info.MultiInstanceEnabled)
					{
						throw new System.InvalidOperationException("Only single instance app can be declareed as Main application for floating controller");
					}
				}
			}
			catch (SapaConnectionNotSetException e)
			{
				Log.w(TAG, "connection was not set");
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		public override void onServiceConnected()
		{
			Log.d(TAG, "onServiceConnected %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
			checkMainApp();
			if (Context != null)
			{
				try
				{
				((Activity) Context).runOnUiThread(() =>
				{
					lock (ControlBar.this)
					{
						ControlBar.this.initializeActions();
						ControlBar.this.updateBar();
						ControlBar.this.fillBar();
					}
				});
				}
				catch (System.NullReferenceException e)
				{
					Log.e(TAG, "Exception:", e);
				}
			}
		}

		public override void onAppInstalled(SapaApp sapaApp)
		{
			checkMainApp();

		}

		public override void onAppUninstalled(SapaApp sapaApp)
		{
			// nothing to be done
		}

		public override void onTransportMasterChanged(SapaApp sapaApp)
		{

		}

		internal class Info
		{
			internal bool mListHidden;
			internal IDictionary<string, bool?> mDevsExpanded;
			internal IDictionary<string, bool?> mCopiedDevs;
			//private boolean mSavedHiddenFlag;
			internal int mHandleDrawableRes;
			internal string mMainApplicationPackage = "";


			public Info(bool listHidden, IDictionary<string, bool?> devicesExpanded, int handleDrawableRes)
			{
				this.mListHidden = listHidden;
				mHandleDrawableRes = (handleDrawableRes != 0 ? handleDrawableRes : R.drawable.arrow_open);
				this.mDevsExpanded = devicesExpanded == null ? this.mDevsExpanded = new Dictionary<string, bool?>() : devicesExpanded;
			}

			public Info()
			{
				this.mListHidden = true;
				this.mDevsExpanded = new Dictionary<string, bool?>();
				this.mHandleDrawableRes = R.drawable.arrow_open;
			}

			public virtual bool isDevExpanded(string dev)
			{
				if (!mDevsExpanded.ContainsKey(dev))
				{
					return false;
				}
				return mDevsExpanded[dev].booleanValue();
			}

			public virtual void setDeviceExpanded(string dev, bool expanded)
			{
				mDevsExpanded[dev] = expanded;
			}

			public virtual void saveState()
			{
				lock (this)
				{
					mCopiedDevs = new Dictionary<string, bool?>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					mCopiedDevs.putAll(mDevsExpanded);
				}
			}

			public virtual void restoreState()
			{
				lock (this)
				{
					if (mCopiedDevs == null)
					{
						return;
					}
					mDevsExpanded = new Dictionary<string, bool?>();
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
					mDevsExpanded.putAll(mCopiedDevs);
				}
			}

		}
	}

}