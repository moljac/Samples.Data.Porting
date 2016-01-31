package com.samsung.android.sdk.professionalaudio.widgets;

import java.util.List;
import java.util.Map;
import java.util.TreeMap;

import android.app.Activity;
import android.content.res.TypedArray;
import android.view.DragEvent;
import android.view.View;
import android.view.View.OnDragListener;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

class CenteredFloatingControlerHandler extends
		AbstractFloatingControllerHandler {

	private int mBarAlignment = ALIGN_PARENT_TOP;

	@SuppressWarnings("unused")
	private final static String TAG = "professionalaudioconnection:widget:j:"
			+ CenteredFloatingControlerHandler.class.getName();

	private Map<Integer, ViewGroup> mPanelMap;

	public static final int ALIGN_PARENT_TOP = 0;
	public static final int ALIGN_PARENT_RIGHT = 1;
	public static final int ALIGN_PARENT_LEFT = 2;
	public static final int ALIGN_PARENT_BOTTOM = 3;

	public CenteredFloatingControlerHandler(Activity activity, TypedArray array) {
		initControlBar(activity, false, null, array);
	}

	@Override
	public int getLayoutResource() {
		return R.layout.jam_control_view;
	}
	
	//in this version there is no main app
	@Override
	public void setSapaAppService(SapaAppService sapaAppService,
	    String mainPackagename) {
		super.setSapaAppService(sapaAppService, mainPackagename);

	}

	@Override
	public int getBarAlignment() {
		return mBarAlignment;
	}

	@Override
	public void loadBarState(FloatingController floatingController, int barAlignment) {
		mBarAlignment = barAlignment;
		putBarOnBoard(mBarAlignment);
	}

	@Override
	public void initLayout(ViewGroup root) {

		FrameLayout layout = (FrameLayout) root.findViewById(R.id.topScroll);
		layout.addView(this.mControlBar);
		mMainView = root;
		mPanelMap = new TreeMap<Integer, ViewGroup>();
		mPanelMap.put(ALIGN_PARENT_BOTTOM,
				(ViewGroup) root.findViewById(R.id.bottomScroll));
		mPanelMap.put(ALIGN_PARENT_TOP,
				(ViewGroup) root.findViewById(R.id.topScroll));
		mPanelMap.put(ALIGN_PARENT_LEFT,
				(ViewGroup) root.findViewById(R.id.leftScroll));
		mPanelMap.put(ALIGN_PARENT_RIGHT,
				(ViewGroup) root.findViewById(R.id.rightScroll));
		View dragReceiver = root.findViewById(R.id.for_content);

		(dragReceiver != null ? dragReceiver : root)
				.setOnDragListener(new MyDropListener());
		mBarAlignment = mPanelMap.containsKey(mBarAlignment) ? mBarAlignment
				: ALIGN_PARENT_TOP;
		putBarOnBoard(mBarAlignment);
	}

	protected void initControlBar(Activity activity, boolean barExpanded,
			Map<String, Boolean> devicesExpanded, TypedArray array) {
		super.initControlBar(activity,  array);
		int handleDrawable = array.getResourceId(
				R.styleable.FloatingControler_handle_drawable, 0);
		mBarAlignment = array.getInt(
				R.styleable.FloatingControler_bar_alignment, ALIGN_PARENT_TOP);
		ControlBar.Info info = new ControlBar.Info(true, null, handleDrawable);
		try {
			this.mControlBar = new ControlBar(activity,  info) {

				@Override
				void addSubViews(List<? extends View> subViews, View view) {
					int index = mDevicesLayout.indexOfChild(view);
					if (index == -1)
						return;
					for (int i = 0; i < subViews.size(); ++i) {
						View v = subViews.get(i);
						if (mDevicesLayout.indexOfChild(v) != -1)
							mDevicesLayout.removeView(v);
						//v.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH, DeviceActionsLayout.BUTTON_HEIGHT));
						mDevicesLayout.addView(v, ++index);
					}
				}

				@Override
				void clearSubViews(int count, View view) {
					int index = mDevicesLayout.indexOfChild(view);
					if (index == -1)
						return;
					mDevicesLayout.removeViews(index + 1, count);
				}

				@Override
				public void onServiceDisconnected() {
				}

			};
			Map<Integer, Integer> map = new TreeMap<Integer, Integer>();
			map.put(LinearLayout.HORIZONTAL, R.drawable.ctrl_action_horiz_end);
			map.put(LinearLayout.VERTICAL, R.drawable.ctrl_action_vert_end);
			mControlBar.setEndImageMap(map);
		} catch (NotActivityException e) {
			e.printStackTrace();
		}
	}

	/**
	 * This method sets bar on an appriopriate border.
	 * 
	 * @param aligment
	 *            This parameter shall be RelativeLayout aligment. It indicates
	 *            on which border floating controler shall be placed initially.
	 *            Its value ma be: RelativeLayout.ALIGN_PARENT_LEFT,
	 *            RelativeLayout.ALIGN_PARENT_RIGHT,
	 *            RelativeLayout.ALIGN_PARENT_TOP,
	 *            RelativeLayout.ALIGN_PARENT_BOTTOM.
	 */
	private void putBarOnBoard(int aligment) {
		ViewGroup layout = mPanelMap.get(aligment);

		if (layout != null) {

			ViewGroup parentLayout = (ViewGroup) this.mControlBar.getParent();
			if (parentLayout != null) {
				parentLayout.removeView(this.mControlBar);
			}
			if (aligment == ALIGN_PARENT_TOP || aligment == ALIGN_PARENT_BOTTOM){
				this.mControlBar.setOrientation(LinearLayout.HORIZONTAL);
			}
			else{
				this.mControlBar.setOrientation(LinearLayout.VERTICAL);
			}
			if (layout != null) {
				layout.addView(this.mControlBar);
			}

		}
	}

	/**
	 * MyDropListener is responsible for moving controlBar between borders of
	 * FloatingControler.
	 * 
	 */
	private final class MyDropListener implements OnDragListener {

		@Override
		public boolean onDrag(View v, DragEvent event) {
			if ((event.getLocalState() instanceof View)
					&& ((View) event.getLocalState()).getId() == v.getContext()
							.hashCode()) {
				if (DragEvent.ACTION_DROP == event.getAction()) {

					float x = event.getX();
					float y = event.getY();

					int aligment = getBorder(x, y);
					putBarOnBoard(aligment);
				} else if (DragEvent.ACTION_DRAG_ENDED == event.getAction()
						&& !event.getResult()) {
					putBarOnBoard(ALIGN_PARENT_TOP);
				}
				return true;
			}
			return false;
		}

	}

	/**
	 * This method checks on which border control bar shall be placed.
	 * 
	 * @param x
	 *            X-coordinate of the place where control bar was released.
	 * @param y
	 *            Y-coordinate of the place where control bar was released.
	 * @return RelativeLayout aligment value of where the basr shall be placed.
	 */
	@Override
	protected int getBorder(float x, float y) {
		float layoutWidth = (float) mMainView.getWidth();
		float layoutHeight = (float) mMainView.getHeight();

		boolean toLeft = x < (layoutWidth / 2);
		boolean toTop = y < (layoutHeight / 2);

		float horizontalDelta = toLeft ? x : layoutWidth - x;
		float verticalDelta = toTop ? y : layoutHeight - y;

		return horizontalDelta < verticalDelta ? (toLeft ? ALIGN_PARENT_LEFT
				: ALIGN_PARENT_RIGHT) : (toTop ? ALIGN_PARENT_TOP
				: ALIGN_PARENT_BOTTOM);
	}
}
