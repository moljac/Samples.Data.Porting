using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{

	using Activity = android.app.Activity;
	using Context = android.content.Context;
	using TypedArray = android.content.res.TypedArray;
	using AttributeSet = android.util.AttributeSet;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using RelativeLayout = android.widget.RelativeLayout;

	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;
	using SapaAppService = com.samsung.android.sdk.professionalaudio.app.SapaAppService;
	using FcControlBar = com.samsung.android.sdk.professionalaudio.widgets.refactor.FcControlBar;

	/// 
	/// <summary>
	/// This class represents a control that exposes actions of all active devices on jam connection
	/// service. To work correctly it shall be placed on whole screen in relative layout. It differs from
	/// other views of similar functionality in a way that it shows four control bars on each side.
	/// 
	/// 
	/// </summary>
	public class FloatingController : RelativeLayout
	{
		public static readonly string TAG = typeof(FloatingController).Name;
		private FcControlBar mControlBar;

		/// <summary>
		/// This ctor is used mostly when FloatingControler is inflated from xml layout
		/// </summary>
		/// <param name="context">
		///            The context for which the view is being inflated. If it is not an Activity the
		///            exception may be thrown. </param>
		/// <param name="attrs">
		///            Attributes given in xml declaration of FloatingControler </param>
		public FloatingController(Context context, AttributeSet attrs) : base(context, attrs)
		{

			TypedArray array = context.Theme.obtainStyledAttributes(attrs, R.styleable.FloatingControler, 0, 0);

			initializeControl(array);
			array.recycle();
		}

		/// 
		/// <summary>
		/// This method sets instance of SapaAppService of the host application, from which info about
		/// apps and actions will be received.
		/// </summary>
		/// <param name="sapaAppService">
		///            An instance of SapaAppService </param>
		public virtual SapaAppService SapaAppService
		{
			set
			{
				mControlBar.setSapaAppService(value, MainPackage);
			}
		}

		/// <summary>
		/// This method exposes information about which devices shall have their actions shown.
		/// </summary>
		/// <returns> Map of device uid with information if it's bar shall be expanded. </returns>
		public virtual IDictionary<string, bool?> DevicesExpanded
		{
			get
			{
				return mControlBar.DevicesExpanded;
    
			}
		}

		/// <summary>
		/// This method exposes information whether whole bar is expanded or not.
		/// </summary>
		/// <returns> true if bar is expanded, false otherwise. </returns>
		public virtual bool BarExpanded
		{
			get
			{
				return mControlBar.BarExpanded;
			}
		}

		/// <summary>
		/// This method stops connection of var to connection service and stops checking whether list of
		/// active devices was changed.
		/// </summary>
		public virtual void stopConnections()
		{
			mControlBar.stopConnections();
		}

		/// <summary>
		/// This method sets layout of floating controller, creates control bar and puts it on its
		/// default position.
		/// </summary>
		/// <param name="barExpanded">
		///            Whether or not control bar shall be expanded. </param>
		/// <param name="devicesExpanded">
		///            Map describing which devices shall be expanded and which not. </param>
		private void initializeControl(TypedArray array)
		{

			int type = array.getInt(R.styleable.FloatingControler_type, 1);
			LayoutInflater inflater = (LayoutInflater) this.Context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);
			int resId;
			switch (type)
			{
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

		/// <summary>
		/// This method returns package of a single instance application which should be always visible
		/// </summary>
		/// <returns> A package of single instance app which should always be visible in Controlbar </returns>
		protected internal virtual string MainPackage
		{
			get
			{
				return "com.sec.musicstudio";
			}
		}

		public virtual void loadBarState(int barAlignment)
		{
			mControlBar.loadBarState(this, barAlignment);
		}

		public virtual int BarAlignment
		{
			get
			{
				return mControlBar.BarAlignment;
			}
		}

		public virtual void reloadView()
		{
			mControlBar.reloadView();
		}

		protected internal override void onDetachedFromWindow()
		{
			base.onDetachedFromWindow();
			mControlBar.onFloatingControllerDetached();
		}
	}

}