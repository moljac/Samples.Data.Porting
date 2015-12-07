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


	using BarChart = org.achartengine.chart.BarChart;
	using BubbleChart = org.achartengine.chart.BubbleChart;
	using CubicLineChart = org.achartengine.chart.CubicLineChart;
	using LineChart = org.achartengine.chart.LineChart;
	using PointStyle = org.achartengine.chart.PointStyle;
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYSeries = org.achartengine.model.XYSeries;
	using XYValueSeries = org.achartengine.model.XYValueSeries;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Align = android.graphics.Paint.Align;

	/// <summary>
	/// Combined temperature demo chart.
	/// </summary>
	public class CombinedTemperatureChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Combined temperature";
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
			return "The average temperature in 2 Greek islands, water temperature and sun shine hours (combined chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		string[] titles = new string[] {"Crete Air Temperature", "Skiathos Air Temperature"};
		IList<double[]> x = new List<double[]>();
		for (int i = 0; i < titles.Length; i++)
		{
		  x.Add(new double[] {1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12});
		}
		IList<double[]> values = new List<double[]>();
		values.Add(new double[] {12.3, 12.5, 13.8, 16.8, 20.4, 24.4, 26.4, 26.1, 23.6, 20.3, 17.2, 13.9});
		values.Add(new double[] {9, 10, 11, 15, 19, 23, 26, 25, 22, 18, 13, 10});
		int[] colors = new int[] {Color.GREEN, Color.rgb(200, 150, 0)};
		PointStyle[] styles = new PointStyle[] {PointStyle.CIRCLE, PointStyle.DIAMOND};
		XYMultipleSeriesRenderer renderer = buildRenderer(colors, styles);
		renderer.PointSize = 5.5f;
		int length = renderer.SeriesRendererCount;
		for (int i = 0; i < length; i++)
		{
		  XYSeriesRenderer r = (XYSeriesRenderer) renderer.getSeriesRendererAt(i);
		  r.LineWidth = 5;
		  r.FillPoints = true;
		}
		setChartSettings(renderer, "Weather data", "Month", "Temperature", 0.5, 12.5, 0, 40, Color.LTGRAY, Color.LTGRAY);

		renderer.XLabels = 12;
		renderer.YLabels = 10;
		renderer.ShowGrid = true;
		renderer.XLabelsAlign = Align.RIGHT;
		renderer.YLabelsAlign = Align.RIGHT;
		renderer.ZoomButtonsVisible = true;
		renderer.PanLimits = new double[] {-10, 20, -10, 40};
		renderer.ZoomLimits = new double[] {-10, 20, -10, 40};

		XYValueSeries sunSeries = new XYValueSeries("Sunshine hours");
		sunSeries.add(1f, 35, 4.3);
		sunSeries.add(2f, 35, 4.9);
		sunSeries.add(3f, 35, 5.9);
		sunSeries.add(4f, 35, 8.8);
		sunSeries.add(5f, 35, 10.8);
		sunSeries.add(6f, 35, 11.9);
		sunSeries.add(7f, 35, 13.6);
		sunSeries.add(8f, 35, 12.8);
		sunSeries.add(9f, 35, 11.4);
		sunSeries.add(10f, 35, 9.5);
		sunSeries.add(11f, 35, 7.5);
		sunSeries.add(12f, 35, 5.5);
		XYSeriesRenderer lightRenderer = new XYSeriesRenderer();
		lightRenderer.Color = Color.YELLOW;

		XYSeries waterSeries = new XYSeries("Water Temperature");
		waterSeries.add(1, 16);
		waterSeries.add(2, 15);
		waterSeries.add(3, 16);
		waterSeries.add(4, 17);
		waterSeries.add(5, 20);
		waterSeries.add(6, 23);
		waterSeries.add(7, 25);
		waterSeries.add(8, 25.5);
		waterSeries.add(9, 26.5);
		waterSeries.add(10, 24);
		waterSeries.add(11, 22);
		waterSeries.add(12, 18);
		renderer.BarSpacing = 0.5;
		XYSeriesRenderer waterRenderer = new XYSeriesRenderer();
		waterRenderer.Color = Color.argb(250, 0, 210, 250);

		XYMultipleSeriesDataset dataset = buildDataset(titles, x, values);
		dataset.addSeries(0, sunSeries);
		dataset.addSeries(0, waterSeries);
		renderer.addSeriesRenderer(0, lightRenderer);
		renderer.addSeriesRenderer(0, waterRenderer);
		waterRenderer.DisplayChartValues = true;
		waterRenderer.ChartValuesTextSize = 10;

		string[] types = new string[] {BarChart.TYPE, BubbleChart.TYPE, LineChart.TYPE, CubicLineChart.TYPE};
		Intent intent = ChartFactory.getCombinedXYChartIntent(context, dataset, renderer, types, "Weather parameters");
		return intent;
	  }

	}

}