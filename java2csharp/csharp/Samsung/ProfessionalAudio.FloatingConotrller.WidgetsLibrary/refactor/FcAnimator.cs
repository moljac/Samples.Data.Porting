namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Animator = android.animation.Animator;
	using AnimatorSet = android.animation.AnimatorSet;
	using ObjectAnimator = android.animation.ObjectAnimator;
	using ValueAnimator = android.animation.ValueAnimator;
	using Log = android.util.Log;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using CycleInterpolator = android.view.animation.CycleInterpolator;

	using LinearLayoutManager = org.solovyev.android.views.llm.LinearLayoutManager;

	/// <summary>
	/// @brief Factory class to create and setup animators for components of Floating Controller
	/// </summary>
	public class FcAnimator
	{

		public static readonly string TAG = typeof(FcAnimator).Name;

		/// <summary>
		/// @brief Create animator to hide selected view
		/// 
		/// The Animator will
		/// - Shrink layout width from full to 0
		/// - Set visibility of the view to GONE
		/// </summary>
		/// <param name="toHide">    View to be hidden </param>
		/// <param name="duration">  Animation duration
		/// </param>
		/// <returns>  Sequential animator to hide the element </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.animation.Animator createCollapseAnimator(final android.view.View toHide, long duration)
		public virtual Animator createCollapseAnimator(View toHide, long duration)
		{
			ValueAnimator collapseAnimator = ValueAnimator.ofFloat(1f, 0f);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int fromDimension = toHide.getWidth();
			int fromDimension = toHide.Width;

			collapseAnimator.Duration = duration;
			collapseAnimator.addUpdateListener(new AnimatorUpdateListenerAnonymousInnerClassHelper(this, toHide, fromDimension));

			collapseAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper(this, toHide));

			return collapseAnimator;
		}

		private class AnimatorUpdateListenerAnonymousInnerClassHelper : ValueAnimator.AnimatorUpdateListener
		{
			private readonly FcAnimator outerInstance;

			private View toHide;
			private int fromDimension;

			public AnimatorUpdateListenerAnonymousInnerClassHelper(FcAnimator outerInstance, View toHide, int fromDimension)
			{
				this.outerInstance = outerInstance;
				this.toHide = toHide;
				this.fromDimension = fromDimension;
			}

			public override void onAnimationUpdate(ValueAnimator animation)
			{
				float value = (float?) animation.AnimatedValue;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int curPos = (int)(fromDimension * value);
				int curPos = (int)(fromDimension * value);
				toHide.LayoutParams.width = curPos;
				toHide.requestLayout();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper : FcAnimatorListener
		{
			private readonly FcAnimator outerInstance;

			private View toHide;

			public FcAnimatorListenerAnonymousInnerClassHelper(FcAnimator outerInstance, View toHide)
			{
				this.outerInstance = outerInstance;
				this.toHide = toHide;
				mCanceled = false;
			}

			private bool mCanceled;

			public override void onAnimationCancel(Animator animation)
			{
				mCanceled = true;
			}

			public override void onAnimationEnd(Animator animation)
			{
				if (!mCanceled)
				{
					toHide.Visibility = View.GONE;
				}
			}
		}

		/// <summary>
		/// @brief Create animator to hide specified app actions
		/// </summary>
		/// <param name="toShow">        View to be shown </param>
		/// <param name="toDimension"> </param>
		/// <param name="duration">      Animation duration of each sub animator
		/// </param>
		/// <returns>  Sequential animator to show the hidden element </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.animation.Animator createExpandAnimator(final android.view.View toShow, final int toDimension, long duration)
		public virtual Animator createExpandAnimator(View toShow, int toDimension, long duration)
		{

			Log.d(TAG, "View params:");
			Log.d(TAG, "  before measure getWidth: " + toShow.Width);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float fromWidth = toShow.getWidth();
			float fromWidth = toShow.Width;

			toShow.LayoutParams.width = ViewGroup.LayoutParams.WRAP_CONTENT;
			toShow.measure(View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED), View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));

			float toMeasuredWidth = toShow.MeasuredWidth;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float toWidth = (toDimension < toMeasuredWidth) ? toDimension : toMeasuredWidth;
			float toWidth = (toDimension < toMeasuredWidth) ? toDimension : toMeasuredWidth;

			ValueAnimator expandAnimator = ValueAnimator.ofFloat(0f, 1f);
			expandAnimator.Duration = duration;
			expandAnimator.addUpdateListener(new AnimatorUpdateListenerAnonymousInnerClassHelper2(this, toShow, toWidth));

			expandAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper2(this, toShow));

			return expandAnimator;
		}

		private class AnimatorUpdateListenerAnonymousInnerClassHelper2 : ValueAnimator.AnimatorUpdateListener
		{
			private readonly FcAnimator outerInstance;

			private View toShow;
			private float toWidth;

			public AnimatorUpdateListenerAnonymousInnerClassHelper2(FcAnimator outerInstance, View toShow, float toWidth)
			{
				this.outerInstance = outerInstance;
				this.toShow = toShow;
				this.toWidth = toWidth;
			}

			public override void onAnimationUpdate(ValueAnimator animation)
			{
				float value = (float?) animation.AnimatedValue;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int curPos = (int)(toWidth * value);
				int curPos = (int)(toWidth * value);
				toShow.LayoutParams.width = curPos;
				toShow.requestLayout();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper2 : FcAnimatorListener
		{
			private readonly FcAnimator outerInstance;

			private View toShow;

			public FcAnimatorListenerAnonymousInnerClassHelper2(FcAnimator outerInstance, View toShow)
			{
				this.outerInstance = outerInstance;
				this.toShow = toShow;
			}


			public override void onAnimationStart(Animator animation)
			{
				toShow.LayoutParams.width = 0;
				toShow.Visibility = View.VISIBLE;
			}

			public override void onAnimationEnd(Animator animation)
			{
				toShow.LayoutParams.width = ViewGroup.LayoutParams.WRAP_CONTENT;
			}
		}

		/// <summary>
		/// @brief Create animator to hide specified app actions
		/// </summary>
		/// <param name="toShow">        View to be shown </param>
		/// <param name="duration">      Animation duration of each sub animator
		/// </param>
		/// <returns>  Sequential animator to show the hidden element </returns>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.animation.Animator createExpandAnimator(final android.view.View toShow, long duration)
		public virtual Animator createExpandAnimator(View toShow, long duration)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int fromWidth = toShow.getWidth();
			int fromWidth = toShow.Width;
			toShow.LayoutParams.width = ViewGroup.LayoutParams.WRAP_CONTENT;
			toShow.measure(View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED), View.MeasureSpec.makeMeasureSpec(0, View.MeasureSpec.UNSPECIFIED));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int toWidth = toShow.getMeasuredWidth();
			int toWidth = toShow.MeasuredWidth;

			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "createExpandAnimator(" + toShow + ")");
				Log.d(TAG, "    duration: " + duration);
				Log.d(TAG, "    width: " + fromWidth + " -> " + toWidth);
			}

			ValueAnimator expandAnimator = ValueAnimator.ofFloat(0f, 1f);
			expandAnimator.Duration = duration;
			expandAnimator.addUpdateListener(new AnimatorUpdateListenerAnonymousInnerClassHelper3(this, toShow, fromWidth, toWidth));
			expandAnimator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper3(this, toShow, fromWidth));

			return expandAnimator;
		}

		private class AnimatorUpdateListenerAnonymousInnerClassHelper3 : ValueAnimator.AnimatorUpdateListener
		{
			private readonly FcAnimator outerInstance;

			private View toShow;
			private int fromWidth;
			private int toWidth;

			public AnimatorUpdateListenerAnonymousInnerClassHelper3(FcAnimator outerInstance, View toShow, int fromWidth, int toWidth)
			{
				this.outerInstance = outerInstance;
				this.toShow = toShow;
				this.fromWidth = fromWidth;
				this.toWidth = toWidth;
			}

			public override void onAnimationUpdate(ValueAnimator animation)
			{
				float value = (float?) animation.AnimatedValue;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int curPos = fromWidth + (int)((toWidth - fromWidth) * value);
				int curPos = fromWidth + (int)((toWidth - fromWidth) * value);
				toShow.LayoutParams.width = curPos;
				toShow.requestLayout();
			}
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper3 : FcAnimatorListener
		{
			private readonly FcAnimator outerInstance;

			private View toShow;
			private int fromWidth;

			public FcAnimatorListenerAnonymousInnerClassHelper3(FcAnimator outerInstance, View toShow, int fromWidth)
			{
				this.outerInstance = outerInstance;
				this.toShow = toShow;
				this.fromWidth = fromWidth;
			}

			public override void onAnimationStart(Animator animation)
			{
				toShow.LayoutParams.width = fromWidth;
				toShow.Visibility = View.VISIBLE;
			}
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="toShow"> </param>
		/// <param name="duration">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.animation.Animator createExpandActionsAnimator(final android.view.View toShow, long duration)
		public virtual Animator createExpandActionsAnimator(View toShow, long duration)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float baseAlpha = toShow.getAlpha();
			float baseAlpha = toShow.Alpha;

			Animator fadeInAnimator = ObjectAnimator.ofFloat(toShow, View.ALPHA, baseAlpha, 1.0f);
			fadeInAnimator.Duration = FcConstants.DEFAULT_FADE_ANIM_DURATION;

			AnimatorSet animator = new AnimatorSet();
			animator.playSequentially(createExpandAnimator(toShow, duration - FcConstants.DEFAULT_FADE_ANIM_DURATION), fadeInAnimator);
			animator.addListener(new FcAnimatorListenerAnonymousInnerClassHelper4(this, toShow));

			return animator;
		}

		private class FcAnimatorListenerAnonymousInnerClassHelper4 : FcAnimatorListener
		{
			private readonly FcAnimator outerInstance;

			private View toShow;

			public FcAnimatorListenerAnonymousInnerClassHelper4(FcAnimator outerInstance, View toShow)
			{
				this.outerInstance = outerInstance;
				this.toShow = toShow;
			}

			public override void onAnimationStart(Animator animation)
			{
				toShow.Visibility = View.VISIBLE;
			}
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="toHide"> </param>
		/// <param name="duration">
		/// @return </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public android.animation.Animator createCollapseActionsAnimator(final android.view.View toHide, long duration)
		public virtual Animator createCollapseActionsAnimator(View toHide, long duration)
		{

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final float baseAlpha = toHide.getAlpha();
			float baseAlpha = toHide.Alpha;
			Animator fadeOutAnimator = ObjectAnimator.ofFloat(toHide, View.ALPHA, baseAlpha, 0.0f);
			fadeOutAnimator.Duration = FcConstants.DEFAULT_FADE_ANIM_DURATION;

			AnimatorSet animator = new AnimatorSet();
			animator.playSequentially(fadeOutAnimator, createCollapseAnimator(toHide, duration - FcConstants.DEFAULT_FADE_ANIM_DURATION));

			return animator;
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="view"> </param>
		/// <param name="duration"> </param>
		/// <param name="dx"> </param>
		/// <param name="dy">
		/// @return </param>
		public virtual Animator createScaleAnimator(View view, long duration, float dx, float dy)
		{
			Animator scaleX = ObjectAnimator.ofFloat(view, View.SCALE_X, 1.0f, 1.0f - dx);
			Animator scaleY = ObjectAnimator.ofFloat(view, View.SCALE_Y, 1.0f, 1.0f - dy);
			scaleX.Interpolator = new CycleInterpolator(1.0f);
			scaleY.Interpolator = new CycleInterpolator(1.0f);

			AnimatorSet animator = new AnimatorSet();
			animator.Duration = duration;
			animator.playTogether(scaleX, scaleY);

			return animator;
		}

		/// <summary>
		/// @brief Empty class implementing <seealso cref="android.animation.Animator.AnimatorListener"/> interface
		/// 
		/// Class used to reduce number of methods required to implement the interface.
		/// </summary>
		public class FcAnimatorListener : Animator.AnimatorListener
		{

			public override void onAnimationStart(Animator animation)
			{
				// NOOP
			}

			public override void onAnimationEnd(Animator animation)
			{
				// NOOP
			}

			public override void onAnimationCancel(Animator animation)
			{
				// NOOP
			}

			public override void onAnimationRepeat(Animator animation)
			{
				// NOOP
			}
		}
	}

}