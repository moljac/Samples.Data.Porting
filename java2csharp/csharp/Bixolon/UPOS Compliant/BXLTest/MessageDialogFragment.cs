namespace com.bxl.postest
{

	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using NonNull = android.support.annotation.NonNull;
	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;

	public class MessageDialogFragment : DialogFragment
	{

		private const string KEY_TITLE = "title";
		private const string KEY_MESSAGE = "message";
		private static readonly string TAG = typeof(MessageDialogFragment).Name;

		internal static void showDialog(FragmentManager manager, string title, string message)
		{
			MessageDialogFragment messageDialogFragment = new MessageDialogFragment();
			Bundle args = new Bundle();
			args.putString(KEY_TITLE, title);
			args.putString(KEY_MESSAGE, message);
			messageDialogFragment.Arguments = args;
			messageDialogFragment.show(manager, TAG);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @NonNull public android.app.Dialog onCreateDialog(android.os.Bundle savedInstanceState)
		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			Bundle args = Arguments;
			string title = args.getString(KEY_TITLE);
			string message = args.getString(KEY_MESSAGE);

			AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
			builder.setTitle(title).setMessage(message).setPositiveButton(R.@string.ok, new OnClickListenerAnonymousInnerClassHelper(this));

			return builder.create();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly MessageDialogFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MessageDialogFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(DialogInterface dialog, int which)
			{
				// TODO Auto-generated method stub

			}
		}
	}

}