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

namespace com.samsung.android.sdk.chord.example
{


	using FileListAdapter = com.samsung.android.sdk.chord.example.adapter.FileListAdapter;

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using DialogInterface = android.content.DialogInterface;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using AdapterView = android.widget.AdapterView;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;

	public class FileSelectActivity : Activity, AdapterView.OnItemClickListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.fileselect;

			mSelect_btn = (Button) findViewById(R.id.select_btn);
			mPwd_textView = (TextView) findViewById(R.id.pwd_textView);

			mFileListView = (ListView) findViewById(R.id.filelist);
			mFileListView.OnItemClickListener = this;

			returnIntent = new Intent();

			mSelect_btn.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			getDir(homeDir);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly FileSelectActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(FileSelectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.returnFilePath();
			}
		}

		public override void onItemClick<T1>(AdapterView<T1> parent, View v, int position, long id)
		{
			File file = new File(filePathList[position]);

			if (file.Directory)
			{
				if (file.canRead())
				{
					getDir(filePathList[position]);
				}
				else
				{
					(new AlertDialog.Builder(this)).setIcon(android.R.drawable.ic_lock_lock).setTitleuniquetempvar.setPositiveButton("OK", new OnClickListenerAnonymousInnerClassHelper(this))
						   .show();
				}
			}
			else
			{
				Log.v(TAG, TAGClass + "fileSelected @@ file : " + file.AbsolutePath);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly FileSelectActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(FileSelectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialog, int which)
			{
			}
		}

		private void getDir(string dir)
		{
			mPwd_textView.Text = dir;

			filePathList = new List<string>();
			mFilelistAdapter = new FileListAdapter(this);

			File file = new File(dir);
			File[] files = file.listFiles();

			if (!dir.Equals(homeDir))
			{
				filePathList.Add(homeDir);
				filePathList.Add(file.Parent);

				mFilelistAdapter.addFile(homeDir);
				mFilelistAdapter.addFile("../");
			}

			for (int i = 0; i < files.Length; i++)
			{
				File mfile = files[i];
				filePathList.Add(mfile.Path);

				if (mfile.Directory)
				{
					mFilelistAdapter.addFile(mfile.Path + "/");
				}
				else
				{
					mFilelistAdapter.addFile(mfile.Name);
				}
			}
			mFileListView.Adapter = mFilelistAdapter;
		}

		private void returnFilePath()
		{
			List<string> selectfiles = new List<string>();
			List<string> checkedFileList = mFilelistAdapter.CheckedFileList;

			for (int i = 0; i < checkedFileList.Count; i++)
			{
				string mFilePath = mPwd_textView.Text + "/" + checkedFileList[i];
				selectfiles.Add(mFilePath);
			}
			returnIntent.putExtra("SELECTED_FILE", selectfiles);
			setResult(RESULT_OK, returnIntent);

			finish();
		}

		private readonly string TAG = "[Chord][Sample]";

		private readonly string TAGClass = "FileSelectActivity : ";

		private Button mSelect_btn;

		private TextView mPwd_textView;

		private ListView mFileListView;

		private FileListAdapter mFilelistAdapter = null;

		private IList<string> filePathList = null;

		private string homeDir = Environment.ExternalStorageDirectory.AbsolutePath;

		private Intent returnIntent;
	}

}