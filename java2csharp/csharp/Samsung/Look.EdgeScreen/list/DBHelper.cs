using System.Collections.Generic;

namespace com.example.edgescreen.list
{

	public class DBHelper
	{
		private List<string> mDatalist = new List<string>();

		private static DBHelper sInstance = new DBHelper();

		private object mLock = new object();

		public static DBHelper Instance
		{
			get
			{
				return sInstance;
			}
		}

		public virtual void addData(string data)
		{
			lock (mLock)
			{
				mDatalist.Add(data);
			}
		}

		public virtual string getData(int index)
		{
			lock (mLock)
			{
				try
				{
					return mDatalist[index];
				}
				catch (System.IndexOutOfRangeException)
				{
					return null;
				}
			}
		}

		public virtual int Size
		{
			get
			{
				lock (mLock)
				{
					return mDatalist.Count;
				}
			}
		}

		public virtual void clearData()
		{
			lock (mLock)
			{
				mDatalist.Clear();
			}
		}
	}

}