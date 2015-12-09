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
	using PanListener = org.achartengine.tools.PanListener;
	using ZoomEvent = org.achartengine.tools.ZoomEvent;
	using ZoomListener = org.achartengine.tools.ZoomListener;

	using Activity = android.app.Activity;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using LayoutParams = android.view.ViewGroup.LayoutParams;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using LinearLayout = android.widget.LinearLayout;
	using Toast = android.widget.Toast;

	public class XYChartBuilderBackup : Activity
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

	  // private int index = 0;

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
		  private readonly XYChartBuilderBackup outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper(XYChartBuilderBackup outerInstance)
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

			outerInstance.mCurrentSeries.add(1, 2);
			outerInstance.mCurrentSeries.add(2, 3);
			outerInstance.mCurrentSeries.add(3, 0.5);
			outerInstance.mCurrentSeries.add(4, -1);
			outerInstance.mCurrentSeries.add(5, 2.5);
			outerInstance.mCurrentSeries.add(6, 3.5);
			outerInstance.mCurrentSeries.add(7, 2.85);
			outerInstance.mCurrentSeries.add(8, 3.25);
			outerInstance.mCurrentSeries.add(9, 4.25);
			outerInstance.mCurrentSeries.add(10, 3.75);
			outerInstance.mRenderer.Range = new double[] {0.5, 10.5, -1.5, 4.75};
			outerInstance.mChartView.repaint();
		  }
	  }

	  private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
	  {
		  private readonly XYChartBuilderBackup outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper2(XYChartBuilderBackup outerInstance)
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
			// Bitmap bitmap = mChartView.toBitmap();
			// try {
			// File file = new File(Environment.getExternalStorageDirectory(),
			// "test" + index++ + ".png");
			// FileOutputStream output = new FileOutputStream(file);
			// bitmap.compress(CompressFormat.PNG, 100, output);
			// } catch (Exception e) {
			// e.printStackTrace();
			// }
		  }
	  }

	  protected internal override void onResume()
	  {
		base.onResume();
		if (mChartView == null)
		{
		  LinearLayout layout = (LinearLayout) findViewById(R.id.chart);
		  // mChartView = ChartFactory.getLineChartView(this, mDataset, mRenderer);
		  // mChartView = ChartFactory.getBarChartView(this, mDataset, mRenderer,
		  // Type.DEFAULT);
		  mChartView = ChartFactory.getScatterChartView(this, mDataset, mRenderer);

		  // enable the chart click events
		  mRenderer.ClickEnabled = true;
		  mRenderer.SelectableBuffer = 100;
		  mChartView.OnClickListener = new OnClickListenerAnonymousInnerClassHelper3(this);
		  // an example of handling the zoom events on the chart
		  mChartView.addZoomListener(new ZoomListenerAnonymousInnerClassHelper(this), true, true);
		  // an example of handling the pan events on the chart
		  mChartView.addPanListener(new PanListenerAnonymousInnerClassHelper(this));
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
		  private readonly XYChartBuilderBackup outerInstance;

		  public OnClickListenerAnonymousInnerClassHelper3(XYChartBuilderBackup outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void onClick(View v)
		  {
			// handle the click event on the chart
			SeriesSelection seriesSelection = outerInstance.mChartView.CurrentSeriesAndPoint;
			if (seriesSelection == null)
			{
			  Toast.makeText(outerInstance, "No chart element was clicked", Toast.LENGTH_SHORT).show();
			}
			else
			{
			  // display information of the clicked point
			  double[] xy = outerInstance.mChartView.toRealPoint(0);
			  Toast.makeTextuniquetempvar.show();
			}
		  }
	  }

	  private class ZoomListenerAnonymousInnerClassHelper : ZoomListener
	  {
		  private readonly XYChartBuilderBackup outerInstance;

		  public ZoomListenerAnonymousInnerClassHelper(XYChartBuilderBackup outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void zoomApplied(ZoomEvent e)
		  {
			string type = "out";
			if (e.ZoomIn)
			{
			  type = "in";
			}
			Log.i("Zoom", "Zoom " + type + " rate " + e.ZoomRate);
		  }

		  public virtual void zoomReset()
		  {
			Log.i("Zoom", "Reset");
		  }
	  }

	  private class PanListenerAnonymousInnerClassHelper : PanListener
	  {
		  private readonly XYChartBuilderBackup outerInstance;

		  public PanListenerAnonymousInnerClassHelper(XYChartBuilderBackup outerInstance)
		  {
			  this.outerInstance = outerInstance;
		  }

		  public virtual void panApplied()
		  {
			Log.i("Pan", "New X range=[" + outerInstance.mRenderer.XAxisMin + ", " + outerInstance.mRenderer.XAxisMax + "], Y range=[" + outerInstance.mRenderer.YAxisMax + ", " + outerInstance.mRenderer.YAxisMax + "]");
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