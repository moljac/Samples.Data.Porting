using System.Collections.Generic;

/*
 * Copyright (c) Microsoft Corporation. All rights reserved.
 */

namespace com.microsoft.azure.engagement.reach.activity
{


	using JSONArray = org.json.JSONArray;
	using JSONException = org.json.JSONException;
	using JSONObject = org.json.JSONObject;

	using Context = android.content.Context;
	using Bundle = android.os.Bundle;
	using LayoutInflater = android.view.LayoutInflater;
	using LinearLayout = android.widget.LinearLayout;
	using RadioButton = android.widget.RadioButton;
	using RadioGroup = android.widget.RadioGroup;
	using OnCheckedChangeListener = android.widget.RadioGroup.OnCheckedChangeListener;
	using TextView = android.widget.TextView;

	/// <summary>
	/// Activity displaying a plain text Engagement poll. Add this in the AndroidManifest.xml file to use
	/// it:
	/// 
	/// <pre>
	/// {@code <activity
	///   android:name="com.microsoft.azure.engagement.reach.activity.EngagementPollActivity"
	///   android:theme="@android:style/Theme.Light">
	///     <intent-filter>
	///       <action android:name="com.microsoft.azure.engagement.reach.intent.action.POLL"/>
	///       <category android:name="android.intent.category.DEFAULT"/>
	///     </intent-filter>
	/// </activity>}
	/// </pre>
	/// </summary>
	public class EngagementPollActivity : EngagementContentActivity<EngagementPoll>
	{
	  /// <summary>
	  /// Radio button groups </summary>
	  private readonly ICollection<RadioGroup> mRadioGroups = new List<RadioGroup>();

	  /// <summary>
	  /// Radio button check watcher </summary>
	  private readonly RadioGroup.OnCheckedChangeListener mRadioGroupListener = new OnCheckedChangeListenerAnonymousInnerClassHelper();

	  private class OnCheckedChangeListenerAnonymousInnerClassHelper : RadioGroup.OnCheckedChangeListener
	  {
		  public OnCheckedChangeListenerAnonymousInnerClassHelper()
		  {
		  }

		  public override void onCheckedChanged(RadioGroup group, int checkedId)
		  {
			outerInstance.updateActionState();
		  }
	  }

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		/* Init layout */
		base.onCreate(savedInstanceState);

		/* If no content, nothing to do, super class already called finish */
		if (mContent == null)
		{
		  return;
		}

		/* Render questions */
		LinearLayout questionsLayout = getView("questions");
		JSONArray questions = mContent.Questions;
		LayoutInflater layoutInflater = (LayoutInflater) getSystemService(Context.LAYOUT_INFLATER_SERVICE);
		try
		{
		  for (int i = 0; i < questions.length(); i++)
		  {
			/* Get question */
			JSONObject question = questions.getJSONObject(i);

			/* Inflate question layout */
			LinearLayout questionLayout = (LinearLayout) layoutInflater.inflate(getLayoutId("engagement_poll_question"), null);

			/* Set question's title */
			TextView questionTitle = (TextView) questionLayout.findViewById(getId("question_title"));
			questionTitle.Text = question.getString("title");

			/* Set choices */
			RadioGroup choicesView = (RadioGroup) questionLayout.findViewById(getId("choices"));
			choicesView.Tag = question;
			JSONArray choices = question.getJSONArray("choices");
			int choiceViewId = 0;
			for (int j = 0; j < choices.length(); j++)
			{
			  /* Get choice */
			  JSONObject choice = choices.getJSONObject(j);

			  /* Inflate choice layout */
			  RadioButton choiceView = (RadioButton) layoutInflater.inflate(getLayoutId("engagement_poll_choice"), null);

			  /* Each choice is a radio button */
			  choiceView.Id = choiceViewId++;
			  choiceView.Tag = choice.getString("id");
			  choiceView.Text = choice.getString("title");
			  choiceView.Checked = choice.optBoolean("isDefault");
			  choicesView.addView(choiceView);
			}

			/* Add to parent layouts */
			questionsLayout.addView(questionLayout);

			/* Watch state */
			mRadioGroups.Add(choicesView);
			choicesView.OnCheckedChangeListener = mRadioGroupListener;
		  }
		}
		catch (JSONException)
		{
		  /* Drop on parsing error */
		  mContent.dropContent(this);
		  finish();
		  return;
		}

		/* Disable action if a choice is not selected */
		updateActionState();
	  }

	  protected internal override string LayoutName
	  {
		  get
		  {
			return "engagement_poll";
		  }
	  }

	  protected internal override void onAction()
	  {
		/* Scan U.I radio button states */
		LinearLayout questionsLayout = getView("questions");
		JSONArray questions = mContent.Questions;
		try
		{
		  for (int i = 0; i < questions.length(); i++)
		  {
			/* Get question */
			JSONObject question = questions.getJSONObject(i);

			/* Get radio group by question */
			string questionId = question.getString("id");
			RadioGroup choicesView = (RadioGroup) questionsLayout.findViewWithTag(question);

			/* Get selected choice id */
			int selectedViewId = choicesView.CheckedRadioButtonId;
			RadioButton selectedChoiceView = (RadioButton) choicesView.findViewById(selectedViewId);
			string choiceId = selectedChoiceView.Tag.ToString();

			/* Fill answer */
			mContent.fillAnswer(questionId, choiceId);
		  }
		}
		catch (JSONException)
		{
		  /* Won't happen */
		}

		/* Submit answers */
		mContent.actionContent(ApplicationContext);
	  }

	  /// <summary>
	  /// Enable action button if all choices selected. Disable otherwise. </summary>
	  private void updateActionState()
	  {
		bool @checked = true;
		foreach (RadioGroup group in mRadioGroups)
		{
		  @checked &= group.CheckedRadioButtonId != -1;
		}
		mActionButton.Enabled = @checked;
	  }
	}

}