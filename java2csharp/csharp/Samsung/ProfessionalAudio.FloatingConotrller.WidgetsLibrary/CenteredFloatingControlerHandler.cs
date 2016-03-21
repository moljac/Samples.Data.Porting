using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{


	using Activity = android.app.Activity;
	using TypedArray = android.content.res.TypedArray;
	using DragEvent = android.view.DragEvent;
	using View = android.view.View;
	using OnDragListener = android.view.View.OnDragListener;
	using ViewGroup = android.view.ViewGroup;
	using FrameLayout = android.widget.FrameLayout;
	using LinearLayout = android.widget.LinearLayout;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;

	internal class CenteredFloatingControlerHandler : AbstractFloatingControllerHandler
	{

		private int mBarAlignment = ALIGN_PARENT_TOP;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private final static String TAG = "professionalaudioconnection:widget:j:" + CenteredFloatingControlerHandler.class.getName();
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly string TAG = "professionalaudioconnection:widget:j:" + typeof(CenteredFloatingControlerHandler).FullName;

		private IDictionary<int?, ViewGroup> mPanelMap;

		public const int ALIGN_PARENT_TOP = 0;
		public const int ALIGN_PARENT_RIGHT = 1;
		public const int ALIGN_PARENT_LEFT = 2;
		public const int ALIGN_PARENT_BOTTOM = 3;

		public CenteredFloatingControlerHandler(Activity activity, TypedArray array)
		{
			initControlBar(activity, false, null, array);
		}

		public override int LayoutResource
		{
			get
			{
				return R.layout.jam_control_view;
			}
		}

		//in this version there is no main app
		public override void setSapaAppService(SapaAppService sapaAppService, string mainPackagename)
		{
			base.setSapaAppService(sapaAppService, mainPackagename);

		}

		public override int BarAlignment
		{
			get
			{
				return mBarAlignment;
			}
		}

		public override void loadBarState(FloatingController floatingController, int barAlignment)
		{
			mBarAlignment = barAlignment;
			putBarOnBoard(mBarAlignment);
		}

		public override void initLayout(ViewGroup root)
		{

			FrameLayout layout = (FrameLayout) root.findViewById(R.id.topScroll);
			layout.addView(this.mControlBar);
			mMainView = root;
			mPanelMap = new SortedDictionary<int?, ViewGroup>();
			mPanelMap[ALIGN_PARENT_BOTTOM] = (ViewGroup) root.findViewById(R.id.bottomScroll);
			mPanelMap[ALIGN_PARENT_TOP] = (ViewGroup) root.findViewById(R.id.topScroll);
			mPanelMap[ALIGN_PARENT_LEFT] = (ViewGroup) root.findViewById(R.id.leftScroll);
			mPanelMap[ALIGN_PARENT_RIGHT] = (ViewGroup) root.findViewById(R.id.rightScroll);
			View dragReceiver = root.findViewById(R.id.for_content);

			(dragReceiver != null ? dragReceiver : root).OnDragListener = new MyDropListener(this);
			mBarAlignment = mPanelMap.ContainsKey(mBarAlignment) ? mBarAlignment : ALIGN_PARENT_TOP;
			putBarOnBoard(mBarAlignment);
		}

		protected internal virtual void initControlBar(Activity activity, bool barExpanded, IDictionary<string, bool?> devicesExpanded, TypedArray array)
		{
			base.initControlBar(activity, array);
			int handleDrawable = array.getResourceId(R.styleable.FloatingControler_handle_drawable, 0);
			mBarAlignment = array.getInt(R.styleable.FloatingControler_bar_alignment, ALIGN_PARENT_TOP);
			ControlBar.Info info = new ControlBar.Info(true, null, handleDrawable);
			try
			{
				this.mControlBar = new ControlBarAnonymousInnerClassHelper(this, activity, info);
				IDictionary<int?, int?> map = new SortedDictionary<int?, int?>();
				map[LinearLayout.HORIZONTAL] = R.drawable.ctrl_action_horiz_end;
				map[LinearLayout.VERTICAL] = R.drawable.ctrl_action_vert_end;
				mControlBar.EndImageMap = map;
			}
			catch (NotActivityException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private class ControlBarAnonymousInnerClassHelper : ControlBar
		{
			private readonly CenteredFloatingControlerHandler outerInstance;

			public ControlBarAnonymousInnerClassHelper(CenteredFloatingControlerHandler outerInstance, Activity activity, com.samsung.android.sdk.professionalaudio.widgets.ControlBar.Info info) : base(activity, info)
			{
				this.outerInstance = outerInstance;
			}


			internal override void addSubViews<T1>(IList<T1> subViews, View view) where T1 : android.view.View
			{
				int index = mDevicesLayout.indexOfChild(view);
				if (index == -1)
				{
					return;
				}
				for (int i = 0; i < subViews.Count; ++i)
				{
					View v = subViews[i];
					if (mDevicesLayout.indexOfChild(v) != -1)
					{
						mDevicesLayout.removeView(v);
					}
					//v.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH, DeviceActionsLayout.BUTTON_HEIGHT));
					mDevicesLayout.addView(v, ++index);
				}
			}

			internal override void clearSubViews(int count, View view)
			{
				int index = mDevicesLayout.indexOfChild(view);
				if (index == -1)
				{
					return;
				}
				mDevicesLayout.removeViews(index + 1, count);
			}

			public override void onServiceDisconnected()
			{
			}

		}

		/// <summary>
		/// This method sets bar on an appriopriate border.
		/// </summary>
		/// <param name="aligment">
		///            This parameter shall be RelativeLayout aligment. It indicates
		///            on which border floating controler shall be placed initially.
		///            Its value ma be: RelativeLayout.ALIGN_PARENT_LEFT,
		///            RelativeLayout.ALIGN_PARENT_RIGHT,
		///            RelativeLayout.ALIGN_PARENT_TOP,
		///            RelativeLayout.ALIGN_PARENT_BOTTOM. </param>
		private void putBarOnBoard(int aligment)
		{
			ViewGroup layout = mPanelMap[aligment];

			if (layout != null)
			{

				ViewGroup parentLayout = (ViewGroup) this.mControlBar.Parent;
				if (parentLayout != null)
				{
					parentLayout.removeView(this.mControlBar);
				}
				if (aligment == ALIGN_PARENT_TOP || aligment == ALIGN_PARENT_BOTTOM)
				{
					this.mControlBar.Orientation = LinearLayout.HORIZONTAL;
				}
				else
				{
					this.mControlBar.Orientation = LinearLayout.VERTICAL;
				}
				if (layout != null)
				{
					layout.addView(this.mControlBar);
				}

			}
		}

		/// <summary>
		/// MyDropListener is responsible for moving controlBar between borders of
		/// FloatingControler.
		/// 
		/// </summary>
		private sealed class MyDropListener : View.OnDragListener
		{
			private readonly CenteredFloatingControlerHandler outerInstance;

			public MyDropListener(CenteredFloatingControlerHandler outerInstance)
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

						int aligment = outerInstance.getBorder(x, y);
						outerInstance.putBarOnBoard(aligment);
					}
					else if (DragEvent.ACTION_DRAG_ENDED == @event.Action && !@event.Result)
					{
						outerInstance.putBarOnBoard(ALIGN_PARENT_TOP);
					}
					return true;
				}
				return false;
			}

		}

		/// <summary>
		/// This method checks on which border control bar shall be placed.
		/// </summary>
		/// <param name="x">
		///            X-coordinate of the place where control bar was released. </param>
		/// <param name="y">
		///            Y-coordinate of the place where control bar was released. </param>
		/// <returns> RelativeLayout aligment value of where the basr shall be placed. </returns>
		protected internal override int getBorder(float x, float y)
		{
			float layoutWidth = (float) mMainView.Width;
			float layoutHeight = (float) mMainView.Height;

			bool toLeft = x < (layoutWidth / 2);
			bool toTop = y < (layoutHeight / 2);

			float horizontalDelta = toLeft ? x : layoutWidth - x;
			float verticalDelta = toTop ? y : layoutHeight - y;

			return horizontalDelta < verticalDelta ? (toLeft ? ALIGN_PARENT_LEFT : ALIGN_PARENT_RIGHT) : (toTop ? ALIGN_PARENT_TOP : ALIGN_PARENT_BOTTOM);
		}
	}

}