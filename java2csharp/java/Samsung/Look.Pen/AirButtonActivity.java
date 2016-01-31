
package com.samsung.android.example.slookdemos;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

import android.app.ListActivity;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ListView;
import android.widget.SimpleAdapter;

public class AirButtonActivity extends ListActivity {

    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setListAdapter(new SimpleAdapter(this, getData(), android.R.layout.simple_list_item_1,
                new String[] {
                    "title"
                }, new int[] {
                    android.R.id.text1
                }));
        getListView().setTextFilterEnabled(true);
    }

    private List<Map<String, Object>> getData() {
        List<Map<String, Object>> myData = new ArrayList<Map<String, Object>>();

        addItem(myData, "1. Simple",
                activityIntent("com.samsung.android.example.slookdemos.AirButtonSimpleActivity"));
        addItem(myData, "2. Default Adapter",
                activityIntent("com.samsung.android.example.slookdemos.AirButtonDefaultActivity"));
        addItem(myData,
                "3. SurfaceView",
                activityIntent("com.samsung.android.example.slookdemos.AirButtonSurfaceViewActivity"));
        addItem(myData, "4. SurfaceView-Manaual",
                activityIntent("com.samsung.android.example.slookdemos.AirButtonManualActivity"));
        return myData;
    }

    private Intent activityIntent(String componentName) {
        Intent result = new Intent();
        result.setClassName(this, componentName);
        return result;
    }

    private void addItem(List<Map<String, Object>> data, String name, Object intent) {
        Map<String, Object> item = new HashMap<String, Object>();
        item.put("title", name);
        item.put("intent", intent);
        data.add(item);
    }

    @Override
    @SuppressWarnings("unchecked")
    protected void onListItemClick(ListView l, View v, int position, long id) {
        Map<String, Object> map = (Map<String, Object>) l.getItemAtPosition(position);
        Intent intent = (Intent) map.get("intent");
        startActivity(intent);
    }

}
