package com.bxl.postest.settings;

import android.os.Parcel;
import android.os.Parcelable;

public class EntryInfo implements Parcelable {
	
	private String logicalName;
	private String deviceCategory;
	private String productName;
	private String deviceBus;
	private String address;

	@Override
	public int describeContents() {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public void writeToParcel(Parcel dest, int flags) {
		dest.writeString(logicalName);
		dest.writeString(deviceCategory);
		dest.writeString(productName);
		dest.writeString(deviceBus);
		dest.writeString(address);
	}
	
	public static final Parcelable.Creator<EntryInfo> CREATOR = new Parcelable.Creator<EntryInfo>() {

		@Override
		public EntryInfo createFromParcel(Parcel source) {
			return new EntryInfo(source);
		}

		@Override
		public EntryInfo[] newArray(int size) {
			return new EntryInfo[size];
		}
	};
	
	public EntryInfo(String logicalName, String deviceCategory, String productName,
			String deviceBus, String address) {
		this.logicalName = logicalName;
		this.deviceCategory = deviceCategory;
		this.productName = productName;
		this.deviceBus = deviceBus;
		this.address = address;
	}

	private EntryInfo(Parcel in) {
		logicalName = in.readString();
		deviceCategory = in.readString();
		productName = in.readString();
		deviceBus = in.readString();
		address = in.readString();
	}
	
	public String getLogicalName() {
		return logicalName;
	}
	
	public String getDeviceCategory() {
		return deviceCategory;
	}
	
	public String getProductName() {
		return productName;
	}
	
	public String getDeviceBus() {
		return deviceBus;
	}
	
	public String getAddress() {
		return address;
	}
}
