using System;
using System.Collections.Generic;

namespace com.samsung.android.sdk.pass.sample
{


	using Activity = android.app.Activity;
	using BroadcastReceiver = android.content.BroadcastReceiver;
	using Context = android.content.Context;
	using Intent = android.content.Intent;
	using IntentFilter = android.content.IntentFilter;
	using Bundle = android.os.Bundle;
	using SparseArray = android.util.SparseArray;
	using Menu = android.view.Menu;
	using View = android.view.View;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using Button = android.widget.Button;
	using ListView = android.widget.ListView;
	using Toast = android.widget.Toast;


	public class SampleActivity : Activity
	{

		private SpassFingerprint mSpassFingerprint;
		private Spass mSpass;
		private Context mContext;
		private ListView mListView;
		private IList<string> mItemArray = new List<string>();
		private ArrayAdapter<string> mListAdapter;
		private bool onReadyIdentify = false;
		private bool onReadyEnroll = false;
		internal bool isFeatureEnabled = false;

		private BroadcastReceiver mPassReceiver = new BroadcastReceiverAnonymousInnerClassHelper();

		private class BroadcastReceiverAnonymousInnerClassHelper : BroadcastReceiver
		{
			public BroadcastReceiverAnonymousInnerClassHelper()
			{
			}

			public override void onReceive(Context context, Intent intent)
			{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String action = intent.getAction();
				string action = intent.Action;
				if (SpassFingerprint.ACTION_FINGERPRINT_RESET.Equals(action))
				{
					Toast.makeText(outerInstance.mContext, "all fingerprints are removed", Toast.LENGTH_SHORT).show();
				}
				else if (SpassFingerprint.ACTION_FINGERPRINT_REMOVED.Equals(action))
				{
					int fingerIndex = intent.getIntExtra("fingerIndex", 0);
					Toast.makeTextuniquetempvar.show();
				}
				else if (SpassFingerprint.ACTION_FINGERPRINT_ADDED.Equals(action))
				{
					int fingerIndex = intent.getIntExtra("fingerIndex", 0);
					Toast.makeTextuniquetempvar.show();
				}
			}
		}

		private void registerBroadcastReceiver()
		{
			IntentFilter filter = new IntentFilter();
			filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_RESET);
			filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_REMOVED);
			filter.addAction(SpassFingerprint.ACTION_FINGERPRINT_ADDED);
			mContext.registerReceiver(mPassReceiver, filter);
		};

		private void unregisterBroadcastReceiver()
		{
			try
			{
				if (mContext != null)
				{
					mContext.unregisterReceiver(mPassReceiver);
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e.ToString());
				Console.Write(e.StackTrace);
			}
		}

		private SpassFingerprint.IdentifyListener listener = new IdentifyListenerAnonymousInnerClassHelper();

		private class IdentifyListenerAnonymousInnerClassHelper : SpassFingerprint.IdentifyListener
		{
			public IdentifyListenerAnonymousInnerClassHelper()
			{
			}

			public override void onFinished(int eventStatus)
			{
				outerInstance.log("identify finished : reason=" + getEventStatusName(eventStatus));
				outerInstance.onReadyIdentify = false;
				int FingerprintIndex = 0;
				try
				{
					FingerprintIndex = outerInstance.mSpassFingerprint.IdentifiedFingerprintIndex;
				}
				catch (System.InvalidOperationException ise)
				{
					outerInstance.log(ise.Message);
				}
				if (eventStatus == SpassFingerprint.STATUS_AUTHENTIFICATION_SUCCESS)
				{
					outerInstance.log("onFinished() : Identify authentification Success with FingerprintIndex : " + FingerprintIndex);
				}
				else if (eventStatus == SpassFingerprint.STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS)
				{
					outerInstance.log("onFinished() : Password authentification Success");
				}
				else
				{
					outerInstance.log("onFinished() : Authentification Fail for identify");
				}
			}

			public override void onReady()
			{
				outerInstance.log("identify state is ready");
			}

			public override void onStarted()
			{
				outerInstance.log("User touched fingerprint sensor!");
			}
		}

		private SpassFingerprint.RegisterListener mRegisterListener = new RegisterListenerAnonymousInnerClassHelper();

		private class RegisterListenerAnonymousInnerClassHelper : SpassFingerprint.RegisterListener
		{
			public RegisterListenerAnonymousInnerClassHelper()
			{
			}


			public override void onFinished()
			{
				outerInstance.onReadyEnroll = false;
				outerInstance.log("RegisterListener.onFinished()");

			}
		}
		private static string getEventStatusName(int eventStatus)
		{
			switch (eventStatus)
			{
			case SpassFingerprint.STATUS_AUTHENTIFICATION_SUCCESS:
				return "STATUS_AUTHENTIFICATION_SUCCESS";
			case SpassFingerprint.STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS:
				return "STATUS_AUTHENTIFICATION_PASSWORD_SUCCESS";
			case SpassFingerprint.STATUS_TIMEOUT_FAILED:
				return "STATUS_TIMEOUT";
			case SpassFingerprint.STATUS_SENSOR_FAILED:
				return "STATUS_SENSOR_ERROR";
			case SpassFingerprint.STATUS_USER_CANCELLED:
				return "STATUS_USER_CANCELLED";
			case SpassFingerprint.STATUS_QUALITY_FAILED:
				return "STATUS_QUALITY_FAILED";
			case SpassFingerprint.STATUS_USER_CANCELLED_BY_TOUCH_OUTSIDE:
				return "STATUS_USER_CANCELLED_BY_TOUCH_OUTSIDE";
			case SpassFingerprint.STATUS_AUTHENTIFICATION_FAILED:
			default:
				return "STATUS_AUTHENTIFICATION_FAILED";
			}

		}
		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);
			ContentView = R.layout.activity_main;

			mContext = this;
			mListAdapter = new ArrayAdapter<string>(this, R.layout.list_entry, mItemArray);
			mListView = (ListView)findViewById(R.id.listView1);

			if (mListView != null)
			{
				mListView.Adapter = mListAdapter;
			}
			mSpass = new Spass();

			try
			{
				mSpass.initialize(SampleActivity.this);
			}
			catch (SsdkUnsupportedException e)
			{
				log("Exception: " + e);
			}
			catch (System.NotSupportedException)
			{
				log("Fingerprint Service is not supported in the device");
			}
			isFeatureEnabled = mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT);

			if (isFeatureEnabled)
			{
				mSpassFingerprint = new SpassFingerprint(SampleActivity.this);
				log("Fingerprint Service is supported in the device.");
				log("SDK version : " + mSpass.VersionName);
			}
			else
			{
				logClear();
				log("Fingerprint Service is not supported in the device.");
			}
			SparseArray<View.OnClickListener> listeners = new SparseArray<View.OnClickListener>();
			registerBroadcastReceiver();
			listeners.put(R.id.buttonHasRegisteredFinger, new OnClickListenerAnonymousInnerClassHelper(this, e));

			listeners.put(R.id.buttonIdentify, new OnClickListenerAnonymousInnerClassHelper2(this, e));

			listeners.put(R.id.buttonShowIdentifyDialogWithPW, new OnClickListenerAnonymousInnerClassHelper3(this, e));

			listeners.put(R.id.buttonShowIdentifyDialogWithoutPW, new OnClickListenerAnonymousInnerClassHelper4(this, e));

			listeners.put(R.id.buttonCancel, new OnClickListenerAnonymousInnerClassHelper5(this, e));
			listeners.put(R.id.buttonRegisterFinger, new OnClickListenerAnonymousInnerClassHelper6(this, e));
			listeners.put(R.id.buttonGetRegisteredFingerprintName, new OnClickListenerAnonymousInnerClassHelper7(this, e));
			listeners.put(R.id.buttonGetRegisteredFingerprintID, new OnClickListenerAnonymousInnerClassHelper8(this, e));

			listeners.put(R.id.buttonIdentifyWithIndex, new OnClickListenerAnonymousInnerClassHelper9(this, e));

			listeners.put(R.id.buttonShowIdentifyDialogWithIndex, new OnClickListenerAnonymousInnerClassHelper10(this, e));
			listeners.put(R.id.buttonShowIdentifyDialogWithTitleNLogo, new OnClickListenerAnonymousInnerClassHelper11(this, e));
			listeners.put(R.id.buttonCustomizedDialogWithTransparency, new OnClickListenerAnonymousInnerClassHelper12(this, e));
			listeners.put(R.id.buttonCustomizedDialogWithSetDialogDismiss, new OnClickListenerAnonymousInnerClassHelper13(this, e));
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int N = listeners.size();
			int N = listeners.size();
			for (int i = 0; i < N; i++)
			{
				int id = listeners.keyAt(i);
				Button button = (Button)findViewById(id);
				if (button != null)
				{
					button.OnClickListener = listeners.valueAt(i);
					button.setTextAppearance(mContext, R.style.ButtonStyle);
					if (!isFeatureEnabled)
					{
						button.Enabled = false;
					}
					else
					{
						if (id == R.id.buttonShowIdentifyDialogWithPW)
						{
							if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_AVAILABLE_PASSWORD))
							{
								button.Enabled = false;
							}
						}
						else if (id == R.id.buttonGetRegisteredFingerprintID)
						{
							if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_UNIQUE_ID))
							{
								button.Enabled = false;
							}
						}
						else if (id == R.id.buttonShowIdentifyDialogWithIndex || id == R.id.buttonIdentifyWithIndex)
						{
							if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX))
							{
								button.Enabled = false;
							}
						}
						else if (id == R.id.buttonShowIdentifyDialogWithTitleNLogo || id == R.id.buttonCustomizedDialogWithTransparency || id == R.id.buttonCustomizedDialogWithSetDialogDismiss)
						{
							if (!mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG))
							{
								button.Enabled = false;
							}
						}
					}
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					bool hasRegisteredFinger = outerInstance.mSpassFingerprint.hasRegisteredFinger();
					outerInstance.log("hasRegisteredFinger() = " + hasRegisteredFinger);
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper2 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper2(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							try
							{
								outerInstance.onReadyIdentify = true;
								outerInstance.mSpassFingerprint.startIdentify(listener);
								outerInstance.log("Please identify finger to verify you");
							}
							catch (SpassInvalidStateException ise)
							{
								outerInstance.onReadyIdentify = false;
								if (ise.Type == SpassInvalidStateException.STATUS_OPERATION_DENIED)
								{
									outerInstance.log("Exception: " + ise.Message);
								}
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper3 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper3(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							if (!outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_AVAILABLE_PASSWORD))
							{
								outerInstance.log("The Backup Password is not supported.");
							}
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, true);
								outerInstance.log("Please identify finger to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper4 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper4(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, false);
								outerInstance.log("Please identify finger to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper5 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper5(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (outerInstance.onReadyIdentify == true)
					{
						try
						{
							outerInstance.mSpassFingerprint.cancelIdentify();
							outerInstance.log("cancelIdentify is called");
						}
						catch (System.InvalidOperationException ise)
						{
							outerInstance.log(ise.Message);
						}
						outerInstance.onReadyIdentify = false;
					}
					else
					{
						outerInstance.log("Please request Identify first");
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper6 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper6(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (outerInstance.onReadyIdentify == false)
					{
						if (outerInstance.onReadyEnroll == false)
						{
							outerInstance.onReadyEnroll = true;
							outerInstance.mSpassFingerprint.registerFinger(outerInstance, mRegisterListener);
							outerInstance.log("Jump to the Enroll screen");
						}
						else
						{
							outerInstance.log("Please wait and try to register again");
						}
					}
					else
					{
						outerInstance.log("Please cancel Identify first");
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper7 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper7(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					outerInstance.log("=Fingerprint Name=");
					SparseArray<string> mList = outerInstance.mSpassFingerprint.RegisteredFingerprintName;
					if (mList == null)
					{
						outerInstance.log("Registered fingerprint is not existed.");
					}
					else
					{
						for (int i = 0; i < mList.size(); i++)
						{
							int index = mList.keyAt(i);
							string name = mList.get(index);
							outerInstance.log("index " + index + ", Name is " + name);
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper8 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper8(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_UNIQUE_ID))
					{
						SparseArray<string> mList = null;
						try
						{
							outerInstance.log("=Fingerprint Unique ID=");
							mList = outerInstance.mSpassFingerprint.RegisteredFingerprintUniqueID;
							if (mList == null)
							{
								outerInstance.log("Registered fingerprint is not existed.");
							}
							else
							{
								for (int i = 0; i < mList.size(); i++)
								{
									int index = mList.keyAt(i);
									string ID = mList.get(index);
									outerInstance.log("index " + index + ", Unique ID is " + ID);
								}
							}
						}
						catch (System.InvalidOperationException ise)
						{
							outerInstance.log(ise.Message);
						}
					}
					else
					{
						outerInstance.log("To get Fingerprint ID is not supported in the device");
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper9 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper9(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							try
							{
								outerInstance.onReadyIdentify = true;
								if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX))
								{
									List<int?> designatedFingers = new List<int?>();
									designatedFingers.Add(1);
									outerInstance.mSpassFingerprint.IntendedFingerprintIndex = designatedFingers;
								}
								outerInstance.mSpassFingerprint.startIdentify(listener);
								outerInstance.log("Please identify fingerprint index 1 to verify you");
							}
							catch (SpassInvalidStateException ise)
							{
								outerInstance.onReadyIdentify = false;
								if (ise.Type == SpassInvalidStateException.STATUS_OPERATION_DENIED)
								{
									outerInstance.log("Exception: " + ise.Message);
								}
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper10 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper10(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_FINGER_INDEX))
							{
								List<int?> designatedFingers = new List<int?>();
								designatedFingers.Add(2);
								designatedFingers.Add(3);
								try
								{
									outerInstance.mSpassFingerprint.IntendedFingerprintIndex = designatedFingers;
								}
								catch (System.InvalidOperationException ise)
								{
									outerInstance.log(ise.Message);
								}
							}
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, true);
								outerInstance.log("Please identify fingerprint index 2,3 to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper11 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper11(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG))
							{
								try
								{
									outerInstance.mSpassFingerprint.setDialogTitle("Customized Dialog With Logo", 0x000000);
									outerInstance.mSpassFingerprint.DialogIcon = "ic_launcher";
								}
								catch (System.InvalidOperationException ise)
								{
									outerInstance.log(ise.Message);
								}
							}
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, false);
								outerInstance.log("Please Identify fingerprint to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper12 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper12(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG))
							{
								try
								{
									outerInstance.mSpassFingerprint.setDialogTitle("Customized Dialog With Transparency", 0x000000);
									outerInstance.mSpassFingerprint.DialogBgTransparency = 0;
								}
								catch (System.InvalidOperationException ise)
								{
									outerInstance.log(ise.Message);
								}
							}
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, false);
								outerInstance.log("Please identify fingerprint to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		private class OnClickListenerAnonymousInnerClassHelper13 : View.OnClickListener
		{
			private readonly SampleActivity outerInstance;

			private SsdkUnsupportedException e;

			public OnClickListenerAnonymousInnerClassHelper13(SampleActivity outerInstance, SsdkUnsupportedException e)
			{
				this.outerInstance = outerInstance;
				this.e = e;
			}

			public override void onClick(View v)
			{
				outerInstance.logClear();
				try
				{
					if (!outerInstance.mSpassFingerprint.hasRegisteredFinger())
					{
						outerInstance.log("Please register finger first");
					}
					else
					{
						if (outerInstance.onReadyIdentify == false)
						{
							outerInstance.onReadyIdentify = true;
							if (outerInstance.mSpass.isFeatureEnabled(Spass.DEVICE_FINGERPRINT_CUSTOMIZED_DIALOG))
							{
								try
								{
									outerInstance.mSpassFingerprint.setDialogTitle("Customized Dialog With Setting Dialog dismiss", 0x000000);
									outerInstance.mSpassFingerprint.CanceledOnTouchOutside = true;
								}
								catch (System.InvalidOperationException ise)
								{
									outerInstance.log(ise.Message);
								}
							}
							try
							{
								outerInstance.mSpassFingerprint.startIdentifyWithDialog(outerInstance, listener, false);
								outerInstance.log("Please identify fingerprint to verify you");
							}
							catch (System.InvalidOperationException e)
							{
								outerInstance.onReadyIdentify = false;
								outerInstance.log("Exception: " + e);
							}
						}
						else
						{
							outerInstance.log("Please cancel Identify first");
						}
					}
				}
				catch (System.NotSupportedException)
				{
					outerInstance.log("Fingerprint Service is not supported in the device");
				}
			}
		}

		protected internal override void onDestroy()
		{
			base.onDestroy();
			unregisterBroadcastReceiver();
		}

		public override bool onCreateOptionsMenu(Menu menu)
		{
			// Inflate the menu; this adds items to the action bar if it is present.
			MenuInflater.inflate(R.menu.main, menu);
			return true;
		}

		public virtual void log(string text)
		{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final String txt = text;
			string txt = text;

			runOnUiThread(() =>
			{
				mItemArray.Insert(0, txt);
				mListAdapter.notifyDataSetChanged();
			});
		}

		public virtual void logClear()
		{
			if (mItemArray != null)
			{
				mItemArray.Clear();
			}
			if (mListAdapter != null)
			{
				mListAdapter.notifyDataSetChanged();
			}
		}

	}

}