using System;

/// <summary>
/// Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
/// 
/// Mobile Communication Division,
/// Digital Media & Communications Business, Samsung Electronics Co., Ltd.
/// 
/// This software and its documentation are confidential and proprietary
/// information of Samsung Electronics Co., Ltd.  No part of the software and
/// documents may be copied, reproduced, transmitted, translated, or reduced to
/// any electronic medium or machine-readable form without the prior written
/// consent of Samsung Electronics.
/// 
/// Samsung Electronics makes no representations with respect to the contents,
/// and assumes no responsibility for any errors that might appear in the
/// software and documents. This publication and the contents hereof are subject
/// to change without notice.
/// </summary>

namespace com.samsung.android.sdk.chord.example
{

	using Activity = android.app.Activity;
	using AlertDialog = android.app.AlertDialog;
	using Intent = android.content.Intent;
	using Bundle = android.os.Bundle;
	using View = android.view.View;
	using AdapterView = android.widget.AdapterView;
	using ArrayAdapter = android.widget.ArrayAdapter;
	using OnItemClickListener = android.widget.AdapterView.OnItemClickListener;
	using ListView = android.widget.ListView;


	public class ChordSampleAppActivity : Activity, AdapterView.OnItemClickListener
	{

		protected internal override void onCreate(Bundle savedInstanceState)
		{
			base.onCreate(savedInstanceState);

			ContentView = R.layout.chord_sample_app_activity;

			mMainMenuListView = (ListView) findViewById(R.id.main_menu_listview);
			ArrayAdapter<string> mMain_adapter = new ArrayAdapter<string>(BaseContext, R.layout.simple_list_item, menu);
			mMainMenuListView.Adapter = mMain_adapter;
			mMainMenuListView.OnItemClickListener = this;

		}

		public override void onItemClick<T1>(AdapterView<T1> parent, View view, int position, long id)
		{
			if (position == LICENSEVIEW)
			{
				showLicenseViewDialog();
				return;
			}

			Intent intent;
			switch (position)
			{
				case HELLOCHORDFRAGMENT:
					intent = new Intent(ChordSampleAppActivity.this, typeof(HelloChordActivity));
					startActivity(intent);
					break;
				case SENDFILESFRAGMENT:
					intent = new Intent(ChordSampleAppActivity.this, typeof(SendFilesActivity));
					startActivity(intent);
					break;
				case USESECURECHANNEL:
					intent = new Intent(ChordSampleAppActivity.this, typeof(UseSecureChannelActivity));
					startActivity(intent);
					break;
				case UDPFRAMEWORK:
					intent = new Intent(ChordSampleAppActivity.this, typeof(UdpFrameworkActivity));
					startActivity(intent);
					break;
				default:
					break;
			}
		}

		private void showLicenseViewDialog()
		{
			string license;
			System.IO.Stream @is = Resources.openRawResource(R.raw.license);
			ByteArrayOutputStream bs = new ByteArrayOutputStream();

			try
			{
				int i = @is.Read();
				while (i != -1)
				{
					bs.write(i);
					i = @is.Read();
				}

			}
			catch (Exception)
			{

			}
			finally
			{
				try
				{
					@is.Close();
				}
				catch (Exception)
				{

				}
			}

			license = bs.ToString();

			AlertDialog alertDialog = (new AlertDialog.Builder(this)).setTitle(R.@string.license).setMessage(license).create();
			alertDialog.show();
		}

		private static readonly string[] menu = new string[] {"Hello Chord", "Send Files", "Use Secure Channel", "Udp Framework", "License"};

		private const int HELLOCHORDFRAGMENT = 0;

		private const int SENDFILESFRAGMENT = 1;

		private const int USESECURECHANNEL = 2;

		private const int UDPFRAMEWORK = 3;

		private const int LICENSEVIEW = 4;

		private ListView mMainMenuListView;

	}

}