package com.samsung.android.sdk.camera.sample.cases.util;

import android.app.Activity;
import android.util.Range;
import android.widget.SeekBar;
import android.widget.TextView;

import com.samsung.android.sdk.camera.sample.R;

/**
 * SettingItem its value is constrained by Range class.
 * @param <V> Type parameter of Range. Only valid for Long, Double, Integer, Float, Short, Byte.
 */
public class RangeSettingItem<K, V extends Number & java.lang.Comparable<? super V>> extends SettingItem<K, V> {

    final private NormalizedRange mNormalizedRange;

    private enum TYPE { TYPE_BYTE, TYPE_DOUBLE, TYPE_FLOAT, TYPE_INTEGER, TYPE_LONG, TYPE_SHORT }

    /**
     * A helper class normalizes range based on given input range.
     * [0 ~ max <= Integer.MAX_VALUE]
     */
    private class NormalizedRange {
        private TYPE mType;
        private Range<V> mRange;
        private int mNormalizedMax;

        @SuppressWarnings("unchecked")
        public NormalizedRange(Range<V> range) {
            // Calculated normalized max value.
            // If original range can fit into 0~Integer.MAX_VALUE then use original. otherwise, range will be 0 ~ Integer.MAX_VALUE.
            mNormalizedMax = Integer.MAX_VALUE;
            mRange = range;

            if(mRange.getLower() instanceof Long) {
                mType = TYPE.TYPE_LONG;
            } else if(mRange.getLower() instanceof Double) {
                mType = TYPE.TYPE_DOUBLE;
            } else if(mRange.getLower() instanceof Integer) {
                mType = TYPE.TYPE_INTEGER;
                // SeekBar only take 0~Integer.MAX_VALUE.
                if(mRange.getUpper().longValue() - mRange.getLower().longValue() <= Integer.MAX_VALUE) {
                    mNormalizedMax = mRange.getUpper().intValue() - mRange.getLower().intValue();
                }
            } else if(mRange.getLower() instanceof Float) {
                mType = TYPE.TYPE_FLOAT;
            } else if(mRange.getLower() instanceof Short) {
                mNormalizedMax = mRange.getUpper().shortValue() - mRange.getLower().shortValue();
                mType = TYPE.TYPE_SHORT;
            } else if(mRange.getLower() instanceof Byte) {
                mNormalizedMax = mRange.getUpper().byteValue() - mRange.getLower().byteValue();
                mType = TYPE.TYPE_BYTE;
            } else {
                throw new IllegalArgumentException("Unsupported Type.");
            }
        }

        /**
         * Returns max of normalized range
         */
        int getNormalizedMax() { return mNormalizedMax; }

        /**
         * normalize the input value
         */
        int toNormalizedValue(V value) {
            return (int)((value.doubleValue() - mRange.getLower().doubleValue()) / (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue()) * mNormalizedMax);
        }

        /**
         * Returns original value from normalized input.
         */
        @SuppressWarnings("unchecked")
        V fromNormalizedValue(int value) {
            switch(mType) {
                case TYPE_LONG:
                    return (V)Long.valueOf((long)(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue())));
                case TYPE_DOUBLE:
                    return (V)Double.valueOf(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue()));
                case TYPE_INTEGER:
                    return (V)Integer.valueOf((int)(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue())));
                case TYPE_FLOAT:
                    return (V)Float.valueOf((float)(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue())));
                case TYPE_SHORT:
                    return (V)Short.valueOf((short)(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue())));
                case TYPE_BYTE:
                default:
                    return (V)Byte.valueOf((byte)(mRange.getLower().doubleValue() + ((double)value / mNormalizedMax) * (mRange.getUpper().doubleValue() - mRange.getLower().doubleValue())));
            }
        }
    }

    /**
     * Contrsutor for this RangeSettingItem
     * @param name String representation of this item.
     * @param key Key of this item.
     * @param range Valid value range for this item.
     */
    RangeSettingItem(Activity activity, String name, K key, Range<V> range) {
        super(activity, name, key);

        mView = mActivity.getLayoutInflater().inflate(R.layout.setting_range_item, null);
        mNormalizedRange = new NormalizedRange(range);

        ((TextView)mView.findViewById(R.id.title)).setText(name);

        SeekBar seekBar = (SeekBar) mView.findViewById(R.id.seekbar);
        seekBar.setMax(mNormalizedRange.getNormalizedMax());

        seekBar.setProgress(seekBar.getMax() / 2);
        seekBar.setOnSeekBarChangeListener(new SeekBar.OnSeekBarChangeListener() {
            @Override
            public void onProgressChanged(SeekBar seekBar, int progress, boolean fromUser) {
                if(mListener != null) mListener.onSettingItemValueChanged(mKey, mNormalizedRange.fromNormalizedValue(progress));
            }

            @Override
            public void onStartTrackingTouch(SeekBar seekBar) { }

            @Override
            public void onStopTrackingTouch(SeekBar seekBar) { }
        });
    }

    @Override
    public V getValue() {
        return mNormalizedRange.fromNormalizedValue(((SeekBar) mView.findViewById(R.id.seekbar)).getProgress());
    }

    @Override
    public void setValue(V value) {
        ((SeekBar) mView.findViewById(R.id.seekbar)).setProgress(mNormalizedRange.toNormalizedValue(value));
    }

    @Override
    public void setEnabled(boolean enable) {
        mView.findViewById(R.id.seekbar).setEnabled(enable);
        if(enable && mListener != null) mListener.onSettingItemValueChanged(mKey, mNormalizedRange.fromNormalizedValue(((SeekBar) (mView.findViewById(R.id.seekbar))).getProgress()));
    }

    @Override
    public boolean isEnabled() {
        return mView.findViewById(R.id.seekbar).isEnabled();
    }
}