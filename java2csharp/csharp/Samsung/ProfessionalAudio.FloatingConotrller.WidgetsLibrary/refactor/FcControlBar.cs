using System.Collections.Generic;
using System.Threading;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Activity = android.app.Activity;
	using ClipData = android.content.ClipData;
	using Context = android.content.Context;
	using PackageManager = android.content.pm.PackageManager;
	using TypedArray = android.content.res.TypedArray;
	using Bundle = android.os.Bundle;
	using Looper = android.os.Looper;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;
	using DragEvent = android.view.DragEvent;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;
	using RelativeLayout = android.widget.RelativeLayout;

	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using SapaConnectionNotSetException = com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;

	using LinearLayoutManager = org.solovyev.android.views.llm.LinearLayoutManager;

	/// <summary>
	/// @brief Floating controller ControlBar view class
	/// </summary>
	public class FcControlBar : LinearLayout, FcContext.FcContextStateChanged
	{

		private FcAdapter mAdapter;

		/// <summary>
		/// Class responsible for responding on long click.
		/// </summary>
		private sealed class FcControlBarLongClickListener : OnLongClickListener
		{
			private readonly FcControlBar outerInstance;

			public FcControlBarLongClickListener(FcControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool onLongClick(View view)
			{
				ClipData data = ClipData.newPlainText("", "");
				DragShadowBuilder shadowBuilder = new View.DragShadowBuilder(outerInstance.mMainAppImage);
				view.startDrag(data, shadowBuilder, outerInstance.mMainAppImage, 0);
				return true;
			}
		}

		public const int ALIGN_PARENT_BOTTOM_RIGHT = 5;
		public const int ALIGN_PARENT_BOTTOM_LEFT = 6;
		public const int ALIGN_PARENT_TOP_RIGHT = 4;
		public const int ALIGN_PARENT_TOP_LEFT = 7;
		public const string FLOATING_CONTROLLER_PREFERENCES = "floating_controller";
		public const string BAR_ALIGNMENT_PREF_KEY = "bar_alignment";

		private ViewGroup mMainView;
		private int mBarAlignment;
		private int mBarInitialAlignment;
		private int mOrientation;

		private static readonly string TAG = typeof(FcControlBar).Name;
		private readonly FcContext mFcContext;
		private RecyclerView mRecyclerView;
		private FcSapaServiceConnector mFcSapaConnector;
		private FcModel mModel;
		private ImageButton mBarHandler;
		private LinearLayout mRoot;
		private ImageButton mMainAppImage;
		private bool mListHidden = true;
		private FcAnimator mFcAnimator;
		private float mDevicesActionLayoutWidth;
		private SapaAppInfo mSelectedApp;


		public FcControlBar(Context context) : this(context, null)
		{
		}

		public FcControlBar(Context context, AttributeSet attrs) : this(context, attrs, 0)
		{
		}

		public FcControlBar(Context context, AttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{

			mFcContext = new FcContext(context);
			mFcContext.FxContextStateChangeListener = this;
			FcActionFactory factory = new FcActionFactory(mFcContext);
			mFcContext.ActionFactory = factory;

			initView();
		}

		private void initView()
		{
			LayoutInflater inflater = (LayoutInflater) Context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			inflater.inflate(R.layout.fc_control_bar, this, true);
			mRoot = (LinearLayout) findViewById(R.id.control_bar_root_layout);
			mRecyclerView = (RecyclerView)findViewById(R.id.devices_layout);

			FcItemAnimator itemAnimator = new FcItemAnimator(mRecyclerView);
			itemAnimator.AddDuration = FcConstants.DEFAULT_ANIM_DURATION;
			itemAnimator.ChangeDuration = FcConstants.DEFAULT_ANIM_DURATION;
			itemAnimator.MoveDuration = FcConstants.DEFAULT_ANIM_DURATION;
			itemAnimator.RemoveDuration = FcConstants.DEFAULT_ANIM_DURATION;

			mRecyclerView.ItemAnimator = itemAnimator;
			mModel = new FcModel(mFcContext);
			mAdapter = new FcAdapter(mFcContext, mModel);

			//
			// Temporary workaround for a bug in RecyclerView:
			//      https://code.google.com/p/android/issues/detail?id=74772
			//
			// This custom LinearLayoutManager is on Apache 2.0 licence
			//
			// Fix this when it's fixed ("targeted early 2016")
			//
			LinearLayoutManager layoutManager = new LinearLayoutManager(Context);
			layoutManager.Orientation = LinearLayoutManager.HORIZONTAL;
			mRecyclerView.LayoutManager = layoutManager;
			mRecyclerView.Adapter = mAdapter;

			mBarHandler = (ImageButton)findViewById(R.id.barhandler);
			mBarHandler.BackgroundResource = R.drawable.arrow_open;
			mBarHandler.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			mMainAppImage = (ImageButton)findViewById(R.id.main_app_image);
			mMainAppImage.OnLongClickListener = new FcControlBarLongClickListener(this);
			mFcAnimator = new FcAnimator();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly FcControlBar outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(FcControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (!outerInstance.mListHidden)
				{
					outerInstance.hide();
				}
				else
				{
					outerInstance.expand();
				}
				outerInstance.mListHidden = !outerInstance.mListHidden;
			}
		}

		private void expand()
		{
			mFcAnimator.createExpandAnimator(mRecyclerView, (int) mDevicesActionLayoutWidth, 300).start();
			mBarHandler.Activated = true;
			mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;
		}

		private void hide()
		{
			mFcAnimator.createCollapseAnimator(mRecyclerView, 300).start();
			mBarHandler.Activated = false;
			mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;
		}

		private void hideNoAnimation()
		{
			mBarHandler.Activated = false;
			mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;

			mListHidden = true;

			mAdapter.hideExpanded();

			mRecyclerView.LayoutParams.width = 0;
			mRecyclerView.Visibility = View.GONE;
			mRecyclerView.requestLayout();
		}

		/// <summary>
		/// @brief This method was c </summary>
		/// <param name="root"> </param>
		/// <param name="array"> </param>
		public virtual void prepareView(View root, TypedArray array)
		{
			View dragReceiver = root.findViewById(R.id.for_content);
			OnDragListener dragListener = new OnDragListenerAnonymousInnerClassHelper(this);

			int barAlignment = array.getInt(R.styleable.FloatingControler_bar_alignment, ALIGN_PARENT_BOTTOM_RIGHT);
			mBarInitialAlignment = barAlignment;
			if (dragReceiver != null)
			{
				dragReceiver.OnDragListener = dragListener;
			}

			root.OnDragListener = dragListener;
			mMainView = (ViewGroup) root;
			mMainAppImage.Id = Context.GetHashCode();
			mDevicesActionLayoutWidth = mFcContext.getDimension(R.dimen.floating_controller_actions_length);

			putBarOnBoard(barAlignment);
		}

		private class OnDragListenerAnonymousInnerClassHelper : OnDragListener
		{
			private readonly FcControlBar outerInstance;

			public OnDragListenerAnonymousInnerClassHelper(FcControlBar outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override bool onDrag(View v, DragEvent @event)
			{
				if ((@event.LocalState is View) && ((View) @event.LocalState).Id == v.Context.GetHashCode())
				{
					if (DragEvent.ACTION_DROP == @event.Action)
					{
						float x = @event.X;
						float y = @event.Y;

						int alignment = outerInstance.getBorder(x, y);
						outerInstance.putBarOnBoard(alignment);
						outerInstance.mRecyclerView.scrollToPosition(0);

					}
					else if (DragEvent.ACTION_DRAG_ENDED == @event.Action && !@event.Result)
					{
						outerInstance.putBarOnBoard(outerInstance.mBarAlignment);
					}
					return true;
				}
				return false;
			}
		}

		/// <summary>
		/// @brief Method return bar alignment of Control Bar
		/// </summary>
		/// <returns> one of value:
		///      ALIGN_PARENT_BOTTOM_RIGHT,
		///      ALIGN_PARENT_BOTTOM_LEFT,
		///      ALIGN_PARENT_TOP_RIGHT,
		///      ALIGN_PARENT_TOP_LEFT </returns>
		public virtual int BarAlignment
		{
			get
			{
				return mBarAlignment;
			}
		}

		/// <summary>
		/// @brief This method load bar state
		/// </summary>
		/// <param name="floatingController"> FloatingController </param>
		/// <param name="barAlignment"> int </param>
		public virtual void loadBarState(FloatingController floatingController, int barAlignment)
		{
			Log.d(TAG, "loadBarState(fc, align=" + barAlignment);
			putBarOnBoard(barAlignment);
		}

		/// <summary>
		/// @brief TODO!
		/// 
		/// </summary>
		public virtual void stopConnections()
		{
			//TODO Implement method body
		}

		/// <summary>
		/// @brief This method return state of floating controller
		/// 
		/// </summary>
		/// <returns> true - if bar is expanded, false - if bar is hidden </returns>
		public virtual bool BarExpanded
		{
			get
			{
				return mListHidden;
			}
		}

		/// <summary>
		/// @brief TODO!
		/// 
		/// @return
		/// </summary>
		public virtual IDictionary<string, bool?> DevicesExpanded
		{
			get
			{
				//TODO Implement method body
				return null;
			}
		}

		/// <summary>
		/// @brief TODO!
		/// </summary>
		/// <param name="sapaAppService"> </param>
		/// <param name="mainPackage"> </param>
		public virtual void setSapaAppService(SapaAppService sapaAppService, string mainPackage)
		{
			Log.d(TAG, "setSapaAppService(" + mainPackage + ")");
			mFcSapaConnector = new FcSapaServiceConnector(mModel, sapaAppService, mainPackage);
			mFcContext.SapaServiceConnector = mFcSapaConnector;
			MainApp = mFcSapaConnector.MainApp;
			SapaAppInfo sapaAppInfo = SapaAppInfo.getAppInfo(((Activity)Context).Intent);
			if (sapaAppInfo != null)
			{
				SelectedApp = sapaAppInfo;
			}
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		/// <param name="mainApp"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void setMainApp(final com.samsung.android.sdk.professionalaudio.app.SapaAppInfo mainApp)
		public virtual SapaAppInfo MainApp
		{
			set
			{
				if (value == null)
				{
					Log.v(TAG, "Main app is null");
					return;
				}
				Log.d(TAG, "setMainApp(" + value.App.InstanceId + ")");
				if (Looper.MainLooper.Thread != Thread.CurrentThread)
				{
					post(() =>
					{
						try
						{
							Log.d(TAG, "set main app icon");
							mMainAppImage.ImageDrawable = value.getIcon(Context);
						}
						catch (PackageManager.NameNotFoundException e)
						{
							Log.e(TAG, "Exception", e);
						}
					});
				}
				else
				{
					try
					{
						Log.d(TAG, "set main app icon");
						mMainAppImage.ImageDrawable = value.getIcon(Context);
					}
					catch (PackageManager.NameNotFoundException e)
					{
						Log.e(TAG, "Exception", e);
					}
				}
			}
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		/// <param name="appInfo"> </param>
		public virtual SapaAppInfo SelectedApp
		{
			set
			{
				Log.d(TAG, "setSelectedApp(" + value.App.InstanceId + ")");
				Log.d(TAG, "mMode:" + mModel);
				Log.d(TAG, "instanceId:" + value.App.InstanceId);
				Log.d(TAG, "mBarAlignment:" + mBarAlignment);
				if (mModel != null && mModel.ItemCount > 0)
				{
					mModel.ActiveApp = value;
				}
				mSelectedApp = value;
				mFcContext.ActiveApp = mSelectedApp;
				Bundle bundle = value.Configuration;
				if (bundle != null)
				{
					int barAlignment = bundle.getInt(BAR_ALIGNMENT_PREF_KEY, mBarAlignment);
					putBarOnBoard(barAlignment);
				}
				Log.d(TAG, "mBarAlignment:" + mBarAlignment);
			}
		}

		public virtual void onFloatingControllerDetached()
		{
			saveViewState();
		}

		/// <summary>
		/// This method checks on which border control bar shall be placed.
		/// </summary>
		/// <param name="x">
		///            X-coordinate of the place where control bar was released. </param>
		/// <param name="y">
		///            Y-coordinate of the place where control bar was released. </param>
		/// <returns> RelativeLayout alignment value of where the bar shall be placed. </returns>
		protected internal virtual int getBorder(float x, float y)
		{
			float relativeX = x / ((float) mMainView.Width);
			float relativeY = y / ((float) mMainView.Height);

			if (relativeX < 0.5)
			{
				return (relativeY < 0.5) ? ALIGN_PARENT_TOP_LEFT : ALIGN_PARENT_BOTTOM_LEFT;
			}
			else
			{
				return (relativeY < 0.5) ? ALIGN_PARENT_TOP_RIGHT : ALIGN_PARENT_BOTTOM_RIGHT;
			}
		}

		private void putBarOnBoard(int alignment)
		{
			Log.d(TAG, "put on board alignment: " + alignment);

			RelativeLayout.LayoutParams @params = prepareLayoutParams(alignment);
			if (Parent != null)
			{
				mMainView.removeView(this);
			}

			mMainView.addView(this, @params);
			mBarAlignment = alignment;
			mOrientation = HORIZONTAL;
		}

		/// <summary>
		/// @brief This method is used when layout direction was changed from RTL to LTR or LTR to RTL
		/// </summary>
		/// <param name="alignment"> int
		/// @return </param>
		protected internal virtual RelativeLayout.LayoutParams prepareLayoutParams(int alignment)
		{
			RelativeLayout.LayoutParams @params = (RelativeLayout.LayoutParams) this.LayoutParams;
			@params.removeRule(RelativeLayout.ALIGN_PARENT_TOP);
			@params.removeRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
			@params.removeRule(RelativeLayout.ALIGN_PARENT_LEFT);
			@params.removeRule(RelativeLayout.ALIGN_PARENT_RIGHT);
			switch (alignment)
			{
				case ALIGN_PARENT_TOP_LEFT:
					@params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
					@params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
					changeLayoutDirection(LAYOUT_DIRECTION_RTL);
					break;
				case ALIGN_PARENT_TOP_RIGHT:
					@params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
					@params.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
					changeLayoutDirection(LAYOUT_DIRECTION_LTR);
					break;
				case ALIGN_PARENT_BOTTOM_LEFT:
					@params.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
					@params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
					changeLayoutDirection(LAYOUT_DIRECTION_RTL);
					break;
				case ALIGN_PARENT_BOTTOM_RIGHT:
					@params.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
					@params.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
					changeLayoutDirection(LAYOUT_DIRECTION_LTR);
					break;
				default:
					@params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
					@params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
					changeLayoutDirection(LAYOUT_DIRECTION_RTL);
				break;
			}

			return @params;
		}

		private void changeLayoutDirection(int layoutDirection)
		{
			LayoutDirection = layoutDirection;
			mRoot.LayoutDirection = layoutDirection;
			mRecyclerView.LayoutDirection = layoutDirection;
			mModel.changeItemDirection(layoutDirection);
			mBarHandler.Background.Level = Orientation * 2 + LayoutDirection;
		}

		public virtual void reloadView()
		{
			Log.d(TAG, "reloadView()");

			Context context = Context;
			if (null == context || !(context is Activity))
			{
				Log.w(TAG, "Cannot reload view: context needs to be an instance of activity");
				return;
			}
			mBarAlignment = mBarInitialAlignment;
			SapaAppInfo sapaAppInfo = SapaAppInfo.getAppInfo(((Activity) context).Intent);
			Log.d(TAG, "    sapa app info:" + sapaAppInfo);
			if (sapaAppInfo != null)
			{
				hideNoAnimation();
				SelectedApp = sapaAppInfo;
			}
		}

		public virtual void onActivityFinished()
		{
			saveViewState();
		}

		private void saveViewState()
		{
			Log.d(TAG, "saveViewState()");
			Log.d(TAG, "mBarAlignment:" + mBarAlignment);

			if (mSelectedApp != null)
			{
				Log.d(TAG, "instanceId:" + mSelectedApp.App.InstanceId);
				Bundle bundle = mSelectedApp.Configuration;
				if (bundle == null)
				{
					bundle = new Bundle();
				}
				bundle.putInt(BAR_ALIGNMENT_PREF_KEY, mBarAlignment);
				mSelectedApp.Configuration = bundle;
				try
				{
					mFcSapaConnector.SapaAppService.changeAppInfo(mSelectedApp);
				}
				catch (SapaConnectionNotSetException e)
				{
					Log.e(TAG, "Exception", e);
				}
			}
		}
	}

}