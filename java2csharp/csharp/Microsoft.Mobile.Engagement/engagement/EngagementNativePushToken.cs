using System;
using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

	using Parcel = android.os.Parcel;
	using Parcelable = android.os.Parcelable;

	/// <summary>
	/// Native push registration with Engagement </summary>
	public class EngagementNativePushToken : Parcelable
	{
	  /// <summary>
	  /// Native push service type </summary>
	  public sealed class Type
	  {
		public static readonly Type GCM = new Type("GCM", InnerEnum.GCM);
		public static readonly Type ADM = new Type("ADM", InnerEnum.ADM);

		private static readonly IList<Type> valueList = new List<Type>();

		static Type()
		{
			valueList.Add(GCM);
			valueList.Add(ADM);
		}

		public enum InnerEnum
		{
			GCM,
			ADM
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		private Type(string name, InnerEnum innerEnum)
		{
			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		public override string ToString()
		{
		  return name().toLowerCase(Locale.US);
		};

		  public static IList<Type> values()
		  {
			  return valueList;
		  }

		  public InnerEnum InnerEnumValue()
		  {
			  return innerEnumValue;
		  }

		  public int ordinal()
		  {
			  return ordinalValue;
		  }

		  public static Type valueOf(string name)
		  {
			  foreach (Type enumInstance in Type.values())
			  {
				  if (enumInstance.nameValue == name)
				  {
					  return enumInstance;
				  }
			  }
			  throw new System.ArgumentException(name);
		  }
	  }

	  /// <summary>
	  /// Parcelable factory </summary>
	  public static readonly Parcelable.Creator<EngagementNativePushToken> CREATOR = new CreatorAnonymousInnerClassHelper();

	  private class CreatorAnonymousInnerClassHelper : Parcelable.Creator<EngagementNativePushToken>
	  {
		  public CreatorAnonymousInnerClassHelper()
		  {
		  }

		  public override EngagementNativePushToken createFromParcel(Parcel @in)
		  {
			return new EngagementNativePushToken(@in);
		  }

		  public override EngagementNativePushToken[] newArray(int size)
		  {
			return new EngagementNativePushToken[size];
		  }
	  }

	  /// <summary>
	  /// Token value (registration identifier) </summary>
	  private readonly string token;

	  /// <summary>
	  /// Token type </summary>
	  private readonly Type type;

	  /// <summary>
	  /// Init. </summary>
	  /// <param name="token"> registration identifier. </param>
	  /// <param name="type"> service type. </param>
	  public EngagementNativePushToken(string token, Type type)
	  {
		this.token = token;
		this.type = type;
	  }

	  /// <summary>
	  /// Unmarshal a parcel. </summary>
	  /// <param name="in"> parcel. </param>
	  private EngagementNativePushToken(Parcel @in)
	  {
		token = @in.readString();
		type = typeFromInt(@in.readInt());
	  }

	  /// <summary>
	  /// Get token value. </summary>
	  /// <returns> token value. </returns>
	  public virtual string Token
	  {
		  get
		  {
			return token;
		  }
	  }

	  /// <summary>
	  /// Get token native push service type. </summary>
	  /// <returns> token native push service type. </returns>
	  public virtual Type getType()
	  {
		return type;
	  }

	  public override void writeToParcel(Parcel dest, int flags)
	  {
		dest.writeString(token);
		dest.writeInt(type == null ? - 1 : type.ordinal());
	  }

	  public override int describeContents()
	  {
		return 0;
	  }

	  /// <summary>
	  /// Get token type from integer (enum ordinal). </summary>
	  /// <param name="ordinal"> enum ordinal. </param>
	  /// <returns> token type, or null if ordinal is invalid. </returns>
	  public static Type typeFromInt(int ordinal)
	  {
		Type[] values = Type.values();
		return ordinal >= 0 && ordinal < values.Length ? values[ordinal] : null;
	  }
	}

}