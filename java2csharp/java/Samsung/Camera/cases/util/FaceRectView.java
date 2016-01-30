package com.samsung.android.sdk.camera.sample.cases.util;

import android.content.Context;
import android.content.res.Configuration;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.Rect;
import android.graphics.RectF;
import android.hardware.camera2.params.Face;
import android.util.AttributeSet;
import android.util.Size;
import android.view.View;

import com.samsung.android.sdk.camera.SCameraCharacteristics;

/**
 * A Simple {@link android.view.View} that can draw face rect.
 */
public class FaceRectView extends View {
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

    public FaceRectView(Context context) {
        this(context, null);
    }

    public FaceRectView(Context context, AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public FaceRectView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);

        mPaint = new Paint();
        mPaint.setStyle(Paint.Style.STROKE);

        mMatrix = new Matrix();
        mAspectRatio = new Matrix();
        mRevisionZoomRect = new RectF();
        mActualRect = new RectF();
        mBoundRect = new RectF();
    }

    /**
     * Sets the aspect ratio for this view. The size of the view will be measured based on the ratio
     * calculated from the parameters. Note that the actual sizes of parameters don't matter, that
     * is, calling setAspectRatio(2, 3) and setAspectRatio(4, 6) make the same result.
     *
     * @param width  Relative horizontal size
     * @param height Relative vertical size
     */
    public void setAspectRatio(int width, int height) {
        if (width < 0 || height < 0) {
            throw new IllegalArgumentException("Size cannot be negative.");
        }
        mRatioWidth = width;
        mRatioHeight = height;
        requestLayout();
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec) {
        super.onMeasure(widthMeasureSpec, heightMeasureSpec);
        int width = MeasureSpec.getSize(widthMeasureSpec);
        int height = MeasureSpec.getSize(heightMeasureSpec);
        if (0 == mRatioWidth || 0 == mRatioHeight) {
            setMeasuredDimension(width, height);
        } else {
            if (width < height * mRatioWidth / mRatioHeight) {
                setMeasuredDimension(width, width * mRatioHeight / mRatioWidth);
            } else {
                setMeasuredDimension(height * mRatioWidth / mRatioHeight, height);
            }
        }
    }

    @Override
    protected void onDraw(Canvas canvas) {

        if(mFaces != null && mZoomRect != null) {
            // Prepare matrix
            mMatrix.reset();
            mAspectRatio.reset();
            mActualRect.set(0, 0, mPreviewSize.getWidth(), mPreviewSize.getHeight());

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
            mMatrix.postScale(mFacing == SCameraCharacteristics.LENS_FACING_FRONT ? -1 : 1, 1);

            // Then rotate and scale to UI size
            mMatrix.postRotate(mRotation);
            if(mOrientation == Configuration.ORIENTATION_LANDSCAPE) {
                mMatrix.postScale((float) getWidth() / mActualRect.width(), (float) getHeight() / mActualRect.height());
            } else {
                mMatrix.postScale((float) getHeight() / mActualRect.width(), (float) getWidth() / mActualRect.height());
            }
            mMatrix.postTranslate((float)getWidth() / 2, (float)getHeight() /2);

            for (Face face : mFaces) {

                mBoundRect.set(face.getBounds());
                mMatrix.mapRect(mBoundRect);

                mPaint.setColor(Color.BLUE);
                mPaint.setStrokeWidth(3);
                canvas.drawRect(mBoundRect, mPaint);

                { // Additional features may not supported.
                    float[] point = new float[2];
                    mPaint.setColor(Color.RED);
                    mPaint.setStrokeWidth(10);

                    if (face.getLeftEyePosition() != null) {
                        mMatrix.mapPoints(point, new float[]{face.getLeftEyePosition().x, face.getLeftEyePosition().y});
                        canvas.drawPoint(point[0], point[1], mPaint);
                    }

                    if (face.getRightEyePosition() != null) {
                        mMatrix.mapPoints(point, new float[]{face.getRightEyePosition().x, face.getRightEyePosition().y});
                        canvas.drawPoint(point[0], point[1], mPaint);
                    }

                    if (face.getMouthPosition() != null) {
                        mMatrix.mapPoints(point, new float[]{face.getMouthPosition().x, face.getMouthPosition().y});
                        canvas.drawPoint(point[0], point[1], mPaint);
                    }

                    mPaint.setColor(Color.YELLOW);
                    mPaint.setStrokeWidth(3);
                    mPaint.setTextSize(30);
                    if (face.getId() != Face.ID_UNSUPPORTED) {
                        canvas.drawText(String.format("ID:%d, Score:%d", face.getId(), face.getScore()), mBoundRect.left, mBoundRect.top, mPaint);
                    } else {
                        canvas.drawText(String.format("Score:%d", face.getScore()), mBoundRect.left, mBoundRect.top, mPaint);
                    }
                }
            }
        }
    }

    /**
     * Sets parameter required to calculate transform
     */
    public void setTransform(Size previewSize, int facing, int rotation, int orientation) {
        mFacing = facing;
        mPreviewSize = previewSize;
        mRotation = rotation;
        mOrientation = orientation;
    }

    /**
     * Sets FaceRect parameters to be drawn.
     */
    public void setFaceRect(Face[] faces, Rect zoomRect) {
        mFaces = faces;
        mZoomRect = zoomRect;
    }
}
