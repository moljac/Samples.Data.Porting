namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using ColorFilter = android.graphics.ColorFilter;
	using Paint = android.graphics.Paint;
	using PixelFormat = android.graphics.PixelFormat;
	using Rect = android.graphics.Rect;
	using Drawable = android.graphics.drawable.Drawable;
	using ShapeDrawable = android.graphics.drawable.ShapeDrawable;

	/// <summary>
	/// Created by k.betlej on 9/23/15.
	/// </summary>
	public class TextDrawable : Drawable
	{

		private readonly Paint mPaint;
		private readonly string mText;

		public TextDrawable(string text, int textSize, int textColor)
		{
			mText = text;
			mPaint = new Paint();
			mPaint.Color = textColor;
			mPaint.TextSize = textSize;
			mPaint.AntiAlias = true;
	//        mPaint.setShadowLayer(6f, 0, 0, Color.BLACK);
			mPaint.Style = Paint.Style.FILL;
			mPaint.TextAlign = Paint.Align.CENTER;
		}

		public override void draw(Canvas canvas)
		{
			Rect r = Bounds;
			Paint paint = mPaint;

			// only draw shape if it may affect output
			if (paint.Alpha != 0 || paint.Xfermode != null)
			{
	//            canvas.drawRect(r, paint);
				Rect textBounds = TextBounds;
				canvas.drawText(mText, r.left + r.width() / 2, r.top + r.height() / 2 + textBounds.height() / 2, paint);
			}

			// restore
		}

		public override int Alpha
		{
			set
			{
				mPaint.Alpha = value;
			}
		}

		public override ColorFilter ColorFilter
		{
			set
			{
				mPaint.ColorFilter = value;
			}
		}

		public override int Opacity
		{
			get
			{
				return PixelFormat.TRANSLUCENT;
			}
		}

		public virtual Rect TextBounds
		{
			get
			{
				Rect bounds = new Rect();
				mPaint.getTextBounds(mText, 0, mText.Length, bounds);
				return bounds;
			}
		}
	}

}