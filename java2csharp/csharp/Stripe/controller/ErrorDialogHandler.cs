namespace com.stripe.example.controller
{

	using DialogFragment = android.support.v4.app.DialogFragment;
	using FragmentManager = android.support.v4.app.FragmentManager;

	using ErrorDialogFragment = com.stripe.example.dialog.ErrorDialogFragment;

	/// <summary>
	/// A convenience class to handle displaying error dialogs.
	/// </summary>
	public class ErrorDialogHandler
	{

		internal FragmentManager mFragmentManager;

		public ErrorDialogHandler(FragmentManager fragmentManager)
		{
			mFragmentManager = fragmentManager;
		}

		public virtual void showError(string errorMessage)
		{
			DialogFragment fragment = ErrorDialogFragment.newInstance(R.@string.validationErrors, errorMessage);
			fragment.show(mFragmentManager, "error");
		}
	}

}