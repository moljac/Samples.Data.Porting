using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Animator = android.animation.Animator;
	using AnimatorSet = android.animation.AnimatorSet;
	using ObjectAnimator = android.animation.ObjectAnimator;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using ViewHolder = android.support.v7.widget.RecyclerView.ViewHolder;
	using LayoutDirection = android.util.LayoutDirection;
	using Log = android.util.Log;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;


	/// <summary>
	/// This implementation of <seealso cref="RecyclerView.ItemAnimator"/> is a modified
	/// <seealso cref="DefaultItemAnimator"/> that replaces default change animation to
	/// more complex.
	/// </summary>
	/// <seealso cref= RecyclerView#setItemAnimator(RecyclerView.ItemAnimator) </seealso>
	public class FcItemAnimator : DefaultItemAnimator
	{

		private static readonly string TAG = typeof(FcItemAnimator).Name;
		private readonly RecyclerView mRecyclerView;
		private readonly FcAnimator mFactory = new FcAnimator();
		private readonly IDictionary<RecyclerView.ViewHolder, Animator> mRunningAnimators;
		private readonly RecyclerView.OnItemTouchListener mDisableScrollingListener = new ScrollDisabler();

		private const float TRANSPARENT = 0f;
		private const float FULL_COLOR = 1f;

		private volatile int mScrollStatus = RecyclerView.SCROLL_STATE_IDLE;
		private RecyclerView.OnScrollListener mScrollListener = new OnScrollListenerAnonymousInnerClassHelper();

		private class OnScrollListenerAnonymousInnerClassHelper : RecyclerView.OnScrollListener
		{
			public OnScrollListenerAnonymousInnerClassHelper()
			{
			}

			public override void onScrollStateChanged(RecyclerView recyclerView, int newState)
			{
				outerInstance.mScrollStatus = newState;
			}
		}
		private int mScrollClientsCount = 0;

		public FcItemAnimator(RecyclerView recyclerView)
		{
			mRecyclerView = recyclerView;
			mRunningAnimators = new Dictionary<RecyclerView.ViewHolder, Animator>();
			SupportsChangeAnimations = true;
		}

		protected internal override void animateChangeImpl(DefaultItemAnimator.ChangeInfo changeInfo)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v7.widget.RecyclerView.ViewHolder holder = changeInfo.oldHolder;
			RecyclerView.ViewHolder holder = changeInfo.oldHolder;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v7.widget.RecyclerView.ViewHolder newHolder = changeInfo.newHolder;
			RecyclerView.ViewHolder newHolder = changeInfo.newHolder;

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				if (null != holder)
				{
					Log.d(TAG, "animateChangeImpl(position=" + holder.AdapterPosition + ")");
				}
				else if (null != newHolder)
				{
					Log.d(TAG, "animateChangeImpl(position=" + newHolder.AdapterPosition + ")");
				}
				else
				{
					Log.d(TAG, "animateChangeImpl(both nulls)");
				}
				Log.d(TAG, "   old = " + holder);
				Log.d(TAG, "   new = " + newHolder);
				Log.d(TAG, "   change: " + changeInfo);
			}

			if (newHolder is OrdinalAppViewHolder)
			{
				if (holder is OrdinalAppViewHolder || null == holder)
				{
					animateChangeOrdinalApp(changeInfo, (OrdinalAppViewHolder) holder, (OrdinalAppViewHolder) newHolder);
				}
			}
			else
			{
				animateChangeDefault(changeInfo, holder, newHolder);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override protected void animateAddImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		protected internal override void animateAddImpl(RecyclerView.ViewHolder holder)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateAddImpl");
				Log.d(TAG, "    holder: " + holder);
				Log.d(TAG, "    duration: " + AddDuration);
				Log.d(TAG, "    translation: " + view.TranslationX + ", " + view.TranslationY);
				if (holder is OrdinalAppViewHolder)
				{
					Log.d(TAG, "    needs scroll focus: " + ((OrdinalAppViewHolder) holder).needsScrollFocus());
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.Animator fadeInAnimator = android.animation.ObjectAnimator.ofFloat(view, android.view.View.ALPHA, FULL_COLOR);
			Animator fadeInAnimator = ObjectAnimator.ofFloat(view, View.ALPHA, FULL_COLOR);
			fadeInAnimator.Duration = AddDuration;
			fadeInAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper(this, holder, view, fadeInAnimator));

			mRunningAnimators[holder] = fadeInAnimator;
			fadeInAnimator.start();
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View view;
			private Animator fadeInAnimator;

			public FcAnimatorListenerAnonymousInnerClassHelper(FcItemAnimator outerInstance, RecyclerView.ViewHolder holder, View view, Animator fadeInAnimator)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.view = view;
				this.fadeInAnimator = fadeInAnimator;
				mCanceled = false;
			}


			private bool mCanceled;

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateAddImpl -> holder -> start");
				}
				dispatchAddStarting(holder);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateAddImpl -> holder -> end");
				}
				view.Alpha = FULL_COLOR;
				outerInstance.mRunningAnimators.Remove(holder);
				fadeInAnimator.removeListener(this);
				dispatchAddFinished(holder);
				outerInstance.dispatchFinishedWhenDone();

				if (holder is OrdinalAppViewHolder)
				{
					OrdinalAppViewHolder appHolder = (OrdinalAppViewHolder) holder;
					if (!mCanceled && appHolder.needsScrollFocus())
					{
						outerInstance.scrollToHolder(appHolder);
					}
				}
			}

			public override void onAnimationCancel(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateAddImpl -> holder -> cancel");
				}
				mCanceled = true;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override protected void animateRemoveImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder)
		protected internal override void animateRemoveImpl(RecyclerView.ViewHolder holder)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateRemoveImpl");
				Log.d(TAG, "    holder: " + holder);
				Log.d(TAG, "    duration: " + AddDuration);
				Log.d(TAG, "    translation: " + view.TranslationX + ", " + view.TranslationY);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.Animator fadeOutAnimator = android.animation.ObjectAnimator.ofFloat(view, android.view.View.ALPHA, TRANSPARENT);
			Animator fadeOutAnimator = ObjectAnimator.ofFloat(view, View.ALPHA, TRANSPARENT);
			fadeOutAnimator.Duration = RemoveDuration;
			fadeOutAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper2(this, holder, view));

			mRunningAnimators[holder] = fadeOutAnimator;
			fadeOutAnimator.start();
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper2 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View view;

			public FcAnimatorListenerAnonymousInnerClassHelper2(FcItemAnimator outerInstance, RecyclerView.ViewHolder holder, View view)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.view = view;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateRemoveImpl -> holder -> start");
				}
				dispatchRemoveStarting(holder);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateRemoveImpl -> holder -> end");
				}
				view.Alpha = TRANSPARENT;
				outerInstance.mRunningAnimators.Remove(holder);
				dispatchRemoveFinished(holder);
				outerInstance.dispatchFinishedWhenDone();
			}

			public override void onAnimationCancel(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateRemoveImpl -> holder -> cancel");
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override protected void animateMoveImpl(final android.support.v7.widget.RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		protected internal override void animateMoveImpl(RecyclerView.ViewHolder holder, int fromX, int fromY, int toX, int toY)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View view = holder.itemView;
			View view = holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int deltaX = toX - fromX;
			int deltaX = toX - fromX;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int deltaY = toY - fromY;
			int deltaY = toY - fromY;

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateMoveImpl(delta: " + deltaX + ", " + deltaY + ")");
				Log.d(TAG, "    duration: " + MoveDuration);
				Log.d(TAG, "    translation: " + view.TranslationX + ", " + view.TranslationY);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet animator = new android.animation.AnimatorSet();
			AnimatorSet animator = new AnimatorSet();
			animator.playTogether(ObjectAnimator.ofFloat(view, View.TRANSLATION_X, 0), ObjectAnimator.ofFloat(view, View.TRANSLATION_Y, 0));

			acquireUserScrolling();

			animator.Duration = MoveDuration;
			animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper3(this, holder));

			mRunningAnimators[holder] = animator;
			animator.start();
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper3 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;

			public FcAnimatorListenerAnonymousInnerClassHelper3(FcItemAnimator outerInstance, RecyclerView.ViewHolder holder)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
			}

			public override void onAnimationStart(Animator animation)
			{
				dispatchMoveStarting(holder);
			}

			public override void onAnimationEnd(Animator animation)
			{
				outerInstance.releaseUserScrolling();
				outerInstance.mMoveAnimations.Remove(holder);
				animation.removeListener(this);
				dispatchMoveFinished(holder);
			}
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="changeInfo"> </param>
		/// <param name="holder"> </param>
		/// <param name="newHolder"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void animateChangeDefault(final ChangeInfo changeInfo, final android.support.v7.widget.RecyclerView.ViewHolder holder, final android.support.v7.widget.RecyclerView.ViewHolder newHolder)
		private void animateChangeDefault(ChangeInfo changeInfo, RecyclerView.ViewHolder holder, RecyclerView.ViewHolder newHolder)
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateChangeDefault");
				Log.d(TAG, "   duration = " + ChangeDuration);
				Log.d(TAG, "   changeInfo: " + changeInfo);
				Log.d(TAG, "   old = " + holder);
				Log.d(TAG, "   new = " + newHolder);
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View oldView = holder == null ? null : holder.itemView;
			View oldView = holder == null ? null : holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = newHolder != null ? newHolder.itemView : null;
			View newView = newHolder != null ? newHolder.itemView : null;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasNoTranslations = changeInfo.hasNoTranslation();
			bool hasNoTranslations = changeInfo.hasNoTranslation();

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "   old view = " + oldView);
				Log.d(TAG, "   new view = " + newView);
				Log.d(TAG, "   has no translations = " + hasNoTranslations);
			}

			if (oldView != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet animator = new android.animation.AnimatorSet();
				AnimatorSet animator = new AnimatorSet();

				if (hasNoTranslations)
				{
					animator.Duration = 0;
				}
				else
				{
					animator.Duration = ChangeDuration;
				}

	//            acquireUserScrolling();

				animator.playSequentially(ObjectAnimator.ofFloat(oldView, View.TRANSLATION_X, changeInfo.toX - changeInfo.fromX), ObjectAnimator.ofFloat(oldView, View.TRANSLATION_Y, changeInfo.toY - changeInfo.fromY));
				animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper4(this, holder, oldView, newView, animator));

				mRunningAnimators[holder] = animator;
				animator.start();
			}

			if (newView != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet animator = new android.animation.AnimatorSet();
				AnimatorSet animator = new AnimatorSet();
				if (hasNoTranslations)
				{
					animator.Duration = 0;
				}
				else
				{
					animator.Duration = ChangeDuration;
				}

				acquireUserScrolling();

				animator.playTogether(ObjectAnimator.ofFloat(newView, View.TRANSLATION_X, 0), ObjectAnimator.ofFloat(newView, View.TRANSLATION_Y, 0));

				animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper5(this, newHolder, oldView, newView, animator));

				mRunningAnimators[newHolder] = animator;
				animator.start();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper4 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private RecyclerView.ViewHolder holder;
			private View oldView;
			private View newView;
			private AnimatorSet animator;

			public FcAnimatorListenerAnonymousInnerClassHelper4(FcItemAnimator outerInstance, RecyclerView.ViewHolder holder, View oldView, View newView, AnimatorSet animator)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.oldView = oldView;
				this.newView = newView;
				this.animator = animator;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateChangeDefault -> oldHolder -> start");
				}
				oldView.Alpha = FULL_COLOR;
				if (null != newView)
				{
					newView.Alpha = TRANSPARENT;
				}
				dispatchChangeStarting(holder, true);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateChangeDefault -> oldHolder -> end");
				}
				oldView.Alpha = TRANSPARENT;
				if (null != newView)
				{
					newView.Alpha = FULL_COLOR;
				}

				animator.removeListener(this);
				outerInstance.mRunningAnimators.Remove(holder);
				dispatchChangeFinished(holder, true);
				outerInstance.dispatchFinishedWhenDone();
		//                    releaseUserScrolling();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper5 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private RecyclerView.ViewHolder newHolder;
			private View oldView;
			private View newView;
			private AnimatorSet animator;

			public FcAnimatorListenerAnonymousInnerClassHelper5(FcItemAnimator outerInstance, RecyclerView.ViewHolder newHolder, View oldView, View newView, AnimatorSet animator)
			{
				this.outerInstance = outerInstance;
				this.newHolder = newHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.animator = animator;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateChangeDefault -> newHolder -> start");
				}
				if (null != oldView)
				{
					newView.Alpha = TRANSPARENT;
				}
				else
				{
					newView.Alpha = FULL_COLOR;
				}
				dispatchChangeStarting(newHolder, false);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateChangeDefault -> newHolder -> end");
				}
				newView.Alpha = FULL_COLOR;
				if (null != oldView)
				{
					oldView.Alpha = TRANSPARENT;
				}

				animator.removeListener(this);
				outerInstance.mRunningAnimators.Remove(animator);
				dispatchChangeFinished(newHolder, false);
				outerInstance.dispatchFinishedWhenDone();
				outerInstance.releaseUserScrolling();
			}
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="changeInfo"> </param>
		/// <param name="holder"> </param>
		/// <param name="newHolder"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void animateChangeOrdinalApp(final ChangeInfo changeInfo, final OrdinalAppViewHolder holder, final OrdinalAppViewHolder newHolder)
		private void animateChangeOrdinalApp(ChangeInfo changeInfo, OrdinalAppViewHolder holder, OrdinalAppViewHolder newHolder)
		{

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateChangeOrdinalApp(position=" + newHolder.AdapterPosition + ", " + changeInfo + ")");
				Log.d(TAG, "   duration = " + ChangeDuration);
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = newHolder.itemView;
			View newView = newHolder.itemView;

			// Default animation whenever one of the two views is not present
			if (newView == null)
			{
				animateChangeDefault(changeInfo, holder, newHolder);
				return;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int oldWidth = (null == holder) ? 0 : holder.mDeviceRootLayout.getWidth();
			int oldWidth = (null == holder) ? 0 : holder.mDeviceRootLayout.Width;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int newWidth = newHolder.mDeviceRootLayout.getWidth();
			int newWidth = newHolder.mDeviceRootLayout.Width;

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "oldWidth = " + oldWidth + ", newWidth = " + newWidth);
			}

			if (oldWidth == 0 && newWidth > 0)
			{
				animateOrdinalAppExpanding(changeInfo, holder, newHolder);

			}
			else if (oldWidth > 0 && newWidth == 0)
			{
				animateOrdinalAppCollapsing(changeInfo, holder, newHolder);

			}
			else
			{
				animateOrdinalAppUpdate(changeInfo, holder, newHolder);
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void animateOrdinalAppUpdate(final ChangeInfo changeInfo, final OrdinalAppViewHolder holder, final OrdinalAppViewHolder newHolder)
		private void animateOrdinalAppUpdate(ChangeInfo changeInfo, OrdinalAppViewHolder holder, OrdinalAppViewHolder newHolder)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View oldView = (null == holder) ? null : holder.itemView;
			View oldView = (null == holder) ? null : holder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = (null == newHolder) ? null : newHolder.itemView;
			View newView = (null == newHolder) ? null : newHolder.itemView;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final boolean hasNoTranslations = changeInfo.hasNoTranslation();
			bool hasNoTranslations = changeInfo.hasNoTranslation();
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateOrdinalAppUpdate(" + holder + ")");
				Log.d(TAG, "   change info: " + changeInfo);
				Log.d(TAG, "   no translations: " + changeInfo.hasNoTranslation());
			}

			if (oldView != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet animator = new android.animation.AnimatorSet();
				AnimatorSet animator = new AnimatorSet();
				animator.playTogether(ObjectAnimator.ofFloat(oldView, View.TRANSLATION_X, changeInfo.toX - changeInfo.fromX), ObjectAnimator.ofFloat(oldView, View.TRANSLATION_Y, changeInfo.toY - changeInfo.fromY));

				if (hasNoTranslations)
				{
					if (FcConstants.OPT_DETAILED_LOGS)
					{
						Log.v(TAG, "animateOrdinalAppUpdate -> oldHolder -> no translations, setting duration to 0");
					}
					animator.Duration = 0;
				}
				else
				{
					animator.Duration = ChangeDuration;
				}

	//            acquireUserScrolling();

				animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper6(this, holder, oldView, newView, animator));

				mRunningAnimators[holder] = animator;
				animator.start();
			}

			if (newView != null)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet animator = new android.animation.AnimatorSet();
				AnimatorSet animator = new AnimatorSet();
				animator.playTogether(ObjectAnimator.ofFloat(newView, View.TRANSLATION_X, 0), ObjectAnimator.ofFloat(newView, View.TRANSLATION_Y, 0));

				if (hasNoTranslations)
				{
					if (FcConstants.OPT_DETAILED_LOGS)
					{
						Log.v(TAG, "animateOrdinalAppUpdate -> oldHolder -> no translations, setting duration to 0");
					}
					animator.Duration = 0;
				}
				else
				{
					animator.Duration = ChangeDuration;
				}

				acquireUserScrolling();

				animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper7(this, newHolder, oldView, newView, animator));

				mRunningAnimators[newHolder] = animator;
				animator.start();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper6 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder holder;
			private View oldView;
			private View newView;
			private AnimatorSet animator;

			public FcAnimatorListenerAnonymousInnerClassHelper6(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder holder, View oldView, View newView, AnimatorSet animator)
			{
				this.outerInstance = outerInstance;
				this.holder = holder;
				this.oldView = oldView;
				this.newView = newView;
				this.animator = animator;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> start");
				}
				oldView.Alpha = FULL_COLOR;
				dispatchChangeStarting(holder, true);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> end");
				}
				oldView.Alpha = TRANSPARENT;
				oldView.TranslationX = 0;
				oldView.TranslationY = 0;
				if (null != newView)
				{
					newView.Alpha = FULL_COLOR;
				}

				animator.removeListener(this);
				outerInstance.mRunningAnimators.Remove(holder);
				dispatchChangeFinished(holder, true);

		//                    if (!hasNoTranslations) {
		//                    releaseUserScrolling();
		//                    }
			}

			public override void onAnimationCancel(Animator animation)
			{
				Log.d(TAG, "animateOrdinalAppUpdate -> oldHolder -> cancel");
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper7 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder;
			private View oldView;
			private View newView;
			private AnimatorSet animator;

			public FcAnimatorListenerAnonymousInnerClassHelper7(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder, View oldView, View newView, AnimatorSet animator)
			{
				this.outerInstance = outerInstance;
				this.newHolder = newHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.animator = animator;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> start");
				}
				newView.Alpha = TRANSPARENT;
				if (null != oldView)
				{
					oldView.Alpha = FULL_COLOR;
				}
				dispatchChangeStarting(newHolder, false);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> end");
				}
				newView.Alpha = FULL_COLOR;
				newView.TranslationX = 0;
				newView.TranslationY = 0;
				if (null != oldView)
				{
					oldView.Alpha = TRANSPARENT;
				}

				outerInstance.mRunningAnimators.Remove(newHolder);
				animator.removeListener(this);
				dispatchChangeFinished(newHolder, false);

		//                    if (!hasNoTranslations) {
					outerInstance.releaseUserScrolling();
		//                    }
			}

			public override void onAnimationCancel(Animator animation)
			{
				Log.d(TAG, "animateOrdinalAppUpdate -> newHolder -> cancel");
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void animateOrdinalAppCollapsing(final ChangeInfo changeInfo, final OrdinalAppViewHolder oldHolder, final OrdinalAppViewHolder newHolder)
		private void animateOrdinalAppCollapsing(ChangeInfo changeInfo, OrdinalAppViewHolder oldHolder, OrdinalAppViewHolder newHolder)
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "    change info: " + changeInfo);
				Log.d(TAG, "    old holder : " + oldHolder);
				if (null != oldHolder)
				{
					Log.d(TAG, "      old view: " + oldHolder.itemView);
					Log.d(TAG, "      needs scroll focus: " + oldHolder.needsScrollFocus());
				}
				if (null != newHolder)
				{
					Log.d(TAG, "      new view: " + newHolder.itemView);
					Log.d(TAG, "      needs scroll focus: " + newHolder.needsScrollFocus());
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View oldView = (null == oldHolder) ? null : oldHolder.itemView;
			View oldView = (null == oldHolder) ? null : oldHolder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = (null == newHolder) ? null : newHolder.itemView;
			View newView = (null == newHolder) ? null : newHolder.itemView;

			if (null != oldView)
			{
				IList<Animator> animatorList = new List<Animator>();
				addOrdinalAppButtonClickedAnimation(animatorList, oldHolder);
				addTranslateOldAnimator(animatorList, changeInfo, oldHolder);

				acquireUserScrolling();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.Animator collapseAnimator = mFactory.createCollapseActionsAnimator(oldHolder.mDeviceRootLayout, getChangeDuration());
				Animator collapseAnimator = mFactory.createCollapseActionsAnimator(oldHolder.mDeviceRootLayout, ChangeDuration);
				collapseAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper8(this, oldHolder, oldView, newView, collapseAnimator));

				animatorList.Add(collapseAnimator);

				AnimatorSet mainAnimator = new AnimatorSet();
				mainAnimator.playTogether(animatorList);

				mRunningAnimators[oldHolder] = mainAnimator;
				mainAnimator.start();
			}

			if (null != newView)
			{
				acquireUserScrolling();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet translationAnimators = new android.animation.AnimatorSet();
				AnimatorSet translationAnimators = new AnimatorSet();
				translationAnimators.Duration = ChangeDuration;
				translationAnimators.playTogether(ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_X, 0), ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_Y, 0));
				translationAnimators.addListener(new FcAnimatorListenerAnonymousInnerClassHelper9(this, newHolder, oldView, newView, translationAnimators));

				mRunningAnimators[newHolder] = translationAnimators;
				translationAnimators.start();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper8 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder oldHolder;
			private View oldView;
			private View newView;
			private Animator collapseAnimator;

			public FcAnimatorListenerAnonymousInnerClassHelper8(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder oldHolder, View oldView, View newView, Animator collapseAnimator)
			{
				this.outerInstance = outerInstance;
				this.oldHolder = oldHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.collapseAnimator = collapseAnimator;
			}

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppCollapsing -> oldHolder -> start");
				}
				oldView.Alpha = FULL_COLOR;
				if (null != newView)
				{
					newView.Alpha = TRANSPARENT;
				}
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppCollapsing -> oldHolder -> end");
				}
				oldView.Alpha = TRANSPARENT;
				oldView.TranslationX = 0;
				oldView.TranslationY = 0;
				if (null != newView)
				{
					newView.Alpha = FULL_COLOR;
				}

				outerInstance.mRunningAnimators.Remove(oldHolder);
				collapseAnimator.removeListener(this);

				dispatchChangeFinished(oldHolder, true);
				outerInstance.dispatchFinishedWhenDone();
				outerInstance.releaseUserScrolling();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper9 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder;
			private View oldView;
			private View newView;
			private AnimatorSet translationAnimators;

			public FcAnimatorListenerAnonymousInnerClassHelper9(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder, View oldView, View newView, AnimatorSet translationAnimators)
			{
				this.outerInstance = outerInstance;
				this.newHolder = newHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.translationAnimators = translationAnimators;
			}


			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppCollapsing -> newHolder -> start");
				}
				newHolder.itemView.Alpha = TRANSPARENT;
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppCollapsing -> newHolder -> end");
				}
				newView.Alpha = FULL_COLOR;
				newView.TranslationX = 0;
				newView.TranslationY = 0;
				if (null != oldView)
				{
					oldView.Alpha = TRANSPARENT;
				}

				if (newHolder.needsScrollFocus())
				{
					newHolder.setScrollFocusFinished();
				}

				translationAnimators.removeListener(this);
				outerInstance.mRunningAnimators.Remove(newHolder);
				dispatchChangeFinished(newHolder, false);
				outerInstance.releaseUserScrolling();
			}
		}

		/// <summary>
		/// This method will be called when app root layout is to be expanded from 0 to some width
		/// 
		/// Old holder will contain a root layout with with = 0, new holder will contain a root layout
		/// with some width greater than 0.
		/// </summary>
		/// <param name="changeInfo"> </param>
		/// <param name="oldHolder"> </param>
		/// <param name="newHolder"> </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void animateOrdinalAppExpanding(final ChangeInfo changeInfo, final OrdinalAppViewHolder oldHolder, final OrdinalAppViewHolder newHolder)
		private void animateOrdinalAppExpanding(ChangeInfo changeInfo, OrdinalAppViewHolder oldHolder, OrdinalAppViewHolder newHolder)
		{

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "animateOrdinalAppExpanding");
				Log.d(TAG, "    change info: " + changeInfo);
				Log.d(TAG, "    old holder : " + oldHolder);
				if (null != oldHolder)
				{
					Log.d(TAG, "      old view: " + oldHolder.itemView);
					Log.d(TAG, "      needs scroll focus: " + oldHolder.needsScrollFocus());
				}
				if (null != newHolder)
				{
					Log.d(TAG, "      new view: " + newHolder.itemView);
					Log.d(TAG, "      needs scroll focus: " + newHolder.needsScrollFocus());
				}
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View oldView = (null == oldHolder) ? null : oldHolder.itemView;
			View oldView = (null == oldHolder) ? null : oldHolder.itemView;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.view.View newView = (null == newHolder) ? null : newHolder.itemView;
			View newView = (null == newHolder) ? null : newHolder.itemView;

			if (null != oldView)
			{
				IList<Animator> oldAnimatorList = new List<Animator>();

				addOrdinalAppButtonClickedAnimation(oldAnimatorList, oldHolder);
				addTranslateOldAnimator(oldAnimatorList, changeInfo, oldHolder);

				acquireUserScrolling();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.Animator expandAnimator = mFactory.createExpandActionsAnimator(oldHolder.mDeviceRootLayout, getChangeDuration());
				Animator expandAnimator = mFactory.createExpandActionsAnimator(oldHolder.mDeviceRootLayout, ChangeDuration);
				expandAnimator.Duration = ChangeDuration;
				expandAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper10(this, oldHolder, oldView, newView, expandAnimator));
				oldAnimatorList.Add(expandAnimator);

				AnimatorSet mainAnimator = new AnimatorSet();
				mainAnimator.Duration = ChangeDuration;
				mainAnimator.playTogether(oldAnimatorList);

				mRunningAnimators[oldHolder] = mainAnimator;
				mainAnimator.start();
			}

			if (null != newView)
			{
				acquireUserScrolling();

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet translationAnimators = new android.animation.AnimatorSet();
				AnimatorSet translationAnimators = new AnimatorSet();

				if (RtlDirection)
				{
					if (FcConstants.OPT_DETAILED_LOGS)
					{
						Log.v(TAG, "RTL direction: setting animation duration to 0");
					}
					translationAnimators.Duration = 0;
				}
				else
				{
					translationAnimators.Duration = ChangeDuration;
				}

				translationAnimators.playTogether(ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_X, 0), ObjectAnimator.ofFloat(newHolder.itemView, View.TRANSLATION_Y, 0));
				translationAnimators.addListener(new FcAnimatorListenerAnonymousInnerClassHelper11(this, newHolder, oldView, newView, translationAnimators));

				mRunningAnimators[newHolder] = translationAnimators;
				translationAnimators.start();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper10 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder oldHolder;
			private View oldView;
			private View newView;
			private Animator expandAnimator;

			public FcAnimatorListenerAnonymousInnerClassHelper10(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder oldHolder, View oldView, View newView, Animator expandAnimator)
			{
				this.outerInstance = outerInstance;
				this.oldHolder = oldHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.expandAnimator = expandAnimator;
				mCanceled = false;
			}


			internal bool mCanceled;

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> start");
				}
				oldView.Alpha = FULL_COLOR;
				if (null != newView)
				{
					newView.Alpha = TRANSPARENT;
				}

				dispatchChangeStarting(oldHolder, true);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> end");
				}
				if (!mCanceled)
				{
					Log.v(TAG, "   old view alpha -> transparent");
					oldView.Alpha = TRANSPARENT;
					oldView.TranslationX = 0;
					oldView.TranslationY = 0;
					if (null != newView)
					{
						Log.v(TAG, "  new view alpha -> full color");
						newView.Alpha = FULL_COLOR;
					}
				}

				outerInstance.mRunningAnimators.Remove(oldHolder);
				expandAnimator.removeListener(this);

				dispatchChangeFinished(oldHolder, true);
				outerInstance.dispatchFinishedWhenDone();
				outerInstance.releaseUserScrolling();
			}

			public override void onAnimationCancel(Animator animation)
			{
				mCanceled = true;
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> oldHolder -> cancel");
				}
				if (null != newView)
				{
					Log.v(TAG, "  new view: full color");
					Log.v(TAG, "  new view: translation (0, 0)");
					newView.Alpha = FULL_COLOR;
					newView.TranslationX = 0f;
					newView.TranslationY = 0f;
				}
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper11 : FcAnimator.FcAnimatorListener
		{
			private readonly FcItemAnimator outerInstance;

			private com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder;
			private View oldView;
			private View newView;
			private AnimatorSet translationAnimators;

			public FcAnimatorListenerAnonymousInnerClassHelper11(FcItemAnimator outerInstance, com.samsung.android.sdk.professionalaudio.widgets.refactor.OrdinalAppViewHolder newHolder, View oldView, View newView, AnimatorSet translationAnimators)
			{
				this.outerInstance = outerInstance;
				this.newHolder = newHolder;
				this.oldView = oldView;
				this.newView = newView;
				this.translationAnimators = translationAnimators;
				mCanceled = false;
			}

			private bool mCanceled;

			public override void onAnimationStart(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> start");
				}
				if (null != oldView)
				{
					newView.Alpha = TRANSPARENT;
					oldView.Alpha = FULL_COLOR;
				}
				else
				{
					newView.Alpha = FULL_COLOR;
				}

				dispatchChangeStarting(newHolder, false);
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> end");
				}
				if (mCanceled)
				{
					newHolder.mDeviceRootLayout.Alpha = TRANSPARENT;
				}
				else
				{
					Log.v(TAG, "  new view -> full color");
					newView.Alpha = FULL_COLOR;
					newView.TranslationX = 0;
					newView.TranslationY = 0;
					if (null != oldView)
					{
						Log.v(TAG, "  old view -> transparent");
						oldView.Alpha = TRANSPARENT;
					}
				}

				if (!mCanceled && newHolder.needsScrollFocus())
				{
					outerInstance.scrollToHolder(newHolder);
				}

				translationAnimators.removeListener(this);
				outerInstance.mRunningAnimators.Remove(newHolder);
				dispatchChangeFinished(newHolder, false);
				outerInstance.releaseUserScrolling();
			}

			public override void onAnimationCancel(Animator animation)
			{
				mCanceled = true;
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "animateOrdinalAppExpanding -> newHolder -> cancel");
				}
			}
		}

		private void scrollToHolder(OrdinalAppViewHolder holder)
		{
			if (ScrollingAllowed)
			{
				int position = holder.AdapterPosition;
				if (position >= 0)
				{
					Log.d(TAG, "Scrolling to position: " + position);
					mRecyclerView.smoothScrollToPosition(position);
					mRecyclerView.addOnScrollListener(mScrollListener);
					holder.setScrollFocusFinished();
				}
				else
				{
					Log.w(TAG, "Cannot scroll to position " + position);
				}
			}
		}

		private void addTranslateOldAnimator(IList<Animator> animatorList, ChangeInfo change, OrdinalAppViewHolder holder)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.animation.AnimatorSet translationAnimators = new android.animation.AnimatorSet();
			AnimatorSet translationAnimators = new AnimatorSet();

			translationAnimators.playTogether(ObjectAnimator.ofFloat(holder.itemView, View.TRANSLATION_X, change.toX - change.fromX), ObjectAnimator.ofFloat(holder.itemView, View.TRANSLATION_Y, change.toY - change.fromY));
			translationAnimators.Duration = ChangeDuration;

			animatorList.Add(translationAnimators);
		}

		/// <summary>
		/// Adds an app button "clicked" animation to the given list
		/// </summary>
		/// <param name="parallelAnimators"> </param>
		/// <param name="holder"> </param>
		private void addOrdinalAppButtonClickedAnimation(IList<Animator> parallelAnimators, OrdinalAppViewHolder holder)
		{

			Animator animator = mFactory.createScaleAnimator(holder.mDeviceAppButton, FcConstants.DEFAULT_SCALE_ANIM_DURATION, 0.3f, 0.2f);

			parallelAnimators.Add(animator);
		}

		protected internal override void onPostEndAnimation(RecyclerView.ViewHolder item)
		{
			Log.d(TAG, "onPostEndAnimation(" + item + ")");
			Animator a = mRunningAnimators[item];
			if (null != a && a.Running)
			{
				a.cancel();
			}
		}

		private void releaseUserScrolling()
		{
			if (--mScrollClientsCount == 0)
			{
				 mRecyclerView.removeOnItemTouchListener(mDisableScrollingListener);
			}
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.v(TAG, "releaseUserScrolling(clients: " + mScrollClientsCount + ")");
			}
		}

		private void acquireUserScrolling()
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.v(TAG, "acquireUserScrolling(clients: " + mScrollClientsCount + ")");
			}
			if (mScrollClientsCount++ == 0)
			{
				 mRecyclerView.addOnItemTouchListener(mDisableScrollingListener);
			}
		}

		private bool ScrollingAllowed
		{
			get
			{
				return mScrollStatus == RecyclerView.SCROLL_STATE_IDLE;
			}
		}

		public virtual bool RtlDirection
		{
			get
			{
				return mRecyclerView.LayoutDirection == LayoutDirection.RTL;
			}
		}

		private class ScrollDisabler : RecyclerView.OnItemTouchListener
		{

			public override bool onInterceptTouchEvent(RecyclerView recyclerView, MotionEvent motionEvent)
			{
				return true;
			}

			public override void onTouchEvent(RecyclerView recyclerView, MotionEvent motionEvent)
			{
				// NOOP
			}

			public override void onRequestDisallowInterceptTouchEvent(bool b)
			{
				// NOOP
			}
		}
	}

}