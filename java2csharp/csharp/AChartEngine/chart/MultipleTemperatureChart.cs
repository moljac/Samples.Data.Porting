using System.Collections.Generic;

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
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Align = android.graphics.Paint.Align;

	/// <summary>
	/// Multiple temperature demo chart.
	/// </summary>
	public class MultipleTemperatureChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Temperature and sunshine";
		  }
	  }

	  /// <summary>
	  /// Returns the chart description.
	  /// </summary>
	  /// <returns> the chart description </returns>
	  public override string Desc
	  {
		  get
		  {
			return "The average temperature and hours of sunshine in Crete (line chart with multiple Y scales and axis)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"Air temperature"};
		IList<double[]> x = new List<double[]>();
		for (int i = 0; i < titles.Length; i++)
		{
		  x.Add(new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});
		}
		IList<double[]> values = new List<double[]>();
		values.Add(new double[] {12.3, 12.5, 13.8, 16.8, 20.4, 24.4, 26.4, 26.1, 23.6, 20.3, 17.2, 13.9});
		int[] colors = new int[] {Color.BLUE, Color.YELLOW};
		PointStyle[] styles = new PointStyle[] {PointStyle.POINT, PointStyle.POINT};
		XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer(2);
		setRenderer(renderer, colors, styles);
		int length = renderer.SeriesRendererCount;
		for (int i = 0; i < length; i++)
		{
		  XYSeriesRenderer r = (XYSeriesRenderer) renderer.getSeriesRendererAt(i);
		  r.LineWidth = 3f;
		}
		setChartSettings(renderer, "Average temperature", "Month", "Temperature", 0.5, 12.5, 0, 32, Color.LTGRAY, Color.LTGRAY);
		renderer.XLabels = 12;
		renderer.YLabels = 10;
		renderer.ShowGrid = true;
		renderer.XLabelsAlign = Align.RIGHT;
		renderer.YLabelsAlign = Align.RIGHT;
		renderer.ZoomButtonsVisible = true;
		renderer.PanLimits = new double[] {-10, 20, -10, 40};
		renderer.ZoomLimits = new double[] {-10, 20, -10, 40};
		renderer.ZoomRate = 1.05f;
		renderer.LabelsColor = Color.WHITE;
		renderer.XLabelsColor = Color.GREEN;
		renderer.setYLabelsColor(0, colors[0]);
		renderer.setYLabelsColor(1, colors[1]);

		renderer.setYTitle("Hours", 1);
		renderer.setYAxisAlign(Align.RIGHT, 1);
		renderer.setYLabelsAlign(Align.LEFT, 1);

		XYMultipleSeriesDataset dataset = buildDataset(titles, x, values);
		values.Clear();
		values.Add(new double[] {4.3, 4.9, 5.9, 8.8, 10.8, 11.9, 13.6, 12.8, 11.4, 9.5, 7.5, 5.5});
		addXYSeries(dataset, new string[] {"Sunshine hours"}, x, values, 1);

		Intent intent = ChartFactory.getCubicLineChartIntent(context, dataset, renderer, 0.3f, "Average temperature");
		return intent;
	  }
	}

}