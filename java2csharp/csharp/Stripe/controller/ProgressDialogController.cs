namespace com.stripe.example.controller
{

	using NonNull = android.support.annotation.NonNull;
	using StringRes = android.support.annotation.StringRes;
	using FragmentManager = android.support.v4.app.FragmentManager;

	using ProgressDialogFragment = com.stripe.example.dialog.ProgressDialogFragment;

	/// <summary>
	/// Class used to show and hide the progress spinner.
	/// </summary>
	public class ProgressDialogController
	{

		private FragmentManager mFragmentManager;
		private ProgressDialogFragment mProgressFragment;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public ProgressDialogController(@NonNull android.support.v4.app.FragmentManager fragmentManager)
		public ProgressDialogController(FragmentManager fragmentManager)
		{
			mFragmentManager = fragmentManager;
			mProgressFragment = ProgressDialogFragment.newInstance(R.@string.progressMessage);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void setMessageResource(@StringRes int resId)
		public virtual int MessageResource
		{
			set
			{
				if (mProgressFragment.Visible)
				{
					mProgressFragment.dismiss();
					mProgressFragment = null;
				}
				mProgressFragment = ProgressDialogFragment.newInstance(value);
			}
		}

		public virtual void startProgress()
		{
			mProgressFragment.show(mFragmentManager, "progress");
		}

		public virtual void finishProgress()
		{
			mProgressFragment.dismiss();
		}
	}

}