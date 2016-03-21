using System;

namespace com.samsung.android.sdk.professionalaudio.sample.simplepiano
{

	using Activity = android.app.Activity;
	using Bundle = android.os.Bundle;
	using Log = android.util.Log;
	using View = android.view.View;
	using OnClickListener = android.view.View.OnClickListener;
	using Button = android.widget.Button;
	using Toast = android.widget.Toast;

	using AssetManager = android.content.res.AssetManager;
	using Environment = android.os.Environment;


	public class SapaSimplePianoActivity : Activity
	{

		private SapaService mService;
		private SapaProcessor mProcessor;
		private const string TAG = "SapaSimplePiano";

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_sapa_simple_piano;

			try
			{
				(new Sapa()).initialize(this);
				mService = new SapaService();
				mService.start(SapaService.START_PARAM_DEFAULT_LATENCY);
				mProcessor = new SapaProcessor(this, null, new StatusListenerAnonymousInnerClassHelper(this));
				mService.register(mProcessor);

				// copy sound font file to sdcard.
				copyAssets();

				mProcessor.sendCommand("START");
				mProcessor.activate();

			}
			catch (SsdkUnsupportedException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "Not support Professional Audio package", Toast.LENGTH_LONG).show();
				finish();
				return;
			}
			catch (System.ArgumentException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "Error - invalid arguments. please check the log", Toast.LENGTH_LONG).show();
				finish();
				return;
			}
			catch (InstantiationException e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				Toast.makeText(this, "Error. please check the log", Toast.LENGTH_LONG).show();
				finish();
				return;
			}

			((Button)findViewById(R.id.play_sound_c1)).OnClickListener = new OnClickListenerAnonymousInnerClassHelper(this);
		}

		private class StatusListenerAnonymousInnerClassHelper : SapaProcessor.StatusListener
		{
			private readonly SapaSimplePianoActivity outerInstance;

			public StatusListenerAnonymousInnerClassHelper(SapaSimplePianoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onKilled()
			{
				Log.v(TAG, "SapaSimplePiano will be closed. because of the SapaProcessor was closed.");
				outerInstance.mService.stop(true);
				finish();
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SapaSimplePianoActivity outerInstance;

			public OnClickListenerAnonymousInnerClassHelper(SapaSimplePianoActivity outerInstance)
			{
				this.outerInstance = outerInstance;
			}


			public override void onClick(View v)
			{
				// play sound c3
				const int noteC3 = 60;
				const int velocity = 90;
				outerInstance.mProcessor.sendCommand(string.Format("PLAY {0:D} {1:D}", noteC3, velocity));
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			if (mService != null)
			{
				if (mProcessor != null)
				{
					mService.unregister(mProcessor);
					mProcessor = null;
				}
				mService.stop(false);
			}
		}

		private void copyAssets()
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String mSoundFontDir = android.os.Environment.getExternalStoragePublicDirectory(android.os.Environment.DIRECTORY_DOWNLOADS).toString() + "/";
			string mSoundFontDir = Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_DOWNLOADS).ToString() + "/";
			string[] files = null;
			string mkdir = null;
			AssetManager assetManager = Assets;
			try
			{
				files = assetManager.list("");
			}
			catch (IOException e)
			{
				Log.e(TAG, e.Message);
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
				return;
			}

			for (int i = 0; i < files.Length; i++)
			{
				System.IO.Stream @in = null;
				System.IO.Stream @out = null;
				try
				{
					File tmp = new File(mSoundFontDir, files[i]);
					if (!tmp.exists())
					{
						@in = assetManager.open(files[i]);
					}

					if (@in == null)
					{
						continue;
					}

					mkdir = mSoundFontDir;
					File mpath = new File(mkdir);

					if (!mpath.Directory)
					{
						mpath.mkdirs();
					}

					@out = new System.IO.FileStream(mSoundFontDir + files[i], System.IO.FileMode.Create, System.IO.FileAccess.Write);

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final byte[] buffer = new byte[1024];
					sbyte[] buffer = new sbyte[1024];
					int read;

					while ((read = @in.Read(buffer, 0, buffer.Length)) != -1)
					{
						@out.Write(buffer, 0, read);
					}

					@in.Close();
					@in = null;
					@out.Flush();
					@out.Close();
					@out = null;
				}
				catch (Exception e)
				{
					Console.WriteLine(e.ToString());
					Console.Write(e.StackTrace);
				}
			}
		}

	}

}