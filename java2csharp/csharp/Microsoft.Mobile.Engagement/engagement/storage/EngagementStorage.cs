using System;
using System.Collections.Generic;
using System.Text;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.storage
{


	using ContentValues = android.content.ContentValues;
	using Context = android.content.Context;
	using Cursor = android.database.Cursor;
	using SQLiteDatabase = android.database.sqlite.SQLiteDatabase;
	using SQLiteOpenHelper = android.database.sqlite.SQLiteOpenHelper;
	using SQLiteQueryBuilder = android.database.sqlite.SQLiteQueryBuilder;

	/// <summary>
	/// Storage abstraction. Attempts to use SQLite and fails over in memory if an error occurs.
	/// </summary>
	public class EngagementStorage : System.IDisposable
	{
	  /// <summary>
	  /// Primary key name </summary>
	  public const string PRIMARY_KEY = "oid";

	  /// <summary>
	  /// Storage capacity in number of entries </summary>
	  private const int CAPACITY = 300;

	  /// <summary>
	  /// Selection pattern for primary key search </summary>
	  private const string PRIMARY_KEY_SELECTION = "oid = ?";

	  /// <summary>
	  /// Application context </summary>
	  private readonly Context mContext;

	  /// <summary>
	  /// In-memory database if SQLite cannot be used </summary>
	  private IDictionary<long?, ContentValues> mIMDB;

	  /// <summary>
	  /// In-memory auto increment </summary>
	  private long mIMDBAutoInc;

	  /// <summary>
	  /// SQLite manager </summary>
	  private readonly SQLiteManager mManager;

	  /// <summary>
	  /// Error listener </summary>
	  private readonly ErrorListener mErrorListener;

	  /// <summary>
	  /// SQLite manager specification </summary>
	  private class SQLiteManager : SQLiteOpenHelper
	  {
		/// <summary>
		/// Database name </summary>
		internal readonly string mDBName;

		/// <summary>
		/// Table name </summary>
		internal readonly string mTableName;

		/// <summary>
		/// Schema, e.g. a specimen with dummy values to have keys and their corresponding value's type </summary>
		internal readonly ContentValues mSchema;

		/// <summary>
		/// Init SQLite manager. </summary>
		/// <param name="context"> application context. </param>
		/// <param name="dbName"> database (file) name. </param>
		/// <param name="version"> schema version. </param>
		/// <param name="tableName"> table name. </param>
		/// <param name="schema"> specimen value. </param>
		internal SQLiteManager(Context context, string dbName, int version, string tableName, ContentValues schema) : base(context, dbName, null, version)
		{
		  mDBName = dbName;
		  mTableName = tableName;
		  mSchema = schema;
		}

		public override void onCreate(SQLiteDatabase db)
		{
		  /* Generate a schema from specimen */
		  StringBuilder sql = new StringBuilder("CREATE TABLE `");
		  sql.Append(mTableName);
		  sql.Append("` (oid INTEGER PRIMARY KEY AUTOINCREMENT");
		  foreach (KeyValuePair<string, object> col in mSchema.valueSet())
		  {
			sql.Append(", `").Append(col.Key).Append("` ");
			object val = col.Value;
			if (val is double? || val is float?)
			{
			  sql.Append("REAL");
			}
			else if (val is Number || val is bool?)
			{
			  sql.Append("INTEGER");
			}
			else if (val is sbyte[])
			{
			  sql.Append("BLOB");
			}
			else
			{
			  sql.Append("TEXT");
			}
		  }
		  sql.Append(");");
		  db.execSQL(sql.ToString());
		}

		public override void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
		{
		  /* For now we upgrade by destroying the old table */
		  db.execSQL("DROP TABLE `" + mTableName + "`");
		  onCreate(db);
		}

		/// <summary>
		/// Get database name. </summary>
		/// <returns> database name. </returns>
		public virtual string DBName
		{
			get
			{
			  return mDBName;
			}
		}

		/// <summary>
		/// Get table name. </summary>
		/// <returns> table name. </returns>
		internal virtual string TableName
		{
			get
			{
			  return mTableName;
			}
		}

		/// <summary>
		/// Get schema. </summary>
		/// <returns> schema as a specimen value. </returns>
		internal virtual ContentValues Schema
		{
			get
			{
			  return mSchema;
			}
		}
	  }

	  /// <summary>
	  /// Listener specification, each callback is called only once per instance </summary>
	  public interface ErrorListener
	  {
		/// <summary>
		/// Notify an exception </summary>
		void onError(string operation, Exception e);
	  }

	  /// <summary>
	  /// Init a database. </summary>
	  /// <param name="context"> application context. </param>
	  /// <param name="dbName"> database (file) name. </param>
	  /// <param name="version"> schema version. </param>
	  /// <param name="tableName"> table name. </param>
	  /// <param name="schema"> specimen value. </param>
	  /// <param name="errorListener"> optional error listener. </param>
	  public EngagementStorage(Context context, string dbName, int version, string tableName, ContentValues schema, ErrorListener errorListener)
	  {
		/* Prepare SQLite manager */
		mContext = context;
		mManager = new SQLiteManager(context, dbName, version, tableName, schema);
		mErrorListener = errorListener;
	  }

	  /// <summary>
	  /// Get SQLite database. </summary>
	  /// <returns> SQLite database. </returns>
	  /// <exception cref="RuntimeException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private android.database.sqlite.SQLiteDatabase getDatabase() throws RuntimeException
	  private SQLiteDatabase Database
	  {
		  get
		  {
			/* Try opening database */
			try
			{
			  return mManager.WritableDatabase;
			}
			catch (Exception)
			{
			  /* First error, try to delete database (may be corrupted) */
			  mContext.deleteDatabase(mManager.DBName);
    
			  /* Retry, let exception thrown if it fails this time */
			  return mManager.WritableDatabase;
			}
		  }
	  }

	  /// <summary>
	  /// Switch to in memory management, trigger error listener. </summary>
	  /// <param name="operation"> operation that triggered the error. </param>
	  /// <param name="e"> exception that triggered the switch. </param>
	  private void switchToInMemory(string operation, Exception e)
	  {
		mIMDB = new LinkedHashMapAnonymousInnerClassHelper(this);
		if (mErrorListener != null)
		{
		  mErrorListener.onError(operation, e);
		}
	  }

	  private class LinkedHashMapAnonymousInnerClassHelper : LinkedHashMap<long?, ContentValues>
	  {
		  private readonly EngagementStorage outerInstance;

		  public LinkedHashMapAnonymousInnerClassHelper(EngagementStorage outerInstance)
		  {
			  this.outerInstance = outerInstance;
			  serialVersionUID = 1L;
		  }

		  private static readonly long serialVersionUID;

		  protected internal override bool removeEldestEntry(KeyValuePair<long?, ContentValues> eldest)
		  {
			return size() > CAPACITY;
		  };
	  }

	  /// <summary>
	  /// Store an entry. </summary>
	  /// <param name="values"> object describing the values to store. </param>
	  /// <returns> database identifier. </returns>
	  public virtual long? put(ContentValues values)
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			/* Insert data */
			long id = Database.insertOrThrow(mManager.TableName, null, values);

			/* Purge oldest entry if capacity reached */
			Cursor cursor = getCursor(null, null);
			int count = cursor.Count;
			if (count > CAPACITY)
			{
			  cursor.moveToNext();
			  delete(cursor.getLong(0));
			}
			cursor.close();

			/* Return id */
			return id;
		  }
		  catch (Exception e)
		  {
			switchToInMemory("put", e);
		  }
		}

		/* If failed over in-memory */
		values.put(PRIMARY_KEY, mIMDBAutoInc);
		mIMDB[mIMDBAutoInc] = values;
		return mIMDBAutoInc++;
	  }

	  /// <summary>
	  /// Update an entry. </summary>
	  /// <param name="id"> existing entry identifier. </param>
	  /// <param name="values"> values to update. </param>
	  /// <returns> true if update was successful, false otherwise. </returns>
	  public virtual bool update(long id, ContentValues values)
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			/* Update data */
			int updated = Database.update(mManager.TableName, values, PRIMARY_KEY_SELECTION, new string[] {id.ToString()});

			/* Return success */
			return updated > 0;
		  }
		  catch (Exception e)
		  {
			switchToInMemory("update", e);
		  }
		}

		/* If failed over in-memory */
		ContentValues existing = mIMDB[id];
		if (existing == null)
		{
		  return false;
		}
		existing.putAll(values);
		return true;
	  }

	  /// <summary>
	  /// Delete an entry from the database. </summary>
	  /// <param name="id"> database identifier. </param>
	  public virtual void delete(long id)
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			Database.delete(mManager.TableName, PRIMARY_KEY_SELECTION, new string[] {id.ToString()});
		  }
		  catch (Exception e)
		  {
			switchToInMemory("delete", e);
		  }
		}

		/* If failed over in-memory */
		else
		{
		  mIMDB.Remove(id);
		}
	  }

	  /// <summary>
	  /// Get an entry by its identifier. </summary>
	  /// <param name="id"> identifier. </param>
	  /// <returns> entry or null if not found. </returns>
	  public virtual ContentValues get(long id)
	  {
		return get(PRIMARY_KEY, id);
	  }

	  /// <summary>
	  /// Get first entry whose key = value. </summary>
	  /// <param name="key"> key. </param>
	  /// <param name="value"> value. </param>
	  /// <returns> matching entry or null. </returns>
	  public virtual ContentValues get(string key, object value)
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			Cursor cursor = getCursor(key, value);
			ContentValues values;
			if (cursor.moveToFirst())
			{
			  values = buildValues(cursor);
			}
			else
			{
			  values = null;
			}
			cursor.close();
			return values;
		  }
		  catch (Exception e)
		  {
			switchToInMemory("get", e);
			return null;
		  }
		}

		/* If failed over in-memory */
		else if (PRIMARY_KEY.Equals(key))
		{
		  return mIMDB[value];
		}
		else
		{
		  foreach (ContentValues values in mIMDB.Values)
		  {
			if (value.Equals(values.get(key)))
			{
			  return values;
			}
		  }
		}
		return null;
	  }

	  /// <summary>
	  /// Check if database is empty. </summary>
	  /// <returns> true if database is empty, false otherwise. </returns>
	  public virtual bool Empty
	  {
		  get
		  {
			Scanner scanner = getScanner(null, null);
	//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			bool empty = !scanner.GetEnumerator().hasNext();
			scanner.close();
			return empty;
		  }
	  }

	  /// <summary>
	  /// Get a scanner to iterate over all values. </summary>
	  /// <returns> a scanner to iterate over all values. </returns>
	  public virtual Scanner getScanner()
	  {
		return getScanner(null, null);
	  }

	  /// <summary>
	  /// Get a scanner to iterate over all values where key matches value. </summary>
	  /// <param name="key"> key to match. </param>
	  /// <param name="value"> value to match. </param>
	  /// <returns> a scanner to iterate over all values. </returns>
	  public virtual Scanner getScanner(string key, object value)
	  {
		return new Scanner(this, key, value);
	  }

	  /// <summary>
	  /// Convert a cursor to a content values </summary>
	  private ContentValues buildValues(Cursor cursor)
	  {
		ContentValues values = new ContentValues();
		for (int i = 0; i < cursor.ColumnCount; i++)
		{
		  if (cursor.isNull(i))
		  {
			continue;
		  }
		  string key = cursor.getColumnName(i);
		  if (key.Equals(PRIMARY_KEY))
		  {
			values.put(key, cursor.getLong(i));
		  }
		  else
		  {
			object specimen = mManager.Schema.get(key);
			if (specimen is sbyte[])
			{
			  values.put(key, cursor.getBlob(i));
			}
			else if (specimen is double?)
			{
			  values.put(key, cursor.getDouble(i));
			}
			else if (specimen is float?)
			{
			  values.put(key, cursor.getFloat(i));
			}
			else if (specimen is int?)
			{
			  values.put(key, cursor.getInt(i));
			}
			else if (specimen is long?)
			{
			  values.put(key, cursor.getLong(i));
			}
			else if (specimen is short?)
			{
			  values.put(key, cursor.getShort(i));
			}
			else
			{
			  values.put(key, cursor.getString(i));
			}
		  }
		}
		return values;
	  }

	  /// <summary>
	  /// Get a cursor on all database rows, all rows where key matches value if specified. </summary>
	  /// <param name="key"> optional key to match. </param>
	  /// <param name="value"> optional value to match. </param>
	  /// <returns> cursor on all database rows. </returns>
	  /// <exception cref="RuntimeException"> if an error occurs. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private android.database.Cursor getCursor(String key, Object value) throws RuntimeException
	  private Cursor getCursor(string key, object value)
	  {
		SQLiteQueryBuilder builder = new SQLiteQueryBuilder();
		builder.Tables = mManager.TableName;
		string[] selectionArgs;
		if (key == null)
		{
		  selectionArgs = null;
		}
		else
		{
		  builder.appendWhere(key + " = ?");
		  selectionArgs = new string[] {value.ToString().ToString()};
		}
		return builder.query(Database, null, null, selectionArgs, null, null, null);
	  }

	  /// <summary>
	  /// Scanner specification.
	  /// </summary>
	  public class Scanner : IEnumerable<ContentValues>, System.IDisposable
	  {
		  private readonly EngagementStorage outerInstance;

		/// <summary>
		/// Filter key if any </summary>
		internal readonly string key;

		/// <summary>
		/// Filter value if any </summary>
		internal readonly object value;

		/// <summary>
		/// SQLite cursor </summary>
		internal Cursor cursor;

		/// <summary>
		/// Init cursor with optional filter </summary>
		internal Scanner(EngagementStorage outerInstance, string key, object value)
		{
			this.outerInstance = outerInstance;
		  this.key = key;
		  this.value = value;
		}

		public virtual void Dispose()
		{
		  /* If was using SQLite, close cursor */
		  if (cursor != null)
		  {
			try
			{
			  cursor.close();
			}
			catch (Exception e)
			{
			  outerInstance.switchToInMemory("scan", e);
			}
		  }
		}

		public virtual IEnumerator<ContentValues> GetEnumerator()
		{
		  /* Try SQLite */
		  if (outerInstance.mIMDB == null)
		  {
			try
			{
			  /* Open cursor, close previous one if any */
			  close();
			  cursor = outerInstance.getCursor(key, value);

			  /* Wrap cursor as iterator */
			  return new IteratorAnonymousInnerClassHelper(this);
			}
			catch (Exception e)
			{
			  outerInstance.switchToInMemory("scan", e);
			}
		  }

		  /* Fail over in-memory */
		  return new IteratorAnonymousInnerClassHelper2(this);
		}

		private class IteratorAnonymousInnerClassHelper : IEnumerator<ContentValues>
		{
			private readonly Scanner outerInstance;

			public IteratorAnonymousInnerClassHelper(Scanner outerInstance)
			{
				this.outerInstance = outerInstance;
			}

					/// <summary>
					/// If null, hasNext is not known yet </summary>
			internal bool? hasNext;

			public virtual bool hasNext()
			{
			  if (hasNext == null)
			  {
				try
				{
				  hasNext = outerInstance.cursor.moveToNext();
				}
				catch (Exception e)
				{
				  /* Consider no next on error */
				  hasNext = false;

				  /* Make close do nothing */
				  outerInstance.cursor = null;

				  /* Switch to in memory DB */
				  outerInstance.outerInstance.switchToInMemory("scan", e);
				}
			  }
			  return hasNext;
			}

			public virtual ContentValues next()
			{
			  /* Check next */
			  if (!hasNext())
			  {
				throw new NoSuchElementException();
			  }
			  hasNext = null;

			  /* Build object */
			  return outerInstance.outerInstance.buildValues(outerInstance.cursor);
			}

			public virtual void remove()
			{
			  throw new System.NotSupportedException();
			}
		}

		private class IteratorAnonymousInnerClassHelper2 : IEnumerator<ContentValues>
		{
			private readonly Scanner outerInstance;

			public IteratorAnonymousInnerClassHelper2(Scanner outerInstance)
			{
				this.outerInstance = outerInstance;
				iterator = outerInstance.outerInstance.mIMDB.Values.GetEnumerator();
			}

				/// <summary>
				/// In memory map iterator that we wrap because of the filter logic </summary>
			internal IEnumerator<ContentValues> iterator;

			/// <summary>
			/// True if we moved the iterator but not retrieved the value </summary>
			internal bool advanced;

			/// <summary>
			/// Next value </summary>
			internal ContentValues next;

			public virtual bool hasNext()
			{
			  /* Move iterator the first time */
			  if (!advanced)
			  {
				next = null;
				while (iterator.hasNext())
				{
				  next = iterator.next();
				  if (outerInstance.key == null || outerInstance.value.Equals(next.get(outerInstance.key)))
				  {
					break;
				  }
				}
				advanced = true;
			  }
			  return next != null;
			}

			public virtual ContentValues next()
			{
			  if (!hasNext())
			  {
				throw new NoSuchElementException();
			  }
			  advanced = false;
			  return next;
			}

			public virtual void remove()
			{
			  throw new System.NotSupportedException();
			}
		}
	  }

	  /// <summary>
	  /// Clear database.
	  /// </summary>
	  public virtual void clear()
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			Database.delete(mManager.TableName, null, null);
		  }
		  catch (Exception e)
		  {
			switchToInMemory("clear", e);
		  }
		}

		/* If failed over in-memory */
		else
		{
		  mIMDB.Clear();
		}
	  }

	  public virtual void Dispose()
	  {
		/* Try SQLite */
		if (mIMDB == null)
		{
		  try
		  {
			Database.close();
		  }
		  catch (Exception e)
		  {
			switchToInMemory("close", e);
		  }
		}

		/* Close in memory database */
		else
		{
		  mIMDB.Clear();
		  mIMDB = null;
		}
	  }
	}

}