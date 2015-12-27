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

namespace io.realm.examples.intro
{

	using Activity = android.app.Activity;
	using AsyncTask = android.os.AsyncTask;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;

	using Cat = io.realm.examples.intro.model.Cat;
	using Dog = io.realm.examples.intro.model.Dog;
	using Person = io.realm.examples.intro.model.Person;


	public class IntroExampleActivity : Activity
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		public static readonly string TAG = typeof(IntroExampleActivity).FullName;
		private LinearLayout rootLayout = null;

		private Realm realm;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_realm_basic_example;
			rootLayout = ((LinearLayout) findViewById(R.id.container));
			rootLayout.removeAllViews();

			// These operations are small enough that
			// we can generally safely run them on the UI thread.

			// Open the default realm for the UI thread.
			realm = Realm.getInstance(this);

			basicCRUD(realm);
			basicQuery(realm);
			basicLinkQuery(realm);

			// More complex operations can be executed on another thread.
			new AsyncTaskAnonymousInnerClassHelper(this)
			.execute();
		}

		private class AsyncTaskAnonymousInnerClassHelper : AsyncTask<Void, Void, string>
		{
			private readonly IntroExampleActivity outerInstance;

			public AsyncTaskAnonymousInnerClassHelper(IntroExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override string doInBackground(params Void[] voids)
			{
				string info;
				info = outerInstance.complexReadWrite();
				info += outerInstance.complexQuery();
				return info;
			}

			protected internal override void onPostExecute(string result)
			{
				outerInstance.showStatus(result);
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			realm.close(); // Remember to close Realm when done.
		}

		private void showStatus(string txt)
		{
			Log.i(TAG, txt);
			TextView tv = new TextView(this);
			tv.Text = txt;
			rootLayout.addView(tv);
		}

		private void basicCRUD(Realm realm)
		{
			showStatus("Perform basic Create/Read/Update/Delete (CRUD) operations...");

			// All writes must be wrapped in a transaction to facilitate safe multi threading
			realm.beginTransaction();

			// Add a person
			Person person = realm.createObject(typeof(Person));
			person.Id = 1;
			person.Name = "Young Person";
			person.Age = 14;

			// When the transaction is committed, all changes a synced to disk.
			realm.commitTransaction();

			// Find the first person (no query conditions) and read a field
			person = realm.@where(typeof(Person)).findFirst();
			showStatus(person.Name + ":" + person.Age);

			// Update person in a transaction
			realm.beginTransaction();
			person.Name = "Senior Person";
			person.Age = 99;
			showStatus(person.Name + " got older: " + person.Age);
			realm.commitTransaction();

			// Delete all persons
			realm.beginTransaction();
			realm.allObjects(typeof(Person)).clear();
			realm.commitTransaction();
		}

		private void basicQuery(Realm realm)
		{
			showStatus("\nPerforming basic Query operation...");
			showStatus("Number of persons: " + realm.allObjects(typeof(Person)).size());

			RealmResults<Person> results = realm.@where(typeof(Person)).equalTo("age", 99).findAll();

			showStatus("Size of result set: " + results.size());
		}

		private void basicLinkQuery(Realm realm)
		{
			showStatus("\nPerforming basic Link Query operation...");
			showStatus("Number of persons: " + realm.allObjects(typeof(Person)).size());

			RealmResults<Person> results = realm.@where(typeof(Person)).equalTo("cats.name", "Tiger").findAll();

			showStatus("Size of result set: " + results.size());
		}

		private string complexReadWrite()
		{
			string status = "\nPerforming complex Read/Write operation...";

			// Open the default realm. All threads must use it's own reference to the realm.
			// Those can not be transferred across threads.
			Realm realm = Realm.getInstance(this);

			// Add ten persons in one transaction
			realm.beginTransaction();
			Dog fido = realm.createObject(typeof(Dog));
			fido.Name = "fido";
			for (int i = 0; i < 10; i++)
			{
				Person person = realm.createObject(typeof(Person));
				person.Id = i;
				person.Name = "Person no. " + i;
				person.Age = i;
				person.Dog = fido;

				// The field tempReference is annotated with @Ignore.
				// This means setTempReference sets the Person tempReference
				// field directly. The tempReference is NOT saved as part of
				// the RealmObject:
				person.TempReference = 42;

				for (int j = 0; j < i; j++)
				{
					Cat cat = realm.createObject(typeof(Cat));
					cat.Name = "Cat_" + j;
					person.Cats.add(cat);
				}
			}
			realm.commitTransaction();

			// Implicit read transactions allow you to access your objects
			status += "\nNumber of persons: " + realm.allObjects(typeof(Person)).size();

			// Iterate over all objects
			foreach (Person pers in realm.allObjects(typeof(Person)))
			{
				string dogName;
				if (pers.Dog == null)
				{
					dogName = "None";
				}
				else
				{
					dogName = pers.Dog.Name;
				}
				status += "\n" + pers.Name + ":" + pers.Age + " : " + dogName + " : " + pers.Cats.size();

				// The field tempReference is annotated with @Ignore
				// Though we initially set its value to 42, it has
				// not been saved as part of the Person RealmObject:
				assert(pers.TempReference == 0);
			}

			// Sorting
			RealmResults<Person> sortedPersons = realm.allObjects(typeof(Person));
			sortedPersons.sort("age", false);
			assert(realm.allObjects(typeof(Person)).last().Name == sortedPersons.first().Name);
			status += "\nSorting " + sortedPersons.last().Name + " == " + realm.allObjects(typeof(Person)).first().Name;

			realm.close();
			return status;
		}

		private string complexQuery()
		{
			string status = "\n\nPerforming complex Query operation...";

			Realm realm = Realm.getInstance(this);
			status += "\nNumber of persons: " + realm.allObjects(typeof(Person)).size();

			// Find all persons where age between 7 and 9 and name begins with "Person".
			RealmResults<Person> results = realm.@where(typeof(Person)).between("age", 7, 9).beginsWith("name", "Person").findAll(); // Notice implicit "and" operation
			status += "\nSize of result set: " + results.size();

			realm.close();
			return status;
		}
	}

}