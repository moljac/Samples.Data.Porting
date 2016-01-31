package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.widget.ImageButton;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.widgets.DrawableTool;
import com.samsung.android.sdk.professionalaudio.widgets.R;

import java.util.List;

/**
 * @brief ViewHolder representing main application (such as soundcamp)
 *
 * - device_buttons         list of buttons to open various components of main (host)
 *                          application (such as: mixer, multi-track)
 * - device_root_layout     main layout to be expanded when main button is clicked
 * - device_actions         actions of the applications
 * - device_volumes         volume actions (separated entries from device_actions)
 *
 * @see android.support.v7.widget.RecyclerView.ViewHolder
 */
public class MainAppViewHolder extends FcAdapter.BaseViewHolder {
    private static final String TAG = MainAppViewHolder.class.getSimpleName();

    LinearLayout mDeviceOpenActions;
    LinearLayout mDeviceLayout;
    LinearLayout mDeviceActions;
    LinearLayout mDeviceVolumes;

    public MainAppViewHolder(View itemView) {
        super(itemView);

        mDeviceLayout = (LinearLayout) itemView.findViewById(R.id.device_root_layout);
        mDeviceOpenActions = (LinearLayout) itemView.findViewById(R.id.device_buttons);
        mDeviceActions = (LinearLayout) itemView.findViewById(R.id.device_action_layout);
        mDeviceVolumes = (LinearLayout) itemView.findViewById(R.id.device_volumes_layout);
    }

    @Override
    public void prepareViewHolder(FcContext fcContext, FcAdapterModel model, FcModelItem item) {
        cleanLayouts();

        itemView.setVisibility(View.VISIBLE);

        mDeviceLayout.setLayoutDirection(item.getDirection());

        mDeviceActions.setLayoutDirection(item.getDirection());
        mDeviceActions.setVisibility(View.VISIBLE);
        mDeviceActions.setGravity(Gravity.CENTER);
        List<FcActionItem> callActions = item.getCallActions();
        prepareActionButtons(fcContext, callActions, mDeviceActions);

        mDeviceOpenActions.setLayoutDirection(item.getDirection());
        mDeviceOpenActions.setVisibility(View.VISIBLE);
        mDeviceOpenActions.setGravity(Gravity.CENTER);
        List<FcActionItem> returnActions = item.getReturnActions();
        prepareActionButtons(fcContext, returnActions, mDeviceOpenActions);

        if (item.hasVolumeActions()) {
            mDeviceVolumes.setLayoutDirection(item.getDirection());
            mDeviceVolumes.setVisibility(View.VISIBLE);
            mDeviceVolumes.setGravity(Gravity.CENTER);
            List<FcActionItem> volumeActions = item.getVolumeActions();
            prepareActionButtons(fcContext, volumeActions, mDeviceVolumes);
        }
    }

    @Override
    protected void cleanLayouts() {
        super.cleanLayouts();
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "cleanLayouts");
        }

        mDeviceOpenActions.removeAllViews();
        mDeviceActions.removeAllViews();
        mDeviceVolumes.removeAllViews();
    }

    private void prepareActionButtons(final FcContext context, List<FcActionItem> actions, LinearLayout parent) {
        int i = 0;
        for (final FcActionItem action : actions) {
            final ImageButton button = context.inflate(R.layout.fc_main_action_button, parent);

            button.setImageDrawable(action.getIcon(context));
            button.setEnabled(action.isEnabled());
            button.setVisibility(action.isVisible() ? View.VISIBLE : View.GONE);
            button.setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    action.getActionRunnable().run();
                }
            });
            int index = i;
            if (parent.getLayoutDirection() == View.LAYOUT_DIRECTION_RTL) {
                index = actions.size() - index - 1;
            }
            DrawableTool.setBackground(button, index, actions.size(), context);

            parent.addView(button);
            i++;
        }
    }
}
