/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.storage;

import java.io.Closeable;
import java.util.Iterator;
import java.util.LinkedHashMap;
import java.util.Map;
import java.util.Map.Entry;
import java.util.NoSuchElementException;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;
import android.database.sqlite.SQLiteQueryBuilder;

/**
 * Storage abstraction. Attempts to use SQLite and fails over in memory if an error occurs.
 */
public class EngagementStorage implements Closeable
{
  /** Primary key name */
  public static final String PRIMARY_KEY = "oid";

  /** Storage capacity in number of entries */
  private static final int CAPACITY = 300;

  /** Selection pattern for primary key search */
  private static final String PRIMARY_KEY_SELECTION = "oid = ?";

  /** Application context */
  private final Context mContext;

  /** In-memory database if SQLite cannot be used */
  private Map<Long, ContentValues> mIMDB;

  /** In-memory auto increment */
  private long mIMDBAutoInc;

  /** SQLite manager */
  private final SQLiteManager mManager;

  /** Error listener */
  private final ErrorListener mErrorListener;

  /** SQLite manager specification */
  private static class SQLiteManager extends SQLiteOpenHelper
  {
    /** Database name */
    private final String mDBName;

    /** Table name */
    private final String mTableName;

    /** Schema, e.g. a specimen with dummy values to have keys and their corresponding value's type */
    private final ContentValues mSchema;

    /**
     * Init SQLite manager.
     * @param context application context.
     * @param dbName database (file) name.
     * @param version schema version.
     * @param tableName table name.
     * @param schema specimen value.
     */
    private SQLiteManager(Context context, String dbName, int version, String tableName,
      ContentValues schema)
    {
      super(context, dbName, null, version);
      mDBName = dbName;
      mTableName = tableName;
      mSchema = schema;
    }

    @Override
    public void onCreate(SQLiteDatabase db)
    {
      /* Generate a schema from specimen */
      StringBuilder sql = new StringBuilder("CREATE TABLE `");
      sql.append(mTableName);
      sql.append("` (oid INTEGER PRIMARY KEY AUTOINCREMENT");
      for (Entry<String, Object> col : mSchema.valueSet())
      {
        sql.append(", `").append(col.getKey()).append("` ");
        Object val = col.getValue();
        if (val instanceof Double || val instanceof Float)
          sql.append("REAL");
        else if (val instanceof Number || val instanceof Boolean)
          sql.append("INTEGER");
        else if (val instanceof byte[])
          sql.append("BLOB");
        else
          sql.append("TEXT");
      }
      sql.append(");");
      db.execSQL(sql.toString());
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion)
    {
      /* For now we upgrade by destroying the old table */
      db.execSQL("DROP TABLE `" + mTableName + "`");
      onCreate(db);
    }

    /**
     * Get database name.
     * @return database name.
     */
    public String getDBName()
    {
      return mDBName;
    }

    /**
     * Get table name.
     * @return table name.
     */
    private String getTableName()
    {
      return mTableName;
    }

    /**
     * Get schema.
     * @return schema as a specimen value.
     */
    private ContentValues getSchema()
    {
      return mSchema;
    }
  }

  /** Listener specification, each callback is called only once per instance */
  public interface ErrorListener
  {
    /** Notify an exception */
    void onError(String operation, RuntimeException e);
  }

  /**
   * Init a database.
   * @param context application context.
   * @param dbName database (file) name.
   * @param version schema version.
   * @param tableName table name.
   * @param schema specimen value.
   * @param errorListener optional error listener.
   */
  public EngagementStorage(Context context, String dbName, int version, String tableName,
    ContentValues schema, ErrorListener errorListener)
  {
    /* Prepare SQLite manager */
    mContext = context;
    mManager = new SQLiteManager(context, dbName, version, tableName, schema);
    mErrorListener = errorListener;
  }

  /**
   * Get SQLite database.
   * @return SQLite database.
   * @throws RuntimeException if an error occurs.
   */
  private SQLiteDatabase getDatabase() throws RuntimeException
  {
    /* Try opening database */
    try
    {
      return mManager.getWritableDatabase();
    }
    catch (RuntimeException e)
    {
      /* First error, try to delete database (may be corrupted) */
      mContext.deleteDatabase(mManager.getDBName());

      /* Retry, let exception thrown if it fails this time */
      return mManager.getWritableDatabase();
    }
  }

  /**
   * Switch to in memory management, trigger error listener.
   * @param operation operation that triggered the error.
   * @param e exception that triggered the switch.
   */
  private void switchToInMemory(String operation, RuntimeException e)
  {
    mIMDB = new LinkedHashMap<Long, ContentValues>()
    {
      private static final long serialVersionUID = 1L;

      @Override
      protected boolean removeEldestEntry(Map.Entry<Long, ContentValues> eldest)
      {
        return size() > CAPACITY;
      };
    };
    if (mErrorListener != null)
      mErrorListener.onError(operation, e);
  }

  /**
   * Store an entry.
   * @param values object describing the values to store.
   * @return database identifier.
   */
  public Long put(ContentValues values)
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        /* Insert data */
        long id = getDatabase().insertOrThrow(mManager.getTableName(), null, values);

        /* Purge oldest entry if capacity reached */
        Cursor cursor = getCursor(null, null);
        int count = cursor.getCount();
        if (count > CAPACITY)
        {
          cursor.moveToNext();
          delete(cursor.getLong(0));
        }
        cursor.close();

        /* Return id */
        return id;
      }
      catch (RuntimeException e)
      {
        switchToInMemory("put", e);
      }

    /* If failed over in-memory */
    values.put(PRIMARY_KEY, mIMDBAutoInc);
    mIMDB.put(mIMDBAutoInc, values);
    return mIMDBAutoInc++;
  }

  /**
   * Update an entry.
   * @param id existing entry identifier.
   * @param values values to update.
   * @return true if update was successful, false otherwise.
   */
  public boolean update(long id, ContentValues values)
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        /* Update data */
        int updated = getDatabase().update(mManager.getTableName(), values, PRIMARY_KEY_SELECTION,
          new String[] { String.valueOf(id) });

        /* Return success */
        return updated > 0;
      }
      catch (RuntimeException e)
      {
        switchToInMemory("update", e);
      }

    /* If failed over in-memory */
    ContentValues existing = mIMDB.get(id);
    if (existing == null)
      return false;
    existing.putAll(values);
    return true;
  }

  /**
   * Delete an entry from the database.
   * @param id database identifier.
   */
  public void delete(long id)
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        getDatabase().delete(mManager.getTableName(), PRIMARY_KEY_SELECTION,
          new String[] { String.valueOf(id) });
      }
      catch (RuntimeException e)
      {
        switchToInMemory("delete", e);
      }

    /* If failed over in-memory */
    else
      mIMDB.remove(id);
  }

  /**
   * Get an entry by its identifier.
   * @param id identifier.
   * @return entry or null if not found.
   */
  public ContentValues get(long id)
  {
    return get(PRIMARY_KEY, id);
  }

  /**
   * Get first entry whose key = value.
   * @param key key.
   * @param value value.
   * @return matching entry or null.
   */
  public ContentValues get(String key, Object value)
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        Cursor cursor = getCursor(key, value);
        ContentValues values;
        if (cursor.moveToFirst())
          values = buildValues(cursor);
        else
          values = null;
        cursor.close();
        return values;
      }
      catch (RuntimeException e)
      {
        switchToInMemory("get", e);
        return null;
      }

    /* If failed over in-memory */
    else if (PRIMARY_KEY.equals(key))
      return mIMDB.get(value);
    else
      for (ContentValues values : mIMDB.values())
        if (value.equals(values.get(key)))
          return values;
    return null;
  }

  /**
   * Check if database is empty.
   * @return true if database is empty, false otherwise.
   */
  public boolean isEmpty()
  {
    Scanner scanner = getScanner(null, null);
    boolean empty = !scanner.iterator().hasNext();
    scanner.close();
    return empty;
  }

  /**
   * Get a scanner to iterate over all values.
   * @return a scanner to iterate over all values.
   */
  public Scanner getScanner()
  {
    return getScanner(null, null);
  }

  /**
   * Get a scanner to iterate over all values where key matches value.
   * @param key key to match.
   * @param value value to match.
   * @return a scanner to iterate over all values.
   */
  public Scanner getScanner(String key, Object value)
  {
    return new Scanner(key, value);
  }

  /** Convert a cursor to a content values */
  private ContentValues buildValues(Cursor cursor)
  {
    ContentValues values = new ContentValues();
    for (int i = 0; i < cursor.getColumnCount(); i++)
    {
      if (cursor.isNull(i))
        continue;
      String key = cursor.getColumnName(i);
      if (key.equals(PRIMARY_KEY))
        values.put(key, cursor.getLong(i));
      else
      {
        Object specimen = mManager.getSchema().get(key);
        if (specimen instanceof byte[])
          values.put(key, cursor.getBlob(i));
        else if (specimen instanceof Double)
          values.put(key, cursor.getDouble(i));
        else if (specimen instanceof Float)
          values.put(key, cursor.getFloat(i));
        else if (specimen instanceof Integer)
          values.put(key, cursor.getInt(i));
        else if (specimen instanceof Long)
          values.put(key, cursor.getLong(i));
        else if (specimen instanceof Short)
          values.put(key, cursor.getShort(i));
        else
          values.put(key, cursor.getString(i));
      }
    }
    return values;
  }

  /**
   * Get a cursor on all database rows, all rows where key matches value if specified.
   * @param key optional key to match.
   * @param value optional value to match.
   * @return cursor on all database rows.
   * @throws RuntimeException if an error occurs.
   */
  private Cursor getCursor(String key, Object value) throws RuntimeException
  {
    SQLiteQueryBuilder builder = new SQLiteQueryBuilder();
    builder.setTables(mManager.getTableName());
    String[] selectionArgs;
    if (key == null)
      selectionArgs = null;
    else
    {
      builder.appendWhere(key + " = ?");
      selectionArgs = new String[] { String.valueOf(value.toString()) };
    }
    return builder.query(getDatabase(), null, null, selectionArgs, null, null, null);
  }

  /**
   * Scanner specification.
   */
  public class Scanner implements Iterable<ContentValues>, Closeable
  {
    /** Filter key if any */
    private final String key;

    /** Filter value if any */
    private final Object value;

    /** SQLite cursor */
    private Cursor cursor;

    /** Init cursor with optional filter */
    private Scanner(String key, Object value)
    {
      this.key = key;
      this.value = value;
    }

    @Override
    public void close()
    {
      /* If was using SQLite, close cursor */
      if (cursor != null)
        try
        {
          cursor.close();
        }
        catch (RuntimeException e)
        {
          switchToInMemory("scan", e);
        }
    }

    @Override
    public Iterator<ContentValues> iterator()
    {
      /* Try SQLite */
      if (mIMDB == null)
        try
        {
          /* Open cursor, close previous one if any */
          close();
          cursor = getCursor(key, value);

          /* Wrap cursor as iterator */
          return new Iterator<ContentValues>()
          {
            /** If null, hasNext is not known yet */
            Boolean hasNext;

            @Override
            public boolean hasNext()
            {
              if (hasNext == null)
                try
                {
                  hasNext = cursor.moveToNext();
                }
                catch (RuntimeException e)
                {
                  /* Consider no next on error */
                  hasNext = false;

                  /* Make close do nothing */
                  cursor = null;

                  /* Switch to in memory DB */
                  switchToInMemory("scan", e);
                }
              return hasNext;
            }

            @Override
            public ContentValues next()
            {
              /* Check next */
              if (!hasNext())
                throw new NoSuchElementException();
              hasNext = null;

              /* Build object */
              return buildValues(cursor);
            }

            @Override
            public void remove()
            {
              throw new UnsupportedOperationException();
            }
          };
        }
        catch (RuntimeException e)
        {
          switchToInMemory("scan", e);
        }

      /* Fail over in-memory */
      return new Iterator<ContentValues>()
      {
        /** In memory map iterator that we wrap because of the filter logic */
        Iterator<ContentValues> iterator = mIMDB.values().iterator();

        /** True if we moved the iterator but not retrieved the value */
        boolean advanced;

        /** Next value */
        ContentValues next;

        @Override
        public boolean hasNext()
        {
          /* Move iterator the first time */
          if (!advanced)
          {
            next = null;
            while (iterator.hasNext())
            {
              next = iterator.next();
              if (key == null || value.equals(next.get(key)))
                break;
            }
            advanced = true;
          }
          return next != null;
        }

        @Override
        public ContentValues next()
        {
          if (!hasNext())
            throw new NoSuchElementException();
          advanced = false;
          return next;
        }

        @Override
        public void remove()
        {
          throw new UnsupportedOperationException();
        }
      };
    }
  }

  /**
   * Clear database.
   */
  public void clear()
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        getDatabase().delete(mManager.getTableName(), null, null);
      }
      catch (RuntimeException e)
      {
        switchToInMemory("clear", e);
      }

    /* If failed over in-memory */
    else
      mIMDB.clear();
  }

  @Override
  public void close()
  {
    /* Try SQLite */
    if (mIMDB == null)
      try
      {
        getDatabase().close();
      }
      catch (RuntimeException e)
      {
        switchToInMemory("close", e);
      }

    /* Close in memory database */
    else
    {
      mIMDB.clear();
      mIMDB = null;
    }
  }
}
