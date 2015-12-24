/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{

	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using ViewGroup = android.view.ViewGroup;
	using Window = android.view.Window;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;

	using EngagementActivity = com.microsoft.azure.engagement.activity.EngagementActivity;
	using EngagementResourcesUtils = com.microsoft.azure.engagement.utils.EngagementResourcesUtils;

	/// <summary>
	/// Base class for all activities displaying Reach content.
	/// </summary>
	public abstract class EngagementContentActivity<T> : EngagementActivity where T : com.microsoft.azure.engagement.reach.EngagementReachInteractiveContent
	{
	  /// <summary>
	  /// Content of this activity </summary>
	  protected internal T mContent;

	  /// <summary>
	  /// Action button </summary>
	  protected internal TextView mActionButton;

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		/* No title section on the top */
		base.onCreate(savedInstanceState);
		requestWindowFeature(Window.FEATURE_NO_TITLE);

		/* Get content */
		mContent = EngagementReachAgent.getInstance(this).getContent(Intent);
		if (mContent == null)
		{
		  /* If problem with content, exit */
		  finish();
		  return;
		}

		/* Inflate layout */
		ContentView = getLayoutId(LayoutName);

		/* Set title */
		TextView titleView = getView("title");
		string title = mContent.Title;
		if (title != null)
		{
		  titleView.Text = title;
		}
		else
		{
		  titleView.Visibility = View.GONE;
		}

		/* Set body */
		setBody(mContent.Body, getView("body"));

		/* Action button */
		mActionButton = getView("action");
		string actionLabel = mContent.ActionLabel;
		if (actionLabel != null)
		{
		  mActionButton.Text = actionLabel;
		  mActionButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		/* No action label means no action button */
		else
		{
		  mActionButton.Visibility = View.GONE;
		}

		/* Exit button */
		Button exitButton = getView("exit");
		string exitLabel = mContent.ExitLabel;
		if (exitLabel != null)
		{
		  exitButton.Text = exitLabel;
		  exitButton.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		/* No exit label means no exit button */
		else
		{
		  exitButton.Visibility = View.GONE;
		}

		/* Hide spacers if only one button is visible (or none) */
		ViewGroup layout = getView("engagement_button_bar");
		bool hideSpacer = actionLabel == null || exitLabel == null;
		for (int i = 0; i < layout.ChildCount; i++)
		{
		  View view = layout.getChildAt(i);
		  if ("spacer".Equals(view.Tag))
		  {
			if (hideSpacer)
			{
			  view.Visibility = View.VISIBLE;
			}
			else
			{
			  view.Visibility = View.GONE;
			}
		  }
		}

		/* Hide button bar if both action and exit buttons are hidden */
		if (actionLabel == null && exitLabel == null)
		{
		  layout.Visibility = View.GONE;
		}
	  }

	  private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
	  {
		  private readonly EngagementContentActivity<T> outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper(EngagementContentActivity<T> outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public override void onClick(View v)
		  {
			/* Action the content */
			outerInstance.action();
		  }
	  }

	  private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
	  {
		  private readonly EngagementContentActivity<T> outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper2(EngagementContentActivity<T> outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public override void onClick(View v)
		  {
			/* Exit the content */
			outerInstance.exit();
		  }
	  }

	  protected internal override void onResume()
	  {
		/* Mark the content displayed */
		mContent.displayContent(this);
		base.onResume();
	  }

	  protected internal override void onPause()
	  {
		checkExitContent();
		base.onPause();
	  }

	  protected internal override void onDestroy()
	  {
		checkExitContent();
		base.onDestroy();
	  }

	  /// <summary>
	  /// Report exit content to reach. Must be done in onPause to avoid glitch with in app notification
	  /// (reach agent will replay the notification if returning to application from there if we didn't
	  /// report exitContent). But if switching from this activity to another showing activity, onPause
	  /// will be skipped (no history intent flag) so we fail over onDestroy. This scenario can happen
	  /// only from a system notification so we avoid the in-app glitch.
	  /// </summary>
	  private void checkExitContent()
	  {
		if (Finishing && mContent != null)
		{
		  mContent.exitContent(this);
		  mContent = null;
		}
	  }

	  protected internal override void onUserLeaveHint()
	  {
		finish();
	  }

	  /// <summary>
	  /// Render the body of the content into the specified view. </summary>
	  /// <param name="body"> content body. </param>
	  /// <param name="view"> body view. </param>
	  protected internal virtual void setBody(string body, View view)
	  {
		TextView textView = (TextView) view;
		textView.Text = body;
	  }

	  /// <summary>
	  /// Get resource identifier. </summary>
	  /// <param name="name"> resource name. </param>
	  /// <param name="defType"> resource type like "layout" or "id". </param>
	  /// <returns> resource identifier or 0 if not found. </returns>
	  protected internal virtual int getId(string name, string defType)
	  {
		return EngagementResourcesUtils.getId(this, name, defType);
	  }

	  /// <summary>
	  /// Get layout identifier by its resource name. </summary>
	  /// <param name="name"> layout resource name. </param>
	  /// <returns> layout identifier or 0 if not found. </returns>
	  protected internal virtual int getLayoutId(string name)
	  {
		return getId(name, "layout");
	  }

	  /// <summary>
	  /// Get identifier by its resource name. </summary>
	  /// <param name="name"> identifier resource name. </param>
	  /// <returns> identifier or 0 if not found. </returns>
	  protected internal virtual int getId(string name)
	  {
		return getId(name, "id");
	  }

	  /// <summary>
	  /// Get a view by its resource name. </summary>
	  /// <param name="name"> view identifier resource name. </param>
	  /// <returns> view or 0 if not found. </returns>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") protected <V extends android.view.View> V getView(String name)
	  protected internal virtual V getView<V>(string name) where V : android.view.View
	  {
		return (V) findViewById(getId(name));
	  }

	  /// <summary>
	  /// Get layout resource name corresponding to this activity. </summary>
	  /// <returns> layout resource name corresponding to this activity. </returns>
	  protected internal abstract string LayoutName {get;}

	  /// <summary>
	  /// Execute the action if any of the content, report it and finish the activity </summary>
	  protected internal virtual void action()
	  {
		/* Delegate action */
		onAction();

		/* And quit */
		finish();
	  }

	  /// <summary>
	  /// Exit the content and report it </summary>
	  protected internal virtual void exit()
	  {
		/* Quit */
		finish();
	  }

	  /// <summary>
	  /// Called when the action button is clicked.
	  /// </summary>
	  protected internal abstract void onAction();
	}

}