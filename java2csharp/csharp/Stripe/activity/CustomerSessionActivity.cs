using System;

namespace com.stripe.example.activity
{

	using Intent = android.content.Intent;
	using NonNull = android.support.annotation.NonNull;
	using Nullable = android.support.annotation.Nullable;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using ProgressBar = android.widget.ProgressBar;
	using TextView = android.widget.TextView;

	using CustomerSession = com.stripe.android.CustomerSession;
	using Customer = com.stripe.android.model.Customer;
	using Source = com.stripe.android.model.Source;
	using SourceCardData = com.stripe.android.model.SourceCardData;
	using PaymentMethodsActivity = com.stripe.android.view.PaymentMethodsActivity;
	using ErrorDialogHandler = com.stripe.example.controller.ErrorDialogHandler;
	using ExampleEphemeralKeyProvider = com.stripe.example.service.ExampleEphemeralKeyProvider;

	/// <summary>
	/// An example activity that handles working with a <seealso cref="CustomerSession"/>, allowing you to
	/// add and select sources for the current customer.
	/// </summary>
	public class CustomerSessionActivity : AppCompatActivity
	{

		private const int REQUEST_CODE_SELECT_SOURCE = 55;

		private Button mSelectSourceButton;
		private TextView mSelectedSourceTextView;
		private ProgressBar mProgressBar;
		private ErrorDialogHandler mErrorDialogHandler;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_customer_session;
			Title = R.@string.customer_payment_data_example;
			mProgressBar = findViewById(R.id.customer_progress_bar);
			mSelectedSourceTextView = findViewById(R.id.tv_customer_default_source_acs);
			mSelectSourceButton = findViewById(R.id.btn_launch_payment_methods_acs);
			mSelectSourceButton.Enabled = false;
			mErrorDialogHandler = new ErrorDialogHandler(SupportFragmentManager);
			CustomerSession.initCustomerSession(new ExampleEphemeralKeyProvider(new ProgressListenerAnonymousInnerClassHelper(this)));

			mProgressBar.Visibility = View.VISIBLE;
			CustomerSession.Instance.retrieveCurrentCustomer(new CustomerRetrievalListenerAnonymousInnerClassHelper(this));

			mSelectSourceButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class ProgressListenerAnonymousInnerClassHelper : ExampleEphemeralKeyProvider.ProgressListener
		{
			private readonly CustomerSessionActivity outerInstance;

			public ProgressListenerAnonymousInnerClassHelper(CustomerSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onStringResponse(string @string)
			{
				if (@string.StartsWith("Error: ", StringComparison.Ordinal))
				{
					outerInstance.mErrorDialogHandler.showError(@string);
				}
			}
		}

		private class CustomerRetrievalListenerAnonymousInnerClassHelper : CustomerSession.CustomerRetrievalListener
		{
			private readonly CustomerSessionActivity outerInstance;

			public CustomerRetrievalListenerAnonymousInnerClassHelper(CustomerSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onCustomerRetrieved(@NonNull com.stripe.android.model.Customer customer)
			public override void onCustomerRetrieved(Customer customer)
			{
				outerInstance.mSelectSourceButton.Enabled = true;
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onError(int errorCode, @Nullable String errorMessage)
			public override void onError(int errorCode, string errorMessage)
			{
				outerInstance.mSelectSourceButton.Enabled = false;
				outerInstance.mErrorDialogHandler.showError(errorMessage);
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly CustomerSessionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(CustomerSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.launchWithCustomer();
			}
		}

		private void launchWithCustomer()
		{
			Intent payIntent = PaymentMethodsActivity.newIntent(this);
			startActivityForResult(payIntent, REQUEST_CODE_SELECT_SOURCE);
		}

		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			if (requestCode == REQUEST_CODE_SELECT_SOURCE && resultCode == RESULT_OK)
			{
				string selectedSource = data.getStringExtra(PaymentMethodsActivity.EXTRA_SELECTED_PAYMENT);
				Source source = Source.fromString(selectedSource);
				// Note: it isn't possible for a null or non-card source to be returned.
				if (source != null && Source.CARD.Equals(source.Type))
				{
					SourceCardData cardData = (SourceCardData) source.SourceTypeModel;
					mSelectedSourceTextView.Text = buildCardString(cardData);
				}
			}
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: private String buildCardString(@NonNull com.stripe.android.model.SourceCardData data)
		private string buildCardString(SourceCardData data)
		{
			return data.Brand + getString(R.@string.ending_in) + data.Last4;
		}
	}

}