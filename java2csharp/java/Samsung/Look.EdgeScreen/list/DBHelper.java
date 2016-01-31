
package com.example.edgescreen.list;

import java.util.ArrayList;

public class DBHelper {
    private ArrayList<String> mDatalist = new ArrayList<String>();

    private static DBHelper sInstance = new DBHelper();

    private Object mLock = new Object();

    public static DBHelper getInstance() {
        return sInstance;
    }

    public void addData(String data) {
        synchronized (mLock) {
            mDatalist.add(data);
        }
    }

    public String getData(int index) {
        synchronized (mLock) {
            try {
                return mDatalist.get(index);
            } catch (IndexOutOfBoundsException e) {
                return null;
            }
        }
    }

    public int getSize() {
        synchronized (mLock) {
            return mDatalist.size();
        }
    }

    public void clearData() {
        synchronized (mLock) {
            mDatalist.clear();
        }
    }
}
