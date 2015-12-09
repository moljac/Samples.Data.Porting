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
	using XYSeries = org.achartengine.model.XYSeries;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Align = android.graphics.Paint.Align;

	/// <summary>
	/// Average temperature demo chart.
	/// </summary>
	public class AverageTemperatureChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Average temperature";
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
			return "The average temperature in 4 Greek islands (line chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"Crete", "Corfu", "Thassos", "Skiathos"};
		IList<double[]> x = new List<double[]>();
		for (int i = 0; i < titles.Length; i++)
		{
		  x.Add(new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});
		}
		IList<double[]> values = new List<double[]>();
		values.Add(new double[] {12.3, 12.5, 13.8, 16.8, 20.4, 24.4, 26.4, 26.1, 23.6, 20.3, 17.2, 13.9});
		values.Add(new double[] {10, 10, 12, 15, 20, 24, 26, 26, 23, 18, 14, 11});
		values.Add(new double[] {5, 5.3, 8, 12, 17, 22, 24.2, 24, 19, 15, 9, 6});
		values.Add(new double[] {9, 10, 11, 15, 19, 23, 26, 25, 22, 18, 13, 10});
		int[] colors = new int[] {Color.BLUE, Color.GREEN, Color.CYAN, Color.YELLOW};
		PointStyle[] styles = new PointStyle[] {PointStyle.CIRCLE, PointStyle.DIAMOND, PointStyle.TRIANGLE, PointStyle.SQUARE};
		XYMultipleSeriesRenderer renderer = buildRenderer(colors, styles);
		int length = renderer.SeriesRendererCount;
		for (int i = 0; i < length; i++)
		{
		  ((XYSeriesRenderer) renderer.getSeriesRendererAt(i)).FillPoints = true;
		}
		setChartSettings(renderer, "Average temperature", "Month", "Temperature", 0.5, 12.5, -10, 40, Color.LTGRAY, Color.LTGRAY);
		renderer.XLabels = 12;
		renderer.YLabels = 10;
		renderer.ShowGrid = true;
		renderer.XLabelsAlign = Align.RIGHT;
		renderer.YLabelsAlign = Align.RIGHT;
		renderer.ZoomButtonsVisible = true;
		renderer.PanLimits = new double[] {-10, 20, -10, 40};
		renderer.ZoomLimits = new double[] {-10, 20, -10, 40};

		XYMultipleSeriesDataset dataset = buildDataset(titles, x, values);
		XYSeries series = dataset.getSeriesAt(0);
		series.addAnnotation("Vacation", 6, 30);
		Intent intent = ChartFactory.getLineChartIntent(context, dataset, renderer, "Average temperature");
		return intent;
	  }

	}

}