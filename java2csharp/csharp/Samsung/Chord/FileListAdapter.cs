using System.Collections.Generic;

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

namespace com.samsung.android.sdk.chord.example.adapter
{

	using Context = android.content.Context;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using CheckBox = android.widget.CheckBox;
	using CompoundButton = android.widget.CompoundButton;
	using OnCheckedChangeListener = android.widget.CompoundButton.OnCheckedChangeListener;
	using TextView = android.widget.TextView;

	public class FileListAdapter : BaseAdapter
	{

		public FileListAdapter(Context context)
		{
			mContext = context;
			mInflater = LayoutInflater.from(mContext);
			mFileList = new List<string>();
			mCheckedFileList = new List<string>();
		}

		private class ViewHolder
		{
			private readonly FileListAdapter outerInstance;

			public ViewHolder(FileListAdapter outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			internal TextView fileList_textView;

			internal CheckBox file_checkBox;
		}

		public virtual void addFile(string file)
		{
			mFileList.Add(file);
			notifyDataSetChanged();
		}

		public virtual List<string> CheckedFileList
		{
			get
			{
				return mCheckedFileList;
			}
		}

		public override int Count
		{
			get
			{
				return mFileList.Count;
			}
		}

		public override object getItem(int position)
		{
			if (position > mFileList.Count)
			{
				return null;
			}
			return mFileList[position];
		}

		public override long getItemId(int arg0)
		{
			return 0;
		}

		public override View getView(int position, View convertView, ViewGroup parent)
		{

			View v = convertView;

			if (v == null)
			{
				mViewHolder = new ViewHolder(this, this);
				v = mInflater.inflate(R.layout.file_list_item, parent, false);

				mViewHolder.fileList_textView = (TextView) v.findViewById(R.id.fileName_textView);
				mViewHolder.file_checkBox = (CheckBox) v.findViewById(R.id.fileName_checkBox);

				v.Tag = mViewHolder;

			}
			else
			{
				mViewHolder = (ViewHolder) v.Tag;
			}

			mViewHolder.fileList_textView.Text = mFileList[position];

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String fileName = mFileList.get(position);
			string fileName = mFileList[position];
			mViewHolder.file_checkBox.OnCheckedChangeListener = new OnCheckedChangeListenerAnonymousInnerClassHelper(this, fileName);

			mViewHolder.file_checkBox.Checked = mCheckedFileList.Contains(fileName);

			return v;
		}

		private class OnCheckedChangeListenerAnonymousInnerClassHelper : CompoundButton.OnCheckedChangeListener
		{
			private readonly FileListAdapter outerInstance;

			private string fileName;

			public OnCheckedChangeListenerAnonymousInnerClassHelper(FileListAdapter outerInstance, string fileName)
			{
				this.outerInstance = outerInstance;
				this.fileName = fileName;
			}


			public override void onCheckedChanged(CompoundButton buttonView, bool isChecked)
			{
				if (isChecked && !outerInstance.mCheckedFileList.Contains(fileName))
				{
					outerInstance.mCheckedFileList.Add(fileName);
				}
				else if (!isChecked && outerInstance.mCheckedFileList.Contains(fileName))
				{
					outerInstance.mCheckedFileList.Remove(fileName);
				}
			}
		}

		private LayoutInflater mInflater = null;

		private Context mContext = null;

		private List<string> mFileList = null;

		private List<string> mCheckedFileList = null;

		private ViewHolder mViewHolder = null;

	}

}