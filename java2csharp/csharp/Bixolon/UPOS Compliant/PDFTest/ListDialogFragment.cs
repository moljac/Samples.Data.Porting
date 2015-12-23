namespace com.bxl.pdftest
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;

	public class ListDialogFragment : DialogFragment
	{

		private static readonly string TAG = typeof(ListDialogFragment).Name;
		private const string KEY_TITLE = "title";
		private const string KEY_ITEMS = "items";

		internal interface OnClickListener
		{
			void onClick(string item);
		}

		private OnClickListener onClickListener;

		internal static void showDialog(FragmentManager manager, string title, string[] items)
		{
			Bundle args = new Bundle();
			args.putString(KEY_TITLE, title);
			args.putStringArray(KEY_ITEMS, items);

			ListDialogFragment listDialogFragment = new ListDialogFragment();
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

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			string title = Arguments.getString(KEY_TITLE);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String[] items = getArguments().getStringArray(KEY_ITEMS);
			string[] items = Arguments.getStringArray(KEY_ITEMS);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.setTitle(title).setItems(items, new OnClickListenerAnonymousInnerClassHelper(this, items));
			return builder.create();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly ListDialogFragment outerInstance;

			private string[] items;

			public OnClickListenerAnonymousInnerClassHelper(ListDialogFragment outerInstance, string[] items)
			{
				this.outerInstance = outerInstance;
				this.items = items;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				outerInstance.onClickListener.onClick(items[which]);
			}
		}
	}

}