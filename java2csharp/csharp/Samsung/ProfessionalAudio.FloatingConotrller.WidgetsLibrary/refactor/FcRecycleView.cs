namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Context = android.content.Context;
	using TypedArray = android.content.res.TypedArray;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;

	/// <summary>
	/// Created by k.betlej on 10/26/15.
	/// </summary>
	public class FcRecycleView : RecyclerView
	{

		public static readonly string TAG = typeof(FcRecycleView).Name;
		private int mMaxWidth;

		public FcRecycleView(Context context) : this(context, null)
		{
		}

		public FcRecycleView(Context context, AttributeSet attrs) : this(context, attrs, 0)
		{
		}

		public FcRecycleView(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			if (attrs != null)
			{
				TypedArray array = context.obtainStyledAttributes(attrs, R.styleable.FcRecycleView, 0, 0);
				mMaxWidth = array.getLayoutDimension(R.styleable.FcRecycleView_max_width, LayoutParams.MATCH_PARENT);
				array.recycle();
			}
		}

		protected internal override void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.onMeasure(widthMeasureSpec, heightMeasureSpec);
			int measureWidth = MeasuredWidth;
			if (mMaxWidth != LayoutParams.MATCH_PARENT && measureWidth > mMaxWidth)
			{
				Log.d(TAG, "width:" + measureWidth);
				Log.d(TAG, "max width:" + mMaxWidth);
				setMeasuredDimension(mMaxWidth, MeasuredHeight);
			}
		}
	}

}