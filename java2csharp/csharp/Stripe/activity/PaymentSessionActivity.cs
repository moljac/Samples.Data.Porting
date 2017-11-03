using System;
using System.Collections.Generic;
using System.Text;

namespace com.stripe.example.activity
{

	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Bundle = android.os.Bundle;
	using NonNull = android.support.annotation.NonNull;
	using Nullable = android.support.annotation.Nullable;
	using LocalBroadcastManager = android.support.v4.content.LocalBroadcastManager;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using View = android.view.View;
	using Button = android.widget.Button;
	using ProgressBar = android.widget.ProgressBar;
	using TextView = android.widget.TextView;

	using CustomerSession = com.stripe.android.CustomerSession;
	using PaymentSession = com.stripe.android.PaymentSession;
	using PaymentSessionConfig = com.stripe.android.PaymentSessionConfig;
	using PaymentSessionData = com.stripe.android.PaymentSessionData;
	using Address = com.stripe.android.model.Address;
	using Customer = com.stripe.android.model.Customer;
	using CustomerSource = com.stripe.android.model.CustomerSource;
	using ShippingInformation = com.stripe.android.model.ShippingInformation;
	using ShippingMethod = com.stripe.android.model.ShippingMethod;
	using Source = com.stripe.android.model.Source;
	using SourceCardData = com.stripe.android.model.SourceCardData;
	using ShippingInfoWidget = com.stripe.android.view.ShippingInfoWidget;
	using ErrorDialogHandler = com.stripe.example.controller.ErrorDialogHandler;
	using ExampleEphemeralKeyProvider = com.stripe.example.service.ExampleEphemeralKeyProvider;


//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.PayWithGoogleUtils.getPriceString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EVENT_SHIPPING_INFO_PROCESSED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EVENT_SHIPPING_INFO_SUBMITTED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EXTRA_DEFAULT_SHIPPING_METHOD;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EXTRA_IS_SHIPPING_INFO_VALID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EXTRA_SHIPPING_INFO_DATA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static com.stripe.android.view.PaymentFlowActivity.EXTRA_VALID_SHIPPING_METHODS;

	/// <summary>
	/// An example activity that handles working with a <seealso cref="com.stripe.android.PaymentSession"/>, allowing you to
	/// collect information needed to request payment for the current customer.
	/// </summary>
	public class PaymentSessionActivity : AppCompatActivity
	{

		private BroadcastReceiver mBroadcastReceiver;
		private Customer mCustomer;
		private ErrorDialogHandler mErrorDialogHandler;
		private PaymentSession mPaymentSession;
		private ProgressBar mProgressBar;
		private TextView mResultTextView;
		private TextView mResultTitleTextView;
		private Button mSelectPaymentButton;
		private Button mSelectShippingButton;
		private PaymentSessionData mPaymentSessionData;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override protected void onCreate(@Nullable android.os.Bundle savedInstanceState)
		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_payment_session;
			mProgressBar = findViewById(R.id.customer_progress_bar);
			mProgressBar.Visibility = View.VISIBLE;
			mSelectPaymentButton = findViewById(R.id.btn_select_payment_method_aps);
			mSelectPaymentButton.Enabled = false;
			mSelectShippingButton = findViewById(R.id.btn_start_payment_flow);
			mSelectShippingButton.Enabled = false;
			mErrorDialogHandler = new ErrorDialogHandler(SupportFragmentManager);
			mResultTitleTextView = findViewById(R.id.tv_payment_session_data_title);
			mResultTextView = findViewById(R.id.tv_payment_session_data);
			setupCustomerSession(); // CustomerSession only needs to be initialized once per app.
			mBroadcastReceiver = new BroadcastReceiverAnonymousInnerClassHelper(this);
			LocalBroadcastManager.getInstance(this).registerReceiver(mBroadcastReceiver, new IntentFilter(EVENT_SHIPPING_INFO_SUBMITTED));
			mSelectPaymentButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
			mSelectShippingButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);

		}

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			private readonly PaymentSessionActivity outerInstance;

			public BroadcastReceiverAnonymousInnerClassHelper(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onReceive(Context context, Intent intent)
			{
				ShippingInformation shippingInformation = intent.getParcelableExtra(EXTRA_SHIPPING_INFO_DATA);
				Intent shippingInfoProcessedIntent = new Intent(EVENT_SHIPPING_INFO_PROCESSED);
				if (shippingInformation.Address == null || !shippingInformation.Address.Country.Equals(Locale.US.Country))
				{
					shippingInfoProcessedIntent.putExtra(EXTRA_IS_SHIPPING_INFO_VALID, false);
				}
				else
				{
					List<ShippingMethod> shippingMethods = outerInstance.createSampleShippingMethods();
					shippingInfoProcessedIntent.putExtra(EXTRA_IS_SHIPPING_INFO_VALID, true);
					shippingInfoProcessedIntent.putParcelableArrayListExtra(EXTRA_VALID_SHIPPING_METHODS, shippingMethods);
					shippingInfoProcessedIntent.putExtra(EXTRA_DEFAULT_SHIPPING_METHOD, shippingMethods[1]);
				}
				LocalBroadcastManager.getInstance(outerInstance).sendBroadcast(shippingInfoProcessedIntent);
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly PaymentSessionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				outerInstance.mPaymentSession.presentPaymentMethodSelection();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly PaymentSessionActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				outerInstance.mPaymentSession.presentShippingFlow();
			}
		}

		private void setupCustomerSession()
		{
			CustomerSession.initCustomerSession(new ExampleEphemeralKeyProvider(new ProgressListenerAnonymousInnerClassHelper(this)));
			CustomerSession.Instance.retrieveCurrentCustomer(new CustomerRetrievalListenerAnonymousInnerClassHelper(this));
		}

		private class ProgressListenerAnonymousInnerClassHelper : ExampleEphemeralKeyProvider.ProgressListener
		{
			private readonly PaymentSessionActivity outerInstance;

			public ProgressListenerAnonymousInnerClassHelper(PaymentSessionActivity outerInstance)
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
			private readonly PaymentSessionActivity outerInstance;

			public CustomerRetrievalListenerAnonymousInnerClassHelper(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onCustomerRetrieved(@NonNull com.stripe.android.model.Customer customer)
			public override void onCustomerRetrieved(Customer customer)
			{
				outerInstance.mCustomer = customer;
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
				outerInstance.setupPaymentSession();
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onError(int errorCode, @Nullable String errorMessage)
			public override void onError(int errorCode, string errorMessage)
			{
				outerInstance.mCustomer = null;
				outerInstance.mSelectPaymentButton.Enabled = false;
				outerInstance.mSelectShippingButton.Enabled = false;
				outerInstance.mErrorDialogHandler.showError(errorMessage);
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
			}
		}

		private void setupPaymentSession()
		{
			mPaymentSession = new PaymentSession(this);
			bool paymentSessionInitialized = mPaymentSession.init(new PaymentSessionListenerAnonymousInnerClassHelper(this), (new PaymentSessionConfig.Builder()).setPrepopulatedShippingInfo(ExampleShippingInfo).setHiddenShippingInfoFields(ShippingInfoWidget.PHONE_FIELD, ShippingInfoWidget.CITY_FIELD).build());
			if (paymentSessionInitialized)
			{
				mSelectPaymentButton.Enabled = true;
				mSelectShippingButton.Enabled = true;
				mPaymentSession.CartTotal = 2000L;
			}
		}

		private class PaymentSessionListenerAnonymousInnerClassHelper : PaymentSession.PaymentSessionListener
		{
			private readonly PaymentSessionActivity outerInstance;

			public PaymentSessionListenerAnonymousInnerClassHelper(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onCommunicatingStateChanged(bool isCommunicating)
			{
				if (isCommunicating)
				{
					outerInstance.mProgressBar.Visibility = View.VISIBLE;
				}
				else
				{
					outerInstance.mProgressBar.Visibility = View.INVISIBLE;
				}
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onError(int errorCode, @Nullable String errorMessage)
			public override void onError(int errorCode, string errorMessage)
			{
				outerInstance.mErrorDialogHandler.showError(errorMessage);
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onPaymentSessionDataChanged(@NonNull com.stripe.android.PaymentSessionData data)
			public override void onPaymentSessionDataChanged(PaymentSessionData data)
			{
				outerInstance.mPaymentSessionData = data;
				outerInstance.checkForCustomerUpdates();
			}
		}

		private void checkForCustomerUpdates()
		{
			mProgressBar.Visibility = View.VISIBLE;
			CustomerSession.Instance.retrieveCurrentCustomer(new CustomerRetrievalListenerAnonymousInnerClassHelper2(this));
		}

		private class CustomerRetrievalListenerAnonymousInnerClassHelper2 : CustomerSession.CustomerRetrievalListener
		{
			private readonly PaymentSessionActivity outerInstance;

			public CustomerRetrievalListenerAnonymousInnerClassHelper2(PaymentSessionActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onCustomerRetrieved(@NonNull com.stripe.android.model.Customer customer)
			public override void onCustomerRetrieved(Customer customer)
			{
				outerInstance.mCustomer = customer;
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
				if (outerInstance.mPaymentSessionData != null)
				{
					outerInstance.mResultTitleTextView.Visibility = View.VISIBLE;
					outerInstance.mResultTextView.Text = outerInstance.formatStringResults(outerInstance.mPaymentSessionData);
				}
			}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override public void onError(int errorCode, @Nullable String errorMessage)
			public override void onError(int errorCode, string errorMessage)
			{
				outerInstance.mProgressBar.Visibility = View.INVISIBLE;
			}
		}

		private string formatStringResults(PaymentSessionData data)
		{
			Currency currency = Currency.getInstance("USD");
			StringBuilder stringBuilder = new StringBuilder();

			if (data.SelectedPaymentMethodId != null && mCustomer != null)
			{
				CustomerSource source = mCustomer.getSourceById(data.SelectedPaymentMethodId);
				if (source != null)
				{
					Source cardSource = source.asSource();
					stringBuilder.Append("Payment Info:\n");
					if (cardSource != null)
					{
						SourceCardData scd = (SourceCardData) cardSource.SourceTypeModel;
						stringBuilder.Append(scd.Brand).Append(" ending in ").Append(scd.Last4);
					}
					else
					{
						stringBuilder.Append('\n').Append(source.ToString()).Append('\n');
					}
					string isOrNot = data.PaymentReadyToCharge ? " IS " : " IS NOT ";
					stringBuilder.Append(isOrNot).Append("ready to charge.\n\n");
				}
			}
			if (data.ShippingInformation != null)
			{
				stringBuilder.Append("Shipping Info: \n");
				stringBuilder.Append(data.ShippingInformation);
				stringBuilder.Append("\n\n");
			}
			if (data.ShippingMethod != null)
			{
				stringBuilder.Append("Shipping Method: \n");
				stringBuilder.Append(data.ShippingMethod).Append('\n');
				if (data.ShippingTotal > 0)
				{
					stringBuilder.Append("Shipping total: ").Append(getPriceString(data.ShippingTotal, currency));
				}
			}

			return stringBuilder.ToString();
		}

		private List<ShippingMethod> createSampleShippingMethods()
		{
			List<ShippingMethod> shippingMethods = new List<ShippingMethod>();
			shippingMethods.Add(new ShippingMethod("UPS Ground", "ups-ground", "Arrives in 3-5 days", 0, "USD"));
			shippingMethods.Add(new ShippingMethod("FedEx", "fedex", "Arrives tomorrow", 599, "USD"));
			return shippingMethods;
		}

		protected internal override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			base.onActivityResult(requestCode, resultCode, data);
			mPaymentSession.handlePaymentData(requestCode, resultCode, data);
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			mPaymentSession.onDestroy();
			LocalBroadcastManager.getInstance(this).unregisterReceiver(mBroadcastReceiver);
		}

		private ShippingInformation ExampleShippingInfo
		{
			get
			{
				Address address = (new Address.Builder()).setCity("San Francisco").setCountry("US").setLine1("123 Market St").setLine2("#345").setPostalCode("94107").setState("CA").build();
				return new ShippingInformation(address, "Fake Name", "(555) 555-5555");
			}
		}
	}

}