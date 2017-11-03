namespace com.stripe.example.service
{

	using Activity = android.app.Activity;
	using IntentService = android.app.IntentService;
	using Intent = android.content.Intent;
	using NonNull = android.support.annotation.NonNull;
	using Nullable = android.support.annotation.Nullable;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;

	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;
	using Stripe = com.stripe.android.Stripe;
	using Card = com.stripe.android.model.Card;
	using Token = com.stripe.android.model.Token;
	using StripeException = com.stripe.android.exception.StripeException;

	/// <summary>
	/// An <seealso cref="IntentService"/> subclass for handling the creation of a <seealso cref="Token"/> from
	/// input <seealso cref="Card"/> information.
	/// </summary>
	public class TokenIntentService : IntentService
	{

		public const string TOKEN_ACTION = "com.stripe.example.service.tokenAction";
		public const string STRIPE_CARD_LAST_FOUR = "com.stripe.example.service.cardLastFour";
		public const string STRIPE_CARD_TOKEN_ID = "com.stripe.example.service.cardTokenId";
		public const string STRIPE_ERROR_MESSAGE = "com.stripe.example.service.errorMessage";

		private const string EXTRA_CARD_NUMBER = "com.stripe.example.service.extra.cardNumber";
		private const string EXTRA_MONTH = "com.stripe.example.service.extra.month";
		private const string EXTRA_YEAR = "com.stripe.example.service.extra.year";
		private const string EXTRA_CVC = "com.stripe.example.service.extra.cvc";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public static android.content.Intent createTokenIntent(@NonNull android.app.Activity launchingActivity, @Nullable String cardNumber, @Nullable Integer month, @Nullable Integer year, @Nullable String cvc)
		public static Intent createTokenIntent(Activity launchingActivity, string cardNumber, int? month, int? year, string cvc)
		{
			return (new Intent(launchingActivity, typeof(TokenIntentService))).putExtra(EXTRA_CARD_NUMBER, cardNumber).putExtra(EXTRA_MONTH, month).putExtra(EXTRA_YEAR, year).putExtra(EXTRA_CVC, cvc);
		}

		public TokenIntentService() : base("TokenIntentService")
		{
		}

		protected internal override void onHandleIntent(Intent intent)
		{
			string errorMessage = null;
			Token token = null;
			if (intent != null)
			{
				string cardNumber = intent.getStringExtra(EXTRA_CARD_NUMBER);
				int? month = (int?) intent.Extras.get(EXTRA_MONTH);
				int? year = (int?) intent.Extras.get(EXTRA_YEAR);
				string cvc = intent.getStringExtra(EXTRA_CVC);

				Card card = new Card(cardNumber, month, year, cvc);

				Stripe stripe = new Stripe(this);
				try
				{
					token = stripe.createTokenSynchronous(card, PaymentConfiguration.Instance.PublishableKey);
				}
				catch (StripeException stripeEx)
				{
					errorMessage = stripeEx.LocalizedMessage;
				}
			}

			Intent localIntent = new Intent(TOKEN_ACTION);
			if (token != null)
			{
				localIntent.putExtra(STRIPE_CARD_LAST_FOUR, token.Card.Last4);
				localIntent.putExtra(STRIPE_CARD_TOKEN_ID, token.Id);
			}

			if (errorMessage != null)
			{
				localIntent.putExtra(STRIPE_ERROR_MESSAGE, errorMessage);
			}

			// Broadcasts the Intent to receivers in this app.
			LocalBroadcastManager.getInstance(this).sendBroadcast(localIntent);
		}
	}

}