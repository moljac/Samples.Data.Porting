using System;

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

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Message = android.os.Message;
	using Menu = android.view.Menu;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ListView = android.widget.ListView;

	using TimeStamp = io.realm.examples.realmadapters.models.TimeStamp;


	public class AdapterExampleActivity : Activity
	{

		private Realm realm;
		private WorkerThread workerThread;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_my;

			RealmConfiguration realmConfig = (new RealmConfiguration.Builder(this)).build();
			Realm.deleteRealm(realmConfig);
			realm = Realm.getInstance(realmConfig);

			RealmResults<TimeStamp> timeStamps = realm.@where(typeof(TimeStamp)).findAll();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final MyAdapter adapter = new MyAdapter(this, R.id.listView, timeStamps, true);
			MyAdapter adapter = new MyAdapter(this, R.id.listView, timeStamps, true);
			ListView listView = (ListView) findViewById(R.id.listView);
			listView.Adapter = adapter;
			listView.OnItemLongClickListener = new OnItemLongClickListenerAnonymousInnerClassHelper(this, adapter);
		}

		private class OnItemLongClickListenerAnonymousInnerClassHelper : AdapterView.OnItemLongClickListener
		{
			private readonly AdapterExampleActivity outerInstance;

			private io.realm.examples.realmadapters.MyAdapter adapter;

			public OnItemLongClickListenerAnonymousInnerClassHelper(AdapterExampleActivity outerInstance, io.realm.examples.realmadapters.MyAdapter adapter)
			{
				this.outerInstance = outerInstance;
				this.adapter = adapter;
			}

			public override bool onItemLongClick<T1>(AdapterView<T1> adapterView, View view, int i, long l)
			{
				TimeStamp timeStamp = adapter.RealmResults.get(i);
				Message message = buildMessage(WorkerHandler.REMOVE_TIMESTAMP, timeStamp.getTimeStamp());

				outerInstance.workerThread.workerHandler.sendMessage(message);
				return true;
			}
		}

		protected internal override void onPause()
		{
			base.onPause();
			workerThread.workerHandler.Looper.quit();
		}

		protected internal override void onResume()
		{
			base.onResume();
			workerThread = new WorkerThread(this);
			workerThread.Start();
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			realm.close(); // Remember to close Realm when done.
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			MenuInflater.inflate(R.menu.my, menu);
			return true;
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{
			int id = item.ItemId;
			if (id == R.id.action_add)
			{
				Message message = buildMessage(WorkerHandler.ADD_TIMESTAMP, Convert.ToString(DateTimeHelperClass.CurrentUnixTimeMillis()));
				workerThread.workerHandler.sendMessage(message);
			}
			return true;
		}

		private static Message buildMessage(int action, string timeStamp)
		{
			Bundle bundle = new Bundle(2);
			bundle.putInt(WorkerHandler.ACTION, action);
			bundle.putString(WorkerHandler.TIMESTAMP, timeStamp);
			Message message = new Message();
			message.Data = bundle;
			return message;
		}
	}

}