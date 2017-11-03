namespace com.stripe.example.controller
{

	using Activity = android.app.Activity;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using NonNull = android.support.annotation.NonNull;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using View = android.view.View;
	using Button = android.widget.Button;

	using Card = com.stripe.android.model.Card;
	using CardInputWidget = com.stripe.android.view.CardInputWidget;
	using TokenIntentService = com.stripe.example.service.TokenIntentService;

	/// <summary>
	/// Example class showing how to save tokens with an <seealso cref="android.app.IntentService"/> doing your
	/// background I/O work.
	/// </summary>
	public class IntentServiceTokenController
	{

		private Activity mActivity;
		private CardInputWidget mCardInputWidget;
		private ErrorDialogHandler mErrorDialogHandler;
		private ListViewController mOutputListViewController;
		private ProgressDialogController mProgressDialogController;

		private TokenBroadcastReceiver mTokenBroadcastReceiver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public IntentServiceTokenController(@NonNull android.support.v7.app.AppCompatActivity appCompatActivity, @NonNull android.widget.Button button, @NonNull com.stripe.android.view.CardInputWidget cardInputWidget, @NonNull ErrorDialogHandler errorDialogHandler, @NonNull ListViewController outputListController, @NonNull ProgressDialogController progressDialogController)
		public IntentServiceTokenController(AppCompatActivity appCompatActivity, Button button, CardInputWidget cardInputWidget, ErrorDialogHandler errorDialogHandler, ListViewController outputListController, ProgressDialogController progressDialogController)
		{

			mActivity = appCompatActivity;
			mCardInputWidget = cardInputWidget;
			mErrorDialogHandler = errorDialogHandler;
			mOutputListViewController = outputListController;
			mProgressDialogController = progressDialogController;

			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			registerBroadcastReceiver();
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly IntentServiceTokenController outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(IntentServiceTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.saveCard();
			}
		}

		/// <summary>
		/// Unregister the <seealso cref="BroadcastReceiver"/>.
		/// </summary>
		public virtual void detach()
		{
			if (mTokenBroadcastReceiver != null)
			{
				LocalBroadcastManager.getInstance(mActivity).unregisterReceiver(mTokenBroadcastReceiver);
				mTokenBroadcastReceiver = null;
				mActivity = null;
			}
			mCardInputWidget = null;
		}

		private void registerBroadcastReceiver()
		{
			mTokenBroadcastReceiver = new TokenBroadcastReceiver(this);
			LocalBroadcastManager.getInstance(mActivity).registerReceiver(mTokenBroadcastReceiver, new IntentFilter(TokenIntentService.TOKEN_ACTION));
		}

		private void saveCard()
		{
			Card cardToSave = mCardInputWidget.Card;
			if (cardToSave == null)
			{
				mErrorDialogHandler.showError("Invalid Card Data");
				return;
			}
			Intent tokenServiceIntent = TokenIntentService.createTokenIntent(mActivity, cardToSave.Number, cardToSave.ExpMonth, cardToSave.ExpYear, cardToSave.CVC);
			mProgressDialogController.startProgress();
			mActivity.startService(tokenServiceIntent);
		}

		private class TokenBroadcastReceiver : BroadcastReceiver
		{
			private readonly IntentServiceTokenController outerInstance;


			// Prevent instantiation of a local broadcast receiver outside this class.
			internal TokenBroadcastReceiver(IntentServiceTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onReceive(Context context, Intent intent)
			{
				outerInstance.mProgressDialogController.finishProgress();

				if (intent == null)
				{
					return;
				}

				if (intent.hasExtra(TokenIntentService.STRIPE_ERROR_MESSAGE))
				{
					outerInstance.mErrorDialogHandler.showError(intent.getStringExtra(TokenIntentService.STRIPE_ERROR_MESSAGE));
					return;
				}

				if (intent.hasExtra(TokenIntentService.STRIPE_CARD_TOKEN_ID) && intent.hasExtra(TokenIntentService.STRIPE_CARD_LAST_FOUR))
				{
					outerInstance.mOutputListViewController.addToList(intent.getStringExtra(TokenIntentService.STRIPE_CARD_LAST_FOUR), intent.getStringExtra(TokenIntentService.STRIPE_CARD_TOKEN_ID));
				}
			}
		}
	}

}