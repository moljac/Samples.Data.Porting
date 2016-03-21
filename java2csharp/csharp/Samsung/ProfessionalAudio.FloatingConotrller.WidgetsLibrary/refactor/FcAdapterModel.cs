namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	/// <summary>
	/// @brief Interface for model to communicate with <seealso cref="FcAdapter"/> class.
	/// </summary>
	public interface FcAdapterModel
	{

		/// <summary>
		/// @brief Return item structure at the given position
		/// </summary>
		/// <param name="position"> position in the adapter
		/// </param>
		/// <returns> structure describing single item </returns>
		FcModelItem getItem(int position);

		/// <summary>
		/// @brief Return number of items in the model
		/// 
		/// Used by the adapter to determine number of elements in the model
		/// </summary>
		/// <returns>  number of elements in the model </returns>
		int ItemCount {get;}

		/// <summary>
		/// @brief Return type of the item in the model under given position
		/// </summary>
		/// <param name="position">
		/// @return </param>
		int getItemType(int position);

		/// <summary>
		/// @brief Get number of the instance.
		/// 
		/// If multi instance is enabled, instances of the same application might be decorated with
		/// instance number, when more than one instance is active at the same time. This method
		/// implies that item at the given position supports multi instance feature.
		/// 
		/// Make sure to validate presence of multiInstance first by calling
		/// <seealso cref="#isMultiInstance(FcModelItem)"/> method.
		/// 
		/// Number is taken from the instance id. In current implementation of the SAPA each instance
		/// of the same application got a name pattern such as: commonprefix^n.
		/// </summary>
		/// <param name="item">  item describing single application (precondition: multi instance enabled)
		/// </param>
		/// <returns> number of the instance </returns>
		int getInstanceNumber(FcModelItem item);

		/// <summary>
		/// @brief Check whether item at given position is multi instance or not
		/// 
		/// It might be used to display number indicator
		/// </summary>
		/// <param name="item">  item to check
		/// </param>
		/// <returns>  true if item at given position is multi instance </returns>
		bool isMultiInstance(FcModelItem item);

		/// <summary>
		/// @brief Set object to be notified when model is changed
		/// </summary>
		/// <param name="listener">  Object to be notified when model is changed (might be null) </param>
		FcAdapterModel_FcModelChangedListener FcModelChangedListener {set;}

		/// <summary>
		/// @brief Set the direction of application layouts to given direction (same as Control Bar's)
		/// </summary>
		/// <param name="layoutDirection"> new direction for application layouts </param>
		void changeItemDirection(int layoutDirection);

		/// 
		/// <summary>
		/// @return
		/// </summary>
		FcModelItem ExpandedItem {get;}

		/// 
		/// <param name="item">
		/// @return </param>
		int getItemPosition(FcModelItem item);

		/// <summary>
		/// @brief Check if app instance is on freeze track </summary>
		/// <param name="instanceId"> instance of application </param>
		/// <returns> boolean </returns>
		bool isAppFreeze(string instanceId);

		/// <summary>
		/// @brief Listener to be notified when model has been changed
		/// 
		/// Note: It could be wise to add new method to notify that only the subset of the model
		/// has been changed.
		/// </summary>

	}

	public interface FcAdapterModel_FcModelChangedListener
	{

		/// <summary>
		/// @brief Called when whole model has been changed
		/// </summary>
		void onFcModelChanged();

		/// <summary>
		/// @brief Called when single model item has been changed
		/// </summary>
		/// <param name="index"> </param>
		void onFcModelChanged(int index);

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="index"> </param>
		void onFcModelItemInserted(int index);

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="index"> </param>
		void onFcModelItemRemoved(int index);
	}

}