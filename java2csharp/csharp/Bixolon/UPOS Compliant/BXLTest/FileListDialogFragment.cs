namespace com.bxl.postest
{


	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using Context = android.content.Context;
	using Bundle = android.os.Bundle;
	using Environment = android.os.Environment;
	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using AdapterView = android.widget.AdapterView;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;
	using BaseAdapter = android.widget.BaseAdapter;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;

	public class FileListDialogFragment : DialogFragment, View.OnClickListener, AdapterView.OnItemClickListener
	{

		private static readonly string TAG = typeof(FileListDialogFragment).Name;

		internal static void showDialog(FragmentManager manager)
		{
			FileListDialogFragment fileListDialogFragment = new FileListDialogFragment();
			fileListDialogFragment.show(manager, TAG);
		}

		internal interface OnClickListener
		{
			void onClick(string text);
		}

		private OnClickListener onClickListener;

		private File[] files;
		private FileAdapter fileAdapter;

		private TextView pathTextView;
		private Button okButton;

		private string filePath;

		public override void onAttach(Activity activity)
		{
			base.onAttach(activity);

			try
			{
				onClickListener = (OnClickListener) activity;
			}
			catch (System.InvalidCastException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				throw new System.InvalidCastException(activity.ToString() + " must implement OnClickListener");
			}
		}

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);

			LayoutInflater inflater = Activity.LayoutInflater;
			View view = inflater.inflate(R.layout.dialog_file_list, (ViewGroup) Activity.findViewById(android.R.id.content).RootView, false);

			ListView listView = (ListView) view.findViewById(R.id.listView1);
			listView.OnItemClickListener = this;
			fileAdapter = new FileAdapter(this, Activity);
			listView.Adapter = fileAdapter;

			pathTextView = (TextView) view.findViewById(R.id.textViewTitle);
			pathTextView.OnClickListener = this;

			okButton = (Button) view.findViewById(R.id.buttonOk);
			okButton.OnClickListener = this;
			view.findViewById(R.id.buttonCancel).OnClickListener = this;

			builder.setView(view).setTitle(getString(R.@string.open));

			updateFileList(new File(Environment.ExternalStorageDirectory.AbsolutePath));

			return builder.create();
		}

		public override void onClick(View v)
		{
			switch (v.Id)
			{
			case R.id.textViewTitle:
				string path = pathTextView.Text.ToString();
				if (!path.Equals(Environment.ExternalStorageDirectory.AbsolutePath))
				{
					string parentPath = path.Substring(0, path.LastIndexOf('/'));
					updateFileList(new File(parentPath));
				}
				break;

			case R.id.buttonOk:
				dismiss();
				onClickListener.onClick(filePath);
				break;

			case R.id.buttonCancel:
				dismiss();
				break;
			}
		}

		public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			if (files[position].Directory)
			{
				updateFileList(files[position]);
			}
			else
			{
				view.Selected = true;
				okButton.Enabled = true;
				filePath = files[position].AbsolutePath;
			}
		}

		private void updateFileList(File file)
		{
			files = file.listFiles();
			Arrays.sort(files);

			if (pathTextView != null)
			{
				pathTextView.Text = file.AbsolutePath;
			}

			if (fileAdapter != null)
			{
				fileAdapter.notifyDataSetInvalidated();
			}

			okButton.Enabled = false;
		}

		private class FileAdapter : BaseAdapter
		{
			private readonly FileListDialogFragment outerInstance;


			internal readonly LayoutInflater layoutInflater;

			public FileAdapter(FileListDialogFragment outerInstance, Context context)
			{
				this.outerInstance = outerInstance;
				layoutInflater = LayoutInflater.from(context);
			}

			public override int Count
			{
				get
				{
					if (outerInstance.files != null)
					{
						return outerInstance.files.Length;
					}
					else
					{
						return -1;
					}
				}
			}

			public override object getItem(int position)
			{
				return outerInstance.files[position];
			}

			public override long getItemId(int position)
			{
				return position;
			}

			public override View getView(int position, View convertView, ViewGroup parent)
			{
				if (convertView == null)
				{
					convertView = layoutInflater.inflate(R.layout.file_list_item, parent, false);
				}

				File file = outerInstance.files[position];
				TextView textView = (TextView) convertView.findViewById(R.id.textView1);

				if (file.Directory)
				{
					textView.setCompoundDrawablesWithIntrinsicBounds(R.drawable.ic_directory, 0, 0, 0);
				}
				else
				{
					textView.setCompoundDrawablesWithIntrinsicBounds(R.drawable.ic_file, 0, 0, 0);
				}

				textView.Text = file.Name;

				return convertView;
			}

		}
	}

}