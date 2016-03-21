namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Context = android.content.Context;
	using Configuration = android.content.res.Configuration;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Matrix = android.graphics.Matrix;
	using Paint = android.graphics.Paint;
	using Rect = android.graphics.Rect;
	using RectF = android.graphics.RectF;
	using Face = android.hardware.camera2.@params.Face;
	using AttributeSet = android.util.AttributeSet;
	using Size = android.util.Size;
	using View = android.view.View;

	/// <summary>
	/// A Simple <seealso cref="android.view.View"/> that can draw face rect.
	/// </summary>
	public class FaceRectView : View
	{
		private int mRatioWidth = 0;
		private int mRatioHeight = 0;

		private Paint mPaint;
		private Face[] mFaces;

		private int mRotation;
		private Rect mZoomRect;
		private int mFacing;
		private Size mPreviewSize;
		private int mOrientation;

		private Matrix mMatrix;
		private Matrix mAspectRatio;

		private RectF mRevisionZoomRect;
		private RectF mActualRect;
		private RectF mBoundRect;

		public FaceRectView(Context context) : this(context, null)
		{
		}

		public FaceRectView(Context context, AttributeSet attrs) : this(context, attrs, 0)
		{
		}

		public FaceRectView(Context context, AttributeSet attrs, int defStyle) : base(context, attrs, defStyle)
		{

			mPaint = new Paint();
			mPaint.Style = Paint.Style.STROKE;

			mMatrix = new Matrix();
			mAspectRatio = new Matrix();
			mRevisionZoomRect = new RectF();
			mActualRect = new RectF();
			mBoundRect = new RectF();
		}

		/// <summary>
		/// Sets the aspect ratio for this view. The size of the view will be measured based on the ratio
		/// calculated from the parameters. Note that the actual sizes of parameters don't matter, that
		/// is, calling setAspectRatio(2, 3) and setAspectRatio(4, 6) make the same result.
		/// </summary>
		/// <param name="width">  Relative horizontal size </param>
		/// <param name="height"> Relative vertical size </param>
		public virtual void setAspectRatio(int width, int height)
		{
			if (width < 0 || height < 0)
			{
				throw new System.ArgumentException("Size cannot be negative.");
			}
			mRatioWidth = width;
			mRatioHeight = height;
			requestLayout();
		}

		protected internal override void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
		{
			base.onMeasure(widthMeasureSpec, heightMeasureSpec);
			int width = MeasureSpec.getSize(widthMeasureSpec);
			int height = MeasureSpec.getSize(heightMeasureSpec);
			if (0 == mRatioWidth || 0 == mRatioHeight)
			{
				setMeasuredDimension(width, height);
			}
			else
			{
				if (width < height * mRatioWidth / mRatioHeight)
				{
					setMeasuredDimension(width, width * mRatioHeight / mRatioWidth);
				}
				else
				{
					setMeasuredDimension(height * mRatioWidth / mRatioHeight, height);
				}
			}
		}

		protected internal override void onDraw(Canvas canvas)
		{

			if (mFaces != null && mZoomRect != null)
			{
				// Prepare matrix
				mMatrix.reset();
				mAspectRatio.reset();
				mActualRect.set(0, 0, mPreviewSize.Width, mPreviewSize.Height);

				// First apply zoom (crop) rect.
				// Unlike the documentation, many device does not report final crop region.
				// So, here we calculate final crop region which takes account aspect ratio between crop region and preview size.
				{
					mRevisionZoomRect.set(mZoomRect);
					float left = mRevisionZoomRect.left;
					float top = mRevisionZoomRect.top;

					mRevisionZoomRect.offsetTo(0, 0);

					mAspectRatio.setRectToRect(mActualRect, mRevisionZoomRect, Matrix.ScaleToFit.CENTER);

					mAspectRatio.mapRect(mActualRect);
					mActualRect.offset(left, top);
				}

				mMatrix.postTranslate(-mActualRect.centerX(), -mActualRect.centerY());

				// compensate mirror
				mMatrix.postScale(mFacing == SCameraCharacteristics.LENS_FACING_FRONT ? - 1 : 1, 1);

				// Then rotate and scale to UI size
				mMatrix.postRotate(mRotation);
				if (mOrientation == Configuration.ORIENTATION_LANDSCAPE)
				{
					mMatrix.postScale((float) Width / mActualRect.width(), (float) Height / mActualRect.height());
				}
				else
				{
					mMatrix.postScale((float) Height / mActualRect.width(), (float) Width / mActualRect.height());
				}
				mMatrix.postTranslate((float)Width / 2, (float)Height / 2);

				foreach (Face face in mFaces)
				{

					mBoundRect.set(face.Bounds);
					mMatrix.mapRect(mBoundRect);

					mPaint.Color = Color.BLUE;
					mPaint.StrokeWidth = 3;
					canvas.drawRect(mBoundRect, mPaint);

					{ // Additional features may not supported.
						float[] point = new float[2];
						mPaint.Color = Color.RED;
						mPaint.StrokeWidth = 10;

						if (face.LeftEyePosition != null)
						{
							mMatrix.mapPoints(point, new float[]{face.LeftEyePosition.x, face.LeftEyePosition.y});
							canvas.drawPoint(point[0], point[1], mPaint);
						}

						if (face.RightEyePosition != null)
						{
							mMatrix.mapPoints(point, new float[]{face.RightEyePosition.x, face.RightEyePosition.y});
							canvas.drawPoint(point[0], point[1], mPaint);
						}

						if (face.MouthPosition != null)
						{
							mMatrix.mapPoints(point, new float[]{face.MouthPosition.x, face.MouthPosition.y});
							canvas.drawPoint(point[0], point[1], mPaint);
						}

						mPaint.Color = Color.YELLOW;
						mPaint.StrokeWidth = 3;
						mPaint.TextSize = 30;
						if (face.Id != Face.ID_UNSUPPORTED)
						{
							canvas.drawText(string.Format("ID:{0:D}, Score:{1:D}", face.Id, face.Score), mBoundRect.left, mBoundRect.top, mPaint);
						}
						else
						{
							canvas.drawText(string.Format("Score:{0:D}", face.Score), mBoundRect.left, mBoundRect.top, mPaint);
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets parameter required to calculate transform
		/// </summary>
		public virtual void setTransform(Size previewSize, int facing, int rotation, int orientation)
		{
			mFacing = facing;
			mPreviewSize = previewSize;
			mRotation = rotation;
			mOrientation = orientation;
		}

		/// <summary>
		/// Sets FaceRect parameters to be drawn.
		/// </summary>
		public virtual void setFaceRect(Face[] faces, Rect zoomRect)
		{
			mFaces = faces;
			mZoomRect = zoomRect;
		}
	}

}