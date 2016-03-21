namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using View = android.view.View;
	using Animation = android.view.animation.Animation;
	using Transformation = android.view.animation.Transformation;
	using LayoutParams = android.widget.LinearLayout.LayoutParams;

	internal abstract class ResizeAnimation : Animation
	{

		protected internal View mView;
		protected internal float mToHeight;
		protected internal float mFromHeight;
		protected internal float mToWidth;
		protected internal float mFromWidth;

		public ResizeAnimation(View v, float fromWidth, float toWidth, float fromHeight, float toHeight, int duration)
		{
			mToHeight = toHeight;
			mToWidth = toWidth;
			mFromHeight = fromHeight;
			mFromWidth = fromWidth;
			mView = v;
			this.Duration = duration;
		}

		protected internal override void applyTransformation(float interpolatedTime, Transformation t)
		{
			float height = (mToHeight - mFromHeight) * interpolatedTime + mFromHeight;
			float width = (mToWidth - mFromWidth) * interpolatedTime + mFromWidth;
			LayoutParams p = (LayoutParams) mView.LayoutParams;
			p.height = (int) height;
			p.width = (int) width;
			this.endCondition(width, height);
			if (width == 0 && this.mToWidth == 0)
			{
				p.height = 0;
			}
			if (height == 0 && this.mToHeight == 0)
			{
				p.width = 0;
			}
			mView.requestLayout();
			mView.invalidate();
		}

		public abstract void endCondition(float width, float height);

	}

}