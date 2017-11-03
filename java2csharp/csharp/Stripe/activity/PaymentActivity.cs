namespace com.stripe.example.activity
{

	using Bundle = android.os.Bundle;
	using AppCompatActivity = android.support.v7.app.AppCompatActivity;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;

	using CardInputWidget = com.stripe.android.view.CardInputWidget;
	using DependencyHandler = com.stripe.example.module.DependencyHandler;

	public class PaymentActivity : AppCompatActivity
	{

		private DependencyHandler mDependencyHandler;

		public override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.payment_activity;

			mDependencyHandler = new DependencyHandler(this, (CardInputWidget) findViewById(R.id.card_input_widget), (ListView) findViewById(R.id.listview));

			Button saveButton = (Button) findViewById(R.id.save);
			mDependencyHandler.attachAsyncTaskTokenController(saveButton);

			Button saveRxButton = (Button) findViewById(R.id.saverx);
			mDependencyHandler.attachRxTokenController(saveRxButton);

			Button saveIntentServiceButton = (Button) findViewById(R.id.saveWithService);
			mDependencyHandler.attachIntentServiceTokenController(this, saveIntentServiceButton);
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			mDependencyHandler.clearReferences();
		}
	}

}