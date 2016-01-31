
package com.samsung.sdk.motion.test;

import com.samsung.android.sdk.SsdkUnsupportedException;
import com.samsung.android.sdk.motion.Smotion;
import com.samsung.android.sdk.motion.SmotionActivity;
import com.samsung.android.sdk.motion.SmotionActivityNotification;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.media.AudioManager;
import android.media.SoundPool;
import android.os.Bundle;
import android.os.Looper;
import android.text.format.Time;
import android.util.Log;
import android.view.Gravity;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.AdapterView.OnItemSelectedListener;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.CompoundButton;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.RadioButton;
import android.widget.RadioGroup;
import android.widget.RadioGroup.OnCheckedChangeListener;
import android.widget.RelativeLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;
import android.widget.ToggleButton;

import java.util.Formatter;

public class MotionTest extends Activity {

    static final int MODE_CALL = 0;

    static final int MODE_PEDOMETER = 1;

    static final int MODE_PEDOMETER_PERIODIC = 2;

    static final int MODE_ACTIVITY = 3;

    static final int MODE_ACTIVITY_NOTIFICATION = 4;

    static int mTestMode = 0;

    private Smotion mMotion;

    private static SoundPool mSoundPool;

    private MotionCall mCall;

    private MotionPedometer mPedometer;

    private MotionActivity mActivity;

    private MotionActivityNotification mActivityNotification;

    private RadioGroup mRadioGroup_activity_mode = null;

    private RadioButton mRadio_acitivity_realtime = null;

    private RadioButton mRadio_acitivity_batch = null;

    private RadioButton mRadio_acitivity_all = null;

    private LinearLayout mCheckBox_Group = null;

    private LinearLayout mCheckBox_Group2 = null;

    private CheckBox mCheckBox_Stationary = null;

    private CheckBox mCheckBox_Walk = null;

    private CheckBox mCheckBox_Run = null;

    private CheckBox mCheckBox_Vehicle = null;

    private CheckBox mCheckBox_Activity_Periodic = null;

    private EditText mEditText_Activity_Periodic_input = null;

    private LinearLayout mLinearLayout_Activity_Periodic_interval = null;

    private Spinner mSpin = null;

    private ToggleButton mBtn_start = null;

    private static boolean mIsTesting = false;

    private static int mSoundId;

    private static TextView mTv_timestamp = null;

    private static TextView mTv_result1 = null;

    private static TextView mTv_result2 = null;

    private static Button mBtn_updateInfo = null;

    private int mActivityMode = SmotionActivity.Info.MODE_REALTIME;

    private RelativeLayout mRelativeLayout;

    static boolean mIsUpdateInfo = false;

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        if (initialize()) {
            mBtn_start.setEnabled(true);
            mSpin.setClickable(true);
            mTv_timestamp.setVisibility(View.VISIBLE);
        } else {
            MotionTest.displayData(0, "Not supported", "");
        }
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        stopMotionTest();
    }

    public boolean initialize() {
        mTv_timestamp = (TextView) findViewById(R.id.res_timestamp);
        mTv_result1 = (TextView) findViewById(R.id.res_result1);
        mTv_result2 = (TextView) findViewById(R.id.res_result2);
        mRelativeLayout = (RelativeLayout) findViewById(R.id.res_layout);

        // Smotion iniialize
        mMotion = new Smotion();

        try {
            mMotion.initialize(this);
        } catch (IllegalArgumentException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return false;
        } catch (SsdkUnsupportedException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return false;
        }

        // SoundPool
        this.setVolumeControlStream(AudioManager.STREAM_MUSIC);
        mSoundPool = new SoundPool(10, AudioManager.STREAM_MUSIC, 0);
        mSoundId = mSoundPool.load(getApplicationContext(), R.raw.dingdong, 0);

        // RadioGroup
        mRadioGroup_activity_mode = (RadioGroup) findViewById(R.id.radioGroup_activity_mode);
        mRadioGroup_activity_mode.setOnCheckedChangeListener(new OnCheckedChangeListener() {

            @Override
            public void onCheckedChanged(RadioGroup group, int checkedId) {
                // TODO Auto-generated method stub
                switch (checkedId) {
                    case R.id.radio_realtime:
                        mActivityMode = SmotionActivity.Info.MODE_REALTIME;
                        mCheckBox_Activity_Periodic.setVisibility(View.VISIBLE);
                        mBtn_updateInfo.setVisibility(View.VISIBLE);
                        if (mCheckBox_Activity_Periodic.isChecked()) {
                            mCheckBox_Activity_Periodic.setChecked(false);
                        }
                        break;
                    case R.id.radio_batch:
                        mActivityMode = SmotionActivity.Info.MODE_BATCH;
                        mCheckBox_Activity_Periodic.setVisibility(View.GONE);
                        mBtn_updateInfo.setVisibility(View.VISIBLE);
                        mBtn_updateInfo.setEnabled(false);
                        mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                        if (mCheckBox_Activity_Periodic.isChecked()) {
                            mCheckBox_Activity_Periodic.setChecked(false);
                        }
                        break;
                    case R.id.radio_all:
                        mActivityMode = SmotionActivity.Info.MODE_ALL;
                        mCheckBox_Activity_Periodic.setVisibility(View.VISIBLE);
                        mBtn_updateInfo.setVisibility(View.VISIBLE);
                        mBtn_updateInfo.setEnabled(false);
                        if (mCheckBox_Activity_Periodic.isChecked()) {
                            mCheckBox_Activity_Periodic.setChecked(false);
                        }
                        break;
                    default:
                        break;
                }
            }
        });

        // RadioButton
        mRadio_acitivity_realtime = (RadioButton) findViewById(R.id.radio_realtime);
        mRadio_acitivity_batch = (RadioButton) findViewById(R.id.radio_batch);
        mRadio_acitivity_all = (RadioButton) findViewById(R.id.radio_all);

        // ToggleButton Start/Stop
        mBtn_start = (ToggleButton) findViewById(R.id.toggle_start);
        mBtn_start.setOnClickListener(new ToggleButton.OnClickListener() {
            @Override
            public void onClick(View view) {
                // Perform action on clicks
                if (mBtn_start.isChecked()) {
                    startMotionTest();
                } else {
                    stopMotionTest();
                }
            }
        });
        // CheckBox Group
        mCheckBox_Group = (LinearLayout) findViewById(R.id.checkbox_group);
        mCheckBox_Group2 = (LinearLayout) findViewById(R.id.checkbox_group2);
        mCheckBox_Stationary = (CheckBox) findViewById(R.id.cb_stationary);
        mCheckBox_Walk = (CheckBox) findViewById(R.id.cb_walk);
        mCheckBox_Run = (CheckBox) findViewById(R.id.cb_run);
        mCheckBox_Vehicle = (CheckBox) findViewById(R.id.cb_vehicle);
        mCheckBox_Activity_Periodic = (CheckBox) findViewById(R.id.cb_activity_realtime_periodic);
        mEditText_Activity_Periodic_input = (EditText) findViewById(R.id.et_activity_realtime_periodic_input);
        mLinearLayout_Activity_Periodic_interval = (LinearLayout) findViewById(R.id.interval_group);

        // Spinner
        mSpin = (Spinner) findViewById(R.id.spinner_mode);
        mSpin.setOnItemSelectedListener(new OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int position,
                    long flag) {
                // TODO Auto-generated method stub
                mTestMode = position;
                mBtn_start.setEnabled(true);
                switch (mTestMode) {
                    case MODE_CALL:
                        mBtn_updateInfo.setVisibility(View.GONE);
                        setTextView(mTv_result1, true);
                        if (!mMotion.isFeatureEnabled(Smotion.TYPE_CALL)) {
                            mBtn_start.setEnabled(false);
                            MotionTest.displayData(0, "Not supported", "");
                        } else {
                            if (mCall == null) {
                                mCall = new MotionCall(Looper.getMainLooper(), mMotion);
                            }
                            mCall.initialize();
                        }
                        break;
                    case MODE_PEDOMETER:
                    case MODE_PEDOMETER_PERIODIC:
                        mBtn_updateInfo.setVisibility(View.GONE);
                        setTextView(mTv_result1, true);
                        if (!mMotion.isFeatureEnabled(Smotion.TYPE_PEDOMETER)) {
                            mBtn_start.setEnabled(false);
                            MotionTest.displayData(0, "Not supported", "");
                        } else {
                            if (mPedometer == null) {
                                boolean isPedometerUpDownAvailable = mMotion
                                        .isFeatureEnabled(Smotion.TYPE_PEDOMETER_WITH_UPDOWN_STEP);
                                mPedometer = new MotionPedometer(Looper.getMainLooper(), mMotion,
                                        isPedometerUpDownAvailable);
                            }
                            mPedometer.initialize();
                        }
                        break;
                    case MODE_ACTIVITY:

                        if (!mMotion.isFeatureEnabled(Smotion.TYPE_ACTIVITY)) {
                            mBtn_start.setEnabled(false);
                            mRadio_acitivity_realtime.setEnabled(false);
                            mRadio_acitivity_batch.setEnabled(false);
                            mRadio_acitivity_all.setEnabled(false);
                            mCheckBox_Activity_Periodic.setEnabled(false);
                            MotionTest.displayData(0, "Not supported", "");
                        } else {
                            mRadioGroup_activity_mode.check(R.id.radio_realtime);
                            setTextView(mTv_result1, false);
                            mBtn_updateInfo.setVisibility(View.VISIBLE);
                            mBtn_updateInfo.setEnabled(false);
                            if (mActivity == null) {
                                mActivity = new MotionActivity(Looper.getMainLooper(), mMotion);
                            }
                            mActivity.initialize();
                            displayActivityStatus(mActivity.checkActivityStatus());

                        }
                        break;
                    case MODE_ACTIVITY_NOTIFICATION:
                        mBtn_updateInfo.setVisibility(View.GONE);
                        setTextView(mTv_result1, false);
                        if (!mMotion.isFeatureEnabled(Smotion.TYPE_ACTIVITY_NOTIFICATION)) {
                            mBtn_start.setEnabled(false);
                            mCheckBox_Stationary.setEnabled(false);
                            mCheckBox_Walk.setEnabled(false);
                            mCheckBox_Run.setEnabled(false);
                            mCheckBox_Vehicle.setEnabled(false);
                            MotionTest.displayData(0, "Not supported", "");
                        } else {
                            if (mActivityNotification == null) {
                                mActivityNotification = new MotionActivityNotification(Looper
                                        .getMainLooper(), mMotion);
                            }
                            mActivityNotification.initialize();
                            displayActivityStatus(mActivityNotification.checkActivityStatus());
                        }
                        break;
                    default:
                        break;
                }
                initializeView();
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {
                // TODO Auto-generated method stub
            }
        });

        mCheckBox_Activity_Periodic
        .setOnCheckedChangeListener(new CompoundButton.OnCheckedChangeListener() {

            @Override
            public void onCheckedChanged(CompoundButton buttonView, boolean isChecked) {
                // TODO Auto-generated method stub
                if (isChecked) {
                    mLinearLayout_Activity_Periodic_interval.setVisibility(View.VISIBLE);
                } else {
                    mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                    String str = mEditText_Activity_Periodic_input.getText().toString();
                    if (str.equals("") || Long.parseLong(str) <= 0) {
                        str = "10";
                        mEditText_Activity_Periodic_input.setText(str);
                    }
                }
            }
        });

        mBtn_updateInfo = (Button) findViewById(R.id.btn_activity_batch_updateinfo);
        mBtn_updateInfo.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View arg0) {
                // TODO Auto-generated method stub
                if (mActivity != null) {
                    if (mActivity.isUpdateInfoBatchModeSupport()
                            || mActivityMode == SmotionActivity.Info.MODE_REALTIME) {
                        mIsUpdateInfo = true;
                        mActivity.updateInfo();
                    } else {
                        if (mActivityMode == SmotionActivity.Info.MODE_ALL) {
                            Log.d("activity", "mode all");
                            mIsUpdateInfo = true;
                            mActivity.updateInfo();
                        }
                        Toast.makeText(getApplicationContext(),
                                "This device doesn't support updateInfo() in batch mode!!",
                                Toast.LENGTH_SHORT).show();
                    }

                }
            }
        });

        return true;
    }

    private void startMotionTest() {
        mIsTesting = true;
        mSpin.setEnabled(false);
        switch (mTestMode) {
            case MODE_CALL:
                mCall.start();
                break;
            case MODE_PEDOMETER:
            case MODE_PEDOMETER_PERIODIC:
                mPedometer.start(mTestMode);
                break;
            case MODE_ACTIVITY:
                boolean isPeriodicMode = mCheckBox_Activity_Periodic.isChecked();
                String str = mEditText_Activity_Periodic_input.getText().toString();
                if (str.equals("") && isPeriodicMode) {
                    showAlertMessage("Input periodic time!");
                } else {
                    long interval = Long.parseLong(str) * 1000;
                    if (interval <= 0) {
                        showAlertMessage("Periodic time must be an excess of 0!");
                    } else {
                        mActivity.start(mActivityMode, isPeriodicMode, interval);
                        mRadio_acitivity_realtime.setEnabled(false);
                        mRadio_acitivity_batch.setEnabled(false);
                        mRadio_acitivity_all.setEnabled(false);
                        mBtn_updateInfo.setEnabled(true);
                        mCheckBox_Activity_Periodic.setVisibility(View.GONE);
                        mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                    }
                }
                break;
            case MODE_ACTIVITY_NOTIFICATION:
                boolean[] notificationFilter = {
                        // STATIONARY, WALK, RUN, VEHICLE
                        false, true, false, false, false
                };
                notificationFilter[0] = mCheckBox_Stationary.isChecked();
                notificationFilter[1] = mCheckBox_Walk.isChecked();
                notificationFilter[2] = mCheckBox_Run.isChecked();
                notificationFilter[3] = mCheckBox_Vehicle.isChecked();

                int count = 0;
                for (int i = 0; i < notificationFilter.length; i++) {
                    if (notificationFilter[i]) {
                        addActivity(i);
                        count++;
                    }
                }

                if (count == 0) {
                    showAlertMessage("Filter is not selected");
                } else {
                    mActivityNotification.start();

                    mCheckBox_Stationary.setEnabled(false);
                    mCheckBox_Walk.setEnabled(false);
                    mCheckBox_Run.setEnabled(false);
                    mCheckBox_Vehicle.setEnabled(false);
                }
                break;
            default:
                break;
        }
    }

    private void stopMotionTest() {
        if (mIsTesting) {
            mIsTesting = false;
            mSpin.setEnabled(true);
            switch (mTestMode) {
                case MODE_CALL:
                    mCall.stop();
                    break;
                case MODE_PEDOMETER:
                case MODE_PEDOMETER_PERIODIC:
                    mPedometer.stop();
                    break;
                case MODE_ACTIVITY:
                    mActivity.stop();
                    mRadio_acitivity_realtime.setEnabled(true);
                    mRadio_acitivity_batch.setEnabled(true);
                    mRadio_acitivity_all.setEnabled(true);
                    mCheckBox_Activity_Periodic.setEnabled(true);
                    mEditText_Activity_Periodic_input.setEnabled(true);
                    mBtn_updateInfo.setVisibility(View.VISIBLE);
                    mBtn_updateInfo.setEnabled(false);
                    if (mActivityMode == SmotionActivity.Info.MODE_REALTIME
                            || mActivityMode == SmotionActivity.Info.MODE_ALL) {
                        mCheckBox_Activity_Periodic.setVisibility(View.VISIBLE);
                    }
                    mBtn_updateInfo.setEnabled(false);
                    if (mCheckBox_Activity_Periodic.isChecked()) {
                        mCheckBox_Activity_Periodic.setVisibility(View.VISIBLE);
                        mLinearLayout_Activity_Periodic_interval.setVisibility(View.VISIBLE);
                    }
                    break;
                case MODE_ACTIVITY_NOTIFICATION:
                    mCheckBox_Stationary.setEnabled(true);
                    mCheckBox_Walk.setEnabled(true);
                    mCheckBox_Run.setEnabled(true);
                    mCheckBox_Vehicle.setEnabled(true);
                    mActivityNotification.stop();
                    break;
                default:
                    break;
            }
        }
    }

    private void addActivity(int activity_type) {
        switch (activity_type) {
            case 0:
                mActivityNotification
                .addActivity(SmotionActivityNotification.Info.STATUS_STATIONARY);
                break;
            case 1:
                mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_WALK);
                break;
            case 2:
                mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_RUN);
                break;
            case 3:
                mActivityNotification.addActivity(SmotionActivityNotification.Info.STATUS_VEHICLE);
                break;
            default:
                break;
        }
    }

    private void initializeView() {
        switch (mTestMode) {
            case MODE_CALL:
                mTv_timestamp.setVisibility(View.VISIBLE);
                mTv_result1.setVisibility(View.VISIBLE);
                mTv_result2.setVisibility(View.GONE);
                mCheckBox_Group.setVisibility(View.GONE);
                mCheckBox_Group2.setVisibility(View.GONE);
                mRadioGroup_activity_mode.setVisibility(View.GONE);
                mCheckBox_Activity_Periodic.setVisibility(View.GONE);
                mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                break;
            case MODE_PEDOMETER:
            case MODE_PEDOMETER_PERIODIC:
                mTv_timestamp.setVisibility(View.VISIBLE);
                mTv_result1.setVisibility(View.VISIBLE);
                mTv_result2.setVisibility(View.VISIBLE);
                mCheckBox_Group.setVisibility(View.GONE);
                mCheckBox_Group2.setVisibility(View.GONE);
                mRadioGroup_activity_mode.setVisibility(View.GONE);
                mCheckBox_Activity_Periodic.setVisibility(View.GONE);
                mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                break;
            case MODE_ACTIVITY:
                mTv_timestamp.setVisibility(View.GONE);
                mTv_result1.setVisibility(View.VISIBLE);
                mTv_result2.setVisibility(View.VISIBLE);
                mCheckBox_Group.setVisibility(View.GONE);
                mCheckBox_Group2.setVisibility(View.GONE);
                mRadioGroup_activity_mode.setVisibility(View.VISIBLE);
                if (mRadio_acitivity_batch.isChecked()) {
                    mRadio_acitivity_batch.setChecked(false);
                    mRadio_acitivity_realtime.setChecked(true);
                }
                mCheckBox_Activity_Periodic.setVisibility(View.VISIBLE);
                if (mCheckBox_Activity_Periodic.isChecked()) {
                    mCheckBox_Activity_Periodic.setChecked(false);
                }
                mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                break;
            case MODE_ACTIVITY_NOTIFICATION:
                mTv_timestamp.setVisibility(View.GONE);
                mTv_result1.setVisibility(View.VISIBLE);
                mTv_result2.setVisibility(View.VISIBLE);
                mCheckBox_Group.setVisibility(View.VISIBLE);
                mCheckBox_Group2.setVisibility(View.VISIBLE);
                mRadioGroup_activity_mode.setVisibility(View.GONE);
                mCheckBox_Activity_Periodic.setVisibility(View.GONE);
                mLinearLayout_Activity_Periodic_interval.setVisibility(View.GONE);
                break;
            default:
                break;
        }
    }

    private void showAlertMessage(String msg) {
        if (msg == null) {
            throw new NullPointerException("AlertMessage is Null!!");
        }

        AlertDialog.Builder builder = new AlertDialog.Builder(this);
        builder.setTitle("Error");
        builder.setMessage(msg);
        builder.setNegativeButton("close", new DialogInterface.OnClickListener() {

            @Override
            public void onClick(DialogInterface dialog, int which) {
                // TODO Auto-generated method stub
                mBtn_start.setChecked(false);
                mSpin.setEnabled(true);
            }
        });
        builder.show();
    }

    static void playSound() {
        mSoundPool.play(mSoundId, 100, 100, 1, 0, 1.0f);
    }

    static void displayData(long timestamp, String str1) {
        // TODO Auto-generated method stub
        if (mTv_timestamp.getVisibility() == View.VISIBLE) {
            if (timestamp == 0) {
                mTv_timestamp.setText("[00:00:00]");
            } else {
                Time time = new Time();
                time.set(timestamp);
                Formatter form = new Formatter();
                form.format("%02d:%02d:%02d", time.hour, time.minute, time.second);
                mTv_timestamp.setText(new StringBuffer("[").append(form.toString()).append("]")
                        .toString());
                form.close();
            }
        }
        if (mTv_result1.getVisibility() == View.VISIBLE) {
            mTv_result1.setText(new StringBuffer(str1).toString());
        }
        if (mTv_result2.getVisibility() == View.VISIBLE) {
            mTv_result2.setText("");
        }
    }

    static void displayData(long timestamp, String str1, String str2) {
        // TODO Auto-generated method stub
        displayData(timestamp, str1);
        mTv_result2.setText(new StringBuffer(str2).toString());
    }

    void displayActivityStatus(StringBuffer sb) {
        if (sb == null)
            return;
        Toast.makeText(getApplicationContext(), sb.toString(), Toast.LENGTH_SHORT).show();
    }

    void setTextView(View v, boolean isActivity) {

        mRelativeLayout.removeView(v);

        TextView tx = (TextView) v;
        RelativeLayout.LayoutParams params = new RelativeLayout.LayoutParams(
                ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT);
        tx.setText("Ready");
        tx.setGravity(Gravity.CENTER_HORIZONTAL);
        params.addRule(RelativeLayout.BELOW, R.id.spinner_mode);

        if (isActivity) {
            params.topMargin = 200;
        } else {
            params.topMargin = 400;
        }

        mRelativeLayout.addView(tx, params);
    }

}
