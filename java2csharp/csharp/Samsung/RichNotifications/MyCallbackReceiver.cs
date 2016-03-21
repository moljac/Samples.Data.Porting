namespace com.samsung.android.richnotification.sample
{

	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using Toast = android.widget.Toast;

	public class MyCallbackReceiver : BroadcastReceiver
	{

		public override void onReceive(Context context, Intent intent)
		{

			string data = intent.getStringExtra("extra_action_data");

			if (data != null)
			{
				Toast.makeText(context, data, Toast.LENGTH_SHORT).show();
			}
		}
	}

}