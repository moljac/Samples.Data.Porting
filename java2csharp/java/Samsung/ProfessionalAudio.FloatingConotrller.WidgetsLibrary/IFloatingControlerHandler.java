package com.samsung.android.sdk.professionalaudio.widgets;

import java.util.Map;

import android.view.ViewGroup;

import com.samsung.android.sdk.professionalaudio.app.SapaAppService;

interface IFloatingControlerHandler {

    int getLayoutResource();

    void initLayout(ViewGroup layout);

    void setSapaAppService(SapaAppService sapaAppService, String mainPackagename);

    void stopConnections();

    boolean getBarExpanded();

    public Map<String, Boolean> getDevicesExpanded();

    int getBarAlignment();

    void loadBarState(FloatingController floatingController, int barAlignment);
}
