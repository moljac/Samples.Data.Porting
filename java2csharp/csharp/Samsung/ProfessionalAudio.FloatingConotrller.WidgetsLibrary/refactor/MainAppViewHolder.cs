using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Log = android.util.Log;
	using Gravity = android.view.Gravity;
	using View = android.view.View;
	using ImageButton = android.widget.ImageButton;
	using LinearLayout = android.widget.LinearLayout;


	/// <summary>
	/// @brief ViewHolder representing main application (such as soundcamp)
	/// 
	/// - device_buttons         list of buttons to open various components of main (host)
	///                          application (such as: mixer, multi-track)
	/// - device_root_layout     main layout to be expanded when main button is clicked
	/// - device_actions         actions of the applications
	/// - device_volumes         volume actions (separated entries from device_actions)
	/// </summary>
	/// <seealso cref= android.support.v7.widget.RecyclerView.ViewHolder </seealso>
	public class MainAppViewHolder : FcAdapter.BaseViewHolder
	{
		private static readonly string TAG = typeof(MainAppViewHolder).Name;

		internal LinearLayout mDeviceOpenActions;
		internal LinearLayout mDeviceLayout;
		internal LinearLayout mDeviceActions;
		internal LinearLayout mDeviceVolumes;

		public MainAppViewHolder(View itemView) : base(itemView)
		{

			mDeviceLayout = (LinearLayout) itemView.findViewById(R.id.device_root_layout);
			mDeviceOpenActions = (LinearLayout) itemView.findViewById(R.id.device_buttons);
			mDeviceActions = (LinearLayout) itemView.findViewById(R.id.device_action_layout);
			mDeviceVolumes = (LinearLayout) itemView.findViewById(R.id.device_volumes_layout);
		}

		public override void prepareViewHolder(FcContext fcContext, FcAdapterModel model, FcModelItem item)
		{
			cleanLayouts();

			itemView.Visibility = View.VISIBLE;

			mDeviceLayout.LayoutDirection = item.Direction;

			mDeviceActions.LayoutDirection = item.Direction;
			mDeviceActions.Visibility = View.VISIBLE;
			mDeviceActions.Gravity = Gravity.CENTER;
			IList<FcActionItem> callActions = item.CallActions;
			prepareActionButtons(fcContext, callActions, mDeviceActions);

			mDeviceOpenActions.LayoutDirection = item.Direction;
			mDeviceOpenActions.Visibility = View.VISIBLE;
			mDeviceOpenActions.Gravity = Gravity.CENTER;
			IList<FcActionItem> returnActions = item.ReturnActions;
			prepareActionButtons(fcContext, returnActions, mDeviceOpenActions);

			if (item.hasVolumeActions())
			{
				mDeviceVolumes.LayoutDirection = item.Direction;
				mDeviceVolumes.Visibility = View.VISIBLE;
				mDeviceVolumes.Gravity = Gravity.CENTER;
				IList<FcActionItem> volumeActions = item.VolumeActions;
				prepareActionButtons(fcContext, volumeActions, mDeviceVolumes);
			}
		}

		protected internal override void cleanLayouts()
		{
			base.cleanLayouts();
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "cleanLayouts");
			}

			mDeviceOpenActions.removeAllViews();
			mDeviceActions.removeAllViews();
			mDeviceVolumes.removeAllViews();
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: private void prepareActionButtons(final FcContext context, java.util.List<FcActionItem> actions, android.widget.LinearLayout parent)
		private void prepareActionButtons(FcContext context, IList<FcActionItem> actions, LinearLayout parent)
		{
			int i = 0;
			foreach (FcActionItem action in actions)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.widget.ImageButton button = context.inflate(com.samsung.android.sdk.professionalaudio.widgets.R.layout.fc_main_action_button, parent);
				ImageButton button = context.inflate(R.layout.fc_main_action_button, parent);

				button.ImageDrawable = action.getIcon(context);
				button.Enabled = action.Enabled;
				button.Visibility = action.Visible ? View.VISIBLE : View.GONE;
				button.OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
				int index = i;
				if (parent.LayoutDirection == View.LAYOUT_DIRECTION_RTL)
				{
					index = actions.Count - index - 1;
				}
				DrawableTool.setBackground(button, index, actions.Count, context);

				parent.addView(button);
				i++;
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly MainAppViewHolder outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(MainAppViewHolder outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public override void onClick(View view)
			{
				action.ActionRunnable.run();
			}
		}
	}

}