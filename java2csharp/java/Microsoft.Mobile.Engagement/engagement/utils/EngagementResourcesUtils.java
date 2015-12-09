/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.utils;

import android.content.Context;
import android.view.View;

/** Utility functions for retrieving resource identifiers by their name. */
public final class EngagementResourcesUtils
{
  private EngagementResourcesUtils()
  {
    /* Prevent instantiation */
  }

  /**
   * Get resource identifier.
   * @param context any application context.
   * @param name resource name.
   * @param defType resource type like "layout" or "id".
   * @return resource identifier or 0 if not found.
   */
  public static int getId(Context context, String name, String defType)
  {
    return context.getResources().getIdentifier(name, defType, context.getPackageName());
  }

  /**
   * Get layout identifier by its resource name.
   * @param context any application context.
   * @param name layout resource name.
   * @return layout identifier or 0 if not found.
   */
  public static int getLayoutId(Context context, String name)
  {
    return getId(context, name, "layout");
  }

  /**
   * Get drawable identifier by its resource name.
   * @param context any application context.
   * @param name drawable resource name.
   * @return drawable identifier or 0 if not found.
   */
  public static int getDrawableId(Context context, String name)
  {
    return getId(context, name, "drawable");
  }

  /**
   * Get identifier by its resource name.
   * @param context any application context.
   * @param name identifier resource name.
   * @return identifier or 0 if not found.
   */
  public static int getId(Context context, String name)
  {
    return getId(context, name, "id");
  }

  /**
   * Get a view by its resource name.
   * @param view ancestor view.
   * @param name view identifier resource name.
   * @return view or 0 if not found.
   */
  @SuppressWarnings("unchecked")
  public static <T extends View> T getView(View view, String name)
  {
    return (T) view.findViewById(getId(view.getContext(), name));
  }
}
