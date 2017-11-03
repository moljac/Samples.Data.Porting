using System;

namespace com.stripe.example.activity
{

	using Intent = android.content.Intent;
	using NonNull = android.support.annotation.NonNull;
	using Nullable = android.support.annotation.Nullable;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Bundle = android.os.Bundle;
	using LinearLayoutManager = android.support.v7.widget.LinearLayoutManager;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using View = android.view.View;
	using Button = android.widget.Button;

	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Stripe = com.stripe.android.Stripe;
	using Card = com.stripe.android.model.Card;
	using Source = com.stripe.android.model.Source;
	using SourceCardData = com.stripe.android.model.SourceCardData;
	using SourceParams = com.stripe.android.model.SourceParams;
	using CardInputWidget = com.stripe.android.view.CardInputWidget;
	using RedirectAdapter = com.stripe.example.adapter.RedirectAdapter;
	using ErrorDialogHandler = com.stripe.example.controller.ErrorDialogHandler;
	using RedirectDialogController = com.stripe.example.controller.RedirectDialogController;
	using ProgressDialogController = com.stripe.example.controller.ProgressDialogController;

	using Observable = rx.Observable;
	using AndroidSchedulers = rx.android.schedulers.AndroidSchedulers;
	using Action0 = rx.functions.Action0;
	using Action1 = rx.functions.Action1;
	using Schedulers = rx.schedulers.Schedulers;
	using CompositeSubscription = rx.subscriptions.CompositeSubscription;

	/// <summary>
	/// Activity that lets you redirect for a 3DS source verification.
	/// </summary>
	public class RedirectActivity : AppCompatActivity
	{

		private const string RETURN_SCHEMA = "stripe://";
		private const string RETURN_HOST_ASYNC = "async";
		private const string RETURN_HOST_SYNC = "sync";

		private const string QUERY_CLIENT_SECRET = "client_secret";
		private const string QUERY_SOURCE_ID = "source";

		private CardInputWidget mCardInputWidget;
		private CompositeSubscription mCompositeSubscription;
		private RedirectAdapter mRedirectAdapter;
		private ErrorDialogHandler mErrorDialogHandler;
		private RedirectDialogController mRedirectDialogController;
		private Source mRedirectSource;
		private ProgressDialogController mProgressDialogController;
		private Stripe mStripe;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_polling;

			mCompositeSubscription = new CompositeSubscription();
			mCardInputWidget = (CardInputWidget) findViewById(R.id.card_widget_three_d);
			mErrorDialogHandler = new ErrorDialogHandler(this.SupportFragmentManager);
			mProgressDialogController = new ProgressDialogController(this.SupportFragmentManager);
			mRedirectDialogController = new RedirectDialogController(this);
			mStripe = new Stripe(this);

			Button threeDSecureButton = (Button) findViewById(R.id.btn_three_d_secure);
			threeDSecureButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			Button threeDSyncButton = (Button) findViewById(R.id.btn_three_d_secure_sync);
			threeDSyncButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
			RecyclerView recyclerView = (RecyclerView) findViewById(R.id.recycler_view);

			RecyclerView.LayoutManager linearLayoutManager = new LinearLayoutManager(this);
			recyclerView.HasFixedSize = true;
			recyclerView.LayoutManager = linearLayoutManager;
			mRedirectAdapter = new RedirectAdapter();
			recyclerView.Adapter = mRedirectAdapter;

		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly RedirectActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.beginSequence();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly RedirectActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.beginSequence();
			}
		}

		protected internal override void onNewIntent(Intent intent)
		{
			base.onNewIntent(intent);
			if (intent.Data != null && intent.Data.Query != null)
			{
				// The client secret and source ID found here is identical to
				// that of the source used to get the redirect URL.
				string clientSecret = intent.Data.getQueryParameter(QUERY_CLIENT_SECRET);
				string sourceId = intent.Data.getQueryParameter(QUERY_SOURCE_ID);
				if (clientSecret != null && sourceId != null && clientSecret.Equals(mRedirectSource.ClientSecret) && sourceId.Equals(mRedirectSource.Id))
				{
					updateSourceList(mRedirectSource);
					mRedirectSource = null;
				}
				mRedirectDialogController.dismissDialog();
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			mCompositeSubscription.unsubscribe();
		}

		internal virtual void beginSequence()
		{
			Card displayCard = mCardInputWidget.Card;
			if (displayCard == null)
			{
				return;
			}
			createCardSource(displayCard);
		}

		/// <summary>
		/// To start the 3DS cycle, create a <seealso cref="Source"/> out of the user-entered <seealso cref="Card"/>.
		/// </summary>
		/// <param name="card"> the <seealso cref="Card"/> used to create a source </param>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: void createCardSource(@NonNull com.stripe.android.model.Card card)
		internal virtual void createCardSource(Card card)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.model.SourceParams cardSourceParams = com.stripe.android.model.SourceParams.createCardParams(card);
			SourceParams cardSourceParams = SourceParams.createCardParams(card);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final rx.Observable<com.stripe.android.model.Source> cardSourceObservable = rx.Observable.fromCallable(new java.util.concurrent.Callable<com.stripe.android.model.Source>()
			Observable<Source> cardSourceObservable = Observable.fromCallable(new CallableAnonymousInnerClassHelper(this, cardSourceParams));

//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
//			mCompositeSubscription.add(cardSourceObservable.subscribeOn(rx.schedulers.Schedulers.io()).observeOn(rx.android.schedulers.AndroidSchedulers.mainThread()).doOnSubscribe(new rx.functions.Action0()
	//		{
	//					@@Override public void call()
	//					{
	//						mProgressDialogController.setMessageResource(R.@string.createSource);
	//						mProgressDialogController.startProgress();
	//					}
	//				}
				   ).subscribe(new Action1AnonymousInnerClassHelper(this)
						   , new Action1AnonymousInnerClassHelper2(this)
				   ));
		}

		private class CallableAnonymousInnerClassHelper : Callable<Source>
		{
			private readonly RedirectActivity outerInstance;

			private SourceParams cardSourceParams;

			public CallableAnonymousInnerClassHelper(RedirectActivity outerInstance, SourceParams cardSourceParams)
			{
				this.outerInstance = outerInstance;
				this.cardSourceParams = cardSourceParams;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public com.stripe.android.model.Source call() throws Exception
			public override Source call()
			{
				return outerInstance.mStripe.createSourceSynchronous(cardSourceParams, PaymentConfiguration.Instance.PublishableKey);
			}
		}

		private class Action1AnonymousInnerClassHelper : Action1<Source>
		{
			private readonly RedirectActivity outerInstance;

			public Action1AnonymousInnerClassHelper(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

								// Because we've made the mapping above, we're now subscribing
								// to the result of creating a 3DS Source
			public override void call(Source source)
			{
				SourceCardData sourceCardData = (SourceCardData) source.SourceTypeModel;

				// Making a note of the Card Source in our list.
				outerInstance.mRedirectAdapter.addItem(source.Status, sourceCardData.ThreeDSecureStatus, source.Id, source.Type);
				// If we need to get 3DS verification for this card, we
				// first create a 3DS Source.
				if (SourceCardData.REQUIRED.Equals(sourceCardData.ThreeDSecureStatus))
				{

					// The card Source can be used to create a 3DS Source
					outerInstance.createThreeDSecureSource(source.Id);
				}
				else
				{
					outerInstance.mProgressDialogController.finishProgress();
				}

			}
		}

		private class Action1AnonymousInnerClassHelper2 : Action1<Exception>
		{
			private readonly RedirectActivity outerInstance;

			public Action1AnonymousInnerClassHelper2(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Exception throwable)
			{
				outerInstance.mErrorDialogHandler.showError(throwable.Message);
			}
		}

		/// <summary>
		/// Create the 3DS Source as a separate call to the API. This is what is needed
		/// to verify the third-party approval. The only information from the Card source
		/// that is used is the ID field.
		/// </summary>
		/// <param name="sourceId"> the <seealso cref="Source#mId"/> from the <seealso cref="Card"/>-created <seealso cref="Source"/>. </param>
		internal virtual void createThreeDSecureSource(string sourceId)
		{
			// This represents a request for a 3DS purchase of 10.00 euro.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final com.stripe.android.model.SourceParams threeDParams = com.stripe.android.model.SourceParams.createThreeDSecureParams(1000L, "EUR", getUrl(true), sourceId);
			SourceParams threeDParams = SourceParams.createThreeDSecureParams(1000L, "EUR", getUrl(true), sourceId);

			Observable<Source> threeDSecureObservable = Observable.fromCallable(new CallableAnonymousInnerClassHelper2(this, threeDParams));

//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
//			mCompositeSubscription.add(threeDSecureObservable.subscribeOn(rx.schedulers.Schedulers.io()).observeOn(rx.android.schedulers.AndroidSchedulers.mainThread()).doOnUnsubscribe(new rx.functions.Action0()
	//		{
	//					@@Override public void call()
	//					{
	//						mProgressDialogController.finishProgress();
	//					}
	//				}
				   ).subscribe(new Action1AnonymousInnerClassHelper3(this)
						   , new Action1AnonymousInnerClassHelper4(this)
				   ));
		}

		private class CallableAnonymousInnerClassHelper2 : Callable<Source>
		{
			private readonly RedirectActivity outerInstance;

			private SourceParams threeDParams;

			public CallableAnonymousInnerClassHelper2(RedirectActivity outerInstance, SourceParams threeDParams)
			{
				this.outerInstance = outerInstance;
				this.threeDParams = threeDParams;
			}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: @Override public com.stripe.android.model.Source call() throws Exception
			public override Source call()
			{
				return outerInstance.mStripe.createSourceSynchronous(threeDParams, PaymentConfiguration.Instance.PublishableKey);
			}
		}

		private class Action1AnonymousInnerClassHelper3 : Action1<Source>
		{
			private readonly RedirectActivity outerInstance;

			public Action1AnonymousInnerClassHelper3(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

								// Because we've made the mapping above, we're now subscribing
								// to the result of creating a 3DS Source
			public override void call(Source source)
			{
				// Once a 3DS Source is created, that is used
				// to initiate the third-party verification
				outerInstance.showDialog(source);
			}
		}

		private class Action1AnonymousInnerClassHelper4 : Action1<Exception>
		{
			private readonly RedirectActivity outerInstance;

			public Action1AnonymousInnerClassHelper4(RedirectActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void call(Exception throwable)
			{
				outerInstance.mErrorDialogHandler.showError(throwable.Message);
			}
		}

		/// <summary>
		/// Show a dialog with a link to the external verification site.
		/// </summary>
		/// <param name="source"> the <seealso cref="Source"/> to verify </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: void showDialog(final com.stripe.android.model.Source source)
		internal virtual void showDialog(Source source)
		{
			// Caching the source object here because this app makes a lot of them.
			mRedirectSource = source;
			mRedirectDialogController.showDialog(source.Redirect.Url);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private void updateSourceList(@Nullable com.stripe.android.model.Source source)
		private void updateSourceList(Source source)
		{
			if (source == null)
			{
				mRedirectAdapter.addItem("No source found", "Stopped", "Error", "None");
				return;
			}

			mRedirectAdapter.addItem(source.Status, "complete", source.Id, source.Type);
		}

		/// <summary>
		/// Helper method to determine the return URL we use. This is one way to return basic information
		/// to the activity (via the return Intent's host field). Because polling has been deprecated,
		/// we no longer use this parameter in the example application, but it is used here to see the
		/// relationship with the returned value for any parameters you may want to send.
		/// </summary>
		/// <param name="isSync"> whether or not to use a URL that tells us to use a sync method when we come
		///               back to the application </param>
		/// <returns> a return url to be sent to the vendor </returns>
		private static string getUrl(bool isSync)
		{
			if (isSync)
			{
				return RETURN_SCHEMA + RETURN_HOST_SYNC;
			}
			else
			{
				return RETURN_SCHEMA + RETURN_HOST_ASYNC;
			}
		}
	}

}