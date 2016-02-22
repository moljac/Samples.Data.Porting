using System.Text;

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

namespace io.realm.examples.realmmigrationexample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;


	using Migration = io.realm.examples.realmmigrationexample.model.Migration;
	using Person = io.realm.examples.realmmigrationexample.model.Person;
	using RealmMigrationNeededException = io.realm.exceptions.RealmMigrationNeededException;

	/*
	** This example demonstrates how you can migrate your data through different updates
	** of your models.
	*/
	public class MigrationExampleActivity : Activity
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		public static readonly string TAG = typeof(MigrationExampleActivity).FullName;

		private LinearLayout rootLayout = null;
		private Realm realm;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_realm_basic_example;

			rootLayout = ((LinearLayout) findViewById(R.id.container));
			rootLayout.removeAllViews();

			// 3 versions of the databases for testing. Normally you would only have one.
			copyBundledRealmFile(this.Resources.openRawResource(R.raw.default0), "default0");
			copyBundledRealmFile(this.Resources.openRawResource(R.raw.default1), "default1");
			copyBundledRealmFile(this.Resources.openRawResource(R.raw.default2), "default2");

			// When you create a RealmConfiguration you can specify the version of the schema.
			// If the schema does not have that version a RealmMigrationNeededException will be thrown.
			RealmConfiguration config0 = (new RealmConfiguration.Builder(this)).name("default0").schemaVersion(3).build();

			// You can then manually call Realm.migrateRealm().
			Realm.migrateRealm(config0, new Migration());
			realm = Realm.getInstance(config0);
			showStatus("Default0");
			showStatus(realm);
			realm.close();

			// Or you can add the migration code to the configuration. This will run the migration code without throwing
			// a RealmMigrationNeededException.
			RealmConfiguration config1 = (new RealmConfiguration.Builder(this)).name("default1").schemaVersion(3).migration(new Migration()).build();

			realm = Realm.getInstance(config1); // Automatically run migration if needed
			showStatus("Default1");
			showStatus(realm);
			realm.close();

			// or you can set .deleteRealmIfMigrationNeeded() if you don't want to bother with migrations.
			// WARNING: This will delete all data in the Realm though.
			RealmConfiguration config2 = (new RealmConfiguration.Builder(this)).name("default2").schemaVersion(3).deleteRealmIfMigrationNeeded().build();

			realm = Realm.getInstance(config2);
			showStatus("default2");
			showStatus(realm);
			realm.close();
		}

		private string copyBundledRealmFile(System.IO.Stream inputStream, string outFileName)
		{
			try
			{
				File file = new File(this.FilesDir, outFileName);
				System.IO.FileStream outputStream = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);
				sbyte[] buf = new sbyte[1024];
				int bytesRead;
				while ((bytesRead = inputStream.Read(buf, 0, buf.Length)) > 0)
				{
					outputStream.Write(buf, 0, bytesRead);
				}
				outputStream.Close();
				return file.AbsolutePath;
			}
			catch (IOException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
			return null;
		}

		private string realmString(Realm realm)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (Person person in realm.allObjects(typeof(Person)))
			{
				stringBuilder.Append(person.ToString()).Append("\n");
			}

			return (stringBuilder.Length == 0) ? "<empty>" : stringBuilder.ToString();
		}

		private void showStatus(Realm realm)
		{
			showStatus(realmString(realm));
		}

		private void showStatus(string txt)
		{
			Log.i(TAG, txt);
			TextView tv = new TextView(this);
			tv.Text = txt;
			rootLayout.addView(tv);
		}
	}

}