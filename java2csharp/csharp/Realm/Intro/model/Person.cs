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

namespace io.realm.examples.intro.model
{

	using Ignore = io.realm.annotations.Ignore;

	// Your model just have to extend RealmObject.
	// This will inherit an annotation which produces proxy getters and setters for all fields.
	public class Person : RealmObject
	{

		// All fields are by default persisted.
		private string name;
		private int age;

		// Other objects in a one-to-one relation must also subclass RealmObject
		private Dog dog;

		// One-to-many relations is simply a RealmList of the objects which also subclass RealmObject
		private RealmList<Cat> cats;

		// You can instruct Realm to ignore a field and not persist it.
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Ignore private int tempReference;
		private int tempReference;

		private long id;

		// The standard getters and setters your IDE generates are fine.
		// Realm will overload them and code inside them is ignored.
		// So if you prefer you can also just have empty abstract methods.

		public virtual string Name
		{
			get
			{
				return name;
			}
			set
			{
				this.name = value;
			}
		}


		public virtual int Age
		{
			get
			{
				return age;
			}
			set
			{
				this.age = value;
			}
		}


		public virtual Dog Dog
		{
			get
			{
				return dog;
			}
			set
			{
				this.dog = value;
			}
		}


		public virtual RealmList<Cat> Cats
		{
			get
			{
				return cats;
			}
			set
			{
				this.cats = value;
			}
		}


		public virtual int TempReference
		{
			get
			{
				return tempReference;
			}
			set
			{
				this.tempReference = value;
			}
		}


		public virtual long Id
		{
			get
			{
				return id;
			}
			set
			{
				this.id = value;
			}
		}

	}

}