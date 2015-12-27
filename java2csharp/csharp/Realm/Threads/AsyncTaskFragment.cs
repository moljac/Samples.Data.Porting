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

	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using LinearLayout = android.widget.LinearLayout;
	using ProgressBar = android.widget.ProgressBar;
	using SeekBar = android.widget.SeekBar;
	using TextView = android.widget.TextView;

	using Score = io.realm.examples.threads.model.Score;

	/// <summary>
	/// This fragment demonstrates how Realm can work with AsyncTasks.
	/// </summary>
	public class AsyncTaskFragment : Fragment
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		private static readonly string TAG = typeof(AsyncTaskFragment).FullName;
		private const int TEST_OBJECTS = 100;

		private LinearLayout logsView;
		private ProgressBar progressView;
		private ImportAsyncTask asyncTask;

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			View rootView = inflater.inflate(R.layout.fragment_asynctask, container, false);
			logsView = (LinearLayout) rootView.findViewById(R.id.logs);
			progressView = (ProgressBar) rootView.findViewById(R.id.progressBar);
			progressView.Visibility = View.GONE;
			rootView.findViewById(R.id.start_button).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

			return rootView;
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly AsyncTaskFragment outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(AsyncTaskFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View v)
			{
				if (outerInstance.asyncTask != null)
				{
					outerInstance.asyncTask.cancel(true);
				}

				outerInstance.asyncTask = new ImportAsyncTask(outerInstance);
				outerInstance.asyncTask.execute();
			}
		}

		private void showStatus(string txt)
		{
			Log.i(TAG, txt);
			TextView tv = new TextView(Activity);
			tv.Text = txt;
			tv.TextColor = Resources.getColor(android.R.color.white);
			logsView.addView(tv);
		}

		// ASyncTask that imports Realm data while providing progress and returns the value of an
		// aggregate function in the end.
		//
		// Note:
		// doInBackground() runs in its own background thread while all other methods are executed on the
		// UI thread. This means that it is not possible to reuse RealmObjects or RealmResults created
		// in doInBackground() in the other methods. Nor is it possible to use RealmObjects as Progress
		// or Result objects.
		private class ImportAsyncTask : AsyncTask<Void, int?, int?>
		{
			private readonly AsyncTaskFragment outerInstance;

			public ImportAsyncTask(AsyncTaskFragment outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			protected internal override int? doInBackground(params Void[] @params)
			{
				Realm realm = Realm.DefaultInstance;

				realm.beginTransaction();
				realm.clear(typeof(Score));
				for (int i = 0; i < TEST_OBJECTS; i++)
				{
					if (Cancelled)
					{
						break;
					}
					Score score = realm.createObject(typeof(Score));
					score.Name = "Name" + i;
					score.setScore(i);
				}
				realm.commitTransaction();

				Number sum = realm.allObjects(typeof(Score)).sum("score");
				realm.close();
				return sum.intValue();
			}

			protected internal override void onPreExecute()
			{
				outerInstance.logsView.removeAllViews();
				outerInstance.progressView.Visibility = View.VISIBLE;
				outerInstance.showStatus("Starting import");
			}

			protected internal override void onPostExecute(int? sum)
			{
				outerInstance.progressView.Visibility = View.GONE;
				outerInstance.showStatus(TEST_OBJECTS + " objects imported.");
				outerInstance.showStatus("The total score is : " + sum);
			}
		}
	}

}