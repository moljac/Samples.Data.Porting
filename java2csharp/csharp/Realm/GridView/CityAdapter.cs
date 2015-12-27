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

	using Context = android.content.Context;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using TextView = android.widget.TextView;

	// This adapter is strictly to interface with the GridView and doesn't
	// particular show much interesting Realm functionality.

	// Alternatively from this example,
	// a developer could update the getView() to pull items from the Realm.

	public class CityAdapter : BaseAdapter
	{

		private LayoutInflater inflater;

		private IList<City> cities = null;

		public CityAdapter(Context context)
		{
			inflater = (LayoutInflater) context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		}

		public virtual IList<City> Data
		{
			set
			{
				this.cities = value;
			}
		}

		public override int Count
		{
			get
			{
				if (cities == null)
				{
					return 0;
				}
				return cities.Count;
			}
		}

		public override object getItem(int position)
		{
			if (cities == null || cities[position] == null)
			{
				return null;
			}
			return cities[position];
		}

		public override long getItemId(int i)
		{
			return i;
		}

		public override View getView(int position, View currentView, ViewGroup parent)
		{
			if (currentView == null)
			{
				currentView = inflater.inflate(R.layout.city_listitem, parent, false);
			}

			City city = cities[position];

			if (city != null)
			{
				((TextView) currentView.findViewById(R.id.name)).Text = city.Name;
				((TextView) currentView.findViewById(R.id.votes)).Text = Convert.ToString(city.Votes);
			}

			return currentView;
		}
	}

}