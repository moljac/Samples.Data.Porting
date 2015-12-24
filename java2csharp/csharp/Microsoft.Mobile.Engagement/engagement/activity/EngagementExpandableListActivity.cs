/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.activity
{

	using ExpandableListActivity = android.app.ExpandableListActivity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseExpandableListAdapter = android.widget.BaseExpandableListAdapter;


	/// <summary>
	/// Helper class used to replace Android's android.app.ExpandableListActivity
	/// class.
	/// </summary>
	public abstract class EngagementExpandableListActivity : ExpandableListActivity
	{
	  private EngagementAgent mEngagementAgent;

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		mEngagementAgent = EngagementAgent.getInstance(this);

		/* FIXME temporary empty adapter to avoid side effects with Reach */
		if (ExpandableListAdapter == null)
		{
		  /* This will trigger required initialization */
		  ListAdapter = new BaseExpandableListAdapterAnonymousInnerClassHelper(this);

		  /*
		   * We can now safely reset the adapter to null to avoid side effect with
		   * 3rd party code testing the null pointer.
		   */
		  ListAdapter = null;
		}
	  }

	  private class BaseExpandableListAdapterAnonymousInnerClassHelper : BaseExpandableListAdapter
	  {
		  private readonly EngagementExpandableListActivity outerInstance;

		  public BaseExpandableListAdapterAnonymousInnerClassHelper(EngagementExpandableListActivity outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual bool isChildSelectable(int groupPosition, int childPosition)
		  {
			return false;
		  }

		  public virtual bool hasStableIds()
		  {
			return false;
		  }

		  public virtual View getGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
		  {
			return null;
		  }

		  public virtual long getGroupId(int groupPosition)
		  {
			return 0;
		  }

		  public virtual int GroupCount
		  {
			  get
			  {
				return 0;
			  }
		  }

		  public virtual object getGroup(int groupPosition)
		  {
			return null;
		  }

		  public virtual int getChildrenCount(int groupPosition)
		  {
			return 0;
		  }

		  public virtual View getChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
		  {
			return null;
		  }

		  public virtual long getChildId(int groupPosition, int childPosition)
		  {
			return 0;
		  }

		  public virtual object getChild(int groupPosition, int childPosition)
		  {
			return null;
		  }
	  }

	  protected internal override void onResume()
	  {
		mEngagementAgent.startActivity(this, EngagementActivityName, EngagementActivityExtra);
		base.onResume();
	  }

	  protected internal override void onPause()
	  {
		mEngagementAgent.endActivity();
		base.onPause();
	  }

	  /// <summary>
	  /// Get the Engagement agent attached to this activity. </summary>
	  /// <returns> the Engagement agent </returns>
	  public EngagementAgent EngagementAgent
	  {
		  get
		  {
			return mEngagementAgent;
		  }
	  }

	  /// <summary>
	  /// Override this to specify the name reported by your activity. The default
	  /// implementation returns the simple name of the class and removes the
	  /// "Activity" suffix if any (e.g. "com.mycompany.MainActivity" -> "Main"). </summary>
	  /// <returns> the activity name reported by the Engagement service. </returns>
	  protected internal virtual string EngagementActivityName
	  {
		  get
		  {
			return EngagementAgentUtils.buildEngagementActivityName(this.GetType());
		  }
	  }

	  /// <summary>
	  /// Override this to attach extra information to your activity. The default
	  /// implementation attaches no extra information (i.e. return null). </summary>
	  /// <returns> activity extra information, null or empty if no extra. </returns>
	  protected internal virtual Bundle EngagementActivityExtra
	  {
		  get
		  {
			return null;
		  }
	  }
	}

}