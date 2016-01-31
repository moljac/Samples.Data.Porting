package com.samsung.android.sdk.sample.mediabrowser;


import android.app.AlertDialog;
import android.app.Dialog;
import android.app.DialogFragment;
import android.os.Bundle;
import android.view.ViewGroup;
import android.widget.TableLayout;
import android.widget.TableRow;
import android.widget.TextView;

import java.util.Map;

/**
 * This class is a dialog which displays file properties.
 */
public class PropertiesDialog extends DialogFragment {

    private Map<String, String> props;

    @SuppressWarnings("unchecked")
	@Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        Bundle b = getArguments();
        props = (Map<String, String>) b.getSerializable("props");
    }

    @Override
    public Dialog onCreateDialog(Bundle savedInstanceState) {
        TableLayout l = new TableLayout(getActivity());
        TableRow tr;
        TextView label, value;
        for (Map.Entry<String, String> e : props.entrySet()) {
            tr = new TableRow(getActivity());
            label = new TextView(getActivity());
            value = new TextView(getActivity());
            label.setText(e.getKey());
            value.setText(e.getValue());
            label.setPadding(10, 10, 10, 10);
            value.setPadding(10, 10, 10, 10);
            tr.addView(label);
            tr.addView(value);
            l.addView(tr, new TableLayout.LayoutParams(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.WRAP_CONTENT));
        }
        return new AlertDialog.Builder(getActivity()).setView(l).setPositiveButton(android.R.string.ok, null).setTitle(R.string.properties).create();
    }
}
