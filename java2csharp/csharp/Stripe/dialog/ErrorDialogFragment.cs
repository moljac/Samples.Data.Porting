namespace com.stripe.example.dialog
{

	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogInterface = android.content.DialogInterface;
	using Bundle = android.os.Bundle;
	using DialogFragment = android.support.v4.app.DialogFragment;

	public class ErrorDialogFragment : DialogFragment
	{
		public static ErrorDialogFragment newInstance(int titleId, string message)
		{
			ErrorDialogFragment fragment = new ErrorDialogFragment();

			Bundle args = new Bundle();
			args.putInt("titleId", titleId);
			args.putString("message", message);

			fragment.Arguments = args;

			return fragment;
		}

		public ErrorDialogFragment()
		{
			// Empty constructor required for DialogFragment
		}

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			int titleId = Arguments.getInt("titleId");
			string message = Arguments.getString("message");

			return (new AlertDialog.Builder(Activity)).setTitle(titleId).setMessage(message).setPositiveButton(R.@string.ok, new OnClickListenerAnonymousInnerClassHelper(this))
			   .create();
		}

		private class OnClickListenerAnonymousInnerClassHelper : DialogInterface.OnClickListener
		{
			private readonly ErrorDialogFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(ErrorDialogFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(DialogInterface dialogInterface, int i)
			{
				dialogInterface.dismiss();
			}
		}
	}
}