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

	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using XYValueSeries = org.achartengine.model.XYValueSeries;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;
	using XYSeriesRenderer = org.achartengine.renderer.XYSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;

	/// <summary>
	/// Project status demo bubble chart.
	/// </summary>
	public class ProjectStatusBubbleChart : AbstractDemoChart
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
			return "The opened tickets and the fixed tickets (bubble chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		XYMultipleSeriesDataset series = new XYMultipleSeriesDataset();
		XYValueSeries newTicketSeries = new XYValueSeries("New Tickets");
		newTicketSeries.add(1f, 2, 14);
		newTicketSeries.add(2f, 2, 12);
		newTicketSeries.add(3f, 2, 18);
		newTicketSeries.add(4f, 2, 5);
		newTicketSeries.add(5f, 2, 1);
		series.addSeries(newTicketSeries);
		XYValueSeries fixedTicketSeries = new XYValueSeries("Fixed Tickets");
		fixedTicketSeries.add(1f, 1, 7);
		fixedTicketSeries.add(2f, 1, 4);
		fixedTicketSeries.add(3f, 1, 18);
		fixedTicketSeries.add(4f, 1, 3);
		fixedTicketSeries.add(5f, 1, 1);
		series.addSeries(fixedTicketSeries);

		XYMultipleSeriesRenderer renderer = new XYMultipleSeriesRenderer();
		renderer.AxisTitleTextSize = 16;
		renderer.ChartTitleTextSize = 20;
		renderer.LabelsTextSize = 15;
		renderer.LegendTextSize = 15;
		renderer.Margins = new int[] {20, 30, 15, 0};
		XYSeriesRenderer newTicketRenderer = new XYSeriesRenderer();
		newTicketRenderer.Color = Color.BLUE;
		renderer.addSeriesRenderer(newTicketRenderer);
		XYSeriesRenderer fixedTicketRenderer = new XYSeriesRenderer();
		fixedTicketRenderer.Color = Color.GREEN;
		renderer.addSeriesRenderer(fixedTicketRenderer);

		setChartSettings(renderer, "Project work status", "Priority", "", 0.5, 5.5, 0, 5, Color.GRAY, Color.LTGRAY);
		renderer.XLabels = 7;
		renderer.YLabels = 0;
		renderer.ShowGrid = false;
		return ChartFactory.getBubbleChartIntent(context, series, renderer, "Project tickets");
	  }

	}

}