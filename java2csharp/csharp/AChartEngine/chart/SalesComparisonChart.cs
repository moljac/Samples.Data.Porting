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
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;
	using FillOutsideLine = org.achartengine.renderer.XYSeriesRenderer.FillOutsideLine;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Typeface = android.graphics.Typeface;

	/// <summary>
	/// Sales comparison demo chart.
	/// </summary>
	public class SalesComparisonChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Sales comparison";
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
			return "Monthly sales advance for 2 years (interpolated line and area charts)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"Sales for 2008", "Sales for 2007", "Difference between 2008 and 2007 sales"};
		IList<double[]> values = new List<double[]>();
		values.Add(new double[] {14230, 12300, 14240, 15244, 14900, 12200, 11030, 12000, 12500, 15500, 14600, 15000});
		values.Add(new double[] {10230, 10900, 11240, 12540, 13500, 14200, 12530, 11200, 10500, 12500, 11600, 13500});
		int length = values[0].Length;
		double[] diff = new double[length];
		for (int i = 0; i < length; i++)
		{
		  diff[i] = values[0][i] - values[1][i];
		}
		values.Add(diff);
		int[] colors = new int[] {Color.BLUE, Color.CYAN, Color.GREEN};
		PointStyle[] styles = new PointStyle[] {PointStyle.POINT, PointStyle.POINT, PointStyle.POINT};
		XYMultipleSeriesRenderer renderer = buildRenderer(colors, styles);
		setChartSettings(renderer, "Monthly sales in the last 2 years", "Month", "Units sold", 0.75, 12.25, -5000, 19000, Color.GRAY, Color.LTGRAY);
		renderer.XLabels = 12;
		renderer.YLabels = 10;
		renderer.ChartTitleTextSize = 20;
		renderer.setTextTypeface("sans_serif", Typeface.BOLD);
		renderer.LabelsTextSize = 14f;
		renderer.AxisTitleTextSize = 15;
		renderer.LegendTextSize = 15;
		length = renderer.SeriesRendererCount;

		for (int i = 0; i < length; i++)
		{
		  XYSeriesRenderer seriesRenderer = (XYSeriesRenderer) renderer.getSeriesRendererAt(i);
		  if (i == length - 1)
		  {
			XYSeriesRenderer.FillOutsideLine fill = new XYSeriesRenderer.FillOutsideLine(XYSeriesRenderer.FillOutsideLine.Type.BOUNDS_ALL);
			fill.Color = Color.GREEN;
			seriesRenderer.addFillOutsideLine(fill);
		  }
		  seriesRenderer.LineWidth = 2.5f;
		  seriesRenderer.DisplayChartValues = true;
		  seriesRenderer.ChartValuesTextSize = 10f;
		}
		return ChartFactory.getCubicLineChartIntent(context, buildBarDataset(titles, values), renderer, 0.5f);
	  }
	}

}