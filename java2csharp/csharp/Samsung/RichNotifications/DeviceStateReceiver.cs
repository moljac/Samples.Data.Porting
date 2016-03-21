namespace com.samsung.android.richnotification.sample
{

	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Log = android.util.Log;
	using Toast = android.widget.Toast;

	public class DeviceStateReceiver : BroadcastReceiver
	{

		private static readonly string TAG = typeof(DeviceStateReceiver).Name;

		public override void onReceive(Context context, Intent intent)
		{
			bool isConnected = intent.getBooleanExtra("isConnected", false);

			Log.d(TAG, "Connected : " + isConnected);

			Toast.makeTextuniquetempvar.show();

		}

	}

}