namespace com.antonyt.infiniteviewpager
{

	using Color = android.graphics.Color;
	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using FragmentActivity = android.support.v4.app.FragmentActivity;
	using FragmentPagerAdapter = android.support.v4.app.FragmentPagerAdapter;
	using PagerAdapter = android.support.v4.view.PagerAdapter;
	using ViewPager = android.support.v4.view.ViewPager;

	/// <summary>
	/// Sample for <seealso cref="com.antonyt.infiniteviewpager.MinFragmentPagerAdapter"/> (support for less than
	/// 4 pages). Duplicate instances of pages will be created to fulfill the min case.
	/// </summary>
	public class InfiniteViewPager2Activity : FragmentActivity
	{

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.main;

			FragmentPagerAdapter adapter = new FragmentPagerAdapterAnonymousInnerClassHelper(this, SupportFragmentManager);

			// wrap pager to provide a minimum of 4 pages
			MinFragmentPagerAdapter wrappedMinAdapter = new MinFragmentPagerAdapter(SupportFragmentManager);
			wrappedMinAdapter.Adapter = adapter;

			// wrap pager to provide infinite paging with wrap-around
			PagerAdapter wrappedAdapter = new InfinitePagerAdapter(wrappedMinAdapter);

			// actually an InfiniteViewPager
			ViewPager viewPager = (ViewPager) findViewById(R.id.pager);
			viewPager.Adapter = wrappedAdapter;

		}

		private class FragmentPagerAdapterAnonymousInnerClassHelper : FragmentPagerAdapter
		{
			private readonly InfiniteViewPager2Activity outerInstance;

			public FragmentPagerAdapterAnonymousInnerClassHelper(InfiniteViewPager2Activity outerInstance, UnknownType getSupportFragmentManager) : base(getSupportFragmentManager)
			{
				this.outerInstance = outerInstance;
				colours = new int[]{Color.CYAN, Color.MAGENTA};
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