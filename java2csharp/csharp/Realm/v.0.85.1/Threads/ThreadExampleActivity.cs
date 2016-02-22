/*
 * Copyright 2014 Realm Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace io.realm.examples.threads
{

	using Bundle = android.os.Bundle;
	using Fragment = android.support.v4.app.Fragment;
	using FragmentManager = android.support.v4.app.FragmentManager;
	using FragmentPagerAdapter = android.support.v4.app.FragmentPagerAdapter;
	using FragmentTransaction = android.support.v4.app.FragmentTransaction;
	using ViewPager = android.support.v4.view.ViewPager;
	using ActionBar = android.support.v7.app.ActionBar;
	using ActionBarActivity = android.support.v7.app.ActionBarActivity;


	public class ThreadExampleActivity : ActionBarActivity, ActionBar.TabListener
	{

		private ViewPager viewPager;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			// Set up the action bar.
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final android.support.v7.app.ActionBar actionBar = getSupportActionBar();
			ActionBar actionBar = SupportActionBar;
			actionBar.NavigationMode = ActionBar.NAVIGATION_MODE_TABS;

			SectionsPagerAdapter pageAdapter = new SectionsPagerAdapter(this, SupportFragmentManager);
			viewPager = (ViewPager) findViewById(R.id.pager);
			viewPager.Adapter = pageAdapter;

			viewPager.OnPageChangeListener = new SimpleOnPageChangeListenerAnonymousInnerClassHelper(this, actionBar);

			for (int i = 0; i < pageAdapter.Count; i++)
			{
				SupportActionBar.addTab(SupportActionBar.newTab().setText(pageAdapter.getPageTitle(i)).setTabListener(this));
			}
		}

		private class SimpleOnPageChangeListenerAnonymousInnerClassHelper : ViewPager.SimpleOnPageChangeListener
		{
			private readonly ThreadExampleActivity outerInstance;

			private ActionBar actionBar;

			public SimpleOnPageChangeListenerAnonymousInnerClassHelper(ThreadExampleActivity outerInstance, ActionBar actionBar)
			{
				this.outerInstance = outerInstance;
				this.actionBar = actionBar;
			}

			public override void onPageSelected(int position)
			{
				actionBar.SelectedNavigationItem = position;
			}
		}

		public override void onTabSelected(ActionBar.Tab tab, FragmentTransaction fragmentTransaction)
		{
			viewPager.CurrentItem = tab.Position;
		}

		public override void onTabUnselected(ActionBar.Tab tab, FragmentTransaction fragmentTransaction)
		{

		}

		public override void onTabReselected(ActionBar.Tab tab, FragmentTransaction fragmentTransaction)
		{

		}


		public class SectionsPagerAdapter : FragmentPagerAdapter
		{
			private readonly ThreadExampleActivity outerInstance;


			public SectionsPagerAdapter(ThreadExampleActivity outerInstance, FragmentManager fm) : base(fm)
			{
				this.outerInstance = outerInstance;
			}

			public override Fragment getItem(int position)
			{
				switch (position)
				{
					case 0:
						return new ThreadFragment();
					case 1:
						return new AsyncTaskFragment();
					case 2:
						return new AsyncQueryFragment();
					default:
						return null;
				}
			}

			public override int Count
			{
				get
				{
					return 3;
				}
			}

			public override CharSequence getPageTitle(int position)
			{
				Locale l = Locale.Default;
				switch (position)
				{
					case 0:
						return getString(R.@string.title_section1).toUpperCase(l);
					case 1:
						return getString(R.@string.title_section2).toUpperCase(l);
					case 2:
						return getString(R.@string.title_section3).toUpperCase(l);
					default:
						return null;
				}
			}
		}
	}
}