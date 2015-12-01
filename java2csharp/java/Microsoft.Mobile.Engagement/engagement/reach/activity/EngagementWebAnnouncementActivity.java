/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

package com.microsoft.azure.engagement.reach.activity;

import android.annotation.SuppressLint;
import android.view.View;
import android.webkit.JavascriptInterface;
import android.webkit.WebView;
import android.webkit.WebViewClient;

/**
 * Activity displaying a web Engagement announcement. Add this in the AndroidManifest.xml file to
 * use it:
 * 
 * <pre>
 * {@code <activity
 *   android:name="com.microsoft.azure.engagement.reach.activity.EngagementWebAnnouncementActivity"
 *   android:theme="@android:style/Theme.Light">
 *     <intent-filter>
 *       <action android:name="com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT" />
 *       <category android:name="android.intent.category.DEFAULT" />
 *       <data android:mimeType="text/html" />
 *     </intent-filter>
 * </activity>}
 * </pre>
 */
public class EngagementWebAnnouncementActivity extends EngagementAnnouncementActivity
{
  @Override
  protected String getLayoutName()
  {
    return "engagement_web_announcement";
  }

  /**
   * Interface that is bound to the JavasScript object named "EngagementReachContent" object.
   */
  protected class EngagementReachContentJS
  {
    /** Web view */
    private final WebView mWebView;

    /**
     * Init.
     * @param webView web view.
     */
    protected EngagementReachContentJS(WebView webView)
    {
      mWebView = webView;
    }

    /**
     * Called by web view's JavaScript function EngagementReachContent.actionContent() (not in the
     * U.I thread).
     */
    @JavascriptInterface
    public void actionContent()
    {
      mWebView.post(new Runnable()
      {
        @Override
        public void run()
        {
          action();
        }
      });
    }

    /**
     * Called by web view's JavaScript function EngagementReachContent.exitContent() (not in the U.I
     * thread).
     */
    @JavascriptInterface
    public void exitContent()
    {
      mWebView.post(new Runnable()
      {
        @Override
        public void run()
        {
          exit();
        }
      });
    }
  }

  @Override
  @SuppressLint("SetJavaScriptEnabled")
  protected void setBody(String body, View bodyView)
  {
    /* Init web view with JavaScript enabled */
    WebView webView = (WebView) bodyView;
    webView.getSettings().setJavaScriptEnabled(true);
    webView.setWebViewClient(new WebViewClient()
    {
      @Override
      public boolean shouldOverrideUrlLoading(WebView view, String url)
      {
        try
        {
          /* Launch activity */
          executeActionURL(url);

          /* Report action on success */
          onAction();
          return true;
        }
        catch (Exception e)
        {
          /* If it fails, fail over default behavior */
          return false;
        }
      }
    });

    /* Bind methods for the content */
    webView.addJavascriptInterface(new EngagementReachContentJS(webView), "engagementReachContent");

    /*
     * Render HTML. The loadData method won't work with some characters since Android 2.0, we use
     * loadDataWithBaseURL instead.
     */
    webView.loadDataWithBaseURL(null, body, "text/html", "utf-8", null);
  }
}
