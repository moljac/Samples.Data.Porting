package com.samsung.android.sdk.professionalaudio.widgets.refactor;

import android.animation.Animator;
import android.animation.AnimatorSet;
import android.content.Context;
import android.support.v7.widget.RecyclerView;
import android.text.Layout;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.LinearLayout;

import com.samsung.android.sdk.professionalaudio.widgets.R;

import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/**
 * @brief Adapter for recycler view to hold info necessary to display SAPA app info in
 *        a floating controller bar
 *
 * Layouts:
 *  - fc_item_ordinal.xml:      ordinal application (instrument, effect)
 *  - fc_item_main.xml          actions of main application (such as: soundcamp)
 */
public class FcAdapter extends RecyclerView.Adapter<FcAdapter.BaseViewHolder>
        implements FcAdapterModel.FcModelChangedListener {

    private static final String TAG = FcAdapter.class.getSimpleName();
    private static final int ADDITIONAL_ITEMS_COUNT = 1;
    private static final float FULL_COLOR = 1f;

    private final FcContext mContext;
    private FcAdapterModel mModel;

    /**
     * @brief Constructor of the adapter for applications to be displayed in floating
     *        controller bar
     *
     * @param fcContext FloatingController internals
     * @param model     Database of the applications to be wrapped by this adapter
     */
    public FcAdapter(FcContext fcContext, FcAdapterModel model) {
        mContext = fcContext;
        mModel = model;
        mModel.setFcModelChangedListener(this);
        setHasStableIds(true);
    }

    //
    // FcModelChangedListener interface
    //

    @Override
    public void onFcModelChanged() {
        mContext.runOnMainThread(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "onFcModelChanged()");
                notifyDataSetChanged();
            }
        });
    }

    @Override
    public void onFcModelChanged(final int index) {
        mContext.runOnMainThread(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "onFcModelChanged(" + index + ")");
                notifyItemChanged(index);
            }
        });
    }

    @Override
    public void onFcModelItemInserted(final int index) {
        mContext.runOnMainThread(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "onFcModelItemInserted(" + index + ")");
                // Invalidating whole model to recalculate number icons
                notifyDataSetChanged();
            }
        });
    }

    @Override
    public void onFcModelItemRemoved(final int index) {
        mContext.runOnMainThread(new Runnable() {
            @Override
            public void run() {
                Log.d(TAG, "onFcModelItemRemoved(" + index + ")");
                // Invalidating whole model to recalculate number icons
                notifyDataSetChanged();
            }
        });
    }

    //
    // RecyclerView.Adapter base class
    //

    @Override
    public BaseViewHolder onCreateViewHolder(ViewGroup parent, int type) {
        if (FcConstants.OPT_DETAILED_LOGS) {
            Log.d(TAG, "onCreateViewHolder: type=" + type);
        }
        BaseViewHolder holder = null;
        switch (type) {
            case FcConstants.APP_TYPE_MAIN: {
                holder = createMainAppHolder(parent);
            } break;

            case FcConstants.APP_TYPE_ORDINAL: {
                holder = createOrdinalAppHolder(parent);
            } break;

            case FcConstants.APP_TYPE_SPACER: {
                holder = createSpacerHolder(parent);
            } break;
        }

        return holder;
    }

    @Override
    public void onBindViewHolder(BaseViewHolder viewHolder, int position) {
        synchronized (mModel) {
            if (FcConstants.OPT_DETAILED_LOGS) {
                Log.d(TAG, "onBindViewHolder: position=" + position);
            }
            FcModelItem item = (mModel.getItemCount() > position)
                    ? mModel.getItem(position)
                    : null;
            viewHolder.prepareViewHolder(mContext, mModel, item);
        }
    }

    @Override
    public int getItemCount() {
        return mModel.getItemCount() + ADDITIONAL_ITEMS_COUNT;
    }

    @Override
    public long getItemId(int position) {
        if (position >= mModel.getItemCount()) {
            int overflowPos = position - mModel.getItemCount();
            return Long.MAX_VALUE - overflowPos;
        }

        FcModelItem item = mModel.getItem(position);
        return item.getInstanceId().hashCode();
    }

    @Override
    public int getItemViewType(int position) {
        synchronized (mModel) {
            // Adapter got one more item for a padding
            if (position >= mModel.getItemCount()) {
                return FcConstants.APP_TYPE_SPACER;
            }
            return mModel.getItemType(position);
        }
    }

    /**
     * @brief Called when RecyclerView needs a new RecyclerView.ViewHolder of "ordinal application"
     * type to represent an item.
     *
     * Ordinal application means instrument or effect.
     *
     * @param parent The ViewGroup into which the new View will be added after it is bound to
     *               an adapter position
     *
     * @return A new ViewHolder that holds a View of "ordinal application" type
     */
    private BaseViewHolder createOrdinalAppHolder(ViewGroup parent) {

        LayoutInflater inflater = (LayoutInflater) parent.getContext().getSystemService(
                Context.LAYOUT_INFLATER_SERVICE);

        View view = inflater.inflate(R.layout.fc_item_ordinal, parent, false);
        return new OrdinalAppViewHolder(mContext, view, this);
    }

    /**
     * @brief Called when RecyclerView needs a new RecyclerView.ViewHolder of "main application"
     * type to represent an item.
     *
     * Main application means the host (such as: soundcamp)
     *
     * @param parent The ViewGroup into which the new View will be added after it is bound to
     *               an adapter position
     *
     * @return A new ViewHolder that holds a View of "main application" type
     */
    private BaseViewHolder createMainAppHolder(ViewGroup parent) {
        LayoutInflater inflater = (LayoutInflater) parent.getContext().getSystemService(
                Context.LAYOUT_INFLATER_SERVICE);

        View view = inflater.inflate(R.layout.fc_item_main, parent, false);
        return new MainAppViewHolder(view);
    }

    /**
     * @brief
     */
    public synchronized void hideExpanded() {
        synchronized (mModel) {
            Log.d(TAG, "hideExpanded");
            for (int i = 0; i < mModel.getItemCount(); ++i) {
                FcModelItem item = mModel.getItem(i);
                item.setExpanded(false);
                // Do not notify
            }
        }
    }

    /**
     * @brief
     *
     * @param item
     */
    public synchronized void notifyItemChanged(FcModelItem item) {
        synchronized (mModel) {
            int position = mModel.getItemPosition(item);
            Log.d(TAG, "notifyItemChanged(" + item + ") position = " + position);
            if (position < 0) {
                Log.w(TAG, "The item " + item.getInstanceId() + " cannot be found in the model");
                return;
            }

            notifyItemChanged(position);
        }
    }

    private BaseViewHolder createSpacerHolder(ViewGroup parent) {

        LayoutInflater inflater = (LayoutInflater) parent.getContext().getSystemService(
                Context.LAYOUT_INFLATER_SERVICE);

        View view = inflater.inflate(R.layout.fc_item_spacer, parent, false);
        return new SpacerViewHolder(view);
    }

    public synchronized FcModelItem getExpandedItem() {
        return mModel.getExpandedItem();
    }

    public int getItemPosition(FcModelItem item) {
        return mModel.getItemPosition(item);
    }

    /**
     * @brief TODO
     */
    public static abstract class BaseViewHolder  extends RecyclerView.ViewHolder {

        public BaseViewHolder(View itemView) {
            super(itemView);
        }

        /**
         * @brief TODO
         *
         * @param context Context
         * @param item FcModelItem
         */
        public abstract void prepareViewHolder(FcContext context, FcAdapterModel model,
                                               FcModelItem item);

        protected void cleanLayouts() {
            if (FcConstants.OPT_DETAILED_LOGS) {
                Log.d(TAG, "cleanLayouts");
            }
            itemView.setAlpha(FULL_COLOR);
            itemView.setTranslationX(0);
            itemView.setTranslationY(0);
            itemView.setScaleX(1f);
            itemView.setScaleY(1f);
        }
    }

    public static class SpacerViewHolder extends BaseViewHolder {

        public SpacerViewHolder(View itemView) {
            super(itemView);
        }

        @Override
        public void prepareViewHolder(FcContext context, FcAdapterModel model, FcModelItem item) {
            // NOOP
        }
    }
}
