using System;
using System.Threading;

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

namespace io.realm.examples.threads
{

	using Bundle = android.os.Bundle;
	using SystemClock = android.os.SystemClock;
	using Fragment = android.support.v4.app.Fragment;
	using LayoutInflater = android.view.LayoutInflater;
	using Menu = android.view.Menu;
	using MenuInflater = android.view.MenuInflater;
	using MenuItem = android.view.MenuItem;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;

	using Dot = io.realm.examples.threads.model.Dot;
	using DotsView = io.realm.examples.threads.widget.DotsView;

	/// <summary>
	/// This fragment demonstrates how Realm can interact with a background thread.
	/// </summary>
	public class ThreadFragment : Fragment
	{

		private Realm realm;
		private Random random = new Random();
		private Thread backgroundThread;
		private DotsView dotsView;

		// Realm change listener that refreshes the UI when there is changes to Realm.
		private RealmChangeListener realmListener = new RealmChangeListenerAnonymousInnerClassHelper();

		private class RealmChangeListenerAnonymousInnerClassHelper : RealmChangeListener
		{
			public RealmChangeListenerAnonymousInnerClassHelper()
			{
			}

			public override void onChange()
			{
				outerInstance.dotsView.invalidate();
			}
		}

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			HasOptionsMenu = true;
		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.inflate(R.layout.fragment_thread, container, false);
			dotsView = (DotsView) rootView.findViewById(R.id.dots);
			return rootView;
		}

		public override void onCreateOptionsMenu(Menu menu, MenuInflater inflater)
		{
			base.onCreateOptionsMenu(menu, inflater);
			inflater.inflate(R.menu.menu_backgroundthread, menu);
		}

		public override bool onOptionsItemSelected(MenuItem item)
		{

			switch (item.ItemId)
			{
				case R.id.action_add_dot:
					// Add blue dot from the UI thread
					realm.beginTransaction();
					Dot dot = realm.createObject(typeof(Dot));
					dot.X = random.Next(100);
					dot.Y = random.Next(100);
					dot.Color = Resources.getColor(R.color.realm_blue);
					realm.commitTransaction();
					return true;

				case R.id.action_clear:
					realm.beginTransaction();
					realm.clear(typeof(Dot));
					realm.commitTransaction();
					return true;

				default:
					return base.onOptionsItemSelected(item);
			}
		}

		public override void onStart()
		{
			base.onStart();
			// Create Realm instance for the UI thread
			realm = Realm.DefaultInstance;

			// Create a RealmQuery on the UI thread and send the results to the custom view. The
			// RealmResults will automatically be updated whenever the Realm data is changed.
			// We still need to invalidate the UI to show the changes however. See the RealmChangeListener.
			//
			// Note that the query gets updated by rerunning it on the thread it was
			// created. This can negatively effect frame rates if it is a complicated query or a very
			// large data set.
			dotsView.RealmResults = realm.allObjects(typeof(Dot));
		}

		public override void onResume()
		{
			base.onResume();

			// Enable UI refresh while the fragment is active.
			realm.addChangeListener(realmListener);

			// Create background thread that add a new dot every 0.5 second.
			backgroundThread = new ThreadAnonymousInnerClassHelper(this);
			backgroundThread.Start();
		}

		private class ThreadAnonymousInnerClassHelper : System.Threading.Thread
		{
			private readonly ThreadFragment outerInstance;

			public ThreadAnonymousInnerClassHelper(ThreadFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void run()
			{
				// Realm instances cannot be shared between threads, so we need to create a new
				// instance on the background thread.
				int redColor = Resources.getColor(R.color.realm_red);
				Realm backgroundThreadRealm = Realm.getInstance(Activity);
				while (!outerInstance.backgroundThread.Interrupted)
				{
					backgroundThreadRealm.beginTransaction();

					// Add red dot from the background thread
					Dot dot = backgroundThreadRealm.createObject(typeof(Dot));
					dot.X = outerInstance.random.Next(100);
					dot.Y = outerInstance.random.Next(100);
					dot.Color = redColor;
					backgroundThreadRealm.commitTransaction();

					// Wait 0.5 sec. before adding the next dot.
					SystemClock.sleep(500);
				}

				// Also close Realm instances used in background threads.
				backgroundThreadRealm.close();
			}
		}

		public override void onPause()
		{
			base.onPause();

			// Disable UI refresh while the fragment is no longer active.
			realm.removeChangeListener(realmListener);
			backgroundThread.Interrupt();
		}

		public override void onStop()
		{
			base.onStop();
			// Remember to close the Realm instance when done with it.
			realm.close();
		}
	}

}