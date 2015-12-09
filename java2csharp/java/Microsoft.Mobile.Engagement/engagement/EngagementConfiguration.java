/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import android.os.Parcel;
import android.os.Parcelable;

/**
 * Engagement configuration. This contains configuration that can be dynamically changed at runtime.
 * All the fields are optional.
 */
public class EngagementConfiguration implements Parcelable
{
  /** Prefix for keys */
  private static final String PREFIX = "engagement:";

  /** Location report key prefix */
  private static final String LOCATION_REPORT_PREFIX = PREFIX + "locationReport:";

  /** Agent enabled setting */
  public static final String ENABLED = PREFIX + "enabled";

  /** Lazy location report flag */
  public static final String LOCATION_REPORT_LAZY_AREA = LOCATION_REPORT_PREFIX + "lazyArea";

  /** Realtime location report flag */
  public static final String LOCATION_REPORT_REAL_TIME = LOCATION_REPORT_PREFIX + "realTime";

  /** Realtime location report flag (fine mode) */
  public static final String LOCATION_REPORT_REAL_TIME_FINE = LOCATION_REPORT_REAL_TIME + ":fine";

  /** Realtime location report flag (background mode) */
  public static final String LOCATION_REPORT_REAL_TIME_BACKGROUND = LOCATION_REPORT_REAL_TIME
    + ":background";

  /** Parcelable factory */
  public static final Parcelable.Creator<EngagementConfiguration> CREATOR = new Parcelable.Creator<EngagementConfiguration>()
  {
    @Override
    public EngagementConfiguration createFromParcel(Parcel in)
    {
      return new EngagementConfiguration(in);
    }

    @Override
    public EngagementConfiguration[] newArray(int size)
    {
      return new EngagementConfiguration[size];
    }
  };

  /** True serialized in Parcelable */
  private static final byte TRUE = Byte.MAX_VALUE;

  /** False serialized in Parcelable */
  private static final byte FALSE = 0;

  /**
   * Serialize a boolean for Parcelable.
   * @param value boolean to serialize.
   * @return byte value to use in Parcelable.
   */
  private static byte toByte(boolean value)
  {
    return value ? TRUE : FALSE;
  }

  /**
   * Parse a boolean from Parcelable.
   * @param value value to parse from Parcelable.
   * @return parsed boolean.
   */
  private static boolean toBoolean(byte value)
  {
    return value == TRUE;
  }

  /** Connection string */
  private String mConnectionString;

  /** Lazy area location report flag */
  private boolean mLazyAreaLocationReport;

  /** Real time location report flag */
  private boolean mRealtimeLocationReport;

  /** Fine real time location report flag */
  private boolean mFineRealtimeLocationReport;

  /** Background real time location report flag */
  private boolean mBackgroundRealtimeLocationReport;

  /** Init. */
  public EngagementConfiguration()
  {
  }

  /**
   * Unmarshal a parcel.
   * @param in parcel.
   */
  private EngagementConfiguration(Parcel in)
  {
    mConnectionString = in.readString();
    mLazyAreaLocationReport = toBoolean(in.readByte());
    mRealtimeLocationReport = toBoolean(in.readByte());
    mFineRealtimeLocationReport = toBoolean(in.readByte());
    mBackgroundRealtimeLocationReport = toBoolean(in.readByte());
  }

  @Override
  public void writeToParcel(Parcel dest, int flags)
  {
    dest.writeString(mConnectionString);
    dest.writeByte(toByte(mLazyAreaLocationReport));
    dest.writeByte(toByte(mRealtimeLocationReport));
    dest.writeByte(toByte(mFineRealtimeLocationReport));
    dest.writeByte(toByte(mBackgroundRealtimeLocationReport));
  }

  @Override
  public int describeContents()
  {
    return 0;
  }

  /**
   * Get connection string.
   * @return connection string.
   */
  public String getConnectionString()
  {
    return mConnectionString;
  }

  /**
   * Set connection string.
   * @param connectionString connection string.
   */
  public void setConnectionString(String connectionString)
  {
    mConnectionString = connectionString;
  }

  /**
   * Get lazy area location report flag.
   * @return lazy area location report flag.
   */
  public boolean isLazyAreaLocationReport()
  {
    return mLazyAreaLocationReport;
  }

  /**
   * Set lazy area location report flag.
   * @param lazyAreaLocationReport lazy area location report flag.
   */
  public void setLazyAreaLocationReport(boolean lazyAreaLocationReport)
  {
    mLazyAreaLocationReport = lazyAreaLocationReport;
  }

  /**
   * Get real time location report flag.
   * @return real time location report flag.
   */
  public boolean isRealtimeLocationReport()
  {
    return mRealtimeLocationReport;
  }

  /**
   * Set real time location report flag.
   * @param realtimeLocationReport real time location report flag.
   */
  public void setRealtimeLocationReport(boolean realtimeLocationReport)
  {
    mRealtimeLocationReport = realtimeLocationReport;
  }

  /**
   * Get fine real time location report flag.
   * @return fine real time location report flag.
   */
  public boolean isFineRealtimeLocationReport()
  {
    return mFineRealtimeLocationReport;
  }

  /**
   * Set fine real time location report flag.
   * @param fineRealtimeLocationReport fine real time location report flag.
   */
  public void setFineRealtimeLocationReport(boolean fineRealtimeLocationReport)
  {
    mFineRealtimeLocationReport = fineRealtimeLocationReport;
  }

  /**
   * Get background real time location report flag.
   * @return background real time location report flag.
   */
  public boolean isBackgroundRealtimeLocationReport()
  {
    return mBackgroundRealtimeLocationReport;
  }

  /**
   * Set background real time location report flag.
   * @param backgroundRealtimeLocationReport background real time location report flag.
   */
  public void setBackgroundRealtimeLocationReport(boolean backgroundRealtimeLocationReport)
  {
    mBackgroundRealtimeLocationReport = backgroundRealtimeLocationReport;
  }
}
