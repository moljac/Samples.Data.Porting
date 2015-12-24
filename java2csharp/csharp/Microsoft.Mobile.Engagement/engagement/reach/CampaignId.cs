using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach
{

	using Context = android.content.Context;
	using Bundle = android.os.Bundle;

	/// <summary>
	/// Campaign Id (kind + id) </summary>
	public class CampaignId
	{
	  /// <summary>
	  /// Kind </summary>
	  public sealed class Kind
	  {
		/// <summary>
		/// Announcement </summary>
		public static readonly Kind ANNOUNCEMENT = new Kind("ANNOUNCEMENT", InnerEnum.ANNOUNCEMENT, "a");

		/// <summary>
		/// Poll </summary>
		public static readonly Kind POLL = new Kind("POLL", InnerEnum.POLL, "p");

		/// <summary>
		/// Data push </summary>
		public static readonly Kind DATAPUSH = new Kind("DATAPUSH", InnerEnum.DATAPUSH, "d");

		private static readonly IList<Kind> valueList = new List<Kind>();

		static Kind()
		{
			valueList.Add(ANNOUNCEMENT);
			valueList.Add(POLL);
			valueList.Add(DATAPUSH);
		}

		public enum InnerEnum
		{
			ANNOUNCEMENT,
			POLL,
			DATAPUSH
		}

		private readonly string nameValue;
		private readonly int ordinalValue;
		private readonly InnerEnum innerEnumValue;
		private static int nextOrdinal = 0;

		/// <summary>
		/// Kind short name: 1 letter </summary>
		internal readonly string shortName;

		/// <summary>
		/// Init kind </summary>
		internal Kind(string name, InnerEnum innerEnum, string shortName)
		{
		  this.shortName = shortName;

			nameValue = name;
			ordinalValue = nextOrdinal++;
			innerEnumValue = innerEnum;
		}

		/// <summary>
		/// Parse kind </summary>
		internal static Kind parse(string ci)
		{
		  string value = ci.Substring(0, 1);
		  foreach (Kind kind in values())
		  {
			if (kind.shortName.Equals(value))
			{
			  return kind;
			}
		  }
		  throw new System.ArgumentException("Invalid kind");
		}

		/// <summary>
		/// Get kind as 1 letter. </summary>
		/// <returns> kind short name. </returns>
		public string ShortName
		{
			get
			{
			  return shortName;
			}
		}

		  public static IList<Kind> values()
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

		  public override string ToString()
		  {
			  return nameValue;
		  }

		  public static Kind valueOf(string name)
		  {
			  foreach (Kind enumInstance in Kind.values())
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
	  /// Kind </summary>
	  private readonly Kind mKind;

	  /// <summary>
	  /// Id </summary>
	  private readonly string mId;

	  /// <summary>
	  /// Parse campaign id </summary>
	  public CampaignId(string ci)
	  {
		mKind = Kind.parse(ci);
		mId = ci.Substring(1);
		if (mId.Length == 0)
		{
		  throw new System.ArgumentException("malformed campaign id");
		}
	  }

	  /// <summary>
	  /// Get kind. </summary>
	  /// <returns> kind. </returns>
	  public virtual Kind getKind()
	  {
		return mKind;
	  }

	  /// <summary>
	  /// Get id. </summary>
	  /// <returns> id. </returns>
	  public virtual string Id
	  {
		  get
		  {
			return mId;
		  }
	  }

	  /// <summary>
	  /// Send feedback to Reach. </summary>
	  /// <param name="context"> application context. </param>
	  /// <param name="status"> feedback status. </param>
	  /// <param name="extras"> optional feedback payload (like poll answers). </param>
	  public virtual void sendFeedBack(Context context, string status, Bundle extras)
	  {
		/* Don't send feedback if test campaign */
		if (mId[0] != '-')
		{
		  EngagementAgent agent = EngagementAgent.getInstance(context);
		  agent.sendReachFeedback(mKind.ShortName, mId, status, extras);
		}
	  }
	}

}