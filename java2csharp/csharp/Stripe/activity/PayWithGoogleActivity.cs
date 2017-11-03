namespace com.stripe.example.activity
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using NonNull = android.support.annotation.NonNull;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ProgressBar = android.widget.ProgressBar;
	using Toast = android.widget.Toast;

	using ApiException = com.google.android.gms.common.api.ApiException;
	using Status = com.google.android.gms.common.api.Status;
	using UserAddress = com.google.android.gms.identity.intents.model.UserAddress;
	using OnCompleteListener = com.google.android.gms.tasks.OnCompleteListener;
	using Task = com.google.android.gms.tasks.Task;
	using AutoResolveHelper = com.google.android.gms.wallet.AutoResolveHelper;
	using CardInfo = com.google.android.gms.wallet.CardInfo;
	using CardRequirements = com.google.android.gms.wallet.CardRequirements;
	using IsReadyToPayRequest = com.google.android.gms.wallet.IsReadyToPayRequest;
	using PaymentData = com.google.android.gms.wallet.PaymentData;
	using PaymentDataRequest = com.google.android.gms.wallet.PaymentDataRequest;
	using PaymentMethodTokenizationParameters = com.google.android.gms.wallet.PaymentMethodTokenizationParameters;
	using PaymentsClient = com.google.android.gms.wallet.PaymentsClient;
	using TransactionInfo = com.google.android.gms.wallet.TransactionInfo;
	using Wallet = com.google.android.gms.wallet.Wallet;
	using WalletConstants = com.google.android.gms.wallet.WalletConstants;
	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Token = com.stripe.android.model.Token;

	public class PayWithGoogleActivity : AppCompatActivity
	{

		private const int LOAD_PAYMENT_DATA_REQUEST_CODE = 53;

		private View mPayWithGoogleButton;
		private PaymentsClient mPaymentsClient;
		private ProgressBar mProgressBar;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_pay_with_google;
			mPaymentsClient = Wallet.getPaymentsClient(this, new Wallet.WalletOptions.Builder()
									.setEnvironment(WalletConstants.ENVIRONMENT_TEST).build());

			mProgressBar = findViewById(R.id.pwg_progress_bar);
			mPayWithGoogleButton = findViewById(R.id.btn_buy_pwg);
			mPayWithGoogleButton.Enabled = false;
			mPayWithGoogleButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			ReadyToPay;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly PayWithGoogleActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(PayWithGoogleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.payWithGoogle();
			}
		}

		private void payWithGoogle()
		{
			PaymentDataRequest request = createPaymentDataRequest();
			if (request != null)
			{
				AutoResolveHelper.resolveTask(mPaymentsClient.loadPaymentData(request), PayWithGoogleActivity.this, LOAD_PAYMENT_DATA_REQUEST_CODE);
			}
		}

		private void isReadyToPay()
		{
			mProgressBar.Visibility = View.VISIBLE;
			IsReadyToPayRequest request = IsReadyToPayRequest.newBuilder().addAllowedPaymentMethod(WalletConstants.PAYMENT_METHOD_CARD).addAllowedPaymentMethod(WalletConstants.PAYMENT_METHOD_TOKENIZED_CARD).build();
			Task<bool?> task = mPaymentsClient.isReadyToPay(request);
			task.addOnCompleteListener(new OnCompleteListenerAnonymousInnerClassHelper(this, task));

		}

		private class OnCompleteListenerAnonymousInnerClassHelper : OnCompleteListener<bool?>
		{
			private readonly PayWithGoogleActivity outerInstance;

//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: private com.google.android.gms.tasks.Task<bool?> task;
			private Task<bool?> task;

			public OnCompleteListenerAnonymousInnerClassHelper<T1>(PayWithGoogleActivity outerInstance, Task<T1> task)
			{
				this.outerInstance = outerInstance;
				this.task = task;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void onComplete(@NonNull com.google.android.gms.tasks.Task<Boolean> task)
			public virtual void onComplete(Task<bool?> task)
			{
				try
				{
					bool result = task.getResult(typeof(ApiException));
					outerInstance.mProgressBar.Visibility = View.INVISIBLE;
					if (result)
					{
						Toast.makeText(outerInstance, "Ready", Toast.LENGTH_SHORT).show();
						outerInstance.mPayWithGoogleButton.Enabled = true;
					}
					else
					{
						Toast.makeText(outerInstance, "No PWG", Toast.LENGTH_SHORT).show();
						//hide Google as payment option
					}
				}
				catch (ApiException exception)
				{
					Toast.makeTextuniquetempvar.show();
				}
			}
		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			switch (requestCode)
			{
				case LOAD_PAYMENT_DATA_REQUEST_CODE:
					switch (resultCode)
					{
						case Activity.RESULT_OK:
							PaymentData paymentData = PaymentData.getFromIntent(data);
							// You can get some data on the user's card, such as the brand and last 4 digits
							CardInfo info = paymentData.CardInfo;
							// You can also pull the user address from the PaymentData object.
							UserAddress address = paymentData.ShippingAddress;
							// This is the raw string version of your Stripe token.
							string rawToken = paymentData.PaymentMethodToken.Token;

							// Now that you have a Stripe token object, charge that by using the id
							Token stripeToken = Token.fromString(rawToken);
							if (stripeToken != null)
							{
								// This chargeToken function is a call to your own server, which should then connect
								// to Stripe's API to finish the charge.
								// chargeToken(stripeToken.getId());
								Toast.makeTextuniquetempvar.show();
							}
							break;
						case Activity.RESULT_CANCELED:
							Toast.makeText(PayWithGoogleActivity.this, "Canceled", Toast.LENGTH_LONG).show();

							break;
						case AutoResolveHelper.RESULT_ERROR:
							Status status = AutoResolveHelper.getStatusFromIntent(data);
							Toast.makeTextuniquetempvar.show();

							// Log the status for debugging
							// Generally there is no need to show an error to
							// the user as the Google Payment API will do that
							break;
						default:
							// Do nothing.
					break;
					}
					break; // Breaks the case LOAD_PAYMENT_DATA_REQUEST_CODE
					goto default;
				default:
					// Do nothing.
			break;
			}
		}

		private PaymentMethodTokenizationParameters createTokenizationParameters()
		{
			return PaymentMethodTokenizationParameters.newBuilder().setPaymentMethodTokenizationType(WalletConstants.PAYMENT_METHOD_TOKENIZATION_TYPE_PAYMENT_GATEWAY).addParameter("gateway", "stripe").addParameter("stripe:publishableKey", PaymentConfiguration.Instance.PublishableKey).addParameter("stripe:version", "5.1.1").build();
		}

		private PaymentDataRequest createPaymentDataRequest()
		{
			PaymentDataRequest.Builder request = PaymentDataRequest.newBuilder().setTransactionInfo(TransactionInfo.newBuilder().setTotalPriceStatus(WalletConstants.TOTAL_PRICE_STATUS_FINAL).setTotalPrice("10.00").setCurrencyCode("USD").build()).addAllowedPaymentMethod(WalletConstants.PAYMENT_METHOD_CARD).addAllowedPaymentMethod(WalletConstants.PAYMENT_METHOD_TOKENIZED_CARD).setCardRequirements(CardRequirements.newBuilder().addAllowedCardNetworks(Arrays.asList(WalletConstants.CARD_NETWORK_AMEX, WalletConstants.CARD_NETWORK_DISCOVER, WalletConstants.CARD_NETWORK_VISA, WalletConstants.CARD_NETWORK_MASTERCARD)).build());

			request.PaymentMethodTokenizationParameters = createTokenizationParameters();
			return request.build();
		}
	}

}