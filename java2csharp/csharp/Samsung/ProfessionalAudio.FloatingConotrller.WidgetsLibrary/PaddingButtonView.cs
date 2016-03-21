namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using Canvas = android.graphics.Canvas;
	using AttributeSet = android.util.AttributeSet;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;

	public class PaddingButtonView : ImageButton
	{

		private int mFloatingPaddingLeft = -1;
		private int mFloatingPaddingRight = -1;
		private int mFloatingPaddingTop = -1;
		private int mFloatingPaddingBottom = -1;

		public PaddingButtonView(Context context) : base(context)
		{
		   // init();
		}

		public PaddingButtonView(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			//init();
		}

		public PaddingButtonView(Context context, AttributeSet attrs) : base(context, attrs)
		{
			//init();
		}

		private void init()
		{
			if (mFloatingPaddingBottom != -1 && mFloatingPaddingLeft != -1 && mFloatingPaddingRight != -1 && mFloatingPaddingTop != -1)
			{
				return;
			}
			mFloatingPaddingLeft = base.PaddingLeft;
			mFloatingPaddingRight = base.PaddingRight;
			mFloatingPaddingTop = base.PaddingTop;
			mFloatingPaddingBottom = base.PaddingBottom;
		}

		protected internal override void onDraw(Canvas canvas)
		{
			init();
			resetPaddings();
			base.onDraw(canvas);
		}

		private void resetPaddings()
		{
			if (!(Parent is LinearLayout))
			{
				return;
			}
			LinearLayout parent = (LinearLayout)Parent.Parent;
			if (parent.Orientation == LinearLayout.HORIZONTAL && parent.LayoutDirection == LAYOUT_DIRECTION_LTR)
			{
				base.setPadding(mFloatingPaddingLeft, mFloatingPaddingTop, mFloatingPaddingRight, mFloatingPaddingBottom);
			}
			else if (parent.Orientation == LinearLayout.HORIZONTAL)
			{
				base.setPadding(mFloatingPaddingRight, mFloatingPaddingTop, mFloatingPaddingLeft, mFloatingPaddingBottom);
			}
			if (parent.Orientation == LinearLayout.VERTICAL && parent.LayoutDirection == LAYOUT_DIRECTION_RTL)
			{
				base.setPadding(mFloatingPaddingBottom, mFloatingPaddingRight, mFloatingPaddingTop, mFloatingPaddingLeft);
			}
			else if (parent.Orientation == LinearLayout.VERTICAL)
			{
				base.setPadding(mFloatingPaddingBottom, mFloatingPaddingLeft, mFloatingPaddingTop, mFloatingPaddingRight);
			}
			invalidate();
		}
	}

}