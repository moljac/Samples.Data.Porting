using System;

namespace com.stripe.example.controller
{

	using Context = android.content.Context;
	using NonNull = android.support.annotation.NonNull;
	using Button = android.widget.Button;

	using RxView = com.jakewharton.rxbinding.view.RxView;
	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Stripe = com.stripe.android.Stripe;
	using Card = com.stripe.android.model.Card;
	using Token = com.stripe.android.model.Token;
	using CardInputWidget = com.stripe.android.view.CardInputWidget;

	using Observable = rx.Observable;
	using AndroidSchedulers = rx.android.schedulers.AndroidSchedulers;
	using Action0 = rx.functions.Action0;
	using Action1 = rx.functions.Action1;
	using Schedulers = rx.schedulers.Schedulers;
	using CompositeSubscription = rx.subscriptions.CompositeSubscription;

	/// <summary>
	/// Class containing all the logic needed to create a token and listen for the results using
	/// RxJava.
	/// </summary>
	public class RxTokenController
	{

		private CardInputWidget mCardInputWidget;
		private CompositeSubscription mCompositeSubscription;
		private Context mContext;
		private ErrorDialogHandler mErrorDialogHandler;
		private ListViewController mOutputListController;
		private ProgressDialogController mProgressDialogController;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public RxTokenController(@NonNull android.widget.Button button, @NonNull com.stripe.android.view.CardInputWidget cardInputWidget, @NonNull android.content.Context context, @NonNull ErrorDialogHandler errorDialogHandler, @NonNull ListViewController outputListController, @NonNull ProgressDialogController progressDialogController)
		public RxTokenController(Button button, CardInputWidget cardInputWidget, Context context, ErrorDialogHandler errorDialogHandler, ListViewController outputListController, ProgressDialogController progressDialogController)
		{
			mCompositeSubscription = new CompositeSubscription();

			mCardInputWidget = cardInputWidget;
			mContext = context;
			mErrorDialogHandler = errorDialogHandler;
			mOutputListController = outputListController;
			mProgressDialogController = progressDialogController;

			mCompositeSubscription.add(RxView.clicks(button).subscribe(new Action1AnonymousInnerClassHelper(this)));
		}

		private class Action1AnonymousInnerClassHelper : Action1<Void>
		{
			private readonly RxTokenController outerInstance;

			public Action1AnonymousInnerClassHelper(RxTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Void aVoid)
			{
				outerInstance.saveCard();
			}
		}

		/// <summary>
		/// Release subscriptions to prevent memory leaks.
		/// </summary>
		public virtual void detach()
		{
			if (mCompositeSubscription != null)
			{
				mCompositeSubscription.unsubscribe();
			}
			mCardInputWidget = null;
		}

		private void saveCard()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.model.Card cardToSave = mCardInputWidget.getCard();
			Card cardToSave = mCardInputWidget.Card;
			if (cardToSave == null)
			{
				mErrorDialogHandler.showError("Invalid Card Data");
				return;
			}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.Stripe stripe = new com.stripe.android.Stripe(mContext);
			Stripe stripe = new Stripe(mContext);

			// Note: using this style of Observable creation results in us having a method that
			// will not be called until we subscribe to it.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final rx.Observable<com.stripe.android.model.Token> tokenObservable = rx.Observable.fromCallable(new java.util.concurrent.Callable<com.stripe.android.model.Token>()
			Observable<Token> tokenObservable = Observable.fromCallable(new CallableAnonymousInnerClassHelper(this, cardToSave, stripe));

//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
//			mCompositeSubscription.add(tokenObservable.subscribeOn(rx.schedulers.Schedulers.io()).observeOn(rx.android.schedulers.AndroidSchedulers.mainThread()).doOnSubscribe(new rx.functions.Action0()
	//		{
	//							@@Override public void call()
	//							{
	//								mProgressDialogController.startProgress();
	//							}
	//						}
						   ).doOnUnsubscribe(new Action0AnonymousInnerClassHelper2(this)
						   ).subscribe(new Action1AnonymousInnerClassHelper2(this)
						   , new Action1AnonymousInnerClassHelper3(this)
						   ));
		}

		private class CallableAnonymousInnerClassHelper : Callable<Token>
		{
			private readonly RxTokenController outerInstance;

			private Card cardToSave;
			private Stripe stripe;

			public CallableAnonymousInnerClassHelper(RxTokenController outerInstance, Card cardToSave, Stripe stripe)
			{
				this.outerInstance = outerInstance;
				this.cardToSave = cardToSave;
				this.stripe = stripe;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public com.stripe.android.model.Token call() throws Exception
			public override Token call()
			{
				return stripe.createTokenSynchronous(cardToSave, PaymentConfiguration.Instance.PublishableKey);
			}
		}

		private class Action0AnonymousInnerClassHelper2 : Action0
		{
			private readonly RxTokenController outerInstance;

			public Action0AnonymousInnerClassHelper2(RxTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call()
			{
				outerInstance.mProgressDialogController.finishProgress();
			}
		}

		private class Action1AnonymousInnerClassHelper2 : Action1<Token>
		{
			private readonly RxTokenController outerInstance;

			public Action1AnonymousInnerClassHelper2(RxTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Token token)
			{
				outerInstance.mOutputListController.addToList(token);
			}
		}

		private class Action1AnonymousInnerClassHelper3 : Action1<Exception>
		{
			private readonly RxTokenController outerInstance;

			public Action1AnonymousInnerClassHelper3(RxTokenController outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Exception throwable)
			{
				outerInstance.mErrorDialogHandler.showError(throwable.LocalizedMessage);
			}
		}
	}

}