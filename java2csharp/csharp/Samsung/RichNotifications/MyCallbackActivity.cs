namespace com.samsung.android.richnotification.sample
{

	using Activity = android.app.Activity;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using Toast = android.widget.Toast;

	public class MyCallbackActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			Intent intent = Intent;

			string data = intent.getStringExtra("extra_action_data");

			if (data != null)
			{
				Toast.makeText(this, data, Toast.LENGTH_SHORT).show();
			}
		}
	}

}