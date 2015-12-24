/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

	using Application = android.app.Application;
	using Configuration = android.content.res.Configuration;

	/// <summary>
	/// Helper class used to replace Android's <seealso cref="Application"/> class.<br/>
	/// If you currently extend the <seealso cref="Application"/> class, please make your class extend this class
	/// instead. Your <seealso cref="#onCreate()"/> function has to be renamed
	/// <seealso cref="#onApplicationProcessCreate()"/> and the same rule applies for the other callbacks. Make
	/// sure to also rename the calls to the super methods the same way to avoid an infinite loop (which
	/// will trigger a <tt>java.lang.StackOverflowError</tt>).<br/>
	/// These new methods are called only if the current process is not dedicated to the Engagement service,
	/// avoiding unnecessary initialization in that process.<br/>
	/// If you use an Application sub-class but you don't want to extend this class, you can use directly
	/// <seealso cref="EngagementAgentUtils#isInDedicatedEngagementProcess(android.content.Context)"/> and execute your legacy
	/// code only if this method return <tt>false</tt>. </summary>
	/// <seealso cref= EngagementAgentUtils#isInDedicatedEngagementProcess(android.content.Context) </seealso>
	public abstract class EngagementApplication : Application
	{
	  public override void onCreate()
	  {
		if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
		{
		  onApplicationProcessCreate();
		}
	  }

	  public override void onTerminate()
	  {
		if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
		{
		  onApplicationProcessTerminate();
		}
	  }

	  public override void onLowMemory()
	  {
		if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
		{
		  onApplicationProcessLowMemory();
		}
	  }

	  public override void onConfigurationChanged(Configuration newConfig)
	  {
		if (!EngagementAgentUtils.isInDedicatedEngagementProcess(this))
		{
		  onApplicationProcessConfigurationChanged(newConfig);
		}
	  }

	  /// <summary>
	  /// Override this method instead of <seealso cref="#onCreate()"/> to avoid doing unnecessary operations when
	  /// the current process is the one dedicated to the Engagement service.
	  /// </summary>
	  protected internal virtual void onApplicationProcessCreate()
	  {
		/* Sub-class template method */
	  }

	  /// <summary>
	  /// Override this method instead of <seealso cref="#onTerminate()"/> to avoid doing unnecessary operations
	  /// when the current process is the one dedicated to the Engagement service.
	  /// </summary>
	  protected internal virtual void onApplicationProcessTerminate()
	  {
		/* Sub-class template method */
	  }

	  /// <summary>
	  /// Override this method instead of <seealso cref="#onLowMemory()"/> to avoid doing unnecessary operations
	  /// when the current process is the one dedicated to the Engagement service.
	  /// </summary>
	  protected internal virtual void onApplicationProcessLowMemory()
	  {
		/* Sub-class template method */
	  }

	  /// <summary>
	  /// Override this method instead of <seealso cref="#onConfigurationChanged(Configuration)"/> to avoid doing
	  /// unnecessary operations when the current process is the one dedicated to the Engagement service.
	  /// </summary>
	  protected internal virtual void onApplicationProcessConfigurationChanged(Configuration newConfig)
	  {
		/* Sub-class template method */
	  }
	}

}