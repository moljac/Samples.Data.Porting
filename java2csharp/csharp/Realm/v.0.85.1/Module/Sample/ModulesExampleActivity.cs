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

namespace io.realm.examples.appmodules
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;

	using Cow = io.realm.examples.appmodules.model.Cow;
	using Pig = io.realm.examples.appmodules.model.Pig;
	using Snake = io.realm.examples.appmodules.model.Snake;
	using Spider = io.realm.examples.appmodules.model.Spider;
	using CreepyAnimalsModule = io.realm.examples.appmodules.modules.CreepyAnimalsModule;
	using Zoo = io.realm.examples.librarymodules.Zoo;
	using Cat = io.realm.examples.librarymodules.model.Cat;
	using Dog = io.realm.examples.librarymodules.model.Dog;
	using Elephant = io.realm.examples.librarymodules.model.Elephant;
	using Lion = io.realm.examples.librarymodules.model.Lion;
	using Zebra = io.realm.examples.librarymodules.model.Zebra;
	using DomesticAnimalsModule = io.realm.examples.librarymodules.modules.DomesticAnimalsModule;
	using ZooAnimalsModule = io.realm.examples.librarymodules.modules.ZooAnimalsModule;
	using RealmException = io.realm.exceptions.RealmException;

	/// <summary>
	/// This example demonstrates how you can use modules to control which classes belong to which Realms and how you can
	/// work with multiple Realms at the same time.
	/// </summary>
	public class ModulesExampleActivity : Activity
	{

//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
		public static readonly string TAG = typeof(ModulesExampleActivity).FullName;
		private LinearLayout rootLayout = null;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_modules_example;
			rootLayout = ((LinearLayout) findViewById(R.id.container));
			rootLayout.removeAllViews();

			// The default Realm instance implicitly knows about all classes in the realmModuleAppExample Android Studio
			// module. This does not include the classes from the realmModuleLibraryExample AS module so a Realm using this
			// configuration would know about the following classes: { Cow, Pig, Snake, Spider }
			RealmConfiguration defaultConfig = (new RealmConfiguration.Builder(this)).build();

			// It is possible to extend the default schema by adding additional Realm modules using setModule(). This can
			// also be Realm modules from libraries. The below Realm contains the following classes: { Cow, Pig, Snake,
			// Spider, Cat, Dog }
			RealmConfiguration farmAnimalsConfig = (new RealmConfiguration.Builder(this)).name("farm.realm").setModules(Realm.DefaultModule, new DomesticAnimalsModule()).build();

			// Or you can completely replace the default schema.
			// This Realm contains the following classes: { Elephant, Lion, Zebra, Snake, Spider }
			RealmConfiguration exoticAnimalsConfig = (new RealmConfiguration.Builder(this)).name("exotic.realm").setModules(new ZooAnimalsModule(), new CreepyAnimalsModule()).build();

			// Multiple Realms can be open at the same time
			showStatus("Opening multiple Realms");
			Realm defaultRealm = Realm.getInstance(defaultConfig);
			Realm farmRealm = Realm.getInstance(farmAnimalsConfig);
			Realm exoticRealm = Realm.getInstance(exoticAnimalsConfig);

			// Objects can be added to each Realm independantly
			showStatus("Create objects in the default Realm");
			defaultRealm.executeTransaction(new TransactionAnonymousInnerClassHelper(this));

			showStatus("Create objects in the farm Realm");
			farmRealm.executeTransaction(new TransactionAnonymousInnerClassHelper2(this));

			showStatus("Create objects in the exotic Realm");
			exoticRealm.executeTransaction(new TransactionAnonymousInnerClassHelper3(this));

			// You can copy objects between Realms
			showStatus("Copy objects between Realms");
			showStatus("Number of pigs on the farm : " + farmRealm.@where(typeof(Pig)).count());
			showStatus("Copy pig from defaultRealm to farmRealm");
			Pig defaultPig = defaultRealm.@where(typeof(Pig)).findFirst();
			farmRealm.beginTransaction();
			farmRealm.copyToRealm(defaultPig);
			farmRealm.commitTransaction();
			showStatus("Number of pigs on the farm : " + farmRealm.@where(typeof(Pig)).count());

			// Each Realm is restricted to only accept the classes in their schema.
			showStatus("Trying to add an unsupported class");
			defaultRealm.beginTransaction();
			try
			{
				defaultRealm.createObject(typeof(Elephant));
			}
			catch (RealmException expected)
			{
				showStatus("This throws a :" + expected.ToString());
			}
			finally
			{
				defaultRealm.cancelTransaction();
			}

			// And Realms in library projects are independent from Realms in the app code
			showStatus("Interacting with library code that uses Realm internally");
			int animals = 5;
			Zoo libraryZoo = new Zoo(this);
			libraryZoo.open();
			showStatus("Adding animals: " + animals);
			libraryZoo.addAnimals(5);
			showStatus("Number of animals in the library Realm:" + libraryZoo.NoOfAnimals);
			libraryZoo.close();

			// Remember to close all open Realms
			defaultRealm.close();
			farmRealm.close();
			exoticRealm.close();
		}

		private class TransactionAnonymousInnerClassHelper : Realm.Transaction
		{
			private readonly ModulesExampleActivity outerInstance;

			public TransactionAnonymousInnerClassHelper(ModulesExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void execute(Realm realm)
			{
				realm.createObject(typeof(Cow));
				realm.createObject(typeof(Pig));
				realm.createObject(typeof(Snake));
				realm.createObject(typeof(Spider));
			}
		}

		private class TransactionAnonymousInnerClassHelper2 : Realm.Transaction
		{
			private readonly ModulesExampleActivity outerInstance;

			public TransactionAnonymousInnerClassHelper2(ModulesExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void execute(Realm realm)
			{
				realm.createObject(typeof(Cow));
				realm.createObject(typeof(Pig));
				realm.createObject(typeof(Cat));
				realm.createObject(typeof(Dog));
			}
		}

		private class TransactionAnonymousInnerClassHelper3 : Realm.Transaction
		{
			private readonly ModulesExampleActivity outerInstance;

			public TransactionAnonymousInnerClassHelper3(ModulesExampleActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void execute(Realm realm)
			{
				realm.createObject(typeof(Elephant));
				realm.createObject(typeof(Lion));
				realm.createObject(typeof(Zebra));
				realm.createObject(typeof(Snake));
				realm.createObject(typeof(Spider));
			}
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