using System.Collections.Generic;

namespace com.sinch.verification.sample
{

	using Context = android.content.Context;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Spinner = android.widget.Spinner;


	/* A class for initializing a spinner with country names list, setting the default country
	   as the first in the list, and providing the country 2-digit ISO code on selection.
	* */
	public class CountrySpinner : Spinner
	{
		private IDictionary<string, string> mCountries = new SortedDictionary<string, string>();
		private IList<CountryIsoSelectedListener> mListeners = new List<CountryIsoSelectedListener>();

		public interface CountryIsoSelectedListener
		{
			void onCountryIsoSelected(string iso);
		}

		public CountrySpinner(Context context) : base(context)
		{
		}

		public CountrySpinner(Context context, AttributeSet attrs) : base(context, attrs)
		{
		}

		public virtual void init(string defaultCountry)
		{
			initCountries();
			IList<string> countryList = new List<string>();

			((List<string>)countryList).AddRange(mCountries.Keys);
			countryList.Remove(defaultCountry);
			countryList.Insert(0, defaultCountry);

			ArrayAdapter adapter = new ArrayAdapter<string>(Context, android.R.layout.simple_spinner_item, countryList);

			Adapter = adapter;

			OnItemSelectedListener = new OnItemSelectedListenerAnonymousInnerClassHelper(this);
		}

		private class OnItemSelectedListenerAnonymousInnerClassHelper : AdapterView.OnItemSelectedListener
		{
			private readonly CountrySpinner outerInstance;

			public OnItemSelectedListenerAnonymousInnerClassHelper(CountrySpinner outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onItemSelected<T1>(AdapterView<T1> adapterView, View view, int position, long id)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String selectedCountry = (String) adapterView.getItemAtPosition(position);
				string selectedCountry = (string) adapterView.getItemAtPosition(position);
				outerInstance.notifyListeners(selectedCountry);
			}

			public override void onNothingSelected<T1>(AdapterView<T1> adapterView)
			{
			}
		}

		public virtual void addCountryIsoSelectedListener(CountryIsoSelectedListener listener)
		{
			mListeners.Add(listener);
		}

		public virtual void removeCountryIsoSelectedListener(CountryIsoSelectedListener listener)
		{
			mListeners.Remove(listener);
		}

		private void initCountries()
		{
			string[] isoCountryCodes = Locale.ISOCountries;
			foreach (string iso in isoCountryCodes)
			{
				string country = (new Locale("", iso)).DisplayCountry;
				mCountries[country] = iso;
			}
		}

		private void notifyListeners(string selectedCountry)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String selectedIso = mCountries.get(selectedCountry);
			string selectedIso = mCountries[selectedCountry];
			foreach (CountryIsoSelectedListener listener in mListeners)
			{
				listener.onCountryIsoSelected(selectedIso);
			}
		}
	}

}