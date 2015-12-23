using System;
using System.Collections.Generic;

namespace com.bixolon.printersample
{


	using ListActivity = android.app.ListActivity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using View = android.view.View;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;

	public class FileExplorerActivity : ListActivity
	{
		private ArrayAdapter<string> mAdapter;
		private List<string> mArrayList = new List<string>();

		private TextView mTextView;
		private string mCurrentPath;

		private const string PARENT_DIRECTORY_PATH = "..";
		private const string DIRECTORY_DELIMITER = "/";

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_file_explorer;

			mCurrentPath = Environment.ExternalStorageDirectory.Path + DIRECTORY_DELIMITER;
			mTextView = (TextView) findViewById(R.id.textView1);
			mTextView.Text = mCurrentPath;

			mAdapter = new ArrayAdapter<string>(this, android.R.layout.simple_list_item_1, mArrayList);
			ListAdapter = mAdapter;

			createDirectoryList(new File(mCurrentPath));
		}

		public override void onListItemClick(ListView l, View v, int position, long id)
		{
			string path = mCurrentPath + mArrayList[position];
			File file = new File(path);

			if (file.Directory)
			{
				if (path.IndexOf(PARENT_DIRECTORY_PATH, StringComparison.Ordinal) > 0)
				{
					mCurrentPath = file.ParentFile.Parent + DIRECTORY_DELIMITER;
				}
				else
				{
					mCurrentPath = path;
				}
				createDirectoryList(new File(mCurrentPath));

				mTextView.Text = mCurrentPath;
				mAdapter.notifyDataSetChanged();
			}
			else
			{
				if (!path.EndsWith(".fls", StringComparison.Ordinal) && !path.EndsWith(".bin", StringComparison.Ordinal))
				{
					Toast.makeText(this, "Must choose .fls or .bin file", Toast.LENGTH_SHORT).show();
				}
				else
				{
					Intent data = new Intent();
					data.putExtra(MainActivity.FIRMWARE_FILE_NAME, path);
					setResult(MainActivity.RESULT_CODE_SELECT_FIRMWARE, data);
					finish();
				}
			}
		}

		private void createDirectoryList(File file)
		{
			mArrayList.Clear();
			mArrayList.Add(PARENT_DIRECTORY_PATH);
			File[] files = file.listFiles();
			if (files != null)
			{
				Arrays.sort(files);

				for (int i = 0; i < files.Length; i++)
				{
					if (files[i].Directory)
					{
						mArrayList.Add(files[i].Name + DIRECTORY_DELIMITER);
					}
					else
					{
						mArrayList.Add(files[i].Name);
					}
				}
			}
		}
	}

}