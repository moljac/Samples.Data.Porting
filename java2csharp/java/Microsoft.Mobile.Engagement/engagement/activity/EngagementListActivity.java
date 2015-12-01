/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.activity;

import android.app.ListActivity;
import android.os.Bundle;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;

import com.microsoft.azure.engagement.EngagementAgent;
import com.microsoft.azure.engagement.EngagementAgentUtils;

/**
 * Helper class used to replace Android's android.app.ListActivity class.
 */
public abstract class EngagementListActivity extends ListActivity
{
  private EngagementAgent mEngagementAgent;

  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    super.onCreate(savedInstanceState);
    mEngagementAgent = EngagementAgent.getInstance(this);

    /* FIXME temporary empty adapter to avoid side effects with Reach */
    if (getListAdapter() == null)
    {
      /* This will trigger required initialization */
      setListAdapter(new BaseAdapter()
      {
        public View getView(int position, View convertView, ViewGroup parent)
        {
          return null;
        }

        public long getItemId(int position)
        {
          return 0;
        }

        public Object getItem(int position)
        {
          return null;
        }

        public int getCount()
        {
          return 0;
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
