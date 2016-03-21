using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

	/// <summary>
	/// @brief Interface for model to communicate with Sapa API
	/// </summary>
	public interface FcSapaModel
	{

		/// <summary>
		/// @brief Register new application in the model
		/// 
		/// The application might be either main or ordinary, depending on the <code>mode</code>
		/// parameter.
		/// </summary>
		/// <param name="appInfo"> </param>
		/// <param name="mode"> </param>
		void insertApp(SapaAppInfo appInfo, int mode);

		/// <summary>
		/// @brief Unregister application from the model
		/// </summary>
		/// <param name="instanceId"> </param>
		/// <param name="mode"> </param>
		void removeApp(string instanceId, int mode);

		/// <summary>
		/// @brief Update application from the model
		/// </summary>
		/// <param name="appInfo"> </param>
		/// <param name="mode"> </param>
		void updateApp(SapaAppInfo appInfo, int mode);

		/// <summary>
		/// @brief Clear the model to the empty state
		/// 
		/// It might be called when main application is shut down
		/// </summary>
		void clear();

		/// <summary>
		/// Order applications to display them properly in FloatingController
		/// </summary>
		/// <param name="trackOrder"> applications order according to their position on tracks </param>
		void orderApps(IList<string> trackOrder);

		/// <summary>
		/// @brief Set given application as the active one
		/// 
		/// Active application is the one that displays floating controller
		/// </summary>
		/// <param name="appInfo"> </param>
		SapaAppInfo ActiveApp {set;}

		IList<string> FreezeApps {set;}
	}

}