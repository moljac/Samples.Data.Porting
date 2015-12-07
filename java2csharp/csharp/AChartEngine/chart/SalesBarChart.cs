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


	using Type = org.achartengine.chart.BarChart.Type;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using Orientation = org.achartengine.renderer.XYMultipleSeriesRenderer.Orientation;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;

	/// <summary>
	/// Sales demo bar chart.
	/// </summary>
	public class SalesBarChart : AbstractDemoChart
	{

	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Sales horizontal bar chart";
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
			return "The monthly sales for the last 2 years (horizontal bar chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"2007", "2008"};
		IList<double[]> values = new List<double[]>();
		values.Add(new double[] {5230, 7300, 9240, 10540, 7900, 9200, 12030, 11200, 9500, 10500, 11600, 13500});
		values.Add(new double[] {14230, 12300, 14240, 15244, 15900, 19200, 22030, 21200, 19500, 15500, 12600, 14000});
		int[] colors = new int[] {Color.CYAN, Color.BLUE};
		XYMultipleSeriesRenderer renderer = buildBarRenderer(colors);
		renderer.Orientation = XYMultipleSeriesRenderer.Orientation.VERTICAL;
		setChartSettings(renderer, "Monthly sales in the last 2 years", "Month", "Units sold", 0.5, 12.5, 0, 24000, Color.GRAY, Color.LTGRAY);
		renderer.XLabels = 1;
		renderer.YLabels = 10;
		renderer.addXTextLabel(1, "Jan");
		renderer.addXTextLabel(3, "Mar");
		renderer.addXTextLabel(5, "May");
		renderer.addXTextLabel(7, "Jul");
		renderer.addXTextLabel(10, "Oct");
		renderer.addXTextLabel(12, "Dec");
		int length = renderer.SeriesRendererCount;
		for (int i = 0; i < length; i++)
		{
		  SimpleSeriesRenderer seriesRenderer = renderer.getSeriesRendererAt(i);
		  seriesRenderer.DisplayChartValues = true;
		}
		return ChartFactory.getBarChartIntent(context, buildBarDataset(titles, values), renderer, Type.DEFAULT);
	  }

	}

}