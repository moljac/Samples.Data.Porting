/// <summary>
/// Copyright (C) 2009 - 2013 SC 4ViewSoft SRL
/// 
/// Licensed under the Apache License, Version 2.0 (the "License");
/// you may not use this file except in compliance with the License.
/// You may obtain a copy of the License at
/// 
///      http://www.apache.org/licenses/LICENSE-2.0
/// 
/// Unless required by applicable law or agreed to in writing, software
/// distributed under the License is distributed on an "AS IS" BASIS,
/// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/// See the License for the specific language governing permissions and
/// limitations under the License.
/// </summary>
namespace org.achartengine.chartdemo.demo.chart
{

	using CategorySeries = org.achartengine.model.CategorySeries;
	using SeriesSelection = org.achartengine.model.SeriesSelection;
	using DefaultRenderer = org.achartengine.renderer.DefaultRenderer;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;

	using Activity = android.app.Activity;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using LayoutParams = android.view.ViewGroup.LayoutParams;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using Toast = android.widget.Toast;

	public class PieChartBuilder : Activity
	{
	  /// <summary>
	  /// Colors to be used for the pie slices. </summary>
	  private static int[] COLORS = new int[] {Color.GREEN, Color.BLUE, Color.MAGENTA, Color.CYAN};
	  /// <summary>
	  /// The main series that will include all the data. </summary>
	  private CategorySeries mSeries = new CategorySeries("");
	  /// <summary>
	  /// The main renderer for the main dataset. </summary>
	  private DefaultRenderer mRenderer = new DefaultRenderer();
	  /// <summary>
	  /// Button for adding entered data to the current series. </summary>
	  private Button mAdd;
	  /// <summary>
	  /// Edit text field for entering the slice value. </summary>
	  private EditText mValue;
	  /// <summary>
	  /// The chart view that displays the data. </summary>
	  private GraphicalView mChartView;

	  protected internal override void onRestoreInstanceState(Bundle savedState)
	  {
		base.onRestoreInstanceState(savedState);
		mSeries = (CategorySeries) savedState.getSerializable("current_series");
		mRenderer = (DefaultRenderer) savedState.getSerializable("current_renderer");
	  }

	  protected internal override void onSaveInstanceState(Bundle outState)
	  {
		base.onSaveInstanceState(outState);
		outState.putSerializable("current_series", mSeries);
		outState.putSerializable("current_renderer", mRenderer);
	  }

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		ContentView = R.layout.xy_chart;
		mValue = (EditText) findViewById(R.id.xValue);
		mRenderer.ZoomButtonsVisible = true;
		mRenderer.StartAngle = 180;
		mRenderer.DisplayValues = true;

		mAdd = (Button) findViewById(R.id.add);
		mAdd.Enabled = true;
		mValue.Enabled = true;

		mAdd.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
	  }

	  private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
	  {
		  private readonly PieChartBuilder outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper(PieChartBuilder outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onClick(View v)
		  {
			double value = 0;
			try
			{
			  value = double.Parse(outerInstance.mValue.Text.ToString());
			}
			catch (System.FormatException)
			{
			  outerInstance.mValue.requestFocus();
			  return;
			}
			outerInstance.mValue.Text = "";
			outerInstance.mValue.requestFocus();
			outerInstance.mSeries.add("Series " + (outerInstance.mSeries.ItemCount + 1), value);
			SimpleSeriesRenderer renderer = new SimpleSeriesRenderer();
			renderer.Color = COLORS[(outerInstance.mSeries.ItemCount - 1) % COLORS.Length];
			outerInstance.mRenderer.addSeriesRenderer(renderer);
			outerInstance.mChartView.repaint();
		  }
	  }

	  protected internal override void onResume()
	  {
		base.onResume();
		if (mChartView == null)
		{
		  LinearLayout layout = (LinearLayout) findViewById(R.id.chart);
		  mChartView = ChartFactory.getPieChartView(this, mSeries, mRenderer);
		  mRenderer.ClickEnabled = true;
		  mChartView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		  layout.addView(mChartView, new LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.FILL_PARENT));
		}
		else
		{
		  mChartView.repaint();
		}
	  }

	  private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
	  {
		  private readonly PieChartBuilder outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper2(PieChartBuilder outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public override void onClick(View v)
		  {
			SeriesSelection seriesSelection = outerInstance.mChartView.CurrentSeriesAndPoint;
			if (seriesSelection == null)
			{
			  Toast.makeText(outerInstance, "No chart element selected", Toast.LENGTH_SHORT).show();
			}
			else
			{
			  for (int i = 0; i < outerInstance.mSeries.ItemCount; i++)
			  {
				outerInstance.mRenderer.getSeriesRendererAt(i).Highlighted = i == seriesSelection.PointIndex;
			  }
			  outerInstance.mChartView.repaint();
			  Toast.makeTextuniquetempvar.show();
			}
		  }
	  }
	}

}