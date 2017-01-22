namespace com.twilio.video.examples.customrenderer
{

	using Bitmap = android.graphics.Bitmap;
	using BitmapFactory = android.graphics.BitmapFactory;
	using Matrix = android.graphics.Matrix;
	using Rect = android.graphics.Rect;
	using YuvImage = android.graphics.YuvImage;
	using Handler = android.os.Handler;
	using Looper = android.os.Looper;
	using ImageView = android.widget.ImageView;



//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static android.graphics.ImageFormat.NV21;

	/// <summary>
	/// SnapshotVideoRenderer demonstrates how to implement a custom <seealso cref="VideoRenderer"/>. Caches the
	/// last frame rendered and will update the provided image view any time <seealso cref="#takeSnapshot()"/> is
	/// invoked.
	/// </summary>
	public class SnapshotVideoRenderer : VideoRenderer
	{
		private readonly ImageView imageView;
		private readonly AtomicBoolean snapshotRequsted = new AtomicBoolean(false);
		private readonly Handler handler = new Handler(Looper.MainLooper);

		public SnapshotVideoRenderer(ImageView imageView)
		{
			this.imageView = imageView;
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void renderFrame(final com.twilio.video.I420Frame i420Frame)
		public override void renderFrame(I420Frame i420Frame)
		{
			// Capture bitmap and post to main thread
			if (snapshotRequsted.compareAndSet(true, false))
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.graphics.Bitmap bitmap = captureBitmap(i420Frame);
				Bitmap bitmap = captureBitmap(i420Frame);
				handler.post(() =>
				{
					// Update the bitmap of image view
					imageView.ImageBitmap = bitmap;

					// Frames must be released after rendering to free the native memory
					i420Frame.release();
				});
			}
			else
			{
				i420Frame.release();
			}
		}

		/// <summary>
		/// Request a snapshot on the rendering thread.
		/// </summary>
		public virtual void takeSnapshot()
		{
			snapshotRequsted.set(true);
		}

		private Bitmap captureBitmap(I420Frame i420Frame)
		{
			YuvImage yuvImage = i420ToYuvImage(i420Frame);
			ByteArrayOutputStream stream = new ByteArrayOutputStream();
			Rect rect = new Rect(0, 0, yuvImage.Width, yuvImage.Height);

			// Compress YuvImage to jpeg
			yuvImage.compressToJpeg(rect, 100, stream);

			// Convert jpeg to Bitmap
			sbyte[] imageBytes = stream.toByteArray();
			Bitmap bitmap = BitmapFactory.decodeByteArray(imageBytes, 0, imageBytes.Length);
			Matrix matrix = new Matrix();

			// Apply any needed rotation
			matrix.postRotate(i420Frame.rotationDegree);
			bitmap = Bitmap.createBitmap(bitmap, 0, 0, bitmap.Width, bitmap.Height, matrix, true);

			return bitmap;
		}

		private YuvImage i420ToYuvImage(I420Frame i420Frame)
		{
			if (i420Frame.yuvStrides[0] != i420Frame.width)
			{
				return fastI420ToYuvImage(i420Frame);
			}
			if (i420Frame.yuvStrides[1] != i420Frame.width / 2)
			{
				return fastI420ToYuvImage(i420Frame);
			}
			if (i420Frame.yuvStrides[2] != i420Frame.width / 2)
			{
				return fastI420ToYuvImage(i420Frame);
			}

			sbyte[] bytes = new sbyte[i420Frame.yuvStrides[0] * i420Frame.height + i420Frame.yuvStrides[1] * i420Frame.height / 2 + i420Frame.yuvStrides[2] * i420Frame.height / 2];
			ByteBuffer tmp = ByteBuffer.wrap(bytes, 0, i420Frame.width * i420Frame.height);
			copyPlane(i420Frame.yuvPlanes[0], tmp);

			sbyte[] tmpBytes = new sbyte[i420Frame.width / 2 * i420Frame.height / 2];
			tmp = ByteBuffer.wrap(tmpBytes, 0, i420Frame.width / 2 * i420Frame.height / 2);

			copyPlane(i420Frame.yuvPlanes[2], tmp);
			for (int row = 0 ; row < i420Frame.height / 2 ; row++)
			{
				for (int col = 0 ; col < i420Frame.width / 2 ; col++)
				{
					bytes[i420Frame.width * i420Frame.height + row * i420Frame.width + col * 2] = tmpBytes[row * i420Frame.width / 2 + col];
				}
			}
			copyPlane(i420Frame.yuvPlanes[1], tmp);
			for (int row = 0 ; row < i420Frame.height / 2 ; row++)
			{
				for (int col = 0 ; col < i420Frame.width / 2 ; col++)
				{
					bytes[i420Frame.width * i420Frame.height + row * i420Frame.width + col * 2 + 1] = tmpBytes[row * i420Frame.width / 2 + col];
				}
			}
			return new YuvImage(bytes, NV21, i420Frame.width, i420Frame.height, null);
		}

		private YuvImage fastI420ToYuvImage(I420Frame i420Frame)
		{
			sbyte[] bytes = new sbyte[i420Frame.width * i420Frame.height * 3 / 2];
			int i = 0;
			for (int row = 0 ; row < i420Frame.height ; row++)
			{
				for (int col = 0 ; col < i420Frame.width ; col++)
				{
					bytes[i++] = i420Frame.yuvPlanes[0].get(col + row * i420Frame.yuvStrides[0]);
				}
			}
			for (int row = 0 ; row < i420Frame.height / 2 ; row++)
			{
				for (int col = 0 ; col < i420Frame.width / 2; col++)
				{
					bytes[i++] = i420Frame.yuvPlanes[2].get(col + row * i420Frame.yuvStrides[2]);
					bytes[i++] = i420Frame.yuvPlanes[1].get(col + row * i420Frame.yuvStrides[1]);
				}
			}
			return new YuvImage(bytes, NV21, i420Frame.width, i420Frame.height, null);
		}

		private void copyPlane(ByteBuffer src, ByteBuffer dst)
		{
			src.position(0).limit(src.capacity());
			dst.put(src);
			dst.position(0).limit(dst.capacity());
		}
	}

}