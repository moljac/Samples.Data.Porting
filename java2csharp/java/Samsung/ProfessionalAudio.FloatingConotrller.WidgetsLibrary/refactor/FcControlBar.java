package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.app.Activity;
import android.content.ClipData;
import android.content.Context;
import android.content.pm.PackageManager;
import android.content.res.TypedArray;
import android.os.Bundle;
import android.os.Looper;
import android.support.v7.widget.RecyclerView;
import android.util.AttributeSet;
import android.util.Log;
import android.view.DragEvent;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.RelativeLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.app.SapaConnectionNotSetException;
import com.samsung.android.sdk.professionalaudio.widgets.FloatingController;
import com.samsung.android.sdk.professionalaudio.widgets.R;

import org.solovyev.android.views.llm.LinearLayoutManager;

import java.util.Map;

/**
 * @brief Floating controller ControlBar view class
 */
public class FcControlBar extends LinearLayout implements FcContext.FcContextStateChanged {

    private FcAdapter mAdapter;

    /**
     * Class responsible for responding on long click.
     */
    private final class FcControlBarLongClickListener implements OnLongClickListener {

        @Override
        public boolean onLongClick(View view) {
            ClipData data = ClipData.newPlainText("", "");
            DragShadowBuilder shadowBuilder = new View.DragShadowBuilder(mMainAppImage);
            view.startDrag(data, shadowBuilder, mMainAppImage, 0);
            return true;
        }
    }

    public static final int ALIGN_PARENT_BOTTOM_RIGHT = 5;
    public static final int ALIGN_PARENT_BOTTOM_LEFT = 6;
    public static final int ALIGN_PARENT_TOP_RIGHT = 4;
    public static final int ALIGN_PARENT_TOP_LEFT = 7;
    public static final String FLOATING_CONTROLLER_PREFERENCES = "floating_controller";
    public static final String BAR_ALIGNMENT_PREF_KEY = "bar_alignment";

    private ViewGroup mMainView;
    private int mBarAlignment;
    private int mBarInitialAlignment;
    private int mOrientation;

    private final static String TAG = FcControlBar.class.getSimpleName();
    private final FcContext mFcContext;
    private RecyclerView mRecyclerView;
    private FcSapaServiceConnector mFcSapaConnector;
    private FcModel mModel;
    private ImageButton mBarHandler;
    private LinearLayout mRoot;
    private ImageButton mMainAppImage;
    private boolean mListHidden = true;
    private FcAnimator mFcAnimator;
    private float mDevicesActionLayoutWidth;
    private SapaAppInfo mSelectedApp;


    public FcControlBar(Context context) {
        this(context, null);
    }

    public FcControlBar(Context context, AttributeSet attrs) {
        this(context, attrs, 0);
    }

    public FcControlBar(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);

        mFcContext = new FcContext(context);
        mFcContext.setFxContextStateChangeListener(this);
        FcActionFactory factory = new FcActionFactory(mFcContext);
        mFcContext.setActionFactory(factory);

        initView();
    }

    private void initView() {
        LayoutInflater inflater = (LayoutInflater) getContext().getSystemService(
                Context.LAYOUT_INFLATER_SERVICE);
        inflater.inflate(R.layout.fc_control_bar, this, true);
        mRoot = (LinearLayout) findViewById(R.id.control_bar_root_layout);
        mRecyclerView = (RecyclerView)findViewById(R.id.devices_layout);

        FcItemAnimator itemAnimator = new FcItemAnimator(mRecyclerView);
        itemAnimator.setAddDuration(FcConstants.DEFAULT_ANIM_DURATION);
        itemAnimator.setChangeDuration(FcConstants.DEFAULT_ANIM_DURATION);
        itemAnimator.setMoveDuration(FcConstants.DEFAULT_ANIM_DURATION);
        itemAnimator.setRemoveDuration(FcConstants.DEFAULT_ANIM_DURATION);

        mRecyclerView.setItemAnimator(itemAnimator);
        mModel = new FcModel(mFcContext);
        mAdapter = new FcAdapter(mFcContext, mModel);

        //
        // Temporary workaround for a bug in RecyclerView:
        //      https://code.google.com/p/android/issues/detail?id=74772
        //
        // This custom LinearLayoutManager is on Apache 2.0 licence
        //
        // Fix this when it's fixed ("targeted early 2016")
        //
        LinearLayoutManager layoutManager = new LinearLayoutManager(getContext());
        layoutManager.setOrientation(LinearLayoutManager.HORIZONTAL);
        mRecyclerView.setLayoutManager(layoutManager);
        mRecyclerView.setAdapter(mAdapter);

        mBarHandler = (ImageButton)findViewById(R.id.barhandler);
        mBarHandler.setBackgroundResource(R.drawable.arrow_open);
        mBarHandler.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (!mListHidden) {
                    hide();
                } else {
                    expand();
                }
                mListHidden = !mListHidden;
            }
        });

        mMainAppImage = (ImageButton)findViewById(R.id.main_app_image);
        mMainAppImage.setOnLongClickListener(new FcControlBarLongClickListener());
        mFcAnimator = new FcAnimator();
    }

    private void expand() {
        mFcAnimator.createExpandAnimator(mRecyclerView, (int) mDevicesActionLayoutWidth, 300).start();
        mBarHandler.setActivated(true);
        mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
    }

    private void hide() {
        mFcAnimator.createCollapseAnimator(mRecyclerView, 300).start();
        mBarHandler.setActivated(false);
        mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
    }

    private void hideNoAnimation() {
        mBarHandler.setActivated(false);
        mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());

        mListHidden = true;

        mAdapter.hideExpanded();

        mRecyclerView.getLayoutParams().width = 0;
        mRecyclerView.setVisibility(View.GONE);
        mRecyclerView.requestLayout();
    }

    /**
     * @brief This method was c
     *@param root
     * @param array
     */
    public void prepareView(View root, TypedArray array) {
        View dragReceiver = root.findViewById(R.id.for_content);
        OnDragListener dragListener = new OnDragListener() {

            @Override
            public boolean onDrag(View v, DragEvent event) {
                if ((event.getLocalState() instanceof View)
                        && ((View) event.getLocalState()).getId() == v.getContext().hashCode()) {
                    if (DragEvent.ACTION_DROP == event.getAction()) {
                        float x = event.getX();
                        float y = event.getY();

                        int alignment = getBorder(x, y);
                        putBarOnBoard(alignment);
                        mRecyclerView.scrollToPosition(0);

                    } else if (DragEvent.ACTION_DRAG_ENDED == event.getAction() && !event.getResult()) {
                        putBarOnBoard(mBarAlignment);
                    }
                    return true;
                }
                return false;
            }
        };

        int barAlignment = array.getInt(R.styleable.FloatingControler_bar_alignment, ALIGN_PARENT_BOTTOM_RIGHT);
        mBarInitialAlignment = barAlignment;
        if (dragReceiver != null) {
            dragReceiver.setOnDragListener(dragListener);
        }

        root.setOnDragListener(dragListener);
        mMainView = (ViewGroup) root;
        mMainAppImage.setId(getContext().hashCode());
        mDevicesActionLayoutWidth = mFcContext.getDimension(R.dimen.floating_controller_actions_length);

        putBarOnBoard(barAlignment);
    }

    /**
     * @brief Method return bar alignment of Control Bar
     *
     * @return one of value:
     *      ALIGN_PARENT_BOTTOM_RIGHT,
     *      ALIGN_PARENT_BOTTOM_LEFT,
     *      ALIGN_PARENT_TOP_RIGHT,
     *      ALIGN_PARENT_TOP_LEFT
     */
    public int getBarAlignment() {
        return mBarAlignment;
    }

    /**
     * @brief This method load bar state
     *
     * @param floatingController FloatingController
     * @param barAlignment int
     */
    public void loadBarState(FloatingController floatingController, int barAlignment) {
        Log.d(TAG, "loadBarState(fc, align=" + barAlignment);
        putBarOnBoard(barAlignment);
    }

    /**
     * @brief TODO!
     *
     */
    public void stopConnections() {
        //TODO Implement method body
    }

    /**
     * @brief This method return state of floating controller
     *
     *
     * @return true - if bar is expanded, false - if bar is hidden
     */
    public boolean getBarExpanded() {
        return mListHidden;
    }

    /**
     * @brief TODO!
     *
     * @return
     */
    public Map<String, Boolean> getDevicesExpanded() {
        //TODO Implement method body
        return null;
    }

    /**
     * @brief TODO!
     *
     * @param sapaAppService
     * @param mainPackage
     */
    public void setSapaAppService(SapaAppService sapaAppService, String mainPackage) {
        Log.d(TAG, "setSapaAppService(" + mainPackage + ")");
        mFcSapaConnector = new FcSapaServiceConnector(mModel, sapaAppService, mainPackage);
        mFcContext.setSapaServiceConnector(mFcSapaConnector);
        setMainApp(mFcSapaConnector.getMainApp());
        SapaAppInfo sapaAppInfo = SapaAppInfo.getAppInfo(((Activity)getContext()).getIntent());
        if(sapaAppInfo != null) {
            setSelectedApp(sapaAppInfo);
        }
    }

    /**
     * @brief TODO
     *
     * @param mainApp
     */
    public void setMainApp(final SapaAppInfo mainApp) {
        if(mainApp == null) {
            Log.v(TAG, "Main app is null");
            return;
        }
        Log.d(TAG, "setMainApp(" + mainApp.getApp().getInstanceId() + ")");
        if(Looper.getMainLooper().getThread() != Thread.currentThread() ) {
            post(new Runnable() {
                @Override
                public void run() {
                    try {
                        Log.d(TAG, "set main app icon");
                        mMainAppImage.setImageDrawable(mainApp.getIcon(getContext()));
                    } catch (PackageManager.NameNotFoundException e) {
                        Log.e(TAG, "Exception", e);
                    }
                }
            });
        } else {
            try {
                Log.d(TAG, "set main app icon");
                mMainAppImage.setImageDrawable(mainApp.getIcon(getContext()));
            } catch (PackageManager.NameNotFoundException e) {
                Log.e(TAG, "Exception", e);
            }
        }
    }

    /**
     * @brief TODO
     *
     * @param appInfo
     */
    public void setSelectedApp(SapaAppInfo appInfo) {
        Log.d(TAG, "setSelectedApp(" + appInfo.getApp().getInstanceId() + ")");
        Log.d(TAG, "mMode:" + mModel);
        Log.d(TAG, "instanceId:" + appInfo.getApp().getInstanceId());
        Log.d(TAG, "mBarAlignment:" + mBarAlignment);
        if(mModel != null && mModel.getItemCount() > 0) {
            mModel.setActiveApp(appInfo);
        }
        mSelectedApp = appInfo;
        mFcContext.setActiveApp(mSelectedApp);
        Bundle bundle = appInfo.getConfiguration();
        if (bundle != null) {
            int barAlignment = bundle.getInt(BAR_ALIGNMENT_PREF_KEY, mBarAlignment);
            putBarOnBoard(barAlignment);
        }
        Log.d(TAG, "mBarAlignment:" + mBarAlignment);
    }

    public void onFloatingControllerDetached() {
        saveViewState();
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
    protected int getBorder(float x, float y) {
        float relativeX = x / ((float) mMainView.getWidth());
        float relativeY = y / ((float) mMainView.getHeight());

        if (relativeX < 0.5) {
            return (relativeY < 0.5) ? ALIGN_PARENT_TOP_LEFT : ALIGN_PARENT_BOTTOM_LEFT;
        } else {
            return (relativeY < 0.5) ? ALIGN_PARENT_TOP_RIGHT : ALIGN_PARENT_BOTTOM_RIGHT;
        }
    }

    private void putBarOnBoard(int alignment) {
        Log.d(TAG, "put on board alignment: " + alignment);

        RelativeLayout.LayoutParams params = prepareLayoutParams(alignment);
        if (getParent() != null) {
            mMainView.removeView(this);
        }

        mMainView.addView(this, params);
        mBarAlignment = alignment;
        mOrientation = HORIZONTAL;
    }

    /**
     * @brief This method is used when layout direction was changed from RTL to LTR or LTR to RTL
     *
     * @param alignment int
     * @return
     */
    protected RelativeLayout.LayoutParams prepareLayoutParams(int alignment) {
        RelativeLayout.LayoutParams params = (RelativeLayout.LayoutParams) this.getLayoutParams();
        params.removeRule(RelativeLayout.ALIGN_PARENT_TOP);
        params.removeRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
        params.removeRule(RelativeLayout.ALIGN_PARENT_LEFT);
        params.removeRule(RelativeLayout.ALIGN_PARENT_RIGHT);
        switch (alignment) {
            case ALIGN_PARENT_TOP_LEFT:
                params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
                changeLayoutDirection(LAYOUT_DIRECTION_RTL);
                break;
            case ALIGN_PARENT_TOP_RIGHT:
                params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                params.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
                changeLayoutDirection(LAYOUT_DIRECTION_LTR);
                break;
            case ALIGN_PARENT_BOTTOM_LEFT:
                params.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
                params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
                changeLayoutDirection(LAYOUT_DIRECTION_RTL);
                break;
            case ALIGN_PARENT_BOTTOM_RIGHT:
                params.addRule(RelativeLayout.ALIGN_PARENT_BOTTOM);
                params.addRule(RelativeLayout.ALIGN_PARENT_RIGHT);
                changeLayoutDirection(LAYOUT_DIRECTION_LTR);
                break;
            default:
                params.addRule(RelativeLayout.ALIGN_PARENT_TOP);
                params.addRule(RelativeLayout.ALIGN_PARENT_LEFT);
                changeLayoutDirection(LAYOUT_DIRECTION_RTL);
        }

        return params;
    }

    private void changeLayoutDirection(int layoutDirection) {
        setLayoutDirection(layoutDirection);
        mRoot.setLayoutDirection(layoutDirection);
        mRecyclerView.setLayoutDirection(layoutDirection);
        mModel.changeItemDirection(layoutDirection);
        mBarHandler.getBackground().setLevel(getOrientation() * 2 + getLayoutDirection());
    }

    public void reloadView() {
        Log.d(TAG, "reloadView()");

        Context context = getContext();
        if (null == context || !(context instanceof Activity)) {
            Log.w(TAG, "Cannot reload view: context needs to be an instance of activity");
            return;
        }
        mBarAlignment = mBarInitialAlignment;
        SapaAppInfo sapaAppInfo = SapaAppInfo.getAppInfo(((Activity) context).getIntent());
        Log.d(TAG, "    sapa app info:" + sapaAppInfo);
        if (sapaAppInfo != null) {
            hideNoAnimation();
            setSelectedApp(sapaAppInfo);
        }
    }

    @Override
    public void onActivityFinished() {
        saveViewState();
    }

    private void saveViewState() {
        Log.d(TAG, "saveViewState()");
        Log.d(TAG, "mBarAlignment:" + mBarAlignment);

        if(mSelectedApp != null) {
            Log.d(TAG, "instanceId:" + mSelectedApp.getApp().getInstanceId());
            Bundle bundle = mSelectedApp.getConfiguration();
            if(bundle == null) {
                bundle = new Bundle();
            }
            bundle.putInt(BAR_ALIGNMENT_PREF_KEY, mBarAlignment);
            mSelectedApp.setConfiguration(bundle);
            try {
                mFcSapaConnector.getSapaAppService().changeAppInfo(mSelectedApp);
            } catch (SapaConnectionNotSetException e) {
                Log.e(TAG, "Exception", e);
            }
        }
    }
}
