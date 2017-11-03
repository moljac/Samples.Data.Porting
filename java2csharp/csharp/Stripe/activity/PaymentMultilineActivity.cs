using System;
using System.Collections.Generic;

namespace com.stripe.example.activity
{

	using Nullable = android.support.annotation.Nullable;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Bundle = android.os.Bundle;
	using ListView = android.widget.ListView;
	using SimpleAdapter = android.widget.SimpleAdapter;

	using RxView = com.jakewharton.rxbinding.view.RxView;
	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Stripe = com.stripe.android.Stripe;
	using Card = com.stripe.android.model.Card;
	using Source = com.stripe.android.model.Source;
	using SourceCardData = com.stripe.android.model.SourceCardData;
	using SourceParams = com.stripe.android.model.SourceParams;
	using CardMultilineWidget = com.stripe.android.view.CardMultilineWidget;
	using ErrorDialogHandler = com.stripe.example.controller.ErrorDialogHandler;
	using ProgressDialogController = com.stripe.example.controller.ProgressDialogController;


	using Observable = rx.Observable;
	using AndroidSchedulers = rx.android.schedulers.AndroidSchedulers;
	using Action0 = rx.functions.Action0;
	using Action1 = rx.functions.Action1;
	using Schedulers = rx.schedulers.Schedulers;
	using CompositeSubscription = rx.subscriptions.CompositeSubscription;

	public class PaymentMultilineActivity : AppCompatActivity
	{

		internal ProgressDialogController mProgressDialogController;
		internal ErrorDialogHandler mErrorDialogHandler;

		internal CardMultilineWidget mCardMultilineWidget;
		internal CompositeSubscription mCompositeSubscription;

		private SimpleAdapter mSimpleAdapter;
		private IList<IDictionary<string, string>> mCardSources = new List<IDictionary<string, string>>();

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_payment_multiline;

			mCompositeSubscription = new CompositeSubscription();
			mCardMultilineWidget = findViewById(R.id.card_multiline_widget);

			mProgressDialogController = new ProgressDialogController(SupportFragmentManager);

			mErrorDialogHandler = new ErrorDialogHandler(SupportFragmentManager);

			ListView listView = findViewById(R.id.card_list_pma);
			mSimpleAdapter = new SimpleAdapter(this, mCardSources, R.layout.list_item_layout, new string[]{"last4", "tokenId"}, new int[]{R.id.last4, R.id.tokenId});

			listView.Adapter = mSimpleAdapter;
			mCompositeSubscription.add(RxView.clicks(findViewById(R.id.save_payment)).subscribe(new Action1AnonymousInnerClassHelper(this)));
		}

		private class Action1AnonymousInnerClassHelper : Action1<Void>
		{
			private readonly PaymentMultilineActivity outerInstance;

			public Action1AnonymousInnerClassHelper(PaymentMultilineActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Void aVoid)
			{
				outerInstance.saveCard();
			}
		}

		private void saveCard()
		{
			Card card = mCardMultilineWidget.Card;
			if (card == null)
			{
				return;
			}

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.Stripe stripe = new com.stripe.android.Stripe(this);
			Stripe stripe = new Stripe(this);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.model.SourceParams cardSourceParams = com.stripe.android.model.SourceParams.createCardParams(card);
			SourceParams cardSourceParams = SourceParams.createCardParams(card);
			// Note: using this style of Observable creation results in us having a method that
			// will not be called until we subscribe to it.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final rx.Observable<com.stripe.android.model.Source> tokenObservable = rx.Observable.fromCallable(new java.util.concurrent.Callable<com.stripe.android.model.Source>()
			Observable<Source> tokenObservable = Observable.fromCallable(new CallableAnonymousInnerClassHelper(this, stripe, cardSourceParams));

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

		private class CallableAnonymousInnerClassHelper : Callable<Source>
		{
			private readonly PaymentMultilineActivity outerInstance;

			private Stripe stripe;
			private SourceParams cardSourceParams;

			public CallableAnonymousInnerClassHelper(PaymentMultilineActivity outerInstance, Stripe stripe, SourceParams cardSourceParams)
			{
				this.outerInstance = outerInstance;
				this.stripe = stripe;
				this.cardSourceParams = cardSourceParams;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public com.stripe.android.model.Source call() throws Exception
			public override Source call()
			{
				return stripe.createSourceSynchronous(cardSourceParams, PaymentConfiguration.Instance.PublishableKey);
			}
		}

		private class Action0AnonymousInnerClassHelper2 : Action0
		{
			private readonly PaymentMultilineActivity outerInstance;

			public Action0AnonymousInnerClassHelper2(PaymentMultilineActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call()
			{
				outerInstance.mProgressDialogController.finishProgress();
			}
		}

		private class Action1AnonymousInnerClassHelper2 : Action1<Source>
		{
			private readonly PaymentMultilineActivity outerInstance;

			public Action1AnonymousInnerClassHelper2(PaymentMultilineActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Source source)
			{
				outerInstance.addToList(source);
			}
		}

		private class Action1AnonymousInnerClassHelper3 : Action1<Exception>
		{
			private readonly PaymentMultilineActivity outerInstance;

			public Action1AnonymousInnerClassHelper3(PaymentMultilineActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Exception throwable)
			{
				outerInstance.mErrorDialogHandler.showError(throwable.LocalizedMessage);
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private void addToList(@Nullable com.stripe.android.model.Source source)
		private void addToList(Source source)
		{
			if (source == null || !Source.CARD.Equals(source.Type))
			{
				return;
			}
			SourceCardData sourceCardData = (SourceCardData) source.SourceTypeModel;

			string endingIn = getString(R.@string.endingIn);
			IDictionary<string, string> map = new Dictionary<string, string>();
			map["last4"] = endingIn + " " + sourceCardData.Last4;
			map["tokenId"] = source.Id;
			mCardSources.Add(map);
			mSimpleAdapter.notifyDataSetChanged();
		}

	}

}