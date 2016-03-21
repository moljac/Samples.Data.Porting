using System;

namespace com.samsung.android.sdk.camera.sample.cases.util
{

	using Activity = android.app.Activity;
	using Range = android.util.Range;
	using SeekBar = android.widget.SeekBar;
	using TextView = android.widget.TextView;

	/// <summary>
	/// SettingItem its value is constrained by Range class. </summary>
	/// @param <V> Type parameter of Range. Only valid for Long, Double, Integer, Float, Short, Byte. </param>
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: public class RangeSettingItem<K, V extends Number & java.lang.Comparable<? super V>> extends SettingItem<K, V>
	public class RangeSettingItem<K, V> : SettingItem<K, V> where V : Number, java.lang.Comparable<? base V>
	{

		private readonly NormalizedRange mNormalizedRange;

		private enum TYPE
		{
			TYPE_BYTE,
			TYPE_DOUBLE,
			TYPE_FLOAT,
			TYPE_INTEGER,
			TYPE_LONG,
			TYPE_SHORT
		}

		/// <summary>
		/// A helper class normalizes range based on given input range.
		/// [0 ~ max <= Integer.MAX_VALUE]
		/// </summary>
		private class NormalizedRange
		{
			private readonly RangeSettingItem<K, V> outerInstance;

			internal TYPE mType;
			internal Range<V> mRange;
			internal int mNormalizedMax;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public NormalizedRange(android.util.Range<V> range)
			public NormalizedRange(RangeSettingItem<K, V> outerInstance, Range<V> range)
			{
				this.outerInstance = outerInstance;
				// Calculated normalized max value.
				// If original range can fit into 0~Integer.MAX_VALUE then use original. otherwise, range will be 0 ~ Integer.MAX_VALUE.
				mNormalizedMax = int.MaxValue;
				mRange = range;

				if (mRange.Lower is long?)
				{
					mType = TYPE.TYPE_LONG;
				}
				else if (mRange.Lower is double?)
				{
					mType = TYPE.TYPE_DOUBLE;
				}
				else if (mRange.Lower is int?)
				{
					mType = TYPE.TYPE_INTEGER;
					// SeekBar only take 0~Integer.MAX_VALUE.
					if (mRange.Upper.longValue() - mRange.Lower.longValue() <= int.MaxValue)
					{
						mNormalizedMax = mRange.Upper.intValue() - mRange.Lower.intValue();
					}
				}
				else if (mRange.Lower is float?)
				{
					mType = TYPE.TYPE_FLOAT;
				}
				else if (mRange.Lower is short?)
				{
					mNormalizedMax = mRange.Upper.shortValue() - mRange.Lower.shortValue();
					mType = TYPE.TYPE_SHORT;
				}
				else if (mRange.Lower is sbyte?)
				{
					mNormalizedMax = mRange.Upper.byteValue() - mRange.Lower.byteValue();
					mType = TYPE.TYPE_BYTE;
				}
				else
				{
					throw new System.ArgumentException("Unsupported Type.");
				}
			}

			/// <summary>
			/// Returns max of normalized range
			/// </summary>
			internal virtual int NormalizedMax
			{
				get
				{
					return mNormalizedMax;
				}
			}

			/// <summary>
			/// normalize the input value
			/// </summary>
			internal virtual int toNormalizedValue(V value)
			{
				return (int)((value.doubleValue() - mRange.Lower.doubleValue()) / (mRange.Upper.doubleValue() - mRange.Lower.doubleValue()) * mNormalizedMax);
			}

			/// <summary>
			/// Returns original value from normalized input.
			/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") V fromNormalizedValue(int value)
			internal virtual V fromNormalizedValue(int value)
			{
				switch (mType)
				{
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_LONG:
						return (V)Convert.ToInt64((long)(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue())));
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_DOUBLE:
						return (V)Convert.ToDouble(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue()));
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_INTEGER:
						return (V)Convert.ToInt32((int)(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue())));
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_FLOAT:
						return (V)Convert.ToSingle((float)(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue())));
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_SHORT:
						return (V)Convert.ToInt16((short)(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue())));
					case com.samsung.android.sdk.camera.sample.cases.util.RangeSettingItem.TYPE.TYPE_BYTE:
					default:
						return (V)Convert.ToSByte((sbyte)(mRange.Lower.doubleValue() + ((double)value / mNormalizedMax) * (mRange.Upper.doubleValue() - mRange.Lower.doubleValue())));
				}
			}
		}

		/// <summary>
		/// Contrsutor for this RangeSettingItem </summary>
		/// <param name="name"> String representation of this item. </param>
		/// <param name="key"> Key of this item. </param>
		/// <param name="range"> Valid value range for this item. </param>
		internal RangeSettingItem(Activity activity, string name, K key, Range<V> range) : base(activity, name, key)
		{

			mView = mActivity.LayoutInflater.inflate(R.layout.setting_range_item, null);
			mNormalizedRange = new NormalizedRange(this, this, this, range);

			((TextView)mView.findViewById(R.id.title)).Text = name;

			SeekBar seekBar = (SeekBar) mView.findViewById(R.id.seekbar);
			seekBar.Max = mNormalizedRange.NormalizedMax;

			seekBar.Progress = seekBar.Max / 2;
			seekBar.OnSeekBarChangeListener = new OnSeekBarChangeListenerAnonymousInnerClassHelper(this, seekBar);
		}

		private class OnSeekBarChangeListenerAnonymousInnerClassHelper : SeekBar.OnSeekBarChangeListener
		{
			private readonly RangeSettingItem<K, V> outerInstance;

			private SeekBar seekBar;

			public OnSeekBarChangeListenerAnonymousInnerClassHelper(RangeSettingItem<K, V> outerInstance, SeekBar seekBar)
			{
				this.outerInstance = outerInstance;
				this.seekBar = seekBar;
			}

			public override void onProgressChanged(SeekBar seekBar, int progress, bool fromUser)
			{
				if (mListener != null)
				{
					mListener.onSettingItemValueChanged(mKey, outerInstance.mNormalizedRange.fromNormalizedValue(progress));
				}
			}

			public override void onStartTrackingTouch(SeekBar seekBar)
			{
			}
			public override void onStopTrackingTouch(SeekBar seekBar)
			{
			}
		}

		public override V Value
		{
			get
			{
				return mNormalizedRange.fromNormalizedValue(((SeekBar) mView.findViewById(R.id.seekbar)).Progress);
			}
			set
			{
				((SeekBar) mView.findViewById(R.id.seekbar)).Progress = mNormalizedRange.toNormalizedValue(value);
			}
		}


		public override bool Enabled
		{
			set
			{
				mView.findViewById(R.id.seekbar).Enabled = value;
				if (value && mListener != null)
				{
					mListener.onSettingItemValueChanged(mKey, mNormalizedRange.fromNormalizedValue(((SeekBar)(mView.findViewById(R.id.seekbar))).Progress));
				}
			}
			get
			{
				return mView.findViewById(R.id.seekbar).Enabled;
			}
		}

	}
}