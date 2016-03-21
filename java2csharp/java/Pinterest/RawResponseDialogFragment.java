package com.pinterest.android.pinsdk;

import android.os.Bundle;
import android.support.v4.app.DialogFragment;
import android.support.v4.app.Fragment;
import android.text.method.ScrollingMovementMethod;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;


/**
 * A simple {@link Fragment} subclass.
 * Activities that contain this fragment must implement the
 * Use the {@link RawResponseDialogFragment#newInstance} factory method to
 * create an instance of this fragment.
 */
public class RawResponseDialogFragment extends DialogFragment {

    private static final String ARG_PARAM1 = "raw";
    private String raw;

    public static RawResponseDialogFragment newInstance(String raw) {
        RawResponseDialogFragment fragment = new RawResponseDialogFragment();
        Bundle args = new Bundle();
        args.putString(ARG_PARAM1, raw);
        fragment.setArguments(args);
        return fragment;
    }

    public RawResponseDialogFragment() {
        // Required empty public constructor
    }

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        if (getArguments() != null) {
            raw = getArguments().getString(ARG_PARAM1);
        }
    }

    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container,
        Bundle savedInstanceState) {
        super.onCreateView(inflater, container, savedInstanceState);
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_raw_response_dialog, container, false);
    }

    @Override
    public void onActivityCreated(Bundle b) {
        super.onActivityCreated(b);
        TextView tv = (TextView) getView().findViewById(R.id.tv);
        tv.setMovementMethod(new ScrollingMovementMethod());
        tv.setText(raw);
    }

}
