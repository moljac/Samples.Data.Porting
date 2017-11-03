using System.Collections.Generic;

namespace com.stripe.example.adapter
{

	using RecyclerView = android.support.v7.widget.RecyclerView;
	using LayoutInflater = android.view.LayoutInflater;
	using ViewGroup = android.view.ViewGroup;
	using LinearLayout = android.widget.LinearLayout;
	using TextView = android.widget.TextView;

	using Source = com.stripe.android.model.Source;


	/// <summary>
	/// A simple <seealso cref="RecyclerView"/> implementation to hold our data.
	/// </summary>
	public class RedirectAdapter : RecyclerView.Adapter<RedirectAdapter.ViewHolder>
	{
		private IList<ViewModel> mDataset = new List<ViewModel>();

		// Provide a reference to the views for each data item
		// Complex data items may need more than one view per item, and
		// you provide access to all the views for a data item in a view holder
		internal class ViewHolder : RecyclerView.ViewHolder
		{
			// each data item is just a string in this case
			internal TextView mFinalStatusView;
			internal TextView mRedirectStatusView;
			internal TextView mSourceIdView;
			internal TextView mSourceTypeView;
			internal ViewHolder(LinearLayout pollingLayout) : base(pollingLayout)
			{
				mFinalStatusView = (TextView) pollingLayout.findViewById(R.id.tv_ending_status);
				mRedirectStatusView = (TextView) pollingLayout.findViewById(R.id.tv_redirect_status);
				mSourceIdView = (TextView) pollingLayout.findViewById(R.id.tv_source_id);
				mSourceTypeView = (TextView) pollingLayout.findViewById(R.id.tv_source_type);
			}

			public virtual string FinalStatus
			{
				set
				{
					mFinalStatusView.Text = value;
				}
			}

			public virtual string SourceId
			{
				set
				{
					string last6 = value == null || value.Length < 6 ? value : value.Substring(value.Length - 6);
					mSourceIdView.Text = last6;
				}
			}

			public virtual string SourceType
			{
				set
				{
					string viewableType = value;
					if (Source.THREE_D_SECURE.Equals(value))
					{
						viewableType = "3DS";
					}
					mSourceTypeView.Text = viewableType;
				}
			}

			public virtual string RedirectStatus
			{
				set
				{
					mRedirectStatusView.Text = value;
				}
			}
		}

		public class ViewModel
		{
			public readonly string mFinalStatus;
			public readonly string mRedirectStatus;
			public readonly string mSourceId;
			public readonly string mSourceType;

			public ViewModel(string finalStatus, string redirectStatus, string sourceId, string sourceType)
			{
				mFinalStatus = finalStatus;
				mRedirectStatus = redirectStatus;
				mSourceId = sourceId;
				mSourceType = sourceType;
			}
		}

		// Provide a suitable constructor (depends on the kind of dataset)
		public RedirectAdapter()
		{
		}

		// Create new views (invoked by the layout manager)
		public override RedirectAdapter.ViewHolder onCreateViewHolder(ViewGroup parent, int viewType)
		{
			// create a new view

			LinearLayout pollingView = (LinearLayout) LayoutInflater.from(parent.Context).inflate(R.layout.polling_list_item, parent, false);

			//
			ViewHolder vh = new ViewHolder(pollingView);
			return vh;
		}

		// Replace the contents of a view (invoked by the layout manager)
		public override void onBindViewHolder(ViewHolder holder, int position)
		{
			// - get element from your dataset at this position
			// - replace the contents of the view with that element
			ViewModel model = mDataset[position];
			holder.FinalStatus = model.mFinalStatus;
			holder.RedirectStatus = model.mRedirectStatus;
			holder.SourceId = model.mSourceId;
			holder.SourceType = model.mSourceType;
		}

		// Return the size of your dataset (invoked by the layout manager)
		public override int ItemCount
		{
			get
			{
				return mDataset.Count;
			}
		}

		public virtual void addItem(string finalStatus, string redirectStatus, string sourceId, string sourceType)
		{
			mDataset.Add(new ViewModel(finalStatus, redirectStatus, sourceId, sourceType));
			notifyDataSetChanged();
		}
	}

}