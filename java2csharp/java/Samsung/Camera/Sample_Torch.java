package com.samsung.android.sdk.camera.sample.cases;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.hardware.camera2.CameraAccessException;
import android.os.Build;
import android.os.Bundle;
import android.util.Log;
import android.view.TextureView;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;
import android.widget.ToggleButton;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.camera.SCamera;
import com.samsung.android.sdk.camera.SCameraCharacteristics;
import com.samsung.android.sdk.camera.SCameraManager;
import com.samsung.android.sdk.camera.sample.R;

import java.util.HashMap;
import java.util.Map;

public class Sample_Torch extends Activity {
    private static final String TAG = Sample_Torch.class.getSimpleName();
    private SCamera mSCamera;
    private SCameraManager mSCameraManager;

    private Map<String, ToggleButton> mCameraIdViewMap;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_torch);
    }

    @Override
    protected void onResume() {
        super.onResume();

        // initialize SCamera
        mSCamera = new SCamera();
        try {
            mSCamera.initialize(this);
        } catch (SsdkUnsupportedException e) {
            showAlertDialog("Fail to initialize SCamera.", true);
            return;
        }

        if(!checkRequiredFeatures()) return;

        createUI();
    }

    private void createUI() {

        ViewGroup torchButtons = (ViewGroup) findViewById(R.id.torch_buttons_layout);
        torchButtons.removeAllViews();

        mCameraIdViewMap = new HashMap<>();
        mSCameraManager = mSCamera.getSCameraManager();

        int numberOfCameraWithFlash = 0;

        try {
            for(String id : mSCameraManager.getCameraIdList()) {
                ViewGroup torchItem = (ViewGroup)getLayoutInflater().inflate(R.layout.torch_item, null);
                torchButtons.addView(torchItem);

                ((TextView)torchItem.findViewById(R.id.cameraId_textView)).setText(id);

                ToggleButton toggleButton = (ToggleButton)torchItem.findViewById(R.id.toggleButton);
                toggleButton.setTag(id);

                if(mSCameraManager.getCameraCharacteristics(id).get(SCameraCharacteristics.FLASH_INFO_AVAILABLE)) {
                    numberOfCameraWithFlash++;

                    toggleButton.setOnClickListener(new View.OnClickListener() {
                        @Override
                        public void onClick(View v) {
                            String cameraId = (String) v.getTag();
                            try {
                                mSCameraManager.setTorchMode(cameraId, ((ToggleButton) v).isChecked());
                            } catch (Exception e) {
                                showAlertDialog("Fail to change torch mode for camera with id(" + cameraId + ").", true);
                            }
                        }
                    });
                } else {
                    toggleButton.setChecked(false);
                    toggleButton.setEnabled(false);
                }
                mCameraIdViewMap.put(id, toggleButton);
            }

            mSCameraManager.registerTorchCallback(new SCameraManager.TorchCallback() {
                @Override
                public void onTorchModeUnavailable(String cameraId) {
                    ToggleButton toggleButton = mCameraIdViewMap.get(cameraId);

                    if (null != toggleButton) {
                        toggleButton.setEnabled(false);
                    }
                }

                @Override
                public void onTorchModeChanged(String cameraId, boolean enabled) {
                    ToggleButton toggleButton = mCameraIdViewMap.get(cameraId);

                    if (null != toggleButton) {
                        //Enable toggle button as mode changed is invoked.
                        toggleButton.setEnabled(true);

                        if (toggleButton.isChecked() != enabled)
                            toggleButton.setChecked(enabled);
                    }
                }
            }, null);
        } catch (CameraAccessException e) {
            e.printStackTrace();
        }

        ((TextView)findViewById(R.id.totalNumber_textView)).setText("Number of camera device with flash unit = " + numberOfCameraWithFlash);
    }

    private boolean checkRequiredFeatures() {
        if(Build.VERSION.SDK_INT < Build.VERSION_CODES.M) {
            showAlertDialog("Device running Android prior to M is not compatible with torch mode control APIs.", true);
            Log.e(TAG, "Device running Android prior to M is not compatible with torch mode control APIs.");
            return false;
        }

        try {
            boolean flashFound = false;

            for(String id : mSCamera.getSCameraManager().getCameraIdList()) {
                SCameraCharacteristics cameraCharacteristics = mSCamera.getSCameraManager().getCameraCharacteristics(id);
                if(cameraCharacteristics.get(SCameraCharacteristics.FLASH_INFO_AVAILABLE)) {
                    flashFound = true;
                    break;
                }
            }

            // no camera device with flash unit.
            if(!flashFound) {
                showAlertDialog("Device does not have a camera device with flash unit.", true);
                Log.e(TAG, "Device does not have a camera device with flash unit.");

                return false;
            }

        } catch (CameraAccessException e) {
            showAlertDialog("Cannot access the camera.", true);
            Log.e(TAG, "Cannot access the camera.", e);
            return false;
        }
        return true;
    }

    /**
     * Shows alert dialog.
     */
    private void showAlertDialog(String message, final boolean finishActivity) {

        final AlertDialog.Builder dialog = new AlertDialog.Builder(this);
        dialog.setMessage(message)
                .setIcon(android.R.drawable.ic_dialog_alert)
                .setTitle("Alert")
                .setPositiveButton(android.R.string.ok, new DialogInterface.OnClickListener() {
                    @Override
                    public void onClick(DialogInterface dialog, int which) {
                        dialog.dismiss();
                        if (finishActivity) finish();
                    }
                }).setCancelable(false);

        runOnUiThread(new Runnable() {
            @Override
            public void run() {
                dialog.show();
            }
        });
    }
}
