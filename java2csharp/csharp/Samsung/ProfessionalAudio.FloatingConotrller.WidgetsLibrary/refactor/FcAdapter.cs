namespace com.samsung.android.sdk.professionalaudio.widgets.refactor
{

	using Animator = android.animation.Animator;
	using AnimatorSet = android.animation.AnimatorSet;
	using Context = android.content.Context;
	using RecyclerView = android.support.v7.widget.RecyclerView;
	using Layout = android.text.Layout;
	using Log = android.util.Log;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using LinearLayout = android.widget.LinearLayout;


	/// <summary>
	/// @brief Adapter for recycler view to hold info necessary to display SAPA app info in
	///        a floating controller bar
	/// 
	/// Layouts:
	///  - fc_item_ordinal.xml:      ordinal application (instrument, effect)
	///  - fc_item_main.xml          actions of main application (such as: soundcamp)
	/// </summary>
	public class FcAdapter : RecyclerView.Adapter<FcAdapter.BaseViewHolder>, FcAdapterModel_FcModelChangedListener
	{

		private static readonly string TAG = typeof(FcAdapter).Name;
		private const int ADDITIONAL_ITEMS_COUNT = 1;
		private const float FULL_COLOR = 1f;

		private readonly FcContext mContext;
		private FcAdapterModel mModel;

		/// <summary>
		/// @brief Constructor of the adapter for applications to be displayed in floating
		///        controller bar
		/// </summary>
		/// <param name="fcContext"> FloatingController internals </param>
		/// <param name="model">     Database of the applications to be wrapped by this adapter </param>
		public FcAdapter(FcContext fcContext, FcAdapterModel model)
		{
			mContext = fcContext;
			mModel = model;
			mModel.FcModelChangedListener = this;
			HasStableIds = true;
		}

		//
		// FcModelChangedListener interface
		//

		public virtual void onFcModelChanged()
		{
			mContext.runOnMainThread(() =>
			{
				Log.d(TAG, "onFcModelChanged()");
				notifyDataSetChanged();
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onFcModelChanged(final int index)
		public virtual void onFcModelChanged(int index)
		{
			mContext.runOnMainThread(() =>
			{
				Log.d(TAG, "onFcModelChanged(" + index + ")");
				notifyItemChanged(index);
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onFcModelItemInserted(final int index)
		public virtual void onFcModelItemInserted(int index)
		{
			mContext.runOnMainThread(() =>
			{
				Log.d(TAG, "onFcModelItemInserted(" + index + ")");
				// Invalidating whole model to recalculate number icons
				notifyDataSetChanged();
			});
		}

//JAVA TO C# CONVERTER WARNING: 'final' parameters are not available in .NET:
//ORIGINAL LINE: @Override public void onFcModelItemRemoved(final int index)
		public virtual void onFcModelItemRemoved(int index)
		{
			mContext.runOnMainThread(() =>
			{
				Log.d(TAG, "onFcModelItemRemoved(" + index + ")");
				// Invalidating whole model to recalculate number icons
				notifyDataSetChanged();
			});
		}

		//
		// RecyclerView.Adapter base class
		//

		public override BaseViewHolder onCreateViewHolder(ViewGroup parent, int type)
		{
			if (FcConstants.OPT_DETAILED_LOGS)
			{
				Log.d(TAG, "onCreateViewHolder: type=" + type);
			}
			BaseViewHolder holder = null;
			switch (type)
			{
				case FcConstants.APP_TYPE_MAIN:
				{
					holder = createMainAppHolder(parent);
				}
				break;

				case FcConstants.APP_TYPE_ORDINAL:
				{
					holder = createOrdinalAppHolder(parent);
				}
				break;

				case FcConstants.APP_TYPE_SPACER:
				{
					holder = createSpacerHolder(parent);
				}
				break;
			}

			return holder;
		}

		public override void onBindViewHolder(BaseViewHolder viewHolder, int position)
		{
			lock (mModel)
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "onBindViewHolder: position=" + position);
				}
				FcModelItem item = (mModel.ItemCount > position) ? mModel.getItem(position) : null;
				viewHolder.prepareViewHolder(mContext, mModel, item);
			}
		}

		public override int ItemCount
		{
			get
			{
				return mModel.ItemCount + ADDITIONAL_ITEMS_COUNT;
			}
		}

		public override long getItemId(int position)
		{
			if (position >= mModel.ItemCount)
			{
				int overflowPos = position - mModel.ItemCount;
				return long.MaxValue - overflowPos;
			}

			FcModelItem item = mModel.getItem(position);
			return item.InstanceId.GetHashCode();
		}

		public override int getItemViewType(int position)
		{
			lock (mModel)
			{
				// Adapter got one more item for a padding
				if (position >= mModel.ItemCount)
				{
					return FcConstants.APP_TYPE_SPACER;
				}
				return mModel.getItemType(position);
			}
		}

		/// <summary>
		/// @brief Called when RecyclerView needs a new RecyclerView.ViewHolder of "ordinal application"
		/// type to represent an item.
		/// 
		/// Ordinal application means instrument or effect.
		/// </summary>
		/// <param name="parent"> The ViewGroup into which the new View will be added after it is bound to
		///               an adapter position
		/// </param>
		/// <returns> A new ViewHolder that holds a View of "ordinal application" type </returns>
		private BaseViewHolder createOrdinalAppHolder(ViewGroup parent)
		{

			LayoutInflater inflater = (LayoutInflater) parent.Context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);

			View view = inflater.inflate(R.layout.fc_item_ordinal, parent, false);
			return new OrdinalAppViewHolder(mContext, view, this);
		}

		/// <summary>
		/// @brief Called when RecyclerView needs a new RecyclerView.ViewHolder of "main application"
		/// type to represent an item.
		/// 
		/// Main application means the host (such as: soundcamp)
		/// </summary>
		/// <param name="parent"> The ViewGroup into which the new View will be added after it is bound to
		///               an adapter position
		/// </param>
		/// <returns> A new ViewHolder that holds a View of "main application" type </returns>
		private BaseViewHolder createMainAppHolder(ViewGroup parent)
		{
			LayoutInflater inflater = (LayoutInflater) parent.Context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);

			View view = inflater.inflate(R.layout.fc_item_main, parent, false);
			return new MainAppViewHolder(view);
		}

		/// <summary>
		/// @brief
		/// </summary>
		public virtual void hideExpanded()
		{
			lock (this)
			{
				lock (mModel)
				{
					Log.d(TAG, "hideExpanded");
					for (int i = 0; i < mModel.ItemCount; ++i)
					{
						FcModelItem item = mModel.getItem(i);
						item.Expanded = false;
						// Do not notify
					}
				}
			}
		}

		/// <summary>
		/// @brief
		/// </summary>
		/// <param name="item"> </param>
		public virtual void notifyItemChanged(FcModelItem item)
		{
			lock (this)
			{
				lock (mModel)
				{
					int position = mModel.getItemPosition(item);
					Log.d(TAG, "notifyItemChanged(" + item + ") position = " + position);
					if (position < 0)
					{
						Log.w(TAG, "The item " + item.InstanceId + " cannot be found in the model");
						return;
					}
        
					notifyItemChanged(position);
				}
			}
		}

		private BaseViewHolder createSpacerHolder(ViewGroup parent)
		{

			LayoutInflater inflater = (LayoutInflater) parent.Context.getSystemService(Context.LAYOUT_INFLATER_SERVICE);

			View view = inflater.inflate(R.layout.fc_item_spacer, parent, false);
			return new SpacerViewHolder(view);
		}

		public virtual FcModelItem ExpandedItem
		{
			get
			{
				lock (this)
				{
					return mModel.ExpandedItem;
				}
			}
		}

		public virtual int getItemPosition(FcModelItem item)
		{
			return mModel.getItemPosition(item);
		}

		/// <summary>
		/// @brief TODO
		/// </summary>
		public abstract class BaseViewHolder : RecyclerView.ViewHolder
		{

			public BaseViewHolder(View itemView) : base(itemView)
			{
			}

			/// <summary>
			/// @brief TODO
			/// </summary>
			/// <param name="context"> Context </param>
			/// <param name="item"> FcModelItem </param>
			public abstract void prepareViewHolder(FcContext context, FcAdapterModel model, FcModelItem item);

			protected internal virtual void cleanLayouts()
			{
				if (FcConstants.OPT_DETAILED_LOGS)
				{
					Log.d(TAG, "cleanLayouts");
				}
				itemView.Alpha = FULL_COLOR;
				itemView.TranslationX = 0;
				itemView.TranslationY = 0;
				itemView.ScaleX = 1f;
				itemView.ScaleY = 1f;
			}
		}

		public class SpacerViewHolder : BaseViewHolder
		{

			public SpacerViewHolder(View itemView) : base(itemView)
			{
			}

			public override void prepareViewHolder(FcContext context, FcAdapterModel model, FcModelItem item)
			{
				// NOOP
			}
		}
	}

}