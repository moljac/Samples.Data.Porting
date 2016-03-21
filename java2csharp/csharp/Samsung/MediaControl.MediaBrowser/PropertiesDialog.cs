using System.Collections.Generic;

namespace com.samsung.android.sdk.sample.mediabrowser
{


	using AlertDialog = android.app.AlertDialog;
	using Dialog = android.app.Dialog;
	using DialogFragment = android.app.DialogFragment;
	using Bundle = android.os.Bundle;
	using ViewGroup = android.view.ViewGroup;
	using TableLayout = android.widget.TableLayout;
	using TableRow = android.widget.TableRow;
	using TextView = android.widget.TextView;

	/// <summary>
	/// This class is a dialog which displays file properties.
	/// </summary>
	public class PropertiesDialog : DialogFragment
	{

		private IDictionary<string, string> props;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") @Override public void onCreate(android.os.Bundle savedInstanceState)
		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			Bundle b = Arguments;
			props = (IDictionary<string, string>) b.getSerializable("props");
		}

		public override Dialog onCreateDialog(Bundle savedInstanceState)
		{
			TableLayout l = new TableLayout(Activity);
			TableRow tr;
			TextView label, value;
			foreach (KeyValuePair<string, string> e in props.SetOfKeyValuePairs())
			{
				tr = new TableRow(Activity);
				label = new TextView(Activity);
				value = new TextView(Activity);
				label.Text = e.Key;
				value.Text = e.Value;
				label.setPadding(10, 10, 10, 10);
				value.setPadding(10, 10, 10, 10);
				tr.addView(label);
				tr.addView(value);
				l.addView(tr, new TableLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
			}
			return (new AlertDialog.Builder(Activity)).setView(l).setPositiveButton(android.R.@string.ok, null).setTitle(R.@string.properties).create();
		}
	}

}