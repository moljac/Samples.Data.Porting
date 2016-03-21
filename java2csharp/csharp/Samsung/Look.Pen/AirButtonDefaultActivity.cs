namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Uri = android.net.Uri;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using Toast = android.widget.Toast;

	using Slook = com.samsung.android.sdk.look.Slook;
	using SlookAirButton = com.samsung.android.sdk.look.airbutton.SlookAirButton;
	using ItemSelectListener = com.samsung.android.sdk.look.airbutton.SlookAirButton.ItemSelectListener;
	using SlookAirButtonFrequentContactAdapter = com.samsung.android.sdk.look.airbutton.SlookAirButtonFrequentContactAdapter;
	using SlookAirButtonRecentMediaAdapter = com.samsung.android.sdk.look.airbutton.SlookAirButtonRecentMediaAdapter;

	public class AirButtonDefaultActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_airbutton_default;

			Button btnRecipient = (Button) findViewById(R.id.btn_default_recipient);
			createRecipientListWidgetFromView(btnRecipient);
			Button btnImage = (Button) findViewById(R.id.btn_default_image);
			createImageListWidgetFromView(btnImage);

			Slook slook = new Slook();
			if (!slook.isFeatureEnabled(Slook.AIRBUTTON))
			{
				Toast.makeText(this, "This model doesn't support AirButton", Toast.LENGTH_LONG).show();
			}
			else
			{
				Toast.makeText(this, "Please hover and push the Spen button on each button", Toast.LENGTH_SHORT).show();
			}

		}

		public virtual SlookAirButton createImageListWidgetFromView(View v)
		{

			SlookAirButton airButtonWidget = new SlookAirButton(v, new SlookAirButtonRecentMediaAdapter(v, null), SlookAirButton.UI_TYPE_LIST);
			airButtonWidget.ItemSelectListener = new ItemSelectListenerAnonymousInnerClassHelper(this);
			airButtonWidget.Gravity = SlookAirButton.GRAVITY_LEFT;
			airButtonWidget.Direction = SlookAirButton.DIRECTION_UPPER;
			airButtonWidget.setPosition(0, -50);

			return airButtonWidget;
		}

		private class ItemSelectListenerAnonymousInnerClassHelper : SlookAirButton.ItemSelectListener
		{
			private readonly AirButtonDefaultActivity outerInstance;

			public ItemSelectListenerAnonymousInnerClassHelper(AirButtonDefaultActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onItemSelected(View arg0, int arg1, object arg2)
			{
				if (arg1 < 0)
				{
					return;
				}
				Uri uri = (Uri) arg2;
				Toast.makeText(outerInstance, uri.ToString(), Toast.LENGTH_SHORT).show();
			}
		}

		public virtual SlookAirButton createRecipientListWidgetFromView(View v)
		{
			Bundle option = new Bundle();
			option.putString("MIME_TYPE", "vnd.android.cursor.item/phone_v2");
			SlookAirButton airButtonWidget = new SlookAirButton(v, new SlookAirButtonFrequentContactAdapter(v, null), SlookAirButton.UI_TYPE_LIST);
			// airButtonWidget.steDir
			airButtonWidget.Direction = SlookAirButton.DIRECTION_UPPER;
			airButtonWidget.ItemSelectListener = new ItemSelectListenerAnonymousInnerClassHelper2(this);

			return airButtonWidget;
		}

		private class ItemSelectListenerAnonymousInnerClassHelper2 : SlookAirButton.ItemSelectListener
		{
			private readonly AirButtonDefaultActivity outerInstance;

			public ItemSelectListenerAnonymousInnerClassHelper2(AirButtonDefaultActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public virtual void onItemSelected(View arg0, int arg1, object arg2)
			{
				Bundle bundle = (Bundle) arg2;
				if (bundle != null)
				{
					string name = bundle.getString(SlookAirButtonFrequentContactAdapter.DISPLAY_NAME);
					string data = bundle.getString(SlookAirButtonFrequentContactAdapter.DATA);

					Toast.makeTextuniquetempvar.show();
				}
			}
		}
	}

}