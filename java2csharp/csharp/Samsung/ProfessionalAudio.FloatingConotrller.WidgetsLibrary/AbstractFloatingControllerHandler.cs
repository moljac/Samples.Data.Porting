using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Activity = android.app.Activity;
	using TypedArray = android.content.res.TypedArray;
	using View = android.view.View;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;


	internal abstract class AbstractFloatingControllerHandler : IFloatingControlerHandler
	{
		public abstract void loadBarState(FloatingController floatingController, int barAlignment);
		public abstract int BarAlignment {get;}
		public abstract void initLayout(android.view.ViewGroup layout);
		public abstract int LayoutResource {get;}

		protected internal ControlBar mControlBar;
		protected internal View mMainView;

		public virtual void setSapaAppService(SapaAppService sapaAppService, string mainPackagename)
		{
			mControlBar.setSapaAppService(sapaAppService, mainPackagename);
		}

		public virtual void stopConnections()
		{
			this.mControlBar.removeListeners();
		}

		public virtual bool BarExpanded
		{
			get
			{
				if (this.mControlBar != null)
				{
					return this.mControlBar.BarExpanded;
				}
				return false;
			}
		}

		public virtual IDictionary<string, bool?> DevicesExpanded
		{
			get
			{
				if (this.mControlBar != null)
				{
					return this.mControlBar.DevicesExpanded;
				}
				return new Dictionary<string, bool?>();
			}
		}

		protected internal abstract int getBorder(float x, float y);

		protected internal virtual void initControlBar(Activity activity, TypedArray array)
		{
			DeviceActionsLayout.BUTTON_HEIGHT = array.getLayoutDimension(R.styleable.FloatingControler_bar_thickness, activity.Resources.getDimensionPixelSize(R.dimen.default_controlbar_thickness));
			DeviceActionsLayout.BUTTON_WIDTH = activity.Resources.getDimensionPixelSize(R.dimen.button_width);
			if (DeviceActionsLayout.BUTTON_WIDTH == 0)
			{
				DeviceActionsLayout.BUTTON_WIDTH = (int) Math.Ceiling(((double)DeviceActionsLayout.BUTTON_HEIGHT * 68) / 56);
			}
			//mBarThickness=array.getLayoutDimension(R.styleable.FloatingControler_bar_thickness, LayoutParams.WRAP_CONTENT);
		}
	}

}