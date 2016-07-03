package com.generalscan.activity.bluetoothConnect;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;

import com.generalscan.SendConstant;
import com.generalscan.bluetooth.BluetoothSend;
import com.generalscan.sdk.R;

/**
 * 发送数据列表
 * 
 * @author Administrator
 * 
 */
public class BluetoothSendList {

	// 所有的发送内容
	// All send
	private final int[] AllConstant = {
			// SendConstant.ListRead显示可以读取扫描器内容的列表
			// SendConstant.ListFunction 显示扫面器功能的列表
			// SendConstant.ListConfig 显示配置扫描器的列表
			// 目前有这3种列表，对应不同的功能
			// There are three type constants
			SendConstant.ListFunction, SendConstant.ListConfig,
			SendConstant.ListRead };

	private Context myContext;

	public BluetoothSendList(Context context) {
		myContext = context;
	}

	/**
	 * 显示所有发送数据的对话框 Display Dialog of send data
	 */
	public void ShowoDialog() {

		// 对应发送内容的描述
		// The corresponding transmitting content description
		CharSequence[] items = { myContext.getString(R.string.Function),
				myContext.getString(R.string.Config),
				myContext.getString(R.string.Read) };

		AlertDialog.Builder builer = new AlertDialog.Builder(myContext);
		builer.setSingleChoiceItems(items, -1,
				new DialogInterface.OnClickListener() {

					@Override
					public void onClick(DialogInterface dialog, int which) {

						// 发送对应内容
						// Send content
						BluetoothSend
								.SendContent(myContext, AllConstant[which]);
						// 此方法有4个参数
						// This method has 4 parameters
						// 第一个为Context,第二个是要发送的内容索引
						// The first is Context, second is the content index to
						// send
						// 第三个是列表的名称（不是必须）
						// The third is a list of names (not required)
						// BluetoothSend.SendContent(myContext,AllConstant[which],"自定义列表名称");
						// 第四个是是否显示默认发送命令(默认是不显示，不是必须)
						// The fourth is whether to display the default send
						// command (the default is not displayed, not a must)
						// BluetoothSend.SendContent(myContext,AllConstant[which],"自定义列表名称",true);
						dialog.cancel();
					}

				});
		builer.create().show();
	}
}
