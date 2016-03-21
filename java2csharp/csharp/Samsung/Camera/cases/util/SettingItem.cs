namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Activity = android.app.Activity;
	using View = android.view.View;

	/// <summary>
	/// Abstract Class for SettingItem
	/// </summary>
	public abstract class SettingItem<K, V>
	{
		public const int SETTING_TYPE_REQUEST_KEY = -1;
		public const int SETTING_TYPE_CAMERA_FACING = 0;
		public const int SETTING_TYPE_IMAGE_FORMAT = 1;

		protected internal readonly Activity mActivity;
		protected internal readonly string mName;
		protected internal readonly K mKey;

		protected internal OnSettingItemValueChangedListener mListener;

		protected internal View mView;

		public SettingItem(Activity activity, string name, K key)
		{
			mActivity = activity;
			mName = name;
			mKey = key;
		}

		/// <summary>
		/// Returns current setting value for this setting item.
		/// </summary>
		public abstract V Value {get;set;}


		/// <summary>
		/// Set initial setting value for this setting item.
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public void setInitialValue(Object value)
		public virtual object InitialValue
		{
			set
			{
				if (value != null)
				{
					Value = (V)value;
				}
				if (mListener != null)
				{
					mListener.onSettingItemValueChanged(mKey, Value);
				}
			}
		}

		/// <summary>
		/// Returns key for this setting item.
		/// </summary>
		public virtual K Key
		{
			get
			{
				return mKey;
			}
		}

		/// <summary>
		/// Returns view for this setting item.
		/// </summary>
		public virtual View View
		{
			get
			{
				return mView;
			}
		}

		/// <summary>
		/// Enables/Disables this setting item.
		/// </summary>
		public abstract bool Enabled {set;get;}


		/// <summary>
		/// Set <seealso cref="SettingItem.OnSettingItemValueChangedListener"/> for this setting item.
		/// </summary>
		public virtual void setOnSettingItemValueChangedListener(OnSettingItemValueChangedListener listener)
		{
			mListener = listener;
		}

		public interface OnSettingItemValueChangedListener
		{
			void onSettingItemValueChanged<K_Listener, V_Listener>(K_Listener key, V_Listener value);
		}
	}

}