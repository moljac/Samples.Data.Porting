namespace com.stripe.example.dialog
{

	using Dialog = android.app.Dialog;
	using ProgressDialog = android.app.ProgressDialog;
	using Bundle = android.os.Bundle;
	using DialogFragment = android.support.v4.app.DialogFragment;

	public class ProgressDialogFragment : DialogFragment
	{
		public static ProgressDialogFragment newInstance(int msgId)
		{
			ProgressDialogFragment fragment = new ProgressDialogFragment();

			Bundle args = new Bundle();
			args.putInt("msgId", msgId);

			fragment.Arguments = args;

			return fragment;
		}

		public ProgressDialogFragment()
		{
			// Empty constructor required for DialogFragment
		}

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			int msgId = Arguments.getInt("msgId");
			ProgressDialog dialog = new ProgressDialog(Activity);
			dialog.Message = Activity.Resources.getString(msgId);
			return dialog;
		}
	}

}