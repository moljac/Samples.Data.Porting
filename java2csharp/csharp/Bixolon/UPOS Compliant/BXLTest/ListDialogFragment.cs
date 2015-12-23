namespace com.bxl.postest
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using NonNull = android.support.annotation.NonNull;
	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;

	public class ListDialogFragment : DialogFragment
	{

		private const string KEY_TITLE = "title";
		private const string KEY_ITEMS = "items";
		private static readonly string TAG = typeof(ListDialogFragment).Name;

		internal interface OnClickListener
		{
			void onClick(string title, string text);
		}

		private OnClickListener onClickListener;

		internal static void showDialog(FragmentManager manager, string title, string[] items)
		{
			ListDialogFragment listDialogFragment = new ListDialogFragment();
			Bundle args = new Bundle();
			args.putString(KEY_TITLE, title);
			args.putStringArray(KEY_ITEMS, items);
			listDialogFragment.Arguments = args;
			listDialogFragment.show(manager, TAG);
		}

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
				throw new System.InvalidCastException(activity.ToString() + " must implement ListDialogFragment.OnClickListener");
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @NonNull public android.app.Dialog onCreateDialog(android.os.Bundle savedInstanceState)
		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			Bundle args = Arguments;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String title = args.getString(KEY_TITLE);
			string title = args.getString(KEY_TITLE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = args.getStringArray(KEY_ITEMS);
			string[] items = args.getStringArray(KEY_ITEMS);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.setTitle(title).setItems(items, new OnClickListenerAnonymousInnerClassHelper(this, title, items));
			return builder.create();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly ListDialogFragment outerInstance;

			private string title;
			private string[] items;

			public OnClickListenerAnonymousInnerClassHelper(ListDialogFragment outerInstance, string title, string[] items)
			{
				this.outerInstance = outerInstance;
				this.title = title;
				this.items = items;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				outerInstance.onClickListener.onClick(title, items[which]);
			}
		}
	}

}