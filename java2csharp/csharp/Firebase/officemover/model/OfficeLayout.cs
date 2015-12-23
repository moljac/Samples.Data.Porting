using System.Collections.Generic;

namespace com.firebase.officemover.model
{


	/// <summary>
	/// @author Jenny Tong (mimming)
	/// </summary>
	public class OfficeLayout : Dictionary<string, OfficeThing>
	{

		public virtual int HighestzIndex
		{
			get
			{
				if (this.Count == 0)
				{
					return 0;
				}
				else
				{
					int runningHighest = 0;
					foreach (OfficeThing thing in this.Values)
					{
						if (thing.getzIndex() > runningHighest)
						{
							runningHighest = thing.getzIndex();
						}
					}
					return runningHighest;
				}
			}
		}
		public virtual IList<OfficeThing> ThingsTopDown
		{
			get
			{
				IList<OfficeThing> things = new List<OfficeThing>(this.Values);
				things.Sort(new ComparatorAnonymousInnerClassHelper(this));
				return things;
			}
		}

		private class ComparatorAnonymousInnerClassHelper : IComparer<OfficeThing>
		{
			private readonly OfficeLayout outerInstance;

			public ComparatorAnonymousInnerClassHelper(OfficeLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int Compare(OfficeThing lhs, OfficeThing rhs)
			{
				return rhs.getzIndex() - lhs.getzIndex();
			}
		}
		public virtual IList<OfficeThing> ThingsBottomUp
		{
			get
			{
				IList<OfficeThing> things = new List<OfficeThing>(this.Values);
				things.Sort(new ComparatorAnonymousInnerClassHelper2(this));
				return things;
			}
		}

		private class ComparatorAnonymousInnerClassHelper2 : IComparer<OfficeThing>
		{
			private readonly OfficeLayout outerInstance;

			public ComparatorAnonymousInnerClassHelper2(OfficeLayout outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual int Compare(OfficeThing lhs, OfficeThing rhs)
			{
				return lhs.getzIndex() - rhs.getzIndex();
			}
		}
	}

}