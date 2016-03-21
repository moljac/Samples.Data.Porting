using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using ViewGroup = android.view.ViewGroup;

	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;

	internal interface IFloatingControlerHandler
	{

		int LayoutResource {get;}

		void initLayout(ViewGroup layout);

		void setSapaAppService(SapaAppService sapaAppService, string mainPackagename);

		void stopConnections();

		bool BarExpanded {get;}

		IDictionary<string, bool?> DevicesExpanded {get;}

		int BarAlignment {get;}

		void loadBarState(FloatingController floatingController, int barAlignment);
	}

}