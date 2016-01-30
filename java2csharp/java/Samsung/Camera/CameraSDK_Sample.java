package com.samsung.android.sdk.camera.sample;

import android.Manifest;
import android.app.Activity;
import android.app.AlertDialog;
import android.app.Dialog;
import android.app.DialogFragment;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.Bundle;
import android.support.annotation.DrawableRes;
import android.support.annotation.NonNull;
import android.support.v4.app.ActivityCompat;
import android.support.v4.content.ContextCompat;
import android.util.Log;
import android.view.View;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import com.samsung.android.sdk.camera.sample.cases.Sample_Constrained_HighSpeedVideo;
import com.samsung.android.sdk.camera.sample.cases.Sample_DOF;
import com.samsung.android.sdk.camera.sample.cases.Sample_Effect;
import com.samsung.android.sdk.camera.sample.cases.Sample_Filter;
import com.samsung.android.sdk.camera.sample.cases.Sample_HDR;
import com.samsung.android.sdk.camera.sample.cases.Sample_HighSpeedVideo;
import com.samsung.android.sdk.camera.sample.cases.Sample_Image;
import com.samsung.android.sdk.camera.sample.cases.Sample_LowLight;
import com.samsung.android.sdk.camera.sample.cases.Sample_Panorama;
import com.samsung.android.sdk.camera.sample.cases.Sample_Single;
import com.samsung.android.sdk.camera.sample.cases.Sample_Torch;
import com.samsung.android.sdk.camera.sample.cases.Sample_YUV;
import com.samsung.android.sdk.camera.sample.cases.Sample_ZSL;

import java.util.ArrayList;

public class CameraSDK_Sample extends Activity {

    private final String SAMPLE_SINGLE = "Single";
    private final String SAMPLE_EFFECT = "Effect";
    private final String SAMPLE_HDR = "High Dynamic Range";
    private final String SAMPLE_LLS = "Low Light";
    private final String SAMPLE_PANORAMA = "Panorama";
    private final String SAMPLE_DOF = "Depth of Field";
    private final String SAMPLE_FILTER = "Filter";
    private final String SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO = "Constrained High Speed Video Recording";
    private final String SAMPLE_HIGHSPEEDVIDEO = "High Speed Video Recording via Scene Mode";
    private final String SAMPLE_IPX = "Image Processing Accelerator";
    private final String SAMPLE_TORCH = "Torch";
    private final String SAMPLE_ZSL = "Opaque reprocessing (Simple ZSL)";
    private final String SAMPLE_YUV = "YUV reprocessing";
    private final String SAMPLE_VERSION = "Version";

    private final String SAMPLE_NAMES_LIST[] = {
        SAMPLE_SINGLE,
        SAMPLE_EFFECT,
        SAMPLE_HDR,
        SAMPLE_LLS,
        SAMPLE_PANORAMA,
        SAMPLE_DOF,
        SAMPLE_FILTER,
        SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO,
        SAMPLE_HIGHSPEEDVIDEO,
        SAMPLE_IPX,
        SAMPLE_TORCH,
        SAMPLE_ZSL,
        SAMPLE_YUV,
        SAMPLE_VERSION,
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_camera_sdk_sample);

        createUI();

        if(savedInstanceState == null) {
            //Check permissions
            ensurePermissions(Manifest.permission.CAMERA,
                    Manifest.permission.RECORD_AUDIO,
                    Manifest.permission.READ_EXTERNAL_STORAGE,
                    Manifest.permission.WRITE_EXTERNAL_STORAGE);
        }
    }

    @Override
    protected void onResume() {
        Log.e("SEC", "onResume");
        super.onResume();


    }

    private void ensurePermissions(String... permissions) {
        ArrayList<String> deniedPermissionList = new ArrayList<>();

        for(String permission : permissions) {
            if(PackageManager.PERMISSION_GRANTED != ContextCompat.checkSelfPermission(this, permission)) deniedPermissionList.add(permission);
        }

        if(!deniedPermissionList.isEmpty()) ActivityCompat.requestPermissions(this, deniedPermissionList.toArray(new String[0]), 0);
    }

    @Override
    public void onRequestPermissionsResult(int requestCode, String[] permissions, int[] grantResults) {
        for(int result : grantResults) {
            if(result != PackageManager.PERMISSION_GRANTED) {
                showAlertDialog("Requested permission is not granted.", true);
            }
        }
    }

    @Override
    protected void onSaveInstanceState(Bundle outState) {
        Log.e("SEC", "onSaveInstanceState");
        super.onSaveInstanceState(outState);
    }

    @Override
    protected void onPause() {
        Log.e("SEC", "onPause");
        super.onPause();
    }

    /**
     * Shows alert dialog.
     */
    private void showAlertDialog(final String message, final boolean finishActivity) {
        AlertDialogFragment.newInstance(android.R.drawable.ic_dialog_alert,
                "Alert",
                message,
                finishActivity).show(getFragmentManager(), "alert_dialog");
    }

    private void createUI() {

        ArrayAdapter arrayAdapter = new ArrayAdapter<String>(this, android.R.layout.simple_list_item_1, SAMPLE_NAMES_LIST);

        ListView listView = (ListView) findViewById(R.id.sample_list);
        listView.setAdapter(arrayAdapter);
        listView.setOnItemClickListener(new AdapterView.OnItemClickListener() {
            @Override
            public void onItemClick(AdapterView<?> parent, View view, int position, long id) {

                if (position >= SAMPLE_NAMES_LIST.length) return;

                switch (SAMPLE_NAMES_LIST[position]) {
                    case SAMPLE_SINGLE: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Single.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_EFFECT: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Effect.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_HDR: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_HDR.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_LLS: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_LowLight.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_PANORAMA: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Panorama.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_DOF: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_DOF.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_FILTER: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Filter.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_CONSTRAINED_HIGHSPEEDVIDEO: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Constrained_HighSpeedVideo.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_HIGHSPEEDVIDEO: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_HighSpeedVideo.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_IPX: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Image.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_TORCH: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_Torch.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_ZSL: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_ZSL.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_YUV: {
                        Intent intent = new Intent(CameraSDK_Sample.this, Sample_YUV.class);
                        startActivity(intent);
                        break;
                    }

                    case SAMPLE_VERSION: {
                        try {
                            StringBuilder builder = new StringBuilder();
                            PackageInfo packageInfo = getPackageManager().getPackageInfo(getPackageName(), 0);

                            builder.append(String.format("Version code: %d\n", packageInfo.versionCode))
                                    .append(String.format("Version name: %s\n", packageInfo.versionName));

                            AlertDialogFragment.newInstance(android.R.drawable.ic_dialog_info,
                                    "CameraSDK Sample Application Version",
                                    builder.toString(),
                                    false).show(getFragmentManager(), "info_dialog");
                            break;

                        } catch (PackageManager.NameNotFoundException e) {
                            // This should not happen.
                        }
                    }
                }
            }
        });
    }

    /** Alert Dialog Fragment */
    public static class AlertDialogFragment extends DialogFragment {
        public static AlertDialogFragment newInstance(@DrawableRes int iconId, CharSequence title, CharSequence message, boolean finishActivity) {
            AlertDialogFragment fragment = new AlertDialogFragment();

            Bundle args = new Bundle();
            args.putInt("icon", iconId);
            args.putCharSequence("title", title);
            args.putCharSequence("message", message);
            args.putBoolean("finish_activity", finishActivity);

            fragment.setArguments(args);
            return fragment;
        }

        @NonNull
        @Override
        public Dialog onCreateDialog(Bundle savedInstanceState) {
            boolean finishActivity = getArguments().getBoolean("finish_activity");

            return new AlertDialog.Builder(getActivity())
                    .setTitle(getArguments().getCharSequence("title"))
                    .setIcon(getArguments().getInt("icon"))
                    .setMessage(getArguments().getCharSequence("message"))
                    .setPositiveButton(android.R.string.ok, finishActivity ?
                            new DialogInterface.OnClickListener() {
                                @Override
                                public void onClick(DialogInterface dialog, int which) {
                                    dialog.dismiss();
                                    getActivity().finish();
                                }
                            } : null)
                    .setCancelable(false)
                    .create();
        }
    }
}
