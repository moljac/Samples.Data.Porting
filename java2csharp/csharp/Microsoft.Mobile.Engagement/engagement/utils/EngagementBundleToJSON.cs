using System;
using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.utils
{


	using JSONException = org.json.JSONException;
	using JSONStringer = org.json.JSONStringer;

	using Bundle = android.os.Bundle;

	/// <summary>
	/// Utility class to convert an Android <seealso cref="android.os.Bundle Bundle"/> into a JSON string: numbers,
	/// boolean, strings, arrays (and <seealso cref="java.util.ArrayList ArrayList"/>) are converted into the
	/// proper JSON format. Bundles are supported, e.g. a deep conversion is performed. <seealso cref="Throwable"/>
	/// object are formatted like in the console. <seealso cref="android.os.Parcelable Parcelable"/>,
	/// <seealso cref="java.io.Serializable Serializable"/> and <seealso cref="android.util.SparseArray SparseArray"/> are
	/// formatted according to their <seealso cref="#toString()"/> implementation (so it's totally useless to use a
	/// SparseArray since <seealso cref="#toString()"/> is not overridden in it}.
	/// </summary>
	public class EngagementBundleToJSON
	{
	  /// <summary>
	  /// Write an Android <seealso cref="android.os.Bundle Bundle"/> as a JSON string. </summary>
	  /// <param name="bundle"> the Bundle to convert into JSON. </param>
	  /// <returns> the JSON object representing the Bundle as a String. </returns>
	  /// <exception cref="IllegalArgumentException"> if bundle is null. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: public static String toString(android.os.Bundle bundle) throws IllegalArgumentException
	  public static string ToString(Bundle bundle)
	  {
		/* Throw an exception if bundle is null */
		if (bundle == null)
		{
		  throw new System.ArgumentException("bundle can not be null");
		}

		/* Launch JSON serializer */
		JSONStringer json = new JSONStringer();
		try
		{
		  convert(json, bundle);
		}
		catch (JSONException)
		{
		  /* Ignore this key */
		}

		/* Return the JSON string */
		return json.ToString();
	  }

	  /// <summary>
	  /// Recursive function to write a value to JSON. </summary>
	  /// <param name="json"> the JSON serializer. </param>
	  /// <param name="value"> the value to write in JSON. </param>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: private static void convert(org.json.JSONStringer json, Object value) throws org.json.JSONException
	  private static void convert(JSONStringer json, object value)
	  {
		/* Handle null */
		if (value == null)
		{
		  json.value(null);
		}

		/* The function is recursive if it encounters a bundle */
		else if (value is Bundle)
		{
		  /* Cast bundle */
		  Bundle bundle = (Bundle) value;

		  /* Open object */
		  json.@object();

		  /* Traverse bundle */
		  foreach (string key in bundle.Keys)
		  {
			/* Write key */
			json.key(key);

			/* Recursive call to write the value */
			convert(json, bundle.get(key));
		  }

		  /* End object */
		  json.endObject();
		}

		/* Handle array, write it as a JSON array */
		else if (value.GetType().IsArray)
		{
		  /* Open array */
		  json.array();

		  /* Recursive call on each value */
		  int length = Array.getLength(value);
		  for (int i = 0; i < length; i++)
		  {
			convert(json, Array.get(value, i));
		  }

		  /* Close array */
		  json.endArray();
		}

		/* Handle ArrayList, write it as a JSON array */
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: else if (value instanceof java.util.ArrayList<?>)
		else if (value is List<?>)
		{
		  /* Open array */
		  json.array();

		  /* Recursive call on each value */
//JAVA TO C# CONVERTER TODO TASK: Java wildcard generics are not converted to .NET:
//ORIGINAL LINE: java.util.ArrayList<?> arrayList = (java.util.ArrayList<?>) value;
		  List<?> arrayList = (List<?>) value;
		  foreach (object val in arrayList)
		  {
			convert(json, val);
		  }

		  /* Close array */
		  json.endArray();
		}

		/* Format throwable values with the stack trace */
		else if (value is Exception)
		{
		  Exception t = (Exception) value;
		  StringWriter text = new StringWriter();
		  t.printStackTrace(new PrintWriter(text));
		  json.value(text.ToString());
		}

		/* Other values are handled directly by JSONStringer (numerical, boolean and String) */
		else
		{
		  json.value(value);
		}
	  }
	}

}