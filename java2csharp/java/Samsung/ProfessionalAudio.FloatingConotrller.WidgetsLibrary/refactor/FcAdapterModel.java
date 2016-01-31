package com.samsung.android.sdk.professionalaudio.widgets.refactor;

/**
 * @brief Interface for model to communicate with {@link FcAdapter} class.
 */
public interface FcAdapterModel {

    /**
     * @brief Return item structure at the given position
     *
     * @param position position in the adapter

     * @return structure describing single item
     */
    FcModelItem getItem(int position);

    /**
     * @brief Return number of items in the model
     *
     * Used by the adapter to determine number of elements in the model
     *
     * @return  number of elements in the model
     */
    int getItemCount();

    /**
     * @brief Return type of the item in the model under given position
     *
     * @param position
     * @return
     */
    int getItemType(int position);

    /**
     * @brief Get number of the instance.
     *
     * If multi instance is enabled, instances of the same application might be decorated with
     * instance number, when more than one instance is active at the same time. This method
     * implies that item at the given position supports multi instance feature.
     *
     * Make sure to validate presence of multiInstance first by calling
     * {@link #isMultiInstance(FcModelItem)} method.
     *
     * Number is taken from the instance id. In current implementation of the SAPA each instance
     * of the same application got a name pattern such as: commonprefix^n.
     *
     * @param item  item describing single application (precondition: multi instance enabled)
     *
     * @return number of the instance
     */
    int getInstanceNumber(FcModelItem item);

    /**
     * @brief Check whether item at given position is multi instance or not
     *
     * It might be used to display number indicator
     *
     * @param item  item to check
     *
     * @return  true if item at given position is multi instance
     */
    boolean isMultiInstance(FcModelItem item);

    /**
     * @brief Set object to be notified when model is changed
     *
     * @param listener  Object to be notified when model is changed (might be null)
     */
    void setFcModelChangedListener(FcModelChangedListener listener);

    /**
     * @brief Set the direction of application layouts to given direction (same as Control Bar's)
     *
     * @param layoutDirection new direction for application layouts
     */
    void changeItemDirection(int layoutDirection);

    /**
     *
     * @return
     */
    FcModelItem getExpandedItem();

    /**
     *
     * @param item
     * @return
     */
    int getItemPosition(FcModelItem item);

    /**
     * @brief Check if app instance is on freeze track
     * @param instanceId instance of application
     * @return boolean
     */
    boolean isAppFreeze(String instanceId);

    /**
     * @brief Listener to be notified when model has been changed
     *
     * Note: It could be wise to add new method to notify that only the subset of the model
     * has been changed.
     */
    interface FcModelChangedListener {

        /**
         * @brief Called when whole model has been changed
         */
        void onFcModelChanged();

        /**
         * @brief Called when single model item has been changed
         *
         * @param index
         */
        void onFcModelChanged(int index);

        /**
         * @brief
         *
         * @param index
         */
        void onFcModelItemInserted(int index);

        /**
         * @brief
         *
         * @param index
         */
        void onFcModelItemRemoved(int index);
    }

}
