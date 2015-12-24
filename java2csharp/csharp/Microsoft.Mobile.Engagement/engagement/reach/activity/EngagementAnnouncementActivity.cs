using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{

	using Intent = android.content.Intent;

	/// <summary>
	/// Base class for all announcement activities.
	/// </summary>
	public abstract class EngagementAnnouncementActivity : EngagementContentActivity<EngagementAnnouncement>
	{
	  /// <summary>
	  /// Track if we already executed an action, we want to do it only once (in web announcements we can
	  /// have an action URL and links in web view which count also as actions).
	  /// </summary>
	  private bool mActioned;

	  protected internal override void onAction()
	  {
		/* Report action */
		mContent.actionContent(ApplicationContext);

		/* Action the URL if specified */
		string url = mContent.ActionURL;
		if (url != null)
		{
		  executeActionURL(url);
		}
	  }

	  /// <summary>
	  /// Execute action URL. Only the first call will be processed (whatever the URL). Other calls are
	  /// ignored. </summary>
	  /// <param name="url"> action URL (not null). </param>
	  protected internal virtual void executeActionURL(string url)
	  {
		if (!mActioned)
		{
		  try
		  {
			startActivity(Intent.parseUri(url, 0));
			mActioned = true;
		  }
		  catch (Exception)
		  {
			/* Ignore */
		  }
		}
	  }
	}

}