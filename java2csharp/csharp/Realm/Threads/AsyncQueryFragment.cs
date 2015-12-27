using System;
using System.Collections.Generic;

/*
 * Copyright 2015 Realm Inc.
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

namespace io.realm.examples.threads
{

	using Context = android.content.Context;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;
	using ListView = android.widget.ListView;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	using Dot = io.realm.examples.threads.model.Dot;

	/// <summary>
	/// This fragment demonstrates how you can perform asynchronous queries with Realm.
	/// </summary>
	public class AsyncQueryFragment : Fragment, View.OnClickListener, RealmChangeListener
	{
		private Realm realm;
		private DotAdapter dotAdapter;
		private RealmResults<Dot> allSortedDots;
		private RealmAsyncTask asyncTransaction;

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.inflate(R.layout.fragment_async_query, container, false);
			rootView.findViewById(R.id.translate_button).OnClickListener = this;

			ListView listView = (ListView) rootView.findViewById(android.R.id.list);
			dotAdapter = new DotAdapter(Activity);
			listView.Adapter = dotAdapter;
			return rootView;
		}

		public override void onStart()
		{
			base.onStart();
			// Create Realm instance for the UI thread
			realm = Realm.DefaultInstance;
			allSortedDots = realm.@where(typeof(Dot)).between("x", 25, 75).between("y", 0, 50).findAllSortedAsync("x", RealmResults.SORT_ORDER_ASCENDING, "y", RealmResults.SORT_ORDER_DESCENDING);
			dotAdapter.updateList(allSortedDots);
			allSortedDots.addChangeListener(this);
		}

		public override void onStop()
		{
			base.onStop();
			// Remember to close the Realm instance when done with it.
			cancelAsyncTransaction();
			allSortedDots.removeChangeListener(this);
			allSortedDots = null;
			realm.close();
		}

		public override void onClick(View view)
		{
			switch (view.Id)
			{
				case R.id.translate_button:
				{
					cancelAsyncTransaction();
					// translate all points coordinates using an async transaction
//JAVA TO C# CONVERTER TODO TASK: The following anonymous inner class could not be converted:
//					asyncTransaction = realm.executeTransaction(new io.realm.Realm.Transaction()
	//				{
	//					@@Override public void execute(Realm realm)
	//					{
	//						// query for all points
	//						RealmResults<Dot> dots = realm.@where(Dot.class).findAll();
	//
	//						for (int i = dots.size() - 1; i >= 0; i--)
	//						{
	//							Dot dot = dots.get(i);
	//							if (dot.isValid())
	//							{
	//								int x = dot.getX();
	//								int y = dot.getY();
	//								dot.setX(y);
	//								dot.setY(x);
	//							}
	//						}
	//					}
	//				}
				   , new CallbackAnonymousInnerClassHelper(this)
				   );
					break;
				}
			}
		}

		private class CallbackAnonymousInnerClassHelper : Realm.Transaction.Callback
		{
			private readonly AsyncQueryFragment outerInstance;

			public CallbackAnonymousInnerClassHelper(AsyncQueryFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onSuccess()
			{
				if (Added)
				{
					Toast.makeText(Activity, "Translation completed", Toast.LENGTH_SHORT).show();
				}
			}

			public override void onError(Exception e)
			{
				if (Added)
				{
					Toast.makeText(Activity, "Error while translating dots", Toast.LENGTH_SHORT).show();
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

		private void cancelAsyncTransaction()
		{
			if (asyncTransaction != null && !asyncTransaction.Cancelled)
			{
				asyncTransaction.cancel();
				asyncTransaction = null;
			}
		}

		public override void onChange()
		{
			dotAdapter.notifyDataSetChanged();
		}

		// Using a generic Adapter instead of RealmBaseAdapter, because
		// RealmBaseAdapter registers a listener against all Realm changes
		// whereas in this scenario we're only interested on the changes of our query
		private class DotAdapter : BaseAdapter
		{
			internal IList<Dot> dots = Collections.emptyList();
			internal readonly LayoutInflater inflater;

			internal DotAdapter(Context context)
			{
				this.inflater = LayoutInflater.from(context);
			}

			internal virtual void updateList(RealmResults<Dot> dots)
			{
				this.dots = dots;
				notifyDataSetChanged();
			}

			public override int Count
			{
				get
				{
					return dots.Count;
				}
			}

			public override Dot getItem(int i)
			{
				return dots[i];
			}

			public override long getItemId(int i)
			{
				return i;
			}

			public override View getView(int i, View view, ViewGroup viewGroup)
			{
				if (view == null)
				{
					view = inflater.inflate(android.R.layout.simple_list_item_1, viewGroup, false);
					ViewHolder viewHolder = new ViewHolder(this, view);
					view.Tag = viewHolder;
				}
				ViewHolder vh = (ViewHolder) view.Tag;
				vh.text.Text = "[X= " + getItem(i).X + " Y= " + getItem(i).Y + "]";

				return view;
			}

			private class ViewHolder
			{
				private readonly AsyncQueryFragment.DotAdapter outerInstance;

				internal TextView text;

				internal ViewHolder(AsyncQueryFragment.DotAdapter outerInstance, View view)
				{
					this.outerInstance = outerInstance;
					text = (TextView) view.findViewById(android.R.id.text1);
				}
			}
		}
	}

}