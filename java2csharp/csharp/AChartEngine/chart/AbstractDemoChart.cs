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
	using CategorySeries = org.achartengine.model.CategorySeries;
	using MultipleCategorySeries = org.achartengine.model.MultipleCategorySeries;
	using TimeSeries = org.achartengine.model.TimeSeries;
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYSeries = org.achartengine.model.XYSeries;
	using DefaultRenderer = org.achartengine.renderer.DefaultRenderer;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	/// <summary>
	/// An abstract class for the demo charts to extend. It contains some methods for
	/// building datasets and renderers.
	/// </summary>
	public abstract class AbstractDemoChart : IDemoChart
	{
		public abstract android.content.Intent execute(android.content.Context context);
		public abstract string Desc {get;}
		public abstract string Name {get;}

	  /// <summary>
	  /// Builds an XY multiple dataset using the provided values.
	  /// </summary>
	  /// <param name="titles"> the series titles </param>
	  /// <param name="xValues"> the values for the X axis </param>
	  /// <param name="yValues"> the values for the Y axis </param>
	  /// <returns> the XY multiple dataset </returns>
	  protected internal virtual XYMultipleSeriesDataset buildDataset(string[] titles, IList<double[]> xValues, IList<double[]> yValues)
	  {
		XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
		addXYSeries(dataset, titles, xValues, yValues, 0);
		return dataset;
	  }

	  public virtual void addXYSeries(XYMultipleSeriesDataset dataset, string[] titles, IList<double[]> xValues, IList<double[]> yValues, int scale)
	  {
		int length = titles.Length;
		for (int i = 0; i < length; i++)
		{
		  XYSeries series = new XYSeries(titles[i], scale);
		  double[] xV = xValues[i];
		  double[] yV = yValues[i];
		  int seriesLength = xV.Length;
		  for (int k = 0; k < seriesLength; k++)
		  {
			series.add(xV[k], yV[k]);
		  }
		  dataset.addSeries(series);
		}
	  }

	  /// <summary>
	  /// Builds an XY multiple series renderer.
	  /// </summary>
	  /// <param name="colors"> the series rendering colors </param>
	  /// <param name="styles"> the series point styles </param>
	  /// <returns> the XY multiple series renderers </returns>
	  protected internal virtual XYMultipleSeriesRenderer buildRenderer(int[] colors, PointStyle[] styles)
	  {
		XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer();
		setRenderer(renderer, colors, styles);
		return renderer;
	  }

	  protected internal virtual void setRenderer(XYMultipleSeriesRenderer renderer, int[] colors, PointStyle[] styles)
	  {
		renderer.AxisTitleTextSize = 16;
		renderer.ChartTitleTextSize = 20;
		renderer.LabelsTextSize = 15;
		renderer.LegendTextSize = 15;
		renderer.PointSize = 5f;
		renderer.Margins = new int[] {20, 30, 15, 20};
		int length = colors.Length;
		for (int i = 0; i < length; i++)
		{
		  XYSeriesRenderer r = new XYSeriesRenderer();
		  r.Color = colors[i];
		  r.PointStyle = styles[i];
		  renderer.addSeriesRenderer(r);
		}
	  }

	  /// <summary>
	  /// Sets a few of the series renderer settings.
	  /// </summary>
	  /// <param name="renderer"> the renderer to set the properties to </param>
	  /// <param name="title"> the chart title </param>
	  /// <param name="xTitle"> the title for the X axis </param>
	  /// <param name="yTitle"> the title for the Y axis </param>
	  /// <param name="xMin"> the minimum value on the X axis </param>
	  /// <param name="xMax"> the maximum value on the X axis </param>
	  /// <param name="yMin"> the minimum value on the Y axis </param>
	  /// <param name="yMax"> the maximum value on the Y axis </param>
	  /// <param name="axesColor"> the axes color </param>
	  /// <param name="labelsColor"> the labels color </param>
	  protected internal virtual void setChartSettings(XYMultipleSeriesRenderer renderer, string title, string xTitle, string yTitle, double xMin, double xMax, double yMin, double yMax, int axesColor, int labelsColor)
	  {
		renderer.ChartTitle = title;
		renderer.XTitle = xTitle;
		renderer.YTitle = yTitle;
		renderer.XAxisMin = xMin;
		renderer.XAxisMax = xMax;
		renderer.YAxisMin = yMin;
		renderer.YAxisMax = yMax;
		renderer.AxesColor = axesColor;
		renderer.LabelsColor = labelsColor;
	  }

	  /// <summary>
	  /// Builds an XY multiple time dataset using the provided values.
	  /// </summary>
	  /// <param name="titles"> the series titles </param>
	  /// <param name="xValues"> the values for the X axis </param>
	  /// <param name="yValues"> the values for the Y axis </param>
	  /// <returns> the XY multiple time dataset </returns>
	  protected internal virtual XYMultipleSeriesDataset buildDateDataset(string[] titles, IList<DateTime[]> xValues, IList<double[]> yValues)
	  {
		XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
		int length = titles.Length;
		for (int i = 0; i < length; i++)
		{
		  TimeSeries series = new TimeSeries(titles[i]);
		  DateTime[] xV = xValues[i];
		  double[] yV = yValues[i];
		  int seriesLength = xV.Length;
		  for (int k = 0; k < seriesLength; k++)
		  {
			series.add(xV[k], yV[k]);
		  }
		  dataset.addSeries(series);
		}
		return dataset;
	  }

	  /// <summary>
	  /// Builds a category series using the provided values.
	  /// </summary>
	  /// <param name="titles"> the series titles </param>
	  /// <param name="values"> the values </param>
	  /// <returns> the category series </returns>
	  protected internal virtual CategorySeries buildCategoryDataset(string title, double[] values)
	  {
		CategorySeries series = new CategorySeries(title);
		int k = 0;
		foreach (double value in values)
		{
		  series.add("Project " + ++k, value);
		}

		return series;
	  }

	  /// <summary>
	  /// Builds a multiple category series using the provided values.
	  /// </summary>
	  /// <param name="titles"> the series titles </param>
	  /// <param name="values"> the values </param>
	  /// <returns> the category series </returns>
	  protected internal virtual MultipleCategorySeries buildMultipleCategoryDataset(string title, IList<string[]> titles, IList<double[]> values)
	  {
		MultipleCategorySeries series = new MultipleCategorySeries(title);
		int k = 0;
		foreach (double[] value in values)
		{
		  series.add(2007 + k + "", titles[k], value);
		  k++;
		}
		return series;
	  }

	  /// <summary>
	  /// Builds a category renderer to use the provided colors.
	  /// </summary>
	  /// <param name="colors"> the colors </param>
	  /// <returns> the category renderer </returns>
	  protected internal virtual DefaultRenderer buildCategoryRenderer(int[] colors)
	  {
		DefaultRenderer renderer = new DefaultRenderer();
		renderer.LabelsTextSize = 15;
		renderer.LegendTextSize = 15;
		renderer.Margins = new int[] {20, 30, 15, 0};
		foreach (int color in colors)
		{
		  SimpleSeriesRenderer r = new SimpleSeriesRenderer();
		  r.Color = color;
		  renderer.addSeriesRenderer(r);
		}
		return renderer;
	  }

	  /// <summary>
	  /// Builds a bar multiple series dataset using the provided values.
	  /// </summary>
	  /// <param name="titles"> the series titles </param>
	  /// <param name="values"> the values </param>
	  /// <returns> the XY multiple bar dataset </returns>
	  protected internal virtual XYMultipleSeriesDataset buildBarDataset(string[] titles, IList<double[]> values)
	  {
		XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
		int length = titles.Length;
		for (int i = 0; i < length; i++)
		{
		  CategorySeries series = new CategorySeries(titles[i]);
		  double[] v = values[i];
		  int seriesLength = v.Length;
		  for (int k = 0; k < seriesLength; k++)
		  {
			series.add(v[k]);
		  }
		  dataset.addSeries(series.toXYSeries());
		}
		return dataset;
	  }

	  /// <summary>
	  /// Builds a bar multiple series renderer to use the provided colors.
	  /// </summary>
	  /// <param name="colors"> the series renderers colors </param>
	  /// <returns> the bar multiple series renderer </returns>
	  protected internal virtual XYMultipleSeriesRenderer buildBarRenderer(int[] colors)
	  {
		XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer();
		renderer.AxisTitleTextSize = 16;
		renderer.ChartTitleTextSize = 20;
		renderer.LabelsTextSize = 15;
		renderer.LegendTextSize = 15;
		int length = colors.Length;
		for (int i = 0; i < length; i++)
		{
		  SimpleSeriesRenderer r = new SimpleSeriesRenderer();
		  r.Color = colors[i];
		  renderer.addSeriesRenderer(r);
		}
		return renderer;
	  }

	}

}