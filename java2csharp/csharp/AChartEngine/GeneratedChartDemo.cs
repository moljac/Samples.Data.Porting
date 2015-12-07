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
namespace org.achartengine.chartdemo.demo
{


	using Type = org.achartengine.chart.BarChart.Type;
	using PointStyle = org.achartengine.chart.PointStyle;
	using TimeChart = org.achartengine.chart.TimeChart;
	using IDemoChart = org.achartengine.chartdemo.demo.chart.IDemoChart;
	using CategorySeries = org.achartengine.model.CategorySeries;
	using TimeSeries = org.achartengine.model.TimeSeries;
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYSeries = org.achartengine.model.XYSeries;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using ListActivity = android.app.ListActivity;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ListView = android.widget.ListView;
	using SimpleAdapter = android.widget.SimpleAdapter;

	public class GeneratedChartDemo : ListActivity
	{
	  private const int SERIES_NR = 2;

	  private string[] mMenuText;

	  private string[] mMenuSummary;

	  /// <summary>
	  /// Called when the activity is first created. </summary>
	  public override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		// I know, I know, this should go into strings.xml and accessed using
		// getString(R.string....)
		mMenuText = new string[] {"Line chart", "Scatter chart", "Time chart", "Bar chart"};
		mMenuSummary = new string[] {"Line chart with randomly generated values", "Scatter chart with randomly generated values", "Time chart with randomly generated values", "Bar chart with randomly generated values"};
		ListAdapter = new SimpleAdapter(this, ListValues, android.R.layout.simple_list_item_2, new string[] {org.achartengine.chartdemo.demo.chart.IDemoChart_Fields.NAME, org.achartengine.chartdemo.demo.chart.IDemoChart_Fields.DESC}, new int[] {android.R.id.text1, android.R.id.text2});
	  }

	  private IList<IDictionary<string, string>> ListValues
	  {
		  get
		  {
			IList<IDictionary<string, string>> values = new List<IDictionary<string, string>>();
			int length = mMenuText.Length;
			for (int i = 0; i < length; i++)
			{
			  IDictionary<string, string> v = new Dictionary<string, string>();
			  v[org.achartengine.chartdemo.demo.chart.IDemoChart_Fields.NAME] = mMenuText[i];
			  v[org.achartengine.chartdemo.demo.chart.IDemoChart_Fields.DESC] = mMenuSummary[i];
			  values.Add(v);
			}
			return values;
		  }
	  }

	  private XYMultipleSeriesDataset DemoDataset
	  {
		  get
		  {
			XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
			const int nr = 10;
			Random r = new Random();
			for (int i = 0; i < SERIES_NR; i++)
			{
			  XYSeries series = new XYSeries("Demo series " + (i + 1));
			  for (int k = 0; k < nr; k++)
			  {
				series.add(k, 20 + r.Next() % 100);
			  }
			  dataset.addSeries(series);
			}
			return dataset;
		  }
	  }

	  private XYMultipleSeriesDataset DateDemoDataset
	  {
		  get
		  {
			XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
			const int nr = 10;
			long value = (DateTime.Now).Ticks - 3 * TimeChart.DAY;
			Random r = new Random();
			for (int i = 0; i < SERIES_NR; i++)
			{
			  TimeSeries series = new TimeSeries("Demo series " + (i + 1));
			  for (int k = 0; k < nr; k++)
			  {
				series.add(new DateTime(value + k * TimeChart.DAY / 4), 20 + r.Next() % 100);
			  }
			  dataset.addSeries(series);
			}
			return dataset;
		  }
	  }

	  private XYMultipleSeriesDataset BarDemoDataset
	  {
		  get
		  {
			XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
			const int nr = 10;
			Random r = new Random();
			for (int i = 0; i < SERIES_NR; i++)
			{
			  CategorySeries series = new CategorySeries("Demo series " + (i + 1));
			  for (int k = 0; k < nr; k++)
			  {
				series.add(100 + r.Next() % 100);
			  }
			  dataset.addSeries(series.toXYSeries());
			}
			return dataset;
		  }
	  }

	  private XYMultipleSeriesRenderer DemoRenderer
	  {
		  get
		  {
			XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer();
			renderer.AxisTitleTextSize = 16;
			renderer.ChartTitleTextSize = 20;
			renderer.LabelsTextSize = 15;
			renderer.LegendTextSize = 15;
			renderer.PointSize = 5f;
			renderer.Margins = new int[] {20, 30, 15, 0};
			XYSeriesRenderer r = new XYSeriesRenderer();
			r.Color = Color.BLUE;
			r.PointStyle = PointStyle.SQUARE;
			r.FillBelowLine = true;
			r.FillBelowLineColor = Color.WHITE;
			r.FillPoints = true;
			renderer.addSeriesRenderer(r);
			r = new XYSeriesRenderer();
			r.PointStyle = PointStyle.CIRCLE;
			r.Color = Color.GREEN;
			r.FillPoints = true;
			renderer.addSeriesRenderer(r);
			renderer.AxesColor = Color.DKGRAY;
			renderer.LabelsColor = Color.LTGRAY;
			return renderer;
		  }
	  }

	  public virtual XYMultipleSeriesRenderer BarDemoRenderer
	  {
		  get
		  {
			XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer();
			renderer.AxisTitleTextSize = 16;
			renderer.ChartTitleTextSize = 20;
			renderer.LabelsTextSize = 15;
			renderer.LegendTextSize = 15;
			renderer.Margins = new int[] {20, 30, 15, 0};
			SimpleSeriesRenderer r = new SimpleSeriesRenderer();
			r.Color = Color.BLUE;
			renderer.addSeriesRenderer(r);
			r = new SimpleSeriesRenderer();
			r.Color = Color.GREEN;
			renderer.addSeriesRenderer(r);
			return renderer;
		  }
	  }

	  private XYMultipleSeriesRenderer ChartSettings
	  {
		  set
		  {
			value.ChartTitle = "Chart demo";
			value.XTitle = "x values";
			value.YTitle = "y values";
			value.XAxisMin = 0.5;
			value.XAxisMax = 10.5;
			value.YAxisMin = 0;
			value.YAxisMax = 210;
		  }
	  }

	  protected internal override void onListItemClick(ListView l, View v, int position, long id)
	  {
		base.onListItemClick(l, v, position, id);
		switch (position)
		{
		case 0:
		  Intent intent = ChartFactory.getLineChartIntent(this, DemoDataset, DemoRenderer);
		  startActivity(intent);
		  break;
		case 1:
		  intent = ChartFactory.getScatterChartIntent(this, DemoDataset, DemoRenderer);
		  startActivity(intent);
		  break;
		case 2:
		  intent = ChartFactory.getTimeChartIntent(this, DateDemoDataset, DemoRenderer, null);
		  startActivity(intent);
		  break;
		case 3:
		  XYMultipleSeriesRenderer renderer = BarDemoRenderer;
		  ChartSettings = renderer;
		  intent = ChartFactory.getBarChartIntent(this, BarDemoDataset, renderer, Type.DEFAULT);
		  startActivity(intent);
		  break;
		}
	  }
	}
}