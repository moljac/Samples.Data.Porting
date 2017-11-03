namespace com.stripe.example.module
{

	using Context = android.content.Context;
	using NonNull = android.support.annotation.NonNull;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;

	using Card = com.stripe.android.model.Card;
	using CardInputWidget = com.stripe.android.view.CardInputWidget;
	using AsyncTaskTokenController = com.stripe.example.controller.AsyncTaskTokenController;
	using ErrorDialogHandler = com.stripe.example.controller.ErrorDialogHandler;
	using IntentServiceTokenController = com.stripe.example.controller.IntentServiceTokenController;
	using ListViewController = com.stripe.example.controller.ListViewController;
	using ProgressDialogController = com.stripe.example.controller.ProgressDialogController;
	using RxTokenController = com.stripe.example.controller.RxTokenController;

	/// <summary>
	/// A dagger-free simple way to handle dependencies in the Example project. Most of this work would
	/// ordinarily be done in a module class.
	/// </summary>
	public class DependencyHandler
	{

		private AsyncTaskTokenController mAsyncTaskController;
		private CardInputWidget mCardInputWidget;
		private Context mContext;
		private ErrorDialogHandler mErrorDialogHandler;
		private IntentServiceTokenController mIntentServiceTokenController;
		private ListViewController mListViewController;
		private RxTokenController mRxTokenController;
		private ProgressDialogController mProgresDialogController;

		public DependencyHandler(AppCompatActivity activity, CardInputWidget cardInputWidget, ListView outputListView)
		{

			mCardInputWidget = cardInputWidget;
			mContext = activity.BaseContext;

			mProgresDialogController = new ProgressDialogController(activity.SupportFragmentManager);

			mListViewController = new ListViewController(outputListView);

			mErrorDialogHandler = new ErrorDialogHandler(activity.SupportFragmentManager);
		}

		/// <summary>
		/// Attach a listener that creates a token using the <seealso cref="android.os.AsyncTask"/>-based method.
		/// Only gets attached once, unless you call <seealso cref="#clearReferences()"/>.
		/// </summary>
		/// <param name="button"> a button that, when clicked, gets a token. </param>
		/// <returns> a reference to the <seealso cref="AsyncTaskTokenController"/> </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NonNull public com.stripe.example.controller.AsyncTaskTokenController attachAsyncTaskTokenController(android.widget.Button button)
		public virtual AsyncTaskTokenController attachAsyncTaskTokenController(Button button)
		{
			if (mAsyncTaskController == null)
			{
				mAsyncTaskController = new AsyncTaskTokenController(button, mCardInputWidget, mContext, mErrorDialogHandler, mListViewController, mProgresDialogController);
			}
			return mAsyncTaskController;
		}

		/// <summary>
		/// Attach a listener that creates a token using an <seealso cref="android.app.IntentService"/> and the
		/// synchronous <seealso cref="com.stripe.android.Stripe#createTokenSynchronous(Card, String)"/> method.
		/// 
		/// Only gets attached once, unless you call <seealso cref="#clearReferences()"/>.
		/// </summary>
		/// <param name="button"> a button that, when clicked, gets a token. </param>
		/// <returns> a reference to the <seealso cref="IntentServiceTokenController"/> </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NonNull public com.stripe.example.controller.IntentServiceTokenController attachIntentServiceTokenController(android.support.v7.app.AppCompatActivity appCompatActivity, android.widget.Button button)
		public virtual IntentServiceTokenController attachIntentServiceTokenController(AppCompatActivity appCompatActivity, Button button)
		{
			if (mIntentServiceTokenController == null)
			{
				mIntentServiceTokenController = new IntentServiceTokenController(appCompatActivity, button, mCardInputWidget, mErrorDialogHandler, mListViewController, mProgresDialogController);
			}
			return mIntentServiceTokenController;
		}

		/// <summary>
		/// Attach a listener that creates a token using a <seealso cref="rx.Subscription"/> and the
		/// synchronous <seealso cref="com.stripe.android.Stripe#createTokenSynchronous(Card, String)"/> method.
		/// 
		/// Only gets attached once, unless you call <seealso cref="#clearReferences()"/>.
		/// </summary>
		/// <param name="button"> a button that, when clicked, gets a token. </param>
		/// <returns> a reference to the <seealso cref="RxTokenController"/> </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @NonNull public com.stripe.example.controller.RxTokenController attachRxTokenController(android.widget.Button button)
		public virtual RxTokenController attachRxTokenController(Button button)
		{
			if (mRxTokenController == null)
			{
				mRxTokenController = new RxTokenController(button, mCardInputWidget, mContext, mErrorDialogHandler, mListViewController, mProgresDialogController);
			}
			return mRxTokenController;
		}

		/// <summary>
		/// Clear all the references so that we can start over again.
		/// </summary>
		public virtual void clearReferences()
		{

			if (mAsyncTaskController != null)
			{
				mAsyncTaskController.detach();
			}

			if (mRxTokenController != null)
			{
				mRxTokenController.detach();
			}

			if (mIntentServiceTokenController != null)
			{
				mIntentServiceTokenController.detach();
			}

			mAsyncTaskController = null;
			mRxTokenController = null;
			mIntentServiceTokenController = null;
		}
	}

}