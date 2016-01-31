package com.samsung.android.sdk.professionalaudio.widgets;

import android.app.Activity;
import android.content.Context;
import android.content.res.TypedArray;
import android.util.AttributeSet;
import android.util.Log;
import android.view.LayoutInflater;
import android.widget.RelativeLayout;

import com.samsung.android.sdk.professionalaudio.app.SapaApp;
import com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
import com.samsung.android.sdk.professionalaudio.app.SapaAppService;
import com.samsung.android.sdk.professionalaudio.widgets.refactor.FcControlBar;

import java.util.Map;

/**
 * 
 * This class represents a control that exposes actions of all active devices on jam connection
 * service. To work correctly it shall be placed on whole screen in relative layout. It differs from
 * other views of similar functionality in a way that it shows four control bars on each side.
 * 
 * 
 */
public class FloatingController extends RelativeLayout {
    public static final String TAG = FloatingController.class.getSimpleName();
    private FcControlBar mControlBar;

    /**
     * This ctor is used mostly when FloatingControler is inflated from xml layout
     * 
     * @param context
     *            The context for which the view is being inflated. If it is not an Activity the
     *            exception may be thrown.
     * @param attrs
     *            Attributes given in xml declaration of FloatingControler
     */
    public FloatingController(Context context, AttributeSet attrs) {
        super(context, attrs);

        TypedArray array = context.getTheme().obtainStyledAttributes(attrs,
                R.styleable.FloatingControler, 0, 0);

        initializeControl(array);
        array.recycle();
    }

    /**
     * 
     * This method sets instance of SapaAppService of the host application, from which info about
     * apps and actions will be received.
     * 
     * @param sapaAppService
     *            An instance of SapaAppService
     */
    public void setSapaAppService(SapaAppService sapaAppService) {
        mControlBar.setSapaAppService(sapaAppService, getMainPackage());
    }

    /**
     * This method exposes information about which devices shall have their actions shown.
     * 
     * @return Map of device uid with information if it's bar shall be expanded.
     */
    public Map<String, Boolean> getDevicesExpanded() {
        return mControlBar.getDevicesExpanded();

    }

    /**
     * This method exposes information whether whole bar is expanded or not.
     * 
     * @return true if bar is expanded, false otherwise.
     */
    public boolean getBarExpanded() {
        return mControlBar.getBarExpanded();
    }

    /**
     * This method stops connection of var to connection service and stops checking whether list of
     * active devices was changed.
     */
    public void stopConnections() {
        mControlBar.stopConnections();
    }

    /**
     * This method sets layout of floating controller, creates control bar and puts it on its
     * default position.
     * 
     * @param barExpanded
     *            Whether or not control bar shall be expanded.
     * @param devicesExpanded
     *            Map describing which devices shall be expanded and which not.
     */
    private void initializeControl(TypedArray array) {

        int type = array.getInt(R.styleable.FloatingControler_type, 1);
        LayoutInflater inflater = (LayoutInflater) this.getContext()
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        int resId;
        switch (type) {
            case 0:
                resId = R.layout.floating_controller_layout;
                break;

            case 1:
            default: // fall-through
                resId = R.layout.floating_controller_layout;
                break;
        }
        inflater.inflate(resId, this, true);
        mControlBar = (FcControlBar) findViewById(R.id.control_bar_layout);
        mControlBar.prepareView(this, array);
    }

    /**
     * This method returns package of a single instance application which should be always visible
     * 
     * @return A package of single instance app which should always be visible in Controlbar
     */
    protected String getMainPackage() {
        return "com.sec.musicstudio";
    }

    public void loadBarState(int barAlignment) {
        mControlBar.loadBarState(this, barAlignment);
    }

    public int getBarAlignment() {
        return mControlBar.getBarAlignment();
    }

    public void reloadView() {
        mControlBar.reloadView();
    }

    @Override
    protected void onDetachedFromWindow() {
        super.onDetachedFromWindow();
        mControlBar.onFloatingControllerDetached();
    }
}
