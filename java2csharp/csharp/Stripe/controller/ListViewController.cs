using System.Collections.Generic;

namespace com.stripe.example.controller
{

	using Context = android.content.Context;
	using NonNull = android.support.annotation.NonNull;
	using ListView = android.widget.ListView;
	using SimpleAdapter = android.widget.SimpleAdapter;

	using Token = com.stripe.android.model.Token;


	/// <summary>
	/// A controller for the <seealso cref="ListView"/> used to display the results.
	/// </summary>
	public class ListViewController
	{

		private SimpleAdapter mAdatper;
		private IList<IDictionary<string, string>> mCardTokens = new List<IDictionary<string, string>>();
		private Context mContext;

		public ListViewController(ListView listView)
		{
			mContext = listView.Context;
			mAdatper = new SimpleAdapter(mContext, mCardTokens, R.layout.list_item_layout, new string[]{"last4", "tokenId"}, new int[]{R.id.last4, R.id.tokenId});
			listView.Adapter = mAdatper;
		}

		internal virtual void addToList(Token token)
		{
			addToList(token.Card.Last4, token.Id);
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: public void addToList(@NonNull String last4, @NonNull String tokenId)
		public virtual void addToList(string last4, string tokenId)
		{
			string endingIn = mContext.getString(R.@string.endingIn);
			IDictionary<string, string> map = new Dictionary<string, string>();
			map["last4"] = endingIn + " " + last4;
			map["tokenId"] = tokenId;
			mCardTokens.Add(map);
			mAdatper.notifyDataSetChanged();
		}
	}

}