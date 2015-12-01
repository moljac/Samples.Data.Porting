/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.activity;

import android.os.Bundle;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.Button;
import android.widget.TextView;

import com.microsoft.azure.engagement.activity.EngagementActivity;
import com.microsoft.azure.engagement.reach.EngagementReachAgent;
import com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent;
import com.microsoft.azure.engagement.utils.EngagementResourcesUtils;

/**
 * Base class for all activities displaying Reach content.
 */
public abstract class EngagementContentActivity<T extends EngagementReachInteractiveContent>
  extends EngagementActivity
{
  /** Content of this activity */
  protected T mContent;

  /** Action button */
  protected TextView mActionButton;

  @Override
  protected void onCreate(Bundle savedInstanceState)
  {
    /* No title section on the top */
    super.onCreate(savedInstanceState);
    requestWindowFeature(Window.FEATURE_NO_TITLE);

    /* Get content */
    mContent = EngagementReachAgent.getInstance(this).getContent(getIntent());
    if (mContent == null)
    {
      /* If problem with content, exit */
      finish();
      return;
    }

    /* Inflate layout */
    setContentView(getLayoutId(getLayoutName()));

    /* Set title */
    TextView titleView = getView("title");
    String title = mContent.getTitle();
    if (title != null)
      titleView.setText(title);
    else
      titleView.setVisibility(View.GONE);

    /* Set body */
    setBody(mContent.getBody(), getView("body"));

    /* Action button */
    mActionButton = getView("action");
    String actionLabel = mContent.getActionLabel();
    if (actionLabel != null)
    {
      mActionButton.setText(actionLabel);
      mActionButton.setOnClickListener(new OnClickListener()
      {
        @Override
        public void onClick(View v)
        {
          /* Action the content */
          action();
        }
      });
    }

    /* No action label means no action button */
    else
      mActionButton.setVisibility(View.GONE);

    /* Exit button */
    Button exitButton = getView("exit");
    String exitLabel = mContent.getExitLabel();
    if (exitLabel != null)
    {
      exitButton.setText(exitLabel);
      exitButton.setOnClickListener(new OnClickListener()
      {
        @Override
        public void onClick(View v)
        {
          /* Exit the content */
          exit();
        }
      });
    }

    /* No exit label means no exit button */
    else
      exitButton.setVisibility(View.GONE);

    /* Hide spacers if only one button is visible (or none) */
    ViewGroup layout = getView("engagement_button_bar");
    boolean hideSpacer = actionLabel == null || exitLabel == null;
    for (int i = 0; i < layout.getChildCount(); i++)
    {
      View view = layout.getChildAt(i);
      if ("spacer".equals(view.getTag()))
        if (hideSpacer)
          view.setVisibility(View.VISIBLE);
        else
          view.setVisibility(View.GONE);
    }

    /* Hide button bar if both action and exit buttons are hidden */
    if (actionLabel == null && exitLabel == null)
      layout.setVisibility(View.GONE);
  }

  @Override
  protected void onResume()
  {
    /* Mark the content displayed */
    mContent.displayContent(this);
    super.onResume();
  }

  @Override
  protected void onPause()
  {
    checkExitContent();
    super.onPause();
  }

  @Override
  protected void onDestroy()
  {
    checkExitContent();
    super.onDestroy();
  }

  /**
   * Report exit content to reach. Must be done in onPause to avoid glitch with in app notification
   * (reach agent will replay the notification if returning to application from there if we didn't
   * report exitContent). But if switching from this activity to another showing activity, onPause
   * will be skipped (no history intent flag) so we fail over onDestroy. This scenario can happen
   * only from a system notification so we avoid the in-app glitch.
   */
  private void checkExitContent()
  {
    if (isFinishing() && mContent != null)
    {
      mContent.exitContent(this);
      mContent = null;
    }
  }

  @Override
  protected void onUserLeaveHint()
  {
    finish();
  }

  /**
   * Render the body of the content into the specified view.
   * @param body content body.
   * @param view body view.
   */
  protected void setBody(String body, View view)
  {
    TextView textView = (TextView) view;
    textView.setText(body);
  }

  /**
   * Get resource identifier.
   * @param name resource name.
   * @param defType resource type like "layout" or "id".
   * @return resource identifier or 0 if not found.
   */
  protected int getId(String name, String defType)
  {
    return EngagementResourcesUtils.getId(this, name, defType);
  }

  /**
   * Get layout identifier by its resource name.
   * @param name layout resource name.
   * @return layout identifier or 0 if not found.
   */
  protected int getLayoutId(String name)
  {
    return getId(name, "layout");
  }

  /**
   * Get identifier by its resource name.
   * @param name identifier resource name.
   * @return identifier or 0 if not found.
   */
  protected int getId(String name)
  {
    return getId(name, "id");
  }

  /**
   * Get a view by its resource name.
   * @param name view identifier resource name.
   * @return view or 0 if not found.
   */
  @SuppressWarnings("unchecked")
  protected <V extends View> V getView(String name)
  {
    return (V) findViewById(getId(name));
  }

  /**
   * Get layout resource name corresponding to this activity.
   * @return layout resource name corresponding to this activity.
   */
  protected abstract String getLayoutName();

  /** Execute the action if any of the content, report it and finish the activity */
  protected void action()
  {
    /* Delegate action */
    onAction();

    /* And quit */
    finish();
  }

  /** Exit the content and report it */
  protected void exit()
  {
    /* Quit */
    finish();
  }

  /**
   * Called when the action button is clicked.
   */
  protected abstract void onAction();
}
