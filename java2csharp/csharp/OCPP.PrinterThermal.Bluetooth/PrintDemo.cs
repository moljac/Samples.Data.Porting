using System;

namespace com.zj.printdemo
{

	using Intent = android.content.Intent;
	using BluetoothService = com.zj.btsdk.BluetoothService;
	using PrintPic = com.zj.btsdk.PrintPic;
	using SuppressLint = android.annotation.SuppressLint;
	using Activity = android.app.Activity;
	using BluetoothAdapter = android.bluetooth.BluetoothAdapter;
	using BluetoothDevice = android.bluetooth.BluetoothDevice;
	using Bundle = android.os.Bundle;
	using Handler = android.os.Handler;
	using Message = android.os.Message;
	using View = android.view.View;
	using Button = android.widget.Button;
	using EditText = android.widget.EditText;
	using Toast = android.widget.Toast;

	using Log = android.util.Log;


	public class PrintDemo : Activity
	{
		internal Button btnSearch;
		internal Button btnSendDraw;
		internal Button btnSend;
		internal Button btnClose;
		internal EditText edtContext;
		internal EditText edtPrint;
		private const int REQUEST_ENABLE_BT = 2;
		internal BluetoothService mService = null;
		internal BluetoothDevice con_dev = null;
		private const int REQUEST_CONNECT_DEVICE = 1; //��ȡ�豸��Ϣ



		/// <summary>
		/// Called when the activity is first created. </summary>
		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.main;
			mService = new BluetoothService(this, mHandler);
			//�����������˳�����
			if (mService.Available == false)
			{
				Toast.makeText(this, "Bluetooth is not available", Toast.LENGTH_LONG).show();
				finish();
			}
		}

		public override void onStart()
		{
			base.onStart();
			//����δ�򿪣�������
			if (mService.BTopen == false)
			{
				Intent enableIntent = new Intent(BluetoothAdapter.ACTION_REQUEST_ENABLE);
				startActivityForResult(enableIntent, REQUEST_ENABLE_BT);
			}
			try
			{
				btnSendDraw = (Button) this.findViewById(R.id.btn_test);
				btnSendDraw.OnClickListener = new ClickEvent(this);
				btnSearch = (Button) this.findViewById(R.id.btnSearch);
				btnSearch.OnClickListener = new ClickEvent(this);
				btnSend = (Button) this.findViewById(R.id.btnSend);
				btnSend.OnClickListener = new ClickEvent(this);
				btnClose = (Button) this.findViewById(R.id.btnClose);
				btnClose.OnClickListener = new ClickEvent(this);
				edtContext = (EditText) findViewById(R.id.txt_content);
				btnClose.Enabled = false;
				btnSend.Enabled = false;
				btnSendDraw.Enabled = false;
			}
			catch (Exception ex)
			{
				Log.e("������Ϣ",ex.Message);
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			if (mService != null)
			{
				mService.stop();
			}
			mService = null;
		}

		internal class ClickEvent : View.OnClickListener
		{
			private readonly PrintDemo outerInstance;

			public ClickEvent(PrintDemo outerInstance)
			{
				this.outerInstance = outerInstance;
			}

			public virtual void onClick(View v)
			{
				if (v == outerInstance.btnSearch)
				{
					Intent serverIntent = new Intent(outerInstance,typeof(DeviceListActivity)); //��������һ����Ļ
					startActivityForResult(serverIntent,REQUEST_CONNECT_DEVICE);
				}
				else if (v == outerInstance.btnSend)
				{
					string msg = outerInstance.edtContext.Text.ToString();
					if (msg.Length > 0)
					{
						outerInstance.mService.sendMessage(msg + "\n", "GBK");
					}
				}
				else if (v == outerInstance.btnClose)
				{
					outerInstance.mService.stop();
				}
				else if (v == outerInstance.btnSendDraw)
				{
					string msg = "";
					string lang = getString(R.@string.strLang);
					//printImage();

					sbyte[] cmd = new sbyte[3];
					cmd[0] = 0x1b;
					cmd[1] = 0x21;
					if ((lang.CompareTo("en")) == 0)
					{
						cmd[2] |= 0x10;
						outerInstance.mService.write(cmd); //���������ģʽ
						outerInstance.mService.sendMessage("Congratulations!\n", "GBK");
						cmd[2] &= unchecked((sbyte)0xEF);
						outerInstance.mService.write(cmd); //ȡ�����ߡ�����ģʽ
						msg = "  You have sucessfully created communications between your device and our bluetooth printer.\n\n" + "  the company is a high-tech enterprise which specializes" + " in R&D,manufacturing,marketing of thermal printers and barcode scanners.\n\n";


						outerInstance.mService.sendMessage(msg,"GBK");
					}
					else if ((lang.CompareTo("ch")) == 0)
					{
						cmd[2] |= 0x10;
						outerInstance.mService.write(cmd); //���������ģʽ
						outerInstance.mService.sendMessage("��ϲ����\n", "GBK");
						cmd[2] &= unchecked((sbyte)0xEF);
						outerInstance.mService.write(cmd); //ȡ�����ߡ�����ģʽ
						msg = "  ���Ѿ��ɹ��������������ǵ�������ӡ����\n\n" + "  ����˾��һ��רҵ�����з�����������������Ʊ�ݴ�ӡ��������ɨ���豸��һ��ĸ߿Ƽ���ҵ.\n\n";

						outerInstance.mService.sendMessage(msg,"GBK");
					}
				}
			}
		}

		/// <summary>
		/// ����һ��Handlerʵ�������ڽ���BluetoothService�෵�ػ�������Ϣ
		/// </summary>
		private readonly Handler mHandler = new HandlerAnonymousInnerClassHelper();

		private class HandlerAnonymousInnerClassHelper : Handler
		{
			public HandlerAnonymousInnerClassHelper()
			{
			}

			public override void handleMessage(Message msg)
			{
				switch (msg.what)
				{
				case BluetoothService.MESSAGE_STATE_CHANGE:
					switch (msg.arg1)
					{
					case BluetoothService.STATE_CONNECTED: //������
						Toast.makeText(ApplicationContext, "Connect successful", Toast.LENGTH_SHORT).show();
						outerInstance.btnClose.Enabled = true;
						outerInstance.btnSend.Enabled = true;
						outerInstance.btnSendDraw.Enabled = true;
						break;
					case BluetoothService.STATE_CONNECTING: //��������
						Log.d("��������","��������.....");
						break;
					case BluetoothService.STATE_LISTEN: //�������ӵĵ���
					case BluetoothService.STATE_NONE:
						Log.d("��������","�ȴ�����.....");
						break;
					}
					break;
				case BluetoothService.MESSAGE_CONNECTION_LOST: //�����ѶϿ�����
					Toast.makeText(ApplicationContext, "Device connection was lost", Toast.LENGTH_SHORT).show();
					outerInstance.btnClose.Enabled = false;
					outerInstance.btnSend.Enabled = false;
					outerInstance.btnSendDraw.Enabled = false;
					break;
				case BluetoothService.MESSAGE_UNABLE_CONNECT: //�޷������豸
					Toast.makeText(ApplicationContext, "Unable to connect device", Toast.LENGTH_SHORT).show();
					break;
				}
			}

		}

		public override void onActivityResult(int requestCode, int resultCode, Intent data)
		{
			switch (requestCode)
			{
			case REQUEST_ENABLE_BT: //���������
				if (resultCode == Activity.RESULT_OK)
				{ //�����Ѿ���
					Toast.makeText(this, "Bluetooth open successful", Toast.LENGTH_LONG).show();
				}
				else
				{ //�û������������
					finish();
				}
				break;
			case REQUEST_CONNECT_DEVICE: //��������ĳһ�����豸
				if (resultCode == Activity.RESULT_OK)
				{ //�ѵ�������б��е�ĳ���豸��
					string address = data.Extras.getString(DeviceListActivity.EXTRA_DEVICE_ADDRESS); //��ȡ�б������豸��mac��ַ
					con_dev = mService.getDevByMac(address);

					mService.connect(con_dev);
				}
				break;
			}
		}

		//��ӡͼ��
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressLint("SdCardPath") private void printImage()
		private void printImage()
		{
			sbyte[] sendData = null;
			PrintPic pg = new PrintPic();
			pg.initCanvas(384);
			pg.initPaint();
			pg.drawImage(0, 0, "/mnt/sdcard/icon.jpg");
			sendData = pg.printDraw();
			mService.write(sendData); //��ӡbyte������
		}
	}

}