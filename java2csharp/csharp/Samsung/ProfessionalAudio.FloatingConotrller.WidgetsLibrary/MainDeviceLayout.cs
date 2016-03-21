namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using Resources = android.content.res.Resources;
	using Bitmap = android.graphics.Bitmap;
	using BitmapDrawable = android.graphics.drawable.BitmapDrawable;
	using Drawable = android.graphics.drawable.Drawable;
	using LayoutInflater = android.view.LayoutInflater;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;

	internal class MainDeviceLayout : DeviceActionsLayout
	{

		private int perpendicular;
		private int along;

		protected internal MainDeviceLayout(Context context, DeviceActionsLayoutData data, SapaAppService sapaAppService, bool isExpanded, int orientation, ControlBar bar) : base(context, data, sapaAppService, isExpanded, orientation, bar)
		{
		}

		protected internal override ImageButton createDeviceButton()
		{
			ImageButton button = (ImageButton)LayoutInflater.from(Context).inflate(R.layout.main_app_view, this,false);
			Drawable drawable = this.mData.mInstanceIcon;
			if (drawable.IntrinsicHeight > mDeviceIconMaxSize)
			{
				drawable.setBounds(0,0,mDeviceIconMaxSize,mDeviceIconMaxSize);
			}
			button.ImageDrawable = drawable;
			button.Clickable = false;
			button.LongClickable = true;

			Resources res = Resources;
			perpendicular = res.getDimensionPixelSize(R.dimen.default_controlbar_widget);
			along = res.getDimensionPixelSize(R.dimen.main_app_btn_length);
			return button;
		}

		protected internal override LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation)
		{
			return orientation == LinearLayout.HORIZONTAL ? new LinearLayout.LayoutParams(along, perpendicular) : new LinearLayout.LayoutParams(perpendicular, along);
		}
	}

}