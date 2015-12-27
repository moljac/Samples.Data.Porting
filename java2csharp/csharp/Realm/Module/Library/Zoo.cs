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

namespace io.realm.examples.librarymodules
{

	using Context = android.content.Context;

	using Cat = io.realm.examples.librarymodules.model.Cat;
	using AllAnimalsModule = io.realm.examples.librarymodules.modules.AllAnimalsModule;

	/// <summary>
	/// Library projects can also use Realms, but some configuration options are mandatory to avoid clashing with Realms used
	/// in the app code.
	/// </summary>
	public class Zoo
	{

		private readonly RealmConfiguration realmConfig;
		private Realm realm;

		public Zoo(Context context)
		{
			realmConfig = (new RealmConfiguration.Builder(context)).name("library.zoo.realm").setModules(new AllAnimalsModule()).build(); // Always use explicit modules in library projects -  So always use a unique name -  Beware this is the app context

			// Reset Realm
			Realm.deleteRealm(realmConfig);
		}

		public virtual void open()
		{
			// Don't use Realm.setDefaultInstance() in library projects. It is unsafe as app developers can override the
			// default configuration. So always use explicit configurations in library projects.
			realm = Realm.getInstance(realmConfig);
		}

		public virtual long NoOfAnimals
		{
			get
			{
				return realm.@where(typeof(Cat)).count();
			}
		}

		public virtual void addAnimals(int count)
		{
			realm.beginTransaction();
			for (int i = 0; i < count; i++)
			{
				Cat cat = realm.createObject(typeof(Cat));
				cat.Name = "Cat " + i;
			}
			realm.commitTransaction();
		}

		public virtual void close()
		{
			realm.close();
		}
	}

}