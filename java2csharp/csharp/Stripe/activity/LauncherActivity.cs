namespace com.stripe.example.activity
{

	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using View = android.view.View;
	using Button = android.widget.Button;

	using PaymentConfiguration = com.stripe.android.PaymentConfiguration;

	public class LauncherActivity : AppCompatActivity
	{

		/*
		 * Change this to your publishable key.
		 *
		 * You can get your key here: https://dashboard.stripe.com/account/apikeys
		 */
		private const string PUBLISHABLE_KEY = "put your key here";

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_launcher;

			PaymentConfiguration.init(PUBLISHABLE_KEY);
			Button tokenButton = findViewById(R.id.btn_make_card_tokens);
			tokenButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			Button multilineButton = findViewById(R.id.btn_make_card_sources);
			multilineButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

			Button sourceButton = findViewById(R.id.btn_make_sources);
			sourceButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);

			Button customerSessionButton = findViewById(R.id.btn_customer_session_launch);
			customerSessionButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper4(this);

			Button selectShippingAddressButton = findViewById(R.id.btn_payment_session_launch);
			selectShippingAddressButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper5(this);

			Button payWithGoogleButton = findViewById(R.id.btn_payment_with_google_launch);
			payWithGoogleButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper6(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				Intent intent = new Intent(outerInstance, typeof(PaymentActivity));
				startActivity(intent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				Intent intent = new Intent(outerInstance, typeof(PaymentMultilineActivity));
				startActivity(intent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper3(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				Intent intent = new Intent(outerInstance, typeof(RedirectActivity));
				startActivity(intent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper4(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				Intent intent = new Intent(outerInstance, typeof(CustomerSessionActivity));
				startActivity(intent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper5(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				Intent intent = new Intent(outerInstance, typeof(PaymentSessionActivity));
				startActivity(intent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper6 : View.OnClickListener
		{
			private readonly LauncherActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper6(LauncherActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				Intent intent = new Intent(outerInstance, typeof(PayWithGoogleActivity));
				startActivity(intent);
			}
		}

	}

}