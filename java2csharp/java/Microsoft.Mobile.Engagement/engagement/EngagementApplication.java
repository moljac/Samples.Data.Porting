/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import android.app.Application;
import android.content.res.Configuration;

/**
 * Helper class used to replace Android's {@link Application} class.<br/>
 * If you currently extend the {@link Application} class, please make your class extend this class
 * instead. Your {@link #onCreate()} function has to be renamed
 * {@link #onApplicationProcessCreate()} and the same rule applies for the other callbacks. Make
 * sure to also rename the calls to the super methods the same way to avoid an infinite loop (which
 * will trigger a <tt>java.lang.StackOverflowError</tt>).<br/>
 * These new methods are called only if the current process is not dedicated to the Engagement service,
 * avoiding unnecessary initialization in that process.<br/>
 * If you use an Application sub-class but you don't want to extend this class, you can use directly
 * {@link EngagementAgentUtils#isInDedicatedEngagementProcess(android.content.Context)} and execute your legacy
 * code only if this method return <tt>false</tt>.
 * @see EngagementAgentUtils#isInDedicatedEngagementProcess(android.content.Context)
 */
public abstract class EngagementApplication extends Application
{
  @Override
  public final void onCreate()
  {
    if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
      onApplicationProcessCreate();
  }

  @Override
  public final void onTerminate()
  {
    if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
      onApplicationProcessTerminate();
  }

  @Override
  public final void onLowMemory()
  {
    if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
      onApplicationProcessLowMemory();
  }

  @Override
  public final void onConfigurationChanged(Configuration newConfig)
  {
    if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
      onApplicationProcessConfigurationChanged(newConfig);
  }

  /**
   * Override this method instead of {@link #onCreate()} to avoid doing unnecessary operations when
   * the current process is the one dedicated to the Engagement service.
   */
  protected void onApplicationProcessCreate()
  {
    /* Sub-class template method */
  }

  /**
   * Override this method instead of {@link #onTerminate()} to avoid doing unnecessary operations
   * when the current process is the one dedicated to the Engagement service.
   */
  protected void onApplicationProcessTerminate()
  {
    /* Sub-class template method */
  }

  /**
   * Override this method instead of {@link #onLowMemory()} to avoid doing unnecessary operations
   * when the current process is the one dedicated to the Engagement service.
   */
  protected void onApplicationProcessLowMemory()
  {
    /* Sub-class template method */
  }

  /**
   * Override this method instead of {@link #onConfigurationChanged(Configuration)} to avoid doing
   * unnecessary operations when the current process is the one dedicated to the Engagement service.
   */
  protected void onApplicationProcessConfigurationChanged(Configuration newConfig)
  {
    /* Sub-class template method */
  }
}
