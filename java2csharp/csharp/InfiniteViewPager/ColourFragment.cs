namespace com.antonyt.infiniteviewpager
{

	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using Gravity = android.view.Gravity;
	using LayoutInflater = android.view.LayoutInflater;
	using View = android.view.View;
	using ViewGroup = android.view.ViewGroup;
	using TextView = android.widget.TextView;

	/// <summary>
	/// Example Fragment class that shows an identifier inside a TextView.
	/// </summary>
	public class ColourFragment : Fragment
	{

		private int identifier;
		private int colour;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			Bundle args = Arguments;
			colour = args.getInt("colour");
			identifier = args.getInt("identifier");
		}

		public override View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			TextView v = new TextView(Activity);
			v.Gravity = Gravity.CENTER;
			v.TextSize = 40;
			v.TextColor = Color.BLACK;
			v.BackgroundColor = colour;
			v.Text = "Fragment ID: " + identifier;
			return v;
		}

		public override void onSaveInstanceState(Bundle outState)
		{
			base.onSaveInstanceState(outState);
			outState.putBoolean("dummy", true);
		}
	}

}