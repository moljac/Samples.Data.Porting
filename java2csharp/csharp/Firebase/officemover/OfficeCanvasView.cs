namespace com.firebase.officemover
{

	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using Canvas = android.graphics.Canvas;
	using Color = android.graphics.Color;
	using Paint = android.graphics.Paint;
	using Typeface = android.graphics.Typeface;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using ViewTreeObserver = android.view.ViewTreeObserver;

	using OfficeLayout = com.firebase.officemover.model.OfficeLayout;
	using OfficeThing = com.firebase.officemover.model.OfficeThing;

	/// <summary>
	/// This class contains the View that renders the office to the screen. There's not much in here
	/// that's specific to Firebase.
	/// 
	/// @author Jenny Tong (mimming)
	/// </summary>
	public class OfficeCanvasView : View
	{

		private static readonly string TAG = typeof(OfficeCanvasView).Name;
		private static readonly Paint DEFAULT_PAINT = new Paint();
		private static readonly Paint DESK_LABEL_PAINT = new Paint();

		// The height and width of the canvas in Firebase
		public const int LOGICAL_HEIGHT = 800;
		public const int LOGICAL_WIDTH = 600;

		public float mScreenRatio;

		private OfficeThing mSelectedThing;

		private OfficeLayout mOfficeLayout;

		// Listeners for communicating back to the owner activity
		private OfficeMoverActivity.ThingChangeListener mThingChangedListener;
		private OfficeMoverActivity.SelectedThingChangeListener mSelectedThingChangeListener;

		private OfficeThingRenderUtil mRenderUtil;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public OfficeCanvasView(final android.content.Context ct)
		public OfficeCanvasView(Context ct) : base(ct)
		{
			init();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public OfficeCanvasView(final android.content.Context ct, final android.util.AttributeSet attrs)
		public OfficeCanvasView(Context ct, AttributeSet attrs) : base(ct, attrs)
		{
			init();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: public OfficeCanvasView(final android.content.Context ct, final android.util.AttributeSet attrs, final int defStyle)
		public OfficeCanvasView(Context ct, AttributeSet attrs, int defStyle) : base(ct, attrs, defStyle)
		{
			init();
		}

		private void init()
		{
			Log.v(TAG, "init new canvas");
			DESK_LABEL_PAINT.Color = Color.WHITE;
			DESK_LABEL_PAINT.TextSize = 50;
			DESK_LABEL_PAINT.TextAlign = Paint.Align.CENTER;
			DESK_LABEL_PAINT.Typeface = Typeface.DEFAULT;


			// This view's height and width aren't known until they're measured once. Add a listener for
			// that time so we can calculate the conversion factor.
			this.ViewTreeObserver.addOnGlobalLayoutListener(new OnGlobalLayoutListenerAnonymousInnerClassHelper(this));
		}

		private class OnGlobalLayoutListenerAnonymousInnerClassHelper : ViewTreeObserver.OnGlobalLayoutListener
		{
			private readonly OfficeCanvasView outerInstance;

			public OnGlobalLayoutListenerAnonymousInnerClassHelper(OfficeCanvasView outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onGlobalLayout()
			{
				outerInstance.ViewTreeObserver.removeOnGlobalLayoutListener(this);
				if (outerInstance.Height / LOGICAL_HEIGHT != outerInstance.Width / LOGICAL_WIDTH)
				{
					Log.e(TAG, "Aspect ratio is incorrect. Expected " + LOGICAL_WIDTH / LOGICAL_HEIGHT + " got " + outerInstance.Width / outerInstance.Height);
				}
				// Set the conversion factor for pixels to logical (Firebase) model
				outerInstance.mScreenRatio = (float) OfficeCanvasView.this.Height / (float) LOGICAL_HEIGHT;
				outerInstance.mRenderUtil = new OfficeThingRenderUtil(Context, outerInstance.mScreenRatio);
			}
		}

		// Setters called from OfficeMoverActivity
		public virtual OfficeLayout OfficeLayout
		{
			set
			{
				this.mOfficeLayout = value;
			}
		}

		public virtual OfficeMoverActivity.ThingChangeListener ThingChangedListener
		{
			set
			{
				this.mThingChangedListener = value;
			}
		}

		public virtual OfficeMoverActivity.SelectedThingChangeListener ThingFocusChangeListener
		{
			set
			{
				this.mSelectedThingChangeListener = value;
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onDraw(final android.graphics.Canvas canv)
		public override void onDraw(Canvas canv)
		{
			if (null == mOfficeLayout || null == mRenderUtil)
			{
				Log.w(TAG, "Tried to render empty office");
				return;
			}

			foreach (OfficeThing thing in mOfficeLayout.ThingsBottomUp)
			{
				Bitmap thingBitmap = mRenderUtil.getBitmap(thing);

				//TODO: reimplement glow rendering
	//            // If it's the selected thing, make it GLOW!
	//            if (thing.getKey().equals(mSelectedThingKey)) {
	//                thingBitmap = mRenderUtil.getGlowingBitmap(thing);
	//            }

				// Draw furniture
				canv.drawBitmap(thingBitmap, modelToScreen(thing.Left), modelToScreen(thing.Top), DEFAULT_PAINT);

				// Draw desk label
				if (thing.Type.Equals("desk") && thing.Name != null)
				{
					// TODO: these offset numbers were empirically determined. Calculate them instead
					float centerX = modelToScreen(thing.Left) + 102;
					float centerY = modelToScreen(thing.Top) + 70;

					canv.save();
					// TODO: OMG this is so hacky. Fix it. These numbers were empirically determined
					if (thing.Rotation == 180)
					{
						canv.rotate(-thing.Rotation, centerX, centerY - 10);
					}
					else if (thing.Rotation == 90)
					{
						canv.rotate(-thing.Rotation, centerX, centerY + 45);
					}
					else if (thing.Rotation == 270)
					{
						canv.rotate(-thing.Rotation, centerX - 40, centerY);
					}

					canv.drawText(thing.Name, centerX, centerY, DESK_LABEL_PAINT);
					canv.restore();
				}
			}
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public boolean onTouchEvent(final android.view.MotionEvent event)
		public override bool onTouchEvent(MotionEvent @event)
		{
			bool handled = false;

			OfficeThing touchedThing;
			int xTouchLogical;
			int yTouchLogical;
			int pointerId;
			int actionIndex = @event.ActionIndex;

			// get touch event coordinates and make transparent wrapper from it
			switch (@event.ActionMasked)
			{

				case MotionEvent.ACTION_DOWN:
					// first pointer, clear the pointer
					mSelectedThing = null;

					xTouchLogical = screenToModel((int) @event.getX(0));
					yTouchLogical = screenToModel((int) @event.getY(0));

					// check if we've touched inside something
					touchedThing = getTouchedThing(xTouchLogical, yTouchLogical);

					if (touchedThing == null)
					{
						mSelectedThingChangeListener.thingChanged(null);
						break;
					}

					mSelectedThing = touchedThing;

					if (null != mSelectedThingChangeListener)
					{
						mSelectedThingChangeListener.thingChanged(touchedThing);
					}
					touchedThing.setzIndex(mOfficeLayout.HighestzIndex + 1);

					Log.v(TAG, "Selected " + touchedThing);
					handled = true;
					break;


				case MotionEvent.ACTION_MOVE:

					pointerId = @event.getPointerId(actionIndex);
					if (pointerId > 0)
					{
						break;
					}

					xTouchLogical = screenToModel((int) @event.getX(actionIndex));
					yTouchLogical = screenToModel((int) @event.getY(actionIndex));

					touchedThing = mSelectedThing;

					if (null == touchedThing)
					{
						break;
					}

					moveThing(touchedThing, xTouchLogical, yTouchLogical);

					handled = true;
					break;

				case MotionEvent.ACTION_UP:
					mSelectedThing = null;
					invalidate();
					handled = true;
					break;

				case MotionEvent.ACTION_CANCEL:
					handled = true;
					break;
			}

			return base.onTouchEvent(@event) || handled;
		}

		private void moveThing(OfficeThing touchedThing, int xTouchLogical, int yTouchLogical)
		{
			//TODO: make sure these are accurate
			int newTop = yTouchLogical - mRenderUtil.getModelHeight(touchedThing) / 2;
			int newLeft = xTouchLogical - mRenderUtil.getModelWidth(touchedThing) / 2;
			int newBottom = yTouchLogical + mRenderUtil.getModelHeight(touchedThing) / 2;
			int newRight = xTouchLogical + mRenderUtil.getModelWidth(touchedThing) / 2;

			if (newTop < 0 || newLeft < 0 || newBottom > LOGICAL_HEIGHT || newRight > LOGICAL_WIDTH)
			{
				Log.v(TAG, "Dragging beyond screen edge. Limiting");
			}
			// Limit moves to the boundaries of the screen
			if (newTop < 0)
			{
				newTop = 0;
			}
			if (newLeft < 0)
			{
				newLeft = 0;
			}
			if (newBottom > LOGICAL_HEIGHT)
			{
				newTop = LOGICAL_HEIGHT - mRenderUtil.getModelHeight(touchedThing);
			}
			if (newRight > LOGICAL_WIDTH)
			{
				newLeft = LOGICAL_WIDTH - mRenderUtil.getModelWidth(touchedThing);
			}

			// Save the object
			touchedThing.Top = newTop;
			touchedThing.Left = newLeft;

			// Notify listeners
			if (null != this.mThingChangedListener)
			{
				mThingChangedListener.thingChanged(touchedThing.Key, touchedThing);
			}
		}

		private OfficeThing getTouchedThing(int xTouchModel, int yTouchModel)
		{

			OfficeThing touched = null;

			foreach (OfficeThing thing in mOfficeLayout.ThingsTopDown)
			{
				int top = thing.Top;
				int left = thing.Left;
				int bottom = thing.Top + mRenderUtil.getScreenHeight(thing);
				int right = thing.Left + mRenderUtil.getScreenWidth(thing);

				if (yTouchModel <= bottom && yTouchModel >= top && xTouchModel >= left && xTouchModel <= right)
				{
					touched = thing;
					break;
				}
			}
			return touched;
		}

		//Coordinate conversion
		private int modelToScreen(int coordinate)
		{
			return (int)(coordinate * mScreenRatio);
		}

		private int screenToModel(int coordinate)
		{
			return (int)(coordinate / mScreenRatio);
		}
	}

}