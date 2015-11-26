package com.applause.hellointegration;

import android.app.Activity;
import android.os.Bundle;
import android.widget.Toast;
import com.applause.android.Applause;
// The next line will import Applause's built-in logging methods. You should use
// these in place of Android's regular logging methods. Applause's methods
// automatically send log messages to the Applause servers where you can review
// log data along with crash reports and bug reports. In addition, Applause's
// logging methods will still make your log entries available locally in Logcat.
// Learn more by visiting
// https://help.applause.com/hc/en-us/articles/201954883-Android-SDK-Pre-Production-Installation-Guide
import com.applause.android.Log;
import com.applause.android.config.IBuilder;
import com.applause.android.config.Configuration;

public class MainActivity extends Activity {

    /************************************/
    /* Application Key for for Applause */
    // You must insert your Applause application key in the next line of code:
    public static final String APP_KEY = "Your-Applause-Application-Key-Goes-Here";

    // If you do not have an Applause application key, you must first create a
    // free account. Visit http://www.applause.com for more information about
    // how to create your account.
    //
    // If you already have an Applause account, login to the Applause control
    // panel at: https://sdk.applause.com
    // Create a new application (if you have not done so already). Your
    // application key can be retrieved at any time from the "Settings" tab
    // on the right menu.

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_main);

        if (APP_KEY.equals("Your-Applause-Application-Key-Goes-Here")) {
            Toast.makeText(this, "You have not provided Applause Application Key in MainActivity.APP_KEY",
                    Toast.LENGTH_LONG).show();
            return;
        }

        /********************************/
        /* Creating session configuration */
        /********************************/
        // Here create a configuration Applause will use to start a session.
        // Configuration object uses builder pattern, so you can chain calls and
        // make the process of creation more compact.
        IBuilder builder = new Configuration.Builder(this);
        builder = builder.withAPIKey(APP_KEY); // specify app key

        // disable with uTest integration for demo purposes
        builder = builder.withUTestEnabled(false);


        Configuration configuration = builder.build();

        /********************************/
        /* Starting an Applause session */
        /********************************/
        // The following line of code actually starts an Applause session in QA
        // mode. Learn more by visiting
        // https://help.applause.com/hc/en-us/articles/201954883-Android-SDK-Pre-Production-Installation-Guide
        Applause.startNewSession(this, configuration);

        /*************************/
        /* Logging with Applause */
        /*************************/
        // Applause can replace the default logging mechanism with its own
        // mechanism, assuming you import com.applause.android.Log. See above
        // for more details.
        Log.w("myTag", "This log message will be sent to Applause.");
    }


}
