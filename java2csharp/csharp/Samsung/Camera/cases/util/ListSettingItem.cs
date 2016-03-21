using System.Collections.Generic;

namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Activity = android.app.Activity;
	using ContextCompat = android.support.v4.content.ContextCompat;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Spinner = android.widget.Spinner;
	using TextView = android.widget.TextView;


	/// <summary>
	/// SettingItem its value is constrained by List class.
	/// </summary>
	public class ListSettingItem<K, V> : SettingItem<K, V>
	{
		/// <summary>
		/// A list of value can be used with SCaptureRequest.Key
		/// </summary>
		private readonly IList<V> mValueList;
		/// <summary>
		/// A list of value that is supported,
		/// </summary>
		private readonly IList<V> mAvailableValueList;

		/// <summary>
		/// Constructor for ListSettingItem
		/// </summary>
		/// <param name="name"> String representation of this item. </param>
		/// <param name="key"> Key of this item </param>
		/// <param name="valueNameList"> String representation of value list </param>
		/// <param name="valueList"> value list for this item </param>
		/// <param name="availableValueList"> value list that is available for this device </param>
		internal ListSettingItem(Activity activity, string name, K key, IList<string> valueNameList, IList<V> valueList, IList<V> availableValueList) : base(activity, name, key)
		{

			mValueList = valueList;
			mAvailableValueList = availableValueList;

			mView = mActivity.LayoutInflater.inflate(R.layout.setting_list_item, null);

			((TextView)mView.findViewById(R.id.title)).Text = name;

			Spinner spinner = (Spinner) mView.findViewById(R.id.itemlist);
			spinner.Adapter = new ArrayAdapterAnonymousInnerClassHelper(this, mActivity, R.layout.spinner_item, valueNameList);

			for (int index = 0; index < mValueList.Count; index++)
			{
				if (mAvailableValueList.Contains(mValueList[index]))
				{
					spinner.Selection = index;
					break;
				}
			}

			spinner.OnItemSelectedListener = new OnItemSelectedListenerAnonymousInnerClassHelper(this);
		}

		private class ArrayAdapterAnonymousInnerClassHelper : ArrayAdapter<string>
		{
			private readonly ListSettingItem<K, V> outerInstance;

			public ArrayAdapterAnonymousInnerClassHelper(ListSettingItem<K, V> outerInstance, Activity mActivity, UnknownType spinner_item, IList<string> valueNameList) : base(mActivity, spinner_item, valueNameList)
			{
				this.outerInstance = outerInstance;
			}

			public override bool isEnabled(int position)
			{
				return outerInstance.mAvailableValueList.Contains(outerInstance.mValueList[position]);
			}

			public override View getDropDownView(int position, View convertView, ViewGroup parent)
			{
				TextView view = (TextView)base.getDropDownView(position, convertView, parent);
				if (isEnabled(position))
				{
					view.TextColor = ContextCompat.getColor(outerInstance.mActivity, android.R.color.primary_text_light);
				}
				else
				{
					view.TextColor = ContextCompat.getColor(outerInstance.mActivity, android.R.color.secondary_text_dark);
				}

				return view;
			}
		}

		private class OnItemSelectedListenerAnonymousInnerClassHelper : AdapterView.OnItemSelectedListener
		{
			private readonly ListSettingItem<K, V> outerInstance;

			public OnItemSelectedListenerAnonymousInnerClassHelper(ListSettingItem<K, V> outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemSelected<T1>(AdapterView<T1> parent, View view, int position, long id)
			{
				if (outerInstance.mListener != null)
				{
					outerInstance.mListener.onSettingItemValueChanged(outerInstance.mKey, outerInstance.mValueList[position]);
				}
			}

			public override void onNothingSelected<T1>(AdapterView<T1> parent)
			{
			}
		}

		public override V Value
		{
			get
			{
				return mValueList[((Spinner) mView.findViewById(R.id.itemlist)).SelectedItemPosition];
			}
			set
			{
				if (mAvailableValueList.Contains(value))
				{
					((Spinner) mView.findViewById(R.id.itemlist)).Selection = mValueList.IndexOf(value);
				}
			}
		}


		public override bool Enabled
		{
			set
			{
				mView.findViewById(R.id.itemlist).Enabled = value;
				if (value && mListener != null)
				{
					mListener.onSettingItemValueChanged(mKey, mValueList[((Spinner) mView.findViewById(R.id.itemlist)).SelectedItemPosition]);
				}
			}
			get
			{
				return mView.findViewById(R.id.itemlist).Enabled;
			}
		}

	}

}