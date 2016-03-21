using System.Collections.Generic;

namespace com.samsung.android.sdk.professionalaudio.sample.simplepluginclient
{


	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using TextView = android.widget.TextView;
	using Toast = android.widget.Toast;


	public class SapaSimplePluginClient : Activity
	{

		private SapaService mService;
		private SapaProcessor mProcessor;

		private const string TAG = "SapaSimplePluginClient";

		protected internal override void onDestroy()
		{

			base.onDestroy();
			if (mService != null)
			{
				if (mProcessor != null)
				{
					mProcessor.deactivate();
					mService.unregister(mProcessor);
				}
				mService.stop(false);
			}
		}

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_sapa_plugin_simple_client;

			try
			{
				Sapa sapa = new Sapa();
				sapa.initialize(this);
				mService = new SapaService();
				mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);

			}
			catch (SsdkUnsupportedException e)
			{
				Toast.makeText(this, "Not support professional audio package", Toast.LENGTH_LONG).show();
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				finish();
				return;
			}
			catch (InstantiationException e)
			{
				Toast.makeText(this, "fail to instantiation SapaService", Toast.LENGTH_LONG).show();
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				finish();
				return;
			}

			((Button) findViewById(R.id.load_button)).Enabled = true;

			((Button) findViewById(R.id.load_button)).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this, e);

			((Button) findViewById(R.id.activate_button)).Enabled = false;
			((Button) findViewById(R.id.activate_button)).OnClickListener = new OnClickListenerAnonymousInnerClassHelper2(this);
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SapaSimplePluginClient outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimplePluginClient outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				((TextView) findViewById(R.id.load_status_text)).Text = "Loading... ";
				IList<SapaPluginInfo> pluginList = outerInstance.mService.AllPlugin;
				IEnumerator<SapaPluginInfo> iter = pluginList.GetEnumerator();
				while (iter.MoveNext())
				{
					SapaPluginInfo info = iter.Current;
					if (info.Name.contentEquals("SapaSimplePlugin") == true)
					{
						// load SapaSimplePlugin
						try
						{
							outerInstance.mProcessor = new SapaProcessor(v.Context, info, new StatusListenerAnonymousInnerClassHelper(this));
							outerInstance.mService.register(outerInstance.mProcessor);
							((TextView) findViewById(R.id.load_status_text)).Text = "Loading SUCCESS ";
							((Button) findViewById(R.id.activate_button)).Enabled = true;
							((Button) findViewById(R.id.load_button)).Enabled = false;
							return;
						}
						catch (InstantiationException e)
						{
							Toast.makeText(v.Context, "Fail to register SapaProcessor", Toast.LENGTH_LONG).show();
							((TextView) findViewById(R.id.load_status_text)).Text = "Loading FAIL ";
							Console.WriteLine(e.ToString());
							Console.Write(e.StackTrace);
						}
						break;
					}
				}
				((TextView) findViewById(R.id.load_status_text)).Text = "Loading FAIL ";
			}

			private class StatusListenerAnonymousInnerClassHelper : SapaProcessor.StatusListener
			{
				private readonly OnClickListenerAnonymousInnerClassHelper outerInstance;

				public StatusListenerAnonymousInnerClassHelper(OnClickListenerAnonymousInnerClassHelper outerInstance)
				{
					this.outerInstance = outerInstance;
				}


				public override void onKilled()
				{
					Log.v(TAG, "SapaSimplePluginClient will be closed. because of the SapaProcessor was closed.");
					outerInstance.outerInstance.mService.stop(true);
					finish();
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly SapaSimplePluginClient outerInstance;

			public OnClickListenerAnonymousInnerClassHelper2(SapaSimplePluginClient outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				if (outerInstance.mProcessor != null)
				{
					outerInstance.mProcessor.activate();
				}
			}
		}
	}

}