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

namespace io.realm.examples.encryptionexample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;


	public class EncryptionExampleActivity : Activity
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		public static readonly string TAG = typeof(EncryptionExampleActivity).FullName;

		private Realm realm;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			// Generate a key
			// IMPORTANT! This is a silly way to generate a key. It is also never stored.
			// For proper key handling please consult:
			// * https://developer.android.com/training/articles/keystore.html
			// * http://nelenkov.blogspot.dk/2012/05/storing-application-secrets-in-androids.html
			sbyte[] key = new sbyte[64];
			(new SecureRandom()).NextBytes(key);
			RealmConfiguration realmConfiguration = (new RealmConfiguration.Builder(this)).encryptionKey(key).build();

			// Start with a clean slate every time
			Realm.deleteRealm(realmConfiguration);

			// Open the Realm with encryption enabled
			realm = Realm.getInstance(realmConfiguration);

			// Everything continues to work as normal except for that the file is encrypted on disk
			realm.beginTransaction();
			Person person = realm.createObject(typeof(Person));
			person.Name = "Happy Person";
			person.Age = 14;
			realm.commitTransaction();

			person = realm.@where(typeof(Person)).findFirst();
			Log.i(TAG, string.Format("Person name: {0}", person.Name));
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			realm.close(); // Remember to close Realm when done.
		}
	}

}