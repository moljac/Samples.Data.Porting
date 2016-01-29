using System;

namespace com.opentok.android.demo.ui
{

	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using Canvas = android.graphics.Canvas;
	using Paint = android.graphics.Paint;
	using Style = android.graphics.Paint.Style;
	using RadialGradient = android.graphics.RadialGradient;
	using Rect = android.graphics.Rect;
	using TileMode = android.graphics.Shader.TileMode;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;

	public class AudioLevelView : View
	{

		private Context mContext;
		private float mValue = 0;
		private Paint mPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
		private Paint mPaintGradient = new Paint(Paint.ANTI_ALIAS_FLAG);
		private Rect mBounds = new Rect();
		private Bitmap mHeadset;
		private bool mMute = false;

		public AudioLevelView(Context context) : base(context)
		{
			mContext = context;
			init();
		}

		public AudioLevelView(Context context, AttributeSet attrs) : base(context, attrs)
		{
			mContext = context;
			init();
		}

		private void init()
		{
			mPaint.Style = Paint.Style.FILL;
			mPaint.Color = Resources.getColor(com.opentok.android.demo.opentoksamples.R.color.black_grey);
			mPaintGradient.Style = Paint.Style.FILL;
			mPaintGradient.Color = Resources.getColor(com.opentok.android.demo.opentoksamples.R.color.init_gradient);
		}

		protected internal override void onDraw(Canvas canvas)
		{
			float radius = 500 * 0.5f;
			canvas.drawCircle(Width, 0, radius, mPaint);

			if (!mMute)
			{
				if (mHeadset != null)
				{
					canvas.drawBitmap(mHeadset, (Width - mHeadset.Width - 30), mHeadset.Height * 0.3f, mPaint);
				}
				canvas.drawCircle(Width, 0, radius * mValue, mPaintGradient);
			}
		}

		public virtual Bitmap Icons
		{
			set
			{
				mHeadset = value;
			}
		}

		public virtual float MeterValue
		{
			set
			{
				// Convert linear value to logarithmic
				double db = 20 * Math.Log10(value);
				float floor = -40;
				float level = 0;
				if (db > floor)
				{
					level = (float) db - floor;
					level /= -floor;
				}
				mValue = level;
    
				// force redraw
				invalidate();
			}
		}

		protected internal override void onSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.onSizeChanged(w, h, oldw, oldh);
			mBounds.left = (int)(0 + w * 0.10);
			mBounds.top = (int)(0 + h * 0.10);
			mBounds.right = (int)(w * 0.90);
			mBounds.bottom = (int)(h * 0.90);
			// Update gradient
			mPaintGradient.Shader = new RadialGradient(w / 2, h / 2, h / 2, Resources.getColor(com.opentok.android.demo.opentoksamples.R.color.color0_gradient), Resources.getColor(com.opentok.android.demo.opentoksamples.R.color.color1_gradient), TileMode.CLAMP);

		}


	}

}