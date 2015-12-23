using System;
using System.Collections.Generic;

namespace com.firebase.drawing
{

	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Matrix = android.graphics.Matrix;
	using Paint = android.graphics.Paint;
	using Path = android.graphics.Path;
	using PorterDuff = android.graphics.PorterDuff;
	using RectF = android.graphics.RectF;
	using Log = android.util.Log;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using Toast = android.widget.Toast;

	using ChildEventListener = com.firebase.client.ChildEventListener;
	using DataSnapshot = com.firebase.client.DataSnapshot;
	using Firebase = com.firebase.client.Firebase;
	using FirebaseError = com.firebase.client.FirebaseError;
	using Logger = com.firebase.client.Logger;


	public class DrawingView : View
	{

		public const int PIXEL_SIZE = 8;

		private Paint mPaint;
		private int mLastX;
		private int mLastY;
		private Canvas mBuffer;
		private Bitmap mBitmap;
		private Paint mBitmapPaint;
		private Firebase mFirebaseRef;
		private ChildEventListener mListener;
		private int mCurrentColor = unchecked((int)0xFFFF0000);
		private Path mPath;
		private ISet<string> mOutstandingSegments;
		private Segment mCurrentSegment;
		private float mScale = 1.0f;
		private int mCanvasWidth;
		private int mCanvasHeight;

		public DrawingView(Context context, Firebase @ref) : this(context, @ref, 1.0f)
		{
		}
		public DrawingView(Context context, Firebase @ref, int width, int height) : this(context, @ref)
		{
			this.BackgroundColor = Color.DKGRAY;
			mCanvasWidth = width;
			mCanvasHeight = height;
		}
		public DrawingView(Context context, Firebase @ref, float scale) : base(context)
		{

			mOutstandingSegments = new HashSet<string>();
			mPath = new Path();
			this.mFirebaseRef = @ref;
			this.mScale = scale;

			mListener = @ref.addChildEventListener(new ChildEventListenerAnonymousInnerClassHelper(this));


			mPaint = new Paint();
			mPaint.AntiAlias = true;
			mPaint.Dither = true;
			mPaint.Color = 0xFFFF0000;
			mPaint.Style = Paint.Style.STROKE;

			mBitmapPaint = new Paint(Paint.DITHER_FLAG);
		}

		private class ChildEventListenerAnonymousInnerClassHelper : ChildEventListener
		{
			private readonly DrawingView outerInstance;

			public ChildEventListenerAnonymousInnerClassHelper(DrawingView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

					/// <param name="dataSnapshot"> The data we need to construct a new Segment </param>
					/// <param name="previousChildName"> Supplied for ordering, but we don't really care about ordering in this app </param>
			public override void onChildAdded(DataSnapshot dataSnapshot, string previousChildName)
			{
				string name = dataSnapshot.Key;
				// To prevent lag, we draw our own segments as they are created. As a result, we need to check to make
				// sure this event is a segment drawn by another user before we draw it
				if (!outerInstance.mOutstandingSegments.Contains(name))
				{
					// Deserialize the data into our Segment class
					Segment segment = dataSnapshot.getValue(typeof(Segment));
					outerInstance.drawSegment(segment, paintFromColor(segment.Color));
					// Tell the view to redraw itself
					invalidate();
				}
			}

			public override void onChildChanged(DataSnapshot dataSnapshot, string s)
			{
				// No-op
			}

			public override void onChildRemoved(DataSnapshot dataSnapshot)
			{
				// No-op
			}

			public override void onChildMoved(DataSnapshot dataSnapshot, string s)
			{
				// No-op
			}

			public override void onCancelled(FirebaseError firebaseError)
			{
				// No-op
			}
		}

		public virtual void cleanup()
		{
			mFirebaseRef.removeEventListener(mListener);
		}

		public virtual int Color
		{
			set
			{
				mCurrentColor = value;
				mPaint.Color = value;
			}
		}

		public virtual void clear()
		{
			mBitmap = Bitmap.createBitmap(mBitmap.Width, mBitmap.Height, Bitmap.Config.ARGB_8888);
			mBuffer = new Canvas(mBitmap);
			mCurrentSegment = null;
			mOutstandingSegments.Clear();
			invalidate();
		}

		protected internal override void onSizeChanged(int w, int h, int oldW, int oldH)
		{
			base.onSizeChanged(w, h, oldW, oldH);

			mScale = Math.Min(1.0f * w / mCanvasWidth, 1.0f * h / mCanvasHeight);

			mBitmap = Bitmap.createBitmap(Math.Round(mCanvasWidth * mScale), Math.Round(mCanvasHeight * mScale), Bitmap.Config.ARGB_8888);
			mBuffer = new Canvas(mBitmap);
			Log.i("AndroidDrawing", "onSizeChanged: created bitmap/buffer of " + mBitmap.Width + "x" + mBitmap.Height);
		}

		protected internal override void onDraw(Canvas canvas)
		{
			canvas.drawColor(Color.DKGRAY);
			canvas.drawRect(0, 0, mBitmap.Width, mBitmap.Height, paintFromColor(Color.WHITE, Paint.Style.FILL_AND_STROKE));

			canvas.drawBitmap(mBitmap, 0, 0, mBitmapPaint);

			canvas.drawPath(mPath, mPaint);
		}

		public static Paint paintFromColor(int color)
		{
			return paintFromColor(color, Paint.Style.STROKE);
		}

		public static Paint paintFromColor(int color, Paint.Style style)
		{
			Paint p = new Paint();
			p.AntiAlias = true;
			p.Dither = true;
			p.Color = color;
			p.Style = style;
			return p;
		}

		public static Path getPathForPoints(IList<Point> points, double scale)
		{
			Path path = new Path();
			scale = scale * PIXEL_SIZE;
			Point current = points[0];
			path.moveTo(Math.Round(scale * current.x), Math.Round(scale * current.y));
			Point next = null;
			for (int i = 1; i < points.Count; ++i)
			{
				next = points[i];
				path.quadTo(Math.Round(scale * current.x), Math.Round(scale * current.y), Math.Round(scale * (next.x + current.x) / 2), Math.Round(scale * (next.y + current.y) / 2));
				current = next;
			}
			if (next != null)
			{
				path.lineTo(Math.Round(scale * next.x), Math.Round(scale * next.y));
			}
			return path;
		}


		private void drawSegment(Segment segment, Paint paint)
		{
			if (mBuffer != null)
			{
				mBuffer.drawPath(getPathForPoints(segment.Points, mScale), paint);
			}
		}

		private void onTouchStart(float x, float y)
		{
			mPath.reset();
			mPath.moveTo(x, y);
			mCurrentSegment = new Segment(mCurrentColor);
			mLastX = (int) x / PIXEL_SIZE;
			mLastY = (int) y / PIXEL_SIZE;
			mCurrentSegment.addPoint(mLastX, mLastY);
		}

		private void onTouchMove(float x, float y)
		{

			int x1 = (int) x / PIXEL_SIZE;
			int y1 = (int) y / PIXEL_SIZE;

			float dx = Math.Abs(x1 - mLastX);
			float dy = Math.Abs(y1 - mLastY);
			if (dx >= 1 || dy >= 1)
			{
				mPath.quadTo(mLastX * PIXEL_SIZE, mLastY * PIXEL_SIZE, ((x1 + mLastX) * PIXEL_SIZE) / 2, ((y1 + mLastY) * PIXEL_SIZE) / 2);
				mLastX = x1;
				mLastY = y1;
				mCurrentSegment.addPoint(mLastX, mLastY);
			}
		}

		private void onTouchEnd()
		{
			mPath.lineTo(mLastX * PIXEL_SIZE, mLastY * PIXEL_SIZE);
			mBuffer.drawPath(mPath, mPaint);
			mPath.reset();
			Firebase segmentRef = mFirebaseRef.push();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String segmentName = segmentRef.getKey();
			string segmentName = segmentRef.Key;
			mOutstandingSegments.Add(segmentName);

			// create a scaled version of the segment, so that it matches the size of the board
			Segment segment = new Segment(mCurrentSegment.Color);
			foreach (Point point in mCurrentSegment.Points)
			{
				segment.addPoint((int)Math.Round(point.x / mScale), (int)Math.Round(point.y / mScale));
			}

			// Save our segment into Firebase. This will let other clients see the data and add it to their own canvases.
			// Also make a note of the outstanding segment name so we don't do a duplicate draw in our onChildAdded callback.
			// We can remove the name from mOutstandingSegments once the completion listener is triggered, since we will have
			// received the child added event by then.
			segmentRef.setValue(segment, new CompletionListenerAnonymousInnerClassHelper(this, segmentName));
		}

		private class CompletionListenerAnonymousInnerClassHelper : Firebase.CompletionListener
		{
			private readonly DrawingView outerInstance;

			private string segmentName;

			public CompletionListenerAnonymousInnerClassHelper(DrawingView outerInstance, string segmentName)
			{
				this.outerInstance = outerInstance;
				this.segmentName = segmentName;
			}

			public override void onComplete(FirebaseError error, Firebase firebaseRef)
			{
				if (error != null)
				{
					Log.e("AndroidDrawing", error.ToString());
					throw error.toException();
				}
				outerInstance.mOutstandingSegments.Remove(segmentName);
			}
		}

		public override bool onTouchEvent(MotionEvent @event)
		{
			float x = @event.X;
			float y = @event.Y;

			switch (@event.Action)
			{
				case MotionEvent.ACTION_DOWN:
					onTouchStart(x, y);
					invalidate();
					break;
				case MotionEvent.ACTION_MOVE:
					onTouchMove(x, y);
					invalidate();
					break;
				case MotionEvent.ACTION_UP:
					onTouchEnd();
					invalidate();
					break;
			}
			return true;
		}

	}

}