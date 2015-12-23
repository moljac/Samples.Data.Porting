using System;
using System.Collections.Generic;

namespace com.firebase.officemover
{

	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using BlurMaskFilter = android.graphics.BlurMaskFilter;
	using Canvas = android.graphics.Canvas;
	using Matrix = android.graphics.Matrix;
	using Paint = android.graphics.Paint;
	using PorterDuff = android.graphics.PorterDuff;
	using Log = android.util.Log;
	using LruCache = android.util.LruCache;

	using OfficeThing = com.firebase.officemover.model.OfficeThing;


	/// <summary>
	/// A utility class that manages the bitmap representations of office things
	/// 
	/// @author Jenny Tong (mimming)
	/// @since 12/9/2014
	/// </summary>
	public class OfficeThingRenderUtil
	{
		private static readonly string TAG = typeof(OfficeThingRenderUtil).Name;

		private LruCache<string, Bitmap> mBitmapCache;
		private Context mContext;
		private IDictionary<string, int?> mHeightCache;
		private IDictionary<string, int?> mWidthCache;
		private float mCanvasRatio;

		public OfficeThingRenderUtil(Context context, float canvasRatio)
		{
			mContext = context;

			// Use 1/8th of the available memory for this memory cache.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int cacheSize = (int)(Runtime.getRuntime().maxMemory() / 1024) / 8;
			int cacheSize = (int)(Runtime.Runtime.maxMemory() / 1024) / 8;
			Log.v(TAG, "init bitmap cache with size " + cacheSize);
			mBitmapCache = new LruCacheAnonymousInnerClassHelper(this, cacheSize);

			mHeightCache = new Dictionary<>();
			mWidthCache = new Dictionary<>();
			mCanvasRatio = canvasRatio;
		}

		private class LruCacheAnonymousInnerClassHelper : LruCache<string, Bitmap>
		{
			private readonly OfficeThingRenderUtil outerInstance;

			public LruCacheAnonymousInnerClassHelper(OfficeThingRenderUtil outerInstance, int cacheSize) : base(cacheSize)
			{
				this.outerInstance = outerInstance;
			}

			protected internal override int sizeOf(string key, Bitmap bitmap)
			{
				// The cache size will be measured in kilobytes rather than number of items.
				return bitmap.ByteCount / 1024;
			}
		}

		private string getCacheKey(OfficeThing thing)
		{
			return thing.Type + "-" + thing.Rotation;
		}

		private string getCacheKey(OfficeThing thing, bool isGlowing)
		{
			if (isGlowing)
			{
				return thing.Type + "-" + thing.Rotation + "-glowing";
			}
			else
			{
				return getCacheKey(thing);
			}
		}

		public virtual Bitmap getBitmap(OfficeThing thing)
		{
			if (thing == null || thing.Type == null)
			{
				throw new Exception("Thing must be defined and not null");
			}

			string cacheKey = getCacheKey(thing);

			if (mBitmapCache.get(cacheKey) == null)
			{
				string packageName = mContext.PackageName;
				int resourceId = mContext.Resources.getIdentifier(thing.Type, "drawable", packageName);
				Bitmap bitmap = BitmapFactory.decodeResource(mContext.Resources, resourceId);
				int resourceWidth = bitmap.Width;
				int resourceHeight = bitmap.Height;

				// rotate counter clockwise
				Matrix matrix = new Matrix();
				matrix.postRotate(-thing.Rotation);
				matrix.postScale(0.93F, 0.93F); //TODO: figure out why this hack makes android match web
				// Is it render ratio / px per dp -> 1.5 / 1.8

				bitmap = Bitmap.createBitmap(bitmap, 0, 0, resourceWidth, resourceHeight, matrix, true);
				mBitmapCache.put(cacheKey, bitmap);
			}
			return mBitmapCache.get(cacheKey);
		}


		public virtual Bitmap getGlowingBitmap(OfficeThing thing)
		{
			if (thing == null || thing.Type == null)
			{
				throw new Exception("Thing must be defined and not null");
			}

			string cacheKey = getCacheKey(thing, true);
			if (mBitmapCache.get(cacheKey) == null)
			{
				// Get the non-glowy bitmap
				Bitmap bitmap = this.getBitmap(thing);

				// Blur it
				Paint blurPaint = new Paint();
				int[] offsetXY = new int[2];
				blurPaint.MaskFilter = new BlurMaskFilter(15, BlurMaskFilter.Blur.NORMAL);
				Bitmap bmAlpha = bitmap.extractAlpha(blurPaint, offsetXY);

				Paint glowPaint = new Paint();
				glowPaint.Color = 0xFF2896DD;

				// Create a virtual canvas on which to do the transformation and draw it
				Bitmap glowingBitmap = Bitmap.createBitmap(bitmap.Width + (-offsetXY[0] * 2), bitmap.Height + (-offsetXY[0] * 2), Bitmap.Config.ARGB_8888);
				Canvas canvas = new Canvas(glowingBitmap);
				canvas.drawColor(0, PorterDuff.Mode.CLEAR);
				canvas.drawBitmap(bmAlpha, 0, 0, glowPaint);
				bmAlpha.recycle();
				canvas.drawBitmap(bitmap, -offsetXY[0], -offsetXY[1], null);

				// Put it into the cache for later use
				mBitmapCache.put(cacheKey, glowingBitmap);
			}
			return mBitmapCache.get(cacheKey);
		}

		// TODO: make less copy and paste
		public virtual int getScreenHeight(OfficeThing thing)
		{
			string key = getCacheKey(thing);

			if (!mHeightCache.ContainsKey(key))
			{
				string packageName = mContext.PackageName;
				int resourceId = mContext.Resources.getIdentifier(thing.Type, "drawable", packageName);

				BitmapFactory.Options dimensions = new BitmapFactory.Options();
				dimensions.inJustDecodeBounds = true;
				BitmapFactory.decodeResource(mContext.Resources, resourceId, dimensions);

				int screenHeight;
				if (thing.Rotation == 0 || thing.Rotation == 180)
				{
					screenHeight = (int)((dimensions.outHeight / 2D) * mCanvasRatio);
				}
				else
				{
					screenHeight = (int)((dimensions.outWidth / 2D) * mCanvasRatio);
				}
				mHeightCache[key] = screenHeight;
			}
			return mHeightCache[key].Value;
		}

		public virtual int getScreenWidth(OfficeThing thing)
		{
			string key = getCacheKey(thing);

			if (!mWidthCache.ContainsKey(key))
			{
				string packageName = mContext.PackageName;
				int resourceId = mContext.Resources.getIdentifier(thing.Type, "drawable", packageName);

				BitmapFactory.Options dimensions = new BitmapFactory.Options();
				dimensions.inJustDecodeBounds = true;
				BitmapFactory.decodeResource(mContext.Resources, resourceId, dimensions);

				int screenWidth;
				if (thing.Rotation == 0 || thing.Rotation == 180)
				{
					screenWidth = (int)((dimensions.outWidth / 2D) * mCanvasRatio);
				}
				else
				{
					screenWidth = (int)((dimensions.outHeight / 2D) * mCanvasRatio);
				}
				mWidthCache[key] = screenWidth;
			}
			return mWidthCache[key].Value;
		}

		public virtual int getModelHeight(OfficeThing thing)
		{
			return (int)(getScreenHeight(thing) / mCanvasRatio);
		}
		public virtual int getModelWidth(OfficeThing thing)
		{
			return (int)(getScreenWidth(thing) / mCanvasRatio);
		}
	}



}