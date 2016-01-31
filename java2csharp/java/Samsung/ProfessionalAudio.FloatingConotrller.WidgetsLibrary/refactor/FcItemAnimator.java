package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.animation.Animator;
import android.animation.AnimatorSet;
import android.animation.ObjectAnimator;
import android.support.v7.widget.RecyclerView;
import android.support.v7.widget.RecyclerView.ViewHolder;
import android.util.LayoutDirection;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

/**
 * This implementation of {@link RecyclerView.ItemAnimator} is a modified
 * {@link DefaultItemAnimator} that replaces default change animation to
 * more complex.
 *
 * @see RecyclerView#setItemAnimator(RecyclerView.ItemAnimator)
 */
public class FcItemAnimator extends DefaultItemAnimator {

    private static final String TAG = FcItemAnimator.class.getSimpleName();
    private final RecyclerView mRecyclerView;
    private final FcAnimator mFactory = new FcAnimator();
    private final Map<ViewHolder, Animator> mRunningAnimators;
    private final RecyclerView.OnItemTouchListener mDisableScrollingListener = new ScrollDisabler();

    private static final float TRANSPARENT = 0f;
    private static final float FULL_COLOR = 1f;

    private volatile int mScrollStatus = RecyclerView.SCROLL_STATE_IDLE;
    private RecyclerView.OnScrollListener mScrollListener = new RecyclerView.OnScrollListener() {
        @Override
        public void onScrollStateChanged(RecyclerView recyclerView, int newState) {
            mScrollStatus = newState;
        }
    };
    private int mScrollClientsCount = 0;

    public FcItemAnimator(RecyclerView recyclerView) {
        mRecyclerView = recyclerView;
        mRunningAnimators = new HashMap<ViewHolder, Animator>();
        setSupportsChangeAnimations(true);
    }

    @Override
    protected void animateChangeImpl(DefaultItemAnimator.ChangeInfo changeInfo) {
        final RecyclerView.ViewHolder holder = changeInfo.oldHolder;
        final RecyclerView.ViewHolder newHolder = changeInfo.newHolder;

        if (FcConstants.OPT_DETAILED_LOGS) {
            if (null != holder) {
                Log.d(TAG, "animateChangeImpl(position=" + holder.getAdapterPosition() + ")");
            } else if (null != newHolder) {
                Log.d(TAG, "animateChangeImpl(position=" + newHolder.getAdapterPosition() + ")");
            } else {
                Log.d(TAG, "animateChangeImpl(both nulls)");
            }
            Log.d(TAG, "   old = " + holder);
            Log.d(TAG, "   new = " + newHolder);
            Log.d(TAG, "   change: " + changeInfo);
        }

        if (newHolder instanceof OrdinalAppViewHolder) {
            if (holder instanceof OrdinalAppViewHolder || null == holder) {
                animateChangeOrdinalApp(changeInfo, (OrdinalAppViewHolder) holder,
                        (OrdinalAppViewHolder) newHolder);
            }
        } else {
            animateChangeDefault(changeInfo, holder, newHolder);
        }
    }

    @Override
    protected void animateAddImpl(final ViewHolder holder) {
        final View view = holder.itemView;
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateAddImpl");
            Log.d(TAG, "    holder: " + holder);
            Log.d(TAG, "    duration: " + getAddDuration());
            Log.d(TAG, "    translation: " + view.getTranslationX() + ", " + view.getTranslationY());
            if (holder instanceof  OrdinalAppViewHolder) {
                Log.d(TAG, "    needs scroll focus: " + ((OrdinalAppViewHolder) holder).needsScrollFocus());
            }
        }

        final Animator fadeInAnimator = ObjectAnimator.ofFloat(view, View.ALPHA, FULL_COLOR);
        fadeInAnimator.setDuration(getAddDuration());
        fadeInAnimator.addListener(new FcAnimator.FcAnimatorListener() {

            private boolean mCanceled = false;

            @Override
            public void onAnimationStart(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateAddImpl -> holder -> start");
                }
                dispatchAddStarting(holder);
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateAddImpl -> holder -> end");
                }
                view.setAlpha(FULL_COLOR);
                mRunningAnimators.remove(holder);
                fadeInAnimator.removeListener(this);
                dispatchAddFinished(holder);
                dispatchFinishedWhenDone();

                if (holder instanceof OrdinalAppViewHolder) {
                    OrdinalAppViewHolder appHolder = (OrdinalAppViewHolder) holder;
                    if (!mCanceled && appHolder.needsScrollFocus()) {
                        scrollToHolder(appHolder);
                    }
                }
            }

            @Override
            public void onAnimationCancel(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateAddImpl -> holder -> cancel");
                }
                mCanceled = true;
            }
        });

        mRunningAnimators.put(holder, fadeInAnimator);
        fadeInAnimator.start();
    }

    @Override
    protected void animateRemoveImpl(final ViewHolder holder) {
        final View view = holder.itemView;
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateRemoveImpl");
            Log.d(TAG, "    holder: " + holder);
            Log.d(TAG, "    duration: " + getAddDuration());
            Log.d(TAG, "    translation: " + view.getTranslationX() + ", " + view.getTranslationY());
        }

        final Animator fadeOutAnimator = ObjectAnimator.ofFloat(view, View.ALPHA, TRANSPARENT);
        fadeOutAnimator.setDuration(getRemoveDuration());
        fadeOutAnimator.addListener(new FcAnimator.FcAnimatorListener() {
            @Override
            public void onAnimationStart(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateRemoveImpl -> holder -> start");
                }
                dispatchRemoveStarting(holder);
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateRemoveImpl -> holder -> end");
                }
                view.setAlpha(TRANSPARENT);
                mRunningAnimators.remove(holder);
                dispatchRemoveFinished(holder);
                dispatchFinishedWhenDone();
            }

            @Override
            public void onAnimationCancel(Animator animation) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.d(TAG, "animateRemoveImpl -> holder -> cancel");
                }
            }
        });

        mRunningAnimators.put(holder, fadeOutAnimator);
        fadeOutAnimator.start();
    }

    @Override
    protected void animateMoveImpl(final ViewHolder holder, int fromX, int fromY, int toX, int toY) {
        final View view = holder.itemView;
        final int deltaX = toX - fromX;
        final int deltaY = toY - fromY;

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateMoveImpl(delta: " + deltaX + ", " + deltaY + ")");
            Log.d(TAG, "    duration: " + getMoveDuration());
            Log.d(TAG, "    translation: " + view.getTranslationX() + ", " + view.getTranslationY());
        }

        final AnimatorSet animator = new AnimatorSet();
        animator.playTogether(
                ObjectAnimator.ofFloat(view, View.TRANSLATION_X, 0),
                ObjectAnimator.ofFloat(view, View.TRANSLATION_Y, 0)
        );

        acquireUserScrolling();

        animator.setDuration(getMoveDuration());
        animator.addListener(new FcAnimator.FcAnimatorListener() {
            @Override
            public void onAnimationStart(Animator animation) {
                dispatchMoveStarting(holder);
            }

            @Override
            public void onAnimationEnd(Animator animation) {
                releaseUserScrolling();
                mMoveAnimations.remove(holder);
                animation.removeListener(this);
                dispatchMoveFinished(holder);
            }
        });

        mRunningAnimators.put(holder, animator);
        animator.start();
    }

    /**
     * @brief
     *
     * @param changeInfo
     * @param holder
     * @param newHolder
     */
    private void animateChangeDefault(final ChangeInfo changeInfo, final ViewHolder holder,
                                      final ViewHolder newHolder) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateChangeDefault");
            Log.d(TAG, "   duration = " + getChangeDuration());
            Log.d(TAG, "   changeInfo: " + changeInfo);
            Log.d(TAG, "   old = " + holder);
            Log.d(TAG, "   new = " + newHolder);
        }
        final View oldView = holder == null ? null : holder.itemView;
        final View newView = newHolder != null ? newHolder.itemView : null;

        final boolean hasNoTranslations = changeInfo.hasNoTranslation();

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "   old view = " + oldView);
            Log.d(TAG, "   new view = " + newView);
            Log.d(TAG, "   has no translations = " + hasNoTranslations);
        }

        if (oldView != null) {
            final AnimatorSet animator = new AnimatorSet();

            if (hasNoTranslations) {
                animator.setDuration(0);
            } else {
                animator.setDuration(getChangeDuration());
            }

//            acquireUserScrolling();

            animator.playSequentially(
                    ObjectAnimator.ofFloat(oldView, View.TRANSLATION_X, changeInfo.toX - changeInfo.fromX),
                    ObjectAnimator.ofFloat(oldView, View.TRANSLATION_Y, changeInfo.toY - changeInfo.fromY)
            );
            animator.addListener(new FcAnimator.FcAnimatorListener() {
                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateChangeDefault -> oldHolder -> start");
                    }
                    oldView.setAlpha(FULL_COLOR);
                    if (null != newView) {
                        newView.setAlpha(TRANSPARENT);
                    }
                    dispatchChangeStarting(holder, true);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateChangeDefault -> oldHolder -> end");
                    }
                    oldView.setAlpha(TRANSPARENT);
                    if (null != newView) {
                        newView.setAlpha(FULL_COLOR);
                    }

                    animator.removeListener(this);
                    mRunningAnimators.remove(holder);
                    dispatchChangeFinished(holder, true);
                    dispatchFinishedWhenDone();
//                    releaseUserScrolling();
                }
            });

            mRunningAnimators.put(holder, animator);
            animator.start();
        }

        if (newView != null) {
            final AnimatorSet animator = new AnimatorSet();
            if (hasNoTranslations) {
                animator.setDuration(0);
            } else {
                animator.setDuration(getChangeDuration());
            }

            acquireUserScrolling();

            animator.playTogether(
                    ObjectAnimator.ofFloat(newView, View.TRANSLATION_X, 0),
                    ObjectAnimator.ofFloat(newView, View.TRANSLATION_Y, 0)
            );

            animator.addListener(new FcAnimator.FcAnimatorListener() {
                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateChangeDefault -> newHolder -> start");
                    }
                    if (null != oldView) {
                        newView.setAlpha(TRANSPARENT);
                    } else {
                        newView.setAlpha(FULL_COLOR);
                    }
                    dispatchChangeStarting(newHolder, false);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateChangeDefault -> newHolder -> end");
                    }
                    newView.setAlpha(FULL_COLOR);
                    if (null != oldView) {
                        oldView.setAlpha(TRANSPARENT);
                    }

                    animator.removeListener(this);
                    mRunningAnimators.remove(animator);
                    dispatchChangeFinished(newHolder, false);
                    dispatchFinishedWhenDone();
                    releaseUserScrolling();
                }
            });

            mRunningAnimators.put(newHolder, animator);
            animator.start();
        }
    }

    /**
     * @brief
     *
     * @param changeInfo
     * @param holder
     * @param newHolder
     */
    private void animateChangeOrdinalApp(final ChangeInfo changeInfo, final OrdinalAppViewHolder holder,
                                         final OrdinalAppViewHolder newHolder) {

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateChangeOrdinalApp(position=" + newHolder.getAdapterPosition() + ", " + changeInfo + ")");
            Log.d(TAG, "   duration = " + getChangeDuration());
        }

        final View newView = newHolder.itemView;

        // Default animation whenever one of the two views is not present
        if (newView == null) {
            animateChangeDefault(changeInfo, holder, newHolder);
            return;
        }

        final int oldWidth = (null == holder) ? 0 : holder.mDeviceRootLayout.getWidth();
        final int newWidth = newHolder.mDeviceRootLayout.getWidth();

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "oldWidth = " + oldWidth + ", newWidth = " + newWidth);
        }

        if (oldWidth == 0 && newWidth > 0) {
            animateOrdinalAppExpanding(changeInfo, holder, newHolder);

        } else if (oldWidth > 0 && newWidth == 0) {
            animateOrdinalAppCollapsing(changeInfo, holder, newHolder);

        } else {
            animateOrdinalAppUpdate(changeInfo, holder, newHolder);
        }
    }

    private void animateOrdinalAppUpdate(final ChangeInfo changeInfo,
                                         final OrdinalAppViewHolder holder,
                                         final OrdinalAppViewHolder newHolder) {
        final View oldView = (null == holder) ? null : holder.itemView;
        final View newView = (null == newHolder) ? null : newHolder.itemView;

        final boolean hasNoTranslations = changeInfo.hasNoTranslation();
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateOrdinalAppUpdate(" + holder + ")");
            Log.d(TAG, "   change info: " + changeInfo);
            Log.d(TAG, "   no translations: " + changeInfo.hasNoTranslation());
        }

        if (oldView != null) {
            final AnimatorSet animator = new AnimatorSet();
            animator.playTogether(
                    ObjectAnimator.ofFloat(oldView, View.TRANSLATION_X, changeInfo.toX - changeInfo.fromX),
                    ObjectAnimator.ofFloat(oldView, View.TRANSLATION_Y, changeInfo.toY - changeInfo.fromY)
            );

            if (hasNoTranslations) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.v(TAG, "animateOrdinalAppUpdate -> oldHolder -> no translations, setting duration to 0");
                }
                animator.setDuration(0);
            } else {
                animator.setDuration(getChangeDuration());
            }

//            acquireUserScrolling();

            animator.addListener(new FcAnimator.FcAnimatorListener() {
                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> start");
                    }
                    oldView.setAlpha(FULL_COLOR);
                    dispatchChangeStarting(holder, true);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> end");
                    }
                    oldView.setAlpha(TRANSPARENT);
                    oldView.setTranslationX(0);
                    oldView.setTranslationY(0);
                    if (null != newView) {
                        newView.setAlpha(FULL_COLOR);
                    }

                    animator.removeListener(this);
                    mRunningAnimators.remove(holder);
                    dispatchChangeFinished(holder, true);

//                    if (!hasNoTranslations) {
//                    releaseUserScrolling();
//                    }
                }

                @Override
                public void onAnimationCancel(Animator animation) {
                    Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> cancel");
                }
            });

            mRunningAnimators.put(holder, animator);
            animator.start();
        }

        if (newView != null) {
            final AnimatorSet animator = new AnimatorSet();
            animator.playTogether(
                    ObjectAnimator.ofFloat(newView, View.TRANSLATION_X, 0),
                    ObjectAnimator.ofFloat(newView, View.TRANSLATION_Y, 0));

            if (hasNoTranslations) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.v(TAG, "animateOrdinalAppUpdate -> oldHolder -> no translations, setting duration to 0");
                }
                animator.setDuration(0);
            } else {
                animator.setDuration(getChangeDuration());
            }

            acquireUserScrolling();

            animator.addListener(new FcAnimator.FcAnimatorListener() {
                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> start");
                    }
                    newView.setAlpha(TRANSPARENT);
                    if (null != oldView) {
                        oldView.setAlpha(FULL_COLOR);
                    }
                    dispatchChangeStarting(newHolder, false);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> end");
                    }
                    newView.setAlpha(FULL_COLOR);
                    newView.setTranslationX(0);
                    newView.setTranslationY(0);
                    if (null != oldView) {
                        oldView.setAlpha(TRANSPARENT);
                    }

                    mRunningAnimators.remove(newHolder);
                    animator.removeListener(this);
                    dispatchChangeFinished(newHolder, false);

//                    if (!hasNoTranslations) {
                        releaseUserScrolling();
//                    }
                }

                @Override
                public void onAnimationCancel(Animator animation) {
                    Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> cancel");
                }
            });

            mRunningAnimators.put(newHolder, animator);
            animator.start();
        }
    }

    private void animateOrdinalAppCollapsing(final ChangeInfo changeInfo,
                                             final OrdinalAppViewHolder oldHolder,
                                             final OrdinalAppViewHolder newHolder) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "    change info: " + changeInfo);
            Log.d(TAG, "    old holder : " + oldHolder);
            if (null != oldHolder) {
                Log.d(TAG, "      old view: " + oldHolder.itemView);
                Log.d(TAG, "      needs scroll focus: " + oldHolder.needsScrollFocus());
            }
            if (null != newHolder) {
                Log.d(TAG, "      new view: " + newHolder.itemView);
                Log.d(TAG, "      needs scroll focus: " + newHolder.needsScrollFocus());
            }
        }

        final View oldView = (null == oldHolder) ? null : oldHolder.itemView;
        final View newView = (null == newHolder) ? null : newHolder.itemView;

        if (null != oldView) {
            List<Animator> animatorList = new ArrayList<Animator>();
            addOrdinalAppButtonClickedAnimation(animatorList, oldHolder);
            addTranslateOldAnimator(animatorList, changeInfo, oldHolder);

            acquireUserScrolling();

            final Animator collapseAnimator = mFactory.createCollapseActionsAnimator(
                    oldHolder.mDeviceRootLayout, getChangeDuration());
            collapseAnimator.addListener(new FcAnimator.FcAnimatorListener() {
                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppCollapsing -> oldHolder -> start");
                    }
                    oldView.setAlpha(FULL_COLOR);
                    if (null != newView) {
                        newView.setAlpha(TRANSPARENT);
                    }
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppCollapsing -> oldHolder -> end");
                    }
                    oldView.setAlpha(TRANSPARENT);
                    oldView.setTranslationX(0);
                    oldView.setTranslationY(0);
                    if (null != newView) {
                        newView.setAlpha(FULL_COLOR);
                    }

                    mRunningAnimators.remove(oldHolder);
                    collapseAnimator.removeListener(this);

                    dispatchChangeFinished(oldHolder, true);
                    dispatchFinishedWhenDone();
                    releaseUserScrolling();
                }
            });

            animatorList.add(collapseAnimator);

            AnimatorSet mainAnimator = new AnimatorSet();
            mainAnimator.playTogether(animatorList);

            mRunningAnimators.put(oldHolder, mainAnimator);
            mainAnimator.start();
        }

        if (null != newView) {
            acquireUserScrolling();

            final AnimatorSet translationAnimators = new AnimatorSet();
            translationAnimators.setDuration(getChangeDuration());
            translationAnimators.playTogether(
                    ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_X, 0),
                    ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_Y, 0));
            translationAnimators.addListener(new FcAnimator.FcAnimatorListener() {

                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppCollapsing -> newHolder -> start");
                    }
                    newHolder.itemView.setAlpha(TRANSPARENT);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppCollapsing -> newHolder -> end");
                    }
                    newView.setAlpha(FULL_COLOR);
                    newView.setTranslationX(0);
                    newView.setTranslationY(0);
                    if (null != oldView) {
                        oldView.setAlpha(TRANSPARENT);
                    }

                    if (newHolder.needsScrollFocus()) {
                        newHolder.setScrollFocusFinished();
                    }

                    translationAnimators.removeListener(this);
                    mRunningAnimators.remove(newHolder);
                    dispatchChangeFinished(newHolder, false);
                    releaseUserScrolling();
                }
            });

            mRunningAnimators.put(newHolder, translationAnimators);
            translationAnimators.start();
        }
    }

    /**
     * This method will be called when app root layout is to be expanded from 0 to some width
     *
     * Old holder will contain a root layout with with = 0, new holder will contain a root layout
     * with some width greater than 0.
     *
     * @param changeInfo
     * @param oldHolder
     * @param newHolder
     */
    private void animateOrdinalAppExpanding(final ChangeInfo changeInfo,
                                            final OrdinalAppViewHolder oldHolder,
                                            final OrdinalAppViewHolder newHolder) {

        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "animateOrdinalAppExpanding");
            Log.d(TAG, "    change info: " + changeInfo);
            Log.d(TAG, "    old holder : " + oldHolder);
            if (null != oldHolder) {
                Log.d(TAG, "      old view: " + oldHolder.itemView);
                Log.d(TAG, "      needs scroll focus: " + oldHolder.needsScrollFocus());
            }
            if (null != newHolder) {
                Log.d(TAG, "      new view: " + newHolder.itemView);
                Log.d(TAG, "      needs scroll focus: " + newHolder.needsScrollFocus());
            }
        }

        final View oldView = (null == oldHolder) ? null : oldHolder.itemView;
        final View newView = (null == newHolder) ? null : newHolder.itemView;

        if (null != oldView) {
            List<Animator> oldAnimatorList = new ArrayList<Animator>();

            addOrdinalAppButtonClickedAnimation(oldAnimatorList, oldHolder);
            addTranslateOldAnimator(oldAnimatorList, changeInfo, oldHolder);

            acquireUserScrolling();

            final Animator expandAnimator = mFactory.createExpandActionsAnimator(
                    oldHolder.mDeviceRootLayout, getChangeDuration());
            expandAnimator.setDuration(getChangeDuration());
            expandAnimator.addListener(new FcAnimator.FcAnimatorListener() {

                boolean mCanceled = false;

                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> start");
                    }
                    oldView.setAlpha(FULL_COLOR);
                    if (null != newView) {
                        newView.setAlpha(TRANSPARENT);
                    }

                    dispatchChangeStarting(oldHolder, true);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> end");
                    }
                    if (!mCanceled) {
                        Log.v(TAG, "   old view alpha -> transparent");
                        oldView.setAlpha(TRANSPARENT);
                        oldView.setTranslationX(0);
                        oldView.setTranslationY(0);
                        if (null != newView) {
                            Log.v(TAG, "  new view alpha -> full color");
                            newView.setAlpha(FULL_COLOR);
                        }
                    }

                    mRunningAnimators.remove(oldHolder);
                    expandAnimator.removeListener(this);

                    dispatchChangeFinished(oldHolder, true);
                    dispatchFinishedWhenDone();
                    releaseUserScrolling();
                }

                @Override
                public void onAnimationCancel(Animator animation) {
                    mCanceled = true;
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> cancel");
                    }
                    if (null != newView) {
                        Log.v(TAG, "  new view: full color");
                        Log.v(TAG, "  new view: translation (0, 0)");
                        newView.setAlpha(FULL_COLOR);
                        newView.setTranslationX(0f);
                        newView.setTranslationY(0f);
                    }
                }
            });
            oldAnimatorList.add(expandAnimator);

            AnimatorSet mainAnimator = new AnimatorSet();
            mainAnimator.setDuration(getChangeDuration());
            mainAnimator.playTogether(oldAnimatorList);

            mRunningAnimators.put(oldHolder, mainAnimator);
            mainAnimator.start();
        }

        if (null != newView) {
            acquireUserScrolling();

            final AnimatorSet translationAnimators = new AnimatorSet();

            if (isRtlDirection()) {
                if (FcConstants.OPT_DETAILED_LOGS) {
                    Log.v(TAG, "RTL direction: setting animation duration to 0");
                }
                translationAnimators.setDuration(0);
            } else {
                translationAnimators.setDuration(getChangeDuration());
            }

            translationAnimators.playTogether(
                    ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_X, 0),
                    ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_Y, 0));
            translationAnimators.addListener(new FcAnimator.FcAnimatorListener() {
                private boolean mCanceled = false;

                @Override
                public void onAnimationStart(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> start");
                    }
                    if (null != oldView) {
                        newView.setAlpha(TRANSPARENT);
                        oldView.setAlpha(FULL_COLOR);
                    } else {
                        newView.setAlpha(FULL_COLOR);
                    }

                    dispatchChangeStarting(newHolder, false);
                }

                @Override
                public void onAnimationEnd(Animator animation) {
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> end");
                    }
                    if (mCanceled) {
                        newHolder.mDeviceRootLayout.setAlpha(TRANSPARENT);
                    } else {
                        Log.v(TAG, "  new view -> full color");
                        newView.setAlpha(FULL_COLOR);
                        newView.setTranslationX(0);
                        newView.setTranslationY(0);
                        if (null != oldView) {
                            Log.v(TAG, "  old view -> transparent");
                            oldView.setAlpha(TRANSPARENT);
                        }
                    }

                    if (!mCanceled && newHolder.needsScrollFocus()) {
                        scrollToHolder(newHolder);
                    }

                    translationAnimators.removeListener(this);
                    mRunningAnimators.remove(newHolder);
                    dispatchChangeFinished(newHolder, false);
                    releaseUserScrolling();
                }

                @Override
                public void onAnimationCancel(Animator animation) {
                    mCanceled = true;
                    if (FcConstants.OPT_DETAILED_LOGS) {
                        Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> cancel");
                    }
                }
            });

            mRunningAnimators.put(newHolder, translationAnimators);
            translationAnimators.start();
        }
    }

    private void scrollToHolder(OrdinalAppViewHolder holder) {
        if (isScrollingAllowed()) {
            int position = holder.getAdapterPosition();
            if (position >= 0) {
                Log.d(TAG, "Scrolling to position: " + position);
                mRecyclerView.smoothScrollToPosition(position);
                mRecyclerView.addOnScrollListener(mScrollListener);
                holder.setScrollFocusFinished();
            } else {
                Log.w(TAG, "Cannot scroll to position " + position);
            }
        }
    }

    private void addTranslateOldAnimator(List<Animator> animatorList, ChangeInfo change,
                                         OrdinalAppViewHolder holder) {

        final AnimatorSet translationAnimators = new AnimatorSet();

        translationAnimators.playTogether(
                ObjectAnimator.ofFloat(holder.itemView, View.TRANSLATION_X, change.toX - change.fromX),
                ObjectAnimator.ofFloat(holder.itemView, View.TRANSLATION_Y, change.toY - change.fromY)
        );
        translationAnimators.setDuration(getChangeDuration());

        animatorList.add(translationAnimators);
    }

    /**
     * Adds an app button "clicked" animation to the given list
     *
     * @param parallelAnimators
     * @param holder
     */
    private void addOrdinalAppButtonClickedAnimation(List<Animator> parallelAnimators,
                                                     OrdinalAppViewHolder holder) {

        Animator animator = mFactory.createScaleAnimator(holder.mDeviceAppButton,
                FcConstants.DEFAULT_SCALE_ANIM_DURATION, 0.3f, 0.2f);

        parallelAnimators.add(animator);
    }

    @Override
    protected void onPostEndAnimation(ViewHolder item) {
        Log.d(TAG, "onPostEndAnimation(" + item + ")");
        Animator a = mRunningAnimators.get(item);
        if (null != a && a.isRunning()) {
            a.cancel();
        }
    }

    private void releaseUserScrolling() {
        if (--mScrollClientsCount == 0) {
             mRecyclerView.removeOnItemTouchListener(mDisableScrollingListener);
        }
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.v(TAG, "releaseUserScrolling(clients: " + mScrollClientsCount + ")");
        }
    }

    private void acquireUserScrolling() {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.v(TAG, "acquireUserScrolling(clients: " + mScrollClientsCount + ")");
        }
        if (mScrollClientsCount++ == 0) {
             mRecyclerView.addOnItemTouchListener(mDisableScrollingListener);
        }
    }

    private boolean isScrollingAllowed() {
        return mScrollStatus == RecyclerView.SCROLL_STATE_IDLE;
    }

    public boolean isRtlDirection() {
        return mRecyclerView.getLayoutDirection() == LayoutDirection.RTL;
    }

    private static class ScrollDisabler implements RecyclerView.OnItemTouchListener {

        @Override
        public boolean onInterceptTouchEvent(RecyclerView recyclerView, MotionEvent motionEvent) {
            return true;
        }

        @Override
        public void onTouchEvent(RecyclerView recyclerView, MotionEvent motionEvent) {
            // NOOP
        }

        @Override
        public void onRequestDisallowInterceptTouchEvent(boolean b) {
            // NOOP
        }
    }
}
