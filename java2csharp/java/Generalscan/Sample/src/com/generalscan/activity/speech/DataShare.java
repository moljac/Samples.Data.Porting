package com.generalscan.activity.speech;

import android.content.Context;
import android.content.SharedPreferences;
import android.content.SharedPreferences.Editor;

public class DataShare {

	private final static String TAG = "DataShare";
	
	private final static String NAME = "NAME";
	private final static String BARCODE = "BARCODE";
	private final static String COUNT = "COUNT";
	/**
	 * 
	 */
	public final static void SetName(Context myContext,String day)
	{
		Editor sharedata = myContext.getSharedPreferences(TAG, 0).edit();
		sharedata.putString(NAME,day);   
		sharedata.commit();	
	}
	/**
	 * 
	 */
	public final static void SetBarcode(Context myContext,String day)
	{
		Editor sharedata = myContext.getSharedPreferences(TAG, 0).edit();
		sharedata.putString(BARCODE,day);   
		sharedata.commit();	
	}
	
	/**
	 * 
	 */
	public final static void SetCount(Context myContext,String day)
	{
		Editor sharedata = myContext.getSharedPreferences(TAG, 0).edit();
		sharedata.putString(COUNT,day);   
		sharedata.commit();	
	}
	
	
	
	/**
	 *
	 * @return
	 */
	public final static String getName(Context myContext)
	{
		
		SharedPreferences sharedata = myContext.getSharedPreferences(TAG, 0);  
		String userTime= sharedata.getString(NAME, "");	
		return userTime;
	}
	/**
	 *
	 * @return
	 */
	public final static String getBarcode(Context myContext)
	{
		
		SharedPreferences sharedata = myContext.getSharedPreferences(TAG, 0);  
		String userTime= sharedata.getString(BARCODE, "");	
		return userTime;
	}
	/**
	 *
	 * @return
	 */
	public final static String getCount(Context myContext)
	{
		
		SharedPreferences sharedata = myContext.getSharedPreferences(TAG, 0);  
		String userTime= sharedata.getString(COUNT, "");	
		return userTime;
	}
}
