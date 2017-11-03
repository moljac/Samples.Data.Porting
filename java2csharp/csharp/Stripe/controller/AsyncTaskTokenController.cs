using System;

namespace com.stripe.example.controller
{

	using Context = android.content.Context;
	using NonNull = android.support.annotation.NonNull;
	using View = android.view.View;
	using Button = android.widget.Button;

	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Stripe = com.stripe.android.Stripe;
	using TokenCallback = com.stripe.android.TokenCallback;
	using Card = com.stripe.android.model.Card;
	using Token = com.stripe.android.model.Token;
	using CardInputWidget = com.stripe.android.view.CardInputWidget;

	/// <summary>
	/// Logic needed to create tokens using the <seealso cref="android.os.AsyncTask"/> methods included in the
	/// sdk: <seealso cref="Stripe#createToken(Card, String, TokenCallback)"/>.
	/// </summary>
	public class AsyncTaskTokenController
	{

		private CardInputWidget mCardInputWidget;
		private Context mContext;
		private ErrorDialogHandler mErrorDialogHandler;
		private ListViewController mOutputListController;
		private ProgressDialogController mProgressDialogController;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public AsyncTaskTokenController(@NonNull android.widget.Button button, @NonNull com.stripe.android.view.CardInputWidget cardInputWidget, @NonNull android.content.Context context, @NonNull ErrorDialogHandler errorDialogHandler, @NonNull ListViewController outputListController, @NonNull ProgressDialogController progressDialogController)
		public AsyncTaskTokenController(Button button, CardInputWidget cardInputWidget, Context context, ErrorDialogHandler errorDialogHandler, ListViewController outputListController, ProgressDialogController progressDialogController)
		{
			mCardInputWidget = cardInputWidget;
			mContext = context;
			mErrorDialogHandler = errorDialogHandler;
			mProgressDialogController = progressDialogController;
			mOutputListController = outputListController;

			button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly AsyncTaskTokenController outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(AsyncTaskTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.saveCard();
			}
		}

		public virtual void detach()
		{
			mCardInputWidget = null;
		}

		private void saveCard()
		{
			Card cardToSave = mCardInputWidget.Card;
			if (cardToSave == null)
			{
				mErrorDialogHandler.showError("Invalid Card Data");
				return;
			}
			mProgressDialogController.startProgress();
			(new Stripe(mContext)).createToken(cardToSave, PaymentConfiguration.Instance.PublishableKey, new TokenCallbackAnonymousInnerClassHelper(this));
		}

		private class TokenCallbackAnonymousInnerClassHelper : TokenCallback
		{
			private readonly AsyncTaskTokenController outerInstance;

			public TokenCallbackAnonymousInnerClassHelper(AsyncTaskTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onSuccess(Token token)
			{
				outerInstance.mOutputListController.addToList(token);
				outerInstance.mProgressDialogController.finishProgress();
			}
			public virtual void onError(Exception error)
			{
				outerInstance.mErrorDialogHandler.showError(error.LocalizedMessage);
				outerInstance.mProgressDialogController.finishProgress();
			}
		}
	}

}