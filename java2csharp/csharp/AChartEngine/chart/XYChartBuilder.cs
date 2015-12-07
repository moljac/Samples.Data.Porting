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

	using PointStyle = org.achartengine.chart.PointStyle;
	using SeriesSelection = org.achartengine.model.SeriesSelection;
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYSeries = org.achartengine.model.XYSeries;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using Activity = android.app.Activity;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using LayoutParams = android.view.ViewGroup.LayoutParams;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using Toast = android.widget.Toast;

	public class XYChartBuilder : Activity
	{
	  /// <summary>
	  /// The main dataset that includes all the series that go into a chart. </summary>
	  private XYMultipleSeriesDataset mDataset = new XYMultipleSeriesDataset();
	  /// <summary>
	  /// The main renderer that includes all the renderers customizing a chart. </summary>
	  private XYMultipleSeriesRenderer mRenderer = new XYMultipleSeriesRenderer();
	  /// <summary>
	  /// The most recently added series. </summary>
	  private XYSeries mCurrentSeries;
	  /// <summary>
	  /// The most recently created renderer, customizing the current series. </summary>
	  private XYSeriesRenderer mCurrentRenderer;
	  /// <summary>
	  /// Button for creating a new series of data. </summary>
	  private Button mNewSeries;
	  /// <summary>
	  /// Button for adding entered data to the current series. </summary>
	  private Button mAdd;
	  /// <summary>
	  /// Edit text field for entering the X value of the data to be added. </summary>
	  private EditText mX;
	  /// <summary>
	  /// Edit text field for entering the Y value of the data to be added. </summary>
	  private EditText mY;
	  /// <summary>
	  /// The chart view that displays the data. </summary>
	  private GraphicalView mChartView;

	  protected internal override void onSaveInstanceState(Bundle outState)
	  {
		base.onSaveInstanceState(outState);
		// save the current data, for instance when changing screen orientation
		outState.putSerializable("dataset", mDataset);
		outState.putSerializable("renderer", mRenderer);
		outState.putSerializable("current_series", mCurrentSeries);
		outState.putSerializable("current_renderer", mCurrentRenderer);
	  }

	  protected internal override void onRestoreInstanceState(Bundle savedState)
	  {
		base.onRestoreInstanceState(savedState);
		// restore the current data, for instance when changing the screen
		// orientation
		mDataset = (XYMultipleSeriesDataset) savedState.getSerializable("dataset");
		mRenderer = (XYMultipleSeriesRenderer) savedState.getSerializable("renderer");
		mCurrentSeries = (XYSeries) savedState.getSerializable("current_series");
		mCurrentRenderer = (XYSeriesRenderer) savedState.getSerializable("current_renderer");
	  }

	  protected internal override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		ContentView = R.layout.xy_chart;

		// the top part of the UI components for adding new data points
		mX = (EditText) findViewById(R.id.xValue);
		mY = (EditText) findViewById(R.id.yValue);
		mAdd = (Button) findViewById(R.id.add);

		// set some properties on the main renderer
		mRenderer.ApplyBackgroundColor = true;
		mRenderer.BackgroundColor = Color.argb(100, 50, 50, 50);
		mRenderer.AxisTitleTextSize = 16;
		mRenderer.ChartTitleTextSize = 20;
		mRenderer.LabelsTextSize = 15;
		mRenderer.LegendTextSize = 15;
		mRenderer.Margins = new int[] {20, 30, 15, 0};
		mRenderer.ZoomButtonsVisible = true;
		mRenderer.PointSize = 5;

		// the button that handles the new series of data creation
		mNewSeries = (Button) findViewById(R.id.new_series);
		mNewSeries.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);

		mAdd.OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
	  }

	  private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
	  {
		  private readonly XYChartBuilder outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper(XYChartBuilder outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onClick(View v)
		  {
			string seriesTitle = "Series " + (outerInstance.mDataset.SeriesCount + 1);
			// create a new series of data
			XYSeries series = new XYSeries(seriesTitle);
			outerInstance.mDataset.addSeries(series);
			outerInstance.mCurrentSeries = series;
			// create a new renderer for the new series
			XYSeriesRenderer renderer = new XYSeriesRenderer();
			outerInstance.mRenderer.addSeriesRenderer(renderer);
			// set some renderer properties
			renderer.PointStyle = PointStyle.CIRCLE;
			renderer.FillPoints = true;
			renderer.DisplayChartValues = true;
			renderer.DisplayChartValuesDistance = 10;
			outerInstance.mCurrentRenderer = renderer;
			outerInstance.SeriesWidgetsEnabled = true;
			outerInstance.mChartView.repaint();
		  }
	  }

	  private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
	  {
		  private readonly XYChartBuilder outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper2(XYChartBuilder outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onClick(View v)
		  {
			double x = 0;
			double y = 0;
			try
			{
			  x = double.Parse(outerInstance.mX.Text.ToString());
			}
			catch (System.FormatException)
			{
			  outerInstance.mX.requestFocus();
			  return;
			}
			try
			{
			  y = double.Parse(outerInstance.mY.Text.ToString());
			}
			catch (System.FormatException)
			{
			  outerInstance.mY.requestFocus();
			  return;
			}
			// add a new data point to the current series
			outerInstance.mCurrentSeries.add(x, y);
			outerInstance.mX.Text = "";
			outerInstance.mY.Text = "";
			outerInstance.mX.requestFocus();
			// repaint the chart such as the newly added point to be visible
			outerInstance.mChartView.repaint();
		  }
	  }

	  protected internal override void onResume()
	  {
		base.onResume();
		if (mChartView == null)
		{
		  LinearLayout layout = (LinearLayout) findViewById(R.id.chart);
		  mChartView = ChartFactory.getLineChartView(this, mDataset, mRenderer);
		  // enable the chart click events
		  mRenderer.ClickEnabled = true;
		  mRenderer.SelectableBuffer = 10;
		  mChartView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);
		  layout.addView(mChartView, new LayoutParams(LayoutParams.FILL_PARENT, LayoutParams.FILL_PARENT));
		  bool enabled = mDataset.SeriesCount > 0;
		  SeriesWidgetsEnabled = enabled;
		}
		else
		{
		  mChartView.repaint();
		}
	  }

	  private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
	  {
		  private readonly XYChartBuilder outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper3(XYChartBuilder outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onClick(View v)
		  {
			// handle the click event on the chart
			SeriesSelection seriesSelection = outerInstance.mChartView.CurrentSeriesAndPoint;
			if (seriesSelection == null)
			{
			  Toast.makeText(outerInstance, "No chart element", Toast.LENGTH_SHORT).show();
			}
			else
			{
			  // display information of the clicked point
			  Toast.makeTextuniquetempvar.show();
			}
		  }
	  }

	  /// <summary>
	  /// Enable or disable the add data to series widgets
	  /// </summary>
	  /// <param name="enabled"> the enabled state </param>
	  private bool SeriesWidgetsEnabled
	  {
		  set
		  {
			mX.Enabled = value;
			mY.Enabled = value;
			mAdd.Enabled = value;
		  }
	  }
	}
}