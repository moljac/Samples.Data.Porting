namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using View = android.view.View;
	using LayoutParams = android.widget.LinearLayout.LayoutParams;

	internal class HorizontalResizeAnimation : ResizeAnimation
	{

		public HorizontalResizeAnimation(View v, float fromWidth, float toWidth, float height, int duration) : base(v, fromWidth, toWidth, height, height, duration)
		{
		}

		public override void endCondition(float width, float height)
		{
			if (width == this.mToWidth && this.mToWidth > this.mFromWidth)
			{
				this.mView.LayoutParams = new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT);
			}
		}
	}

}