namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using LinearLayout = android.widget.LinearLayout;

	/// <summary>
	/// This class is a LinearLayout which displays its children in reverse order when its orientation is
	/// vertical and layoutdirection is RTL
	/// </summary>
	public class ReversableLinearLayout : LinearLayout
	{

		public ReversableLinearLayout(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
		}

		public ReversableLinearLayout(Context context) : base(context)
		{
		}

		public ReversableLinearLayout(Context context, AttributeSet attrs) : base(context, attrs)
		{
		}

		/// <summary>
		/// IMPORTANT
		/// When the orientation is vertical and layoutdirection is RTL this method returns view counting
		/// from the end. For example when index is 0 then last view is returnd.
		/// </summary>
		public override View getChildAt(int index)
		{
			if (Orientation == LinearLayout.VERTICAL && LayoutDirection == ViewGroup.LAYOUT_DIRECTION_RTL)
			{
				index = ChildCount - index - 1;
			}
			return base.getChildAt(index);
		}

	}

}