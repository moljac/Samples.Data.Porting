package com.samsung.android.sdk.professionalaudio.widgets;

import android.animation.LayoutTransition;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.content.ActivityNotFoundException;
import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageManager.NameNotFoundException;
import android.content.res.Resources;
import android.content.res.Resources.NotFoundException;
import android.graphics.drawable.Drawable;
import android.os.Bundle;
import android.util.LayoutDirection;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.animation.AnimationSet;
import android.widget.ImageButton;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.app.SapaUndeclaredActionException;

import java.lang.ref.WeakReference;
import java.util.ArrayList;
import java.util.HashSet;
import java.util.List;

/**
 * This class represents view of actions of one application. It's content is set according to the
 * given device action info.
 * 
 * 
 */
@SuppressLint("ViewConstructor")
abstract class DeviceActionsLayout extends LinearLayout {

    public static final String APP_RET_BUTTONS = "app_ret_buttons";
    public static final String APP_RET_BUTTONS_OPTIONS = "app_ret_buttons_options";
    private static String TAG = "professionalaudioconnection:widget:j:DeviceActionsLayout";
    protected DeviceActionsLayoutData mData;
    private SapaAppService mSapaAppService;
    protected boolean mIsExpanded = false;
    private int mAnimationTime;
    protected ArrayList<View> mActionButtons;
    private ImageButton mAppButton;
    protected ControlBar mParent;
    private Runnable  updateThread;
    protected int mDeviceIconMaxSize;

    private volatile int mPrevLayoutDir=-1;
    protected static int BUTTON_WIDTH;
    protected static int BUTTON_HEIGHT;
    
    private final int FIRST=0;
    private final int INNER=1;
    private final int LAST=2;
    private final int ONLY=3;
    private View mButtonsView;
    private LinearLayout mActionButtonsLayout;

    /**
     * @return True if actions are expanded (visible), false otherwise.
     */
    protected boolean isExpanded() {
        return this.mIsExpanded;
    }

    /**
     * @return Uid of the device.
     */
    public SapaApp getSapaApp() {
        return this.mData.mSapaApp;
    }

    public boolean equalsActionsOfInstance(DeviceActionsLayoutData data) {
        return this.mData.mSapaApp.equals(data.mSapaApp)
                && this.mData.mActionList.equals(data.mActionList)
                && this.mData.mInstanceIcon.equals(data.mInstanceIcon)
                && this.mData.mInstancePackageName.equals(data.mInstancePackageName);
    }

    /**
     * This method updates values on the layout. As a result the layout is the representation of the
     * given device action info.
     * 
     * @param data
     *            DeviceActionsLayoutData object with parameters for the view to create
     * 
     */
    public void updateInfo(DeviceActionsLayoutData data) {
        this.mData.mSapaApp = data.mSapaApp;
        this.mData.mActionList = data.mActionList;
        this.mData.mInstanceIcon = data.mInstanceIcon;
        this.mData.mInstancePackageName = data.mInstancePackageName;
        this.mData.loadActionMap();
        this.prepareActions(mParent.getOrientation(), true);
    }

    /**
     * This method sets the orietation of the layout.
     */
    @Override
    public void setOrientation(int orientation) {
        // if (this.getOrientation() != orientation) {
        this.forceOrientationSet(orientation);
        // }
    }

    private synchronized void forceOrientationSet(int orientation) {
        // this.mActions.setOrientation(orientation);
        super.setOrientation(orientation);
        int level = calculateLevel();
        if (mAppButton != null){
            mAppButton.getBackground().setLevel(level);
            mAppButton.setLayoutParams(getAppBtnLayoutParams(orientation));
        }
        if (mActionButtons != null) {
            for (View actionButton : this.mActionButtons) {
                if(actionButton.getBackground() != null) {
                    actionButton.getBackground().setLevel(level);
                }
            }
        }
    }

    public int getAnimationTime() {
        return this.mAnimationTime + ControlBar.ANIM_TIME;
    }

    /**
     * This Ctor creates device info that represents given device action info and calls actions by
     * jam connection service. Layout is created with buttons of actions hidden.
     * 
     * @param context
     *            Context in which layout is created.
     * @param data
     *            DeviceActionsLayoutData object with parameters for the view to create
     * @param sapaAppService 
     *            Instance of SapaAppService of this application
     * @param orientation
     *            Orientation of the bar.
     * @param controlbar
     *            Local instance of ControlBar
     */
    protected DeviceActionsLayout(Context context, DeviceActionsLayoutData data,
            SapaAppService sapaAppService, int orientation,
            ControlBar controlbar) {
        this(context, data, sapaAppService, false, orientation, controlbar);
    }

    /**
     * This Ctor creates device info that represents given device action info and calls actions by
     * jam connection service.
     * 
     * @param context
     *            Context in which layout is created.
     * @param data
     *            DeviceActionsLayoutData object with parameters for the view to create
     * @param sapaAppService 
     *            Instance of SapaAppService of this application 
     * @param isExpanded
     *             Boolean indicating whether the bar should be expanded
     * @param orientation
     *            Orientation of the bar.
     * @param controlbar
     *            Reference to parent controlbar widget
     */
    protected DeviceActionsLayout(Context context, DeviceActionsLayoutData data,
    		SapaAppService sapaAppService, boolean isExpanded, int orientation,
            ControlBar controlbar) {
        super(context);
        initAppIconMaxSize();
        mParent = controlbar;

        this.mData = data;
        this.setHorizontalGravity(Gravity.CENTER_HORIZONTAL);
        this.setVerticalGravity(Gravity.CENTER_VERTICAL);
        this.setOrientation(orientation);
        this.setLayoutDirection(LayoutDirection.INHERIT);


        // mActionButtons = new ArrayList<ImageButton>();
        this.mIsExpanded = isExpanded;
        mButtonsView = createOpenButtonLayout();
        this.mAppButton = createDeviceButton();
        String appName = getAppName();
        if (appName != null && !appName.isEmpty()) {
            this.mAppButton.setContentDescription(getResources().getString(R.string.app_btn_options, appName));
        }
        this.setLayoutParams(new LayoutParams(LayoutParams.WRAP_CONTENT, LayoutParams.WRAP_CONTENT));
        this.addView(this.mAppButton, getAppBtnLayoutParams(getOrientation()));
        this.mSapaAppService = sapaAppService;
        createActionList();
        int buttonsNo = this.mData.mActionList != null ? (this.mData.mActionList.size() + 1) : 0;
        this.mAnimationTime = ControlBar.ANIM_TIME * buttonsNo;
        if (this.mIsExpanded) {
            show();
        }
    }

    protected LinearLayout.LayoutParams getAppBtnLayoutParams(int orientation){
        return orientation == LinearLayout.HORIZONTAL ?
                new LinearLayout.LayoutParams(90, 80 ) :
                new LinearLayout.LayoutParams(80 , 90);
    }

    private void createActionList() {
        mActionButtons = new ArrayList<View>();

        mActionButtonsLayout = (LinearLayout) LayoutInflater.from(getContext())
                .inflate(R.layout.open_buttons_layout, this, false);
        createActionButtons();
        if(this instanceof MainDeviceLayout) {
            mActionButtons.add(mActionButtonsLayout);
            mActionButtons.add(mButtonsView);
        } else {
            mActionButtons.add(mButtonsView);
            mActionButtons.add(mActionButtonsLayout);
        }
        checkInvisibles();
    }

    private void initAppIconMaxSize(){
        Resources res = getResources();
        mDeviceIconMaxSize=res.getDimensionPixelSize(R.dimen.max_app_ic_size);
    }

    protected void show() {
        show(ShowMode.ENABLED_TRANSITION);
    }

    protected void show(ShowMode mode) {
        int size = mActionButtons.size();

        if (mPrevLayoutDir != mParent.getLayoutDirection() && mActionButtonsLayout.getChildCount() > 1) {
            createActionButtons();
            mPrevLayoutDir = mParent.getLayoutDirection();
            checkInvisibles();
        }

        int level = calculateLevel();
        for (View v : mActionButtons) {
            if(v.getBackground() != null) {
                v.getBackground().setLevel(level);
            }

            // This is a hack, not really a good design. It's a hack for bad design of
            // FloatingController. The hack sets alpha to 0 to all ordinary device layouts
            // when they got shown on the controller bar. The reason is to make alpha animation
            // (fade in) to work. APPEAR animation is run after the CHANGING_APPEAR. The latter one
            // is displayed by default with alpha 1, so we need to set it to 0, so the APPEAR
            // animation will animate it from 0 to 1.
            //
            if (!(this instanceof MainDeviceLayout) && mode != ShowMode.DISABLED_TRANSITION) {
                v.setAlpha(0f);
            } else {
                v.setAlpha(1f);
            }
        }

        mParent.addSubViews(mActionButtons.subList(0, 1), this);

        if (mActionButtons.size() > 1)
            mParent.addSubViews(mActionButtons.subList(1, size), mActionButtons.get(0));
    }

    private void createActionButtons() {
        mActionButtonsLayout.removeAllViews();
        if(mParent.getLayoutDirection() == LAYOUT_DIRECTION_LTR) {
            for (int i = 0, size = mData.mActionList.size(); i <= size - 1; ++i) {
                mActionButtonsLayout.addView(createActionView(mData.mActionList.get(i), R.layout.action_button));
            }
        } else {
            for (int i = mData.mActionList.size() - 1; i >= 0; --i) {
                mActionButtonsLayout.addView(createActionView(mData.mActionList.get(i), R.layout.action_button));
            }
        }

    }

    protected void hide() {
        if (this.getParent() != null) //for (View button : mActionButtons)
            mParent.clearSubViews(mActionButtons.size(), this);
    }

    private void removeActionView(View v) {
        if (v.isShown()) {
            int index = mActionButtonsLayout.indexOfChild(v);
            mActionButtonsLayout.removeView(v);
        }
    }

    private View createActionView(SapaActionInfo info, int res) {
        ImageButton button = null;
        try {
            button = (ImageButton) LayoutInflater.from(getContext()).inflate(res, mActionButtonsLayout, false);
            button.setImageDrawable(info.getIcon(getContext()));
            
            if (!info.isEnabled()) {
                button.setEnabled(false);
            }

            button.setOnClickListener(new OnActionClickListener(this.mData.mSapaApp,
                    info.getId()));
            String actionName = info.getName(getContext());
            if (actionName!=null) {
                button.setContentDescription(actionName);
            }

        } catch (NameNotFoundException e) {
            Log.w(TAG, "Action " + info.getId() + " could not be shown.");

        } catch (NotFoundException e) {
            Log.w(TAG, "Action " + info.getId() + " could not be shown.");
        }

        button.setTag(info);
        if (info.isVisible()) {
            button.setVisibility(View.VISIBLE);
        } else {
            button.setVisibility(View.GONE);
        }
        return button;
    }

    private void replaceActionOnView(View v, SapaActionInfo info) {
        int order = mActionButtonsLayout.indexOfChild(v);
        if (info == null || order == -1) return;
        View newView = createActionView(info, R.layout.action_button);
        if (mParent.isShown()) {
            mActionButtonsLayout.removeView(v);
            mActionButtonsLayout.addView(newView, order);
        }
    }

    @Override
    protected void onLayout(boolean changed, int l, int t, int r, int b) {
        super.onLayout(changed, l, t, r, b);
        int level = calculateLevel();
        if (mActionButtons != null) for (View v : mActionButtons) {
            v.getBackground().setLevel(level);
        }
        mAppButton.getBackground().setLevel(level);
    }

    /**
     * This method prepares view of actions (without application icon).
     */
    void prepareActions(int orientation, boolean addView) {
        if (!isExpanded()) {
            createActionList();
            return;
        }
        LayoutTransition trans = null;
        if (mParent != null) {
            trans = mParent.mDevicesLayout.getLayoutTransition();
            mParent.mDevicesLayout.setLayoutTransition(null);
        }
        ArrayList<View> toBeRemoved = new ArrayList<View>();

        for (int i = 0, size = mActionButtonsLayout.getChildCount(); i < size; ++i) {
            View v = mActionButtonsLayout.getChildAt(i);
            SapaActionInfo info = (SapaActionInfo) v.getTag();
            if (!mData.mActionMap.containsKey(info.getId())) {
                toBeRemoved.add(v);
            }
        }
        if (!toBeRemoved.isEmpty()) {
            int sizeToRemove = toBeRemoved.size();
            for (int i = 0; i < sizeToRemove; ++i) {
                removeActionView(toBeRemoved.get(i));
            }
        }
        HashSet<String> ids = new HashSet<String>();
        for (int i = 0, size = mActionButtonsLayout.getChildCount(); i < size; ++i) {
            View v = mActionButtonsLayout.getChildAt(i);
            SapaActionInfo info = (SapaActionInfo) v.getTag();
            ids.add(info.getId());
            if (!info.equals(mData.mActionMap.get(info.getId()))) {
                replaceActionOnView(v, mData.mActionMap.get(info.getId()));
            }
        }
        List<View> viewsToAdd = new ArrayList<View>();
        int i = -1;
        for (String actionId : mData.mActionMap.keySet()) {
            ++i;
            if (ids.contains(actionId)) continue;
            if (i == 1) {
				viewsToAdd.add(createActionView(mData.mActionMap.get(actionId), R.layout.action_button));
			}
            else {
                viewsToAdd.add(createActionView(mData.mActionMap.get(actionId), R.layout.action_button));
			}
        }
        if (viewsToAdd.size() > 0) {

            View viewToUpdate = mActionButtons.get(mActionButtons.size() - 1);
            for (View v : viewsToAdd) {
                mActionButtons.add(v);
            }
            replaceActionOnView(viewToUpdate, (SapaActionInfo) viewToUpdate.getTag());
            replaceActionOnView(viewsToAdd.get(0), (SapaActionInfo) viewsToAdd.get(0).getTag());
            if (viewsToAdd.size() > 1)
                replaceActionOnView(viewsToAdd.get(viewsToAdd.size() - 1), (SapaActionInfo) viewsToAdd
                        .get(viewsToAdd.size() - 1).getTag());

            int size = mActionButtons.size();
            mParent.addSubViews(mActionButtons.subList(0, 1), this);
            mParent.addSubViews(mActionButtons.subList(1, size), mActionButtons.get(0));

        }
        checkInvisibles();
        if (mParent != null && trans != null) mParent.mDevicesLayout.setLayoutTransition(trans);
        if(mParent != null) this.forceOrientationSet(mParent.getOrientation());
    }
    
    private String getAppName(){
    	String name = null;
    	ApplicationInfo ai=null;
        try {
			ai = getContext().getPackageManager().getApplicationInfo(mData.mInstancePackageName, 0);
		} catch (NameNotFoundException e1) {
			Log.e(TAG, e1.getMessage());
		}

        if(ai!=null)
        	name =getContext().getPackageManager().getApplicationLabel(ai).toString();
        return name;
    }

    protected void openAppActivity(){
        openAppActivity(1);
    }

    protected void openAppActivity(int mode){
        if (DeviceActionsLayout.this.mData.mInstancePackageName.length() != 0) {
            Intent intent;
            try {
                intent = DeviceActionsLayout.this.mSapaAppService
                        .getLaunchIntent(DeviceActionsLayout.this.mData.mSapaApp);
                if (intent != null) {
                    if(getContext() != null){
                        intent.setExtrasClassLoader(SapaAppInfo.class.getClassLoader());
                        intent.putExtra("Edit_mode", mode);
                        getContext().startActivity(intent);

                        // send broadcast
                        Intent bi = new Intent("com.samsung.android.sdk.professionalaudio.ACTION.SWITCH_TO_SAPA_APP");
                        bi.putExtra("com.samsung.android.sdk.professionalaudio.switchTo.instanceID", DeviceActionsLayout.this.mData.mSapaApp.getInstanceId());
                        bi.putExtra("com.samsung.android.sdk.professionalaudio.switchTo.packageName", DeviceActionsLayout.this.mData.mSapaApp.getPackageName());
                        getContext().sendBroadcast(bi, "com.samsung.android.sdk.professionalaudio.permission.USE_CONNECTION_SERVICE");

                        Context context = getContext();
                        if(context instanceof Activity &&
                                    context.getPackageName().contentEquals(DeviceActionsLayout.this.mData.mSapaApp.getPackageName()) == false){
                                ((Activity) context).finish();
                        }
                    } else {
                        Log.w(TAG, "Fail to swith because of the context is null.");
                    }
                } else {
                    Log.w(TAG, "Fail to swith because of the launchIntent is null.");
                }
            } catch (SapaConnectionNotSetException e) {
                Log.w(TAG,
                        "Application can not be opened from ControlBar due to connection problem.");
            } catch (IllegalAccessException e) {
                Log.w(TAG,
                        "Application can not be opened from FloatingController because of its internal error.");
            } catch(ActivityNotFoundException e){
                Log.w(TAG,
                        "Application can not be opened from FloatingController because of not existing activity");
            }
        }
    }

    /**
     * This method creates button to jump to applicaction. It sets its looks as well as bahaviour.
     *
     * @return Button to open application which actions are respresented by this layout.
     */
    private View createOpenButtonLayout() {
        Bundle bundle = mData.mAppInfo.getConfiguration();
        LinearLayout layout = (LinearLayout)LayoutInflater.from(getContext())
                .inflate(R.layout.open_buttons_layout, this, false);
        if(bundle != null && bundle.getIntArray(APP_RET_BUTTONS) != null
                && bundle.getIntArray(APP_RET_BUTTONS_OPTIONS) != null) {
            int[] openRes = bundle.getIntArray(APP_RET_BUTTONS);
            final int[] openOptions = bundle.getIntArray(APP_RET_BUTTONS_OPTIONS);
            for(int i = 0; i < openRes.length; ++i) {
                ImageButton openButton = (ImageButton) LayoutInflater.from(getContext()).inflate(
                        R.layout.open_btn_view, layout, false);
                Drawable drawable = getDrawableFromApp(mData.mAppInfo, openRes[i]);
                if (drawable != null) {
                    openButton.setImageDrawable(drawable);
                }
                String appName = getAppName();
                if (appName != null && !appName.isEmpty()) {
                    openButton.setContentDescription(getResources().getString(R.string.open_app_btn_desc, appName));
                }
                final int mode = openOptions[i];
                openButton.setOnClickListener(new View.OnClickListener() {
                    @Override
                    public void onClick(View v) {
                        openAppActivity(mode);
                    }
                });
                layout.addView(openButton);
            }
            return layout;
        } else {
            ImageButton openButton = (ImageButton) LayoutInflater.from(getContext()).inflate(
                    R.layout.open_btn_view, this, false);

            String appName = getAppName();
            if (appName != null && !appName.isEmpty()) {
                openButton.setContentDescription(getResources().getString(R.string.open_app_btn_desc, appName));
            }
            openButton.setOnClickListener(new View.OnClickListener() {

                @Override
                public void onClick(View v) {
                    openAppActivity();
                }
            });
            return openButton;
        }
    }

    private Drawable getDrawableFromApp(SapaAppInfo appInfo, int openRe) {
        Context context = getContext();
        try {
            return context.getPackageManager().getResourcesForApplication(appInfo.getPackageName())
                    .getDrawable(openRe);
        } catch (NameNotFoundException e) {
            Log.e(TAG, "Exception:", e);
        }
        return null;
    }

    /**
     * This method creates button to show and hide its buttons of actions. It sets its looks as well
     * as bahaviour.
     * 
     * @return Button to hide and show action of represented application.
     */
    abstract protected ImageButton createDeviceButton();

    void expand() {
        if (mIsExpanded) {
            return;
		}
        DeviceActionsLayout.this.mIsExpanded = true;
        show();
        mParent.setExpandCalled(true);
        mParent.getInfo().setDeviceExpanded(mData.mSapaApp.getInstanceId(), true);
    }

    void collapse() {
        if (!mIsExpanded) {
			return;
		}
        DeviceActionsLayout.this.mIsExpanded = false;
        hide();
        mParent.getInfo().setDeviceExpanded(mData.mSapaApp.getInstanceId(), false);
    }

    @Override
    protected void onVisibilityChanged(View changedView, int visibility) {
        super.onVisibilityChanged(changedView, visibility);
        if (visibility == View.VISIBLE){
            prepareActions(mParent.getOrientation(), true);
            if(updateThread!=null)
                post(updateThread);
            updateThread=null;
        }
    }

    public enum ShowMode {
        ENABLED_TRANSITION,
        DISABLED_TRANSITION,
    }

    public void refresh(){
        updateThread = new Runnable() {
            @Override
            public void run() {
                LayoutTransition trans = null;
                if (mParent != null) {
                    trans = mParent.mDevicesLayout.getLayoutTransition();
                    mParent.mDevicesLayout.setLayoutTransition(null);

                    if (mParent.getInfo().isDevExpanded(mData.mSapaApp.getInstanceId())) {
                        if (mIsExpanded) hide();
                        show(ShowMode.DISABLED_TRANSITION);
                        mIsExpanded = true;
                    } else {
                        if (mIsExpanded) hide();
                        mIsExpanded = false;
                    }
                    if (trans != null)
                        mParent.mDevicesLayout.setLayoutTransition(trans);
                }
            }
        };

        if(isShown()){
            post(updateThread);
            updateThread=null;
        }
    }
    
    private void checkInvisibles(){
        if(mActionButtons==null) return;
        for (int i=0, size=mActionButtonsLayout.getChildCount(); i<size; ++i){
            View v = mActionButtonsLayout.getChildAt(i);
            SapaActionInfo info = (SapaActionInfo) v.getTag();
            if (info != null && info.isVisible()) {
                mActionButtonsLayout.getChildAt(i).setVisibility(VISIBLE);
            } else {
                mActionButtonsLayout.getChildAt(i).setVisibility(GONE);
            }
        }
    }
    

    // TODO get rid of it
    protected int calculateLevel() {
        return (mParent.getOrientation() * 2) + mParent.getLayoutDirection();
    }

    /**
     * This class is used to handle onclick on action's buttons.
     * 
     */
    private class OnActionClickListener implements View.OnClickListener {

        private String actionId;
        private SapaApp mSapaApp;

        /**
         * @param sapaApp 
         *             SapaApp object for instance of application to which chosen action belongs
         * @param actionId
         *            Id of action that is represented on button.
         */
        public OnActionClickListener(SapaApp sapaApp, String actionId) {
            this.actionId = actionId;
            this.mSapaApp = sapaApp;
        }

        /**
         * As the result of onClick action is called on the device using jam connection service.
         */
        @Override
        public void onClick(View v) {
            if (DeviceActionsLayout.this.mSapaAppService != null) {
                try {
                    DeviceActionsLayout.this.mSapaAppService.callAction(
                            this.mSapaApp, this.actionId);
                } catch (SapaConnectionNotSetException e) {
                    Log.w(TAG, "Action could not be called due to connection problem.");
                } catch (SapaUndeclaredActionException e) {
                    Log.w(TAG, "Attempt to call undeclared action.");
                }
            }
        }
    }
}
