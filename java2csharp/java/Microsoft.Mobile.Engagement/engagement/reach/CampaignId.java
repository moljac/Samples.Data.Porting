/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach;

import android.content.Context;
import android.os.Bundle;

import com.microsoft.azure.engagement.EngagementAgent;

/** Campaign Id (kind + id) */
public class CampaignId
{
  /** Kind */
  public enum Kind
  {
    /** Announcement */
    ANNOUNCEMENT("a"),

    /** Poll */
    POLL("p"),

    /** Data push */
    DATAPUSH("d");

    /** Kind short name: 1 letter */
    private final String shortName;

    /** Init kind */
    private Kind(String shortName)
    {
      this.shortName = shortName;
    }

    /** Parse kind */
    static Kind parse(String ci)
    {
      String value = ci.substring(0, 1);
      for (Kind kind : values())
        if (kind.shortName.equals(value))
          return kind;
      throw new IllegalArgumentException("Invalid kind");
    }

    /**
     * Get kind as 1 letter.
     * @return kind short name.
     */
    public String getShortName()
    {
      return shortName;
    }
  }

  /** Kind */
  private final Kind mKind;

  /** Id */
  private final String mId;

  /** Parse campaign id */
  public CampaignId(String ci)
  {
    mKind = Kind.parse(ci);
    mId = ci.substring(1);
    if (mId.isEmpty())
      throw new IllegalArgumentException("malformed campaign id");
  }

  /**
   * Get kind.
   * @return kind.
   */
  public Kind getKind()
  {
    return mKind;
  }

  /**
   * Get id.
   * @return id.
   */
  public String getId()
  {
    return mId;
  }

  /**
   * Send feedback to Reach.
   * @param context application context.
   * @param status feedback status.
   * @param extras optional feedback payload (like poll answers).
   */
  public void sendFeedBack(Context context, String status, Bundle extras)
  {
    /* Don't send feedback if test campaign */
    if (mId.charAt(0) != '-')
    {
      EngagementAgent agent = EngagementAgent.getInstance(context);
      agent.sendReachFeedback(mKind.getShortName(), mId, status, extras);
    }
  }
}
