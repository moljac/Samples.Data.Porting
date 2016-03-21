namespace com.sinch.verification.sample
{

	using Manifest = android.Manifest;
	using Activity = android.app.Activity;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using PackageManager = android.content.pm.PackageManager;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using ActivityCompat = android.support.v4.app.ActivityCompat;
	using TelephonyManager = android.telephony.TelephonyManager;
	using Editable = android.text.Editable;
	using TextWatcher = android.text.TextWatcher;
	using View = android.view.View;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using Toast = android.widget.Toast;


	public class MainActivity : Activity
	{

		public const string SMS = "sms";
		public const string FLASHCALL = "flashcall";
		public const string INTENT_PHONENUMBER = "phonenumber";
		public const string INTENT_METHOD = "method";

		private EditText mPhoneNumber;
		private Button mSmsButton;
		private Button mFlashCallButton;
		private string mCountryIso;
		private TextWatcher mNumberTextWatcher;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_main;

			mPhoneNumber = (EditText) findViewById(R.id.phoneNumber);
			mSmsButton = (Button) findViewById(R.id.smsVerificationButton);
			mFlashCallButton = (Button) findViewById(R.id.callVerificationButton);

			mCountryIso = PhoneNumberUtils.getDefaultCountryIso(this);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String defaultCountryName = new java.util.Locale("", mCountryIso).getDisplayName();
			string defaultCountryName = (new Locale("", mCountryIso)).DisplayName;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final CountrySpinner spinner = (CountrySpinner) findViewById(R.id.spinner);
			CountrySpinner spinner = (CountrySpinner) findViewById(R.id.spinner);
			spinner.init(defaultCountryName);
			spinner.addCountryIsoSelectedListener(new CountryIsoSelectedListenerAnonymousInnerClassHelper(this));
			resetNumberTextWatcher(mCountryIso);

			tryAndPrefillPhoneNumber();
		}

		private class CountryIsoSelectedListenerAnonymousInnerClassHelper : CountrySpinner.CountryIsoSelectedListener
		{
			private readonly MainActivity outerInstance;

			public CountryIsoSelectedListenerAnonymousInnerClassHelper(MainActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onCountryIsoSelected(string selectedIso)
			{
				if (selectedIso != null)
				{
					outerInstance.mCountryIso = selectedIso;
					outerInstance.resetNumberTextWatcher(outerInstance.mCountryIso);
					// force update:
					outerInstance.mNumberTextWatcher.afterTextChanged(outerInstance.mPhoneNumber.Text);
				}
			}
		}

		private void tryAndPrefillPhoneNumber()
		{
			if (checkCallingOrSelfPermission(Manifest.permission.READ_PHONE_STATE) == PackageManager.PERMISSION_GRANTED)
			{
				TelephonyManager manager = (TelephonyManager) getSystemService(Context.TELEPHONY_SERVICE);
				mPhoneNumber.Text = manager.Line1Number;
			}
			else
			{
				ActivityCompat.requestPermissions(this, new string[]{Manifest.permission.READ_PHONE_STATE}, 0);
			}
		}

		public virtual void onRequestPermissionsResult(int requestCode, string[] permissions, int[] grantResults)
		{
			if (grantResults.Length > 0 && grantResults[0] == PackageManager.PERMISSION_GRANTED)
			{
				tryAndPrefillPhoneNumber();
			}
			else
			{
				if (ActivityCompat.shouldShowRequestPermissionRationale(this, permissions[0]))
				{
					Toast.makeTextuniquetempvar.show();
				}
			}
		}

		private void openActivity(string phoneNumber, string method)
		{
			Intent verification = new Intent(this, typeof(VerificationActivity));
			verification.putExtra(INTENT_PHONENUMBER, phoneNumber);
			verification.putExtra(INTENT_METHOD, method);
			startActivity(verification);
		}

		private bool ButtonsEnabled
		{
			set
			{
				mSmsButton.Enabled = value;
				mFlashCallButton.Enabled = value;
			}
		}

		public virtual void onButtonClicked(View view)
		{
			if (view == mSmsButton)
			{
				openActivity(E164Number, SMS);
			}
			else if (view == mFlashCallButton)
			{
				openActivity(E164Number, FLASHCALL);
			}
		}

		private void resetNumberTextWatcher(string countryIso)
		{

			if (mNumberTextWatcher != null)
			{
				mPhoneNumber.removeTextChangedListener(mNumberTextWatcher);
			}

			mNumberTextWatcher = new PhoneNumberFormattingTextWatcherAnonymousInnerClassHelper(this, countryIso);

			mPhoneNumber.addTextChangedListener(mNumberTextWatcher);
		}

		private class PhoneNumberFormattingTextWatcherAnonymousInnerClassHelper : PhoneNumberFormattingTextWatcher
		{
			private readonly MainActivity outerInstance;

			public PhoneNumberFormattingTextWatcherAnonymousInnerClassHelper(MainActivity outerInstance, string countryIso) : base(countryIso)
			{
				this.outerInstance = outerInstance;
			}

			public override void onTextChanged(CharSequence s, int start, int before, int count)
			{
				base.onTextChanged(s, start, before, count);
			}

			public override void beforeTextChanged(CharSequence s, int start, int count, int after)
			{
				base.beforeTextChanged(s, start, count, after);
			}

			public override void afterTextChanged(Editable s)
			{
				lock (this)
				{
					base.afterTextChanged(s);
					if (outerInstance.PossiblePhoneNumber)
					{
						outerInstance.ButtonsEnabled = true;
						outerInstance.mPhoneNumber.TextColor = Color.BLACK;
					}
					else
					{
						outerInstance.ButtonsEnabled = false;
						outerInstance.mPhoneNumber.TextColor = Color.RED;
					}
				}
			}
		}

		private bool PossiblePhoneNumber
		{
			get
			{
				return PhoneNumberUtils.isPossibleNumber(mPhoneNumber.Text.ToString(), mCountryIso);
			}
		}

		private string E164Number
		{
			get
			{
				return PhoneNumberUtils.formatNumberToE164(mPhoneNumber.Text.ToString(), mCountryIso);
			}
		}
	}

}