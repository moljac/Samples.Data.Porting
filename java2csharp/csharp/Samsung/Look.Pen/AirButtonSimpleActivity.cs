using System.Collections.Generic;

namespace com.samsung.android.example.slookdemos
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Button = android.widget.Button;
	using Toast = android.widget.Toast;

	using Slook = com.samsung.android.sdk.look.Slook;
	using SlookAirButton = com.samsung.android.sdk.look.airbutton.SlookAirButton;
	using ItemSelectListener = com.samsung.android.sdk.look.airbutton.SlookAirButton.ItemSelectListener;
	using SlookAirButtonAdapter = com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter;
	using AirButtonItem = com.samsung.android.sdk.look.airbutton.SlookAirButtonAdapter.AirButtonItem;

	public class AirButtonSimpleActivity : Activity
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.activity_airbutton_simple;

			Button btnMenu = (Button) findViewById(R.id.btn_menu);
			createMenuWidgetFromView(btnMenu);
			Button btnRecipient = (Button) findViewById(R.id.btn_recipient);
			createRecipientListWidgetFromView(btnRecipient);
			Button btnImage = (Button) findViewById(R.id.btn_image);
			createImageListWidgetFromView(btnImage);
			Button btnText = (Button) findViewById(R.id.btn_text);
			createTextListWidgetFromView(btnText);

			Slook slook = new Slook();
			if (!slook.isFeatureEnabled(Slook.AIRBUTTON))
			{
				Toast.makeText(this, "This model doesn't support AirButton", Toast.LENGTH_LONG).show();
			}
			else
			{
				Toast.makeText(this, "Please hover and press the S-pen side-button on each button", Toast.LENGTH_SHORT).show();
			}
		}

		private SlookAirButton.ItemSelectListener mCallback = new ItemSelectListenerAnonymousInnerClassHelper();

		private class ItemSelectListenerAnonymousInnerClassHelper : SlookAirButton.ItemSelectListener
		{
			public ItemSelectListenerAnonymousInnerClassHelper()
			{
			}

			public virtual void onItemSelected(View arg0, int arg1, object arg2)
			{
				Toast.makeTextuniquetempvar.show();
			}
		}

		public virtual SlookAirButton createImageListWidgetFromView(View v)
		{

			SlookAirButton airButtonWidget = new SlookAirButton(v, AdapterImageList, SlookAirButton.UI_TYPE_LIST);
			airButtonWidget.ItemSelectListener = mCallback;
			airButtonWidget.Gravity = SlookAirButton.GRAVITY_LEFT;
			airButtonWidget.Direction = SlookAirButton.DIRECTION_UPPER;
			airButtonWidget.setPosition(0, -50);

			return airButtonWidget;
		}

		public virtual SlookAirButton createTextListWidgetFromView(View v)
		{
			SlookAirButton airButtonWidget = new SlookAirButton(v, AdapterStringList, SlookAirButton.UI_TYPE_LIST);
			airButtonWidget.ItemSelectListener = mCallback;

			airButtonWidget.setPosition(0, 50);

			return airButtonWidget;
		}

		public virtual SlookAirButton createRecipientListWidgetFromView(View v)
		{
			SlookAirButton airButtonWidget = new SlookAirButton(v, AdapterRecipientList, SlookAirButton.UI_TYPE_LIST);
			airButtonWidget.Direction = SlookAirButton.DIRECTION_LOWER;
			airButtonWidget.ItemSelectListener = mCallback;

			return airButtonWidget;
		}

		public virtual SlookAirButton createMenuWidgetFromView(View v)
		{
			SlookAirButton airButtonWidget = new SlookAirButton(v, AdapterMenuList, SlookAirButton.UI_TYPE_MENU);
			airButtonWidget.Direction = SlookAirButton.DIRECTION_RIGHT;
			airButtonWidget.ItemSelectListener = mCallback;

			return airButtonWidget;
		}

		public virtual SlookAirButtonAdapter AdapterImageList
		{
			get
			{
				List<SlookAirButtonAdapter.AirButtonItem> itemList = new List<SlookAirButtonAdapter.AirButtonItem>();
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic01), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic02), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic03), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic04), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic05), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic06), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic07), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic08), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic09), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic10), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic11), null, null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.pic12), null, null));
    
				return new SlookAirButtonAdapter(itemList);
			}
		}

		public virtual SlookAirButtonAdapter AdapterStringList
		{
			get
			{
				List<SlookAirButtonAdapter.AirButtonItem> stringList = new List<SlookAirButtonAdapter.AirButtonItem>();
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "You can come here at 5:00", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "Why?", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "Please send your e-mail address", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "Ok. No problem", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "kkkkkkk", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "I'm a boy", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "You are a girl", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "How about this weekend?", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "You are so sexy!", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "Haha it's really good", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "What you really want to?", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "I wanna watch movie", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "No...", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "ASAP", null));
				stringList.Add(new SlookAirButtonAdapter.AirButtonItem(null, "Really? I can't agree with you", null));
    
				return new SlookAirButtonAdapter(stringList);
			}
		}

		public virtual SlookAirButtonAdapter AdapterRecipientList
		{
			get
			{
				List<SlookAirButtonAdapter.AirButtonItem> itemList = new List<SlookAirButtonAdapter.AirButtonItem>();
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Alexander Hamilton", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Oliver Wolcott Jr", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Samuel Dexter", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Albert Gallatin", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "George W. Campbell", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Richard Rush", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Richard Rush", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "William J. Duane", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Thomas Ewing", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "George M. Bibb", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "William M. Meredith", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Howell Cobb", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.recipient), "Salmon P. Chase", null));
    
				return new SlookAirButtonAdapter(itemList);
			}
		}

		public virtual SlookAirButtonAdapter AdapterMenuList
		{
			get
			{
				List<SlookAirButtonAdapter.AirButtonItem> itemList = new List<SlookAirButtonAdapter.AirButtonItem>();
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_add), "Add", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_archive), "Help", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_edit), "Edit", null));
				itemList.Add(new SlookAirButtonAdapter.AirButtonItem(Resources.getDrawable(R.drawable.ic_menu_help), "Help", null));
				;
    
				return new SlookAirButtonAdapter(itemList);
			}
		}
	}

}