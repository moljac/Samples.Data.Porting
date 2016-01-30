/**
 * Copyright (C) 2013 Samsung Electronics Co., Ltd. All rights reserved.
 *
 * Mobile Communication Division,
 * Digital Media & Communications Business, Samsung Electronics Co., Ltd.
 *
 * This software and its documentation are confidential and proprietary
 * information of Samsung Electronics Co., Ltd.  No part of the software and
 * documents may be copied, reproduced, transmitted, translated, or reduced to
 * any electronic medium or machine-readable form without the prior written
 * consent of Samsung Electronics.
 *
 * Samsung Electronics makes no representations with respect to the contents,
 * and assumes no responsibility for any errors that might appear in the
 * software and documents. This publication and the contents hereof are subject
 * to change without notice.
 */

package com.samsung.android.sdk.chord.example;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.AdapterView.OnItemClickListener;
import android.widget.ListView;

import java.io.ByteArrayOutputStream;
import java.io.InputStream;

public class ChordSampleAppActivity extends Activity implements OnItemClickListener {

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.chord_sample_app_activity);

        mMainMenuListView = (ListView) findViewById(R.id.main_menu_listview);
        ArrayAdapter<String> mMain_adapter = new ArrayAdapter<String>(getBaseContext(),
                R.layout.simple_list_item, menu);
        mMainMenuListView.setAdapter(mMain_adapter);
        mMainMenuListView.setOnItemClickListener(this);

    }

    @Override
    public void onItemClick(AdapterView<?> parent, View view, int position, long id) {
        if (position == LICENSEVIEW) {
            showLicenseViewDialog();
            return;
        }

        Intent intent;
        switch (position) {
            case HELLOCHORDFRAGMENT:
                intent = new Intent(ChordSampleAppActivity.this, HelloChordActivity.class);
                startActivity(intent);
                break;
            case SENDFILESFRAGMENT:
                intent = new Intent(ChordSampleAppActivity.this, SendFilesActivity.class);
                startActivity(intent);
                break;
            case USESECURECHANNEL:
                intent = new Intent(ChordSampleAppActivity.this, UseSecureChannelActivity.class);
                startActivity(intent);
                break;
            case UDPFRAMEWORK:
                intent = new Intent(ChordSampleAppActivity.this, UdpFrameworkActivity.class);
                startActivity(intent);
                break;
            default:
                break;
        }
    }

    private void showLicenseViewDialog() {
        String license;
        InputStream is = getResources().openRawResource(R.raw.license);
        ByteArrayOutputStream bs = new ByteArrayOutputStream();

        try {
            int i = is.read();
            while (i != -1) {
                bs.write(i);
                i = is.read();
            }

        } catch (Exception e) {

        } finally {
            try {
                is.close();
            } catch (Exception e) {

            }
        }

        license = bs.toString();

        AlertDialog alertDialog = new AlertDialog.Builder(this).setTitle(R.string.license)
                .setMessage(license).create();
        alertDialog.show();
    }

    private static final String[] menu = {
            "Hello Chord", "Send Files", "Use Secure Channel", "Udp Framework", "License"
    };

    private static final int HELLOCHORDFRAGMENT = 0;

    private static final int SENDFILESFRAGMENT = 1;

    private static final int USESECURECHANNEL = 2;

    private static final int UDPFRAMEWORK = 3;

    private static final int LICENSEVIEW = 4;

    private ListView mMainMenuListView;

}
