/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.activity
{

	using ListActivity = android.app.ListActivity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using BaseAdapter = android.widget.BaseAdapter;


	/// <summary>
	/// Helper class used to replace Android's android.app.ListActivity class.
	/// </summary>
	public abstract class EngagementListActivity : ListActivity
	{
	  private EngagementAgent mEngagementAgent;

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		mEngagementAgent = EngagementAgent.getInstance(this);

		/* FIXME temporary empty adapter to avoid side effects with Reach */
		if (ListAdapter == null)
		{
		  /* This will trigger required initialization */
		  ListAdapter = new BaseAdapterAnonymousInnerClassHelper(this);

		  /*
		   * We can now safely reset the adapter to null to avoid side effect with
		   * 3rd party code testing the null pointer.
		   */
		  ListAdapter = null;
		}
	  }

	  private class BaseAdapterAnonymousInnerClassHelper : BaseAdapter
	  {
		  private readonly EngagementListActivity outerInstance;

		  public BaseAdapterAnonymousInnerClassHelper(EngagementListActivity outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual View getView(int position, View convertView, ViewGroup parent)
		  {
			return null;
		  }

		  public virtual long getItemId(int position)
		  {
			return 0;
		  }

		  public virtual object getItem(int position)
		  {
			return null;
		  }

		  public virtual int Count
		  {
			  get
			  {
				return 0;
			  }
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