namespace com.antonyt.infiniteviewpager
{

	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using FragmentActivity = android.support.v4.app.FragmentActivity;
	using FragmentPagerAdapter = android.support.v4.app.FragmentPagerAdapter;
	using PagerAdapter = android.support.v4.view.PagerAdapter;
	using ViewPager = android.support.v4.view.ViewPager;

	public class InfiniteViewPagerActivity : FragmentActivity
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.main;

			PagerAdapter adapter = new FragmentPagerAdapterAnonymousInnerClassHelper(this, SupportFragmentManager);

			// wrap pager to provide infinite paging with wrap-around
			PagerAdapter wrappedAdapter = new InfinitePagerAdapter(adapter);

			// actually an InfiniteViewPager
			ViewPager viewPager = (ViewPager) findViewById(R.id.pager);
			viewPager.Adapter = wrappedAdapter;

		}

		private class FragmentPagerAdapterAnonymousInnerClassHelper : FragmentPagerAdapter
		{
			private readonly InfiniteViewPagerActivity outerInstance;

			public FragmentPagerAdapterAnonymousInnerClassHelper(InfiniteViewPagerActivity outerInstance, UnknownType getSupportFragmentManager) : base(getSupportFragmentManager)
			{
				this.outerInstance = outerInstance;
				colours = new int[]{Color.CYAN, Color.GRAY, Color.MAGENTA, Color.LTGRAY, Color.GREEN, Color.WHITE, Color.YELLOW};
			}

			internal int[] colours;

			public override int Count
			{
				get
				{
					return colours.length;
				}
			}

			public override Fragment getItem(int position)
			{
				Fragment fragment = new ColourFragment();
				Bundle args = new Bundle();
				args.putInt("colour", colours[position]);
				args.putInt("identifier", position);
				fragment.Arguments = args;
				return fragment;
			}
		}
	}
}