package com.example.generalscanusbbroadcasttest;

/**
 * 发送命令的常量
 * 
 * @author Administrator
 * 
 */
public class SendConstant {

	/**
	 * 发送列表
	 */
	public static final int ListConfig = 0;
	public static final int ListFunction = 1;
	public static final int ListRead = 2;

	// 功能
	/**
	 * 发送开始扫描
	 */
	public static final int F_StartScan = 11;
	/**
	 * 关闭休眠时间
	 */
	public static final int F_CloseHibernate = 12;

	/**
	 * 关闭读取数据
	 */
	public static final int F_CloseRead = 13;
	/**
	 * 打开读取数据
	 */
	public static final int F_OpenRead = 14;
	/**
	 * 恢复默认设置
	 */
	public static final int F_DefaultSetting = 15;
	/**
	 * 快速充电
	 */
	public static final int F_ChargeFast = 16;
	/**
	 * 低电流充电
	 */
	public static final int F_ChargeNormal = 17;
	// 读取
	/**
	 * 获取密匙
	 */
	public static final int GetCipher = 101;

	/**
	 * 获取产品硬件ID
	 */
	public static final int GetScanerID = 102;
	/**
	 * 获取电池电量
	 */
	public static final int GetBattery = 103;
	/**
	 * 获取版本信息
	 */
	public static final int GetVersion = 104;
	/**
	 * 获取充电状态
	 */
	public static final int GetCharge = 105;
	// 配置
	/**
	 * 发送休眠时间
	 */
	public static final int SetSleepTime = 1001;

	/**
	 * 设置SPP蓝牙名称
	 */
	public static final int SetSPPBluetoothName = 1002;
	/**
	 * 设置HID蓝牙名称
	 */
	public static final int SetHIDBluetoothName = 1003;
	/**
	 * 设置后缀字符
	 */
	public static final int SetPostambleCharacter = 1004;
	/**
	 * 设置前缀字符
	 */
	public static final int SetPreambleCharacter = 1005;
	/**
	 * 输出模式
	 */
	public static final int SetOutputMode = 1006;
	/**
	 * LED
	 */
	public static final int SetLED = 1007;
	/**
	 * Buzzer
	 */
	public static final int SetBuzzer = 1008;
	/**
	 * NoReadOutput无解码输出
	 */
	public static final int SetNoReadOutput = 1009;
	/**
	 * TriggerMode触发模式
	 */
	public static final int SetTriggerMode = 1010;
	/**
	 * NoDecodeTimeOut无解码超时
	 */
	public static final int SetNoDecodeTimeOut = 1011;

	/**
	 * TTR
	 */
	public static final int SetTTR = 1012;
	/**
	 * DecodesBeforeOutput同一条码解码纠错
	 */
	public static final int SetDecodesBeforeOutput = 1013;
	/**
	 * SetVolume音量设置
	 */
	public static final int SetVolume = 1014;
	/**
	 * 39码
	 */
	public static final int Code39 = 1100;
	/**
	 * 128码
	 */
	public static final int Code128 = 1101;
	/**
	 * Codabar码
	 */
	public static final int Codabar = 1102;
	/**
	 * Interleaved2of5码
	 */
	public static final int Interleaved2of5 = 1103;
	/**
	 * UPC/EAN码
	 */
	public static final int UPCEAN = 1104;

	// 发送读取到数据的广播
	public final static String GetDataAction = "generalscan.intent.action.usb.data";
	public final static String GetData = "GetData";
	// 发送发送读取数据后得到数据的广播
	public final static String GetReadDataAction = "generalscan.intent.action.usb.readdata";
	public final static String GetReadData = "GetReadData";
	public static final String GetReadName = "GetReadName";
}
