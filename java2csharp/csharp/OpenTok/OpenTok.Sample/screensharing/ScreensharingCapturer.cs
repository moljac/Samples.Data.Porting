namespace com.opentok.android.demo.screensharing
{

	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using Canvas = android.graphics.Canvas;
	using Handler = android.os.Handler;
	using View = android.view.View;

	public class ScreensharingCapturer : BaseVideoCapturer
	{

	  private Context mContext;

		private bool capturing = false;
		private View contentView;

		private int fps = 15;
		private int width = 0;
		private int height = 0;
		private int[] frame;

		private Bitmap bmp;
		private Canvas canvas;

		private Handler mHandler = new Handler();

		private Runnable newFrame = new RunnableAnonymousInnerClassHelper();

		private class RunnableAnonymousInnerClassHelper : Runnable
		{
			public RunnableAnonymousInnerClassHelper()
			{
			}

			public virtual void run()
			{
				if (outerInstance.capturing)
				{
					int width = outerInstance.contentView.Width;
					int height = outerInstance.contentView.Height;

					if (outerInstance.frame == null || outerInstance.width != width || outerInstance.height != height)
					{

						outerInstance.width = width;
						outerInstance.height = height;

						if (outerInstance.bmp != null)
						{
							outerInstance.bmp.recycle();
							outerInstance.bmp = null;
						}

						outerInstance.bmp = Bitmap.createBitmap(width, height, Bitmap.Config.ARGB_8888);

						outerInstance.canvas = new Canvas(outerInstance.bmp);
						outerInstance.frame = new int[width * height];
					}
					outerInstance.canvas.save(Canvas.MATRIX_SAVE_FLAG);
					outerInstance.canvas.translate(-outerInstance.contentView.ScrollX, - outerInstance.contentView.ScrollY);
					outerInstance.contentView.draw(outerInstance.canvas);

					outerInstance.bmp.getPixels(outerInstance.frame, 0, width, 0, 0, width, height);

					provideIntArrayFrame(outerInstance.frame, ARGB, width, height, 0, false);

					outerInstance.canvas.restore();

					outerInstance.mHandler.postDelayed(newFrame, 1000 / outerInstance.fps);

				}
			}
		}

		public ScreensharingCapturer(Context context, View view)
		{
			this.mContext = context;
			this.contentView = view;
		}

		public override void init()
		{

		}

		public override int startCapture()
		{
			capturing = true;

			mHandler.postDelayed(newFrame, 1000 / fps);
			return 0;
		}

		public override int stopCapture()
		{
			capturing = false;
			mHandler.removeCallbacks(newFrame);
			return 0;
		}

		public override bool CaptureStarted
		{
			get
			{
				return capturing;
			}
		}

		public override CaptureSettings CaptureSettings
		{
			get
			{
    
				CaptureSettings settings = new CaptureSettings();
				settings.fps = fps;
				settings.width = width;
				settings.height = height;
				settings.format = ARGB;
				return settings;
			}
		}

		public override void destroy()
		{

		}

		public override void onPause()
		{

		}

		public override void onResume()
		{

		}

	}
}