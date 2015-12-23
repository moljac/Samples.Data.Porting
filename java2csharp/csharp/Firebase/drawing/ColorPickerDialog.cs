using System;

namespace com.firebase.drawing
{

	using Dialog = android.app.Dialog;
	using Context = android.content.Context;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using ColorMatrix = android.graphics.ColorMatrix;
	using Paint = android.graphics.Paint;
	using RectF = android.graphics.RectF;
	using Shader = android.graphics.Shader;
	using SweepGradient = android.graphics.SweepGradient;
	using Bundle = android.os.Bundle;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;

	/// <summary>
	/// @author greg
	/// @since 6/27/13
	/// </summary>
	public class ColorPickerDialog : Dialog
	{

		public interface OnColorChangedListener
		{
			void colorChanged(int newColor);
		}

		private OnColorChangedListener mListener;

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly FirebaseListAdapterAnonymousInnerClassHelper outerInstance;

			private string key;

			public OnClickListenerAnonymousInnerClassHelper(FirebaseListAdapterAnonymousInnerClassHelper outerInstance, string key)
			{
				this.outerInstance = outerInstance;
				this.key = key;
			}

			public override void onClick(View v)
			{
				SyncedBoardManager.toggle(mSegmentsRef, key);
			}
		}

		private class CompletionListenerAnonymousInnerClassHelper : com.firebase.client.Firebase.CompletionListener
		{
			private readonly BoardListActivity outerInstance;

			private com.firebase.client.Firebase newBoardRef;

			public CompletionListenerAnonymousInnerClassHelper(BoardListActivity outerInstance, com.firebase.client.Firebase newBoardRef)
			{
				this.outerInstance = outerInstance;
				this.newBoardRef = newBoardRef;
			}

			public override void onComplete(FirebaseError firebaseError, Firebase @ref)
			{
				if (firebaseError != null)
				{
					Log.e(TAG, firebaseError.ToString());
					throw firebaseError.toException();
				}
				else
				{
					// once the board is created, start a DrawingActivity on it
					openBoard(newBoardRef.Key);
				}
			}
		}
		private int mInitialColor;

		public ColorPickerDialog(Context ctx, OnColorChangedListener mListener, int mInitialColor) : base(ctx)
		{
			this.mListener = mListener;
			this.mInitialColor = mInitialColor;
		}

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			OnColorChangedListener l = new OnColorChangedListenerAnonymousInnerClassHelper(this);

			ContentView = new ColorPickerView(this, Context, l, mInitialColor);
			Title = "Pick a Color";
		}

		private class OnColorChangedListenerAnonymousInnerClassHelper : OnColorChangedListener
		{
			private readonly ColorPickerDialog outerInstance;

			public OnColorChangedListenerAnonymousInnerClassHelper(ColorPickerDialog outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void colorChanged(int newColor)
			{
				outerInstance.mListener.colorChanged(newColor);
				dismiss();
			}
		}

		private class ColorPickerView : View
		{
			private readonly ColorPickerDialog outerInstance;


			internal Paint mPaint;
			internal Paint mCenterPaint;
			internal readonly int[] mColors;
			internal OnColorChangedListener mListener;

			internal ColorPickerView(ColorPickerDialog outerInstance, Context c, OnColorChangedListener l, int color) : base(c)
			{
				this.outerInstance = outerInstance;
				mListener = l;
				mColors = new int[]{unchecked((int)0xFFFF0000), unchecked((int)0xFFFF00FF), unchecked((int)0xFF0000FF), unchecked((int)0xFF00FFFF), unchecked((int)0xFF00FF00), unchecked((int)0xFFFFFF00), unchecked((int)0xFFFF0000)};
				Shader s = new SweepGradient(0, 0, mColors, null);

				mPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
				mPaint.Shader = s;
				mPaint.Style = Paint.Style.STROKE;
				mPaint.StrokeWidth = 32;

				mCenterPaint = new Paint(Paint.ANTI_ALIAS_FLAG);
				mCenterPaint.Color = color;
				mCenterPaint.StrokeWidth = 5;
			}

			internal bool mTrackingCenter;
			internal bool mHighlightCenter;

			protected internal override void onDraw(Canvas canvas)
			{
				float r = CENTER_X - mPaint.StrokeWidth * 0.5f;

				canvas.translate(CENTER_X, CENTER_X);

				canvas.drawOval(new RectF(-r, -r, r, r), mPaint);
				canvas.drawCircle(0, 0, CENTER_RADIUS, mCenterPaint);

				if (mTrackingCenter)
				{
					int c = mCenterPaint.Color;
					mCenterPaint.Style = Paint.Style.STROKE;

					if (mHighlightCenter)
					{
						mCenterPaint.Alpha = 0xFF;
					}
					else
					{
						mCenterPaint.Alpha = 0x80;
					}
					canvas.drawCircle(0, 0, CENTER_RADIUS + mCenterPaint.StrokeWidth, mCenterPaint);

					mCenterPaint.Style = Paint.Style.FILL;
					mCenterPaint.Color = c;
				}
			}

			protected internal override void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
			{
				setMeasuredDimension(CENTER_X * 2, CENTER_Y * 2);
			}

			internal const int CENTER_X = 100;
			internal const int CENTER_Y = 100;
			internal const int CENTER_RADIUS = 32;

			internal virtual int floatToByte(float x)
			{
				int n = Math.Round(x);
				return n;
			}

			internal virtual int pinToByte(int n)
			{
				if (n < 0)
				{
					n = 0;
				}
				else if (n > 255)
				{
					n = 255;
				}
				return n;
			}

			internal virtual int ave(int s, int d, float p)
			{
				return s + Math.Round(p * (d - s));
			}

			internal virtual int interpColor(int[] colors, float unit)
			{
				if (unit <= 0)
				{
					return colors[0];
				}
				if (unit >= 1)
				{
					return colors[colors.Length - 1];
				}

				float p = unit * (colors.Length - 1);
				int i = (int) p;
				p -= i;

				// now p is just the fractional part [0...1) and i is the index
				int c0 = colors[i];
				int c1 = colors[i + 1];
				int a = ave(Color.alpha(c0), Color.alpha(c1), p);
				int r = ave(Color.red(c0), Color.red(c1), p);
				int g = ave(Color.green(c0), Color.green(c1), p);
				int b = ave(Color.blue(c0), Color.blue(c1), p);

				return Color.argb(a, r, g, b);
			}

			internal virtual int rotateColor(int color, float rad)
			{
				float deg = rad * 180 / 3.1415927f;
				int r = Color.red(color);
				int g = Color.green(color);
				int b = Color.blue(color);

				ColorMatrix cm = new ColorMatrix();
				ColorMatrix tmp = new ColorMatrix();

				cm.setRGB2YUV();
				tmp.setRotate(0, deg);
				cm.postConcat(tmp);
				tmp.setYUV2RGB();
				cm.postConcat(tmp);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float[] a = cm.getArray();
				float[] a = cm.Array;

				int ir = floatToByte(a[0] * r + a[1] * g + a[2] * b);
				int ig = floatToByte(a[5] * r + a[6] * g + a[7] * b);
				int ib = floatToByte(a[10] * r + a[11] * g + a[12] * b);

				return Color.argb(Color.alpha(color), pinToByte(ir), pinToByte(ig), pinToByte(ib));
			}

			internal const float PI = 3.1415926f;

			public override bool onTouchEvent(MotionEvent @event)
			{
				float x = @event.X - CENTER_X;
				float y = @event.Y - CENTER_Y;
				bool inCenter = Math.Sqrt(x * x + y * y) <= CENTER_RADIUS;

				switch (@event.Action)
				{
					case MotionEvent.ACTION_DOWN:
						mTrackingCenter = inCenter;
						if (inCenter)
						{
							mHighlightCenter = true;
							invalidate();
							break;
						}
					case MotionEvent.ACTION_MOVE:
						if (mTrackingCenter)
						{
							if (mHighlightCenter != inCenter)
							{
								mHighlightCenter = inCenter;
								invalidate();
							}
						}
						else
						{
							float angle = (float) Math.Atan2(y, x);
							// need to turn angle [-PI ... PI] into unit [0....1]
							float unit = angle / (2 * PI);
							if (unit < 0)
							{
								unit += 1;
							}
							mCenterPaint.Color = interpColor(mColors, unit);
							invalidate();
						}
						break;
					case MotionEvent.ACTION_UP:
						if (mTrackingCenter)
						{
							if (inCenter)
							{
								mListener.colorChanged(mCenterPaint.Color);
							}
							mTrackingCenter = false; // so we draw w/o halo
							invalidate();
						}
						break;
				}
				return true;
			}
		}
	}

}