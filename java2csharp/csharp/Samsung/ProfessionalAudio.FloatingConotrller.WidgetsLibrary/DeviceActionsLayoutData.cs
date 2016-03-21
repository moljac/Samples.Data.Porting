using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.widgets
{


	using Context = android.content.Context;
	using NameNotFoundException = android.content.pm.PackageManager.NameNotFoundException;
	using Drawable = android.graphics.drawable.Drawable;
	using SparseArray = android.util.SparseArray;

	using SapaActionInfo = com.samsung.android.sdk.professionalaudio.app.SapaActionInfo;
	using SapaApp = com.samsung.android.sdk.professionalaudio.app.SapaApp;
	using SapaAppInfo = com.samsung.android.sdk.professionalaudio.app.SapaAppInfo;

	internal class DeviceActionsLayoutData
	{
		internal Drawable mInstanceIcon;
		internal SapaApp mSapaApp;
		internal string mInstancePackageName;
		internal SparseArray<SapaActionInfo> mActionList;
		internal bool mIsMultiInstanceEnabled;
		internal IDictionary<string, SapaActionInfo> mActionMap;
		internal SapaAppInfo mAppInfo;
		internal int number = -1;

		internal DeviceActionsLayoutData()
		{
			mIsMultiInstanceEnabled = false;
		}

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in .NET:
//ORIGINAL LINE: DeviceActionsLayoutData(com.samsung.android.sdk.professionalaudio.app.SapaAppInfo info, android.content.Context context, boolean isMultiInstance) throws android.content.pm.PackageManager.NameNotFoundException
		internal DeviceActionsLayoutData(SapaAppInfo info, Context context, bool isMultiInstance)
		{
			mAppInfo = info;
			this.mInstanceIcon = info.getIcon(context);
			this.mSapaApp = info.App;
			this.mInstancePackageName = info.PackageName;
			this.mActionList = info.Actions;
			if (this.mActionList == null)
			{
				this.mActionList = new SparseArray<SapaActionInfo>();
			}
			mActionMap = new Dictionary<string, SapaActionInfo>();
			loadActionMap();
			this.mIsMultiInstanceEnabled = info.MultiInstanceEnabled;
			if (this.mIsMultiInstanceEnabled && isMultiInstance)
			{
				char c = this.mSapaApp.InstanceId.charAt(this.mSapaApp.InstanceId.length() - 1);
				number = char.digit(c, 10);
				if (number > 0)
				{
					mInstanceIcon = DrawableTool.getDefaultDrawableWithNumber(mInstanceIcon, number, context);
				}
			}
		}

		internal virtual void loadActionMap()
		{
			mActionMap.Clear();
			for (int i = mActionList.size() - 1; i >= 0; --i)
			{
				SapaActionInfo action = mActionList.valueAt(i);
				mActionMap[action.Id] = action;
			}
		}

		public virtual SparseArray<SapaActionInfo> Actions
		{
			set
			{
				this.mActionList = value;
				loadActionMap();
			}
		}

		public override bool Equals(object o)
		{
			if (o == null)
			{
				return false;
			}
			if (this == o)
			{
				return true;
			}
			if (o is DeviceActionsLayoutData)
			{
				DeviceActionsLayoutData obj = (DeviceActionsLayoutData) o;
				return (mInstancePackageName.Equals(obj.mInstancePackageName) && mSapaApp.Equals(obj.mSapaApp));
			}
			return false;
		}
	}
}