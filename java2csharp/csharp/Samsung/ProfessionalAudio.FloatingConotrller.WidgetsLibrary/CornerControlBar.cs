using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;

	internal class CornerControlBar : ControlBar
	{

		private const string TAG = "professionalaudioconnection:widget:j:CornerControlBar";

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: protected CornerControlBar(android.content.Context activity, Info info) throws NotActivityException
		protected internal CornerControlBar(Context activity, Info info) : base(activity, info, R.layout.corner_controlbar)
		{
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public CornerControlBar(android.content.Context context, android.util.AttributeSet attrs) throws NotActivityException
		public CornerControlBar(Context context, AttributeSet attrs) : base(context, attrs)
		{
		}

		protected internal override void addMainDevLayout()
		{
			Log.d(TAG, "mainLayout add");
			if (mDevicesLayout.Shown)
			{
				ViewGroup mainParent = ((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel));
				if (mainParent.getChildAt(1).Equals(mMainDeviceLayout))
				{
					mMainDeviceLayout.collapse();
					mainParent.removeView(mMainDeviceLayout);
				}

				((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel)).addView(CornerControlBar.this.mMainDeviceLayout, 1);
			}
		}

		protected internal override ViewGroup MainDevParent
		{
			get
			{
				return ((ViewGroup) CornerControlBar.this.findViewById(R.id.total_panel));
			}
		}

		internal override void addSubViews<T1>(IList<T1> subViews, View view) where T1 : android.view.View
		{
			int index = mDevicesLayout.indexOfChild(view);
			for (int i = 0, size = subViews.Count; i < size; ++i)
			{
				View v = subViews[calculateArrayIndex(i, size)];
				if (mDevicesLayout.indexOfChild(v) != -1)
				{
					mDevicesLayout.removeView(v);
				}
				//v.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH, DeviceActionsLayout.BUTTON_HEIGHT));
				mDevicesLayout.addView(v, ++index);
			}
		}

		private int calculateArrayIndex(int initialIndex, int arraySize)
		{
			if (mDevicesLayout.LayoutDirection == LAYOUT_DIRECTION_LTR)
			{
				return initialIndex;
			}
			else
			{
				return arraySize-1 - initialIndex;
			}
		}

		internal override void clearSubViews(int count, View view)
		{
			int index = mDevicesLayout.indexOfChild(view);
			mDevicesLayout.removeViews(index + 1, count);
		}

		public override void onServiceDisconnected()
		{

		}

	}

}