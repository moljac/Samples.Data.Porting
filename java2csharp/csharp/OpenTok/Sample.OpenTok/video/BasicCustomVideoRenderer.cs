using System;

namespace com.opentok.android.demo.video
{



	using Context = android.content.Context;
	using Bitmap = android.graphics.Bitmap;
	using GLES20 = android.opengl.GLES20;
	using GLSurfaceView = android.opengl.GLSurfaceView;
	using Matrix = android.opengl.Matrix;
	using Environment = android.os.Environment;
	using Log = android.util.Log;
	using View = android.view.View;

	public class BasicCustomVideoRenderer : BaseVideoRenderer
	{

		private const string LOG_TAG = "custom-videorenderer";

		internal Context mContext;

		internal GLSurfaceView mView;

		internal MyRenderer mRenderer;

		internal bool mSaveScreenshot;

		internal class MyRenderer : GLSurfaceView.Renderer
		{

			internal int[] mTextureIds = new int[3];
			internal float[] mScaleMatrix = new float[16];

			internal FloatBuffer mVertexBuffer;
			internal FloatBuffer mTextureBuffer;
			internal ShortBuffer mDrawListBuffer;

			internal bool mVideoFitEnabled = true;
			internal bool mVideoDisabled = false;

			// number of coordinates per vertex in this array
			internal const int COORDS_PER_VERTEX = 3;
			internal const int TEXTURECOORDS_PER_VERTEX = 2;

			internal static float[] mXYZCoords = new float[] {-1.0f, 1.0f, 0.0f, -1.0f, -1.0f, 0.0f, 1.0f, -1.0f, 0.0f, 1.0f, 1.0f, 0.0f}; // top left

			internal static float[] mUVCoords = new float[] {0, 0, 0, 1, 1, 1, 1, 0}; // top left

			internal short[] mVertexIndex = new short[] {0, 1, 2, 0, 2, 3}; // order to draw
			// vertices

			internal readonly string vertexShaderCode = "uniform mat4 uMVPMatrix;" + "attribute vec4 aPosition;\n" + "attribute vec2 aTextureCoord;\n" + "varying vec2 vTextureCoord;\n" + "void main() {\n" + "  gl_Position = uMVPMatrix * aPosition;\n" + "  vTextureCoord = aTextureCoord;\n" + "}\n";

			internal readonly string fragmentShaderCode = "precision mediump float;\n" + "uniform sampler2D Ytex;\n" + "uniform sampler2D Utex,Vtex;\n" + "varying vec2 vTextureCoord;\n" + "void main(void) {\n" + "  float nx,ny,r,g,b,y,u,v;\n" + "  mediump vec4 txl,ux,vx;" + "  nx=vTextureCoord[0];\n" + "  ny=vTextureCoord[1];\n" + "  y=texture2D(Ytex,vec2(nx,ny)).r;\n" + "  u=texture2D(Utex,vec2(nx,ny)).r;\n" + "  v=texture2D(Vtex,vec2(nx,ny)).r;\n" + "  y=1.1643*(y-0.0625);\n" + "  u=u-0.5;\n" + "  v=v-0.5;\n" + "  r=y+1.5958*v;\n" + "  g=y-0.39173*u-0.81290*v;\n" + "  b=y+2.017*u;\n" + "  gl_FragColor=vec4(r,g,b,1.0);\n" + "}\n";
					//+ "  y=1.0-1.1643*(y-0.0625);\n" // Invert effect

			internal ReentrantLock mFrameLock = new ReentrantLock();
			internal Frame mCurrentFrame;

			internal int mProgram;
			internal int mTextureWidth;
			internal int mTextureHeight;
			internal int mViewportWidth;
			internal int mViewportHeight;
			internal BasicCustomVideoRenderer mCustomVideoRenderer;

			public MyRenderer(BasicCustomVideoRenderer parent)
			{

				this.mCustomVideoRenderer = parent;

				ByteBuffer bb = ByteBuffer.allocateDirect(mXYZCoords.Length * 4);
				bb.order(ByteOrder.nativeOrder());
				mVertexBuffer = bb.asFloatBuffer();
				mVertexBuffer.put(mXYZCoords);
				mVertexBuffer.position(0);

				ByteBuffer tb = ByteBuffer.allocateDirect(mUVCoords.Length * 4);
				tb.order(ByteOrder.nativeOrder());
				mTextureBuffer = tb.asFloatBuffer();
				mTextureBuffer.put(mUVCoords);
				mTextureBuffer.position(0);

				ByteBuffer dlb = ByteBuffer.allocateDirect(mVertexIndex.Length * 2);
				dlb.order(ByteOrder.nativeOrder());
				mDrawListBuffer = dlb.asShortBuffer();
				mDrawListBuffer.put(mVertexIndex);
				mDrawListBuffer.position(0);
			}

			public override void onSurfaceCreated(GL10 gl, EGLConfig config)
			{
				GLES20.glClearColor(0.0f, 0.0f, 0.0f, 1.0f);

				int vertexShader = loadShader(GLES20.GL_VERTEX_SHADER, vertexShaderCode);
				int fragmentShader = loadShader(GLES20.GL_FRAGMENT_SHADER, fragmentShaderCode);

				mProgram = GLES20.glCreateProgram(); // create empty OpenGL ES
				// Program
				GLES20.glAttachShader(mProgram, vertexShader); // add the vertex
				// shader to program
				GLES20.glAttachShader(mProgram, fragmentShader); // add the fragment
				// shader to
				// program
				GLES20.glLinkProgram(mProgram);

				int positionHandle = GLES20.glGetAttribLocation(mProgram, "aPosition");
				int textureHandle = GLES20.glGetAttribLocation(mProgram, "aTextureCoord");

				GLES20.glVertexAttribPointer(positionHandle, COORDS_PER_VERTEX, GLES20.GL_FLOAT, false, COORDS_PER_VERTEX * 4, mVertexBuffer);

				GLES20.glEnableVertexAttribArray(positionHandle);

				GLES20.glVertexAttribPointer(textureHandle, TEXTURECOORDS_PER_VERTEX, GLES20.GL_FLOAT, false, TEXTURECOORDS_PER_VERTEX * 4, mTextureBuffer);

				GLES20.glEnableVertexAttribArray(textureHandle);

				GLES20.glUseProgram(mProgram);
				int i = GLES20.glGetUniformLocation(mProgram, "Ytex");
				GLES20.glUniform1i(i, 0); // Bind Ytex to texture unit 0

				i = GLES20.glGetUniformLocation(mProgram, "Utex");
				GLES20.glUniform1i(i, 1); // Bind Utex to texture unit 1

				i = GLES20.glGetUniformLocation(mProgram, "Vtex");
				GLES20.glUniform1i(i, 2); // Bind Vtex to texture unit 2

				mTextureWidth = 0;
				mTextureHeight = 0;
			}

			internal static void initializeTexture(int name, int id, int width, int height)
			{
				GLES20.glActiveTexture(name);
				GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, id);
				GLES20.glTexParameterf(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MIN_FILTER, GLES20.GL_NEAREST);
				GLES20.glTexParameterf(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_MAG_FILTER, GLES20.GL_LINEAR);
				GLES20.glTexParameterf(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_WRAP_S, GLES20.GL_CLAMP_TO_EDGE);
				GLES20.glTexParameterf(GLES20.GL_TEXTURE_2D, GLES20.GL_TEXTURE_WRAP_T, GLES20.GL_CLAMP_TO_EDGE);
				GLES20.glTexImage2D(GLES20.GL_TEXTURE_2D, 0, GLES20.GL_LUMINANCE, width, height, 0, GLES20.GL_LUMINANCE, GLES20.GL_UNSIGNED_BYTE, null);
			}

			internal virtual void setupTextures(Frame frame)
			{
				if (mTextureIds[0] != 0)
				{
					GLES20.glDeleteTextures(3, mTextureIds, 0);
				}
				GLES20.glGenTextures(3, mTextureIds, 0);

				int w = frame.Width;
				int h = frame.Height;
				int hw = (w + 1) >> 1;
				int hh = (h + 1) >> 1;

				initializeTexture(GLES20.GL_TEXTURE0, mTextureIds[0], w, h);
				initializeTexture(GLES20.GL_TEXTURE1, mTextureIds[1], hw, hh);
				initializeTexture(GLES20.GL_TEXTURE2, mTextureIds[2], hw, hh);

				mTextureWidth = frame.Width;
				mTextureHeight = frame.Height;
			}

			internal virtual void updateTextures(Frame frame)
			{
				int width = frame.Width;
				int height = frame.Height;
				int half_width = (width + 1) >> 1;
				int half_height = (height + 1) >> 1;
				int y_size = width * height;
				int uv_size = half_width * half_height;

				ByteBuffer bb = frame.Buffer;
				// If we are reusing this frame, make sure we reset position and
				// limit
				bb.clear();

				if (bb.remaining() == y_size + uv_size * 2)
				{
					bb.position(0);
					GLES20.glActiveTexture(GLES20.GL_TEXTURE0);
					GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextureIds[0]);
					GLES20.glTexSubImage2D(GLES20.GL_TEXTURE_2D, 0, 0, 0, width, height, GLES20.GL_LUMINANCE, GLES20.GL_UNSIGNED_BYTE, bb);

					bb.position(y_size);
					GLES20.glActiveTexture(GLES20.GL_TEXTURE1);
					GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextureIds[1]);
					GLES20.glTexSubImage2D(GLES20.GL_TEXTURE_2D, 0, 0, 0, half_width, half_height, GLES20.GL_LUMINANCE, GLES20.GL_UNSIGNED_BYTE, bb);

					bb.position(y_size + uv_size);
					GLES20.glActiveTexture(GLES20.GL_TEXTURE2);
					GLES20.glBindTexture(GLES20.GL_TEXTURE_2D, mTextureIds[2]);
					GLES20.glTexSubImage2D(GLES20.GL_TEXTURE_2D, 0, 0, 0, half_width, half_height, GLES20.GL_LUMINANCE, GLES20.GL_UNSIGNED_BYTE, bb);
				}
				else
				{
					mTextureWidth = 0;
					mTextureHeight = 0;
				}

			}

			public override void onSurfaceChanged(GL10 gl, int width, int height)
			{
				GLES20.glViewport(0, 0, width, height);
				mViewportWidth = width;
				mViewportHeight = height;
			}

			public override void onDrawFrame(GL10 gl)
			{
				GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT);

				mFrameLock.@lock();
				if (mCurrentFrame != null && !mVideoDisabled)
				{
					GLES20.glUseProgram(mProgram);

					if (mTextureWidth != mCurrentFrame.Width || mTextureHeight != mCurrentFrame.Height)
					{
						setupTextures(mCurrentFrame);
					}
					updateTextures(mCurrentFrame);

					Matrix.setIdentityM(mScaleMatrix, 0);
					float scaleX = 1.0f, scaleY = 1.0f;
					float ratio = (float) mCurrentFrame.Width / mCurrentFrame.Height;
					float vratio = (float) mViewportWidth / mViewportHeight;

					if (mVideoFitEnabled)
					{
						if (ratio > vratio)
						{
							scaleY = vratio / ratio;
						}
						else
						{
							scaleX = ratio / vratio;
						}
					}
					else
					{
						if (ratio < vratio)
						{
							scaleY = vratio / ratio;
						}
						else
						{
							scaleX = ratio / vratio;
						}
					}

					Matrix.scaleM(mScaleMatrix, 0, scaleX * (mCurrentFrame.MirroredX ? - 1.0f : 1.0f), scaleY, 1);

					int mMVPMatrixHandle = GLES20.glGetUniformLocation(mProgram, "uMVPMatrix");
					GLES20.glUniformMatrix4fv(mMVPMatrixHandle, 1, false, mScaleMatrix, 0);

					GLES20.glDrawElements(GLES20.GL_TRIANGLES, mVertexIndex.Length, GLES20.GL_UNSIGNED_SHORT, mDrawListBuffer);
				}
				mFrameLock.unlock();

			}

			public virtual void displayFrame(Frame frame)
			{
				mFrameLock.@lock();
				if (this.mCurrentFrame != null)
				{
					this.mCurrentFrame.recycle();
				}
				this.mCurrentFrame = frame;
				mFrameLock.unlock();

				if (mCustomVideoRenderer.mSaveScreenshot)
				{
					Log.d(LOG_TAG, "Screenshot capture");

					ByteBuffer bb = frame.Buffer;
					bb.clear();

					int width = frame.Width;
					int height = frame.Height;
					int half_width = (width + 1) >> 1;
					int half_height = (height + 1) >> 1;
					int y_size = width * height;
					int uv_size = half_width * half_height;

					sbyte[] yuv = new sbyte[y_size + uv_size * 2];
					bb.get(yuv);
					int[] intArray = new int[width * height];

					// Decode Yuv data to integer array
					decodeYUV420(intArray, yuv, width, height);

					// Initialize the bitmap, with the replaced color
					Bitmap bmp = Bitmap.createBitmap(intArray, width, height, Bitmap.Config.ARGB_8888);

					try
					{
						string path = Environment.ExternalStorageDirectory.ToString();
						System.IO.Stream fOutputStream = null;
						File file = new File(path, "opentok-capture-" + DateTimeHelperClass.CurrentUnixTimeMillis() + ".png");
						fOutputStream = new System.IO.FileStream(file, System.IO.FileMode.Create, System.IO.FileAccess.Write);

						bmp.compress(Bitmap.CompressFormat.PNG, 100, fOutputStream);

						fOutputStream.Flush();
						fOutputStream.Close();

					}
					catch (Exception e)
					{
						Console.WriteLine(e.ToString());
						Console.Write(e.StackTrace);
						return;
					}

					mCustomVideoRenderer.mSaveScreenshot = false;
				}
			}


			public static void decodeYUV420(int[] rgba, sbyte[] yuv420, int width, int height)
			{
				int half_width = (width + 1) >> 1;
				int half_height = (height + 1) >> 1;
				int y_size = width * height;
				int uv_size = half_width * half_height;

				for (int j = 0; j < height; j++)
				{
					for (int i = 0; i < width; i++)
					{

						double y = (yuv420[j * width + i]) & 0xff;
						double v = (yuv420[y_size + (j >> 1) * half_width + (i >> 1)]) & 0xff;
						double u = (yuv420[y_size + uv_size + (j >> 1) * half_width + (i >> 1)]) & 0xff;

						double r;
						double g;
						double b;

						r = y + 1.402 * (u - 128);
						g = y - 0.34414 * (v - 128) - 0.71414 * (u - 128);
						b = y + 1.772 * (v - 128);

						if (r < 0)
						{
							r = 0;
						}
						else if (r > 255)
						{
							r = 255;
						}
						if (g < 0)
						{
							g = 0;
						}
						else if (g > 255)
						{
							g = 255;
						}
						if (b < 0)
						{
							b = 0;
						}
						else if (b > 255)
						{
							b = 255;
						}

						int ir = (int)r;
						int ig = (int)g;
						int ib = (int)b;
						rgba[j * width + i] = unchecked((int)0xff000000) | (ir << 16) | (ig << 8) | ib;
					}
				}
			}


			public static int loadShader(int type, string shaderCode)
			{
				int shader = GLES20.glCreateShader(type);

				GLES20.glShaderSource(shader, shaderCode);
				GLES20.glCompileShader(shader);

				return shader;
			}

			public virtual void disableVideo(bool b)
			{
				mFrameLock.@lock();

				mVideoDisabled = b;

				if (mVideoDisabled)
				{
					if (this.mCurrentFrame != null)
					{
						this.mCurrentFrame.recycle();
					}
					this.mCurrentFrame = null;
				}

				mFrameLock.unlock();
			}

			public virtual void enableVideoFit(bool enableVideoFit)
			{
				mVideoFitEnabled = enableVideoFit;
			}
		}

		public BasicCustomVideoRenderer(Context context)
		{
			this.mContext = context;

			mView = new GLSurfaceView(context);
			mView.EGLContextClientVersion = 2;

			mRenderer = new MyRenderer(this);
			mView.Renderer = mRenderer;

			mView.RenderMode = GLSurfaceView.RENDERMODE_WHEN_DIRTY;
		}

		public override void onFrame(Frame frame)
		{
			mRenderer.displayFrame(frame);
			mView.requestRender();
		}

		public override void setStyle(string key, string value)
		{
			if (BaseVideoRenderer.STYLE_VIDEO_SCALE.Equals(key))
			{
				if (BaseVideoRenderer.STYLE_VIDEO_FIT.Equals(value))
				{
					mRenderer.enableVideoFit(true);
				}
				else if (BaseVideoRenderer.STYLE_VIDEO_FILL.Equals(value))
				{
					mRenderer.enableVideoFit(false);
				}
			}
		}

		public override void onVideoPropertiesChanged(bool videoEnabled)
		{
			mRenderer.disableVideo(!videoEnabled);
		}

		public override View View
		{
			get
			{
				return mView;
			}
		}

		public override void onPause()
		{
			mView.onPause();
		}

		public override void onResume()
		{
			mView.onResume();
		}

		public virtual void saveScreenshot(bool? enableScreenshot)
		{
			mSaveScreenshot = enableScreenshot.Value;
		}

	}


}