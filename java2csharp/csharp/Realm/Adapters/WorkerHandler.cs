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

	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using Message = android.os.Message;

	using TimeStamp = io.realm.examples.realmadapters.models.TimeStamp;

	public class WorkerHandler : Handler
	{

		public const int ADD_TIMESTAMP = 1;
		public const int REMOVE_TIMESTAMP = 2;

		public const string ACTION = "action";
		public const string TIMESTAMP = "timestamp";

		private Realm realm;

		public WorkerHandler(Realm realm)
		{
			this.realm = realm;
		}

		public override void handleMessage(Message msg)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.os.Bundle bundle = msg.getData();
			Bundle bundle = msg.Data;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int action = bundle.getInt(ACTION);
			int action = bundle.getInt(ACTION);
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String timestamp = bundle.getString(TIMESTAMP);
			string timestamp = bundle.getString(TIMESTAMP);

			switch (action)
			{
				case ADD_TIMESTAMP:
					realm.beginTransaction();
					realm.createObject(typeof(TimeStamp)).TimeStamp = timestamp;
					realm.commitTransaction();
					break;
				case REMOVE_TIMESTAMP:
					realm.beginTransaction();
					realm.@where(typeof(TimeStamp)).equalTo("timeStamp", timestamp).findAll().clear();
					realm.commitTransaction();
					break;
			}
		}
	}

}