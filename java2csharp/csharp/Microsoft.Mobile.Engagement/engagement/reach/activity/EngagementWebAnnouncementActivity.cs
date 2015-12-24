using System;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{

	using SuppressLint = android.annotation.SuppressLint;
	using View = android.view.View;
	using JavascriptInterface = android.webkit.JavascriptInterface;
	using WebView = android.webkit.WebView;
	using WebViewClient = android.webkit.WebViewClient;

	/// <summary>
	/// Activity displaying a web Engagement announcement. Add this in the AndroidManifest.xml file to
	/// use it:
	/// 
	/// <pre>
	/// {@code <activity
	///   android:name="com.microsoft.azure.engagement.reach.activity.EngagementWebAnnouncementActivity"
	///   android:theme="@android:style/Theme.Light">
	///     <intent-filter>
	///       <action android:name="com.microsoft.azure.engagement.reach.intent.action.ANNOUNCEMENT" />
	///       <category android:name="android.intent.category.DEFAULT" />
	///       <data android:mimeType="text/html" />
	///     </intent-filter>
	/// </activity>}
	/// </pre>
	/// </summary>
	public class EngagementWebAnnouncementActivity : EngagementAnnouncementActivity
	{
	  protected internal override string LayoutName
	  {
		  get
		  {
			return "engagement_web_announcement";
		  }
	  }

	  /// <summary>
	  /// Interface that is bound to the JavasScript object named "EngagementReachContent" object.
	  /// </summary>
	  protected internal class EngagementReachContentJS
	  {
		  private readonly EngagementWebAnnouncementActivity outerInstance;

		/// <summary>
		/// Web view </summary>
		internal readonly WebView mWebView;

		/// <summary>
		/// Init. </summary>
		/// <param name="webView"> web view. </param>
		protected internal EngagementReachContentJS(EngagementWebAnnouncementActivity outerInstance, WebView webView)
		{
			this.outerInstance = outerInstance;
		  mWebView = webView;
		}

		/// <summary>
		/// Called by web view's JavaScript function EngagementReachContent.actionContent() (not in the
		/// U.I thread).
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JavascriptInterface public void actionContent()
		public virtual void actionContent()
		{
		  mWebView.post(() =>
		  {
		  outerInstance.action();
		  });
		}

		/// <summary>
		/// Called by web view's JavaScript function EngagementReachContent.exitContent() (not in the U.I
		/// thread).
		/// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @JavascriptInterface public void exitContent()
		public virtual void exitContent()
		{
		  mWebView.post(() =>
		  {
		  outerInstance.exit();
		  });
		}
	  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressLint("SetJavaScriptEnabled") protected void setBody(String body, android.view.View bodyView)
	  protected internal override void setBody(string body, View bodyView)
	  {
		/* Init web view with JavaScript enabled */
		WebView webView = (WebView) bodyView;
		webView.Settings.JavaScriptEnabled = true;
		webView.WebViewClient = new WebViewClientAnonymousInnerClassHelper(this);

		/* Bind methods for the content */
		webView.addJavascriptInterface(new EngagementReachContentJS(this, webView), "engagementReachContent");

		/*
		 * Render HTML. The loadData method won't work with some characters since Android 2.0, we use
		 * loadDataWithBaseURL instead.
		 */
		webView.loadDataWithBaseURL(null, body, "text/html", "utf-8", null);
	  }

	  private class WebViewClientAnonymousInnerClassHelper : WebViewClient
	  {
		  private readonly EngagementWebAnnouncementActivity outerInstance;

		  public WebViewClientAnonymousInnerClassHelper(EngagementWebAnnouncementActivity outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public override bool shouldOverrideUrlLoading(WebView view, string url)
		  {
			try
			{
			  /* Launch activity */
			  outerInstance.executeActionURL(url);

			  /* Report action on success */
			  outerInstance.onAction();
			  return true;
			}
			catch (Exception)
			{
			  /* If it fails, fail over default behavior */
			  return false;
			}
		  }
	  }
	}

}