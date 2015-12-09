/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement;

import static android.content.Context.BIND_AUTO_CREATE;

import java.lang.Thread.UncaughtExceptionHandler;
import java.util.LinkedList;
import java.util.Queue;

import android.app.Activity;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.ServiceConnection;
import android.content.SharedPreferences;
import android.content.SharedPreferences.OnSharedPreferenceChangeListener;
import android.os.Bundle;
import android.os.Handler;
import android.os.IBinder;
import android.os.Looper;
import android.text.TextUtils;
import android.util.Log;

import com.microsoft.azure.engagement.utils.EngagementUtils;

/**
 * This is the main class to access Engagement features.<br/>
 * It is responsible for managing the connection between the application and the Engagement service
 * in a seamless way (so you don't have to use {@link IEngagementService} directly).<br/>
 * There is some configuration you can alter in the <tt>AndroidManifest.xml</tt> file, thanks to
 * meta-data inside the application tag:<br/>
 * <ul>
 * <li>To configure the application, copy connection string from portal and set it using
 * {@link EngagementConfiguration#setConnectionString(String)}. Then pass configuration to
 * {@link #init(EngagementConfiguration)}.</li>
 * <li>To disable crash report add {@code <meta-data android:name="engagement:reportCrash"
 * android:value="false"/>}.</li>
 * <li>By default, logs are reported in real time, if you want to report logs at regular intervals,
 * add {@code <meta-data android:name="engagement:burstThreshold"
 * android:value="<interval_in_millis>"/>}.</li> </li>
 * <li>To configure the session timeout (which is set to 10s by default), add
 * {@code <meta-data android:name="engagement:sessionTimeout" android:value="<duration_in_millis>"/>}
 * , see {@link #endActivity()}.</li>
 * <li>To enable lazy area location report, add {@code <meta-data android:name="engagement:locationReport:lazyArea"
 * android:value="true"/>}.</li>
 * <li>To enable real time location report, add {@code <meta-data android:name="engagement:locationReport:realTime"
 * android:value="true"/>}.<br/>
 * There are sub settings:
 * <ul>
 * <li>By default, only network based locations are reported. To enable GPS, add
 * {@code <meta-data android:name="engagement:locationReport:realTime:fine" android:value="true"/>}.
 * </li>
 * <li>By default, the reporting is done only when there is an active session. To enable background
 * mode, add {@code <meta-data android:name="engagement:locationReport:realTime:background"
 * android:value="true"/>}.<br/>
 * The background mode is only for network based locations, not GPS.<br/>
 * To make the background mode starts when the device boots, see
 * {@link EngagementLocationBootReceiver}.</li>
 * </ul>
 * </li>
 * <li>To enable test logs, add:
 * {@code <meta-data android:name="engagement:log:test" android:value="true" />}, the log tag is
 * "engagement-test".</li>
 * </ul>
 */
public class EngagementAgent
{
  /** Agent created action */
  public static final String INTENT_ACTION_AGENT_CREATED = "com.microsoft.azure.engagement.intent.action.AGENT_CREATED";

  /** Maximum pending commands to keep while not bound to a service */
  private static final int MAX_COMMANDS = 200;

  /** Unbind timeout */
  private static final long BINDER_TIMEOUT = 30000;

  /** Setting key, prefixed in case of shared integration */
  private static final String ENABLED = "engagement:enabled";

  /** Unique instance */
  private static EngagementAgent sInstance;

  /** Android default crash handler */
  private static final UncaughtExceptionHandler sAndroidCrashHandler = Thread.getDefaultUncaughtExceptionHandler();

  /** Our crash handler */
  private final CrashHandler mEngagementCrashHandler = new CrashHandler();

  /** Engagement crash handler implementation */
  private final class CrashHandler implements UncaughtExceptionHandler
  {
    @Override
    public void uncaughtException(Thread thread, Throwable ex)
    {
      /*
       * Report crash to the service via start service, because we can't bind to a service if not
       * already bound at this stage. We also check if the agent is enabled.
       */
      if (isEnabled())
      {
        /* Get crash identifier */
        EngagementCrashId crashId = EngagementCrashId.from(mContext, ex);

        /* Dump stack trace */
        String stackTrace = Log.getStackTraceString(ex);

        /* Set parameters and send the intent */
        Intent intent = EngagementAgentUtils.getServiceIntent(mContext);
        intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_TYPE", crashId.getType());
        intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_LOCATION",
          crashId.getLocation());
        intent.putExtra("com.microsoft.azure.engagement.intent.extra.CRASH_STACK_TRACE", stackTrace);
        mContext.startService(intent);
      }

      /* Do not prevent android from doing its job (or another crash handler) */
      sAndroidCrashHandler.uncaughtException(thread, ex);
    }
  }

  /** Settings */
  private final SharedPreferences mSettings;

  /** Settings listener */
  private final OnSharedPreferenceChangeListener mSettingsListener;

  /** True if crash are reported */
  private final boolean mReportCrash;

  /** Engagement service binder */
  private IEngagementService mEngagementService;

  /** Last configuration sent to service */
  private EngagementConfiguration mEngagementConfiguration;

  /** Context used for binding to the Engagement service */
  private final Context mContext;

  /** Calls made before the service was bound */
  private final Queue<Runnable> mPendingCmds = new LinkedList<Runnable>();

  /** Unbind task */
  private final Runnable mUnbindTask = new Runnable()
  {
    @Override
    public void run()
    {
      /* Guard against failed cancel */
      if (mEngagementService == null || !mUnbindScheduled)
        return;

      /* Unbind from Engagement service */
      mContext.unbindService(mServiceConnection);

      /* Store unbound state */
      mEngagementService = null;

      /* Task done */
      mUnbindScheduled = false;
    }
  };

  /** True if unbind has been scheduled */
  private boolean mUnbindScheduled;

  /** True if binding, false otherwise */
  private boolean mBindingService;

  /** Handler for unbind timeouts */
  private final Handler mHandler;

  /** Service connection */
  private final ServiceConnection mServiceConnection = new ServiceConnection()
  {
    @Override
    public void onServiceConnected(ComponentName name, IBinder service)
    {
      /* Cast the binder into the proper API and keep a reference */
      mEngagementService = IEngagementService.Stub.asInterface(service);

      /* We are not binding anymore */
      mBindingService = false;

      /* Send pending commands */
      for (Runnable cmd : mPendingCmds)
        cmd.run();
      mPendingCmds.clear();

      /* Schedule unbind (if not in session) */
      scheduleUnbind();
    }

    @Override
    public void onServiceDisconnected(ComponentName name)
    {
      /* We are not bound anymore */
      mEngagementService = null;

      /*
       * Simulate disconnected intent targeting the current package name since the engagement
       * process has been killed.
       */
      Intent disconnectedIntent = new Intent(
        "com.microsoft.azure.engagement.intent.action.DISCONNECTED");
      disconnectedIntent.setPackage(mContext.getPackageName());
      mContext.sendBroadcast(disconnectedIntent);

      /* Mark we are auto re-binding to it */
      mBindingService = true;
    }
  };

  /** Interface for retrieving results via callback */
  public interface Callback<T>
  {
    /**
     * Called when the function has been executed.
     * @param result the function result.
     */
    void onResult(T result);
  }

  /**
   * Init the agent.
   * @param context application context.
   */
  private EngagementAgent(Context context)
  {
    /* Store application context, we'll use this to bind */
    mContext = context;

    /* Create main thread handler */
    mHandler = new Handler(Looper.getMainLooper());

    /* Retrieve configuration */
    Bundle config = EngagementUtils.getMetaData(context);
    mReportCrash = config.getBoolean("engagement:reportCrash", true);
    String settingsFile = config.getString("engagement:agent:settings:name");
    int settingsMode = config.getInt("engagement:agent:settings:mode", 0);
    if (TextUtils.isEmpty(settingsFile))
      settingsFile = "engagement.agent";

    /* Watch preferences */
    mSettings = context.getSharedPreferences(settingsFile, settingsMode);
    mSettingsListener = new OnSharedPreferenceChangeListener()
    {
      @Override
      public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key)
      {
        /* Cancel all commands and unbind if agent disabled */
        if (ENABLED.equals(key) && !isEnabled())
        {
          mPendingCmds.clear();
          scheduleUnbind();
        }
      }
    };
    mSettings.registerOnSharedPreferenceChangeListener(mSettingsListener);

    /* Install Engagement crash handler if enabled */
    if (mReportCrash)
      Thread.setDefaultUncaughtExceptionHandler(mEngagementCrashHandler);

    /* Broadcast intent for Engagement modules */
    Intent agentCreatedIntent = new Intent(INTENT_ACTION_AGENT_CREATED);
    agentCreatedIntent.setPackage(context.getPackageName());
    context.sendBroadcast(agentCreatedIntent);
  }

  /**
   * Get the unique instance.
   * @param context any valid context
   */
  public static synchronized EngagementAgent getInstance(Context context)
  {
    /* Always check this even if we instantiate once to trigger null pointer in all cases */
    if (sInstance == null)
      sInstance = new EngagementAgent(context.getApplicationContext());
    return sInstance;
  }

  /**
   * Bind the agent to the Engagement service if not already done. This method opens the connection
   * to the Engagement service. Calling this method is required before calling any of the other
   * methods of the Engagement agent. This cancels unbind.
   */
  private void bind()
  {
    /* Cancel unbind */
    cancelUnbind();

    /* Bind to the Engagement service if not already done or being done */
    if (mEngagementService == null && !mBindingService)
    {
      mBindingService = true;
      mContext.bindService(EngagementAgentUtils.getServiceIntent(mContext), mServiceConnection,
        BIND_AUTO_CREATE);
    }
  }

  /**
   * The service is automatically unbound when not used after a timeout. This method starts the
   * timer. Calling {@link #bind()} will cancel the timer. This method has no effect if the user is
   * in an session.
   */
  private void scheduleUnbind()
  {
    /* If not in session or disabled and not already unbinding */
    if (!mUnbindScheduled && (!isInSession() || !isEnabled()))
    {
      /* Schedule unbind */
      mHandler.postDelayed(mUnbindTask, BINDER_TIMEOUT);
      mUnbindScheduled = true;
    }
  }

  /**
   * Cancel a call to {@link #scheduleUnbind()}.
   */
  private void cancelUnbind()
  {
    mHandler.removeCallbacks(mUnbindTask);
    mUnbindScheduled = false;
  }

  /**
   * Check if a session is running.
   * @return true if a session is running, false otherwise.
   */
  private boolean isInSession()
  {
    return EngagementActivityManager.getInstance().getCurrentActivityAlias() != null;
  }

  /**
   * Call the Engagement Service API if bound, otherwise keep the call for later use.
   * @param cmd the task using the Engagement Service API.
   */
  private void sendEngagementCommand(final Runnable cmd)
  {
    /* The command needs to run on the main thread to avoid race conditions */
    runOnMainThread(new Runnable()
    {
      @Override
      public void run()
      {
        /* Nothing to do if disabled */
        if (!isEnabled())
          return;

        /* Bind if needed */
        bind();

        /* If we are not bound, spool command */
        if (mEngagementService == null)
        {
          mPendingCmds.offer(cmd);
          if (mPendingCmds.size() > MAX_COMMANDS)
            mPendingCmds.remove();
        }

        /* Otherwise call API and set unbind timer */
        else
        {
          cmd.run();
          scheduleUnbind();
        }
      }
    });
  }

  /** Check calling thread, if main, run now, otherwise post in main thread */
  private void runOnMainThread(Runnable task)
  {
    if (Thread.currentThread() == mHandler.getLooper().getThread())
      task.run();
    else
      mHandler.post(task);
  }

  /**
   * Notify the start of a new activity within the current session. A session being a sequence of
   * activities, this call sets the current activity within the current session. If there is no
   * current session, this call starts a new session.
   * @param activity current activity instance, may be null. Engagement modules may need to watch
   *          activity changes and may want to add content to the current view, passing null will
   *          prevent these modules to behave like expected.
   * @param activityName the name of the current activity for the current session, can be null for
   *          default name (but cannot be empty). Name is limited to 64 characters.
   * @param extras the extra details associated with the activity. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   * @see #endActivity()
   */
  public void startActivity(Activity activity, final String activityName, final Bundle extras)
  {
    EngagementActivityManager.getInstance().setCurrentActivity(activity, activityName);
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.startActivity(activityName, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Notify the end of the current activity within the current session. A session being a sequence
   * of activities, this call sets the current session idle. The current session is ended only if no
   * call to {@link #startActivity(Activity, String, Bundle)} follows this call within a time equal
   * to the session timeout (which is set to 10s by default). You can configure the session timeout
   * by adding {@code <meta-data android:name="engagement:sessionTimeout"
   * android:value="<duration_in_millis>"/>} under the application tag in your AndroidManifest.xml
   * file.
   * @see #startActivity(Activity, String, Bundle)
   */
  public void endActivity()
  {
    EngagementActivityManager.getInstance().removeCurrentActivity();
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.endActivity();
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Notify the start of a new job.
   * @param name unique job name, two jobs with the same name can't run at the same time, if a job
   *          is started twice, the second version of the job will replace the first one. Name is
   *          limited to 64 characters and cannot be empty.
   * @param extras the extra details associated with this job. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   * @see #endJob(String)
   */
  public void startJob(final String name, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.startJob(name, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Notify the end of a job. This has no effect if no job is running for the specified name.
   * @param name the name of a job that has been started with {@link #startJob(String, Bundle)
   *          startJob}
   * @see #startJob(String, Bundle)
   */
  public void endJob(final String name)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.endJob(name);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an event unrelated to any session or job.
   * @param name event name/tag. Name is limited to 64 characters and cannot be empty.
   * @param extras the extra details associated with this event. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendEvent(final String name, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendEvent(name, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an event related to the current session. This has no effect if the session has not been
   * started.
   * @param name event name/tag. Name is limited to 64 characters and cannot be empty.
   * @param extras the extra details associated with this event. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendSessionEvent(final String name, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendSessionEvent(name, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an event related to a running job. This has no effect if no job is running for the
   * specified name.
   * @param eventName event name/tag. Name is limited to 64 characters and cannot be empty.
   * @param jobName job name.
   * @param extras the extra details associated with this event. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendJobEvent(final String eventName, final String jobName, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendJobEvent(eventName, jobName, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an error unrelated to any session or job.
   * @param name error name/tag. Name is limited to 64 characters and cannot be empty.
   * @param extras the extra details associated with this error. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendError(final String name, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendError(name, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an error related to the current session. This has no effect if the session has not been
   * started.
   * @param name error name/tag. Name is limited to 64 characters and cannot be empty.
   * @param extras the extra details associated with this error. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendSessionError(final String name, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendSessionError(name, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send an error related to a running job. This has no effect if no job is running for the
   * specified name.
   * @param errorName error name/tag.
   * @param jobName job name.
   * @param extras the extra details associated with this error. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendJobError(final String errorName, final String jobName, final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendJobError(errorName, jobName, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Get the identifier used by Engagement to identify this device.
   * @param callback a callback to retrieve the result.
   */
  public void getDeviceId(final Callback<String> callback)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          callback.onResult(mEngagementService.getDeviceId());
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send application specific information.
   * @param appInfo application information as a Bundle. Keys must match the
   *          <tt>^[a-zA-Z][a-zA-Z_0-9]*</tt> regular expression. Extras are encoded into JSON
   *          before being sent to the server, the encoded limit is 1024 characters.
   */
  public void sendAppInfo(final Bundle appInfo)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendAppInfo(appInfo);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Send feedback to reach about a content.
   * @param kind content kind e.g. announcement, poll or datapush.
   * @param contentId content identifier.
   * @param status feedback status e.g. ok or cancelled.
   * @param extras extra information like poll answers.
   */
  public void sendReachFeedback(final String kind, final String contentId, final String status,
    final Bundle extras)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.sendReachFeedback(kind, contentId, status, extras);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Initialize SDK with the specified configuration. The configuration is persisted so that
   * background push, background real time location reporting or opening application from any
   * activity works even if you integrated this call at only one place such as a launcher activity.
   * @param configuration full configuration.
   */
  public void init(final EngagementConfiguration configuration)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.init(configuration);
          mEngagementConfiguration = configuration;
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Call this method if a runtime permission needed for the SDK has just been granted by the user
   * so that the SDK can enable the features associated to the permissions.
   */
  public void refreshPermissions()
  {
    if (mEngagementConfiguration != null)
    {
      final EngagementConfiguration configuration = mEngagementConfiguration;
      sendEngagementCommand(new Runnable()
      {
        @Override
        public void run()
        {
          try
          {
            mEngagementService.init(configuration);
          }
          catch (Exception e)
          {
            e.printStackTrace();
          }
        }
      });
    }
  }

  /**
   * Register this device for Native Push.
   * @param token native push token (describing registration identifier and service type).
   */
  public void registerNativePush(final EngagementNativePushToken token)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.registerNativePush(token);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Get message by id, result is delivered via intent (application may be killed in the mean time).
   * @param id message identifier.
   */
  public void getMessage(final String id)
  {
    sendEngagementCommand(new Runnable()
    {
      @Override
      public void run()
      {
        try
        {
          mEngagementService.getMessage(id);
        }
        catch (Exception e)
        {
          e.printStackTrace();
        }
      }
    });
  }

  /**
   * Enable or disable the agent. The change is persistent. As an example you don't need to call
   * this function every time the application is launched to disable the agent.<br/>
   * You can also integrate this setting in a preference activity, the preference file name is by
   * default "engagement.agent" (with mode 0) but this can be configured using the following
   * meta-data tags under the application tag in the AndroidManifest.xml file:
   * 
   * <pre>
   * {@code <meta-data
   *   android:name="engagement:agent:settings:name"
   *   android:value="engagement.agent" />
   * <meta-data
   *   android:name="engagement:agent:settings:mode"
   *   android:value="0" />
   * }
   * </pre>
   * 
   * The key within the preference file is "engagement:enabled" and is a boolean. You can use a
   * section like the following one in your preference layout:
   * 
   * <pre>
   * {@code <CheckBoxPreference
   *   android:key="engagement:enabled"
   *   android:defaultValue="true"
   *   android:title="Use Engagement"
   *   android:summaryOn="Engagement is enabled."
   *   android:summaryOff="Engagement is disabled." />}
   * </pre>
   * @param enabled true to enable, false to disable.
   */
  public void setEnabled(boolean enabled)
  {
    mSettings.edit().putBoolean(ENABLED, enabled).commit();
  }

  /**
   * Check if the agent is enabled.
   * @return true if the agent is enabled, false otherwise.
   */
  public boolean isEnabled()
  {
    return mSettings.getBoolean(ENABLED, true);
  }
}
