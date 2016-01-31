package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.animation.Animator;
import android.animation.AnimatorSet;
import android.animation.ObjectAnimator;
import android.animation.ValueAnimator;
import android.util.Log;
import android.view.View;
import android.view.ViewGroup;
import android.view.animation.CycleInterpolator;

import org.solovyev.android.views.llm.LinearLayoutManager;

/**
 * @brief Factory class to create and setup animators for components of Floating Controller
 */
public class FcAnimator {

    public static final String TAG = FcAnimator.class.getSimpleName();

    /**
     * @brief Create animator to hide selected view
     *
     * The Animator will
     * - Shrink layout width from full to 0
     * - Set visibility of the view to GONE
     *
     * @param toHide    View to be hidden
     * @param duration  Animation duration
     *
     * @return  Sequential animator to hide the element
     */
    public Animator createCollapseAnimator(final View toHide, long duration) {
        ValueAnimator collapseAnimator = ValueAnimator.ofFloat(1f, 0f);

        final int fromDimension = toHide.getWidth();

        collapseAnimator.setDuration(duration);
        collapseAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                float value = (Float) animation.getAnimatedValue();
                final int curPos = (int) (fromDimension * value);
                toHide.getLayoutParams().width = curPos;
                toHide.requestLayout();
            }
        });

        collapseAnimator.addListener(new FcAnimatorListener() {
            private boolean mCanceled = false;

            @Override
            public void onAnimationCancel(Animator animation) {
                mCanceled = true;
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                if (!mCanceled) {
                    toHide.setVisibility(View.GONE);
                }
            }
        });

        return collapseAnimator;
    }

    /**
     * @brief Create animator to hide specified app actions
     *
     * @param toShow        View to be shown
     * @param toDimension
     * @param duration      Animation duration of each sub animator
     *
     * @return  Sequential animator to show the hidden element
     */
    public Animator createExpandAnimator(final View toShow, final int toDimension, long duration) {

        Log.d(TAG, "View params:");
        Log.d(TAG, "  before measure getWidth: " + toShow.getWidth());

        final float fromWidth = toShow.getWidth();

        toShow.getLayoutParams().width = ViewGroup.LayoutParams.WRAP_CONTENT;
        toShow.measure(
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED),
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED)
        );

        float toMeasuredWidth = toShow.getMeasuredWidth();

        final float toWidth = (toDimension < toMeasuredWidth) ? toDimension : toMeasuredWidth;

        ValueAnimator expandAnimator = ValueAnimator.ofFloat(0f, 1f);
        expandAnimator.setDuration(duration);
        expandAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                float value = (Float) animation.getAnimatedValue();
                final int curPos = (int) (toWidth * value);
                toShow.getLayoutParams().width = curPos;
                toShow.requestLayout();
            }
        });

        expandAnimator.addListener(new FcAnimatorListener() {

            @Override
            public void onAnimationStart(Animator animation) {
                toShow.getLayoutParams().width = 0;
                toShow.setVisibility(View.VISIBLE);
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                toShow.getLayoutParams().width = ViewGroup.LayoutParams.WRAP_CONTENT;
            }
        });

        return expandAnimator;
    }

    /**
     * @brief Create animator to hide specified app actions
     *
     * @param toShow        View to be shown
     * @param duration      Animation duration of each sub animator
     *
     * @return  Sequential animator to show the hidden element
     */
    public Animator createExpandAnimator(final View toShow, long duration) {
        final int fromWidth = toShow.getWidth();
        toShow.getLayoutParams().width = ViewGroup.LayoutParams.WRAP_CONTENT;
        toShow.measure(
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED),
                View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
        final int toWidth = toShow.getMeasuredWidth();

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "createExpandAnimator(" + toShow + ")");
            Log.d(TAG, "    duration: " + duration);
            Log.d(TAG, "    width: " + fromWidth + " -> " + toWidth);
        }

        ValueAnimator expandAnimator = ValueAnimator.ofFloat(0f, 1f);
        expandAnimator.setDuration(duration);
        expandAnimator.addUpdateListener(new ValueAnimator.AnimatorUpdateListener() {
            @Override
            public void onAnimationUpdate(ValueAnimator animation) {
                float value = (Float) animation.getAnimatedValue();
                final int curPos = fromWidth + (int) ((toWidth - fromWidth) * value);
                toShow.getLayoutParams().width = curPos;
                toShow.requestLayout();
            }
        });
        expandAnimator.addListener(new FcAnimatorListener() {
            @Override
            public void onAnimationStart(Animator animation) {
                toShow.getLayoutParams().width = fromWidth;
                toShow.setVisibility(View.VISIBLE);
            }
        });

        return expandAnimator;
    }

    /**
     * @brief
     *
     * @param toShow
     * @param duration
     * @return
     */
    public Animator createExpandActionsAnimator(final View toShow, long duration) {

        final float baseAlpha = toShow.getAlpha();

        Animator fadeInAnimator = ObjectAnimator.ofFloat(toShow, View.ALPHA, baseAlpha, 1.0f);
        fadeInAnimator.setDuration(FcConstants.DEFAULT_FADE_ANIM_DURATION);

        AnimatorSet animator = new AnimatorSet();
        animator.playSequentially(
                createExpandAnimator(toShow, duration - FcConstants.DEFAULT_FADE_ANIM_DURATION),
                fadeInAnimator
        );
        animator.addListener(new FcAnimatorListener() {
            @Override
            public void onAnimationStart(Animator animation) {
                toShow.setVisibility(View.VISIBLE);
            }
        });

        return animator;
    }

    /**
     * @brief
     *
     * @param toHide
     * @param duration
     * @return
     */
    public Animator createCollapseActionsAnimator(final View toHide, long duration) {

        final float baseAlpha = toHide.getAlpha();
        Animator fadeOutAnimator = ObjectAnimator.ofFloat(toHide, View.ALPHA, baseAlpha, 0.0f);
        fadeOutAnimator.setDuration(FcConstants.DEFAULT_FADE_ANIM_DURATION);

        AnimatorSet animator = new AnimatorSet();
        animator.playSequentially(
                fadeOutAnimator,
                createCollapseAnimator(toHide, duration - FcConstants.DEFAULT_FADE_ANIM_DURATION)
        );

        return animator;
    }

    /**
     * @brief
     *
     * @param view
     * @param duration
     * @param dx
     * @param dy
     * @return
     */
    public Animator createScaleAnimator(View view, long duration, float dx, float dy) {
        Animator scaleX = ObjectAnimator.ofFloat(view, View.SCALE_X, 1.0f, 1.0f - dx);
        Animator scaleY = ObjectAnimator.ofFloat(view, View.SCALE_Y, 1.0f, 1.0f - dy);
        scaleX.setInterpolator(new CycleInterpolator(1.0f));
        scaleY.setInterpolator(new CycleInterpolator(1.0f));

        AnimatorSet animator = new AnimatorSet();
        animator.setDuration(duration);
        animator.playTogether(scaleX, scaleY);

        return animator;
    }

    /**
     * @brief Empty class implementing {@link android.animation.Animator.AnimatorListener} interface
     *
     * Class used to reduce number of methods required to implement the interface.
     */
    public static class FcAnimatorListener implements Animator.AnimatorListener {

        @Override
        public void onAnimationStart(Animator animation) {
            // NOOP
        }

        @Override
        public void onAnimationEnd(Animator animation) {
            // NOOP
        }

        @Override
        public void onAnimationCancel(Animator animation) {
            // NOOP
        }

        @Override
        public void onAnimationRepeat(Animator animation) {
            // NOOP
        }
    }
}
