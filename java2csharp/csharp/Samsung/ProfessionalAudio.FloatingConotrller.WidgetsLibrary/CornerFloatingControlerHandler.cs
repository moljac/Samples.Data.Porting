using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{


	using Activity = android.app.Activity;
	using Context = android.content.Context;
	using TypedArray = android.content.res.TypedArray;
	using DragEvent = android.view.DragEvent;
	using Gravity = android.view.Gravity;
	using View = android.view.View;
	using OnDragListener = android.view.View.OnDragListener;
	using ViewGroup = android.view.ViewGroup;
	using FrameLayout = android.widget.FrameLayout;
	using LayoutParams = android.widget.FrameLayout.LayoutParams;
	using LinearLayout = android.widget.LinearLayout;
	using RelativeLayout = android.widget.RelativeLayout;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;

	internal class CornerFloatingControlerHandler : AbstractFloatingControllerHandler
	{

		private static readonly int HORIZONTAL = LinearLayout.HORIZONTAL;

		public const int ALIGN_PARENT_BOTTOM_RIGHT = 5;
		public const int ALIGN_PARENT_BOTTOM_LEFT = 6;
		public const int ALIGN_PARENT_TOP_RIGHT = 4;
		public const int ALIGN_PARENT_TOP_LEFT = 7;

		private const int DEFAULT_POSITION = ALIGN_PARENT_BOTTOM_RIGHT;

		private int mInitOrientation;
		private int mOrientation;

		private int mInitialAlign;
		private int mBarAlignment = -1;

		private IDictionary<int?, FrameLayout> mPanelMap;
		private IDictionary<int?, int?> mGravityMap;

		public CornerFloatingControlerHandler(Activity activity, TypedArray array)
		{
			initControlBar(activity, array);
		}

		public override int LayoutResource
		{
			get
			{
				return R.layout.corner_jam_control_view;
			}
		}

		public override void initLayout(ViewGroup root)
		{
			mMainView = root;
			// layout.addView(this.mControlBar);
			mGravityMap = createGravityMap();
			mPanelMap = createPanelMap(root);

			View dragReceiver = root.findViewById(R.id.for_content);

			View.OnDragListener dragListener = new MyDropListener(this);
			if (dragReceiver != null)
			{
				dragReceiver.OnDragListener = dragListener;
			}
			root.OnDragListener = dragListener;

			mBarAlignment = mPanelMap.ContainsKey(mBarAlignment) ? mBarAlignment : DEFAULT_POSITION;
			//putBarOnBoard(mBarAlignment);
		}

		private SortedDictionary<int?, int?> createGravityMap()
		{
			SortedDictionary<int?, int?> gravityMap = new SortedDictionary<int?, int?>();

			gravityMap[ALIGN_PARENT_BOTTOM_LEFT] = Gravity.BOTTOM | Gravity.LEFT;
			gravityMap[ALIGN_PARENT_BOTTOM_RIGHT] = Gravity.BOTTOM | Gravity.RIGHT;
			gravityMap[ALIGN_PARENT_TOP_LEFT] = Gravity.TOP | Gravity.LEFT;
			gravityMap[ALIGN_PARENT_TOP_RIGHT] = Gravity.TOP | Gravity.RIGHT;

			return gravityMap;
		}

		private SortedDictionary<int?, FrameLayout> createPanelMap(ViewGroup root)
		{
			SortedDictionary<int?, FrameLayout> panelMap = new SortedDictionary<int?, FrameLayout>();

			panelMap[ALIGN_PARENT_BOTTOM_LEFT] = (FrameLayout) root.findViewById(R.id.leftBottomScroll);
			panelMap[ALIGN_PARENT_BOTTOM_RIGHT] = (FrameLayout) root.findViewById(R.id.rightBottomScroll);
			panelMap[ALIGN_PARENT_TOP_LEFT] = (FrameLayout) root.findViewById(R.id.leftTopScroll);
			panelMap[ALIGN_PARENT_TOP_RIGHT] = (FrameLayout) root.findViewById(R.id.rightTopScroll);

			return panelMap;
		}

		protected internal override void initControlBar(Activity activity, TypedArray array)
		{
			base.initControlBar(activity, array);
			mInitOrientation = array.getInt(R.styleable.FloatingControler_orientation, HORIZONTAL);
			int handleDrawable = array.getResourceId(R.styleable.FloatingControler_handle_drawable, 0);
			mBarAlignment = array.getInt(R.styleable.FloatingControler_bar_alignment, DEFAULT_POSITION);
			prepareBarState();
			try
			{
				mControlBar = createControlBar(activity, handleDrawable);
			}
			catch (NotActivityException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private void prepareBarState()
		{
			mOrientation = mInitOrientation;
		}

		/// <summary>
		/// This method sets bar on an appropriate border.
		/// </summary>
		/// <param name="alignment">
		///            This parameter shall be RelativeLayout alignment. It indicates on which border
		///            floating controller shall be placed initially. Its value ma be:
		///            RelativeLayout.ALIGN_PARENT_LEFT, RelativeLayout.ALIGN_PARENT_RIGHT,
		///            RelativeLayout.ALIGN_PARENT_TOP, RelativeLayout.ALIGN_PARENT_BOTTOM. </param>
		private void putBarOnBoard(int alignment)
		{
			FrameLayout hLayout = mPanelMap[alignment];
			View CurrentChild = null;

			if (hLayout != null)
			{
				CurrentChild = hLayout.getChildAt(0);
			}

			if (this.mBarAlignment == alignment && CurrentChild != null && CurrentChild.Visibility == View.VISIBLE)
			{
				return;
			}

			ControlBar visibleBar = null;
			FrameLayout.LayoutParams @params;

			if (hLayout != null)
			{
				FrameLayout parentLayout = null;
				int size = mControlBar.Context.Resources.getDimensionPixelSize(R.dimen.default_controlbar_thickness);

				mBarAlignment = alignment;
				mOrientation = HORIZONTAL;
				visibleBar = mControlBar;

				@params = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WRAP_CONTENT, size, mGravityMap[alignment]);

				if (visibleBar != null)
				{
					parentLayout = (FrameLayout) visibleBar.Parent;
				}

				if (parentLayout != null)
				{
					parentLayout.removeView(visibleBar);
				}

				hLayout.addView(visibleBar, @params);

				if (visibleBar != null)
				{
					visibleBar.CurrentPosition = mBarAlignment;
					visibleBar.Visibility = View.INVISIBLE;
					visibleBar.getChildAt(0).forceLayout();
				}
			}

			if (visibleBar != null)
			{
				visibleBar.CurrentPosition = mBarAlignment;
				visibleBar.Visibility = View.VISIBLE;
				visibleBar.expand();
			}
		}

		// because it is required to suppress
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") private int getAlignment(android.content.res.TypedArray array)
		private int getAlignment(TypedArray array)
		{
			if (array == null)
			{
				return RelativeLayout.ALIGN_PARENT_TOP;
			}
			int alignment = array.getInt(R.styleable.FloatingControler_bar_alignment, 0);
			return alignment;
		}

		/// <summary>
		/// MyDropListener is responsible for moving controlBar between borders of FloatingControler.
		/// 
		/// </summary>
		private sealed class MyDropListener : View.OnDragListener
		{
			private readonly CornerFloatingControlerHandler outerInstance;

			public MyDropListener(CornerFloatingControlerHandler outerInstance)
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
					}
					else if (DragEvent.ACTION_DRAG_ENDED == @event.Action && !@event.Result)
					{
						outerInstance.putBarOnBoard(outerInstance.mInitialAlign);
					}
					return true;
				}
				return false;
			}
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private ControlBar createControlBar(android.content.Context activity, int handleDrawable) throws NotActivityException
		private ControlBar createControlBar(Context activity, int handleDrawable)
		{

			ControlBar.Info info = new ControlBar.Info(true, null, handleDrawable);
			ControlBar controlBar = new CornerControlBar(activity, info);

			IDictionary<int?, int?> map = new SortedDictionary<int?, int?>();
			map[LinearLayout.HORIZONTAL] = android.R.color.transparent;

			controlBar.EndImageMap = map;
			controlBar.Orientation = LinearLayout.HORIZONTAL;
			controlBar.CurrentPosition = mBarAlignment;

			return controlBar;
		}

		/// <summary>
		/// This method checks on which border control bar shall be placed.
		/// </summary>
		/// <param name="x">
		///            X-coordinate of the place where control bar was released. </param>
		/// <param name="y">
		///            Y-coordinate of the place where control bar was released. </param>
		/// <returns> RelativeLayout alignment value of where the bar shall be placed. </returns>
		protected internal override int getBorder(float x, float y)
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

		public override void stopConnections()
		{
			base.stopConnections();
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
			mOrientation = HORIZONTAL;
			mBarAlignment = barAlignment;
			mInitOrientation = mOrientation;
			prepareBarState();
			putBarOnBoard(mOrientation + mBarAlignment);
		}

		public override void setSapaAppService(SapaAppService sapaAppService, string mainPackagename)
		{

			if (mInitOrientation == HORIZONTAL)
			{
				putBarOnBoard(mBarAlignment);
				mInitialAlign = mBarAlignment;
			}
			else
			{
				putBarOnBoard(mBarAlignment);
			}
			base.setSapaAppService(sapaAppService, mainPackagename);
		}
	}

}