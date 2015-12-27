/*
 * Copyright 2014 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace io.realm.examples.threads.widget
{

	using Context = android.content.Context;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Paint = android.graphics.Paint;
	using AttributeSet = android.util.AttributeSet;
	using View = android.view.View;

	using Dot = io.realm.examples.threads.model.Dot;

	/// <summary>
	/// Custom view that plot (x,y) coordinates from a RealmQuery
	/// </summary>
	public class DotsView : View
	{

		// RealmResults will automatically be up to date.
		private RealmResults<Dot> results;

		private Paint circlePaint;
		private float circleRadius;
		private float pixelsPrWidth;
		private float pixelsPrHeight;


		public DotsView(Context context) : base(context)
		{
			init();
		}

		public DotsView(Context context, AttributeSet attrs) : base(context, attrs)
		{
			init();
		}

		public DotsView(Context context, AttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
		{
			init();
		}

		private void init()
		{
			circlePaint = new Paint(Paint.ANTI_ALIAS_FLAG);
			circlePaint.Style = Paint.Style.FILL;
			circlePaint.Color = Color.RED;
			circleRadius = 10;
		}

		public virtual RealmResults RealmResults
		{
			set
			{
				this.results = value;
				invalidate();
			}
		}

		protected internal override void onSizeChanged(int w, int h, int oldw, int oldh)
		{
			base.onSizeChanged(w, h, oldw, oldh);
			pixelsPrWidth = w / 100f;
			pixelsPrHeight = h / 100f;
		}

		protected internal override void onDraw(Canvas canvas)
		{
			base.onDraw(canvas);
			canvas.drawColor(Color.TRANSPARENT);
			foreach (Dot dot in results)
			{
				circlePaint.Color = dot.Color;
				canvas.drawCircle(dot.X * pixelsPrWidth, dot.Y * pixelsPrHeight, circleRadius, circlePaint);
			}
		}
	}

}