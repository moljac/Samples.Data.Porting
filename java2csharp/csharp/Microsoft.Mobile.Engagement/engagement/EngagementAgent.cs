using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement
{

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.content.Context.BIND_AUTO_CREATE;

	using UncaughtExceptionHandler = Thread.UncaughtExceptionHandler;

	using Activity = android.app.Activity;
	using ComponentName = android.content.ComponentName;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using ServiceConnection = android.content.ServiceConnection;
	using SharedPreferences = android.content.SharedPreferences;
	using OnSharedPreferenceChangeListener = android.content.SharedPreferences.OnSharedPreferenceChangeListener;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using IBinder = android.os.IBinder;
	using Looper = android.os.Looper;
	using TextUtils = android.text.TextUtils;
	using Log = android.util.Log;

	using EngagementUtils = com.microsoft.azure.engagement.utils.EngagementUtils;

	/// <summary>
	/// This is the main class to access Engagement features.<br/>
	/// It is responsible for managing the connection between the application and the Engagement service
	/// in a seamless way (so you don't have to use <seealso cref="IEngagementService"/> directly).<br/>
	/// There is some configuration you can alter in the <tt>AndroidManifest.xml</tt> file, thanks to
	/// meta-data inside the application tag:<br/>
	/// <ul>
	/// <li>To configure the application, copy connection string from portal and set it using
	/// <seealso cref="EngagementConfiguration#setConnectionString(String)"/>. Then pass configuration to
	/// <seealso cref="#init(EngagementConfiguration)"/>.</li>
	/// <li>To disable crash report add {@code <meta-data android:name="engagement:reportCrash"
	/// android:value="false"/>}.</li>
	/// <li>By default, logs are reported in real time, if you want to report logs at regular intervals,
	/// add {@code <meta-data android:name="engagement:burstThreshold"
	/// android:value="<interval_in_millis>"/>}.</li> </li>
	/// <li>To configure the session timeout (which is set to 10s by default), add
	/// {@code <meta-data android:name="engagement:sessionTimeout" android:value="<duration_in_millis>"/>}
	/// , see <seealso cref="#endActivity()"/>.</li>
	/// <li>To enable lazy area location report, add {@code <meta-data android:name="engagement:locationReport:lazyArea"
	/// android:value="true"/>}.</li>
	/// <li>To enable real time location report, add {@code <meta-data android:name="engagement:locationReport:realTime"
	/// android:value="true"/>}.<br/>
	/// There are sub settings:
	/// <ul>
	/// <li>By default, only network based locations are reported. To enable GPS, add
	/// {@code <meta-data android:name="engagement:locationReport:realTime:fine" android:value="true"/>}.
	/// </li>
	/// <li>By default, the reporting is done only when there is an active session. To enable background
	/// mode, add {@code <meta-data android:name="engagement:locationReport:realTime:background"
	/// android:value="true"/>}.<br/>
	/// The background mode is only for network based locations, not GPS.<br/>
	/// To make the background mode starts when the device boots, see
	/// <seealso cref="EngagementLocationBootReceiver"/>.</li>
	/// </ul>
	/// </li>
	/// <li>To enable test logs, add:
	/// {@code <meta-data android:name="engagement:log:test" android:value="true" />}, the log tag is
	/// "engagement-test".</li>
	/// </ul>
	/// </summary>
	public class EngagementAgent
	{
		private bool InstanceFieldsInitialized = false;

		private void InitializeInstanceFields()
		{
			mEngagementCrashHandler = new CrashHandler(this);
		}

	  /// <summary>
	  /// Agent created action </summary>
	  public const string INTENT_ACTION_AGENT_CREATED = "com.microsoft.azure.engagement.intent.action.AGENT_CREATED";

	  /// <summary>
	  /// Maximum pending commands to keep while not bound to a service </summary>
	  private const int MAX_COMMANDS = 200;

	  /// <summary>
	  /// Unbind timeout </summary>
	  private const long BINDER_TIMEOUT = 30000;

	  /// <summary>
	  /// Setting key, prefixed in case of shared integration </summary>
	  private const string ENABLED = "engagement:enabled";

	  /// <summary>
	  /// Unique instance </summary>
	  private static EngagementAgent sInstance;

	  /// <summary>
	  /// Android default crash handler </summary>
	  private static readonly UncaughtExceptionHandler sAndroidCrashHandler = Thread.DefaultUncaughtExceptionHandler;

	  /// <summary>
	  /// Our crash handler </summary>
	  private CrashHandler mEngagementCrashHandler;

	  /// <summary>
	  /// Engagement crash handler implementation </summary>
	  private sealed class CrashHandler : UncaughtExceptionHandler
	  {
		  private readonly EngagementAgent outerInstance;

		  public CrashHandler(EngagementAgent outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		public override void uncaughtException(Thread thread, Exception ex)
		{
		  /*
		   * Report crash to the service via start service, because we can't bind to a service if not
		   * already bound at this stage. We also check if the agent is enabled.
		   */
		  if (outerInstance.Enabled)
		  {
			/* Get crash identifier */
			EngagementCrashId crashId = EngagementCrashId.from(outerInstance.mContext, ex);

			/* Dump stack trace */
			string stackTrace = Log.getStackTraceString(ex);

			/* Set parameters and send the intent */
			Intent intent = EngagementAgentUtils.getServiceIntent(outerInstance.mContext);
			intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_TYPE", crashId.Type);
			intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_LOCATION", crashId.Location);
			intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_STACK_TRACE", stackTrace);
			outerInstance.mContext.startService(intent);
		  }

		  /* Do not prevent android from doing its job (or another crash handler) */
		  sAndroidCrashHandler.uncaughtException(thread, ex);
		}
	  }

	  /// <summary>
	  /// Settings </summary>
	  private readonly SharedPreferences mSettings;

	  /// <summary>
	  /// Settings listener </summary>
	  private readonly SharedPreferences.OnSharedPreferenceChangeListener mSettingsListener;

	  /// <summary>
	  /// True if crash are reported </summary>
	  private readonly bool mReportCrash;

	  /// <summary>
	  /// Engagement service binder </summary>
	  private IEngagementService mEngagementService;

	  /// <summary>
	  /// Last configuration sent to service </summary>
	  private EngagementConfiguration mEngagementConfiguration;

	  /// <summary>
	  /// Context used for binding to the Engagement service </summary>
	  private readonly Context mContext;

	  /// <summary>
	  /// Calls made before the service was bound </summary>
	  private readonly LinkedList<Runnable> mPendingCmds = new LinkedList<Runnable>();

	  /// <summary>
	  /// Unbind task </summary>
	  private readonly Runnable mUnbindTask = new RunnableAnonymousInnerClassHelper();

	  private class RunnableAnonymousInnerClassHelper : Runnable
	  {
		  public RunnableAnonymousInnerClassHelper()
		  {
		  }

		  public override void run()
		  {
			/* Guard against failed cancel */
			if (outerInstance.mEngagementService == null || !outerInstance.mUnbindScheduled)
			{
			  return;
			}

			/* Unbind from Engagement service */
			outerInstance.mContext.unbindService(mServiceConnection);

			/* Store unbound state */
			outerInstance.mEngagementService = null;

			/* Task done */
			outerInstance.mUnbindScheduled = false;
		  }
	  }

	  /// <summary>
	  /// True if unbind has been scheduled </summary>
	  private bool mUnbindScheduled;

	  /// <summary>
	  /// True if binding, false otherwise </summary>
	  private bool mBindingService;

	  /// <summary>
	  /// Handler for unbind timeouts </summary>
	  private readonly Handler mHandler;

	  /// <summary>
	  /// Service connection </summary>
	  private readonly ServiceConnection mServiceConnection = new ServiceConnectionAnonymousInnerClassHelper();

	  private class ServiceConnectionAnonymousInnerClassHelper : ServiceConnection
	  {
		  public ServiceConnectionAnonymousInnerClassHelper()
		  {
		  }

		  public override void onServiceConnected(ComponentName name, IBinder service)
		  {
			/* Cast the binder into the proper API and keep a reference */
			outerInstance.mEngagementService = IEngagementService.Stub.asInterface(service);

			/* We are not binding anymore */
			outerInstance.mBindingService = false;

			/* Send pending commands */
			foreach (Runnable cmd in outerInstance.mPendingCmds)
			{
			  cmd.run();
			}
			outerInstance.mPendingCmds.Clear();

			/* Schedule unbind (if not in session) */
			outerInstance.scheduleUnbind();
		  }

		  public override void onServiceDisconnected(ComponentName name)
		  {
			/* We are not bound anymore */
			outerInstance.mEngagementService = null;

			/*
			 * Simulate disconnected intent targeting the current package name since the engagement
			 * process has been killed.
			 */
			Intent disconnectedIntent = new Intent("com.microsoft.azure.engagement.intent.action.DISCONNECTED");
			disconnectedIntent.Package = outerInstance.mContext.PackageName;
			outerInstance.mContext.sendBroadcast(disconnectedIntent);

			/* Mark we are auto re-binding to it */
			outerInstance.mBindingService = true;
		  }
	  }

	  /// <summary>
	  /// Interface for retrieving results via callback </summary>
	  public interface Callback<T>
	  {
		/// <summary>
		/// Called when the function has been executed. </summary>
		/// <param name="result"> the function result. </param>
		void onResult(T result);
	  }

	  /// <summary>
	  /// Init the agent. </summary>
	  /// <param name="context"> application context. </param>
	  private EngagementAgent(Context context)
	  {
		  if (!InstanceFieldsInitialized)
		  {
			  InitializeInstanceFields();
			  InstanceFieldsInitialized = true;
		  }
		/* Store application context, we'll use this to bind */
		mContext = context;

		/* Create main thread handler */
		mHandler = new Handler(Looper.MainLooper);

		/* Retrieve configuration */
		Bundle config = EngagementUtils.getMetaData(context);
		mReportCrash = config.getBoolean("engagement:reportCrash", true);
		string settingsFile = config.getString("engagement:agent:settings:name");
		int settingsMode = config.getInt("engagement:agent:settings:mode", 0);
		if (TextUtils.isEmpty(settingsFile))
		{
		  settingsFile = "engagement.agent";
		}

		/* Watch preferences */
		mSettings = context.getSharedPreferences(settingsFile, settingsMode);
		mSettingsListener = new OnSharedPreferenceChangeListenerAnonymousInnerClassHelper(this);
		mSettings.registerOnSharedPreferenceChangeListener(mSettingsListener);

		/* Install Engagement crash handler if enabled */
		if (mReportCrash)
		{
		  Thread.DefaultUncaughtExceptionHandler = mEngagementCrashHandler;
		}

		/* Broadcast intent for Engagement modules */
		Intent agentCreatedIntent = new Intent(INTENT_ACTION_AGENT_CREATED);
		agentCreatedIntent.Package = context.PackageName;
		context.sendBroadcast(agentCreatedIntent);
	  }

	  private class OnSharedPreferenceChangeListenerAnonymousInnerClassHelper : SharedPreferences.OnSharedPreferenceChangeListener
	  {
		  private readonly EngagementAgent outerInstance;

		  public OnSharedPreferenceChangeListenerAnonymousInnerClassHelper(EngagementAgent outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public override void onSharedPreferenceChanged(SharedPreferences sharedPreferences, string key)
		  {
			/* Cancel all commands and unbind if agent disabled */
			if (ENABLED.Equals(key) && !outerInstance.Enabled)
			{
			  outerInstance.mPendingCmds.Clear();
			  outerInstance.scheduleUnbind();
			}
		  }
	  }

	  /// <summary>
	  /// Get the unique instance. </summary>
	  /// <param name="context"> any valid context </param>
	  public static EngagementAgent getInstance(Context context)
	  {
		  lock (typeof(EngagementAgent))
		  {
			/* Always check this even if we instantiate once to trigger null pointer in all cases */
			if (sInstance == null)
			{
			  sInstance = new EngagementAgent(context.ApplicationContext);
			}
			return sInstance;
		  }
	  }

	  /// <summary>
	  /// Bind the agent to the Engagement service if not already done. This method opens the connection
	  /// to the Engagement service. Calling this method is required before calling any of the other
	  /// methods of the Engagement agent. This cancels unbind.
	  /// </summary>
	  private void bind()
	  {
		/* Cancel unbind */
		cancelUnbind();

		/* Bind to the Engagement service if not already done or being done */
		if (mEngagementService == null && !mBindingService)
		{
		  mBindingService = true;
		  mContext.bindService(EngagementAgentUtils.getServiceIntent(mContext), mServiceConnection, BIND_AUTO_CREATE);
		}
	  }

	  /// <summary>
	  /// The service is automatically unbound when not used after a timeout. This method starts the
	  /// timer. Calling <seealso cref="#bind()"/> will cancel the timer. This method has no effect if the user is
	  /// in an session.
	  /// </summary>
	  private void scheduleUnbind()
	  {
		/* If not in session or disabled and not already unbinding */
		if (!mUnbindScheduled && (!InSession || !Enabled))
		{
		  /* Schedule unbind */
		  mHandler.postDelayed(mUnbindTask, BINDER_TIMEOUT);
		  mUnbindScheduled = true;
		}
	  }

	  /// <summary>
	  /// Cancel a call to <seealso cref="#scheduleUnbind()"/>.
	  /// </summary>
	  private void cancelUnbind()
	  {
		mHandler.removeCallbacks(mUnbindTask);
		mUnbindScheduled = false;
	  }

	  /// <summary>
	  /// Check if a session is running. </summary>
	  /// <returns> true if a session is running, false otherwise. </returns>
	  private bool InSession
	  {
		  get
		  {
			return EngagementActivityManager.Instance.CurrentActivityAlias != null;
		  }
	  }

	  /// <summary>
	  /// Call the Engagement Service API if bound, otherwise keep the call for later use. </summary>
	  /// <param name="cmd"> the task using the Engagement Service API. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void sendEngagementCommand(final Runnable cmd)
	  private void sendEngagementCommand(Runnable cmd)
	  {
		/* The command needs to run on the main thread to avoid race conditions */
		runOnMainThread(() =>
		{
		/* Nothing to do if disabled */
		if (!Enabled)
		{
		  return;
		}

		/* Bind if needed */
		bind();

		/* If we are not bound, spool command */
		if (mEngagementService == null)
		{
		  mPendingCmds.AddLast(cmd);
		  if (mPendingCmds.Count > MAX_COMMANDS)
		  {
			mPendingCmds.RemoveFirst();
		  }
		}

		/* Otherwise call API and set unbind timer */
		else
		{
		  cmd.run();
		  scheduleUnbind();
		}
		});
	  }

	  /// <summary>
	  /// Check calling thread, if main, run now, otherwise post in main thread </summary>
	  private void runOnMainThread(Runnable task)
	  {
		if (Thread.CurrentThread == mHandler.Looper.Thread)
		{
		  task.run();
		}
		else
		{
		  mHandler.post(task);
		}
	  }

	  /// <summary>
	  /// Notify the start of a new activity within the current session. A session being a sequence of
	  /// activities, this call sets the current activity within the current session. If there is no
	  /// current session, this call starts a new session. </summary>
	  /// <param name="activity"> current activity instance, may be null. Engagement modules may need to watch
	  ///          activity changes and may want to add content to the current view, passing null will
	  ///          prevent these modules to behave like expected. </param>
	  /// <param name="activityName"> the name of the current activity for the current session, can be null for
	  ///          default name (but cannot be empty). Name is limited to 64 characters. </param>
	  /// <param name="extras"> the extra details associated with the activity. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
	  /// <seealso cref= #endActivity() </seealso>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void startActivity(android.app.Activity activity, final String activityName, final android.os.Bundle extras)
	  public virtual void startActivity(Activity activity, string activityName, Bundle extras)
	  {
		EngagementActivityManager.Instance.setCurrentActivity(activity, activityName);
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.startActivity(activityName, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Notify the end of the current activity within the current session. A session being a sequence
	  /// of activities, this call sets the current session idle. The current session is ended only if no
	  /// call to <seealso cref="#startActivity(Activity, String, Bundle)"/> follows this call within a time equal
	  /// to the session timeout (which is set to 10s by default). You can configure the session timeout
	  /// by adding {@code <meta-data android:name="engagement:sessionTimeout"
	  /// android:value="<duration_in_millis>"/>} under the application tag in your AndroidManifest.xml
	  /// file. </summary>
	  /// <seealso cref= #startActivity(Activity, String, Bundle) </seealso>
	  public virtual void endActivity()
	  {
		EngagementActivityManager.Instance.removeCurrentActivity();
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.endActivity();
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Notify the start of a new job. </summary>
	  /// <param name="name"> unique job name, two jobs with the same name can't run at the same time, if a job
	  ///          is started twice, the second version of the job will replace the first one. Name is
	  ///          limited to 64 characters and cannot be empty. </param>
	  /// <param name="extras"> the extra details associated with this job. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
	  /// <seealso cref= #endJob(String) </seealso>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void startJob(final String name, final android.os.Bundle extras)
	  public virtual void startJob(string name, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.startJob(name, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Notify the end of a job. This has no effect if no job is running for the specified name. </summary>
	  /// <param name="name"> the name of a job that has been started with {@link #startJob(String, Bundle)
	  ///          startJob} </param>
	  /// <seealso cref= #startJob(String, Bundle) </seealso>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void endJob(final String name)
	  public virtual void endJob(string name)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.endJob(name);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an event unrelated to any session or job. </summary>
	  /// <param name="name"> event name/tag. Name is limited to 64 characters and cannot be empty. </param>
	  /// <param name="extras"> the extra details associated with this event. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendEvent(final String name, final android.os.Bundle extras)
	  public virtual void sendEvent(string name, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendEvent(name, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an event related to the current session. This has no effect if the session has not been
	  /// started. </summary>
	  /// <param name="name"> event name/tag. Name is limited to 64 characters and cannot be empty. </param>
	  /// <param name="extras"> the extra details associated with this event. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendSessionEvent(final String name, final android.os.Bundle extras)
	  public virtual void sendSessionEvent(string name, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendSessionEvent(name, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an event related to a running job. This has no effect if no job is running for the
	  /// specified name. </summary>
	  /// <param name="eventName"> event name/tag. Name is limited to 64 characters and cannot be empty. </param>
	  /// <param name="jobName"> job name. </param>
	  /// <param name="extras"> the extra details associated with this event. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendJobEvent(final String eventName, final String jobName, final android.os.Bundle extras)
	  public virtual void sendJobEvent(string eventName, string jobName, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendJobEvent(eventName, jobName, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an error unrelated to any session or job. </summary>
	  /// <param name="name"> error name/tag. Name is limited to 64 characters and cannot be empty. </param>
	  /// <param name="extras"> the extra details associated with this error. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendError(final String name, final android.os.Bundle extras)
	  public virtual void sendError(string name, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendError(name, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an error related to the current session. This has no effect if the session has not been
	  /// started. </summary>
	  /// <param name="name"> error name/tag. Name is limited to 64 characters and cannot be empty. </param>
	  /// <param name="extras"> the extra details associated with this error. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendSessionError(final String name, final android.os.Bundle extras)
	  public virtual void sendSessionError(string name, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendSessionError(name, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send an error related to a running job. This has no effect if no job is running for the
	  /// specified name. </summary>
	  /// <param name="errorName"> error name/tag. </param>
	  /// <param name="jobName"> job name. </param>
	  /// <param name="extras"> the extra details associated with this error. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendJobError(final String errorName, final String jobName, final android.os.Bundle extras)
	  public virtual void sendJobError(string errorName, string jobName, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendJobError(errorName, jobName, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Get the identifier used by Engagement to identify this device. </summary>
	  /// <param name="callback"> a callback to retrieve the result. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void getDeviceId(final Callback<String> callback)
	  public virtual void getDeviceId(Callback<string> callback)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  callback.onResult(mEngagementService.DeviceId);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send application specific information. </summary>
	  /// <param name="appInfo"> application information as a Bundle. Keys must match the
	  ///          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
	  ///          before being sent to the server, the encoded limit is 1024 characters. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendAppInfo(final android.os.Bundle appInfo)
	  public virtual void sendAppInfo(Bundle appInfo)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendAppInfo(appInfo);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Send feedback to reach about a content. </summary>
	  /// <param name="kind"> content kind e.g. announcement, poll or datapush. </param>
	  /// <param name="contentId"> content identifier. </param>
	  /// <param name="status"> feedback status e.g. ok or cancelled. </param>
	  /// <param name="extras"> extra information like poll answers. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void sendReachFeedback(final String kind, final String contentId, final String status, final android.os.Bundle extras)
	  public virtual void sendReachFeedback(string kind, string contentId, string status, Bundle extras)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.sendReachFeedback(kind, contentId, status, extras);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Initialize SDK with the specified configuration. The configuration is persisted so that
	  /// background push, background real time location reporting or opening application from any
	  /// activity works even if you integrated this call at only one place such as a launcher activity. </summary>
	  /// <param name="configuration"> full configuration. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void init(final EngagementConfiguration configuration)
	  public virtual void init(EngagementConfiguration configuration)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.init(configuration);
		  mEngagementConfiguration = configuration;
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Call this method if a runtime permission needed for the SDK has just been granted by the user
	  /// so that the SDK can enable the features associated to the permissions.
	  /// </summary>
	  public virtual void refreshPermissions()
	  {
		if (mEngagementConfiguration != null)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final EngagementConfiguration configuration = mEngagementConfiguration;
		  EngagementConfiguration configuration = mEngagementConfiguration;
		  sendEngagementCommand(() =>
		  {
		  try
		  {
			mEngagementService.init(configuration);
		  }
		  catch (Exception e)
		  {
			Console.WriteLine(e.ToString());
			Console.Write(e.StackTrace);
		  }
		  });
		}
	  }

	  /// <summary>
	  /// Register this device for Native Push. </summary>
	  /// <param name="token"> native push token (describing registration identifier and service type). </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void registerNativePush(final EngagementNativePushToken token)
	  public virtual void registerNativePush(EngagementNativePushToken token)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.registerNativePush(token);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Get message by id, result is delivered via intent (application may be killed in the mean time). </summary>
	  /// <param name="id"> message identifier. </param>
//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public void getMessage(final String id)
	  public virtual void getMessage(string id)
	  {
		sendEngagementCommand(() =>
		{
		try
		{
		  mEngagementService.getMessage(id);
		}
		catch (Exception e)
		{
		  Console.WriteLine(e.ToString());
		  Console.Write(e.StackTrace);
		}
		});
	  }

	  /// <summary>
	  /// Enable or disable the agent. The change is persistent. As an example you don't need to call
	  /// this function every time the application is launched to disable the agent.<br/>
	  /// You can also integrate this setting in a preference activity, the preference file name is by
	  /// default "engagement.agent" (with mode 0) but this can be configured using the following
	  /// meta-data tags under the application tag in the AndroidManifest.xml file:
	  /// 
	  /// <pre>
	  /// {@code <meta-data
	  ///   android:name="engagement:agent:settings:name"
	  ///   android:value="engagement.agent" />
	  /// <meta-data
	  ///   android:name="engagement:agent:settings:mode"
	  ///   android:value="0" />
	  /// }
	  /// </pre>
	  /// 
	  /// The key within the preference file is "engagement:enabled" and is a boolean. You can use a
	  /// section like the following one in your preference layout:
	  /// 
	  /// <pre>
	  /// {@code <CheckBoxPreference
	  ///   android:key="engagement:enabled"
	  ///   android:defaultValue="true"
	  ///   android:title="Use Engagement"
	  ///   android:summaryOn="Engagement is enabled."
	  ///   android:summaryOff="Engagement is disabled." />}
	  /// </pre> </summary>
	  /// <param name="enabled"> true to enable, false to disable. </param>
	  public virtual bool Enabled
	  {
		  set
		  {
			mSettings.edit().putBoolean(ENABLED, value).commit();
		  }
		  get
		  {
			return mSettings.getBoolean(ENABLED, true);
		  }
	  }

	}

}