using System.Collections.Generic;

/*
 * Copyright 2014 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace io.realm.examples.json
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using GridView = android.widget.GridView;

	using JSONObject = org.json.JSONObject;



	/// <summary>
	/// This example demonstrates how to import RealmObjects as JSON. Realm supports JSON represented
	/// as Strings, JSONObject, JSONArray or InputStreams (from API 11+)
	/// </summary>
	public class JsonExampleActivity : Activity
	{

		private GridView mGridView;
		private CityAdapter mAdapter;
		private Realm realm;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_realm_example;

			RealmConfiguration realmConfiguration = (new RealmConfiguration.Builder(this)).build();
			Realm.deleteRealm(realmConfiguration);
			realm = Realm.getInstance(realmConfiguration);
		}

		public override void onResume()
		{
			base.onResume();

			// Load from file "cities.json" first time
			if (mAdapter == null)
			{
				IList<City> cities = null;
				try
				{
					cities = loadCities();
				}
				catch (IOException e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}

				//This is the GridView adapter
				mAdapter = new CityAdapter(this);
				mAdapter.Data = cities;

				//This is the GridView which will display the list of cities
				mGridView = (GridView) findViewById(R.id.cities_list);
				mGridView.Adapter = mAdapter;
				mAdapter.notifyDataSetChanged();
				mGridView.invalidate();
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			realm.close();
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public java.util.List<City> loadCities() throws java.io.IOException
		public virtual IList<City> loadCities()
		{

			loadJsonFromStream();
			loadJsonFromJsonObject();
			loadJsonFromString();

			return realm.allObjects(typeof(City));
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private void loadJsonFromStream() throws java.io.IOException
		private void loadJsonFromStream()
		{
			// Use streams if you are worried about the size of the JSON whether it was persisted on disk
			// or received from the network.
			System.IO.Stream stream = Assets.open("cities.json");

			// Open a transaction to store items into the realm
			realm.beginTransaction();
			try
			{
				realm.createAllFromJson(typeof(City), stream);
				realm.commitTransaction();
			}
			catch (IOException)
			{
				// Remember to cancel the transaction if anything goes wrong.
				realm.cancelTransaction();
			}
			finally
			{
				if (stream != null)
				{
					stream.Close();
				}
			}
		}

		private void loadJsonFromJsonObject()
		{
			IDictionary<string, string> city = new Dictionary<string, string>();
			city["name"] = "København";
			city["votes"] = "9";
			JSONObject json = new JSONObject(city);

			realm.beginTransaction();
			realm.createObjectFromJson(typeof(City), json);
			realm.commitTransaction();
		}

		private void loadJsonFromString()
		{
			string json = "{ city: \"Aarhus\", votes: 99 }";

			realm.beginTransaction();
			realm.createObjectFromJson(typeof(City), json);
			realm.commitTransaction();
		}
	}

}