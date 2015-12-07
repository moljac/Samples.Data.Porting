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

	using CategorySeries = org.achartengine.model.CategorySeries;
	using DialRenderer = org.achartengine.renderer.DialRenderer;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using Type = org.achartengine.renderer.DialRenderer.Type;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;

	/// <summary>
	/// Budget demo pie chart.
	/// </summary>
	public class WeightDialChart : AbstractDemoChart
	{
	  /// <summary>
	  /// Returns the chart name. </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Weight chart";
		  }
	  }

	  /// <summary>
	  /// Returns the chart description. </summary>
	  /// <returns> the chart description </returns>
	  public override string Desc
	  {
		  get
		  {
			return "The weight indicator (dial chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo. </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		CategorySeries category = new CategorySeries("Weight indic");
		category.add("Current", 75);
		category.add("Minimum", 65);
		category.add("Maximum", 90);
		DialRenderer renderer = new DialRenderer();
		renderer.ChartTitleTextSize = 20;
		renderer.LabelsTextSize = 15;
		renderer.LegendTextSize = 15;
		renderer.Margins = new int[] {20, 30, 15, 0};
		SimpleSeriesRenderer r = new SimpleSeriesRenderer();
		r.Color = Color.BLUE;
		renderer.addSeriesRenderer(r);
		r = new SimpleSeriesRenderer();
		r.Color = Color.rgb(0, 150, 0);
		renderer.addSeriesRenderer(r);
		r = new SimpleSeriesRenderer();
		r.Color = Color.GREEN;
		renderer.addSeriesRenderer(r);
		renderer.LabelsTextSize = 10;
		renderer.LabelsColor = Color.WHITE;
		renderer.ShowLabels = true;
		renderer.VisualTypes = new DialRenderer.Type[] {DialRenderer.Type.ARROW, DialRenderer.Type.NEEDLE, DialRenderer.Type.NEEDLE};
		renderer.MinValue = 0;
		renderer.MaxValue = 150;
		return ChartFactory.getDialChartIntent(context, category, renderer, "Weight indicator");
	  }

	}

}