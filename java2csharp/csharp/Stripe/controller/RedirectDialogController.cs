namespace com.stripe.example.controller
{

	using Intent = android.content.Intent;
	using Uri = android.net.Uri;
	using AlertDialog = android.support.v7.app.AlertDialog;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using TextView = android.widget.TextView;

	/// <summary>
	/// Controller for the redirect dialog used to direct users out of the application.
	/// </summary>
	public class RedirectDialogController
	{

		internal AppCompatActivity mActivity;
		internal AlertDialog mAlertDialog;

		public RedirectDialogController(AppCompatActivity appCompatActivity)
		{
			mActivity = appCompatActivity;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void showDialog(final String url)
		public virtual void showDialog(string url)
		{
			AlertDialog.Builder builder = new AlertDialog.Builder(mActivity);
			View dialogView = LayoutInflater.from(mActivity).inflate(R.layout.polling_dialog, null);

			TextView linkView = (TextView) dialogView.findViewById(R.id.tv_link_redirect);
			linkView.Text = R.@string.verify;
			linkView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, url);
			builder.View = dialogView;

			mAlertDialog = builder.create();
			mAlertDialog.show();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly RedirectDialogController outerInstance;

			private string url;

			public OnClickListenerAnonymousInnerClassHelper(RedirectDialogController outerInstance, string url)
			{
				this.outerInstance = outerInstance;
				this.url = url;
			}

			public override void onClick(View v)
			{
				Intent browserIntent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
				outerInstance.mActivity.startActivity(browserIntent);
			}
		}

		public virtual void dismissDialog()
		{
			if (mAlertDialog != null)
			{
				mAlertDialog.dismiss();
				mAlertDialog = null;
			}
		}
	}

}