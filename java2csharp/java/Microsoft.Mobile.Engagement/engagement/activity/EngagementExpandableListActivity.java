/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.activity;

import android.app.ExpandableListActivity;
import android.os.Bundle;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;

import com.microsoft.azure.engagement.EngagementAgent;
import com.microsoft.azure.engagement.EngagementAgentUtils;

/**
 * Helper class used to replace Android's android.app.ExpandableListActivity
 * class.
 */
public abstract class EngagementExpandableListActivity extends ExpandableListActivity
{
  private EngagementAgent mEngagementAgent;

  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    super.onCreate(savedInstanceState);
    mEngagementAgent = EngagementAgent.getInstance(this);

    /* FIXME temporary empty adapter to avoid side effects with Reach */
    if (getExpandableListAdapter() == null)
    {
      /* This will trigger required initialization */
      setListAdapter(new BaseExpandableListAdapter()
      {
        public boolean isChildSelectable(int groupPosition, int childPosition)
        {
          return false;
        }

        public boolean hasStableIds()
        {
          return false;
        }

        public View getGroupView(int groupPosition, boolean isExpanded, View convertView,
          ViewGroup parent)
        {
          return null;
        }

        public long getGroupId(int groupPosition)
        {
          return 0;
        }

        public int getGroupCount()
        {
          return 0;
        }

        public Object getGroup(int groupPosition)
        {
          return null;
        }

        public int getChildrenCount(int groupPosition)
        {
          return 0;
        }

        public View getChildView(int groupPosition, int childPosition, boolean isLastChild,
          View convertView, ViewGroup parent)
        {
          return null;
        }

        public long getChildId(int groupPosition, int childPosition)
        {
          return 0;
        }

        public Object getChild(int groupPosition, int childPosition)
        {
          return null;
        }
      });

      /*
       * We can now safely reset the adapter to null to avoid side effect with
       * 3rd party code testing the null pointer.
       */
      setListAdapter(null);
    }
  }

  @Override
  protected void onResume()
  {
    mEngagementAgent.startActivity(this, getEngagementActivityName(), getEngagementActivityExtra());
    super.onResume();
  }

  @Override
  protected void onPause()
  {
    mEngagementAgent.endActivity();
    super.onPause();
  }

  /**
   * Get the Engagement agent attached to this activity.
   * @return the Engagement agent
   */
  public final EngagementAgent getEngagementAgent()
  {
    return mEngagementAgent;
  }

  /**
   * Override this to specify the name reported by your activity. The default
   * implementation returns the simple name of the class and removes the
   * "Activity" suffix if any (e.g. "com.mycompany.MainActivity" -> "Main").
   * @return the activity name reported by the Engagement service.
   */
  protected String getEngagementActivityName()
  {
    return EngagementAgentUtils.buildEngagementActivityName(getClass());
  }

  /**
   * Override this to attach extra information to your activity. The default
   * implementation attaches no extra information (i.e. return null).
   * @return activity extra information, null or empty if no extra.
   */
  protected Bundle getEngagementActivityExtra()
  {
    return null;
  }
}
