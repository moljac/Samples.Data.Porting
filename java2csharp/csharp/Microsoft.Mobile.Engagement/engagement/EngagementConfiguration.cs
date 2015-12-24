/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

	using Parcel = android.os.Parcel;
	using Parcelable = android.os.Parcelable;

	/// <summary>
	/// Engagement configuration. This contains configuration that can be dynamically changed at runtime.
	/// All the fields are optional.
	/// </summary>
	public class EngagementConfiguration : Parcelable
	{
	  /// <summary>
	  /// Prefix for keys </summary>
	  private const string PREFIX = "engagement:";

	  /// <summary>
	  /// Location report key prefix </summary>
	  private static readonly string LOCATION_REPORT_PREFIX = PREFIX + "locationReport:";

	  /// <summary>
	  /// Agent enabled setting </summary>
	  public static readonly string ENABLED = PREFIX + "enabled";

	  /// <summary>
	  /// Lazy location report flag </summary>
	  public static readonly string LOCATION_REPORT_LAZY_AREA = LOCATION_REPORT_PREFIX + "lazyArea";

	  /// <summary>
	  /// Realtime location report flag </summary>
	  public static readonly string LOCATION_REPORT_REAL_TIME = LOCATION_REPORT_PREFIX + "realTime";

	  /// <summary>
	  /// Realtime location report flag (fine mode) </summary>
	  public static readonly string LOCATION_REPORT_REAL_TIME_FINE = LOCATION_REPORT_REAL_TIME + ":fine";

	  /// <summary>
	  /// Realtime location report flag (background mode) </summary>
	  public static readonly string LOCATION_REPORT_REAL_TIME_BACKGROUND = LOCATION_REPORT_REAL_TIME + ":background";

	  /// <summary>
	  /// Parcelable factory </summary>
	  public static readonly Parcelable.Creator<EngagementConfiguration> CREATOR = new CreatorAnonymousInnerClassHelper();

	  private class CreatorAnonymousInnerClassHelper : Parcelable.Creator<EngagementConfiguration>
	  {
		  public CreatorAnonymousInnerClassHelper()
		  {
		  }

		  public override EngagementConfiguration createFromParcel(Parcel @in)
		  {
			return new EngagementConfiguration(@in);
		  }

		  public override EngagementConfiguration[] newArray(int size)
		  {
			return new EngagementConfiguration[size];
		  }
	  }

	  /// <summary>
	  /// True serialized in Parcelable </summary>
	  private static readonly sbyte TRUE = SByte.MaxValue;

	  /// <summary>
	  /// False serialized in Parcelable </summary>
	  private const sbyte FALSE = 0;

	  /// <summary>
	  /// Serialize a boolean for Parcelable. </summary>
	  /// <param name="value"> boolean to serialize. </param>
	  /// <returns> byte value to use in Parcelable. </returns>
	  private static sbyte toByte(bool value)
	  {
		return value ? TRUE : FALSE;
	  }

	  /// <summary>
	  /// Parse a boolean from Parcelable. </summary>
	  /// <param name="value"> value to parse from Parcelable. </param>
	  /// <returns> parsed boolean. </returns>
	  private static bool toBoolean(sbyte value)
	  {
		return value == TRUE;
	  }

	  /// <summary>
	  /// Connection string </summary>
	  private string mConnectionString;

	  /// <summary>
	  /// Lazy area location report flag </summary>
	  private bool mLazyAreaLocationReport;

	  /// <summary>
	  /// Real time location report flag </summary>
	  private bool mRealtimeLocationReport;

	  /// <summary>
	  /// Fine real time location report flag </summary>
	  private bool mFineRealtimeLocationReport;

	  /// <summary>
	  /// Background real time location report flag </summary>
	  private bool mBackgroundRealtimeLocationReport;

	  /// <summary>
	  /// Init. </summary>
	  public EngagementConfiguration()
	  {
	  }

	  /// <summary>
	  /// Unmarshal a parcel. </summary>
	  /// <param name="in"> parcel. </param>
	  private EngagementConfiguration(Parcel @in)
	  {
		mConnectionString = @in.readString();
		mLazyAreaLocationReport = toBoolean(@in.readByte());
		mRealtimeLocationReport = toBoolean(@in.readByte());
		mFineRealtimeLocationReport = toBoolean(@in.readByte());
		mBackgroundRealtimeLocationReport = toBoolean(@in.readByte());
	  }

	  public override void writeToParcel(Parcel dest, int flags)
	  {
		dest.writeString(mConnectionString);
		dest.writeByte(toByte(mLazyAreaLocationReport));
		dest.writeByte(toByte(mRealtimeLocationReport));
		dest.writeByte(toByte(mFineRealtimeLocationReport));
		dest.writeByte(toByte(mBackgroundRealtimeLocationReport));
	  }

	  public override int describeContents()
	  {
		return 0;
	  }

	  /// <summary>
	  /// Get connection string. </summary>
	  /// <returns> connection string. </returns>
	  public virtual string ConnectionString
	  {
		  get
		  {
			return mConnectionString;
		  }
		  set
		  {
			mConnectionString = value;
		  }
	  }


	  /// <summary>
	  /// Get lazy area location report flag. </summary>
	  /// <returns> lazy area location report flag. </returns>
	  public virtual bool LazyAreaLocationReport
	  {
		  get
		  {
			return mLazyAreaLocationReport;
		  }
		  set
		  {
			mLazyAreaLocationReport = value;
		  }
	  }


	  /// <summary>
	  /// Get real time location report flag. </summary>
	  /// <returns> real time location report flag. </returns>
	  public virtual bool RealtimeLocationReport
	  {
		  get
		  {
			return mRealtimeLocationReport;
		  }
		  set
		  {
			mRealtimeLocationReport = value;
		  }
	  }


	  /// <summary>
	  /// Get fine real time location report flag. </summary>
	  /// <returns> fine real time location report flag. </returns>
	  public virtual bool FineRealtimeLocationReport
	  {
		  get
		  {
			return mFineRealtimeLocationReport;
		  }
		  set
		  {
			mFineRealtimeLocationReport = value;
		  }
	  }


	  /// <summary>
	  /// Get background real time location report flag. </summary>
	  /// <returns> background real time location report flag. </returns>
	  public virtual bool BackgroundRealtimeLocationReport
	  {
		  get
		  {
			return mBackgroundRealtimeLocationReport;
		  }
		  set
		  {
			mBackgroundRealtimeLocationReport = value;
		  }
	  }

	}

}