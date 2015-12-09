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

	using Context = android.content.Context;
	using Intent = android.content.Intent;

	/// <summary>
	/// Defines the demo charts.
	/// </summary>
	public interface IDemoChart
	{
	  /// <summary>
	  /// A constant for the name field in a list activity. </summary>
	  /// <summary>
	  /// A constant for the description field in a list activity. </summary>

	  /// <summary>
	  /// Returns the chart name.
	  /// </summary>
	  /// <returns> the chart name </returns>
	  string Name {get;}

	  /// <summary>
	  /// Returns the chart description.
	  /// </summary>
	  /// <returns> the chart description </returns>
	  string Desc {get;}

	  /// <summary>
	  /// Executes the chart demo.
	  /// </summary>
	  /// <param name="context"> the context </param>
	  /// <returns> the built intent </returns>
	  Intent execute(Context context);

	}

	public static class IDemoChart_Fields
	{
	  public const string NAME = "name";
	  public const string DESC = "desc";
	}

}