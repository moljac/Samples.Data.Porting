namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Context = android.content.Context;
	using TypedArray = android.content.res.TypedArray;
	using Bitmap = android.graphics.Bitmap;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Paint = android.graphics.Paint;
	using Rect = android.graphics.Rect;
	using BitmapDrawable = android.graphics.drawable.BitmapDrawable;
	using Drawable = android.graphics.drawable.Drawable;
	using LayerDrawable = android.graphics.drawable.LayerDrawable;
	using Log = android.util.Log;
	using View = android.view.View;

	using FcContext = com.samsung.android.sdk.professionalaudio.widgets.refactor.FcContext;

	/// 
	/// <summary>
	/// Class for obtaining drawables with numbers on them.
	/// 
	/// </summary>
	public class DrawableTool
	{

		private const int DEFFAULT_PROPORTION_TO_ICON = 50;

		/// 
		/// <summary>
		/// This method returns the given drawable with a number on it.
		/// </summary>
		/// <param name="baseDrawable">
		///            Drawable to which number will be added </param>
		/// <param name="number">
		///            An integer which will be expressed by the drawable </param>
		/// <param name="shiftFromLeft">
		///            Left margin of number drawable </param>
		/// <param name="shiftFromBottom">
		///            Bottom margin of number drawable </param>
		/// <param name="context">
		///            The base context </param>
		/// <returns> Drawable with number </returns>
		public static Drawable getDrawableWithNumber(Drawable baseDrawable, int number, int shiftFromLeft, int shiftFromBottom, Context context)
		{

			if (number < 0 || number > 9)
			{
				number = 0;
			}
			int textSize = context.Resources.getDimensionPixelSize(R.dimen.number_icon_text_size);

			// TODO: Remove hardcoded text drawable color
			TextDrawable textDrawable = new TextDrawable(number.ToString(), textSize, Color.WHITE);
			Drawable numberDrawable = context.Resources.getDrawable(R.drawable.number_round);

			textDrawable.setBounds(0, 0, numberDrawable.IntrinsicWidth, numberDrawable.IntrinsicHeight);

			Drawable[] drawables = new Drawable[] {baseDrawable, numberDrawable, textDrawable};
			LayerDrawable layerDrawable = new LayerDrawable(drawables);
			layerDrawable.setLayerInset(1, shiftFromLeft, 0, 0, shiftFromBottom);
			layerDrawable.setLayerInset(2, shiftFromLeft, 0, 0, shiftFromBottom);

			return layerDrawable;
		}

		public static Drawable getDrawableWithNumber(Drawable baseDrawable, int number, int iconSize, Context context)
		{

			if (number < 0 || number > 9)
			{
				number = 0;
			}
			Drawable numberDrawable = context.Resources.getDrawable(R.drawable.number_round);
			if (numberDrawable == null)
			{
				return baseDrawable;
			}
			int appIconShift = context.Resources.getDimensionPixelOffset(R.dimen.ord_app_icon_shift);
			Bitmap bitmap = Bitmap.createBitmap(iconSize, iconSize, Bitmap.Config.ARGB_8888);
			Canvas canvas = new Canvas(bitmap);
			int textSize = context.Resources.getDimensionPixelSize(R.dimen.number_icon_text_size);
			int textColor = context.Resources.getColor(R.color.fc_app_icon_number_color);
			TextDrawable textDrawable = new TextDrawable(number.ToString(), textSize, textColor);


			baseDrawable.setBounds(0, 0, iconSize, iconSize);
			baseDrawable.draw(canvas);

			numberDrawable.setBounds(iconSize - (appIconShift + numberDrawable.IntrinsicWidth), appIconShift, iconSize - (appIconShift), appIconShift + numberDrawable.IntrinsicHeight);
			numberDrawable.draw(canvas);

			textDrawable.setBounds(iconSize - (appIconShift + numberDrawable.IntrinsicWidth), appIconShift, iconSize - (appIconShift), appIconShift + numberDrawable.IntrinsicHeight);
			textDrawable.draw(canvas);
			return new BitmapDrawable(context.Resources, bitmap);
		}

		/// <summary>
		/// This method returns the given drawable with a number on it. The number drawable will be on
		/// the top left of base drawable and its size with respect to the base drawable shall be set in
		/// theme attributes R.attr.numberIconWidthAsPercentOfAppIcon and
		/// R.attr.numberIconHeightAsPercentOfAppIcon (default is 50%)
		/// </summary>
		/// <param name="baseDrawable">
		///            Drawable to which number will be added </param>
		/// <param name="number">
		///            An integer which will be expressed by the drawable </param>
		/// <param name="context">
		///            The base context </param>
		/// <returns> Drawable with number </returns>
		public static Drawable getDefaultDrawableWithNumber(Drawable baseDrawable, int number, Context context)
		{
			int[] percentArray = new int[] {R.attr.numberIconWidthAsPercentOfAppIcon, R.attr.numberIconHeightAsPercentOfAppIcon};

			TypedArray array = context.obtainStyledAttributes(percentArray);
			array.getInt(0, 50);

			float appIconWidthRatio = (float)array.getInt(0, DEFFAULT_PROPORTION_TO_ICON) / 100f;
			float appIconHeightRatio = (float)array.getInt(1, DEFFAULT_PROPORTION_TO_ICON) / 100f;

			int shiftFromLeft = (int)((1.0f - appIconWidthRatio) * baseDrawable.IntrinsicWidth);
			int shiftFromBottom = (int)((1.0f - appIconHeightRatio) * baseDrawable.IntrinsicHeight);
			array.recycle();

			return getDrawableWithNumber(baseDrawable, number, shiftFromLeft, shiftFromBottom, context);
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public static void setBackground(final android.view.View view, final int index, final int numberOfElements, final com.samsung.android.sdk.professionalaudio.widgets.refactor.FcContext fcContext)
		public static void setBackground(View view, int index, int numberOfElements, FcContext fcContext)
		{
			Drawable background;

			// It is the only button
			if (index == 0 && numberOfElements == 1)
			{
				background = fcContext.getDrawable(R.drawable.action_button_background_only);
			// It is the first button of many
			}
			else if (index == 0 && numberOfElements > 1)
			{
				background = fcContext.getDrawable(R.drawable.action_button_background_start);
			// It is the last button of many
			}
			else if (index == numberOfElements - 1)
			{
				background = fcContext.getDrawable(R.drawable.action_button_background_end);
			// In other case it is center button
			}
			else
			{
				background = fcContext.getDrawable(R.drawable.action_button_background_center);
			}

			view.Background = background;
		}

		private DrawableTool()
		{
		}
	}

}