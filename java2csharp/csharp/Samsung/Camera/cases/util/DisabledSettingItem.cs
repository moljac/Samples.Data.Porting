namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Activity = android.app.Activity;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using View = android.view.View;
	using TextView = android.widget.TextView;

	/// <summary>
	/// Dummy Setting Item for setting feature that is not supported by the device.
	/// This item only show the gary outed text label.
	/// </summary>
	public class DisabledSettingItem : SettingItem<object, object>
	{
		public DisabledSettingItem(Activity activity, string name) : base(activity, name, null)
		{
			mView = mActivity.LayoutInflater.inflate(R.layout.setting_list_item, null);
			((TextView)mView.findViewById(R.id.title)).Text = name;
			((TextView)mView.findViewById(R.id.title)).TextColor = ContextCompat.getColor(mActivity, android.R.color.secondary_text_dark);
			mView.findViewById(R.id.itemlist).Visibility = View.GONE;
		}

		public override object Value
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		public override bool Enabled
		{
			set
			{
			}
			get
			{
				return false;
			}
		}
	}

}