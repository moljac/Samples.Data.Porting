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
	using RangeCategorySeries = org.achartengine.model.RangeCategorySeries;
	using XYMultipleSeriesDataset = org.achartengine.model.XYMultipleSeriesDataset;
	using SimpleSeriesRenderer = org.achartengine.renderer.SimpleSeriesRenderer;
	using XYMultipleSeriesRenderer = org.achartengine.renderer.XYMultipleSeriesRenderer;

	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Color = android.graphics.Color;
	using Align = android.graphics.Paint.Align;

	/// <summary>
	/// Temperature demo range chart.
	/// </summary>
	public class TemperatureChart : AbstractDemoChart
	{

	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  public override string Name
	  {
		  get
		  {
			return "Temperature range chart";
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
			return "The monthly temperature (vertical range chart)";
		  }
	  }

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  public override Intent execute(Context context)
	  {
		double[] minValues = new double[] {-24, -19, -10, -1, 7, 12, 15, 14, 9, 1, -11, -16};
		double[] maxValues = new double[] {7, 12, 24, 28, 33, 35, 37, 36, 28, 19, 11, 4};

		XYMultipleSeriesDataset dataset = new XYMultipleSeriesDataset();
		RangeCategorySeries series = new RangeCategorySeries("Temperature");
		int length = minValues.Length;
		for (int k = 0; k < length; k++)
		{
		  series.add(minValues[k], maxValues[k]);
		}
		dataset.addSeries(series.toXYSeries());
		int[] colors = new int[] {Color.CYAN};
		XYMultipleSeriesRenderer renderer = buildBarRenderer(colors);
		setChartSettings(renderer, "Monthly temperature range", "Month", "Celsius degrees", 0.5, 12.5, -30, 45, Color.GRAY, Color.LTGRAY);
		renderer.BarSpacing = 0.5;
		renderer.XLabels = 0;
		renderer.YLabels = 10;
		renderer.addXTextLabel(1, "Jan");
		renderer.addXTextLabel(3, "Mar");
		renderer.addXTextLabel(5, "May");
		renderer.addXTextLabel(7, "Jul");
		renderer.addXTextLabel(10, "Oct");
		renderer.addXTextLabel(12, "Dec");
		renderer.addYTextLabel(-25, "Very cold");
		renderer.addYTextLabel(-10, "Cold");
		renderer.addYTextLabel(5, "OK");
		renderer.addYTextLabel(20, "Nice");
		renderer.Margins = new int[] {30, 70, 10, 0};
		renderer.YLabelsAlign = Align.RIGHT;
		SimpleSeriesRenderer r = renderer.getSeriesRendererAt(0);
		r.DisplayChartValues = true;
		r.ChartValuesTextSize = 12;
		r.ChartValuesSpacing = 3;
		r.GradientEnabled = true;
		r.setGradientStart(-20, Color.BLUE);
		r.setGradientStop(20, Color.GREEN);
		return ChartFactory.getRangeBarChartIntent(context, dataset, renderer, Type.DEFAULT, "Temperature range");
	  }

	}

}