package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.animation.Animator;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.graphics.drawable.Drawable;
import android.graphics.drawable.LayerDrawable;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;
import android.widget.ImageButton;
import android.widget.LinearLayout;
import android.widget.TextView;
import android.widget.Toast;

import com.samsung.android.sdk.professionalaudio.widgets.DrawableTool;
import com.samsung.android.sdk.professionalaudio.widgets.R;

import java.util.List;

/**
 * @brief ViewHolder representing ordinal application (instrument, effect)
 *
 * - device_app_button      main button of the application to expand/collapse actions
 * - device_root_layout     main layout to be expanded when main button is clicked
 * - device_open_actions    list of buttons to open activities of the application (effect,
 *                          instrument)
 * - device_actions         actions of the applications
 * - device_volumes         volume up/down actions if present
 *
 * @see android.support.v7.widget.RecyclerView.ViewHolder
 */
public class OrdinalAppViewHolder extends FcAdapter.BaseViewHolder {
    private static final String TAG = OrdinalAppViewHolder.class.getSimpleName();

    private final AppClickedListener mExpandActionsButtonListener;
    private final View.OnClickListener mOpenAppDirectlyButtonListener;

    ImageButton mDeviceAppButton;
    LinearLayout mDeviceRootLayout;
    LinearLayout mDeviceOpenActions;
    LinearLayout mDeviceActions;

    LinearLayout mDeviceVolumeActions;
    LinearLayout mDeviceVolumes;
    TextView mDeviceVolumesLabel;

    private FcModelItem mFcModelItem;
    private boolean mIsFreeze = false;
    private Toast mToast = null;

    public OrdinalAppViewHolder(final FcContext context, View itemView, FcAdapter adapter) {
        super(itemView);

        mDeviceAppButton = (ImageButton) itemView.findViewById(R.id.device_app_button);
        mDeviceRootLayout = (LinearLayout) itemView.findViewById(R.id.device_root_layout);
        mDeviceOpenActions = (LinearLayout) itemView.findViewById(R.id.device_open_actions);
        mDeviceActions = (LinearLayout) itemView.findViewById(R.id.device_actions);

        mDeviceVolumeActions = (LinearLayout) itemView.findViewById(R.id.device_volume_actions);
        mDeviceVolumes = (LinearLayout) itemView.findViewById(R.id.device_volumes);
        mDeviceVolumesLabel = (TextView) itemView.findViewById(R.id.device_volume_label);

        final FcAnimator animator = new FcAnimator();
        mExpandActionsButtonListener = new AppClickedListener(adapter);
        mOpenAppDirectlyButtonListener = new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                Log.d(TAG, "ordinal app view holder: device app clicked, opening activity directly");
                if (!mIsFreeze) {
                    // This should be called only if there is a single return action and no more
                    // other actions.
                    final List<FcActionItem> item = mFcModelItem.getReturnActions();
                    if (item.size() == 1) {
                        Animator a = animator.createScaleAnimator(mDeviceAppButton,
                                FcConstants.DEFAULT_SCALE_ANIM_DURATION, 0.3f, 0.2f);

                        a.addListener(new FcAnimator.FcAnimatorListener() {
                            @Override
                            public void onAnimationEnd(Animator animation) {
                                context.runOnMainThread(item.get(0).getActionRunnable());
                            }
                        });

                        a.start();

                    } else {
                        Log.w(TAG, "Return actions size should be equal to 1!");
                    }
                } else {
                    displayToast(mDeviceAppButton, R.string.frozen_app_text);
                }
            }
        };

        mDeviceAppButton.setOnClickListener(mExpandActionsButtonListener);
    }

    private void displayToast(View v, int resId) {
        if (mToast == null) {
            mToast = Toast.makeText(v.getContext(), resId, Toast.LENGTH_SHORT);
        } else {
            mToast.setDuration(Toast.LENGTH_SHORT);
            mToast.setText(resId);
        }
        mToast.show();
    }

    @Override
    public void prepareViewHolder(FcContext fcContext, FcAdapterModel model, FcModelItem item) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "prepareViewHolder(" + item + ")");
        }
        Context context = fcContext.getContext();
        cleanLayouts();

        mFcModelItem = item;
        mExpandActionsButtonListener.setItem(mFcModelItem);

        if (item.isExpanded()) {
            mDeviceRootLayout.setVisibility(View.VISIBLE);
            mDeviceRootLayout.getLayoutParams().width = ViewGroup.LayoutParams.WRAP_CONTENT;
        } else {
            mDeviceRootLayout.setVisibility(View.VISIBLE);
            mDeviceRootLayout.getLayoutParams().width = 0;
        }
        mDeviceRootLayout.requestLayout();

        // Setting up device_app_button
        mDeviceAppButton.setBackground(getBackground(fcContext, item.isActive()));
        LinearLayout.LayoutParams params = (LinearLayout.LayoutParams) mDeviceAppButton.getLayoutParams();
        params.gravity = Gravity.CENTER;

        int padding = context.getResources().getDimensionPixelSize(R.dimen.floating_controller_active_app_frame_stroke_width) + context.getResources().getDimensionPixelSize(R.dimen.floating_controller_ordinal_app_layout_distance_between_app_icon_and_active_indication_frame);

        Drawable deviceAppDrawable = mFcModelItem.getIcon();
        if (model.isMultiInstance(item)) {
            int number = model.getInstanceNumber(item);
            int iconSize = context.getResources()
                    .getDimensionPixelSize(R.dimen.ord_app_expand_action_button_width);
            deviceAppDrawable = DrawableTool.getDrawableWithNumber(
                    deviceAppDrawable, number, iconSize, context);
        }

        mIsFreeze = model.isAppFreeze(item.getInstanceId());

        if (mIsFreeze) {
            deviceAppDrawable = getFreezeDrawable(fcContext.getContext(), deviceAppDrawable);
        }

        int actionCount = item.getCallActions().size();
        actionCount += item.getVolumeActions().size();
        actionCount += item.getReturnActions().size();

        if (actionCount > 1) {
            mDeviceAppButton.setOnClickListener(mExpandActionsButtonListener);
        } else {
            mDeviceAppButton.setOnClickListener(mOpenAppDirectlyButtonListener);
        }

        mDeviceAppButton.setPadding(padding, padding, padding, padding);
        mDeviceAppButton.setImageDrawable(deviceAppDrawable);

        // Open Actions
        final List<FcActionItem> returnActions = item.getReturnActions();
        int numberOfElements = returnActions.size();

        for (int i = 0; i < numberOfElements; i++) {
            addActionButton(mDeviceOpenActions, returnActions.get(i),
                    R.layout.fc_ordinal_open_app_button, i,
                    numberOfElements, fcContext, false);
        }

        if (numberOfElements > 0) {
            mDeviceOpenActions.setVisibility(View.VISIBLE);
        } else {
            mDeviceOpenActions.setVisibility(View.GONE);
        }

        // Call Actions
        final List<FcActionItem> callActions = item.getCallActions();
        numberOfElements = callActions.size();
        String bypassOn = fcContext.getContext().getResources().getString(R.string.bypass_on);
        String bypassOff = fcContext.getContext().getResources().getString(R.string.bypass_off);

        for (int i = 0; i < numberOfElements; i++) {
            String itemId = callActions.get(i).getId();
            if(itemId.equals(bypassOff)) {
                addBypassAction(fcContext, mDeviceActions, callActions.get(i), R.string.bypass_text_off);
            } else if(itemId.equals(bypassOn)) {
                addBypassAction(fcContext, mDeviceActions, callActions.get(i), R.string.bypass_text_on);
            } else {
                addActionButton(mDeviceActions, callActions.get(i), R.layout.fc_ordinal_action_button, i, numberOfElements, fcContext, true);
            }
        }

        if (numberOfElements > 0) {
            mDeviceActions.setVisibility(View.VISIBLE);
        } else {
            mDeviceActions.setVisibility(View.GONE);
        }

        // Volume Actions
        if (item.hasVolumeActions()) {
            final List<FcActionItem> volumeActions = item.getVolumeActions();
            numberOfElements = volumeActions.size();

            // handle volume label anomaly
            if (item.getDirection() == LinearLayout.LAYOUT_DIRECTION_LTR) {
                for (int i = 0; i < numberOfElements; i++) {
                    addActionButton(mDeviceVolumes, volumeActions.get(i),
                            R.layout.fc_ordinal_action_button, i,
                            numberOfElements, fcContext, true);
                }
            } else {
                for (int i = numberOfElements - 1; i >= 0; i--) {
                    addActionButton(mDeviceVolumes, volumeActions.get(i),
                            R.layout.fc_ordinal_action_button, numberOfElements - 1 - i,
                            numberOfElements, fcContext, true);
                }
            }
            // this handles the requirement that volume label is always before volume actions in a layout
            float marginSize = fcContext.getDimensionPixelSize(R.dimen.ord_app_action_volume_layout_margin_ltr);
            if (item.getDirection() == LinearLayout.LAYOUT_DIRECTION_RTL) {
                marginSize = fcContext.getDimensionPixelSize(R.dimen.ord_app_action_volume_layout_margin_rtl);
            }
            reverseLayoutWithMargins(mDeviceVolumeActions, item.getDirection(), marginSize);

            mDeviceVolumeActions.setVisibility(View.VISIBLE);
        } else {
            mDeviceVolumeActions.setVisibility(View.GONE);
        }
    }

    private void reverseLayoutWithMargins(LinearLayout layout, int origDirection, float margin) {
        LinearLayout.LayoutParams deviceVolumeActionsParams =
                (LinearLayout.LayoutParams) layout.getLayoutParams();
        if (origDirection == LinearLayout.LAYOUT_DIRECTION_RTL) {
            layout.setLayoutDirection(LinearLayout.LAYOUT_DIRECTION_LTR);
            deviceVolumeActionsParams.setMarginStart(0);
            deviceVolumeActionsParams.setMarginEnd((int) margin);
        } else {
            deviceVolumeActionsParams.setMarginStart((int) margin);
            deviceVolumeActionsParams.setMarginEnd(0);
        }
    }

    private Drawable getBackground(FcContext fcContext, boolean active) {
        return active ? fcContext.getDrawable(R.drawable.floating_controller_active_app_frame) :
                new ColorDrawable(Color.parseColor("#00ffffff"));
    }

    private void addBypassAction(FcContext fcContext, LinearLayout linearLayout, final FcActionItem fcActionItem, int buttonText) {
        Button bypassButton =
                (Button) LayoutInflater.from(fcContext.getContext()).inflate(R.layout.bypass_action_button, mDeviceActions, false);

//        bypassButton.setBackground(fcActionItem.getIcon(fcContext));
        bypassButton.setEnabled(fcActionItem.isEnabled());
        bypassButton.setVisibility(fcActionItem.isVisible() ? View.VISIBLE : View.GONE);
        bypassButton.setText(fcContext.getContext().getResources().getString(buttonText));
        bypassButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                fcActionItem.getActionRunnable().run();
            }
        });

        DrawableTool.setBackground(bypassButton, 0, 1, fcContext);
        linearLayout.addView(bypassButton);
    }

    private void addActionButton(final LinearLayout linearLayout, final FcActionItem fcActionItem,
                                 final int resId, final int index, final int numberOfElements,
                                 final FcContext fcContext, final boolean adjustBackgroundToPositionInLayout) {
        ImageButton imageButton =
                (ImageButton) LayoutInflater.from(fcContext.getContext()).inflate(resId, mDeviceActions, false);
        imageButton.setImageDrawable(fcActionItem.getIcon(fcContext));
        imageButton.setEnabled(fcActionItem.isEnabled());
        imageButton.setVisibility(fcActionItem.isVisible() ? View.VISIBLE : View.GONE);
        imageButton.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                fcActionItem.getActionRunnable().run();
            }
        });

        if (adjustBackgroundToPositionInLayout) {
            DrawableTool.setBackground(imageButton, index, numberOfElements, fcContext);
        }

        linearLayout.addView(imageButton);
    }

    @Override
    protected void cleanLayouts() {
        super.cleanLayouts();

        // First, remove views from layouts
        mDeviceOpenActions.removeAllViews();
        mDeviceActions.removeAllViews();
        mDeviceVolumes.removeAllViews();

        // Main layout could be in expanded/collapsed state
        mDeviceRootLayout.setVisibility(View.GONE);
        mDeviceRootLayout.setAlpha(1.0f);
        mDeviceRootLayout.getLayoutParams().width = ViewGroup.LayoutParams.WRAP_CONTENT;

        // Just to be sure, button should have the valid scale
        mDeviceAppButton.setScaleX(1.0f);
        mDeviceAppButton.setScaleY(1.0f);
        mDeviceAppButton.setEnabled(true);

        // Hide action button layouts
        mDeviceOpenActions.setVisibility(View.GONE);
        mDeviceActions.setVisibility(View.GONE);
        mDeviceVolumeActions.setVisibility(View.GONE);
    }

    public Drawable getFreezeDrawable(Context context, Drawable baseDrawable) {
        Drawable[] drawables = new Drawable[]{
                baseDrawable,
                new ColorDrawable(Color.parseColor("#55000000")),
                context.getResources().getDrawable(R.drawable.freeze_multitrack_icon)
        };
        return new LayerDrawable(drawables);
    }

    public void setScrollFocusFinished() {
        mFcModelItem.setScrollFocus(true);
    }

    public boolean needsScrollFocus() {
        return mFcModelItem.isScrollFocus();
    }

    public void setAppButtonEnabled(boolean enabled) {
        mDeviceAppButton.setEnabled(enabled);
    }


    /**
     *
     */
    private class AppClickedListener implements View.OnClickListener {

        private final FcAdapter mAdapter;
        private FcModelItem mItem;

        /**
         * @brief
         *
         * @param adapter
         */
        public AppClickedListener(FcAdapter adapter) {
            mAdapter = adapter;
        }

        /**
         * @brief
         *
         * @param item
         */
        public void setItem(FcModelItem item) {
            mItem = item;
        }

        @Override
        public void onClick(View v) {
            FcModelItem curItem = mItem;

            if (null == curItem) {
                Log.w(TAG, "Clicked on an app without bound item");
                return;
            }

            if (mIsFreeze) {
                displayToast(v, R.string.frozen_app_text);
                return;
            }

            Log.d(TAG, "onClick for app " + mItem.getInstanceId());

            synchronized (curItem) {
                if (curItem.isExpanded()) {
                    curItem.setExpanded(false);
                    mAdapter.notifyItemChanged(curItem);
                    return;
                }
            }

            boolean invalidateAll = false;
            int previousIndex = -1;
            synchronized (curItem) {
                FcModelItem expandedItem = mAdapter.getExpandedItem();
                if (null != expandedItem) {
                    expandedItem.setExpanded(false);
                    invalidateAll = true;
                    previousIndex = mAdapter.getItemPosition(expandedItem);
                }
                curItem.setExpanded(true);
                curItem.setScrollFocus(true);
            }

            if (invalidateAll) {
                final int nextIndex = mAdapter.getItemPosition(curItem);
                final int lowerIndex = Math.min(previousIndex, nextIndex);
                final int higherIndex = Math.max(previousIndex, nextIndex);
                mAdapter.notifyItemRangeChanged(lowerIndex, higherIndex - lowerIndex + 1);
            } else {
                mAdapter.notifyItemChanged(curItem);
            }
        }
    }

}
