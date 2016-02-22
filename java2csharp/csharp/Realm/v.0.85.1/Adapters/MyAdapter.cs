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

namespace io.realm.examples.realmadapters
{

	using Context = android.content.Context;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using ListAdapter = android.widget.ListAdapter;
	using TextView = android.widget.TextView;

	using TimeStamp = io.realm.examples.realmadapters.models.TimeStamp;

	public class MyAdapter : RealmBaseAdapter<TimeStamp>, ListAdapter
	{

		private class ViewHolder
		{
			internal TextView timestamp;
		}

		public MyAdapter(Context context, int resId, RealmResults<TimeStamp> realmResults, bool automaticUpdate) : base(context, realmResults, automaticUpdate)
		{
		}

		public override View getView(int position, View convertView, ViewGroup parent)
		{
			ViewHolder viewHolder;
			if (convertView == null)
			{
				convertView = inflater.inflate(android.R.layout.simple_list_item_1, parent, false);
				viewHolder = new ViewHolder();
				viewHolder.timestamp = (TextView) convertView.findViewById(android.R.id.text1);
				convertView.Tag = viewHolder;
			}
			else
			{
				viewHolder = (ViewHolder) convertView.Tag;
			}

			TimeStamp item = realmResults.get(position);
			viewHolder.timestamp.Text = item.getTimeStamp();
			return convertView;
		}

		public virtual RealmResults<TimeStamp> RealmResults
		{
			get
			{
				return realmResults;
			}
		}
	}

}