package com.twilio.video.quickstart.activity;

import android.util.Log;

import java.io.BufferedReader;
import java.io.IOException;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;

public class HttpUtils {
    private static final String TAG = "HttpUtils";
    private static final String HTTP_GET = "GET";

    public static String get(String urlString) {
        BufferedReader in = null;
        StringBuilder response = new StringBuilder();

        try {
            URL url = new URL(urlString);
            String inputLine;
            HttpURLConnection httpUrlConnection = (HttpURLConnection) url.openConnection();

            // Set request method
            httpUrlConnection.setRequestMethod(HTTP_GET);

            // Execute request
            int responseCode = httpUrlConnection.getResponseCode();

            if (responseCode == HttpURLConnection.HTTP_OK) {
                in = new BufferedReader(new InputStreamReader(httpUrlConnection.getInputStream()));
                while ((inputLine = in.readLine()) != null) {
                    response.append(inputLine);
                }
            } else {
                Log.e(TAG, "Error performing GET request code: " + responseCode);
            }
        } catch (IOException e) {
            Log.e(TAG, e.getMessage());
        } finally {
            if (in != null) {
                try {
                    in.close();
                } catch (IOException e) {
                    Log.e(TAG, e.getMessage());
                }
            }
        }

        return response.toString();
    }
}
