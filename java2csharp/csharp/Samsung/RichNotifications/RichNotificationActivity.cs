namespace com.samsung.android.richnotification.sample
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using Spinner = android.widget.Spinner;
	using Toast = android.widget.Toast;

	using SsdkUnsupportedException = com.samsung.android.sdk.SsdkUnsupportedException;
	using Srn = com.samsung.android.sdk.richnotification.Srn;
	using SrnRichNotificationManager = com.samsung.android.sdk.richnotification.SrnRichNotificationManager;
	using ErrorType = com.samsung.android.sdk.richnotification.SrnRichNotificationManager.ErrorType;
	using EventListener = com.samsung.android.sdk.richnotification.SrnRichNotificationManager.EventListener;

	public class RichNotificationActivity : Activity, SrnRichNotificationManager.EventListener
	{

		public enum TemplateTypes
		{
			SMALL_HEADER,
			MEDIUM_HEADER,
			LARGE_HEADER,
			FULL_SCREEN,
			EVENT,
			IMAGE
		}

		private SrnRichNotificationManager mRichNotificationManager;
		private Spinner mSpinner;

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.notification_main;

			Srn srn = new Srn();
			try
			{
				// Initialize an instance of Srn.
				srn.initialize(this);
			}
			catch (SsdkUnsupportedException)
			{
				// Error handling
			}

			mRichNotificationManager = new SrnRichNotificationManager(ApplicationContext);

			mSpinner = (Spinner) findViewById(R.id.spinner1);

			Toast.makeTextuniquetempvar.show();
		}

		protected internal override void onResume()
		{
			base.onResume();

			mRichNotificationManager.start();
			mRichNotificationManager.registerRichNotificationListener(this);
		}

		protected internal override void onPause()
		{
			base.onPause();

			mRichNotificationManager.unregisterRichNotificationListener(this);
			mRichNotificationManager.stop();
		}

		public virtual void onSendClick(View v)
		{
			perform(mSpinner.SelectedItemPosition);
		}

		public virtual void perform(int primary)
		{
			if (primary < 0 || primary >= Enum.GetValues(typeof(TemplateTypes)).length)
			{
				return;
			}

			Toast.makeText(RichNotificationActivity.this, "Sending Notification ...", Toast.LENGTH_SHORT).show();

			switch (Enum.GetValues(typeof(TemplateTypes))[primary])
			{
				case SMALL_HEADER:
					performExample(new SmallHeaderExample(ApplicationContext));
					break;

				case MEDIUM_HEADER:
					performExample(new MediumHeaderExample(ApplicationContext));
					break;

				case LARGE_HEADER:
					performExample(new LargeHeaderExample(ApplicationContext));
					break;

				case FULL_SCREEN:
					performExample(new FullScreenExample(ApplicationContext));
					break;

				case EVENT:
					performExample(new EventExample(ApplicationContext));
					break;

				case IMAGE:
					performExample(new ImageExample(ApplicationContext));
					break;
			}
		}

		private void performExample(IExample example)
		{
			UUID uuid = mRichNotificationManager.notify(example.createRichNoti());

			Toast.makeTextuniquetempvar.show();
		}

		public override void onError(UUID arg0, SrnRichNotificationManager.ErrorType arg1)
		{
			// TODO Auto-generated method stub
			Toast.makeTextuniquetempvar.show();
		}

		public override void onRead(UUID arg0)
		{
			// TODO Auto-generated method stub
			Toast.makeTextuniquetempvar.show();

		}

		public override void onRemoved(UUID arg0)
		{
			// TODO Auto-generated method stub
			Toast.makeTextuniquetempvar.show();

		}
	}

}