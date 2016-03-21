namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Context = android.content.Context;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Paint = android.graphics.Paint;
	using Point = android.graphics.Point;
	using Rect = android.graphics.Rect;
	using AttributeSet = android.util.AttributeSet;
	using Size = android.util.Size;
	using View = android.view.View;

	/// <summary>
	/// A Simple <seealso cref="View"/> that can draw rect.
	/// </summary>
	public class RectView : View
	{

		public const int GRAVITY_START = 0x00000001;
		public const int GRAVITY_TOP = 0x00000002;
		public const int GRAVITY_END = 0x00000004;
		public const int GRAVITY_BOTTOM = 0x00000008;

		public const int GRAVITY_CENTER = 0x00000010;
		public const int GRAVITY_CENTER_VERTICAL = 0x00000020;
		public const int GRAVITY_CENTER_HORIZONTAL = 0x00000040;

		private Paint mPaint;
		private Rect mRect;
		private Point mOffset;
		private int mGravity;

		public RectView(Context context) : this(context, null)
		{
		}

		public RectView(Context context, AttributeSet attrs) : this(context, attrs, 0)
		{
		}

		public RectView(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{
			clearRect();
		}

		public virtual Paint RectPaint
		{
			set
			{
				mPaint = value;
			}
		}

		public virtual void setRectSize(int width, int height)
		{
			mRect.set(mRect.left, mRect.top, mRect.left + width, mRect.top + height);
		}

		public virtual Size RectSize
		{
			get
			{
				return new Size(mRect.width(), mRect.height());
			}
		}

		public virtual int RectGravity
		{
			set
			{
				mGravity = value;
			}
			get
			{
				return mGravity;
			}
		}


		public virtual void setRectOffset(int x, int y)
		{
			mOffset.set(x, y);
		}

		public virtual void clearRect()
		{
			mPaint = new Paint();
			mPaint.Color = Color.BLUE;
			mPaint.Style = Paint.Style.STROKE;
			mPaint.StrokeWidth = 3;

			mRect = new Rect();
			mOffset = new Point();
			mGravity = GRAVITY_START;
		}

		protected internal override void onDraw(Canvas canvas)
		{
			// Setup gravity.
			if ((mGravity & GRAVITY_START) > 0)
			{
				mRect.set(0, mRect.top, mRect.width(), mRect.bottom);
			}
			else if ((mGravity & GRAVITY_TOP) > 0)
			{
				mRect.set(mRect.left, 0, mRect.right, mRect.height());
			}
			else if ((mGravity & GRAVITY_END) > 0)
			{
				mRect.set(canvas.Width - mRect.width(), mRect.top, canvas.Width, mRect.bottom);
			}
			else if ((mGravity & GRAVITY_BOTTOM) > 0)
			{
				mRect.set(mRect.left, canvas.Height - mRect.height(), mRect.right, canvas.Height);
			}

			if ((mGravity & GRAVITY_CENTER) > 0)
			{
				mRect.set((canvas.Width / 2) - (mRect.width() / 2), (canvas.Height / 2) - (mRect.height() / 2), (canvas.Width / 2) - (mRect.width() / 2) + mRect.width(), (canvas.Height / 2) - (mRect.height() / 2) + mRect.height());
			}
			else if ((mGravity & GRAVITY_CENTER_HORIZONTAL) > 0)
			{
				mRect.set((canvas.Width / 2) - (mRect.width() / 2), mRect.top, (canvas.Width / 2) - (mRect.width() / 2) + mRect.width(), mRect.bottom);
			}
			else if ((mGravity & GRAVITY_CENTER_VERTICAL) > 0)
			{
				mRect.set(mRect.left, (canvas.Height / 2) - (mRect.height() / 2), mRect.right, (canvas.Height / 2) - (mRect.height() / 2) + mRect.height());
			}

			// Merge offset.
			mRect.offset(mOffset.x, mOffset.y);

			canvas.drawRect(mRect, mPaint);
		}
	}

}