namespace com.bxl.postest.settings
{

	using Parcel = android.os.Parcel;
	using Parcelable = android.os.Parcelable;

	public class EntryInfo : Parcelable
	{

		private string logicalName;
		private string deviceCategory;
		private string productName;
		private string deviceBus;
		private string address;

		public override int describeContents()
		{
			// TODO Auto-generated method stub
			return 0;
		}

		public override void writeToParcel(Parcel dest, int flags)
		{
			dest.writeString(logicalName);
			dest.writeString(deviceCategory);
			dest.writeString(productName);
			dest.writeString(deviceBus);
			dest.writeString(address);
		}

		public static readonly Parcelable.Creator<EntryInfo> CREATOR = new CreatorAnonymousInnerClassHelper();

		private class CreatorAnonymousInnerClassHelper : Parcelable.Creator<EntryInfo>
		{
			public CreatorAnonymousInnerClassHelper()
			{
			}


			public override EntryInfo createFromParcel(Parcel source)
			{
				return new EntryInfo(source);
			}

			public override EntryInfo[] newArray(int size)
			{
				return new EntryInfo[size];
			}
		}

		public EntryInfo(string logicalName, string deviceCategory, string productName, string deviceBus, string address)
		{
			this.logicalName = logicalName;
			this.deviceCategory = deviceCategory;
			this.productName = productName;
			this.deviceBus = deviceBus;
			this.address = address;
		}

		private EntryInfo(Parcel @in)
		{
			logicalName = @in.readString();
			deviceCategory = @in.readString();
			productName = @in.readString();
			deviceBus = @in.readString();
			address = @in.readString();
		}

		public virtual string LogicalName
		{
			get
			{
				return logicalName;
			}
		}

		public virtual string DeviceCategory
		{
			get
			{
				return deviceCategory;
			}
		}

		public virtual string ProductName
		{
			get
			{
				return productName;
			}
		}

		public virtual string DeviceBus
		{
			get
			{
				return deviceBus;
			}
		}

		public virtual string Address
		{
			get
			{
				return address;
			}
		}
	}

}