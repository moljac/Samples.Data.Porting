package com.samsung.android.sdk.professionalaudio.widgets;

import android.animation.AnimatorSet;
import android.animation.LayoutTransition;
import android.animation.ObjectAnimator;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.ClipData;
import android.content.Context;
import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.AttributeSet;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.HorizontalScrollView;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaAppStateListener;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.app.SapaServiceConnectListener;

import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.SortedMap;
import java.util.TreeMap;

/**
 * 
 * This class represents the bar that exposes actions of all active devices on jam connection
 * service. It shall be used only as a part of JamControl.
 */
@SuppressLint("ViewConstructor")
abstract class ControlBar extends LinearLayout implements SapaServiceConnectListener,
        SapaAppStateListener {

    protected static final int ANIM_TIME = 100;

    private final static String TAG = "professionalaudioconnection:widget:j:ControlBar";
    private static final long HIDE_ANIM_DURATION = 300;
    private static final long SHOW_ANIM_DURATION = 300;

    protected Info mInfo;
    protected SortedMap<String, DeviceActionsLayout> mActionsLayouts;
    protected Map<String, DeviceActionsLayoutData> mDevicesActions;
    protected HorizontalScrollView mHorizontalView;
    protected LinearLayout mDevicesLayout;
    private ImageButton mBarHandler;
    private Object mLock = new Object();

    private ImageView mEnd;
    private Map<Integer, Integer> mEndImageRes;

    // this set of variables are to support FloatingController focus changes
    private boolean mExpandCalled;
    private int mScrollTargetX;
    private int mCurrentPosition;
    private int mPreviousPosition;

    private DeviceActionsLayout mExpandedLayout;

    protected SapaAppService mSapaAppService;
    protected DeviceActionsLayout mMainDeviceLayout;
    protected boolean mRemoveMain=false;
    private List<String> mOrderingList;

    /**
     * This method changes orientation of the bar.
     *
     * @param orientation
     *            LinearLayout orientation to be set.
     */
    @Override
    public synchronized void setOrientation(int orientation) {

        super.setOrientation(orientation);
        this.mDevicesLayout.setOrientation(orientation);

        //if (mEndImageRes != null && mEndImageRes.containsKey(getOrientation()))
         //   this.mEnd.setImageResource(mEndImageRes.get(orientation));
        updateDrawables();
        if (mMainDeviceLayout != null) mMainDeviceLayout.setOrientation(orientation);
        //if (orientation == LinearLayout.HORIZONTAL) mEnd.setLayoutParams(new LayoutParams(
        //        LayoutParams.WRAP_CONTENT, DeviceActionsLayout.BUTTON_HEIGHT));
        //else mEnd.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_HEIGHT,
         //       LayoutParams.WRAP_CONTENT));
        for (DeviceActionsLayout layout : this.mActionsLayouts.values()) {
            layout.setOrientation(orientation);
        }
    }

    /**
     * This method exposes information about which devices shall have their actions shown.
     * 
     * @return Map of device uid with information if it's bar shall be expanded.
     */
    public synchronized Map<String, Boolean> getDevicesExpanded() {
        Map<String, Boolean> devsExpanded = new HashMap<String, Boolean>();
        for (DeviceActionsLayout layout : this.mActionsLayouts.values()) {
            devsExpanded.put(layout.getSapaApp().getInstanceId(), layout.isExpanded());
        }
        return devsExpanded;
    }

    /**
     * This method exposes information whether whole bar is expanded or not.
     * 
     * @return true if bar is expanded, false otherwise.
     */
    public boolean getBarExpanded() {
        return mInfo.mListHidden;
    }

    /**
     * This ctor is used mostly when ControlBar is inflated from xml layout
     * 
     * @param context
     *            The context for which the view is being inflated. If it is not an Activity the
     *            exception may be thrown.
     * @param attrs
     *            Attributes given in xml declaration of ControlBar
     * @throws NotActivityException
     *             Exception thrown if the context of this view is not an activity
     */
    public ControlBar(Context context, AttributeSet attrs) throws NotActivityException {
        super(context, attrs);
        init(context, new Info(), R.layout.controlbar);
    }

    /**
     * This ctor starts control bar in state (which parts are extended which not) given by
     * parameters.
     * @param activity 
     *            Context which MUST be an instance of the activity in which the ControlBar will be visible
     * @param info
     *            ControlBar.Info object with some settings
     * @param layoutResource
     *            A resource defining layout for this ControlBar
     * @throws NotActivityException 
     *            This exception is throw if the Context is not an Activity
     */
    protected ControlBar(Context activity, Info info, int layoutResource) throws NotActivityException {
        super(activity);
        init(activity, info, layoutResource);
    }
    
    ControlBar(Context activity, Info info) throws NotActivityException {
        super(activity);
        init(activity, info, R.layout.controlbar);
    }

    private synchronized void init(Context activity, Info info, int res) throws NotActivityException {
        if (!(activity instanceof Activity)) throw new NotActivityException();
        LayoutInflater.from(getContext()).inflate(res, this, true);
        setLayoutDirection(LAYOUT_DIRECTION_INHERIT);
        mInfo = info;
        this.mActionsLayouts = new TreeMap<String, DeviceActionsLayout>();
        this.setControlBar();

        this.setDevicesLayout();

        initHorizontalScrolling();

        this.mBarHandler.setOnLongClickListener(new MyLongTounchListener());

        this.mBarHandler.setId(getContext().hashCode());

        this.mEnd = (ImageView) findViewById(R.id.end);
        //if (mEndImageRes != null && mEndImageRes.containsKey(getOrientation())) this.mEnd
          //      .setImageResource(mEndImageRes.get(getOrientation()));
       // else this.mEnd.setImageResource(android.R.color.transparent);
        if (mInfo.mListHidden || this.mActionsLayouts == null || this.mActionsLayouts.size() == 0) {
            this.mEnd.setVisibility(LinearLayout.GONE);
        } else {
            this.mEnd.setVisibility(LinearLayout.VISIBLE);
        }
        //if (getOrientation() == LinearLayout.HORIZONTAL) mEnd.setLayoutParams(new LayoutParams(
         //       LayoutParams.WRAP_CONTENT, DeviceActionsLayout.BUTTON_HEIGHT));
        //else mEnd.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_HEIGHT,
         //       LayoutParams.WRAP_CONTENT));

        this.mSapaAppService = null;
        this.setOrientation(getOrientation());

    }

    private void initHorizontalScrolling() {
        mHorizontalView = (HorizontalScrollView) findViewById(R.id.main_scroll_view);

        mPreviousPosition = mCurrentPosition;

        mHorizontalView.addOnLayoutChangeListener(new OnLayoutChangeListener() {
            @Override
            public void onLayoutChange(View view, int left, int top, int right, int bottom,
                                       int oldLeft, int oldTop, int oldRight, int oldBottom) {
                if (mExpandCalled) {
                    if (mPreviousPosition != mCurrentPosition) {
                        if (mCurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_BOTTOM_LEFT ||
                                mCurrentPosition == CornerFloatingControlerHandler.ALIGN_PARENT_TOP_LEFT) {
                            mHorizontalView.fullScroll(HorizontalScrollView.FOCUS_RIGHT);
                        } else {
                            mHorizontalView.fullScroll(HorizontalScrollView.FOCUS_LEFT);
                        }
                    } else {
                        mHorizontalView.scrollTo(mScrollTargetX, 0);
                    }
                    mPreviousPosition = mCurrentPosition;
                    mExpandCalled = false;
                }
            }
        });
    }

    public void setCurrentPosition(int barPosition) {
        mCurrentPosition = barPosition;
    }

    public int getCurrentPosition() {
        return mCurrentPosition;
    }

    public void setExpandCalled(boolean mExpandCalled) {
        this.mExpandCalled = mExpandCalled;
    }

    public void setScrollTargetX(int mScrollTargetX) {
        this.mScrollTargetX = mScrollTargetX;
    }

    public void setExpandedLayout(DeviceActionsLayout expandedLayout) {
        mExpandedLayout = expandedLayout;
    }

    public DeviceActionsLayout getExpandedLayout() {
        return mExpandedLayout;
    }

    public int getEndItemWidth() {
        return mEnd.getWidth();
    }

    void setSapaAppService(SapaAppService sapaAppService, String mainAppPackageName) {
    	Log.d(TAG, "setSapaservice "+sapaAppService);
    	SapaAppService previous = this.mSapaAppService;
    	this.mSapaAppService = sapaAppService;
        if (sapaAppService != null) {
            mInfo.mMainApplicationPackage = mainAppPackageName;
            this.mSapaAppService.addConnectionListener(this);
            this.mSapaAppService.addAppStateListener(this);
            if(getContext() != null){
                try{
                ((Activity) getContext()).runOnUiThread(new Runnable() {
                    public void run() {
                        synchronized (ControlBar.this) {
                            ControlBar.this.initializeActions();
                            ControlBar.this.updateBar();
                            ControlBar.this.fillBar();
                        }
                    }
                });
                }catch(NullPointerException e){
                    ;
                }
            };
        }else if(previous!=null){
        	previous.removeConnectionListener(this);
        	previous.removeAppStateListener(this);
        }
    }
    
    void updateSapaAppService(SapaAppService sapaAppService){
    	setSapaAppService(sapaAppService, mInfo.mMainApplicationPackage);
    }
    
    SapaAppService getSapaAppService(){
    	return this.mSapaAppService;
    }

    void setEndImageMap(Map<Integer, Integer> map) {
        this.mEndImageRes = map;
    }

    void removeListeners() {

    	if(mSapaAppService==null) return;
        this.mSapaAppService.removeConnectionListener(this);
        this.mSapaAppService.removeAppStateListener(this);
    }

    private void setDevicesLayout() {
        mDevicesLayout = (LinearLayout) findViewById(R.id.main_panel);

        LayoutTransition transition = new LayoutTransition();
        transition.setAnimateParentHierarchy(false);
        transition.enableTransitionType(LayoutTransition.DISAPPEARING);
        {
            // Fade out
            transition.setAnimator(LayoutTransition.DISAPPEARING,
                    ObjectAnimator.ofFloat(null, View.ALPHA, 1, 0));

            transition.setStartDelay(LayoutTransition.DISAPPEARING, 0);
            transition.setDuration(LayoutTransition.DISAPPEARING, HIDE_ANIM_DURATION / 2);

            transition.setStartDelay(LayoutTransition.CHANGE_DISAPPEARING, HIDE_ANIM_DURATION / 2);
            transition.setDuration(LayoutTransition.CHANGE_DISAPPEARING, HIDE_ANIM_DURATION / 2);
        }

        transition.enableTransitionType(LayoutTransition.APPEARING);
        {
            // Fade in
            transition.setAnimator(LayoutTransition.APPEARING,
                    ObjectAnimator.ofFloat(null, View.ALPHA, 0, 1));

            transition.setStartDelay(LayoutTransition.CHANGE_APPEARING, 0);
            transition.setDuration(LayoutTransition.CHANGE_APPEARING, SHOW_ANIM_DURATION / 2);

            transition.setStartDelay(LayoutTransition.APPEARING, SHOW_ANIM_DURATION / 2);
            transition.setDuration(LayoutTransition.APPEARING, SHOW_ANIM_DURATION / 2);
        }

        mDevicesLayout.setLayoutTransition(transition);
    }

    @Override
    protected void onLayout(boolean changed, int l, int t, int r, int b) {
        super.onLayout(changed, l, t, r, b);
        updateDrawables();
    }

    /**
     * Class responsible for responding on long click.
     */
    private final class MyLongTounchListener implements OnLongClickListener {

        @Override
        public boolean onLongClick(View view) {
            ClipData data = ClipData.newPlainText("", "");
            DragShadowBuilder shadowBuilder = new View.DragShadowBuilder(mMainDeviceLayout!=null ? mMainDeviceLayout : mBarHandler);
            view.startDrag(data, shadowBuilder, mBarHandler, 0);
            return true;
        }
    }

    /**
     * This method fills control bar and sets its behaviour.
     */
    private void setControlBar() {
        this.setBarHandler();
        // this.addView(this.mBarHandler,0);
    }

    /*
     * this method sets levels of drawables depending on orientation as follows 0 horizontal left to
     * right 1 horizontal right to left 2 vertical left to right 3 vertical right to left (used as
     * bottom to top)
     * 
     * This solution was used as temporary one
     */
    private void updateDrawables() {
        this.mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
        Drawable bg = mDevicesLayout.getBackground();
        if (bg != null) bg.setLevel(getOrientation() * 2 + getLayoutDirection());
        /*for(int i=0; i<mDevicesLayout.getChildCount(); ++i){
            View v = mDevicesLayout.getChildAt(i);
            if(v.getTag()!=null && v.getTag() instanceof DeviceActionsLayoutData) break;
            v.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
        }*/
    }

    /*
     * This method sets bar handler.
     */
    private void setBarHandler() {
        this.mBarHandler = (ImageButton) findViewById(R.id.barhandler);

        this.mBarHandler.setBackgroundResource(mInfo.mHandleDrawableRes);
        // TODO remove formula
        this.mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
        this.mBarHandler.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (!mInfo.mListHidden) {
                    ControlBar.this.hide();
                } else {
                    expand();
                }
                mInfo.mListHidden = !mInfo.mListHidden;
            }
        });
    }
    
    public void expand(){
        mInfo.restoreState();
        refresh();
    }
    
    public void hide() {
        if(mInfo.mListHidden) return;
        mInfo.saveState();
        ControlBar.this.mEnd.setVisibility(LinearLayout.GONE);
        ControlBar.this.mBarHandler.setActivated(false);
        if (ControlBar.this.mMainDeviceLayout != null)
            ControlBar.this.mMainDeviceLayout.collapse();
        if (this.mDevicesLayout != null) for (DeviceActionsLayout dev : mActionsLayouts.values()) {
            dev.collapse();
            this.mDevicesLayout.removeView(dev);
        }
        //mDevicesLayout.requestLayout();
    }

    private void show() {
        if (ControlBar.this.mMainDeviceLayout != null
                || (ControlBar.this.mDevicesActions != null && ControlBar.this.mDevicesActions
                        .size() > 0)) {
            // ControlBar.this.mBarHandler.setBackgroundResource(mHandleDrawableRes);
            ControlBar.this.mEnd.setVisibility(LinearLayout.VISIBLE);
            ControlBar.this.mBarHandler.setActivated(true);
        } else {
            mInfo.mListHidden = true;
            return;
        }
        if (ControlBar.this.mMainDeviceLayout != null) ControlBar.this.mMainDeviceLayout.expand();
        if (ControlBar.this.mDevicesActions != null) {

            for (String instanceId : mOrderingList) {
                DeviceActionsLayoutData info = mDevicesActions.get(instanceId);
                if(info == null) {
                    continue;
                }
                DeviceActionsLayout layout = ControlBar.this.mActionsLayouts.get(info.mSapaApp.getInstanceId());
                Log.d(TAG, "info " + info);
                if (layout != null) {
                    if (!mInfo.mListHidden && mDevicesLayout.indexOfChild(layout) == -1) {
                        ControlBar.this.mDevicesLayout.addView(layout,
                                ControlBar.this.getEndIndex());
                        Boolean oExpanded = mInfo.mDevsExpanded.get(info.mSapaApp.getInstanceId());
                        boolean expanded = oExpanded != null ? oExpanded : false;
                        if (expanded) layout.expand();
                    }
                }
            }
        }
    }

    /**
     * This method removes all views from bar besides handler.
     */
    @SuppressWarnings("unused")
    private void clearBar() {
        if (this.mDevicesLayout != null) for (DeviceActionsLayout dev : mActionsLayouts.values()) {
            dev.collapse();
            this.mDevicesLayout.removeView(dev);
        }
    }

    @Override
    protected void onVisibilityChanged(View changedView, int visibility) {
        
        if (visibility == View.VISIBLE && mUpdateThread != null) {
            post(mUpdateThread);
            mUpdateThread = null;
        }
        super.onVisibilityChanged(changedView, visibility);
    }

    private void updateMainLayout() {
        if (!isShown()) return;
        if (mMainDeviceLayout!=null) {
            boolean addView = true;
            for (int i = 1; i < mDevicesLayout.getChildCount() - 1; ++i) {
                View v = mDevicesLayout.getChildAt(i);
                if (v.getTag() == null) continue;
                if ((v.getTag() instanceof DeviceActionsLayoutData)
                        && ((DeviceActionsLayoutData) v.getTag())
                                .equals(mMainDeviceLayout.getTag())) {
                    addView = false;
                    break;
                }
            }

            if (addView) addMainDevLayout();
            if (!mInfo.mListHidden) mMainDeviceLayout.expand();
        }  if((mRemoveMain)){
            mMainDeviceLayout.collapse();
            ViewGroup parent = (ViewGroup)mMainDeviceLayout.getParent();
            parent.removeView(mMainDeviceLayout);
            mMainDeviceLayout=null;
            mRemoveMain=false;
        }
    }

    protected ViewGroup getMainDevParent() {
        return (ViewGroup) mDevicesLayout;
    }

    protected void removeMainDevLayout() {

    }

    /**
     * This method sets content of control bar according to the list of active devices from
     * connection service.
     */
    private void fillBar() {
        Log.d(TAG, "\t\tActive:");

        if (getContext() != null && isShown()) {
            if (ControlBar.this.mDevicesActions != null) for (DeviceActionsLayoutData info : ControlBar.this.mDevicesActions
                    .values()) {
                Log.d(TAG, "action \t\t\t\t\t" + info);
            }
            else Log.d(TAG, "actions null");
            updateMainLayout();
            if (ControlBar.this.mDevicesActions != null) {
                for (DeviceActionsLayoutData info : ControlBar.this.mDevicesActions.values()) {
                    DeviceActionsLayout layout = ControlBar.this.mActionsLayouts
                            .get(info.mSapaApp.getInstanceId());
                    if (layout != null) {
                        if (!layout.equalsActionsOfInstance(info)) {
                            layout.updateInfo(info);
                        }
                    } else {
                        Boolean oExpanded = false;
                        boolean expanded = oExpanded != null ? oExpanded : false;
                        // check
                        if (info.mInstancePackageName.equals(mInfo.mMainApplicationPackage)) {
                            if (ControlBar.this.mMainDeviceLayout == null) {
                                createMainAppLayout(info, expanded);
                            }
                        } else {
                            DeviceActionsLayout newLayout = createAppLayout(info, expanded);
                            ControlBar.this.mActionsLayouts.put(info.mSapaApp.getInstanceId(), newLayout);
                            if (!mInfo.mListHidden)
                                ControlBar.this.mDevicesLayout.addView(newLayout,
                                        ControlBar.this.getEndIndex());
                        }
                    }
                }
            }
        }
    }

    /**
     * This method sets local devices actions layout map according to the list on connection
     * service.
     */
    private void updateBar() {
        List<String> toRemove = new ArrayList<String>();
        for (String deviceUid : ControlBar.this.mActionsLayouts.keySet()) {
            if (!ControlBar.this.mDevicesActions.containsKey(deviceUid)) {
                toRemove.add(deviceUid);
            }
        }
        for (String key : toRemove) {
            DeviceActionsLayout dev = ControlBar.this.mActionsLayouts.remove(key);
            dev.collapse();
            ControlBar.this.mDevicesLayout.removeView(dev);

        }
    }

    private int getEndIndex() {
        return ControlBar.this.mDevicesLayout.getChildCount()-1;
    }

    /**
     * 
     * @return true if the list of devices actions has changed, false otherwise
     */
    private synchronized boolean initializeActions() {
        if (this.mSapaAppService != null) {
            Map<String, DeviceActionsLayoutData> devicesActions = new HashMap<String, DeviceActionsLayoutData>();
            List<String> orderingList = null;
            synchronized (mLock) {
                List<SapaAppInfo> list;
                try {
                    list = this.mSapaAppService.getAllActiveApp();
                    if (list != null) {
                        for (SapaAppInfo info : list) {
                            Log.d(TAG, "app instanceId:" + info.getApp().getInstanceId());
                            Bundle bundle = info.getConfiguration();

                            if(bundle != null) {
                                orderingList = bundle.getStringArrayList("track_order");
                            }
                            try {
                                devicesActions.put(info.getApp().getInstanceId(),
                                        new DeviceActionsLayoutData(info, getContext(),
                                                isMultiInstance(info)));
                            } catch (NameNotFoundException e) {
                                Log.d(TAG, "Actions of " + info.getApp().getInstanceId());
                            }

                            if (info.getPackageName().equals(mInfo.mMainApplicationPackage)) {
                                if (ControlBar.this.mMainDeviceLayout == null) {
                                    createMainAppLayout(info, false);
                                }
                            }
                        }

                        if (!devicesActions.equals(this.mDevicesActions)) {
                            if (this.mDevicesActions == null
                                    || !this.mDevicesActions.equals(devicesActions)) {
                                this.mDevicesActions = devicesActions;
                                mOrderingList = orderingList;
                                return true;
                            }
                        }
                    }
                } catch (SapaConnectionNotSetException e1) {
                    //devicesActions = new HashMap<String, DeviceActionsLayoutData>();
                    Log.w(TAG, "Devices were not obtained due to problems with connection");
                }
            }
        }
        return false;
    }

    abstract void addSubViews(List<? extends View> subViews, View view);

    abstract void clearSubViews(int number, View view);

    private void createMainAppLayout(SapaAppInfo info, boolean expanded) {
        try {
            DeviceActionsLayoutData data = new DeviceActionsLayoutData(info, getContext(),
                    isMultiInstance(info));
            createMainAppLayout(data, expanded);
        } catch (NameNotFoundException e) {
            e.printStackTrace();
        }
    }

    private DeviceActionsLayout createAppLayout(DeviceActionsLayoutData info, boolean expanded) {
        DeviceActionsLayout newLayout = null;
        newLayout = new OrdinaryDeviceLayout(ControlBar.this.getContext(), info,
                ControlBar.this.mSapaAppService, expanded, ControlBar.this.getOrientation(), this);
        newLayout.setLayoutParams(new LayoutParams(DeviceActionsLayout.BUTTON_WIDTH,
                DeviceActionsLayout.BUTTON_HEIGHT));
        return newLayout;
    }

    private void createMainAppLayout(DeviceActionsLayoutData info, boolean expanded) {
        DeviceActionsLayout newLayout = null;
        newLayout = new MainDeviceLayout(ControlBar.this.getContext(), info,
                ControlBar.this.mSapaAppService, expanded, ControlBar.this.getOrientation(), this);
        newLayout.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT,
                LayoutParams.WRAP_CONTENT));
        newLayout.getChildAt(0).setOnLongClickListener(new MyLongTounchListener());
        ControlBar.this.mMainDeviceLayout = newLayout;
        ControlBar.this.mMainDeviceLayout.setTag(info);
        addMainDevLayout();
    }

    protected void addMainDevLayout() {
        Log.d(TAG, "mainLayout add");
        if (mDevicesLayout.isShown())
            ControlBar.this.mDevicesLayout.addView(ControlBar.this.mMainDeviceLayout, 1);
    }

    /**
     * This method logs content of devices actions list.
     */
    @SuppressWarnings("unused")
    private synchronized void logReceivedAppActions() {
        if (this.mDevicesActions != null) {
            for (DeviceActionsLayoutData appAction : this.mDevicesActions.values()) {
                Log.d(TAG, "DeviceId: " + appAction.mSapaApp);
            }
        } else {
            Log.d(TAG, "no devices");
        }
    }

    @Override
    public synchronized void onAppActivated(SapaApp sapaApp) {
        Log.d(TAG, "onAppActivated &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        try {

        	final SapaAppInfo appInfo = this.mSapaAppService.getActiveApp(sapaApp);

            if (appInfo.getPackageName().equals(mInfo.mMainApplicationPackage)) {
                if (mMainDeviceLayout == null) {
                	post(new Runnable() {
						
						@Override
						public void run() {
							createMainAppLayout(appInfo, false);
						}
					});
                    
                }
                return;
            }
            DeviceActionsLayoutData data = new DeviceActionsLayoutData(appInfo, getContext(),
                    isMultiInstance(appInfo));
            this.mDevicesActions.put(sapaApp.getInstanceId(), data);
            mInfo.mDevsExpanded.put(sapaApp.getInstanceId(), false);
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "AppInfo of activated instance " + sapaApp.getInstanceId() + " could not be obtained");
        } catch (NameNotFoundException e) {
            Log.w(TAG, "SapaAppInfo of activated instance " + sapaApp.getInstanceId()
                    + " could not be obtained as it's icon could not be found");
        }
        ControlBar.this.refresh();
        this.logActive();
    }

    private boolean isMultiInstance(SapaAppInfo appInfo) {
        if(!appInfo.isMultiInstanceEnabled()) {
            return false;
        }
        List<SapaAppInfo> list = null;
        try {
            list = mSapaAppService.getAllActiveApp();
            for(SapaAppInfo info : list) {
                if(info.getApp().getInstanceId().equals(appInfo.getApp().getInstanceId())) {
                    continue;
                }
                if(info.getPackageName().equals(appInfo.getPackageName())) {
                    return true;
                }
            }
        } catch (SapaConnectionNotSetException e) {
            e.printStackTrace();
        }

        return false;
    }

    private synchronized void logActive() {
        Log.d(TAG, "\t\tActive:");
        for (String id : this.mActionsLayouts.keySet()) {
            Log.d(TAG, "\t\t\t\t\t" + id);
        }
    }

    @Override
    public synchronized void onAppDeactivated(SapaApp sapaApp) {
        Log.d(TAG, "onAppDeactivated &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");
        if (mMainDeviceLayout != null
                && (DeviceActionsLayoutData) mMainDeviceLayout.getTag() != null
                && ((DeviceActionsLayoutData) mMainDeviceLayout.getTag()).mSapaApp != null
                && ((DeviceActionsLayoutData) mMainDeviceLayout.getTag()).mSapaApp.getInstanceId() != null
                && sapaApp != null
                && ((DeviceActionsLayoutData) mMainDeviceLayout.getTag()).mSapaApp.getInstanceId()
                        .equals(sapaApp.getInstanceId())) {
            Log.d(TAG, "main to nul");
            mRemoveMain=true;
            post(new Runnable() {
				
				@Override
				public void run() {
					updateMainLayout();
				}
			});
            
        }
        // this.mActionsLayouts.remove(instanceId);
        if(sapaApp != null){
            if(mDevicesActions != null) this.mDevicesActions.remove(sapaApp.getInstanceId());
            if(mInfo != null && mInfo.mDevsExpanded != null ) mInfo.mDevsExpanded.remove(sapaApp.getInstanceId());
        }
        if(getContext() != null){
            try{
            ((Activity) getContext()).runOnUiThread(new Runnable() {
                public void run() {
                    synchronized (ControlBar.this) {
                        ControlBar.this.updateBar();
                        ControlBar.this.fillBar();
                        if (ControlBar.this.mDevicesActions.size() == 0) hide();
                    }
                }
            });
            }catch(NullPointerException e){
                ;
            }
        }

        this.logActive();
    }

    @Override
    public synchronized void onAppChanged(SapaApp sapaApp) {
        Log.d(TAG, "onInstanceChanged &&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&");

        SapaAppInfo appInfo;
        try {
            appInfo = ControlBar.this.mSapaAppService.getActiveApp(sapaApp);
            DeviceActionsLayoutData data = new DeviceActionsLayoutData(appInfo, getContext(),
                    isMultiInstance(appInfo));
            if (!sapaApp.getPackageName().equals(mInfo.mMainApplicationPackage))
                ControlBar.this.mDevicesActions.put(sapaApp.getInstanceId(), data);


            ((Activity)getContext()).runOnUiThread(new Updater(sapaApp.getInstanceId(), data));

        } catch (SapaConnectionNotSetException e) {
            e.printStackTrace();
        } catch (NameNotFoundException e) {
            e.printStackTrace();
        }
    }

    class Updater implements Runnable {

        String mInstanceId;
        DeviceActionsLayoutData mData;

        Updater(String instanceId, DeviceActionsLayoutData data) {
            this.mInstanceId = instanceId;
            this.mData = data;
        }

        @Override
        public void run() {
            synchronized (ControlBar.this) {
                if (mData.mInstancePackageName.equals(mInfo.mMainApplicationPackage)) {
                    mMainDeviceLayout.updateInfo(mData);
                } else if (ControlBar.this.mActionsLayouts != null
                        && ControlBar.this.mActionsLayouts.get(mInstanceId) != null) {
                    ControlBar.this.mActionsLayouts.get(mInstanceId).updateInfo(mData);
                }
            }
        }

    }
    
    private Runnable mUpdateThread;

    public void refresh() {
        mUpdateThread = new Runnable() {
            
            @Override
            public void run() {
                LayoutTransition trans=null;
                updateMainLayout();
                if (mDevicesLayout != null) {
                    trans = mDevicesLayout.getLayoutTransition();
                    mDevicesLayout.setLayoutTransition(null);
                }
                if (mInfo.mListHidden) hide();
                else{
                    updateBar();
                    fillBar();
                    show();

                    if (mMainDeviceLayout != null) mMainDeviceLayout.refresh();
                    if (mActionsLayouts != null)
                        for (DeviceActionsLayout lay : mActionsLayouts.values()) {
                            lay.refresh();
                        }
                }
                mDevicesLayout.setLayoutTransition(trans);

                updateDrawables();
                mDevicesLayout.invalidate();
            }
        };
        if (isShown()) {
            post(mUpdateThread);
            mUpdateThread = null;
        }
    }

    public Info getInfo() {
        return mInfo;
    }
    
    private void checkMainApp(){
        if(mSapaAppService==null || mInfo.mMainApplicationPackage==null || mInfo.mMainApplicationPackage.isEmpty()) return;
        try {
            for (SapaAppInfo info : mSapaAppService.getAllInstalledApp()) {
                if (info.getPackageName().equals(mInfo.mMainApplicationPackage) && info.isMultiInstanceEnabled()) {
                    throw new IllegalStateException(
                            "Only single instance app can be declareed as Main application for floating controller");
                }
            }
        } catch (SapaConnectionNotSetException e) {
            Log.w(TAG, "connection was not set");
            e.printStackTrace();
        }
    }

    @Override
    public void onServiceConnected() {
        Log.d(TAG, "onServiceConnected %%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%");
        checkMainApp();
        if(getContext() != null){
            try{
            ((Activity) getContext()).runOnUiThread(new Runnable() {
                public void run() {
                    synchronized (ControlBar.this) {
                        ControlBar.this.initializeActions();
                        ControlBar.this.updateBar();
                        ControlBar.this.fillBar();
                    }
                }
            });
            }catch(NullPointerException e){
                Log.e(TAG, "Exception:", e);
            }
        }
    }

    @Override
    public void onAppInstalled(SapaApp sapaApp) {
    	checkMainApp();

    }

    @Override
    public void onAppUninstalled(SapaApp sapaApp) {
        // nothing to be done
    }

    @Override
    public void onTransportMasterChanged(SapaApp sapaApp) {

    }

    static class Info {
        private boolean mListHidden;
        private Map<String, Boolean> mDevsExpanded;
        private Map<String, Boolean> mCopiedDevs;
        //private boolean mSavedHiddenFlag;
        private int mHandleDrawableRes;
        private String mMainApplicationPackage = "";
        

        public Info(boolean listHidden, Map<String, Boolean> devicesExpanded, int handleDrawableRes) {
            this.mListHidden = listHidden;
            mHandleDrawableRes = (handleDrawableRes != 0 ? handleDrawableRes
                    : R.drawable.arrow_open);
            this.mDevsExpanded = devicesExpanded == null ? this.mDevsExpanded = new HashMap<String, Boolean>()
                    : devicesExpanded;
        }

        public Info() {
            this.mListHidden = true;
            this.mDevsExpanded = new HashMap<String, Boolean>();
            this.mHandleDrawableRes = R.drawable.arrow_open;
        }

        public boolean isDevExpanded(String dev) {
            if (!mDevsExpanded.containsKey(dev)) return false;
            return mDevsExpanded.get(dev).booleanValue();
        }
        
        public void setDeviceExpanded(String dev, boolean expanded){
            mDevsExpanded.put(dev, expanded);
        }

        public synchronized void saveState(){
            mCopiedDevs= new HashMap<String, Boolean>();
            mCopiedDevs.putAll(mDevsExpanded);
        }

        public synchronized void restoreState(){
            if(mCopiedDevs==null) return;
            mDevsExpanded= new HashMap<String, Boolean>();
            mDevsExpanded.putAll(mCopiedDevs);
        }

    }
}
