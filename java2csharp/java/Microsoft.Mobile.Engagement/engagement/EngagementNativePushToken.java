/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import java.util.Locale;

import android.os.Parcel;
import android.os.Parcelable;

/** Native push registration with Engagement */
public class EngagementNativePushToken implements Parcelable
{
  /** Native push service type */
  public enum Type
  {
    GCM,
    ADM;

    @Override
    public String toString()
    {
      return name().toLowerCase(Locale.US);
    };
  }

  /** Parcelable factory */
  public static final Parcelable.Creator<EngagementNativePushToken> CREATOR = new Parcelable.Creator<EngagementNativePushToken>()
  {
    @Override
    public EngagementNativePushToken createFromParcel(Parcel in)
    {
      return new EngagementNativePushToken(in);
    }

    @Override
    public EngagementNativePushToken[] newArray(int size)
    {
      return new EngagementNativePushToken[size];
    }
  };

  /** Token value (registration identifier) */
  private final String token;

  /** Token type */
  private final Type type;

  /**
   * Init.
   * @param token registration identifier.
   * @param type service type.
   */
  public EngagementNativePushToken(String token, Type type)
  {
    this.token = token;
    this.type = type;
  }

  /**
   * Unmarshal a parcel.
   * @param in parcel.
   */
  private EngagementNativePushToken(Parcel in)
  {
    token = in.readString();
    type = typeFromInt(in.readInt());
  }

  /**
   * Get token value.
   * @return token value.
   */
  public String getToken()
  {
    return token;
  }

  /**
   * Get token native push service type.
   * @return token native push service type.
   */
  public Type getType()
  {
    return type;
  }

  @Override
  public void writeToParcel(Parcel dest, int flags)
  {
    dest.writeString(token);
    dest.writeInt(type == null ? -1 : type.ordinal());
  }

  @Override
  public int describeContents()
  {
    return 0;
  }

  /**
   * Get token type from integer (enum ordinal).
   * @param ordinal enum ordinal.
   * @return token type, or null if ordinal is invalid.
   */
  public static Type typeFromInt(int ordinal)
  {
    Type[] values = Type.values();
    return ordinal >= 0 && ordinal < values.length ? values[ordinal] : null;
  }
}
