/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.activity
{

	using AliasActivity = android.app.AliasActivity;
	using Bundle = android.os.Bundle;


	/// <summary>
	/// Helper class used to replace Android's android.app.AliasActivity class.
	/// </summary>
	public abstract class EngagementAliasActivity : AliasActivity
	{
	  private EngagementAgent mEngagementAgent;

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		mEngagementAgent = EngagementAgent.getInstance(this);
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
	  /// Override this to specify the name reported by your activity. The default implementation returns
	  /// the simple name of the class and removes the "Activity" suffix if any (e.g.
	  /// "com.mycompany.MainActivity" -> "Main"). </summary>
	  /// <returns> the activity name reported by the Engagement service. </returns>
	  protected internal virtual string EngagementActivityName
	  {
		  get
		  {
			return EngagementAgentUtils.buildEngagementActivityName(this.GetType());
		  }
	  }

	  /// <summary>
	  /// Override this to attach extra information to your activity. The default implementation attaches
	  /// no extra information (i.e. return null). </summary>
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