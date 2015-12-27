using System;
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

namespace io.realm.examples.realmgridview
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using GridView = android.widget.GridView;

	using ExclusionStrategy = com.google.gson.ExclusionStrategy;
	using FieldAttributes = com.google.gson.FieldAttributes;
	using Gson = com.google.gson.Gson;
	using GsonBuilder = com.google.gson.GsonBuilder;
	using JsonElement = com.google.gson.JsonElement;
	using JsonParser = com.google.gson.JsonParser;
	using TypeToken = com.google.gson.reflect.TypeToken;



	public class GridViewExampleActivity : Activity, AdapterView.OnItemClickListener
	{

		private GridView mGridView;
		private CityAdapter mAdapter;

		private Realm realm;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_realm_example;

			RealmConfiguration realmConfiguration = (new RealmConfiguration.Builder(this)).build();

			// Clear the realm from last time
			Realm.deleteRealm(realmConfiguration);

			// Create a new empty instance of Realm
			realm = Realm.getInstance(realmConfiguration);
		}

		public override void onResume()
		{
			base.onResume();

			// Load from file "cities.json" first time
			if (mAdapter == null)
			{
				IList<City> cities = loadCities();

				//This is the GridView adapter
				mAdapter = new CityAdapter(this);
				mAdapter.Data = cities;

				//This is the GridView which will display the list of cities
				mGridView = (GridView) findViewById(R.id.cities_list);
				mGridView.Adapter = mAdapter;
				mGridView.OnItemClickListener = GridViewExampleActivity.this;
				mAdapter.notifyDataSetChanged();
				mGridView.invalidate();
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			realm.close(); // Remember to close Realm when done.
		}

		private IList<City> loadCities()
		{
			// In this case we're loading from local assets.
			// NOTE: could alternatively easily load from network
			System.IO.Stream stream;
			try
			{
				stream = Assets.open("cities.json");
			}
			catch (IOException)
			{
				return null;
			}

			// GSON can parse the data.
			// Note there is a bug in GSON 2.3.1 that can cause it to StackOverflow when working with RealmObjects.
			// To work around this, use the ExclusionStrategy below or downgrade to 1.7.1
			// See more here: https://code.google.com/p/google-gson/issues/detail?id=440
			Gson gson = (new GsonBuilder()).setExclusionStrategies(new ExclusionStrategyAnonymousInnerClassHelper(this))
				   .create();

			JsonElement json = (new JsonParser()).parse(new System.IO.StreamReader(stream));
			IList<City> cities = gson.fromJson(json, (new TypeTokenAnonymousInnerClassHelper(this)).Type);

			// Open a transaction to store items into the realm
			// Use copyToRealm() to convert the objects into proper RealmObjects managed by Realm.
			realm.beginTransaction();
			ICollection<City> realmCities = realm.copyToRealm(cities);
			realm.commitTransaction();

			return new List<City>(realmCities);
		}

		private class ExclusionStrategyAnonymousInnerClassHelper : ExclusionStrategy
		{
			private readonly GridViewExampleActivity outerInstance;

			public ExclusionStrategyAnonymousInnerClassHelper(GridViewExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override bool shouldSkipField(FieldAttributes f)
			{
				return f.DeclaringClass.Equals(typeof(RealmObject));
			}

			public override bool shouldSkipClass(Type clazz)
			{
				return false;
			}
		}

		private class TypeTokenAnonymousInnerClassHelper : TypeToken<IList<City>>
		{
			private readonly GridViewExampleActivity outerInstance;

			public TypeTokenAnonymousInnerClassHelper(GridViewExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

		}

		public virtual void updateCities()
		{
			// Pull all the cities from the realm
			RealmResults<City> cities = realm.@where(typeof(City)).findAll();

			// Put these items in the Adapter
			mAdapter.Data = cities;
			mAdapter.notifyDataSetChanged();
			mGridView.invalidate();
		}

		public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			City modifiedCity = (City)mAdapter.getItem(position);

			// Acquire the list of realm cities matching the name of the clicked City.
			City city = realm.@where(typeof(City)).equalTo("name", modifiedCity.Name).findFirst();

			// Create a transaction to increment the vote count for the selected City in the realm
			realm.beginTransaction();
			city.Votes = city.Votes + 1;
			realm.commitTransaction();

			updateCities();
		}
	}

}