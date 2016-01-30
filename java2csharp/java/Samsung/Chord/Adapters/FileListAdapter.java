/**
 * Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
 *
 * Mobile Communication Division,
 * Digital Media & Communications Business, Samsung Electronics Co., Ltd.
 *
 * This software and its documentation are confidential and proprietary
 * information of Samsung Electronics Co., Ltd.  No part of the software and
 * documents may be copied, reproduced, transmitted, translated, or reduced to
 * any electronic medium or machine-readable form without the prior written
 * consent of Samsung Electronics.
 *
 * Samsung Electronics makes no representations with respect to the contents,
 * and assumes no responsibility for any errors that might appear in the
 * software and documents. This publication and the contents hereof are subject
 * to change without notice.
 */

package com.samsung.android.sdk.chord.example.adapter;

import java.util.ArrayList;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.CompoundButton.OnCheckedChangeListener;
import android.widget.TextView;

import com.samsung.android.sdk.chord.example.R;

public class FileListAdapter extends BaseAdapter {

    public FileListAdapter(Context context) {
        mContext = context;
        mInflater = LayoutInflater.from(mContext);
        mFileList = new ArrayList<String>();
        mCheckedFileList = new ArrayList<String>();
    }

    private class ViewHolder {
        TextView fileList_textView;

        CheckBox file_checkBox;
    }

    public void addFile(String file) {
        mFileList.add(file);
        notifyDataSetChanged();
    }

    public ArrayList<String> getCheckedFileList() {
        return mCheckedFileList;
    }

    @Override
    public int getCount() {
        return mFileList.size();
    }

    @Override
    public Object getItem(int position) {
        if (position > mFileList.size()) {
            return null;
        }
        return mFileList.get(position);
    }

    @Override
    public long getItemId(int arg0) {
        return 0;
    }

    @Override
    public View getView(int position, View convertView, ViewGroup parent) {

        View v = convertView;

        if (v == null) {
            mViewHolder = new ViewHolder();
            v = mInflater.inflate(R.layout.file_list_item, parent, false);

            mViewHolder.fileList_textView = (TextView) v.findViewById(R.id.fileName_textView);
            mViewHolder.file_checkBox = (CheckBox) v.findViewById(R.id.fileName_checkBox);

            v.setTag(mViewHolder);

        } else {
            mViewHolder = (ViewHolder) v.getTag();
        }

        mViewHolder.fileList_textView.setText(mFileList.get(position));

        final String fileName = mFileList.get(position);
        mViewHolder.file_checkBox.setOnCheckedChangeListener(new OnCheckedChangeListener() {

            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                if (isChecked && !mCheckedFileList.contains(fileName)) {
                    mCheckedFileList.add(fileName);
                } else if (!isChecked && mCheckedFileList.contains(fileName)) {
                    mCheckedFileList.remove(fileName);
                }
            }
        });

        mViewHolder.file_checkBox.setChecked(mCheckedFileList.contains(fileName));

        return v;
    }

    private LayoutInflater mInflater = null;

    private Context mContext = null;

    private ArrayList<String> mFileList = null;

    private ArrayList<String> mCheckedFileList = null;

    private ViewHolder mViewHolder = null;

}
