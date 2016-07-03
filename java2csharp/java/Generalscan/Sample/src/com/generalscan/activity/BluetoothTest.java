package com.generalscan.activity;


public class BluetoothTest {

	private final String TestString = "Scanner BT Check   ";
	private final String AllEndString = "Check Done! ";
	private final String EndString = "\r\n";

	private String mTestString = "";
	private String mAllEndString = "";

	private int StringIndex = 0;
	private int Count = 0;

	private int mAllCount = 200;

	public BluetoothTest() {

		init();
	}

	public void test(String data) {

		if (Count < mAllCount) {
			//判断是否小于长度，不要超出范围
			if (StringIndex <= mTestString.length()) {
				// 判断字符是否一样
				if (!data.equals(String.valueOf(mTestString.charAt(StringIndex)))) {
					// 不一样
					if (mOnTestListener != null) {
						mOnTestListener.Test(Count, StringIndex);
					}
				}
				StringIndex++;
				if (data.equals("\n")) {

					StringIndex = 0;
					Count++;
					if (Count < 10) {
						mTestString = TestString + Count + EndString;
					}

					if (Count >= 10 && Count < 100) {
						mTestString = TestString.substring(0,
								TestString.length() - 1)
								+ Count + EndString;
					}
					if (Count >= 100 && Count < 1000) {
						mTestString = TestString.substring(0,
								TestString.length() - 2)
								+ Count + EndString;
					}
				}
			}
		} else if (Count == mAllCount) {
			//判断是否小于长度，不要超出范围
			if (StringIndex <= mAllEndString.length()) {
			// 判断字符是否一样
			if (!data.equals(String.valueOf(mAllEndString.charAt(StringIndex)))) {
				// 不一样
				if (mOnTestListener != null) {
					mOnTestListener.Test(Count, StringIndex);
				}
			} 
			StringIndex++;
			if (data.equals("\n")) {
				Count++;

			}
			}
			
		} else if (Count == mAllCount + 1) {
			if (data.equals("\n")) {

				init();

			}
		}

	}

	private void init() {
		Count = 0;
		StringIndex = 0;
		mTestString = TestString + Count + EndString;
		mAllEndString = AllEndString + mAllCount + EndString + EndString;
	}

	// public void GetTestResult() {
	// mAllString.length();
	// String mAllLine[] = mAllString.toString().split(mEndString);
	// int lineCount = 0;
	// for (String lineString : mAllLine) {
	// String testString = "";
	// // 最后一个字符
	// if (lineCount == 200) {
	// testString = mAllEndString + 200;
	//
	// } else {
	// testString = mTestString + lineCount;
	//
	// }
	// for (int x = 0; x < lineString.length(); x++) {
	// // 字符判断
	// if (lineString.charAt(x) == testString.charAt(x)) {
	//
	// } else {
	// if (mOnTestListener != null) {
	// mOnTestListener.Test(lineCount, x);
	// }
	// }
	//
	// }
	// lineCount++;
	// }
	//
	// }

	public interface OnTestListener {
		public void Test(int lineCount, int cellCount);
	};

	private OnTestListener mOnTestListener;

	public void setOnTestListener(OnTestListener mOnTestListener) {
		this.mOnTestListener = mOnTestListener;
	}

}
