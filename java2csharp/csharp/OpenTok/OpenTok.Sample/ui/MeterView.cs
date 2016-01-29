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
	using MotionEventCompat = android.support.v4.view.MotionEventCompat;
	using AttributeSet = android.util.AttributeSet;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;

	public class MeterView : View
	{

		private Context mContext;

		private float mValue = 0;
		private Paint mPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
		private Paint mPaintGradient = new Paint(Paint.ANTI_ALIAS_FLAG);
		private Rect mBounds = new Rect();
		private Bitmap mIconOn;
		private Bitmap mIconOff;
		private bool mMute = false;
		private OnClickListener mListener;

		public interface OnClickListener
		{
			void onClick(MeterView view);
		}

		protected internal override void onDraw(Canvas canvas)
		{
			canvas.drawCircle(mBounds.centerX(), mBounds.centerY(), mBounds.width() * 0.5f, mPaint);

			if (!mMute)
			{
				if (mIconOn != null)
				{
					canvas.drawBitmap(mIconOn, mBounds.centerX() - mIconOn.Width * 0.5f, mBounds.centerY() - mIconOn.Height * 0.5f, mPaint);
				}
				canvas.drawCircle(mBounds.centerX(), mBounds.centerY(), mBounds.width() * 0.5f * mValue, mPaintGradient);

			}
			else
			{
				if (mIconOff != null)
				{
					canvas.drawBitmap(mIconOff, mBounds.centerX() - mIconOff.Width * 0.5f, mBounds.centerY() - mIconOff.Height * 0.5f, mPaint);
				}
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
			mPaintGradient.Shader = new RadialGradient(w / 2, h / 2, h / 2, 0xff98CE00, 0x8098CE00, TileMode.CLAMP);

		}

		private void init()
		{
			mPaint.Style = Paint.Style.FILL;
			mPaint.Color = 0xff1f1f1f;
			mPaintGradient.Style = Paint.Style.FILL;
			mPaintGradient.Color = 0xff98CE00;
		}

		public MeterView(Context context) : base(context)
		{
			mContext = context;
			init();
		}

		public MeterView(Context context, AttributeSet attrs) : base(context, attrs)
		{
			mContext = context;
			init();
		}

		public virtual void setIcons(Bitmap iconOn, Bitmap iconOff)
		{
			mIconOn = iconOn;
			mIconOff = iconOff;
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

		public override bool onTouchEvent(MotionEvent @event)
		{
			int action = MotionEventCompat.getActionMasked(@event);

			switch (action)
			{
				case (MotionEvent.ACTION_DOWN):
					mMute = !mMute;
					if (mListener != null)
					{
						mListener.onClick(this);
					}
					return true;
				default:
					return base.onTouchEvent(@event);
			}
		}

		public virtual bool Muted
		{
			get
			{
				return mMute;
			}
		}

		public virtual void setOnClickListener(OnClickListener listener)
		{
			mListener = listener;
		}
	}

}