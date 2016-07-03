package com.example.com.generalscan.usb.broadcast;

import java.util.ArrayList;
import java.util.List;
import java.util.logging.Logger;

import android.content.Context;
import android.content.Intent;
import android.content.pm.ApplicationInfo;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.content.pm.PackageManager.NameNotFoundException;
import android.graphics.drawable.Drawable;
import android.widget.Toast;

public class AppFilter {

	private Context context;

	private ArrayList<AppInfo> appList = new ArrayList<AppInfo>();

	public AppFilter(Context context) {
		this.context = context;
		getAppInfo();
	}

	private void getAppInfo() {
		PackageManager pm = context.getPackageManager();
		List<PackageInfo> pakageinfos = pm
				.getInstalledPackages(PackageManager.GET_UNINSTALLED_PACKAGES);
		for (PackageInfo packageInfo : pakageinfos) {
			AppInfo appInfo = new AppInfo();

			// 获取应用程序的名称，不是包名，而是清单文件中的label name
			String str_name = packageInfo.applicationInfo.loadLabel(pm)
					.toString();

			appInfo.setName(str_name);

			// 获取应用程序的版本号码
			String version = packageInfo.versionName;

			// 获取应用程序的快捷方式图标
			Drawable drawable = packageInfo.applicationInfo.loadIcon(pm);
			appInfo.setDrawable(drawable);

			// 获取应用程序是否是第三方应用程序
			filterApp(packageInfo.applicationInfo);

			// 给一同程序设置包名
			appInfo.setPackage(packageInfo.packageName);

			appList.add(appInfo);
		}

	}

	/**
	 * 三方应用程序的过滤器
	 * 
	 * @param info
	 * @return true 三方应用 false 系统应用
	 */
	public boolean filterApp(ApplicationInfo info) {
		if ((info.flags & ApplicationInfo.FLAG_UPDATED_SYSTEM_APP) != 0) {
			// 代表的是系统的应用,但是被用户升级了. 用户应用
			return true;
		} else if ((info.flags & ApplicationInfo.FLAG_SYSTEM) == 0) {
			// 代表的用户的应用
			return true;
		}
		return false;
	}

	/* * 启动一个app */
	public static boolean startAPP(Context context,String appPackageName) {
		try {
			Intent intent = context.getPackageManager()
					.getLaunchIntentForPackage(appPackageName);
			context.startActivity(intent);
			return true;
		} catch (Exception e) {
			// Toast.makeText(this, "没有安装", Toast.LENGTH_LONG).show();
			return false;
		}
	}

	public ArrayList<AppInfo> getAppList() {
		return appList;
	}

}
