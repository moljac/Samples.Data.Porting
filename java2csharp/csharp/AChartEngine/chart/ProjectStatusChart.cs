using System;
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
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;

	/// <summary>
	/// Project status demo chart.
	/// </summary>
	public class ProjectStatusChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Project tickets status";
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
			return "The opened tickets and the fixed tickets (time chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"New tickets", "Fixed tickets"};
		IList<DateTime[]> dates = new List<DateTime[]>();
		IList<double[]> values = new List<double[]>();
		int length = titles.Length;
		for (int i = 0; i < length; i++)
		{
		  dates.Add(new DateTime[12]);
		  dates[i][0] = new DateTime(108, 9, 1);
		  dates[i][1] = new DateTime(108, 9, 8);
		  dates[i][2] = new DateTime(108, 9, 15);
		  dates[i][3] = new DateTime(108, 9, 22);
		  dates[i][4] = new DateTime(108, 9, 29);
		  dates[i][5] = new DateTime(108, 10, 5);
		  dates[i][6] = new DateTime(108, 10, 12);
		  dates[i][7] = new DateTime(108, 10, 19);
		  dates[i][8] = new DateTime(108, 10, 26);
		  dates[i][9] = new DateTime(108, 11, 3);
		  dates[i][10] = new DateTime(108, 11, 10);
		  dates[i][11] = new DateTime(108, 11, 17);
		}
		values.Add(new double[] {142, 123, 142, 152, 149, 122, 110, 120, 125, 155, 146, 150});
		values.Add(new double[] {102, 90, 112, 105, 125, 112, 125, 112, 105, 115, 116, 135});
		length = values[0].Length;
		int[] colors = new int[] {Color.BLUE, Color.GREEN};
		PointStyle[] styles = new PointStyle[] {PointStyle.POINT, PointStyle.POINT};
		XYMultipleSeriesRenderer renderer = buildRenderer(colors, styles);
		setChartSettings(renderer, "Project work status", "Date", "Tickets", dates[0][0].Ticks, dates[0][11].Ticks, 50, 190, Color.GRAY, Color.LTGRAY);
		renderer.XLabels = 0;
		renderer.YLabels = 10;
		renderer.addYTextLabel(100, "test");
		length = renderer.SeriesRendererCount;
		for (int i = 0; i < length; i++)
		{
		  SimpleSeriesRenderer seriesRenderer = renderer.getSeriesRendererAt(i);
		  seriesRenderer.DisplayChartValues = true;
		}
		renderer.XRoundedLabels = false;
		return ChartFactory.getTimeChartIntent(context, buildDateDataset(titles, dates, values), renderer, "MM/dd/yyyy");
	  }

	}

}