/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.utils
{

	using Context = android.content.Context;
	using View = android.view.View;

	/// <summary>
	/// Utility functions for retrieving resource identifiers by their name. </summary>
	public sealed class EngagementResourcesUtils
	{
	  private EngagementResourcesUtils()
	  {
		/* Prevent instantiation */
	  }

	  /// <summary>
	  /// Get resource identifier. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="name"> resource name. </param>
	  /// <param name="defType"> resource type like "layout" or "id". </param>
	  /// <returns> resource identifier or 0 if not found. </returns>
	  public static int getId(Context context, string name, string defType)
	  {
		return context.Resources.getIdentifier(name, defType, context.PackageName);
	  }

	  /// <summary>
	  /// Get layout identifier by its resource name. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="name"> layout resource name. </param>
	  /// <returns> layout identifier or 0 if not found. </returns>
	  public static int getLayoutId(Context context, string name)
	  {
		return getId(context, name, "layout");
	  }

	  /// <summary>
	  /// Get drawable identifier by its resource name. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="name"> drawable resource name. </param>
	  /// <returns> drawable identifier or 0 if not found. </returns>
	  public static int getDrawableId(Context context, string name)
	  {
		return getId(context, name, "drawable");
	  }

	  /// <summary>
	  /// Get identifier by its resource name. </summary>
	  /// <param name="context"> any application context. </param>
	  /// <param name="name"> identifier resource name. </param>
	  /// <returns> identifier or 0 if not found. </returns>
	  public static int getId(Context context, string name)
	  {
		return getId(context, name, "id");
	  }

	  /// <summary>
	  /// Get a view by its resource name. </summary>
	  /// <param name="view"> ancestor view. </param>
	  /// <param name="name"> view identifier resource name. </param>
	  /// <returns> view or 0 if not found. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <T extends android.view.View> T getView(android.view.View view, String name)
	  public static T getView<T>(View view, string name) where T : android.view.View
	  {
		return (T) view.findViewById(getId(view.Context, name));
	  }
	}

}