/// <summary>
/// Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
/// 
/// Mobile Communication Division,
/// Digital Media & Communications Business, Samsung Electronics Co., Ltd.
/// 
/// This software and its documentation are confidential and proprietary
/// information of Samsung Electronics Co., Ltd.  No part of the software and
/// documents may be copied, reproduced, transmitted, translated, or reduced to
/// any electronic medium or machine-readable form without the prior written
/// consent of Samsung Electronics.
/// 
/// Samsung Electronics makes no representations with respect to the contents,
/// and assumes no responsibility for any errors that might appear in the
/// software and documents. This publication and the contents hereof are subject
/// to change without notice.
/// </summary>

namespace com.samsung.android.sdk.chord.example
{

	using Context = android.content.Context;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;
	using LinearLayout = android.widget.LinearLayout;
	using ScrollView = android.widget.ScrollView;
	using TextView = android.widget.TextView;

	public class ChordLogView : ScrollView
	{
		private TextView logTextView;

		public ChordLogView(Context context, AttributeSet attr) : base(context, attr)
		{
			logTextView = new TextView(context);
			LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MATCH_PARENT, LinearLayout.LayoutParams.WRAP_CONTENT);
			logTextView.setPadding(6, 0, 3, 0);
			logTextView.LayoutParams = lp;
			addView(logTextView);
		}

		public virtual void appendLog(string log)
		{
			logTextView.append(log + "\n");
			post(() =>
			{
				fullScroll(View.FOCUS_DOWN);
			});
		}
	}

}