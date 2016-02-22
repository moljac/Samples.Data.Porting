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

namespace io.realm.examples.realmmigrationexample.model
{

	using Ignore = io.realm.annotations.Ignore;

	public class Person : RealmObject
	{

		private string fullName;
		private int age;
		private RealmList<Pet> pets;

		public virtual string FullName
		{
			get
			{
				return fullName;
			}
			set
			{
				this.fullName = value;
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


		public virtual RealmList<Pet> Pets
		{
			get
			{
				return pets;
			}
			set
			{
				this.pets = value;
			}
		}

	}


}