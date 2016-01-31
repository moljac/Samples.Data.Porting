package com.samsung.android.sdk.professionalaudio.widgets;

import java.util.Map;
import java.util.TreeMap;

import android.app.Activity;
import android.content.Context;
import android.content.res.TypedArray;
import android.view.DragEvent;
import android.view.Gravity;
import android.view.View;
import android.view.View.OnDragListener;
import android.view.ViewGroup;
import android.widget.FrameLayout;
import android.widget.FrameLayout.LayoutParams;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

class CornerFloatingControlerHandler extends AbstractFloatingControllerHandler {

    private static final int HORIZONTAL = LinearLayout.HORIZONTAL;

    public static final int ALIGN_PARENT_BOTTOM_RIGHT = 5;
    public static final int ALIGN_PARENT_BOTTOM_LEFT = 6;
    public static final int ALIGN_PARENT_TOP_RIGHT = 4;
    public static final int ALIGN_PARENT_TOP_LEFT = 7;

    private static final int DEFAULT_POSITION = ALIGN_PARENT_BOTTOM_RIGHT;

    private int mInitOrientation;
    private int mOrientation;

    private int mInitialAlign;
    private int mBarAlignment = -1;

    private Map<Integer, FrameLayout> mPanelMap;
    private Map<Integer, Integer> mGravityMap;

    public CornerFloatingControlerHandler(Activity activity, TypedArray array) {
        initControlBar(activity, array);
    }

    @Override
    public int getLayoutResource() {
        return R.layout.corner_jam_control_view;
    }

    @Override
    public void initLayout(ViewGroup root) {
        mMainView = root;
        // layout.addView(this.mControlBar);
        mGravityMap = createGravityMap();
        mPanelMap = createPanelMap(root);

        View dragReceiver = root.findViewById(R.id.for_content);

        OnDragListener dragListener = new MyDropListener();
        if (dragReceiver != null)
            dragReceiver.setOnDragListener(dragListener);
        root.setOnDragListener(dragListener);

        mBarAlignment = mPanelMap.containsKey(mBarAlignment) ? mBarAlignment : DEFAULT_POSITION;
        //putBarOnBoard(mBarAlignment);
    }

    private TreeMap<Integer, Integer> createGravityMap() {
        TreeMap<Integer, Integer> gravityMap = new TreeMap<Integer, Integer>();

        gravityMap.put(ALIGN_PARENT_BOTTOM_LEFT, Gravity.BOTTOM | Gravity.LEFT);
        gravityMap.put(ALIGN_PARENT_BOTTOM_RIGHT, Gravity.BOTTOM | Gravity.RIGHT);
        gravityMap.put(ALIGN_PARENT_TOP_LEFT, Gravity.TOP | Gravity.LEFT);
        gravityMap.put(ALIGN_PARENT_TOP_RIGHT, Gravity.TOP | Gravity.RIGHT);

        return gravityMap;
    }

    private TreeMap<Integer, FrameLayout> createPanelMap(ViewGroup root) {
        TreeMap<Integer, FrameLayout> panelMap = new TreeMap<Integer, FrameLayout>();

        panelMap.put(ALIGN_PARENT_BOTTOM_LEFT,
                (FrameLayout) root.findViewById(R.id.leftBottomScroll));
        panelMap.put(ALIGN_PARENT_BOTTOM_RIGHT,
                (FrameLayout) root.findViewById(R.id.rightBottomScroll));
        panelMap.put(ALIGN_PARENT_TOP_LEFT,
                (FrameLayout) root.findViewById(R.id.leftTopScroll));
        panelMap.put(ALIGN_PARENT_TOP_RIGHT,
                (FrameLayout) root.findViewById(R.id.rightTopScroll));

        return panelMap;
    }

    protected void initControlBar(Activity activity, TypedArray array) {
        super.initControlBar(activity, array);
        mInitOrientation = array.getInt(R.styleable.FloatingControler_orientation, HORIZONTAL);
        int handleDrawable = array.getResourceId(R.styleable.FloatingControler_handle_drawable, 0);
        mBarAlignment = array
                .getInt(R.styleable.FloatingControler_bar_alignment, DEFAULT_POSITION);
        prepareBarState();
        try {
            mControlBar = createControlBar(activity, handleDrawable);
        } catch (NotActivityException e) {
            e.printStackTrace();
        }
    }

    private void prepareBarState() {
        mOrientation = mInitOrientation;
    }

    /**
     * This method sets bar on an appropriate border.
     * 
     * @param alignment
     *            This parameter shall be RelativeLayout alignment. It indicates on which border
     *            floating controller shall be placed initially. Its value ma be:
     *            RelativeLayout.ALIGN_PARENT_LEFT, RelativeLayout.ALIGN_PARENT_RIGHT,
     *            RelativeLayout.ALIGN_PARENT_TOP, RelativeLayout.ALIGN_PARENT_BOTTOM.
     */
    private void putBarOnBoard(int alignment) {
        FrameLayout hLayout = mPanelMap.get(alignment);
        View CurrentChild = null;

        if (hLayout != null)
            CurrentChild = hLayout.getChildAt(0);

        if (this.mBarAlignment == alignment && CurrentChild != null && CurrentChild.getVisibility() == View.VISIBLE)
            return;

        ControlBar visibleBar = null;
        LayoutParams params;

        if (hLayout != null) {
            FrameLayout parentLayout = null;
            int size = mControlBar.getContext().getResources()
                    .getDimensionPixelSize(R.dimen.default_controlbar_thickness);

            mBarAlignment = alignment;
            mOrientation = HORIZONTAL;
            visibleBar = mControlBar;

            params = new LayoutParams(LayoutParams.WRAP_CONTENT, size,
                    mGravityMap.get(alignment));

            if (visibleBar != null) {
                parentLayout = (FrameLayout) visibleBar.getParent();
            }

            if (parentLayout != null) {
                parentLayout.removeView(visibleBar);
            }

            hLayout.addView(visibleBar, params);

            if (visibleBar != null) {
                visibleBar.setCurrentPosition(mBarAlignment);
                visibleBar.setVisibility(View.INVISIBLE);
                visibleBar.getChildAt(0).forceLayout();
            }
        }

        if (visibleBar != null) {
            visibleBar.setCurrentPosition(mBarAlignment);
            visibleBar.setVisibility(View.VISIBLE);
            visibleBar.expand();
        }
    }

    // because it is required to suppress
    @SuppressWarnings("unused")
    private int getAlignment(TypedArray array) {
        if (array == null) return RelativeLayout.ALIGN_PARENT_TOP;
        int alignment = array.getInt(R.styleable.FloatingControler_bar_alignment, 0);
        return alignment;
    }

    /**
     * MyDropListener is responsible for moving controlBar between borders of FloatingControler.
     * 
     */
    private final class MyDropListener implements OnDragListener {

        @Override
        public boolean onDrag(View v, DragEvent event) {
            if ((event.getLocalState() instanceof View)
                    && ((View) event.getLocalState()).getId() == v.getContext().hashCode()) {
                if (DragEvent.ACTION_DROP == event.getAction()) {

                    float x = event.getX();
                    float y = event.getY();

                    int alignment = getBorder(x, y);
                    putBarOnBoard(alignment);
                } else if (DragEvent.ACTION_DRAG_ENDED == event.getAction() && !event.getResult()) {
                    putBarOnBoard(mInitialAlign);
                }
                return true;
            }
            return false;
        }
    }

    private ControlBar createControlBar(Context activity, int handleDrawable)
            throws NotActivityException {

        ControlBar.Info info = new ControlBar.Info(true, null, handleDrawable);
        ControlBar controlBar = new CornerControlBar(activity, info);

        Map<Integer, Integer> map = new TreeMap<Integer, Integer>();
        map.put(LinearLayout.HORIZONTAL, android.R.color.transparent);

        controlBar.setEndImageMap(map);
        controlBar.setOrientation(LinearLayout.HORIZONTAL);
        controlBar.setCurrentPosition(mBarAlignment);

        return controlBar;
    }

    /**
     * This method checks on which border control bar shall be placed.
     * 
     * @param x
     *            X-coordinate of the place where control bar was released.
     * @param y
     *            Y-coordinate of the place where control bar was released.
     * @return RelativeLayout alignment value of where the bar shall be placed.
     */
    @Override
    protected int getBorder(float x, float y) {
        float relativeX = x / ((float) mMainView.getWidth());
        float relativeY = y / ((float) mMainView.getHeight());

        if (relativeX < 0.5)
            return (relativeY < 0.5) ? ALIGN_PARENT_TOP_LEFT : ALIGN_PARENT_BOTTOM_LEFT;
        else
            return (relativeY < 0.5) ? ALIGN_PARENT_TOP_RIGHT : ALIGN_PARENT_BOTTOM_RIGHT;
    }
    
    @Override
    public void stopConnections() {
    	super.stopConnections();
    }

    @Override
    public int getBarAlignment() {
        return mBarAlignment;
    }

    @Override
    public void loadBarState(FloatingController floatingController, int barAlignment) {
        mOrientation = HORIZONTAL;
        mBarAlignment = barAlignment;
        mInitOrientation = mOrientation;
        prepareBarState();
        putBarOnBoard(mOrientation + mBarAlignment);
    }

    @Override
    public void setSapaAppService(SapaAppService sapaAppService,
            String mainPackagename) {

        if (mInitOrientation == HORIZONTAL) {
            putBarOnBoard(mBarAlignment);
            mInitialAlign = mBarAlignment;
        } else {
            putBarOnBoard(mBarAlignment);
        }
        super.setSapaAppService(sapaAppService, mainPackagename);
    }
}
