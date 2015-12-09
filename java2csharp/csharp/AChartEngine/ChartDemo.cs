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


	using AverageCubicTemperatureChart = org.achartengine.chartdemo.demo.chart.AverageCubicTemperatureChart;
	using AverageTemperatureChart = org.achartengine.chartdemo.demo.chart.AverageTemperatureChart;
	using BudgetDoughnutChart = org.achartengine.chartdemo.demo.chart.BudgetDoughnutChart;
	using BudgetPieChart = org.achartengine.chartdemo.demo.chart.BudgetPieChart;
	using CombinedTemperatureChart = org.achartengine.chartdemo.demo.chart.CombinedTemperatureChart;
	using IDemoChart = org.achartengine.chartdemo.demo.chart.IDemoChart;
	using MultipleTemperatureChart = org.achartengine.chartdemo.demo.chart.MultipleTemperatureChart;
	using PieChartBuilder = org.achartengine.chartdemo.demo.chart.PieChartBuilder;
	using ProjectStatusBubbleChart = org.achartengine.chartdemo.demo.chart.ProjectStatusBubbleChart;
	using ProjectStatusChart = org.achartengine.chartdemo.demo.chart.ProjectStatusChart;
	using SalesBarChart = org.achartengine.chartdemo.demo.chart.SalesBarChart;
	using SalesComparisonChart = org.achartengine.chartdemo.demo.chart.SalesComparisonChart;
	using SalesGrowthChart = org.achartengine.chartdemo.demo.chart.SalesGrowthChart;
	using SalesStackedBarChart = org.achartengine.chartdemo.demo.chart.SalesStackedBarChart;
	using ScatterChart = org.achartengine.chartdemo.demo.chart.ScatterChart;
	using SensorValuesChart = org.achartengine.chartdemo.demo.chart.SensorValuesChart;
	using TemperatureChart = org.achartengine.chartdemo.demo.chart.TemperatureChart;
	using TrigonometricFunctionsChart = org.achartengine.chartdemo.demo.chart.TrigonometricFunctionsChart;
	using WeightDialChart = org.achartengine.chartdemo.demo.chart.WeightDialChart;
	using XYChartBuilder = org.achartengine.chartdemo.demo.chart.XYChartBuilder;

	using ListActivity = android.app.ListActivity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using ListView = android.widget.ListView;
	using SimpleAdapter = android.widget.SimpleAdapter;

	public class ChartDemo : ListActivity
	{
	  private IDemoChart[] mCharts = new IDemoChart[]
	  {
		  new AverageTemperatureChart(),
		  new AverageCubicTemperatureChart(),
		  new SalesStackedBarChart(),
		  new SalesBarChart(),
		  new TrigonometricFunctionsChart(),
		  new ScatterChart(),
		  new SalesComparisonChart(),
		  new ProjectStatusChart(),
		  new SalesGrowthChart(),
		  new BudgetPieChart(),
		  new BudgetDoughnutChart(),
		  new ProjectStatusBubbleChart(),
		  new TemperatureChart(),
		  new WeightDialChart(),
		  new SensorValuesChart(),
		  new CombinedTemperatureChart(),
		  new MultipleTemperatureChart()
	  };

	  private string[] mMenuText;

	  private string[] mMenuSummary;

	  /// <summary>
	  /// Called when the activity is first created. </summary>
	  public override void onCreate(Bundle savedInstanceState)
	  {
		base.onCreate(savedInstanceState);
		int length = mCharts.Length;
		mMenuText = new string[length + 3];
		mMenuSummary = new string[length + 3];
		mMenuText[0] = "Embedded line chart demo";
		mMenuSummary[0] = "A demo on how to include a clickable line chart into a graphical activity";
		mMenuText[1] = "Embedded pie chart demo";
		mMenuSummary[1] = "A demo on how to include a clickable pie chart into a graphical activity";
		for (int i = 0; i < length; i++)
		{
		  mMenuText[i + 2] = mCharts[i].Name;
		  mMenuSummary[i + 2] = mCharts[i].Desc;
		}
		mMenuText[length + 2] = "Random values charts";
		mMenuSummary[length + 2] = "Chart demos using randomly generated values";
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

	  protected internal override void onListItemClick(ListView l, View v, int position, long id)
	  {
		base.onListItemClick(l, v, position, id);
		Intent intent = null;
		if (position == 0)
		{
		  intent = new Intent(this, typeof(XYChartBuilder));
		}
		else if (position == 1)
		{
		  intent = new Intent(this, typeof(PieChartBuilder));
		}
		else if (position <= mCharts.Length + 1)
		{
		  intent = mCharts[position - 2].execute(this);
		}
		else
		{
		  intent = new Intent(this, typeof(GeneratedChartDemo));
		}
		startActivity(intent);
	  }
	}
}