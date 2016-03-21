using System.Collections.Generic;

namespace com.samsung.android.example.slookdemos
{


	using Activity = android.app.Activity;
	using GLSurfaceView = android.opengl.GLSurfaceView;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using MotionEvent = android.view.MotionEvent;
	using View = android.view.View;
	using Toast = android.widget.Toast;

	using Slook = com.samsung.android.sdk.look.Slook;
	using SlookAirButton = com.samsung.android.sdk.look.airbutton.SlookAirButton;
	using SlookAirButtonAdapter = com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter;
	using AirButtonItem = com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter.AirButtonItem;

	public class AirButtonSurfaceViewActivity : Activity
	{

		private SlookAirButton mAirButton;

		private GLSurfaceView mGlSurfaceView;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			mGlSurfaceView = new GLSurfaceView(this);
			mGlSurfaceView.Renderer = new RendererAnonymousInnerClassHelper(this);
			ContentView = mGlSurfaceView;
			mGlSurfaceView.OnHoverListener = new OnHoverListenerAnonymousInnerClassHelper(this);
			mAirButton = new SlookAirButton(mGlSurfaceView, AdapterMenuList, SlookAirButton.UI_TYPE_MENU);
			mAirButton.Gravity = SlookAirButton.GRAVITY_HOVER_POINT;

			Slook slook = new Slook();
			if (!slook.isFeatureEnabled(Slook.AIRBUTTON))
			{
				Toast.makeText(this, "This model doesn't support AirButton", Toast.LENGTH_LONG).show();
			}
			else
			{
				Toast.makeText(this, "Please hover and push the Spen button on each button", Toast.LENGTH_SHORT).show();
			}
		}

		private class RendererAnonymousInnerClassHelper : GLSurfaceView.Renderer
		{
			private readonly AirButtonSurfaceViewActivity outerInstance;

			public RendererAnonymousInnerClassHelper(AirButtonSurfaceViewActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onSurfaceCreated(GL10 gl, EGLConfig config)
			{
			}

			public virtual void onSurfaceChanged(GL10 gl, int width, int height)
			{
				gl.glViewport(0, 0, width, height);

			}

			public virtual void onDrawFrame(GL10 gl)
			{
				gl.glClearColor((float) 1.0, (float) 0.5, (float) 0.5, (float) 1.0);
				gl.glClear(GL10.GL_COLOR_BUFFER_BIT | GL10.GL_DEPTH_BUFFER_BIT);
			}
		}

		private class OnHoverListenerAnonymousInnerClassHelper : View.OnHoverListener
		{
			private readonly AirButtonSurfaceViewActivity outerInstance;

			public OnHoverListenerAnonymousInnerClassHelper(AirButtonSurfaceViewActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual bool onHover(View v, MotionEvent @event)
			{
				if (@event.Action == MotionEvent.ACTION_HOVER_MOVE)
				{
					if ((@event.ButtonState & MotionEvent.BUTTON_SECONDARY) != 0)
					{
						Log.e("newkkc79","event");
					}
				}
				return false;
			}
		}

		protected internal override void onResume()
		{
			base.onResume();
			mGlSurfaceView.onResume();
		}

		protected internal override void onPause()
		{
			base.onPause();
			mGlSurfaceView.onPause();
		}

		public virtual SlookAirButtonAdapter AdapterMenuList
		{
			get
			{
				List<SlookAirButtonAdapter.AirButtonItem> itemList = new List<SlookAirButtonAdapter.AirButtonItem>();
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_archive), "Help", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_edit), "Edit", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_help), "Help", null));
    
				return new SlookAirButtonAdapter(itemList);
			}
		}

	}

}